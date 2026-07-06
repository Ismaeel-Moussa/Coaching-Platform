# Joker Nutrition Coaching Platform – API Reference for Frontend Team
## Day 6 — Weekly Check-In System, Progress Photos & Notification Alerts

**API Base URL**: `https://localhost:7001`
All endpoint URLs below are relative to this base. All request and response bodies use the `application/json` format unless otherwise noted.

> [!NOTE]
> All endpoints in this document require a valid JWT in the Authorization header unless marked **Public**:
> `Authorization: Bearer <accessToken>`

---

## Check-In Endpoints (`/api/checkins/*`)

The **Check-In** flow is a 2-step process for the athlete:
1. Submit biometrics + subjective scores → `POST /api/checkins`
2. Optionally upload progress photos → `POST /api/checkins/{id}/photos`

**Key behaviors:**
- Photos (Front / Side / Back) are **all optional** — athletes may submit 0, 1, 2, or all 3 angles
- Resubmitting a check-in for the same week **updates the record in-place** (upsert — no duplicate rows)
- Uploading a photo for an angle that already has one **replaces** it (old file deleted from blob storage)
- Athletes can delete individual photo angles via `DELETE /api/checkins/{id}/photos/{angle}`

---

### 1. Submit Weekly Check-In
Creates or updates the biometrics + subjective check-in record for the current week.
On the **first** submission, the athlete's assigned coach automatically receives an in-app notification.

* **URL**: `/api/checkins`
* **Method**: `POST`
* **Authentication**: Required (`Athlete` role)
* **Request Body** (`SubmitCheckInForm` — `application/json`):
  ```json
  {
    "weightKg": 84.5,
    "waistCm": 87.0,
    "chestCm": 102.5,
    "thighCm": 60.0,
    "sleepQuality": 8,
    "energyLevel": 7,
    "gutHealth": 6,
    "trainingStress": 5
  }
  ```
  | Field | Type | Required | Notes |
  |-------|------|----------|-------|
  | `weightKg` | decimal | **Yes** | Body weight in kg |
  | `waistCm` | decimal | No | Waist measurement |
  | `chestCm` | decimal | No | Chest measurement |
  | `thighCm` | decimal | No | Thigh measurement |
  | `sleepQuality` | integer | **Yes** | Scale 1–10 |
  | `energyLevel` | integer | **Yes** | Scale 1–10 |
  | `gutHealth` | integer | **Yes** | Scale 1–10 |
  | `trainingStress` | integer | **Yes** | Scale 1–10 |

* **Response Body**:
  - `201 Created` — first submission this week
  - `200 OK` — resubmission (record updated in-place)
  - Both return a full `CheckInDto`:
  ```json
  {
    "id": 12,
    "athleteId": 3,
    "athleteFullName": "John Smith",
    "weekOf": "2026-06-30",
    "submittedAt": "2026-07-06T09:15:00Z",
    "weightKg": 84.5,
    "waistCm": 87.0,
    "chestCm": 102.5,
    "thighCm": 60.0,
    "sleepQuality": 8,
    "energyLevel": 7,
    "gutHealth": 6,
    "trainingStress": 5,
    "coachNotes": null,
    "coachReviewedAt": null,
    "photos": []
  }
  ```
* **Error Responses**:
  - `400 Bad Request` — Slider values outside 1–10 range, or required fields missing

**Frontend Usage**:
- Call this at the end of **Step 1** of the 4-step wizard (Biometrics + Subjective Sliders filled)
- Store the returned `id` — it is required for the photo upload call
- `weekOf` is always the **Monday of the current ISO week**, set server-side
- On resubmission, the same `id` is returned — no need to update the stored check-in ID
- After Step 1 completes, show **Step 2 (Photo Upload)** as optional — athlete may skip it

---

### 2. Upload Progress Photos
Uploads 1–3 progress photos for an existing check-in. Photos are sent as `multipart/form-data`.
If a photo already exists for a given angle, the old file is **deleted from blob storage** and replaced.

