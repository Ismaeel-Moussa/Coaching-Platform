# Joker Nutrition Coaching Platform – API Reference for Frontend Team
## Day 5 — Admin Content Libraries & Workout Template Builder

**API Base URL**: `https://localhost:7001`
All endpoint URLs below are relative to this base. All request and response bodies use the `application/json` format unless otherwise noted.

> [!NOTE]
> All endpoints in this document require a Coach or Admin JWT in the Authorization header unless marked **Public**:
> `Authorization: Bearer <accessToken>`

---

## Exercise Library Endpoints (`/api/exercises/*`)

---

### 1. List Exercises (Paginated + Filtered)
Returns the full exercise catalog with optional category filter and keyword search.
Used by the **Exercise Library Admin** card grid and the **Workout Template Builder** exercise selector panel.

* **URL**: `/api/exercises`
* **Method**: `GET`
* **Authentication**: Required (all authenticated roles)
* **Query Parameters**:
  | Parameter | Type | Required | Description |
  |-----------|------|----------|-------------|
  | `page` | integer | No | Defaults to `1` |
  | `pageSize` | integer | No | Defaults to `20` |
  | `muscleGroup` | string | No | Filter by muscle group. See enum values below. |
  | `search` | string | No | Keyword search on exercise name (case-insensitive) |

* **Response Body** (`200 OK` - `PagedResult<ExerciseDto>`):
  ```json
  {
    "items": [
      {
        "id": 1,
        "name": "Barbell Bench Press",
        "primaryMuscle": "Chest",
        "equipmentRequired": "Barbell, Bench",
        "instructions": "Lie flat on bench, grip bar slightly wider than shoulder width...",
        "youTubeVideoId": "rT7DgCr-3pg",
        "isActive": true,
        "createdAt": "2026-06-01T00:00:00Z"
      },
      {
        "id": 2,
        "name": "Incline Dumbbell Press",
        "primaryMuscle": "Chest",
        "equipmentRequired": "Dumbbells, Incline Bench",
        "instructions": null,
        "youTubeVideoId": null,
        "isActive": true,
        "createdAt": "2026-06-01T00:00:00Z"
      }
    ],
    "totalCount": 32,
    "page": 1,
    "pageSize": 20
  }
  ```

**`muscleGroup` filter values**: `Chest` | `Back` | `Shoulders` | `Arms` | `Legs` | `Cardio` | `Core`

**Frontend Usage**:
- Category filter tabs (Chest, Back, Shoulders, Arms, Legs, Cardio) pass as `?muscleGroup=Chest`
- Search bar passes as `?search=squat`
- Combine: `GET /api/exercises?muscleGroup=Legs&search=squat&page=1&pageSize=10`

---

### 2. Get Exercise by ID
Returns full details for a single exercise, including instructions and YouTube video ID.

* **URL**: `/api/exercises/{id}`
* **Method**: `GET`
* **Authentication**: Required (all authenticated roles)
* **URL Parameter**: `{id}` — the exercise's integer ID
* **Request Body**: None
* **Response Body** (`200 OK` - `ExerciseDto`):
  ```json
  {
    "id": 1,
    "name": "Barbell Bench Press",
    "primaryMuscle": "Chest",
    "equipmentRequired": "Barbell, Bench",
    "instructions": "Lie flat on bench, grip bar slightly wider than shoulder width. Lower bar to mid-chest. Press back to lockout.",
    "youTubeVideoId": "rT7DgCr-3pg",
    "isActive": true,
    "createdAt": "2026-06-01T00:00:00Z"
  }
  ```

**Frontend Usage**: Populate the **Add Exercise Modal** fields when editing. Embed YouTube preview with `https://www.youtube.com/embed/{youTubeVideoId}`.

---

### 3. Create Exercise
Creates a new exercise entry in the global exercise library.

