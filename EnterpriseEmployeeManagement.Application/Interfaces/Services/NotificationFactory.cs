using EnterpriseEmployeeManagement.Application.Interfaces.Services;
using Microsoft.Extensions.DependencyInjection;

namespace EnterpriseEmployeeManagement.Application.Interfaces.Services
{
    public class NotificationFactory : INotificationFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public NotificationFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IEmployeeNotificationService Create(string type)
        {
            return type.ToLower() switch
            {
                "email" =>
                    _serviceProvider
                        .GetRequiredKeyedService<IEmployeeNotificationService>("email"),

                "sms" =>
                    _serviceProvider
                        .GetRequiredKeyedService<IEmployeeNotificationService>("sms"),

                _ => throw new ArgumentException(
                    "Invalid notification type.")
            };
        }
    }
}