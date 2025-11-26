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
