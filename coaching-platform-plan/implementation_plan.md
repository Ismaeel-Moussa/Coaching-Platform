# Implementation Plan: Joker Nutrition Platform — 7-Day Delivery Roadmap

**Project**: Joker Nutrition Coaching Platform  
**Tech Stack**: Vite + React + Tailwind CSS (Frontend) + ASP.NET Core Web API + SQL Server (Backend)  
**Screens to Implement**: 19 UI screens mapped from Google Stitch  
**Team Size**: 2 Developers (Dev A = Frontend, Dev B = Backend)

---

> [!IMPORTANT]
> Each day is an **atomic delivery unit**. By the end of each day, the feature should be fully functional end-to-end — frontend component + API endpoint + database schema — not partially wired. Every day ends with a smoke test and a mini-demo to confirm progress.

---

## 📁 Proposed Workspace Structure

```
Coach-Project/
├── frontend/                     # Vite + React + Tailwind
│   ├── src/
│   │   ├── assets/              # Icons, images, seeded data
│   │   ├── components/          # Reusable UI components
│   │   ├── context/             # AppContext (role + state)
│   │   ├── pages/               # Route-level page components
│   │   │   ├── auth/
│   │   │   ├── athlete/
│   │   │   └── coach/
│   │   ├── services/            # API call wrappers (axios)
│   │   ├── hooks/               # Custom React hooks
│   │   └── utils/               # Calorie calculators, formatters
│   ├── tailwind.config.js       # Joker Nutrition design tokens
│   └── vite.config.js
│
└── backend/                      # ASP.NET Core Web API
    ├── JokerNutrition.API/
    │   ├── Controllers/         # Route controllers
    │   ├── Data/                # DbContext + EF migrations
    │   ├── Models/              # Entity models
    │   ├── DTOs/                # Request/response shapes
    │   ├── Services/            # Business logic
    │   └── Middleware/          # Auth, error handling
    └── JokerNutrition.sln
```

---

## 🗓️ Day 1 — Foundation, Design System & Authentication

**Goal**: Both projects are initialized, connected, and the full authentication flow works end-to-end.

### ✅ Frontend (Dev A)
| Task | Description |
|------|-------------|
| Scaffold Vite + React | `npm create vite@latest frontend -- --template react` |
| Install Tailwind CSS | Configure `tailwind.config.js` with all Joker Nutrition custom tokens (Navy `#0b132b`, Gold `#fdc003`, Red `#ba1a1a`, Surface `#f9f9ff`) |
| Google Fonts setup | Import `Archivo Narrow`, `Inter`, `JetBrains Mono` via Google Fonts link in `index.html` |
| Material Symbols | Add Google Material Symbols Outlined icon font |
| React Router | Install and configure `react-router-dom` v6 with route guards based on user role |
| `AppContext.jsx` | Build shared context with `currentUser`, `currentRole` (athlete / coach / admin), and auth token |
| **Screen: Login Page** | Implement `Joker Nutrition - Login` screen — navy header, email + password fields, validation states in Action Red, "Sign In" button in Gold |
| **Screen: Join the Team** | Implement `Joker Nutrition - Join the Team` onboarding form — baseline measurements, goal selection, coach assignment |
| Axios service layer | Create `services/api.js` base Axios instance pointing to `https://localhost:7001/api` |

### ✅ Backend (Dev B)
| Task | Description |
|------|-------------|
| Scaffold ASP.NET Core Web API | `dotnet new webapi -n JokerNutrition.API` with .NET 8 |
| Install Entity Framework Core | `Microsoft.EntityFrameworkCore.SqlServer`, `EF Tools` |
| `ApplicationDbContext.cs` | Configure DB context |
| Database Migrations | Create initial migration: `Users`, `Roles`, `RefreshTokens` tables |
| **Auth: JWT setup** | Register JWT Bearer middleware, configure `appsettings.json` secrets |
| **Auth: Register endpoint** | `POST /api/auth/register` — validate invitation token, hash password, create user with role |
| **Auth: Login endpoint** | `POST /api/auth/login` — verify credentials, return JWT + refresh token in HttpOnly cookie |
| **Auth: Refresh Token endpoint** | `POST /api/auth/refresh` — rotate refresh token securely |
| **Invitation: Create invite** | `POST /api/invitations` — generate invite link with role, expiry time |
| CORS config | Allow `http://localhost:5173` (Vite dev server) |

