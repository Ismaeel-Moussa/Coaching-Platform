# Joker Nutrition Platform — Frontend Build Prompt & Architecture Blueprint

> This prompt is written so that any developer (or AI agent) can scaffold and build the Joker Nutrition Platform frontend with the **exact same technology stack, folder structure, and coding conventions** used in the **88ninety-academy-ui** project. The domain, however, is a **high-performance nutrition & fitness coaching platform** built around two user experience hubs: an **Athlete (Client) Hub** and a **Coach/Admin Hub**.

---

## 1. Project Overview

Build the **Joker Nutrition** web application — an elite fitness coaching and nutrition tracking platform. The platform supports:

- **Admins** — manage all users, exercise library, food database, workout templates, and invitation lifecycle.
- **Coaches** — monitor athlete rosters in real-time, review nutrition compliance, build workout templates, manage check-ins, and assign programs.
- **Athletes (Clients)** — track daily macros/food, log workouts (6-day PPL split), check off supplements, submit weekly biometric check-ins with progress photos, and view personal progress dashboards.

---

## 2. Technology Stack

> Use **exactly** the same stack as `88ninety-academy-ui`.

| Layer | Technology |
|---|---|
| Framework | **React 19** with TypeScript (strict mode) |
| Build Tool | **Vite 8** (`@vitejs/plugin-react`) |
| Routing | **React Router DOM v7** (`createBrowserRouter`) |
| Server State | **TanStack React Query v5** (`QueryClient`, `useQuery`, `useMutation`) |
| HTTP Client | **Axios** (one authenticated instance + one bare instance for auth) |
| UI Library | **Ant Design v6** (`antd`) |
| Icons | **HugeIcons** (`@hugeicons/react`, `@hugeicons/core-free-icons`) |
| Drag & Drop | **@dnd-kit** (`core`, `sortable`, `utilities`) |
| Charts | **Recharts** (`recharts`) — for weight trend, macro compliance charts |
| Styling | **SCSS** (one `.scss` file per component, global `index.scss`) |
| Fonts | **Google Fonts** — Archivo Narrow (headings), Inter (body), JetBrains Mono (data labels) |
| Language | **TypeScript ~5.9** |
| Linting | **ESLint 9** with `typescript-eslint`, `react-hooks`, `react-refresh`, `@tanstack/eslint-plugin-query` |

### `package.json` scripts

```json
{
  "scripts": {
    "dev": "vite",
    "build": "tsc -b && vite build",
    "lint": "eslint .",
    "preview": "vite preview"
  }
}
```

---

## 3. Project Initialization

```bash
# 1. Scaffold with Vite
npx -y create-vite@latest joker-nutrition-ui -- --template react-ts

# 2. Install production dependencies
npm install react-router-dom@^7 @tanstack/react-query@^5 axios@^1 antd@^6 \
  @hugeicons/react @hugeicons/core-free-icons \
  @dnd-kit/core @dnd-kit/sortable @dnd-kit/utilities \
  recharts sass@^1

# 3. Install dev dependencies
npm install -D @tanstack/eslint-plugin-query typescript-eslint \
  eslint-plugin-react-hooks eslint-plugin-react-refresh @types/node
```

### `.env`

```env
VITE_API_URL=http://localhost:7001/api
```

---

## 4. Folder Structure

Mirror the exact same `src/` layout, adapted to Joker Nutrition screens:

