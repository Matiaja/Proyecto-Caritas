public interface IRequestStatusService
{
    Task UpdateOrderLineAndRequestStatusAsync(int orderLineId);
}