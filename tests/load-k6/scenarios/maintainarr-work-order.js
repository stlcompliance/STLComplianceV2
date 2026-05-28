/**
 * M13 load test — MaintainArr work order journey.
 * Product-owner thresholds mirror StlLoadTestSloCatalog.MaintainArrWorkOrderKey.
 */
import { sleep } from 'k6';
import {
  apiEndpoints,
  loadDemoCredentials,
  loadJourneyDefaults,
  loadScenarioOptions,
} from '../lib/stl-config.js';
import { bootstrapProductSession } from '../lib/stl-auth.js';
import { runMaintainArrWorkOrderJourney } from '../lib/stl-journey.js';

const nexarrBaseUrl = apiEndpoints.find((endpoint) => endpoint.key === 'nexarr').url;
const maintainarrBaseUrl = apiEndpoints.find((endpoint) => endpoint.key === 'maintainarr').url;

export const options = {
  scenarios: {
    maintainarr_work_order: loadScenarioOptions(2, '30s'),
  },
  thresholds: {
    http_req_failed: ['rate<0.05'],
    http_req_duration: ['p(95)<18000'],
    http_reqs: ['count>=6'],
  },
};

export default function () {
  const credentials = loadDemoCredentials();
  const journeyDefaults = loadJourneyDefaults();
  const maintainarrToken = bootstrapProductSession(
    nexarrBaseUrl,
    maintainarrBaseUrl,
    'maintainarr',
    credentials,
  );
  if (maintainarrToken) {
    runMaintainArrWorkOrderJourney(
      maintainarrBaseUrl,
      maintainarrToken,
      journeyDefaults.subjectPersonId,
      `${__VU}-${__ITER}`,
    );
  }
  sleep(0.5);
}
