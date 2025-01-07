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
    [ApiController]
    [Route("api/[controller]")]
    public class OrderLinesController : ControllerBase
    {
        private readonly YourDbContext context;

        public OrderLinesController(YourDbContext context)
        {
            this.context = context;
        }

        [HttpPost]
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

            var donationRequest = orderLineDTO.DonationRequestId.HasValue
                ? await context.DonationRequests.FindAsync(orderLineDTO.DonationRequestId)
                : null;
            if (orderLineDTO.DonationRequestId.HasValue && donationRequest == null)
            {
                return BadRequest(new
                {
                    Status = "400",
                    Error = "Bad Request",
                    Message = "Donation Request not found."
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
                DonationRequestId = orderLineDTO.DonationRequestId,
                Quantity = orderLineDTO.Quantity,
                Description = orderLineDTO.Description,
                Product = product
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
            orderLine.DonationRequestId = orderLineDTO.DonationRequestId;

            await context.SaveChangesAsync();

            return Ok(OrderLineToDTO(orderLine));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<OrderLineDTO>> GetOrderLineById(int id)
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
                DonationRequestId = orderLine.DonationRequestId,
                Quantity = orderLine.Quantity,
                Description = orderLine.Description,
                ProductId = orderLine.Product?.Id
            };
        }
    }
}