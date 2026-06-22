# Joker Nutrition Coaching Platform – API Reference for Frontend Team (Day 2)

**API Base URL**: `https://localhost:7001`  
All endpoint URLs below are relative to this base. All request and response bodies use the `application/json` format.

> [!NOTE]
> All endpoints in this document (except where marked **Public**) require the JWT in the request header:  
> `Authorization: Bearer <accessToken>`

---

## Food Endpoints (`/api/foods/*`)

### 1. Search Foods
Returns a paginated list of foods matching the search query and/or category filter.

* **URL**: `/api/foods`
* **Method**: `GET`
* **Authentication**: Required (any role)
* **Query Parameters**:
  - `search` (string, optional) — partial match on food name, e.g. `?search=chicken`
  - `category` (string, optional) — exact match: `"Protein"` | `"Carbs"` | `"Fat"` | `"Vegetable"` | `"Dairy"`
  - `page` (integer, optional, default `1`)
  - `pageSize` (integer, optional, default `20`)
* **Response Body** (`200 OK` - `PagedResult<FoodDto>`):
  ```json
  {
    "items": [
      {
        "id": 1,
        "name": "Chicken Breast (Skinless)",
        "category": "Protein",
        "caloriesPer100g": 165.0,
        "proteinPer100g": 31.0,
        "carbsPer100g": 0.0,
        "fatPer100g": 3.6,
        "fiberPer100g": 0.0,
        "isCustom": false
      }
    ],
    "totalCount": 53,
    "page": 1,
    "pageSize": 20,
    "totalPages": 3,
    "hasNextPage": true,
    "hasPreviousPage": false
  }
  ```

---

### 2. Get Food By ID
Returns full macro-per-100g data for a single food.

* **URL**: `/api/foods/{id}` (where `{id}` is an integer)
* **Method**: `GET`
* **Authentication**: Required (any role)
* **Response Body** (`200 OK` - `FoodDto`):
  ```json
  {
    "id": 1,
    "name": "Chicken Breast (Skinless)",
    "category": "Protein",
    "caloriesPer100g": 165.0,
    "proteinPer100g": 31.0,
    "carbsPer100g": 0.0,
    "fatPer100g": 3.6,
    "fiberPer100g": 0.0,
    "isCustom": false
  }
  ```

---

## Diary Endpoints (`/api/diary/*`)

> [!IMPORTANT]
> All `/api/diary/*` endpoints require the `Athlete` role.

### 1. Get Full Diary for a Date
Returns the complete diary for a given date, with all meal logs grouped by meal type (Breakfast, Lunch, Dinner, Snack) plus daily totals.

* **URL**: `/api/diary/{date}` (where `{date}` is ISO format `YYYY-MM-DD`, e.g. `2026-06-22`)
* **Method**: `GET`
* **Authentication**: Required (`Athlete` role)
* **Response Body** (`200 OK` - `DailyDiaryDto`):
  ```json
  {
    "id": 42,
    "date": "2026-06-22",
    "targetCalories": 1800.0,
    "targetProtein": 160.0,
    "targetCarbs": 180.0,
    "targetFat": 50.0,
    "waterLitersConsumed": 2.5,
    "waterLitersTarget": 4.0,
    "stepsWalked": 4200,
    "stepsTarget": 8000,
    "totalCalories": 850.0,
    "totalProtein": 78.0,
    "totalCarbs": 90.0,
    "totalFat": 22.0,
    "breakfast": [
      {
        "id": 101,
        "mealType": 0,
        "food": { "id": 17, "name": "Rolled Oats (Dry)", "category": "Carbs" },
        "recipe": null,
        "quantityGrams": 80.0,
        "state": 0,
        "calories": 311.2,
        "protein": 13.6,
        "carbs": 52.8,
        "fat": 5.6,
        "loggedAt": "2026-06-22T06:32:00Z"
      }
    ],
    "lunch": [],
    "dinner": [],
    "snack": [],
    "suhoor": [],
    "iftar": [],
    "preWorkout": [],
    "postWorkout": []
  }
  ```

---

### 2. Get Macro Summary for a Date
Returns aggregated macro totals vs. targets — the data for the Dashboard macro progress bars.

