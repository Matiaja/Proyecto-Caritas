using ProyectoCaritas.Data;
using ProyectoCaritas.Models.Entities;
using ProyectoCaritas.Models.DTOs;
using Microsoft.EntityFrameworkCore;

public class RequestStatusService : IRequestStatusService
{
    private readonly ApplicationDbContext _context;

    public RequestStatusService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task UpdateOrderLineAndRequestStatusAsync(int orderLineId)
    {
        var orderLine = await _context.OrderLines
            .Include(ol => ol.DonationRequests)
            .Include(ol => ol.Request)
                .ThenInclude(r => r.OrderLines)
            .FirstOrDefaultAsync(ol => ol.Id == orderLineId);

        if (orderLine == null) throw new ArgumentException("OrderLine not found.");

        // Sumar todas las cantidades recibidas de esta orderline
        var totalRecibido = orderLine.DonationRequests
            .Where(dr => dr.Status == "Recibida")
            .Sum(dr => dr.Quantity);

        if (totalRecibido >= orderLine.Quantity)
        {
            orderLine.Status = "Completa";
        }
        else if (totalRecibido > 0)
        {
            orderLine.Status = "Parcial";
        }
        else
        {
            orderLine.Status = "Pendiente";
        }

        // Revisar si la request entera está finalizada
        var allOrderLinesComplete = orderLine.Request.OrderLines
            .All(ol => ol.Status == "Completa");

        if (allOrderLinesComplete)
        {
            orderLine.Request.Status = "Finalizada";
        }

        await _context.SaveChangesAsync();
    }
}
