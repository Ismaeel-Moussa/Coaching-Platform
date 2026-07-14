/**
 * WARNING: Running this test will trigger IP-based rate limiting on the local backend
 * (configured for a maximum of 10 login requests per minute in appsettings.Development.json).
 * This will cause your IP to be blocked (429 Too Many Requests) for 1 minute.
 * 
 * Consequently, any other load test run immediately after this one will fail to authenticate
 * during its setup() stage. To run continuous tests:
 * 1. Temporarily increase or disable limits in appsettings.Development.json under "IpRateLimiting"
 * 2. Or, wait 1 minute between test runs.
 */

import http from 'k6/http';
import { check } from 'k6';
import { BASE_URL } from './config.js';

export const options = {
  vus: 1,
  iterations: 105, // Run 105 sequential attempts
};

export default function () {
  const payload = JSON.stringify({
    email: 'ratelimit.test@jokernutrition.com',
    password: 'WrongPassword!',
  });

  const params = {
    headers: {
      'Content-Type': 'application/json',
    },
  };

  const res = http.post(`${BASE_URL}/auth/login`, payload, params);

  // __ITER is a 0-indexed global variable in k6 representing current iteration
  const iteration = __ITER;

  if (iteration < 100) {
    check(res, {
      'Initial requests (< 100) return 401 Unauthorized': (r) => r.status === 401,
    });
  } else {
    check(res, {
      'Exceeded requests (>= 100) return 429 Too Many Requests': (r) => r.status === 429,
    });
  }
}
