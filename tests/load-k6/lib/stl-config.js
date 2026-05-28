/**
 * Shared environment configuration for STL Compliance k6 scenarios.
 */
export const apiEndpoints = [
  { key: 'nexarr', url: __ENV.STL_NEXARR_BASE_URL || 'http://localhost:5101' },
  { key: 'staffarr', url: __ENV.STL_STAFFARR_BASE_URL || 'http://localhost:5102' },
  { key: 'trainarr', url: __ENV.STL_TRAINARR_BASE_URL || 'http://localhost:5103' },
  { key: 'maintainarr', url: __ENV.STL_MAINTAINARR_BASE_URL || 'http://localhost:5104' },
  { key: 'routarr', url: __ENV.STL_ROUTARR_BASE_URL || 'http://localhost:5105' },
  { key: 'supplyarr', url: __ENV.STL_SUPPLYARR_BASE_URL || 'http://localhost:5106' },
  { key: 'compliancecore', url: __ENV.STL_COMPLIANCECORE_BASE_URL || 'http://localhost:5107' },
];

export const productApiEndpoints = apiEndpoints.filter((endpoint) => endpoint.key !== 'nexarr');

export function normalizeBaseUrl(url) {
  return url.replace(/\/$/, '');
}

export function loadDemoCredentials() {
  return {
    email: __ENV.STL_LOAD_DEMO_EMAIL || 'admin@demo.stl',
    password: __ENV.STL_LOAD_DEMO_PASSWORD || 'ChangeMe!Demo2026',
    tenantId: __ENV.STL_LOAD_DEMO_TENANT_ID || '11111111-1111-1111-1111-111111111101',
  };
}

export function loadJourneyDefaults() {
  const journeyTripId = (__ENV.STL_LOAD_JOURNEY_TRIP_ID || '').trim();
  return {
    subjectPersonId:
      __ENV.STL_LOAD_SUBJECT_PERSON_ID || '22222222-2222-2222-2222-222222222201',
    qualificationKey: __ENV.STL_LOAD_QUALIFICATION_KEY || 'hazmat_endorsement',
    rulePackKey: __ENV.STL_LOAD_RULE_PACK_KEY || 'driver_qualification',
    journeyTripId: journeyTripId.length > 0 ? journeyTripId : null,
  };
}

export function loadScenarioOptions(defaultVus, defaultDuration) {
  return {
    executor: 'constant-vus',
    vus: Number(__ENV.STL_LOAD_VUS || defaultVus),
    duration: __ENV.STL_LOAD_DURATION || defaultDuration,
  };
}
