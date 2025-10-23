public static class AgentManager
{
    public static async Task StartAgent(string endpointStr, string agentId, string tenantId)
    {
        Console.WriteLine($"Contacting agent {agentId} at {endpointStr}...");
        var endpoint = new Uri(endpointStr);
        var creds = Utilities.GetCredentials(tenantId);
        AIProjectClient projectClient = new(endpoint, creds);
        PersistentAgentsClient agentsClient = projectClient.GetPersistentAgentsClient();
        PersistentAgent agent = agentsClient.Administration.GetAgent(agentId);

        await StartConversation(projectClient, agentsClient, agent);
    }

    private static async Task StartConversation(AIProjectClient projectClient, PersistentAgentsClient agentsClient, PersistentAgent agent)
    {
        PersistentAgentThread thread = agentsClient.Threads.CreateThread();
        Console.WriteLine($"Agent is ready and waiting ({thread.Id})... \n");

        DateTimeOffset? lastMessageShown = await SendMessageAndShowResponse(agentsClient, thread, agent, "Hello Agent - tell me about yourself!", null);

        string userInput = Utilities.GetUserInput();
        while (!string.IsNullOrEmpty(userInput))
        {
            lastMessageShown = await SendMessageAndShowResponse(agentsClient, thread, agent, userInput, lastMessageShown);
            userInput = Utilities.GetUserInput();
        }
    }

    private static async Task<DateTimeOffset?> SendMessageAndShowResponse(PersistentAgentsClient agentsClient, PersistentAgentThread thread, PersistentAgent agent, string? message, DateTimeOffset? lastTimestamp)
    {
        if (string.IsNullOrEmpty(message)) return lastTimestamp;
        await SendMessageToAgent(agentsClient, thread, agent, message);
        return ShowRecentMessages(agentsClient, thread, lastTimestamp);
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

    public static DateTimeOffset? ShowRecentMessages(PersistentAgentsClient agentsClient, PersistentAgentThread thread, DateTimeOffset? lastTimestamp)
    {
        Pageable<PersistentThreadMessage> messages = agentsClient.Messages.GetMessages(thread.Id, order: ListSortOrder.Ascending);
        return DisplayNewMessages(messages, lastTimestamp);
    }

    public static DateTimeOffset? DisplayNewMessages(Pageable<PersistentThreadMessage> messages, DateTimeOffset? lastMessageShown = null)
    {
        DateTimeOffset? latest = null;
        foreach (PersistentThreadMessage threadMessage in messages)
        {
            if (latest == null || threadMessage.CreatedAt > latest)
            {
                latest = threadMessage.CreatedAt;
            }
            if (lastMessageShown.HasValue && threadMessage.CreatedAt <= lastMessageShown.Value)
            {
                continue;
            }
            foreach (MessageContent contentItem in threadMessage.ContentItems)
            {
                if (contentItem is MessageTextContent textItem)
                {
                    Utilities.DisplayMessage(threadMessage.Role, threadMessage.CreatedAt, textItem.Text);
                }
                else if (contentItem is MessageImageFileContent imageFileItem)
                {
                    Utilities.DisplayMessage(threadMessage.Role, threadMessage.CreatedAt, $"<image from ID: {imageFileItem.FileId}");
                }
            }
        }
        return latest;
    }

}