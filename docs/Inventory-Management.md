# Inventory Management System

## Overview

The inventory management system provides comprehensive stock tracking, reservation, and monitoring capabilities for the e-commerce platform. It ensures accurate stock levels, prevents overselling, and provides alerts when stock levels are low.

## Key Features

### 1. Stock Reservation System

The stock reservation system prevents overselling by temporarily locking inventory during the checkout process.

#### How It Works

1. **Reservation Creation**: When a customer begins checkout, stock is reserved for items in their cart
2. **Time-Limited**: Reservations expire after 15 minutes (configurable)
3. **Available Stock Calculation**: Available stock = Actual Stock - Active Reservations
4. **Automatic Release**: Expired reservations are automatically released
5. **Order Confirmation**: Reservations are marked as confirmed when an order is placed

#### API Endpoints

**Reserve Stock** (Internal - Called automatically during checkout)
```
POST /api/inventory/reserve
```

**Get Available Stock**
```
GET /api/inventory/available-stock/{productId}
```

Response:
```json
{
  "productId": 1,
  "sku": "PROD-001",
  "name": "Product Name",
  "actualStock": 100,
  "availableStock": 85,
  "reserved": 15
}
```

**Release Expired Reservations** (Admin - Should be run periodically)
```
POST /api/inventory/release-expired-reservations
```

### 2. Low-Stock Monitoring

The system monitors stock levels and creates alerts when inventory falls below configured thresholds.

#### Configuration

Each product has a `LowStockThreshold` property (default: 10 units). When available stock falls at or below this threshold, an alert is created.

#### Features

- **Automatic Detection**: Checked after every stock reduction
- **Deduplication**: Only one alert per product per 24 hours to avoid spam
- **Acknowledgement Tracking**: Admin users can acknowledge alerts
- **Product-Specific Thresholds**: Each product can have its own threshold

#### API Endpoints

**Get Low-Stock Alerts**
```
GET /api/inventory/low-stock-alerts?unacknowledgedOnly=true
```

Response:
```json
[
  {
    "id": 1,
    "productId": 5,
    "productName": "Wireless Headphones",
    "productSKU": "ELEC-WH-001",
    "currentStock": 8,
    "threshold": 10,
    "createdAt": "2025-11-27T07:30:00Z",
    "isAcknowledged": false,
    "acknowledgedAt": null,
    "acknowledgedBy": null
  }
]
```

**Acknowledge Low-Stock Alert**
```
POST /api/inventory/low-stock-alerts/{eventId}/acknowledge
Content-Type: application/json

"admin-user-id"
```

**Manual Low-Stock Check** (Admin - Can be scheduled)
```
POST /api/inventory/check-low-stock
```

Response:
```json
{
  "message": "Low-stock check completed",
  "unacknowledgedAlerts": 3
}
```

### 3. ERP Integration

The system integrates with ERP systems to synchronize inventory levels.

#### Stock Synchronization from ERP

**Push Stock to Local System**
```
POST /api/inventory/erp-sync
Content-Type: application/json

{
  "sku": "PROD-001",
  "quantity": 150,
  "reference": "ERP-SYNC-2025-11-27",
  "notes": "Daily inventory sync from ERP"
}
```

Response:
```json
{
  "message": "Stock synchronized successfully",
  "sku": "PROD-001",
  "previousStock": 100,
  "newStock": 150,
  "difference": 50,
  "movementId": 123
}
```

**Push Stock to ERP**
```
POST /api/inventory/push-to-erp/{productId}
```

Response:
```json
{
  "message": "Inventory updated successfully",
  "sku": "PROD-001",
  "quantity": 100
}
```

## Database Schema

### StockReservation Table

| Column | Type | Description |
|--------|------|-------------|
| Id | int | Primary key |
| ProductId | int | Foreign key to Products |
| Quantity | int | Reserved quantity |
| SessionId | string | User session identifier |
| OrderNumber | string | Order number (when confirmed) |
| CreatedAt | datetime | Reservation creation time |
| ExpiresAt | datetime | Reservation expiration time |
| IsReleased | bool | Whether reservation is released |
| ReleasedAt | datetime | When reservation was released |

### LowStockEvent Table

| Column | Type | Description |
|--------|------|-------------|
| Id | int | Primary key |
| ProductId | int | Foreign key to Products |
| ProductName | string | Product name (snapshot) |
| ProductSKU | string | Product SKU (snapshot) |
| CurrentStock | int | Stock level when alert created |
| Threshold | int | Threshold that triggered alert |
| CreatedAt | datetime | Alert creation time |
| IsAcknowledged | bool | Whether alert is acknowledged |
| AcknowledgedAt | datetime | When alert was acknowledged |
| AcknowledgedBy | string | Who acknowledged the alert |