* **URL**: `/api/exercises`
* **Method**: `POST`
* **Authentication**: Required (`Coach` or `Admin` roles)
* **Request Body** (`CreateExerciseForm`):
  ```json
  {
    "name": "Cable Lateral Raise",
    "primaryMuscle": "Shoulders",
    "equipmentRequired": "Cable Machine",
    "instructions": "Stand side-on to cable, grasp low pulley handle. Raise arm to shoulder height...",
    "youTubeVideoId": "3VcKaXpzqRo"
  }
  ```
  | Field | Type | Rules |
  |-------|------|-------|
  | `name` | string | Required, max 200 chars |
  | `primaryMuscle` | string | Required. Must be a valid `MuscleGroup` enum value |
  | `equipmentRequired` | string | Optional, max 500 chars |
  | `instructions` | string | Optional, max 4000 chars |
  | `youTubeVideoId` | string | Optional, max 20 chars (video hash only, not full URL) |

* **Response Body** (`201 Created` - `ExerciseDto`):
  Returns the fully created exercise object including its new `id` and `createdAt`.

---

### 4. Update Exercise
Updates an existing exercise entry. All fields are optional — only send fields you want to change.

* **URL**: `/api/exercises/{id}`
* **Method**: `PUT`
* **Authentication**: Required (`Coach` or `Admin` roles)
* **URL Parameter**: `{id}` — the exercise's integer ID
* **Request Body** (`UpdateExerciseForm`):
  ```json
  {
    "name": "Cable Lateral Raise (Updated)",
    "youTubeVideoId": "newVideoHash123"
  }
  ```
  Same fields as `CreateExerciseForm`. All optional for partial updates.

* **Response Body** (`200 OK` - `ExerciseDto`): Returns the updated exercise object.

---

### 5. Delete Exercise (Soft-Delete)
Soft-deletes an exercise by setting `isActive = false`. Deleted exercises no longer appear in list or search results and cannot be added to new templates. Existing template exercises that reference this exercise are **not** removed.

* **URL**: `/api/exercises/{id}`
* **Method**: `DELETE`
* **Authentication**: Required (`Coach` or `Admin` roles)
* **URL Parameter**: `{id}` — the exercise's integer ID
* **Request Body**: None
* **Response Body** (`204 No Content`): Empty response indicating success.

> [!IMPORTANT]
> After a successful `204`, remove the exercise card from the UI locally — do NOT re-fetch the full list. Attempting to `GET /api/exercises/{id}` for a soft-deleted exercise returns `404 Not Found`.

---

## Workout Template Endpoints (`/api/workout-templates/*`)

---

### 1. List Workout Templates
Returns all workout templates created by the logged-in coach.

* **URL**: `/api/workout-templates`
* **Method**: `GET`
* **Authentication**: Required (`Coach` or `Admin` roles)
* **Query Parameters**:
  - `page` (integer, optional, defaults to `1`)
  - `pageSize` (integer, optional, defaults to `20`)
* **Response Body** (`200 OK` - `PagedResult<WorkoutTemplateSummaryDto>`):
  ```json
  {
    "items": [
      {
        "id": 1,
        "name": "PPL Phase 1 — Beginner",
        "description": "6-day Push/Pull/Legs split for hypertrophy",
        "coachName": "Marcus Steel",
        "dayCount": 7,
        "isActive": true,
        "createdAt": "2026-06-01T00:00:00Z"
      },
      {
        "id": 2,
        "name": "PPL Phase 2 — Intermediate",
        "description": null,
        "coachName": "Marcus Steel",
        "dayCount": 7,
        "isActive": true,
        "createdAt": "2026-06-15T00:00:00Z"
      }
    ],
    "totalCount": 2,
    "page": 1,
    "pageSize": 20
  }
  ```

---

### 2. Get Workout Template by ID
Returns a single template with all its days and exercises — the full nested structure to hydrate the Template Builder canvas.

