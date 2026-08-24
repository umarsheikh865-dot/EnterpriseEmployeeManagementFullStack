using EnterpriseEmployeeManagement.Application.Interfaces.Services;

namespace EnterpriseEmployeeManagement.Infrastructure.Services
{
    public class SmsNotificationService : IEmployeeNotificationService
    {
        public Task SendAsync(string message)
        {
            Console.WriteLine($"SMS: {message}");

            return Task.CompletedTask;
        }
    }
}