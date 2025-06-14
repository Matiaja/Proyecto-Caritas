using ProyectoCaritas.Models.Entities;

public interface INotificationService
{
    Task CreateAssignmentNotification(int orderLineId, int donationRequestId, int donorCenterId, string userId);
    Task<List<Notification>> CreateAcceptanceNotification(int orderLineId, int donationRequestId, string userId);
    Task CreateShippingNotification(int orderLineId, int? recipientCenterId, DateTime estimatedArrival);
    Task CreateReceptionNotification(int orderLineId, int requestingCenterId, string reason);
    Task SendSignalRNotifications(List<Notification> notifications);
}