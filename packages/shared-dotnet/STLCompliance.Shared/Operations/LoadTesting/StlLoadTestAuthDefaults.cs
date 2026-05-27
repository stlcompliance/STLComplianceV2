namespace STLCompliance.Shared.Operations.LoadTesting;

/// <summary>
/// Demo credential defaults for authenticated k6 load-test scenarios against docker-compose.
/// Override via environment variables in operator scripts.
/// </summary>
public static class StlLoadTestAuthDefaults
{
    public const string DemoEmail = "admin@demo.stl";
    public const string DemoPassword = "ChangeMe!Demo2026";
    public const string DemoTenantId = "11111111-1111-1111-1111-111111111101";

    public const string EmailEnvVar = "STL_LOAD_DEMO_EMAIL";
    public const string PasswordEnvVar = "STL_LOAD_DEMO_PASSWORD";
    public const string TenantIdEnvVar = "STL_LOAD_DEMO_TENANT_ID";
}
