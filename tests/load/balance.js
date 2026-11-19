import http from 'k6/http';
import { check, fail } from 'k6';

const setting = (name, fallback) => __ENV[name] || fallback;

const config = {
  balanceUrl: setting('BALANCE_URL', 'http://balance:8080'),
  keycloakUrl: setting('KEYCLOAK_URL', 'http://keycloak:8080'),
  realm: setting('KEYCLOAK_REALM', 'cashflow'),
  clientId: setting('KEYCLOAK_CLIENT_ID', 'cashflow-web'),
  username: setting('LOAD_USER', 'proprietario'),
  password: setting('LOAD_PASSWORD', 'cashflow'),
  rate: Number(setting('RATE', '50')),
  duration: setting('DURATION', '5m'),
  preAllocatedVUs: Number(setting('PRE_ALLOCATED_VUS', '50')),
  maxVUs: Number(setting('MAX_VUS', '200')),
  periodDays: Number(setting('PERIOD_DAYS', '30')),
  tokenMaxAgeMs: Number(setting('TOKEN_MAX_AGE_SECONDS', '120')) * 1000,
};

export const options = {
  scenarios: {
    daily_balances: {
      executor: 'constant-arrival-rate',
      rate: config.rate,
      timeUnit: '1s',
      duration: config.duration,
      preAllocatedVUs: config.preAllocatedVUs,
      maxVUs: config.maxVUs,
      gracefulStop: '15s',
    },
  },
  thresholds: {
    http_req_failed: ['rate<0.05'],
    'http_req_duration{endpoint:daily_balances}': ['p(95)<500'],
    dropped_iterations: ['count==0'],
  },
  summaryTrendStats: ['avg', 'min', 'med', 'max', 'p(90)', 'p(95)', 'p(99)'],
};

let token = null;
let tokenIssuedAt = 0;

function requestToken() {
  const response = http.post(
    `${config.keycloakUrl}/realms/${config.realm}/protocol/openid-connect/token`,
    {
      grant_type: 'password',
      client_id: config.clientId,
      username: config.username,
      password: config.password,
    },
    { tags: { endpoint: 'token' } },
  );

  if (response.status !== 200) {
    fail(`Token request failed with status ${response.status}`);
  }

  return response.json('access_token');
}

function currentToken() {
  if (token === null || Date.now() - tokenIssuedAt > config.tokenMaxAgeMs) {
    token = requestToken();
    tokenIssuedAt = Date.now();
  }

  return token;
}

function isoDate(date) {
  return date.toISOString().slice(0, 10);
}

function dailyBalancesUrl() {
  const end = new Date();
  const start = new Date(end.getTime() - config.periodDays * 24 * 60 * 60 * 1000);

  return `${config.balanceUrl}/api/v1/daily-balances?startDate=${isoDate(start)}&endDate=${isoDate(end)}`;
}

export function setup() {
  const response = http.get(dailyBalancesUrl(), {
    headers: { Authorization: `Bearer ${requestToken()}` },
    tags: { endpoint: 'setup' },
  });

  if (response.status !== 200) {
    fail(`Setup request failed with status ${response.status}: ${response.body}`);
  }

  return { rows: response.json().length };
}

export default function () {
  const response = http.get(dailyBalancesUrl(), {
    headers: { Authorization: `Bearer ${currentToken()}` },
    tags: { endpoint: 'daily_balances' },
  });

  check(response, {
    'status is 200': (r) => r.status === 200,
    'returns a JSON array': (r) => Array.isArray(r.json()),
  });
}
