import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { buildSemanticKey, normalizeUom } from '@stl/shared-ui'
import { useEffect, useState } from 'react'
import { Navigate, useNavigate, useSearchParams } from 'react-router-dom'
import {
  activateInspectionTemplate,
  completeInspectionRun,
  createDefect,
  createDefectsFromInspectionRun,
  createWorkOrder,
  createWorkOrderFromDefect,
  createWorkOrderTask,
  createWorkOrderPartsDemandLine,
  getWorkOrder,
  getWorkOrderEvidence,
  getWorkOrderLabor,
  getWorkOrderPartsDemand,
  getWorkOrderPartsDemandStatusEvents,
  getWorkOrderSupplyReadiness,
  getWorkOrderTasks,
  getWorkOrders,
  logWorkOrderLabor,
  publishWorkOrderPartsDemand,
  uploadWorkOrderEvidence,
  createAssetMeter,
  createInspectionChecklistItem,
  createInspectionTemplate,
  createInspectionTemplateCategory,
  getAssetClasses,
  getAssetFieldContext,
  getAssetMeters,
  getAssets,
  getAssetTypes,
  getMeterPmForecast,
  getMeterReadings,
  getDefects,
  getDefectEvidence,
  uploadDefectEvidence,
  getInspectionRunEvidence,
  uploadInspectionRunEvidence,
  getDuePmSchedules,
  getPmPrograms,
  getPmProgram,
  getPmSchedules,
  createPmProgram,
  replacePmProgramSchedules,
  activatePmProgram,
  getInspectionRun,
  getInspectionRuns,
  getInspectionTemplate,
  getInspectionTemplates,
  getInspectionVoiceGuidance,
  getTechnicianRefs,
  normalizeInspectionVoiceNumeric,
  getAssetReadiness,
  getAssetReadinessFleet,
  getMaintenanceHistory,
  getMaintenanceHistorySummary,
  getMe,
  recordMeterReading,
  replaceInspectionTemplateAssetTypes,
  startInspectionRun,
  submitInspectionRunAnswers,
  updateDefectStatus,
  updateWorkOrderStatus,
} from '../api/client'
import {
  canCloseWorkOrders,
  canCreateWorkOrders,
  canManageAssets,
  canManageDefectStatus,
  canExportAuditPackage,
  canManageNotificationSettings,
  canViewAllInspectionRuns,
  loadSession,
} from '../auth/sessionStorage'
import {
  isSpeechRecognitionSupported,
  isSpeechSynthesisSupported,
  listenForTranscript,
  parsePassFailTranscript,
  speakPrompt,
} from '../inspections/voiceGuidance'
async function fileToBase64(file: File): Promise<string> {
  const buffer = await file.arrayBuffer()
  const bytes = new Uint8Array(buffer)
  let binary = ''
  bytes.forEach((byte) => {
    binary += String.fromCharCode(byte)
  })
  return btoa(binary)
}

