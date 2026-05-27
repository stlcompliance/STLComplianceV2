/**
 * M13 load test — NexArr platform health aggregation endpoint.
 * Product-owner thresholds mirror StlLoadTestSloCatalog.NexArrPlatformHealthKey.
 */
import http from 'k6/http';
import { check, sleep } from 'k6';

const baseUrl = (__ENV.STL_NEXARR_BASE_URL || 'http://localhost:5101').replace(/\/$/, '');

export const options = {
  scenarios: {
    nexarr_platform_health: {
      executor: 'constant-vus',
      vus: Number(__ENV.STL_LOAD_VUS || 3),
      duration: __ENV.STL_LOAD_DURATION || '30s',
    },
  },
  thresholds: {
    http_req_failed: ['rate<0.03'],
    http_req_duration: ['p(95)<4000'],
    http_reqs: ['count>=20'],
  },
};

export default function () {
  const response = http.get(`${baseUrl}/api/platform/health`);
  check(response, {
    'platform health status 200': (r) => r.status === 200,
    'platform health has products': (r) => {
      try {
        const body = r.json();
        return Array.isArray(body?.products) && body.products.length >= 1;
      } catch {
        return false;
      }
    },
  });
  sleep(0.3);
}
