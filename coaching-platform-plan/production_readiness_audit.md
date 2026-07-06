# 🔍 Production Readiness Audit — Coaching Platform

Full audit of the **Joker Nutrition Coaching Platform** (backend + frontend) for production deployment readiness.

---

## Verdict Summary

| Category | Status | Severity |
|---|---|---|
| 🔴 **Security — Secrets in Source** | FAIL | **CRITICAL** |
| 🔴 **Security — No HSTS** | FAIL | **HIGH** |
| 🔴 **Configuration — Hardcoded localhost** | FAIL | **CRITICAL** |
| 🔴 **Configuration — No Production Config** | FAIL | **CRITICAL** |
| 🔴 **DevOps — No Dockerfile / CI/CD** | FAIL | **HIGH** |
| 🟡 **Code Quality — Dead Scaffold Files** | WARN | MEDIUM |
| 🟡 **Code Quality — Console Logs in Prod** | WARN | MEDIUM |
| 🟡 **Security — Error Messages Leak Details** | WARN | MEDIUM |
| 🟡 **Testing — Thin Coverage** | WARN | MEDIUM |
| 🟡 **Frontend — No .env.production** | WARN | HIGH |
| 🟡 **Token Cleanup — No Expired Token Purge** | WARN | MEDIUM |
| 🟢 **Auth — JWT + Refresh Token** | PASS | — |
| 🟢 **Auth — Rate Limiting** | PASS | — |
| 🟢 **Auth — Role-Based Access** | PASS | — |
| 🟢 **Architecture — Clean Layering** | PASS | — |
| 🟢 **Frontend — Lazy Loading + PWA** | PASS | — |
| 🟢 **Backend — Health Check Endpoint** | PASS | — |
| 🟢 **Backend — Serilog Structured Logging** | PASS | — |
| 🟢 **Backend — Global Exception Filter** | PASS | — |
| 🟢 **Frontend — Token Refresh + Queue** | PASS | — |
| 🟢 **Frontend — Error Boundary** | PASS | — |

---

## 🔴 CRITICAL Issues (Must Fix Before Production)

### 1. Secrets Committed to Source Control

