using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
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
            //if (StockDTO.CenterId <= 0 || string.IsNullOrEmpty(StockDTO.Status))
            //{
            //    return BadRequest(new
            //    {
            //        Status = "400",
            //        Error = "Bad Request",
            //        Message = "Invalid data. Ensure all required fields are provided."
            //    });
            //}

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

            var result = await ValidateQuantity(StockDTO.CenterId, (int)StockDTO.ProductId, StockDTO.Quantity, StockDTO.Type);

            if (result is BadRequestObjectResult badRequest)
            {
                return BadRequest(badRequest.Value);
            }

            if (result is OkObjectResult okResult)
            {
                var totalQuantity = (int)((dynamic)okResult.Value).totalStock;

                if (totalQuantity >= 0)
                {

                    var stock = new Stock
                    {
                        CenterId = StockDTO.CenterId,
                        ProductId = StockDTO.ProductId,
                        Date = StockDTO.Date,
                        ExpirationDate = StockDTO.ExpirationDate,
                        Description = StockDTO.Description,
                        Quantity = StockDTO.Quantity,
                        Weight = StockDTO.Weight,
                        Type = StockDTO.Type,
                        Center = center
                    };

                    _context.Stocks.Add(stock);
                    await _context.SaveChangesAsync();

                    return CreatedAtAction(nameof(GetStockById), new { id = stock.Id }, StockToDto(stock));
                }
            }

            return BadRequest(new { message = "Invalid Quantity" });
        }

        // PUT: api/Stocks/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateStock(int id, StockDTO updateGetStockDTO)
        {
            //if (updateGetStockDTO.CenterId < 0 || string.IsNullOrEmpty(updateGetStockDTO.Status))
            //{
            //    return BadRequest(new
            //    {
            //        Status = "400",
            //        Error = "Bad Request",
            //        Message = "Invalid data. Ensure all required fields are provided."
            //    });
            //}

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
            existingStock.Date = updateGetStockDTO.Date;
            existingStock.ExpirationDate = updateGetStockDTO.ExpirationDate;
            existingStock.Description = updateGetStockDTO.Description;
            existingStock.Quantity = updateGetStockDTO.Quantity;
            existingStock.Weight = updateGetStockDTO.Weight;
            //existingStock.Status = updateGetStockDTO.Status;
            existingStock.Type = updateGetStockDTO.Type;

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

        // GET: api/Stocks/validate-quantity
        [HttpGet("validate-quantity")]
        public async Task<IActionResult> ValidateQuantity(int centerId, int productId, int newQuantity, string type)
        {
            var stocks = await _context.Stocks
                .Where(s => s.CenterId == centerId && s.ProductId == productId)
                .ToListAsync();

            int totalStock = stocks
                .Sum(s => s.Type == "Ingreso" ? s.Quantity : -s.Quantity);

            if (type == "Egreso")
            {
                if (totalStock < newQuantity)
                {
                    return BadRequest(new { message = "El stock no puede ser negativo." });
                }
                return Ok(new { totalStock = totalStock - newQuantity });
            }

            return Ok(new { totalStock = totalStock + newQuantity });


        }

        // GET: api/Stocks/product-with-stock
        [HttpGet("product-with-stock")]
        public async Task<ActionResult<List<ProductStockDTO>>> GetProductWithStock(
            [FromHeader] string centerId,
            [FromQuery] int? categoryId = null,
            [FromQuery] string? sortBy = null,
            [FromQuery] string? order = "asc")
        {
            if (!int.TryParse(centerId, out int centerIdInt))
                return BadRequest(new { message = "Invalid centerId" });

            var stocksQuery = _context.Stocks
                .Include(s => s.Product)
                    .ThenInclude(p => p.Category)
                .Where(s => s.CenterId == centerIdInt);

            if (categoryId.HasValue && categoryId > 0)
            {
                stocksQuery = stocksQuery.Where(s => s.Product.CategoryId == categoryId.Value);
            }

            var grouped = await stocksQuery
                   .GroupBy(s => new { s.ProductId, s.Product.Name, s.Product.Code })
                   .Select(g => new ProductStockDTO
                   {
                       ProductId = (int)g.Key.ProductId,
                       ProductName = g.Key.Name,
                       ProductCode = g.Key.Code,
                       StockQuantity = g.Sum(s => s.Type == "Ingreso" ? s.Quantity : -s.Quantity)
                   })
                   .Where(dto => dto.StockQuantity > 0)
                   .ToListAsync();

            if (!string.IsNullOrEmpty(sortBy))
            {
                switch (sortBy.ToLower())
                {
                    case "productname":
                        grouped = (order == "desc")
                            ? grouped.OrderByDescending(x => x.ProductName).ToList()
                            : grouped.OrderBy(x => x.ProductName).ToList();
                        break;
                    case "stockquantity":
                        grouped = (order == "desc")
                            ? grouped.OrderByDescending(x => x.StockQuantity).ToList()
                            : grouped.OrderBy(x => x.StockQuantity).ToList();
                        break;
                }
            }

            return Ok(grouped);
        }

        // GET: api/Stocks/product-with-all-stock
        [HttpGet("product-with-all-stocks")]
        public async Task<ActionResult<List<ProductStockDTO>>> GetProductWithAllStocks([FromHeader] string productId)
        {
            if (int.TryParse(productId, out int productIdInt))
            {
                var productWithStock = await _context.Stocks
                    .Where(s => s.ProductId == productIdInt)
                    .Include(s => s.Center)
                    .GroupBy(s => new { s.ProductId, s.Product.Name, s.Product.Code, s.CenterId, CenterName = s.Center.Name })
                    .Select(s => new ProductStockDTO
                    {
                        ProductId = (int)s.Key.ProductId,
                        ProductName = s.Key.Name,
                        ProductCode = s.Key.Code,
                        CenterId = s.Key.CenterId,
                        CenterName = s.Key.CenterName,
                        StockQuantity = s.Sum(s => s.Type == "Ingreso" ? s.Quantity : -s.Quantity)
                    })
                    .Where(p => p.StockQuantity > 0)
                    .ToListAsync();

                return Ok(productWithStock);
            }
            return BadRequest(new { message = "Invalid productId" });

        }


        // GET: api/Stocks/product-with-stock-for-id"
        [HttpGet("product-with-stock-for-id")]
        public async Task<ActionResult<List<GetStockDTO>>> GetProductWithStock([FromHeader] int centerId, [FromHeader] int productId)
        {
            var productWithStock = await _context.Stocks
                .Where(s => s.CenterId == centerId && s.ProductId == productId)
                .Include(s => s.Product)
                .Select(s => StockToDto(s))
                .ToListAsync();
            return Ok(productWithStock);
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
                Date = stock.Date,
                ExpirationDate = stock.ExpirationDate,
                Description = stock.Description,
                Quantity = stock.Quantity,
                Weight = stock.Weight,
                Type = stock.Type,
                Product = stock.Product != null ? new GetProductDTO
                {
                    Name = stock.Product.Name,
                    Code = stock.Product.Code,
                    CategoryId = stock.Product.CategoryId,
                } : null
                //Status = stock.Status
            };
    }
}
