import http from 'k6/http';
import { BASE_URL, TEST_USERS } from './config.js';

/**
 * Authenticates as the specified role and returns the accessToken.
 * @param {string} role - 'coach' or 'athlete'
 * @param {number|null} poolIndex - Optional pool index (1-based, e.g., 1 to poolSize)
 * @returns {string} accessToken
 */
export function getAuthToken(role, poolIndex = null) {
  let email;
  let password;

  if (poolIndex !== null) {
    email = `${role}${poolIndex}@jokernutrition.com`;
    password = role === 'coach' ? 'Coach@Joker123!' : 'Athlete@Joker123!';
  } else {
    const user = TEST_USERS[role];
    if (!user) {
      throw new Error(`Unknown role for load testing: ${role}`);
    }
    email = user.email;
    password = user.password;
  }

  const payload = JSON.stringify({
    email: email,
    password: password,
  });

  const params = {
    headers: {
      'Content-Type': 'application/json',
    },
  };

  const res = http.post(`${BASE_URL}/auth/login`, payload, params);

  if (res.status !== 200) {
    throw new Error(`Authentication failed for ${email}. Status: ${res.status}. Body: ${res.body}`);
  }

  const jsonRes = res.json();
  const accessToken = jsonRes.accessToken;

  if (!accessToken) {
    throw new Error(`Auth response did not contain accessToken for ${email}: ${res.body}`);
  }

  return accessToken;
}

