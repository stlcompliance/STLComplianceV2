import { useMemo, useState, type ReactNode } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Link, Navigate, useNavigate, useSearchParams } from 'react-router-dom'
import {
  AlertTriangle,
  ArrowLeft,
  ArrowRight,
  BadgeCheck,
  CheckCircle2,
  ChevronDown,
  ChevronUp,
  CirclePlus,
  Loader2,
  Package,
  ShieldCheck,
  Sparkles,
  Trash2,
  Wrench,
  FileStack,
} from 'lucide-react'
import {
  AsyncMultiPicker,
  AsyncSearchPicker,
  ControlledSelect,
  DetailBadge,
  GeneratedKeyField,
  PageHeader,
  StaticSearchPicker,
  buildSemanticKey,
  type PickerOption,
} from '@stl/shared-ui'
import {
  activateMaintenancePartsKit,
  createMaintenancePartsKit,
  getCatalogs,
  getAsset,
  getMaintenancePartsKit,
  getMaintenancePartsKits,
  getMe,
  getParts,
  getPeople,
  getSites,
  getTeams,
  previewMaintenancePartsKit,
  searchAssets,
  submitMaintenancePartsKitForApproval,
  updateMaintenancePartsKit,
  validateMaintenancePartsKit,
} from '../../api/client'
import type {
  CatalogResponse,
  MaintenancePartsKitDefinitionRequest,
  MaintenancePartsKitPreviewRequest,
  MaintenancePartsKitPreviewResponse,
  MaintenancePartsKitResponse,
  MaintenancePartsKitValidationResponse,
  ReferenceOptionResponse,
} from '../../api/types'
import {
  canActivatePartsKits,
  canCreatePartsKits,
  canPreviewPartsKits,
  canRetirePartsKits,
  loadSession,
} from '../../auth/sessionStorage'

type SectionKey =
  | 'basics'
  | 'applicability'
  | 'scope'
  | 'items'
  | 'rules'
  | 'availability'
  | 'behavior'
  | 'compliance'
  | 'approval'
  | 'review'

interface SectionDefinition {
  key: SectionKey
  label: string
  description: string
  icon: ReactNode
}

interface KitItemDraft {
  id: string
  itemRef: string
  supplyarrPartId: string
  itemDescriptionSnapshot: string
  partNumberSnapshot: string
  manufacturerPartNumberSnapshot: string
  vendorPartNumberSnapshot: string
  quantity: string
  unitOfMeasure: string
  required: boolean
  criticality: string
  substituteAllowed: boolean
  preferredSubstituteRefsText: string
  consumable: boolean
  serialized: boolean
  coreReturnExpected: boolean
  hazardous: boolean
  warrantySensitive: boolean
  requiredByTask: string
  notes: string
  tagsText: string
  isPlaceholder: boolean
}

interface QuantityRuleDraft {
  id: string
  ruleId: string
  ruleType: string
  appliesToItemRef: string
  assetConditionSummary: string
  workConditionSummary: string
  conditionSummary: string
  baseQuantity: string
  multiplier: string
  minimumQuantity: string
  maximumQuantity: string
  roundingBehavior: string
  plainLanguageSummary: string
}

const SECTION_DEFINITIONS: SectionDefinition[] = [
  {
    key: 'basics',
    label: 'Basics',
    description: 'Set the kit identity, ownership, and high-level metadata.',
    icon: <Sparkles className="h-4 w-4" aria-hidden />,
  },
  {
    key: 'applicability',
    label: 'Applicability',
    description: 'Describe where this kit is intended to be used.',
    icon: <BadgeCheck className="h-4 w-4" aria-hidden />,
  },
  {
    key: 'scope',
    label: 'Asset Scope',
    description: 'Constrain the kit to assets, sites, classes, or exclusions.',
    icon: <Package className="h-4 w-4" aria-hidden />,
  },
  {
    key: 'items',
    label: 'Kit Items',
    description: 'Add the parts, snapshots, substitutes, and flags.',
    icon: <FileStack className="h-4 w-4" aria-hidden />,
  },
  {
    key: 'rules',
    label: 'Quantity Rules',
    description: 'Define rule-based quantity adjustments and rounding.',
    icon: <CirclePlus className="h-4 w-4" aria-hidden />,
  },
  {
    key: 'availability',
    label: 'Availability',
    description: 'Choose which availability signals should surface in preview.',
    icon: <Wrench className="h-4 w-4" aria-hidden />,
  },
  {
    key: 'behavior',
    label: 'Work Order Behavior',
    description: 'Control how the kit behaves on work orders and PM flows.',
    icon: <Wrench className="h-4 w-4" aria-hidden />,
  },
  {
    key: 'compliance',
    label: 'Compliance',
    description: 'Capture safety, readiness, and compliance impact.',
    icon: <ShieldCheck className="h-4 w-4" aria-hidden />,
  },
  {
    key: 'approval',
    label: 'Approval',
    description: 'Decide whether the kit needs approval before activation.',
    icon: <BadgeCheck className="h-4 w-4" aria-hidden />,
  },
  {
    key: 'review',
    label: 'Review',
    description: 'Validate, preview, save, submit, and activate the kit.',
    icon: <CheckCircle2 className="h-4 w-4" aria-hidden />,
  },
]

const shellStyle = {
  background: 'linear-gradient(180deg, var(--color-bg-app) 0%, var(--color-bg-surface-elevated) 100%)',
  color: 'var(--color-text-primary)',
}

const panelStyle = {
  backgroundColor: 'var(--color-bg-surface)',
  borderColor: 'var(--color-border-subtle)',
}

const fieldClass =
  'mt-1 w-full rounded-xl border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100 placeholder:text-[var(--color-text-muted)] focus:border-sky-500 focus:outline-none'

const textareaClass = `${fieldClass} min-h-[104px]`
const smallFieldClass = `${fieldClass} min-h-[42px]`

function createId(): string {
  return globalThis.crypto?.randomUUID?.() ?? `${Date.now()}-${Math.random().toString(16).slice(2)}`
}

function trimToNull(value: string): string | null {
  const trimmed = value.trim()
  return trimmed.length > 0 ? trimmed : null
}

function splitDelimited(value: string): string[] {
  return value
    .split(/[\n,;]+/)
    .map((part) => part.trim())
    .filter(Boolean)
}

function joinDelimited(values: string[]): string {
  return values.join('\n')
}

function mapReferenceOptions(items: ReferenceOptionResponse[] | undefined): PickerOption[] {
  return (items ?? []).map((item) => ({
    value: item.id ?? item.key,
    label: item.label,
    inactive: !item.isActive,
  }))
}

function mapCatalogOptions(catalogs: CatalogResponse[] | undefined, key: string): PickerOption[] {
  const catalog = catalogs?.find((item) => item.key.toLowerCase() === key.toLowerCase())
  return (catalog?.options ?? [])
    .filter((option) => option.isActive)
    .map((option) => ({
      value: option.key,
      label: option.label || option.key,
    }))
}

function mapAssetOptions(items: Awaited<ReturnType<typeof searchAssets>>) {
  return items.map((asset) => ({
    value: asset.assetId,
    label: `${asset.assetTag} - ${asset.name}`,
    inactive: false,
  }))
}

function mapAssetSelectionOption(asset: Awaited<ReturnType<typeof getAsset>>): PickerOption {
  return {
    value: asset.assetId,
    label: `${asset.assetTag} - ${asset.name}`,
    inactive: false,
  }
}

function mapPartsOptions(items: ReferenceOptionResponse[] | undefined): PickerOption[] {
  return (items ?? []).map((item) => ({
    value: item.id ?? item.key,
    label: item.label,
    inactive: !item.isActive,
  }))
}

function createBlankItem(): KitItemDraft {
  return {
    id: createId(),
    itemRef: '',
    supplyarrPartId: '',
    itemDescriptionSnapshot: '',
    partNumberSnapshot: '',
    manufacturerPartNumberSnapshot: '',
    vendorPartNumberSnapshot: '',
    quantity: '1',
    unitOfMeasure: 'each',
    required: true,
    criticality: 'medium',
    substituteAllowed: false,
    preferredSubstituteRefsText: '',
    consumable: false,
    serialized: false,
    coreReturnExpected: false,
    hazardous: false,
    warrantySensitive: false,
    requiredByTask: '',
    notes: '',
    tagsText: '',
    isPlaceholder: false,
  }
}

function createBlankRule(): QuantityRuleDraft {
  return {
    id: createId(),
    ruleId: '',
    ruleType: 'default',
    appliesToItemRef: '',
    assetConditionSummary: '',
    workConditionSummary: '',
    conditionSummary: '',
    baseQuantity: '1',
    multiplier: '1',
    minimumQuantity: '',
    maximumQuantity: '',
    roundingBehavior: 'nearest',
    plainLanguageSummary: '',
  }
}

function toneForStatus(status: string | null | undefined): 'good' | 'warn' | 'bad' | 'neutral' {
  switch ((status ?? '').toLowerCase()) {
    case 'active':
      return 'good'
    case 'pending_approval':
      return 'warn'
    case 'retired':
    case 'archived':
      return 'bad'
    default:
      return 'neutral'
  }
}

function formatDateTimeInput(value: string | null | undefined): string {
  if (!value) return ''
  const parsed = new Date(value)
  if (Number.isNaN(parsed.getTime())) return ''
  const offsetMs = parsed.getTimezoneOffset() * 60_000
  return new Date(parsed.getTime() - offsetMs).toISOString().slice(0, 16)
}

function parseDateTimeInput(value: string): string | null {
  const trimmed = value.trim()
  if (!trimmed) return null
  const parsed = new Date(trimmed)
  return Number.isNaN(parsed.getTime()) ? null : parsed.toISOString()
}

function parseDecimal(value: string, fallback = 0): number {
  const parsed = Number(value.trim())
  return Number.isFinite(parsed) ? parsed : fallback
}

function summarizeSelectedItems(items: KitItemDraft[]): string {
  if (items.length === 0) return 'No kit items have been added yet.'
  const required = items.filter((item) => item.required).length
  return `${items.length} item${items.length === 1 ? '' : 's'} total, ${required} required.`
}

function summarizeRules(rules: QuantityRuleDraft[]): string {
  if (rules.length === 0) return 'No quantity rules configured yet.'
  return `${rules.length} rule${rules.length === 1 ? '' : 's'} ready for preview.`
}

function summarizeScope(value: string[]): string {
  if (value.length === 0) return 'All'
  if (value.length === 1) return value[0] ?? '1 value'
  return `${value.length} values`
}

function SectionPanel({
  title,
  description,
  icon,
  open,
  locked,
  complete,
  onToggle,
  children,
  footer,
}: {
  title: string
  description: string
  icon: ReactNode
  open: boolean
  locked: boolean
  complete: boolean
  onToggle: () => void
  children: ReactNode
  footer?: ReactNode
}) {
  return (
    <section className="overflow-hidden rounded-[1.75rem] border border-slate-800 bg-slate-950/70 shadow-2xl shadow-sky-950/10">
      <button
        type="button"
        className="flex w-full items-start justify-between gap-4 border-b border-slate-800 px-5 py-5 text-left hover:bg-slate-900/30"
        onClick={onToggle}
      >
        <div className="flex min-w-0 items-start gap-3">
          <div className="mt-1 flex h-10 w-10 shrink-0 items-center justify-center rounded-2xl border border-sky-700/40 bg-sky-500/10 text-sky-300">
            {icon}
          </div>
          <div className="min-w-0">
            <div className="flex flex-wrap items-center gap-2">
              <h2 className="text-lg font-semibold text-white">{title}</h2>
              <DetailBadge label={complete ? 'Ready' : 'In progress'} tone={complete ? 'good' : 'warn'} />
              {locked ? <DetailBadge label="Locked" tone="neutral" /> : null}
            </div>
            <p className="mt-1 text-sm text-slate-400">{description}</p>
          </div>
        </div>
        <div className="mt-1 text-slate-400">{open ? <ChevronUp className="h-5 w-5" /> : <ChevronDown className="h-5 w-5" />}</div>
      </button>
      {open ? <div className="space-y-6 p-5">{children}{footer ? <div className="pt-2">{footer}</div> : null}</div> : null}
      {!open ? (
        <div className="border-t border-slate-800 px-5 py-4 text-sm text-slate-400">
          {locked ? 'Save the prior section to unlock this step.' : 'Click to expand this step.'}
        </div>
      ) : null}
    </section>
  )
}

