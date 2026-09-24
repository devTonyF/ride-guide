# Ride Guide — Design Document

> Capstone project for SE Transition Track B (Backend). A REST API for discovering and rating motorcycle routes.

---

## 1. Project Overview

**Concept:** A backend API for tracking and rating motorcycle routes. Users can browse routes by region, see road character and pace details, and submit ratings and reviews. Authenticated users can also submit new routes.

**Goals:**
- Demonstrate full Track B backend competency (ASP.NET Core, EF Core, JWT, integration testing, CI)
- Build incrementally alongside coursework so each topic is applied while fresh
- Produce a portfolio-quality GitHub repo

**Capstone Requirements (Track B):**
- ASP.NET Core REST API with validation, error handling, and structured logging
- EF Core data layer with at least one migration
- JWT-protected endpoints
- xUnit unit tests with Moq and `WebApplicationFactory` integration tests
- Playwright API tests committed and passing in CI
- Clean PR reviewed and merged

**Out of scope (for now):**
- Frontend UI — Swagger serves as the API interface; Angular frontend is a stretch goal
- Company infrastructure — GitHub Actions for CI; Azure free tier for hosting if deployed

---

## 2. Technology Stack

| Concern | Choice | Notes |
|---|---|---|
| Framework | ASP.NET Core 8 (LTS) | Matches transition plan target |
| Language | C# 12 | |
| ORM | EF Core 8 | Code First, migrations |
| Database | PostgreSQL | Free everywhere; easy to host for live demo; EF Core abstracts differences |
| Authentication | ASP.NET Core Identity + JWT | Industry standard; handles password hashing, lockout, claims out of the box |
| Validation | FluentValidation | `FluentValidation.AspNetCore` NuGet |
| Logging | Serilog | `Serilog.AspNetCore` NuGet; `ILogger<T>` used throughout |
| Error Handling | ProblemDetails + Global Exception Middleware | RFC 7807; built into ASP.NET Core 7+ |
| API Documentation | Swagger / OpenAPI | Auto-generated; serves as the UI |
| Testing | xUnit + Moq + `WebApplicationFactory` | Unit + integration tests |
| API Testing | Playwright (`APIRequestContext`) | Committed and passing in CI |
| CI | GitHub Actions | Personal repo; no ADO dependency |
| IDE | Visual Studio 2022 | |

---

## 3. Solution Structure

Clean Architecture — five projects in one solution:

```
RideGuide/
├── RideGuide.Api/              # ASP.NET Core project — entry point
│   ├── Controllers/            # AuthController, RoutesController, etc.
│   ├── Middleware/             # Global exception handler
│   ├── DTOs/                   # Request/response models
│   ├── Validators/             # FluentValidation validators
│   └── Program.cs              # Startup, DI registration
│
├── RideGuide.Domain/           # Class library — domain layer
│   ├── Entities/               # User, Route, Region, Rating
│   ├── Enums/                  # RoadCharacter, Pace
│   └── Interfaces/             # IRouteRepository, IRatingRepository, etc.
│
├── RideGuide.Infrastructure/   # Class library — data layer
│   ├── Data/                   # AppDbContext, Global Query Filters
│   ├── Repositories/           # Repository implementations
│   └── Migrations/             # EF Core migrations
│
├── RideGuide.Tests/            # xUnit — unit and integration tests
│   ├── Unit/                   # Unit tests (Moq)
│   └── Integration/            # WebApplicationFactory integration tests
│
└── RideGuide.Tests.Playwright/ # Playwright — API tests
```

**Why Clean Architecture:**
- Transition plan (Month 5–6) calls it out explicitly
- Enforces separation of concerns — domain logic has no dependency on EF Core or ASP.NET Core
- Repository interfaces live in `Domain`; implementations live in `Infrastructure` — testable by design

---

## 4. Data Model

### Entities

#### `User`
| Column | Type | Notes |
|---|---|---|
| Id | int | PK |
| Email | string | Required, unique |
| PasswordHash | string | Required |
| DisplayName | string | Required |
| AvatarUrl | string | Optional — stores URL/path only; image storage handled externally |
| CreatedAt | datetime | |

**Decisions:**
- No phone field — no feature requires it; add later if needed
- Avatar stored as `AvatarUrl` (nullable string) — same pattern as Route images; storage backend is external and swappable
- On delete: hard delete with content anonymization (replace PII with placeholder values) — soft delete avoided for privacy reasons

---