```
src/
├── main.tsx                  # App entry — providers, QueryClient, router
├── App.tsx
├── index.scss                # CSS variables (Joker design tokens), global reset, Ant Design overrides
│
├── AppRoutes/
│   ├── AppRoutes.tsx         # createBrowserRouter — all 19 screen routes declared here
│   ├── ProtectedRoute.tsx    # Redirect to /sign-in if no token in localStorage
│   └── RoleGuard.tsx         # Redirect based on role (Athlete → /athlete/dashboard, Coach → /coach/dashboard)
│
├── layouts/
│   ├── AthleteLayout.tsx     # Left sidebar nav (Dashboard, Nutrition, Workouts, Supplements, Check-In) + top bar
│   ├── AthleteLayout.scss
│   ├── CoachLayout.tsx       # Dark Navy sidebar (Dashboard, Roster, Libraries, Templates, Check-Ins, Invitations)
│   ├── CoachLayout.scss
│   ├── AuthLayout.tsx        # Centered branded card layout for auth screens
│   └── AuthLayout.scss
│
├── pages/
│   │
│   ├── auth/
│   │   ├── SignIn/           # Screen 1 — "Joker Nutrition - Login"
│   │   ├── JoinTheTeam/      # Screen 2 — "Joker Nutrition - Join the Team" (athlete onboarding)
│   │   └── InvalidInvite/
│   │
│   ├── athlete/
│   │   ├── Dashboard/        # Screen 4 — "Customer Dashboard" (macros bento-grid, today's session, streak)
│   │   ├── MealLogger/       # Screen 5 — "Meal Logger" (Breakfast/Lunch/Dinner/Snack tabs)
│   │   ├── RecipeLibrary/    # Screen 7 — "Recipe Library" (Muscle Building / Fat Loss / Custom tabs)
│   │   ├── WorkoutLogger/    # Screen 9 — "Workout Logger" (6-day tab selector, exercise cards)
│   │   ├── SupplementsTracker/ # Screen 10 — "Supplements Tracker" (Essential + Optional checklist)
│   │   └── WeeklyCheckIn/    # Screen 11 — "Weekly Check-In" (4-step form: biometrics, sliders, photos)
│   │
│   └── coach/
│       ├── CoachDashboard/   # Screen 12 — "Coach Operations Dashboard" (KPIs, live feed, compliance)
│       ├── ClientRoster/     # Screen 13 — "Client Roster" (table with filters)
│       ├── ClientDetail/     # Screen 14 — "Client Detail View" (charts, check-in gallery, notes)
│       ├── WorkoutTemplateBuilder/ # Screen 15 — "Workout Template Builder" (drag-and-drop canvas)
│       ├── ExerciseLibraryAdmin/   # Screen 16 — "Exercise Library Admin" (card grid + CRUD)
│       ├── FoodRecipeAdmin/  # Screens 18+19 — "Food & Recipe Admin" (Foods tab + Recipes tab)
│       └── InvitationManagement/   # Screen 3 — "Invitation Management" (issue, resend, revoke)
│
├── components/               # Shared, reusable UI components — one folder each
│   ├── Header/
│   ├── Sidebar/
│   ├── ActionButton/
│   ├── PageHeader/
│   ├── PaginatedTable/
│   ├── PaginationBar/
│   ├── FiltersRow/
│   ├── SearchInput/
│   ├── Pill/                 # Status badge (Active/Gold, Missed/Red, InProgress/Gold+pulse)
│   ├── GlobalConfirmModal/
│   ├── Inputs/
│   │
│   ├── MacroProgressBar/     # Labelled progress bar — turns Red when limit exceeded
│   ├── RingProgress/         # SVG ring component for hydration and steps
│   ├── ExerciseCard/         # Exercise card: name, sets/reps inputs, video play icon, completion checkbox
│   ├── LiveFeedItem/         # Coach live feed row: athlete avatar, workout name, status badge
│   ├── ComplianceBar/        # Named athlete compliance bar — Gold fill, Red overfill
│   ├── SubjectiveSlider/     # Range slider styled in Joker Gold with numerical label
│   ├── PhotoUploadZone/      # Drag-and-drop zone using HTML5 File API with preview
│   ├── BiometricInputRow/    # Labeled number input with unit suffix (kg / cm)
│   ├── DraggableExercise/    # Draggable exercise card using @dnd-kit/core
│   ├── WorkoutDayColumn/     # Day column for the template canvas, receives dropped exercises
│   ├── RecipeCard/           # Recipe card with cook time, macro badges, "Quick Add" shortcut
│   ├── VideoDemoModal/       # YouTube iframe overlay triggered from ExerciseCard play icon
│   ├── AddFoodModal/         # Screen 6 — food search overlay, quantity input, state selector
│   ├── CreateRecipeModal/    # Screen 8 — 3-step wizard to create custom recipe
│   ├── AddExerciseModal/     # Screen 17 — form to add exercise to library
│   └── BulkImportModal/      # Screen 19 — CSV drag-and-drop + paste area for food import
│
├── api/                      # One file per domain — pure async functions
│   ├── axiosInstance.ts      # Authenticated Axios instance (Bearer token)
│   ├── auth.ts               # Bare Axios for unauthenticated calls (login, register, password reset)
│   ├── user.ts
│   ├── athlete.ts            # Athlete dashboard, targets, streak
│   ├── diary.ts              # Daily diary, meal logging, macro summary, water, steps
│   ├── food.ts               # Food search, admin CRUD, bulk import
│   ├── recipe.ts             # Recipe library, create recipe, quick-add to diary
│   ├── workout.ts            # Today's workout, set logging, completion
│   ├── workoutTemplate.ts    # Template CRUD, assignment
│   ├── exercise.ts           # Exercise library CRUD
│   ├── supplement.ts         # Schedule & daily check-off
│   ├── checkIn.ts            # Submit check-in, photo upload, history
│   ├── coachHub.ts           # Dashboard stats, live feed, compliance, roster, client profile
│   ├── invitation.ts         # Invitation CRUD
│   └── notification.ts       # Notification management
│
├── hooks/                    # One folder per domain hook
│   ├── useAuth/
│   ├── useUser/
│   ├── useAthlete/           # Dashboard summary, targets
│   ├── useDiary/             # Diary fetch, meal log mutations
│   ├── useFoods/             # Food search, admin CRUD
│   ├── useRecipes/           # Recipe library, create recipe
│   ├── useWorkout/           # Today's workout, set logging, completion
│   ├── useWorkoutTemplates/  # Template builder CRUD, assignment
│   ├── useExercises/         # Exercise library
│   ├── useSupplements/       # Schedule & daily toggle
│   ├── useCheckIn/           # Submit, photo upload, history
│   ├── useCoachHub/          # Dashboard, live feed, compliance, roster, client detail
│   ├── useInvitations/
│   ├── useNotifications/
│   ├── usePagination/
│   └── useDebouncedSearch/
│
├── contexts/
│   ├── GlobalConfirmModalContext.tsx
│   └── UserFormModalContext.tsx
│
├── types/                    # TypeScript interfaces — one file per domain
│   ├── GeneralTypes.ts       # PaginatedResult<T>, ApiResponse<T>, PaginatedApiResponse<T>
│   ├── auth.ts
│   ├── user.ts
│   ├── Athlete.ts            # AthleteDto, DashboardSummary, MacroTarget, StreakInfo
│   ├── Coach.ts              # CoachDto, RosterEntry
│   ├── Diary.ts              # DailyDiaryDto, MealLogDto, MacroSummaryDto
│   ├── Food.ts               # FoodDto, MealType, FoodState
│   ├── Recipe.ts             # RecipeDto, RecipeIngredientDto, RecipeCategory
│   ├── Workout.ts            # WorkoutTemplateDayDto, ExerciseCardData, WorkoutLogDto, SetLogDto
│   ├── Exercise.ts           # ExerciseDto, MuscleGroup, ExerciseSection
│   ├── Supplement.ts         # SupplementScheduleDto, SupplementLogDto, SupplementType
│   ├── CheckIn.ts            # ClientCheckInDto, CheckInPhotoDto, PhotoAngle
│   ├── CoachHub.ts           # CoachDashboardDto, LiveFeedItemDto, ComplianceEntryDto, ClientDetailDto
│   ├── Invitation.ts         # InvitationDto, InvitationStatus
│   └── Notification.ts       # NotificationDto, NotificationType
│
├── utils/
│   ├── getCurrentUserRoles.ts
│   ├── macroCalc.ts          # Client-side macro preview (food weight × state factor × per100g)
│   ├── date.ts
│   ├── clipboard.ts
│   └── mockAzureUpload.ts    # Simulates pre-signed URL upload in dev mode
│
└── assets/
    └── (Joker Nutrition logo, icons, placeholder images)
```

