# Coffee Management Software - Architecture

## Project Structure

```
CoffeeManagementSoftware/
├── Program.cs                    # Entry point - routes to menu or command mode
├── Models.cs                     # Data models (Coffee, Order, InventoryData, OrderData)
├── DataStore.cs                  # JSON file operations and ID generation
├── ApiSync.cs                    # HTTP client for API synchronization
├── MenuMode.cs                   # Interactive menu interface
├── AutoMode.cs                   # File watcher for auto-sync
├── Commands.cs                   # CLI command handlers
├── CoffeeManagementSoftware.csproj
├── CoffeeManagementSoftware.sln
├── README.md
├── QUICKSTART.md
├── LICENSE
├── .gitignore
├── inventory.example.json        # Example configuration
└── orders.example.json           # Example order data
```

## Component Overview

### Program.cs
Main entry point that:
- Parses command-line arguments
- Routes to interactive menu mode (no args) or command mode (with args)
- Handles command dispatch

### Models.cs
Data structures:
- **Coffee**: Full coffee product details matching API schema
- **InventoryData**: Wrapper containing API URL + coffee list
- **Order**: Sale record with coffee details and pricing
- **OrderData**: Wrapper containing order list

### DataStore.cs
File management utilities:
- Load/save inventory and orders from/to JSON
- Generate unique 12-character IDs
- Handle custom file paths via command-line args
- Create default files if they don't exist

### ApiSync.cs
API synchronization:
- Sync entire inventory to API server
- Check if coffee exists (GET) before deciding to POST or PUT
- Test API connection
- Handle HTTP errors gracefully

### MenuMode.cs
Interactive terminal UI:
- Menu-driven interface for all operations
- Guides user through adding coffees, recording orders, etc.
- Shows formatted lists and reports
- Confirms before syncing to API

### AutoMode.cs
File watching system:
- Monitors inventory.json for changes
- Automatically syncs when file is modified
- Debounces multiple rapid changes
- Runs continuously until Ctrl+C

### Commands.cs
CLI command implementations:
- `add` - Add new coffee with all details
- `update` - Update stock quantity
- `list` - Show all coffees
- `order` - Record sale and update stock
- `report` - Generate sales statistics
- `sync` - Manual sync to API
- `help` - Show usage information

## Data Flow

### Adding a Coffee
```
User Input (Menu or CLI)
    ↓
Collect coffee details
    ↓
Generate unique ID
    ↓
Create Coffee object
    ↓
Load inventory.json
    ↓
Add coffee to list
    ↓
Save inventory.json
    ↓
Optional: Sync to API
```

### Recording an Order
```
User selects coffee and quantity
    ↓
Load inventory.json and orders.json
    ↓
Validate stock availability
    ↓
Create Order record
    ↓
Save to orders.json
    ↓
Decrease coffee stock
    ↓
Update coffee availability flag
    ↓
Save inventory.json
    ↓
Optional: Sync inventory to API
```

### Auto-Sync Mode
```
Start FileSystemWatcher
    ↓
Monitor inventory.json
    ↓
Detect file change
    ↓
Debounce (wait 500ms)
    ↓
Load inventory.json
    ↓
Sync all coffees to API
    ↓
Report results
    ↓
Continue monitoring...
```

## API Integration

The system integrates with the WeRoasting API:

**Endpoints Used:**
- `GET /api/Coffee/{id}` - Check if coffee exists
- `POST /api/Coffee` - Create new coffee
- `PUT /api/Coffee/{id}` - Update existing coffee

**Sync Strategy:**
1. For each coffee in inventory
2. Try GET to check existence
3. If exists → PUT (update)
4. If not exists → POST (create)
5. Track success/failure for reporting

## Design Decisions

### Why JSON Files?
- Simple and human-readable
- Easy to backup and version control
- No database setup required
- Can be edited manually if needed

### Why Separate Files?
- `inventory.json` - Current state (needs frequent updates)
- `orders.json` - Historical record (append-only)
- Cleaner separation of concerns
- Easier to archive old orders

### Why Auto-Generated IDs?
- No manual tracking needed
- Guaranteed uniqueness
- Short enough to be manageable (12 chars)
- Compatible with API expectations

### Why Dual Interface?
- **Interactive menu**: Best for daily operations, user-friendly
- **Command-line**: Best for scripting, automation, power users
- Flexibility for different workflows

### Why No Customer Data?
- Focus on inventory and volume tracking
- Privacy-friendly
- Simpler data model
- Actual customer data handled by API/web store

## Extension Points

### Adding New Commands
1. Add handler method in `Commands.cs`
2. Add case in `Program.cs` switch statement
3. Add menu option in `MenuMode.cs` (if applicable)

### Adding New Data Fields
1. Update model in `Models.cs`
2. Commands and menu will automatically handle new fields via JSON serialization

### Changing API Endpoints
Modify `ApiSync.cs` methods - all API calls are centralized there

### Custom Reports
Add new report methods in `Commands.cs` and call from menu or CLI

## Performance Considerations

- File I/O is async throughout
- Debouncing prevents excessive API calls in auto-sync mode
- JSON serialization is efficient for small datasets (< 1000 coffees)
- HTTP client is reused, not recreated per request

## Error Handling

- File operations wrapped in try-catch
- API failures reported but don't crash app
- Invalid input validated before processing
- Missing files auto-created with defaults
- Connection tested before major operations
