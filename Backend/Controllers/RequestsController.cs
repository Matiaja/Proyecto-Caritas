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

        public RequestsController(ApplicationDbContext context, OrderLineService orderLineService)
        {
            _context = context;
            _orderLineService = orderLineService;
        }

        // GET: api/Requests
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RequestDTO>>> GetAllRequests()
        {
            return await _context.Requests
                .Include(r => r.RequestingCenter)
                .Include(r => r.OrderLines)
                .Select(r => RequestToDTO(r))
                .ToListAsync();
        }

        // GET: api/Requests/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<RequestDTO>> GetRequestById(int id)
        {
            var request = await _context.Requests
                .Include(r => r.RequestingCenter)
                .Include(r => r.OrderLines)
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

            return RequestToDTO(request);
        }

        // POST: api/Requests
        [HttpPost]
        public async Task<ActionResult<RequestDTO>> AddRequest(RequestDTO requestDTO)
        {
            //validaciones correspondientes
            if (requestDTO.RequestingCenterId < 0 || string.IsNullOrEmpty(requestDTO.UrgencyLevel) || requestDTO.RequestDate == default)
            {
                return BadRequest(new
                {
                    Status = "400",
                    Error = "Bad Request",
                    Message = "Invalid data. Ensure all required fields are provided."
                });
            }
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
                    Message = "Request have not orderline."
                });
            }

            var request = new Request
            {
                RequestingCenterId = requestDTO.RequestingCenterId,
                UrgencyLevel = requestDTO.UrgencyLevel,
                RequestDate = requestDTO.RequestDate,
                RequestingCenter = requestingCenter,
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
        public async Task<IActionResult> UpdateRequest(int id, UpdateRequestDTO updateRequestDTO)
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
               RequestDate = request.RequestDate,
               OrderLines = request.OrderLines?.Select(ol => new OrderLineDTO
               {
                   Id = ol.Id,
                   RequestId = ol.RequestId,
                   DonationRequestId = ol.DonationRequestId,
                   Quantity = ol.Quantity,
                   Description = ol.Description,
                   ProductId = ol.ProductId
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
