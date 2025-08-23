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
    public class CentersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CentersController(ApplicationDbContext context)
        {
            _context = context;
        }


        // GET: api/Centers
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<GetCenterDTO>>> GetAllCenters()
        {
            return await _context.Centers
                .Include(c => c.DonationRequests)
                .Include(c => c.Stocks)
                .Include(c => c.Users)
                .Select(c => CenterToDto(c))
                .ToListAsync();
        }

        [HttpGet("GetCentersByFilter")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<GetCenterDTO>>> GetCentersByFilter([FromQuery] string? sortBy, [FromQuery] string? order)
        {
            // Start with IQueryable to build the query dynamically
            var query = _context.Centers.AsQueryable();

            // Apply sorting before fetching the data
            if (!string.IsNullOrEmpty(sortBy))
            {
                var isAsc = string.IsNullOrEmpty(order) || order.ToLower() == "asc";
                query = sortBy.ToLower() switch
                {
                    "name" => isAsc ? query.OrderBy(c => c.Name) : query.OrderByDescending(c => c.Name),
                    "location" => isAsc ? query.OrderBy(c => c.Location) : query.OrderByDescending(c => c.Location),
                    "manager" => isAsc ? query.OrderBy(c => c.Manager) : query.OrderByDescending(c => c.Manager),
                    _ => query
                };
            }

            // Now, include related entities and project to DTOs
            // The database will execute a single, efficient query
            var result = await query
                .Include(c => c.DonationRequests)
                .Include(c => c.Stocks)
                .Include(c => c.Users)
                .Select(c => CenterToDto(c))
                .ToListAsync();

            return result;
        }

        // GET: api/Centers/{id}
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<GetCenterDTO>> GetCenterById(int id)
        {
            var center = await _context.Centers
                .Include(c => c.DonationRequests)
                .Include(c => c.Stocks)
                .Include(c => c.Users)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (center == null)
            {
                return NotFound(new
                {
                    Status = "404",
                    Error = "Not Found",
                    Message = "Center not found."
                });
            }

            return CenterToDto(center);
        }

        // POST: api/Centers
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<CenterDTO>> CreateCenter(CenterDTO addCenterDto)
        {
            if (addCenterDto == null)
            {
                return BadRequest(new
                {
                    Status = "400",
                    Error = "Bad Request",
                    Message = "Center data is missing."
                });
            }
            if (string.IsNullOrWhiteSpace(addCenterDto.Name) || string.IsNullOrWhiteSpace(addCenterDto.Location) || string.IsNullOrWhiteSpace(addCenterDto.Manager))
            {
                return BadRequest(new
                {
                    Status = "400",
                    Error = "Bad Request",
                    Message = "Name, Location, and Manager cannot be empty or just spaces."
                });
            }
            // This regex ensures the phone contains only digits (0-9)
            if (!System.Text.RegularExpressions.Regex.IsMatch(addCenterDto.Phone, @"^[0-9]+$"))
            {
                return BadRequest(new
                {
                    Status = "400",
                    Error = "Bad Request",
                    Message = "Phone number must contain only digits."
                });
            }
            // validate if capacity limit exists and is a positive integer
            if (addCenterDto.CapacityLimit != null && addCenterDto.CapacityLimit <= 0)
            {
                return BadRequest(new
                {
                    Status = "400",
                    Error = "Bad Request",
                    Message = "Capacity limit must be a positive integer."
                });
            }
            // if email exists validate it
            if (!string.IsNullOrWhiteSpace(addCenterDto.Email) && !System.Text.RegularExpressions.Regex.IsMatch(addCenterDto.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                return BadRequest(new
                {
                    Status = "400",
                    Error = "Bad Request",
                    Message = "Email format is invalid."
                });
            }
            var center = new Center
            {
                Name = addCenterDto.Name.Trim(),
                Location = addCenterDto.Location.Trim(),
                Manager = addCenterDto.Manager.Trim(),
                CapacityLimit = addCenterDto.CapacityLimit ?? null,
                Phone = addCenterDto.Phone,
                Email = addCenterDto.Email?.Trim()
            };
            _context.Centers.Add(center);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetCenterById), new { id = center.Id }, CenterToDto(center));
        }

        // PUT: api/Centers/{id}
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateCenter(int id, CenterDTO centerDto)
        {
            var center = _context.Centers.Find(id);
            if (center == null)
            {
                return NotFound();
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized();

            var user = await _context.Users.Include(u => u.Center).FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                return Unauthorized();

            if (!User.IsInRole("Admin") && user.CenterId != center.Id)
            {
                return Forbid();
            }

            center.Name = centerDto.Name;
            center.Location = centerDto.Location;
            center.Manager = centerDto.Manager;
            center.CapacityLimit = centerDto.CapacityLimit;
            center.Phone = centerDto.Phone;
            center.Email = centerDto.Email;
            _context.Centers.Update(center);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/Centers/{id}
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCenter(int id)
        {
            var center = await _context.Centers.FindAsync(id);
            if (center == null)
            {
                return NotFound();
            }

            try
            {
                _context.Centers.Remove(center);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Centro eliminado con éxito." });
            }
            catch (DbUpdateException)
            {
                // Catches errors from the database, like foreign key violations
                return Conflict(new { message = "Este centro no se puede eliminar porque está asociado a usuarios o registros existentes." });
            }
        }


        private static GetCenterDTO CenterToDto(Center center) =>
            new GetCenterDTO
            {
                Id = center.Id,
                Name = center.Name,
                Location = center.Location,
                Manager = center.Manager,
                CapacityLimit = center.CapacityLimit,
                Phone = center.Phone,
                Email = center.Email,
                DonationRequests = center.DonationRequests?.Select(dr => new GetDonationRequestDTO
                {
                    Id = dr.Id,
                    AssignedCenterId = dr.AssignedCenterId,
                    Status = dr.Status
                }).ToList() ?? new List<GetDonationRequestDTO>(),
                Stocks = center.Stocks?.Select(s => new GetStockDTO
                {
                    Id = s.Id,
                    ProductId = s.ProductId,
                    CenterId = s.CenterId,
                    Quantity = s.Quantity,
                    Type = s.Type,
                    //Status = s.Status
                }).ToList() ?? new List<GetStockDTO>(),
                Users = center.Users?.Select(u => new UserDTO
                {
                    Id = u.Id,
                    UserName = u.UserName ?? string.Empty,
                    Email = u.Email ?? string.Empty,
                    Role = u.Role,
                    PhoneNumber = u.PhoneNumber ?? string.Empty,
                    FirstName = u.FirstName,
                    LastName = u.LastName
                }).ToList() ?? new List<UserDTO>()
            };
    }
}
