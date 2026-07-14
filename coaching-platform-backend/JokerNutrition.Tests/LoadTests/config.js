export const BASE_URL = __ENV.BASE_URL || 'http://localhost:7000/api';
export const WS_URL = __ENV.WS_URL || 'ws://localhost:7000/hubs/notifications';

export const TEST_USERS = {
  coach: {
    email: 'coach@jokernutrition.com',
    password: 'Coach@Joker123!',
  },
  athlete: {
    email: 'athlete@jokernutrition.com',
    password: 'Athlete@Joker123!',
  },
  athletePoolSize: 20,
  coachPoolSize: 5,
};

