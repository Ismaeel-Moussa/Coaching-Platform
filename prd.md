# Product Requirements Document (PRD): Joker Nutrition Platform

## 1. Project Overview
**Joker Nutrition** is an elite, high-performance coaching and nutrition tracking platform. It bridges the gap between scientific nutrition, training programming, and administrative coach operations. The application is divided into two primary experience hubs:
1. **Athlete (Client) Hub**: Focuses on daily target tracking (calories, macros, steps, water), workout split execution, supplement logging, and weekly accountability check-ins.
2. **Coach/Admin Hub**: Focuses on client roster management, nutrition compliance auditing, real-time activity feeds, workout template construction, exercise libraries, and onboarding flow operations.

The user interfaces are designed to be high-contrast, professional, and data-dense, conveying authority, precision, and discipline.

---

## 2. Visual Identity & Design System
Based on the design theme configuration defined in Google Stitch:
*   **Color Palette (Harmonious High-Contrast & Athletic Slate)**:
    *   **Primary Surface / Background**: `#f9f9ff` (Ice white/light violet background)
    *   **Level 0 Canvas / Layout Bg**: `#F3F4F6` (Light gray for desktop margins)
    *   **Primary Branding / Header Container**: `#131a33` / `#0b132b` (Deep Navy Blue)
    *   **Secondary / Active / Accent**: `#fdc003` / `#ffc107` / `#785900` (**Joker Gold**) - Used for active tabs, checkmarks, sliders, progress tracks, and primary call-to-actions.
    *   **Tertiary / Danger / Limit Exceeded**: `#ba1a1a` / `#dc2626` (**Action Red**) - Used for missed schedules, exceeded calorie limits, and urgent alerts.
    *   **Text Base**: `#141b2b` (Dark Charcoal) for optimal legibility.
    *   **Neutral Borders**: `#c6c6ce` / `#76767e` (Slate Grey borders).
*   **Typography**:
    *   **Headings / Title accents**: `Archivo Narrow` (Condensed, punchy, sports-journalism typography weight).
    *   **Body / Inputs**: `Inter` (Clean, legible sans-serif for reading numbers and labels).
    *   **Data Labels / Metrics**: `JetBrains Mono` (Monospace text for calorie counts, macro figures, and technical lists).
*   **Shapes**:
    *   **Low Radius / Soft (0.25rem - 0.5rem)**: Standard buttons use `4px` (`rounded-lg` in context of Stitch custom definitions), card components use `8px` (`rounded-xl`), and avatars or pill badges are circular (`rounded-full`).
*   **Depth**:
    *   Depth is achieved through tonal layering and light, crisp borders (`border border-outline-variant/30`) rather than heavy drop shadows. Interactive cards lift slightly on hover (`translate-y-[-2px]`).

---

## 3. Key Modules & Screen Index (Based on Google Stitch)

The application implements the following 19 user-interface screens and modal overlays:

### 3.1 Authentication & Onboarding
1.  **Joker Nutrition - Login**
    *   High-contrast branded entry screen with username, password fields, validation warnings (Action Red), and action buttons styled in Deep Navy/Joker Gold.
2.  **Joker Nutrition - Join the Team**
    *   Athlete onboarding/onramp form where new invitees can sign up, select their target profile, enter baseline measurements, and link up with their assigned coach.
3.  **Joker Nutrition - Invitation Management**
    *   Coach/Staff administrative panel to issue registration links, manage pending invites, edit role-based access claims (Athlete, Coach, Admin), and track registration dates.

### 3.2 Client (Athlete) Portal
4.  **Customer Dashboard**
    *   **Daily Macros Panel**: Bento-grid style layout with progress gauges showing Protein (g), Carbs (g), and Fats (g) consumption out of the daily target budget (e.g., 2400 kcal).
    *   **Today's Session**: Card detailing the active workout routine (e.g., "Hypertrophy Base - Upper Body Focus • 60 mins • 6 Movements") with a "Start Workout" launcher button.
    *   **Daily Targets**: Ring-progress track for Hydration (e.g., "3.0 / 4.0 L" with a quick-add incrementer) and Steps tracker ("4,200 / 7,000 steps").
    *   **Streak Tracker**: Custom indicator highlighting consecutive workout days logged.
