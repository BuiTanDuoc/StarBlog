# Clinic Management SaaS

A multi-tenant clinic management platform built with ASP.NET Core (targeting .NET 10) and
Clean Architecture. Each subscribing clinic (tenant) sees only its own data, enforced at the
database query level, not just in application code.

## Architecture

```
ClinicManagement.Domain          Entities, enums, Result pattern, domain exceptions. No dependencies.
ClinicManagement.Application     CQRS (MediatR), DTOs, validators, business rules. Depends on Domain.
ClinicManagement.Persistence     EF Core, repositories, migrations, seed data. Depends on Application.
ClinicManagement.Infrastructure  JWT, authorization, Hangfire jobs, export/report generation.
ClinicManagement.API             Controllers, middleware, composition root (Program.cs).
ClinicManagement.Shared          Cross-cutting constants (Roles, Permissions) and PaginatedList<T>.
ClinicManagement.Web             Reserved for a first-party frontend — not built out in this pass.
```

Dependencies flow one direction only: `API → Infrastructure → Persistence → Application → Domain`.
`Shared` has no project dependencies and is referenced by everything.

## Multi-tenancy

Every tenant-owned entity implements `IMustHaveTenant`. `ApplicationDbContext` walks the EF Core
model at startup and attaches a global query filter to every such entity — `TenantId == current
tenant` (from the JWT's `tenant_id` claim), automatically combined with a soft-delete filter where
applicable. No repository method or handler ever writes `WHERE TenantId = ...` by hand, which means
there's no place a developer can forget to scope a query. `SuperAdmin` bypasses the filter
platform-wide; the two places that deliberately query across all tenants
(`ExpiredSubscriptionsJob`, the subscription report) do so explicitly via `IgnoreQueryFilters()`
with a comment explaining why.

## Getting started

### Prerequisites
- .NET 10 SDK
- SQL Server (LocalDB is fine for local dev) or Docker

### Run locally
```bash
dotnet restore

# Create the database
dotnet ef migrations add InitialCreate -p src/ClinicManagement.Persistence -s src/ClinicManagement.API
dotnet ef database update -p src/ClinicManagement.Persistence -s src/ClinicManagement.API

dotnet run --project src/ClinicManagement.API
```
The API seeds roles, the 4 default plans, a SuperAdmin account, and a demo tenant on first run
(see `ClinicManagement.Persistence/Seed/DatabaseSeeder.cs`). Swagger UI is at `/swagger` in
Development.

**Seeded accounts** (dev password for both: `ChangeMe!123` — change this before anything touches
a real network):
| Role | Email |
|---|---|
| SuperAdmin | `superadmin@clinicsaas.dev` |
| ClinicOwner (Demo Clinic) | `owner@demo-clinic.dev` |

### Run the frontend
```bash
cd frontend
cp .env.example .env   # points at the API; defaults to https://localhost:7168
npm install
npm run dev
```
Opens at `http://localhost:5173`. Log in with the seeded ClinicOwner account above and you'll land
on `/dashboard/owner`. `appsettings.json`'s CORS config already allows `localhost:5173`.

### Run with Docker
```bash
cp .env.example .env   # fill in SA_PASSWORD and JWT_SECRET
docker compose up --build
```
This starts the API, SQL Server, and Redis. Migrations run automatically on API startup.

## Key design decisions worth knowing about

- **Result pattern over exceptions for expected failures.** Command handlers return
  `Result`/`Result<T>` for business-rule failures (validation, not-found, limit-exceeded).
  Exceptions are reserved for genuinely exceptional cases (`AppointmentConflictException`,
  `SubscriptionLimitExceededException`) and are mapped to HTTP status codes in one place —
  `GlobalExceptionMiddleware` — so no controller has its own try/catch.
- **Permission claims are baked into the JWT at login**, not looked up per-request. `RolePermissions`
  (Shared) maps each of the 6 roles to a set of fine-grained `Permissions.*` strings; `TokenService`
  unions them across a user's roles at login time. This means `PermissionAuthorizationHandler` does
  zero database I/O — but also means a permission change for a role doesn't take effect for an
  already-logged-in user until their token refreshes.
- **Two layers of subscription enforcement**, deliberately not merged into one: `SubscriptionGuardMiddleware`
  (API) is a cheap "has this tenant's subscription expired at all" check on every request.
  `ISubscriptionLimitService` (Application) is the more expensive "is this tenant over its specific
  plan's doctor/patient/appointment quota" check, run only inside the handlers that create those
  resources.
- **Notifications flow through MediatR domain events, not directly from command handlers.**
  `CreateAppointmentCommandHandler` publishes `AppointmentCreatedEvent`; a separate handler persists
  a `Notification` row and pushes it over SignalR via `INotificationDispatcher`. The interface lives
  in Application; the concrete SignalR implementation lives in API (only that layer knows about the
  Hub type) — which is also why `ExpiredSubscriptionsJob`, sitting in Infrastructure, can push live
  notifications without Infrastructure ever referencing API.
- **Reports export generically.** Every report handler returns a flat row DTO; `IExportService`
  builds the Excel/PDF from that DTO's public properties via reflection. Adding a 6th report means
  writing one query, not new export code.

## Known gaps / hardening still needed before production

These are flagged in code comments at the relevant spot, collected here for visibility:

- **Hangfire dashboard (`/hangfire`) has no authorization filter.** Lock it to SuperAdmin
  (`DashboardOptions.Authorization`) before deploying anywhere but localhost.
- **Refresh token reuse detection is minimal.** `RefreshTokenCommandHandler` rejects a reused
  revoked token but doesn't yet revoke the whole token family, which is the fuller mitigation for
  token theft.
- **No email verification pipeline.** `RegisterTenantCommand` sets `EmailConfirmed = true`
  immediately; wire up a real verification flow before this is customer-facing.
- **Subscription upgrade assumes payment already succeeded.** `UpgradeSubscriptionCommand` takes a
  `PaymentReference` and books it — actual payment capture (Stripe/PayPal/etc.) needs to wrap this
  command, not be replaced by it.
- **JWT secret in `appsettings.json` is a placeholder** (`REPLACE_WITH_...`) in every environment
  except `appsettings.Development.json`, which has a working (but public, in this repo) dev-only key.
  Use user-secrets or environment variables for anything beyond local dev.
- **`ClinicManagement.Web`** — the original stub .NET project is unused; the actual frontend lives
  in `/frontend` as a separate React SPA (see below), not inside the .NET solution.
- **Frontend covers login + the 3 role dashboards only.** Patients, Appointments, Billing, Reports,
  etc. all have working API endpoints (see the "what's implemented" list below) but no screens yet
  — the sidebar nav in `AppShell.tsx` has placeholder links for them. Building out one of those
  screens follows the same pattern as the dashboards: `useApiData` (or a mutation via `api.post`/
  `api.put`) + `AppShell` + whatever presentational components the screen needs.

## What's implemented vs. scaffolded

Fully implemented with CQRS commands/queries, validators, and controllers: **Auth** (login, tenant
signup, refresh/revoke, staff invite, password change), **Patients**, **Appointments** (including
conflict detection, working-hours validation, and availability lookup), **Plans**, **Subscriptions**
(including usage-vs-limit stats), **Reports** (5 report types × Excel/PDF export).

Domain entities exist with full EF Core configuration but don't yet have their own CQRS
slices — extending any of these follows the exact same pattern as Patients/Appointments:
**Branches, Departments, Doctors, Employees, MedicalRecords, Prescriptions, Invoices, Payments,
Expenses, FileAttachments, AuditLogs** (the last is populated automatically by the SaveChanges
interceptor and mainly needs a read-only query surface).
