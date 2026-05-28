import http from 'k6/http';
import { check } from 'k6';
import { loadJourneyDefaults, normalizeBaseUrl } from './stl-config.js';

const jsonHeaders = { 'Content-Type': 'application/json' };

export function runQualificationCheck(trainarrBaseUrl, accessToken, journeyDefaults = loadJourneyDefaults()) {
  const response = http.post(
    `${normalizeBaseUrl(trainarrBaseUrl)}/api/qualification-checks`,
    JSON.stringify({
      staffarrPersonId: journeyDefaults.subjectPersonId,
      qualificationKey: journeyDefaults.qualificationKey,
      rulePackKey: journeyDefaults.rulePackKey,
      context: null,
    }),
    {
      headers: {
        ...jsonHeaders,
        Authorization: `Bearer ${accessToken}`,
      },
      tags: { name: 'trainarr_qualification_check' },
    },
  );

  check(response, {
    'trainarr qualification check status 200': (r) => r.status === 200,
    'trainarr qualification check returns outcome': (r) => {
      try {
        return Boolean(r.json('outcome'));
      } catch {
        return false;
      }
    },
  });

  return response;
}

export function resolveJourneyTripId(
  routarrBaseUrl,
  accessToken,
  journeyDefaults,
  titleSuffix = '',
) {
  if (journeyDefaults?.journeyTripId) {
    return journeyDefaults.journeyTripId;
  }

  return createTrip(routarrBaseUrl, accessToken, titleSuffix);
}

export function createTrip(routarrBaseUrl, accessToken, titleSuffix = '') {
  const now = Date.now();
  const scheduledStartAt = new Date(now + 2 * 60 * 60 * 1000).toISOString();
  const scheduledEndAt = new Date(now + 6 * 60 * 60 * 1000).toISOString();

  const response = http.post(
    `${normalizeBaseUrl(routarrBaseUrl)}/api/trips`,
    JSON.stringify({
      title: `Load test trip ${titleSuffix}`.trim(),
      description: 'M13 load-test dispatch workflow gate journey',
      vehicleRefKey: null,
      scheduledStartAt,
      scheduledEndAt,
      loads: null,
    }),
    {
      headers: {
        ...jsonHeaders,
        Authorization: `Bearer ${accessToken}`,
      },
      tags: { name: 'routarr_create_trip' },
    },
  );

  check(response, {
    'routarr create trip status 201': (r) => r.status === 201,
    'routarr create trip returns tripId': (r) => {
      try {
        return Boolean(r.json('tripId'));
      } catch {
        return false;
      }
    },
  });

  if (response.status !== 201) {
    return null;
  }

  return response.json('tripId');
}

export function runDispatchWorkflowGateCheck(
  routarrBaseUrl,
  accessToken,
  tripId,
  driverPersonId,
) {
  const response = http.post(
    `${normalizeBaseUrl(routarrBaseUrl)}/api/dispatch-workflow-gates/check`,
    JSON.stringify({
      tripId,
      driverPersonId,
      vehicleRefKey: null,
      assignmentKind: 'driver',
    }),
    {
      headers: {
        ...jsonHeaders,
        Authorization: `Bearer ${accessToken}`,
      },
      tags: { name: 'routarr_dispatch_workflow_gate_check' },
    },
  );

  check(response, {
    'routarr workflow gate check status 200': (r) => r.status === 200,
    'routarr workflow gate check returns outcome': (r) => {
      try {
        return Boolean(r.json('outcome'));
      } catch {
        return false;
      }
    },
  });

  return response;
}
