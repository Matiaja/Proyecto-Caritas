using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoCaritas.Data;
using ProyectoCaritas.Models.Entities;
using ProyectoCaritas.Models.DTOs;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace ProyectoCaritas.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PurchasesController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<User> _userManager;

    public PurchasesController(ApplicationDbContext context, UserManager<User> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // === COMPRAS ===
    // GET: api/purchases
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PurchaseDTO>>> GetPurchases()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        // Si el token no tiene ID, devuelve un error 401
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "Invalid token." });
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return NotFound(new { message = "User not found." });
        }

        var purchases = await _context.Purchases
            .Include(p => p.Center)
            .Include(p => p.Items).ThenInclude(i => i.ItemsDistribution)
            .Include(p => p.Items).ThenInclude(i => i.Product)
            .Where(p => p.CenterId == user.CenterId)
            .OrderByDescending(p => p.PurchaseDate)
            .ToListAsync();

        return purchases.Select(p => new PurchaseDTO(p)).ToList();
    }

    // GET: api/purchases/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<PurchaseDTO>> GetPurchase(int id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        // Si el token no tiene ID, devuelve un error 401
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "Invalid token." });
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return NotFound(new { message = "User not found." });
        }

        var purchase = await _context.Purchases
            .Include(p => p.Center)
            .Include(p => p.OriginalCenter)
            .Include(p => p.Distributions).ThenInclude(d => d.Items)
            .Include(p => p.Distributions).ThenInclude(d => d.Center)
            .Include(p => p.Items).ThenInclude(i => i.ItemsDistribution)
            .Include(p => p.Items).ThenInclude(i => i.Product)
            .Where(p => p.CenterId == user.CenterId)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (purchase == null)
            return NotFound();

        return new PurchaseDTO(purchase);
    }

    [HttpPost]
    public async Task<ActionResult<PurchaseDTO>> PostPurchase(CreatePurchaseDTO dto)
    {
        // Val Fecha
        if (string.IsNullOrWhiteSpace(dto.PurchaseDate) ||
            !DateTime.TryParse(dto.PurchaseDate, out var parsedDate))
            return BadRequest(new { message = "La fecha de compra es obligatoria y debe ser válida." });

        if (parsedDate.Date > DateTime.UtcNow.Date)
            return BadRequest(new { message = "La fecha de compra no puede ser futura." });

        // Val Tipo
        if (string.IsNullOrWhiteSpace(dto.Type))
            return BadRequest(new { message = "El tipo de compra es obligatorio." });
        if (dto.Type.Length > 100)
            return BadRequest(new { message = "El tipo de compra no puede superar los 100 caracteres." });

        // Val Centro
        var centerExists = await _context.Centers.AnyAsync(c => c.Id == dto.CenterId);
        if (!centerExists)
            return BadRequest(new { message = "El centro especificado no existe." });

        // Val Items
        if (dto.Items == null || !dto.Items.Any())
            return BadRequest(new { message = "La compra debe incluir al menos un ítem." });

        foreach (var item in dto.Items)
        {
            if (item.ProductId <= 0)
                return BadRequest(new { message = $"Producto del item nro. {dto.Items.IndexOf(item) + 1} no ingresado." });

            if (!await _context.Products.AnyAsync(p => p.Id == item.ProductId))
                return BadRequest(new { message = $"El producto del item nro. {dto.Items.IndexOf(item) + 1} no existe" });

            if (item.Quantity <= 0)
                return BadRequest(new { message = "La cantidad de cada ítem debe ser mayor que cero." });

            if (!string.IsNullOrWhiteSpace(item.Description) && item.Description.Length > 255)
                return BadRequest(new { message = "La descripción no puede superar los 255 caracteres." });
        }

        // Crear la compra
        var purchase = new Purchase
        {
            PurchaseDate = parsedDate.Date,
            Type = dto.Type.Trim(),
            CenterId = dto.CenterId,
            OriginalCenterId = dto.CenterId, // El origen es el mismo Centro que compra
            BuyerName = string.IsNullOrWhiteSpace(dto.BuyerName) ? null : dto.BuyerName.Trim(),
            Items = dto.Items.Select(i => new ItemPurchase
            {
                ProductId = i.ProductId,
                Quantity = i.Quantity,
                Description = string.IsNullOrWhiteSpace(i.Description) ? null : i.Description.Trim()
            }).ToList()
        };

        _context.Purchases.Add(purchase);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetPurchase), new { id = purchase.Id }, new PurchaseDTO(purchase));
    }
}