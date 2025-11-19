namespace CoffeeManagementSoftware;

public static class MenuMode
{
    public static async Task RunAsync()
    {
        Console.Clear();
        Console.WriteLine(@"
╔═══════════════════════════════════════════╗
║   Coffee Management Software - Menu Mode  ║
╚═══════════════════════════════════════════╝
");

        var inventoryPath = DataStore.GetInventoryPath();
        var ordersPath = DataStore.GetOrdersPath();

        Console.WriteLine($"Inventory file: {inventoryPath}");
        Console.WriteLine($"Orders file: {ordersPath}");
        Console.WriteLine();

        while (true)
        {
            Console.WriteLine(@"
Main Menu:
----------
1. Add New Coffee
2. Update Stock Quantity
3. Toggle Coffee Availability
4. List All Coffees
5. Record Order/Sale
6. View Sales Report
7. Sync to API (Push)
8. Pull from API
9. Switch to Auto-Sync Mode
10. Exit

");
            Console.Write("Select option (1-10): ");
            var choice = Console.ReadLine()?.Trim();

            try
            {
                switch (choice)
                {
                    case "1":
                        await AddCoffeeInteractive(inventoryPath);
                        break;
                    case "2":
                        await UpdateStockInteractive(inventoryPath);
                        break;
                    case "3":
                        await ToggleAvailabilityInteractive(inventoryPath);
                        break;
                    case "4":
                        await ListCoffeesInteractive(inventoryPath);
                        break;
                    case "5":
                        await RecordOrderInteractive(inventoryPath, ordersPath);
                        break;
                    case "6":
                        await ViewReportInteractive(ordersPath);
                        break;
                    case "7":
                        await SyncToApiInteractive(inventoryPath);
                        break;
                    case "8":
                        await PullFromApiInteractive(inventoryPath);
                        break;
                    case "9":
                        Console.WriteLine("\nStarting auto-sync mode...");
                        await AutoMode.RunAsync(Array.Empty<string>());
                        return;
                    case "10":
                        Console.WriteLine("\nGoodbye!");
                        return;
                    default:
                        Console.WriteLine("\nInvalid option. Please try again.");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nError: {ex.Message}");
            }

            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
            Console.Clear();
        }
    }

    private static async Task AddCoffeeInteractive(string inventoryPath)
    {
        Console.WriteLine("\n=== Add New Coffee ===\n");

        Console.Write("Coffee name: ");
        var name = Console.ReadLine()?.Trim();

        Console.Write("Origin: ");
        var origin = Console.ReadLine()?.Trim();

        Console.Write("Roast level (Light/Medium/Dark): ");
        var roast = Console.ReadLine()?.Trim();

        Console.Write("Price per bag ($): ");
        var priceStr = Console.ReadLine()?.Trim();

        Console.Write("Initial stock (bags): ");
        var stockStr = Console.ReadLine()?.Trim();

        Console.Write("Description (optional): ");
        var description = Console.ReadLine()?.Trim() ?? "";

        Console.Write("Image URL (optional): ");
        var imageUrl = Console.ReadLine()?.Trim();

        Console.Write("Flavor notes (comma-separated, optional): ");
        var flavorInput = Console.ReadLine()?.Trim();
        var flavorNotes = string.IsNullOrEmpty(flavorInput)
            ? new List<string>()
            : flavorInput.Split(',').Select(f => f.Trim()).ToList();

        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(origin) ||
            string.IsNullOrEmpty(roast) || !double.TryParse(priceStr, out var price) ||
            !int.TryParse(stockStr, out var stock))
        {
            Console.WriteLine("\nError: Invalid input values.");
            return;
        }

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
            ImageUrl = string.IsNullOrEmpty(imageUrl) ? "" : imageUrl,
            IsAvailable = stock > 0,
            RoastedDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        inventory.Coffees.Add(coffee);
        await DataStore.SaveInventoryAsync(inventoryPath, inventory);

        Console.WriteLine($"\n✓ Coffee added successfully!");
        Console.WriteLine($"  ID: {coffee.Id}");
        Console.WriteLine($"  Name: {coffee.Name}");
        Console.WriteLine($"  Price: ${coffee.PricePerBag:F2}/bag");
        Console.WriteLine($"  Stock: {coffee.StockQuantity} bags");

        Console.Write("\nSync to API now? (y/n): ");
        var sync = Console.ReadLine()?.ToLower();
        if (sync == "y" || sync == "yes")
        {
            await ApiSync.SyncInventoryAsync(inventory);
        }
    }

    private static async Task UpdateStockInteractive(string inventoryPath)
    {
        Console.WriteLine("\n=== Update Stock Quantity ===\n");

        var inventory = await DataStore.LoadInventoryAsync(inventoryPath);

        if (inventory.Coffees.Count == 0)
        {
            Console.WriteLine("No coffees in inventory.");
            return;
        }

        // Show list
        Console.WriteLine("Available coffees:");
        for (int i = 0; i < inventory.Coffees.Count; i++)
        {
            var c = inventory.Coffees[i];
            Console.WriteLine($"{i + 1}. {c.Name} (Current stock: {c.StockQuantity} bags) [ID: {c.Id}]");
        }

        Console.Write("\nEnter coffee number or ID: ");
        var input = Console.ReadLine()?.Trim();

        Coffee? coffee = null;
        if (int.TryParse(input, out var index) && index > 0 && index <= inventory.Coffees.Count)
        {
            coffee = inventory.Coffees[index - 1];
        }
        else
        {
            coffee = inventory.Coffees.FirstOrDefault(c => c.Id == input);
        }

        if (coffee == null)
        {
            Console.WriteLine("Invalid selection.");
            return;
        }

        Console.Write($"New stock quantity for '{coffee.Name}': ");
        var stockStr = Console.ReadLine()?.Trim();

        if (!int.TryParse(stockStr, out var newStock))
        {
            Console.WriteLine("Invalid quantity.");
            return;
        }

        var oldStock = coffee.StockQuantity;
        coffee.StockQuantity = newStock;
        coffee.IsAvailable = newStock > 0;
        coffee.UpdatedAt = DateTime.UtcNow;

        await DataStore.SaveInventoryAsync(inventoryPath, inventory);

        Console.WriteLine($"\n✓ Stock updated for '{coffee.Name}'");
        Console.WriteLine($"  Old stock: {oldStock} bags");
        Console.WriteLine($"  New stock: {newStock} bags");

        Console.Write("\nSync to API now? (y/n): ");
        var sync = Console.ReadLine()?.ToLower();
        if (sync == "y" || sync == "yes")
        {
            await ApiSync.SyncInventoryAsync(inventory);
        }
    }

    private static async Task ListCoffeesInteractive(string inventoryPath)
    {
        var inventory = await DataStore.LoadInventoryAsync(inventoryPath);

        if (inventory.Coffees.Count == 0)
        {
            Console.WriteLine("\nNo coffees in inventory.");
            return;
        }

        Console.WriteLine($"\n=== Inventory ({inventory.Coffees.Count} coffees) ===\n");

        foreach (var coffee in inventory.Coffees.OrderBy(c => c.Name))
        {
            var availability = coffee.IsAvailable ? "✓ Available" : "✗ Out of Stock";
            Console.WriteLine($"[{coffee.Id}] {coffee.Name} - {availability}");
            Console.WriteLine($"  Origin: {coffee.Origin}");
            Console.WriteLine($"  Roast: {coffee.RoastLevel}");
            Console.WriteLine($"  Price: ${coffee.PricePerBag:F2}/bag");
            Console.WriteLine($"  Stock: {coffee.StockQuantity} bags");
            if (!string.IsNullOrEmpty(coffee.Description))
            {
                Console.WriteLine($"  Description: {coffee.Description}");
            }
            if (coffee.FlavorNotes.Count > 0)
            {
                Console.WriteLine($"  Flavors: {string.Join(", ", coffee.FlavorNotes)}");
            }
            Console.WriteLine($"  Roasted: {coffee.RoastedDate:MMM dd, yyyy}");
            Console.WriteLine();
        }
    }

    private static async Task RecordOrderInteractive(string inventoryPath, string ordersPath)
    {
        Console.WriteLine("\n=== Record Order/Sale ===\n");

        var inventory = await DataStore.LoadInventoryAsync(inventoryPath);
        var orders = await DataStore.LoadOrdersAsync(ordersPath);

        if (inventory.Coffees.Count == 0)
        {
            Console.WriteLine("No coffees in inventory.");
            return;
        }

        // Show available coffees
        var availableCoffees = inventory.Coffees.Where(c => c.IsAvailable).ToList();
        Console.WriteLine("Available coffees:");
        for (int i = 0; i < availableCoffees.Count; i++)
        {
            var c = availableCoffees[i];
            Console.WriteLine($"{i + 1}. {c.Name} - ${c.PricePerBag:F2}/bag (Stock: {c.StockQuantity} bags)");
        }

        Console.Write("\nSelect coffee number: ");
        var input = Console.ReadLine()?.Trim();

        if (!int.TryParse(input, out var index) || index < 1 || index > availableCoffees.Count)
        {
            Console.WriteLine("Invalid selection.");
            return;
        }

        var coffee = availableCoffees[index - 1];

        Console.Write($"Quantity to sell (bags, max {coffee.StockQuantity}): ");
        var quantityStr = Console.ReadLine()?.Trim();

        if (!int.TryParse(quantityStr, out var quantity) || quantity <= 0 || quantity > coffee.StockQuantity)
        {
            Console.WriteLine("Invalid quantity.");
            return;
        }

        // Create order
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

        Console.WriteLine($"\n✓ Order recorded successfully!");
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

    private static async Task ToggleAvailabilityInteractive(string inventoryPath)
    {
        Console.WriteLine("\n=== Toggle Coffee Availability ===\n");

        var inventory = await DataStore.LoadInventoryAsync(inventoryPath);

        if (inventory.Coffees.Count == 0)
        {
            Console.WriteLine("No coffees in inventory.");
            return;
        }

        // Show list
        Console.WriteLine("Coffees:");
        for (int i = 0; i < inventory.Coffees.Count; i++)
        {
            var c = inventory.Coffees[i];
            var status = c.IsAvailable ? "✓ AVAILABLE" : "✗ UNAVAILABLE";
            Console.WriteLine($"{i + 1}. {c.Name} [{status}] (Stock: {c.StockQuantity} bags) [ID: {c.Id}]");
        }

        Console.Write("\nEnter coffee number or ID: ");
        var input = Console.ReadLine()?.Trim();

        Coffee? coffee = null;
        if (int.TryParse(input, out var index) && index > 0 && index <= inventory.Coffees.Count)
        {
            coffee = inventory.Coffees[index - 1];
        }
        else
        {
            coffee = inventory.Coffees.FirstOrDefault(c => c.Id == input);
        }

        if (coffee == null)
        {
            Console.WriteLine("Invalid selection.");
            return;
        }

        coffee.IsAvailable = !coffee.IsAvailable;
        coffee.UpdatedAt = DateTime.UtcNow;

        await DataStore.SaveInventoryAsync(inventoryPath, inventory);

        var newStatus = coffee.IsAvailable ? "AVAILABLE" : "UNAVAILABLE";
        Console.WriteLine($"\n✓ '{coffee.Name}' is now {newStatus}");

        Console.Write("\nSync to API now? (y/n): ");
        var syncChoice = Console.ReadLine()?.ToLower();
        if (syncChoice == "y" || syncChoice == "yes")
        {
            await ApiSync.SyncInventoryAsync(inventory);
        }
    }

    private static async Task ViewReportInteractive(string ordersPath)
    {
        Console.WriteLine("\n=== Sales Report ===\n");

        Console.Write("Report period in days (default 30): ");
        var daysInput = Console.ReadLine()?.Trim();
        var days = string.IsNullOrEmpty(daysInput) || !int.TryParse(daysInput, out var d) ? 30 : d;

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

        Console.WriteLine($"\nSummary:");
        Console.WriteLine($"  Total Orders: {recentOrders.Count}");
        Console.WriteLine($"  Total Revenue: ${totalRevenue:F2}");
        Console.WriteLine($"  Total Bags Sold: {totalBags}");
        Console.WriteLine($"  Average Order Value: ${(totalRevenue / recentOrders.Count):F2}");
        Console.WriteLine($"  Average Bags per Order: {(totalBags / (double)recentOrders.Count):F2}");

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

        Console.WriteLine($"\nSales by Coffee:");
        Console.WriteLine(new string('-', 80));
        foreach (var sale in salesByCoffee)
        {
            Console.WriteLine($"\n{sale.Name}");
            Console.WriteLine($"  Orders: {sale.OrderCount}");
            Console.WriteLine($"  Total Bags: {sale.TotalBags}");
            Console.WriteLine($"  Total Revenue: ${sale.TotalRevenue:F2}");
            Console.WriteLine($"  Avg per Order: ${(sale.TotalRevenue / sale.OrderCount):F2}");
        }

        // Recent orders
        Console.WriteLine($"\nRecent Orders (Last 10):");
        Console.WriteLine(new string('-', 80));
        foreach (var order in recentOrders.OrderByDescending(o => o.OrderDate).Take(10))
        {
            Console.WriteLine($"{order.OrderDate:MMM dd, yyyy HH:mm} - {order.CoffeeName}");
            Console.WriteLine($"  {order.QuantityBags} bags @ ${order.PricePerBag:F2}/bag = ${order.TotalPrice:F2}");
        }
    }

    private static async Task SyncToApiInteractive(string inventoryPath)
    {
        Console.WriteLine("\n=== Sync to API (Push) ===\n");

        var inventory = await DataStore.LoadInventoryAsync(inventoryPath);

        Console.WriteLine($"API URL: {inventory.ApiUrl}");
        Console.WriteLine($"Coffees to sync: {inventory.Coffees.Count}");
        Console.WriteLine();

        Console.Write("Continue with sync? (y/n): ");
        var confirm = Console.ReadLine()?.ToLower();

        if (confirm != "y" && confirm != "yes")
        {
            Console.WriteLine("Sync cancelled.");
            return;
        }

        await ApiSync.SyncInventoryAsync(inventory);
    }

    private static async Task PullFromApiInteractive(string inventoryPath)
    {
        Console.WriteLine("\n=== Pull from API ===\n");

        var inventory = await DataStore.LoadInventoryAsync(inventoryPath);

        Console.WriteLine($"API URL: {inventory.ApiUrl}");
        Console.WriteLine();

        Console.Write("Testing API connection... ");
        var connected = await ApiSync.TestConnectionAsync(inventory.ApiUrl);
        Console.WriteLine(connected ? "✓ Connected" : "✗ Failed");

        if (!connected)
        {
            Console.WriteLine("Error: Could not connect to API");
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
}