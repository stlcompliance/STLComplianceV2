/**
 * M13 load test — StaffArr person readiness journey.
 * Product-owner thresholds mirror StlLoadTestSloCatalog.StaffArrPersonReadinessKey.
 */
import { sleep } from 'k6';
import { apiEndpoints, loadDemoCredentials, loadJourneyDefaults, loadScenarioOptions } from '../lib/stl-config.js';
import { bootstrapProductSession } from '../lib/stl-auth.js';
import { runPersonReadiness } from '../lib/stl-journey.js';

const nexarrBaseUrl = apiEndpoints.find((endpoint) => endpoint.key === 'nexarr').url;
const staffarrBaseUrl = apiEndpoints.find((endpoint) => endpoint.key === 'staffarr').url;

export const options = {
  scenarios: {
    staffarr_person_readiness: loadScenarioOptions(2, '30s'),
  },
  thresholds: {
    http_req_failed: ['rate<0.04'],
    http_req_duration: ['p(95)<8000'],
    http_reqs: ['count>=10'],
  },
};

export default function () {
  const credentials = loadDemoCredentials();
  const journeyDefaults = loadJourneyDefaults();
  const staffarrToken = bootstrapProductSession(
    nexarrBaseUrl,
    staffarrBaseUrl,
    'staffarr',
    credentials,
  );
  if (staffarrToken) {
    runPersonReadiness(staffarrBaseUrl, staffarrToken, journeyDefaults.subjectPersonId);
  }
  sleep(0.5);
}
