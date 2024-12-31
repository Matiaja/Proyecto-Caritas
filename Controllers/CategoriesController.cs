using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProyectoCaritas.Data;
using ProyectoCaritas.Models;
using ProyectoCaritas.Models.Entities;

namespace ProyectoCaritas.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly ApplicationDbContext dbContext;

        public CategoriesController(ApplicationDbContext dbContext) {
            this.dbContext = dbContext;
        }
        [HttpGet]
        public IActionResult GetAllCateogies()
        {
           var allCategories = dbContext.Categories.ToList();

           return Ok(allCategories);
        }

        [HttpGet]
        [Route("{id:guid}")]
        public IActionResult GetCategoryById(Guid id)
        {
            var category = dbContext.Categories.Find(id);
            if (category == null)
            {
                return NotFound();
            }
            return Ok(category);
        }

        [HttpPost]
        public IActionResult CreateCategory(AddCategoryDto addCategoryDto)
        {
            var newCategory = new Category
            {
                Name = addCategoryDto.Name,
                Description = addCategoryDto.Description
            };
            dbContext.Categories.Add(newCategory);
            dbContext.SaveChanges();
            return StatusCode(StatusCodes.Status201Created);
        }

        [HttpPut]
        [Route("{id:guid}")]
        public IActionResult UpdateCategory(Guid id, UpdateCategoryDto updateCategoryDto)
        {
            var category = dbContext.Categories.Find(id);
            if (category == null)
            {
                return NotFound();
            }
            category.Name = updateCategoryDto.Name;
            category.Description = updateCategoryDto.Description;
            dbContext.SaveChanges();
            return Ok();
        }

        [HttpDelete]
        [Route("{id:guid}")]
        public IActionResult DeleteCategory(Guid id)
        {
            var category = dbContext.Categories.Find(id);
            if (category == null)
            {
                return NotFound();
            }
            dbContext.Categories.Remove(category);
            dbContext.SaveChanges();
            return Ok();
        }
    }
}
