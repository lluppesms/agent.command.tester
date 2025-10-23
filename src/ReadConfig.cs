public static class ConfigHelper
{
    public static (string endpointStr, string agentId, string tenantId) ReadConfig()
    {
        Console.WriteLine("Reading configuration settings...");
        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .AddUserSecrets<Program>(optional: true)
            .Build();

        string? endpointStr = config["Foundry:ProjectEndpoint"];
        string? agentId = config["Foundry:AgentId"];
        string? tenantId = config["VisualStudioTenantId"];

        if (string.IsNullOrWhiteSpace(endpointStr) || string.IsNullOrWhiteSpace(agentId))
        {
            Console.WriteLine("Error: you must have Foundry:ProjectEndpoint and Foundry:AgentId in your configuration!");
            Environment.Exit(1);
        }

        return (endpointStr, agentId, tenantId);
    }
}
