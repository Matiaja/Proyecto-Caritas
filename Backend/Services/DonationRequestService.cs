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

    public async Task UpdateStatus(int donationRequestId, string newStatus, DateTime changeDate)
    {
        var donationRequest = await _context.DonationRequests.FindAsync(donationRequestId);

        // Actualizar el estado
        donationRequest.Status = newStatus;
        donationRequest.LastStatusChangeDate = changeDate;

        // Registrar en el historial
        var statusHistory = new DonationRequestStatusHistory
        {
            DonationRequestId = donationRequest.Id,
            Status = newStatus,
            ChangeDate = changeDate
        };

        _context.DonationRequestStatusHistories.Add(statusHistory);
        await _context.SaveChangesAsync();
    }
}