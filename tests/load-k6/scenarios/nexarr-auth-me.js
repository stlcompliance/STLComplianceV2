/**
 * M13 load test — NexArr authenticated session bootstrap.
 * Product-owner thresholds mirror StlLoadTestSloCatalog.NexArrAuthMeKey.
 */
import { sleep } from 'k6';
import { apiEndpoints, loadScenarioOptions } from '../lib/stl-config.js';
import { getAuthorizedMe, loginNexArr } from '../lib/stl-auth.js';

const nexarrBaseUrl = apiEndpoints.find((endpoint) => endpoint.key === 'nexarr').url;

export const options = {
  scenarios: {
    nexarr_auth_me: loadScenarioOptions(3, '30s'),
  },
  thresholds: {
    http_req_failed: ['rate<0.01'],
    http_req_duration: ['p(95)<1200'],
    http_reqs: ['count>=30'],
  },
};

export default function () {
  const accessToken = loginNexArr(nexarrBaseUrl);
  if (accessToken) {
    getAuthorizedMe(nexarrBaseUrl, accessToken, 'nexarr');
  }
  sleep(0.2);
}
