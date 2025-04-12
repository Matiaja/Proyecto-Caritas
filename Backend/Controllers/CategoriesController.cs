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
        public async Task<ActionResult<IEnumerable<GetCategoryDTO>>> GetAllCategories()
        {
            return await _context.Categories
                .Include(c => c.Products)
                .Select(c => CategoryToDto(c))
                .ToListAsync();
        }

        // GET: api/Categories/{id}
        [HttpGet("{id}")]
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

            var category = new Category
            {
                Name = addCategoryDto.Name,
                Description = addCategoryDto.Description
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCategoryById), new { id = category.Id }, CategoryToDto(category));

        }

        // PUT: api/Categories/{id}
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
            category.Name = updateCategoryDto.Name;
            category.Description = updateCategoryDto.Description;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/Categories/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
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
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("filter")]
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
