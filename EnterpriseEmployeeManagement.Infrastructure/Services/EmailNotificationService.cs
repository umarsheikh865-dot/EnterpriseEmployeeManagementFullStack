using EnterpriseEmployeeManagement.Application.Interfaces.Services;

namespace EnterpriseEmployeeManagement.Infrastructure.Services
{
    public class EmailNotificationService : IEmployeeNotificationService
    {
        public Task SendAsync(string message)

        {
            Console.WriteLine($"Email: {message}");

            return Task.CompletedTask;
        }
    }
}