> [!CAUTION]
> **Real credentials are hardcoded in [appsettings.json](file:///c:/Users/ismail/Desktop/Coaching-Platform/coaching-platform-backend/JokerNutrition.Api/appsettings.json) and tracked by Git.**

| Secret | File | Line |
|---|---|---|
| JWT Secret Key | `appsettings.json` | L18 |
| SMTP Password (Gmail App Password) | `appsettings.json` | L35 |
| SMTP Username (real email) | `appsettings.json` | L34 |
| SQL Server Connection String | `appsettings.json` | L3 |

**What to do:**
- Move ALL secrets to **environment variables** or **Azure Key Vault** / **User Secrets**
- Add `appsettings.json` sensitive values to `.gitignore` or use the `appsettings.Production.json` override pattern
- **Rotate the leaked SMTP app password immediately** — it's already in Git history
- Use `builder.Configuration.AddEnvironmentVariables()` for production values

---

### 2. No `appsettings.Production.json` Exists

> [!CAUTION]
> There is no production configuration file. The comment at [Program.cs:79](file:///c:/Users/ismail/Desktop/Coaching-Platform/coaching-platform-backend/JokerNutrition.Api/Program.cs#L79) says *"lock in Production via appsettings.Production.json"* — but that file does not exist.

All production URLs, connection strings, CORS origins, JWT issuer/audience, and SMTP settings are **hardcoded to localhost values** in the only `appsettings.json`:

| Setting | Current Value | Required for Prod |
|---|---|---|
| `ConnectionStrings.DefaultConnection` | `(localdb)\MSSQLLocalDB` | Real SQL Server/Azure SQL |
| `CorsAllowedOrigins` | `localhost:5173` | Production frontend domain |
| `JwtSettings.Issuer` | `https://localhost:7001` | Production API domain |
| `JwtSettings.Audience` | `https://localhost:7001` | Production API domain |
| `SmtpSettings.SignUpBaseUrl` | `http://localhost:5173/join-the-team` | Production frontend URL |
| `AppSettings.ResetPasswordPageUrl` | `http://localhost:5173/reset-password` | Production frontend URL |
| `BlobStorageSettings.ConnectionString` | `UseDevelopmentStorage=true` | Real Azure Blob connection |

**What to do:**
- Create `appsettings.Production.json` with all production values
- Do NOT commit it to Git — use deployment pipeline secrets instead

---

### 3. Hardcoded `localhost` in Backend Source Code

> [!WARNING]
> [BlobStorageService.cs:127](file:///c:/Users/ismail/Desktop/Coaching-Platform/coaching-platform-backend/JokerNutrition.Business/Services/BlobStorageService.cs#L127) has a hardcoded fallback URL:

```csharp
return $"http://localhost:7000/uploads/{uniqueFileName}";
```

In production, the local fallback should either be removed entirely (always require Azure Blob) or dynamically resolve the host from the request context.

---

### 4. No `.env.production` for Frontend

> [!WARNING]
> The frontend has only a single [.env](file:///c:/Users/ismail/Desktop/Coaching-Platform/coaching-platform-frontend/.env) pointing to `http://localhost:7000/api`. There is no `.env.production` file.

**What to do:**
- Create `.env.production` with `VITE_API_URL=https://your-production-api.com/api`
- Vite will automatically use it when running `vite build`

---

## 🟠 HIGH Severity Issues

### 5. No HSTS (HTTP Strict Transport Security)

[Program.cs](file:///c:/Users/ismail/Desktop/Coaching-Platform/coaching-platform-backend/JokerNutrition.Api/Program.cs) does NOT call `app.UseHsts()` in the production pipeline. This is a security requirement for production:

```csharp
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}
```

### 6. No Dockerfile or CI/CD Pipeline

There is **no Dockerfile**, **no `docker-compose.yml`**, **no GitHub Actions**, and **no Azure Pipelines** config. For production deployment you need at minimum:

- `Dockerfile` for the backend API
- Frontend build + static hosting config (e.g., Nginx, Azure Static Web Apps, Vercel)
- CI/CD pipeline for automated build, test, and deploy

### 7. Swagger Exposed Only in Dev — But No `UseExceptionHandler` for Prod

The Swagger UI is correctly gated behind `IsDevelopment()` ✅, but there's no `app.UseExceptionHandler("/error")` middleware for production. The `ApiExceptionFilter` on each controller handles it per-controller, but a global middleware catch-all is more reliable in production.

---

## 🟡 MEDIUM Severity Issues

### 8. Dead Scaffold / Boilerplate Files

These files are **Vite/dotnet template leftovers** that should be deleted:

| File | Why |
|---|---|
| [counter.ts](file:///c:/Users/ismail/Desktop/Coaching-Platform/coaching-platform-frontend/src/counter.ts) | Vite template boilerplate |
| [main.ts](file:///c:/Users/ismail/Desktop/Coaching-Platform/coaching-platform-frontend/src/main.ts) | Old Vite vanilla entry (actual entry is `main.tsx`) |
| [style.css](file:///c:/Users/ismail/Desktop/Coaching-Platform/coaching-platform-frontend/src/style.css) | Vite template CSS (actual styles in `index.scss`) |
| [Class1.cs](file:///c:/Users/ismail/Desktop/Coaching-Platform/coaching-platform-backend/JokerNutrition.Business/Class1.cs) | Empty .NET scaffold class |
| [Class1.cs](file:///c:/Users/ismail/Desktop/Coaching-Platform/coaching-platform-backend/JokerNutrition.Data/Class1.cs) | Empty .NET scaffold class |

### 9. Console Statements in Frontend Production Code

Found **12 `console.log/warn/error` statements** in production code, primarily in:

- [NotificationContext.tsx](file:///c:/Users/ismail/Desktop/Coaching-Platform/coaching-platform-frontend/src/contexts/NotificationContext.tsx) — 9 statements
- [Notifications.tsx](file:///c:/Users/ismail/Desktop/Coaching-Platform/coaching-platform-frontend/src/pages/shared/Notifications/Notifications.tsx) — 1 statement
- [ErrorBoundary.tsx](file:///c:/Users/ismail/Desktop/Coaching-Platform/coaching-platform-frontend/src/components/ErrorBoundary/ErrorBoundary.tsx) — 1 statement (acceptable)

**What to do:** Replace debug `console.log` with conditional logging or remove them. The `console.error` in ErrorBoundary is acceptable.

### 10. Error Messages Leak Internal Details

In [ApiExceptionFilter.cs](file:///c:/Users/ismail/Desktop/Coaching-Platform/coaching-platform-backend/JokerNutrition.Api/Filters/ApiExceptionFilter.cs), `exception.Message` is returned directly to the client for `ArgumentException`, `KeyNotFoundException`, etc. While these are generally safe, in production you should:

- Never return stack traces
- Consider sanitizing messages for `InvalidOperationException` (line 26) which could leak EF/Identity internals

### 11. No Expired Token Cleanup

[AuthService.cs](file:///c:/Users/ismail/Desktop/Coaching-Platform/coaching-platform-backend/JokerNutrition.Business/Services/AuthService.cs) creates `PasswordResetToken` records for refresh tokens and password resets but **never cleans up expired/used tokens**. Over time, the `PasswordResetTokens` table will grow unbounded.

**What to do:** Add a background hosted service (`IHostedService`) that periodically deletes expired tokens, or implement a cleanup during token rotation.

### 12. `.env` File NOT in `.gitignore`

The frontend [.gitignore](file:///c:/Users/ismail/Desktop/Coaching-Platform/coaching-platform-frontend/.gitignore) does NOT exclude `.env` files. While the current `.env` only contains `localhost`, any future production secrets would be committed.

**What to do:** Add `.env` and `.env.production` to `.gitignore`

---

## 🟢 What's Already Good (Production-Ready)

### ✅ Authentication & Authorization
- JWT Bearer authentication with proper validation (issuer, audience, lifetime, signing key) ✅
- Refresh token rotation with single-use enforcement ✅  
- Password hashing via ASP.NET Identity ✅
- Role-based authorization (`[Authorize(Roles = "...")]`) on every controller ✅
- Frontend `ProtectedRoute` + `RoleGuard` components ✅
- Frontend token refresh interceptor with failed-request queue ✅

### ✅ Rate Limiting
- `AspNetCoreRateLimit` configured for login (10/min) and forgot-password (3/hr) ✅
- Frontend handles 429 responses with user-friendly messages ✅

### ✅ Architecture
- Clean 3-layer separation: API → Business → Data ✅
- Autofac DI with module registration ✅
- Repository pattern with base class ✅
- Service pattern with base class ✅

### ✅ API Quality
- Global exception filter with consistent error responses ✅
- Health check endpoint with database ping ✅
- Swagger documentation with XML comments ✅
- UTC DateTime converters for consistent timezone handling ✅
- Enum string serialization ✅

### ✅ Frontend Quality
- React 19 with TypeScript strict mode ✅
- TanStack Query for data fetching ✅
- Code splitting with `React.lazy()` on all page routes ✅
- PWA configuration with service worker ✅
- Error boundary wrapping routes ✅
- 404 page handling ✅
- Ant Design component library ✅

### ✅ Logging & Observability
- Serilog with structured logging ✅
- File + Console sinks with 7-day retention ✅
- Audit logging for auth events ✅

---

## 📋 Production Deployment Checklist

Before going to production, complete these items in order:

- [ ] **1. Rotate leaked SMTP credentials** — change the Gmail App Password immediately
- [ ] **2. Create `appsettings.Production.json`** with all production values (DB, CORS, JWT, SMTP, Blob)
- [ ] **3. Move secrets to environment variables** or Azure Key Vault
- [ ] **4. Create `.env.production`** for frontend with production API URL
- [ ] **5. Add `.env` to frontend `.gitignore`**
- [ ] **6. Fix hardcoded localhost** in `BlobStorageService.cs`
- [ ] **7. Add `app.UseHsts()`** for production
- [ ] **8. Add global `app.UseExceptionHandler()`** middleware
- [ ] **9. Delete dead scaffold files** (counter.ts, main.ts, style.css, Class1.cs × 2)
- [ ] **10. Remove debug console.log** statements from frontend
- [ ] **11. Create Dockerfile(s)** for backend and frontend
- [ ] **12. Set up CI/CD pipeline** (GitHub Actions recommended)
- [ ] **13. Add token cleanup** background service
- [ ] **14. Run `npm run build`** and fix any TypeScript errors
- [ ] **15. Run `dotnet test`** to verify all integration tests pass
- [ ] **16. Add more test coverage** — currently only 4 test files with ~5 tests total