#### `Region`
| Column | Type | Notes |
|---|---|---|
| Id | int | PK |
| Name | string | Required — e.g., "Central Florida" |
| State | string | Required — e.g., "FL" |

**Decisions:**
- Scope is nationwide, not Florida-only — allows the app to grow naturally
- Stored as a lookup table (not a string on Route) to enforce consistency and enable reliable grouping by state

---

#### `Route`
| Column | Type | Notes |
|---|---|---|
| Id | int | PK |
| Name | string | Required |
| Description | string | Optional |
| RegionId | int | FK → Region |
| StartPoint | string | Required |
| EndPoint | string | Required |
| DistanceMiles | decimal | Optional |
| RoadCharacter | enum | Required — `City`, `Rural`, `Mixed` |
| Pace | enum | Required — `Leisurely`, `Moderate`, `Spirited` |
| ImageUrl | string | Optional — stores URL/path only; image storage handled externally |
| IsDeleted | bool | Default false |
| DeletedAt | datetime | Optional |
| CreatedByUserId | int | FK → User |
| CreatedAt | datetime | |
| UpdatedAt | datetime | |

**Decisions:**
- `LastUpdatedByUserId` omitted — redundant if history tracking is added later; unnecessary without it
- Images stored as `ImageUrl` (nullable string) — keeps schema stable regardless of storage backend (local files today, Azure Blob later)
- Soft delete applied — routes with existing ratings shouldn't vanish cleanly with a hard delete
- EF Core Global Query Filters will enforce soft delete transparently across all queries

---

#### `Rating`
| Column | Type | Notes |
|---|---|---|
| Id | int | PK |
| RouteId | int | FK → Route |
| UserId | int | FK → User |
| Score | int | Required, 1–5 |
| Review | string | Optional |
| IsDeleted | bool | Default false |
| DeletedAt | datetime | Optional |
| CreatedAt | datetime | |
| UpdatedAt | datetime | |

**Constraints:** Unique on `(RouteId, UserId)` — one rating per user per route

**Decisions:**
- Users can update their rating — the unique constraint means it's always one row per user per route; a change is a standard `PUT`
- Soft delete applied — cascades from Route soft delete; preserves data integrity

---

### Relationships

```
Region  ──< Route >── Ratings ──> User
                  └─────────────> User (creator)
```

- One Region → many Routes
- One Route → many Ratings
- One User → many Ratings submitted
- One User → many Routes created
- Average rating is calculated from the Ratings table at query time (not stored)

---

## 3. API Endpoints

### Auth
| Method | Endpoint | Auth | Description |
|---|---|---|---|
| POST | `/api/auth/register` | None | Create account |
| POST | `/api/auth/login` | None | Returns JWT |
| PUT | `/api/auth/change-password` | Required | Change password (requires current password) |

**Decisions:**
- Auth endpoints are split from user endpoints — password and identity management are security concerns, not profile data
- `change-password` lives under `/api/auth` (not `/api/users/{id}/password`) — no ID in the URL needed since user identity comes from the JWT; avoids URL-based targeting of another user's password

---

### Regions
| Method | Endpoint | Auth | Description |
|---|---|---|---|
| GET | `/api/regions` | None | List all regions |
| GET | `/api/regions/{id}` | None | Get single region |
| POST | `/api/regions` | Required | Create region |

---

### Routes
| Method | Endpoint | Auth | Description |
|---|---|---|---|
| GET | `/api/routes` | None | List routes — paginated, filterable |
| GET | `/api/routes/{id}` | None | Get route detail + average rating |
| POST | `/api/routes` | Required | Submit a new route |
| PUT | `/api/routes/{id}` | Required | Update route (creator only) |
| DELETE | `/api/routes/{id}` | Required | Soft delete route (creator only) |

**Pagination** on `GET /api/routes`:
- Query params: `?page=1&pageSize=20`
- Response includes page of results + metadata (total count, total pages)
- Designed in from day one — Month 9–10 of the transition plan calls this out as a specific practice task

**Filtering** on `GET /api/routes`:
- Query params: `?regionId=1&pace=Spirited&roadCharacter=Rural`
- Combinable with pagination

---

### Ratings
| Method | Endpoint | Auth | Description |
|---|---|---|---|
| GET | `/api/routes/{id}/ratings` | None | List ratings for a route |
| POST | `/api/routes/{id}/ratings` | Required | Submit a rating |
| PUT | `/api/routes/{id}/ratings/{ratingId}` | Required | Update your rating |
| DELETE | `/api/routes/{id}/ratings/{ratingId}` | Required | Soft delete your rating |