---

## 5. Entry Point — `main.tsx`

```tsx
import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import './index.scss';
import { RouterProvider } from 'react-router/dom';
import { router } from './AppRoutes/AppRoutes';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { GlobalConfirmModalProvider } from './contexts/GlobalConfirmModalContext';

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      refetchOnWindowFocus: false,
      retry: false,
    },
  },
});

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <QueryClientProvider client={queryClient}>
      <GlobalConfirmModalProvider>
        <RouterProvider router={router} />
      </GlobalConfirmModalProvider>
    </QueryClientProvider>
  </StrictMode>,
);
```

---

## 6. Global Styles — `index.scss`

Map the full Joker Nutrition design token system from Google Stitch:

```scss
@import url('https://fonts.googleapis.com/css2?family=Archivo+Narrow:ital,wght@0,400..700;1,400..700&family=Inter:wght@300;400;500;600;700&family=JetBrains+Mono:wght@400;500;600&display=swap');

:root {
  /* === Joker Nutrition Color Palette === */
  --color-navy:          #0b132b;       /* Primary: Header/Nav background */
  --color-navy-light:    #131a33;       /* Primary container */
  --color-gold:          #fdc003;       /* Secondary: Active states, CTAs, progress fills */
  --color-gold-dim:      #fabd00;       /* Gold on hover */
  --color-gold-text:     #785900;       /* Gold text on light backgrounds */
  --color-red:           #ba1a1a;       /* Tertiary: Missed/exceeded/danger states */
  --color-red-bright:    #dc2626;       /* High-visibility error */

  /* Surfaces */
  --background-color:       #f9f9ff;    /* App background (ice white) */
  --surface-container-low:  #f1f3ff;
  --surface-container:      #e9edff;
  --surface-container-high: #e1e8fd;
  --surface-variant:        #dce2f7;
  --color-canvas:           #F3F4F6;    /* Desktop margin / Level 0 */

  /* Text */
  --color-text-primary:   #141b2b;      /* Dark Charcoal — main body text */
  --color-text-secondary: #45464d;      /* On-surface-variant */

  /* Borders */
  --color-border:         #c6c6ce;      /* outline-variant */
  --color-border-strong:  #76767e;      /* outline */

  /* Semantic */
  --color-success:   #12B76A;
  --color-warning:   #F79009;
  --color-error:     #ba1a1a;
  --color-info:      #0BA5EC;

  /* Typography */
  --font-heading:  'Archivo Narrow', sans-serif;
  --font-body:     'Inter', sans-serif;
  --font-data:     'JetBrains Mono', monospace;
  --font-family:   var(--font-body);

  /* Radius */
  --radius-btn:    4px;
  --radius-card:   8px;
  --radius-full:   9999px;

  /* Spacing (8px base scale) */
  --space-xs:  8px;
  --space-sm:  16px;
  --space-md:  24px;
  --space-lg:  32px;
  --space-xl:  40px;
}

* { padding: 0; margin: 0; box-sizing: border-box; }

body {
  background-color: var(--color-canvas);
  color: var(--color-text-primary);
  font-family: var(--font-body);
}

/* Headings always use Archivo Narrow */
h1, h2, h3, h4, h5, h6 {
  font-family: var(--font-heading);
  color: var(--color-text-primary);
}

/* Data metrics & labels always use JetBrains Mono */
.mono, .data-label {
  font-family: var(--font-data);
  letter-spacing: 0.05em;
}

/* Global Ant Design overrides */
.table-menu-dropdown {
  .ant-dropdown-menu { border-radius: 10px; box-shadow: 0 4px 20px rgba(11,19,43,0.10); padding: 4px; }
  .ant-dropdown-menu-item { border-radius: 6px; padding: 0; &:hover { background-color: var(--surface-container-low); } }
}

.table-menu-item {
  display: flex; align-items: center; gap: 8px;
  padding: 8px 12px; font-size: 14px; color: var(--color-text-primary); cursor: pointer;
  svg { color: var(--color-border-strong); flex-shrink: 0; }
  &.delete-item, &.delete-item svg { color: var(--color-red); }
}
```

