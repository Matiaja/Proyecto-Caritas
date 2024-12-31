using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProyectoCaritas.Data;
using ProyectoCaritas.Models;
using ProyectoCaritas.Models.Entities;

namespace ProyectoCaritas.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CentersController : ControllerBase
    {
        private readonly ApplicationDbContext dbContext;

        public CentersController(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        [HttpGet]
        public IActionResult GetAllCenters()
        {
            var allCenters = dbContext.Centers.ToList();
            return Ok(allCenters);
        }

        [HttpGet]
        [Route("{id:guid}")]
        public IActionResult GetCenterById(Guid id)
        {
            var center = dbContext.Centers.Find(id);
            if (center == null)
            {
                return NotFound();
            }
            return Ok(center);
        }

        [HttpPost]
        public IActionResult CreateCenter(AddCenterDto addCenterDto)
        {
            var newCenter = new Center
            {
                Name = addCenterDto.Name,
                Location = addCenterDto.Location,
                Phone = addCenterDto.Phone,
                Email = addCenterDto.Email,
                Manager = addCenterDto.Manager,
                CapacityLimit = addCenterDto.CapacityLimit
            };
            dbContext.Centers.Add(newCenter);
            dbContext.SaveChanges();
            return StatusCode(StatusCodes.Status201Created);
        }

        [HttpPut]
        [Route("{id:guid}")]
        public IActionResult UpdateCenter(Guid id, UpdateCenterDto updateCenterDto)
        {
            var center = dbContext.Centers.Find(id);
            if (center == null)
            {
                return NotFound();
            }
            center.Name = updateCenterDto.Name;
            center.Location = updateCenterDto.Location;
            center.Phone = updateCenterDto.Phone;
            center.Email = updateCenterDto.Email;
            center.Manager = updateCenterDto.Manager;
            center.CapacityLimit = updateCenterDto.CapacityLimit;
            dbContext.SaveChanges();
            return Ok(center);
        }

        [HttpDelete]
        [Route("{id:guid}")]
        public IActionResult DeleteCenter(Guid id)
        {
            var center = dbContext.Centers.Find(id);
            if (center == null)
            {
                return NotFound();
            }
            dbContext.Centers.Remove(center);
            dbContext.SaveChanges();
            return Ok();
        }
    }
}
