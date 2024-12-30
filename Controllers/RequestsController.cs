using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
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
    public class RequestsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public RequestsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Requests
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RequestDTO>>> GetAllRequests()
        {
            return await _context.Requests
                .Select(x => RequestToDTO(x))
                .ToListAsync();
        }

        // GET: api/Requests/id
        [HttpGet("{id}")]
        public async Task<ActionResult<RequestDTO>> GetRequestById(int id)
        {
            var request = await _context.Requests.FindAsync(id);

            if (request == null)
            {
                return NotFound();
            }

            return RequestToDTO(request);
        }

        // POST: api/Requests
        [HttpPost]
        public async Task<ActionResult<RequestDTO>> AddRequest(RequestDTO requestDTO)
        {
            var requestingCenter = await _context.Centers.FindAsync(requestDTO.RequestingCenterId);
            if (requestingCenter == null)
            {
                return BadRequest("Requesting center not found.");
            }

            var request = new Request
            {
                RequestingCenterId = requestDTO.RequestingCenterId,
                UrgencyLevel = requestDTO.UrgencyLevel,
                RequestDate = requestDTO.RequestDate,
                RequestingCenter = requestingCenter
            };

            _context.Requests.Add(request);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetRequestById),
                new { id = request.Id },
                RequestToDTO(request));
        }

        // PUT: api/Requests/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRequest(int id, RequestDTO updateRequestDTO)
        {

            // Recuperar la entidad existente de la base de datos
            var existingRequest = await _context.Requests.FindAsync(id);
            if (existingRequest == null)
            {
                return NotFound();
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
                    return NotFound();
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
                return NotFound();
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
               //Id = request.Id,
               RequestingCenterId = request.RequestingCenterId,
               UrgencyLevel = request.UrgencyLevel,
               RequestDate = request.RequestDate
           };
    }
}
