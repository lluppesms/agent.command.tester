public static class Utilities
{
    #region Input/Output
    public static string GetUserInput()
    {
        var prevColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.Write("Enter a question: (type bye to exit) ");
        string? userInput = Console.ReadLine();
        Console.ForegroundColor = prevColor;
        Console.WriteLine();

        if (string.IsNullOrEmpty(userInput) || userInput.Equals("bye", StringComparison.OrdinalIgnoreCase) || userInput.Equals("exit", StringComparison.OrdinalIgnoreCase))
        {
            return string.Empty;
        }
        return userInput;
    }
    public static void DisplayMessage(MessageRole role, DateTimeOffset createdAt, string message)
    {
        var prevColor = Console.ForegroundColor;
        Console.ForegroundColor = role == MessageRole.User ? ConsoleColor.Green : ConsoleColor.Blue;
        Console.Write($"{createdAt:hh:mm:ss} - {role,10}: ");
        Console.ForegroundColor = role == MessageRole.User ? ConsoleColor.Green : ConsoleColor.Blue;
        Console.WriteLine(message);
        Console.WriteLine();
        Console.ForegroundColor = prevColor;
    }

    //public static DateTimeOffset? ShowRecentMessages(PersistentAgentsClient agentsClient, PersistentAgentThread thread, DateTimeOffset? lastTimestamp)
    //{
    //    Pageable<PersistentThreadMessage> messages = agentsClient.Messages.GetMessages(thread.Id, order: ListSortOrder.Ascending);
    //    return DisplayNewMessages(messages, lastTimestamp);
    //}
    //public static DateTimeOffset? DisplayNewMessages(Pageable<PersistentThreadMessage> msgs, DateTimeOffset? lastMessageShown = null)
    //{
    //    DateTimeOffset? latest = null;

    //    foreach (PersistentThreadMessage threadMessage in msgs)
    //    {
    //        if (latest == null || threadMessage.CreatedAt > latest)
    //        {
    //            latest = threadMessage.CreatedAt;
    //        }
    //        if (lastMessageShown.HasValue && threadMessage.CreatedAt <= lastMessageShown.Value)
    //        {
    //            continue;
    //        }

    //        var prevColor = Console.ForegroundColor;
    //        Console.ForegroundColor = threadMessage.Role == MessageRole.User ? ConsoleColor.Green : ConsoleColor.Blue;
    //        Console.Write($"{threadMessage.CreatedAt:hh:mm:ss} - {threadMessage.Role,10}: ");
    //        Console.ForegroundColor = threadMessage.Role == MessageRole.User ? ConsoleColor.Yellow : ConsoleColor.Cyan;
    //        foreach (MessageContent contentItem in threadMessage.ContentItems)
    //        {
    //            if (contentItem is MessageTextContent textItem)
    //            {
    //                Console.Write(textItem.Text);
    //            }
    //            else if (contentItem is MessageImageFileContent imageFileItem)
    //            {
    //                Console.Write($"<image from ID: {imageFileItem.FileId}");
    //            }
    //            Console.WriteLine();
    //        }
    //        Console.WriteLine();
    //        Console.ForegroundColor = prevColor;
    //    }
    //    return latest;
    //}
    #endregion

    #region Configuration
    public static (string endpointStr, string agentId, string tenantId) ReadConfig()
    {
        Console.WriteLine("Reading configuration settings...");
        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .AddUserSecrets<Program>(optional: true)
            .Build();

        string? endpointStr = config["Foundry:Endpoint"];
        string? agentId = config["Foundry:AgentId"];
        string? tenantId = config["VisualStudioTenantId"];

        if (string.IsNullOrWhiteSpace(endpointStr) || string.IsNullOrWhiteSpace(agentId))
        {
            Console.WriteLine("Error: you must have Foundry:Endpoint and Foundry:AgentId in your configuration!");
            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
            Environment.Exit(1);
        }

        return (endpointStr, agentId, tenantId);
    }
    #endregion

    #region Credentials
    public static DefaultAzureCredential GetCredentials(IConfiguration configuration)
    {
        return GetCredentials(configuration["VisualStudioTenantId"], configuration["UserAssignedManagedIdentityClientId"]);
    }

    public static DefaultAzureCredential GetCredentials()
    {
        return GetCredentials(string.Empty, string.Empty);
    }

    public static DefaultAzureCredential GetCredentials(string visualStudioTenantId)
    {
        return GetCredentials(visualStudioTenantId, string.Empty);
    }

    public static DefaultAzureCredential GetCredentials(string visualStudioTenantId, string userAssignedManagedIdentityClientId)
    {
        if (!string.IsNullOrEmpty(visualStudioTenantId))
        {
            var azureCredential = new DefaultAzureCredential(new DefaultAzureCredentialOptions
            {
                VisualStudioTenantId = visualStudioTenantId,
                Diagnostics = { IsLoggingContentEnabled = true }
            });
            return azureCredential;
        }
        else
        {
            if (!string.IsNullOrEmpty(userAssignedManagedIdentityClientId))
            {
                var azureCredential = new DefaultAzureCredential(new DefaultAzureCredentialOptions
                {
                    ManagedIdentityClientId = userAssignedManagedIdentityClientId,
                    Diagnostics = { IsLoggingContentEnabled = true }
                });
                return azureCredential;
            }
            else
            {
                var azureCredential = new DefaultAzureCredential(new DefaultAzureCredentialOptions
                {
                    Diagnostics = { IsLoggingContentEnabled = true }
                });
                return azureCredential;
            }
        }
    }
    #endregion
}