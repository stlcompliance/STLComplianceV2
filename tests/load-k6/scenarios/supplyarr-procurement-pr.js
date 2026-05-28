/**
 * M13 load test — SupplyArr procurement PR journey.
 * Product-owner thresholds mirror StlLoadTestSloCatalog.SupplyArrProcurementPrKey.
 */
import { sleep } from 'k6';
import { apiEndpoints, loadDemoCredentials, loadScenarioOptions } from '../lib/stl-config.js';
import { bootstrapProductSession } from '../lib/stl-auth.js';
import { runSupplyArrProcurement } from '../lib/stl-journey.js';

const nexarrBaseUrl = apiEndpoints.find((endpoint) => endpoint.key === 'nexarr').url;
const supplyarrBaseUrl = apiEndpoints.find((endpoint) => endpoint.key === 'supplyarr').url;

export const options = {
  scenarios: {
    supplyarr_procurement_pr: loadScenarioOptions(2, '30s'),
  },
  thresholds: {
    http_req_failed: ['rate<0.05'],
    http_req_duration: ['p(95)<15000'],
    http_reqs: ['count>=6'],
  },
};

export default function () {
  const credentials = loadDemoCredentials();
  const supplyarrToken = bootstrapProductSession(
    nexarrBaseUrl,
    supplyarrBaseUrl,
    'supplyarr',
    credentials,
  );
  if (supplyarrToken) {
    runSupplyArrProcurement(supplyarrBaseUrl, supplyarrToken, `${__VU}-${__ITER}`);
  }
  sleep(0.5);
}
