using System.Text.Json;

namespace CoffeeManagementSoftware;

public static class DataStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        TypeInfoResolver = CoffeeJsonContext.Default
    };

    public static string GetInventoryPath(string[]? commandArgs = null)
    {
        // Check command line args for custom path
        if (commandArgs != null)
        {
            var pathIndex = Array.IndexOf(commandArgs, "--inventory-path");
            if (pathIndex >= 0 && pathIndex + 1 < commandArgs.Length)
            {
                return commandArgs[pathIndex + 1];
            }
        }

        return Path.Combine(Directory.GetCurrentDirectory(), "inventory.json");
    }

    public static string GetOrdersPath(string[]? commandArgs = null)
    {
        if (commandArgs != null)
        {
            var pathIndex = Array.IndexOf(commandArgs, "--orders-path");
            if (pathIndex >= 0 && pathIndex + 1 < commandArgs.Length)
            {
                return commandArgs[pathIndex + 1];
            }
        }

        return Path.Combine(Directory.GetCurrentDirectory(), "orders.json");
    }

    public static async Task<InventoryData> LoadInventoryAsync(string path)
    {
        if (!File.Exists(path))
        {
            var defaultData = new InventoryData
            {
                ApiUrl = "https://api.weroasting.com",
                Coffees = new List<Coffee>()
            };
            await SaveInventoryAsync(path, defaultData);
            return defaultData;
        }

        var json = await File.ReadAllTextAsync(path);
        return JsonSerializer.Deserialize<InventoryData>(json, JsonOptions) ?? new InventoryData();
    }

    public static async Task SaveInventoryAsync(string path, InventoryData data)
    {
        var json = JsonSerializer.Serialize(data, JsonOptions);
        await File.WriteAllTextAsync(path, json);
    }

    public static async Task<OrderData> LoadOrdersAsync(string path)
    {
        if (!File.Exists(path))
        {
            var defaultData = new OrderData { Orders = new List<Order>() };
            await SaveOrdersAsync(path, defaultData);
            return defaultData;
        }

        var json = await File.ReadAllTextAsync(path);
        return JsonSerializer.Deserialize<OrderData>(json, JsonOptions) ?? new OrderData();
    }

    public static async Task SaveOrdersAsync(string path, OrderData data)
    {
        var json = JsonSerializer.Serialize(data, JsonOptions);
        await File.WriteAllTextAsync(path, json);
    }

    public static string GenerateId()
    {
        return Guid.NewGuid().ToString("N")[..12]; // 12 character hex string
    }
}