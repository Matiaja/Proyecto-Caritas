using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using ProyectoCaritas.Data;
using ProyectoCaritas.Models.DTOs;
using ProyectoCaritas.Models.Entities;
using ProyectoCaritas.Services;

namespace ProyectoCaritas.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StocksController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IStockService _stockService;

        public StocksController(ApplicationDbContext context, IStockService stockService)
        {
            _context = context;
            _stockService = stockService;
        }

        // GET: api/Stocks
        [Authorize]
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
        [Authorize]
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
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<StockDTO>> AddStock([FromBody] StockDTO StockDTO)
        {
            // Validaciones
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized();

            var user = await _context.Users.Include(u => u.Center).FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                return Unauthorized();

            if (user.CenterId != StockDTO.CenterId)
            {
                return BadRequest(new
                {
                    Status = "400",
                    Error = "Bad Request",
                    Message = "You cannot modify stock to a different center."
                });
            }

            try
            {

                var stock = await _stockService.AddStock(StockDTO);

                return CreatedAtAction(nameof(GetStockById), new { id = stock.Id }, StockToDto(stock));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    Status = "400",
                    Error = "Bad Request",
                    Message = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    Status = "400",
                    Error = "Bad Request",
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Status = "500",
                    Error = "Internal Server Error",
                    Message = "An unexpected error occurred: " + ex.Message
                });
            }
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
        [Authorize]
        [HttpGet("validate-quantity")]
        public async Task<IActionResult> ValidateQuantity(int centerId, int productId, int newQuantity, string type)
        {
            Console.WriteLine($">>>>>> Validating stock for CenterId: {centerId}, ProductId: {productId}, NewQuantity: {newQuantity}, Type: {type}");
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
        [Authorize]
        [HttpGet("product-with-stock")]
        public async Task<ActionResult<List<ProductStockDTO>>> GetProductWithStock(
            [FromHeader] string? centerId,
            [FromQuery] int? categoryId = null,
            [FromQuery] string? sortBy = null,
            [FromQuery] string? order = "asc",
            [FromQuery] bool groupByCenter = false)
        {
            /*
            Devuelve todos los productos con stock en un centro específico,
            con la opción de filtrar por categoría y ordenar por nombre o cantidad de stock.
            */

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized();

            var user = await _context.Users.Include(u => u.Center).FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                return Unauthorized();

            IQueryable<Stock> stocksQuery = _context.Stocks
                .Include(s => s.Product)
                    .ThenInclude(p => p.Category);

            // Si no es admin, forzar su propio centro
            if (!User.IsInRole("Admin"))
            {
                if (!user.CenterId.HasValue)
                    return BadRequest(new { message = "User does not belong to any center." });

                int userCenterId = user.CenterId ?? 0;
                stocksQuery = stocksQuery.Where(s => s.CenterId == userCenterId);
            }
            else
            {
                // Admin: si centerId se envía, filtra por ese centro
                if (!string.IsNullOrEmpty(centerId) && centerId.ToLower() != "null")
                {
                    if (!int.TryParse(centerId, out int parsedCenterId))
                        return BadRequest(new { message = "Invalid centerId" });

                    stocksQuery = stocksQuery.Where(s => s.CenterId == parsedCenterId);
                }
                // Si centerId es null o "null", no se filtra => muestra de todos los centros
            }

            if (categoryId.HasValue && categoryId > 0)
            {
                stocksQuery = stocksQuery.Where(s => s.Product.CategoryId == categoryId.Value);
            }

            // Consulta para obtener DonationRequests pendientes
            var donationRequestsQuery = _context.DonationRequests
                .Include(dr => dr.OrderLine)
                .Where(dr => dr.Status == "Aceptada" || dr.Status == "Enviada");

            var grouped = groupByCenter
                ? await stocksQuery
                    .GroupBy(s => new { s.ProductId, s.Product.Name, s.Product.Code, s.CenterId })
                    .Select(g => new
                    {
                        ProductId = (int)g.Key.ProductId,
                        ProductName = g.Key.Name,
                        ProductCode = g.Key.Code,
                        CenterId = g.Key.CenterId,
                        StockQuantity = g.Sum(s => s.Type == "Ingreso" ? s.Quantity : -s.Quantity),
                        // Calcular cantidad asignada desde DonationRequests
                        AssignedQuantity = donationRequestsQuery
                            .Where(dr => dr.AssignedCenterId == g.Key.CenterId && dr.OrderLine.ProductId == g.Key.ProductId)
                            .Sum(dr => dr.Quantity)
                    })
                    .Select(g => new ProductStockDTO
                    {
                        ProductId = g.ProductId,
                        ProductName = g.ProductName,
                        ProductCode = g.ProductCode,
                        CenterId = g.CenterId,
                        StockQuantity = g.StockQuantity,
                        AvailableQuantity = g.StockQuantity - g.AssignedQuantity < 0 ? 0 : g.StockQuantity - g.AssignedQuantity
                    })
                    .ToListAsync()
                : await stocksQuery
                    .GroupBy(s => new { s.ProductId, s.Product.Name, s.Product.Code })
                    .Select(g => new ProductStockDTO
                    {
                        ProductId = (int)g.Key.ProductId,
                        ProductName = g.Key.Name,
                        ProductCode = g.Key.Code,
                        StockQuantity = g.Sum(s => s.Type == "Ingreso" ? s.Quantity : -s.Quantity)
                    })
                    .ToListAsync();

            // Ordenar los resultados
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
            else
            {
                grouped = grouped.OrderBy(x => x.ProductName).ToList();
            }

            return Ok(grouped);
        }

        [HttpGet("more-quantity-product-with-stock")]
        public async Task<ActionResult<List<ProductStockDTO>>> GetMoreQuantityProduct(
            [FromHeader] string centerId)
        {
            /*
            Devuelve los 5 productos con más stock en un centro específico.
            */
            if (!int.TryParse(centerId, out int centerIdInt))
                return BadRequest(new { message = "Invalid centerId" });

            var stocksQuery = _context.Stocks
                .Include(s => s.Product)
                    .ThenInclude(p => p.Category)
                .Where(s => s.CenterId == centerIdInt);

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
                .OrderByDescending(dto => dto.StockQuantity)
                .Take(5)
                .ToListAsync();

            return Ok(grouped);
        }


        [HttpGet("stock-by-category")]
        public async Task<ActionResult<List<CategoryStockDTO>>> GetStockByCategory([FromHeader] string centerId)
        {
            if (!int.TryParse(centerId, out int centerIdInt))
                return BadRequest(new { message = "Invalid centerId" });

            var stocksQuery = _context.Stocks
                .Include(s => s.Product)
                    .ThenInclude(p => p.Category)
                .Where(s => s.CenterId == centerIdInt);

            var groupedByCategory = await stocksQuery
                .GroupBy(s => new { s.Product.CategoryId, s.Product.Category.Name })
                .Select(g => new CategoryStockDTO
                {
                    CategoryId = g.Key.CategoryId,
                    CategoryName = g.Key.Name,
                    TotalStock = g.Sum(s => s.Type == "Ingreso" ? s.Quantity : -s.Quantity)
                })
                .Where(c => c.TotalStock > 0)
                .ToListAsync();

            return Ok(groupedByCategory);
        }


        // GET: api/Stocks/product-with-all-stock
        [Authorize]
        [HttpGet("product-with-all-stocks")]
        public async Task<ActionResult<List<ProductStockDTO>>> GetProductWithAllStocks([FromHeader] string productId)
        {
            /*
            Devuelve la cantidad de stock en todos los centros para un producto específico. 
            */
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

                // Calcular AvailableQuantity para cada centro
                foreach (var stock in productWithStock)
                {
                    stock.AvailableQuantity = await _stockService.GetQuantityAvailable(stock.CenterId ?? 0, stock.ProductId);
                }

                return Ok(productWithStock);
            }
            return BadRequest(new { message = "Invalid productId" });

        }

        // GET: api/Stocks/product-with-stock-for-id"
        [Authorize]
        [HttpGet("product-with-stock-for-id")]
        public async Task<ActionResult<List<GetStockDTO>>> GetProductWithStock([FromHeader] int centerId, [FromHeader] int productId)
        {
            /*
            Devuelve todos los movimientos (ingresos y egresos) de un producto en un centro específico.
            */

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized();

            var user = await _context.Users.Include(u => u.Center).FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                return Unauthorized();

            if (!User.IsInRole("Admin"))
            {
                if (!user.CenterId.HasValue)
                    return BadRequest(new { message = "User does not belong to any center." });

                int userCenterId = user.CenterId ?? 0;
                if (userCenterId != centerId)
                    return new ObjectResult(new
                    {
                        Status = 403,
                        Message = "You cannot access stock moves from a different center."
                    })
                    {
                        StatusCode = StatusCodes.Status403Forbidden
                    };
            }

            var productWithStock = await _context.Stocks
                .Where(s => s.CenterId == centerId && s.ProductId == productId)
                .Include(s => s.Product)
                .Select(s => StockToDto(s))
                .ToListAsync();
            return Ok(productWithStock);
        }


        // GET: api/Stocks/center-stocks
        [HttpGet("center-stocks")]
        public async Task<ActionResult<List<GetStockDTO>>> GetStocksByCenter([FromHeader] string centerId)
        {
            if (!int.TryParse(centerId, out int centerIdInt))
                return BadRequest(new { message = "Invalid centerId" });

            var stocks = await _context.Stocks
                .Where(s => s.CenterId == centerIdInt)
                .Include(s => s.Product)
                .Select(s => StockToDto(s))
                .ToListAsync();

            return Ok(stocks);
        }

        [Authorize]
        [HttpGet("filtered-stock")]
        public async Task<ActionResult<List<ProductStockSummaryDTO>>> GetFilteredStock(
            [FromQuery] int? centerId = null,
            [FromQuery] int? categoryId = null,
            [FromQuery] int? productId = null,
            [FromQuery] DateTime? dateFrom = null,
            [FromQuery] DateTime? dateTo = null)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            var user = await _context.Users.Include(u => u.Center).FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return Unauthorized();

            IQueryable<Stock> query = _context.Stocks
                .Include(s => s.Product)
                .ThenInclude(p => p.Category)
                .Include(s => s.Center);

            // Rol no Admin: forzar su propio centro
            if (!User.IsInRole("Admin"))
            {
                if (!user.CenterId.HasValue)
                    return BadRequest(new { message = "User does not belong to any center." });

                query = query.Where(s => s.CenterId == user.CenterId);
            }
            else if (centerId.HasValue)
            {
                query = query.Where(s => s.CenterId == centerId.Value);
            }

            if (categoryId.HasValue)
                query = query.Where(s => s.Product.CategoryId == categoryId.Value);

            if (productId.HasValue)
                query = query.Where(s => s.ProductId == productId.Value);

            if (dateFrom.HasValue)
                query = query.Where(s => s.Date >= dateFrom.Value);

            if (dateTo.HasValue)
                query = query.Where(s => s.Date <= dateTo.Value);

            var result = await query
                .GroupBy(s => new
                {
                    s.ProductId,
                    s.Product.Name,
                    s.Product.Code,
                    s.Product.CategoryId,
                    CategoryName = s.Product.Category.Name,
                    s.CenterId,
                    CenterName = s.Center.Name
                })
                .Select(g => new ProductStockSummaryDTO
                {
                    ProductId = (int)g.Key.ProductId,
                    ProductName = g.Key.Name,
                    ProductCode = g.Key.Code,
                    CategoryId = g.Key.CategoryId,
                    CategoryName = g.Key.CategoryName,
                    CenterId = g.Key.CenterId,
                    CenterName = g.Key.CenterName,
                    TotalStock = g.Sum(s => s.Type == "Ingreso" ? s.Quantity : -s.Quantity),
                    TotalIngresos = g.Where(s => s.Type == "Ingreso").Sum(s => s.Quantity),
                    TotalEgresos = g.Where(s => s.Type == "Egreso").Sum(s => s.Quantity),
                    LastMovementDate = g.Max(s => s.Date),
                    MovementCount = g.Count()
                })
                .ToListAsync();

            return Ok(result);
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