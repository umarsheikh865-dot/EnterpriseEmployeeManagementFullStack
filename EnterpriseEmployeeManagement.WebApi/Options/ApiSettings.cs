namespace EnterpriseEmployeeManagement.WebApi.Options
{
    public class ApiSettings
    {
        public string ApplicationName { get; set; } = string.Empty;

        public string Version { get; set; } = string.Empty;

        public bool EnableDetailedErrors { get; set; }
    }
}