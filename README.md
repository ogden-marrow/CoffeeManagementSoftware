# Coffee Management Software

A simple console application for managing coffee inventory and tracking sales for your roasting business. Supports both interactive menu mode and command-line interface, with automatic syncing to your API server.

## Features

- **Dual Interface**: Interactive menu mode or command-line arguments
- **Auto-Sync Mode**: Watches inventory file and automatically syncs changes to API
- **Inventory Management**: Add coffees, update stock quantities
- **Order Tracking**: Record sales and track volume without customer data
- **Sales Reports**: View sales statistics and trends
- **API Integration**: Syncs inventory to your WeRoasting API server

## Installation

### Prerequisites
- .NET 8.0 SDK

### Build
```bash
dotnet build
dotnet publish -c Release
```

## Usage

### Interactive Menu Mode
Simply run without arguments to enter interactive mode:
```bash
dotnet run
```

### Command-Line Mode

#### View Help
```bash
dotnet run help
```

#### Add a New Coffee
```bash
dotnet run add --name "Ethiopian Yirgacheffe" --origin "Ethiopia" --roast "Light" --price 18.50 --stock 50 --description "Bright and floral" --flavors "Blueberry, Jasmine, Citrus"
```

#### Update Stock Quantity
```bash
dotnet run update <coffee-id> --stock 25
```

#### Toggle Coffee Availability
Manually mark a coffee as available or unavailable (regardless of stock):
```bash
dotnet run toggle <coffee-id>
```

#### List All Coffees
```bash
dotnet run list
```

#### Record a Sale/Order
```bash
dotnet run order <coffee-id> --quantity 2.5
```

#### View Sales Report
```bash
# Last 30 days (default)
dotnet run report

# Last 7 days
dotnet run report --days 7
```

#### Manual Sync to API
```bash
dotnet run sync
```

#### Auto-Sync Mode
Watches the inventory file and automatically syncs changes:
```bash
dotnet run auto
```

### Custom File Paths
By default, files are stored in the current directory. You can specify custom paths:
```bash
dotnet run list --inventory-path /path/to/inventory.json --orders-path /path/to/orders.json
```

## Data Files

### inventory.json
Stores coffee inventory and API configuration:
```json
{
  "apiUrl": "https://api.weroasting.com",
  "apiKey": "your-api-key-here",
  "coffees": [
    {
      "id": "abc123def456",
      "name": "Ethiopian Yirgacheffe",
      "origin": "Ethiopia",
      "roastLevel": "Light",
      "description": "Bright and floral",
      "pricePerPound": 18.50,
      "stockQuantity": 50,
      "flavorNotes": ["Blueberry", "Jasmine", "Citrus"],
      "imageUrl": null,
      "isAvailable": true,
      "roastedDate": "2025-11-13T10:30:00Z",
      "createdAt": "2025-11-13T10:30:00Z",
      "updatedAt": "2025-11-13T10:30:00Z"
    }
  ]
}
```

### orders.json
Tracks sales/orders without customer information:
```json
{
  "orders": [
    {
      "id": "xyz789abc123",
      "coffeeId": "abc123def456",
      "coffeeName": "Ethiopian Yirgacheffe",
      "quantityPounds": 2.5,
      "pricePerPound": 18.50,
      "totalPrice": 46.25,
      "orderDate": "2025-11-13T14:20:00Z"
    }
  ]
}
```

## Workflow Examples

### Starting a New Day
1. Run in auto-sync mode to monitor inventory:
   ```bash
   dotnet run auto
   ```
2. Use the interactive menu (in another terminal) to manage daily operations

### Adding New Coffee
Interactive mode:
1. Run `dotnet run`
2. Select option 1 (Add New Coffee)
3. Follow the prompts

Command-line mode:
```bash
dotnet run add --name "Colombian Supremo" --origin "Colombia" --roast "Medium" --price 16.00 --stock 40
```

### Recording Daily Sales
1. Use interactive menu option 4, or
2. Use command line: `dotnet run order <coffee-id> --quantity 3.0`

### Weekly Sales Review
```bash
dotnet run report --days 7
```

### Manual Inventory Updates
When you receive new stock or roast a new batch:
1. Use interactive menu option 2, or
2. Use command line: `dotnet run update <coffee-id> --stock 75`

## API Configuration

The API URL and API key are stored in `inventory.json`:
```json
{
  "apiUrl": "https://api.weroasting.com",
  "apiKey": "your-api-key-here",
  "coffees": []
}
```

The API key is sent in the `X-API-Key` header with every request.

The application will sync to these endpoints:
- `POST /api/Coffee` - Create new coffee
- `PUT /api/Coffee/{id}` - Update existing coffee
- `GET /api/Coffee/{id}` - Check if coffee exists

## Sales Reports

The report shows:
- Total orders and revenue
- Total pounds sold
- Average order value
- Sales breakdown by coffee type
- Recent order history

Example:
```
Sales Report (Last 30 days)
================================================================================
Total Orders: 45
Total Revenue: $892.50
Total Pounds Sold: 87.50
Average Order Value: $19.83

Sales by Coffee:
--------------------------------------------------------------------------------
Ethiopian Yirgacheffe
  Orders: 18 | Pounds: 35.50 | Revenue: $656.75
Colombian Supremo
  Orders: 15 | Pounds: 32.00 | Revenue: $512.00
```

## Tips

- Run in auto-sync mode during business hours for automatic updates
- Use the interactive menu for daily operations - it's more user-friendly
- Use command-line mode for scripting or automation
- Check the sales report weekly to understand your best sellers
- The system auto-generates IDs for new coffees and orders

## License

MIT License - See LICENSE file for details