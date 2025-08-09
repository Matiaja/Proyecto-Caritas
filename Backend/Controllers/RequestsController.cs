using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ProyectoCaritas.Data;
using ProyectoCaritas.Models.DTOs;
using ProyectoCaritas.Models.Entities;

namespace ProyectoCaritas.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RequestsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly OrderLineService _orderLineService;

        private readonly UserManager<User> _userManager;


        public RequestsController(ApplicationDbContext context, UserManager<User> userManager, OrderLineService orderLineService)
        {
            _context = context;
            _userManager = userManager;
            _orderLineService = orderLineService;
        }

        // GET: api/Requests
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<RequestDTO>>> GetAllRequests()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            // Si el token no tiene ID, devuelve un error 401
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "Invalid token." });
            }

            // Si es admin, devuelve todas las requests
            if (role == "Admin")
            {
                return await _context.Requests
                    .Include(r => r.RequestingCenter)
                    .Include(r => r.OrderLines)
                    .OrderByDescending(r => r.RequestDate)
                    .Select(r => RequestToDTO(r))
                    .ToListAsync();
            }

            // Si no es admin, buscar el centro del usuario
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            // Filtrar las requests por centro del usuario
            var userRequests = await _context.Requests
                .Include(r => r.RequestingCenter)
                .Include(r => r.OrderLines)
                .Where(r => r.RequestingCenterId == user.CenterId)
                .OrderByDescending(r => r.RequestDate)
                .Select(r => RequestToDTO(r))
                .ToListAsync();

            return Ok(userRequests); ;
        }

        // GET: api/Requests/{id}
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<RequestDTO>> GetRequestById(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            // Si el token no tiene ID, devuelve un error 401
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "Invalid token." });
            }

            var request = await _context.Requests
                .Include(r => r.RequestingCenter)
                .Include(r => r.OrderLines)
                    .ThenInclude(ol => ol.DonationRequests)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (request == null)
            {
                return NotFound(new
                {
                    Status = "404",
                    Error = "Not Found",
                    Message = "Request not found."
                });
            }

            // Si es user, valida si la request es del centro del usuario
            if (role != "Admin")
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return NotFound(new
                    {
                        Status = "404",
                        Message = "User not found."
                    });
                }

                if (request.RequestingCenterId != user.CenterId)
                {
                    return new ObjectResult(new
                    {
                        Status = 403,
                        Message = "You do not have permission to access this request."
                    })
                    {
                        StatusCode = StatusCodes.Status403Forbidden
                    };
                }
            }

            return RequestToDTO(request);
        }

        // POST: api/Requests
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<RequestDTO>> AddRequest(AddRequestDTO requestDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            // Si el token no tiene ID, devuelve un error 401
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(role))
            {
                return Unauthorized(new { message = "Invalid token." });
            }

            // Validar si el CenterId proporcionado existe en la base de datos
            var requestingCenter = await _context.Centers.FindAsync(requestDTO.RequestingCenterId);
            if (requestingCenter == null)
            {
                return BadRequest(new
                {
                    Status = "400",
                    Error = "Bad Request",
                    Message = "Requesting center not found."
                });
            }
            if (requestDTO.OrderLines.IsNullOrEmpty())
            {
                return BadRequest(new
                {
                    Status = "400",
                    Error = "Bad Request",
                    Message = "Request has not orderline."
                });
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new
                {
                    Status = "404",
                    Message = "User not found."
                });
            }
            // si el usuario NO es admin, validar que solo pueda usar su centro para solicitar
            if (role != "Admin")
            {
                if (user.CenterId != requestDTO.RequestingCenterId)
                {
                    return new ObjectResult(new
                    {
                        Status = 403,
                        Message = "You do not have permission to make a request from this center."
                    })
                    {
                        StatusCode = StatusCodes.Status403Forbidden
                    };
                }
            }

            var request = new Request
            {
                RequestingCenterId = requestDTO.RequestingCenterId,
                UrgencyLevel = requestDTO.UrgencyLevel,
                RequestDate = DateTime.UtcNow,
            };

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {

                _context.Requests.Add(request);
                await _context.SaveChangesAsync();

                // Agregar líneas de pedido
                foreach (var line in requestDTO.OrderLines)
                {
                    var orderLine = new OrderLineDTO
                    {
                        RequestId = request.Id,
                        ProductId = line.ProductId,
                        Quantity = line.Quantity,
                        Description = line.Description
                    };

                    await _orderLineService.AddOrderLineAsync(orderLine, request.Id);
                }

                await transaction.CommitAsync();

                return CreatedAtAction(
                    nameof(GetRequestById),
                    new { id = request.Id },
                    RequestToDTO(request));
            }
            catch (ArgumentException ex)
            {
                await transaction.RollbackAsync();
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, $"Error al agregar la solicitud: {ex.Message}");
            }
        }

        // PUT: api/Requests/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRequest(int id, AddRequestDTO updateRequestDTO)
        {
            //validaciones correspondientes
            if (updateRequestDTO.RequestingCenterId < 0 || string.IsNullOrEmpty(updateRequestDTO.UrgencyLevel) || updateRequestDTO.RequestDate == default)
            {
                return BadRequest(new
                {
                    Status = "400",
                    Error = "Bad Request",
                    Message = "Invalid data. Ensure all required fields are provided."
                });
            }
            // Recuperar la entidad existente de la base de datos
            var existingRequest = await _context.Requests.FindAsync(id);
            if (existingRequest == null)
            {
                return NotFound(new
                {
                    Status = "404",
                    Error = "Not Found",
                    Message = "Request not found."
                });
            }
            // Validar si el CenterId proporcionado existe en la base de datos
            var centerExists = await _context.Centers.AnyAsync(c => c.Id == updateRequestDTO.RequestingCenterId);
            if (!centerExists)
            {
                return BadRequest(new
                {
                    Status = "400",
                    Error = "Bad Request",
                    Message = "The provided CenterId does not exist."
                });
            }
            // Mapear el DTO a la entidad
            existingRequest.RequestingCenterId = updateRequestDTO.RequestingCenterId;
            existingRequest.UrgencyLevel = updateRequestDTO.UrgencyLevel;
            existingRequest.RequestDate = updateRequestDTO.RequestDate;

            // Marcar la entidad como modificada
            _context.Entry(existingRequest).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RequestExists(id))
                {
                    return NotFound(new
                    {
                        Status = "404",
                        Error = "Not Found",
                        Message = "Request not found during concurrency check."
                    });
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // PUT: api/Requests/{id}/close
        [HttpPut("{id}/close")]
        [Authorize]
        public async Task<IActionResult> CloseRequestManually(int id)
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

            var request = await _context.Requests
                .Include(r => r.OrderLines)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (request == null)
                return NotFound("Solicitud no encontrada.");

            // Verificar si la solicitud ya está finalizada o cerrada
            if (request.Status == "Finalizada" || request.Status == "Cerrada")
                return BadRequest("La solicitud ya está finalizada o cerrada.");

            // Verificar si el usuario es admin o si la solicitud pertenece a su centro
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (role != "Admin" && request.RequestingCenterId != user.CenterId)
            {
                return Forbid("No tienes permiso para cerrar esta solicitud.");
            }

            request.Status = "Cerrada";
            request.ClosedByUserId = userId;
            request.ClosedDate = DateTime.UtcNow;
            // Actualizar el estado de las líneas de pedido asociadas
            foreach (var ol in request.OrderLines)
            {
                if (ol.Status != "Completa" || ol.Status != "Parcial")
                {
                    ol.Status = "Cancelada";
                }
            }

            await _context.SaveChangesAsync();

            return Ok(new { Message = "Solicitud cerrada manualmente.", RequestId = id });

        }

        // DELETE: api/Requests/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRequest(int id)
        {
            var request = await _context.Requests.FindAsync(id);
            if (request == null)
            {
                return NotFound(new
                {
                    Status = "404",
                    Error = "Not Found",
                    Message = "Request not found."
                });
            }

            _context.Requests.Remove(request);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool RequestExists(int id)
        {
            return _context.Requests.Any(e => e.Id == id);
        }

        private static RequestDTO RequestToDTO(Request request) =>
           new RequestDTO
           {
               Id = request.Id,
               RequestingCenterId = request.RequestingCenterId,
               UrgencyLevel = request.UrgencyLevel,
               Status = request.Status,
               RequestDate = request.RequestDate,
               OrderLines = request.OrderLines?.Select(ol => new OrderLineDTO
               {
                   Id = ol.Id,
                   RequestId = ol.RequestId,
                   Quantity = ol.Quantity,
                   Description = ol.Description,
                   ProductId = ol.ProductId,
                   Status = ol.Status,
                   DonationRequests = ol.DonationRequests?.Select(dr => new GetDonationRequestDTO
                   {
                       Id = dr.Id,
                       AssignedCenterId = dr.AssignedCenterId,
                       Quantity = dr.Quantity,
                       AssignmentDate = dr.AssignmentDate,
                       Status = dr.Status,
                       LastStatusChangeDate = dr.LastStatusChangeDate
                   }).ToList()
               }).ToList() ?? new List<OrderLineDTO>(),
               RequestingCenter = request.RequestingCenter != null ? new GetCenterDTO
               {
                   Id = request.RequestingCenter.Id,
                   Name = request.RequestingCenter.Name,
                   Location = request.RequestingCenter.Location,
                   Manager = request.RequestingCenter.Manager,
                   Phone = request.RequestingCenter.Phone,
               } : null
           };
    }
}
