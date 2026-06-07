import { afterEach, describe, expect, it, vi } from 'vitest'
import {
  getAssets,
  getAsset,
  getAssetCreateFieldset,
  getAssetEditFieldset,
  getAssetMeters,
  getDefects,
  getWorkOrders,
  getDuePmSchedules,
  getAssetReadiness,
  getAssetReadinessHistory,
  getAssetReadinessFleet,
  getMaintenanceHistory,
  getMaintenancePartsKits,
  createMaintenancePartsKit,
  getMaintenanceVendorWork,
  upsertMaintenanceVendorWork,
  getPmPrograms,
  getPmSchedules,
  getMeterReadings,
  recordMeterReading,
  getInspectionRuns,
  getInspectionTemplates,
  getMaintenanceReportSummary,
  getComplianceReportSummary,
  getExecutiveReportSummary,
  MaintainArrApiError,
  updateAssetControlledV1,
  getWorkOrderLabor,
  logWorkOrderLabor,
  updateWorkOrderLaborStatus,
} from './client'

describe('maintainarr api client', () => {
  afterEach(() => {
    vi.restoreAllMocks()
  })

  it('loads assets successfully', async () => {
    const fetchMock = vi.spyOn(globalThis, 'fetch').mockResolvedValue(
      new Response(
        JSON.stringify([
          {
            assetId: '11111111-1111-1111-1111-111111111111',
            assetTypeId: '22222222-2222-2222-2222-222222222222',
            typeKey: 'excavator',
            typeName: 'Excavator',
            classKey: 'heavy-equipment',
            className: 'Heavy Equipment',
            assetTag: 'EX-1001',
            name: 'Excavator 1001',
            description: 'Primary yard excavator',
            lifecycleStatus: 'active',
            siteRef: 'site-yard-a',
            createdAt: '2026-05-27T00:00:00Z',
            updatedAt: '2026-05-27T00:00:00Z',
          },
        ]),
        { status: 200, headers: { 'Content-Type': 'application/json' } },
      ),
    )

    const result = await getAssets('token-123')
    expect(result).toHaveLength(1)
    expect(result[0]?.assetTag).toBe('EX-1001')
    expect(fetchMock).toHaveBeenCalledWith('/api/assets', expect.any(Object))
  })

  it('throws MaintainArrApiError when assets list is forbidden', async () => {
    vi.spyOn(globalThis, 'fetch').mockResolvedValue(
      new Response('Forbidden', { status: 403, headers: { 'Content-Type': 'text/plain' } }),
    )

    await expect(getAssets('token-123')).rejects.toBeInstanceOf(MaintainArrApiError)
  })

  it('loads one asset and asset fieldsets from v1 endpoints', async () => {
    const assetId = '11111111-1111-1111-1111-111111111111'
    const fetchMock = vi
      .spyOn(globalThis, 'fetch')
      .mockResolvedValueOnce(
        new Response(
          JSON.stringify({
            assetId,
            assetTypeId: '22222222-2222-2222-2222-222222222222',
            typeKey: 'pickup',
            typeName: 'Pickup',
            classKey: 'vehicle',
            className: 'Vehicle',
            assetTag: 'TRK-100',
            name: 'Truck 100',
            description: '',
            lifecycleStatus: 'in_service',
            siteRef: 'North Yard',
            createdAt: '2026-05-27T00:00:00Z',
            updatedAt: '2026-05-27T00:00:00Z',
          }),
          { status: 200, headers: { 'Content-Type': 'application/json' } },
        ),
      )
      .mockResolvedValueOnce(
        new Response(JSON.stringify({ key: 'assets', label: 'Assets', entityType: 'asset', purpose: 'create', fields: [] }), {
          status: 200,
          headers: { 'Content-Type': 'application/json' },
        }),
      )
      .mockResolvedValueOnce(
        new Response(JSON.stringify({ key: 'assets', label: 'Assets', entityType: 'asset', purpose: 'edit', fields: [] }), {
          status: 200,
          headers: { 'Content-Type': 'application/json' },
        }),
      )

    await getAsset('token-123', assetId)
    await getAssetCreateFieldset('token-123')
    await getAssetEditFieldset('token-123', assetId)

    expect(fetchMock).toHaveBeenNthCalledWith(1, `/api/v1/assets/${assetId}`, expect.any(Object))
    expect(fetchMock).toHaveBeenNthCalledWith(2, '/api/v1/fieldsets/assets/create', expect.any(Object))
    expect(fetchMock).toHaveBeenNthCalledWith(3, `/api/v1/fieldsets/assets/${assetId}/edit`, expect.any(Object))
  })

  it('surfaces problem details title/detail in API errors', async () => {
    vi.spyOn(globalThis, 'fetch').mockResolvedValue(
      new Response(
        JSON.stringify({
          title: 'Asset validation failed',
          detail: 'Asset tag is already in use.',
        }),
        { status: 400, headers: { 'Content-Type': 'application/json' } },
      ),
    )

    await expect(getAssets('token-123')).rejects.toMatchObject({
      status: 400,
      message: 'Asset validation failed - Asset tag is already in use.',
    })
  })

  it('surfaces validation errors in API error messages', async () => {
    vi.spyOn(globalThis, 'fetch').mockResolvedValue(
      new Response(
        JSON.stringify({
          title: 'Validation failed',
          errors: {
            assetTag: ['Asset tag is required.'],
            siteRef: ['Site reference is invalid.'],
          },
        }),
        { status: 422, headers: { 'Content-Type': 'application/json' } },
      ),
    )

    await expect(getAssets('token-123')).rejects.toMatchObject({
      status: 422,
      message:
        'Validation failed - assetTag: Asset tag is required.; siteRef: Site reference is invalid.',
    })
  })

  it('explains expired sessions when asset update is unauthorized', async () => {
    const assetId = '11111111-1111-1111-1111-111111111111'
    vi.spyOn(globalThis, 'fetch').mockResolvedValue(new Response('', { status: 401 }))

    await expect(
      updateAssetControlledV1('expired-token', assetId, {
        assetTag: 'TRK-100',
        name: 'Truck 100',
        values: {},
      }),
    ).rejects.toMatchObject({
      status: 401,
      message:
        'Failed to update controlled asset: your MaintainArr session expired before this request was accepted. Launch MaintainArr from the suite again, then retry.',
    })
  })

  it('loads due PM schedules successfully', async () => {
    const fetchMock = vi.spyOn(globalThis, 'fetch').mockResolvedValue(
      new Response(
        JSON.stringify([
          {
            pmScheduleId: '11111111-1111-1111-1111-111111111111',
            assetId: '22222222-2222-2222-2222-222222222222',
            assetTag: 'EX-1001',
            assetName: 'Excavator 1001',
            scheduleKey: 'oil-change',
            name: 'Oil Change',
            description: '',
            scheduleMode: 'calendar',
            assetMeterId: null,
            meterKey: null,
            meterUnit: null,
            intervalUsage: null,
            nextDueAtUsage: null,
            lastCompletedUsage: null,
            intervalDays: 90,
            nextDueAt: '2026-05-20T00:00:00Z',
            lastCompletedAt: null,
            dueStatus: 'due',
            status: 'active',
            lastDueScanAt: '2026-05-27T12:00:00Z',
            createdAt: '2026-01-01T00:00:00Z',
            updatedAt: '2026-05-27T12:00:00Z',
          },
        ]),
        { status: 200, headers: { 'Content-Type': 'application/json' } },
      ),
    )

    const result = await getDuePmSchedules('token-123')
    expect(result).toHaveLength(1)
    expect(result[0]?.dueStatus).toBe('due')
    expect(fetchMock).toHaveBeenCalledWith('/api/preventive-maintenance/due', expect.any(Object))
  })

  it('loads PM programs successfully', async () => {
    const fetchMock = vi.spyOn(globalThis, 'fetch').mockResolvedValue(
      new Response(
        JSON.stringify([
          {
            pmProgramId: '11111111-1111-1111-1111-111111111111',
            programKey: 'forklift-pm',
            name: 'Forklift PM Program',
            scopeType: 'asset_type',
            assetTypeId: '22222222-2222-2222-2222-222222222222',
            assetTypeName: 'Forklift',
            assetId: null,
            assetTag: null,
            status: 'draft',
            scheduleCount: 1,
            createdAt: '2026-01-01T00:00:00Z',
            updatedAt: '2026-05-27T12:00:00Z',
          },
        ]),
        { status: 200, headers: { 'Content-Type': 'application/json' } },
      ),
    )

    const result = await getPmPrograms('token-123')
    expect(result).toHaveLength(1)
    expect(result[0]?.programKey).toBe('forklift-pm')
    expect(fetchMock).toHaveBeenCalledWith('/api/preventive-maintenance/programs', expect.any(Object))
  })

  it('loads PM schedules successfully', async () => {
    const fetchMock = vi.spyOn(globalThis, 'fetch').mockResolvedValue(
      new Response(JSON.stringify([]), { status: 200, headers: { 'Content-Type': 'application/json' } }),
    )

    await getPmSchedules('token-123')
    expect(fetchMock).toHaveBeenCalledWith('/api/preventive-maintenance/schedules', expect.any(Object))
  })

  it('loads inspection templates successfully', async () => {
    const fetchMock = vi.spyOn(globalThis, 'fetch').mockResolvedValue(
      new Response(
        JSON.stringify([
          {
            inspectionTemplateId: '11111111-1111-1111-1111-111111111111',
            templateKey: 'daily-walkaround',
            name: 'Daily Walkaround',
            description: '',
            version: 1,
            status: 'draft',
            categoryCount: 0,
            checklistItemCount: 0,
            linkedAssetTypeCount: 0,
            createdAt: '2026-01-01T00:00:00Z',
            updatedAt: '2026-01-01T00:00:00Z',
          },
        ]),
        { status: 200, headers: { 'Content-Type': 'application/json' } },
      ),
    )

    const result = await getInspectionTemplates('token-123')
    expect(result).toHaveLength(1)
    expect(result[0]?.templateKey).toBe('daily-walkaround')
    expect(fetchMock).toHaveBeenCalledWith('/api/inspection-templates', expect.any(Object))
  })

  it('loads inspection runs successfully', async () => {
    const fetchMock = vi.spyOn(globalThis, 'fetch').mockResolvedValue(
      new Response(
        JSON.stringify([
          {
            inspectionRunId: '11111111-1111-1111-1111-111111111111',
            assetId: '22222222-2222-2222-2222-222222222222',
            assetTag: 'FL-001',
            assetName: 'Yard Forklift',
            inspectionTemplateId: '33333333-3333-3333-3333-333333333333',
            templateKey: 'pre-trip',
            templateName: 'Pre-Trip',
            templateVersion: 1,
            status: 'completed',
            result: 'passed',
            startedByUserId: '44444444-4444-4444-4444-444444444444',
            startedAt: '2026-05-27T12:00:00Z',
            completedAt: '2026-05-27T12:30:00Z',
            answerCount: 1,
            requiredItemCount: 1,
          },
        ]),
        { status: 200, headers: { 'Content-Type': 'application/json' } },
      ),
    )

    const result = await getInspectionRuns('token-123')
    expect(result).toHaveLength(1)
    expect(result[0]?.result).toBe('passed')
    expect(fetchMock).toHaveBeenCalledWith('/api/inspections', expect.any(Object))
  })

  it('loads defects successfully', async () => {
    const fetchMock = vi.spyOn(globalThis, 'fetch').mockResolvedValue(
      new Response(
        JSON.stringify([
          {
            defectId: '11111111-1111-1111-1111-111111111111',
            assetId: '22222222-2222-2222-2222-222222222222',
            assetTag: 'FL-001',
            assetName: 'Yard Forklift',
            inspectionRunId: '33333333-3333-3333-3333-333333333333',
            checklistItemId: '44444444-4444-4444-4444-444444444444',
            checklistItemKey: 'brakes-ok',
            title: 'Failed: Brakes operate correctly',
            severity: 'medium',
            status: 'open',
            source: 'inspection_auto',
            reportedByUserId: '55555555-5555-5555-5555-555555555555',
            createdAt: '2026-05-27T12:00:00Z',
            updatedAt: '2026-05-27T12:00:00Z',
            resolvedAt: null,
          },
        ]),
        { status: 200, headers: { 'Content-Type': 'application/json' } },
      ),
    )

    const result = await getDefects('token-123', { status: 'open' })
    expect(result).toHaveLength(1)
    expect(result[0]?.source).toBe('inspection_auto')
    expect(fetchMock).toHaveBeenCalledWith('/api/defects?status=open', expect.any(Object))
  })

  it('loads work orders successfully', async () => {
    const fetchMock = vi.spyOn(globalThis, 'fetch').mockResolvedValue(
      new Response(
        JSON.stringify([
          {
            workOrderId: '11111111-1111-1111-1111-111111111111',
            workOrderNumber: 'WO-20260527-AB12CD34',
            assetId: '22222222-2222-2222-2222-222222222222',
            assetTag: 'FL-001',
            assetName: 'Yard Forklift',
            defectId: '33333333-3333-3333-3333-333333333333',
            pmScheduleId: null,
            title: 'Repair: Hydraulic leak',
            priority: 'high',
            status: 'open',
            source: 'defect',
            assignedTechnicianPersonId: null,
            createdByUserId: '44444444-4444-4444-4444-444444444444',
            createdAt: '2026-05-27T12:00:00Z',
            updatedAt: '2026-05-27T12:00:00Z',
            startedAt: null,
            completedAt: null,
            cancelledAt: null,
          },
        ]),
        { status: 200, headers: { 'Content-Type': 'application/json' } },
      ),
    )

    const result = await getWorkOrders('token-123', { status: 'open' })
    expect(result).toHaveLength(1)
    expect(result[0]?.workOrderNumber).toBe('WO-20260527-AB12CD34')
    expect(fetchMock).toHaveBeenCalledWith('/api/work-orders?status=open', expect.any(Object))
  })

  it('loads asset meters successfully', async () => {
    const assetId = '11111111-1111-1111-1111-111111111111'
    const fetchMock = vi.spyOn(globalThis, 'fetch').mockResolvedValue(
      new Response(
        JSON.stringify([
          {
            assetMeterId: '22222222-2222-2222-2222-222222222222',
            assetId,
            assetTag: 'EX-1001',
            assetName: 'Excavator 1001',
            meterKey: 'engine-hours',
            name: 'Engine hours',
            description: '',
            unit: 'hours',
            baselineReading: 1000,
            currentReading: 1050,
            lastReadingAt: '2026-05-27T12:00:00Z',
            status: 'active',
            createdAt: '2026-01-01T00:00:00Z',
            updatedAt: '2026-05-27T12:00:00Z',
          },
        ]),
        { status: 200, headers: { 'Content-Type': 'application/json' } },
      ),
    )

    const result = await getAssetMeters('token-123', assetId)
    expect(result[0]?.meterKey).toBe('engine-hours')
    expect(fetchMock).toHaveBeenCalledWith(`/api/assets/${assetId}/meters`, expect.any(Object))
  })

  it('records meter reading successfully', async () => {
    const meterId = '22222222-2222-2222-2222-222222222222'
    const fetchMock = vi.spyOn(globalThis, 'fetch').mockResolvedValue(
      new Response(
        JSON.stringify({
          meterReadingId: '33333333-3333-3333-3333-333333333333',
          assetMeterId: meterId,
          assetId: '11111111-1111-1111-1111-111111111111',
          readingValue: 1100,
          deltaFromPrevious: 50,
          readAt: '2026-05-27T12:00:00Z',
          recordedByUserId: '44444444-4444-4444-4444-444444444444',
          notes: '',
          isCorrection: false,
          createdAt: '2026-05-27T12:00:00Z',
        }),
        { status: 201, headers: { 'Content-Type': 'application/json' } },
      ),
    )

    const result = await recordMeterReading('token-123', meterId, {
      readingValue: 1100,
      notes: '',
      isCorrection: false,
    })
    expect(result.readingValue).toBe(1100)
    expect(fetchMock).toHaveBeenCalledWith(
      `/api/meters/${meterId}/readings`,
      expect.objectContaining({ method: 'POST' }),
    )
  })

  it('loads meter readings successfully', async () => {
    const meterId = '22222222-2222-2222-2222-222222222222'
    vi.spyOn(globalThis, 'fetch').mockResolvedValue(
      new Response(JSON.stringify([]), { status: 200, headers: { 'Content-Type': 'application/json' } }),
    )

    await getMeterReadings('token-123', meterId, 25)
    expect(globalThis.fetch).toHaveBeenCalledWith(
      `/api/meters/${meterId}/readings?limit=25`,
      expect.any(Object),
    )
  })

  it('loads maintenance history successfully', async () => {
    const assetId = '11111111-1111-1111-1111-111111111111'
    const fetchMock = vi.spyOn(globalThis, 'fetch').mockResolvedValue(
      new Response(
        JSON.stringify({
          items: [
            {
              entryId: 'inspection:1:completed',
              assetId,
              category: 'inspection',
              eventType: 'inspection_completed',
              title: 'Inspection completed: Pre-Trip',
              detail: 'passed · completed',
              occurredAt: '2026-05-27T12:30:00Z',
              actorUserId: '44444444-4444-4444-4444-444444444444',
              sourceEntityType: 'inspection_run',
              sourceEntityId: '55555555-5555-5555-5555-555555555555',
              relatedEntityId: null,
            },
          ],
          page: 1,
          pageSize: 50,
          totalCount: 1,
          hasNextPage: false,
        }),
        { status: 200, headers: { 'Content-Type': 'application/json' } },
      ),
    )

    const result = await getMaintenanceHistory('token-123', assetId)
    expect(result.items).toHaveLength(1)
    expect(result.items[0]?.eventType).toBe('inspection_completed')
    expect(fetchMock).toHaveBeenCalledWith(
      `/api/maintenance-history?assetId=${assetId}&page=1&pageSize=50`,
      expect.any(Object),
    )
  })

  it('loads asset readiness for a single asset', async () => {
    const assetId = 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa'
    const fetchMock = vi.spyOn(globalThis, 'fetch').mockResolvedValueOnce(
      new Response(
        JSON.stringify({
          assetId,
          assetTag: 'TRK-01',
          assetName: 'Truck 01',
          lifecycleStatus: 'active',
          readinessStatus: 'ready',
          readinessBasis: 'maintenance_clear',
          calculatedAt: '2026-05-27T12:00:00Z',
          blockers: [],
          signals: {
            openCriticalDefectCount: 0,
            openHighDefectCount: 0,
            activeWorkOrderCount: 0,
            pmDueCount: 0,
            pmOverdueCount: 0,
            failedInspectionCount: 0,
          },
        }),
        { status: 200, headers: { 'Content-Type': 'application/json' } },
      ),
    )

    const result = await getAssetReadiness('token-123', assetId)
    expect(result.readinessStatus).toBe('ready')
    expect(fetchMock).toHaveBeenCalledWith(
      `/api/v1/readiness?assetId=${assetId}`,
      expect.any(Object),
    )
  })

  it('loads asset readiness fleet summaries', async () => {
    const fetchMock = vi.spyOn(globalThis, 'fetch').mockResolvedValueOnce(
      new Response(
        JSON.stringify([
          {
            assetId: 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb',
            assetTag: 'TRK-02',
            assetName: 'Truck 02',
            lifecycleStatus: 'active',
            readinessStatus: 'not_ready',
            blockerCount: 2,
            primaryBlockerMessage: 'PM overdue: Oil change',
          },
        ]),
        { status: 200, headers: { 'Content-Type': 'application/json' } },
      ),
    )

    const result = await getAssetReadinessFleet('token-123')
    expect(result).toHaveLength(1)
    expect(result[0]?.readinessStatus).toBe('not_ready')
    expect(fetchMock).toHaveBeenCalledWith('/api/v1/readiness', expect.any(Object))
  })

  it('loads parts kits and posts new kits to the v1 endpoint', async () => {
    const fetchMock = vi
      .spyOn(globalThis, 'fetch')
      .mockResolvedValueOnce(
        new Response(
          JSON.stringify({
            items: [
              {
                partsKitId: '11111111-1111-1111-1111-111111111111',
                kitNumber: 'KIT-001',
                title: 'Brake service kit',
                description: 'Standard brake materials',
                assetTypeApplicability: ['truck'],
                workOrderTypeApplicability: ['corrective'],
                pmPlanRef: null,
                status: 'active',
                lineRefs: [],
                lines: [],
                createdAt: '2026-06-06T00:00:00Z',
                updatedAt: '2026-06-06T00:00:00Z',
              },
            ],
          }),
          { status: 200, headers: { 'Content-Type': 'application/json' } },
        ),
      )
      .mockResolvedValueOnce(
        new Response(
          JSON.stringify({
            partsKitId: '22222222-2222-2222-2222-222222222222',
            kitNumber: 'KIT-002',
            title: 'Filter kit',
            description: '',
            assetTypeApplicability: [],
            workOrderTypeApplicability: [],
            pmPlanRef: null,
            status: 'draft',
            lineRefs: [],
            lines: [],
            createdAt: '2026-06-06T00:00:00Z',
            updatedAt: '2026-06-06T00:00:00Z',
          }),
          { status: 201, headers: { 'Content-Type': 'application/json' } },
        ),
      )

    const list = await getMaintenancePartsKits('token-123')
    const created = await createMaintenancePartsKit('token-123', {
      kitNumber: 'KIT-002',
      title: 'Filter kit',
      description: '',
    })

    expect(list.items[0]?.kitNumber).toBe('KIT-001')
    expect(created.kitNumber).toBe('KIT-002')
    expect(fetchMock).toHaveBeenNthCalledWith(1, '/api/v1/maintenance-parts-kits', expect.any(Object))
    expect(fetchMock).toHaveBeenNthCalledWith(2, '/api/v1/maintenance-parts-kits', expect.any(Object))
  })

  it('loads and updates vendor work on the work-order endpoint', async () => {
    const fetchMock = vi
      .spyOn(globalThis, 'fetch')
      .mockResolvedValueOnce(
        new Response(
          JSON.stringify({
            items: [
              {
                vendorWorkId: 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa',
                workOrderId: 'wo-1',
                supplierRef: 'vendor-01',
                vendorContactSnapshot: 'Vendor Ops',
                status: 'scheduled',
                workDescription: 'Replace drive shaft',
                quoteRecordRef: 'quote-1',
                approvalRef: 'approval-1',
                scheduledAt: '2026-06-06T10:00:00Z',
                completedAt: null,
                costEstimateSnapshot: '$1,200',
                invoiceRecordRef: null,
                warrantyFlag: true,
                notes: 'Bring vendor crane',
                createdAt: '2026-06-06T09:00:00Z',
                updatedAt: '2026-06-06T09:30:00Z',
                duplicate: false,
              },
            ],
          }),
          { status: 200, headers: { 'Content-Type': 'application/json' } },
        ),
      )
      .mockResolvedValueOnce(
        new Response(
          JSON.stringify({
            vendorWorkId: 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa',
            workOrderId: 'wo-1',
            supplierRef: 'vendor-01',
            vendorContactSnapshot: 'Vendor Ops',
            status: 'completed',
            workDescription: 'Replace drive shaft',
            quoteRecordRef: 'quote-1',
            approvalRef: 'approval-1',
            scheduledAt: '2026-06-06T10:00:00Z',
            completedAt: '2026-06-06T18:00:00Z',
            costEstimateSnapshot: '$1,200',
            invoiceRecordRef: 'invoice-1',
            warrantyFlag: true,
            notes: 'Work completed',
            createdAt: '2026-06-06T09:00:00Z',
            updatedAt: '2026-06-06T18:15:00Z',
            duplicate: true,
          }),
          { status: 200, headers: { 'Content-Type': 'application/json' } },
        ),
      )

    const list = await getMaintenanceVendorWork('token-123', 'wo-1')
    const updated = await upsertMaintenanceVendorWork('token-123', 'wo-1', {
      supplierRef: 'vendor-01',
      vendorContactSnapshot: 'Vendor Ops',
      status: 'completed',
      workDescription: 'Replace drive shaft',
      quoteRecordRef: 'quote-1',
      approvalRef: 'approval-1',
      scheduledAt: '2026-06-06T10:00:00Z',
      completedAt: '2026-06-06T18:00:00Z',
      costEstimateSnapshot: '$1,200',
      invoiceRecordRef: 'invoice-1',
      warrantyFlag: true,
      notes: 'Work completed',
    })

    expect(list.items[0]?.supplierRef).toBe('vendor-01')
    expect(updated.status).toBe('completed')
    expect(fetchMock).toHaveBeenNthCalledWith(1, '/api/v1/work-orders/wo-1/vendor-work', expect.any(Object))
    expect(fetchMock).toHaveBeenNthCalledWith(2, '/api/v1/work-orders/wo-1/vendor-work', expect.any(Object))
  })

  it('loads, logs, and approves labor on the work-order endpoint', async () => {
    const fetchMock = vi
      .spyOn(globalThis, 'fetch')
      .mockResolvedValueOnce(
        new Response(
          JSON.stringify([
            {
              laborEntryId: 'labor-1',
              workOrderId: 'wo-1',
              workOrderTaskLineId: null,
              personId: 'person-1',
              hoursWorked: 2.5,
              laborTypeKey: 'regular',
              status: 'submitted',
              notes: 'Checked alignment',
              submittedAt: '2026-06-06T09:00:00Z',
              approvedByPersonId: null,
              approvedAt: null,
              rejectionReason: null,
              loggedByUserId: 'user-1',
              loggedAt: '2026-06-06T09:00:00Z',
            },
          ]),
          { status: 200, headers: { 'Content-Type': 'application/json' } },
        ),
      )
      .mockResolvedValueOnce(
        new Response(
          JSON.stringify({
            laborEntryId: 'labor-2',
            workOrderId: 'wo-1',
            workOrderTaskLineId: null,
            personId: 'person-2',
            hoursWorked: 1.25,
            laborTypeKey: 'overtime',
            status: 'submitted',
            notes: null,
            submittedAt: '2026-06-06T10:00:00Z',
            approvedByPersonId: null,
            approvedAt: null,
            rejectionReason: null,
            loggedByUserId: 'user-2',
            loggedAt: '2026-06-06T10:00:00Z',
          }),
          { status: 201, headers: { 'Content-Type': 'application/json' } },
        ),
      )
      .mockResolvedValueOnce(
        new Response(
          JSON.stringify({
            laborEntryId: 'labor-2',
            workOrderId: 'wo-1',
            workOrderTaskLineId: null,
            personId: 'person-2',
            hoursWorked: 1.25,
            laborTypeKey: 'overtime',
            status: 'approved',
            notes: null,
            submittedAt: '2026-06-06T10:00:00Z',
            approvedByPersonId: 'person-supervisor',
            approvedAt: '2026-06-06T11:00:00Z',
            rejectionReason: null,
            loggedByUserId: 'user-2',
            loggedAt: '2026-06-06T10:00:00Z',
          }),
          { status: 200, headers: { 'Content-Type': 'application/json' } },
        ),
      )

    const list = await getWorkOrderLabor('token-123', 'wo-1')
    const created = await logWorkOrderLabor('token-123', 'wo-1', {
      personId: 'person-2',
      hoursWorked: 1.25,
      laborTypeKey: 'overtime',
      workOrderTaskLineId: null,
      notes: null,
    })
    const approved = await updateWorkOrderLaborStatus('token-123', 'wo-1', 'labor-2', {
      status: 'approved',
    })

    expect(list[0]?.status).toBe('submitted')
    expect(created.status).toBe('submitted')
    expect(approved.approvedByPersonId).toBe('person-supervisor')
    expect(fetchMock).toHaveBeenNthCalledWith(1, '/api/work-orders/wo-1/labor', expect.any(Object))
    expect(fetchMock).toHaveBeenNthCalledWith(2, '/api/work-orders/wo-1/labor', expect.any(Object))
    expect(fetchMock).toHaveBeenNthCalledWith(3, '/api/work-orders/wo-1/labor/labor-2/status', expect.any(Object))
  })

  it('loads asset readiness history', async () => {
    const fetchMock = vi.spyOn(globalThis, 'fetch').mockResolvedValueOnce(
      new Response(
        JSON.stringify({
          assetId: '11111111-1111-1111-1111-111111111111',
          assetTag: 'TRK-01',
          assetName: 'Truck 01',
          totalCount: 1,
          limit: 15,
          items: [
            {
              entryId: 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa',
              statusFieldKey: 'readinessStatus',
              statusValueKey: 'ready',
              notes: 'Cleared after PM completion.',
              changedByPersonId: 'person-1',
              changedAt: '2026-06-03T10:00:00Z',
              createdAt: '2026-06-03T10:00:00Z',
            },
          ],
        }),
        { status: 200, headers: { 'Content-Type': 'application/json' } },
      ),
    )

    const result = await getAssetReadinessHistory('token-123', '11111111-1111-1111-1111-111111111111')
    expect(result.totalCount).toBe(1)
    expect(result.items[0]?.statusFieldKey).toBe('readinessStatus')
    expect(fetchMock).toHaveBeenCalledWith(
      '/api/v1/readiness/history?assetId=11111111-1111-1111-1111-111111111111&limit=15',
      expect.any(Object),
    )
  })

  it('loads maintenance and compliance report summaries from v1 endpoints', async () => {
    const fetchMock = vi
      .spyOn(globalThis, 'fetch')
      .mockResolvedValueOnce(
        new Response(JSON.stringify({ totalAssetCount: 0, assets: [], workOrderStatusCounts: [] }), {
          status: 200,
          headers: { 'Content-Type': 'application/json' },
        }),
      )
      .mockResolvedValueOnce(
        new Response(
          JSON.stringify({ regulatoryKeyMirrorCount: 0, regulatoryKeyGroups: [], templateSummaries: [] }),
          { status: 200, headers: { 'Content-Type': 'application/json' } },
        ),
      )

    await getMaintenanceReportSummary('token-123')
    await getComplianceReportSummary('token-123')

    expect(fetchMock).toHaveBeenNthCalledWith(1, '/api/v1/reports/maintenance/summary', expect.any(Object))
    expect(fetchMock).toHaveBeenNthCalledWith(2, '/api/v1/reports/compliance/summary', expect.any(Object))
  })

  it('loads executive report summary from v1 endpoint', async () => {
    const fetchMock = vi.spyOn(globalThis, 'fetch').mockResolvedValueOnce(
      new Response(JSON.stringify({}), {
        status: 200,
        headers: { 'Content-Type': 'application/json' },
      }),
    )

    await getExecutiveReportSummary('token-123')
    expect(fetchMock).toHaveBeenCalledWith('/api/v1/reports/executive/summary', expect.any(Object))
  })
})