### 🔌 Day 1 Integration Milestone
- Login form submits to the real API → receives JWT token
- JWT is stored in memory context and sent on every subsequent request
- Unauthenticated users are redirected to `/login`
- Role-based routing: Athletes land on `/athlete/dashboard`, Coaches on `/coach/dashboard`

### 📋 Day 1 Deliverables
- [ ] Vite + React project runs on `localhost:5173`
- [ ] ASP.NET Core API runs on `localhost:7001/swagger`
- [ ] SQL Server database created with `Users`, `Roles` tables
- [ ] Login form authenticates against live API
- [ ] JWT authentication protects all subsequent endpoints

---

## 🗓️ Day 2 — Athlete Dashboard & Nutrition Logging

**Goal**: Athletes can see their daily overview and log meals with real-time macro tracking.

### ✅ Frontend (Dev A)
| Task | Description |
|------|-------------|
| Layout shell | Build `AthleteLayout.jsx` — Left sidebar nav (Dashboard, Workouts, Nutrition, Supplements, Check-ins), responsive top bar for mobile |
| **Screen: Customer Dashboard** | Bento grid: Daily Macros card (Protein/Carbs/Fats progress bars in Gold), Today's Session card (Navy), Daily Targets card (hydration ring + steps ring), 14-Day Streak indicator |
| `MacroProgressBar.jsx` | Reusable component: labelled progress bar, turns Red when target exceeded |
| `RingProgress.jsx` | SVG ring component for hydration and steps |
| **Screen: Meal Logger** | Section tabs (Breakfast, Lunch, Dinner, Snack), food item rows with macro columns (P/C/F/kcal), per-meal subtotals |
| **Screen: Add Food Modal** | Search input, ingredient results list, quantity input (grams), state selector (Raw/Cooked/Dry), instant macro preview, "Add to Diary" button |
| **Screen: Recipe Library** | Tabbed card grid (Muscle Building, Fat Loss, Custom), recipe cards with cook time, macro badges, "Quick Add" shortcut |
| **Screen: Create Recipe Modal** | 3-step wizard: Select Ingredients → Configure Portions → Preview Cumulative Macros → Save |

### ✅ Backend (Dev B)
| Task | Description |
|------|-------------|
| Database schemas | `Foods` (name, per100gCalories, protein, carbs, fat, category), `MealLogs`, `DailyDiaries`, `Recipes`, `RecipeIngredients` |
| Seed data | 50+ foods (Chicken Breast, Oats, White Rice, Eggs, Tuna, Cottage Cheese, etc.) with accurate macro data |
| `GET /api/foods` | Search foods by name, filter by category |
| `GET /api/foods/{id}` | Single food details |
| `GET /api/diary/{date}` | Get athlete's full diary for a given date |
| `POST /api/diary/log` | Log a food item to meal type (breakfast/lunch/dinner/snack) |
| `DELETE /api/diary/log/{id}` | Remove a food entry |
| `GET /api/diary/summary/{date}` | Return total macros consumed vs. target for the day |
| `GET /api/recipes` | List recipes with filter tabs |
| `POST /api/recipes` | Create custom recipe |
| `POST /api/recipes/{id}/add-to-diary` | Quick-add full recipe to today's diary |
| Macro targets | `GET /api/athletes/{id}/targets` — daily macro + calorie targets set by coach |

### 🔌 Day 2 Integration Milestone
- Dashboard loads today's real macro data from API
- Meal Logger lists all food items from API database
- Add Food Modal searches real food catalog and logs to DB
- Macro bar updates instantly after logging a meal

### 📋 Day 2 Deliverables
- [ ] Athlete Dashboard renders with live data from API
- [ ] Full Meal Logger with Add Food Modal working
- [ ] Recipe Library with Create Recipe modal working
- [ ] All macros correctly calculated from API responses

---

## 🗓️ Day 3 — Workout System & Supplements Tracker

