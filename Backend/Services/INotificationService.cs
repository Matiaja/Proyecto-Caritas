public interface INotificationService
{
    Task CreateAssignmentNotification(int orderLineId, int donationRequestId, int donorCenterId, string userId);
    Task CreateAcceptanceNotification(int orderLineId, int requestingCenterId);
    Task CreateShippingNotification(int orderLineId, int? recipientCenterId, DateTime estimatedArrival);
    Task CreateReceptionNotification(int orderLineId, int requestingCenterId, string reason);
}