# FeatureFlow — SaaS Feature Feedback & Roadmap Platform

A portfolio-grade, full-stack SaaS MVP for collecting product feedback, prioritizing feature requests, and publishing roadmap progress.

## Tech Stack

- **Backend:** .NET 8, ASP.NET Core Web API, EF Core, PostgreSQL, JWT + refresh tokens, FluentValidation, Serilog, API versioning, Swagger.
- **Frontend:** Vue 3 (Composition API), TypeScript, Pinia, Vue Router, Axios interceptors.
- **DevOps:** Dockerfiles, docker-compose orchestration, GitHub Actions CI/CD pipeline.
- **Testing:** xUnit unit, integration, and end-to-end backend test projects.

## Architecture Overview

### Backend Clean Architecture

```text
backend/src
├── Roadmap.Domain          # Entities + core domain enums
├── Roadmap.Application     # DTOs, interfaces, validators, mapping, business services
├── Roadmap.Infrastructure  # EF Core DbContext, repository impl, token + hash services
└── Roadmap.API             # Controllers, middleware, auth, rate limiting, Swagger
```

Key decisions:
- **Entity isolation:** EF entities stay inside Domain/Infrastructure, while API exchanges DTOs from Application.
- **Service orchestration:** `AuthService` and `FeatureService` encapsulate business logic and async workflows.
- **Cross-cutting boundaries:** middleware handles exceptions globally; structured logging and rate limiting are enforced at API boundary.
- **Security by default:** short-lived access tokens, refresh token rotation, role-based admin routes, strong password hashing.

### Frontend Feature-First Structure

```text
frontend/src
├── app/                    # App shell/layout
├── components/             # Reusable primitives (toasts, skeletons)
├── features/
│   ├── auth/
│   ├── roadmap/
│   └── admin/
├── router/                 # Route setup + guards
├── services/               # API client + interceptors
├── stores/                 # Pinia stores
└── styles/                 # SaaS visual system
```

Key decisions:
- **Business logic in stores/composables** (not in presentation components).
- **Optimistic updates** for voting improve perceived performance.
- **Resilient auth UX** with automatic token refresh in Axios interceptor.
- **Reusable UI system** with consistent cards, states, badges, and motion tokens.

## Feature Coverage

### User capabilities
- Register/login + refresh token flow.
- Create feature requests.
- Search/filter/paginate feature feed.
- Upvote with optimistic UI.
- Comment on feature requests.
- View roadmap status (Planned / InProgress / Released).

### Admin capabilities
- Update feature status.
- Moderate comments.
- View analytics KPIs.
- Role-protected routes on API and frontend.

### UX capabilities
- Dark/light mode toggle.
- Loading skeletons.
- Empty states.
- Error toasts.
- Responsive layout.
- Motion/transitions for polished feel.

## Setup Instructions

### Prerequisites
- Docker + Docker Compose
- (Optional local) .NET 8 SDK and Node 22+

### Run with Docker

```bash
docker compose up --build
```

- API: `http://localhost:8080/swagger`
- Web: `http://localhost:5173`

### Local development (without Docker)

Backend:
```bash
cd backend
dotnet restore src/Roadmap.API/Roadmap.API.csproj
dotnet run --project src/Roadmap.API/Roadmap.API.csproj
```

Frontend:
```bash
cd frontend
npm install
npm run dev
```


## CI/CD

A GitHub Actions workflow is available at `.github/workflows/ci.yml`:

- **CI (pull requests + pushes):** Runs backend unit, integration, and end-to-end tests, then builds the frontend.
- **CD (main branch pushes):** Builds and publishes API and web Docker images to GHCR using the commit SHA tag.

## Deployment Notes

- Build production artifacts with provided Dockerfiles.
- Prefer secret injection via environment variables (`Jwt__Secret`, DB connection string).
- Add reverse proxy + HTTPS termination in production (Nginx/Traefik + managed certs).
- Use managed PostgreSQL with automated backups.

## Testing

```bash
# backend unit tests
dotnet test backend/tests/Roadmap.UnitTests/Roadmap.UnitTests.csproj

# backend integration tests
dotnet test backend/tests/Roadmap.IntegrationTests/Roadmap.IntegrationTests.csproj

# backend end-to-end tests
dotnet test backend/tests/Roadmap.E2ETests/Roadmap.E2ETests.csproj

# frontend type/build check
cd frontend && npm run build
```

## AI Disclosure

### Primarily AI-generated
- Initial layered backend scaffolding (Domain/Application/Infrastructure/API).
- Frontend feature module scaffolding and UI system baseline.
- Docker baseline and initial README composition.

### Manually refactored/redesigned
- Authentication architecture boundaries and refresh token rotation flow.
- Error-handling strategy (global exception middleware + toast UX).
- Feature prioritization UX (optimistic voting, skeleton/empty-state treatment).
- Folder conventions and naming for long-term maintainability.

### Human-led architectural decisions
- Choosing Clean Architecture split and limiting EF leakage.
- Selecting role boundaries (Admin/User) and enforcing both API and route guards.
- Defining SaaS aesthetic direction and component reusability constraints.

### Improvements beyond baseline scaffolding
- Added rate limiting policy.
- Added API versioning and Swagger version-ready setup.
- Added structured logging with Serilog.
- Added explicit folder-level architecture documentation.
