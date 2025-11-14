namespace CoffeeManagementSoftware;

public static class AutoMode
{
    private static DateTime _lastSyncTime = DateTime.MinValue;
    private static readonly TimeSpan SyncCooldown = TimeSpan.FromSeconds(3);

    public static async Task RunAsync(string[] commandArgs)
    {
        var inventoryPath = DataStore.GetInventoryPath(commandArgs);

        Console.WriteLine(@"
╔═══════════════════════════════════════════╗
║   Coffee Management Software - Auto Mode  ║
╚═══════════════════════════════════════════╝
");
        Console.WriteLine($"Watching: {inventoryPath}");
        Console.WriteLine($"Press Ctrl+C to exit auto-sync mode\n");

        // Load initial data and test connection
        var inventory = await DataStore.LoadInventoryAsync(inventoryPath);
        Console.WriteLine($"Loaded {inventory.Coffees.Count} coffees");
        Console.WriteLine($"API URL: {inventory.ApiUrl}");

        Console.Write("Testing API connection... ");
        var connected = await ApiSync.TestConnectionAsync(inventory.ApiUrl);
        Console.WriteLine(connected ? "✓ Connected" : "✗ Failed");

        if (!connected)
        {
            Console.WriteLine("Warning: API connection failed. Will retry on each sync.");
        }

        Console.WriteLine("\nMonitoring for changes...\n");

        // Set up file watcher
        var directory = Path.GetDirectoryName(inventoryPath) ?? Directory.GetCurrentDirectory();
        var fileName = Path.GetFileName(inventoryPath);

        using var watcher = new FileSystemWatcher(directory)
        {
            Filter = fileName,
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size
        };

        watcher.Changed += async (sender, e) => await OnFileChanged(e.FullPath);
        watcher.EnableRaisingEvents = true;

        // Keep the application running
        var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (sender, e) =>
        {
            e.Cancel = true;
            cts.Cancel();
            Console.WriteLine("\n\nStopping auto-sync mode...");
        };

        try
        {
            await Task.Delay(Timeout.Infinite, cts.Token);
        }
        catch (TaskCanceledException)
        {
            // Expected when user presses Ctrl+C
        }

        Console.WriteLine("Auto-sync mode stopped.");
    }

    private static async Task OnFileChanged(string filePath)
    {
        // Debounce multiple file change events
        var now = DateTime.UtcNow;
        if (now - _lastSyncTime < SyncCooldown)
        {
            return;
        }

        _lastSyncTime = now;

        // Wait a moment for file write to complete
        await Task.Delay(500);

        try
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] File changed detected, syncing...");

            var inventory = await DataStore.LoadInventoryAsync(filePath);
            var success = await ApiSync.SyncInventoryAsync(inventory);

            if (success)
            {
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] ✓ Sync completed successfully\n");
            }
            else
            {
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] ✗ Sync completed with errors\n");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Error during sync: {ex.Message}\n");
        }
    }
}