* **URL**: `/api/workout-templates/{id}`
* **Method**: `GET`
* **Authentication**: Required (`Coach` or `Admin` roles)
* **URL Parameter**: `{id}` — the template's integer ID
* **Request Body**: None
* **Response Body** (`200 OK` - `WorkoutTemplateDto`):
  ```json
  {
    "id": 1,
    "name": "PPL Phase 1 — Beginner",
    "description": "6-day Push/Pull/Legs split for hypertrophy",
    "coachName": "Marcus Steel",
    "isActive": true,
    "createdAt": "2026-06-01T00:00:00Z",
    "days": [
      {
        "id": 10,
        "dayNumber": 1,
        "dayLabel": "Push Day 1",
        "isRestDay": false,
        "exercises": [
          {
            "id": 101,
            "exerciseId": 1,
            "exerciseName": "Barbell Bench Press",
            "youTubeVideoId": "rT7DgCr-3pg",
            "section": "Main",
            "orderIndex": 0,
            "targetSets": 4,
            "targetReps": "8-12",
            "restSeconds": 90,
            "progressiveOverloadTargetKg": null
          },
          {
            "id": 102,
            "exerciseId": 3,
            "exerciseName": "Overhead Press",
            "youTubeVideoId": null,
            "section": "Main",
            "orderIndex": 1,
            "targetSets": 3,
            "targetReps": "10-12",
            "restSeconds": 60,
            "progressiveOverloadTargetKg": null
          }
        ]
      },
      {
        "id": 11,
        "dayNumber": 2,
        "dayLabel": "Pull Day 1",
        "isRestDay": false,
        "exercises": []
      },
      {
        "id": 16,
        "dayNumber": 7,
        "dayLabel": "Rest Day",
        "isRestDay": true,
        "exercises": []
      }
    ]
  }
  ```

**`section` values**: `"WarmUp"` | `"Main"` | `"CoolDown"`

**Frontend Usage**: `days` are sorted by `dayNumber` (1-7). Use to hydrate the 6-day canvas. The `isRestDay: true` day renders an empty "Rest" column.

---

### 3. Create Workout Template
Saves a complete new 6-day workout template. Called when the coach clicks **"Save Template"** in the Template Builder.

* **URL**: `/api/workout-templates`
* **Method**: `POST`
* **Authentication**: Required (`Coach` or `Admin` roles)
* **Request Body** (`CreateWorkoutTemplateForm`):
  ```json
  {
    "name": "PPL Phase 2 — Intermediate",
    "description": "Higher volume push/pull/legs split",
    "days": [
      {
        "dayNumber": 1,
        "dayLabel": "Push Day 1",
        "isRestDay": false,
        "exercises": [
          {
            "exerciseId": 1,
            "section": "WarmUp",
            "orderIndex": 0,
            "targetSets": 2,
            "targetReps": "15",
            "restSeconds": 45,
            "progressiveOverloadTargetKg": null
          },
          {
            "exerciseId": 1,
            "section": "Main",
            "orderIndex": 1,
            "targetSets": 4,
            "targetReps": "8-12",
            "restSeconds": 90,
            "progressiveOverloadTargetKg": 80.0
          }
        ]
      },
      {
        "dayNumber": 7,
        "dayLabel": "Rest Day",
        "isRestDay": true,
        "exercises": []
      }
    ]
  }
  ```
  | Field | Type | Rules |
  |-------|------|-------|
  | `name` | string | Required, max 200 chars |
  | `description` | string | Optional, max 1000 chars |
  | `days` | array | Required, 1-7 items. `dayNumber` must be unique (1-7) |
  | `days[].dayNumber` | integer | Required, 1-7 |
  | `days[].dayLabel` | string | Required, max 100 chars |
  | `days[].isRestDay` | boolean | Required |
  | `days[].exercises[].exerciseId` | integer | Required, must exist in library |
  | `days[].exercises[].section` | string | Required: `"WarmUp"`, `"Main"`, or `"CoolDown"` |
  | `days[].exercises[].orderIndex` | integer | Required, 0-based sort order within section |
  | `days[].exercises[].targetSets` | integer | Required, min 1 |
  | `days[].exercises[].targetReps` | string | Required, e.g. `"8-12"` or `"15"` |
  | `days[].exercises[].restSeconds` | integer | Optional |
  | `days[].exercises[].progressiveOverloadTargetKg` | decimal | Optional |

* **Response Body** (`201 Created` - `WorkoutTemplateDto`):
  Returns the full saved template (same shape as **Get by ID** response).

---

### 4. Update Workout Template
Replaces a template's metadata and all its days/exercises. Full replacement — send the complete updated structure.

* **URL**: `/api/workout-templates/{id}`
* **Method**: `PUT`
* **Authentication**: Required (`Coach` or `Admin` roles)
* **URL Parameter**: `{id}` — the template's integer ID
* **Request Body**: Same shape as **Create Workout Template**.
* **Response Body** (`200 OK` - `WorkoutTemplateDto`): Returns the fully updated template.