* **URL**: `/api/checkins/{id}/photos`
* **Method**: `POST`
* **Authentication**: Required (`Athlete` role — must own the check-in)
* **URL Parameter**: `{id}` — check-in integer ID
* **Content-Type**: `multipart/form-data`
* **Form Fields**:
  | Field Name | Type | Required | Notes |
  |------------|------|----------|-------|
  | `Front` | file | No | Front-facing progress photo |
  | `Side` | file | No | Side profile progress photo |
  | `Back` | file | No | Back-facing progress photo |

  At least one field must be provided. Accepted: `image/jpeg`, `image/png`. Max: **10 MB per file**.

* **Response Body** (`200 OK` — `CheckInDto`):
  Returns the full updated check-in object with the current `photos` array:
  ```json
  {
    "id": 12,
    "athleteId": 3,
    "athleteFullName": "John Smith",
    "weekOf": "2026-06-30",
    "submittedAt": "2026-07-06T09:15:00Z",
    "weightKg": 84.5,
    "waistCm": 87.0,
    "chestCm": 102.5,
    "thighCm": 60.0,
    "sleepQuality": 8,
    "energyLevel": 7,
    "gutHealth": 6,
    "trainingStress": 5,
    "coachNotes": null,
    "coachReviewedAt": null,
    "photos": [
      {
        "id": 5,
        "angle": "Front",
        "signedDownloadUrl": "https://jokernutrition.blob.core.windows.net/joker-progress-photos/checkins/12/front.jpg?sv=...&se=...&sp=r",
        "uploadedAt": "2026-07-06T09:20:00Z"
      },
      {
        "id": 6,
        "angle": "Side",
        "signedDownloadUrl": "https://jokernutrition.blob.core.windows.net/...",
        "uploadedAt": "2026-07-06T09:20:00Z"
      }
    ]
  }
  ```
* **Error Responses**:
  - `400 Bad Request` — Unsupported file type, file exceeds 10 MB, or no files provided
  - `403 Forbidden` — Athlete does not own this check-in
  - `404 Not Found` — Check-in ID does not exist

**Frontend Usage — Multipart Upload**:
```javascript
const formData = new FormData();
if (frontFile)  formData.append('Front', frontFile);
if (sideFile)   formData.append('Side',  sideFile);
if (backFile)   formData.append('Back',  backFile);

const response = await fetch(`/api/checkins/${checkInId}/photos`, {
  method: 'POST',
  headers: { 'Authorization': `Bearer ${token}` },
  body: formData
  // Do NOT set Content-Type manually — browser sets multipart boundary automatically
});
```
- Show upload progress bars using `XMLHttpRequest` with `onprogress`
- Use the `PhotoUploadZone.jsx` component for Front/Side/Back drag-drop zones
- After success, transition to **Step 4 Submit Confirmation** screen

> [!NOTE]
> If the athlete skips photos, simply skip calling this endpoint.
> The check-in is valid without photos — `photos: []` is a normal state.

---

### 3. Delete a Progress Photo
Removes a single photo angle from a check-in. Deletes the file from Azure Blob Storage and removes the DB record.

* **URL**: `/api/checkins/{id}/photos/{angle}`
* **Method**: `DELETE`
* **Authentication**: Required (`Athlete` role — must own the check-in)
* **URL Parameters**:
  - `{id}` — check-in integer ID
  - `{angle}` — one of: `Front` | `Side` | `Back`
* **Request Body**: None
* **Response Body** (`204 No Content`): Empty response indicating success.
* **Error Responses**:
  - `404 Not Found` — No photo exists for this angle on the given check-in
  - `403 Forbidden` — Athlete does not own this check-in

**Frontend Usage**:
- Wire to the **trash / X icon** on each `PhotoUploadZone.jsx` preview thumbnail
- After `204`, remove the photo from local check-in state and reset the zone to the empty drop area
- Athlete can immediately re-upload a new photo for that same angle after deletion

---

### 4. Get Check-In History (Athlete)
Returns the athlete's full weekly check-in history, paginated, ordered newest first.
Includes coach notes and signed photo download URLs.

