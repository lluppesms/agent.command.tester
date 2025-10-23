public static class Utilities
{
    public static string GetUserInput()
    {
        var prevColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.Write("Enter a question: (type bye to exit) ");
        string? userInput = Console.ReadLine();
        Console.ForegroundColor = prevColor;

        if (string.IsNullOrEmpty(userInput) || userInput.Equals("bye", StringComparison.OrdinalIgnoreCase) || userInput.Equals("exit", StringComparison.OrdinalIgnoreCase))
        {
            return string.Empty;
        }
        return userInput;
    }
    public static DateTimeOffset? ShowRecentMessages(PersistentAgentsClient agentsClient, PersistentAgentThread thread, DateTimeOffset? lastTimestamp)
    {
        Pageable<PersistentThreadMessage> messages = agentsClient.Messages.GetMessages(thread.Id, order: ListSortOrder.Ascending);
        return DisplayNewMessages(messages, lastTimestamp);
    }
    public static DateTimeOffset? DisplayNewMessages(Pageable<PersistentThreadMessage> msgs, DateTimeOffset? lastMessageShown = null)
    {
        DateTimeOffset? latest = null;

        foreach (PersistentThreadMessage threadMessage in msgs)
        {
            if (latest == null || threadMessage.CreatedAt > latest)
            {
                latest = threadMessage.CreatedAt;
            }
            if (lastMessageShown.HasValue && threadMessage.CreatedAt <= lastMessageShown.Value)
            {
                continue;
            }

            var prevColor = Console.ForegroundColor;
            Console.ForegroundColor = threadMessage.Role == MessageRole.User ? ConsoleColor.Green : ConsoleColor.Blue;
            Console.Write($"{threadMessage.CreatedAt:hh:mm:ss} - {threadMessage.Role,10}: ");
            Console.ForegroundColor = threadMessage.Role == MessageRole.User ? ConsoleColor.Yellow: ConsoleColor.Cyan;
            foreach (MessageContent contentItem in threadMessage.ContentItems)
            {
                if (contentItem is MessageTextContent textItem)
                {
                    Console.Write(textItem.Text);
                }
                else if (contentItem is MessageImageFileContent imageFileItem)
                {
                    Console.Write($"<image from ID: {imageFileItem.FileId}");
                }
                Console.WriteLine();
            }
            Console.WriteLine();
            Console.ForegroundColor = prevColor;
        }
        return latest;
    }
}