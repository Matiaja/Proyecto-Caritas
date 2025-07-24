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
    public class DonationRequestsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;
        private readonly IStockService _stockService;

        public DonationRequestsController(ApplicationDbContext context, INotificationService notificationService, IStockService stockService)
        {
            _context = context;
            _notificationService = notificationService;
            _stockService = stockService;
        }

        // GET: api/DonationRequests
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<GetDonationRequestDTO>>> GetAllDonationRequests()
        {
            return await _context.DonationRequests
                .Include(dr => dr.OrderLine)
                .Select(dr => DonationRequestToDto(dr))
                .ToListAsync();
        }

        // GET: api/DonationRequests/{id}
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<GetDonationRequestDTO>> GetDonationRequestById(int id)
        {
            var donationRequest = await _context.DonationRequests
                .Include(dr => dr.OrderLine)
                .FirstOrDefaultAsync(dr => dr.Id == id);

            if (donationRequest == null)
            {
                return NotFound(new
                {
                    Status = "404",
                    Error = "Not Found",
                    Message = "Donation Request not found."
                });
            }

            return DonationRequestToDto(donationRequest);
        }

        // POST: api/DonationRequests
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<DonationRequestDTO>> CreateDonationRequest(DonationRequestDTO addDonationRequestDto)
        {
            // validate if the DonationRequest was sended
            if (addDonationRequestDto == null)
            {
                return BadRequest(new
                {
                    Status = "400",
                    Error = "Bad Request",
                    Message = "Donation Request data is required."
                });
            }

            // validate if the Center was sended
            if (addDonationRequestDto.AssignedCenterId <= 0 || addDonationRequestDto.AssignedCenterId.ToString() == null)
            {
                return BadRequest(new
                {
                    Status = "400",
                    Error = "Bad Request",
                    Message = "Assigned Center ID is required."
                });
            }
            // validate if the Center exists
            var centerExists = await _context.Centers
            .AnyAsync(c => c.Id == addDonationRequestDto.AssignedCenterId);

            if (!centerExists)
            {
                return BadRequest(new
                {
                    Status = "400",
                    Error = "Bad Request",
                    Message = $"Assigned Center with ID {addDonationRequestDto.AssignedCenterId} not found."
                });
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized();

            var user = await _context.Users.Include(u => u.Center).FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                return Unauthorized();

            // validate if the user is assigned to the center
            if (!User.IsInRole("Admin") && user.CenterId != addDonationRequestDto.AssignedCenterId)
            {
                return BadRequest(new
                {
                    Status = "400",
                    Error = "Bad Request",
                    Message = "User is not assigned to the center assigned."
                });
            }

            // validate if the OrderLine exists
            var orderLine = await _context.OrderLines
                .Include(ol => ol.Product)
                .Include(ol => ol.DonationRequests)
                .FirstOrDefaultAsync(ol => ol.Id == addDonationRequestDto.OrderLineId);

            if (orderLine == null)
            {
                return BadRequest(new
                {
                    Status = "400",
                    Error = "Bad Request",
                    Message = $"Order Line with ID {addDonationRequestDto.OrderLineId} not found."
                });
            }
            // validate if quantity is greater than 0
            if (addDonationRequestDto.Quantity <= 0)
            {
                return BadRequest(new
                {
                    Status = "400",
                    Error = "Bad Request",
                    Message = "La cantidad debe ser mayor a 0."
                });
            }

            var productId = orderLine.ProductId;

            // Validar que productId no sea null
            if (!productId.HasValue)
            {
                return BadRequest(new
                {
                    Status = "400",
                    Error = "Bad Request",
                    Message = "Product ID is missing or invalid."
                });
            }

            // validate stock available in center assigned (stock - donationRequests pending)
            var isStockAvailable = await _stockService.CanAssignDonation(addDonationRequestDto.AssignedCenterId, productId.Value, addDonationRequestDto.Quantity);

            if (!isStockAvailable)
            {
                return BadRequest(new
                {
                    Status = "400",
                    Error = "Bad Request",
                    Message = $"El centro no tiene suficiente stock disponible."
                });
            }

            // validate quantity already assigned to the order line
            var cantidadAsignada = orderLine.DonationRequests != null && orderLine.DonationRequests.Count != 0 ?
                orderLine.DonationRequests.Sum(dr => dr.Quantity) : 0;
            var cantidadPendiente = orderLine.Quantity - cantidadAsignada;

            if (addDonationRequestDto.Quantity > cantidadPendiente)
            {
                return BadRequest(new
                {
                    Status = "400",
                    Error = "Bad Request",
                    Message = $"La cantidad excede la cantidad solicitada. Se necesitan {cantidadPendiente}."
                });
            }

            // create DonationRequest
            var donationRequest = new DonationRequest
            {
                OrderLineId = addDonationRequestDto.OrderLineId,
                AssignedCenterId = addDonationRequestDto.AssignedCenterId,
                Quantity = addDonationRequestDto.Quantity,
                Status = "Asignada"
            };
            _context.DonationRequests.Add(donationRequest);
            await _context.SaveChangesAsync();

            await _notificationService.CreateAssignmentNotification(
                addDonationRequestDto.OrderLineId,
                donationRequest.Id,
                addDonationRequestDto.AssignedCenterId,
                userId
            );

            return CreatedAtAction(nameof(GetDonationRequestById), new { id = donationRequest.Id }, DonationRequestToDto(donationRequest));

        }

        // GET: api/DonationRequests/movements
        [HttpGet("movements")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<MovementDTO>>> GetMovements(
            [FromQuery] DateTime? dateFrom = null,
            [FromQuery] DateTime? dateTo = null,
            [FromQuery] string status = null,
            [FromQuery] int? productId = null
        )
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized();

            var user = await _context.Users.Include(u => u.Center).FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                return Unauthorized();

            // Validar parámetros
            if (dateFrom.HasValue && dateTo.HasValue && dateTo < dateFrom)
                return BadRequest(new { message = "La 'Fecha hasta' no puede ser anterior a la 'Fecha desde'." });

            if (!string.IsNullOrEmpty(status) && !new[] { "En camino", "Recibida" }.Contains(status))
                return BadRequest(new { message = "Estado inválido." });

            var query = _context.DonationRequests
                .Include(dr => dr.OrderLine)
                    .ThenInclude(ol => ol.Product)
                .Include(dr => dr.AssignedCenter)
                .Include(dr => dr.OrderLine.Request)
                    .ThenInclude(r => r.RequestingCenter)
                .Where(dr => dr.Status == "En camino" || dr.Status == "Recibida");

            // Filtros básicos
            if (dateFrom.HasValue)
                query = query.Where(dr => dr.AsignationDate >= dateFrom.Value);

            if (dateTo.HasValue)
                query = query.Where(dr => dr.AsignationDate <= dateTo.Value);

            if (!string.IsNullOrEmpty(status))
                query = query.Where(dr => dr.Status == status);

            if (productId.HasValue)
                query = query.Where(dr => dr.OrderLine.ProductId == productId.Value);

            // Filtro por centro si no es admin
            if (!User.IsInRole("Admin") && user.CenterId != null)
            {
                query = query.Where(dr => dr.AssignedCenterId == user.CenterId || // Movimientos donde es el centro donante
                     dr.OrderLine.Request.RequestingCenterId == user.CenterId); // donde es el centro receptor
            }

            var movements = await query
                .OrderByDescending(dr => dr.AsignationDate)
                .Select(dr => new MovementDTO
                {
                    DonationRequestId = dr.Id,
                    FromCenter = dr.AssignedCenter.Name,
                    ToCenter = dr.OrderLine.Request.RequestingCenter.Name,
                    ProductName = dr.OrderLine.Product.Name,
                    Quantity = dr.Quantity,
                    Status = dr.Status,
                    AssignmentDate = dr.AsignationDate
                })
                .ToListAsync();

            return Ok(movements);
        }

        // PUT: api/DonationRequests/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDonationRequest(int id, DonationRequestDTO updateDonationRequestDto)
        {
            var donationRequest = await _context.DonationRequests.FindAsync(id);
            if (donationRequest == null)
            {
                return NotFound(new
                {
                    Status = "404",
                    Error = "Not Found",
                    Message = "Donation Request not found."
                });
            }
            if (updateDonationRequestDto.AssignedCenterId.ToString() == null || updateDonationRequestDto.AssignedCenterId.ToString() == "0")
            {
                return BadRequest(new
                {
                    Status = "400",
                    Error = "Bad Request",
                    Message = "Assigned Center ID is required."
                });
            }

            var centerExists = await _context.Centers
            .AnyAsync(c => c.Id == updateDonationRequestDto.AssignedCenterId);
            if (!centerExists)
            {
                return BadRequest(new
                {
                    Status = "400",
                    Error = "Bad Request",
                    Message = $"The Assigned Center with ID {updateDonationRequestDto.AssignedCenterId} does not exist."
                });
            }


            donationRequest.AssignedCenterId = updateDonationRequestDto.AssignedCenterId;
            donationRequest.ShipmentDate = updateDonationRequestDto.ShipmentDate;
            donationRequest.ReceptionDate = updateDonationRequestDto.ReceptionDate;
            donationRequest.Status = updateDonationRequestDto.Status;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Status = "400",
                    Error = "Bad Request",
                    Message = ex.Message
                });

            }

            return NoContent();
        }


        // DELETE: api/DonationRequests/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDonationRequest(int id)
        {
            var donationRequest = await _context.DonationRequests.FindAsync(id);
            if (donationRequest == null)
            {
                return NotFound(new
                {
                    Status = "404",
                    Error = "Not Found",
                    Message = "Donation Request not found."
                });
            }
            _context.DonationRequests.Remove(donationRequest);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private static GetDonationRequestDTO DonationRequestToDto(DonationRequest donationRequest) =>
            new GetDonationRequestDTO
            {
                Id = donationRequest.Id,
                AssignedCenterId = donationRequest.AssignedCenterId,
                OrderLineId = donationRequest.OrderLineId,
                Quantity = donationRequest.Quantity,
                ShipmentDate = donationRequest.ShipmentDate ?? null,
                ReceptionDate = donationRequest.ReceptionDate ?? null,
                Status = donationRequest.Status,
                OrderLine = donationRequest.OrderLine != null ? new OrderLineDTO
                {
                    Id = donationRequest.OrderLine.Id,
                    RequestId = donationRequest.OrderLine.RequestId,
                    Quantity = donationRequest.OrderLine.Quantity,
                    Description = donationRequest.OrderLine.Description,
                    ProductId = donationRequest.OrderLine.ProductId
                } : null
            };
    }
}
