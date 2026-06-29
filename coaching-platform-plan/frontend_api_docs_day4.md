# Joker Nutrition Coaching Platform – API Reference for Frontend Team
## Day 4 — Coach Operations Hub

**API Base URL**: `https://localhost:7001`  
All endpoint URLs below are relative to this base. All request and response bodies use the `application/json` format.

> [!NOTE]
> All endpoints in this section require a Coach or Admin JWT in the Authorization header:  
> `Authorization: Bearer <accessToken>`

---

## Coach Hub Endpoints (`/api/coach-hub/*`)

---

### 1. Coach Dashboard
Returns the KPI strip and the 10 most recent live feed events for the coach's dashboard page.

* **URL**: `/api/coach-hub/dashboard`
* **Method**: `GET`
* **Authentication**: Required (`Coach` or `Admin` roles)
* **Request Body**: None
* **Response Body** (`200 OK` – `CoachDashboardDto`):
  ```json
  {
    "activeAthleteCount": 12,
    "avgWorkoutCompletionPercent": 78.3,
    "pendingCheckInsCount": 3,
    "recentFeed": [
      {
        "athleteId": 5,
        "athleteName": "Sarah Lopez",
        "athleteAvatarUrl": null,
        "workoutDayLabel": "Push Day 1",
        "status": "Completed",
        "completedAt": "2026-06-29T07:45:00Z",
        "date": "2026-06-29"
      },
      {
        "athleteId": 8,
        "athleteName": "Jake Morrison",
        "athleteAvatarUrl": null,
        "workoutDayLabel": "Pull Day 2",
        "status": "InProgress",
        "completedAt": null,
        "date": "2026-06-29"
      }
    ]
  }
  ```

**Frontend Usage**: Render the KPI strip cards and prime the Live Feed panel on initial load. The full paginated feed is loaded separately via `/live-feed`.

---

### 2. Live Feed (Paginated)
Returns a paginated, chronologically-ordered list of all workout events across the coach's roster.  
The frontend polls this every 30 seconds to keep the feed live.

* **URL**: `/api/coach-hub/live-feed`
* **Method**: `GET`
* **Authentication**: Required (`Coach` or `Admin` roles)
* **Query Parameters**:
  - `page` (integer, optional, defaults to `1`)
  - `pageSize` (integer, optional, defaults to `20`)
* **Response Body** (`200 OK` – `PagedResult<LiveFeedItemDto>`):
  ```json
  {
    "items": [
      {
        "athleteId": 5,
        "athleteName": "Sarah Lopez",
        "athleteAvatarUrl": null,
        "workoutDayLabel": "Push Day 1",
        "status": "Completed",
        "completedAt": "2026-06-29T07:45:00Z",
        "date": "2026-06-29"
      },
      {
        "athleteId": 11,
        "athleteName": "Omar Hassan",
        "athleteAvatarUrl": null,
        "workoutDayLabel": "Legs Day 1",
        "status": "Missed",
        "completedAt": null,
        "date": "2026-06-28"
      }
    ],
    "totalCount": 87,
    "page": 1,
    "pageSize": 20
  }
  ```

**`status` values**: `"InProgress"` | `"Completed"` | `"Missed"`

**Frontend Usage**:
- `"InProgress"` → Gold badge (`#fdc003`) with pulse animation
- `"Completed"` → Green badge
- `"Missed"` → Red badge (`#ba1a1a`)

Poll with:
```javascript
setInterval(() => refetchLiveFeed(), 30000);
```

---

### 3. Compliance Roster (Today)
Returns per-athlete macro and calorie compliance for today.  
Used to populate the `ComplianceBar` components on the coach dashboard.

* **URL**: `/api/coach-hub/compliance`
* **Method**: `GET`
* **Authentication**: Required (`Coach` or `Admin` roles)
* **Request Body**: None
* **Response Body** (`200 OK` – `ComplianceItemDto[]`):
  ```json
  [
    {
      "athleteId": 5,
      "athleteName": "Sarah Lopez",
      "athleteAvatarUrl": null,
      "targetCalories": 2400.0,
      "consumedCalories": 1850.0,
      "targetProtein": 180.0,
      "consumedProtein": 140.0,
      "targetCarbs": 240.0,
      "consumedCarbs": 180.0,
      "targetFat": 70.0,
      "consumedFat": 55.0,
      "isOverCalorieTarget": false,
      "compliancePercent": 77.1
    },
    {
      "athleteId": 8,
      "athleteName": "Jake Morrison",
      "athleteAvatarUrl": null,
      "targetCalories": 3000.0,
      "consumedCalories": 3210.0,
      "targetProtein": 220.0,
      "consumedProtein": 230.0,
      "targetCarbs": 300.0,
      "consumedCarbs": 315.0,
      "targetFat": 80.0,
      "consumedFat": 86.0,
      "isOverCalorieTarget": true,
      "compliancePercent": 100.0
    }
  ]
  ```

