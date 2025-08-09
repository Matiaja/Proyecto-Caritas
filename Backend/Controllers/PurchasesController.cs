using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoCaritas.Data;
using ProyectoCaritas.Models.Entities;
using ProyectoCaritas.Models.DTOs;

namespace ProyectoCaritas.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PurchasesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public PurchasesController(ApplicationDbContext context)
    {
        _context = context;
    }

    // === COMPRAS ===
    // GET: api/purchases
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PurchaseDTO>>> GetPurchases()
    {
        var purchases = await _context.Purchases
            .Include(p => p.Center)
            .Include(p => p.Items).ThenInclude(i => i.ItemsDistribution)
            .Include(p => p.Items).ThenInclude(i => i.Product)
            .ToListAsync();

        return purchases.Select(p => new PurchaseDTO(p)).ToList();
    }

    // GET: api/purchases/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<PurchaseDTO>> GetPurchase(int id)
    {
        var purchase = await _context.Purchases
            .Include(p => p.Center)
            .Include(p => p.Distributions).ThenInclude(d => d.Items)
            .Include(p => p.Distributions).ThenInclude(d => d.Center)
            .Include(p => p.Items).ThenInclude(i => i.ItemsDistribution)
            .Include(p => p.Items).ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (purchase == null)
            return NotFound();

        return new PurchaseDTO(purchase);
    }

    [HttpPost]
    public async Task<ActionResult<PurchaseDTO>> PostPurchase(CreatePurchaseDTO dto)
    {
        if (dto.Items == null || !dto.Items.Any())
            return BadRequest("La compra debe incluir al menos un ítem.");

        var purchase = new Purchase
        {
            PurchaseDate = dto.PurchaseDate,
            Type = dto.Type,
            CenterId = dto.CenterId,
            Items = dto.Items.Select(i => new ItemPurchase
            {
                ProductId = i.ProductId,
                Quantity = i.Quantity,
                Description = i.Description
            }).ToList()
        };

        _context.Purchases.Add(purchase);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetPurchase), new { id = purchase.Id }, new PurchaseDTO(purchase));
    }
}