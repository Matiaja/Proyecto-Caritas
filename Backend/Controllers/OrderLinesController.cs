using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoCaritas.Data;
using ProyectoCaritas.Models.DTOs;
using ProyectoCaritas.Models.Entities;

namespace ProyectoCaritas.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderLinesController : ControllerBase
    {
        private readonly ApplicationDbContext context;

        public OrderLinesController(ApplicationDbContext context)
        {
            this.context = context;
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<OrderLineDTO>> AddOrderLine(OrderLineDTO orderLineDTO)
        {
            // Validaciones
            if (orderLineDTO.Quantity <= 0)
            {
                return BadRequest(new
                {
                    Status = "400",
                    Error = "Bad Request",
                    Message = "Quantity must be greater than 0."
                });
            }

            // Validar RequestId, DonationRequestId o ProductId si existen
            var request = orderLineDTO.RequestId.HasValue
                ? await context.Requests.FindAsync(orderLineDTO.RequestId)
                : null;
            if (orderLineDTO.RequestId.HasValue && request == null)
            {
                return BadRequest(new
                {
                    Status = "400",
                    Error = "Bad Request",
                    Message = "Request not found."
                });
            }

            var product = orderLineDTO.ProductId.HasValue
                ? await context.Products.FindAsync(orderLineDTO.ProductId)
                : null;
            if (orderLineDTO.ProductId.HasValue && product == null)
            {
                return BadRequest(new
                {
                    Status = "400",
                    Error = "Bad Request",
                    Message = "Product not found."
                });
            }

            // Crear la entidad OrderLine
            var orderLine = new OrderLine
            {
                RequestId = orderLineDTO.RequestId,
                Quantity = orderLineDTO.Quantity,
                Description = orderLineDTO.Description,
                Product = product,
                Status = orderLineDTO.Status ?? "",
            };

            context.OrderLines.Add(orderLine);
            await context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetOrderLineById),
                new { id = orderLine.Id },
                OrderLineToDTO(orderLine));
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<OrderLineDTO>> UpdateOrderLine(int id, OrderLineDTO orderLineDTO)
        {
            if (id != orderLineDTO.Id)
            {
                return BadRequest(new
                {
                    Status = "400",
                    Error = "Bad Request",
                    Message = "ID mismatch between route and body."
                });
            }

            var orderLine = await context.OrderLines.FindAsync(id);
            if (orderLine == null)
            {
                return NotFound(new
                {
                    Status = "404",
                    Error = "Not Found",
                    Message = "OrderLine not found."
                });
            }

            var product = orderLineDTO.ProductId.HasValue
                ? await context.Products.FindAsync(orderLineDTO.ProductId)
                : null;
            if (orderLineDTO.ProductId.HasValue && product == null)
            {
                return BadRequest(new
                {
                    Status = "400",
                    Error = "Bad Request",
                    Message = "Product not found."
                });
            }

            orderLine.Quantity = orderLineDTO.Quantity;
            orderLine.Description = orderLineDTO.Description;
            orderLine.Product = product;
            orderLine.RequestId = orderLineDTO.RequestId;

            await context.SaveChangesAsync();

            return Ok(OrderLineToDTO(orderLine));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<OrderLineDTO>> GetOrderLineById(int id)
        {
            var orderLine = await context.OrderLines
                .Include(ol => ol.Request!)
                    .ThenInclude(r => r.RequestingCenter)
                .Include(ol => ol.Product)
                .Include(ol => ol.DonationRequests!)
                    .ThenInclude(dr => dr.AssignedCenter)
                .FirstOrDefaultAsync(ol => ol.Id == id);

            if (orderLine == null)
            {
                return NotFound(new
                {
                    Status = "404",
                    Error = "Not Found",
                    Message = "OrderLine not found."
                });
            }

            return Ok(OrderLineToDTO(orderLine));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrderLine(int id)
        {
            var orderLine = await context.OrderLines.FindAsync(id);
            if (orderLine == null)
            {
                return NotFound(new
                {
                    Status = "404",
                    Error = "Not Found",
                    Message = "OrderLine not found."
                });
            }

            context.OrderLines.Remove(orderLine);
            await context.SaveChangesAsync();

            return NoContent();
        }

        private OrderLineDTO OrderLineToDTO(OrderLine orderLine)
        {
            return new OrderLineDTO
            {
                Id = orderLine.Id,
                RequestId = orderLine.RequestId,
                Quantity = orderLine.Quantity,
                Description = orderLine.Description,
                ProductId = orderLine.ProductId,
                Product = orderLine.Product != null ? new ProductDTO
                {
                    Id = orderLine.Product.Id,
                    Name = orderLine.Product.Name,
                    CategoryId = orderLine.Product.CategoryId
                } : null,
                Request = orderLine.Request != null ? new RequestBasicDTO
                {
                    Id = orderLine.Request.Id,
                    RequestDate = orderLine.Request.RequestDate,
                    UrgencyLevel = orderLine.Request.UrgencyLevel,
                    RequestingCenterId = orderLine.Request.RequestingCenterId,
                    RequestingCenter = orderLine.Request.RequestingCenter != null ? new GetCenterDTO
                    {
                        Id = orderLine.Request.RequestingCenter.Id,
                        Name = orderLine.Request.RequestingCenter.Name,
                        Location = orderLine.Request.RequestingCenter.Location,
                        Manager = orderLine.Request.RequestingCenter.Manager,
                        Phone = orderLine.Request.RequestingCenter.Phone
                    } : null
                } : null,
                DonationRequests = orderLine.DonationRequests?.Select(dr => new GetDonationRequestDTO
                {
                    Id = dr.Id,
                    AssignedCenterId = dr.AssignedCenterId,
                    AssignedCenter = dr.AssignedCenter != null ? new GetCenterDTO
                    {
                        Id = dr.AssignedCenter.Id,
                        Name = dr.AssignedCenter.Name,
                        Location = dr.AssignedCenter.Location,
                        Manager = dr.AssignedCenter.Manager,
                        Phone = dr.AssignedCenter.Phone
                    } : null,
                    OrderLineId = dr.OrderLineId,
                    Quantity = dr.Quantity,
                    Status = dr.Status
                }).ToList()
            };
        }
    }
}