**Frontend Usage**:
- `compliancePercent` → fills the `ComplianceBar` in Gold (`#fdc003`)
- `isOverCalorieTarget: true` → turns the bar Red (`#ba1a1a`) with alert icon
- Alert threshold: consumed calories > target × 1.05

---

### 4. Client Roster (Paginated + Filtered)
Returns the full paginated athlete roster for the coach, with optional status filters.

* **URL**: `/api/coach-hub/roster`
* **Method**: `GET`
* **Authentication**: Required (`Coach` or `Admin` roles)
* **Query Parameters**:
  - `page` (integer, optional, defaults to `1`)
  - `pageSize` (integer, optional, defaults to `20`)
  - `filter` (string, optional): `"ComplianceAlert"` | `"NoRecentCheckIn"` | omit for All
* **Response Body** (`200 OK` – `PagedResult<RosterItemDto>`):
  ```json
  {
    "items": [
      {
        "athleteId": 5,
        "athleteName": "Sarah Lopez",
        "athleteAvatarUrl": null,
        "activeProgramName": "PPL Phase 1",
        "macroCompliancePercent": 77.1,
        "lastCheckInDate": "2026-06-22T10:30:00Z",
        "status": "Active"
      },
      {
        "athleteId": 11,
        "athleteName": "Omar Hassan",
        "athleteAvatarUrl": null,
        "activeProgramName": "PPL Phase 1",
        "macroCompliancePercent": 105.0,
        "lastCheckInDate": "2026-06-10T08:00:00Z",
        "status": "NoRecentCheckIn"
      }
    ],
    "totalCount": 12,
    "page": 1,
    "pageSize": 20
  }
  ```

**`status` values**:
| Status | Badge Color | Condition |
|--------|------------|-----------|
| `"Active"` | Green | Check-in within 7 days AND compliance normal |
| `"ComplianceAlert"` | Red | compliance > 105% OR < 40% |
| `"NoRecentCheckIn"` | Amber | No check-in submitted in the past 7 days |

**Frontend Usage**: Render a sortable data table. Clicking any row navigates to `/coach/athletes/{athleteId}`.

**Filter examples**:
```
GET /api/coach-hub/roster?filter=ComplianceAlert
GET /api/coach-hub/roster?filter=NoRecentCheckIn
GET /api/coach-hub/roster?page=2&pageSize=10
```

---

### 5. Athlete Deep Profile
Returns the full profile for a single athlete for the Client Detail View.