export function PartsKitCreatePage() {
  const session = loadSession()
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const [searchParams] = useSearchParams()

  const [kitNumberOverride, setKitNumberOverride] = useState('')
  const [title, setTitle] = useState('')
  const [description, setDescription] = useState('')
  const [kitCategoryKey, setKitCategoryKey] = useState('')
  const [kitTypeKey, setKitTypeKey] = useState('')
  const [priorityKey, setPriorityKey] = useState('')
  const [owningSiteRef, setOwningSiteRef] = useState('')
  const [owningTeamRef, setOwningTeamRef] = useState('')
  const [ownerPersonId, setOwnerPersonId] = useState('')
  const [ownerRoleKey, setOwnerRoleKey] = useState('')
  const [tagsText, setTagsText] = useState('')
  const [effectiveAt, setEffectiveAt] = useState('')
  const [expiresAt, setExpiresAt] = useState('')
  const [sourceKitId, setSourceKitId] = useState(searchParams.get('cloneFrom') ?? '')

  const [assetTypeApplicabilityText, setAssetTypeApplicabilityText] = useState('')
  const [workOrderTypeApplicabilityText, setWorkOrderTypeApplicabilityText] = useState('')
  const [pmPlanRef, setPmPlanRef] = useState('')
  const [applicabilityWorkOrderTypesText, setApplicabilityWorkOrderTypesText] = useState('')
  const [applicabilityPmProgramRefsText, setApplicabilityPmProgramRefsText] = useState('')
  const [applicabilityInspectionTemplateRefsText, setApplicabilityInspectionTemplateRefsText] = useState('')
  const [applicabilityDefectTypesText, setApplicabilityDefectTypesText] = useState('')
  const [applicabilityTaskTemplateRefsText, setApplicabilityTaskTemplateRefsText] = useState('')
  const [applicabilityRepairCategoriesText, setApplicabilityRepairCategoriesText] = useState('')
  const [workSourceCompatibilitiesText, setWorkSourceCompatibilitiesText] = useState('')

  const [assetClassKeysText, setAssetClassKeysText] = useState('')
  const [assetTypeKeysText, setAssetTypeKeysText] = useState('')
  const [assetCategoryKeysText, setAssetCategoryKeysText] = useState('')
  const [assetStatusKeysText, setAssetStatusKeysText] = useState('')
  const [siteRefsText, setSiteRefsText] = useState('')
  const [departmentRefsText, setDepartmentRefsText] = useState('')
  const [makeKeysText, setMakeKeysText] = useState('')
  const [modelKeysText, setModelKeysText] = useState('')
  const [yearFrom, setYearFrom] = useState('')
  const [yearTo, setYearTo] = useState('')
  const [fuelTypeKeysText, setFuelTypeKeysText] = useState('')
  const [bodyTypeKeysText, setBodyTypeKeysText] = useState('')
  const [configurationKeysText, setConfigurationKeysText] = useState('')
  const [variantFlagsText, setVariantFlagsText] = useState('')
  const [requiredAttributesText, setRequiredAttributesText] = useState('')
  const [excludedAttributesText, setExcludedAttributesText] = useState('')
  const [includedAssetIds, setIncludedAssetIds] = useState<string[]>([])
  const [excludedAssetIds, setExcludedAssetIds] = useState<string[]>([])
  const [selectedAssetId, setSelectedAssetId] = useState('')

  const [items, setItems] = useState<KitItemDraft[]>([createBlankItem()])
  const [quantityRules, setQuantityRules] = useState<QuantityRuleDraft[]>([])

  const [availabilityEnabled, setAvailabilityEnabled] = useState(false)
  const [availabilityPreferredSource, setAvailabilityPreferredSource] = useState('')
  const [availabilityShowSite, setAvailabilityShowSite] = useState(true)
  const [availabilityShowNearby, setAvailabilityShowNearby] = useState(true)
  const [availabilityShowOnOrder, setAvailabilityShowOnOrder] = useState(true)
  const [availabilityShowLeadTime, setAvailabilityShowLeadTime] = useState(true)
  const [availabilityRequestReservation, setAvailabilityRequestReservation] = useState(false)
  const [availabilityNotes, setAvailabilityNotes] = useState('')

  const [canBeManuallyAdded, setCanBeManuallyAdded] = useState(true)
  const [autoSuggestOnMatchingWorkOrder, setAutoSuggestOnMatchingWorkOrder] = useState(false)
  const [autoAddToMatchingWorkOrder, setAutoAddToMatchingWorkOrder] = useState(false)
  const [autoAddToPmGeneratedWorkOrder, setAutoAddToPmGeneratedWorkOrder] = useState(false)
  const [autoAddAfterFailedInspectionQuestion, setAutoAddAfterFailedInspectionQuestion] = useState(false)
  const [autoAddAfterMatchingDefectType, setAutoAddAfterMatchingDefectType] = useState(false)
  const [requireSupervisorApprovalBeforeAdding, setRequireSupervisorApprovalBeforeAdding] = useState(false)
  const [requirePartsReviewBeforeWorkCanStart, setRequirePartsReviewBeforeWorkCanStart] = useState(false)
  const [requireAvailabilityCheckBeforeScheduling, setRequireAvailabilityCheckBeforeScheduling] = useState(false)
  const [allowTechnicianAdjustQuantities, setAllowTechnicianAdjustQuantities] = useState(true)
  const [requireAdjustmentReason, setRequireAdjustmentReason] = useState(false)
  const [allowTechnicianRemoveOptionalItems, setAllowTechnicianRemoveOptionalItems] = useState(true)
  const [allowTechnicianRemoveRequiredItems, setAllowTechnicianRemoveRequiredItems] = useState(false)
  const [requireReasonToRemoveRequiredItem, setRequireReasonToRemoveRequiredItem] = useState(false)
  const [snapshotKitItemsOntoWorkOrder, setSnapshotKitItemsOntoWorkOrder] = useState(true)
  const [keepLiveReferenceAfterWorkOrderCreation, setKeepLiveReferenceAfterWorkOrderCreation] = useState(false)

  const [complianceRelated, setComplianceRelated] = useState(false)
  const [governingBodyKeysText, setGoverningBodyKeysText] = useState('')
  const [citationRefsText, setCitationRefsText] = useState('')
  const [safetyCritical, setSafetyCritical] = useState(false)
  const [readinessSensitive, setReadinessSensitive] = useState(false)
  const [missingRequiredPartsBlockWorkStart, setMissingRequiredPartsBlockWorkStart] = useState(false)
  const [missingRequiredPartsBlockWorkCompletion, setMissingRequiredPartsBlockWorkCompletion] = useState(false)
  const [requireSupervisorApprovalForSubstitution, setRequireSupervisorApprovalForSubstitution] = useState(false)
  const [requireDocumentationForSubstitution, setRequireDocumentationForSubstitution] = useState(false)
  const [requireFinalInspectionAfterUse, setRequireFinalInspectionAfterUse] = useState(false)
  const [linkedInspectionTemplateId, setLinkedInspectionTemplateId] = useState('')

  const [requiresApprovalBeforeActivation, setRequiresApprovalBeforeActivation] = useState(false)
  const [approverRoleKey, setApproverRoleKey] = useState('')
  const [approverPersonId, setApproverPersonId] = useState('')
  const [retireReplacedKitAfterActivation, setRetireReplacedKitAfterActivation] = useState(false)
  const [notesForApprover, setNotesForApprover] = useState('')
  const [changeReason, setChangeReason] = useState('')
  const [versionLabel, setVersionLabel] = useState('')

  const [draftId, setDraftId] = useState<string | null>(null)
  const [draftStatus, setDraftStatus] = useState('draft')
  const [draftVersion, setDraftVersion] = useState(1)
  const [savedKitNumber, setSavedKitNumber] = useState('')
  const [lastSavedAt, setLastSavedAt] = useState<string | null>(null)
  const [expandedSectionIndex, setExpandedSectionIndex] = useState(0)
  const [errorMessage, setErrorMessage] = useState<string | null>(null)
  const [validationResult, setValidationResult] = useState<MaintenancePartsKitValidationResponse | null>(null)
  const [previewResult, setPreviewResult] = useState<MaintenancePartsKitPreviewResponse | null>(null)

  const meQuery = useQuery({
    queryKey: ['maintainarr-me', session?.accessToken],
    queryFn: () => getMe(session!.accessToken),
    enabled: Boolean(session?.accessToken),
    retry: false,
  })

  const canCreate = meQuery.data ? canCreatePartsKits(meQuery.data.tenantRoleKey, meQuery.data.isPlatformAdmin) : false
  const canPreview = meQuery.data ? canPreviewPartsKits(meQuery.data.tenantRoleKey, meQuery.data.isPlatformAdmin) : false
  const canActivate = meQuery.data ? canActivatePartsKits(meQuery.data.tenantRoleKey, meQuery.data.isPlatformAdmin) : false
  const canRetire = meQuery.data ? canRetirePartsKits(meQuery.data.tenantRoleKey, meQuery.data.isPlatformAdmin) : false
  const baseDataEnabled = Boolean(session?.accessToken && canCreate)

  const catalogsQuery = useQuery({
    queryKey: ['maintainarr-parts-kit-catalogs', session?.accessToken],
    queryFn: () =>
      getCatalogs(session!.accessToken, [
        'partsKitCategory',
        'partsKitType',
        'priority',
        'criticality',
        'kitQuantityRuleType',
        'kitQuantityRoundingBehavior',
        'workOrderType',
      ]),
    enabled: baseDataEnabled,
    retry: false,
  })

  const sitesQuery = useQuery({
    queryKey: ['maintainarr-sites', session?.accessToken],
    queryFn: () => getSites(session!.accessToken),
    enabled: baseDataEnabled,
    retry: false,
  })

  const teamsQuery = useQuery({
    queryKey: ['maintainarr-teams', session?.accessToken],
    queryFn: () => getTeams(session!.accessToken),
    enabled: baseDataEnabled,
    retry: false,
  })

  const peopleQuery = useQuery({
    queryKey: ['maintainarr-people', session?.accessToken],
    queryFn: () => getPeople(session!.accessToken),
    enabled: baseDataEnabled,
    retry: false,
  })

  const selectedPreviewAssetQuery = useQuery({
    queryKey: ['maintainarr-parts-kit-preview-asset', session?.accessToken, selectedAssetId],
    queryFn: async () => {
      if (!selectedAssetId) return null
      try {
        return mapAssetSelectionOption(await getAsset(session!.accessToken, selectedAssetId))
      } catch {
        return { value: selectedAssetId, label: selectedAssetId, inactive: true }
      }
    },
    enabled: Boolean(session?.accessToken && canPreview && selectedAssetId),
    retry: false,
  })

  const includedAssetOptionsQuery = useQuery({
    queryKey: ['maintainarr-parts-kit-included-assets', session?.accessToken, includedAssetIds],
    queryFn: async () =>
      Promise.all(
        includedAssetIds.map(async (assetId) => {
          try {
            return mapAssetSelectionOption(await getAsset(session!.accessToken, assetId))
          } catch {
            return { value: assetId, label: assetId, inactive: true }
          }
        }),
      ),
    enabled: Boolean(session?.accessToken && includedAssetIds.length > 0),
    retry: false,
  })

  const excludedAssetOptionsQuery = useQuery({
    queryKey: ['maintainarr-parts-kit-excluded-assets', session?.accessToken, excludedAssetIds],
    queryFn: async () =>
      Promise.all(
        excludedAssetIds.map(async (assetId) => {
          try {
            return mapAssetSelectionOption(await getAsset(session!.accessToken, assetId))
          } catch {
            return { value: assetId, label: assetId, inactive: true }
          }
        }),
      ),
    enabled: Boolean(session?.accessToken && excludedAssetIds.length > 0),
    retry: false,
  })

  const sourceKitOption = useQuery({
    queryKey: ['maintainarr-parts-kit-source', session?.accessToken, sourceKitId],
    queryFn: async () => {
      if (!sourceKitId) return null
      const kits = await getMaintenancePartsKits(session!.accessToken)
      const match = kits.items.find((kit) => kit.partsKitId === sourceKitId)
      return match
        ? { value: match.partsKitId, label: `${match.kitNumber} - ${match.title}` }
        : null
    },
    enabled: Boolean(session?.accessToken && sourceKitId),
    retry: false,
  })

  const siteOptions = useMemo(() => mapReferenceOptions(sitesQuery.data), [sitesQuery.data])
  const teamOptions = useMemo(() => mapReferenceOptions(teamsQuery.data), [teamsQuery.data])
  const peopleOptions = useMemo(() => mapReferenceOptions(peopleQuery.data), [peopleQuery.data])
  const categoryOptions = useMemo(() => mapCatalogOptions(catalogsQuery.data, 'partsKitCategory'), [catalogsQuery.data])
  const typeOptions = useMemo(() => mapCatalogOptions(catalogsQuery.data, 'partsKitType'), [catalogsQuery.data])
  const priorityOptions = useMemo(() => mapCatalogOptions(catalogsQuery.data, 'priority'), [catalogsQuery.data])
  const criticalityOptions = useMemo(
    () => [
      { value: 'low', label: 'Low' },
      { value: 'medium', label: 'Medium' },
      { value: 'high', label: 'High' },
      { value: 'critical', label: 'Critical' },
    ],
    [],
  )
  const roundingOptions = useMemo(
    () => [
      { value: 'nearest', label: 'Nearest' },
      { value: 'ceil', label: 'Ceiling' },
      { value: 'floor', label: 'Floor' },
      { value: 'none', label: 'No rounding' },
    ],
    [],
  )
  const behaviorSelectOptions = useMemo(
    () => [
      { value: 'default', label: 'Default' },
      { value: 'kit_multiplier', label: 'Kit multiplier' },
      { value: 'asset_condition', label: 'Asset condition' },
      { value: 'work_condition', label: 'Work condition' },
      { value: 'custom', label: 'Custom' },
    ],
    [],
  )

  const generatedKitNumber = useMemo(
    () =>
      buildSemanticKey({
        domain: 'maintainarr',
        kind: 'parts-kit',
        title,
        aliases: [kitNumberOverride],
      }),
    [title, kitNumberOverride],
  )

  const saveMutation = useMutation({
    mutationFn: async (advanceToSectionIndex: number) => {
      if (!session?.accessToken) {
        throw new Error('Missing session.')
      }

      const payload = buildPayload()
      const response = draftId
        ? await updateMaintenancePartsKit(session.accessToken, draftId, payload)
        : await createMaintenancePartsKit(session.accessToken, payload)
      return { response, advanceToSectionIndex }
    },
    onSuccess: async ({ response, advanceToSectionIndex }) => {
      setErrorMessage(null)
      setDraftId(response.partsKitId)
      setDraftStatus(response.status)
      setDraftVersion(response.version ?? draftVersion)
      setSavedKitNumber(response.kitNumber)
      setKitNumberOverride(response.kitNumber)
      setLastSavedAt(new Date().toISOString())
      setExpandedSectionIndex((current) => Math.max(current, advanceToSectionIndex))
      await queryClient.invalidateQueries({ queryKey: ['maintainarr-parts-kits', session?.accessToken] })
      await queryClient.invalidateQueries({ queryKey: ['maintainarr-parts-kit', session?.accessToken, response.partsKitId] })
    },
    onError: (error) => {
      setErrorMessage(error instanceof Error ? error.message : 'Unable to save the draft.')
    },
  })

  const seedMutation = useMutation({
    mutationFn: async () => {
      if (!session?.accessToken || !sourceKitId) {
        throw new Error('Select a source kit first.')
      }
      return getMaintenancePartsKit(session.accessToken, sourceKitId)
    },
    onSuccess: (kit) => {
      applySeedKit(kit)
      setExpandedSectionIndex(SECTION_DEFINITIONS.length - 1)
      setErrorMessage(null)
    },
    onError: (error) => {
      setErrorMessage(error instanceof Error ? error.message : 'Unable to load the source kit.')
    },
  })

  const validateMutation = useMutation({
    mutationFn: async () => {
      if (!session?.accessToken) {
        throw new Error('Missing session.')
      }
      return validateMaintenancePartsKit(session.accessToken, buildPreviewPayload())
    },
    onSuccess: (result) => {
      setValidationResult(result)
      setErrorMessage(null)
    },
    onError: (error) => {
      setErrorMessage(error instanceof Error ? error.message : 'Unable to validate the draft.')
    },
  })

  const previewMutation = useMutation({
    mutationFn: async () => {
      if (!session?.accessToken) {
        throw new Error('Missing session.')
      }
      return previewMaintenancePartsKit(session.accessToken, buildPreviewPayload())
    },
    onSuccess: (result) => {
      setPreviewResult(result)
      setValidationResult(result.validation)
      setErrorMessage(null)
    },
    onError: (error) => {
      setErrorMessage(error instanceof Error ? error.message : 'Unable to preview the draft.')
    },
  })

  const submitMutation = useMutation({
    mutationFn: async () => {
      if (!session?.accessToken || !draftId) {
        throw new Error('Save the draft before requesting approval.')
      }
      return submitMaintenancePartsKitForApproval(session.accessToken, draftId)
    },
    onSuccess: (response) => {
      setDraftStatus(response.status)
      setErrorMessage(null)
    },
    onError: (error) => {
      setErrorMessage(error instanceof Error ? error.message : 'Unable to submit the kit for approval.')
    },
  })

  const activateMutation = useMutation({
    mutationFn: async () => {
      if (!session?.accessToken || !draftId) {
        throw new Error('Save the draft before activation.')
      }
      return activateMaintenancePartsKit(session.accessToken, draftId)
    },
    onSuccess: (response) => {
      setDraftStatus(response.status)
      setErrorMessage(null)
      void queryClient.invalidateQueries({ queryKey: ['maintainarr-parts-kits', session?.accessToken] })
      navigate('/parts-kits')
    },
    onError: (error) => {
      setErrorMessage(error instanceof Error ? error.message : 'Unable to activate the kit.')
    },
  })

  if (!session) {
    return <Navigate to="/launch" replace />
  }

  if (meQuery.isLoading) {
    return (
      <div className="mx-auto flex min-h-screen max-w-6xl items-center justify-center px-4 py-10" style={shellStyle}>
        <div className="rounded-3xl border px-6 py-5 shadow-2xl" style={panelStyle}>
          <div className="flex items-center gap-3 text-sm text-slate-200">
            <Loader2 className="h-4 w-4 animate-spin" />
            Loading MaintainArr permissions...
          </div>
        </div>
      </div>
    )
  }

  if (meQuery.isError) {
    return (
      <div className="mx-auto min-h-screen max-w-6xl px-4 py-10" style={shellStyle}>
        <div className="rounded-3xl border px-6 py-5 text-sm text-red-100 shadow-2xl" style={panelStyle}>
          Unable to load your MaintainArr session. Please launch the product again.
        </div>
      </div>
    )
  }

  if (!canCreate) {
    return (
      <div className="mx-auto min-h-screen max-w-6xl px-4 py-10" style={shellStyle}>
        <PageHeader title="Create Parts Kit" subtitle="MaintainArr guided builder" />
        <div className="rounded-3xl border px-6 py-6 text-sm text-slate-200 shadow-2xl" style={panelStyle}>
          <div className="flex items-start gap-3">
            <AlertTriangle className="mt-0.5 h-5 w-5 text-amber-400" />
            <div>
              <h2 className="text-lg font-semibold text-white">Permission denied</h2>
              <p className="mt-1 text-slate-300">
                Your role cannot create parts kits in this tenant.
              </p>
            </div>
          </div>
        </div>
      </div>
    )
  }

  const currentSectionComplete = (key: SectionKey): boolean => {
    switch (key) {
      case 'basics':
        return Boolean(title.trim() && (kitNumberOverride.trim() || generatedKitNumber.trim()))
      case 'applicability':
        return Boolean(
          assetTypeApplicabilityText.trim()
          || workOrderTypeApplicabilityText.trim()
          || pmPlanRef.trim()
          || applicabilityWorkOrderTypesText.trim()
          || applicabilityPmProgramRefsText.trim()
          || applicabilityInspectionTemplateRefsText.trim()
        )
      case 'scope':
        return Boolean(
          assetClassKeysText.trim()
          || assetTypeKeysText.trim()
          || assetCategoryKeysText.trim()
          || assetStatusKeysText.trim()
          || siteRefsText.trim()
          || includedAssetIds.length > 0
        )
      case 'items':
        return items.length > 0 && items.some((item) => item.itemDescriptionSnapshot.trim().length > 0)
      case 'rules':
        return quantityRules.length > 0
      case 'availability':
        return availabilityEnabled || availabilityNotes.trim().length > 0
      case 'behavior':
        return canBeManuallyAdded || autoSuggestOnMatchingWorkOrder || autoAddToMatchingWorkOrder || autoAddToPmGeneratedWorkOrder
      case 'compliance':
        return complianceRelated || safetyCritical || readinessSensitive
      case 'approval':
        return requiresApprovalBeforeActivation || approverRoleKey.trim().length > 0 || approverPersonId.trim().length > 0
      case 'review':
        return Boolean(draftId)
      default:
        return false
    }
  }

  function buildDefinitionRequest(): MaintenancePartsKitDefinitionRequest {
    const itemPayloads = items.map((item) => ({
      itemRef: item.itemRef.trim() || `item-${createId()}`,
      supplyarrPartId: trimToNull(item.supplyarrPartId),
      itemDescriptionSnapshot: item.itemDescriptionSnapshot.trim(),
      partNumberSnapshot: trimToNull(item.partNumberSnapshot),
      manufacturerPartNumberSnapshot: trimToNull(item.manufacturerPartNumberSnapshot),
      vendorPartNumberSnapshot: trimToNull(item.vendorPartNumberSnapshot),
      quantity: parseDecimal(item.quantity, 0),
      unitOfMeasure: item.unitOfMeasure.trim() || 'each',
      required: item.required,
      criticality: item.criticality.trim() || 'medium',
      substituteAllowed: item.substituteAllowed,
      preferredSubstituteRefs: splitDelimited(item.preferredSubstituteRefsText),
      consumable: item.consumable,
      serialized: item.serialized,
      coreReturnExpected: item.coreReturnExpected,
      hazardous: item.hazardous,
      warrantySensitive: item.warrantySensitive,
      requiredByTask: trimToNull(item.requiredByTask),
      notes: trimToNull(item.notes),
      tags: splitDelimited(item.tagsText),
      isPlaceholder: item.isPlaceholder,
    }))

    const rulePayloads = quantityRules.map((rule) => ({
      ruleId: rule.ruleId.trim() || `rule-${createId()}`,
      ruleType: rule.ruleType.trim() || 'default',
      appliesToItemRef: rule.appliesToItemRef.trim(),
      assetConditionSummary: trimToNull(rule.assetConditionSummary),
      workConditionSummary: trimToNull(rule.workConditionSummary),
      conditionSummary: trimToNull(rule.conditionSummary),
      baseQuantity: parseDecimal(rule.baseQuantity, 1),
      multiplier: parseDecimal(rule.multiplier, 1),
      minimumQuantity: trimToNull(rule.minimumQuantity) ? parseDecimal(rule.minimumQuantity, 0) : null,
      maximumQuantity: trimToNull(rule.maximumQuantity) ? parseDecimal(rule.maximumQuantity, 0) : null,
      roundingBehavior: rule.roundingBehavior.trim() || 'nearest',
      plainLanguageSummary: rule.plainLanguageSummary.trim(),
    }))

    return {
      applicabilityWorkOrderTypes: splitDelimited(applicabilityWorkOrderTypesText),
      applicabilityPmProgramRefs: splitDelimited(applicabilityPmProgramRefsText),
      applicabilityInspectionTemplateRefs: splitDelimited(applicabilityInspectionTemplateRefsText),
      applicabilityDefectTypes: splitDelimited(applicabilityDefectTypesText),
      applicabilityTaskTemplateRefs: splitDelimited(applicabilityTaskTemplateRefsText),
      applicabilityRepairCategories: splitDelimited(applicabilityRepairCategoriesText),
      workSourceCompatibilities: splitDelimited(workSourceCompatibilitiesText),
      assetScope: {
        assetClassKeys: splitDelimited(assetClassKeysText),
        assetTypeKeys: splitDelimited(assetTypeKeysText),
        assetCategoryKeys: splitDelimited(assetCategoryKeysText),
        assetStatusKeys: splitDelimited(assetStatusKeysText),
        siteRefs: splitDelimited(siteRefsText),
        departmentRefs: splitDelimited(departmentRefsText),
        makeKeys: splitDelimited(makeKeysText),
        modelKeys: splitDelimited(modelKeysText),
        yearFrom: trimToNull(yearFrom),
        yearTo: trimToNull(yearTo),
        fuelTypeKeys: splitDelimited(fuelTypeKeysText),
        bodyTypeKeys: splitDelimited(bodyTypeKeysText),
        configurationKeys: splitDelimited(configurationKeysText),
        variantFlags: splitDelimited(variantFlagsText),
        requiredAttributes: splitDelimited(requiredAttributesText),
        excludedAttributes: splitDelimited(excludedAttributesText),
        includedAssetIds,
        excludedAssetIds,
      },
      items: itemPayloads,
      quantityRules: rulePayloads,
      availability: {
        enabled: availabilityEnabled,
        preferredFulfillmentSource: trimToNull(availabilityPreferredSource),
        showSiteAvailability: availabilityShowSite,
        showNearbyAvailability: availabilityShowNearby,
        showOnOrder: availabilityShowOnOrder,
        showEstimatedLeadTime: availabilityShowLeadTime,
        requestReservation: availabilityRequestReservation,
        notes: trimToNull(availabilityNotes),
      },
      workOrderBehavior: {
        canBeManuallyAdded,
        autoSuggestOnMatchingWorkOrder,
        autoAddToMatchingWorkOrder,
        autoAddToPmGeneratedWorkOrder,
        autoAddAfterFailedInspectionQuestion,
        autoAddAfterMatchingDefectType,
        requireSupervisorApprovalBeforeAdding,
        requirePartsReviewBeforeWorkCanStart,
        requireAvailabilityCheckBeforeScheduling,
        allowTechnicianAdjustQuantities,
        requireAdjustmentReason,
        allowTechnicianRemoveOptionalItems,
        allowTechnicianRemoveRequiredItems,
        requireReasonToRemoveRequiredItem,
        snapshotKitItemsOntoWorkOrder,
        keepLiveReferenceAfterWorkOrderCreation,
      },
      compliance: {
        complianceRelated,
        governingBodyKeys: splitDelimited(governingBodyKeysText),
        citationRefs: splitDelimited(citationRefsText),
        safetyCritical,
        readinessSensitive,
        missingRequiredPartsBlockWorkStart,
        missingRequiredPartsBlockWorkCompletion,
        requireSupervisorApprovalForSubstitution,
        requireDocumentationForSubstitution,
        requireFinalInspectionAfterUse,
        linkedInspectionTemplateId: trimToNull(linkedInspectionTemplateId),
      },
      approval: {
        requiresApprovalBeforeActivation,
        approverRoleKey: trimToNull(approverRoleKey),
        approverPersonId: trimToNull(approverPersonId),
        retireReplacedKitAfterActivation,
        notesForApprover: trimToNull(notesForApprover),
      },
      changeReason: trimToNull(changeReason),
      versionLabel: trimToNull(versionLabel),
    }
  }

  function buildPayload(): Parameters<typeof createMaintenancePartsKit>[1] {
    return {
      kitNumber: kitNumberOverride.trim() || generatedKitNumber,
      title: title.trim(),
      description: trimToNull(description),
      assetTypeApplicability: splitDelimited(assetTypeApplicabilityText),
      workOrderTypeApplicability: splitDelimited(workOrderTypeApplicabilityText),
      pmPlanRef: trimToNull(pmPlanRef),
      kitCategoryKey: trimToNull(kitCategoryKey),
      kitTypeKey: trimToNull(kitTypeKey),
      priorityKey: trimToNull(priorityKey),
      owningSiteRef: trimToNull(owningSiteRef),
      owningTeamRef: trimToNull(owningTeamRef),
      ownerPersonId: trimToNull(ownerPersonId),
      ownerRoleKey: trimToNull(ownerRoleKey),
      tags: splitDelimited(tagsText),
      definition: buildDefinitionRequest(),
      effectiveAt: parseDateTimeInput(effectiveAt),
      expiresAt: parseDateTimeInput(expiresAt),
      cloneSourcePartsKitId: trimToNull(sourceKitId),
    }
  }

  function buildPreviewPayload(): MaintenancePartsKitPreviewRequest {
    return {
      ...buildPayload(),
      selectedAssetId: trimToNull(selectedAssetId),
    }
  }

  function applySeedKit(kit: MaintenancePartsKitResponse) {
    setDraftId(null)
    setDraftStatus('draft')
    setDraftVersion(kit.version ?? 1)
    setSavedKitNumber('')
    setLastSavedAt(null)
    setErrorMessage(null)
    setValidationResult(null)
    setPreviewResult(null)

    setKitNumberOverride(kit.kitNumber)
    setTitle(kit.title)
    setDescription(kit.description)
    setKitCategoryKey(kit.kitCategoryKey ?? '')
    setKitTypeKey(kit.kitTypeKey ?? '')
    setPriorityKey(kit.priorityKey ?? '')
    setOwningSiteRef(kit.owningSiteRef ?? '')
    setOwningTeamRef(kit.owningTeamRef ?? '')
    setOwnerPersonId(kit.ownerPersonId ?? '')
    setOwnerRoleKey(kit.ownerRoleKey ?? '')
    setTagsText(joinDelimited(kit.tags ?? []))
    setEffectiveAt(formatDateTimeInput(kit.effectiveAt ?? null))
    setExpiresAt(formatDateTimeInput(kit.expiresAt ?? null))
    setSourceKitId(kit.partsKitId)

    const definition = kit.definition
    if (definition) {
      setAssetTypeApplicabilityText(joinDelimited(kit.assetTypeApplicability ?? []))
      setWorkOrderTypeApplicabilityText(joinDelimited(kit.workOrderTypeApplicability ?? []))
      setPmPlanRef(kit.pmPlanRef ?? '')
      setApplicabilityWorkOrderTypesText(joinDelimited(definition.applicabilityWorkOrderTypes ?? []))
      setApplicabilityPmProgramRefsText(joinDelimited(definition.applicabilityPmProgramRefs ?? []))
      setApplicabilityInspectionTemplateRefsText(joinDelimited(definition.applicabilityInspectionTemplateRefs ?? []))
      setApplicabilityDefectTypesText(joinDelimited(definition.applicabilityDefectTypes ?? []))
      setApplicabilityTaskTemplateRefsText(joinDelimited(definition.applicabilityTaskTemplateRefs ?? []))
      setApplicabilityRepairCategoriesText(joinDelimited(definition.applicabilityRepairCategories ?? []))
      setWorkSourceCompatibilitiesText(joinDelimited(definition.workSourceCompatibilities ?? []))
      setAssetClassKeysText(joinDelimited(definition.assetScope.assetClassKeys ?? []))
      setAssetTypeKeysText(joinDelimited(definition.assetScope.assetTypeKeys ?? []))
      setAssetCategoryKeysText(joinDelimited(definition.assetScope.assetCategoryKeys ?? []))
      setAssetStatusKeysText(joinDelimited(definition.assetScope.assetStatusKeys ?? []))
      setSiteRefsText(joinDelimited(definition.assetScope.siteRefs ?? []))
      setDepartmentRefsText(joinDelimited(definition.assetScope.departmentRefs ?? []))
      setMakeKeysText(joinDelimited(definition.assetScope.makeKeys ?? []))
      setModelKeysText(joinDelimited(definition.assetScope.modelKeys ?? []))
      setYearFrom(definition.assetScope.yearFrom ?? '')
      setYearTo(definition.assetScope.yearTo ?? '')
      setFuelTypeKeysText(joinDelimited(definition.assetScope.fuelTypeKeys ?? []))
      setBodyTypeKeysText(joinDelimited(definition.assetScope.bodyTypeKeys ?? []))
      setConfigurationKeysText(joinDelimited(definition.assetScope.configurationKeys ?? []))
      setVariantFlagsText(joinDelimited(definition.assetScope.variantFlags ?? []))
      setRequiredAttributesText(joinDelimited(definition.assetScope.requiredAttributes ?? []))
      setExcludedAttributesText(joinDelimited(definition.assetScope.excludedAttributes ?? []))
      setIncludedAssetIds(definition.assetScope.includedAssetIds ?? [])
      setExcludedAssetIds(definition.assetScope.excludedAssetIds ?? [])
      setItems(
        (definition.items ?? []).map((item) => ({
          id: createId(),
          itemRef: item.itemRef,
          supplyarrPartId: item.supplyarrPartId ?? '',
          itemDescriptionSnapshot: item.itemDescriptionSnapshot,
          partNumberSnapshot: item.partNumberSnapshot ?? '',
          manufacturerPartNumberSnapshot: item.manufacturerPartNumberSnapshot ?? '',
          vendorPartNumberSnapshot: item.vendorPartNumberSnapshot ?? '',
          quantity: String(item.quantity),
          unitOfMeasure: item.unitOfMeasure,
          required: item.required,
          criticality: item.criticality,
          substituteAllowed: item.substituteAllowed,
          preferredSubstituteRefsText: joinDelimited(item.preferredSubstituteRefs ?? []),
          consumable: item.consumable,
          serialized: item.serialized,
          coreReturnExpected: item.coreReturnExpected,
          hazardous: item.hazardous,
          warrantySensitive: item.warrantySensitive,
          requiredByTask: item.requiredByTask ?? '',
          notes: item.notes ?? '',
          tagsText: joinDelimited(item.tags ?? []),
          isPlaceholder: item.isPlaceholder,
        })),
      )
      setQuantityRules(
        (definition.quantityRules ?? []).map((rule) => ({
          id: createId(),
          ruleId: rule.ruleId,
          ruleType: rule.ruleType,
          appliesToItemRef: rule.appliesToItemRef,
          assetConditionSummary: rule.assetConditionSummary ?? '',
          workConditionSummary: rule.workConditionSummary ?? '',
          conditionSummary: rule.conditionSummary ?? '',
          baseQuantity: String(rule.baseQuantity),
          multiplier: String(rule.multiplier),
          minimumQuantity: rule.minimumQuantity == null ? '' : String(rule.minimumQuantity),
          maximumQuantity: rule.maximumQuantity == null ? '' : String(rule.maximumQuantity),
          roundingBehavior: rule.roundingBehavior,
          plainLanguageSummary: rule.plainLanguageSummary,
        })),
      )
      setAvailabilityEnabled(definition.availability.enabled)
      setAvailabilityPreferredSource(definition.availability.preferredFulfillmentSource ?? '')
      setAvailabilityShowSite(definition.availability.showSiteAvailability)
      setAvailabilityShowNearby(definition.availability.showNearbyAvailability)
      setAvailabilityShowOnOrder(definition.availability.showOnOrder)
      setAvailabilityShowLeadTime(definition.availability.showEstimatedLeadTime)
      setAvailabilityRequestReservation(definition.availability.requestReservation)
      setAvailabilityNotes(definition.availability.notes ?? '')
      setCanBeManuallyAdded(definition.workOrderBehavior.canBeManuallyAdded)
      setAutoSuggestOnMatchingWorkOrder(definition.workOrderBehavior.autoSuggestOnMatchingWorkOrder)
      setAutoAddToMatchingWorkOrder(definition.workOrderBehavior.autoAddToMatchingWorkOrder)
      setAutoAddToPmGeneratedWorkOrder(definition.workOrderBehavior.autoAddToPmGeneratedWorkOrder)
      setAutoAddAfterFailedInspectionQuestion(definition.workOrderBehavior.autoAddAfterFailedInspectionQuestion)
      setAutoAddAfterMatchingDefectType(definition.workOrderBehavior.autoAddAfterMatchingDefectType)
      setRequireSupervisorApprovalBeforeAdding(definition.workOrderBehavior.requireSupervisorApprovalBeforeAdding)
      setRequirePartsReviewBeforeWorkCanStart(definition.workOrderBehavior.requirePartsReviewBeforeWorkCanStart)
      setRequireAvailabilityCheckBeforeScheduling(definition.workOrderBehavior.requireAvailabilityCheckBeforeScheduling)
      setAllowTechnicianAdjustQuantities(definition.workOrderBehavior.allowTechnicianAdjustQuantities)
      setRequireAdjustmentReason(definition.workOrderBehavior.requireAdjustmentReason)
      setAllowTechnicianRemoveOptionalItems(definition.workOrderBehavior.allowTechnicianRemoveOptionalItems)
      setAllowTechnicianRemoveRequiredItems(definition.workOrderBehavior.allowTechnicianRemoveRequiredItems)
      setRequireReasonToRemoveRequiredItem(definition.workOrderBehavior.requireReasonToRemoveRequiredItem)
      setSnapshotKitItemsOntoWorkOrder(definition.workOrderBehavior.snapshotKitItemsOntoWorkOrder)
      setKeepLiveReferenceAfterWorkOrderCreation(definition.workOrderBehavior.keepLiveReferenceAfterWorkOrderCreation)
      setComplianceRelated(definition.compliance.complianceRelated)
      setGoverningBodyKeysText(joinDelimited(definition.compliance.governingBodyKeys ?? []))
      setCitationRefsText(joinDelimited(definition.compliance.citationRefs ?? []))
      setSafetyCritical(definition.compliance.safetyCritical)
      setReadinessSensitive(definition.compliance.readinessSensitive)
      setMissingRequiredPartsBlockWorkStart(definition.compliance.missingRequiredPartsBlockWorkStart)
      setMissingRequiredPartsBlockWorkCompletion(definition.compliance.missingRequiredPartsBlockWorkCompletion)
      setRequireSupervisorApprovalForSubstitution(definition.compliance.requireSupervisorApprovalForSubstitution)
      setRequireDocumentationForSubstitution(definition.compliance.requireDocumentationForSubstitution)
      setRequireFinalInspectionAfterUse(definition.compliance.requireFinalInspectionAfterUse)
      setLinkedInspectionTemplateId(definition.compliance.linkedInspectionTemplateId ?? '')
      setRequiresApprovalBeforeActivation(definition.approval.requiresApprovalBeforeActivation)
      setApproverRoleKey(definition.approval.approverRoleKey ?? '')
      setApproverPersonId(definition.approval.approverPersonId ?? '')
      setRetireReplacedKitAfterActivation(definition.approval.retireReplacedKitAfterActivation)
      setNotesForApprover(definition.approval.notesForApprover ?? '')
      setChangeReason(definition.changeReason ?? '')
      setVersionLabel(definition.versionLabel ?? '')
    }
  }

  const nextSectionIndex = (index: number) => Math.min(index + 1, SECTION_DEFINITIONS.length - 1)

  const sectionCount = SECTION_DEFINITIONS.length

  const saveSection = (sectionIndex: number) => {
    void saveMutation.mutateAsync(nextSectionIndex(sectionIndex))
  }

  const runValidation = () => void validateMutation.mutateAsync()
  const runPreview = () => void previewMutation.mutateAsync()
  const runSave = () => void saveMutation.mutateAsync(expandedSectionIndex)

  const previewSummary = previewResult?.validation.summary ?? 'Validate the draft to see readiness guidance.'

  return (
    <div className="min-h-screen px-4 py-6 sm:px-6 lg:px-8" style={shellStyle} data-testid="parts-kit-create-page">
      <div className="mx-auto max-w-7xl space-y-6">
        <div className="rounded-[2rem] border px-5 py-5 shadow-2xl sm:px-6" style={panelStyle}>
          <PageHeader
            title="Create Parts Kit"
            subtitle={`Guided full-page builder for ${session.tenantDisplayName}`}
          />

          <div className="flex flex-wrap items-center justify-between gap-3">
            <Link
              to="/parts-kits"
              className="inline-flex items-center gap-2 text-sm text-slate-300 hover:text-white"
            >
              <ArrowLeft className="h-4 w-4" />
              Back to parts kits
            </Link>
            <div className="flex flex-wrap items-center gap-2">
              <DetailBadge label={draftStatus.replace(/_/g, ' ')} tone={toneForStatus(draftStatus)} />
              <DetailBadge label={draftId ? `Draft ${draftVersion}` : 'Unsaved draft'} tone={draftId ? 'good' : 'warn'} />
              {lastSavedAt ? <DetailBadge label={`Saved ${new Date(lastSavedAt).toLocaleTimeString()}`} tone="neutral" /> : null}
            </div>
          </div>
        </div>

        {errorMessage ? (
          <div className="rounded-3xl border border-rose-700/50 bg-rose-950/30 px-5 py-4 text-sm text-rose-100">
            {errorMessage}
          </div>
        ) : null}

        <div className="grid gap-6 xl:grid-cols-[minmax(0,1fr)_24rem]">
          <div className="space-y-5">
            {SECTION_DEFINITIONS.map((section, index) => {
              const open = index <= expandedSectionIndex
              const locked = index > expandedSectionIndex
              return (
                <SectionPanel
                  key={section.key}
                  title={section.label}
                  description={section.description}
                  icon={section.icon}
                  open={open}
                  locked={locked}
                  complete={currentSectionComplete(section.key)}
                  onToggle={() => {
                    if (!locked) {
                      setExpandedSectionIndex(index)
                    }
                  }}
                  footer={
                    <div className="flex flex-wrap gap-2">
                      <button
                        type="button"
                        onClick={() => saveSection(index)}
                        disabled={saveMutation.isPending}
                        className="inline-flex items-center gap-2 rounded-xl border border-sky-500/40 bg-sky-500/15 px-4 py-2 text-sm font-medium text-sky-100 hover:bg-sky-500/25 disabled:cursor-not-allowed disabled:opacity-60"
                      >
                        {saveMutation.isPending ? <Loader2 className="h-4 w-4 animate-spin" /> : <Sparkles className="h-4 w-4" />}
                        Save and continue
                      </button>
                      {index < sectionCount - 1 ? (
                        <button
                          type="button"
                          onClick={() => setExpandedSectionIndex(nextSectionIndex(index))}
                          className="inline-flex items-center gap-2 rounded-xl border border-slate-700 bg-slate-950 px-4 py-2 text-sm font-medium text-slate-200 hover:border-sky-600 hover:text-white"
                        >
                          Continue
                          <ArrowRight className="h-4 w-4" />
                        </button>
                      ) : null}
                    </div>
                  }
                >
                  {section.key === 'basics' ? (
                    <div className="grid gap-5 md:grid-cols-2">
                      <div className="md:col-span-2">
                        <GeneratedKeyField
                          sourceLabel={title}
                          generatedKey={generatedKitNumber}
                          confirmedKey={savedKitNumber || undefined}
                          manualOverride={kitNumberOverride}
                          onManualOverrideChange={setKitNumberOverride}
                          showAdvancedKey
                          allowManualOverride
                          label="Kit number"
                        />
                      </div>
                      <label className="block text-sm text-slate-300">
                        Title
                        <input className={fieldClass} value={title} onChange={(event) => setTitle(event.target.value)} placeholder="Brake service kit" />
                      </label>
                      <label className="block text-sm text-slate-300">
                        Category
                        <ControlledSelect value={kitCategoryKey} onChange={setKitCategoryKey} options={categoryOptions} emptyLabel="Choose a category" />
                      </label>
                      <label className="block text-sm text-slate-300">
                        Type
                        <ControlledSelect value={kitTypeKey} onChange={setKitTypeKey} options={typeOptions} emptyLabel="Choose a type" />
                      </label>
                      <label className="block text-sm text-slate-300">
                        Priority
                        <ControlledSelect value={priorityKey} onChange={setPriorityKey} options={priorityOptions} emptyLabel="Choose a priority" />
                      </label>
                      <label className="md:col-span-2 block text-sm text-slate-300">
                        Description
                        <textarea className={textareaClass} value={description} onChange={(event) => setDescription(event.target.value)} placeholder="What this kit is for, and when it should be used." />
                      </label>
                      <label className="block text-sm text-slate-300">
                        Owning site
                        <StaticSearchPicker value={owningSiteRef} onChange={setOwningSiteRef} options={siteOptions} placeholder="Search sites" />
                      </label>
                      <label className="block text-sm text-slate-300">
                        Owning team
                        <StaticSearchPicker value={owningTeamRef} onChange={setOwningTeamRef} options={teamOptions} placeholder="Search teams" />
                      </label>
                      <label className="block text-sm text-slate-300">
                        Owner person
                        <StaticSearchPicker value={ownerPersonId} onChange={setOwnerPersonId} options={peopleOptions} placeholder="Search people" />
                      </label>
                      <label className="block text-sm text-slate-300">
                        Owner role key
                        <input className={fieldClass} value={ownerRoleKey} onChange={(event) => setOwnerRoleKey(event.target.value)} placeholder="manager" />
                      </label>
                      <label className="md:col-span-2 block text-sm text-slate-300">
                        Tags
                        <textarea className={smallFieldClass} value={tagsText} onChange={(event) => setTagsText(event.target.value)} placeholder="brakes, safety, quarterly" />
                      </label>
                      <label className="block text-sm text-slate-300">
                        Effective at
                        <input className={fieldClass} type="datetime-local" value={effectiveAt} onChange={(event) => setEffectiveAt(event.target.value)} />
                      </label>
                      <label className="block text-sm text-slate-300">
                        Expires at
                        <input className={fieldClass} type="datetime-local" value={expiresAt} onChange={(event) => setExpiresAt(event.target.value)} />
                      </label>
                      <div className="md:col-span-2 grid gap-4 md:grid-cols-[minmax(0,1fr)_auto]">
                        <label className="block text-sm text-slate-300">
                          Seed from existing kit
                          <AsyncSearchPicker
                            value={sourceKitId}
                            onChange={setSourceKitId}
                            queryKey={['maintainarr-parts-kits-seed', session?.accessToken]}
                            queryFn={async (query) => {
                              const kits = await getMaintenancePartsKits(session!.accessToken)
                              const needle = query.trim().toLowerCase()
                              return kits.items
                                .filter((kit) => !needle || kit.kitNumber.toLowerCase().includes(needle) || kit.title.toLowerCase().includes(needle))
                                .slice(0, 25)
                                .map((kit) => ({ value: kit.partsKitId, label: `${kit.kitNumber} - ${kit.title}` }))
                            }}
                            selectedOption={sourceKitOption.data ?? undefined}
                            placeholder="Search existing kits"
                          />
                        </label>
                        <div className="flex items-end">
                          <button
                            type="button"
                            onClick={() => seedMutation.mutate()}
                            disabled={!sourceKitId || seedMutation.isPending}
                            className="inline-flex h-[42px] items-center gap-2 rounded-xl border border-slate-700 bg-slate-950 px-4 py-2 text-sm font-medium text-slate-200 hover:border-sky-600 hover:text-white disabled:cursor-not-allowed disabled:opacity-60"
                          >
                            {seedMutation.isPending ? <Loader2 className="h-4 w-4 animate-spin" /> : <ArrowRight className="h-4 w-4" />}
                            Load seed
                          </button>
                        </div>
                      </div>
                    </div>
                  ) : null}

                  {section.key === 'applicability' ? (
                    <div className="grid gap-5 md:grid-cols-2">
                      <label className="block text-sm text-slate-300">
                        Asset type applicability
                        <textarea className={textareaClass} value={assetTypeApplicabilityText} onChange={(event) => setAssetTypeApplicabilityText(event.target.value)} placeholder="type keys, one per line or comma-separated" />
                      </label>
                      <label className="block text-sm text-slate-300">
                        Work order type applicability
                        <textarea className={textareaClass} value={workOrderTypeApplicabilityText} onChange={(event) => setWorkOrderTypeApplicabilityText(event.target.value)} placeholder="corrective, preventive" />
                      </label>
                      <label className="block text-sm text-slate-300">
                        PM plan ref
                        <input className={fieldClass} value={pmPlanRef} onChange={(event) => setPmPlanRef(event.target.value)} placeholder="pm-plan-123" />
                      </label>
                      <label className="block text-sm text-slate-300">
                        Work source compatibilities
                        <textarea className={textareaClass} value={workSourceCompatibilitiesText} onChange={(event) => setWorkSourceCompatibilitiesText(event.target.value)} placeholder="manual, pm, defect, inspection" />
                      </label>
                      <label className="block text-sm text-slate-300">
                        PM program refs
                        <textarea className={textareaClass} value={applicabilityPmProgramRefsText} onChange={(event) => setApplicabilityPmProgramRefsText(event.target.value)} placeholder="pm-program refs" />
                      </label>
                      <label className="block text-sm text-slate-300">
                        Inspection template refs
                        <textarea className={textareaClass} value={applicabilityInspectionTemplateRefsText} onChange={(event) => setApplicabilityInspectionTemplateRefsText(event.target.value)} placeholder="inspection template refs" />
                      </label>
                      <label className="block text-sm text-slate-300">
                        Defect types
                        <textarea className={textareaClass} value={applicabilityDefectTypesText} onChange={(event) => setApplicabilityDefectTypesText(event.target.value)} placeholder="defect types" />
                      </label>
                      <label className="block text-sm text-slate-300">
                        Task template refs
                        <textarea className={textareaClass} value={applicabilityTaskTemplateRefsText} onChange={(event) => setApplicabilityTaskTemplateRefsText(event.target.value)} placeholder="task template refs" />
                      </label>
                      <label className="block text-sm text-slate-300">
                        Repair categories
                        <textarea className={textareaClass} value={applicabilityRepairCategoriesText} onChange={(event) => setApplicabilityRepairCategoriesText(event.target.value)} placeholder="repair categories" />
                      </label>
                    </div>
                  ) : null}

                  {section.key === 'scope' ? (
                    <div className="space-y-5">
                      <div className="grid gap-5 md:grid-cols-2">
                        <label className="block text-sm text-slate-300">
                          Asset class keys
                          <textarea className={textareaClass} value={assetClassKeysText} onChange={(event) => setAssetClassKeysText(event.target.value)} placeholder="truck, trailer" />
                        </label>
                        <label className="block text-sm text-slate-300">
                          Asset type keys
                          <textarea className={textareaClass} value={assetTypeKeysText} onChange={(event) => setAssetTypeKeysText(event.target.value)} placeholder="type keys" />
                        </label>
                        <label className="block text-sm text-slate-300">
                          Asset category keys
                          <textarea className={textareaClass} value={assetCategoryKeysText} onChange={(event) => setAssetCategoryKeysText(event.target.value)} placeholder="category keys" />
                        </label>
                        <label className="block text-sm text-slate-300">
                          Asset status keys
                          <textarea className={textareaClass} value={assetStatusKeysText} onChange={(event) => setAssetStatusKeysText(event.target.value)} placeholder="active, inactive" />
                        </label>
                        <label className="block text-sm text-slate-300">
                          Site refs
                          <textarea className={textareaClass} value={siteRefsText} onChange={(event) => setSiteRefsText(event.target.value)} placeholder="site refs" />
                        </label>
                        <label className="block text-sm text-slate-300">
                          Department refs
                          <textarea className={textareaClass} value={departmentRefsText} onChange={(event) => setDepartmentRefsText(event.target.value)} placeholder="department refs" />
                        </label>
                        <label className="block text-sm text-slate-300">
                          Make keys
                          <textarea className={textareaClass} value={makeKeysText} onChange={(event) => setMakeKeysText(event.target.value)} placeholder="make keys" />
                        </label>
                        <label className="block text-sm text-slate-300">
                          Model keys
                          <textarea className={textareaClass} value={modelKeysText} onChange={(event) => setModelKeysText(event.target.value)} placeholder="model keys" />
                        </label>
                        <label className="block text-sm text-slate-300">
                          Year from
                          <input className={fieldClass} value={yearFrom} onChange={(event) => setYearFrom(event.target.value)} placeholder="2018" />
                        </label>
                        <label className="block text-sm text-slate-300">
                          Year to
                          <input className={fieldClass} value={yearTo} onChange={(event) => setYearTo(event.target.value)} placeholder="2024" />
                        </label>
                        <label className="block text-sm text-slate-300">
                          Fuel type keys
                          <textarea className={textareaClass} value={fuelTypeKeysText} onChange={(event) => setFuelTypeKeysText(event.target.value)} placeholder="diesel, electric" />
                        </label>
                        <label className="block text-sm text-slate-300">
                          Body type keys
                          <textarea className={textareaClass} value={bodyTypeKeysText} onChange={(event) => setBodyTypeKeysText(event.target.value)} placeholder="van, box, chassis" />
                        </label>
                        <label className="block text-sm text-slate-300">
                          Configuration keys
                          <textarea className={textareaClass} value={configurationKeysText} onChange={(event) => setConfigurationKeysText(event.target.value)} placeholder="configurations" />
                        </label>
                        <label className="block text-sm text-slate-300">
                          Variant flags
                          <textarea className={textareaClass} value={variantFlagsText} onChange={(event) => setVariantFlagsText(event.target.value)} placeholder="short-wheelbase, heavy-duty" />
                        </label>
                        <label className="block text-sm text-slate-300">
                          Required attributes
                          <textarea className={textareaClass} value={requiredAttributesText} onChange={(event) => setRequiredAttributesText(event.target.value)} placeholder="required attributes" />
                        </label>
                        <label className="block text-sm text-slate-300">
                          Excluded attributes
                          <textarea className={textareaClass} value={excludedAttributesText} onChange={(event) => setExcludedAttributesText(event.target.value)} placeholder="excluded attributes" />
                        </label>
                      </div>
                      <div className="grid gap-5 md:grid-cols-2">
                        <label className="block text-sm text-slate-300">
                          Included assets
                            <AsyncMultiPicker
                              values={includedAssetIds}
                              onChange={setIncludedAssetIds}
                              queryKey={['maintainarr-parts-kit-assets-included', session?.accessToken]}
                              queryFn={async (query) => mapAssetOptions(await searchAssets(session!.accessToken, query, 25))}
                              selectedOptions={includedAssetOptionsQuery.data ?? []}
                              placeholder="Search assets to include"
                            />
                        </label>
                        <label className="block text-sm text-slate-300">
                          Excluded assets
                            <AsyncMultiPicker
                              values={excludedAssetIds}
                              onChange={setExcludedAssetIds}
                              queryKey={['maintainarr-parts-kit-assets-excluded', session?.accessToken]}
                              queryFn={async (query) => mapAssetOptions(await searchAssets(session!.accessToken, query, 25))}
                              selectedOptions={excludedAssetOptionsQuery.data ?? []}
                              placeholder="Search assets to exclude"
                            />
                        </label>
                      </div>
                      <div className="grid gap-5 md:grid-cols-[minmax(0,1fr)_minmax(0,1fr)]">
                        <label className="block text-sm text-slate-300">
                          Preview asset
                          <AsyncSearchPicker
                            value={selectedAssetId}
                            onChange={setSelectedAssetId}
                            queryKey={['maintainarr-parts-kit-assets-preview', session?.accessToken]}
                            queryFn={async (query) => mapAssetOptions(await searchAssets(session!.accessToken, query, 25))}
                            selectedOption={selectedPreviewAssetQuery.data ?? undefined}
                            placeholder="Search an asset to preview"
                          />
                        </label>
                        <div className="rounded-2xl border border-slate-800 bg-slate-950/60 p-4 text-sm text-slate-300">
                          <p className="font-semibold text-white">Scope summary</p>
                          <p className="mt-2">{summarizeScope(splitDelimited(assetClassKeysText))} asset classes, {summarizeScope(splitDelimited(assetTypeKeysText))} asset types, and {summarizeScope(splitDelimited(siteRefsText))} sites.</p>
                          <p className="mt-2 text-slate-400">
                            Included assets and excluded assets are honored directly in the preview and validation passes.
                          </p>
                        </div>
                      </div>
                    </div>
                  ) : null}

                  {section.key === 'items' ? (
                    <div className="space-y-4">
                      {items.map((item, indexItem) => (
                        <div key={item.id} className="rounded-2xl border border-slate-800 bg-slate-950/70 p-4">
                          <div className="flex items-center justify-between gap-3">
                            <div>
                              <h3 className="text-base font-semibold text-white">Kit item {indexItem + 1}</h3>
                              <p className="text-sm text-slate-400">Describe the line item and any snapshot data you want on work orders.</p>
                            </div>
                            <button
                              type="button"
                              onClick={() => setItems((current) => current.filter((entry) => entry.id !== item.id))}
                              className="inline-flex items-center gap-2 rounded-xl border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-300 hover:border-rose-600 hover:text-rose-200"
                            >
                              <Trash2 className="h-4 w-4" />
                              Remove
                            </button>
                          </div>
                          <div className="mt-4 grid gap-4 md:grid-cols-2">
                            <label className="block text-sm text-slate-300">
                              Line key
                              <input className={fieldClass} value={item.itemRef} onChange={(event) => setItems((current) => current.map((entry) => (entry.id === item.id ? { ...entry, itemRef: event.target.value } : entry)))} placeholder="brake-pads" />
                            </label>
                            <label className="block text-sm text-slate-300">
                              Part reference
                            <AsyncSearchPicker
                              value={item.supplyarrPartId}
                              onChange={(value) => setItems((current) => current.map((entry) => (entry.id === item.id ? { ...entry, supplyarrPartId: value } : entry)))}
                              queryKey={['maintainarr-parts-search', session?.accessToken]}
                              queryFn={async (_query) => mapPartsOptions(await getParts(session!.accessToken))}
                              placeholder="Search parts"
                            />
                            </label>
                            <label className="md:col-span-2 block text-sm text-slate-300">
                              Description snapshot
                              <input className={fieldClass} value={item.itemDescriptionSnapshot} onChange={(event) => setItems((current) => current.map((entry) => (entry.id === item.id ? { ...entry, itemDescriptionSnapshot: event.target.value } : entry)))} placeholder="Brake pads, front axle" />
                            </label>
                            <label className="block text-sm text-slate-300">
                              Part number snapshot
                              <input className={fieldClass} value={item.partNumberSnapshot} onChange={(event) => setItems((current) => current.map((entry) => (entry.id === item.id ? { ...entry, partNumberSnapshot: event.target.value } : entry)))} />
                            </label>
                            <label className="block text-sm text-slate-300">
                              Manufacturer part number
                              <input className={fieldClass} value={item.manufacturerPartNumberSnapshot} onChange={(event) => setItems((current) => current.map((entry) => (entry.id === item.id ? { ...entry, manufacturerPartNumberSnapshot: event.target.value } : entry)))} />
                            </label>
                            <label className="block text-sm text-slate-300">
                              Vendor part number
                              <input className={fieldClass} value={item.vendorPartNumberSnapshot} onChange={(event) => setItems((current) => current.map((entry) => (entry.id === item.id ? { ...entry, vendorPartNumberSnapshot: event.target.value } : entry)))} />
                            </label>
                            <label className="block text-sm text-slate-300">
                              Quantity
                              <input className={fieldClass} value={item.quantity} onChange={(event) => setItems((current) => current.map((entry) => (entry.id === item.id ? { ...entry, quantity: event.target.value } : entry)))} />
                            </label>
                            <label className="block text-sm text-slate-300">
                              Unit of measure
                              <input className={fieldClass} value={item.unitOfMeasure} onChange={(event) => setItems((current) => current.map((entry) => (entry.id === item.id ? { ...entry, unitOfMeasure: event.target.value } : entry)))} />
                            </label>
                            <label className="block text-sm text-slate-300">
                              Criticality
                              <ControlledSelect value={item.criticality} onChange={(value) => setItems((current) => current.map((entry) => (entry.id === item.id ? { ...entry, criticality: value } : entry)))} options={criticalityOptions} />
                            </label>
                            <label className="block text-sm text-slate-300">
                              Required by task
                              <input className={fieldClass} value={item.requiredByTask} onChange={(event) => setItems((current) => current.map((entry) => (entry.id === item.id ? { ...entry, requiredByTask: event.target.value } : entry)))} />
                            </label>
                            <label className="md:col-span-2 block text-sm text-slate-300">
                              Preferred substitutes
                              <textarea className={smallFieldClass} value={item.preferredSubstituteRefsText} onChange={(event) => setItems((current) => current.map((entry) => (entry.id === item.id ? { ...entry, preferredSubstituteRefsText: event.target.value } : entry)))} placeholder="part refs, one per line" />
                            </label>
                            <label className="md:col-span-2 block text-sm text-slate-300">
                              Notes
                              <textarea className={smallFieldClass} value={item.notes} onChange={(event) => setItems((current) => current.map((entry) => (entry.id === item.id ? { ...entry, notes: event.target.value } : entry)))} />
                            </label>
                            <label className="md:col-span-2 block text-sm text-slate-300">
                              Tags
                              <textarea className={smallFieldClass} value={item.tagsText} onChange={(event) => setItems((current) => current.map((entry) => (entry.id === item.id ? { ...entry, tagsText: event.target.value } : entry)))} placeholder="line-item tags" />
                            </label>
                          </div>
                          <div className="mt-4 grid gap-3 md:grid-cols-2 xl:grid-cols-4">
                            {[
                              ['required', item.required, 'Required'],
                              ['substituteAllowed', item.substituteAllowed, 'Substitute allowed'],
                              ['consumable', item.consumable, 'Consumable'],
                              ['serialized', item.serialized, 'Serialized'],
                              ['coreReturnExpected', item.coreReturnExpected, 'Core return expected'],
                              ['hazardous', item.hazardous, 'Hazardous'],
                              ['warrantySensitive', item.warrantySensitive, 'Warranty sensitive'],
                              ['isPlaceholder', item.isPlaceholder, 'Placeholder'],
                            ].map(([key, checked, label]) => (
                              <label key={String(key)} className="flex items-center gap-2 rounded-xl border border-slate-800 bg-slate-950 px-3 py-2 text-sm text-slate-300">
                                <input
                                  type="checkbox"
                                  checked={Boolean(checked)}
                                  onChange={(event) => setItems((current) => current.map((entry) => (entry.id === item.id ? { ...entry, [key as keyof KitItemDraft]: event.target.checked } : entry)))}
                                />
                                {label}
                              </label>
                            ))}
                          </div>
                        </div>
                      ))}
                      <button
                        type="button"
                        onClick={() => setItems((current) => [...current, createBlankItem()])}
                        className="inline-flex items-center gap-2 rounded-xl border border-slate-700 bg-slate-950 px-4 py-2 text-sm font-medium text-slate-200 hover:border-sky-600 hover:text-white"
                      >
                        <CirclePlus className="h-4 w-4" />
                        Add kit item
                      </button>
                    </div>
                  ) : null}

                  {section.key === 'rules' ? (
                    <div className="space-y-4">
                      {quantityRules.map((rule, indexRule) => (
                        <div key={rule.id} className="rounded-2xl border border-slate-800 bg-slate-950/70 p-4">
                          <div className="flex items-center justify-between gap-3">
                            <div>
                              <h3 className="text-base font-semibold text-white">Quantity rule {indexRule + 1}</h3>
                              <p className="text-sm text-slate-400">Use this for multipliers, minimums, or alternate rounding.</p>
                            </div>
                            <button
                              type="button"
                              onClick={() => setQuantityRules((current) => current.filter((entry) => entry.id !== rule.id))}
                              className="inline-flex items-center gap-2 rounded-xl border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-300 hover:border-rose-600 hover:text-rose-200"
                            >
                              <Trash2 className="h-4 w-4" />
                              Remove
                            </button>
                          </div>
                          <div className="mt-4 grid gap-4 md:grid-cols-2">
                            <label className="block text-sm text-slate-300">
                              Rule key
                              <input className={fieldClass} value={rule.ruleId} onChange={(event) => setQuantityRules((current) => current.map((entry) => (entry.id === rule.id ? { ...entry, ruleId: event.target.value } : entry)))} placeholder="rule-1" />
                            </label>
                            <label className="block text-sm text-slate-300">
                              Rule type
                              <ControlledSelect value={rule.ruleType} onChange={(value) => setQuantityRules((current) => current.map((entry) => (entry.id === rule.id ? { ...entry, ruleType: value } : entry)))} options={behaviorSelectOptions} />
                            </label>
                            <label className="block text-sm text-slate-300">
                              Applies to item ref
                              <input className={fieldClass} value={rule.appliesToItemRef} onChange={(event) => setQuantityRules((current) => current.map((entry) => (entry.id === rule.id ? { ...entry, appliesToItemRef: event.target.value } : entry)))} />
                            </label>
                            <label className="block text-sm text-slate-300">
                              Rounding
                              <ControlledSelect value={rule.roundingBehavior} onChange={(value) => setQuantityRules((current) => current.map((entry) => (entry.id === rule.id ? { ...entry, roundingBehavior: value } : entry)))} options={roundingOptions} />
                            </label>
                            <label className="block text-sm text-slate-300">
                              Base quantity
                              <input className={fieldClass} value={rule.baseQuantity} onChange={(event) => setQuantityRules((current) => current.map((entry) => (entry.id === rule.id ? { ...entry, baseQuantity: event.target.value } : entry)))} />
                            </label>
                            <label className="block text-sm text-slate-300">
                              Multiplier
                              <input className={fieldClass} value={rule.multiplier} onChange={(event) => setQuantityRules((current) => current.map((entry) => (entry.id === rule.id ? { ...entry, multiplier: event.target.value } : entry)))} />
                            </label>
                            <label className="block text-sm text-slate-300">
                              Minimum quantity
                              <input className={fieldClass} value={rule.minimumQuantity} onChange={(event) => setQuantityRules((current) => current.map((entry) => (entry.id === rule.id ? { ...entry, minimumQuantity: event.target.value } : entry)))} />
                            </label>
                            <label className="block text-sm text-slate-300">
                              Maximum quantity
                              <input className={fieldClass} value={rule.maximumQuantity} onChange={(event) => setQuantityRules((current) => current.map((entry) => (entry.id === rule.id ? { ...entry, maximumQuantity: event.target.value } : entry)))} />
                            </label>
                            <label className="md:col-span-2 block text-sm text-slate-300">
                              Condition summary
                              <input className={fieldClass} value={rule.conditionSummary} onChange={(event) => setQuantityRules((current) => current.map((entry) => (entry.id === rule.id ? { ...entry, conditionSummary: event.target.value } : entry)))} />
                            </label>
                            <label className="md:col-span-2 block text-sm text-slate-300">
                              Plain-language summary
                              <textarea className={smallFieldClass} value={rule.plainLanguageSummary} onChange={(event) => setQuantityRules((current) => current.map((entry) => (entry.id === rule.id ? { ...entry, plainLanguageSummary: event.target.value } : entry)))} />
                            </label>
                          </div>
                        </div>
                      ))}
                      <button
                        type="button"
                        onClick={() => setQuantityRules((current) => [...current, createBlankRule()])}
                        className="inline-flex items-center gap-2 rounded-xl border border-slate-700 bg-slate-950 px-4 py-2 text-sm font-medium text-slate-200 hover:border-sky-600 hover:text-white"
                      >
                        <CirclePlus className="h-4 w-4" />
                        Add quantity rule
                      </button>
                    </div>
                  ) : null}

                  {section.key === 'availability' ? (
                    <div className="grid gap-5 md:grid-cols-2">
                      <label className="flex items-center gap-3 rounded-2xl border border-slate-800 bg-slate-950/80 p-4 text-sm text-slate-200">
                        <input type="checkbox" checked={availabilityEnabled} onChange={(event) => setAvailabilityEnabled(event.target.checked)} />
                        Enable availability preview
                      </label>
                      <label className="block text-sm text-slate-300">
                        Preferred source
                        <input className={fieldClass} value={availabilityPreferredSource} onChange={(event) => setAvailabilityPreferredSource(event.target.value)} placeholder="supplyarr, inventory, vendor" />
                      </label>
                      {[
                        ['showSite', availabilityShowSite, setAvailabilityShowSite, 'Show site availability'],
                        ['showNearby', availabilityShowNearby, setAvailabilityShowNearby, 'Show nearby availability'],
                        ['showOnOrder', availabilityShowOnOrder, setAvailabilityShowOnOrder, 'Show on-order quantities'],
                        ['showLeadTime', availabilityShowLeadTime, setAvailabilityShowLeadTime, 'Show lead time'],
                        ['requestReservation', availabilityRequestReservation, setAvailabilityRequestReservation, 'Request reservation'],
                      ].map(([key, checked, setter, label]) => (
                        <label key={String(key)} className="flex items-center gap-3 rounded-2xl border border-slate-800 bg-slate-950/80 p-4 text-sm text-slate-200">
                          <input type="checkbox" checked={Boolean(checked)} onChange={(event) => (setter as (value: boolean) => void)(event.target.checked)} />
                          {label as string}
                        </label>
                      ))}
                      <label className="md:col-span-2 block text-sm text-slate-300">
                        Availability notes
                        <textarea className={smallFieldClass} value={availabilityNotes} onChange={(event) => setAvailabilityNotes(event.target.value)} />
                      </label>
                    </div>
                  ) : null}

                  {section.key === 'behavior' ? (
                    <div className="grid gap-3 md:grid-cols-2 xl:grid-cols-3">
                      {[
                        ['canBeManuallyAdded', canBeManuallyAdded, setCanBeManuallyAdded, 'Can be manually added'],
                        ['autoSuggestOnMatchingWorkOrder', autoSuggestOnMatchingWorkOrder, setAutoSuggestOnMatchingWorkOrder, 'Auto-suggest on matching work order'],
                        ['autoAddToMatchingWorkOrder', autoAddToMatchingWorkOrder, setAutoAddToMatchingWorkOrder, 'Auto-add to matching work order'],
                        ['autoAddToPmGeneratedWorkOrder', autoAddToPmGeneratedWorkOrder, setAutoAddToPmGeneratedWorkOrder, 'Auto-add to PM-generated work order'],
                        ['autoAddAfterFailedInspectionQuestion', autoAddAfterFailedInspectionQuestion, setAutoAddAfterFailedInspectionQuestion, 'Auto-add after failed inspection'],
                        ['autoAddAfterMatchingDefectType', autoAddAfterMatchingDefectType, setAutoAddAfterMatchingDefectType, 'Auto-add after matching defect type'],
                        ['requireSupervisorApprovalBeforeAdding', requireSupervisorApprovalBeforeAdding, setRequireSupervisorApprovalBeforeAdding, 'Require supervisor approval before adding'],
                        ['requirePartsReviewBeforeWorkCanStart', requirePartsReviewBeforeWorkCanStart, setRequirePartsReviewBeforeWorkCanStart, 'Require parts review before work can start'],
                        ['requireAvailabilityCheckBeforeScheduling', requireAvailabilityCheckBeforeScheduling, setRequireAvailabilityCheckBeforeScheduling, 'Require availability check before scheduling'],
                        ['allowTechnicianAdjustQuantities', allowTechnicianAdjustQuantities, setAllowTechnicianAdjustQuantities, 'Allow technicians to adjust quantities'],
                        ['requireAdjustmentReason', requireAdjustmentReason, setRequireAdjustmentReason, 'Require adjustment reason'],
                        ['allowTechnicianRemoveOptionalItems', allowTechnicianRemoveOptionalItems, setAllowTechnicianRemoveOptionalItems, 'Allow removing optional items'],
                        ['allowTechnicianRemoveRequiredItems', allowTechnicianRemoveRequiredItems, setAllowTechnicianRemoveRequiredItems, 'Allow removing required items'],
                        ['requireReasonToRemoveRequiredItem', requireReasonToRemoveRequiredItem, setRequireReasonToRemoveRequiredItem, 'Require reason to remove required item'],
                        ['snapshotKitItemsOntoWorkOrder', snapshotKitItemsOntoWorkOrder, setSnapshotKitItemsOntoWorkOrder, 'Snapshot items onto work order'],
                        ['keepLiveReferenceAfterWorkOrderCreation', keepLiveReferenceAfterWorkOrderCreation, setKeepLiveReferenceAfterWorkOrderCreation, 'Keep live reference after creation'],
                      ].map(([key, checked, setter, label]) => (
                        <label key={String(key)} className="flex items-center gap-3 rounded-2xl border border-slate-800 bg-slate-950/80 p-4 text-sm text-slate-200">
                          <input type="checkbox" checked={Boolean(checked)} onChange={(event) => (setter as (value: boolean) => void)(event.target.checked)} />
                          {label as string}
                        </label>
                      ))}
                    </div>
                  ) : null}

                  {section.key === 'compliance' ? (
                    <div className="grid gap-5 md:grid-cols-2">
                      <label className="flex items-center gap-3 rounded-2xl border border-slate-800 bg-slate-950/80 p-4 text-sm text-slate-200">
                        <input type="checkbox" checked={complianceRelated} onChange={(event) => setComplianceRelated(event.target.checked)} />
                        Compliance related
                      </label>
                      <label className="flex items-center gap-3 rounded-2xl border border-slate-800 bg-slate-950/80 p-4 text-sm text-slate-200">
                        <input type="checkbox" checked={safetyCritical} onChange={(event) => setSafetyCritical(event.target.checked)} />
                        Safety critical
                      </label>
                      <label className="flex items-center gap-3 rounded-2xl border border-slate-800 bg-slate-950/80 p-4 text-sm text-slate-200">
                        <input type="checkbox" checked={readinessSensitive} onChange={(event) => setReadinessSensitive(event.target.checked)} />
                        Readiness sensitive
                      </label>
                      <label className="flex items-center gap-3 rounded-2xl border border-slate-800 bg-slate-950/80 p-4 text-sm text-slate-200">
                        <input type="checkbox" checked={missingRequiredPartsBlockWorkStart} onChange={(event) => setMissingRequiredPartsBlockWorkStart(event.target.checked)} />
                        Block work start when parts are missing
                      </label>
                      <label className="flex items-center gap-3 rounded-2xl border border-slate-800 bg-slate-950/80 p-4 text-sm text-slate-200">
                        <input type="checkbox" checked={missingRequiredPartsBlockWorkCompletion} onChange={(event) => setMissingRequiredPartsBlockWorkCompletion(event.target.checked)} />
                        Block work completion when parts are missing
                      </label>
                      <label className="flex items-center gap-3 rounded-2xl border border-slate-800 bg-slate-950/80 p-4 text-sm text-slate-200">
                        <input type="checkbox" checked={requireSupervisorApprovalForSubstitution} onChange={(event) => setRequireSupervisorApprovalForSubstitution(event.target.checked)} />
                        Supervisor approval for substitution
                      </label>
                      <label className="flex items-center gap-3 rounded-2xl border border-slate-800 bg-slate-950/80 p-4 text-sm text-slate-200">
                        <input type="checkbox" checked={requireDocumentationForSubstitution} onChange={(event) => setRequireDocumentationForSubstitution(event.target.checked)} />
                        Documentation for substitution
                      </label>
                      <label className="flex items-center gap-3 rounded-2xl border border-slate-800 bg-slate-950/80 p-4 text-sm text-slate-200">
                        <input type="checkbox" checked={requireFinalInspectionAfterUse} onChange={(event) => setRequireFinalInspectionAfterUse(event.target.checked)} />
                        Final inspection after use
                      </label>
                      <label className="md:col-span-2 block text-sm text-slate-300">
                        Governing body keys
                        <textarea className={textareaClass} value={governingBodyKeysText} onChange={(event) => setGoverningBodyKeysText(event.target.value)} placeholder="osha, dot, compliance-core" />
                      </label>
                      <label className="md:col-span-2 block text-sm text-slate-300">
                        Citation refs
                        <textarea className={textareaClass} value={citationRefsText} onChange={(event) => setCitationRefsText(event.target.value)} placeholder="citations, standards, policy refs" />
                      </label>
                      <label className="block text-sm text-slate-300">
                        Linked inspection template
                        <input className={fieldClass} value={linkedInspectionTemplateId} onChange={(event) => setLinkedInspectionTemplateId(event.target.value)} placeholder="inspection template id" />
                      </label>
                    </div>
                  ) : null}

                  {section.key === 'approval' ? (
                    <div className="grid gap-5 md:grid-cols-2">
                      <label className="flex items-center gap-3 rounded-2xl border border-slate-800 bg-slate-950/80 p-4 text-sm text-slate-200">
                        <input type="checkbox" checked={requiresApprovalBeforeActivation} onChange={(event) => setRequiresApprovalBeforeActivation(event.target.checked)} />
                        Requires approval before activation
                      </label>
                      <label className="flex items-center gap-3 rounded-2xl border border-slate-800 bg-slate-950/80 p-4 text-sm text-slate-200">
                        <input type="checkbox" checked={retireReplacedKitAfterActivation} onChange={(event) => setRetireReplacedKitAfterActivation(event.target.checked)} />
                        Retire replaced kit after activation
                      </label>
                      <label className="block text-sm text-slate-300">
                        Approver role key
                        <input className={fieldClass} value={approverRoleKey} onChange={(event) => setApproverRoleKey(event.target.value)} placeholder="manager" />
                      </label>
                      <label className="block text-sm text-slate-300">
                        Approver person
                        <StaticSearchPicker value={approverPersonId} onChange={setApproverPersonId} options={peopleOptions} placeholder="Search people" />
                      </label>
                      <label className="md:col-span-2 block text-sm text-slate-300">
                        Notes for approver
                        <textarea className={smallFieldClass} value={notesForApprover} onChange={(event) => setNotesForApprover(event.target.value)} />
                      </label>
                      <label className="md:col-span-2 block text-sm text-slate-300">
                        Change reason
                        <textarea className={smallFieldClass} value={changeReason} onChange={(event) => setChangeReason(event.target.value)} />
                      </label>
                      <label className="block text-sm text-slate-300">
                        Version label
                        <input className={fieldClass} value={versionLabel} onChange={(event) => setVersionLabel(event.target.value)} placeholder="v1.0" />
                      </label>
                    </div>
                  ) : null}

                  {section.key === 'review' ? (
                    <div className="grid gap-6 xl:grid-cols-[minmax(0,1fr)_20rem]">
                      <div className="space-y-4">
                        <div className="rounded-2xl border border-slate-800 bg-slate-950/60 p-4">
                          <h3 className="text-base font-semibold text-white">Validation summary</h3>
                          <p className="mt-2 text-sm text-slate-300">{previewSummary}</p>
                          {validationResult ? (
                            <div className="mt-3 grid gap-3 md:grid-cols-2">
                              <div className="rounded-xl border border-slate-800 bg-slate-950/70 p-3">
                                <p className="text-xs uppercase tracking-wide text-[var(--color-text-muted)]">Compatible assets</p>
                                <p className="mt-1 text-lg font-semibold text-white">{validationResult.compatibleAssetCount}</p>
                              </div>
                              <div className="rounded-xl border border-slate-800 bg-slate-950/70 p-3">
                                <p className="text-xs uppercase tracking-wide text-[var(--color-text-muted)]">Items</p>
                                <p className="mt-1 text-lg font-semibold text-white">{validationResult.requiredItemCount + validationResult.optionalItemCount}</p>
                              </div>
                            </div>
                          ) : null}
                          {validationResult?.errors.length ? (
                            <ul className="mt-4 space-y-2 text-sm text-rose-200">
                              {validationResult.errors.map((error) => (
                                <li key={error} className="rounded-xl border border-rose-700/40 bg-rose-950/30 px-3 py-2">{error}</li>
                              ))}
                            </ul>
                          ) : null}
                          {validationResult?.warnings.length ? (
                            <ul className="mt-4 space-y-2 text-sm text-amber-200">
                              {validationResult.warnings.map((warning) => (
                                <li key={warning} className="rounded-xl border border-amber-700/40 bg-amber-950/30 px-3 py-2">{warning}</li>
                              ))}
                            </ul>
                          ) : null}
                        </div>
                        <div className="rounded-2xl border border-slate-800 bg-slate-950/60 p-4">
                          <h3 className="text-base font-semibold text-white">Preview results</h3>
                          {previewResult ? (
                            <div className="mt-4 space-y-4">
                              <div className="rounded-xl border border-slate-800 bg-slate-950/70 p-3 text-sm text-slate-300">
                                <p className="font-semibold text-white">Asset scope</p>
                                <p className="mt-1">{previewResult.assetScopeSummary}</p>
                                <p className="mt-1">{previewResult.availabilitySummary}</p>
                                <p className="mt-1">{previewResult.workOrderBehaviorSummary}</p>
                                <p className="mt-1">{previewResult.complianceSummary}</p>
                                <p className="mt-1">{previewResult.approvalSummary}</p>
                              </div>
                              <div className="space-y-3">
                                {previewResult.items.map((item) => (
                                  <div key={item.itemRef} className="rounded-xl border border-slate-800 bg-slate-950/70 p-3">
                                    <div className="flex items-start justify-between gap-3">
                                      <div>
                                        <p className="font-semibold text-white">{item.itemDescriptionSnapshot || item.itemRef}</p>
                                        <p className="text-xs text-[var(--color-text-muted)]">{item.itemRef}</p>
                                      </div>
                                      <DetailBadge label={item.availabilityStatus} tone={item.availabilityStatus === 'available' ? 'good' : item.availabilityStatus === 'limited' ? 'warn' : item.availabilityStatus === 'unavailable' ? 'bad' : 'neutral'} />
                                    </div>
                                    <p className="mt-2 text-sm text-slate-300">
                                      {item.calculatedQuantity} {item.unitOfMeasure} requested, based on {item.baseQuantity} base.
                                    </p>
                                    <p className="mt-1 text-xs text-[var(--color-text-muted)]">{item.availabilityMessage}</p>
                                  </div>
                                ))}
                              </div>
                              {previewResult.sampleAssets.length > 0 ? (
                                <div>
                                  <h4 className="text-sm font-semibold text-white">Sample compatible assets</h4>
                                  <ul className="mt-3 space-y-2">
                                    {previewResult.sampleAssets.map((asset) => (
                                      <li key={asset.assetId} className="rounded-xl border border-slate-800 bg-slate-950/70 px-3 py-2 text-sm text-slate-300">
                                        <div className="flex items-center justify-between gap-2">
                                          <span className="font-medium text-white">{asset.assetTag}</span>
                                          <span>{asset.readinessStatus}</span>
                                        </div>
                                        <p className="text-slate-400">{asset.name}</p>
                                      </li>
                                    ))}
                                  </ul>
                                </div>
                              ) : null}
                            </div>
                          ) : (
                            <p className="mt-2 text-sm text-slate-400">Run preview to see asset readiness and quantity impacts.</p>
                          )}
                        </div>
                      </div>

                      <aside className="space-y-4">
                        <div className="rounded-2xl border border-slate-800 bg-slate-950/60 p-4">
                          <h3 className="text-base font-semibold text-white">Actions</h3>
                          <div className="mt-4 space-y-2">
                            <button
                              type="button"
                              onClick={runSave}
                              disabled={saveMutation.isPending}
                              className="flex w-full items-center justify-center gap-2 rounded-xl border border-sky-500/40 bg-sky-500/15 px-4 py-3 text-sm font-medium text-sky-100 hover:bg-sky-500/25 disabled:cursor-not-allowed disabled:opacity-60"
                            >
                              {saveMutation.isPending ? <Loader2 className="h-4 w-4 animate-spin" /> : <Sparkles className="h-4 w-4" />}
                              Save draft
                            </button>
                            <button
                              type="button"
                              onClick={runValidation}
                              disabled={validateMutation.isPending}
                              className="flex w-full items-center justify-center gap-2 rounded-xl border border-slate-700 bg-slate-950 px-4 py-3 text-sm font-medium text-slate-200 hover:border-sky-600 hover:text-white disabled:cursor-not-allowed disabled:opacity-60"
                            >
                              {validateMutation.isPending ? <Loader2 className="h-4 w-4 animate-spin" /> : <BadgeCheck className="h-4 w-4" />}
                              Validate
                            </button>
                            <button
                              type="button"
                              onClick={runPreview}
                              disabled={previewMutation.isPending}
                              className="flex w-full items-center justify-center gap-2 rounded-xl border border-slate-700 bg-slate-950 px-4 py-3 text-sm font-medium text-slate-200 hover:border-sky-600 hover:text-white disabled:cursor-not-allowed disabled:opacity-60"
                            >
                              {previewMutation.isPending ? <Loader2 className="h-4 w-4 animate-spin" /> : <Wrench className="h-4 w-4" />}
                              Preview
                            </button>
                            <button
                              type="button"
                              onClick={() => submitMutation.mutate()}
                              disabled={!draftId || submitMutation.isPending}
                              className="flex w-full items-center justify-center gap-2 rounded-xl border border-amber-500/40 bg-amber-500/15 px-4 py-3 text-sm font-medium text-amber-100 hover:bg-amber-500/25 disabled:cursor-not-allowed disabled:opacity-60"
                            >
                              {submitMutation.isPending ? <Loader2 className="h-4 w-4 animate-spin" /> : <BadgeCheck className="h-4 w-4" />}
                              Submit for approval
                            </button>
                            <button
                              type="button"
                              onClick={() => activateMutation.mutate()}
                              disabled={!draftId || activateMutation.isPending || !canActivate}
                              className="flex w-full items-center justify-center gap-2 rounded-xl border border-emerald-500/40 bg-emerald-500/15 px-4 py-3 text-sm font-medium text-emerald-100 hover:bg-emerald-500/25 disabled:cursor-not-allowed disabled:opacity-60"
                            >
                              {activateMutation.isPending ? <Loader2 className="h-4 w-4 animate-spin" /> : <CheckCircle2 className="h-4 w-4" />}
                              Activate
                            </button>
                            <p className="pt-2 text-xs text-[var(--color-text-muted)]">
                              Save and validate before activation. If approval is required, submit the draft first.
                            </p>
                          </div>
                        </div>
                        <div className="rounded-2xl border border-slate-800 bg-slate-950/60 p-4 text-sm text-slate-300">
                          <p className="font-semibold text-white">Draft status</p>
                          <p className="mt-2">Draft ID: {draftId ?? 'Not saved yet'}</p>
                          <p>Version: {draftVersion}</p>
                          <p>Compatible with approval: {validationResult?.canApprove ? 'Yes' : 'Not yet'}</p>
                          <p>Can retire: {canRetire ? 'Yes' : 'No'}</p>
                        </div>
                      </aside>
                    </div>
                  ) : null}
                </SectionPanel>
              )
            })}
          </div>

          <aside className="space-y-4 xl:sticky xl:top-6 xl:self-start">
            <div className="rounded-[1.75rem] border border-slate-800 bg-slate-950/80 p-5 shadow-2xl">
              <div className="flex items-start justify-between gap-4">
                <div>
                  <p className="text-xs uppercase tracking-wide text-[var(--color-text-muted)]">Draft summary</p>
                  <h2 className="mt-1 text-lg font-semibold text-white">{title || 'Untitled parts kit'}</h2>
                </div>
                <DetailBadge label={draftStatus.replace(/_/g, ' ')} tone={toneForStatus(draftStatus)} />
              </div>

              <div className="mt-4 grid gap-3 text-sm text-slate-300">
                <div className="rounded-2xl border border-slate-800 bg-slate-950/60 px-3 py-2">
                  <p className="text-xs uppercase tracking-wide text-[var(--color-text-muted)]">Kit number</p>
                  <p className="mt-1 font-mono text-slate-100">{kitNumberOverride.trim() || generatedKitNumber || 'Not generated yet'}</p>
                </div>
                <div className="rounded-2xl border border-slate-800 bg-slate-950/60 px-3 py-2">
                  <p className="text-xs uppercase tracking-wide text-[var(--color-text-muted)]">Items</p>
                  <p className="mt-1">{summarizeSelectedItems(items)}</p>
                </div>
                <div className="rounded-2xl border border-slate-800 bg-slate-950/60 px-3 py-2">
                  <p className="text-xs uppercase tracking-wide text-[var(--color-text-muted)]">Quantity rules</p>
                  <p className="mt-1">{summarizeRules(quantityRules)}</p>
                </div>
              </div>

              <div className="mt-4 space-y-2">
                <button
                  type="button"
                  onClick={runSave}
                  disabled={saveMutation.isPending || !title.trim()}
                  className="flex w-full items-center justify-center gap-2 rounded-xl border border-sky-500/40 bg-sky-500/15 px-4 py-3 text-sm font-medium text-sky-100 hover:bg-sky-500/25 disabled:cursor-not-allowed disabled:opacity-60"
                >
                  {saveMutation.isPending ? <Loader2 className="h-4 w-4 animate-spin" /> : <Sparkles className="h-4 w-4" />}
                  Save draft
                </button>
                <button
                  type="button"
                  onClick={runValidation}
                  disabled={!canPreview || validateMutation.isPending}
                  className="flex w-full items-center justify-center gap-2 rounded-xl border border-slate-700 bg-slate-950 px-4 py-3 text-sm font-medium text-slate-200 hover:border-sky-600 hover:text-white disabled:cursor-not-allowed disabled:opacity-60"
                >
                  {validateMutation.isPending ? <Loader2 className="h-4 w-4 animate-spin" /> : <BadgeCheck className="h-4 w-4" />}
                  Validate
                </button>
                <button
                  type="button"
                  onClick={runPreview}
                  disabled={!canPreview || previewMutation.isPending}
                  className="flex w-full items-center justify-center gap-2 rounded-xl border border-slate-700 bg-slate-950 px-4 py-3 text-sm font-medium text-slate-200 hover:border-sky-600 hover:text-white disabled:cursor-not-allowed disabled:opacity-60"
                >
                  {previewMutation.isPending ? <Loader2 className="h-4 w-4 animate-spin" /> : <Wrench className="h-4 w-4" />}
                  Preview
                </button>
                <button
                  type="button"
                  onClick={() => submitMutation.mutate()}
                  disabled={!draftId || submitMutation.isPending}
                  className="flex w-full items-center justify-center gap-2 rounded-xl border border-amber-500/40 bg-amber-500/15 px-4 py-3 text-sm font-medium text-amber-100 hover:bg-amber-500/25 disabled:cursor-not-allowed disabled:opacity-60"
                >
                  {submitMutation.isPending ? <Loader2 className="h-4 w-4 animate-spin" /> : <BadgeCheck className="h-4 w-4" />}
                  Submit for approval
                </button>
                <button
                  type="button"
                  onClick={() => activateMutation.mutate()}
                  disabled={!draftId || activateMutation.isPending || !canActivate}
                  className="flex w-full items-center justify-center gap-2 rounded-xl border border-emerald-500/40 bg-emerald-500/15 px-4 py-3 text-sm font-medium text-emerald-100 hover:bg-emerald-500/25 disabled:cursor-not-allowed disabled:opacity-60"
                >
                  {activateMutation.isPending ? <Loader2 className="h-4 w-4 animate-spin" /> : <CheckCircle2 className="h-4 w-4" />}
                  Activate
                </button>
              </div>
            </div>

            <div className="rounded-[1.75rem] border border-slate-800 bg-slate-950/80 p-5 shadow-2xl">
              <p className="text-xs uppercase tracking-wide text-[var(--color-text-muted)]">Progress</p>
              <ul className="mt-4 space-y-2">
                {SECTION_DEFINITIONS.map((section, index) => (
                  <li key={section.key} className="flex items-center justify-between gap-3 rounded-2xl border border-slate-800 bg-slate-950/60 px-3 py-2 text-sm">
                    <span className="text-slate-200">{index + 1}. {section.label}</span>
                    <DetailBadge label={index <= expandedSectionIndex ? 'Open' : 'Locked'} tone={index <= expandedSectionIndex ? 'good' : 'neutral'} />
                  </li>
                ))}
              </ul>
            </div>

            {validationResult ? (
              <div className="rounded-[1.75rem] border border-slate-800 bg-slate-950/80 p-5 shadow-2xl">
                <p className="text-xs uppercase tracking-wide text-[var(--color-text-muted)]">Latest validation</p>
                <p className="mt-2 text-sm text-slate-300">{validationResult.summary}</p>
                <div className="mt-4 grid grid-cols-2 gap-3 text-sm text-slate-300">
                  <div className="rounded-2xl border border-slate-800 bg-slate-950/60 px-3 py-2">
                    <p className="text-xs uppercase tracking-wide text-[var(--color-text-muted)]">Errors</p>
                    <p className="mt-1 text-lg font-semibold text-white">{validationResult.errors.length}</p>
                  </div>
                  <div className="rounded-2xl border border-slate-800 bg-slate-950/60 px-3 py-2">
                    <p className="text-xs uppercase tracking-wide text-[var(--color-text-muted)]">Warnings</p>
                    <p className="mt-1 text-lg font-semibold text-white">{validationResult.warnings.length}</p>
                  </div>
                </div>
              </div>
            ) : null}
          </aside>
        </div>
      </div>
    </div>
  )
}
