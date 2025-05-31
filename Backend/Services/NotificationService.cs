using ProyectoCaritas.Data;
using ProyectoCaritas.Models.Entities;
using ProyectoCaritas.Models.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using ProyectoCaritas.Hubs;

public class NotificationService : INotificationService
{
    private readonly ApplicationDbContext _context;
    private readonly IHubContext<NotificationHub> _hubContext;

    public NotificationService(ApplicationDbContext context, IHubContext<NotificationHub> hubContext)
    {
        _context = context;
        _hubContext = hubContext;
    }

    public async Task CreateAssignmentNotification(int orderLineId, int donationRequestId, int assignedCenterId, string userId)
    {
        var orderLine = await _context.OrderLines
            .Include(ol => ol.Product)
            .Include(ol => ol.Request)
                .ThenInclude(r => r.RequestingCenter)
            .FirstOrDefaultAsync(ol => ol.Id == orderLineId);
        if (orderLine == null)
        {
            throw new ArgumentException("Order line not found.");
        }

        var donationRequest = await _context.DonationRequests
            .FindAsync(donationRequestId);
        if (donationRequest == null)
        {
            throw new ArgumentException("Donation request not found.");
        }

        var notification = new Notification
        {
            Title = "Nueva asignación",
            Message = $"Se te ha asignado proveer {donationRequest.Quantity} unidades de {orderLine.Product?.Name} al centro {orderLine.Request?.RequestingCenter?.Name}.",
            Type = NotificationType.Assignment,
            OrderLineId = orderLineId,
            DonationRequestId = donationRequestId,
            RecipientCenterId = assignedCenterId,
            UserId = int.Parse(userId),
            CreatedAt = DateTime.UtcNow,
            IsRead = false,
            Status = "Active"
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();

        // Notify the assigned center via SignalR
        await _hubContext.Clients.Group($"center-{assignedCenterId}")
            .SendAsync("ReceiveNotification", notification);
    }

    public async Task CreateAcceptanceNotification(int orderLineId, int requestingCenterId)
    {
        // Implement this method
    }

    public async Task CreateShippingNotification(int orderLineId, int? recipientCenterId, DateTime estimatedArrival)
    {
        // Implement this method
    }

    public async Task CreateReceptionNotification(int orderLineId, int requestingCenterId, string reason)
    {
        // Implement this method
    }
}