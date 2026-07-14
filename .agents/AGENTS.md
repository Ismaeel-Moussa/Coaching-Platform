# Project-Scoped Rules for JokerNutrition Coaching Platform

These rules instruct Antigravity on how to implement new features, maintain code quality, and follow project conventions for the JokerNutrition Coaching Platform.

---

## 1. Core Feature Implementation Workflow

Whenever implementing a new feature, follow this phase-based workflow:

1. **Research & Scan**:
   - Check existing implementations first. For example, if adding a new dashboard widget, look at existing widgets in the frontend. If adding a service, look at other services under `JokerNutrition.Business/Services`.
   - Never write redundant helper functions or components. Re-use existing patterns, converters, layouts, and hooks.

2. **Backend-First (If API is needed)**:
   - Define data models and EF Core mappings in `JokerNutrition.Data`.
   - Add database migrations if DB schema changes are required.
   - Design DTOs in `JokerNutrition.Business/DTOs` and Mapper configurations in `JokerNutrition.Business/Mappers`.
   - Create Service contracts/implementations in `JokerNutrition.Business/Services` and register them via Autofac.
   - Create API Controllers under `JokerNutrition.Api/Controllers` returning standard `IActionResult` (using custom filters like `ApiExceptionFilter` for error mapping).

3. **Frontend Integration**:
   - Create typescript types/interfaces in `src/types/` representing the backend DTOs.
   - Implement API functions using React query/Axios in the `src/api` or `src/services` directories.
   - Implement business logic via custom hooks (`src/hooks`) or context providers (`src/contexts`) where appropriate.
   - Create reusable presentation components under `src/components`.
   - Assemble full pages in `src/pages` and add routes in `src/AppRoutes`.

4. **Verify & Test**:
   - Ensure the backend builds cleanly: `dotnet build`.
   - Add/update unit tests under `JokerNutrition.Tests`.
   - Run tests: `dotnet test`.
   - Ensure the frontend compiles cleanly and has no TypeScript or lint errors: `npm run build` or similar local checks.
   - Verify localization in Arabic (`ar`) and English (`en`) directories under `src/i18n/locales`.

---

## 2. Backend Coding Standards (.NET Web API)

- **Asynchronous First**: All I/O, database queries, and service methods must use `async` / `await` and return `Task` or `Task<T>`.
- **Dependency Injection**: Use constructor injection. Services must be registered through Autofac configurations (`JokerNutrition.Business/Autofac` or `JokerNutrition.Api/Autofac`).
- **Layers Isolation**:
  - **Controllers**: Keep controllers lean. They should only validate requests, invoke services, and return appropriate HTTP status codes.
  - **Business Services**: Place all core business rules, calculations, and domain logic inside the Business project.
  - **Data Access**: Perform all DB queries via Entity Framework Core. Do not write raw SQL queries unless absolutely necessary and approved.
- **Exception Handling**: Do not write repetitive `try-catch` blocks inside controllers. Let exceptions bubble up to the global `ApiExceptionFilter` (configured in the Api project). Throw specific business exception types when rules are violated.
- **Mapping**: Always use auto-mappers or explicit mapper methods to convert EF Core entities to DTOs before returning them to the client. Never return raw database entities from controllers.

---

## 3. Frontend Coding Standards (React, TS, Vite)

- **Strict TypeScript**: Avoid the use of `any`. Always define proper interfaces or types for API responses, components props, and state.
- **Styling & Premium UI**:
  - Keep styling consistent. Use CSS variables defined in `index.scss` for themes, colors, and spacings.
  - Prioritize rich, premium designs with smooth hover effects, micro-animations, and balanced spacing.
  - Ensure full responsiveness on mobile, tablet, and desktop screens.
- **Internationalization (i18n)**:
  - Do not hardcode user-facing strings.
  - Always add translation keys to Arabic (`ar`) and English (`en`) JSON files in `src/i18n/locales/`.
  - Use the translation hook `useTranslation()` from `react-i18n` (or the equivalent context helper) to load and render strings dynamically.
- **State Management**:
  - Use React Context for global app states (e.g. user session, notifications, current theme).
  - Use standard hooks (`useState`, `useEffect`, `useMemo`) for local component state.
  - Ensure API calls are centralized and use loading/error indicators appropriately.

---

## 4. Verification Checklists

Before declaring a feature complete:
- [ ] Backend builds without warnings/errors: `dotnet build`
- [ ] Backend tests pass: `dotnet test`
- [ ] API endpoints are documented with XML comments in controllers.
- [ ] Frontend compiles without type errors: `npx tsc --noEmit`
- [ ] Localization is complete: All new UI labels are translated in both `ar` and `en` locales.
- [ ] Clean coding: Remove any dead code, unused imports, or `console.log` statements.
