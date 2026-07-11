// Clear the chunk reload flag once the application loads successfully
try {
  sessionStorage.removeItem('chunk-load-error-reload');
} catch (e) {
  // Ignore errors if sessionStorage is not supported/accessible
}

// Handle Vite preload errors (dynamic import failures due to outdated chunk hashes)
window.addEventListener('vite:preloadError', (event) => {
  event.preventDefault();
  try {
    const hasReloaded = sessionStorage.getItem('chunk-load-error-reload');
    if (!hasReloaded) {
      sessionStorage.setItem('chunk-load-error-reload', 'true');
      window.location.reload();
    }
  } catch (e) {
    window.location.reload();
  }
});

import '@ant-design/v5-patch-for-react-19';
import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import './index.scss';
import './i18n/i18n'; // Initialize i18n
import { RouterProvider } from 'react-router-dom';
import { router } from './AppRoutes/AppRoutes';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { GlobalConfirmModalProvider } from './contexts/GlobalConfirmModalContext';
import { LanguageProvider } from './contexts/LanguageContext';
import { SpeedInsights } from '@vercel/speed-insights/react';
import { Genie } from 'genie-react';

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      refetchOnWindowFocus: false,
      retry: false,
      staleTime: 1000 * 60 * 2, // 2 minutes
    },
  },
});

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <QueryClientProvider client={queryClient}>
      <LanguageProvider>
        <GlobalConfirmModalProvider>
          <RouterProvider router={router} />
          <SpeedInsights />
        </GlobalConfirmModalProvider>
      </LanguageProvider>
      {import.meta.env.DEV && <Genie />}
    </QueryClientProvider>
  </StrictMode>,
);