---

## 7. Routing — `AppRoutes.tsx`

Use `createBrowserRouter` with a **three-tree pattern**:
1. **Auth tree** — wrapped in `<AuthLayout />`, no `ProtectedRoute`
2. **Athlete tree** — `<ProtectedRoute>` → `<AthleteLayout />` → `<RoleGuard allowedRoles={['Athlete']} />`
3. **Coach/Admin tree** — `<ProtectedRoute>` → `<CoachLayout />` → `<RoleGuard allowedRoles={['Coach', 'Admin']} />`

```tsx
export const router = createBrowserRouter([
  // ── Auth tree ──────────────────────────────────────────
  {
    element: <AuthLayout />,
    children: [
      { path: '/sign-in',         element: <SignIn /> },
      { path: '/join-the-team',   element: <JoinTheTeam /> },    // Athlete onboarding via invite token
      { path: '/invalid-invite',  element: <InvalidInvite /> },
    ],
  },

  // ── Athlete tree ────────────────────────────────────────
  {
    path: '/athlete',
    element: (
      <ProtectedRoute>
        <RoleGuard allowedRoles={['Athlete']}>
          <AthleteLayout />
        </RoleGuard>
      </ProtectedRoute>
    ),
    children: [
      { index: true, element: <Navigate to="/athlete/dashboard" replace /> },
      { path: 'dashboard',     element: <AthleteDashboard /> },      // Screen 4
      { path: 'meal-logger',   element: <MealLogger /> },            // Screen 5
      { path: 'recipes',       element: <RecipeLibrary /> },         // Screen 7
      { path: 'workouts',      element: <WorkoutLogger /> },         // Screen 9
      { path: 'supplements',   element: <SupplementsTracker /> },    // Screen 10
      { path: 'check-in',      element: <WeeklyCheckIn /> },         // Screen 11
    ],
  },

  // ── Coach / Admin tree ─────────────────────────────────
  {
    path: '/coach',
    element: (
      <ProtectedRoute>
        <RoleGuard allowedRoles={['Coach', 'Admin']}>
          <CoachLayout />
        </RoleGuard>
      </ProtectedRoute>
    ),
    children: [
      { index: true, element: <Navigate to="/coach/dashboard" replace /> },
      { path: 'dashboard',               element: <CoachDashboard /> },        // Screen 12
      { path: 'roster',                  element: <ClientRoster /> },          // Screen 13
      { path: 'roster/:athleteId',       element: <ClientDetail /> },          // Screen 14
      { path: 'template-builder',        element: <WorkoutTemplateBuilder /> },// Screen 15
      { path: 'template-builder/:id',    element: <WorkoutTemplateBuilder /> },// Edit mode
      { path: 'exercise-library',        element: <ExerciseLibraryAdmin /> },  // Screen 16
      { path: 'food-admin',              element: <FoodRecipeAdmin /> },       // Screens 18 & 19
      { path: 'invitations',             element: <InvitationManagement /> },  // Screen 3
    ],
  },

  // ── Root redirect ──────────────────────────────────────
  { path: '/', element: <Navigate to="/sign-in" replace /> },
  { path: '*', element: <NotFound /> },
]);
```

---

## 8. Auth & Route Guards

### `ProtectedRoute.tsx`
```tsx
const ProtectedRoute = ({ children }: { children: React.ReactNode }) => {
  const isAuthenticated = !!localStorage.getItem('token');
  if (!isAuthenticated) return <Navigate to="/sign-in" replace />;
  return <>{children}</>;
};
```

