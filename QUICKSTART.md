# Quick Start Guide

## First Time Setup

1. **Build the project:**
   ```bash
   dotnet build
   ```

2. **Run in interactive mode to create initial files:**
   ```bash
   dotnet run
   ```
   This will create `inventory.json` and `orders.json` in the current directory.

3. **Configure your API URL:**
   Edit `inventory.json` and update the `apiUrl` field:
   ```json
   {
     "apiUrl": "https://weroasting.com",
     "apiKey": "your-api-key-here",
     "coffees": []
   }
   ```

## Daily Usage

### Option 1: Auto-Sync Mode (Recommended)
Perfect for when you're actively working with the inventory:

```bash
dotnet run auto
```

Leave this running in one terminal. Any changes to `inventory.json` will automatically sync to your API.

### Option 2: Interactive Menu
User-friendly interface for all operations:

```bash
dotnet run
```

Then select from the menu:
- **1** - Add new coffee
- **2** - Update stock
- **3** - Toggle availability
- **4** - List coffees
- **5** - Record sale
- **6** - View report
- **7** - Sync to API
- **8** - Switch to auto-sync
- **9** - Exit

### Option 3: Command Line
Quick operations without menus:

```bash
# Add a coffee
dotnet run add --name "Guatemalan Antigua" --origin "Guatemala" --roast "Medium" --price 17.00 --stock 30

# Record a sale (in bags)
dotnet run order <coffee-id> --quantity 2

# View sales report
dotnet run report --days 7

# List inventory
dotnet run list
```

## Common Workflows

### Morning Routine
1. Start auto-sync mode
2. Use interactive menu (new terminal) for daily operations

### Adding New Roast Batch
1. Interactive menu → Option 1 (Add New Coffee)
2. Or: `dotnet run add --name "..." --origin "..." --roast "..." --price X.XX --stock XX`

### Recording Sales
1. Interactive menu → Option 5 (Record Order)
2. Or: `dotnet run order <coffee-id> --quantity X`

### End of Week Review
```bash
dotnet run report --days 7
```

## File Locations

Both JSON files are created in the directory where you run the application:
- `inventory.json` - Your coffee inventory and API config
- `orders.json` - Sales history

You can specify custom paths:
```bash
dotnet run list --inventory-path /path/to/inventory.json
```

## Tips

✓ Use auto-sync mode during business hours  
✓ Interactive menu is easiest for daily use  
✓ Check sales reports weekly  
✓ System auto-generates IDs - no need to track them  
✓ Stock automatically decreases when you record orders  
✓ All quantities are in **bags** (not pounds) to match API schema  

## Troubleshooting

**"API connection failed"**
- Check your `apiUrl` in `inventory.json`
- Verify your server is running
- The app will still work locally and you can sync manually later

**"Coffee not found"**
- Run `dotnet run list` to see all coffee IDs
- Copy the full ID from the list

**"File not found"**
- Make sure you're in the correct directory
- Or use `--inventory-path` to specify location

## Need Help?

Run `dotnet run help` to see all available commands and options.