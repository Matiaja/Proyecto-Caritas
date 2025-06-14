using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoCaritas.Data;
using ProyectoCaritas.Models.DTOs;
using ProyectoCaritas.Models.Entities;

namespace ProyectoCaritas.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;

        public NotificationsController(ApplicationDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
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
            if (donationRequest == null) return NotFound("DonarionRequest not found.");

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

            // Iniciar transacción
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Actualizar estado de la donacion
                donationRequest.Status = "Aceptada";

                // Marcar la notificación relacionada como leída
                var notification = await _context.Notifications
                    .FirstOrDefaultAsync(n =>
                        n.DonationRequestId == dto.DonationRequestId &&
                        n.OrderLineId == dto.OrderLineId &&
                        n.Id == dto.IdNotification &&
                        n.Type == NotificationType.Assignment);
                if (notification == null) return NotFound("Notification not found.");

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