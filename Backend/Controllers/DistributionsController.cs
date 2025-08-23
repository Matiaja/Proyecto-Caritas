using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoCaritas.Data;
using ProyectoCaritas.Models.Entities;
using ProyectoCaritas.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

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

    // GET: api/DonationRequests/movements-centers
    [HttpGet("movements-centers")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<MovementDTO>>> GetMovements(
        [FromQuery] DateTime? dateFrom = null,
        [FromQuery] DateTime? dateTo = null,
        [FromQuery] string status = null,
        [FromQuery] string productName = null,
        [FromQuery] int? centerId = null,
        [FromQuery] string typeCenter = null
    )
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
            return Unauthorized();

        var user = await _context.Users.Include(u => u.Center).FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
            return Unauthorized();

        // Validar parámetros
        if (dateFrom.HasValue && dateTo.HasValue && dateTo < dateFrom)
            return BadRequest(new { message = "La 'Fecha hasta' no puede ser anterior a la 'Fecha desde'." });

        // Obtengo las distribuciones con sus relaciones necesarias
        var query = _context.ItemsDistribution
            .Include(i => i.ItemPurchase).ThenInclude(ip => ip.Product)
            .Include(i => i.Distribution).ThenInclude(d => d.Center)
            .Include(i => i.Distribution.Purchase).ThenInclude(p => p.Center)
            .Where(i => i.Distribution.CenterId != null)
            .AsQueryable();

        // Filtros básicos
        if (dateFrom.HasValue)
            query = query.Where(i => i.Distribution.DeliveryDate >= dateFrom.Value);

        if (dateTo.HasValue)
            query = query.Where(i => i.Distribution.DeliveryDate <= dateTo.Value);

        if (!string.IsNullOrEmpty(status))
            query = query.Where(i => i.Distribution.Status == status);
        // Filtro por nombre de producto dentro del ItemPurchase de cada ItemDistribution
        if (!string.IsNullOrEmpty(productName))
        {
            var productNameLower = productName.ToLower();
            query = query.Where(i => i.ItemPurchase.Product.Name.ToLower().Contains(productNameLower));
        }

        if (centerId.HasValue)
        {
            if (typeCenter == "from")
                query = query.Where(i => i.Distribution.Purchase.CenterId == centerId);
            else if (typeCenter == "to")
                query = query.Where(i => i.Distribution.CenterId == centerId);
            else
                query = query.Where(i => i.Distribution.CenterId == centerId ||
                        i.Distribution.Purchase.CenterId == centerId);
        }

        // Filtro por centro si no es admin
        if (!User.IsInRole("Admin") && user.CenterId != null)
        {
            query = query.Where(i => i.Distribution.CenterId == user.CenterId || // Movimientos donde es el centro receptor
                i.Distribution.Purchase.CenterId == user.CenterId); // donde es el centro de compra
        }

        var movements = await query
            .OrderByDescending(i => i.Distribution.DeliveryDate)
            .Select(i => new MovementDTO
            {
                DonationRequestId = i.Id,
                FromCenter = i.Distribution.Purchase.Center.Name ?? "N/A",
                ToCenter = i.Distribution.Center.Name ?? "N/A",
                ProductName = i.ItemPurchase.Product.Name,
                Quantity = i.Quantity,
                Status = i.Distribution.Status,
                UpdatedDate = i.Distribution.DeliveryDate,
                AssignmentDate = i.Distribution.CreatedAt
            })
            .ToListAsync();

        return Ok(movements);
    }

    // POST: api/distributions
    [HttpPost]
    public async Task<ActionResult<DistributionDTO>> PostDistribution(CreateDistributionDTO dto)
    {
        // validar compra
        var purchase = await _context.Purchases.FirstOrDefaultAsync(p => p.Id == dto.PurchaseId);
        if (purchase == null)
            return BadRequest(new { message = "La compra no fue encontrada." });

        // validar CENTRO
        if (dto.CenterId.HasValue)
        {
            var centerExists = await _context.Centers.AnyAsync(c => c.Id == dto.CenterId.Value);
            if (!centerExists)
                return BadRequest(new { message = $"El centro no fue encontrado." });
        }

        // Validar nombre
        if (string.IsNullOrWhiteSpace(dto.PersonName))
            return BadRequest(new { message = "El nombre de la persona es obligatorio." });
        // validar DNI
        if (string.IsNullOrWhiteSpace(dto.PersonDNI))
            return BadRequest(new { message = "El DNI de la persona es obligatorio." });

        if (dto.PersonDNI.Length != 8 || !dto.PersonDNI.All(char.IsDigit))
            return BadRequest(new { message = "El DNI no es válido." });
        // validar ubicación
        if (string.IsNullOrWhiteSpace(dto.PersonLocation))
            return BadRequest(new { message = "La ubicación de la persona es obligatoria." });

        // validar campo exclusivo de PERSONA
        if (!dto.CenterId.HasValue)
        {
            if (string.IsNullOrWhiteSpace(dto.PersonMemberFamily))
            {
                return BadRequest(new { message = "El tipo de miembro de familia es obligatorio." });
            }
        }
        // validar fecha obligatoria
        if (string.IsNullOrWhiteSpace(dto.DeliveryDate.ToString()) ||
            !DateTime.TryParse(dto.DeliveryDate.ToString(), out var parsedDate))
            return BadRequest(new { message = "La fecha de entrega es obligatoria y debe ser válida." });

        // Validar items
        if (dto.Items == null || !dto.Items.Any())
            return BadRequest(new { message = "La entrega debe incluir al menos un ítem." });

        // Validación de items
        foreach (var itemDto in dto.Items)
        {
            var itemPurchase = await _context.ItemsPurchase
                .Include(ip => ip.ItemsDistribution)
                .FirstOrDefaultAsync(ip => ip.Id == itemDto.ItemPurchaseId);

            if (itemPurchase == null)
                return BadRequest(new { message = $"Item de compra no encontrado." });

            var restante = itemPurchase.RemainingQuantity;

            if (itemDto.Quantity < 0)
                return BadRequest(new { message = $"La cantidad entregada de productos debe ser mayor o igual a 0." });

            if (itemDto.Quantity > restante)
                return BadRequest(new { message = $"La cantidad ({itemDto.Quantity}) excede lo disponible ({restante}) para el producto {itemPurchase.ProductId}." });
        }

        var salida = new Distribution
        {
            PurchaseId = dto.PurchaseId,
            DeliveryDate = DateTime.UtcNow,
            CenterId = dto.CenterId,
            PersonName = dto.PersonName,
            PersonDNI = dto.PersonDNI,
            PersonMemberFamily = dto.PersonMemberFamily,
            PersonLocation = dto.PersonLocation,
            Items = dto.Items.Select(i => new ItemDistribution
            {
                ItemPurchaseId = i.ItemPurchaseId,
                Quantity = i.Quantity,
                Description = i.Description
            }).ToList()
        };

        // Iniciar transacción
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            _context.Distributions.Add(salida);

            // si la entrega es a un centro -> generar un ingreso en compra para ese centro
            if (dto.CenterId.HasValue)
            {
                var newPurchase = new Purchase
                {
                    PurchaseDate = DateTime.UtcNow,
                    Type = purchase.Type ?? "Transferencia interna",
                    CenterId = dto.CenterId.Value, // centro que recibe
                    OriginalCenterId = purchase.OriginalCenterId, // centro que originó
                    BuyerName = dto.PersonName, // quien recibe (persona o responsable del centro)
                    Items = dto.Items.Select(i => new ItemPurchase
                    {
                        ProductId = _context.ItemsPurchase
                            .Where(ip => ip.Id == i.ItemPurchaseId)
                            .Select(ip => ip.ProductId)
                            .FirstOrDefault(),
                        Quantity = i.Quantity,
                        Description = i.Description
                    }).ToList()
                };

                _context.Purchases.Add(newPurchase);
            }
            await _context.SaveChangesAsync();
            // confirmar transacción
            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            // Si ocurre un error, revertir la transacción
            await transaction.RollbackAsync();
            return BadRequest(new { message = $"Error al procesar la entrega: {ex.Message}" });
        }



        return CreatedAtAction(nameof(GetDistribution), new { id = salida.Id }, new DistributionDTO(salida));
    }
}