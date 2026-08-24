namespace EnterpriseEmployeeManagement.Application.Interfaces.Services
{
    public interface IEmployeeNotificationService
    {
        Task SendAsync(string message);
    }
}
