using EnterpriseEmployeeManagement.Application.DTOs.Authentication;
using EnterpriseEmployeeManagement.Domain.Entities;

namespace EnterpriseEmployeeManagement.Application.Interfaces.Services
{
    public interface IJwtService
    {
        AuthenticationResponse GenerateToken(Employee employee);

        DateTime GetExpirationTime();
    }
}