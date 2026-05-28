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

export function runPersonReadiness(staffarrBaseUrl, accessToken, personId) {
  const response = http.get(
    `${normalizeBaseUrl(staffarrBaseUrl)}/api/people/${personId}/readiness`,
    {
      headers: { Authorization: `Bearer ${accessToken}` },
      tags: { name: 'staffarr_person_readiness' },
    },
  );

  check(response, {
    'staffarr person readiness status 200': (r) => r.status === 200,
    'staffarr person readiness returns status': (r) => {
      try {
        return Boolean(r.json('readinessStatus'));
      } catch {
        return false;
      }
    },
  });

  return response;
}

export function seedComplianceCoreJourney(compliancecoreBaseUrl, accessToken, journeyDefaults) {
  if (journeyDefaults?.journeyRulePackId) {
    return journeyDefaults.journeyRulePackId;
  }

  const response = http.post(
    `${normalizeBaseUrl(compliancecoreBaseUrl)}/api/load-test-journey/seed`,
    '{}',
    {
      headers: {
        ...jsonHeaders,
        Authorization: `Bearer ${accessToken}`,
      },
      tags: { name: 'compliancecore_journey_seed' },
    },
  );

  check(response, {
    'compliancecore journey seed status 200': (r) => r.status === 200,
    'compliancecore journey seed returns rulePackId': (r) => {
      try {
        return Boolean(r.json('rulePackId'));
      } catch {
        return false;
      }
    },
  });

  if (response.status !== 200) {
    return null;
  }

  return response.json('rulePackId');
}

export function runRulePackEvaluate(
  compliancecoreBaseUrl,
  accessToken,
  rulePackId,
  journeyDefaults = loadJourneyDefaults(),
) {
  const response = http.post(
    `${normalizeBaseUrl(compliancecoreBaseUrl)}/api/rule-packs/${rulePackId}/evaluate`,
    JSON.stringify({
      facts: {
        [journeyDefaults.driverLicenseFactKey]: true,
      },
      emitFindings: false,
    }),
    {
      headers: {
        ...jsonHeaders,
        Authorization: `Bearer ${accessToken}`,
      },
      tags: { name: 'compliancecore_rule_pack_evaluate' },
    },
  );

  check(response, {
    'compliancecore rule evaluate status 201': (r) => r.status === 201,
    'compliancecore rule evaluate returns overallResult': (r) => {
      try {
        return Boolean(r.json('overallResult'));
      } catch {
        return false;
      }
    },
  });

  return response;
}

export function runSupplyArrProcurement(supplyarrBaseUrl, accessToken, titleSuffix = '') {
  const suffix = titleSuffix || `${Date.now()}`;

  const vendorResponse = http.post(
    `${normalizeBaseUrl(supplyarrBaseUrl)}/api/vendors`,
    JSON.stringify({
      partyKey: `load-vendor-${suffix}`,
      displayName: 'Load test vendor',
      legalName: 'Load Test Vendor LLC',
      contactEmail: null,
      notes: '',
    }),
    {
      headers: {
        ...jsonHeaders,
        Authorization: `Bearer ${accessToken}`,
      },
      tags: { name: 'supplyarr_create_vendor' },
    },
  );

  check(vendorResponse, {
    'supplyarr create vendor status 201': (r) => r.status === 201,
  });
  if (vendorResponse.status !== 201) {
    return null;
  }

  const partyId = vendorResponse.json('partyId');

  const partResponse = http.post(
    `${normalizeBaseUrl(supplyarrBaseUrl)}/api/parts`,
    JSON.stringify({
      partKey: `load-part-${suffix}`,
      catalogId: null,
      displayName: 'Load test part',
      description: '',
      categoryKey: 'general',
      unitOfMeasure: 'each',
      manufacturerName: '',
      manufacturerPartNumber: '',
    }),
    {
      headers: {
        ...jsonHeaders,
        Authorization: `Bearer ${accessToken}`,
      },
      tags: { name: 'supplyarr_create_part' },
    },
  );

  check(partResponse, {
    'supplyarr create part status 201': (r) => r.status === 201,
  });
  if (partResponse.status !== 201) {
    return null;
  }

  const partId = partResponse.json('partId');

  const prResponse = http.post(
    `${normalizeBaseUrl(supplyarrBaseUrl)}/api/purchase-requests`,
    JSON.stringify({
      requestKey: `load-pr-${suffix}`,
      title: `Load test PR ${suffix}`,
      notes: 'M13 procurement journey',
      vendorPartyId: partyId,
      lines: [{ partId, quantity: 2, notes: '' }],
    }),
    {
      headers: {
        ...jsonHeaders,
        Authorization: `Bearer ${accessToken}`,
      },
      tags: { name: 'supplyarr_create_purchase_request' },
    },
  );

  check(prResponse, {
    'supplyarr create purchase request status 201': (r) => r.status === 201,
  });
  if (prResponse.status !== 201) {
    return null;
  }

  const purchaseRequestId = prResponse.json('purchaseRequestId');

  const submitResponse = http.post(
    `${normalizeBaseUrl(supplyarrBaseUrl)}/api/purchase-requests/${purchaseRequestId}/submit`,
    null,
    {
      headers: { Authorization: `Bearer ${accessToken}` },
      tags: { name: 'supplyarr_submit_purchase_request' },
    },
  );

  check(submitResponse, {
    'supplyarr submit purchase request status 200': (r) => r.status === 200,
  });

  const approveResponse = http.post(
    `${normalizeBaseUrl(supplyarrBaseUrl)}/api/purchase-requests/${purchaseRequestId}/approve`,
    null,
    {
      headers: { Authorization: `Bearer ${accessToken}` },
      tags: { name: 'supplyarr_approve_purchase_request' },
    },
  );

  check(approveResponse, {
    'supplyarr approve purchase request status 200': (r) => r.status === 200,
  });

  return purchaseRequestId;
}