* **URL**: `/api/checkins/history`
* **Method**: `GET`
* **Authentication**: Required (`Athlete` role)
* **Query Parameters**:
  | Parameter | Type | Required | Description |
  |-----------|------|----------|-------------|
  | `page` | integer | No | Defaults to `1` |
  | `pageSize` | integer | No | Defaults to `10` |

* **Response Body** (`200 OK` — `PagedResult<CheckInDto>`):
  ```json
  {
    "items": [
      {
        "id": 12,
        "athleteId": 3,
        "athleteFullName": "John Smith",
        "weekOf": "2026-06-30",
        "submittedAt": "2026-07-06T09:15:00Z",
        "weightKg": 84.5,
        "waistCm": 87.0,
        "chestCm": 102.5,
        "thighCm": 60.0,
        "sleepQuality": 8,
        "energyLevel": 7,
        "gutHealth": 6,
        "trainingStress": 5,
        "coachNotes": "Great progress this week. Keep carbs high on training days.",
        "coachReviewedAt": "2026-07-06T11:30:00Z",
        "photos": [
          {
            "id": 5,
            "angle": "Front",
            "signedDownloadUrl": "https://jokernutrition.blob.core.windows.net/joker-progress-photos/checkins/12/front.jpg?sv=...&se=...&sp=r",
            "uploadedAt": "2026-07-06T09:20:00Z"
          }
        ]
      }
    ],
    "totalCount": 8,
    "page": 1,
    "pageSize": 10
  }
  ```

> [!WARNING]
> `signedDownloadUrl` values expire after **24 hours**. Re-fetch this endpoint each session to get fresh URLs. Do NOT cache these URLs in localStorage or any persistent store.

---

### 5. Get Pending Check-Ins (Coach)
Returns athletes under the coach's roster who have **not** submitted a check-in for the current week.
Used to drive the "Pending Check-Ins" KPI count and alert list on the Coach Dashboard.

* **URL**: `/api/checkins/pending`
* **Method**: `GET`
* **Authentication**: Required (`Coach` or `Admin` roles)
* **Query Parameters**:
  | Parameter | Type | Required | Description |
  |-----------|------|----------|-------------|
  | `page` | integer | No | Defaults to `1` |
  | `pageSize` | integer | No | Defaults to `20` |

* **Response Body** (`200 OK` — `PagedResult<PendingCheckInDto>`):
  ```json
  {
    "items": [
      {
        "athleteId": 4,
        "athleteFullName": "Maria Garcia",
        "profilePictureUrl": null,
        "lastCheckInWeekOf": "2026-06-23",
        "daysSinceLastCheckIn": 13
      },
      {
        "athleteId": 7,
        "athleteFullName": "Kevin Osei",
        "profilePictureUrl": "https://jokernutrition.blob.core.windows.net/.../kevin.jpg",
        "lastCheckInWeekOf": null,
        "daysSinceLastCheckIn": -1
      }
    ],
    "totalCount": 2,
    "page": 1,
    "pageSize": 20
  }
  ```

**Notes**:
- `lastCheckInWeekOf` is `null` if the athlete has **never** submitted a check-in
- `daysSinceLastCheckIn` is `-1` if the athlete has never submitted

**Frontend Usage**:
- `totalCount` drives the **red alert badge** on the Coach Sidebar and Dashboard KPI strip
- `GET /api/checkins/pending?pageSize=1` is sufficient for badge count (use `totalCount`)

---

### 6. Add Coach Notes to Check-In
Saves the coach's written feedback for a specific check-in submission.

* **URL**: `/api/checkins/{id}/coach-notes`
* **Method**: `PUT`
* **Authentication**: Required (`Coach` or `Admin` roles)
* **URL Parameter**: `{id}` — check-in integer ID
* **Request Body** (`AddCoachNotesForm`):
  ```json
  {
    "notes": "Great progress this week. Keep carbs high on training days. Reduce sodium before next check-in."
  }
  ```
  Max length: 2000 characters.

* **Response Body** (`200 OK` — `CheckInDto`):
  Returns the full updated `CheckInDto` with `coachNotes` and `coachReviewedAt` populated.

