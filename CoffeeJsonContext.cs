using System.Text.Json.Serialization;

namespace CoffeeManagementSoftware;

// Context for local file storage (PascalCase property names)
[JsonSerializable(typeof(Coffee))]
[JsonSerializable(typeof(InventoryData))]
[JsonSerializable(typeof(Order))]
[JsonSerializable(typeof(OrderData))]
[JsonSourceGenerationOptions(
    WriteIndented = true,
    PropertyNameCaseInsensitive = true,
    DefaultIgnoreCondition = JsonIgnoreCondition.Never)]
public partial class CoffeeJsonContext : JsonSerializerContext
{
}

// Context for API communication (camelCase property names)
[JsonSerializable(typeof(Coffee))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.Never)]
public partial class ApiJsonContext : JsonSerializerContext
{
}   