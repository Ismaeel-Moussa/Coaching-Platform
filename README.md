# 🃏 Joker Nutrition Coaching Platform

**Joker Nutrition** is a state-of-the-art, high-performance coaching platform designed to connect fitness and nutrition coaches with athletes (clients). The platform delivers a premium, real-time, and mobile-friendly tracking experience that helps clients hit their goals and enables coaches to monitor compliance, adjust programs dynamically, and build workout/diet templates.

🔗 **Live Platform URL:** <a href="https://www.jokernutrition.app" target="_blank" rel="noopener noreferrer">www.jokernutrition.app</a>

---

## 📩 Want to Try the Project?

Contact us so we can send you an invitation to the website:

*   **Email:** [support@jokernutrition.app](mailto:support@jokernutrition.app)


---

## 🌟 Key Features

### 🏋️‍♂️ For Athletes (Clients)
*   **Dynamic Dashboard:** Real-time calorie & macro rings (protein, carbs, fats), daily check-lists, and workout calendars.
*   **Daily Meal Logger:** Extensive food database search, quick macro logger, and historical daily intake tracker.
*   **Recipe Library:** Create, scale, and save custom recipes with automatic per-portion macronutrient calculations.
*   **Workout Logger:** Record completed sets, reps, load, and notes. View exercise history and instruction guides in-app.
*   **Weekly Check-In:** High-fidelity flow to record weight logs, submit subjective feedback metrics, and upload progress photos.
*   **Supplements Tracker:** Time-scheduled supplement reminders and logs.

### 📋 For Coaches
*   **Coach Dashboard:** Central hub with pending weekly check-ins, client compliance alerts, and a real-time live activity feed.
*   **Client Roster:** Clean overview of all athletes with custom filters and quick metrics.
*   **Client Detail View:** Deep-dive into an athlete's historical weights, daily nutrition charts, chat history, and coach notes.
*   **Workout Template Builder:** Drag-and-drop workflow tool to create reusable workout programs.
*   **Athlete Assignment Hub:** Assign diet targets, meal frameworks, and workout templates to specific athletes.
*   **Database Management:** Admin portals to manage the global exercise library and standard food items.
*   **Invitation Management:** Invite clients via automated email invitations with secure signup tokens.

---

## 🛠️ Technology Stack

### Frontend (`coaching-platform-frontend`)
*   **Core:** React 19, TypeScript, Vite
*   **UI Components:** Ant Design (AntD v5), Custom SCSS styles
*   **State Management:** TanStack React Query (for API queries and caching)
*   **Routing:** React Router v7
*   **Icons:** Hugeicons React
*   **Real-time Hubs:** ASP.NET Core SignalR Client (`@microsoft/signalr`)
*   **Data Visualization:** Recharts (for biometric and nutrition history)
*   **Performance:** Vite PWA capability, Vercel Speed Insights

### Backend (`coaching-platform-backend`)
*   **Runtime:** .NET 10 (ASP.NET Core Web API)
*   **Dependency Injection:** Autofac & Autofac.Extensions.DependencyInjection
*   **Database ORM:** Entity Framework Core (EF Core)
*   **Database:** PostgreSQL (via `Npgsql`)
*   **Real-time Communication:** SignalR WebSockets (Notification Hub)
*   **Authentication:** JWT Bearer authentication, ASP.NET Core Identity
*   **Logging:** Serilog (with file and console logging sinks)
*   **Image Storage:** Azure Blob Storage for secure progress photo uploads
*   **Emails:** Resend API integration
*   **Security:** IP Rate Limiting (via `AspNetCoreRateLimit`)
*   **Documentation:** Swagger UI (Swashbuckle)

---

## 📝 License

This project is private and proprietary. All rights reserved. For any inquiries, please contact [support@jokernutrition.app](mailto:support@jokernutrition.app).