export function useMaintainArrWorkspaceState() {

  const [searchParams] = useSearchParams()
  const navigate = useNavigate()
  const handoff = searchParams.get('handoff')
  const handoffRedirect = handoff
    ? <Navigate to={`/launch?handoff=${encodeURIComponent(handoff)}`} replace />
    : null

  const session = loadSession()
  const accessToken = session?.accessToken ?? ''
  const queryClient = useQueryClient()
  const [selectedAssetId, setSelectedAssetId] = useState<string | null>(null)
  const [templateKey, setTemplateKey] = useState('')
  const [templateName, setTemplateName] = useState('')
  const [templateDescription, setTemplateDescription] = useState('')
  const [selectedTemplateId, setSelectedTemplateId] = useState('')
  const [categoryKey, setCategoryKey] = useState('')
  const [categoryName, setCategoryName] = useState('')
  const [itemKey, setItemKey] = useState('')
  const [itemPrompt, setItemPrompt] = useState('')
  const [itemType, setItemType] = useState('pass_fail')
  const [selectedCategoryId, setSelectedCategoryId] = useState('')
  const [selectedAssetTypeIds, setSelectedAssetTypeIds] = useState<string[]>([])
  const [runAssetId, setRunAssetId] = useState('')
  const [runTemplateId, setRunTemplateId] = useState('')
  const [selectedRunId, setSelectedRunId] = useState('')
  const [answerDrafts, setAnswerDrafts] = useState<
    Record<string, { passFailValue?: string; numericValue?: string; textValue?: string }>
  >({})
  const [defectAssetId, setDefectAssetId] = useState('')
  const [defectTitle, setDefectTitle] = useState('')
  const [defectDescription, setDefectDescription] = useState('')
  const [defectSeverity, setDefectSeverity] = useState('medium')
  const [defectStatusFilter, setDefectStatusFilter] = useState('')
  const [selectedDefectId, setSelectedDefectId] = useState('')
  const [defectEvidenceTypeKey, setDefectEvidenceTypeKey] = useState('defect_photo')
  const [defectEvidenceNotes, setDefectEvidenceNotes] = useState('')
  const [defectEvidenceFile, setDefectEvidenceFile] = useState<File | null>(null)
  const [inspectionEvidenceTypeKey, setInspectionEvidenceTypeKey] = useState('inspection_photo')
  const [inspectionEvidenceNotes, setInspectionEvidenceNotes] = useState('')
  const [inspectionEvidenceFile, setInspectionEvidenceFile] = useState<File | null>(null)
  const [inspectionEvidenceChecklistItemId, setInspectionEvidenceChecklistItemId] = useState('')
  const [workOrderAssetId, setWorkOrderAssetId] = useState('')
  const [workOrderTitle, setWorkOrderTitle] = useState('')
  const [workOrderDescription, setWorkOrderDescription] = useState('')
  const [workOrderPriority, setWorkOrderPriority] = useState('medium')
  const [assignedPersonId, setAssignedPersonId] = useState('')
  const [workOrderStatusFilter, setWorkOrderStatusFilter] = useState('')
  const [selectedWorkOrderId, setSelectedWorkOrderId] = useState('')
  const [creatingWorkOrderDefectId, setCreatingWorkOrderDefectId] = useState<string | null>(null)
  const [woTaskTitle, setWoTaskTitle] = useState('')
  const [woLaborHours, setWoLaborHours] = useState('1')
  const [woLaborTypeKey, setWoLaborTypeKey] = useState('regular')
  const [woLaborPersonId, setWoLaborPersonId] = useState('')
  const [woSelectedTaskLineId, setWoSelectedTaskLineId] = useState('')
  const [woEvidenceTypeKey, setWoEvidenceTypeKey] = useState('completion_photo')
  const [woEvidenceNotes, setWoEvidenceNotes] = useState('')
  const [woEvidenceFile, setWoEvidenceFile] = useState<File | null>(null)
  const [demandPartNumber, setDemandPartNumber] = useState('')
  const [demandSupplyarrPartId, setDemandSupplyarrPartId] = useState('')
  const [demandQuantity, setDemandQuantity] = useState('1')
  const [demandUnitOfMeasure, setDemandUnitOfMeasure] = useState('each')
  const [demandNotes, setDemandNotes] = useState('')
  const [createPurchaseRequestDraft, setCreatePurchaseRequestDraft] = useState(false)
  const [meterAssetId, setMeterAssetId] = useState('')
  const [selectedMeterId, setSelectedMeterId] = useState('')
  const [confirmedMeterKey, setConfirmedMeterKey] = useState<string | null>(null)
  const [meterName, setMeterName] = useState('')
  const [meterUnit, setMeterUnit] = useState('hours')
  const [baselineReading, setBaselineReading] = useState('')
  const [readingValue, setReadingValue] = useState('')
  const [readingNotes, setReadingNotes] = useState('')
  const [programKey, setProgramKey] = useState('')
  const [programName, setProgramName] = useState('')
  const [programDescription, setProgramDescription] = useState('')
  const [programScopeType, setProgramScopeType] = useState('asset_type')
  const [programAssetTypeId, setProgramAssetTypeId] = useState('')
  const [programAssetId, setProgramAssetId] = useState('')
  const [selectedProgramId, setSelectedProgramId] = useState('')
  const [selectedProgramScheduleIds, setSelectedProgramScheduleIds] = useState<string[]>([])
  const [historyAssetId, setHistoryAssetId] = useState('')
  const [voiceGuidanceEnabled, setVoiceGuidanceEnabled] = useState(false)
  const [voiceStatusMessage, setVoiceStatusMessage] = useState<string | null>(null)
  const [isVoiceListening, setIsVoiceListening] = useState(false)
  const [apiError, setApiError] = useState<string | null>(null)

  useEffect(() => {
    setConfirmedMeterKey(null)
  }, [meterName])

  const meQuery = useQuery({
    queryKey: ['maintainarr-me', session?.accessToken],
    queryFn: () => getMe(session!.accessToken),
    enabled: Boolean(session?.accessToken),
    retry: false,
  })

  const classesQuery = useQuery({
    queryKey: ['maintainarr-asset-classes'],
    queryFn: () => getAssetClasses(accessToken),
    enabled: meQuery.isSuccess,
  })

  const typesQuery = useQuery({
    queryKey: ['maintainarr-asset-types'],
    queryFn: () => getAssetTypes(accessToken),
    enabled: meQuery.isSuccess,
  })

  const assetsQuery = useQuery({
    queryKey: ['maintainarr-assets'],
    queryFn: () => getAssets(accessToken),
    enabled: meQuery.isSuccess,
  })

  const assetReadinessFleetQuery = useQuery({
    queryKey: ['maintainarr-asset-readiness-fleet'],
    queryFn: () => getAssetReadinessFleet(accessToken),
    enabled: meQuery.isSuccess,
  })

  const assetReadinessDetailQuery = useQuery({
    queryKey: ['maintainarr-asset-readiness-detail', selectedAssetId],
    queryFn: () => getAssetReadiness(accessToken, selectedAssetId!),
    enabled: meQuery.isSuccess && Boolean(selectedAssetId),
  })

  const assetFieldContextQuery = useQuery({
    queryKey: ['maintainarr-asset-field-context', selectedAssetId],
    queryFn: () => getAssetFieldContext(accessToken, selectedAssetId!),
    enabled: meQuery.isSuccess && Boolean(selectedAssetId),
  })

  const duePmQuery = useQuery({
    queryKey: ['maintainarr-pm-due'],
    queryFn: () => getDuePmSchedules(accessToken),
    enabled: meQuery.isSuccess,
  })

  const pmProgramsQuery = useQuery({
    queryKey: ['maintainarr-pm-programs'],
    queryFn: () => getPmPrograms(accessToken),
    enabled: meQuery.isSuccess,
  })

  const pmProgramDetailQuery = useQuery({
    queryKey: ['maintainarr-pm-program', selectedProgramId],
    queryFn: () => getPmProgram(accessToken, selectedProgramId),
    enabled: meQuery.isSuccess && Boolean(selectedProgramId),
  })

  const pmSchedulesQuery = useQuery({
    queryKey: ['maintainarr-pm-schedules'],
    queryFn: () => getPmSchedules(accessToken),
    enabled: meQuery.isSuccess,
  })

  const templatesQuery = useQuery({
    queryKey: ['maintainarr-inspection-templates'],
    queryFn: () => getInspectionTemplates(accessToken),
    enabled: meQuery.isSuccess,
  })

  const templateDetailQuery = useQuery({
    queryKey: ['maintainarr-inspection-template', selectedTemplateId],
    queryFn: () => getInspectionTemplate(accessToken, selectedTemplateId),
    enabled: meQuery.isSuccess && Boolean(selectedTemplateId),
  })

  const inspectionRunsQuery = useQuery({
    queryKey: ['maintainarr-inspection-runs'],
    queryFn: () => getInspectionRuns(accessToken),
    enabled: meQuery.isSuccess,
  })

  const inspectionRunQuery = useQuery({
    queryKey: ['maintainarr-inspection-run', selectedRunId],
    queryFn: () => getInspectionRun(accessToken, selectedRunId),
    enabled: meQuery.isSuccess && Boolean(selectedRunId),
  })

  const voiceGuidanceQuery = useQuery({
    queryKey: ['maintainarr-inspection-voice-guidance', selectedRunId],
    queryFn: () => getInspectionVoiceGuidance(accessToken, selectedRunId),
    enabled:
      meQuery.isSuccess
      && Boolean(selectedRunId)
      && voiceGuidanceEnabled
      && inspectionRunQuery.data?.status === 'in_progress',
  })

  const technicianRefsQuery = useQuery({
    queryKey: ['maintainarr-technician-refs'],
    queryFn: () => getTechnicianRefs(accessToken),
    enabled: meQuery.isSuccess,
  })

  const defectsQuery = useQuery({
    queryKey: ['maintainarr-defects', defectStatusFilter],
    queryFn: () =>
      getDefects(accessToken, {
        status: defectStatusFilter || undefined,
      }),
    enabled: meQuery.isSuccess,
  })

  const defectEvidenceQuery = useQuery({
    queryKey: ['maintainarr-defect-evidence', selectedDefectId],
    queryFn: () => getDefectEvidence(accessToken, selectedDefectId),
    enabled: meQuery.isSuccess && Boolean(selectedDefectId),
  })

  const inspectionRunEvidenceQuery = useQuery({
    queryKey: ['maintainarr-inspection-run-evidence', selectedRunId],
    queryFn: () => getInspectionRunEvidence(accessToken, selectedRunId),
    enabled: meQuery.isSuccess && Boolean(selectedRunId),
  })

  const workOrdersQuery = useQuery({
    queryKey: ['maintainarr-work-orders', workOrderStatusFilter],
    queryFn: () =>
      getWorkOrders(accessToken, {
        status: workOrderStatusFilter || undefined,
      }),
    enabled: meQuery.isSuccess,
  })

  const maintenanceHistoryQuery = useQuery({
    queryKey: ['maintainarr-maintenance-history', historyAssetId],
    queryFn: () => getMaintenanceHistory(accessToken, historyAssetId, 1, 50),
    enabled: meQuery.isSuccess && Boolean(historyAssetId),
  })

  const maintenanceHistorySummaryQuery = useQuery({
    queryKey: ['maintainarr-maintenance-history-summary', historyAssetId],
    queryFn: () => getMaintenanceHistorySummary(accessToken, historyAssetId),
    enabled: meQuery.isSuccess && Boolean(historyAssetId),
  })

  const workOrderDetailQuery = useQuery({
    queryKey: ['maintainarr-work-order', selectedWorkOrderId],
    queryFn: () => getWorkOrder(accessToken, selectedWorkOrderId),
    enabled: meQuery.isSuccess && Boolean(selectedWorkOrderId),
  })

  const workOrderTasksQuery = useQuery({
    queryKey: ['maintainarr-work-order-tasks', selectedWorkOrderId],
    queryFn: () => getWorkOrderTasks(accessToken, selectedWorkOrderId),
    enabled: meQuery.isSuccess && Boolean(selectedWorkOrderId),
  })

  const workOrderLaborQuery = useQuery({
    queryKey: ['maintainarr-work-order-labor', selectedWorkOrderId],
    queryFn: () => getWorkOrderLabor(accessToken, selectedWorkOrderId),
    enabled: meQuery.isSuccess && Boolean(selectedWorkOrderId),
  })

  const workOrderEvidenceQuery = useQuery({
    queryKey: ['maintainarr-work-order-evidence', selectedWorkOrderId],
    queryFn: () => getWorkOrderEvidence(accessToken, selectedWorkOrderId),
    enabled: meQuery.isSuccess && Boolean(selectedWorkOrderId),
  })

  const workOrderPartsDemandQuery = useQuery({
    queryKey: ['maintainarr-work-order-parts-demand', selectedWorkOrderId],
    queryFn: () => getWorkOrderPartsDemand(accessToken, selectedWorkOrderId),
    enabled: meQuery.isSuccess && Boolean(selectedWorkOrderId),
  })

  const workOrderPartsDemandStatusEventsQuery = useQuery({
    queryKey: ['maintainarr-work-order-parts-demand-status-events', selectedWorkOrderId],
    queryFn: () => getWorkOrderPartsDemandStatusEvents(accessToken, selectedWorkOrderId),
    enabled: meQuery.isSuccess && Boolean(selectedWorkOrderId),
  })

  const workOrderSupplyReadinessQuery = useQuery({
    queryKey: ['maintainarr-work-order-supply-readiness', selectedWorkOrderId],
    queryFn: () => getWorkOrderSupplyReadiness(accessToken, selectedWorkOrderId),
    enabled: meQuery.isSuccess && Boolean(selectedWorkOrderId),
  })

  const assetMetersQuery = useQuery({
    queryKey: ['maintainarr-asset-meters', meterAssetId],
    queryFn: () => getAssetMeters(accessToken, meterAssetId),
    enabled: meQuery.isSuccess && Boolean(meterAssetId),
  })

  const meterReadingsQuery = useQuery({
    queryKey: ['maintainarr-meter-readings', selectedMeterId],
    queryFn: () => getMeterReadings(accessToken, selectedMeterId),
    enabled: meQuery.isSuccess && Boolean(selectedMeterId),
  })

  const meterForecastQuery = useQuery({
    queryKey: ['maintainarr-meter-forecast', selectedMeterId],
    queryFn: () => getMeterPmForecast(accessToken, selectedMeterId),
    enabled: meQuery.isSuccess && Boolean(selectedMeterId),
  })

  const activeTemplates = (templatesQuery.data ?? []).filter((template) => template.status === 'active')

  const canManage = meQuery.data
    ? canManageAssets(meQuery.data.tenantRoleKey, meQuery.data.isPlatformAdmin)
    : false
  const canManageNotifications = meQuery.data
    ? canManageNotificationSettings(meQuery.data.tenantRoleKey, meQuery.data.isPlatformAdmin)
    : false
  const canExportAudit = meQuery.data
    ? canExportAuditPackage(meQuery.data.tenantRoleKey, meQuery.data.isPlatformAdmin)
    : false

  const viewAllRuns = meQuery.data
    ? canViewAllInspectionRuns(meQuery.data.tenantRoleKey, meQuery.data.isPlatformAdmin)
    : false

  const viewAllDefects = viewAllRuns

  const canManageDefects = meQuery.data
    ? canManageDefectStatus(meQuery.data.tenantRoleKey, meQuery.data.isPlatformAdmin)
    : false

  const canCreateWorkOrder = meQuery.data
    ? canCreateWorkOrders(meQuery.data.tenantRoleKey, meQuery.data.isPlatformAdmin)
    : false

  const canCloseWorkOrder = meQuery.data
    ? canCloseWorkOrders(meQuery.data.tenantRoleKey, meQuery.data.isPlatformAdmin)
    : false

  const viewAllWorkOrders = viewAllRuns

  const canExecuteInspections = meQuery.isSuccess

  useEffect(() => {
    if (templateDetailQuery.data) {
      setSelectedAssetTypeIds(templateDetailQuery.data.linkedAssetTypes.map((x) => x.assetTypeId))
    }
  }, [templateDetailQuery.data])

  useEffect(() => {
    if (pmProgramDetailQuery.data) {
      setSelectedProgramScheduleIds(pmProgramDetailQuery.data.schedules.map((x) => x.pmScheduleId))
    }
  }, [pmProgramDetailQuery.data])

  const scopedPmSchedules = (() => {
    const schedules = pmSchedulesQuery.data ?? []
    const program = pmProgramDetailQuery.data
    if (!program) {
      return schedules
    }
    if (program.scopeType === 'asset' && program.assetId) {
      return schedules.filter((schedule) => schedule.assetId === program.assetId)
    }
    if (program.scopeType === 'asset_type' && program.assetTypeId) {
      const assetIds = new Set(
        (assetsQuery.data ?? [])
          .filter((asset) => asset.assetTypeId === program.assetTypeId)
          .map((asset) => asset.assetId),
      )
      return schedules.filter((schedule) => assetIds.has(schedule.assetId))
    }
    return schedules
  })()

  const invalidateRegistry = async () => {
    await Promise.all([
      queryClient.invalidateQueries({ queryKey: ['maintainarr-asset-classes'] }),
      queryClient.invalidateQueries({ queryKey: ['maintainarr-asset-types'] }),
      queryClient.invalidateQueries({ queryKey: ['maintainarr-assets'] }),
    ])
  }

  const invalidateTemplates = async (templateId?: string) => {
    await queryClient.invalidateQueries({ queryKey: ['maintainarr-inspection-templates'] })
    if (templateId) {
      await queryClient.invalidateQueries({ queryKey: ['maintainarr-inspection-template', templateId] })
    }
  }

  const invalidateInspectionRuns = async (runId?: string) => {
    await queryClient.invalidateQueries({ queryKey: ['maintainarr-inspection-runs'] })
    if (runId) {
      await queryClient.invalidateQueries({ queryKey: ['maintainarr-inspection-run', runId] })
    }
  }

  const invalidateDefects = async () => {
    await queryClient.invalidateQueries({ queryKey: ['maintainarr-defects'] })
  }

  const invalidateDefectEvidence = async (defectId: string) => {
    await queryClient.invalidateQueries({ queryKey: ['maintainarr-defect-evidence', defectId] })
    await invalidateDefects()
  }

  const invalidateInspectionRunEvidence = async (inspectionRunId: string) => {
    await queryClient.invalidateQueries({
      queryKey: ['maintainarr-inspection-run-evidence', inspectionRunId],
    })
  }

  const invalidateWorkOrders = async (workOrderId?: string) => {
    await Promise.all([
      queryClient.invalidateQueries({ queryKey: ['maintainarr-work-orders'] }),
      queryClient.invalidateQueries({ queryKey: ['maintainarr-technician-refs'] }),
    ])
    if (workOrderId) {
      await queryClient.invalidateQueries({ queryKey: ['maintainarr-work-order', workOrderId] })
      await queryClient.invalidateQueries({ queryKey: ['maintainarr-work-order-tasks', workOrderId] })
      await queryClient.invalidateQueries({ queryKey: ['maintainarr-work-order-labor', workOrderId] })
      await queryClient.invalidateQueries({ queryKey: ['maintainarr-work-order-evidence', workOrderId] })
      await queryClient.invalidateQueries({ queryKey: ['maintainarr-work-order-parts-demand', workOrderId] })
      await queryClient.invalidateQueries({
        queryKey: ['maintainarr-work-order-parts-demand-status-events', workOrderId],
      })
      await queryClient.invalidateQueries({
        queryKey: ['maintainarr-work-order-supply-readiness', workOrderId],
      })
    }
  }

  const invalidateMeters = async (assetId?: string, meterId?: string) => {
    if (assetId) {
      await queryClient.invalidateQueries({ queryKey: ['maintainarr-asset-meters', assetId] })
    }
    if (meterId) {
      await queryClient.invalidateQueries({ queryKey: ['maintainarr-meter-readings', meterId] })
      await queryClient.invalidateQueries({ queryKey: ['maintainarr-meter-forecast', meterId] })
    }
    await queryClient.invalidateQueries({ queryKey: ['maintainarr-pm-due'] })
  }

  const invalidatePmPrograms = async (pmProgramId?: string) => {
    await queryClient.invalidateQueries({ queryKey: ['maintainarr-pm-programs'] })
    if (pmProgramId) {
      await queryClient.invalidateQueries({ queryKey: ['maintainarr-pm-program', pmProgramId] })
    }
  }

  const resolveMeterKey = () =>
    confirmedMeterKey ??
    buildSemanticKey({
      domain: 'asset',
      kind: 'meter',
      title: meterName,
      existingKeys: (assetMetersQuery.data ?? []).map((item) => item.meterKey),
      maxLength: 128,
    })

  const createTemplateMutation = useMutation({
    mutationFn: () =>
      createInspectionTemplate(accessToken, {
        templateKey,
        name: templateName,
        description: templateDescription,
      }),
    onSuccess: async (created) => {
      setTemplateKey('')
      setTemplateName('')
      setTemplateDescription('')
      setSelectedTemplateId(created.inspectionTemplateId)
      setSelectedAssetTypeIds(created.linkedAssetTypes.map((x) => x.assetTypeId))
      setApiError(null)
      await invalidateTemplates(created.inspectionTemplateId)
    },
    onError: (error) => setApiError(error instanceof Error ? error.message : 'Failed to create inspection template'),
  })

  const createCategoryMutation = useMutation({
    mutationFn: () =>
      createInspectionTemplateCategory(accessToken, selectedTemplateId, {
        categoryKey,
        name: categoryName,
        sortOrder: 10,
      }),
    onSuccess: async () => {
      setCategoryKey('')
      setCategoryName('')
      setApiError(null)
      await invalidateTemplates(selectedTemplateId)
    },
    onError: (error) => setApiError(error instanceof Error ? error.message : 'Failed to create category'),
  })

  const createItemMutation = useMutation({
    mutationFn: () =>
      createInspectionChecklistItem(accessToken, selectedTemplateId, {
        itemKey,
        prompt: itemPrompt,
        itemType,
        isRequired: true,
        sortOrder: 10,
        categoryId: selectedCategoryId || null,
      }),
    onSuccess: async () => {
      setItemKey('')
      setItemPrompt('')
      setApiError(null)
      await invalidateTemplates(selectedTemplateId)
    },
    onError: (error) => setApiError(error instanceof Error ? error.message : 'Failed to create checklist item'),
  })

  const saveAssetTypesMutation = useMutation({
    mutationFn: () =>
      replaceInspectionTemplateAssetTypes(accessToken, selectedTemplateId, selectedAssetTypeIds),
    onSuccess: async () => {
      setApiError(null)
      await invalidateTemplates(selectedTemplateId)
    },
    onError: (error) => setApiError(error instanceof Error ? error.message : 'Failed to save asset type links'),
  })

  const activateTemplateMutation = useMutation({
    mutationFn: () => activateInspectionTemplate(accessToken, selectedTemplateId),
    onSuccess: async () => {
      setApiError(null)
      await invalidateTemplates(selectedTemplateId)
    },
    onError: (error) => setApiError(error instanceof Error ? error.message : 'Failed to activate template'),
  })

  const startRunMutation = useMutation({
    mutationFn: () =>
      startInspectionRun(accessToken, {
        assetId: runAssetId,
        inspectionTemplateId: runTemplateId,
      }),
    onSuccess: async (created) => {
      setSelectedRunId(created.inspectionRunId)
      setAnswerDrafts({})
      setApiError(null)
      await invalidateInspectionRuns(created.inspectionRunId)
    },
    onError: (error) => setApiError(error instanceof Error ? error.message : 'Failed to start inspection run'),
  })

  const submitAnswersMutation = useMutation({
    mutationFn: () => {
      const run = inspectionRunQuery.data
      if (!run) {
        throw new Error('No active inspection run selected')
      }

      const answers = run.checklistItems
        .map((item) => {
          const draft = answerDrafts[item.checklistItemId]
          const existing = run.answers.find((a) => a.checklistItemId === item.checklistItemId)
          if (item.itemType === 'pass_fail') {
            const value = draft?.passFailValue ?? existing?.passFailValue
            if (!value) {
              return null
            }
            return { checklistItemId: item.checklistItemId, passFailValue: value, numericValue: null, textValue: null }
          }
          if (item.itemType === 'numeric') {
            const raw = draft?.numericValue ?? existing?.numericValue?.toString()
            if (!raw) {
              return null
            }
            return {
              checklistItemId: item.checklistItemId,
              passFailValue: null,
              numericValue: Number(raw),
              textValue: null,
            }
          }
          const text = draft?.textValue ?? existing?.textValue
          if (!text) {
            return null
          }
          return { checklistItemId: item.checklistItemId, passFailValue: null, numericValue: null, textValue: text }
        })
        .filter((answer): answer is NonNullable<typeof answer> => answer !== null)

      return submitInspectionRunAnswers(accessToken, run.inspectionRunId, { answers })
    },
    onSuccess: async () => {
      setApiError(null)
      await invalidateInspectionRuns(selectedRunId)
    },
    onError: (error) => setApiError(error instanceof Error ? error.message : 'Failed to submit answers'),
  })

  const completeRunMutation = useMutation({
    mutationFn: () => completeInspectionRun(accessToken, selectedRunId),
    onSuccess: async () => {
      setAnswerDrafts({})
      setApiError(null)
      await invalidateInspectionRuns(selectedRunId)
      await invalidateDefects()
    },
    onError: (error) => setApiError(error instanceof Error ? error.message : 'Failed to complete inspection run'),
  })

  const createDefectsFromRunMutation = useMutation({
    mutationFn: () =>
      createDefectsFromInspectionRun(accessToken, selectedRunId, { checklistItemIds: null }),
    onSuccess: async () => {
      setApiError(null)
      await invalidateDefects()
    },
    onError: (error) =>
      setApiError(error instanceof Error ? error.message : 'Failed to create defects from inspection run'),
  })

  const createMeterMutation = useMutation({
    mutationFn: () =>
      createAssetMeter(accessToken, meterAssetId, {
        meterKey: resolveMeterKey(),
        name: meterName,
        description: '',
        unit: meterUnit,
        baselineReading: Number(baselineReading),
      }),
    onSuccess: async (created) => {
      setConfirmedMeterKey(created.meterKey)
      setMeterName('')
      setBaselineReading('')
      setSelectedMeterId(created.assetMeterId)
      setApiError(null)
      await invalidateMeters(meterAssetId, created.assetMeterId)
    },
    onError: (error) => setApiError(error instanceof Error ? error.message : 'Failed to create meter'),
  })

  const recordMeterReadingMutation = useMutation({
    mutationFn: () =>
      recordMeterReading(accessToken, selectedMeterId, {
        readingValue: Number(readingValue),
        notes: readingNotes,
        isCorrection: false,
      }),
    onSuccess: async () => {
      setReadingValue('')
      setReadingNotes('')
      setApiError(null)
      await invalidateMeters(meterAssetId, selectedMeterId)
    },
    onError: (error) => setApiError(error instanceof Error ? error.message : 'Failed to record meter reading'),
  })

  const createDefectMutation = useMutation({
    mutationFn: () =>
      createDefect(accessToken, {
        assetId: defectAssetId,
        title: defectTitle,
        description: defectDescription,
        severity: defectSeverity,
      }),
    onSuccess: async (created) => {
      setDefectTitle('')
      setDefectDescription('')
      setApiError(null)
      await invalidateDefects()
      if (created.downtimeFollowUp?.deepLinkPath) {
        navigate(created.downtimeFollowUp.deepLinkPath)
      }
    },
    onError: (error) => setApiError(error instanceof Error ? error.message : 'Failed to create defect'),
  })

  const updateDefectStatusMutation = useMutation({
    mutationFn: ({ defectId, status }: { defectId: string; status: string }) =>
      updateDefectStatus(accessToken, defectId, { status }),
    onSuccess: async () => {
      setApiError(null)
      await invalidateDefects()
    },
    onError: (error) => setApiError(error instanceof Error ? error.message : 'Failed to update defect status'),
  })

  const createWorkOrderMutation = useMutation({
    mutationFn: () =>
      createWorkOrder(accessToken, {
        assetId: workOrderAssetId,
        title: workOrderTitle,
        description: workOrderDescription,
        priority: workOrderPriority,
        assignedTechnicianPersonId: assignedPersonId.trim() || null,
        pmScheduleId: null,
      }),
    onSuccess: async (created) => {
      setWorkOrderTitle('')
      setWorkOrderDescription('')
      setSelectedWorkOrderId(created.workOrderId)
      setApiError(null)
      await invalidateWorkOrders(created.workOrderId)
    },
    onError: (error) => setApiError(error instanceof Error ? error.message : 'Failed to create work order'),
  })

  const createWorkOrderFromDefectMutation = useMutation({
    mutationFn: (defectId: string) => {
      setCreatingWorkOrderDefectId(defectId)
      return createWorkOrderFromDefect(accessToken, defectId, {})
    },
    onSuccess: async (created) => {
      setCreatingWorkOrderDefectId(null)
      setSelectedWorkOrderId(created.workOrderId)
      setApiError(null)
      await invalidateWorkOrders(created.workOrderId)
    },
    onError: (error) => {
      setCreatingWorkOrderDefectId(null)
      setApiError(error instanceof Error ? error.message : 'Failed to create work order from defect')
    },
  })

  const updateWorkOrderStatusMutation = useMutation({
    mutationFn: ({ workOrderId, status }: { workOrderId: string; status: string }) =>
      updateWorkOrderStatus(accessToken, workOrderId, { status }),
    onSuccess: async (updated, variables) => {
      setApiError(null)
      await invalidateWorkOrders(variables.workOrderId)
      if (updated.downtimeFollowUp?.deepLinkPath) {
        navigate(updated.downtimeFollowUp.deepLinkPath)
      }
    },
    onError: (error) => setApiError(error instanceof Error ? error.message : 'Failed to update work order status'),
  })

  const addWorkOrderTaskMutation = useMutation({
    mutationFn: () =>
      createWorkOrderTask(accessToken, selectedWorkOrderId, {
        title: woTaskTitle,
        description: null,
        sortOrder: null,
      }),
    onSuccess: async () => {
      setWoTaskTitle('')
      setApiError(null)
      await invalidateWorkOrders(selectedWorkOrderId)
    },
    onError: (error) => setApiError(error instanceof Error ? error.message : 'Failed to add work order task'),
  })

  const logWorkOrderLaborMutation = useMutation({
    mutationFn: () =>
      logWorkOrderLabor(accessToken, selectedWorkOrderId, {
        personId: woLaborPersonId.trim() || session!.personId,
        hoursWorked: Number.parseFloat(woLaborHours),
        laborTypeKey: woLaborTypeKey,
        workOrderTaskLineId: woSelectedTaskLineId || null,
        notes: null,
      }),
    onSuccess: async () => {
      setApiError(null)
      await invalidateWorkOrders(selectedWorkOrderId)
    },
    onError: (error) => setApiError(error instanceof Error ? error.message : 'Failed to log labor'),
  })

  const uploadWorkOrderEvidenceMutation = useMutation({
    mutationFn: async () => {
      if (!woEvidenceFile) {
        throw new Error('Select a file to upload')
      }
      const contentBase64 = await fileToBase64(woEvidenceFile)
      return uploadWorkOrderEvidence(accessToken, selectedWorkOrderId, {
        evidenceTypeKey: woEvidenceTypeKey,
        fileName: woEvidenceFile.name,
        contentType: woEvidenceFile.type || 'application/octet-stream',
        contentBase64,
        notes: woEvidenceNotes || null,
      })
    },
    onSuccess: async () => {
      setWoEvidenceFile(null)
      setWoEvidenceNotes('')
      setApiError(null)
      await invalidateWorkOrders(selectedWorkOrderId)
    },
    onError: (error) => setApiError(error instanceof Error ? error.message : 'Failed to upload evidence'),
  })

  const uploadDefectEvidenceMutation = useMutation({
    mutationFn: async () => {
      if (!defectEvidenceFile || !selectedDefectId) {
        throw new Error('Select a defect and file to upload')
      }
      const contentBase64 = await fileToBase64(defectEvidenceFile)
      return uploadDefectEvidence(accessToken, selectedDefectId, {
        evidenceTypeKey: defectEvidenceTypeKey,
        fileName: defectEvidenceFile.name,
        contentType: defectEvidenceFile.type || 'application/octet-stream',
        contentBase64,
        notes: defectEvidenceNotes || null,
      })
    },
    onSuccess: async () => {
      setDefectEvidenceFile(null)
      setDefectEvidenceNotes('')
      setApiError(null)
      await invalidateDefectEvidence(selectedDefectId)
    },
    onError: (error) =>
      setApiError(error instanceof Error ? error.message : 'Failed to upload defect evidence'),
  })

  const uploadInspectionRunEvidenceMutation = useMutation({
    mutationFn: async () => {
      if (!inspectionEvidenceFile || !selectedRunId) {
        throw new Error('Select an inspection run and file to upload')
      }
      const contentBase64 = await fileToBase64(inspectionEvidenceFile)
      return uploadInspectionRunEvidence(accessToken, selectedRunId, {
        evidenceTypeKey: inspectionEvidenceTypeKey,
        fileName: inspectionEvidenceFile.name,
        contentType: inspectionEvidenceFile.type || 'application/octet-stream',
        contentBase64,
        notes: inspectionEvidenceNotes || null,
        checklistItemId: inspectionEvidenceChecklistItemId || null,
      })
    },
    onSuccess: async () => {
      setInspectionEvidenceFile(null)
      setInspectionEvidenceNotes('')
      setApiError(null)
      await invalidateInspectionRunEvidence(selectedRunId)
    },
    onError: (error) =>
      setApiError(error instanceof Error ? error.message : 'Failed to upload inspection evidence'),
  })

  const addWorkOrderPartsDemandMutation = useMutation({
    mutationFn: () =>
      createWorkOrderPartsDemandLine(accessToken, selectedWorkOrderId, {
        supplyarrPartId: demandSupplyarrPartId || null,
        partNumber: demandPartNumber || null,
        quantityRequested: Number.parseFloat(demandQuantity),
        unitOfMeasure: normalizeUom(demandUnitOfMeasure),
        notes: demandNotes || null,
      }),
    onSuccess: async () => {
      setDemandPartNumber('')
      setDemandSupplyarrPartId('')
      setDemandQuantity('1')
      setDemandNotes('')
      await invalidateWorkOrders(selectedWorkOrderId)
    },
    onError: (error) =>
      setApiError(error instanceof Error ? error.message : 'Failed to add parts demand line'),
  })

  const publishWorkOrderPartsDemandMutation = useMutation({
    mutationFn: () =>
      publishWorkOrderPartsDemand(accessToken, selectedWorkOrderId, {
        createPurchaseRequestDraft,
      }),
    onSuccess: async () => {
      await invalidateWorkOrders(selectedWorkOrderId)
    },
    onError: (error) =>
      setApiError(error instanceof Error ? error.message : 'Failed to publish parts demand'),
  })

  const createPmProgramMutation = useMutation({
    mutationFn: () =>
      createPmProgram(accessToken, {
        programKey,
        name: programName,
        description: programDescription,
        scopeType: programScopeType,
        assetTypeId: programScopeType === 'asset_type' ? programAssetTypeId || null : null,
        assetId: programScopeType === 'asset' ? programAssetId || null : null,
        pmScheduleIds: [],
      }),
    onSuccess: async (created) => {
      setProgramKey('')
      setProgramName('')
      setProgramDescription('')
      setSelectedProgramId(created.pmProgramId)
      setSelectedProgramScheduleIds(created.schedules.map((x) => x.pmScheduleId))
      setApiError(null)
      await invalidatePmPrograms(created.pmProgramId)
    },
    onError: (error) => setApiError(error instanceof Error ? error.message : 'Failed to create PM program'),
  })

  const savePmProgramSchedulesMutation = useMutation({
    mutationFn: () =>
      replacePmProgramSchedules(accessToken, selectedProgramId, {
        pmScheduleIds: selectedProgramScheduleIds,
      }),
    onSuccess: async (updated) => {
      setApiError(null)
      await invalidatePmPrograms(updated.pmProgramId)
    },
    onError: (error) => setApiError(error instanceof Error ? error.message : 'Failed to assign PM schedules'),
  })

  const activatePmProgramMutation = useMutation({
    mutationFn: () => activatePmProgram(accessToken, selectedProgramId),
    onSuccess: async (updated) => {
      setApiError(null)
      await invalidatePmPrograms(updated.pmProgramId)
    },
    onError: (error) => setApiError(error instanceof Error ? error.message : 'Failed to activate PM program'),
  })

  const voiceGuidanceSupported = isSpeechSynthesisSupported() && isSpeechRecognitionSupported()
  const currentVoicePrompt =
    voiceGuidanceQuery.data?.prompts[voiceGuidanceQuery.data.nextUnansweredIndex] ?? null

  const handleReadCurrentPrompt = () => {
    if (!currentVoicePrompt) {
      return
    }
    speakPrompt(currentVoicePrompt.ttsPrompt)
    setVoiceStatusMessage('Reading prompt aloud…')
  }

  const handleListenForAnswer = async () => {
    if (!currentVoicePrompt) {
      return
    }

    setIsVoiceListening(true)
    setVoiceStatusMessage(null)
    try {
      const transcript = await listenForTranscript()
      if (currentVoicePrompt.itemType === 'pass_fail') {
        const value = parsePassFailTranscript(transcript)
        if (!value) {
          throw new Error(`Could not map "${transcript}" to pass, fail, or N/A.`)
        }
        setAnswerDrafts((current) => ({
          ...current,
          [currentVoicePrompt.checklistItemId]: {
            ...current[currentVoicePrompt.checklistItemId],
            passFailValue: value,
          },
        }))
      } else if (currentVoicePrompt.itemType === 'numeric') {
        const normalized = await normalizeInspectionVoiceNumeric(accessToken, transcript)
        if (!normalized.understood || normalized.value == null) {
          throw new Error(`Could not normalize "${transcript}" as a number.`)
        }
        setAnswerDrafts((current) => ({
          ...current,
          [currentVoicePrompt.checklistItemId]: {
            ...current[currentVoicePrompt.checklistItemId],
            numericValue: String(normalized.value),
          },
        }))
      } else {
        setAnswerDrafts((current) => ({
          ...current,
          [currentVoicePrompt.checklistItemId]: {
            ...current[currentVoicePrompt.checklistItemId],
            textValue: transcript,
          },
        }))
      }

      setVoiceStatusMessage(`Captured voice answer: ${transcript}`)
      await queryClient.invalidateQueries({ queryKey: ['maintainarr-inspection-voice-guidance', selectedRunId] })
    } catch (error) {
      setVoiceStatusMessage(error instanceof Error ? error.message : 'Voice capture failed.')
    } finally {
      setIsVoiceListening(false)
    }
  }

  return {
    handoffRedirect,
    ready: Boolean(session && meQuery.data),
    loadingMessage: 'Loading asset workspace…',
    me: meQuery.data!,
    session: session!,
    accessToken,
    apiError,
    searchParams,
    templateKey,
    templateName,
    templateDescription,
    selectedTemplateId,
    categoryKey,
    categoryName,
    itemKey,
    itemPrompt,
    itemType,
    selectedCategoryId,
    selectedAssetTypeIds,
    runAssetId,
    runTemplateId,
    selectedRunId,
    answerDrafts,
    voiceGuidanceEnabled,
    voiceGuidanceSupported,
    voiceGuidanceLoading: voiceGuidanceQuery.isLoading,
    currentVoicePrompt,
    voiceStatusMessage,
    isVoiceListening,
    setVoiceGuidanceEnabled,
    handleReadCurrentPrompt,
    handleListenForAnswer,
    technicianRefs: technicianRefsQuery.data?.items ?? [],
    defectAssetId,
    defectTitle,
    defectDescription,
    defectSeverity,
    defectStatusFilter,
    selectedDefectId,
    defectEvidenceTypeKey,
    defectEvidenceNotes,
    defectEvidenceFile,
    inspectionEvidenceTypeKey,
    inspectionEvidenceNotes,
    inspectionEvidenceFile,
    inspectionEvidenceChecklistItemId,
    workOrderAssetId,
    workOrderTitle,
    workOrderDescription,
    workOrderPriority,
    assignedPersonId,
    workOrderStatusFilter,
    selectedWorkOrderId,
    creatingWorkOrderDefectId,
    woTaskTitle,
    woLaborHours,
    woLaborTypeKey,
    woLaborPersonId,
    woSelectedTaskLineId,
    woEvidenceTypeKey,
    woEvidenceNotes,
    woEvidenceFile,
    demandPartNumber,
    demandSupplyarrPartId,
    demandQuantity,
    demandUnitOfMeasure,
    demandNotes,
    createPurchaseRequestDraft,
    meterAssetId,
    selectedMeterId,
    confirmedMeterKey,
    meterName,
    meterUnit,
    baselineReading,
    readingValue,
    readingNotes,
    programKey,
    programName,
    programDescription,
    programScopeType,
    programAssetTypeId,
    programAssetId,
    selectedProgramId,
    selectedProgramScheduleIds,
    historyAssetId,
    setSelectedAssetId,
    setTemplateKey,
    setTemplateName,
    setTemplateDescription,
    setSelectedTemplateId,
    setCategoryKey,
    setCategoryName,
    setItemKey,
    setItemPrompt,
    setItemType,
    setSelectedCategoryId,
    setSelectedAssetTypeIds,
    setRunAssetId,
    setRunTemplateId,
    setSelectedRunId,
    setAnswerDrafts,
    setDefectAssetId,
    setDefectTitle,
    setDefectDescription,
    setDefectSeverity,
    setDefectStatusFilter,
    setSelectedDefectId,
    setDefectEvidenceTypeKey,
    setDefectEvidenceNotes,
    setDefectEvidenceFile,
    setInspectionEvidenceTypeKey,
    setInspectionEvidenceNotes,
    setInspectionEvidenceFile,
    setInspectionEvidenceChecklistItemId,
    setWorkOrderAssetId,
    setWorkOrderTitle,
    setWorkOrderDescription,
    setWorkOrderPriority,
    setAssignedPersonId,
    setWorkOrderStatusFilter,
    setSelectedWorkOrderId,
    setCreatingWorkOrderDefectId,
    setWoTaskTitle,
    setWoLaborHours,
    setWoLaborTypeKey,
    setWoLaborPersonId,
    setWoSelectedTaskLineId,
    setWoEvidenceTypeKey,
    setWoEvidenceNotes,
    setWoEvidenceFile,
    setDemandPartNumber,
    setDemandSupplyarrPartId,
    setDemandQuantity,
    setDemandUnitOfMeasure,
    setDemandNotes,
    setCreatePurchaseRequestDraft,
    setMeterAssetId,
    setSelectedMeterId,
    setConfirmedMeterKey,
    setMeterName,
    setMeterUnit,
    setBaselineReading,
    setReadingValue,
    setReadingNotes,
    setProgramKey,
    setProgramName,
    setProgramDescription,
    setProgramScopeType,
    setProgramAssetTypeId,
    setProgramAssetId,
    setSelectedProgramId,
    setSelectedProgramScheduleIds,
    setHistoryAssetId,
    setApiError,
    meQuery,
    classesQuery,
    typesQuery,
    assetsQuery,
    assetFieldContextQuery,
    selectedAssetId,
    assetReadinessFleetQuery,
    assetReadinessDetailQuery,
    duePmQuery,
    pmProgramsQuery,
    pmProgramDetailQuery,
    pmSchedulesQuery,
    templatesQuery,
    templateDetailQuery,
    inspectionRunsQuery,
    inspectionRunQuery,
    voiceGuidanceQuery,
    technicianRefsQuery,
    defectsQuery,
    defectEvidenceQuery,
    inspectionRunEvidenceQuery,
    workOrdersQuery,
    maintenanceHistoryQuery,
    maintenanceHistorySummaryQuery,
    workOrderDetailQuery,
    workOrderTasksQuery,
    workOrderLaborQuery,
    workOrderEvidenceQuery,
    workOrderPartsDemandQuery,
    workOrderPartsDemandStatusEventsQuery,
    workOrderSupplyReadinessQuery,
    assetMetersQuery,
    meterReadingsQuery,
    meterForecastQuery,
    activeTemplates,
    canManage,
    canManageNotifications,
    canExportAudit,
    viewAllRuns,
    viewAllDefects,
    canManageDefects,
    canCreateWorkOrder,
    canCloseWorkOrder,
    viewAllWorkOrders,
    canExecuteInspections,
    scopedPmSchedules,
    invalidateRegistry,
    invalidateTemplates,
    invalidateInspectionRuns,
    invalidateDefects,
    invalidateWorkOrders,
    invalidateMeters,
    invalidatePmPrograms,
    createTemplateMutation,
    createCategoryMutation,
    createItemMutation,
    saveAssetTypesMutation,
    activateTemplateMutation,
    startRunMutation,
    submitAnswersMutation,
    completeRunMutation,
    createDefectsFromRunMutation,
    createMeterMutation,
    recordMeterReadingMutation,
    createDefectMutation,
    updateDefectStatusMutation,
    createWorkOrderMutation,
    createWorkOrderFromDefectMutation,
    updateWorkOrderStatusMutation,
    addWorkOrderTaskMutation,
    logWorkOrderLaborMutation,
    uploadWorkOrderEvidenceMutation,
    uploadDefectEvidenceMutation,
    uploadInspectionRunEvidenceMutation,
    addWorkOrderPartsDemandMutation,
    publishWorkOrderPartsDemandMutation,
    createPmProgramMutation,
    savePmProgramSchedulesMutation,
    activatePmProgramMutation,
  }
}

export type MaintainArrWorkspaceState = ReturnType<typeof useMaintainArrWorkspaceState>
