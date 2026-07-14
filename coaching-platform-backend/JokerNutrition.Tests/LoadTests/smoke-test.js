import http from 'k6/http';
import { check, sleep } from 'k6';
import { BASE_URL } from './config.js';
import { getAuthToken } from './auth-helper.js';

export const options = {
  vus: 1,
  duration: '10s',
  thresholds: {
    http_req_duration: ['p(95)<1000'], // 95% of requests must complete under 1s
    http_req_failed: ['rate<0.01'],    // Less than 1% errors
  },
};

// 1. Authenticate once at start
export function setup() {
  const athleteToken = getAuthToken('athlete');
  const coachToken = getAuthToken('coach');
  return { athleteToken, coachToken };
}

export default function (data) {
  const athleteParams = {
    headers: {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${data.athleteToken}`,
    },
  };

  const coachParams = {
    headers: {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${data.coachToken}`,
    },
  };

  // 2. Query Athlete Dashboard
  const resAthleteDashboard = http.get(`${BASE_URL}/athletes/me/dashboard`, athleteParams);
  check(resAthleteDashboard, {
    'Athlete Dashboard status is 200': (r) => r.status === 200,
    'Athlete Dashboard has athlete info': (r) => r.json('athlete') !== undefined,
  });

  // 3. Query Food Database
  const resFood = http.get(`${BASE_URL}/foods?Search=Chicken`, athleteParams);
  check(resFood, {
    'Food Search status is 200': (r) => r.status === 200,
    'Food Search has items': (r) => Array.isArray(r.json('items')),
  });

  // 4. Query Coach Dashboard
  const resCoachDashboard = http.get(`${BASE_URL}/coach-hub/dashboard`, coachParams);
  check(resCoachDashboard, {
    'Coach Dashboard status is 200': (r) => r.status === 200,
    'Coach Dashboard has active athlete count': (r) => r.json('activeAthleteCount') !== undefined,
  });

  // 5. Query Coach Roster
  const resCoachRoster = http.get(`${BASE_URL}/coach-hub/roster`, coachParams);
  check(resCoachRoster, {
    'Coach Roster status is 200': (r) => r.status === 200,
    'Coach Roster has items': (r) => Array.isArray(r.json('items')),
  });

  sleep(1);
}