### Product Table Updates

New field added:
- `LowStockThreshold` (int, default: 10): Minimum stock level before alert

## Usage Examples

### Checkout Flow with Stock Reservation

```csharp
// 1. Customer starts checkout
var cart = await _cartRepository.GetBySessionIdAsync(sessionId);

// 2. System validates and reserves stock
foreach (var item in cart.Items)
{
    var availableStock = await _stockReservationRepository.GetAvailableStockAsync(item.ProductId);
    if (availableStock < item.Quantity)
    {
        throw new InvalidOperationException("Insufficient stock");
    }
    
    await _stockReservationRepository.ReserveStockAsync(
        item.ProductId, 
        item.Quantity, 
        sessionId, 
        expirationMinutes: 15);
}

// 3. Process payment and create order
var order = await _checkoutService.ProcessCheckoutAsync(checkoutData);

// 4. Reservations are confirmed with order number
// Stock is reduced from actual inventory
```

### Monitoring Low Stock

```csharp
// Run this periodically (e.g., hourly via scheduled job)
await _inventoryService.CheckLowStockLevelsAsync();

// Get unacknowledged alerts
var alerts = await _lowStockEventRepository.GetUnacknowledgedAsync();

// Send notifications to admin
foreach (var alert in alerts)
{
    await _notificationService.SendLowStockAlertAsync(alert);
}
```

### ERP Synchronization

**Daily Stock Sync from ERP:**
```csharp
// Get products from ERP
var erpProducts = await _erpIntegration.GetAllProductsAsync();

foreach (var erpProduct in erpProducts)
{
    // Sync stock levels
    await _inventoryController.SyncStockFromErp(new ErpStockSyncDto
    {
        SKU = erpProduct.SKU,
        Quantity = erpProduct.StockLevel,
        Reference = $"ERP-SYNC-{DateTime.UtcNow:yyyyMMdd}",
        Notes = "Daily automated sync from ERP"
    });
}
```

**Push Order to ERP After Completion:**
```csharp
// After order is shipped
await _integrationEngine.ExecuteAsync(
    "erp_provider", 
    "syncorder", 
    order.OrderNumber);

// Update inventory in ERP
await _inventoryController.PushStockToErp(product.Id);
```

## Scheduled Tasks

### Recommended Background Jobs

1. **Release Expired Reservations** (Every 5 minutes)
   ```
   POST /api/inventory/release-expired-reservations
   ```

2. **Low-Stock Check** (Hourly or after every order)
   ```
   POST /api/inventory/check-low-stock
   ```

3. **ERP Sync** (Daily at off-peak hours)
   ```
   POST /api/inventory/erp-sync (for each product)
   ```

## Best Practices

1. **Set Appropriate Thresholds**: Configure `LowStockThreshold` based on:
   - Average daily sales velocity
   - Supplier lead time
   - Desired safety stock level

2. **Monitor Reservations**: Regularly release expired reservations to free up stock

3. **Acknowledge Alerts Promptly**: Review and acknowledge low-stock alerts to track which have been addressed

4. **Sync with ERP**: Maintain bi-directional sync with ERP systems for accurate inventory

5. **Test Stock Reservation**: Ensure reservations work correctly during high-traffic periods

## Error Handling

### Common Errors

**Insufficient Stock**
```json
{
  "error": "Insufficient stock available. Requested: 10, Available: 5"
}
```

**Product Not Found**
```json
{
  "error": "Product with SKU PROD-001 not found"
}
```

**ERP Sync Failed**
```json
{
  "error": "Failed to sync with ERP: Connection timeout"
}
```

## Integration Points

### Dependencies
- **Product Catalog**: Required for product information
- **Cart System**: Integration for checkout flow
- **Order Management**: Stock reduction on order creation
- **ERP Integration**: Bi-directional sync via Integration Engine

### Blocks
- **Checkout Process**: Cannot complete without available stock
- **Product Listing**: Display available stock to customers

## Testing

### Unit Tests
- `InventoryServiceTests`: Tests for inventory service logic
- Stock reservation business logic
- Low-stock detection logic

### Integration Tests
- `StockReservationRepositoryTests`: Database operations for reservations
- `LowStockEventRepositoryTests`: Database operations for alerts
- Available stock calculations
- Reservation expiration and release

## Future Enhancements

- Email notifications for low-stock alerts
- Real-time stock updates via SignalR
- Multi-location inventory support
- Automatic reorder suggestions
- Stock forecasting based on sales trends
- Bulk import of inventory levels from ERP
