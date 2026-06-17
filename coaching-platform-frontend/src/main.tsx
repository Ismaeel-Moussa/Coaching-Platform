import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import './index.scss';
import { RouterProvider } from 'react-router-dom';
import { router } from './AppRoutes/AppRoutes';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { GlobalConfirmModalProvider } from './contexts/GlobalConfirmModalContext';

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
      <GlobalConfirmModalProvider>
        <RouterProvider router={router} />
      </GlobalConfirmModalProvider>
    </QueryClientProvider>
  </StrictMode>,
);