### `RoleGuard.tsx`
```tsx
const RoleGuard = ({ allowedRoles, children }: { allowedRoles: string[]; children: React.ReactNode }) => {
  const roles = getCurrentUserRoles();
  const hasAccess = allowedRoles.some(r => roles.includes(r));
  if (!hasAccess) {
    // Route to the correct hub based on actual role
    if (roles.includes('Athlete')) return <Navigate to="/athlete/dashboard" replace />;
    return <Navigate to="/coach/dashboard" replace />;
  }
  return <>{children}</>;
};
```

### `utils/getCurrentUserRoles.ts`
```ts
export const getCurrentUserRoles = (): string[] => {
  const user = localStorage.getItem('user');
  if (!user) return [];
  try {
    const parsed = JSON.parse(user);
    return parsed.roles ?? [];
  } catch {
    return [];
  }
};
```

---

## 9. Axios Instances — `api/`

### `axiosInstance.ts` — Authenticated
```ts
import axios from 'axios';
const axiosInstance = axios.create({ baseURL: `${import.meta.env.VITE_API_URL}/` });
axiosInstance.interceptors.request.use(config => {
  const token = localStorage.getItem('token');
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});
export default axiosInstance;
```

### `auth.ts` — Unauthenticated (bare instance)
```ts
import axios from 'axios';
const authApi = axios.create({ baseURL: import.meta.env.VITE_API_URL });
// loginUser, registerWithInvite, validateInviteToken, requestPasswordReset, updatePassword
// ALL use authApi — NOT axiosInstance
```

### Domain API file pattern (`api/diary.ts`)
```ts
import axiosInstance from './axiosInstance';
import type { DailyDiaryDto, MacroSummaryDto, LogFoodForm } from '../types/Diary';

export const getDiary = async (date: string): Promise<DailyDiaryDto> => {
  const response = await axiosInstance.get(`/diary/${date}`);
  return response.data.result;
};

export const logFood = async (form: LogFoodForm): Promise<void> => {
  await axiosInstance.post('/diary/log', form);
};

export const removeLogEntry = async (id: number): Promise<void> => {
  await axiosInstance.delete(`/diary/log/${id}`);
};

export const getMacroSummary = async (date: string): Promise<MacroSummaryDto> => {
  const response = await axiosInstance.get(`/diary/summary/${date}`);
  return response.data.result;
};

export const updateWater = async (date: string, liters: number): Promise<void> => {
  await axiosInstance.patch(`/diary/${date}/water`, { liters });
};
```

---

## 10. Shared Type Conventions — `types/GeneralTypes.ts`

```ts
export interface PaginatedResult<T> {
  page: number;
  pageSize: number;
  totalRecords: number;
  totalPages: number;
  searchTerm: string | null;
  sortingCriteria: string | null;
  sortingOrder: string | null;
  data: T[];
}

export interface PaginatedApiResponse<T> {
  version: string | null;
  statusCode: number;
  message: string | null;
  isError: boolean | null;
  responseException: unknown | null;
  result: PaginatedResult<T>;
}

export interface ApiResponse<T> {
  version: string | null;
  statusCode: number;
  message: string | null;
  isError: boolean | null;
  responseException: unknown | null;
  result: T;
}

export interface ValidationErrorResponse {
  type: string;
  title: string;
  status: number;
  errors: Record<string, string[]>;
  traceId: string;
}
```

---

## 11. Hooks Convention

Each domain hook lives in its own folder inside `src/hooks/`. Use **TanStack React Query** for all server state.

```ts
// hooks/useDiary/useDiary.ts
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { getDiary, logFood, removeLogEntry, getMacroSummary, updateWater } from '../../api/diary';
import { message as antMessage } from 'antd';
import type { LogFoodForm } from '../../types/Diary';

export const useGetDiary = (date: string) =>
  useQuery({
    queryKey: ['diary', date],
    queryFn: () => getDiary(date),
  });

export const useGetMacroSummary = (date: string) =>
  useQuery({
    queryKey: ['diary-summary', date],
    queryFn: () => getMacroSummary(date),
  });

export const useLogFood = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (form: LogFoodForm) => logFood(form),
    onSuccess: (_, variables) => {
      antMessage.success('Food logged successfully!');
      queryClient.invalidateQueries({ queryKey: ['diary', variables.date] });
      queryClient.invalidateQueries({ queryKey: ['diary-summary', variables.date] });
    },
    onError: (error: Error) => antMessage.error(error.message),
  });
};

export const useRemoveLogEntry = (date: string) => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: removeLogEntry,
    onSuccess: () => {
      antMessage.success('Entry removed.');
      queryClient.invalidateQueries({ queryKey: ['diary', date] });
      queryClient.invalidateQueries({ queryKey: ['diary-summary', date] });
    },
    onError: (error: Error) => antMessage.error(error.message),
  });
};
```

---

## 12. Context Convention

```tsx
// contexts/GlobalConfirmModalContext.tsx
import { createContext, useState } from 'react';
// showModal(config) / hideModal()
// Renders <GlobalConfirmModal /> as a portal child
// (exact same pattern as the academy project)
```