---

### 5. Assign Template to Athletes
Assigns a template to one or more athletes. Creates an active `ClientProgram` for each. If an athlete already has an active program, it is **deactivated** first.

* **URL**: `/api/workout-templates/{id}/assign`
* **Method**: `POST`
* **Authentication**: Required (`Coach` or `Admin` roles)
* **URL Parameter**: `{id}` — the template's integer ID to assign
* **Request Body** (`AssignTemplateForm`):
  ```json
  {
    "athleteIds": [5, 8, 11]
  }
  ```
  | Field | Type | Rules |
  |-------|------|-------|
  | `athleteIds` | integer[] | Required, at least 1 ID. All IDs must belong to the coach's roster. |

* **Response Body** (`200 OK`):
  ```json
  {
    "assignedCount": 3,
    "message": "Template assigned to 3 athlete(s) successfully."
  }
  ```

> [!IMPORTANT]
> After a `200` response, the assigned athletes' `GET /api/workouts/today` and `GET /api/workouts/program` endpoints will immediately reflect the new template. No further action required on the frontend.

**Frontend Usage**: Call after the coach clicks **"Assign to Athletes"**. Navigate back to the Client Roster on success.

---

## Food Admin Endpoints (`/api/foods/*`)

> [!NOTE]
> `GET /api/foods` (search) and `GET /api/foods/{id}` were built on **Day 2** and are available to all authenticated roles. The endpoints below are **admin-only mutations**.

---

### 1. Create Food Item
Creates a new food entry in the global ingredient database.

* **URL**: `/api/foods`
* **Method**: `POST`
* **Authentication**: Required (`Admin` role only)
* **Request Body** (`CreateFoodForm`):
  ```json
  {
    "name": "Cottage Cheese (Low Fat)",
    "category": "Dairy",
    "caloriesPer100g": 72.0,
    "proteinPer100g": 11.0,
    "carbsPer100g": 3.4,
    "fatPer100g": 1.0,
    "fiberPer100g": 0.0
  }
  ```
  | Field | Type | Rules |
  |-------|------|-------|
  | `name` | string | Required, max 200 chars |
  | `category` | string | Optional. Values: `"Protein"`, `"Carb"`, `"Fat"`, `"Dairy"`, `"Vegetable"`, `"Fruit"` |
  | `caloriesPer100g` | decimal | Required, min 0 |
  | `proteinPer100g` | decimal | Required, min 0 |
  | `carbsPer100g` | decimal | Required, min 0 |
  | `fatPer100g` | decimal | Required, min 0 |
  | `fiberPer100g` | decimal | Required, min 0 |

* **Response Body** (`201 Created` - `FoodDto`):
  ```json
  {
    "id": 53,
    "name": "Cottage Cheese (Low Fat)",
    "category": "Dairy",
    "caloriesPer100g": 72.0,
    "proteinPer100g": 11.0,
    "carbsPer100g": 3.4,
    "fatPer100g": 1.0,
    "fiberPer100g": 0.0,
    "isCustom": false
  }
  ```

---

### 2. Update Food Item
Updates an existing food entry.

* **URL**: `/api/foods/{id}`
* **Method**: `PUT`
* **Authentication**: Required (`Admin` role only)
* **URL Parameter**: `{id}` — the food's integer ID
* **Request Body**: Same shape as **Create Food Item** (`CreateFoodForm`).
* **Response Body** (`200 OK` - `FoodDto`): Returns the updated food object.

---

### 3. Delete Food Item (Soft-Delete)
Soft-deletes a food entry. Existing meal log entries referencing this food are **preserved**. The food no longer appears in athlete food search results.

* **URL**: `/api/foods/{id}`
* **Method**: `DELETE`
* **Authentication**: Required (`Admin` role only)
* **URL Parameter**: `{id}` — the food's integer ID
* **Request Body**: None
* **Response Body** (`204 No Content`): Empty response indicating success.

---

### 4. Bulk Import Foods (CSV)
Parses an uploaded CSV file and batch-inserts all valid food rows. Invalid rows are skipped and reported — the import does **not** fail if some rows are invalid.

