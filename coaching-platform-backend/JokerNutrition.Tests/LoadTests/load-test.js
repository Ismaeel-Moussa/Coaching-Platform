import http from 'k6/http';
import { check, sleep } from 'k6';
import exec from 'k6/execution';
import { BASE_URL, TEST_USERS } from './config.js';
import { getAuthToken } from './auth-helper.js';

export const options = {
  stages: [
    { duration: '1m', target: 20 },  // Ramp-up
    { duration: '3m', target: 20 },  // Steady load
    { duration: '1m', target: 0 },   // Ramp-down
  ],
  thresholds: {
    http_req_duration: ['p(95)<500'], // 95% of requests should be below 500ms
    http_req_failed: ['rate<0.01'],    // Under 1% failures
  },
};

// One-time login tokens retrieval for user pools
export function setup() {
  const athleteTokens = [];
  const coachTokens = [];

  for (let i = 1; i <= TEST_USERS.athletePoolSize; i++) {
    athleteTokens.push(getAuthToken('athlete', i));
  }

  for (let i = 1; i <= TEST_USERS.coachPoolSize; i++) {
    coachTokens.push(getAuthToken('coach', i));
  }

  return { athleteTokens, coachTokens };
}

export default function (data) {
  const vuId = exec.vu.idInTest; // 1-based VU ID

  const athleteTokenIndex = (vuId - 1) % data.athleteTokens.length;
  const athleteToken = data.athleteTokens[athleteTokenIndex];
  const athleteParams = {
    headers: {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${athleteToken}`,
    },
  };

  const coachTokenIndex = (vuId - 1) % data.coachTokens.length;
  const coachToken = data.coachTokens[coachTokenIndex];
  const coachParams = {
    headers: {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${coachToken}`,
    },
  };

  // 80% of VUs simulate athletes, 20% simulate coaches
  const isAthlete = Math.random() < 0.8;

  if (isAthlete) {

    // --- Athlete Flow ---
    // 1. Visit Dashboard
    const dashboardRes = http.get(`${BASE_URL}/athletes/me/dashboard`, athleteParams);
    check(dashboardRes, {
      'athlete dashboard 200': (r) => r.status === 200,
    });
    sleep(Math.random() * 3 + 2); // 2-5s think time

    // 2. Load Targets
    const targetsRes = http.get(`${BASE_URL}/athletes/me/targets`, athleteParams);
    check(targetsRes, {
      'athlete targets 200 or 404': (r) => r.status === 200 || r.status === 404,
    });
    sleep(Math.random() * 2 + 1);

    // 3. Search Food Database
    const foods = ['Chicken', 'Rice', 'Egg', 'Oats', 'Salmon'];
    const query = foods[Math.floor(Math.random() * foods.length)];
    const foodRes = http.get(`${BASE_URL}/foods?Search=${query}`, athleteParams);
    check(foodRes, {
      'athlete food search 200': (r) => r.status === 200,
      'athlete search contains items': (r) => Array.isArray(r.json('items')),
    });
    sleep(Math.random() * 4 + 2);

  } else {
    // --- Coach Flow ---
    // 1. Load Coach Dashboard
    const coachDbRes = http.get(`${BASE_URL}/coach-hub/dashboard`, coachParams);
    check(coachDbRes, {
      'coach dashboard 200': (r) => r.status === 200,
    });
    sleep(Math.random() * 3 + 2);

    // 2. View Client Roster
    const rosterRes = http.get(`${BASE_URL}/coach-hub/roster`, coachParams);
    check(rosterRes, {
      'coach roster 200': (r) => r.status === 200,
    });
    sleep(Math.random() * 3 + 2);

    // 3. Check Compliance status
    const complianceRes = http.get(`${BASE_URL}/coach-hub/compliance`, coachParams);
    check(complianceRes, {
      'coach compliance 200': (r) => r.status === 200,
    });
    sleep(Math.random() * 2 + 1);

    // 4. View Live Feed
    const feedRes = http.get(`${BASE_URL}/coach-hub/live-feed`, coachParams);
    check(feedRes, {
      'coach live feed 200': (r) => r.status === 200,
    });
    sleep(Math.random() * 3 + 2);
  }
}
