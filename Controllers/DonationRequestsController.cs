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
    public class DonationRequestsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DonationRequestsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/DonationRequests
        [HttpGet]
        public async Task<ActionResult<IEnumerable<GetDonationRequestDTO>>> GetAllDonationRequests()
        {
            return await _context.DonationRequests
                .Include(dr => dr.AssignedCenter)
                .Include(dr => dr.OrderLines)
                .Select(dr => DonationRequestToDto(dr))
                .ToListAsync();
        }

        // GET: api/DonationRequests/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<GetDonationRequestDTO>> GetDonationRequestById(int id)
        {
            var donationRequest = await _context.DonationRequests
                .Include(dr => dr.AssignedCenter)
                .Include(dr => dr.OrderLines)
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
        public async Task<ActionResult<DonationRequestDTO>> CreateDonationRequest(DonationRequestDTO addDonationRequestDto)
        {
            if (addDonationRequestDto == null)
            {
                return BadRequest(new
                {
                    Status = "400",
                    Error = "Bad Request",
                    Message = "Donation Request data is required."
                });
            }

            if (addDonationRequestDto.AssignedCenterId.HasValue)
            {
                var centerExists = await _context.Centers
                .AnyAsync(c => c.Id == addDonationRequestDto.AssignedCenterId.Value);

                if (!centerExists)
                {
                    return BadRequest(new
                    {
                        Status = "400",
                        Error = "Bad Request",
                        Message = $"The Assigned Center with ID {addDonationRequestDto.AssignedCenterId} does not exist."
                    });
                }
            }

            var donationRequest = new DonationRequest
            {
                AssignedCenterId = addDonationRequestDto.AssignedCenterId,
                ShipmentDate = addDonationRequestDto.ShipmentDate,
                ReceptionDate = addDonationRequestDto.ReceptionDate,
                Status = addDonationRequestDto.Status
            };
            _context.DonationRequests.Add(donationRequest);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetDonationRequestById), new { id = donationRequest.Id }, DonationRequestToDto(donationRequest));

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
            if (updateDonationRequestDto.AssignedCenterId.HasValue)
            {
                var centerExists = await _context.Centers
                .AnyAsync(c => c.Id == updateDonationRequestDto.AssignedCenterId.Value);
                if (!centerExists)
                {
                    return BadRequest(new
                    {
                        Status = "400",
                        Error = "Bad Request",
                        Message = $"The Assigned Center with ID {updateDonationRequestDto.AssignedCenterId} does not exist."
                    });
                }
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
                ShipmentDate = donationRequest.ShipmentDate,
                ReceptionDate = donationRequest.ReceptionDate,
                Status = donationRequest.Status,
                OrderLines = donationRequest.OrderLines
            };
    }
}
