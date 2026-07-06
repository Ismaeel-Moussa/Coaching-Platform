export interface HealthStatusDto {
  status: 'healthy' | 'degraded';
  database: 'connected' | 'unreachable' | 'error';
  timestamp: string;
  error?: string;
}