**Decision:** Ratings are nested under routes — they don't exist independently of a route, so flat `/api/ratings` would be misleading.

---

### Users
| Method | Endpoint | Auth | Description |
|---|---|---|---|
| GET | `/api/users/{id}` | None | Public profile (display name, avatar, routes submitted) |
| PUT | `/api/users/{id}` | Required | Update display name, email, avatar (self only) |
| DELETE | `/api/users/{id}` | Required | Delete own account |

---

## 4. Design Patterns

| Pattern | Fit | Application |
|---|---|---|
| **Repository** | Essential | `IRouteRepository`, `IRatingRepository`, etc. — abstracts EF Core DbContext from service layer; called out explicitly in Month 5–6 of transition plan |
| **Builder** | Natural (tests) | `RouteBuilder`, `RatingBuilder` — fluent test data construction; readable tests and a real production pattern |
| **Strategy** | Reasonable | Route filtering/sorting logic — each sort or filter approach is a strategy; keeps controllers clean as complexity grows |
| **Factory (Method)** | Weak fit | Nothing in this domain demands it; don't force it |
| **Observer** | Weak fit | Shines in event-driven/real-time scenarios; revisit only if SignalR is added as a stretch goal |

---

## 5. Key Design Decisions

| # | Decision | Outcome |
|---|---|---|
| 1 | RoadCharacter and Pace as enums | Fixed values warrant enums over lookup tables; EF Core handles cleanly |
| 2 | Tag system | Deferred — stretch goal if core build goes well |
| 3 | Region as lookup table | Enforces consistency; enables reliable state-level grouping |
| 4 | Image storage | `ImageUrl` string on Route and `AvatarUrl` on User; storage backend is external and swappable |
| 5 | Soft deletes | Applied to Routes and Ratings; Users are hard-deleted with PII anonymization |
| 6 | History/audit log | Deferred — stretch goal; `UpdatedAt` satisfies capstone requirements |
| 7 | Frontend | Out of scope for capstone; Swagger is the interface; Angular is a stretch goal |
| 8 | Infrastructure | Personal GitHub repo; GitHub Actions for CI; no ADO or company servers required |
| 9 | Auth vs user endpoints | Split into `AuthController` and `UsersController` — different middleware concerns (rate limiting, security) |
| 10 | Pagination | Built in from day one on `GET /api/routes`; page/pageSize query params + metadata in response |
| 11 | Error handling | `ProblemDetails` (RFC 7807) + global exception middleware — consistent error shape, no try/catch in controllers |
| 12 | Validation | FluentValidation — testable validator classes, clean DTOs, natural cross-field rules |
| 13 | Structured logging | Serilog via `ILogger<T>` — structured JSON events, rich sink ecosystem, set up from day one |
| 14 | Database | PostgreSQL — free everywhere, no licensing concerns, easy to host for live demo |
| 15 | Authentication library | ASP.NET Core Identity + JWT — industry standard; handles security primitives correctly out of the box |

---

## 6. Implementation Notes

### Error Handling — `ProblemDetails` + Global Exception Middleware ✓
RFC 7807-compliant structured error responses, built into ASP.NET Core 7+ via `AddProblemDetails()`. A single global exception middleware catches all unhandled exceptions and returns a consistent error shape — no try/catch scattered across controllers. API consumers always know what an error response looks like.

### Validation — FluentValidation ✓
Separate validator classes with a fluent API. Keeps DTOs clean, validators are independently testable, and cross-field rules (e.g., EndPoint must differ from StartPoint) are natural. Explicitly called out in Month 3–4 of the transition plan. NuGet: `FluentValidation.AspNetCore`.

### Structured Logging — Serilog ✓
Logs events as structured data (JSON), not plain text strings. Plugs into ASP.NET Core's `ILogger<T>` abstraction as a provider — code throughout the app uses `ILogger<T>` and never calls Serilog directly. Rich sink ecosystem (console, files, Seq, Azure Application Insights). Set up from day one so logs are useful throughout the build. Explicitly called out in Month 9–10 of the transition plan. NuGet: `Serilog.AspNetCore`.

### EF Core Global Query Filters
Mechanism for enforcing soft delete transparently — every query automatically excludes `IsDeleted = true` records without manual filtering in every repository method.

---

## 7. Stretch Goals

- Tag system for routes (many-to-many `RouteTag`)
- Route history / audit log
- Angular frontend
- Azure Blob Storage for images
- Deployment to Azure free tier

---

*Document started: 2026-09-23*
