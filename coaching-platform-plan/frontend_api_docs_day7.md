# Joker Nutrition Coaching Platform – API Reference for Frontend Team
## Day 7: Hardening, Health Check & Audit

**API Base URL**: `https://localhost:7001`  
All endpoint URLs below are relative to this base. All request and response bodies use the `application/json` format.

> [!NOTE]
> Day 7 introduces **no new domain endpoints**. This document covers:
> 1. The enriched `GET /api/health` endpoint (changed response shape)
> 2. A reference for all **rate-limiting** behaviour the frontend must handle
> 3. The **CORS origins** that are now config-driven (important for staging/production deploys)
> 4. **Error response format** — a definitive reference so the frontend team handles all error shapes consistently

---

## 1. Health Check Endpoint

### `GET /api/health`
* **Authentication**: None (Public)
* **Rate Limit**: None
* **Response Body** (`200 OK`):
  ```json
  {
    "status": "healthy",      // "healthy" | "degraded"
    "database": "connected",  // "connected" | "unreachable" | "error"
    "timestamp": "2026-07-06T12:00:00Z"
  }
  ```
  > **Note**: The endpoint always returns HTTP 200. Check the `status` field, not the HTTP status code, to determine API health. If `database` is `"unreachable"` or `"error"`, display a banner warning to users.

* **Frontend Usage**:
  - Poll this endpoint every 60 seconds from the app shell.
  - If `status !== "healthy"`, show a non-blocking toast: *"Service is temporarily degraded. Some features may not be available."*
  - Do **not** block user interaction on degraded status.

---

## 2. Rate Limiting — What the Frontend Must Handle

All rate-limited endpoints return **HTTP 429 Too Many Requests** when the limit is exceeded.

| Endpoint | Limit | Period | Frontend Action |
|---|---|---|---|
| `POST /api/auth/login` | 10 requests | 1 minute | Show "Too many login attempts. Please wait 1 minute." toast |
| `POST /api/auth/forgot-password` | 3 requests | 1 hour | Show "Reset link request limit reached. Please try again in 1 hour." |

* **Response Body** (`429 Too Many Requests`):
  ```json
  {
    "message": "API calls quota exceeded! Maximum admitted 10 per 1m."
  }
  ```

* **Implementation Guide**:
  ```javascript
  // In your axios interceptor (services/api.js):
  if (error.response?.status === 429) {
    toast.error(getRateLimitMessage(error.config.url));
    return Promise.reject(error);
  }
  ```

---

## 3. Standard Error Response Format

All API errors (from `ApiExceptionFilter`) are returned in a consistent envelope:

* **4xx Client Errors**:
  ```json
  {
    "message": "Human-readable error description",
    "statusCode": 400
  }
  ```

* **401 Unauthorized** (no/invalid JWT):
  ```json
  {
    "message": "Unauthorized"
  }
  ```

* **403 Forbidden** (wrong role):
  ```json
  {
    "message": "Forbidden"
  }
  ```

* **500 Server Error**:
  ```json
  {
    "message": "An unexpected error occurred.",
    "statusCode": 500
  }
  ```

### Recommended Axios Interceptor Pattern
```javascript
// services/api.js
import axios from 'axios';

const api = axios.create({ baseURL: 'https://localhost:7001/api' });

api.interceptors.request.use(config => {
  const token = getAccessToken(); // from AppContext
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

api.interceptors.response.use(
  res => res,
  async error => {
    const status = error.response?.status;

    if (status === 401) {
      // Try to refresh; if fails, redirect to /login
      try {
        await refreshTokens();
        return api.request(error.config);  // retry original request
      } catch {
        logout();
        window.location.href = '/login';
      }
    }

    if (status === 429) {
      toast.error('Too many requests. Please wait and try again.');
    }

    if (status >= 500) {
      toast.error('Server error. Please try again later.');
    }

    return Promise.reject(error);
  }
);

export default api;
```

---

## 4. CORS Configuration Reference

The API is now configured to read allowed origins from `appsettings.json`. 

**Development origins (current)**:
- `http://localhost:5173`
- `https://localhost:5173`

**What this means for the frontend team**:
- All `axios` requests from `localhost:5173` work as before — no changes needed for local dev.
- In **staging/production**, the backend team will add the production URL to `appsettings.Production.json`. Frontend team does **not** need to change anything; just ensure the deployed frontend serves from the registered origin.
- **Never set `withCredentials: false`** in axios — the API uses `AllowCredentials()` which requires `withCredentials: true` for cookie-based auth. Current JWT implementation doesn't use cookies but the config is forward-compatible.

---

## 5. Swagger / OpenAPI Documentation

**URL**: `https://localhost:7001/swagger` (Development only)

All endpoints now have `/// <summary>` XML documentation visible in Swagger UI. Frontend developers can:
- Browse all endpoints with descriptions
- See request/response schemas
- Try out endpoints using the "Authorize" button (paste your Bearer token)

---

## 6. Audit Logging — What Gets Recorded (Informational)

The following events are now automatically audit-logged to the `AuditLogs` table. **No frontend changes required.** This is informational for awareness:

| Event | Trigger | Recorded Fields |
|---|---|---|
| `Login` | Successful `POST /api/auth/login` | UserId, IP address, timestamp |
| `CheckInPhotoAccess` | `GET /api/checkins/{id}/photos` by a coach | CoachUserId, CheckInId, IP, timestamp |
| `FoodDeleted` | `DELETE /api/foods/{id}` | AdminUserId, FoodId, timestamp |
| `ExerciseDeleted` | `DELETE /api/exercises/{id}` | UserId, ExerciseId, timestamp |

---

## 7. Integration Test Endpoints Reference

These are the endpoints covered by the Day 7 integration test suite (`JokerNutrition.Tests`):

| Test Suite | Endpoints Tested | Key Assertions |
|---|---|---|
| `AuthTests` | `POST /api/auth/login`, `GET /api/health` | 200+JWT on valid creds, 401 on invalid, health always 200 |
| `DiaryTests` | `GET /api/diary/{date}`, `GET /api/diary/summary/{date}`, `PATCH /api/diary/{date}/water` | Athletes: 200, Coaches: 403 |
| `WorkoutTests` | `GET /api/workout-logs/today`, `GET /api/workout-logs/history`, `POST /api/workout-logs/log-set` | Athletes: 200, Coaches: 403; invalid payload: 400 |
| `CheckInTests` | `GET /api/checkins/history`, `GET /api/checkins/pending`, `POST /api/checkins`, `PUT /api/checkins/{id}/coach-notes` | Role enforcement; slider 1-10 validation |

---

## 8. Day 7 Deliverables Summary for Frontend

| # | What changed on backend | Frontend action required |
|---|---|---|
| 1 | Health check returns `{ status, database, timestamp }` | Update health polling to read `body.status` field |
| 2 | Rate limiting: 429 on login after 10/min | Add 429 handler in axios interceptor |
| 3 | CORS reads from config | No change for dev; check prod domain registration |
| 4 | Swagger has XML descriptions | Use Swagger to explore/verify all endpoints |
| 5 | Audit logging active | None — fully server-side |
| 6 | New EF migration applied | None — DB schema auto-migrates on startup |