---

## 13. Layouts

### `AthleteLayout.tsx`
Sidebar nav items: Dashboard, Nutrition (Meal Logger), Recipes, Workouts, Supplements, Check-In.
- Sidebar uses `--color-navy` background, active items highlighted with `--color-gold`.
- Top bar shows athlete name, current streak badge, notification bell.

```tsx
const AthleteLayout = () => (
  <div className="layout">
    <Sidebar role="athlete" />
    <div className="layout-main">
      <Header />
      <main className="content">
        <Outlet />
      </main>
    </div>
  </div>
);
```

### `CoachLayout.tsx`
Sidebar nav items: Dashboard, Client Roster, Exercise Library, Food & Recipes, Template Builder, Check-Ins (with pending badge), Invitations.
- Dark Navy sidebar with Gold active indicators.
- Shows pending check-in count as alert badge on "Check-Ins" nav item.

### `AuthLayout.tsx`
Centered, vertically-stacked branded layout for sign-in and join-the-team screens. Navy header band at top with Joker Nutrition logo/wordmark.

---

## 14. Component Specifications

Each component lives in its own folder:
```
components/MacroProgressBar/
  MacroProgressBar.tsx
  MacroProgressBar.scss
```

- No inline styles — all styles in the co-located `.scss` file.
- Use CSS custom properties from `:root` for all colors/typography.
- Use HugeIcons for all iconography.

### Key Component Behaviors

| Component | Behavior |
|---|---|
| `MacroProgressBar` | Props: `label`, `consumed`, `target`, `unit`. Fill is `--color-gold`. Turns `--color-red` when `consumed > target`. Displays numeric `consumed/target` in JetBrains Mono. |
| `RingProgress` | SVG ring. Props: `value`, `max`, `unit`, `label`. Gold ring track, Light Gray background. Tap area increments hydration value. |
| `ExerciseCard` | Props: `exercise`, `setLogs`, `onSetLog`, `onComplete`. Shows exercise name, warm-up/main/cool-down badge, two inputs (weight kg, reps), play icon opens `VideoDemoModal`, Gold checkbox marks complete. |
| `LiveFeedItem` | Props: `athlete`, `event`, `status`. Status badge: Completed=green, InProgress=Gold + CSS pulse animation, Missed=red. |
| `ComplianceBar` | Props: `athleteName`, `consumed`, `target`. Gold fill to target. Red overflow section when `consumed > target`. Shows numeric values in JetBrains Mono. |
| `SubjectiveSlider` | Styled range slider. Gold thumb and track fill. Numeric label right-aligned. 1–10 range. |
| `PhotoUploadZone` | HTML5 drag-and-drop zone per angle (Front/Side/Back). On drop: shows image thumbnail preview. On submit: fires pre-signed URL upload via `BlobStorageService`. |
| `DraggableExercise` | `@dnd-kit/core` draggable card for the template builder left panel. |
| `WorkoutDayColumn` | `@dnd-kit` droppable column. Receives exercises, renders sorted list. Shows day label (Push Day 1, Pull Day 1, etc.), rest day indicator. |
| `AddFoodModal` | Ant Design Modal. Search input with debounced API call. Results list with macro preview per item. Quantity input (grams), State selector (Raw/Cooked/Dry). Instant calorie calculation preview. |
| `CreateRecipeModal` | 3-step Ant Design Steps wizard. Step 1: Select & add ingredients. Step 2: Configure portions. Step 3: Preview cumulative macros + name/save. |
| `AddExerciseModal` | Form: name, muscle group select, equipment, instructions text area, YouTube video ID with embedded preview. |
| `BulkImportModal` | Two tabs: (1) Drag-and-drop CSV file zone, (2) Manual paste text area. Column mapping preview. Import confirmation button. |
| `VideoDemoModal` | Ant Design Modal with YouTube `<iframe>` embed. Opens from ExerciseCard play icon. |

---

## 15. Roles & Permission Model

| Role | Permissions |
|---|---|
| `Admin` | Full access: all Coach pages + user management, food admin, exercise admin |
| `Coach` | Coach hub: dashboard, roster, client details, template builder, exercise library, food admin, invitations |
| `Athlete` | Athlete hub: personal dashboard, meal logger, recipe library, workout logger, supplements, check-in |

Use `RoleGuard` on layout wrappers. Use role-conditional rendering inside pages for action buttons (e.g., Edit/Delete only shown to `Coach` or `Admin`).

---

## 16. Feature Pages — Screen-by-Screen Specifications

### Athlete Hub

