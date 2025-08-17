using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoCaritas.Data;
using ProyectoCaritas.Models.DTOs;
using ProyectoCaritas.Models.Entities;
using System.Globalization;

namespace ProyectoCaritas.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CategoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Categories
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<GetCategoryDTO>>> GetAllCategories()
        {
            return await _context.Categories
                .Include(c => c.Products)
                .Select(c => CategoryToDto(c))
                .ToListAsync();
        }

        // GET: api/Categories/{id}
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<GetCategoryDTO>> GetCategoryById(int id)
        {
            var category = await _context.Categories
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
            {
                return NotFound(new
                {
                    Status = "404",
                    Error = "Not Found",
                    Message = "Category not found."
                });
            }

            return CategoryToDto(category);


        }

        // POST: api/Categories
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<CategoryDTO>> CreateCategory(CategoryDTO addCategoryDto)
        {
            if (addCategoryDto == null)
            {
                return BadRequest(new
                {
                    Status = "400",
                    Error = "Bad Request",
                    Message = "Category data is required."
                });
            }

            // Validate the name exists and is only words from a to z
            if (string.IsNullOrWhiteSpace(addCategoryDto.Name))
            {
                return BadRequest(new
                {
                    Status = "400",
                    Error = "Bad Request",
                    Message = "El nombre de la categoría es obligatorio."
                });
            }
            if (!System.Text.RegularExpressions.Regex.IsMatch(addCategoryDto.Name, @"^[a-zA-Z\s]+$"))
            {
                return BadRequest(new
                {
                    Status = "400",
                    Error = "Bad Request",
                    Message = "El nombre de la categoría solo puede contener letras y espacios."
                });
            }

            // capitalize name
            addCategoryDto.Name = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(addCategoryDto.Name.ToLower());
            var category = new Category
            {
                Name = addCategoryDto.Name.Trim(),
                Description = addCategoryDto.Description?.Trim()
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCategoryById), new { id = category.Id }, CategoryToDto(category));

        }

        // PUT: api/Categories/{id}
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory(int id, CategoryDTO updateCategoryDto)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound(new
                {
                    Status = "404",
                    Error = "Not Found",
                    Message = "Category not found."
                });
            }
            // Validate the name exists and is only words from a to z
            if (string.IsNullOrWhiteSpace(updateCategoryDto.Name))
            {
                return BadRequest(new
                {
                    Status = "400",
                    Error = "Bad Request",
                    Message = "El nombre de la categoría es obligatorio."
                });
            }
            if (!System.Text.RegularExpressions.Regex.IsMatch(updateCategoryDto.Name, @"^[a-zA-Z\s]+$"))
            {
                return BadRequest(new
                {
                    Status = "400",
                    Error = "Bad Request",
                    Message = "El nombre de la categoría solo puede contener letras y espacios."
                });
            }

            // capitalize name
            updateCategoryDto.Name = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(updateCategoryDto.Name.ToLower());
            category.Name = updateCategoryDto.Name.Trim();
            category.Description = updateCategoryDto.Description?.Trim();
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/Categories/{id}
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.Categories
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
            {
                return NotFound(new
                {
                    Status = "404",
                    Error = "Not Found",
                    Message = "Category not found."
                });
            }

            if (category.Products?.Count != 0)
                return BadRequest("No se puede eliminar la categoría porque tiene productos asociados.");

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("filter")]
        [Authorize]
        public async Task<ActionResult<List<GetCategoryDTO>>> GetCategoriesByFilter(
            [FromQuery] string? sortBy = null,
            [FromQuery] string? order = "asc")

        {
            var categories = await _context.Categories
                .Select(c => new GetCategoryDTO
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description
                })
                .ToListAsync();
            if (sortBy == null)
            {
                return categories;
            }
            if (sortBy == "name")
            {
                if (order == "asc")
                {
                    return categories.OrderBy(c => c.Name).ToList();
                }
                else if (order == "desc")
                {
                    return categories.OrderByDescending(c => c.Name).ToList();
                }
            }
            return BadRequest(new
            {
                Status = "400",
                Error = "Bad Request",
                Message = "Invalid sortBy parameter."
            });
        }


        private static GetCategoryDTO CategoryToDto(Category category) =>
            new GetCategoryDTO
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                Products = category.Products?.Select(p => new ProductDTO
                {
                    Id = p.Id,
                    Name = p.Name,
                    CategoryId = p.CategoryId
                }).ToList() ?? new List<ProductDTO>()
            };
    }
}
