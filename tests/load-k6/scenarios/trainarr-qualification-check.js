/**
 * M13 load test — TrainArr qualification authorization journey.
 * Product-owner thresholds mirror StlLoadTestSloCatalog.TrainArrQualificationCheckKey.
 */
import { sleep } from 'k6';
import { apiEndpoints, loadDemoCredentials, loadScenarioOptions } from '../lib/stl-config.js';
import { bootstrapProductSession } from '../lib/stl-auth.js';
import { runQualificationCheck } from '../lib/stl-journey.js';

const nexarrBaseUrl = apiEndpoints.find((endpoint) => endpoint.key === 'nexarr').url;
const trainarrBaseUrl = apiEndpoints.find((endpoint) => endpoint.key === 'trainarr').url;

export const options = {
  scenarios: {
    trainarr_qualification_check: loadScenarioOptions(2, '30s'),
  },
  thresholds: {
    http_req_failed: ['rate<0.04'],
    http_req_duration: ['p(95)<10000'],
    http_reqs: ['count>=10'],
  },
};

export default function () {
  const credentials = loadDemoCredentials();
  const trainarrToken = bootstrapProductSession(
    nexarrBaseUrl,
    trainarrBaseUrl,
    'trainarr',
    credentials,
  );
  if (trainarrToken) {
    runQualificationCheck(trainarrBaseUrl, trainarrToken);
  }
  sleep(0.5);
}