**Frontend Usage**:
- Wires to the "Coach feedback/notes text area + submit" on the **Client Detail View**
- After a successful `PUT`, update the check-in card UI with the returned `coachNotes` and `coachReviewedAt`

---

### 7. Get Check-In Photos (Coach/Admin/Athlete)
Returns signed 24-hour download URLs for all photos attached to a specific check-in.

* **URL**: `/api/checkins/{id}/photos`
* **Method**: `GET`
* **Authentication**: Required (`Coach`, `Admin`, or the owning `Athlete`)
* **URL Parameter**: `{id}` — check-in integer ID
* **Request Body**: None
* **Response Body** (`200 OK` — `List<CheckInPhotoDto>`):
  ```json
  [
    {
      "id": 5,
      "angle": "Front",
      "signedDownloadUrl": "https://jokernutrition.blob.core.windows.net/joker-progress-photos/checkins/12/front.jpg?sv=...&se=2026-07-07T09%3A15%3A00Z&sp=r&...",
      "uploadedAt": "2026-07-06T09:20:00Z"
    },
    {
      "id": 6,
      "angle": "Side",
      "signedDownloadUrl": "https://...",
      "uploadedAt": "2026-07-06T09:20:00Z"
    }
  ]
  ```

> [!WARNING]
> Signed URLs expire after **24 hours**. Always call this endpoint fresh when opening a check-in detail view. Never cache or store signed URLs.

---

## Notification Endpoints (`/api/notifications/*`)

These endpoints power the **unread badge** on the Coach Sidebar and the notification dropdown.

---

### 1. Get Unread Notifications
Returns all unread notifications for the currently logged-in user, ordered newest first.

* **URL**: `/api/notifications`
* **Method**: `GET`
* **Authentication**: Required (all authenticated roles)
* **Query Parameters**:
  | Parameter | Type | Required | Description |
  |-----------|------|----------|-------------|
  | `page` | integer | No | Defaults to `1` |
  | `pageSize` | integer | No | Defaults to `20` |

* **Response Body** (`200 OK` — `PagedResult<NotificationDto>`):
  ```json
  {
    "items": [
      {
        "id": 55,
        "type": "CheckInSubmitted",
        "message": "John Smith submitted their weekly check-in.",
        "isRead": false,
        "createdAt": "2026-07-06T09:15:00Z"
      },
      {
        "id": 54,
        "type": "CheckInSubmitted",
        "message": "Maria Garcia submitted their weekly check-in.",
        "isRead": false,
        "createdAt": "2026-07-05T18:45:00Z"
      }
    ],
    "totalCount": 2,
    "page": 1,
    "pageSize": 20
  }
  ```

---

### 2. Get Unread Notification Count
Returns only the unread notification count. Used exclusively for the sidebar badge.

* **URL**: `/api/notifications/count`
* **Method**: `GET`
* **Authentication**: Required (all authenticated roles)
* **Request Body**: None
* **Response Body** (`200 OK`):
  ```json
  {
    "unreadCount": 2
  }
  ```

**Frontend Usage**:
- Poll every **30 seconds** (same interval as the live feed) to keep the badge current
- Display as a red circle badge on the "Check-ins" nav item in `CoachLayout.jsx`
- If `unreadCount` is `0`, hide the badge entirely

---

### 3. Mark Notification as Read

* **URL**: `/api/notifications/{id}/read`
* **Method**: `PUT`
* **Authentication**: Required (must own the notification)
* **URL Parameter**: `{id}` — notification integer ID
* **Request Body**: None
* **Response Body** (`204 No Content`): Empty response indicating success.

---

### 4. Mark All Notifications as Read

* **URL**: `/api/notifications/read-all`
* **Method**: `PUT`
* **Authentication**: Required (all authenticated roles)
* **Request Body**: None
* **Response Body** (`204 No Content`): Empty response indicating success.

**Frontend Usage**:
- Trigger on "Mark all as read" button in the notification panel
- After success, immediately reset badge count to `0` in local state

---

## Data Definitions

