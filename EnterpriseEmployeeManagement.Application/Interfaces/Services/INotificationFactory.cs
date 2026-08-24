namespace EnterpriseEmployeeManagement.Application.Interfaces.Services
{
    public interface INotificationFactory
    {
        IEmployeeNotificationService Create(string type);
    }
}