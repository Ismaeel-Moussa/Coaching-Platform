import http from 'k6/http';
import { check, sleep } from 'k6';
import exec from 'k6/execution';
import { BASE_URL, TEST_USERS } from './config.js';
import { getAuthToken } from './auth-helper.js';

export const options = {
  vus: 5,                // 5 concurrent virtual users
  duration: '15s',       // run for 15 seconds locally
  thresholds: {
    http_req_duration: ['p(95)<600'], // 95% of requests must complete under 600ms
    http_req_failed: ['rate<0.01'],    // Under 1% failures
  },
};

// Retrieve athlete tokens during setup from the pool
export function setup() {
  const athleteTokens = [];
  for (let i = 1; i <= TEST_USERS.athletePoolSize; i++) {
    athleteTokens.push(getAuthToken('athlete', i));
  }
  return { athleteTokens };
}

export default function (data) {
  const vuId = exec.vu.idInTest; // 1-based VU ID
  const tokenIndex = (vuId - 1) % data.athleteTokens.length;
  const token = data.athleteTokens[tokenIndex];

  const params = {
    headers: {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${token}`,
    },
  };

  const todayStr = new Date().toISOString().split('T')[0];

  // 1. Log a breakfast food (Food ID 1: Chicken Breast)
  const logFoodPayload1 = JSON.stringify({
    date: todayStr,
    mealType: 'Breakfast',
    foodId: 1,
    quantityGrams: 150.0,
  });

  const resLog1 = http.post(`${BASE_URL}/diary/log`, logFoodPayload1, params);
  
  let logId1 = null;
  check(resLog1, {
    'Log food 1 status is 201': (r) => r.status === 201,
    'Log food 1 returns log ID': (r) => {
      const body = r.json();
      logId1 = body.id;
      return logId1 !== undefined && logId1 !== null;
    },
  });

  sleep(Math.random() * 2 + 1);

  // 2. Log a lunch food (Food ID 17: White Rice)
  const logFoodPayload2 = JSON.stringify({
    date: todayStr,
    mealType: 'Lunch',
    foodId: 17,
    quantityGrams: 100.0,
  });

  const resLog2 = http.post(`${BASE_URL}/diary/log`, logFoodPayload2, params);

  let logId2 = null;
  check(resLog2, {
    'Log food 2 status is 201': (r) => r.status === 201,
    'Log food 2 returns log ID': (r) => {
      const body = r.json();
      logId2 = body.id;
      return logId2 !== undefined && logId2 !== null;
    },
  });

  sleep(Math.random() * 2 + 1);

  // 3. Update daily water intake (PATCH)
  const waterPayload = JSON.stringify({
    waterLiters: parseFloat((Math.random() * 2 + 1.5).toFixed(1)), // random between 1.5 and 3.5 liters
  });

  const resWater = http.patch(`${BASE_URL}/diary/${todayStr}/water`, waterPayload, params);
  check(resWater, {
    'Update water status is 204': (r) => r.status === 204,
  });

  sleep(Math.random() * 2 + 1);

  // 4. Update steps walked (PATCH)
  const stepsPayload = JSON.stringify({
    steps: Math.floor(Math.random() * 5000 + 5000), // random between 5000 and 10000 steps
  });

  const resSteps = http.patch(`${BASE_URL}/diary/${todayStr}/steps`, stepsPayload, params);
  check(resSteps, {
    'Update steps status is 204': (r) => r.status === 204,
  });

  sleep(Math.random() * 2 + 1);

  // 5. Cleanup: Delete logged food entries to keep DB clean
  if (logId1) {
    const resDel1 = http.del(`${BASE_URL}/diary/log/${logId1}`, null, params);
    check(resDel1, {
      'Delete food 1 status is 204': (r) => r.status === 204,
    });
  }

  if (logId2) {
    const resDel2 = http.del(`${BASE_URL}/diary/log/${logId2}`, null, params);
    check(resDel2, {
      'Delete food 2 status is 204': (r) => r.status === 204,
    });
  }

  sleep(Math.random() * 2 + 1);
}
