using Microsoft.Extensions.Options;
using EnterpriseEmployeeManagement.Infrastructure.Options;

namespace EnterpriseEmployeeManagement.Infrastructure.Services.Authentication
{
    // Thin wrapper to provide the JwtService type inside the
    // EnterpriseEmployeeManagement.Infrastructure.Services.Authentication
    // namespace for existing tests and code that reference it.
    public class JwtService : EnterpriseEmployeeManagement.Infrastructure.Services.JwtService
    {
        public JwtService(IOptions<EnterpriseEmployeeManagement.Infrastructure.Options.JwtSettings> jwtSettings)
            : base(jwtSettings)
        {
        }
    }
}
