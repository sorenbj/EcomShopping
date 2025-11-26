# API Documentation

## Base URL

- **Development**: `https://localhost:5001`
- **Production**: Configure based on your deployment

## Authentication

Currently, the API is configured with basic authentication foundation. JWT token-based authentication can be implemented by:

1. Adding `Microsoft.AspNetCore.Authentication.JwtBearer` package
2. Configuring JWT settings in `appsettings.json`
3. Adding `[Authorize]` attributes to protected endpoints

## Endpoints

### Products

#### Get All Products (Paginated)
```http
GET /api/products?page=1&pageSize=10&search=laptop&categoryId=1
```

**Query Parameters:**
- `page` (optional, default: 1): Page number
- `pageSize` (optional, default: 10): Items per page
- `search` (optional): Search term for name/description
- `categoryId` (optional): Filter by category ID

**Response:**
```json
{
  "items": [
    {
      "id": 1,
      "name": "Laptop",
      "description": "High-performance laptop",
      "price": 999.99,
      "sku": "LAP-001",
      "categoryId": 1,
      "stockQuantity": 50,
      "images": ["img1.jpg", "img2.jpg"],
      "createdAt": "2024-01-01T00:00:00Z",
      "updatedAt": null
    }
  ],
  "totalCount": 100,
  "page": 1,
  "pageSize": 10,
  "totalPages": 10
}
```

#### Get Product by ID
```http
GET /api/products/{id}
```

**Response:**
```json
{
  "id": 1,
  "name": "Laptop",
  "description": "High-performance laptop",
  "price": 999.99,
  "sku": "LAP-001",
  "categoryId": 1,
  "stockQuantity": 50,
  "images": ["img1.jpg", "img2.jpg"],
  "createdAt": "2024-01-01T00:00:00Z",
  "updatedAt": null,
  "category": {
    "id": 1,
    "name": "Electronics"
  }
}
```

#### Create Product
```http
POST /api/products
Content-Type: application/json
```

**Request Body:**
```json
{
  "name": "Laptop",
  "description": "High-performance laptop",
  "price": 999.99,
  "sku": "LAP-001",
  "categoryId": 1,
  "stockQuantity": 50,
  "images": ["img1.jpg", "img2.jpg"]
}
```

**Response:** `201 Created` with product object

#### Update Product
```http
PUT /api/products/{id}
Content-Type: application/json
```

**Request Body:** Same as Create Product

**Response:** `204 No Content`

#### Delete Product
```http
DELETE /api/products/{id}
```

**Response:** `204 No Content`

---

### Categories

#### Get All Categories
```http
GET /api/categories
```

**Response:**
```json
[
  {
    "id": 1,
    "name": "Electronics",
    "description": "Electronic devices",
    "parentCategoryId": null,
    "createdAt": "2024-01-01T00:00:00Z",
    "subCategories": [
      {
        "id": 2,
        "name": "Laptops",
        "parentCategoryId": 1
      }
    ]
  }
]
```

#### Get Category by ID
```http
GET /api/categories/{id}
```

#### Create Category
```http
POST /api/categories
Content-Type: application/json
```

**Request Body:**
```json
{
  "name": "Electronics",
  "description": "Electronic devices",
  "parentCategoryId": null
}
```

#### Update Category
```http
PUT /api/categories/{id}
```

#### Delete Category
```http
DELETE /api/categories/{id}
```

---

### Cart

#### Get Cart
```http
GET /api/cart?sessionId=abc123&userId=user123
```

**Query Parameters:**
- `sessionId` (optional): Session ID for anonymous users
- `userId` (optional): User ID for authenticated users

**Response:**
```json
{
  "id": 1,
  "sessionId": "abc123",
  "userId": "user123",
  "createdAt": "2024-01-01T00:00:00Z",
  "items": [
    {
      "id": 1,
      "productId": 1,
      "quantity": 2,
      "unitPrice": 999.99,
      "addedAt": "2024-01-01T00:00:00Z",
      "product": {
        "id": 1,
        "name": "Laptop",
        "price": 999.99
      }
    }
  ]
}
```

#### Add Item to Cart
```http
POST /api/cart/items
Content-Type: application/json
```

**Request Body:**
```json
{
  "sessionId": "abc123",
  "userId": "user123",
  "productId": 1,
  "quantity": 2
}
```

**Response:** `200 OK` with updated cart

#### Update Cart Item
```http
PUT /api/cart/items/{id}
Content-Type: application/json
```

**Request Body:**
```json
{
  "sessionId": "abc123",
  "userId": "user123",
  "quantity": 3
}
```

**Response:** `204 No Content`

#### Remove Cart Item
```http
DELETE /api/cart/items/{id}?sessionId=abc123&userId=user123
```

**Response:** `204 No Content`

---

### Orders

#### Get Orders
```http
GET /api/orders?userId=user123
```

