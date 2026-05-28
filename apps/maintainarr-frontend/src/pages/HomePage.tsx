import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useEffect, useState } from 'react'
import { Navigate, useSearchParams } from 'react-router-dom'
import { PageHeader } from '@stl/shared-ui'
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
  getWorkOrderTasks,
  getWorkOrders,
  logWorkOrderLabor,
  publishWorkOrderPartsDemand,
  uploadWorkOrderEvidence,
  createAsset,
  createAssetClass,
  createAssetMeter,
  createAssetType,
  createInspectionChecklistItem,
  createInspectionTemplate,
  createInspectionTemplateCategory,
  getAssetClasses,
  getAssetMeters,
  getAssets,
  getAssetTypes,
  getMeterPmForecast,
  getMeterReadings,
  getDefects,
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
  getAssetReadinessFleet,
  getMaintenanceHistory,
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
  canManageNotificationSettings,
  canViewAllInspectionRuns,
  loadSession,
} from '../auth/sessionStorage'
import { NotificationSettingsPanel } from '../components/NotificationSettingsPanel'
import { AssetRegistryPanel } from '../components/AssetRegistryPanel'
import { DefectsPanel } from '../components/DefectsPanel'
import { WorkOrdersPanel } from '../components/WorkOrdersPanel'
import { InspectionRunnerPanel } from '../components/InspectionRunnerPanel'
import { InspectionTemplateBuilderPanel } from '../components/InspectionTemplateBuilderPanel'
import { MaintenanceHistoryPanel } from '../components/MaintenanceHistoryPanel'
import { MeterReadingsPanel } from '../components/MeterReadingsPanel'
import { PmDuePanel } from '../components/PmDuePanel'
import { PmProgramBuilderPanel } from '../components/PmProgramBuilderPanel'

async function fileToBase64(file: File): Promise<string> {
  const buffer = await file.arrayBuffer()
  const bytes = new Uint8Array(buffer)
  let binary = ''
  bytes.forEach((byte) => {
    binary += String.fromCharCode(byte)
  })
  return btoa(binary)
}

