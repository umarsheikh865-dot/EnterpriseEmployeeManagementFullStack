using EnterpriseEmployeeManagement.Application.Interfaces.Services;
using Microsoft.Extensions.DependencyInjection;

namespace EnterpriseEmployeeManagement.Application.Interfaces.Services
{
    public class EmployeeNotificationManager
    {
        private readonly IEmployeeNotificationService _notificationService;

        public EmployeeNotificationManager(
            [FromKeyedServices("email")]
            IEmployeeNotificationService notificationService)
    {
        _notificationService = notificationService;
    }

            public async Task SendAsync(string message)
            {
                await _notificationService.SendAsync(message);
            }
        }
    }