using Microsoft.AspNetCore.SignalR;

namespace PESYONG.Api.Hubs
{
    public class PesyongHub : Hub
    {
        public async Task JoinOrderGroup(string orderId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"order-{orderId}");
        }

        public async Task LeaveOrderGroup(string orderId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"order-{orderId}");
        }
    }
}