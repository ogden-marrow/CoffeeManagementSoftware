using System.Text;
using System.Text.Json;

namespace CoffeeManagementSoftware;

public static class ApiSync
{
    private static readonly HttpClient httpClient = new();

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

        foreach (var coffee in inventory.Coffees)
        {
            try
            {
                var result = await SyncCoffeeAsync(inventory.ApiUrl, inventory.ApiKey, coffee);
                if (result)
                {
                    successCount++;
                    Console.WriteLine($"  ✓ Synced: {coffee.Name}");
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

        Console.WriteLine($"\nSync complete: {successCount} succeeded, {errorCount} failed");
        return errorCount == 0;
    }

    private static async Task<bool> SyncCoffeeAsync(string apiUrl, string? apiKey, Coffee coffee)
    {
        var endpoint = $"{apiUrl.TrimEnd('/')}/api/Coffee/{coffee.Id}";

        // First, try to GET to see if it exists
        try
        {
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

            var json = JsonSerializer.Serialize(coffee, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response;

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
            }

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"    API Error ({response.StatusCode}): {errorBody}");
            }

            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"    Connection Error: {ex.Message}");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"    Unexpected Error: {ex.Message}");
            return false;
        }
    }

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