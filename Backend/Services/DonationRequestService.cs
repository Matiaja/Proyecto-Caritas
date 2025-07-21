using ProyectoCaritas.Data;
using ProyectoCaritas.Models.Entities;
using ProyectoCaritas.Models.DTOs;
using Microsoft.EntityFrameworkCore;

public class DonationRequestService
{
    private readonly ApplicationDbContext _context;

    public DonationRequestService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<DonationRequest>> GetDonationRequestsPending(int centerId, int productId)
    {
        var dr = await _context.DonationRequests
                .Include(dr => dr.OrderLine)
                .Where(dr => dr.AssignedCenterId == centerId && dr.OrderLine.ProductId == productId &&
                             (dr.Status == "Aceptada" || dr.Status == "Enviada"))
                .ToListAsync();

        return dr;
    }
}