namespace CoffeeManagementSoftware;

public static class Commands
{
    public static void ShowHelp()
    {
        Console.WriteLine(@"
Coffee Management Software
==========================

Usage: CoffeeManagementSoftware [command] [options]

Commands:
  (none)              - Run interactive menu mode
  auto                - Auto-sync mode (watches inventory.json for changes)
  add                 - Add a new coffee
  update              - Update coffee stock quantity
  toggle              - Toggle coffee availability on/off
  list                - List all coffees
  order               - Record a sale/order
  report              - Show sales report
  sync                - Push local inventory to API
  pull                - Pull inventory from API to local
  help                - Show this help message

Options:
  --inventory-path <path>   - Custom path to inventory.json
  --orders-path <path>      - Custom path to orders.json

Examples:
  CoffeeManagementSoftware
  CoffeeManagementSoftware auto
  CoffeeManagementSoftware add --name ""Ethiopian Yirgacheffe"" --origin ""Ethiopia"" --roast ""Medium"" --price 18.50 --stock 50
  CoffeeManagementSoftware update <id> --stock 25
  CoffeeManagementSoftware toggle <id>
  CoffeeManagementSoftware list
  CoffeeManagementSoftware order <coffee-id> --quantity 2
  CoffeeManagementSoftware report --days 30
  CoffeeManagementSoftware pull
  CoffeeManagementSoftware sync
");
    }

    public static async Task AddCoffeeAsync(string[] commandArgs)
    {
        var name = GetArgValue(commandArgs, "--name");
        var origin = GetArgValue(commandArgs, "--origin");
        var roast = GetArgValue(commandArgs, "--roast");
        var priceStr = GetArgValue(commandArgs, "--price");
        var stockStr = GetArgValue(commandArgs, "--stock");
        var description = GetArgValue(commandArgs, "--description") ?? "";
        var imageUrl = GetArgValue(commandArgs, "--image");
        var flavorNotes = GetArgValue(commandArgs, "--flavors")?.Split(',').Select(f => f.Trim()).ToList() ?? new List<string>();

        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(origin) ||
            string.IsNullOrEmpty(roast) || string.IsNullOrEmpty(priceStr) || string.IsNullOrEmpty(stockStr))
        {
            Console.WriteLine("Error: Missing required arguments");
            Console.WriteLine("Required: --name --origin --roast --price --stock");
            return;
        }

        if (!double.TryParse(priceStr, out var price) || !int.TryParse(stockStr, out var stock))
        {
            Console.WriteLine("Error: Invalid price or stock value");
            return;
        }

        var inventoryPath = DataStore.GetInventoryPath(commandArgs);
        var inventory = await DataStore.LoadInventoryAsync(inventoryPath);

        var coffee = new Coffee
        {
            Id = DataStore.GenerateId(),
            Name = name,
            Origin = origin,
            RoastLevel = roast,
            Description = description,
            PricePerBag = price,
            StockQuantity = stock,
            FlavorNotes = flavorNotes,
            ImageUrl = imageUrl ?? "",
            IsAvailable = stock > 0,
            RoastedDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        inventory.Coffees.Add(coffee);
        await DataStore.SaveInventoryAsync(inventoryPath, inventory);

        Console.WriteLine($"✓ Coffee added successfully!");
        Console.WriteLine($"  ID: {coffee.Id}");
        Console.WriteLine($"  Name: {coffee.Name}");
        Console.WriteLine($"  Price: ${coffee.PricePerBag:F2}/bag");
        Console.WriteLine($"  Stock: {coffee.StockQuantity} bags");

        // Ask if they want to sync
        Console.Write("\nSync to API now? (y/n): ");
        var sync = Console.ReadLine()?.ToLower();
        if (sync == "y" || sync == "yes")
        {
            await ApiSync.SyncInventoryAsync(inventory);
        }
    }

    public static async Task UpdateStockAsync(string[] commandArgs)
    {
        if (commandArgs.Length < 2)
        {
            Console.WriteLine("Error: Missing coffee ID");
            Console.WriteLine("Usage: update <coffee-id> --stock <quantity>");
            return;
        }

        var coffeeId = commandArgs[1];
        var stockStr = GetArgValue(commandArgs, "--stock");

        if (string.IsNullOrEmpty(stockStr) || !int.TryParse(stockStr, out var newStock))
        {
            Console.WriteLine("Error: Invalid or missing --stock value");
            return;
        }

        var inventoryPath = DataStore.GetInventoryPath(commandArgs);
        var inventory = await DataStore.LoadInventoryAsync(inventoryPath);

        var coffee = inventory.Coffees.FirstOrDefault(c => c.Id == coffeeId);
        if (coffee == null)
        {
            Console.WriteLine($"Error: Coffee with ID '{coffeeId}' not found");
            return;
        }

        var oldStock = coffee.StockQuantity;
        coffee.StockQuantity = newStock;
        coffee.IsAvailable = newStock > 0;
        coffee.UpdatedAt = DateTime.UtcNow;

        await DataStore.SaveInventoryAsync(inventoryPath, inventory);

        Console.WriteLine($"✓ Stock updated for '{coffee.Name}'");
        Console.WriteLine($"  Old stock: {oldStock} bags");
        Console.WriteLine($"  New stock: {newStock} bags");

        Console.Write("\nSync to API now? (y/n): ");
        var sync = Console.ReadLine()?.ToLower();
        if (sync == "y" || sync == "yes")
        {
            await ApiSync.SyncInventoryAsync(inventory);
        }
    }

    public static async Task ListCoffeesAsync(string[] commandArgs)
    {
        var inventoryPath = DataStore.GetInventoryPath(commandArgs);
        var inventory = await DataStore.LoadInventoryAsync(inventoryPath);

        if (inventory.Coffees.Count == 0)
        {
            Console.WriteLine("No coffees in inventory.");
            return;
        }

        Console.WriteLine($"\nInventory ({inventory.Coffees.Count} coffees):");
        Console.WriteLine(new string('=', 80));

        foreach (var coffee in inventory.Coffees.OrderBy(c => c.Name))
        {
            var availability = coffee.IsAvailable ? "✓" : "✗";
            Console.WriteLine($"{availability} [{coffee.Id}] {coffee.Name}");
            Console.WriteLine($"   Origin: {coffee.Origin} | Roast: {coffee.RoastLevel}");
            Console.WriteLine($"   Price: ${coffee.PricePerBag:F2}/bag | Stock: {coffee.StockQuantity} bags");
            if (coffee.FlavorNotes.Count > 0)
            {
                Console.WriteLine($"   Flavors: {string.Join(", ", coffee.FlavorNotes)}");
            }
            Console.WriteLine();
        }
    }

    public static async Task RecordOrderAsync(string[] commandArgs)
    {
        if (commandArgs.Length < 2)
        {
            Console.WriteLine("Error: Missing coffee ID");
            Console.WriteLine("Usage: order <coffee-id> --quantity <bags>");
            return;
        }

        var coffeeId = commandArgs[1];
        var quantityStr = GetArgValue(commandArgs, "--quantity");

        if (string.IsNullOrEmpty(quantityStr) || !int.TryParse(quantityStr, out var quantity) || quantity <= 0)
        {
            Console.WriteLine("Error: Invalid or missing --quantity value");
            return;
        }

        var inventoryPath = DataStore.GetInventoryPath(commandArgs);
        var ordersPath = DataStore.GetOrdersPath(commandArgs);

        var inventory = await DataStore.LoadInventoryAsync(inventoryPath);
        var orders = await DataStore.LoadOrdersAsync(ordersPath);

        var coffee = inventory.Coffees.FirstOrDefault(c => c.Id == coffeeId);
        if (coffee == null)
        {
            Console.WriteLine($"Error: Coffee with ID '{coffeeId}' not found");
            return;
        }

        if (coffee.StockQuantity < quantity)
        {
            Console.WriteLine($"Error: Insufficient stock. Available: {coffee.StockQuantity} bags");
            return;
        }

        // Create order record
        var order = new Order
        {
            Id = DataStore.GenerateId(),
            CoffeeId = coffee.Id,
            CoffeeName = coffee.Name,
            QuantityBags = quantity,
            PricePerBag = coffee.PricePerBag,
            TotalPrice = quantity * coffee.PricePerBag,
            OrderDate = DateTime.UtcNow
        };

        orders.Orders.Add(order);
        await DataStore.SaveOrdersAsync(ordersPath, orders);

        // Update inventory
        coffee.StockQuantity -= quantity;
        coffee.IsAvailable = coffee.StockQuantity > 0;
        coffee.UpdatedAt = DateTime.UtcNow;
        await DataStore.SaveInventoryAsync(inventoryPath, inventory);

        Console.WriteLine($"✓ Order recorded successfully!");
        Console.WriteLine($"  Order ID: {order.Id}");
        Console.WriteLine($"  Coffee: {coffee.Name}");
        Console.WriteLine($"  Quantity: {quantity} bags");
        Console.WriteLine($"  Total: ${order.TotalPrice:F2}");
        Console.WriteLine($"  Remaining stock: {coffee.StockQuantity} bags");

        Console.Write("\nSync inventory to API now? (y/n): ");
        var sync = Console.ReadLine()?.ToLower();
        if (sync == "y" || sync == "yes")
        {
            await ApiSync.SyncInventoryAsync(inventory);
        }
    }

    public static async Task ShowReportAsync(string[] commandArgs)
    {
        var daysStr = GetArgValue(commandArgs, "--days") ?? "30";
        if (!int.TryParse(daysStr, out var days))
        {
            days = 30;
        }

        var ordersPath = DataStore.GetOrdersPath(commandArgs);
        var orders = await DataStore.LoadOrdersAsync(ordersPath);

        var cutoffDate = DateTime.UtcNow.AddDays(-days);
        var recentOrders = orders.Orders.Where(o => o.OrderDate >= cutoffDate).ToList();

        Console.WriteLine($"\nSales Report (Last {days} days)");
        Console.WriteLine(new string('=', 80));

        if (recentOrders.Count == 0)
        {
            Console.WriteLine("No orders in this period.");
            return;
        }

        var totalRevenue = recentOrders.Sum(o => o.TotalPrice);
        var totalBags = recentOrders.Sum(o => o.QuantityBags);

        Console.WriteLine($"Total Orders: {recentOrders.Count}");
        Console.WriteLine($"Total Revenue: ${totalRevenue:F2}");
        Console.WriteLine($"Total Bags Sold: {totalBags}");
        Console.WriteLine($"Average Order Value: ${(totalRevenue / recentOrders.Count):F2}");
        Console.WriteLine();

        // Sales by coffee
        var salesByCoffee = recentOrders
            .GroupBy(o => o.CoffeeName)
            .Select(g => new
            {
                Name = g.Key,
                OrderCount = g.Count(),
                TotalBags = g.Sum(o => o.QuantityBags),
                TotalRevenue = g.Sum(o => o.TotalPrice)
            })
            .OrderByDescending(s => s.TotalRevenue)
            .ToList();

        Console.WriteLine("Sales by Coffee:");
        Console.WriteLine(new string('-', 80));
        foreach (var sale in salesByCoffee)
        {
            Console.WriteLine($"{sale.Name}");
            Console.WriteLine($"  Orders: {sale.OrderCount} | Bags: {sale.TotalBags} | Revenue: ${sale.TotalRevenue:F2}");
        }
    }

    public static async Task SyncToApiAsync(string[] commandArgs)
    {
        var inventoryPath = DataStore.GetInventoryPath(commandArgs);
        var inventory = await DataStore.LoadInventoryAsync(inventoryPath);

        Console.WriteLine("Testing API connection...");
        var connected = await ApiSync.TestConnectionAsync(inventory.ApiUrl);

        if (!connected)
        {
            Console.WriteLine($"Warning: Could not connect to API at {inventory.ApiUrl}");
            Console.Write("Continue anyway? (y/n): ");
            var cont = Console.ReadLine()?.ToLower();
            if (cont != "y" && cont != "yes")
            {
                return;
            }
        }

        await ApiSync.SyncInventoryAsync(inventory);
    }

    public static async Task PullFromApiAsync(string[] commandArgs)
    {
        var inventoryPath = DataStore.GetInventoryPath(commandArgs);
        var inventory = await DataStore.LoadInventoryAsync(inventoryPath);

        Console.WriteLine("Testing API connection...");
        var connected = await ApiSync.TestConnectionAsync(inventory.ApiUrl);

        if (!connected)
        {
            Console.WriteLine($"Error: Could not connect to API at {inventory.ApiUrl}");
            return;
        }

        // Warn about overwriting local data
        if (inventory.Coffees.Count > 0)
        {
            Console.WriteLine($"\n⚠️  WARNING: This will replace your {inventory.Coffees.Count} local coffees with data from the API!");
            Console.Write("Continue? (y/n): ");
            var confirm = Console.ReadLine()?.ToLower();
            if (confirm != "y" && confirm != "yes")
            {
                Console.WriteLine("Pull cancelled.");
                return;
            }
        }

        await ApiSync.PullInventoryAsync(inventory, inventoryPath);
    }

    public static async Task ToggleAvailabilityAsync(string[] commandArgs)
    {
        if (commandArgs.Length < 2)
        {
            Console.WriteLine("Error: Missing coffee ID");
            Console.WriteLine("Usage: toggle <coffee-id>");
            return;
        }

        var coffeeId = commandArgs[1];
        var inventoryPath = DataStore.GetInventoryPath(commandArgs);
        var inventory = await DataStore.LoadInventoryAsync(inventoryPath);

        var coffee = inventory.Coffees.FirstOrDefault(c => c.Id == coffeeId);
        if (coffee == null)
        {
            Console.WriteLine($"Error: Coffee with ID '{coffeeId}' not found");
            return;
        }

        coffee.IsAvailable = !coffee.IsAvailable;
        coffee.UpdatedAt = DateTime.UtcNow;

        await DataStore.SaveInventoryAsync(inventoryPath, inventory);

        var status = coffee.IsAvailable ? "AVAILABLE" : "UNAVAILABLE";
        Console.WriteLine($"✓ '{coffee.Name}' is now {status}");

        Console.Write("\nSync to API now? (y/n): ");
        var sync = Console.ReadLine()?.ToLower();
        if (sync == "y" || sync == "yes")
        {
            await ApiSync.SyncInventoryAsync(inventory);
        }
    }

    private static string? GetArgValue(string[] commandArgs, string argName)
    {
        var index = Array.IndexOf(commandArgs, argName);
        if (index >= 0 && index + 1 < commandArgs.Length)
        {
            return commandArgs[index + 1];
        }
        return null;
    }
}