**Goal**: Athletes can view their 6-day PPL program, log workouts, and track daily supplements.

### ✅ Frontend (Dev A)
| Task | Description |
|------|-------------|
| **Screen: Workout Logger** | 6-day tab selector (Push Day 1, Pull Day 1, Legs Day 1, Push Day 2, Pull Day 2, Legs Day 2 + Rest), exercise cards grouped into Warm-up / Main / Cool-down sections |
| `ExerciseCard.jsx` | Displays exercise name, target sets/reps, weight input, reps input, a video play icon (opens demo modal), and completion checkbox |
| Video Demo Modal | Embeds YouTube iframe inside a `Modal` overlay triggered on play icon click |
| Workout completion | Tapping "Complete Workout" marks the session as done and returns user to dashboard with streak updated |
| **Screen: Supplements Tracker** | Two-column list: Essential supplements with large checkboxes, Optional supplements with conditional toggle, daily reset at midnight |
| `SupplementCheckbox.jsx` | Checkbox styled in Joker Gold with label font in JetBrains Mono |

### ✅ Backend (Dev B)
| Task | Description |
|------|-------------|
| Database schemas | `Exercises`, `WorkoutRoutines`, `RoutineExercises`, `ClientPrograms`, `WorkoutLogs`, `ExerciseSetLogs` |
| Exercise seed data | 30+ exercises across Chest, Back, Shoulders, Arms, Legs categories with YouTube video hashes |
| `GET /api/workouts/program/{athleteId}` | Return the assigned 6-day split with exercise list per day |
| `GET /api/workouts/today` | Return today's scheduled workout for the current athlete |
| `POST /api/workouts/log-set` | Log a completed set (exerciseId, weight, reps) |
| `POST /api/workouts/complete` | Mark today's full workout as done, update streak counter |
| `GET /api/workouts/history/{athleteId}` | Exercise history for progressive overload tracking |
| Database schemas | `SupplementSchedules`, `SupplementLogs` |
| `GET /api/supplements` | Get athlete's supplement schedule with today's completion status |
| `POST /api/supplements/log` | Toggle supplement as taken for today |

### 🔌 Day 3 Integration Milestone
- Workout Logger renders today's workout from API
- Weight and reps can be typed and saved to DB
- "Complete" button marks workout and updates streak on Dashboard
- Supplements checklist persists across page refreshes

### 📋 Day 3 Deliverables
- [ ] 6-day workout split loads from API
- [ ] Set logging (weight + reps) persists to database
- [ ] Workout completion increments streak on Dashboard
- [ ] Supplements Tracker daily checkboxes work correctly

---

## 🗓️ Day 4 — Coach Operations Hub

**Goal**: Coaches can monitor all their athletes in real-time — workout feeds, nutrition compliance, and check-in alerts.

### ✅ Frontend (Dev A)
| Task | Description |
|------|-------------|
| Layout shell | Build `CoachLayout.jsx` — dark Navy sidebar (Dashboard, Client Roster, Library Admin, Template Builder, Check-ins, Notifications) |
| **Screen: Coach Operations Dashboard** | KPI strip (Active Athletes count, Workout Completion %, Pending Check-ins with red alert), Real-Time Live Feed panel, Nutrition Compliance tracker panel |
| `LiveFeedItem.jsx` | Row component showing athlete avatar, workout name, status badge (Completed = green, In Progress = Gold + pulse animation, Missed = red) |
| `ComplianceBar.jsx` | Named athlete compliance bar — Gold fill to target, Red overfill when limit exceeded |
| Polling mechanism | Feed auto-refreshes every 30 seconds using `setInterval` |
| **Screen: Client Roster** | Data table with columns: Athlete name + avatar, Active Program, Macro Compliance %, Last Check-in date, Status badge, "View Profile" link |
| Filter & search | Filter roster by: All / Compliance Alert / No Recent Check-In |
| **Screen: Client Detail View** | Athlete profile header (photo, stats, assigned coach), Historical weight chart, Macro Compliance timeline chart, Check-in photo gallery, Coach feedback/notes text area + submit |
| Chart library | Install `recharts` for weight trend line chart and macro compliance bar chart |