**Query Parameters:**
- `userId` (optional): Filter by user ID (admin: returns all if omitted)

**Response:**
```json
[
  {
    "id": 1,
    "orderNumber": "ORD-20240101120000-ABC123",
    "userId": "user123",
    "status": "Pending",
    "totalAmount": 1999.98,
    "orderDate": "2024-01-01T00:00:00Z",
    "items": [
      {
        "id": 1,
        "productId": 1,
        "quantity": 2,
        "unitPrice": 999.99,
        "totalPrice": 1999.98
      }
    ]
  }
]
```

#### Get Order by ID
```http
GET /api/orders/{id}
```

**Response:**
```json
{
  "id": 1,
  "orderNumber": "ORD-20240101120000-ABC123",
  "userId": "user123",
  "status": "Pending",
  "totalAmount": 1999.98,
  "orderDate": "2024-01-01T00:00:00Z",
  "shippingAddress": {
    "id": 1,
    "firstName": "John",
    "lastName": "Doe",
    "street": "123 Main St",
    "city": "New York",
    "state": "NY",
    "postalCode": "10001",
    "country": "USA"
  },
  "billingAddress": {...},
  "items": [...]
}
```

#### Checkout (Create Order)
```http
POST /api/orders/checkout
Content-Type: application/json
```

**Request Body:**
```json
{
  "sessionId": "abc123",
  "userId": "user123",
  "shippingAddressId": 1,
  "billingAddressId": 1
}
```

**Response:** `201 Created` with order object

#### Update Order Status
```http
PUT /api/orders/{id}/status
Content-Type: application/json
```

**Request Body:**
```json
{
  "status": "Shipped"
}
```

**Valid Status Values:**
- `Pending` (0)
- `Processing` (1)
- `Shipped` (2)
- `Delivered` (3)
- `Cancelled` (4)

**Response:** `204 No Content`

---

### Coupons

#### Get All Coupons
```http
GET /api/coupons
```

**Response:**
```json
[
  {
    "id": 1,
    "code": "SAVE20",
    "description": "20% off your order",
    "type": 0,
    "value": 20.0,
    "minimumOrderAmount": 50.0,
    "maximumDiscountAmount": 25.0,
    "validFrom": "2024-01-01T00:00:00Z",
    "validUntil": "2024-12-31T23:59:59Z",
    "usageLimit": 100,
    "usageCount": 25,
    "isActive": true
  }
]
```

**Coupon Types:**
- `Percentage` (0): Discount is a percentage of subtotal
- `FixedAmount` (1): Discount is a fixed dollar amount
- `FreeShipping` (2): Free shipping applied

#### Get Active Coupons
```http
GET /api/coupons/active
```

Returns only coupons that are currently active and within their validity period.

#### Get Coupon by ID
```http
GET /api/coupons/{id}
```

#### Validate Coupon
```http
POST /api/coupons/validate
Content-Type: application/json
```

**Request Body:**
```json
{
  "code": "SAVE20",
  "orderAmount": 100.0
}
```

**Response:**
```json
{
  "isValid": true,
  "coupon": {
    "id": 1,
    "code": "SAVE20",
    "description": "20% off your order",
    "type": 0,
    "value": 20.0,
    "minimumOrderAmount": 50.0,
    "maximumDiscountAmount": 25.0,
    "validFrom": "2024-01-01T00:00:00Z",
    "validUntil": "2024-12-31T23:59:59Z",
    "usageLimit": 100,
    "usageCount": 25,
    "isActive": true
  },
  "discountAmount": 20.0
}
```

**Error Response:**
```json
{
  "isValid": false,
  "errorMessage": "Coupon has expired"
}
```

#### Create Coupon
```http
POST /api/coupons
Content-Type: application/json
```

**Request Body:**
```json
{
  "code": "SAVE20",
  "description": "20% off your order",
  "type": 0,
  "value": 20.0,
  "minimumOrderAmount": 50.0,
  "maximumDiscountAmount": 25.0,
  "validFrom": "2024-01-01T00:00:00Z",
  "validUntil": "2024-12-31T23:59:59Z",
  "usageLimit": 100
}
```

**Response:** `201 Created` with coupon object

#### Update Coupon
```http
PUT /api/coupons/{id}
Content-Type: application/json
```

**Request Body:** Same as Create Coupon

**Response:** `204 No Content`

#### Delete Coupon
```http
DELETE /api/coupons/{id}
```

**Response:** `204 No Content`

---

### Enhanced Checkout

#### Checkout (Create Order from Cart)
```http
POST /api/orders/checkout
Content-Type: application/json
```

