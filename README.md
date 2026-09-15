# Enterprise Employee Management Full Stack

Enterprise Employee Management System built with ASP.NET Core Web API,
Clean Architecture, Entity Framework Core, SQL Server, JWT Authentication,
Role-Based Authorization, and automated testing.

## 🚀 Features

- Employee Management
- Department Management
- Role Management
- Attendance Management
- Leave Management
- User Registration
- Login
- JWT Authentication
- Refresh Tokens
- Logout / Token Revocation
- Role-Based Authorization
- Validation
- Global Exception Handling
- Pagination
- Searching
- Filtering
- Sorting
- Rate Limiting
- CORS
- Health Checks
- Background Services
- SignalR
- Unit Testing
- Integration Testing

## 🏗️ Architecture

- Domain
- Application
- Infrastructure
- WebApi
- Tests

## 🛠️ Technologies

- C#
- .NET 10
- ASP.NET Core Web API
- Entity Framework Core
- SQL Server
- JWT
- xUnit
- Moq
- Swagger/OpenAPI

## 🧪 Testing

The project contains unit tests and integration tests.

## 🔐 Security

- JWT authentication
- Role-based authorization
- Password hashing
- Refresh-token management
- Rate limiting
- Security headers
- Input validation
- Parameterized database queries

## 📖 API Documentation

Swagger/OpenAPI is available when running the API locally.

## 🔮 Future Development

- React frontend
- Production deployment
- Automated CI/CD
## 💳 Stripe Billing & Local Environment Note

If you notice a console warning stating *"Stripe.js requires HTTPS"* while testing locally on `http://localhost:5173`:
* **It is safe to ignore.** This is standard behavior for Stripe on non-secure local URLs. 
* Because the app uses **Stripe Test Mode keys** (`pk_test_...`), test payments will process normally without blocking anything.
* When deployed to production (on Vercel/Render), SSL/TLS certificates are handled automatically.