* **URL**: `/api/foods/bulk-import`
* **Method**: `POST`
* **Authentication**: Required (`Admin` role only)
* **Content-Type**: `multipart/form-data`
* **Request**: Form field `csvFile` containing the `.csv` file.

**Expected CSV Format**:
```
Name,Category,CaloriesPer100g,ProteinPer100g,CarbsPer100g,FatPer100g,FiberPer100g
Chicken Breast,Protein,165,31,0,3.6,0
Brown Rice,Carb,216,4.5,45,1.8,1.8
Invalid Row,,9999,200,200,200,200
```

**Column rules**:
| Column | Required | Rule |
|--------|----------|------|
| `Name` | Yes | Non-empty string, max 200 chars |
| `Category` | No | Any string |
| `CaloriesPer100g` | Yes | Decimal >= 0 |
| `ProteinPer100g` | Yes | Decimal >= 0 |
| `CarbsPer100g` | Yes | Decimal >= 0 |
| `FatPer100g` | Yes | Decimal >= 0 |
| `FiberPer100g` | Yes | Decimal >= 0 |

**Validation rule**: A row is rejected if `(Protein x 4) + (Carbs x 4) + (Fat x 9) > 900 kcal/100g`.

* **Response Body** (`200 OK` - `BulkImportResultDto`):
  ```json
  {
    "insertedCount": 2,
    "skippedCount": 1,
    "errors": [
      "Row 3 (Invalid Row): Macro totals exceed 900 kcal/100g — rejected."
    ]
  }
  ```

**Frontend Usage** (Bulk Import Modal):
1. User drag-drops or selects a `.csv` file
2. Frontend shows column mapping preview (read CSV client-side before uploading)
3. On **"Import"** click: `POST /api/foods/bulk-import` as `multipart/form-data`
4. On `200` response: display summary toast:
   - Inserted: `insertedCount` foods imported
   - If `skippedCount > 0`: show `errors[]` in a scrollable list

---

## Invitation Management Endpoints (`/api/invitations/*`)

> [!NOTE]
> The invitation endpoints were started on **Day 1**. Day 5 confirms they are fully operational and wired to the Invitation Management screen.

---

### 1. List Invitations (Paginated)
Returns all invitations issued by the logged-in coach.

* **URL**: `/api/invitations`
* **Method**: `GET`
* **Authentication**: Required (`Coach` or `Admin` roles)
* **Query Parameters**:
  - `page` (integer, optional, defaults to `1`)
  - `pageSize` (integer, optional, defaults to `10`)
* **Response Body** (`200 OK` - `PagedResult<InvitationDto>`):
  ```json
  {
    "items": [
      {
        "id": 1,
        "email": "newathlete@jokernutrition.com",
        "token": "28f6c479e515446f9a3e206...",
        "role": "Athlete",
        "status": 0,
        "expiresAt": "2026-07-04T09:00:00Z",
        "createdAt": "2026-07-01T09:00:00Z",
        "inviteUrl": "http://localhost:5173/register?token=28f6c479e515..."
      },
      {
        "id": 2,
        "email": "coach2@jokernutrition.com",
        "token": "91a7d842b326559f0c4e...",
        "role": "Coach",
        "status": 2,
        "expiresAt": "2026-06-25T09:00:00Z",
        "createdAt": "2026-06-22T09:00:00Z",
        "inviteUrl": "http://localhost:5173/register?token=91a7d842b326..."
      }
    ],
    "totalCount": 2,
    "page": 1,
    "pageSize": 10
  }
  ```

---

### 2. Create and Send Invitation
Creates a new invitation link for the given email and role, and sends an invitation email.

* **URL**: `/api/invitations`
* **Method**: `POST`
* **Authentication**: Required (`Coach` or `Admin` roles)
* **Request Body** (`CreateInvitationForm`):
  ```json
  {
    "email": "newuser@jokernutrition.com",
    "role": "Athlete",
    "expiryHours": 72
  }
  ```
  | Field | Type | Rules |
  |-------|------|-------|
  | `email` | string | Required, valid email format |
  | `role` | string | Required: `"Athlete"`, `"Coach"`, or `"Admin"` |
  | `expiryHours` | integer | Optional, defaults to 72 hours |

