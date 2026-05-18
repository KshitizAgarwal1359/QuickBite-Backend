# QuickBite Backend — Microservices Architecture

QuickBite is a full-stack food delivery platform built with a microservices architecture. The backend consists of 10 independent ASP.NET Core 8 services deployed on Render using Docker, all sharing a single PostgreSQL database.

## Live Deployment

| Service | URL |
|---|---|
| API Gateway | https://quickbite-api-gateway-q6bs.onrender.com |
| Auth Service | https://quickbite-auth-service.onrender.com |
| Restaurant Service | https://quickbite-restaurant-service.onrender.com |
| Menu Service | https://quickbite-menu-service.onrender.com |
| Cart Service | https://quickbite-cart-service.onrender.com |
| Order Service | https://quickbite-order-service.onrender.com |
| Payment Service | https://quickbite-payment-service.onrender.com |
| Delivery Service | https://quickbite-delivery-service.onrender.com |
| Review Service | https://quickbite-review-service.onrender.com |
| Notification Service | https://quickbite-notification-service.onrender.com |
| Frontend | https://quick-bite-frontend-six.vercel.app |

---

## Architecture Overview

```
Angular Frontend (Vercel)
        │
        ▼
Ocelot API Gateway (Render)
        │
        ├── /api/v1/auth          → Auth Service         (JWT, BCrypt)
        ├── /api/v1/restaurants   → Restaurant Service   (CRUD, approval)
        ├── /api/v1/menu          → Menu Service         (categories, items)
        ├── /api/v1/cart          → Cart Service         (cart, promo codes)
        ├── /api/v1/orders        → Order Service        (order lifecycle)
        ├── /api/v1/payments      → Payment Service      (Razorpay, wallet)
        ├── /api/v1/wallet        → Payment Service      (wallet, statements)
        ├── /api/v1/deliveries    → Delivery Service     (agents, SignalR GPS)
        ├── /api/v1/reviews       → Review Service       (food + delivery rating)
        └── /api/v1/notifications → Notification Service (SignalR, email)
                │
                ▼
        Shared PostgreSQL DB (Render)
        (Each service uses its own migration history table)
```

---

## Services

### API Gateway
- Built with Ocelot library on ASP.NET Core 8
- Routes all client requests to the appropriate microservice
- Handles CORS for the frontend
- Loads `ocelot.json` locally and `ocelot.Production.json` on Render

### Auth Service
- User registration, login, profile management, password change
- JWT token generation and validation (HMAC-SHA256, 24h expiry)
- Roles: `CUSTOMER`, `RESTAURANT_OWNER`, `AGENT`, `ADMIN`
- BCrypt password hashing
- Soft delete (account deactivation)
- Internal endpoint for delivery-service agent cross-checks

### Restaurant Service
- Restaurant CRUD operations
- Admin approval workflow (`IsApproved` flag)
- Owner can manage their own restaurants
- Stores location (lat/lng), cuisine type, delivery radius

### Menu Service
- Menu categories and menu items per restaurant
- Item availability toggle
- Supports IsVeg flag, calorie count, tags, discounted pricing

### Cart Service
- One cart per customer (overwritten when restaurant changes)
- Promo code validation and discount calculation
- Seeded with default promo codes: `WELCOME50` and `FLAT20`

### Order Service
- Places orders from cart
- Tracks order status: `PLACED → CONFIRMED → PREPARING → OUT_FOR_DELIVERY → DELIVERED`
- Calls payment-service for refunds on cancellation

### Payment Service
- Razorpay payment gateway integration
- Wallet system with balance and statement history
- Payment modes: CARD, UPI, WALLET, COD
- Status tracking: `PENDING → PAID → REFUNDED → FAILED`

### Delivery Service
- Delivery agent registration and verification
- Real-time GPS location updates via SignalR WebSocket (`/hub/location`)
- Agent online/offline toggle
- Internal endpoint to force agent offline when their account is deactivated

