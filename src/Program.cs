// Demo of calling an Azure Foundry Agent from a command line program

Console.WriteLine("Agent Testing Service initializing...");

// enter the Azure Foundry Secrets in the Foundry:ProjectEndpoint and Foundry:AgentId in your secrets or appsettings.development.json
(var endpointStr, var agentId, var tenantId) = ConfigHelper.ReadConfig();

// start a conversation with the Agent
await AgentWrangler.SetupAgent(endpointStr, agentId, tenantId);
