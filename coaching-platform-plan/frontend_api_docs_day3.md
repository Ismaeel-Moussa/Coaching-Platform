# Joker Nutrition Coaching Platform – API Reference for Frontend Team

**Day 3: Workout System & Supplements Tracker**  
**API Base URL**: `https://localhost:7001`  
All endpoint URLs below are relative to this base. All request and response bodies use the `application/json` format.

> [!NOTE]
> All endpoints in this document (except where noted) require the JWT in the request header:  
> `Authorization: Bearer <accessToken>`

---

## Exercise Endpoints (`/api/exercises/*`)

### 1. List Exercises (Paginated + Filtered)
Returns the full exercise library. Supports optional search by name and filter by muscle group.

* **URL**: `/api/exercises`
* **Method**: `GET`
* **Authentication**: Required (all roles)
* **Request Query Parameters**:
  - `search` (string, optional) — partial name match
  - `muscle` (integer, optional) — filter by muscle group enum value (see table below)
  - `page` (integer, optional, defaults to `1`)
  - `pageSize` (integer, optional, defaults to `20`)
* **Response Body** (`200 OK` - `PagedResult<ExerciseDto>`):
  ```json
  {
    "items": [
      {
        "id": 3,
        "name": "Barbell Bench Press",
        "instructions": "Lie flat, grip slightly wider than shoulder-width, lower bar to mid-chest and press up.",
        "primaryMuscle": "Chest",
        "equipmentRequired": "Barbell, Bench",
        "youTubeVideoId": "rT7DgCr-3pg"
      }
    ],
    "totalCount": 30,
    "page": 1,
    "pageSize": 20
  }
  ```

---