export function HomePage() {
  const [searchParams] = useSearchParams()
  const handoff = searchParams.get('handoff')
  if (handoff) {
    return <Navigate to={`/launch?handoff=${encodeURIComponent(handoff)}`} replace />
  }

  const session = loadSession()
  const accessToken = session?.accessToken ?? ''
  const queryClient = useQueryClient()
  const [classKey, setClassKey] = useState('')
  const [className, setClassName] = useState('')
  const [classDescription, setClassDescription] = useState('')
  const [selectedClassId, setSelectedClassId] = useState('')
  const [typeKey, setTypeKey] = useState('')
  const [typeName, setTypeName] = useState('')
  const [typeDescription, setTypeDescription] = useState('')
  const [selectedTypeId, setSelectedTypeId] = useState('')
  const [assetTag, setAssetTag] = useState('')
  const [assetName, setAssetName] = useState('')
  const [assetDescription, setAssetDescription] = useState('')
  const [siteRef, setSiteRef] = useState('')
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
  const [meterKey, setMeterKey] = useState('')
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
  const [apiError, setApiError] = useState<string | null>(null)

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

  const defectsQuery = useQuery({
    queryKey: ['maintainarr-defects', defectStatusFilter],
    queryFn: () =>
      getDefects(accessToken, {
        status: defectStatusFilter || undefined,
      }),
    enabled: meQuery.isSuccess,
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

  const invalidateWorkOrders = async (workOrderId?: string) => {
    await queryClient.invalidateQueries({ queryKey: ['maintainarr-work-orders'] })
    if (workOrderId) {
      await queryClient.invalidateQueries({ queryKey: ['maintainarr-work-order', workOrderId] })
      await queryClient.invalidateQueries({ queryKey: ['maintainarr-work-order-tasks', workOrderId] })
      await queryClient.invalidateQueries({ queryKey: ['maintainarr-work-order-labor', workOrderId] })
      await queryClient.invalidateQueries({ queryKey: ['maintainarr-work-order-evidence', workOrderId] })
      await queryClient.invalidateQueries({ queryKey: ['maintainarr-work-order-parts-demand', workOrderId] })
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

  const createClassMutation = useMutation({
    mutationFn: () =>
      createAssetClass(accessToken, {
        classKey,
        name: className,
        description: classDescription,
      }),
    onSuccess: async () => {
      setClassKey('')
      setClassName('')
      setClassDescription('')
      setApiError(null)
      await invalidateRegistry()
    },
    onError: (error) => setApiError(error instanceof Error ? error.message : 'Failed to create asset class'),
  })

  const createTypeMutation = useMutation({
    mutationFn: () =>
      createAssetType(accessToken, {
        assetClassId: selectedClassId,
        typeKey,
        name: typeName,
        description: typeDescription,
      }),
    onSuccess: async () => {
      setTypeKey('')
      setTypeName('')
      setTypeDescription('')
      setApiError(null)
      await invalidateRegistry()
    },
    onError: (error) => setApiError(error instanceof Error ? error.message : 'Failed to create asset type'),
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
        meterKey,
        name: meterName,
        description: '',
        unit: meterUnit,
        baselineReading: Number(baselineReading),
      }),
    onSuccess: async (created) => {
      setMeterKey('')
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
    onSuccess: async () => {
      setDefectTitle('')
      setDefectDescription('')
      setApiError(null)
      await invalidateDefects()
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
    onSuccess: async (_, variables) => {
      setApiError(null)
      await invalidateWorkOrders(variables.workOrderId)
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

  const addWorkOrderPartsDemandMutation = useMutation({
    mutationFn: () =>
      createWorkOrderPartsDemandLine(accessToken, selectedWorkOrderId, {
        supplyarrPartId: demandSupplyarrPartId || null,
        partNumber: demandPartNumber || null,
        quantityRequested: Number.parseFloat(demandQuantity),
        unitOfMeasure: demandUnitOfMeasure,
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

  const createAssetMutation = useMutation({
    mutationFn: () =>
      createAsset(accessToken, {
        assetTypeId: selectedTypeId,
        assetTag,
        name: assetName,
        description: assetDescription,
        siteRef: siteRef || null,
      }),
    onSuccess: async () => {
      setAssetTag('')
      setAssetName('')
      setAssetDescription('')
      setSiteRef('')
      setApiError(null)
      await invalidateRegistry()
    },
    onError: (error) => setApiError(error instanceof Error ? error.message : 'Failed to create asset'),
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

  if (!session || !meQuery.data) {
    return <p className="text-sm text-slate-400">Loading asset workspace…</p>
  }

  return (
    <div className="mx-auto max-w-6xl space-y-6">
      <PageHeader
        title="Asset and maintenance workspace"
        subtitle={`Signed in as ${meQuery.data.displayName} (${meQuery.data.tenantRoleKey})`}
      />

      {apiError ? <p className="mb-4 rounded-lg border border-red-800 bg-red-950/40 p-3 text-sm text-red-200">{apiError}</p> : null}

      <div className="mb-8">
        <PmDuePanel
          dueSchedules={duePmQuery.data ?? []}
          isLoading={duePmQuery.isLoading}
        />
      </div>

      <div className="mb-8">
        <PmProgramBuilderPanel
          canManage={canManage}
          programs={pmProgramsQuery.data ?? []}
          selectedProgram={pmProgramDetailQuery.data ?? null}
          assetTypes={typesQuery.data ?? []}
          assets={assetsQuery.data ?? []}
          availableSchedules={scopedPmSchedules}
          isLoading={pmProgramsQuery.isLoading}
          isDetailLoading={pmProgramDetailQuery.isLoading}
          isSchedulesLoading={pmSchedulesQuery.isLoading}
          programKey={programKey}
          programName={programName}
          programDescription={programDescription}
          scopeType={programScopeType}
          selectedAssetTypeId={programAssetTypeId}
          selectedAssetId={programAssetId}
          selectedProgramId={selectedProgramId}
          selectedScheduleIds={selectedProgramScheduleIds}
          onProgramKeyChange={setProgramKey}
          onProgramNameChange={setProgramName}
          onProgramDescriptionChange={setProgramDescription}
          onScopeTypeChange={setProgramScopeType}
          onSelectedAssetTypeIdChange={setProgramAssetTypeId}
          onSelectedAssetIdChange={setProgramAssetId}
          onSelectedProgramIdChange={setSelectedProgramId}
          onSelectedScheduleIdsChange={setSelectedProgramScheduleIds}
          onCreateProgram={() => createPmProgramMutation.mutate()}
          onSaveSchedules={() => savePmProgramSchedulesMutation.mutate()}
          onActivateProgram={() => activatePmProgramMutation.mutate()}
          isCreatingProgram={createPmProgramMutation.isPending}
          isSavingSchedules={
            savePmProgramSchedulesMutation.isPending || activatePmProgramMutation.isPending
          }
        />
      </div>

      <div className="mb-8">
        <MeterReadingsPanel
          canManageMeters={canManage}
          canRecordReadings={canExecuteInspections}
          assets={assetsQuery.data ?? []}
          meters={assetMetersQuery.data ?? []}
          readings={meterReadingsQuery.data ?? []}
          forecast={meterForecastQuery.data ?? null}
          selectedAssetId={meterAssetId}
          selectedMeterId={selectedMeterId}
          meterKey={meterKey}
          meterName={meterName}
          meterUnit={meterUnit}
          baselineReading={baselineReading}
          readingValue={readingValue}
          readingNotes={readingNotes}
          isLoading={assetMetersQuery.isLoading || meterReadingsQuery.isLoading}
          isCreatingMeter={createMeterMutation.isPending}
          isRecording={recordMeterReadingMutation.isPending}
          onSelectedAssetIdChange={(assetId) => {
            setMeterAssetId(assetId)
            setSelectedMeterId('')
          }}
          onSelectedMeterIdChange={setSelectedMeterId}
          onMeterKeyChange={setMeterKey}
          onMeterNameChange={setMeterName}
          onMeterUnitChange={setMeterUnit}
          onBaselineReadingChange={setBaselineReading}
          onReadingValueChange={setReadingValue}
          onReadingNotesChange={setReadingNotes}
          onCreateMeter={() => createMeterMutation.mutate()}
          onRecordReading={() => recordMeterReadingMutation.mutate()}
        />
      </div>

      <div className="mb-8">
        <WorkOrdersPanel
          canCreate={canCreateWorkOrder}
          canPerform={canExecuteInspections}
          canClose={canCloseWorkOrder}
          viewAllWorkOrders={viewAllWorkOrders}
          sessionPersonId={session.personId}
          assets={assetsQuery.data ?? []}
          workOrders={workOrdersQuery.data ?? []}
          selectedWorkOrder={workOrderDetailQuery.data ?? null}
          selectedWorkOrderId={selectedWorkOrderId}
          selectedAssetId={workOrderAssetId}
          workOrderTitle={workOrderTitle}
          workOrderDescription={workOrderDescription}
          workOrderPriority={workOrderPriority}
          assignedPersonId={assignedPersonId}
          statusFilter={workOrderStatusFilter}
          isLoading={workOrdersQuery.isLoading}
          isDetailLoading={workOrderDetailQuery.isLoading}
          isCreating={createWorkOrderMutation.isPending}
          isUpdatingStatus={updateWorkOrderStatusMutation.isPending}
          onSelectedWorkOrderIdChange={setSelectedWorkOrderId}
          onSelectedAssetIdChange={setWorkOrderAssetId}
          onWorkOrderTitleChange={setWorkOrderTitle}
          onWorkOrderDescriptionChange={setWorkOrderDescription}
          onWorkOrderPriorityChange={setWorkOrderPriority}
          onAssignedPersonIdChange={setAssignedPersonId}
          onStatusFilterChange={setWorkOrderStatusFilter}
          onCreateWorkOrder={() => createWorkOrderMutation.mutate()}
          onUpdateStatus={(workOrderId, status) =>
            updateWorkOrderStatusMutation.mutate({ workOrderId, status })
          }
          tasks={workOrderTasksQuery.data ?? []}
          labor={workOrderLaborQuery.data ?? []}
          evidence={workOrderEvidenceQuery.data ?? []}
          taskTitle={woTaskTitle}
          laborHours={woLaborHours}
          laborTypeKey={woLaborTypeKey}
          laborPersonId={woLaborPersonId}
          selectedTaskLineId={woSelectedTaskLineId}
          evidenceTypeKey={woEvidenceTypeKey}
          evidenceNotes={woEvidenceNotes}
          selectedEvidenceFileName={woEvidenceFile?.name ?? null}
          onTaskTitleChange={setWoTaskTitle}
          onLaborHoursChange={setWoLaborHours}
          onLaborTypeKeyChange={setWoLaborTypeKey}
          onLaborPersonIdChange={setWoLaborPersonId}
          onSelectedTaskLineIdChange={setWoSelectedTaskLineId}
          onEvidenceTypeKeyChange={setWoEvidenceTypeKey}
          onEvidenceNotesChange={setWoEvidenceNotes}
          onSelectEvidenceFile={setWoEvidenceFile}
          onAddTask={() => addWorkOrderTaskMutation.mutate()}
          onLogLabor={() => logWorkOrderLaborMutation.mutate()}
          onUploadEvidence={() => uploadWorkOrderEvidenceMutation.mutate()}
          isAddingTask={addWorkOrderTaskMutation.isPending}
          isLoggingLabor={logWorkOrderLaborMutation.isPending}
          isUploadingEvidence={uploadWorkOrderEvidenceMutation.isPending}
          partsDemand={workOrderPartsDemandQuery.data ?? []}
          demandPartNumber={demandPartNumber}
          demandSupplyarrPartId={demandSupplyarrPartId}
          demandQuantity={demandQuantity}
          demandUnitOfMeasure={demandUnitOfMeasure}
          demandNotes={demandNotes}
          createPurchaseRequestDraft={createPurchaseRequestDraft}
          onDemandPartNumberChange={setDemandPartNumber}
          onDemandSupplyarrPartIdChange={setDemandSupplyarrPartId}
          onDemandQuantityChange={setDemandQuantity}
          onDemandUnitOfMeasureChange={setDemandUnitOfMeasure}
          onDemandNotesChange={setDemandNotes}
          onCreatePurchaseRequestDraftChange={setCreatePurchaseRequestDraft}
          onAddPartsDemandLine={() => addWorkOrderPartsDemandMutation.mutate()}
          onPublishPartsDemand={() => publishWorkOrderPartsDemandMutation.mutate()}
          isAddingPartsDemand={addWorkOrderPartsDemandMutation.isPending}
          isPublishingPartsDemand={publishWorkOrderPartsDemandMutation.isPending}
        />
      </div>

      <div className="mb-8">
        <DefectsPanel
          canCreate={canExecuteInspections}
          canCreateWorkOrder={canCreateWorkOrder}
          canManageStatus={canManageDefects}
          viewAllDefects={viewAllDefects}
          assets={assetsQuery.data ?? []}
          defects={defectsQuery.data ?? []}
          selectedAssetId={defectAssetId}
          defectTitle={defectTitle}
          defectDescription={defectDescription}
          defectSeverity={defectSeverity}
          statusFilter={defectStatusFilter}
          isLoading={defectsQuery.isLoading}
          isCreating={createDefectMutation.isPending}
          isUpdatingStatus={updateDefectStatusMutation.isPending}
          onSelectedAssetIdChange={setDefectAssetId}
          onDefectTitleChange={setDefectTitle}
          onDefectDescriptionChange={setDefectDescription}
          onDefectSeverityChange={setDefectSeverity}
          onStatusFilterChange={setDefectStatusFilter}
          onCreateDefect={() => createDefectMutation.mutate()}
          onCreateWorkOrderFromDefect={(defectId) => createWorkOrderFromDefectMutation.mutate(defectId)}
          creatingWorkOrderDefectId={creatingWorkOrderDefectId}
          onUpdateStatus={(defectId, status) => updateDefectStatusMutation.mutate({ defectId, status })}
        />
      </div>

      <div className="mb-8">
        <InspectionRunnerPanel
          canExecute={canExecuteInspections}
          viewAllRuns={viewAllRuns}
          assets={assetsQuery.data ?? []}
          activeTemplates={activeTemplates}
          runs={inspectionRunsQuery.data ?? []}
          activeRun={inspectionRunQuery.data ?? null}
          selectedAssetId={runAssetId}
          selectedTemplateId={runTemplateId}
          selectedRunId={selectedRunId}
          answerDrafts={answerDrafts}
          isLoading={assetsQuery.isLoading || templatesQuery.isLoading || inspectionRunsQuery.isLoading}
          isRunLoading={inspectionRunQuery.isLoading}
          isStarting={startRunMutation.isPending}
          isSubmitting={submitAnswersMutation.isPending}
          isCompleting={completeRunMutation.isPending}
          isCreatingDefects={createDefectsFromRunMutation.isPending}
          onSelectedAssetIdChange={setRunAssetId}
          onSelectedTemplateIdChange={setRunTemplateId}
          onSelectedRunIdChange={setSelectedRunId}
          onAnswerDraftChange={(checklistItemId, field, value) =>
            setAnswerDrafts((current) => ({
              ...current,
              [checklistItemId]: { ...current[checklistItemId], [field]: value },
            }))
          }
          onStartRun={() => startRunMutation.mutate()}
          onSubmitAnswers={() => submitAnswersMutation.mutate()}
          onCompleteRun={() => completeRunMutation.mutate()}
          onCreateDefectsFromRun={() => createDefectsFromRunMutation.mutate()}
        />
      </div>

      <div className="mb-8">
        <InspectionTemplateBuilderPanel
          canManage={canManage}
          templates={templatesQuery.data ?? []}
          selectedTemplate={templateDetailQuery.data ?? null}
          assetTypes={typesQuery.data ?? []}
          isLoading={templatesQuery.isLoading}
          isDetailLoading={templateDetailQuery.isLoading}
          templateKey={templateKey}
          templateName={templateName}
          templateDescription={templateDescription}
          categoryKey={categoryKey}
          categoryName={categoryName}
          itemKey={itemKey}
          itemPrompt={itemPrompt}
          itemType={itemType}
          selectedCategoryId={selectedCategoryId}
          selectedAssetTypeIds={selectedAssetTypeIds}
          selectedTemplateId={selectedTemplateId}
          onTemplateKeyChange={setTemplateKey}
          onTemplateNameChange={setTemplateName}
          onTemplateDescriptionChange={setTemplateDescription}
          onCategoryKeyChange={setCategoryKey}
          onCategoryNameChange={setCategoryName}
          onItemKeyChange={setItemKey}
          onItemPromptChange={setItemPrompt}
          onItemTypeChange={setItemType}
          onSelectedCategoryIdChange={setSelectedCategoryId}
          onSelectedAssetTypeIdsChange={setSelectedAssetTypeIds}
          onSelectedTemplateIdChange={setSelectedTemplateId}
          onCreateTemplate={() => createTemplateMutation.mutate()}
          onCreateCategory={() => createCategoryMutation.mutate()}
          onCreateItem={() => createItemMutation.mutate()}
          onSaveAssetTypes={() => saveAssetTypesMutation.mutate()}
          onActivateTemplate={() => activateTemplateMutation.mutate()}
          isCreatingTemplate={createTemplateMutation.isPending}
          isSavingBuilder={
            createCategoryMutation.isPending ||
            createItemMutation.isPending ||
            saveAssetTypesMutation.isPending ||
            activateTemplateMutation.isPending
          }
        />
      </div>

      <AssetRegistryPanel
        canManage={canManage}
        classes={classesQuery.data ?? []}
        types={typesQuery.data ?? []}
        assets={assetsQuery.data ?? []}
        readinessByAssetId={Object.fromEntries(
          (assetReadinessFleetQuery.data ?? []).map((item) => [item.assetId, item]),
        )}
        isLoading={classesQuery.isLoading || typesQuery.isLoading || assetsQuery.isLoading}
        isReadinessLoading={assetReadinessFleetQuery.isLoading}
        classKey={classKey}
        className={className}
        classDescription={classDescription}
        selectedClassId={selectedClassId}
        typeKey={typeKey}
        typeName={typeName}
        typeDescription={typeDescription}
        selectedTypeId={selectedTypeId}
        assetTag={assetTag}
        assetName={assetName}
        assetDescription={assetDescription}
        siteRef={siteRef}
        onClassKeyChange={setClassKey}
        onClassNameChange={setClassName}
        onClassDescriptionChange={setClassDescription}
        onSelectedClassIdChange={setSelectedClassId}
        onTypeKeyChange={setTypeKey}
        onTypeNameChange={setTypeName}
        onTypeDescriptionChange={setTypeDescription}
        onSelectedTypeIdChange={setSelectedTypeId}
        onAssetTagChange={setAssetTag}
        onAssetNameChange={setAssetName}
        onAssetDescriptionChange={setAssetDescription}
        onSiteRefChange={setSiteRef}
        onCreateClass={() => createClassMutation.mutate()}
        onCreateType={() => createTypeMutation.mutate()}
        onCreateAsset={() => createAssetMutation.mutate()}
        isCreatingClass={createClassMutation.isPending}
        isCreatingType={createTypeMutation.isPending}
        isCreatingAsset={createAssetMutation.isPending}
      />

      {canManageNotifications && (
        <div className="mt-8">
          <NotificationSettingsPanel accessToken={accessToken} canManage={canManageNotifications} />
        </div>
      )}

      <div className="mt-8">
        <MaintenanceHistoryPanel
          assets={assetsQuery.data ?? []}
          entries={maintenanceHistoryQuery.data?.items ?? []}
          totalCount={maintenanceHistoryQuery.data?.totalCount ?? 0}
          selectedAssetId={historyAssetId}
          isLoading={maintenanceHistoryQuery.isLoading}
          onSelectedAssetIdChange={setHistoryAssetId}
        />
      </div>
    </div>
  )
}
