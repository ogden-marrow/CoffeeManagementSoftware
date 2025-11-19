using System.Text.Json;
using System.Text.Json.Serialization;

namespace CoffeeManagementSoftware;

// Context for local file storage (PascalCase property names)
[JsonSerializable(typeof(Coffee))]
[JsonSerializable(typeof(InventoryData))]
[JsonSerializable(typeof(Order))]
[JsonSerializable(typeof(OrderData))]
[JsonSerializable(typeof(List<Coffee>))]
[JsonSourceGenerationOptions(
    WriteIndented = true,
    PropertyNameCaseInsensitive = true,
    DefaultIgnoreCondition = JsonIgnoreCondition.Never)]
public partial class CoffeeJsonContext : JsonSerializerContext
{
}

// Context for API communication (camelCase property names)
[JsonSerializable(typeof(Coffee))]
[JsonSerializable(typeof(List<Coffee>))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    PropertyNameCaseInsensitive = true,
    DefaultIgnoreCondition = JsonIgnoreCondition.Never)]
public partial class ApiJsonContext : JsonSerializerContext
{
    // Provide static JsonSerializerOptions for easier access
    public static JsonSerializerOptions Options => new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.Never
    };
}