**Request Body:**
```json
{
  "sessionId": "session-123",
  "userId": "user-456",
  "shippingAddress": {
    "firstName": "John",
    "lastName": "Doe",
    "street": "123 Main St",
    "city": "San Francisco",
    "state": "CA",
    "postalCode": "94105",
    "country": "USA",
    "phone": "+1-555-0100"
  },
  "billingAddress": {
    "firstName": "John",
    "lastName": "Doe",
    "street": "123 Main St",
    "city": "San Francisco",
    "state": "CA",
    "postalCode": "94105",
    "country": "USA",
    "phone": "+1-555-0100"
  },
  "useSameAddressForBilling": true,
  "couponCode": "SAVE20",
  "taxRate": 0.08,
  "paymentMethod": "CreditCard",
  "paymentDetails": {
    "cardNumber": "4111111111111111",
    "cardHolderName": "John Doe",
    "expiryMonth": "12",
    "expiryYear": "2025",
    "cvv": "123"
  }
}
```

**Field Descriptions:**
- `sessionId`: Required. Cart session identifier
- `userId`: Optional. User identifier if logged in
- `shippingAddress`: Required. Shipping address details
- `billingAddress`: Optional. If not provided, uses shipping address
- `useSameAddressForBilling`: Default true. Use shipping as billing address
- `couponCode`: Optional. Coupon code to apply
- `taxRate`: Required. Tax rate as decimal (e.g., 0.08 for 8%)
- `paymentMethod`: Required. Payment method name
- `paymentDetails`: Optional. Payment card details for processing

**Response:**
```json
{
  "success": true,
  "order": {
    "id": 1,
    "orderNumber": "ORD-20240101120000-ABC123",
    "userId": "user-456",
    "status": 0,
    "paymentStatus": 2,
    "subTotal": 100.00,
    "discountAmount": 20.00,
    "taxAmount": 6.40,
    "shippingAmount": 5.99,
    "totalAmount": 92.39,
    "couponCode": "SAVE20",
    "taxRate": 0.08,
    "paymentMethod": "CreditCard",
    "paymentTransactionId": "TXN-20240101120000-1000",
    "orderDate": "2024-01-01T12:00:00Z",
    "shippingAddress": {
      "id": 1,
      "firstName": "John",
      "lastName": "Doe",
      "street": "123 Main St",
      "city": "San Francisco",
      "state": "CA",
      "postalCode": "94105",
      "country": "USA",
      "phone": "+1-555-0100"
    },
    "billingAddress": {
      "id": 1,
      "firstName": "John",
      "lastName": "Doe",
      "street": "123 Main St",
      "city": "San Francisco",
      "state": "CA",
      "postalCode": "94105",
      "country": "USA",
      "phone": "+1-555-0100"
    },
    "items": [
      {
        "id": 1,
        "productId": 1,
        "productName": "Wireless Headphones",
        "productSku": "ELEC-WH-001",
        "quantity": 2,
        "unitPrice": 50.00,
        "totalPrice": 100.00
      }
    ]
  }
}
```

**Payment Status Values:**
- `Pending` (0): Payment not yet processed
- `Authorized` (1): Payment authorized but not captured
- `Captured` (2): Payment successfully captured
- `Failed` (3): Payment failed
- `Refunded` (4): Payment fully refunded
- `PartiallyRefunded` (5): Payment partially refunded

**Error Response:**
```json
{
  "success": false,
  "errorMessage": "Cart is empty"
}
```

**Common Error Messages:**
- "Cart is empty"
- "Invalid coupon code"
- "Coupon is not valid or has expired"
- "Insufficient stock for {product}. Only {count} available."
- "Payment failed: {error message}"

**Checkout Flow:**
1. Retrieve cart by sessionId or userId
2. Validate all items have sufficient inventory
3. Calculate subtotal from cart items
4. Apply coupon discount if provided
5. Calculate shipping costs
6. Calculate tax on taxable amount
7. Authorize payment if payment details provided
8. Create order with all calculated amounts
9. Reduce product inventory
10. Clear cart
11. Capture payment if authorized

---

## Error Responses

All endpoints return consistent error responses:

### 400 Bad Request
```json
{
  "message": "Invalid request data"
}
```

### 404 Not Found
```json
{
  "message": "Resource not found"
}
```

### 500 Internal Server Error
```json
{
  "message": "An error occurred while processing your request"
}
```

---

## CORS

CORS is configured to allow all origins in development. For production:

1. Update `Program.cs` to restrict origins
2. Configure specific allowed origins in `appsettings.json`

---

## Rate Limiting

Not currently implemented. Consider adding:
- `AspNetCoreRateLimit` package
- Configure limits per endpoint/IP

---

## Swagger/OpenAPI

Interactive API documentation is available at:
- `https://localhost:5001/swagger`

Features:
- Try out endpoints directly
- View request/response schemas
- Download OpenAPI specification

---

## Best Practices

1. **Pagination**: Always use pagination for list endpoints
2. **Error Handling**: Handle exceptions gracefully
3. **Validation**: Validate input before processing
4. **Logging**: Log errors and important events
5. **Authentication**: Implement JWT for production
6. **Versioning**: Consider API versioning for future changes
