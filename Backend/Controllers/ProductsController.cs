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
    public class ProductsController : ControllerBase
    {
        private readonly ApplicationDbContext context;

        public ProductsController(ApplicationDbContext context)
        {
            this.context = context;
        }

        // GET: api/products
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductDTO>>> GetAllProducts()
        {
            return await context.Products
                .Include(x => x.Stocks)
                .Include(c => c.Category)
                .Select(x => new ProductDTO
                {
                    Id = x.Id,
                    CategoryId = x.CategoryId,
                    Name = x.Name,
                    Code = x.Code,
                    CategoryName = x.Category.Name,
                    Quantity = x.Stocks.Sum(s => s.Type == "Ingreso" ? s.Quantity : -s.Quantity),
                    Stocks = x.Stocks.Select(s => new GetStockDTO
                    {
                        Id = s.Id,
                        Quantity = s.Quantity,
                        Type = s.Type
                    }).ToList()
                })
                .ToListAsync();
        }

        // GET: api/Products/id
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductDTO>> GetProductById(int id)
        {
            var prod = await context.Products
                .Include(x => x.Stocks)
                .Include(c => c.Category)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (prod == null)
            {
                return NotFound(new
                {
                    Status = "404",
                    Error = "Not Found",
                    Message = "Producto no encontrado."
                });
            }

            var productDto = new ProductDTO
            {
                Id = prod.Id,
                CategoryId = prod.CategoryId,
                Name = prod.Name,
                Code = prod.Code,
                CategoryName = prod.Category.Name,
                Quantity = prod.Stocks.Sum(s => s.Type == "Ingreso" ? s.Quantity : -s.Quantity),
                Stocks = prod.Stocks.Select(s => new GetStockDTO
                {
                    Id = s.Id,
                    Quantity = s.Quantity,
                    Type = s.Type
                }).ToList()
            };

            return Ok(productDto);
        }

        [HttpGet("search")]
        public async Task<ActionResult> Search([FromQuery] string query)
        {
            if (string.IsNullOrEmpty(query))
            {
                return BadRequest(new
                {
                    Status = "400",
                    Error = "Bad Request",
                    Message = "La consulta no puede estar vacía."
                });
            }
            var products = await context.Products
                .Include(x => x.Stocks)
                .Where(x => x.Name.Contains(query))
                .Select(x => ProductToDTO(x))
                .ToListAsync();
            return Ok(products);
        }

        [HttpGet("filter")]
        public async Task<ActionResult<IEnumerable<ProductDTO>>> GetProductsByFilter(
            [FromQuery] int? categoryId = null,
            [FromQuery] string? sortBy = null,
            [FromQuery] string? order = "asc")
        {

            var query = context.Products
                .Include(x => x.Stocks)
                .Include(c => c.Category)
                .AsQueryable();

            if (categoryId.HasValue && categoryId > 0)
            {
                query = query.Where(x => x.CategoryId == categoryId.Value);
            }

            if (!string.IsNullOrEmpty(sortBy))
            {
                switch (sortBy.ToLower())
                {
                    case "name":
                        query = (order == "desc") ? query.OrderByDescending(x => x.Name) : query.OrderBy(x => x.Name);
                        break;
                    case "quantity":
                        query = (order == "desc")
                            ? query.OrderByDescending(x => x.Stocks
                                .Where(s => s.Type == "Ingreso" || s.Type == "Egreso")
                                .Sum(s => s.Type == "Ingreso" ? s.Quantity : -s.Quantity))
                            : query.OrderBy(x => x.Stocks
                                .Where(s => s.Type == "Ingreso" || s.Type == "Egreso")
                                .Sum(s => s.Type == "Ingreso" ? s.Quantity : -s.Quantity));
                        break;
                }
            }

            var products = await query
        .Select(x => new ProductDTO
        {
            Id = x.Id,
            CategoryId = x.CategoryId,
            Name = x.Name,
            Code = x.Code,
            CategoryName = x.Category.Name,
            Quantity = x.Stocks.Sum(s => s.Type == "Ingreso" ? s.Quantity : -s.Quantity),
            Stocks = x.Stocks.Select(s => new GetStockDTO
            {
                Id = s.Id,
                Quantity = s.Quantity,
                Type = s.Type
            }).ToList()
        })
        .ToListAsync();

            return Ok(products);
        }


        // POST: api/Products
        [HttpPost]
        public async Task<ActionResult<ProductDTO>> AddProduct(AddProductDTO prod)
        {
            // Validaciones correspondientes
            if (prod.CategoryId <= 0 || string.IsNullOrEmpty(prod.Name))
            {
                return BadRequest(new
                {
                    Status = "400",
                    Error = "Bad Request",
                    Message = "Datos inválidos. Asegura que todos los campos se completen."
                });
            }

            // Verificar que la categoría exista
            var category = await context.Categories.FindAsync(prod.CategoryId);
            if (category == null)
            {
                return BadRequest(new
                {
                    Status = "400",
                    Error = "Bad Request",
                    Message = "Categoría no encontrada."
                });
            }

            // Crear la entidad Product a partir del DTO
            var product = new Product
            {
                Name = prod.Name,
                CategoryId = prod.CategoryId,
                Category = category,
                Code = prod.Code
            };

            // Agregar el producto al contexto
            context.Products.Add(product);
            await context.SaveChangesAsync();

            // Retornar el producto creado
            return CreatedAtAction(
                nameof(GetProductById),
                new { id = product.Id },
                ProductToDTO(product));
        }

        // PUT: api/Products/5
        [HttpPut("{id}")]
        public async Task<ActionResult<ProductDTO>> UpdateProduct(int id, ProductDTO productDTO)
        {
            // Validar que el ID del producto en el DTO coincida con el ID del parámetro
            if (id != productDTO.Id)
            {
                return BadRequest(new
                {
                    Status = "400",
                    Error = "Bad Request",
                    Message = "IDs no coinciden en la solicitud."
                });
            }

            // Buscar el producto existente en la base de datos
            var product = await context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound(new
                {
                    Status = "404",
                    Error = "Not Found",
                    Message = "Producto no encontrado."
                });
            }

            // Validar que la categoría proporcionada exista
            var category = await context.Categories.FindAsync(productDTO.CategoryId);
            if (category == null)
            {
                return BadRequest(new
                {
                    Status = "400",
                    Error = "Bad Request",
                    Message = "Categoría no encontrada."
                });
            }

            // Actualizar las propiedades del producto
            product.Name = productDTO.Name;
            product.Code = productDTO.Code;
            product.CategoryId = productDTO.CategoryId;
            product.Category = category;

            // Guardar los cambios en la base de datos
            await context.SaveChangesAsync();

            // Retornar el producto actualizado
            return Ok(ProductToDTO(product));
        }

        // DELETE: api/Products/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            // Buscar el producto en la base de datos
            var product = await context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound(new
                {
                    Status = "404",
                    Error = "Not Found",
                    Message = "Producto no encontrado."
                });
            }

            // Eliminar el producto del contexto
            context.Products.Remove(product);
            await context.SaveChangesAsync();

            // Responder con un estado 204 No Content
            return NoContent();
        }

        private static ProductDTO ProductToDTO(Product p) =>
           new ProductDTO
           {
               Id = p.Id,
               CategoryId = p.CategoryId,
               Name = p.Name,
               Code = p.Code,
               Stocks = p.Stocks?.Select(x => new GetStockDTO
               {
                   Id = x.Id,
                   ProductId = x.ProductId,
                   CenterId = x.CenterId,
                   Quantity = x.Quantity,
                   Type = x.Type,
                   //Status = x.Status,
                   Description = x.Description,
                   Date = x.Date,
                   ExpirationDate = x.ExpirationDate
               }).ToList() ?? new List<GetStockDTO>(),
               OrderLines = p.OrderLines?.Select(x => new OrderLineDTO
               {
                   Id = x.Id,
                   RequestId = x.RequestId,
                   DonationRequestId = x.DonationRequestId,
                   Quantity = x.Quantity,
                   Description = x.Description,
                   ProductId = x.ProductId
               }).ToList() ?? new List<OrderLineDTO>()
           };
    }
}