* **URL**: `/api/coach-hub/athletes/{id}` (where `{id}` is the athlete's integer ID)
* **Method**: `GET`
* **Authentication**: Required (`Coach` or `Admin` roles)
* **Request Body**: None
* **Response Body** (`200 OK` – `AthleteDeepProfileDto`):
  ```json
  {
    "id": 5,
    "fullName": "Sarah Lopez",
    "avatarUrl": null,
    "targetGoal": "Fat Loss",
    "weightKg": 68.5,
    "heightCm": 165.0,
    "currentStreak": 14,
    "longestStreak": 21,
    "currentTargets": {
      "id": 3,
      "targetCalories": 2400.0,
      "targetProtein": 180.0,
      "targetCarbs": 240.0,
      "targetFat": 70.0,
      "waterLitersTarget": 4.0,
      "stepsTarget": 7000,
      "setAt": "2026-06-15T09:00:00Z",
      "setByCoachName": "Marcus Steel"
    },
    "weightHistory": [
      { "weekOf": "2026-06-01", "weightKg": 70.2 },
      { "weekOf": "2026-06-08", "weightKg": 69.5 },
      { "weekOf": "2026-06-15", "weightKg": 68.9 },
      { "weekOf": "2026-06-22", "weightKg": 68.5 }
    ],
    "feedbackNotes": [
      {
        "id": 7,
        "noteText": "Great progress this week! Increase carbs on training days by 30g.",
        "coachName": "Marcus Steel",
        "createdAt": "2026-06-22T11:00:00Z"
      }
    ]
  }
  ```

> [!IMPORTANT]
> This endpoint returns `401 Unauthorized` if the requested athlete does not belong to the logged-in coach's roster. Never assume cross-coach access.

**Frontend Usage**:
- `weightHistory` → Feed into a Recharts `LineChart` (x-axis = `weekOf`, y-axis = `weightKg`). Array is sorted oldest → newest.
- `feedbackNotes` → Render in a scrollable coach notes panel at the bottom of the profile. Sorted newest → oldest.
- `currentTargets` → Display in a stats card above the charts

---

### 6. Save Coach Feedback Note
Saves a new text note for an athlete and sends them an in-app notification.

* **URL**: `/api/coach-hub/athletes/{id}/notes` (where `{id}` is the athlete's integer ID)
* **Method**: `POST`
* **Authentication**: Required (`Coach` or `Admin` roles)
* **Request Body** (`SaveFeedbackNoteForm`):
  ```json
  {
    "noteText": "Excellent week! Let's push the squat weight up by 5kg next session."
  }
  ```
  | Field | Type | Rules |
  |-------|------|-------|
  | `noteText` | string | Required, min 1 char, max 2000 chars |

* **Response Body** (`201 Created` – `CoachFeedbackNoteDto`):
  ```json
  {
    "id": 8,
    "noteText": "Excellent week! Let's push the squat weight up by 5kg next session.",
    "coachName": "Marcus Steel",
    "createdAt": "2026-06-29T09:35:00Z"
  }
  ```

**Frontend Usage**: On `201` success, prepend the returned note object to the top of the `feedbackNotes` array in component state — no need to re-fetch the full profile.

---

### 7. Athlete Weight History
Returns weight trend data for a single athlete from their weekly check-in submissions.  
Use this to power the standalone weight chart on the client detail page.

* **URL**: `/api/coach-hub/athletes/{id}/weight-history` (where `{id}` is the athlete's integer ID)
* **Method**: `GET`
* **Authentication**: Required (`Coach` or `Admin` roles)
* **Request Body**: None
* **Response Body** (`200 OK` – `WeightHistoryPointDto[]`):
  ```json
  [
    { "weekOf": "2026-06-01", "weightKg": 70.2 },
    { "weekOf": "2026-06-08", "weightKg": 69.5 },
    { "weekOf": "2026-06-15", "weightKg": 68.9 },
    { "weekOf": "2026-06-22", "weightKg": 68.5 }
  ]
  ```

**Frontend Usage**: Feed directly into a Recharts `LineChart`. Array is already sorted oldest → newest. `weekOf` is a `DateOnly` serialized as `"YYYY-MM-DD"`.

---

## Data Definitions

### Workout `status` Enum Mapping
The `status` field in live feed and workout logs maps from the C# `WorkoutStatus` enum:
- `"InProgress"` — Athlete has opened today's workout but not completed it
- `"Completed"` — Athlete tapped "Complete Workout" today
- `"Missed"` — Workout day passed without completion

### Roster `status` Mapping
This is a computed field (not stored in DB). Derived from business logic each request:
- `"Active"` — Check-in within 7 days AND compliance is normal
- `"ComplianceAlert"` — Calorie compliance > 105% or < 40%
- `"NoRecentCheckIn"` — No check-in submitted in the past 7 days

### `PagedResult<T>` Shape
All paginated endpoints return this envelope:
```json
{
  "items": [],       // array of T
  "totalCount": 0,   // total records matching the query (use for pagination controls)
  "page": 1,
  "pageSize": 20
}
```

### `MacroTargetDto` (nested in Deep Profile)
```json
{
  "id": 3,
  "targetCalories": 2400.0,
  "targetProtein": 180.0,
  "targetCarbs": 240.0,
  "targetFat": 70.0,
  "waterLitersTarget": 4.0,
  "stepsTarget": 7000,
  "setAt": "2026-06-15T09:00:00Z",
  "setByCoachName": "Marcus Steel"
}
```

### `DateOnly` Format
All `DateOnly` fields serialize as ISO 8601 date-only strings: `"2026-06-29"` (no time component).

---

## Error Responses

| HTTP Code | Scenario |
|-----------|---------|
| `401 Unauthorized` | Missing or invalid JWT, or athlete does not belong to coach's roster |
| `403 Forbidden` | Authenticated but insufficient role (e.g., Athlete accessing coach routes) |
| `404 Not Found` | Athlete ID does not exist |
| `400 Bad Request` | `noteText` is empty or exceeds 2000 characters |
