import ws from 'k6/ws';
import { check } from 'k6';
import exec from 'k6/execution';
import { WS_URL, TEST_USERS } from './config.js';
import { getAuthToken } from './auth-helper.js';

export const options = {
  vus: 5,         // Simulate 5 concurrent WebSocket clients
  duration: '15s', // Run test for 15 seconds
  thresholds: {
    'checks': ['rate>0.9'], // At least 90% of checks must pass
  },
};

// Retrieve tokens during setup from the pool
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
  const url = `${WS_URL}?access_token=${token}`;

  const res = ws.connect(url, {}, function (socket) {
    socket.on('open', () => {
      // 1. Send SignalR Handshake message (Must end with 0x1E record separator character)
      socket.send('{"protocol":"json","version":1}\u001e');
    });

    socket.on('message', (dataStr) => {
      // Concatenated SignalR messages can arrive in a single WebSocket frame under load,
      // separated by the 0x1E character. We must split and process them.
      const messages = dataStr.split('\u001e');
      for (const msg of messages) {
        if (!msg) continue;

        // 2. Check if server completed handshake successfully
        if (msg === '{}') {
          check(socket, {
            'SignalR handshake complete': () => true,
          });
          continue;
        }

        try {
          const parsed = JSON.parse(msg);
          // 3. Respond to server ping (type 6) to keep connection alive
          if (parsed.type === 6) {
            socket.send('{"type":6}\u001e');
          }
        } catch (e) {
          // Ignore json parse issues for partial or custom messages
        }
      }
    });

    socket.on('close', () => {
      // Connection closed cleanly
    });

    socket.on('error', (e) => {
      // Log errors if they occur
      console.error(`WebSocket Error: ${e.error()}`);
    });

    // Close the connection after 10 seconds to end VU cycle
    socket.setTimeout(() => {
      socket.close();
    }, 10000);
  });

  check(res, {
    'WebSocket handshake succeeded (status 101)': (r) => r.status === 101,
  });
}

