using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.UI.Dispatching;
using System;
using System.Threading.Tasks;

namespace PESYONG.Presentation.Services
{
    public class RealtimeService
    {
        private readonly DispatcherQueue _dispatcherQueue;
        private HubConnection? _connection;

        public event Func<Task>? CatalogChanged;
        public event Func<Guid, string, Task>? OrderStatusChanged;

        public RealtimeService()
        {
            _dispatcherQueue = DispatcherQueue.GetForCurrentThread();
        }

        public async Task StartAsync()
        {
            _connection = new HubConnectionBuilder()
                .WithUrl("https://localhost:7014/hubs/pesyong")
                .WithAutomaticReconnect()
                .Build();

            _connection.On("CatalogChanged", () =>
            {
                _dispatcherQueue.TryEnqueue(async () =>
                {
                    if (CatalogChanged != null)
                    {
                        await CatalogChanged.Invoke();
                    }
                });
            });

            _connection.On<string, string>("OrderStatusChanged", (orderId, status) =>
            {
                System.Diagnostics.Debug.WriteLine($"RealtimeService received OrderStatusChanged: {orderId} -> {status}");

                _dispatcherQueue.TryEnqueue(async () =>
                {
                    if (OrderStatusChanged != null && Guid.TryParse(orderId, out var parsedOrderId))
                    {
                        await OrderStatusChanged.Invoke(parsedOrderId, status);
                    }
                });
            });

            await _connection.StartAsync();
        }
    }
}