#### Screen 4 — `AthleteDashboard` (`/athlete/dashboard`)
- **Bento-grid layout**:
  - *Daily Macros Card*: Three `MacroProgressBar` components (Protein g, Carbs g, Fat g) + total kcal bar. Data from `useGetMacroSummary(today)`.
  - *Today's Session Card*: Navy background. Displays workout name (e.g., "Hypertrophy Base - Upper Body Focus"), duration, movement count. "Start Workout" CTA button in Gold → navigates to `/athlete/workouts`.
  - *Daily Targets Card*: `RingProgress` for hydration (L) with quick-add `+0.25L` button. `RingProgress` for steps. Both pull from `DailyDiary` via `useGetDiary(today)`.
  - *Streak Tracker*: Gold flame icon + consecutive day count. From `useAthlete` hook.
- **Polling**: Dashboard summary auto-refreshes every 60 seconds via `refetchInterval`.

#### Screen 5 — `MealLogger` (`/athlete/meal-logger`)
- Tabbed layout: Breakfast, Lunch, Dinner, Snack (or Suhoor, Iftar, Pre-Workout Snack, Post-Workout Snack when Ramadan Mode is enabled).
- Per tab: Food item rows with columns P / C / F / kcal in JetBrains Mono, delete icon.
- Per-meal subtotal row at bottom of each tab.
- Grand total macros bar at top of page.
- "+ Add Food" button opens `<AddFoodModal />`. "+ Add Recipe" opens recipe quick-add.
- Uses `useGetDiary(today)` and `useLogFood()`, `useRemoveLogEntry()` mutations.

#### Screen 7 — `RecipeLibrary` (`/athlete/recipes`)
- Tabbed card grid: **Muscle Building** (700+ kcal), **Fat Loss** (350- kcal), **Custom** (user-created).
- `RecipeCard` shows: image/color band, recipe name, prep time, cook time, macro badges (P/C/F/kcal), "Quick Add to Diary" shortcut button.
- "+ Create Recipe" button opens `<CreateRecipeModal />`.
- Uses `useGetRecipes(category)`, `useQuickAddRecipe()`.

#### Screen 9 — `WorkoutLogger` (`/athlete/workouts`)
- **6-day tab selector**: Push Day 1 / Pull Day 1 / Legs Day 1 / Push Day 2 / Pull Day 2 / Legs Day 2 / Rest.
- Each day: exercises grouped into Warm-up / Main / Cool-down sections.
- `ExerciseCard` per exercise: name, target sets/reps, weight input, reps input, play icon, completion checkbox.
- "Complete Workout" button at bottom → calls `useCompleteWorkout()` mutation → toast success → updates streak on dashboard.
- Uses `useGetTodaysWorkout()`, `useLogSet()`, `useCompleteWorkout()`.

#### Screen 10 — `SupplementsTracker` (`/athlete/supplements`)
- Two-column checklist: **Essential Supplements** (Creatine, Multivitamins, Omega-3, Vitamin D3) and **Optional/Conditional Supplements**.
- Each row: supplement name, dosage note, Gold checkbox.
- Daily reset at midnight (backend-driven status from `useGetSupplements(today)`).
- Uses `useGetSupplements()`, `useToggleSupplement()`.

#### Screen 11 — `WeeklyCheckIn` (`/athlete/check-in`)
- **4-step Ant Design Steps form**:
  1. *Biometrics*: `BiometricInputRow` components for Weight (kg), Waist (cm), Chest (cm), Thigh (cm).
  2. *Subjective Scores*: Four `SubjectiveSlider` components (Sleep Quality, Energy Level, Gut Health, Training Stress — 1–10).
  3. *Progress Photos*: Three `PhotoUploadZone` areas (Front, Side, Back). Each uploads to pre-signed Azure Blob URL on file drop.
  4. *Confirmation*: Summary card + "Submit Check-In" button.
- Uses `useSubmitCheckIn()`, `useGetPhotoUploadUrl()`.
- Toast on success. Redirects back to Dashboard.

---

### Coach Hub

#### Screen 12 — `CoachDashboard` (`/coach/dashboard`)
- **KPI strip**: Active Athletes count, Workout Completion % (ring chart), Pending Check-ins (red alert badge if > 0).
- **Real-Time Live Feed panel**: Scrollable list of `LiveFeedItem` components showing recent workout events across all roster athletes. Auto-refreshes every 30 seconds via `refetchInterval: 30000`.
- **Nutrition Compliance panel**: `ComplianceBar` per athlete — Gold fill to target, Red overfill when exceeded. Tapping an athlete navigates to their detail page.
- Uses `useGetCoachDashboard()`, `useGetLiveFeed()`, `useGetComplianceRoster()`.

#### Screen 13 — `ClientRoster` (`/coach/roster`)
- `PaginatedTable` with columns: Athlete name + avatar, Active Program, Macro Compliance %, Last Check-In date, Status badge, "View Profile" link button.
- Filter row: All / Compliance Alert / No Recent Check-In tabs.
- Search input (debounced).
- Uses `useGetRoster(params)`.