5.  **Meal Logger**
    *   Granular daily tracking system divided by meal intervals: Breakfast, Lunch, Dinner, and Snacks.
    *   Displays macro counts (P, C, F, kCal) per food item and lists raw vs. cooked ingredient metrics.
6.  **Add Food Modal**
    *   Overlay triggerable from the Meal Logger to search ingredients, input quantities in grams, specify state ("Raw", "Cooked", or "Dry"), and calculate instant calorie impacts.
7.  **Recipe Library**
    *   Tabbed interface splitting food items into categories (e.g., "Muscle Building Entrees" (700+ kcal), "Fat Loss Low Calorie Alternatives" (350- kcal), and "Custom User Recipes").
    *   Cards feature cook times, prep steps, macro badges, and a "Quick Add to Diary" shortcut.
8.  **Create Recipe Modal**
    *   Step-by-step wizard for compiling food items, calculating cumulative macros, assigning servings, and specifying whether measurements represent "Before Cooking" or "After Cooking" states.
9.  **Workout Logger**
    *   Timeline selector for a 6-day split (Push / Pull / Legs / Rest).
    *   Exercise cards containing warm-up/main splits, input boxes for tracking lifting weights and repetition counts, and inline play icons linking to video demonstration overlays.
10. **Supplements Tracker**
    *   Daily checklist separating **Essential Supplements** (Creatine, Multivitamins, Omega-3, Vitamin D3) from **Optional/Conditional Supplements**.
11. **Weekly Check-In**
    *   Form where athletes submit weekly progress reports:
        *   *Biometrics*: Weight, waist, chest, and thigh measurements.
        *   *Subjective Slider Scales*: Sleep quality, energy level, gut health, and training stress.
        *   *Photo Drag & Drop Zone*: Private photo uploads covering Front, Side, and Back profiles.

### 3.3 Coach / Administrative Hub
12. **Coach Operations Dashboard**
    *   **Active Roster Summary**: Total Active Athletes, Average Workout Completion %, and Pending Check-ins (flagging urgent/late submissions).
    *   **Real-Time Live Feed**: Stream showing athlete progress in real-time (e.g., "Marcus Thorne - Completed Block B", "Sarah Lopez - Bench Press: Set 3/5 in progress").
    *   **Nutrition Compliance Panel**: Roster compliance view highlighting calories eaten, macro ratios, and highlighting target violations in red (e.g., "Sarah Lopez - Limit Exceeded: 2150 / 1950 kcal").
13. **Client Roster**
    *   Table listing all assigned athletes, their target categories, active program splits, compliance averages, and date of last check-in.
14. **Client Detail View**
    *   Coach's command center for an individual athlete. Displays historical weight trends, diet macro adherence timelines, exercise progression charts, check-in photo history comparison, and a coach feedback notes box.
15. **Workout Template Builder**
    *   Canvas to assemble movement split configurations, link training blocks, design supersets, and configure progressive overload targets.
16. **Exercise Library Admin**
    *   Administrative archive of custom video animations, workout categories, and exercise movement indexes.
17. **Add Exercise Modal**
    *   Modal form to append a new exercise to the global library database, including fields for target muscle groups, instructions, equipment requirements, and YouTube demo links.
18. **Food & Recipe Admin (Unpopulated & Populated Views)**
    *   Directory of certified food materials (with strict calorie-per-100g database seeds) and custom Joker recipes available for assignation.
19. **Bulk Import Modal**
    *   CSV parser and batch copy-paste area for coaches to import nutritional listings in bulk.

---

## 4. Technical Stack & Implementation Guidelines

*   **Frontend Architecture**: React (Vite-powered SPA) using functional components and hooks for state management.
*   **Styling**: Vanilla Tailwind CSS (aligned with the Stitch design system). We will map the custom design tokens (Navy Blue surface colors, Joker Gold, and Archivo Narrow fonts) in a centralized tailwind configuration to keep layouts premium.
*   **State Management & Backend Mocking**: 
    *   To make the prototype fully interactive without needing a complex C# environment setup, we will implement a robust local storage data synchronization mechanism. 
    *   This will simulate the Database with pre-seeded data for exercises, food databases, recipes, and roster profiles, allowing live navigation between the Client Portal and Coach Operations hub.
*   **SEO & Structure**:
    *   Each routed dashboard section will feature unique page titles, semantic HTML5 header structures, and unique testable DOM element IDs.