### 2. Get Single Exercise
* **URL**: `/api/exercises/{id}` (where `{id}` is the exercise's integer ID)
* **Method**: `GET`
* **Authentication**: Required (all roles)
* **Response Body** (`200 OK` - `ExerciseDto`):
  ```json
  {
    "id": 3,
    "name": "Barbell Bench Press",
    "instructions": "Lie flat, grip slightly wider than shoulder-width, lower bar to mid-chest and press up.",
    "primaryMuscle": "Chest",
    "equipmentRequired": "Barbell, Bench",
    "youTubeVideoId": "rT7DgCr-3pg"
  }
  ```

---

## Workout Endpoints (`/api/workouts/*`)

> [!NOTE]
> All workout endpoints require the `Athlete` role.

### 3. Get Today's Workout
Returns the current day's scheduled workout session based on the athlete's active PPL program.  
Also returns any sets already logged in today's session.  
If no program is assigned, returns `status: "NoProgram"`.

* **URL**: `/api/workouts/today`
* **Method**: `GET`
* **Authentication**: Required (`Athlete` role)
* **Response Body** (`200 OK` - `TodaysWorkoutDto`):
  ```json
  {
    "workoutLogId": 1,
    "status": "InProgress",
    "day": {
      "dayNumber": 1,
      "dayLabel": "Push Day 1",
      "isRestDay": false,
      "warmUp": [
        {
          "id": 1,
          "exercise": {
            "id": 27,
            "name": "Treadmill Walk",
            "instructions": "Moderate pace, 5-10 minutes as warm-up or active recovery.",
            "primaryMuscle": "Cardio",
            "equipmentRequired": "Treadmill",
            "youTubeVideoId": null
          },
          "section": "WarmUp",
          "orderIndex": 1,
          "targetSets": 1,
          "targetReps": "5 min",
          "restSeconds": null,
          "isSupersetWith": false,
          "progressiveOverloadTargetKg": null
        }
      ],
      "main": [
        {
          "id": 3,
          "exercise": {
            "id": 1,
            "name": "Barbell Bench Press",
            "instructions": "Lie flat, grip slightly wider than shoulder-width, lower bar to mid-chest and press up.",
            "primaryMuscle": "Chest",
            "equipmentRequired": "Barbell, Bench",
            "youTubeVideoId": "rT7DgCr-3pg"
          },
          "section": "Main",
          "orderIndex": 3,
          "targetSets": 4,
          "targetReps": "5",
          "restSeconds": 180,
          "isSupersetWith": false,
          "progressiveOverloadTargetKg": 80.0
        }
      ],
      "coolDown": [
        {
          "id": 10,
          "exercise": {
            "id": 30,
            "name": "Dead Bug",
            "instructions": "Lie flat, extend opposite arm/leg simultaneously while keeping lower back pressed.",
            "primaryMuscle": "Core",
            "equipmentRequired": "None",
            "youTubeVideoId": "n12gj-2-0XU"
          },
          "section": "CoolDown",
          "orderIndex": 10,
          "targetSets": 2,
          "targetReps": "10/side",
          "restSeconds": 30,
          "isSupersetWith": false,
          "progressiveOverloadTargetKg": null
        }
      ]
    },
    "loggedSets": [
      {
        "id": 1,
        "exerciseId": 1,
        "exerciseName": "Barbell Bench Press",
        "setNumber": 1,
        "weightKg": 75.0,
        "reps": 5,
        "isCompleted": true
      }
    ]
  }
  ```

---

### 4. Get Full 6-Day Program
Returns the athlete's complete 6-day PPL program structure for the week-grid view.

* **URL**: `/api/workouts/program`
* **Method**: `GET`
* **Authentication**: Required (`Athlete` role)
* **Response Body** (`200 OK` - `WorkoutProgramDto`):
  ```json
  {
    "templateId": 1,
    "templateName": "Joker 6-Day PPL",
    "description": "Push/Pull/Legs split designed for muscle hypertrophy and strength. Repeat twice per week.",
    "startDate": "2026-06-24T00:00:00Z",
    "days": [
      {
        "dayNumber": 1,
        "dayLabel": "Push Day 1",
        "isRestDay": false,
        "warmUp": [ ... ],
        "main": [ ... ],
        "coolDown": [ ... ]
      },
      {
        "dayNumber": 2,
        "dayLabel": "Pull Day 1",
        "isRestDay": false,
        "warmUp": [ ... ],
        "main": [ ... ],
        "coolDown": [ ... ]
      },
      { "dayNumber": 3, "dayLabel": "Legs Day 1", ... },
      { "dayNumber": 4, "dayLabel": "Push Day 2", ... },
      { "dayNumber": 5, "dayLabel": "Pull Day 2", ... },
      { "dayNumber": 6, "dayLabel": "Legs Day 2", ... }
    ]
  }
  ```

---

### 5. Log a Completed Set
Records one exercise set (weight + reps) within the current active workout session.

* **URL**: `/api/workouts/log-set`
* **Method**: `POST`
* **Authentication**: Required (`Athlete` role)
* **Request Body** (`LogSetForm`):
  ```json
  {
    "workoutLogId": 1,      // Required, integer — from GET /api/workouts/today
    "exerciseId": 1,        // Required, integer — the exercise being logged
    "setNumber": 1,         // Required, integer — 1-based set index
    "weightKg": 75.0,       // Required, decimal — weight used (0 for bodyweight)
    "reps": 5               // Required, integer — reps completed
  }
  ```
* **Response Body** (`200 OK` - `SetLogDto`):
  ```json
  {
    "id": 1,
    "exerciseId": 1,
    "exerciseName": "Barbell Bench Press",
    "setNumber": 1,
    "weightKg": 75.0,
    "reps": 5,
    "isCompleted": true
  }
  ```

---

### 6. Complete Today's Workout
Marks the full workout session as **Completed** and updates the athlete's streak counter.

* **URL**: `/api/workouts/complete`
* **Method**: `POST`
* **Authentication**: Required (`Athlete` role)
* **Request Body** (`CompleteWorkoutForm`):
  ```json
  {
    "workoutLogId": 1   // Required, integer — the active workout log ID
  }
  ```
* **Response Body** (`200 OK`):
  ```json
  {
    "message": "Workout completed! Streak updated."
  }
  ```
* **Streak Logic**: If the athlete also completed a workout yesterday → `currentStreak++`. Otherwise `currentStreak = 1`. If `currentStreak > longestStreak`, `longestStreak` is updated. The updated streak is visible on `GET /api/athletes/me/dashboard`.

---

### 7. Get Workout History
Returns per-exercise history sorted by date descending. Used by the frontend to show progressive overload trends.

* **URL**: `/api/workouts/history`
* **Method**: `GET`
* **Authentication**: Required (`Athlete` role)
* **Response Body** (`200 OK` - `List<WorkoutHistoryDto>`):
  ```json
  [
    {
      "exerciseId": 1,
      "exerciseName": "Barbell Bench Press",
      "sessions": [
        {
          "date": "2026-06-24",
          "sets": [
            { "id": 1, "exerciseId": 1, "exerciseName": "Barbell Bench Press", "setNumber": 1, "weightKg": 75.0, "reps": 5, "isCompleted": true },
            { "id": 2, "exerciseId": 1, "exerciseName": "Barbell Bench Press", "setNumber": 2, "weightKg": 75.0, "reps": 5, "isCompleted": true },
            { "id": 3, "exerciseId": 1, "exerciseName": "Barbell Bench Press", "setNumber": 3, "weightKg": 75.0, "reps": 4, "isCompleted": true }
          ]
        },
        {
          "date": "2026-06-21",
          "sets": [ ... ]
        }
      ]
    }
  ]
  ```

---

## Supplement Endpoints (`/api/supplements/*`)

### 8. Get Supplement Schedule
Returns all active supplements assigned to the logged-in athlete, including today's completion status for each.

* **URL**: `/api/supplements`
* **Method**: `GET`
* **Authentication**: Required (`Athlete` role)
* **Response Body** (`200 OK` - `List<SupplementDto>`):
  ```json
  [
    {
      "id": 1,
      "name": "Creatine Monohydrate",
      "type": "Essential",
      "dosage": "5g daily",
      "notes": "Mix with water or shake post-workout.",
      "isTakenToday": true,
      "takenAt": "2026-06-24T07:30:00Z"
    },
    {
      "id": 2,
      "name": "Omega-3 Fish Oil",
      "type": "Essential",
      "dosage": "2 caps (1g EPA/DHA)",
      "notes": "Take with a meal to reduce fishy aftertaste.",
      "isTakenToday": false,
      "takenAt": null
    },
    {
      "id": 3,
      "name": "Multivitamin",
      "type": "Essential",
      "dosage": "1 tablet daily",
      "notes": "Take with breakfast.",
      "isTakenToday": false,
      "takenAt": null
    },
    {
      "id": 4,
      "name": "Vitamin D3",
      "type": "Optional",
      "dosage": "5000 IU daily",
      "notes": "Best taken with a fat-containing meal for absorption.",
      "isTakenToday": false,
      "takenAt": null
    }
  ]
  ```

---

### 9. Toggle Supplement Taken
Toggles a supplement's `isTakenToday` status on/off.  
- If not yet logged → creates a `SupplementLog` with `isTaken: true`  
- If already taken → flips to `isTaken: false` (untoggle)

* **URL**: `/api/supplements/log`
* **Method**: `POST`
* **Authentication**: Required (`Athlete` role)
* **Request Body** (`ToggleSupplementForm`):
  ```json
  {
    "supplementScheduleId": 1,  // Required, integer
    "date": "2026-06-24"        // Required, DateOnly (ISO format YYYY-MM-DD)
  }
  ```
* **Response Body** (`200 OK` - `SupplementDto`):
  ```json
  {
    "id": 1,
    "name": "Creatine Monohydrate",
    "type": "Essential",
    "dosage": "5g daily",
    "notes": "Mix with water or shake post-workout.",
    "isTakenToday": true,
    "takenAt": "2026-06-24T08:15:00Z"
  }
  ```

---

### 10. Assign Supplement to Athlete (Coach/Admin)
Creates a new supplement schedule item for a specified athlete.

* **URL**: `/api/supplements/schedule`
* **Method**: `POST`
* **Authentication**: Required (`Coach` or `Admin` role)
* **Request Body** (`AssignSupplementForm`):
  ```json
  {
    "athleteId": 1,               // Required, integer — target athlete's ID
    "name": "Magnesium Glycinate",// Required, string
    "type": 1,                    // Required, integer — SupplementType enum (see table below)
    "dosage": "400mg before bed", // Optional, string
    "notes": "Helps with sleep quality and muscle recovery." // Optional, string
  }
  ```
* **Response Body** (`201 Created` - `SupplementDto`):
  ```json
  {
    "id": 5,
    "name": "Magnesium Glycinate",
    "type": "Optional",
    "dosage": "400mg before bed",
    "notes": "Helps with sleep quality and muscle recovery.",
    "isTakenToday": false,
    "takenAt": null
  }
  ```

---

## Data Definitions

### MuscleGroup Enum Mapping
The `muscle` query parameter accepts an integer value:

| Integer | String | Description |
|---------|--------|-------------|
| `0` | `Chest` | Chest exercises |
| `1` | `Back` | Back exercises |
| `2` | `Shoulders` | Shoulder exercises |
| `3` | `Arms` | Biceps & triceps exercises |
| `4` | `Legs` | Legs, glutes & calves |
| `5` | `Cardio` | Cardio & conditioning |
| `6` | `Core` | Abs & core stability |

### ExerciseSection Enum Mapping
Exercises within a day are grouped by section:

| String | Description |
|--------|-------------|
| `"WarmUp"` | Warm-up exercises performed before main lifts |
| `"Main"` | Primary working sets |
| `"CoolDown"` | Cool-down / mobility / core finishers |

### WorkoutStatus Enum Mapping
Returned in `TodaysWorkoutDto.status`:

| String | Description |
|--------|-------------|
| `"InProgress"` | Session started, not yet marked complete |
| `"Completed"` | Athlete tapped "Complete Workout" |
| `"Missed"` | Workout day passed without completion |
| `"NoProgram"` | No active workout program assigned to this athlete |

### SupplementType Enum Mapping
Used in `AssignSupplementForm.type` (send as integer):

| Integer | String | Description |
|---------|--------|-------------|
| `0` | `Essential` | Mandatory daily supplement (large checkbox in UI) |
| `1` | `Optional` | Optional supplement with conditional toggle |

---

## PPL Program: Day Label Mapping

The seeded demo program ("Joker 6-Day PPL") rotates on a 6-day cycle from the program start date:

| Day Number | Label | Focus |
|-----------|-------|-------|
| 1 | Push Day 1 | Chest (Bench Press), Shoulders (OHP), Triceps |
| 2 | Pull Day 1 | Back (Pull-Up, Row), Biceps |
| 3 | Legs Day 1 | Squat, Romanian Deadlift, Leg Press, Calves |
| 4 | Push Day 2 | Shoulders (Arnold Press), Chest (Incline, Fly), Triceps |
| 5 | Pull Day 2 | Back (DB Row, Cable Row), Biceps |
| 6 | Legs Day 2 | Squat, Lunges, Leg Press, RDL, Calves |

> Cycle repeats every 6 days. Day 7+ wraps back to Day 1 automatically.

---

## Demo Credentials

Use these seeded accounts to test the endpoints in Swagger or Postman:

| Role | Email | Password |
|------|-------|----------|
| Admin | `admin@jokernutrition.com` | `Admin@Joker123!` |
| Coach | `coach@jokernutrition.com` | `Coach@Joker123!` |
| Athlete | `athlete@jokernutrition.com` | `Athlete@Joker123!` |

> Authenticate via `POST /api/auth/login` → copy `accessToken` → use as `Bearer <token>` in Authorization header for all protected endpoints.

---

## Seeded Data (Day 3)

| Data | Count |
|------|-------|
| Exercises | 30 (Chest ×5, Back ×5, Shoulders ×4, Arms ×5, Legs ×6, Cardio ×2, Core ×2 + Band Pull-Apart) |
| Workout Template | 1 — "Joker 6-Day PPL" |
| Template Days | 6 days (Push Day 1 & 2, Pull Day 1 & 2, Legs Day 1 & 2) |
| Template Exercises | ~60 (10-11 per day across WarmUp/Main/CoolDown) |
| Client Program | 1 — demo athlete assigned to the PPL template |
| Supplement Schedules | 4 (Creatine, Omega-3, Multivitamin — Essential; Vitamin D3 — Optional) |