export function runMaintainArrWorkOrderJourney(
  maintainarrBaseUrl,
  accessToken,
  technicianPersonId,
  titleSuffix = '',
) {
  const suffix = titleSuffix || `${Date.now()}`;

  const assetClassResponse = http.post(
    `${normalizeBaseUrl(maintainarrBaseUrl)}/api/asset-classes`,
    JSON.stringify({
      classKey: `load-class-${suffix}`,
      name: 'Load test production',
      description: 'M13 load-test asset class',
    }),
    {
      headers: {
        ...jsonHeaders,
        Authorization: `Bearer ${accessToken}`,
      },
      tags: { name: 'maintainarr_create_asset_class' },
    },
  );

  check(assetClassResponse, {
    'maintainarr create asset class status 201': (r) => r.status === 201,
  });
  if (assetClassResponse.status !== 201) {
    return null;
  }

  const assetClassId = assetClassResponse.json('assetClassId');

  const assetTypeResponse = http.post(
    `${normalizeBaseUrl(maintainarrBaseUrl)}/api/asset-types`,
    JSON.stringify({
      assetClassId,
      typeKey: `load-type-${suffix}`,
      name: 'Load test conveyor',
      description: 'M13 load-test',
    }),
    {
      headers: {
        ...jsonHeaders,
        Authorization: `Bearer ${accessToken}`,
      },
      tags: { name: 'maintainarr_create_asset_type' },
    },
  );

  check(assetTypeResponse, {
    'maintainarr create asset type status 201': (r) => r.status === 201,
  });
  if (assetTypeResponse.status !== 201) {
    return null;
  }

  const assetTypeId = assetTypeResponse.json('assetTypeId');

  const assetResponse = http.post(
    `${normalizeBaseUrl(maintainarrBaseUrl)}/api/assets`,
    JSON.stringify({
      assetTypeId,
      assetTag: `LOAD-${suffix}`,
      name: 'Load test asset',
      locationLabel: 'Line 1',
      notes: null,
    }),
    {
      headers: {
        ...jsonHeaders,
        Authorization: `Bearer ${accessToken}`,
      },
      tags: { name: 'maintainarr_create_asset' },
    },
  );

  check(assetResponse, {
    'maintainarr create asset status 201': (r) => r.status === 201,
  });
  if (assetResponse.status !== 201) {
    return null;
  }

  const assetId = assetResponse.json('assetId');

  const workOrderResponse = http.post(
    `${normalizeBaseUrl(maintainarrBaseUrl)}/api/work-orders`,
    JSON.stringify({
      assetId,
      title: `Load test WO ${suffix}`,
      description: 'M13 maintainarr work order journey',
      priority: 'medium',
      assignedTechnicianPersonId: technicianPersonId,
      pmScheduleId: null,
    }),
    {
      headers: {
        ...jsonHeaders,
        Authorization: `Bearer ${accessToken}`,
      },
      tags: { name: 'maintainarr_create_work_order' },
    },
  );

  check(workOrderResponse, {
    'maintainarr create work order status 201': (r) => r.status === 201,
  });
  if (workOrderResponse.status !== 201) {
    return null;
  }

  const workOrderId = workOrderResponse.json('workOrderId');

  const getResponse = http.get(
    `${normalizeBaseUrl(maintainarrBaseUrl)}/api/work-orders/${workOrderId}`,
    {
      headers: { Authorization: `Bearer ${accessToken}` },
      tags: { name: 'maintainarr_get_work_order' },
    },
  );

  check(getResponse, {
    'maintainarr get work order status 200': (r) => r.status === 200,
    'maintainarr get work order returns workOrderId': (r) => {
      try {
        return r.json('workOrderId') === workOrderId;
      } catch {
        return false;
      }
    },
  });

  return workOrderId;
}