### ✅ Backend (Dev B)
| Task | Description |
|------|-------------|
| `GET /api/coach/dashboard` | Return: active athlete count, average workout %, pending check-ins, live feed events |
| `GET /api/coach/feed` | Paginated list of recent workout events across all roster athletes |
| `GET /api/coach/compliance` | Per-athlete calorie + macro tracking status for today |
| `GET /api/coach/roster` | Full roster list with latest metrics, last check-in, compliance % |
| `GET /api/coach/athletes/{id}` | Single athlete deep profile: biometrics history, macro logs, check-ins, workout history |
| `POST /api/coach/athletes/{id}/feedback` | Save coach feedback note for specific athlete |
| `GET /api/coach/athletes/{id}/progress-photos` | Return secure signed photo URLs |
| Authorization | Enforce `[Authorize(Roles = "Coach, Admin")]` on all coach routes |

### 🔌 Day 4 Integration Milestone
- Coach Dashboard loads live data for all athletes
- Live Feed refreshes and correctly shows In-Progress / Completed / Missed states
- Compliance bars turn red in real-time when athletes exceed calorie budgets
- Clicking an athlete in the roster opens their detailed profile

### 📋 Day 4 Deliverables
- [ ] Coach Dashboard KPI cards load from API
- [ ] Live Feed auto-refreshes every 30 seconds
- [ ] Client Roster table fully searchable and filterable
- [ ] Client Detail View shows athlete history charts

---

## 🗓️ Day 5 — Admin Content Libraries & Workout Template Builder

**Goal**: Coaches can build workout programs and manage the master food/exercise database.

### ✅ Frontend (Dev A)
| Task | Description |
|------|-------------|
| **Screen: Exercise Library Admin** | Card grid of all exercises — category filter tabs (Chest, Back, Shoulders, Arms, Legs, Cardio), search bar, video thumbnail previews, "Edit" / "Delete" actions |
| **Screen: Add Exercise Modal** | Form: Exercise name, target muscle group, equipment needed, instructions text area, YouTube video ID input with embedded preview |
| **Screen: Food & Recipe Admin (Empty + Populated)** | Two-view admin table: Foods tab (ingredient cards with macro-per-100g) + Recipes tab (Joker Premium recipe cards), search and filter tools |
| **Screen: Bulk Import Modal** | Drag-and-drop CSV file zone + manual paste text area with column mapping preview and "Import" confirmation button |
| **Screen: Workout Template Builder** | Left panel: exercise library selector. Right canvas: 6-day week grid. Drag exercises into day slots. Configure sets, reps, and rest targets. Assign completed template to selected athlete(s) |
| `DraggableExercise.jsx` | Drag-and-drop exercise card using `@dnd-kit/core` |
| `WorkoutDayColumn.jsx` | Day column for the template canvas, receives dropped exercises and renders a sorted list |
| **Screen: Invitation Management** | Table of sent invites: email, role, status (Pending/Accepted/Expired), copy-link icon, "Resend" and "Revoke" buttons, "Invite New Member" button opening a modal |

### ✅ Backend (Dev B)
| Task | Description |
|------|-------------|
| `GET /api/admin/exercises` | Full exercise catalog with pagination + filters |
| `POST /api/admin/exercises` | Create new exercise entry |
| `PUT /api/admin/exercises/{id}` | Update exercise |
| `DELETE /api/admin/exercises/{id}` | Soft-delete exercise |
| `GET /api/admin/foods` | Full food database catalog |
| `POST /api/admin/foods` | Create food item with macros |
| `POST /api/admin/foods/bulk-import` | Parse CSV, validate columns, batch-insert food records |
| `GET /api/admin/recipes` | Joker Premium recipes list |
| `POST /api/admin/recipes` | Create Joker recipe with ingredient list |
| `GET /api/workouts/templates` | All saved workout templates |
| `POST /api/workouts/templates` | Save a new 6-day workout template (JSON structure of exercises per day) |
| `POST /api/workouts/templates/{id}/assign` | Assign a template to one or multiple athletes |
| `GET /api/invitations` | List all invitations with status |
| `POST /api/invitations/resend/{id}` | Resend invitation email |
| `DELETE /api/invitations/{id}` | Revoke invitation |

