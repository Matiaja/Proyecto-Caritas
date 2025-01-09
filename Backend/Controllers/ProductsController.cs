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
                .Select(x => ProductToDTO(x))
                .ToListAsync();
        }

        // GET: api/Products/id
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductDTO>> GetProductById(int id)
        {
            var prod = await context.Products.FindAsync(id);

            if (prod == null)
            {
                return NotFound(new
                {
                    Status = "404",
                    Error = "Not Found",
                    Message = "Producto no encontrado."
                });
            }

            return ProductToDTO(prod);
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
                Category = category
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
               Category = p.Category
           };
    }
}