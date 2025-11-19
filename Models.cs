using System.Text.Json.Serialization;

namespace CoffeeManagementSoftware;

public class Coffee
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("origin")]
    public string Origin { get; set; } = string.Empty;

    [JsonPropertyName("roastLevel")]
    public string RoastLevel { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("pricePerBag")]
    public double PricePerBag { get; set; }

    [JsonPropertyName("stockQuantity")]
    public int StockQuantity { get; set; } // in Bags

    [JsonPropertyName("flavorNotes")]
    public List<string> FlavorNotes { get; set; } = new();

    [JsonPropertyName("imageUrl")]
    public string ImageUrl { get; set; } = "";

    [JsonPropertyName("isAvailable")]
    public bool IsAvailable { get; set; }

    [JsonPropertyName("roastedDate")]
    public DateTime RoastedDate { get; set; }

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("updatedAt")]
    public DateTime UpdatedAt { get; set; }
}

public class InventoryData
{
    [JsonPropertyName("apiUrl")]
    public string ApiUrl { get; set; } = "https://api.example.com";

    [JsonPropertyName("apiKey")]
    public string? ApiKey { get; set; }

    [JsonPropertyName("coffees")]
    public List<Coffee> Coffees { get; set; } = new();
}

public class Order
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("coffeeId")]
    public string CoffeeId { get; set; } = string.Empty;

    [JsonPropertyName("coffeeName")]
    public string CoffeeName { get; set; } = string.Empty;

    [JsonPropertyName("quantityBags")]
    public int QuantityBags { get; set; }

    [JsonPropertyName("pricePerBag")]
    public double PricePerBag { get; set; }

    [JsonPropertyName("totalPrice")]
    public double TotalPrice { get; set; }

    [JsonPropertyName("orderDate")]
    public DateTime OrderDate { get; set; }
}

public class OrderData
{
    [JsonPropertyName("orders")]
    public List<Order> Orders { get; set; } = new();
}