* **Response Body** (`201 Created` - `InvitationDto`):
  Returns the created invitation including `inviteUrl` (used for the copy-link button).

---

### 3. Resend Invitation
Regenerates the token, resets status to `Pending`, extends expiry by 72 hours, and sends a new email.

* **URL**: `/api/invitations/resend/{id}`
* **Method**: `POST`
* **Authentication**: Required (`Coach` or `Admin` roles)
* **URL Parameter**: `{id}` — the invitation's integer ID
* **Request Body**: None
* **Response Body** (`200 OK` - `InvitationDto`):
  Returns the updated invitation with new token, reset status, and extended `expiresAt`.

**Frontend Usage**: Replace the row's data in local state with the returned `InvitationDto` — no need to refetch the full list.

---

### 4. Revoke Invitation
Cancels the invitation immediately. Status becomes `Revoked (3)`.

* **URL**: `/api/invitations/{id}`
* **Method**: `DELETE`
* **Authentication**: Required (`Coach` or `Admin` roles)
* **URL Parameter**: `{id}` — the invitation's integer ID
* **Request Body**: None
* **Response Body** (`204 No Content`): Empty response indicating success.

**Frontend Usage**: On `204`, set the row's status badge to `Revoked` in local state and disable the **Resend** and **Copy Link** buttons.

---

## Data Definitions

### `MuscleGroup` Enum Mapping
| String Value | Description |
|---|---|
| `"Chest"` | Chest exercises (bench press, flyes, cable crossovers) |
| `"Back"` | Back exercises (rows, pull-ups, deadlifts) |
| `"Shoulders"` | Shoulder exercises (OHP, lateral raises, rear delts) |
| `"Arms"` | Biceps and triceps exercises |
| `"Legs"` | Quad, hamstring, glute, calf exercises |
| `"Cardio"` | Cardio and conditioning exercises |
| `"Core"` | Abs and core stability exercises |

### `ExerciseSection` Enum Mapping
| String Value | Description |
|---|---|
| `"WarmUp"` | Warm-up exercises — light sets, mobility work |
| `"Main"` | Primary working sets — main compound and isolation movements |
| `"CoolDown"` | Cool-down — stretching, foam rolling |

### `InvitationStatus` Enum Mapping
| Integer | String | Meaning |
|---------|--------|---------|
| `0` | `Pending` | Active and valid — user can register |
| `1` | `Accepted` | User has registered using this invite |
| `2` | `Expired` | Token lifetime has passed |
| `3` | `Revoked` | Cancelled by Coach/Admin |

### `PagedResult<T>` Shape
All paginated endpoints return this envelope:
```json
{
  "items": [],
  "totalCount": 0,
  "page": 1,
  "pageSize": 20
}
```

### `FoodDto` Shape
```json
{
  "id": 53,
  "name": "Cottage Cheese (Low Fat)",
  "category": "Dairy",
  "caloriesPer100g": 72.0,
  "proteinPer100g": 11.0,
  "carbsPer100g": 3.4,
  "fatPer100g": 1.0,
  "fiberPer100g": 0.0,
  "isCustom": false
}
```

### `WorkoutTemplateSummaryDto` Shape (used in List response)
```json
{
  "id": 1,
  "name": "PPL Phase 1 — Beginner",
  "description": "6-day Push/Pull/Legs split",
  "coachName": "Marcus Steel",
  "dayCount": 7,
  "isActive": true,
  "createdAt": "2026-06-01T00:00:00Z"
}
```

---

## Error Responses

| HTTP Code | Scenario |
|-----------|---------|
| `400 Bad Request` | Validation failure (missing required field, macro total > 900, `athleteIds` is empty) |
| `401 Unauthorized` | Missing or expired JWT |
| `403 Forbidden` | Insufficient role (e.g., Athlete or Coach accessing Admin-only routes) |
| `404 Not Found` | Exercise, template, food, or invitation ID does not exist or is soft-deleted |
| `409 Conflict` | Attempting to create a food with a name that already exists |
| `413 Payload Too Large` | CSV file exceeds the 10 MB maximum size limit |
| `429 Too Many Requests` | Rate limit exceeded on auth endpoints |
