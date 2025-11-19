using CoffeeManagementSoftware;

// Parse command line arguments
var commandArgs = Environment.GetCommandLineArgs().Skip(1).ToArray();

if (commandArgs.Length == 0)
{
    // Interactive menu mode
    await MenuMode.RunAsync();
}
else
{
    var command = commandArgs[0].ToLower();

    switch (command)
    {
        case "auto":
            await AutoMode.RunAsync(commandArgs);
            break;
        case "add":
            await Commands.AddCoffeeAsync(commandArgs);
            break;
        case "update":
            await Commands.UpdateStockAsync(commandArgs);
            break;
        case "toggle":
            await Commands.ToggleAvailabilityAsync(commandArgs);
            break;
        case "list":
            await Commands.ListCoffeesAsync(commandArgs);
            break;
        case "order":
            await Commands.RecordOrderAsync(commandArgs);
            break;
        case "report":
            await Commands.ShowReportAsync(commandArgs);
            break;
        case "sync":
            await Commands.SyncToApiAsync(commandArgs);
            break;
        case "pull":
            await Commands.PullFromApiAsync(commandArgs);
            break;
        case "help":
            Commands.ShowHelp();
            break;
        default:
            Console.WriteLine($"Unknown command: {command}");
            Console.WriteLine("Use 'help' to see available commands.");
            break;
    }
}