* **URL**: `/api/diary/summary/{date}`
* **Method**: `GET`
* **Authentication**: Required (`Athlete` role)
* **Response Body** (`200 OK` - `MacroSummaryDto`):
  ```json
  {
    "date": "2026-06-22",
    "caloriesConsumed": 850.0,
    "proteinConsumed": 78.0,
    "carbsConsumed": 90.0,
    "fatConsumed": 22.0,
    "targetCalories": 1800.0,
    "targetProtein": 160.0,
    "targetCarbs": 180.0,
    "targetFat": 50.0,
    "caloriesRemaining": 950.0,
    "proteinRemaining": 82.0,
    "carbsRemaining": 90.0,
    "fatRemaining": 28.0,
    "waterLitersConsumed": 2.5,
    "waterLitersTarget": 4.0,
    "stepsWalked": 4200,
    "stepsTarget": 8000
  }
  ```
  *(Note: `caloriesRemaining`, `proteinRemaining`, `carbsRemaining`, `fatRemaining` are computed properties — they will be negative if over the target.)*

---

### 3. Log a Food or Recipe Entry
Logs a food or recipe to the diary for a given date and meal type. Macros are automatically calculated from the food database using the quantity and cooking state.

* **URL**: `/api/diary/log`
* **Method**: `POST`
* **Authentication**: Required (`Athlete` role)
* **Request Body** (`LogFoodForm`):
  ```json
  {
    "date": "2026-06-22",          // Required, ISO date
    "mealType": 0,                  // Required, integer (see MealType enum below)
    "foodId": 1,                    // Provide EITHER foodId OR recipeId (not both)
    "recipeId": null,               // null when logging a food directly
    "quantityGrams": 200.0,         // Required, decimal > 0
    "state": 0                      // Required, integer (see FoodState enum below)
  }
  ```
* **Response Body** (`201 Created` - `MealLogDto`):
  ```json
  {
    "id": 101,
    "mealType": 0,
    "food": { "id": 1, "name": "Chicken Breast (Skinless)", "category": "Protein" },
    "recipe": null,
    "quantityGrams": 200.0,
    "state": 0,
    "calories": 330.0,
    "protein": 62.0,
    "carbs": 0.0,
    "fat": 7.2,
    "loggedAt": "2026-06-22T08:45:00Z"
  }
  ```

---

### 4. Remove a Log Entry
Deletes a specific meal log entry from the diary. Only the owning athlete's entries can be deleted.

* **URL**: `/api/diary/log/{id}` (where `{id}` is the meal log integer ID)
* **Method**: `DELETE`
* **Authentication**: Required (`Athlete` role)
* **Response Body** (`204 No Content`): Empty.

---

### 5. Update Water Consumed
Updates the water intake for the day (in liters). Send the new total, not a delta.

* **URL**: `/api/diary/{date}/water`
* **Method**: `PATCH`
* **Authentication**: Required (`Athlete` role)
* **Request Body** (`UpdateWaterForm`):
  ```json
  { "waterLiters": 2.5 }
  ```
* **Response Body** (`204 No Content`): Empty.

---

### 6. Update Steps Walked
Updates the step count for the day. Send the current total, not a delta.

* **URL**: `/api/diary/{date}/steps`
* **Method**: `PATCH`
* **Authentication**: Required (`Athlete` role)
* **Request Body** (`UpdateStepsForm`):
  ```json
  { "steps": 6200 }
  ```
* **Response Body** (`204 No Content`): Empty.

---

## Recipe Endpoints (`/api/recipes/*`)

### 1. List Recipes
Returns a paginated recipe list, optionally filtered by category. Joker-curated recipes always appear first.

* **URL**: `/api/recipes`
* **Method**: `GET`
* **Authentication**: Required (any role)
* **Query Parameters**:
  - `category` (integer, optional) — `0` = MuscleBuilding, `1` = FatLoss, `2` = Custom (omit for all)
  - `page` (integer, optional, default `1`)
  - `pageSize` (integer, optional, default `20`)
* **Response Body** (`200 OK` - `PagedResult<RecipeDto>`):
  ```json
  {
    "items": [
      {
        "id": 1,
        "name": "Joker Classic Muscle Builder",
        "description": "High-protein clean bulk meal: grilled chicken, white rice, and steamed broccoli.",
        "category": 0,
        "prepTimeMinutes": 10,
        "cookTimeMinutes": 25,
        "servings": 1,
        "isJokerRecipe": true,
        "totalCalories": 575.0,
        "totalProtein": 55.0,
        "totalCarbs": 65.0,
        "totalFat": 8.0,
        "createdAt": "2026-06-22T00:00:00Z",
        "ingredients": []
      }
    ],
    "totalCount": 5,
    "page": 1,
    "pageSize": 20,
    "totalPages": 1,
    "hasNextPage": false,
    "hasPreviousPage": false
  }
  ```

