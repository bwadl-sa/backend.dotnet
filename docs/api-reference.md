# API Reference

## Overview

The Bwadl API is a RESTful web service built with ASP.NET Core 8.0, featuring comprehensive CRUD operations, authentication, authorization, and enterprise-grade features like caching, logging, and resilience patterns.

## Base URL

- **Development**: `https://localhost:7260`
- **Staging**: `https://staging-api.bwadl.com`
- **Production**: `https://api.bwadl.com`

## Authentication

### JWT Bearer Token

The API uses JWT (JSON Web Token) for authentication. Include the token in the Authorization header:

```http
Authorization: Bearer <your-jwt-token>
```

### API Key (Optional)

For machine-to-machine communication, API keys can be used:

```http
X-API-Key: <your-api-key>
```

## API Versioning

The API supports versioning through URL path and headers:

### URL Versioning
```http
GET /api/v1/users
GET /api/v2/users
```

### Header Versioning
```http
GET /api/users
X-Version: 1.0
```

## Content Types

- **Request**: `application/json`
- **Response**: `application/json`
- **Character Encoding**: UTF-8

## Standard Response Format

### Success Response
```json
{
  "success": true,
  "data": {
    // Response data
  },
  "metadata": {
    "timestamp": "2025-07-04T10:30:00Z",
    "version": "1.0",
    "requestId": "12345678-1234-1234-1234-123456789012"
  }
}
```

### Error Response
```json
{
  "success": false,
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "One or more validation errors occurred.",
    "details": [
      {
        "field": "email",
        "message": "Email is required."
      }
    ]
  },
  "metadata": {
    "timestamp": "2025-07-04T10:30:00Z",
    "version": "1.0",
    "requestId": "12345678-1234-1234-1234-123456789012"
  }
}
```

## Status Codes

| Code | Description |
|------|-------------|
| 200 | OK - Request successful |
| 201 | Created - Resource created successfully |
| 204 | No Content - Request successful, no content returned |
| 400 | Bad Request - Invalid request data |
| 401 | Unauthorized - Authentication required |
| 403 | Forbidden - Insufficient permissions |
| 404 | Not Found - Resource not found |
| 409 | Conflict - Resource conflict |
| 422 | Unprocessable Entity - Validation errors |
| 429 | Too Many Requests - Rate limit exceeded |
| 500 | Internal Server Error - Server error |
| 503 | Service Unavailable - Service temporarily unavailable |

## Endpoints

### Health Checks

#### Check API Health
```http
GET /health
```

**Response:**
```json
{
  "status": "Healthy",
  "totalDuration": "00:00:00.0125",
  "entries": {
    "database": {
      "status": "Healthy",
      "duration": "00:00:00.0123"
    },
    "redis": {
      "status": "Healthy",
      "duration": "00:00:00.0002"
    }
  }
}
```

#### Health Check UI
```http
GET /health/ui
```

### Configuration

#### Get API Configuration
```http
GET /api/v1/configuration
```

**Response:**
```json
{
  "success": true,
  "data": {
    "version": "1.0.0",
    "environment": "Development",
    "features": {
      "authentication": true,
      "caching": true,
      "logging": true
    }
  }
}
```

### Users Management

#### Get All Users
```http
GET /api/v1/users
```

**Query Parameters:**
- `page` (int, optional): Page number (default: 1)
- `pageSize` (int, optional): Items per page (default: 10, max: 100)
- `search` (string, optional): Search term
- `sortBy` (string, optional): Sort field
- `sortOrder` (string, optional): Sort order (asc/desc)

