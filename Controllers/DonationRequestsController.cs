using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProyectoCaritas.Data;
using ProyectoCaritas.Models;
using ProyectoCaritas.Models.Entities;

namespace ProyectoCaritas.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DonationRequestsController : ControllerBase
    {
        private readonly ApplicationDbContext dbContext;

        public DonationRequestsController(ApplicationDbContext dbContext) 
        {
            this.dbContext = dbContext;
        }

        [HttpGet]
        public IActionResult GetAllDonationRequests()
        {
            var allDonationRequests = dbContext.DonationRequests.ToList();
            return Ok(allDonationRequests);
        }

        [HttpGet]
        [Route("{id:guid}")]
        public IActionResult GetDonationRequestById(Guid id)
        {
            var donationRequest = dbContext.DonationRequests.Find(id);
            if (donationRequest == null)
            {
                return NotFound();
            }
            return Ok(donationRequest);
        }

        [HttpPost]
        public IActionResult CreateDonationRequest(AddDonationRequestDto addDonationRequestDto)
        {
            var newDonationRequest = new DonationRequest
            {
                ShipmentDate = addDonationRequestDto.ShipmentDate,
                ReceptionDate = addDonationRequestDto.ReceptionDate,
                Status = addDonationRequestDto.Status
            };
            dbContext.DonationRequests.Add(newDonationRequest);
            dbContext.SaveChanges();
            return StatusCode(StatusCodes.Status201Created);
        }

        [HttpPut]
        [Route("{id:guid}")]
        public IActionResult UpdateDonationRequest(Guid id, UpdateDonationRequestDto updateDonationRequestDto)
        {
            var donationRequest = dbContext.DonationRequests.Find(id);
            if (donationRequest == null)
            {
                return NotFound();
            }
            donationRequest.ShipmentDate = updateDonationRequestDto.ShipmentDate;
            donationRequest.ReceptionDate = updateDonationRequestDto.ReceptionDate;
            donationRequest.Status = updateDonationRequestDto.Status;
            dbContext.SaveChanges();
            return Ok(donationRequest);
        }

        [HttpDelete]
        [Route("{id:guid}")]
        public IActionResult DeleteDonationRequest(Guid id)
        {
            var donationRequest = dbContext.DonationRequests.Find(id);
            if (donationRequest == null)
            {
                return NotFound();
            }
            dbContext.DonationRequests.Remove(donationRequest);
            dbContext.SaveChanges();
            return Ok();
        }
    }
}
