using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoCaritas.Data;
using ProyectoCaritas.Models.DTOs;
using ProyectoCaritas.Models.Entities;
using ProyectoCaritas.Services;

namespace ProyectoCaritas.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;
        private readonly IStockService _stockService;


        public NotificationsController(ApplicationDbContext context, INotificationService notificationService, IStockService stockService)
        {
            _context = context;
            _notificationService = notificationService;
            _stockService = stockService;
        }

        // GET: api/notifications
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Notification>>> GetNotifications()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized();
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var query = _context.Notifications.AsQueryable();

            // Notificaciones para el centro del usuario
            if (user.CenterId != null)
            {
                query = query.Where(n => n.RecipientCenterId == user.CenterId);
            }

            // Notificaciones para admins (si el usuario es admin)
            if (User.IsInRole("Admin"))
            {
                query = query.Union(
                    _context.Notifications.Where(n => n.RecipientUserId == null && n.RecipientCenterId == null)
                );
            }

            // Filtrar notificaciones activas y ordenarlas por fecha de creación
            var notifications = query
                .Where(n => n.Status == "Active" && n.IsRead == false)
                .OrderByDescending(n => n.CreatedAt)
                .ToList();

            return Ok(notifications);
        }

        [HttpPost("accept")]
        [Authorize]
        public async Task<IActionResult> AcceptAssignment([FromBody] DataAcceptRequestDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized();
            }
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var donationRequest = await _context.DonationRequests.FindAsync(dto.DonationRequestId);
            if (donationRequest == null) return NotFound("DonationRequest not found.");

            // Verificar que la solicitud de donación está pendiente de aceptación (Status "Asignada")
            if (donationRequest.Status != "Asignada")
            {
                return BadRequest(new
                {
                    Status = 400,
                    Message = "La solicitud de donación no está en estado 'Asignada'."
                });
            }

            // Verificar que order line ID es válido
            var orderLine = await _context.OrderLines
                .FindAsync(dto.OrderLineId);
            if (orderLine == null) throw new ArgumentException("Order line not found.");

            // Verificar que el usuario pertenece al centro asignado
            if (donationRequest.AssignedCenterId != user.CenterId)
            {
                return new ObjectResult(new
                {
                    Status = 403,
                    Message = "No tienes permiso para aceptar esta solicitud de donación."
                })
                {
                    StatusCode = StatusCodes.Status403Forbidden
                };
            }

            // Verify donation request is in the order line
            if (donationRequest.OrderLineId != dto.OrderLineId)
            {
                throw new ArgumentException("Donation request does not belong to the specified order line.");
            }

            var notification = await _context.Notifications
                    .FirstOrDefaultAsync(n =>
                        n.DonationRequestId == dto.DonationRequestId &&
                        n.OrderLineId == dto.OrderLineId &&
                        n.Id == dto.IdNotification &&
                        n.IsRead == false &&
                        n.Type == NotificationType.Assignment);
            if (notification == null) return NotFound("Notification not found.");

            // Iniciar transacción
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Actualizar estado de la donacion
                donationRequest.Status = "Aceptada";

                // Marcar la notificación relacionada como leída                
                notification.IsRead = true;

                // Guardar cambios
                await _context.SaveChangesAsync();

                // Crear notificaciones de aceptacion
                var newNotifications = await _notificationService.CreateAcceptanceNotification(
                    dto.OrderLineId,
                    dto.DonationRequestId,
                    userId
                );

                // confirmar transacción
                await transaction.CommitAsync();

                try
                {
                    // notificar via signal r
                    await _notificationService.SendSignalRNotifications(newNotifications);
                }
                catch (Exception ex)
                {
                    // Si falla la notificación
                    // No revertimos la transacción, solo informamos del error en consola
                    Console.WriteLine("Error al enviar notificaciones via SignalR: " + ex.Message);
                }

                return Ok();
            }
            catch (Exception ex)
            {
                // Rollback transaction en caso de error
                await transaction.RollbackAsync();
                return BadRequest(new
                {
                    Status = 500,
                    Message = "Error al aceptar la solicitud de donación: " + ex.Message
                });
            }

        }

        [HttpPost("ship")]
        [Authorize]
        public async Task<IActionResult> MarkAsShipped([FromBody] DataDonationShippedDTO dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized();
            }
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            // Validar existencia de la solicitud
            var donationRequest = await _context.DonationRequests.FindAsync(dto.DonationRequestId);
            if (donationRequest == null) return NotFound("DonationRequest not found.");

            // Verificar que la solicitud de donación está pendiente de envío (Status "Aceptada")
            if (donationRequest.Status != "Aceptada")
            {
                return BadRequest(new
                {
                    Status = 400,
                    Message = "La solicitud de donación no está en estado 'Aceptada'."
                });
            }

            // Verificar que order line ID es válido
            var orderLine = await _context.OrderLines
                .FindAsync(dto.OrderLineId);
            if (orderLine == null) throw new ArgumentException("Order line not found.");

            // Verify donation request is in the order line
            if (donationRequest.OrderLineId != dto.OrderLineId)
            {
                throw new ArgumentException("Donation request does not belong to the specified order line.");
            }

            // Verificar permisos (solo el centro asignado puede marcar como enviado)
            if (donationRequest.AssignedCenterId != user.CenterId)
            {
                return new ObjectResult(new
                {
                    Status = 403,
                    Message = "No tienes permiso para enviar esta solicitud de donación."
                })
                {
                    StatusCode = StatusCodes.Status403Forbidden
                };
            }

            var notification = await _context.Notifications
                    .FirstOrDefaultAsync(n =>
                        n.DonationRequestId == dto.DonationRequestId &&
                        n.OrderLineId == dto.OrderLineId &&
                        n.Id == dto.IdNotification &&
                        n.IsRead == false &&
                        n.Type == NotificationType.Acceptance);
            if (notification == null) return NotFound("Notification not found.");

            // Iniciar transacción
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Actualizar estado de la donación
                donationRequest.Status = "En camino";

                // Marcar notificación de aceptación como leída
                notification.IsRead = true;

                // Guardar cambios
                await _context.SaveChangesAsync();

                // Crear nuevas notificaciones
                var notifications = await _notificationService.CreateShippingNotification(
                    dto.OrderLineId,
                    dto.DonationRequestId,
                    userId
                );

                // Confirmar transacción
                await transaction.CommitAsync();

                try
                {
                    // Enviar notificaciones via SignalR
                    await _notificationService.SendSignalRNotifications(notifications);
                }
                catch (Exception ex)
                {
                    // Si falla la notificación
                    // No revertimos la transacción, solo informamos del error en consola
                    Console.WriteLine("Error al enviar notificaciones via SignalR: " + ex.Message);
                }

                return Ok();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, "Error al marcar como enviado");
            }
        }

        // POST api/notifications/receive
        [HttpPost("receive")]
        [Authorize]
        public async Task<IActionResult> ConfirmReceipt([FromBody] DataConfirmReceiptDTO dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized();
            }
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            // Validar existencia de la notificación
            var notification = await _context.Notifications
                    .FirstOrDefaultAsync(n =>
                        n.DonationRequestId == dto.DonationRequestId &&
                        n.OrderLineId == dto.OrderLineId &&
                        n.Id == dto.IdNotification &&
                        n.IsRead == false &&
                        n.Type == NotificationType.Shipping);
            if (notification == null) return NotFound("Notification not found.");

            // Validar existencia de la solicitud de donación
            var donationRequest = await _context.DonationRequests.FindAsync(dto.DonationRequestId);
            if (donationRequest == null) return NotFound("DonationRequest not found.");

            // Verificar que la solicitud de donación está pendiente de envío (Status "Aceptada")
            if (donationRequest.Status != "En camino")
            {
                return BadRequest(new
                {
                    Status = 400,
                    Message = "La solicitud de donación no está en estado 'En camino'."
                });
            }

            // Verificar que order line ID es válido
            var orderLine = await _context.OrderLines
                .Include(ol => ol.Request)
                    .ThenInclude(r => r.RequestingCenter)
                .FirstOrDefaultAsync(ol => ol.Id == dto.OrderLineId);
            if (orderLine == null) throw new ArgumentException("Order line not found.");

            // Verify donation request is in the order line
            if (donationRequest.OrderLineId != dto.OrderLineId)
            {
                throw new ArgumentException("Donation request does not belong to the specified order line.");
            }

            // Verificar permisos (solo el centro solicitante puede marcar como recibido)
            if (orderLine?.Request?.RequestingCenterId != user.CenterId)
            {
                return new ObjectResult(new
                {
                    Status = 403,
                    Message = "No tienes permiso para recibir esta donación."
                })
                {
                    StatusCode = StatusCodes.Status403Forbidden
                };
            }

            // Si donante es admin, no generamos notificación para el centro donante
            bool generateDonorNotification = true;
            var donorUser = await _context.Users.FindAsync(notification.UserId);
            if (donorUser != null && donorUser.Role == "Admin")
            {
                // Si el donante es admin, no generamos notificación
                generateDonorNotification = false;
            }

            // Iniciar transacción
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Actualizar estado de la donación
                donationRequest.Status = "Recibida";

                // Marcar notificación de aceptación como leída
                notification.IsRead = true;

                // Guardar cambios
                await _context.SaveChangesAsync();

                // Actualizar stocks
                int? requestingCenterId = orderLine?.Request?.RequestingCenterId;
                if (requestingCenterId == null)
                {
                    return BadRequest("Requesting center ID is null.");
                }
                // llamar a AddStock de StockController para actualizar el stock de ingreso
                _ = await _stockService.AddStock(new StockDTO
                {
                    CenterId = (int)requestingCenterId,
                    ProductId = orderLine?.ProductId,
                    Date = DateTime.UtcNow,
                    Type = "Ingreso",
                    Description = orderLine?.Description,
                    Quantity = donationRequest.Quantity,
                });
                // actualizar el stock de egreso
                _ = await _stockService.AddStock(new StockDTO
                {
                    CenterId = donationRequest.AssignedCenterId,
                    ProductId = orderLine?.ProductId,
                    Date = DateTime.UtcNow,
                    Type = "Egreso",
                    Description = "Donación enviada a " + orderLine?.Request?.RequestingCenter?.Name,
                    Quantity = donationRequest.Quantity,
                });

                // Crear nuevas notificaciones
                var notifications = await _notificationService.CreateReceptionNotification(
                    dto.OrderLineId,
                    dto.DonationRequestId,
                    userId,
                    generateDonorNotification
                );

                // Confirmar transacción
                await transaction.CommitAsync();

                try
                {
                    // Enviar notificaciones via SignalR
                    await _notificationService.SendSignalRNotifications(notifications);
                }
                catch (Exception ex)
                {
                    // Si falla la notificación
                    // No revertimos la transacción, solo informamos del error en consola
                    Console.WriteLine("Error al enviar notificaciones via SignalR: " + ex.Message);
                }

                return Ok();
            }
            catch (ArgumentException ex)
            {
                await transaction.RollbackAsync();
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                await transaction.RollbackAsync();
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, ex.Message);
            }
        }

        // PUT api/notifications/{id}/read
        [HttpPut("{id}/read")]
        [Authorize]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized();
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var notification = await _context.Notifications.FindAsync(id);
            if (notification == null) return NotFound();

            // Verificar que la notificación es para el usuario o su centro
            if (!User.IsInRole("Admin"))
            {
                if (notification.RecipientUserId != userId && notification.RecipientCenterId != user.CenterId)
                {
                    return new ObjectResult(new
                    {
                        Status = 403,
                        Message = "No tienes permiso para marcar esta notificación como leída."
                    })
                    {
                        StatusCode = StatusCodes.Status403Forbidden
                    };
                }
            }

            // Marcar la notificación como leída
            notification.IsRead = true;
            _context.Entry(notification).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

    }
}