### CheckInDto Object
| Field | Type | Notes |
|-------|------|-------|
| `id` | integer | Check-in record ID |
| `athleteId` | integer | Athlete's ID |
| `athleteFullName` | string | "FirstName LastName" |
| `weekOf` | string | ISO date "YYYY-MM-DD" — Monday of the check-in week |
| `submittedAt` | string | ISO UTC timestamp — updated on each resubmission |
| `weightKg` | decimal | Body weight in kilograms |
| `waistCm` | decimal or null | Waist measurement |
| `chestCm` | decimal or null | Chest measurement |
| `thighCm` | decimal or null | Thigh measurement |
| `sleepQuality` | integer (1–10) | Subjective slider |
| `energyLevel` | integer (1–10) | Subjective slider |
| `gutHealth` | integer (1–10) | Subjective slider |
| `trainingStress` | integer (1–10) | Subjective slider |
| `coachNotes` | string or null | null until coach reviews |
| `coachReviewedAt` | string or null | ISO UTC timestamp or null |
| `photos` | CheckInPhotoDto[] | Empty array if no photos uploaded |

---

### CheckInPhotoDto Object
| Field | Type | Notes |
|-------|------|-------|
| `id` | integer | Photo record ID |
| `angle` | string | "Front" or "Side" or "Back" |
| `signedDownloadUrl` | string | 24-hour expiring Azure Blob SAS URL |
| `uploadedAt` | string | ISO UTC timestamp |

---

### NotificationDto Object
| Field | Type | Notes |
|-------|------|-------|
| `id` | integer | Notification record ID |
| `type` | string | See type enum mapping below |
| `message` | string | Human-readable alert text |
| `isRead` | boolean | false = unread (shows in badge) |
| `createdAt` | string | ISO UTC timestamp |

### Notification Type Enum Mapping
| String Value | Trigger |
|---|---|
| `"CheckInSubmitted"` | Athlete submits weekly check-in (first submit only) — coach notified |
| `"WorkoutCompleted"` | Athlete marks a workout as complete |
| `"CoachNote"` | Coach adds feedback to an athlete check-in |
| `"MacroAlert"` | Athlete exceeds calorie target by >5% |

---

### PhotoAngle Enum Mapping
| String Value | Form Field Name | URL Segment | UI Label |
|---|---|---|---|
| `"Front"` | `Front` | `Front` | Front view photo zone |
| `"Side"` | `Side` | `Side` | Side profile photo zone |
| `"Back"` | `Back` | `Back` | Back view photo zone |

---

### PendingCheckInDto Object
| Field | Type | Notes |
|-------|------|-------|
| `athleteId` | integer | Athlete's ID |
| `athleteFullName` | string | Full name |
| `profilePictureUrl` | string or null | Avatar URL |
| `lastCheckInWeekOf` | string or null | "YYYY-MM-DD" of last check-in, or null if never |
| `daysSinceLastCheckIn` | integer | -1 if never submitted |

---

## Day 6 Endpoint Summary

| Method | URL | Auth | Description |
|--------|-----|------|-------------|
| `POST` | `/api/checkins` | Athlete | Submit or resubmit check-in (upsert — 201 first time, 200 on update) |
| `POST` | `/api/checkins/{id}/photos` | Athlete | Upload 1–3 photos as multipart/form-data (replaces same-angle) |
| `DELETE` | `/api/checkins/{id}/photos/{angle}` | Athlete | Delete a single photo angle (Front, Side, or Back) |
| `GET` | `/api/checkins/history` | Athlete | Paginated check-in history with coach notes and photos |
| `GET` | `/api/checkins/pending` | Coach/Admin | Athletes missing check-in this week |
| `PUT` | `/api/checkins/{id}/coach-notes` | Coach/Admin | Save coach feedback on a check-in |
| `GET` | `/api/checkins/{id}/photos` | Coach/Admin/Athlete | Signed 24h download URLs for check-in photos |
| `GET` | `/api/notifications` | All | Unread notifications for current user |
| `GET` | `/api/notifications/count` | All | Unread count integer for sidebar badge |
| `PUT` | `/api/notifications/{id}/read` | All | Mark single notification as read |
| `PUT` | `/api/notifications/read-all` | All | Mark all notifications as read |