### Review Service
- Post-delivery food and delivery rating (1–5 stars)
- One review per order (verified purchase only)
- Calls restaurant-service to verify restaurant exists
- Calls delivery-service to verify agent exists

### Notification Service
- In-app notifications via SignalR WebSocket (`/hub/notification`)
- Email notifications via MailKit (SMTP)
- Stores notification history per user

---

## Tech Stack

| Technology | Purpose |
|---|---|
| ASP.NET Core 8 | Web API framework for all services |
| Entity Framework Core 8 | ORM for database access |
| Npgsql | PostgreSQL driver for EF Core |
| Ocelot | API Gateway routing library |
| BCrypt.Net | Password hashing |
| System.IdentityModel.Tokens.Jwt | JWT generation and validation |
| SignalR | Real-time WebSocket communication |
| Serilog | Structured logging (console + file) |
| MailKit | Email sending |
| Docker | Containerisation for Render deployment |
| PostgreSQL | Shared production database |
| SQL Server | Local development database |

---

## Local Development Setup

### Prerequisites
- .NET 8 SDK
- SQL Server (LocalDB or Express)
- Docker Desktop (optional, for container testing)

### Run a single service locally

```bash
cd auth-service
dotnet run
```

Each service runs on its own port defined in `launchSettings.json`:

| Service | Local Port |
|---|---|
| API Gateway | 5000 |
| Auth | 5093 |
| Restaurant | 5228 |
| Menu | 5044 |
| Cart | 5166 |
| Order | 5112 |
| Payment | 5236 |
| Delivery | 5272 |
| Review | 5400 |
| Notification | 5500 |

### Database Migrations
Each service runs `db.Database.Migrate()` on startup automatically. The database and tables are created if they do not exist.

To manually regenerate migrations:
```bash
cd auth-service
dotnet ef migrations add MigrationName
```

---

## Environment Variables (Render Production)

Each service requires these environment variables on Render:

```
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=Host=...;Port=5432;Database=...;Username=...;Password=...;
Jwt__SecretKey=<your-secret>
Jwt__Issuer=QuickBite.Auth
Jwt__Audience=QuickBite.Platform
Jwt__ExpiryInHours=24
Cors__AllowedOrigins__0=https://quick-bite-frontend-six.vercel.app
InternalSecrets__ServiceKey=<your-internal-secret>
```

---

## Docker

Each service has its own `Dockerfile` using multi-stage build:
- Stage 1 (`build`): Restores and builds the project
- Stage 2 (`publish`): Publishes the release build
- Stage 3 (`final`): Copies the published output into a slim ASP.NET runtime image

The Docker build context is the individual service folder. The `.dockerignore` excludes `bin/`, `obj/`, and local config files.

---

## Project Structure

```
QuickBite-Backend/
├── api-gateway/
├── auth-service/
│   ├── Controllers/
│   ├── Services/
│   ├── Repository/
│   ├── Interfaces/
│   ├── Entities/
│   ├── DTOs/
│   ├── Data/
│   ├── Configurations/
│   ├── Middlewares/
│   ├── Migrations/
│   ├── Program.cs
│   └── appsettings.json
├── cart-service/
├── delivery-service/
├── menu-service/
├── notification-service/
├── order-service/
├── payment-service/
├── restaurant-service/
└── review-service/
```

Each service follows the same layered architecture:
- **Controller** → receives HTTP request, calls service
- **Service** → business logic, calls repository
- **Repository** → EF Core database operations
- **Middleware** → global exception handling
- **DTOs** → request/response contracts (never expose entities directly)

---

## Security

- JWT Bearer authentication (HMAC-SHA256)
- Role-based authorization (`CUSTOMER`, `RESTAURANT_OWNER`, `AGENT`, `ADMIN`)
- BCrypt password hashing with auto-generated salt
- CORS restricted to frontend origin only
- Internal service-to-service calls protected by shared secret header (`X-Internal-Secret`)
- Soft delete for user accounts (data integrity preserved)
- `ClockSkew = TimeSpan.Zero` for strict token expiry
