using Microsoft.Extensions.Hosting;
using OrderProcessingApi.Services;

namespace OrderProcessingApi.BackgroundServices
{
    public class OrderFulfillmentBackgroundService : BackgroundService
    {
        private readonly IOrderService _orderService;
        private readonly TimeSpan _interval = TimeSpan.FromSeconds(5);

        public OrderFulfillmentBackgroundService(IOrderService orderService)
        {
            _orderService = orderService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await _orderService.ProcessPendingOrdersAsync();
                await Task.Delay(_interval, stoppingToken);
            }
        }
    }
}
