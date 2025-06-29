using ProyectoCaritas.Models.Entities;

public interface INotificationService
{
    Task CreateAssignmentNotification(int orderLineId, int donationRequestId, int donorCenterId, string userId);
    Task<List<Notification>> CreateAcceptanceNotification(int orderLineId, int donationRequestId, string userId);
    Task<List<Notification>> CreateShippingNotification(int orderLineId, int donationRequestId, string userId);
    Task<List<Notification>> CreateReceptionNotification(int orderLineId, int donationRequestId, string userId, bool generateDonorNotification);
    Task SendSignalRNotifications(List<Notification> notifications);
}