### 🔌 Day 5 Integration Milestone
- Coach can create and drag-build a full 6-day workout template and save it to the database
- Assigning the template to an athlete immediately appears in their Workout Logger
- Bulk CSV food import works and feeds into the athlete's food picker

### 📋 Day 5 Deliverables
- [ ] Workout Template Builder drag-and-drop fully functional
- [ ] Template assigned to athlete is live in their portal immediately
- [ ] Exercise Library Admin: create, edit, delete working
- [ ] Food Admin: manual create + bulk CSV import working
- [ ] Invitation Management: issue, resend, revoke working

---

## 🗓️ Day 6 — Weekly Check-In System, Progress Photos & Advanced Features

**Goal**: Athletes submit weekly biometric check-ins with photos; coaches review and respond.

### ✅ Frontend (Dev A)
| Task | Description |
|------|-------------|
| **Screen: Weekly Check-In** | Stepped form: Step 1 - Biometrics (Weight, Waist, Chest, Thighs input fields), Step 2 - Subjective sliders (Sleep Quality, Energy Level, Gut Health, Training Stress — all on a 1-10 scale in Gold), Step 3 - Progress Photo Upload (Front, Side, Back drag-drop zones with preview thumbnails), Step 4 - Submit Confirmation screen |
| `BiometricInputRow.jsx` | Labeled number input with unit suffix (kg / cm) |
| `SubjectiveSlider.jsx` | Styled range slider in Joker Gold with numerical label |
| `PhotoUploadZone.jsx` | Drag-and-drop zone using native HTML5 File API, displays image preview on drop |
| Photo Upload Flow | `POST` file to API pre-signed URL, display upload progress bar |
| Check-in history | On submit success, show "Your check-in was received" confirmation with summary card |
| Ramadan Mode Toggle | Add Ramadan Mode switch in athlete nutrition settings: when enabled, swaps standard meal labels to "Suhoor", "Iftar", "Pre-Workout Snack", "Post-Workout Snack" and adjusts fasting hour windows |
| Notification badges | Add unread check-in alert badges on the Coach Sidebar |

### ✅ Backend (Dev B)
| Task | Description |
|------|-------------|
| Database schemas | `ClientCheckIns` (weight, waist, chest, thighs, sleepQuality, energyLevel, gutHealth, trainingStress), `CheckInPhotos` (blobUrl, angle, checkInId) |
| Cloud Storage | Configure Azure Blob Storage or AWS S3 bucket. Generate 24-hour expiring pre-signed PUT URLs for photo uploads |
| `POST /api/checkins` | Submit biometrics + subjective markers |
| `POST /api/checkins/{id}/photos/upload-url` | Return a pre-signed URL for each photo angle |
| `GET /api/checkins/history/{athleteId}` | Full check-in history timeline for a client |
| `GET /api/checkins/pending` | Coach endpoint — list all athletes who haven't submitted this week |
| `GET /api/checkins/{id}/photos` | Return list of signed download URLs for photos (24h expiry) |
| Biometric Progress | `GET /api/athletes/{id}/weight-history` — return series of weight measurements for chart |
| `PUT /api/checkins/{id}/coach-notes` | Coach submits feedback notes on a check-in |
| Notification system | Generate in-app alert event when athlete submits check-in (coach sees badge) |

### 🔌 Day 6 Integration Milestone
- Athlete completes full 4-step check-in with photo uploads
- Coach immediately sees new pending check-in alert on dashboard
- Coach can open the check-in, view photos, compare with previous week, and add feedback notes

### 📋 Day 6 Deliverables
- [ ] Weekly Check-In multi-step form fully functional
- [ ] Photos upload directly to cloud storage via pre-signed URLs
- [ ] Coach dashboard shows pending check-in alerts in real-time
- [ ] Coach can view photos and leave feedback notes
- [ ] Ramadan Mode toggle works correctly in athlete portal

---

## 🗓️ Day 7 — Integration, Hardening, PWA Features & Final Polish

**Goal**: The full platform is production-ready, hardened, and tested end-to-end across all 19 screens.

