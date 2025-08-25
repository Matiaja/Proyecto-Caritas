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

        // Verify donation request is in the order line
        if (donationRequest.OrderLineId != orderLineId)
        {
            throw new ArgumentException("Donation request does not belong to the specified order line.");
        }

        var notification = new Notification
        {
            Title = "Nueva asignación",
            Message = $"Se te ha asignado proveer {FormatQuantity(donationRequest.Quantity)}"
                + $" de {FormatProductName(orderLine.Product?.Name ?? "", donationRequest.Quantity)}"
                + $" para el centro \"{orderLine.Request?.RequestingCenter?.Name}\".",
            Type = NotificationType.Assignment,
            OrderLineId = orderLineId,
            DonationRequestId = donationRequestId,
            RecipientCenterId = assignedCenterId,
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            IsRead = false,
            Status = "Active"
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();

        // Notify the assigned center via SignalR
        await SendSignalRNotifications(new List<Notification> { notification });
    }

    public async Task<List<Notification>> CreateAcceptanceNotification(int orderLineId, int donationRequestId, string userId)
    {
        var notifications = new List<Notification>();

        var orderLine = await _context.OrderLines
            .Include(ol => ol.Product)
            .Include(ol => ol.Request)
                .ThenInclude(r => r.RequestingCenter)
            .FirstOrDefaultAsync(ol => ol.Id == orderLineId);

        var donationRequest = await _context.DonationRequests
            .Include(dr => dr.AssignedCenter)
            .FirstOrDefaultAsync(dr => dr.Id == donationRequestId);

        string unitQuantity = FormatQuantity(donationRequest.Quantity);
        string productName = FormatProductName(orderLine?.Product?.Name, donationRequest.Quantity);

        // Notificación para el centro donante (próximo paso: enviar)
        var donorNotification = new Notification
        {
            Title = "Próximo paso: Envío",
            Message = $"Por favor marca como enviado cuando despaches {unitQuantity}"
                + $" de {productName} hacia el centro \"{orderLine?.Request?.RequestingCenter?.Name}\".",
            Type = NotificationType.Acceptance,
            OrderLineId = orderLineId,
            DonationRequestId = donationRequestId,
            RecipientCenterId = donationRequest?.AssignedCenterId,
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            IsRead = false,
            Status = "Active"
        };
        notifications.Add(donorNotification);

        // Notificación para el centro solicitante (aceptación)
        var requestingNotification = new Notification
        {
            Title = "Donación aceptada",
            Message = $"El centro \"{donationRequest?.AssignedCenter?.Name}\" está preparando la donación de "
                + $"{unitQuantity} para tu solicitud de {FormatQuantity(orderLine.Quantity)} de {FormatProductName(orderLine?.Product?.Name, orderLine.Quantity)}.",
            Type = NotificationType.System,
            OrderLineId = orderLineId,
            DonationRequestId = donationRequestId,
            RecipientCenterId = orderLine?.Request?.RequestingCenterId,
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            IsRead = false,
            Status = "Active"
        };
        notifications.Add(requestingNotification);

        // Notificación para admin (aceptación)
        var adminNotification = new Notification
        {
            Title = $"Donación aceptada",
            Message = $"Solicitud #{orderLine?.RequestId} - Pedido #{orderLineId}: el centro \"{donationRequest?.AssignedCenter?.Name}\" aceptó proveer {unitQuantity} de {productName}"
                + $" para el centro \"{orderLine?.Request?.RequestingCenter?.Name}\".",
            Type = NotificationType.System,
            OrderLineId = orderLineId,
            DonationRequestId = donationRequestId,
            RecipientCenterId = null, // Admin notifications don't need a recipient center
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            IsRead = false,
            Status = "Active"
        };
        notifications.Add(adminNotification);

        _context.Notifications.AddRange(donorNotification, requestingNotification, adminNotification);
        await _context.SaveChangesAsync();

        return notifications;
    }

    public async Task SendSignalRNotifications(List<Notification> notifications)
    {
        foreach (var notification in notifications)
        {
            var targetGroup = notification.RecipientCenterId != null
                ? $"center-{notification.RecipientCenterId}"
                : "admins";

            await _hubContext.Clients.Group(targetGroup)
                .SendAsync("ReceiveNotification", notification);
        }
    }

    public async Task<List<Notification>> CreateRejectionNotification(int orderLineId, int donationRequestId, string userId)
    {
        var notifications = new List<Notification>();

        var orderLine = await _context.OrderLines
            .Include(ol => ol.Product)
            .Include(ol => ol.Request)
                .ThenInclude(r => r.RequestingCenter)
            .FirstOrDefaultAsync(ol => ol.Id == orderLineId);

        var donationRequest = await _context.DonationRequests
            .Include(dr => dr.AssignedCenter)
            .FirstOrDefaultAsync(dr => dr.Id == donationRequestId);

        string unitQuantity = FormatQuantity(donationRequest.Quantity);
        string productName = FormatProductName(orderLine?.Product?.Name, donationRequest.Quantity);

        // Notificación para admin (rechazo)
        var adminNotification = new Notification
        {
            Title = $"Donación rechazada",
            Message = $"Solicitud #{orderLine?.RequestId} - Pedido #{orderLineId}: el centro \"{donationRequest?.AssignedCenter?.Name}\" rechazó proveer {unitQuantity} de {productName}"
                + $" para el centro \"{orderLine?.Request?.RequestingCenter?.Name}\".",
            Type = NotificationType.System,
            OrderLineId = orderLineId,
            DonationRequestId = donationRequestId,
            RecipientCenterId = null, // Admin notifications don't need a recipient center
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            IsRead = false,
            Status = "Active"
        };
        notifications.Add(adminNotification);

        _context.Notifications.AddRange(adminNotification);
        await _context.SaveChangesAsync();

        return notifications;
    }

    public async Task<List<Notification>> CreateShippingNotification(int orderLineId, int donationRequestId, string userId)
    {
        var notifications = new List<Notification>();

        var orderLine = await _context.OrderLines
            .Include(ol => ol.Product)
            .Include(ol => ol.Request)
                .ThenInclude(r => r.RequestingCenter)
            .FirstOrDefaultAsync(ol => ol.Id == orderLineId);

        var donationRequest = await _context.DonationRequests
            .Include(dr => dr.AssignedCenter)
            .FirstOrDefaultAsync(dr => dr.Id == donationRequestId);

        string unitQuantity = FormatQuantity(donationRequest.Quantity);
        string productName = FormatProductName(orderLine?.Product?.Name, donationRequest.Quantity);

        // Notificación para centro solicitante (con acción de recibido)
        var requestingCenterNotification = new Notification
        {
            Title = "Donación en camino...",
            Message = $"El centro \"{donationRequest?.AssignedCenter?.Name}\" ha enviado {unitQuantity} " +
                     $"de {productName}. Por favor confirma la recepción cuando llegue.",
            Type = NotificationType.Shipping,
            OrderLineId = orderLineId,
            DonationRequestId = donationRequestId,
            RecipientCenterId = orderLine?.Request?.RequestingCenterId,
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            IsRead = false,
            Status = "Active"
        };
        notifications.Add(requestingCenterNotification);

        // Notificación informativa para admin
        var adminNotification = new Notification
        {
            Title = "Donación enviada",
            Message = $"Solicitud #{orderLine?.RequestId} - Pedido #{orderLineId}. " +
                     $"La donación de {unitQuantity} de {productName} fue marcada como enviada por " +
                     $"el centro \"{donationRequest?.AssignedCenter?.Name}\" hacia " +
                     $"el centro \"{orderLine?.Request?.RequestingCenter?.Name}\".",
            Type = NotificationType.System,
            OrderLineId = orderLineId,
            DonationRequestId = donationRequestId,
            RecipientCenterId = null, // Admin notifications don't need a recipient center
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            IsRead = false,
            Status = "Active"
        };
        notifications.Add(adminNotification);

        _context.Notifications.AddRange(requestingCenterNotification, adminNotification);
        await _context.SaveChangesAsync();

        return notifications;
    }

    public async Task<List<Notification>> CreateReceptionNotification(int orderLineId, int donationRequestId, string userId, bool generateDonorNotification = true)
    {
        var notifications = new List<Notification>();

        var orderLine = await _context.OrderLines
            .Include(ol => ol.Product)
            .Include(ol => ol.Request)
                .ThenInclude(r => r.RequestingCenter)
            .FirstOrDefaultAsync(ol => ol.Id == orderLineId);

        var donationRequest = await _context.DonationRequests
            .Include(dr => dr.AssignedCenter)
            .FirstOrDefaultAsync(dr => dr.Id == donationRequestId);

        string unitQuantity = FormatQuantity(donationRequest.Quantity);
        string productName = FormatProductName(orderLine?.Product?.Name, donationRequest.Quantity);

        if (generateDonorNotification)
        {
            // Notificación para centro donante
            var donorNotification = new Notification
            {
                Title = "Donación recibida",
                Message = $"El centro \"{orderLine?.Request?.RequestingCenter?.Name}\" confirmó " +
                        $"la recepción de {unitQuantity} de {productName}.",
                Type = NotificationType.Receipt,
                OrderLineId = orderLineId,
                DonationRequestId = donationRequestId,
                RecipientCenterId = donationRequest?.AssignedCenterId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                IsRead = false,
                Status = "Active"
            };
            notifications.Add(donorNotification);
        }

        // Notificación para admin
        var adminNotification = new Notification
        {
            Title = "Donación completada",
            Message = $"El centro \"{orderLine?.Request?.RequestingCenter?.Name}\" confirmó " +
                     $"la recepción de {unitQuantity} de {productName} " +
                     $"provenientes del centro \"{donationRequest?.AssignedCenter?.Name}\".",
            Type = NotificationType.System,
            OrderLineId = orderLineId,
            DonationRequestId = donationRequestId,
            RecipientCenterId = null, // Admin notifications don't need a recipient center
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            IsRead = false,
            Status = "Active"
        };
        notifications.Add(adminNotification);

        _context.Notifications.AddRange(notifications);
        await _context.SaveChangesAsync();

        return notifications;
    }

    private static string FormatProductName(string productName, int quantity)
    {
        if (string.IsNullOrEmpty(productName))
            return "";

        productName = productName.ToLower().Trim();
        if (quantity > 1)
        {
            // Casos simples
            if (productName.EndsWith("z"))
                return productName.Substring(0, productName.Length - 1) + "ces";
            if (productName.EndsWith("s") || productName.EndsWith("x"))
                return productName;
            if (productName.EndsWith("ón"))
                return productName.Substring(0, productName.Length - 2) + "ones";
            if (productName.EndsWith("a") || productName.EndsWith("e") || productName.EndsWith("i") || productName.EndsWith("o") || productName.EndsWith("u") || productName.EndsWith("kg"))
                return productName + "s";

            return productName;
        }
        else
        {
            return productName.Trim().ToLower();
        }
    }

    private static string FormatQuantity(int quantity)
    {
        return quantity > 1 ? $"{quantity} unidades" : $"{quantity} unidad";
    }
}