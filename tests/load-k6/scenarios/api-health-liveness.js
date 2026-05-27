/**
 * M13 load test — all seven product API /health liveness probes.
 * Product-owner thresholds mirror StlLoadTestSloCatalog.ApiHealthLivenessKey.
 */
import http from 'k6/http';
import { check, sleep } from 'k6';

const endpoints = [
  { key: 'nexarr', url: __ENV.STL_NEXARR_BASE_URL || 'http://localhost:5101' },
  { key: 'staffarr', url: __ENV.STL_STAFFARR_BASE_URL || 'http://localhost:5102' },
  { key: 'trainarr', url: __ENV.STL_TRAINARR_BASE_URL || 'http://localhost:5103' },
  { key: 'maintainarr', url: __ENV.STL_MAINTAINARR_BASE_URL || 'http://localhost:5104' },
  { key: 'routarr', url: __ENV.STL_ROUTARR_BASE_URL || 'http://localhost:5105' },
  { key: 'supplyarr', url: __ENV.STL_SUPPLYARR_BASE_URL || 'http://localhost:5106' },
  { key: 'compliancecore', url: __ENV.STL_COMPLIANCECORE_BASE_URL || 'http://localhost:5107' },
];

export const options = {
  scenarios: {
    api_health_liveness: {
      executor: 'constant-vus',
      vus: Number(__ENV.STL_LOAD_VUS || 5),
      duration: __ENV.STL_LOAD_DURATION || '30s',
    },
  },
  thresholds: {
    http_req_failed: ['rate<0.005'],
    http_req_duration: ['p(95)<400'],
    http_reqs: ['count>=50'],
  },
};

export default function () {
  for (const endpoint of endpoints) {
    const response = http.get(`${endpoint.url.replace(/\/$/, '')}/health`);
    check(response, {
      [`${endpoint.key} /health status 200`]: (r) => r.status === 200,
    });
  }
  sleep(0.1);
}