### ✅ Frontend (Dev A)
| Task | Description |
|------|-------------|
| Full navigation audit | Confirm every screen in the Stitch design links correctly and no dead buttons remain |
| Loading states | Add skeleton loading placeholders (shimmer effect in surface-variant) for all data-fetched components |
| Error boundaries | Handle API failures gracefully with branded empty states and retry buttons |
| Toast notifications | Implement non-blocking toast messages for: meal logged ✅, workout completed ✅, check-in submitted ✅, error ❌ |
| Mobile responsiveness | Full audit on 375px screen width — all layouts collapse correctly |
| PWA setup | Install `vite-plugin-pwa`, configure service worker for app shell caching, add `manifest.json` with Joker Nutrition icons |
| Performance | Lazy-load page components with `React.lazy()` and `Suspense`. Compress images. |
| Design QA | Cross-check all screens against Google Stitch mockups — colors, spacing, font weights, icon choices |

### ✅ Backend (Dev B)
| Task | Description |
|------|-------------|
| Database indexes | Add performance indexes on: `MealLogs.AthleteId`, `WorkoutLogs.AthleteId + Date`, `ClientCheckIns.AthleteId` |
| AuditLogs table | Implement immutable audit trail for: login events, coach data changes, athlete updates |
| Security hardening | Enforce HTTPS redirect, lock CORS to production domains, move all secrets to environment variables |
| Rate limiting | Add `AspNetCoreRateLimit` middleware on auth endpoints (max 10 requests/min) |
| API health check | Add `GET /api/health` endpoint |
| Swagger docs | Annotate all endpoints with XML docs for Swagger UI |
| Integration tests | Write critical path tests: Auth → Login → Dashboard Load → Log Food → Workout Complete |
| SQL Server optimization | Review query execution plans for compliance and roster queries, add missing indexes |

### 🔌 Day 7 Milestone: Full End-to-End Demo
1. New athlete registers via invitation link
2. Coach assigns workout template and macro targets
3. Athlete logs 3 meals and views dashboard macros update
4. Athlete completes a workout — coach sees update in live feed
5. Athlete submits weekly check-in with photos
6. Coach reviews check-in, adds feedback, athlete sees response

### 📋 Day 7 Deliverables
- [ ] All 19 Stitch screens implemented and navigable
- [ ] PWA installable on mobile with offline shell caching
- [ ] API fully documented via Swagger
- [ ] Security: CORS locked, rate limiting active, secrets in env vars
- [ ] Full end-to-end flow tested from athlete registration to coach review
- [ ] No broken links, no unhandled errors, all loading states present

---

## 📊 Summary Table

| Day | Theme | Key Deliverables | Frontend Screens | Backend Endpoints |
|-----|-------|-----------------|-----------------|------------------|
| 1 | Foundation & Auth | Project scaffolding, login, JWT | Login, Join the Team | `/auth/*`, `/invitations` |
| 2 | Athlete Nutrition | Dashboard + full meal tracking | Dashboard, Meal Logger, Add Food Modal, Recipe Library, Create Recipe | `/diary/*`, `/foods/*`, `/recipes/*` |
| 3 | Workout & Supplements | Workout logger, supplement tracking | Workout Logger, Supplements Tracker | `/workouts/*`, `/supplements/*` |
| 4 | Coach Hub | Coach dashboard, roster, client profiles | Coach Dashboard, Client Roster, Client Detail | `/coach/*` |
| 5 | Admin Libraries | Template builder, content libraries, invites | Template Builder, Exercise Admin, Food Admin, Bulk Import, Invitation Mgmt | `/admin/*`, `/workouts/templates` |
| 6 | Check-Ins & Photos | Weekly check-in form, cloud photos, Ramadan mode | Weekly Check-In | `/checkins/*`, cloud upload |
| 7 | Integration & Polish | PWA, hardening, QA, end-to-end demo | All 19 screens audited | Security, indexes, Swagger |

---

> [!NOTE]
> **Developer Assignment Pattern**: Dev A (Frontend) and Dev B (Backend) should work in parallel every day. Dev B should always expose a mocked or seeded API response first thing in the morning so Dev A can build the UI against real HTTP calls immediately, without waiting for database logic to be complete.
