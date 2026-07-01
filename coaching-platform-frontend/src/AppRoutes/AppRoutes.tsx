import { createBrowserRouter, Navigate } from 'react-router-dom';
import { lazy, Suspense } from 'react';

import ProtectedRoute from './ProtectedRoute';
import RoleGuard from './RoleGuard';

// Layouts
import AuthLayout from '../layouts/AuthLayout';
import AthleteLayout from '../layouts/AthleteLayout';
import CoachLayout from '../layouts/CoachLayout';
import { NotificationProvider } from '../contexts/NotificationContext';

// Auth pages (eager — small, critical path)
import SignIn from '../pages/auth/SignIn/SignIn';
import JoinTheTeam from '../pages/auth/JoinTheTeam/JoinTheTeam';
import InvalidInvite from '../pages/auth/InvalidInvite/InvalidInvite';
import ForgotPassword from '../pages/auth/ForgotPassword/ForgotPassword';
import ResetPassword from '../pages/auth/ResetPassword/ResetPassword';
import NotFound from '../pages/NotFound/NotFound';

// Athlete pages (lazy — loaded only when athlete logs in)
const AthleteDashboard = lazy(() => import('../pages/athlete/Dashboard/Dashboard'));
const MealLogger = lazy(() => import('../pages/athlete/MealLogger/MealLogger'));
const RecipeLibrary = lazy(() => import('../pages/athlete/RecipeLibrary/RecipeLibrary'));
const WorkoutLogger = lazy(() => import('../pages/athlete/WorkoutLogger/WorkoutLogger'));
const SupplementsTracker = lazy(() => import('../pages/athlete/SupplementsTracker/SupplementsTracker'));
const WeeklyCheckIn = lazy(() => import('../pages/athlete/WeeklyCheckIn/WeeklyCheckIn'));

// Coach pages (lazy — loaded only when coach logs in)
const CoachDashboard = lazy(() => import('../pages/coach/CoachDashboard/CoachDashboard'));
const ClientRoster = lazy(() => import('../pages/coach/ClientRoster/ClientRoster'));
const ClientDetail = lazy(() => import('../pages/coach/ClientDetail/ClientDetail'));
const WorkoutTemplateBuilder = lazy(() => import('../pages/coach/WorkoutTemplateBuilder/WorkoutTemplateBuilder'));
const ExerciseLibraryAdmin = lazy(() => import('../pages/coach/ExerciseLibraryAdmin/ExerciseLibraryAdmin'));
const FoodRecipeAdmin = lazy(() => import('../pages/coach/FoodRecipeAdmin/FoodRecipeAdmin'));
const InvitationManagement = lazy(() => import('../pages/coach/InvitationManagement/InvitationManagement'));
const Profile = lazy(() => import('../pages/shared/Profile/Profile'));
const Notifications = lazy(() => import('../pages/shared/Notifications/Notifications'));

const PageLoader = () => (
  <div className="page-loader">
    <div className="page-loader__spinner" />
  </div>
);

export const router = createBrowserRouter([
  // ── Root redirect ──────────────────────────────────────────────────────────
  { path: '/', element: <Navigate to="/sign-in" replace /> },

  // ── Auth tree (no protection) ──────────────────────────────────────────────
  {
    element: <AuthLayout />,
    children: [
      { path: '/sign-in', element: <SignIn /> },
      { path: '/join-the-team', element: <JoinTheTeam /> },
      { path: '/register', element: <JoinTheTeam /> },
      { path: '/invalid-invite', element: <InvalidInvite /> },
      { path: '/forgot-password', element: <ForgotPassword /> },
      { path: '/reset-password', element: <ResetPassword /> },
    ],
  },

  // ── Athlete tree ───────────────────────────────────────────────────────────
  {
    path: '/athlete',
    element: (
      <ProtectedRoute>
        <RoleGuard allowedRoles={['Athlete']}>
          <NotificationProvider>
            <AthleteLayout />
          </NotificationProvider>
        </RoleGuard>
      </ProtectedRoute>
    ),
    children: [
      { index: true, element: <Navigate to="/athlete/dashboard" replace /> },
      {
        path: 'dashboard',
        element: (
          <Suspense fallback={<PageLoader />}>
            <AthleteDashboard />
          </Suspense>
        ),
      },
      {
        path: 'meal-logger',
        element: (
          <Suspense fallback={<PageLoader />}>
            <MealLogger />
          </Suspense>
        ),
      },
      {
        path: 'recipes',
        element: (
          <Suspense fallback={<PageLoader />}>
            <RecipeLibrary />
          </Suspense>
        ),
      },
      {
        path: 'workouts',
        element: (
          <Suspense fallback={<PageLoader />}>
            <WorkoutLogger />
          </Suspense>
        ),
      },
      {
        path: 'supplements',
        element: (
          <Suspense fallback={<PageLoader />}>
            <SupplementsTracker />
          </Suspense>
        ),
      },
      {
        path: 'check-in',
        element: (
          <Suspense fallback={<PageLoader />}>
            <WeeklyCheckIn />
          </Suspense>
        ),
      },
      {
        path: 'profile',
        element: (
          <Suspense fallback={<PageLoader />}>
            <Profile />
          </Suspense>
        ),
      },
      {
        path: 'notifications',
        element: (
          <Suspense fallback={<PageLoader />}>
            <Notifications />
          </Suspense>
        ),
      },
    ],
  },

  // ── Coach / Admin tree ─────────────────────────────────────────────────────
  {
    path: '/coach',
    element: (
      <ProtectedRoute>
        <RoleGuard allowedRoles={['Coach', 'Admin']}>
          <NotificationProvider>
            <CoachLayout />
          </NotificationProvider>
        </RoleGuard>
      </ProtectedRoute>
    ),
    children: [
      { index: true, element: <Navigate to="/coach/dashboard" replace /> },
      {
        path: 'dashboard',
        element: (
          <Suspense fallback={<PageLoader />}>
            <CoachDashboard />
          </Suspense>
        ),
      },
      {
        path: 'roster',
        element: (
          <Suspense fallback={<PageLoader />}>
            <ClientRoster />
          </Suspense>
        ),
      },
      {
        path: 'roster/:athleteId',
        element: (
          <Suspense fallback={<PageLoader />}>
            <ClientDetail />
          </Suspense>
        ),
      },
      {
        path: 'template-builder',
        element: (
          <Suspense fallback={<PageLoader />}>
            <WorkoutTemplateBuilder />
          </Suspense>
        ),
      },
      {
        path: 'template-builder/:id',
        element: (
          <Suspense fallback={<PageLoader />}>
            <WorkoutTemplateBuilder />
          </Suspense>
        ),
      },
      {
        path: 'exercise-library',
        element: (
          <Suspense fallback={<PageLoader />}>
            <ExerciseLibraryAdmin />
          </Suspense>
        ),
      },
      {
        path: 'food-admin',
        element: (
          <Suspense fallback={<PageLoader />}>
            <FoodRecipeAdmin />
          </Suspense>
        ),
      },
      {
        path: 'invitations',
        element: (
          <Suspense fallback={<PageLoader />}>
            <InvitationManagement />
          </Suspense>
        ),
      },
      {
        path: 'profile',
        element: (
          <Suspense fallback={<PageLoader />}>
            <Profile />
          </Suspense>
        ),
      },
      {
        path: 'notifications',
        element: (
          <Suspense fallback={<PageLoader />}>
            <Notifications />
          </Suspense>
        ),
      },
    ],
  },

  // ── 404 ────────────────────────────────────────────────────────────────────
  { path: '*', element: <NotFound /> },
]);
