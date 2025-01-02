using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoCaritas.Data;
using ProyectoCaritas.Models.DTOs;
using ProyectoCaritas.Models.Entities;

namespace ProyectoCaritas.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StocksController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public StocksController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Stocks
        [HttpGet]
        public async Task<ActionResult<IEnumerable<GetStockDTO>>> GetAllStocks()
        {
            return await _context.Stocks
                .Include(s => s.Center)
                .Include(s => s.Product)
                .Select(s => StockToDto(s))
                .ToListAsync();
        }

        // GET: api/Stocks/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<GetStockDTO>> GetStockById(int id)
        {
            var stock = await _context.Stocks
                .Include(s => s.Center)
                .Include(s => s.Product)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (stock == null)
            {
                return NotFound(new
                {
                    Status = "404",
                    Error = "Not Found",
                    Message = "Stock not found."
                });
            }

            return StockToDto(stock);
        }

        // POST: api/Stocks
        [HttpPost]
        public async Task<ActionResult<StockDTO>> AddStock(StockDTO StockDTO)
        {
            // Validaciones
            if (StockDTO.CenterId <= 0 || string.IsNullOrEmpty(StockDTO.Status))
            {
                return BadRequest(new
                {
                    Status = "400",
                    Error = "Bad Request",
                    Message = "Invalid data. Ensure all required fields are provided."
                });
            }

            var center = await _context.Centers.FindAsync(StockDTO.CenterId);
            if (center == null)
            {
                return BadRequest(new
                {
                    Status = "400",
                    Error = "Bad Request",
                    Message = "Center not found."
                });
            }
            if (StockDTO.ProductId.HasValue)
            {
                var product = await _context.Products.FindAsync(StockDTO.ProductId);
                if (product == null)
                {
                    return BadRequest(new
                    {
                        Status = "400",
                        Error = "Bad Request",
                        Message = "Product not found."
                    });
                }
            }
            var stock = new Stock
            {
                CenterId = StockDTO.CenterId,
                ProductId = StockDTO.ProductId,
                EntryDate = StockDTO.EntryDate,
                ExpirationDate = StockDTO.ExpirationDate,
                Description = StockDTO.Description,
                Quantity = StockDTO.Quantity,
                Weight = StockDTO.Weight,
                Status = StockDTO.Status,
                Center = center
            };

            _context.Stocks.Add(stock);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetStockById), new { id = stock.Id }, StockToDto(stock));
        }

        // PUT: api/Stocks/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateStock(int id, StockDTO updateGetStockDTO)
        {
            if (updateGetStockDTO.CenterId < 0 || string.IsNullOrEmpty(updateGetStockDTO.Status))
            {
                return BadRequest(new
                {
                    Status = "400",
                    Error = "Bad Request",
                    Message = "Invalid data. Ensure all required fields are provided."
                });
            }

            var existingStock = await _context.Stocks.FindAsync(id);
            if (existingStock == null)
            {
                return NotFound(new
                {
                    Status = "404",
                    Error = "Not Found",
                    Message = "Stock not found."
                });
            }

            var centerExists = await _context.Centers.AnyAsync(c => c.Id == updateGetStockDTO.CenterId);
            if (!centerExists)
            {
                return BadRequest(new
                {
                    Status = "400",
                    Error = "Bad Request",
                    Message = "The provided CenterId does not exist."
                });
            }
            if (updateGetStockDTO.ProductId.HasValue)
            {
                var productExists = await _context.Products.AnyAsync(p => p.Id == updateGetStockDTO.ProductId);
                if (!productExists)
                {
                    return BadRequest(new
                    {
                        Status = "400",
                        Error = "Bad Request",
                        Message = "The provided ProductId does not exist."
                    });
                }
            }

            existingStock.CenterId = updateGetStockDTO.CenterId;
            existingStock.ProductId = updateGetStockDTO.ProductId;
            existingStock.EntryDate = updateGetStockDTO.EntryDate;
            existingStock.ExpirationDate = updateGetStockDTO.ExpirationDate;
            existingStock.Description = updateGetStockDTO.Description;
            existingStock.Quantity = updateGetStockDTO.Quantity;
            existingStock.Weight = updateGetStockDTO.Weight;
            existingStock.Status = updateGetStockDTO.Status;

            _context.Entry(existingStock).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StockExists(id))
                {
                    return NotFound(new
                    {
                        Status = "404",
                        Error = "Not Found",
                        Message = "Stock not found during concurrency check."
                    });
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/Stocks/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStock(int id)
        {
            var stock = await _context.Stocks.FindAsync(id);
            if (stock == null)
            {
                return NotFound(new
                {
                    Status = "404",
                    Error = "Not Found",
                    Message = "Stock not found."
                });
            }

            _context.Stocks.Remove(stock);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool StockExists(int id)
        {
            return _context.Stocks.Any(s => s.Id == id);
        }

        private static GetStockDTO StockToDto(Stock stock) =>
            new GetStockDTO
            {
                Id = stock.Id,
                CenterId = stock.CenterId,
                ProductId = stock.ProductId,
                EntryDate = stock.EntryDate,
                ExpirationDate = stock.ExpirationDate,
                Description = stock.Description,
                Quantity = stock.Quantity,
                Weight = stock.Weight,
                Status = stock.Status
            };
    }
}