**Response:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "123e4567-e89b-12d3-a456-426614174000",
        "email": "user@example.com",
        "firstName": "John",
        "lastName": "Doe",
        "isActive": true,
        "createdAt": "2025-01-01T10:00:00Z",
        "updatedAt": "2025-01-02T15:30:00Z"
      }
    ],
    "pagination": {
      "currentPage": 1,
      "pageSize": 10,
      "totalItems": 1,
      "totalPages": 1,
      "hasNextPage": false,
      "hasPreviousPage": false
    }
  }
}
```

#### Get User by ID
```http
GET /api/v1/users/{id}
```

**Path Parameters:**
- `id` (guid): User ID

**Response:**
```json
{
  "success": true,
  "data": {
    "id": "123e4567-e89b-12d3-a456-426614174000",
    "email": "user@example.com",
    "firstName": "John",
    "lastName": "Doe",
    "isActive": true,
    "roles": ["User"],
    "preferences": {
      "language": "en",
      "timezone": "UTC"
    },
    "createdAt": "2025-01-01T10:00:00Z",
    "updatedAt": "2025-01-02T15:30:00Z"
  }
}
```

#### Create User
```http
POST /api/v1/users
```

**Request Body:**
```json
{
  "email": "newuser@example.com",
  "firstName": "Jane",
  "lastName": "Smith",
  "password": "SecurePassword123!",
  "roles": ["User"]
}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "id": "123e4567-e89b-12d3-a456-426614174001",
    "email": "newuser@example.com",
    "firstName": "Jane",
    "lastName": "Smith",
    "isActive": true,
    "createdAt": "2025-07-04T10:30:00Z"
  }
}
```

#### Update User
```http
PUT /api/v1/users/{id}
```

**Path Parameters:**
- `id` (guid): User ID

**Request Body:**
```json
{
  "firstName": "Jane",
  "lastName": "Doe",
  "isActive": true
}
```

#### Delete User
```http
DELETE /api/v1/users/{id}
```

**Path Parameters:**
- `id` (guid): User ID

**Response:** 204 No Content

### Authentication

#### Login
```http
POST /api/v1/auth/login
```

**Request Body:**
```json
{
  "email": "user@example.com",
  "password": "SecurePassword123!"
}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "refresh-token-here",
    "expiresAt": "2025-07-04T11:30:00Z",
    "user": {
      "id": "123e4567-e89b-12d3-a456-426614174000",
      "email": "user@example.com",
      "firstName": "John",
      "lastName": "Doe",
      "roles": ["User"]
    }
  }
}
```

#### Refresh Token
```http
POST /api/v1/auth/refresh
```

**Request Body:**
```json
{
  "refreshToken": "refresh-token-here"
}
```

#### Logout
```http
POST /api/v1/auth/logout
```

**Headers:**
```http
Authorization: Bearer <jwt-token>
```

## Error Handling

### Validation Errors

When request validation fails, the API returns a 422 status code with detailed error information:

```json
{
  "success": false,
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "One or more validation errors occurred.",
    "details": [
      {
        "field": "email",
        "message": "Email is required."
      },
      {
        "field": "password",
        "message": "Password must be at least 8 characters long."
      }
    ]
  }
}
```

### Business Logic Errors

For business rule violations:

```json
{
  "success": false,
  "error": {
    "code": "BUSINESS_RULE_VIOLATION",
    "message": "User email already exists.",
    "details": []
  }
}
```

### System Errors

For unexpected system errors:

```json
{
  "success": false,
  "error": {
    "code": "INTERNAL_ERROR",
    "message": "An unexpected error occurred. Please try again later.",
    "details": []
  }
}
```

## Rate Limiting

The API implements rate limiting to prevent abuse:

- **Authenticated Users**: 1000 requests per hour
- **Anonymous Users**: 100 requests per hour

Rate limit headers are included in responses:

```http
X-RateLimit-Limit: 1000
X-RateLimit-Remaining: 999
X-RateLimit-Reset: 1625097600
```

## Caching

The API implements intelligent caching:

- **Cache-Control**: Specifies caching directives
- **ETag**: Entity tags for conditional requests
- **Last-Modified**: Last modification timestamp

Example headers:

```http
Cache-Control: max-age=300, public
ETag: "abc123"
Last-Modified: Wed, 04 Jul 2025 10:30:00 GMT
```

## Pagination

List endpoints support pagination with the following parameters:

- `page`: Page number (1-based, default: 1)
- `pageSize`: Items per page (default: 10, max: 100)

Response includes pagination metadata:

```json
{
  "pagination": {
    "currentPage": 1,
    "pageSize": 10,
    "totalItems": 150,
    "totalPages": 15,
    "hasNextPage": true,
    "hasPreviousPage": false
  }
}
```

## Filtering and Sorting

### Filtering

Use query parameters for filtering:

```http
GET /api/v1/users?isActive=true&role=Admin&createdAfter=2025-01-01
```

### Sorting

Use `sortBy` and `sortOrder` parameters:

```http
GET /api/v1/users?sortBy=lastName&sortOrder=asc
```

Multiple sort fields:

```http
GET /api/v1/users?sortBy=lastName,firstName&sortOrder=asc,desc
```

## API Clients

### .NET Client

```csharp
var client = new HttpClient();
client.BaseAddress = new Uri("https://localhost:7260");
client.DefaultRequestHeaders.Authorization = 
    new AuthenticationHeaderValue("Bearer", token);

var response = await client.GetAsync("/api/v1/users");
var users = await response.Content.ReadFromJsonAsync<ApiResponse<UserDto[]>>();
```

### JavaScript/TypeScript

```typescript
const apiClient = {
  baseURL: 'https://localhost:7260',
  headers: {
    'Content-Type': 'application/json',
    'Authorization': `Bearer ${token}`
  }
};

const response = await fetch(`${apiClient.baseURL}/api/v1/users`, {
  headers: apiClient.headers
});
const data = await response.json();
```

## Testing

### Using .http Files

The project includes `.http` files for testing endpoints:

```http
### Get all users
GET https://localhost:7260/api/v1/users
Authorization: Bearer {{token}}

### Create user
POST https://localhost:7260/api/v1/users
Content-Type: application/json

{
  "email": "test@example.com",
  "firstName": "Test",
  "lastName": "User",
  "password": "TestPassword123!"
}
```

### Swagger/OpenAPI

Access interactive API documentation at:

```
https://localhost:7260/swagger
```

## Security Considerations

1. **Always use HTTPS** in production
2. **Validate JWT tokens** on every request
3. **Implement proper CORS** policies
4. **Use API keys** for machine-to-machine communication
5. **Monitor for suspicious activity**
6. **Regularly rotate secrets**

## Support

For API support:
- **Documentation**: Check this reference and related docs
- **Issues**: Report problems via the project repository
- **Examples**: Review the `.http` files in the project