#### Screen 14 — `ClientDetail` (`/coach/roster/:athleteId`)
- **Athlete profile header**: avatar, full name, goal, current program, assigned coach.
- **Historical weight trend chart**: `<LineChart>` from Recharts.
- **Macro compliance timeline**: `<BarChart>` from Recharts — daily kcal consumed vs. target for past 14 days.
- **Check-in photo gallery**: Grid of recent check-in photos (Front/Side/Back) with before/after comparison. Clicking photo opens full-screen overlay.
- **Coach Notes**: text area + "Save Note" button via `useSaveCoachNote()`.
- Uses `useGetClientDetail(athleteId)`, `useGetWeightHistory(athleteId)`.

#### Screen 15 — `WorkoutTemplateBuilder` (`/coach/template-builder`)
- **Left panel**: Exercise library selector with category filter tabs. `DraggableExercise` cards.
- **Right canvas**: 6-day week grid using `@dnd-kit`. `WorkoutDayColumn` per day. Drag exercises into day slots.
- Per dropped exercise: configure sets, reps, rest seconds, section (Warm-up/Main/Cool-down), superset grouping.
- "Save Template" button → `useSaveTemplate()` mutation.
- "Assign to Athletes" button → opens modal with athlete multi-select → `useAssignTemplate()`.
- Uses `useGetExercises()`, `useSaveTemplate()`, `useAssignTemplate()`.

#### Screen 16 — `ExerciseLibraryAdmin` (`/coach/exercise-library`)
- Card grid with category filter tabs: Chest, Back, Shoulders, Arms, Legs, Cardio, Core.
- Search bar (debounced).
- Each card: video thumbnail (YouTube), exercise name, muscle group badge, "Edit" / "Delete" action menu.
- "+ Add Exercise" button opens `<AddExerciseModal />`.
- Uses `useGetExercises(params)`, `useDeleteExercise()`.

#### Screens 18+19 — `FoodRecipeAdmin` (`/coach/food-admin`)
- Two tabs:
  - **Foods tab**: Data table of all food items (name, category, kcal/100g, P/C/F). Search + filter. "Edit" / "Delete" actions. "+ Add Food" button. "+ Bulk Import" button opens `<BulkImportModal />`.
  - **Recipes tab**: Card grid of all Joker recipes. Filter by category. "Edit" / "Delete" actions. "+ Create Recipe" opens `<CreateRecipeModal />`.
- Uses `useGetFoods(params)`, `useDeleteFood()`, `useBulkImportFoods()`, `useGetRecipes(params)`, `useDeleteRecipe()`.

#### Screen 3 — `InvitationManagement` (`/coach/invitations`)
- Data table: email, assigned role, status badge (Pending/Accepted/Expired/Revoked), expiry date, copy-link icon, "Resend" and "Revoke" action buttons.
- "+ Invite Member" button → inline form modal: email input, role selector (Athlete/Coach/Admin) → `useCreateInvitation()`.
- Uses `useGetInvitations(params)`, `useResendInvitation()`, `useRevokeInvitation()`.

---

## 17. Naming Conventions

| Category | Convention | Example |
|---|---|---|
| Files | PascalCase | `MacroProgressBar.tsx` |
| Folders (components) | PascalCase | `MacroProgressBar/` |
| Folders (hooks) | camelCase, prefixed `use` | `useDiary/` |
| Folders (pages) | PascalCase | `AthleteDashboard/` |
| API files | camelCase | `diary.ts` |
| Type files | PascalCase for entities, camelCase for util types | `Diary.ts`, `auth.ts` |
| SCSS files | Same name as component | `MacroProgressBar.scss` |
| Query keys | kebab-case array | `['diary', date]`, `['coach-roster', params]` |
| CSS variables | `--kebab-case` | `--color-gold`, `--font-heading` |

---

## 18. Verification Checklist

- [ ] `npm run dev` starts without errors on `localhost:5173`
- [ ] `npm run build` compiles cleanly (no TypeScript errors)
- [ ] `npm run lint` returns no errors
- [ ] Sign-in stores `token` + `user` (with `roles`) in `localStorage`
- [ ] Unauthenticated users are redirected to `/sign-in`
- [ ] Athletes are redirected to `/athlete/dashboard`, Coaches to `/coach/dashboard`
- [ ] All API calls use `axiosInstance` (except auth endpoints using `authApi`)
- [ ] Every `useMutation` shows `antMessage.success` / `antMessage.error` feedback
- [ ] All list pages include pagination via `PaginatedTable` + `PaginationBar`
- [ ] All components have co-located `.scss` files, no inline styles
- [ ] `MacroProgressBar` turns red when `consumed > target`
- [ ] `ComplianceBar` shows red overfill when athlete exceeds calorie target
- [ ] `LiveFeedItem` shows Gold pulse animation for "InProgress" status
- [ ] Workout Logger "Complete Workout" updates streak on Dashboard
- [ ] Weekly Check-In photo upload uses pre-signed URL flow
- [ ] Coach Dashboard live feed auto-refreshes every 30 seconds
- [ ] Ramadan Mode toggle correctly swaps meal tab labels
- [ ] Drag-and-drop in Template Builder correctly places exercises into day columns
- [ ] No `any` types except in explicit error catch blocks
