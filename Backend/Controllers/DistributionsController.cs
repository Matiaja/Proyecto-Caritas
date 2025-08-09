using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoCaritas.Data;
using ProyectoCaritas.Models.Entities;
using ProyectoCaritas.Models.DTOs;

namespace ProyectoCaritas.Controllers;

[Route("api/[controller]")]
[ApiController]
public class DistributionsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public DistributionsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // === SALIDAS ===
    // GET: api/distributions
    [HttpGet()]
    public async Task<ActionResult<IEnumerable<DistributionDTO>>> GetDistributions()
    {
        var dists = await _context.Distributions
            .Include(d => d.Center)
            .Include(d => d.Items).ThenInclude(i => i.ItemPurchase).ThenInclude(ip => ip.Product)
            .ToListAsync();

        return dists.Select(d => new DistributionDTO(d)).ToList();
    }

    // GET: api/distributions/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<DistributionDTO>> GetDistribution(int id)
    {
        var dist = await _context.Distributions
            .Include(d => d.Center)
            .Include(d => d.Items).ThenInclude(i => i.ItemPurchase).ThenInclude(ip => ip.Product)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (dist == null)
            return NotFound();

        return new DistributionDTO(dist);
    }

    // POST: api/distributions
    [HttpPost]
    public async Task<ActionResult<DistributionDTO>> PostDistribution(CreateDistributionDTO dto)
    {
        if (dto.Items == null || !dto.Items.Any())
            return BadRequest("La salida debe incluir al menos un ítem.");

        // Validación de cantidades
        foreach (var itemDto in dto.Items)
        {
            var itemPurchase = await _context.ItemsPurchase
                .Include(ip => ip.ItemsDistribution)
                .FirstOrDefaultAsync(ip => ip.Id == itemDto.ItemPurchaseId);

            if (itemPurchase == null)
                return BadRequest($"ItemCompra con ID {itemDto.ItemPurchaseId} no encontrado.");

            var restante = itemPurchase.RemainingQuantity;

            if (itemDto.Quantity > restante)
                return BadRequest($"La cantidad ({itemDto.Quantity}) excede lo disponible ({restante}) para el producto {itemPurchase.ProductId}.");
        }

        var salida = new Distribution
        {
            PurchaseId = dto.PurchaseId,
            DeliveryDate = dto.DeliveryDate,
            CenterId = dto.CenterId,
            PersonName = dto.PersonName,
            PersonDNI = dto.PersonDNI,
            PersonMemberFamily = dto.PersonMemberFamily,
            Items = dto.Items.Select(i => new ItemDistribution
            {
                ItemPurchaseId = i.ItemPurchaseId,
                Quantity = i.Quantity,
                Description = i.Description
            }).ToList()
        };

        _context.Distributions.Add(salida);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetDistribution), new { id = salida.Id }, new DistributionDTO(salida));
    }
}