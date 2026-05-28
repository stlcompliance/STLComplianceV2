/**
 * M13 load test — Compliance Core rule pack evaluate journey.
 * Product-owner thresholds mirror StlLoadTestSloCatalog.ComplianceCoreRuleEvaluateKey.
 */
import { sleep } from 'k6';
import {
  apiEndpoints,
  loadDemoCredentials,
  loadJourneyDefaults,
  loadScenarioOptions,
} from '../lib/stl-config.js';
import { bootstrapProductSession } from '../lib/stl-auth.js';
import { runRulePackEvaluate, seedComplianceCoreJourney } from '../lib/stl-journey.js';

const nexarrBaseUrl = apiEndpoints.find((endpoint) => endpoint.key === 'nexarr').url;
const compliancecoreBaseUrl = apiEndpoints.find((endpoint) => endpoint.key === 'compliancecore').url;

export const options = {
  scenarios: {
    compliancecore_rule_evaluate: loadScenarioOptions(2, '30s'),
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
  const compliancecoreToken = bootstrapProductSession(
    nexarrBaseUrl,
    compliancecoreBaseUrl,
    'compliancecore',
    credentials,
  );
  if (!compliancecoreToken) {
    sleep(0.3);
    return;
  }

  const rulePackId = seedComplianceCoreJourney(
    compliancecoreBaseUrl,
    compliancecoreToken,
    journeyDefaults,
  );
  if (rulePackId) {
    runRulePackEvaluate(
      compliancecoreBaseUrl,
      compliancecoreToken,
      rulePackId,
      journeyDefaults,
    );
  }

  sleep(0.5);
}
