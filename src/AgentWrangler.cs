public static class AgentWrangler
{
    public static async Task SetupAgent(string endpointStr, string agentId)
    {
        Console.WriteLine($"Contacting agent at {endpointStr}...");
        var endpoint = new Uri(endpointStr);
        AIProjectClient projectClient = new(endpoint, new DefaultAzureCredential());
        PersistentAgentsClient agentsClient = projectClient.GetPersistentAgentsClient();
        PersistentAgent agent = agentsClient.Administration.GetAgent(agentId);

        await StartConversation(projectClient, agentsClient, agent);
    }
    private static async Task StartConversation(AIProjectClient projectClient, PersistentAgentsClient agentsClient, PersistentAgent agent)
    {
        PersistentAgentThread thread = agentsClient.Threads.CreateThread();
        Console.WriteLine($"Agent is ready and waiting ({thread.Id})... \n");

        DateTimeOffset? lastMessageShown = await SendMessageAndShowResponse(agentsClient, thread, agent, "Hello Agent - tell me about yourself!", null);

        string userInput = AgentHelpers.GetUserInput();
        while (!string.IsNullOrEmpty(userInput))
        {
            lastMessageShown = await SendMessageAndShowResponse(agentsClient, thread, agent, userInput, lastMessageShown);
            userInput = AgentHelpers.GetUserInput();
        }
    }

    private static async Task<DateTimeOffset?> SendMessageAndShowResponse(PersistentAgentsClient agentsClient, PersistentAgentThread thread, PersistentAgent agent, string? message, DateTimeOffset? lastTimestamp)
    {
        if (string.IsNullOrEmpty(message)) return lastTimestamp;
        await SendMessageToAgent(agentsClient, thread, agent, message);
        return AgentHelpers.ShowRecentMessages(agentsClient, thread, lastTimestamp);
    }

    private static async Task SendMessageToAgent(PersistentAgentsClient agentsClient, PersistentAgentThread thread, PersistentAgent agent, string message)
    {
        PersistentThreadMessage messageResponse = await agentsClient.Messages.CreateMessageAsync(thread.Id, MessageRole.User, message);
        ThreadRun run = agentsClient.Runs.CreateRun(thread.Id, agent.Id);
        do
        {
            await Task.Delay(TimeSpan.FromMilliseconds(500));
            run = agentsClient.Runs.GetRun(thread.Id, run.Id);
        }
        while (run.Status == RunStatus.Queued || run.Status == RunStatus.InProgress);
        if (run.Status != RunStatus.Completed) { throw new InvalidOperationException($"Run failed or was canceled: {run.LastError?.Message}"); }
    }
}