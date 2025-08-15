using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoCaritas.Data;
using ProyectoCaritas.Models.DTOs;
using ProyectoCaritas.Models.Entities;

namespace ProyectoCaritas.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ReportsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/reports/movements-report
    [HttpGet("movements-report")]
    public async Task<ActionResult<IEnumerable<ProductStockSummaryDTO>>> GetMovementsReport(
        [FromQuery] int? centerId,
        [FromQuery] int? categoryId,
        [FromQuery] int? productId,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate)
    {
        var purchasesQ = _context.Purchases
            .Include(p => p.Items).ThenInclude(i => i.Product).ThenInclude(pr => pr.Category)
            .AsQueryable();

        var distributionsQ = _context.Distributions
            .Include(d => d.Items)
                .ThenInclude(it => it.ItemPurchase)
                    .ThenInclude(ip => ip.Product)
                        .ThenInclude(pr => pr.Category)
            .Include(d => d.Purchase)
            .AsQueryable();

        // Filters
        if (centerId.HasValue)
        {
            purchasesQ = purchasesQ.Where(p => p.CenterId == centerId.Value);
            distributionsQ = distributionsQ.Where(d => d.Purchase != null && d.Purchase.CenterId == centerId.Value);
        }
        if (fromDate.HasValue)
        {
            purchasesQ = purchasesQ.Where(p => p.PurchaseDate >= fromDate.Value);
            distributionsQ = distributionsQ.Where(d => d.DeliveryDate >= fromDate.Value);
        }
        if (toDate.HasValue)
        {
            purchasesQ = purchasesQ.Where(p => p.PurchaseDate <= toDate.Value);
            distributionsQ = distributionsQ.Where(d => d.DeliveryDate <= toDate.Value);
        }

        var purchaseEventsQ = purchasesQ.SelectMany(p => p.Items.Select(i => new
        {
            i.ProductId,
            ProductName = i.Product.Name,
            ProductCode = i.Product.Code,
            CategoryId = i.Product.CategoryId,
            CategoryName = i.Product.Category.Name,
            Quantity = i.Quantity,
            Date = p.PurchaseDate
        }));

        if (categoryId.HasValue)
            purchaseEventsQ = purchaseEventsQ.Where(e => e.CategoryId == categoryId.Value);
        if (productId.HasValue)
            purchaseEventsQ = purchaseEventsQ.Where(e => e.ProductId == productId.Value);

        var distributionEventsQ = distributionsQ.SelectMany(d => d.Items.Select(i => new
        {
            ProductId = i.ItemPurchase.ProductId,
            ProductName = i.ItemPurchase.Product.Name,
            ProductCode = i.ItemPurchase.Product.Code,
            CategoryId = i.ItemPurchase.Product.CategoryId,
            CategoryName = i.ItemPurchase.Product.Category.Name,
            Quantity = i.Quantity,
            Date = d.DeliveryDate
        }));

        if (categoryId.HasValue)
            distributionEventsQ = distributionEventsQ.Where(e => e.CategoryId == categoryId.Value);
        if (productId.HasValue)
            distributionEventsQ = distributionEventsQ.Where(e => e.ProductId == productId.Value);

        var ingresosList = await purchaseEventsQ.ToListAsync();
        var egresosList = await distributionEventsQ.ToListAsync();

        var ingresosAgg = ingresosList
            .GroupBy(e => new { e.ProductId, e.ProductName, e.ProductCode, e.CategoryId, e.CategoryName })
            .ToDictionary(g => new { g.Key.ProductId, g.Key.CategoryId }, g => new
            {
                g.Key.ProductId,
                g.Key.ProductName,
                g.Key.ProductCode,
                g.Key.CategoryId,
                g.Key.CategoryName,
                TotalIngresos = g.Sum(x => x.Quantity),
                LastIngreso = g.Max(x => x.Date)
            });

        var egresosAgg = egresosList
            .GroupBy(e => new { e.ProductId, e.ProductName, e.ProductCode, e.CategoryId, e.CategoryName })
            .ToDictionary(g => new { g.Key.ProductId, g.Key.CategoryId }, g => new
            {
                g.Key.ProductId,
                g.Key.ProductName,
                g.Key.ProductCode,
                g.Key.CategoryId,
                g.Key.CategoryName,
                TotalEgresos = g.Sum(x => x.Quantity),
                LastEgreso = g.Max(x => x.Date)
            });

        var allKeys = ingresosAgg.Keys.Union(egresosAgg.Keys).ToList();

        string centerName = string.Empty;
        if (centerId.HasValue)
        {
            centerName = await _context.Centers
                .Where(c => c.Id == centerId.Value)
                .Select(c => c.Name)
                .FirstOrDefaultAsync() ?? string.Empty;
        }

        var productCodeLookup = ingresosList.Select(i => new { i.ProductId, i.ProductCode })
            .Concat(egresosList.Select(i => new { i.ProductId, i.ProductCode }))
            .Distinct()
            .ToDictionary(x => x.ProductId, x => x.ProductCode);

        var result = allKeys.Select(k =>
        {
            ingresosAgg.TryGetValue(k, out var ing);
            egresosAgg.TryGetValue(k, out var egr);
            var lastMovementDate = new[] { ing?.LastIngreso, egr?.LastEgreso }
                .Where(d => d.HasValue)
                .DefaultIfEmpty(DateTime.MinValue)
                .Max()!.Value.Date;

            var totalIngresos = ing?.TotalIngresos ?? 0;
            var totalEgresos = egr?.TotalEgresos ?? 0;

            return new ProductStockSummaryDTO
            {
                ProductId = ing?.ProductId ?? egr!.ProductId,
                ProductName = ing?.ProductName ?? egr!.ProductName,
                ProductCode = ing?.ProductCode ?? egr!.ProductCode ?? productCodeLookup.GetValueOrDefault(ing?.ProductId ?? egr!.ProductId, string.Empty),
                CategoryId = ing?.CategoryId ?? egr!.CategoryId,
                CategoryName = ing?.CategoryName ?? egr!.CategoryName,
                CenterId = centerId ?? 0,
                CenterName = centerId.HasValue ? centerName : "Todos",
                TotalIngresos = totalIngresos,
                TotalEgresos = totalEgresos,
                TotalStock = totalIngresos - totalEgresos,
                LastMovementDate = lastMovementDate,
                MovementCount = totalIngresos + totalEgresos
            };
        })
        .OrderBy(r => r.CategoryName)
        .ThenBy(r => r.ProductName)
        .ToList();

        return Ok(result);
    }

    // GET: api/reports/movements-history
    [HttpGet("movements-history")]
    public async Task<ActionResult<IEnumerable<StockHistoryDTO>>> GetMovementsHistory(
        [FromQuery] int? centerId,
        [FromQuery] int? categoryId,
        [FromQuery] int? productId,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate)
    {
        var purchases = _context.Purchases
            .Include(p => p.Items).ThenInclude(i => i.Product)
            .AsQueryable();

        var distributions = _context.Distributions
            .Include(d => d.Items)
                .ThenInclude(id => id.ItemPurchase)
                    .ThenInclude(ip => ip.Product)
            .Include(d => d.Purchase)
            .AsQueryable();

        if (centerId.HasValue)
        {
            purchases = purchases.Where(p => p.CenterId == centerId);
            distributions = distributions.Where(d => d.Purchase != null && d.Purchase.CenterId == centerId);
        }
        if (fromDate.HasValue)
        {
            purchases = purchases.Where(p => p.PurchaseDate >= fromDate);
            distributions = distributions.Where(d => d.DeliveryDate >= fromDate);
        }
        if (toDate.HasValue)
        {
            purchases = purchases.Where(p => p.PurchaseDate <= toDate);
            distributions = distributions.Where(d => d.DeliveryDate <= toDate);
        }

        var ingresoEvents = purchases
            .SelectMany(p => p.Items.Select(i => new {
                Date = p.PurchaseDate.Date,
                Qty = i.Quantity,
                ProductId = i.ProductId,
                CategoryId = i.Product.CategoryId
            }));

        var egresoEvents = distributions
            .SelectMany(d => d.Items.Select(i => new {
                Date = d.DeliveryDate.Date,
                Qty = i.Quantity,
                ProductId = i.ItemPurchase.ProductId,
                CategoryId = i.ItemPurchase.Product.CategoryId
            }));

        if (categoryId.HasValue)
        {
            ingresoEvents = ingresoEvents.Where(e => e.CategoryId == categoryId);
            egresoEvents = egresoEvents.Where(e => e.CategoryId == categoryId);
        }
        if (productId.HasValue)
        {
            ingresoEvents = ingresoEvents.Where(e => e.ProductId == productId);
            egresoEvents = egresoEvents.Where(e => e.ProductId == productId);
        }

        var consolidated = await ingresoEvents
            .Select(e => new { e.Date, Ingreso = e.Qty, Egreso = 0 })
            .Concat(
                egresoEvents.Select(e => new { e.Date, Ingreso = 0, Egreso = e.Qty })
            )
            .GroupBy(x => x.Date)
            .Select(g => new {
                Date = g.Key,
                Ingresos = g.Sum(z => z.Ingreso),
                Egresos = g.Sum(z => z.Egreso)
            })
            .OrderBy(g => g.Date)
            .ToListAsync();

        int runIng = 0;
        int runEgr = 0;
        var history = consolidated.Select(c => {
            runIng += c.Ingresos;
            runEgr += c.Egresos;
            return new StockHistoryDTO {
                StockDate = c.Date,
                IngresosAcumulados = runIng,
                EgresosAcumulados = runEgr,
                StockAcumulado = runIng - runEgr
            };
        }).ToList();

        return Ok(history);
    }
}