---

### 2. Get Recipe By ID
Returns a single recipe with its full ingredient list, including per-ingredient macro breakdown.

* **URL**: `/api/recipes/{id}`
* **Method**: `GET`
* **Authentication**: Required (any role)
* **Response Body** (`200 OK` - `RecipeDto`):
  ```json
  {
    "id": 1,
    "name": "Joker Classic Muscle Builder",
    "description": "High-protein clean bulk meal: grilled chicken, white rice, and steamed broccoli.",
    "category": 0,
    "prepTimeMinutes": 10,
    "cookTimeMinutes": 25,
    "servings": 1,
    "isJokerRecipe": true,
    "totalCalories": 575.0,
    "totalProtein": 55.0,
    "totalCarbs": 65.0,
    "totalFat": 8.0,
    "createdAt": "2026-06-22T00:00:00Z",
    "ingredients": [
      {
        "foodId": 1,
        "foodName": "Chicken Breast (Skinless)",
        "quantityGrams": 200.0,
        "state": 0,
        "calories": 330.0,
        "protein": 62.0,
        "carbs": 0.0,
        "fat": 7.2
      },
      {
        "foodId": 15,
        "foodName": "White Rice (Dry)",
        "quantityGrams": 80.0,
        "state": 2,
        "calories": 195.0,
        "protein": 5.7,
        "carbs": 52.0,
        "fat": 0.4
      },
      {
        "foodId": 37,
        "foodName": "Broccoli",
        "quantityGrams": 150.0,
        "state": 0,
        "calories": 51.0,
        "protein": 4.2,
        "carbs": 10.5,
        "fat": 0.6
      }
    ]
  }
  ```

---

### 3. Create Custom Recipe
Creates a new custom recipe. The API auto-calculates all total macros from the ingredient list.

* **URL**: `/api/recipes`
* **Method**: `POST`
* **Authentication**: Required (`Athlete` or `Admin` role)
* **Request Body** (`CreateRecipeForm`):
  ```json
  {
    "name": "My High Protein Wrap",              // Required, string
    "description": "Quick post-workout meal.",   // Optional, string
    "category": 2,                               // Required, RecipeCategory enum integer
    "prepTimeMinutes": 5,                        // Required, integer
    "cookTimeMinutes": 0,                        // Required, integer
    "servings": 1,                               // Required, integer (min 1)
    "ingredients": [
      {
        "foodId": 1,          // Required, integer (must exist in food catalog)
        "quantityGrams": 150, // Required, decimal
        "state": 0            // Required, FoodState enum integer
      },
      {
        "foodId": 23,
        "quantityGrams": 60,
        "state": 0
      }
    ]
  }
  ```
* **Response Body** (`201 Created` - `RecipeDto`):
  Returns the created recipe with auto-calculated `totalCalories`, `totalProtein`, `totalCarbs`, `totalFat` and full ingredient breakdown.

---

### 4. Quick-Add Recipe to Today's Diary
Logs an entire recipe as a single entry in today's diary under the specified meal type.

* **URL**: `/api/recipes/{id}/add-to-diary`
* **Method**: `POST`
* **Authentication**: Required (`Athlete` role)
* **Query Parameters**:
  - `mealType` (integer, optional, default `1` = Lunch)
* **Response Body** (`200 OK` - `DailyDiaryDto`):
  Returns the full updated diary for today, reflecting the newly added recipe entry.

---

## Athlete Endpoints (`/api/athletes/*`)

### 1. Get Athlete Dashboard
The primary data source for the Customer Dashboard screen. Returns today's macro summary, streak info, and workout status.

