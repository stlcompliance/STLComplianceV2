/**
 * M13 load test — RoutArr dispatch workflow gate journey.
 * Product-owner thresholds mirror StlLoadTestSloCatalog.RoutArrDispatchWorkflowGateKey.
 */
import { sleep } from 'k6';
import {
  apiEndpoints,
  loadDemoCredentials,
  loadJourneyDefaults,
  loadScenarioOptions,
} from '../lib/stl-config.js';
import { bootstrapProductSession } from '../lib/stl-auth.js';
import { createTrip, runDispatchWorkflowGateCheck } from '../lib/stl-journey.js';

const nexarrBaseUrl = apiEndpoints.find((endpoint) => endpoint.key === 'nexarr').url;
const routarrBaseUrl = apiEndpoints.find((endpoint) => endpoint.key === 'routarr').url;

export const options = {
  scenarios: {
    routarr_dispatch_workflow_gate: loadScenarioOptions(2, '30s'),
  },
  thresholds: {
    http_req_failed: ['rate<0.04'],
    http_req_duration: ['p(95)<12000'],
    http_reqs: ['count>=8'],
  },
};

export default function () {
  const credentials = loadDemoCredentials();
  const journeyDefaults = loadJourneyDefaults();
  const routarrToken = bootstrapProductSession(
    nexarrBaseUrl,
    routarrBaseUrl,
    'routarr',
    credentials,
  );
  if (!routarrToken) {
    sleep(0.3);
    return;
  }

  const tripId = createTrip(routarrBaseUrl, routarrToken, `${__VU}-${__ITER}`);
  if (tripId) {
    runDispatchWorkflowGateCheck(
      routarrBaseUrl,
      routarrToken,
      tripId,
      journeyDefaults.subjectPersonId,
    );
  }

  sleep(0.5);
}
