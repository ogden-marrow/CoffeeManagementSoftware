using System.Text;
using System.Text.Json;

namespace CoffeeManagementSoftware;

public static class ApiSync
{
    private static readonly HttpClient httpClient = new();

    /// <summary>
    /// Pull all coffees from the API and save to local inventory
    /// </summary>
    public static async Task<bool> PullInventoryAsync(InventoryData inventory, string inventoryPath)
    {
        if (string.IsNullOrEmpty(inventory.ApiUrl))
        {
            Console.WriteLine("Error: API URL not configured in inventory.json");
            return false;
        }

        Console.WriteLine($"Pulling inventory from {inventory.ApiUrl}...");

        try
        {
            var endpoint = $"{inventory.ApiUrl.TrimEnd('/')}/api/Coffee";

            using var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
            if (!string.IsNullOrEmpty(inventory.ApiKey))
            {
                request.Headers.Add("X-Deploy-Key", inventory.ApiKey);
            }

            var response = await httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"API Error ({response.StatusCode}): {errorBody}");
                return false;
            }

            var json = await response.Content.ReadAsStringAsync();
            var apiCoffees = JsonSerializer.Deserialize<List<Coffee>>(json, ApiJsonContext.Default.Options);

            if (apiCoffees == null || apiCoffees.Count == 0)
            {
                Console.WriteLine("No coffees found on API server");
                return true;
            }

            // Update local inventory with API data
            inventory.Coffees = apiCoffees;
            await DataStore.SaveInventoryAsync(inventoryPath, inventory);

            Console.WriteLine($"✓ Successfully pulled {apiCoffees.Count} coffees from API");

            // Show summary
            Console.WriteLine("\nPulled coffees:");
            foreach (var coffee in apiCoffees.OrderBy(c => c.Name))
            {
                Console.WriteLine($"  • {coffee.Name} - {coffee.StockQuantity} bags @ ${coffee.PricePerBag:F2}/bag");
            }

            return true;
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Connection Error: {ex.Message}");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error pulling inventory: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Sync local inventory to API - creates new coffees or updates existing ones
    /// </summary>
    public static async Task<bool> SyncInventoryAsync(InventoryData inventory)
    {
        if (string.IsNullOrEmpty(inventory.ApiUrl))
        {
            Console.WriteLine("Error: API URL not configured in inventory.json");
            return false;
        }

        Console.WriteLine($"Syncing {inventory.Coffees.Count} coffees to {inventory.ApiUrl}...");

        var successCount = 0;
        var errorCount = 0;
        var updatedCount = 0;
        var createdCount = 0;

        foreach (var coffee in inventory.Coffees)
        {
            try
            {
                var (success, wasUpdate) = await SyncCoffeeAsync(inventory.ApiUrl, inventory.ApiKey, coffee);
                if (success)
                {
                    successCount++;
                    if (wasUpdate)
                    {
                        updatedCount++;
                        Console.WriteLine($"  ✓ Updated: {coffee.Name}");
                    }
                    else
                    {
                        createdCount++;
                        Console.WriteLine($"  ✓ Created: {coffee.Name}");
                    }
                }
                else
                {
                    errorCount++;
                    Console.WriteLine($"  ✗ Failed: {coffee.Name}");
                }
            }
            catch (Exception ex)
            {
                errorCount++;
                Console.WriteLine($"  ✗ Error syncing {coffee.Name}: {ex.Message}");
            }
        }

        Console.WriteLine($"\nSync complete: {successCount} succeeded ({createdCount} created, {updatedCount} updated), {errorCount} failed");
        return errorCount == 0;
    }

    /// <summary>
    /// Sync a single coffee to the API
    /// </summary>
    private static async Task<(bool success, bool wasUpdate)> SyncCoffeeAsync(string apiUrl, string? apiKey, Coffee coffee)
    {
        var endpoint = $"{apiUrl.TrimEnd('/')}/api/Coffee/{coffee.Id}";

        try
        {
            // First, try to GET to see if it exists
            using var getRequest = new HttpRequestMessage(HttpMethod.Get, endpoint);
            if (!string.IsNullOrEmpty(apiKey))
            {
                getRequest.Headers.Add("X-Deploy-Key", apiKey);
            }

            var getResponse = await httpClient.SendAsync(getRequest);

            // Ensure ImageUrl is not null - use empty string if null
            if (string.IsNullOrEmpty(coffee.ImageUrl))
            {
                coffee.ImageUrl = "";
            }

            var json = JsonSerializer.Serialize(coffee, ApiJsonContext.Default.Coffee);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response;
            bool isUpdate;

            if (getResponse.IsSuccessStatusCode)
            {
                // Coffee exists, use PUT to update
                using var putRequest = new HttpRequestMessage(HttpMethod.Put, endpoint)
                {
                    Content = content
                };
                if (!string.IsNullOrEmpty(apiKey))
                {
                    putRequest.Headers.Add("X-Deploy-Key", apiKey);
                }
                response = await httpClient.SendAsync(putRequest);
                isUpdate = true;
            }
            else
            {
                // Coffee doesn't exist, use POST to create
                var createEndpoint = $"{apiUrl.TrimEnd('/')}/api/Coffee";
                using var postRequest = new HttpRequestMessage(HttpMethod.Post, createEndpoint)
                {
                    Content = content
                };
                if (!string.IsNullOrEmpty(apiKey))
                {
                    postRequest.Headers.Add("X-Deploy-Key", apiKey);
                }
                response = await httpClient.SendAsync(postRequest);
                isUpdate = false;
            }

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"    API Error ({response.StatusCode}): {errorBody}");
            }

            return (response.IsSuccessStatusCode, isUpdate);
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"    Connection Error: {ex.Message}");
            return (false, false);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"    Unexpected Error: {ex.Message}");
            return (false, false);
        }
    }

    /// <summary>
    /// Test connection to the API
    /// </summary>
    public static async Task<bool> TestConnectionAsync(string apiUrl)
    {
        try
        {
            var endpoint = $"{apiUrl.TrimEnd('/')}/api/Coffee";
            var response = await httpClient.GetAsync(endpoint);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}