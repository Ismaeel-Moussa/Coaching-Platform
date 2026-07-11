import React, { Component } from 'react';
import type { ErrorInfo, ReactNode } from 'react';
import './ErrorBoundary.scss';

interface Props {
  children?: ReactNode;
  fallback?: ReactNode;
}

interface State {
  hasError: boolean;
  error: Error | null;
}

class ErrorBoundary extends Component<Props, State> {
  public state: State = {
    hasError: false,
    error: null,
  };

  public static getDerivedStateFromError(error: Error): State {
    return { hasError: true, error };
  }

  public componentDidCatch(error: Error, errorInfo: ErrorInfo) {
    console.error('Uncaught error caught by ErrorBoundary:', error, errorInfo);

    // Detect dynamic import or chunk loading failure (usually caused by new deployments)
    const isChunkLoadError = error && (
      error.message?.includes('dynamically imported module') ||
      error.message?.includes('Failed to fetch dynamically imported module') ||
      error.name === 'ChunkLoadError'
    );

    if (isChunkLoadError) {
      try {
        const hasReloaded = sessionStorage.getItem('chunk-load-error-reload');
        if (!hasReloaded) {
          sessionStorage.setItem('chunk-load-error-reload', 'true');
          window.location.reload();
          return;
        }
      } catch (e) {
        console.error('Failed to handle chunk load error reload:', e);
        window.location.reload();
        return;
      }
    }
  }

  private handleReload = () => {
    window.location.reload();
  };

  public render() {
    if (this.state.hasError) {
      if (this.props.fallback) {
        return this.props.fallback;
      }

      return (
        <div className="error-boundary">
          <div className="error-boundary__card">
            <div className="error-boundary__logo">JN</div>
            <span className="material-symbols-outlined error-boundary__icon">warning</span>
            <h1 className="error-boundary__title">Something Went Wrong</h1>
            <p className="error-boundary__message">
              We encountered an unexpected error while loading this page.
            </p>
            {this.state.error && (
              <pre className="error-boundary__debug">
                {this.state.error.message}
              </pre>
            )}
            <button className="error-boundary__button" onClick={this.handleReload}>
              Reload Page
            </button>
          </div>
        </div>
      );
    }

    return this.props.children;
  }
}

export default ErrorBoundary;
