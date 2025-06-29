using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace ProyectoCaritas.Hubs
{
    [Authorize]
    public class NotificationHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var centerId = Context.User?.FindFirst("CenterId")?.Value;
            var isAdmin = Context.User?.IsInRole("Admin") ?? false;

            // Grupo por usuario individual (para notificaciones personales)
            if (userId != null)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId}");
            }

            // Grupo por centro (para notificaciones del centro completo)
            if (centerId != null)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"center-{centerId}");
            }

            // Grupo de administradores
            if (isAdmin)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, "admins");
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);
        }
    }
}
