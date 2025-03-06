using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OrderProcessingApi.Models;
using OrderProcessingApi.Repositories;
using OrderProcessingApi.Services;

public class OrdersModel : PageModel
{
    private readonly IOrderService _orderService;
    private readonly IOrderRepository _orderRepository;

    [BindProperty]

 
    public OrderItem OrderItem { get; set; } = new();

    public List<Order> Orders { get; set; } = new List<Order>();

    [BindProperty]
    public string OrderIdToCancel { get; set; } = string.Empty;

    [BindProperty(SupportsGet = true)]
    public string OrderIdToCheck { get; set; } = string.Empty;
    public Guid OrderId { get; private set; }
    public string OrderStatus { get; set; } = string.Empty;

    public OrdersModel(IOrderService orderService, IOrderRepository orderRepository)
    {
        _orderService = orderService;
        _orderRepository = orderRepository;
    }


    public async Task OnGetAsync(Guid? ProductId, string handler)
    {
        Orders = (await _orderRepository.GetAllAsync()).ToList();

    }

    public async Task<IActionResult> OnPostPlaceOrderAsync()
    {
        try 
        {
            var order = await _orderService.PlaceOrderAsync(new List<OrderItem> { OrderItem });
            OrderId = order.Id;
            OrderStatus = order.Status.ToString();
        }
        
        catch (Exception ex)
    {
            // Instead of letting the exception bubble up, add the error message to ModelState.
            ModelState.AddModelError(string.Empty, ex.Message);
        }

        return Page();
    }

    public async Task<IActionResult> OnPostCancelOrderAsync()
    {
        try
        {
            if (!string.IsNullOrEmpty(OrderIdToCancel))
            {
                await _orderService.CancelOrderAsync(Guid.Parse(OrderIdToCancel));
                OrderStatus = "Canceled";
            }
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
        }
        return Page();
    }

    public async Task<IActionResult> OnGetCheckStatusAsync()
    {
        if (!string.IsNullOrWhiteSpace(OrderIdToCheck))
        {
            try
            {
                var order = await _orderService.GetOrderAsync(Guid.Parse(OrderIdToCheck));
                OrderStatus = order.Status.ToString();
            }
            catch (Exception ex)
            {
                OrderStatus = "Error: " + ex.Message;
            }
        }
        return Page();
    }
}
