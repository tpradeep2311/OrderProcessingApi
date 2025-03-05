namespace OrderProcessingApi.Services
{
    public class ConsoleNotificationService : INotificationService
    {
        public void SendNotification(string message)
        {
            // Simulate sending a notification (e.g., logging to console)
            Console.WriteLine($"Notification: {message}");
        }
    }
}