* **URL**: `/api/athletes/me/dashboard`
* **Method**: `GET`
* **Authentication**: Required (`Athlete` role)
* **Response Body** (`200 OK` - `AthleteDashboardDto`):
  ```json
  {
    "athlete": {
      "id": 1,
      "firstName": "Sarah",
      "lastName": "Lopez",
      "currentStreak": 3,
      "longestStreak": 7,
      "targetGoal": "Fat Loss",
      "profilePictureUrl": null
    },
    "today": {
      "date": "2026-06-22",
      "caloriesConsumed": 850.0,
      "proteinConsumed": 78.0,
      "carbsConsumed": 90.0,
      "fatConsumed": 22.0,
      "targetCalories": 1800.0,
      "targetProtein": 160.0,
      "targetCarbs": 180.0,
      "targetFat": 50.0,
      "caloriesRemaining": 950.0,
      "proteinRemaining": 82.0,
      "carbsRemaining": 90.0,
      "fatRemaining": 28.0,
      "waterLitersConsumed": 0.0,
      "waterLitersTarget": 4.0,
      "stepsWalked": 0,
      "stepsTarget": 8000
    },
    "todaysWorkoutStatus": "NoProgram"
  }
  ```
  *(Note: `todaysWorkoutStatus` values: `"NoProgram"` | `"InProgress"` | `"Completed"` | `"Missed"`)*

---

### 2. Get My Active Macro Targets
Returns the current athlete's coach-assigned macro targets.

* **URL**: `/api/athletes/me/targets`
* **Method**: `GET`
* **Authentication**: Required (`Athlete` role)
* **Response Body** (`200 OK` - `MacroTargetDto`):
  ```json
  {
    "id": 1,
    "targetCalories": 1800.0,
    "targetProtein": 160.0,
    "targetCarbs": 180.0,
    "targetFat": 50.0,
    "waterLitersTarget": 4.0,
    "stepsTarget": 8000,
    "setAt": "2026-06-22T00:00:00Z",
    "setByCoachName": "Marcus Steel"
  }
  ```
  *(Note: Returns `404` if no active target has been set by the coach yet.)*

---

### 3. Get Athlete Targets (Coach/Admin)
Allows coaches to view the active macro targets for any specific athlete.

* **URL**: `/api/athletes/{id}/targets` (where `{id}` is the athlete's integer ID)
* **Method**: `GET`
* **Authentication**: Required (`Coach` or `Admin` role)
* **Response Body** (`200 OK` - `MacroTargetDto`): Same shape as above.

---

### 4. Set Macro Targets for Athlete (Coach/Admin)
Coach creates a new macro target for an athlete. **Automatically deactivates any previously active target** before creating the new one.

* **URL**: `/api/athletes/{id}/targets`
* **Method**: `POST`
* **Authentication**: Required (`Coach` or `Admin` role)
* **Request Body** (`SetMacroTargetForm`):
  ```json
  {
    "targetCalories": 2200.0,     // Required, decimal
    "targetProtein": 180.0,       // Required, decimal
    "targetCarbs": 240.0,         // Required, decimal
    "targetFat": 60.0,            // Required, decimal
    "waterLitersTarget": 4.0,     // Optional, decimal (defaults to 4.0)
    "stepsTarget": 8000           // Optional, integer (defaults to 7000)
  }
  ```
* **Response Body** (`201 Created` - `MacroTargetDto`):
  Returns the newly created active macro target.

---

## Data Definitions

### MealType Enum Mapping
The `mealType` field is an integer representation of the C# `enum`:
* `0` = **Breakfast**
* `1` = **Lunch**
* `2` = **Dinner**
* `3` = **Snack**
* `4` = **Suhoor** *(Ramadan Mode)*
* `5` = **Iftar** *(Ramadan Mode)*
* `6` = **PreWorkout** *(Ramadan Mode)*
* `7` = **PostWorkout** *(Ramadan Mode)*

---

### FoodState Enum Mapping
The `state` field controls macro calculation conversion factors:
* `0` = **Raw** — macros applied directly (1:1 from per-100g values)
* `1` = **Cooked** — applies a 1.33× correction (cooked weight is lighter than raw)
* `2` = **Dry** — applies a 2.5× expansion factor (dry oats/rice absorb water when cooked)

---

### RecipeCategory Enum Mapping
* `0` = **MuscleBuilding**
* `1` = **FatLoss**
* `2` = **Custom**

---

## Seeded Demo Credentials

| Role    | Email                         | Password          |
|---------|-------------------------------|-------------------|
| Admin   | admin@jokernutrition.com      | Admin@Joker123!   |
| Coach   | coach@jokernutrition.com      | Coach@Joker123!   |
| Athlete | athlete@jokernutrition.com    | Athlete@Joker123! |

**Seeded data available after first run:**
- **53 foods** across 5 categories (Protein, Carbs, Fat, Vegetable, Dairy)
- **5 Joker-curated recipes** (3 MuscleBuilding, 2 FatLoss)
- **1 active MacroTarget** for the demo athlete (1800 kcal / 160g P / 180g C / 50g F)
