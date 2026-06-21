import { useEffect, useMemo, useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Plus, Trash2, X } from 'lucide-react'
import { useLocation, useNavigate } from 'react-router-dom'
import {
  cloneEmploymentApplicationTemplate,
  createEmploymentApplicationTemplate,
  getEmploymentApplicationBuilderCatalog,
  listEmploymentApplicationSubmissions,
  listEmploymentApplicationTemplates,
  publishEmploymentApplicationTemplate,
  updateEmploymentApplicationTemplate,
} from '../../api/client'
import type {
  EmploymentApplicationFieldRequest,
  EmploymentApplicationBuilderCatalogResponse,
  EmploymentApplicationTemplateCreateRequest,
  EmploymentApplicationTemplateResponse,
  EmploymentApplicationTemplateUpsertRequest,
} from '../../api/types'
import { useStaffArrWorkspaceState } from '../../workspace/useStaffArrWorkspaceState'

const publicSiteBase = import.meta.env.VITE_PUBLIC_SITE_BASE ?? 'http://localhost:5173'

const CONTROL_OPTIONS: EmploymentApplicationFieldRequest['control'][] = [
  'text',
  'email',
  'phone',
  'textarea',
  'date',
  'select',
  'multi_select',
  'number',
  'yes_no',
]

const MAPPING_OPTIONS: EmploymentApplicationFieldRequest['mappingMode'][] = ['create', 'eventual', 'unmapped']

const DEFAULT_TARGET_FIELD_GROUPS: EmploymentApplicationBuilderCatalogResponse['targetFieldGroups'] = [
  {
    key: 'identity',
    label: 'Identity',
    fields: [
      { value: 'legal_first_name', label: 'Legal first name', stage: 'create', hint: null, owner: 'staffarr', sourceOfTruth: 'staffarr.employment_application_builder' },
      { value: 'legal_middle_name', label: 'Legal middle name', stage: 'create', hint: null, owner: 'staffarr', sourceOfTruth: 'staffarr.employment_application_builder' },
      { value: 'legal_last_name', label: 'Legal last name', stage: 'create', hint: null, owner: 'staffarr', sourceOfTruth: 'staffarr.employment_application_builder' },
      { value: 'preferred_name', label: 'Preferred name', stage: 'eventual', hint: null, owner: 'staffarr', sourceOfTruth: 'staffarr.person.field_catalog' },
      { value: 'pronouns', label: 'Pronouns', stage: 'eventual', hint: null, owner: 'staffarr', sourceOfTruth: 'staffarr.person.field_catalog' },
      { value: 'given_name', label: 'Given name', stage: 'create', hint: null, owner: 'staffarr', sourceOfTruth: 'staffarr.employment_application_builder' },
      { value: 'family_name', label: 'Family name', stage: 'create', hint: null, owner: 'staffarr', sourceOfTruth: 'staffarr.employment_application_builder' },
    ],
  },
  {
    key: 'contact',
    label: 'Contact',
    fields: [
      { value: 'primary_email', label: 'Primary email', stage: 'create', hint: null, owner: 'staffarr', sourceOfTruth: 'staffarr.employment_application_builder' },
      { value: 'alternate_email', label: 'Alternate email', stage: 'create', hint: null, owner: 'staffarr', sourceOfTruth: 'staffarr.employment_application_builder' },
      { value: 'primary_phone', label: 'Primary phone', stage: 'create', hint: null, owner: 'staffarr', sourceOfTruth: 'staffarr.employment_application_builder' },
      { value: 'alternate_phone', label: 'Alternate phone', stage: 'create', hint: null, owner: 'staffarr', sourceOfTruth: 'staffarr.employment_application_builder' },
      { value: 'work_phone', label: 'Work phone', stage: 'create', hint: null, owner: 'staffarr', sourceOfTruth: 'staffarr.employment_application_builder' },
      { value: 'can_login', label: 'Allow login', stage: 'create', hint: null, owner: 'staffarr', sourceOfTruth: 'staffarr.employment_application_builder' },
    ],
  },
  {
    key: 'employment',
    label: 'Employment',
    fields: [
      { value: 'work_relationship_type', label: 'Work relationship', stage: 'create', hint: null, owner: 'staffarr', sourceOfTruth: 'staffarr.employment_application_builder' },
      { value: 'employment_type', label: 'Employment type', stage: 'create', hint: null, owner: 'staffarr', sourceOfTruth: 'staffarr.employment_application_builder' },
      { value: 'worker_category', label: 'Worker category', stage: 'eventual', hint: null, owner: 'staffarr', sourceOfTruth: 'staffarr.person.field_catalog' },
      { value: 'flsa_status', label: 'FLSA status', stage: 'eventual', hint: null, owner: 'staffarr', sourceOfTruth: 'staffarr.person.field_catalog' },
      { value: 'position_number', label: 'Position number', stage: 'eventual', hint: null, owner: 'staffarr', sourceOfTruth: 'staffarr.person.field_catalog' },
      { value: 'current_employment_action', label: 'Current employment action', stage: 'eventual', hint: null, owner: 'staffarr', sourceOfTruth: 'staffarr.person.field_catalog' },
      { value: 'current_employment_action_at', label: 'Current employment action at', stage: 'eventual', hint: null, owner: 'staffarr', sourceOfTruth: 'staffarr.person.field_catalog' },
      { value: 'leave_status', label: 'Leave status', stage: 'eventual', hint: null, owner: 'staffarr', sourceOfTruth: 'staffarr.person.field_catalog' },
      { value: 'eligible_for_rehire', label: 'Eligible for rehire', stage: 'create', hint: null, owner: 'staffarr', sourceOfTruth: 'staffarr.employment_application_builder' },
      { value: 'job_title', label: 'Job title', stage: 'create', hint: null, owner: 'staffarr', sourceOfTruth: 'staffarr.employment_application_builder' },
      { value: 'start_date', label: 'Start date', stage: 'create', hint: null, owner: 'staffarr', sourceOfTruth: 'staffarr.employment_application_builder' },
      { value: 'expected_start_date', label: 'Expected start date', stage: 'create', hint: null, owner: 'staffarr', sourceOfTruth: 'staffarr.employment_application_builder' },
    ],
  },
  {
    key: 'placement',
    label: 'Placement',
    fields: [
      { value: 'primary_org_unit_id', label: 'Primary org unit', stage: 'eventual', hint: null, owner: 'staffarr', sourceOfTruth: 'staffarr.person.field_catalog' },
      { value: 'site_org_unit_id', label: 'Site org unit', stage: 'eventual', hint: null, owner: 'staffarr', sourceOfTruth: 'staffarr.person.field_catalog' },
      { value: 'department_org_unit_id', label: 'Department org unit', stage: 'eventual', hint: null, owner: 'staffarr', sourceOfTruth: 'staffarr.person.field_catalog' },
      { value: 'team_org_unit_id', label: 'Team org unit', stage: 'eventual', hint: null, owner: 'staffarr', sourceOfTruth: 'staffarr.person.field_catalog' },
      { value: 'position_org_unit_id', label: 'Position org unit', stage: 'eventual', hint: null, owner: 'staffarr', sourceOfTruth: 'staffarr.person.field_catalog' },
      { value: 'manager_person_id', label: 'Manager person', stage: 'eventual', hint: null, owner: 'staffarr', sourceOfTruth: 'staffarr.person.field_catalog' },
      { value: 'home_base_location_id', label: 'Home base location', stage: 'eventual', hint: null, owner: 'staffarr', sourceOfTruth: 'staffarr.person.field_catalog' },
    ],
  },
]

function defaultField(): EmploymentApplicationFieldRequest {
  return {
    fieldKey: `field_${crypto.randomUUID().slice(0, 8)}`,
    label: 'New field',
    control: 'text',
    required: false,
    mappingMode: 'eventual',
    targetFieldKey: null,
    helpText: null,
    placeholder: null,
    options: [],
  }
}

function defaultTemplateName(): string {
  return 'Application builder'
}

function defaultTemplateKey(): string {
  return `employment-application-${crypto.randomUUID().slice(0, 8)}`
}

type BuilderSectionKey = 'basic_information' | 'eligibility' | 'experience' | 'availability' | 'acknowledgements'

type BuilderSection = {
  key: BuilderSectionKey
  title: string
  subtitle: string
  fieldKeys: string[]
}

const BUILDER_SECTIONS: BuilderSection[] = [
  {
    key: 'basic_information',
    title: 'Basic information',
    subtitle: 'Identity and contact details.',
    fieldKeys: [
      'legal_first_name',
      'legal_middle_name',
      'legal_last_name',
      'preferred_name',
      'pronouns',
      'primary_email',
      'alternate_email',
      'primary_phone',
      'alternate_phone',
      'work_phone',
      'given_name',
      'family_name',
    ],
  },
  {
    key: 'eligibility',
    title: 'Eligibility',
    subtitle: 'Relationship and screening fields.',
    fieldKeys: [
      'work_relationship_type',
      'employment_type',
      'worker_category',
      'flsa_status',
      'eligible_for_rehire',
      'can_login',
    ],
  },
  {
    key: 'experience',
    title: 'Experience',
    subtitle: 'Role, history, and manager context.',
    fieldKeys: [
      'job_title',
      'position_number',
      'current_employment_action',
      'current_employment_action_at',
      'manager_person_id',
      'primary_org_unit_id',
      'department_org_unit_id',
      'team_org_unit_id',
      'position_org_unit_id',
      'home_base_location_id',
    ],
  },
  {
    key: 'availability',
    title: 'Availability',
    subtitle: 'Start timing and scheduling context.',
    fieldKeys: ['start_date', 'expected_start_date', 'leave_status'],
  },
  {
    key: 'acknowledgements',
    title: 'Acknowledgements',
    subtitle: 'Freeform notes and consents.',
    fieldKeys: ['application_notes', 'notes', 'agreement', 'signature'],
  },
]

function sectionForFieldKey(fieldKey: string): BuilderSectionKey {
  for (const section of BUILDER_SECTIONS) {
    if (section.fieldKeys.includes(fieldKey)) {
      return section.key
    }
  }

  return 'acknowledgements'
}

function defaultTemplateRequest(): EmploymentApplicationTemplateCreateRequest {
  return {
    templateKey: defaultTemplateKey(),
    templateName: defaultTemplateName(),
    title: 'Applicant intake',
    subtitle: 'Tell us a little about yourself so we can build your applicant profile in StaffArr.',
    submitLabel: 'Submit application',
    publicLinkExpiresAt: new Date(Date.now() + 90 * 24 * 60 * 60 * 1000).toISOString(),
    fields: [
      {
        fieldKey: 'legal_first_name',
        label: 'Legal first name',
        control: 'text',
        required: true,
        mappingMode: 'create',
        targetFieldKey: 'legal_first_name',
        helpText: 'Matches the person record.',
        placeholder: 'First name',
        options: [],
      },
      {
        fieldKey: 'legal_last_name',
        label: 'Legal last name',
        control: 'text',
        required: true,
        mappingMode: 'create',
        targetFieldKey: 'legal_last_name',
        helpText: 'Matches the person record.',
        placeholder: 'Last name',
        options: [],
      },
      {
        fieldKey: 'primary_email',
        label: 'Email',
        control: 'email',
        required: true,
        mappingMode: 'create',
        targetFieldKey: 'primary_email',
        helpText: 'This is the login/contact email.',
        placeholder: 'name@example.com',
        options: [],
      },
      {
        fieldKey: 'primary_phone',
        label: 'Phone',
        control: 'phone',
        required: false,
        mappingMode: 'create',
        targetFieldKey: 'primary_phone',
        helpText: null,
        placeholder: '(555) 123-4567',
        options: [],
      },
      {
        fieldKey: 'preferred_name',
        label: 'Preferred name',
        control: 'text',
        required: false,
        mappingMode: 'eventual',
        targetFieldKey: 'preferred_name',
        helpText: 'Queued for profile review after submission.',
        placeholder: 'What should we call you?',
        options: [],
      },
    ],
  }
}

function draftFromTemplate(template: EmploymentApplicationTemplateResponse): EmploymentApplicationTemplateUpsertRequest {
  return {
    templateName: template.templateName,
    title: template.title,
    subtitle: template.subtitle,
    submitLabel: template.submitLabel,
    publicLinkExpiresAt: template.publicLinkExpiresAt,
    fields: template.fields.map((field) => ({
      ...field,
      options: [...field.options],
    })),
  }
}

function sameJson(left: unknown, right: unknown): boolean {
  return JSON.stringify(left) === JSON.stringify(right)
}

function formatPublicUrl(token: string): string {
  return `${publicSiteBase.replace(/\/+$/, '')}/apply/${token}`
}

function describeTargetField(
  targetFieldKey: string | null,
  targetFieldGroups: EmploymentApplicationBuilderCatalogResponse['targetFieldGroups'],
): string {
  if (!targetFieldKey) {
    return 'none'
  }

  for (const group of targetFieldGroups) {
    const field = group.fields.find((entry) => entry.value === targetFieldKey)
    if (field) {
      return `${group.label} · ${field.label} (${field.stage})`
    }
  }

  return targetFieldKey
}

function describeControl(
  control: EmploymentApplicationFieldRequest['control'],
  controlOptions: EmploymentApplicationBuilderCatalogResponse['controlOptions'],
): string {
  return controlOptions.find((option) => option.value === control)?.label ?? control
}

function sectionLabel(sectionKey: BuilderSectionKey): string {
  return BUILDER_SECTIONS.find((section) => section.key === sectionKey)?.title ?? 'Acknowledgements'
}

export function EmploymentApplicationsPage({
  state,
}: {
  state?: ReturnType<typeof useStaffArrWorkspaceState>
} = {}) {
  return <EmploymentApplicationsPageContent state={state} />
}

function EmploymentApplicationsPageContent({
  state: providedState,
}: {
  state?: ReturnType<typeof useStaffArrWorkspaceState>
}) {
  const hookState = useStaffArrWorkspaceState()
  const state = providedState ?? hookState
  const queryClient = useQueryClient()
  const location = useLocation()
  const navigate = useNavigate()
  const [selectedTemplateId, setSelectedTemplateId] = useState<string | null>(null)
  const [selectedSectionKey, setSelectedSectionKey] = useState<BuilderSectionKey>('basic_information')
  const [selectedFieldKey, setSelectedFieldKey] = useState<string | null>(null)
  const [draft, setDraft] = useState<EmploymentApplicationTemplateUpsertRequest | null>(null)
  const [savedDraft, setSavedDraft] = useState<EmploymentApplicationTemplateUpsertRequest | null>(null)
  const [newTemplateName, setNewTemplateName] = useState(defaultTemplateName())
  const [newTemplateKey, setNewTemplateKey] = useState(defaultTemplateKey())
  const [localError, setLocalError] = useState<string | null>(null)
  const [successMessage, setSuccessMessage] = useState<string | null>(null)
  const isCreateDrawerOpen = location.pathname.startsWith('/applications/create')

  const templatesQuery = useQuery({
    queryKey: ['staffarr-employment-application-templates', state.accessToken],
    queryFn: () => listEmploymentApplicationTemplates(state.accessToken),
    enabled: Boolean(state.ready && state.accessToken),
  })

  const builderCatalogQuery = useQuery({
    queryKey: ['staffarr-employment-application-builder-catalog', state.accessToken],
    queryFn: () => getEmploymentApplicationBuilderCatalog(state.accessToken),
    enabled: Boolean(state.ready && state.accessToken),
  })

  const submissionsQuery = useQuery({
    queryKey: ['staffarr-employment-application-submissions', state.accessToken],
    queryFn: () => listEmploymentApplicationSubmissions(state.accessToken, 20),
    enabled: Boolean(state.ready && state.accessToken),
  })

  const selectedTemplate = useMemo(
    () => templatesQuery.data?.find((template) => template.employmentApplicationTemplateId === selectedTemplateId) ?? null,
    [selectedTemplateId, templatesQuery.data],
  )
  const builderCatalog = builderCatalogQuery.data
  const controlOptions = builderCatalog?.controlOptions ?? CONTROL_OPTIONS.map((value) => ({
    value,
    label:
      value === 'text'
        ? 'Short text'
        : value === 'textarea'
          ? 'Long text'
          : value === 'email'
            ? 'Email'
            : value === 'phone'
              ? 'Phone'
              : value === 'date'
                ? 'Date'
                : value === 'select'
                  ? 'Single select'
                  : value === 'multi_select'
                    ? 'Multi select'
                    : value === 'yes_no'
                      ? 'Yes / no'
                      : 'Number',
    hint: null,
  }))
  const targetFieldGroups = builderCatalog?.targetFieldGroups ?? DEFAULT_TARGET_FIELD_GROUPS
  const sectionGroups = useMemo(() => {
    const fields = draft?.fields ?? []
    return BUILDER_SECTIONS.map((section) => ({
      ...section,
      fields: fields.filter((field) => section.fieldKeys.includes(field.fieldKey) || sectionForFieldKey(field.fieldKey) === section.key),
    }))
  }, [draft?.fields])
  const selectedSection = sectionGroups.find((section) => section.key === selectedSectionKey) ?? sectionGroups[0]
  const selectedField = useMemo(() => {
    const allFields = draft?.fields ?? []
    return (
      allFields.find((field) => field.fieldKey === selectedFieldKey) ??
      selectedSection?.fields[0] ??
      allFields[0] ??
      null
    )
  }, [draft?.fields, selectedFieldKey, selectedSection])

  useEffect(() => {
    const firstTemplateId = templatesQuery.data?.[0]?.employmentApplicationTemplateId ?? null
    if (!selectedTemplateId && firstTemplateId) {
      setSelectedTemplateId(firstTemplateId)
    }
    if (selectedTemplateId && templatesQuery.data && !templatesQuery.data.some((template) => template.employmentApplicationTemplateId === selectedTemplateId)) {
      setSelectedTemplateId(firstTemplateId)
    }
  }, [selectedTemplateId, templatesQuery.data])

  useEffect(() => {
    if (!selectedTemplate) {
      return
    }

    const nextDraft = draftFromTemplate(selectedTemplate)
    setDraft(nextDraft)
    setSavedDraft(draftFromTemplate(selectedTemplate))
  }, [selectedTemplate])

  useEffect(() => {
    const firstSection = sectionGroups.find((section) => section.fields.length > 0) ?? sectionGroups[0]
    if (firstSection && !sectionGroups.some((section) => section.key === selectedSectionKey && section.fields.length > 0)) {
      setSelectedSectionKey(firstSection.key)
    }
    if (firstSection && !selectedFieldKey) {
      setSelectedFieldKey(firstSection.fields[0]?.fieldKey ?? null)
    }
  }, [sectionGroups, selectedFieldKey, selectedSectionKey])

  useEffect(() => {
    if (!selectedSection?.fields.some((field) => field.fieldKey === selectedFieldKey)) {
      setSelectedFieldKey(selectedSection?.fields[0]?.fieldKey ?? null)
    }
  }, [selectedFieldKey, selectedSection])

  const refreshTemplates = async () => {
    await queryClient.invalidateQueries({ queryKey: ['staffarr-employment-application-templates', state.accessToken] })
  }

  const upsertTemplateCache = (template: EmploymentApplicationTemplateResponse) => {
    queryClient.setQueryData(
      ['staffarr-employment-application-templates', state.accessToken],
      (current: EmploymentApplicationTemplateResponse[] | undefined) => {
        const existing = current ?? []
        return [template, ...existing.filter((entry) => entry.employmentApplicationTemplateId !== template.employmentApplicationTemplateId)]
      },
    )
  }

  const createMutation = useMutation({
    mutationFn: (request: EmploymentApplicationTemplateCreateRequest) =>
      createEmploymentApplicationTemplate(state.accessToken, request),
    onSuccess: async (response) => {
      upsertTemplateCache(response)
      await refreshTemplates()
      setSelectedTemplateId(response.employmentApplicationTemplateId)
      navigate('/applications/drawer', { replace: true })
      setLocalError(null)
      setSuccessMessage('Application template created.')
    },
  })

  const saveMutation = useMutation({
    mutationFn: async (request: EmploymentApplicationTemplateUpsertRequest) => {
      if (!selectedTemplate) {
        throw new Error('No template selected.')
      }
      return updateEmploymentApplicationTemplate(state.accessToken, selectedTemplate.employmentApplicationTemplateId, request)
    },
    onSuccess: async (response) => {
      upsertTemplateCache(response)
      await refreshTemplates()
      setLocalError(null)
      setSuccessMessage('Application template saved.')
    },
  })

  const publishMutation = useMutation({
    mutationFn: async () => {
      if (!selectedTemplate) {
        throw new Error('No template selected.')
      }
      return publishEmploymentApplicationTemplate(state.accessToken, selectedTemplate.employmentApplicationTemplateId)
    },
    onSuccess: async (response) => {
      upsertTemplateCache(response)
      await refreshTemplates()
      setSelectedTemplateId(response.employmentApplicationTemplateId)
      setLocalError(null)
      setSuccessMessage('Template published.')
    },
  })

  const cloneMutation = useMutation({
    mutationFn: async () => {
      if (!selectedTemplate) {
        throw new Error('No template selected.')
      }
      return cloneEmploymentApplicationTemplate(state.accessToken, selectedTemplate.employmentApplicationTemplateId)
    },
    onSuccess: async (response) => {
      upsertTemplateCache(response)
      await refreshTemplates()
      setSelectedTemplateId(response.employmentApplicationTemplateId)
      setLocalError(null)
      setSuccessMessage('New version cloned.')
    },
  })

  const dirty = useMemo(() => !sameJson(draft, savedDraft), [draft, savedDraft])
  const publicUrl = selectedTemplate?.status === 'published' ? formatPublicUrl(selectedTemplate.publicToken) : ''

  if (state.handoffRedirect) return state.handoffRedirect
  if (!state.ready) return <p className="text-sm text-slate-400">{state.loadingMessage}</p>

  const updateField = (fieldKey: string, patch: Partial<EmploymentApplicationFieldRequest>) => {
    setDraft((current) =>
      current
        ? {
            ...current,
            fields: current.fields.map((field) => (field.fieldKey === fieldKey ? { ...field, ...patch } : field)),
          }
        : current,
    )
    setLocalError(null)
    setSuccessMessage(null)
  }

  const addField = () => {
    setDraft((current) =>
      current
        ? {
            ...current,
            fields: [...current.fields, defaultField()],
          }
        : current,
    )
  }

  const removeField = (fieldKey: string) => {
    setDraft((current) =>
      current
        ? {
            ...current,
            fields: current.fields.filter((field) => field.fieldKey !== fieldKey),
          }
        : current,
    )
  }

  const handleCreate = () => {
    createMutation.mutate({
      ...defaultTemplateRequest(),
      templateName: newTemplateName.trim() || defaultTemplateName(),
      templateKey: newTemplateKey.trim() || defaultTemplateKey(),
    })
  }

  const openCreateTemplateDrawer = () => {
    setNewTemplateName(defaultTemplateName())
    setNewTemplateKey(defaultTemplateKey())
    navigate('/applications/create')
  }

  const closeCreateTemplateDrawer = () => {
    navigate('/applications/drawer')
  }

  const handleSave = () => {
    if (!draft || !selectedTemplate) return
    if (!draft.title.trim()) {
      setLocalError('A title is required.')
      return
    }
    if (!draft.fields.length) {
      setLocalError('Add at least one field before saving.')
      return
    }
    saveMutation.mutate(draft)
  }

  return (
    <section className="min-h-screen bg-[var(--color-bg-app)] text-slate-100">
      <div className="mx-auto max-w-[1720px] px-4 py-4 sm:px-6 lg:px-8">
        <div className="space-y-4">
          <aside className="rounded-[28px] border border-slate-800/80 bg-slate-950/70 p-4 shadow-2xl shadow-cyan-950/10 backdrop-blur">
            <div className="grid gap-4 xl:grid-cols-[minmax(220px,0.7fr)_minmax(260px,0.9fr)_minmax(0,2.2fr)]">
              <div className="flex items-start justify-between gap-3">
                <div>
                  <div className="text-xs font-semibold uppercase tracking-[0.28em] text-cyan-300">StaffArr</div>
                  <h1 className="mt-2 text-2xl font-medium tracking-tight text-white">Application Builder</h1>
                </div>
                <div className="flex shrink-0 items-center gap-2">
                  <span className="rounded-full border border-amber-400/60 bg-amber-500/10 px-3 py-1 text-xs font-semibold text-amber-200">
                    Draft
                  </span>
                  <button
                    type="button"
                    onClick={isCreateDrawerOpen ? closeCreateTemplateDrawer : openCreateTemplateDrawer}
                    className="grid h-9 w-9 place-items-center rounded-full border border-slate-700 text-slate-100 transition hover:border-cyan-400 hover:text-cyan-100"
                    aria-label={isCreateDrawerOpen ? 'Close template creation' : 'Create template'}
                    title={isCreateDrawerOpen ? 'Close template creation' : 'Create template'}
                  >
                    {isCreateDrawerOpen ? <X className="h-4 w-4" /> : <Plus className="h-4 w-4" />}
                  </button>
                </div>
              </div>

              <div className="rounded-2xl border border-slate-800 bg-slate-900/45 p-4">
                <p className="text-xs uppercase tracking-wide text-[var(--color-text-muted)]">Application template</p>
                <div className="mt-2 text-lg font-semibold text-white">{selectedTemplate?.templateName ?? 'Application template'}</div>
                <p className="mt-2 text-sm text-slate-400">
                  Maps into StaffArr person records during hire conversion, with eventual profile values staged for review.
                </p>
              </div>

              <div className="grid gap-2 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-5">
                {sectionGroups.map((section, index) => {
                  const isActive = section.key === selectedSectionKey
                  return (
                    <button
                      key={section.key}
                      type="button"
                      onClick={() => setSelectedSectionKey(section.key)}
                      className={`flex min-h-20 w-full items-center gap-3 rounded-2xl border px-3 py-3 text-left transition ${
                        isActive
                          ? 'border-[var(--color-accent-border)] bg-[var(--color-accent-soft)] shadow-sm'
                          : 'border-slate-800 bg-slate-900/45 hover:border-slate-600'
                      }`}
                    >
                      <div className={`grid h-8 w-8 shrink-0 place-items-center rounded-full text-sm font-semibold ${isActive ? 'bg-[var(--color-accent)] text-white' : 'bg-slate-800 text-slate-200'}`}>
                        {index + 1}
                      </div>
                      <div className="min-w-0">
                        <div className="truncate font-semibold text-white">{section.title}</div>
                        <div className="text-xs text-slate-400">{section.fields.length} fields</div>
                      </div>
                    </button>
                  )
                })}
              </div>
            </div>

            <div className="mt-4">
              <div className="rounded-2xl border border-slate-800 bg-slate-900/45 p-4">
                <p className="text-xs uppercase tracking-wide text-[var(--color-text-muted)]">Recent submissions</p>
                <div className="mt-3 grid gap-2 sm:grid-cols-2 xl:grid-cols-4">
                  {(submissionsQuery.data ?? []).slice(0, 4).map((submission) => (
                    <div key={submission.employmentApplicationSubmissionId} className="rounded-xl border border-slate-800 bg-slate-950/70 p-3 text-sm">
                      <div className="flex items-center justify-between gap-3">
                        <div className="min-w-0 truncate text-white">{submission.applicantDisplayName || submission.applicantEmail}</div>
                        <span className="text-xs text-[var(--color-text-muted)]">{submission.status}</span>
                      </div>
                      <p className="mt-2 text-xs text-[var(--color-text-muted)]">
                        {submission.recruitingRequisitionId ? 'Requisition linked on submission' : 'No requisition linked yet'}
                      </p>
                    </div>
                  ))}
                  {(submissionsQuery.data ?? []).length === 0 ? (
                    <p className="text-sm text-[var(--color-text-muted)]">No submissions yet.</p>
                  ) : null}
                </div>
              </div>
            </div>
          </aside>

          <main className="space-y-4">
            <div className="flex flex-wrap items-start justify-between gap-4">
              <div>
                <div className="text-xs font-semibold uppercase tracking-[0.28em] text-cyan-300">Builder workspace</div>
                <h2 className="mt-2 text-3xl font-medium tracking-tight text-white">{selectedSection?.title ?? 'Basic information'}</h2>
                <p className="mt-3 text-sm text-slate-400">{selectedSection?.subtitle ?? 'Applicant identity and contact details.'}</p>
              </div>

              <div className="flex flex-wrap gap-2">
                <button
                  type="button"
                  onClick={handleSave}
                  disabled={saveMutation.isPending || !dirty || !selectedTemplate || selectedTemplate.status !== 'draft'}
                  className="rounded-xl border border-slate-700 bg-slate-950/70 px-4 py-3 text-sm font-medium text-white hover:border-cyan-400 disabled:opacity-50"
                >
                  {saveMutation.isPending ? 'Saving...' : 'Save draft'}
                </button>
                <button
                  type="button"
                  onClick={() => {
                    if (publicUrl) {
                      window.open(publicUrl, '_blank', 'noopener,noreferrer')
                    }
                  }}
                  disabled={!publicUrl}
                  className="rounded-xl border border-slate-700 bg-slate-950/70 px-4 py-3 text-sm font-medium text-white hover:border-cyan-400 disabled:opacity-50"
                >
                  Preview
                </button>
                <button
                  type="button"
                  onClick={() => publishMutation.mutate()}
                  disabled={publishMutation.isPending || !draft?.fields.length}
                  className="rounded-xl bg-blue-500 px-4 py-3 text-sm font-semibold text-white hover:bg-blue-400 disabled:opacity-50"
                >
                  {publishMutation.isPending ? 'Publishing...' : 'Publish'}
                </button>
              </div>
            </div>

            {localError ? (
              <p className="rounded-2xl border border-rose-900/60 bg-rose-950/40 px-4 py-3 text-sm text-rose-100">
                {localError}
              </p>
            ) : null}

            {successMessage ? (
              <p className="rounded-2xl border border-emerald-900/40 bg-emerald-950/30 px-4 py-3 text-sm text-emerald-100">
                {successMessage}
              </p>
            ) : null}

            {templatesQuery.isLoading ? <p className="text-sm text-slate-400">Loading application templates...</p> : null}

            {selectedTemplate && draft ? (
              <div className="grid gap-4 xl:grid-cols-[1.1fr_0.92fr_1fr]">
                <section className="rounded-[28px] border border-slate-800/80 bg-slate-950/70 p-5 backdrop-blur">
                  <div className="flex items-start justify-between gap-4">
                    <div>
                      <div className="text-lg font-semibold text-white">Application fields</div>
                      <p className="mt-1 text-sm text-slate-400">
                        Select a field to configure question behavior and StaffArr mapping.
                      </p>
                    </div>
                    <button
                      type="button"
                      onClick={() => cloneMutation.mutate()}
                      disabled={cloneMutation.isPending}
                      className="rounded-xl border border-slate-700 px-3 py-2 text-sm text-slate-100 hover:border-cyan-400 disabled:opacity-50"
                    >
                      Reorder
                    </button>
                  </div>

                  <div className="mt-5 space-y-3">
                    {selectedSection?.fields.map((field) => {
                      const isActive = field.fieldKey === selectedField?.fieldKey
                      return (
                        <button
                          key={field.fieldKey}
                          type="button"
                          onClick={() => {
                            setSelectedFieldKey(field.fieldKey)
                          }}
                          className={`w-full rounded-2xl border p-4 text-left transition ${
                            isActive
                              ? 'border-[var(--color-accent-border)] bg-[var(--color-accent-soft)] shadow-sm'
                              : 'border-slate-800 bg-slate-900/55 hover:border-slate-600'
                          }`}
                        >
                          <div className="flex items-start justify-between gap-4">
                            <div>
                              <div className="text-base font-semibold text-white">{field.label}</div>
                              <div className="mt-1 text-sm text-slate-400">{describeControl(field.control, controlOptions)}</div>
                              <div className="mt-2 text-xs text-[var(--color-text-muted)]">
                                Maps to: {describeTargetField(field.targetFieldKey, targetFieldGroups)}
                              </div>
                            </div>
                            <div className="flex flex-wrap justify-end gap-2">
                              {field.required ? (
                                <span className="rounded-full border border-slate-700 bg-slate-900/60 px-3 py-1 text-xs text-slate-200">
                                  Required
                                </span>
                              ) : null}
                              {field.targetFieldKey ? (
                                <span className="rounded-full border border-cyan-500/50 bg-cyan-500/10 px-3 py-1 text-xs text-cyan-100">
                                  Mapped
                                </span>
                              ) : (
                                <span className="rounded-full border border-slate-700 bg-slate-900/60 px-3 py-1 text-xs text-slate-300">
                                  Application only
                                </span>
                              )}
                              {field.mappingMode === 'eventual' ? (
                                <span className="rounded-full border border-amber-500/50 bg-amber-500/10 px-3 py-1 text-xs text-amber-100">
                                  Review
                                </span>
                              ) : null}
                            </div>
                          </div>
                        </button>
                      )
                    })}
                  </div>

                  <div className="mt-5">
                    <div className="text-sm font-medium text-white">Add question</div>
                    <div className="mt-3 grid grid-cols-2 gap-2 sm:grid-cols-3">
                      {controlOptions.map((option) => (
                        <button
                          key={option.value}
                          type="button"
                          onClick={addField}
                          className="rounded-xl border border-slate-700 bg-slate-950/70 px-3 py-3 text-left text-sm text-white hover:border-cyan-400"
                        >
                          + {option.label}
                        </button>
                      ))}
                    </div>
                  </div>
                </section>

                <section className="rounded-[28px] border border-slate-800/80 bg-slate-950/70 p-5 backdrop-blur">
                  <div className="flex items-start justify-between gap-4">
                    <div>
                      <div className="text-lg font-semibold text-white">Field settings</div>
                      <p className="mt-1 text-sm text-slate-400">Configure applicant question and conversion mapping.</p>
                    </div>
                  </div>
                  <div className="mt-5">
                    {selectedField ? (
                      <FieldEditor
                        field={selectedField}
                        controlOptions={controlOptions}
                        targetFieldGroups={targetFieldGroups}
                        onChange={(patch) => updateField(selectedField.fieldKey, patch)}
                        onDelete={() => removeField(selectedField.fieldKey)}
                      />
                    ) : (
                      <div className="rounded-2xl border border-slate-800 bg-slate-900/60 p-6 text-sm text-slate-400">
                        No field selected.
                      </div>
                    )}
                  </div>
                </section>

                <section className="space-y-4">
                  <div className="rounded-[28px] border border-slate-800/80 bg-slate-950/70 p-5 backdrop-blur">
                    <div className="flex items-start justify-between gap-4">
                      <div>
                        <div className="text-lg font-semibold text-white">Person mapping summary</div>
                        <p className="mt-1 text-sm text-slate-400">What this application will populate during applicant conversion.</p>
                      </div>
                      <button
                        type="button"
                        className="rounded-xl border border-slate-700 px-3 py-2 text-sm text-slate-100 hover:border-cyan-400"
                      >
                        Validate
                      </button>
                    </div>

                    <div className="mt-5 rounded-2xl border border-slate-800 bg-slate-900/55 p-4">
                      <div className="text-sm font-semibold text-white">Conversion rule</div>
                      <p className="mt-2 text-sm leading-6 text-slate-300">
                        Submitted applications do not create StaffArr people automatically. Mapped values are staged and reviewed when a hiring user selects Convert applicant to person.
                      </p>
                    </div>

                    <div className="mt-4 overflow-hidden rounded-2xl border border-slate-800 bg-slate-900/55">
                      <div className="grid grid-cols-[1.1fr_1fr_90px] gap-3 border-b border-slate-800 px-4 py-3 text-xs font-semibold uppercase tracking-wide text-slate-400">
                        <div>Application field</div>
                        <div>StaffArr person field</div>
                        <div>Status</div>
                      </div>
                      <div className="divide-y divide-slate-800">
                        {draft.fields.map((field) => (
                          <div key={field.fieldKey} className="grid grid-cols-[1.1fr_1fr_90px] gap-3 px-4 py-3 text-sm">
                            <div>
                              <div className="font-semibold text-white">{field.label}</div>
                              <div className="text-xs text-[var(--color-text-muted)]">{sectionLabel(sectionForFieldKey(field.fieldKey))}</div>
                            </div>
                            <div>
                              <div className="font-semibold text-slate-200">
                                {describeTargetField(field.targetFieldKey, targetFieldGroups)}
                              </div>
                              <div className="text-xs text-[var(--color-text-muted)]">{describeControl(field.control, controlOptions)}</div>
                            </div>
                            <div className="flex items-center">
                              <span className="rounded-full border border-cyan-500/50 bg-cyan-500/10 px-3 py-1 text-xs font-semibold text-cyan-100">
                                Ready
                              </span>
                            </div>
                          </div>
                        ))}
                      </div>
                    </div>
                  </div>

                  <div className="rounded-[28px] border border-slate-800/80 bg-slate-950/70 p-5 backdrop-blur">
                    <div className="text-sm font-semibold uppercase tracking-wide text-slate-400">StaffArr person mapping</div>
                    <p className="mt-2 text-sm text-slate-400">
                      Controls what copies into the create request versus what stays in the eventual profile review queue.
                    </p>
                    <div className="mt-4 grid gap-3">
                      {targetFieldGroups.map((group) => (
                        <div key={group.key} className="rounded-2xl border border-slate-800 bg-slate-900/55 p-3">
                          <div className="text-xs font-semibold uppercase tracking-wide text-[var(--color-text-muted)]">{group.label}</div>
                          <div className="mt-2 space-y-1 text-sm text-slate-200">
                            {group.fields.slice(0, 3).map((field) => (
                              <div key={field.value} className="flex items-center justify-between gap-3">
                                <span>{field.label}</span>
                                <span className="text-xs text-[var(--color-text-muted)]">{field.stage}</span>
                              </div>
                            ))}
                          </div>
                        </div>
                      ))}
                    </div>
                  </div>
                </section>
              </div>
            ) : (
              <div className="rounded-[28px] border border-slate-800/80 bg-slate-950/70 p-6 text-sm text-slate-400">
                No template selected.
              </div>
            )}

            <div className="flex flex-wrap items-center justify-between gap-3 border-t border-slate-800/80 pt-4">
              <p className="text-xs text-[var(--color-text-muted)]">
                Fields marked <span className="font-semibold text-cyan-200">create</span> are applied immediately. Fields marked{' '}
                <span className="font-semibold text-amber-200">eventual</span> stay in the applicant profile draft for later review.
              </p>
              <div className="text-xs text-[var(--color-text-muted)]">
                {selectedTemplate?.publicLinkExpiresAt ? `Public link expires ${new Date(selectedTemplate.publicLinkExpiresAt).toLocaleString()}` : 'Publish to generate a public link.'}
              </div>
            </div>
          </main>

          {isCreateDrawerOpen ? (
            <div className="fixed inset-0 z-50 bg-slate-950/70 backdrop-blur-sm">
              <button
                type="button"
                aria-label="Close template creation"
                className="absolute inset-0 h-full w-full cursor-default"
                onClick={closeCreateTemplateDrawer}
              />
              <aside
                role="dialog"
                aria-modal="true"
                aria-labelledby="create-employment-template-title"
                className="absolute right-0 top-0 flex h-full w-full max-w-md flex-col border-l border-slate-800 bg-slate-950 shadow-2xl shadow-slate-950/60"
              >
                <div className="flex items-start justify-between gap-4 border-b border-slate-800 p-5">
                  <div>
                    <p className="text-xs font-semibold uppercase tracking-[0.28em] text-cyan-300">Template</p>
                    <h2 id="create-employment-template-title" className="mt-2 text-2xl font-medium tracking-tight text-white">
                      Create
                    </h2>
                  </div>
                  <button
                    type="button"
                    onClick={closeCreateTemplateDrawer}
                    className="grid h-9 w-9 place-items-center rounded-full border border-slate-700 text-slate-100 transition hover:border-cyan-400 hover:text-cyan-100"
                    aria-label="Close template creation"
                    title="Close template creation"
                  >
                    <X className="h-4 w-4" />
                  </button>
                </div>

                <div className="flex-1 space-y-4 overflow-y-auto p-5">
                  <label className="block text-sm font-medium text-slate-300" htmlFor="employment-template-name">
                    Template name
                    <input
                      id="employment-template-name"
                      value={newTemplateName}
                      onChange={(event) => setNewTemplateName(event.target.value)}
                      className="mt-2 w-full rounded-xl border border-slate-700 bg-slate-900 px-3 py-2 text-sm text-white"
                    />
                  </label>
                  <label className="block text-sm font-medium text-slate-300" htmlFor="employment-template-key">
                    Template key
                    <input
                      id="employment-template-key"
                      value={newTemplateKey}
                      onChange={(event) => setNewTemplateKey(event.target.value)}
                      className="mt-2 w-full rounded-xl border border-slate-700 bg-slate-900 px-3 py-2 text-sm text-white"
                    />
                  </label>
                </div>

                <div className="flex justify-end gap-2 border-t border-slate-800 p-5">
                  <button
                    type="button"
                    onClick={closeCreateTemplateDrawer}
                    className="rounded-xl border border-slate-700 px-4 py-3 text-sm font-medium text-slate-100 hover:border-slate-500"
                  >
                    Cancel
                  </button>
                  <button
                    type="button"
                    onClick={handleCreate}
                    disabled={createMutation.isPending}
                    className="inline-flex items-center gap-2 rounded-xl bg-blue-500 px-4 py-3 text-sm font-semibold text-white hover:bg-blue-400 disabled:opacity-50"
                  >
                    <Plus className="h-4 w-4" />
                    {createMutation.isPending ? 'Creating...' : 'Create'}
                  </button>
                </div>
              </aside>
            </div>
          ) : null}
        </div>
      </div>
    </section>
  )
}

function FieldEditor({
  field,
  controlOptions,
  targetFieldGroups,
  onChange,
  onDelete,
}: {
  field: EmploymentApplicationFieldRequest
  controlOptions: EmploymentApplicationBuilderCatalogResponse['controlOptions']
  targetFieldGroups: EmploymentApplicationBuilderCatalogResponse['targetFieldGroups']
  onChange: (patch: Partial<EmploymentApplicationFieldRequest>) => void
  onDelete: () => void
}) {
  return (
    <div className="rounded-2xl border border-slate-800 bg-slate-900/70 p-4">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div className="grid gap-3 md:grid-cols-2 md:flex-1">
          <label className="block text-sm text-slate-300">
            Field key
            <input
              value={field.fieldKey}
              onChange={(event) => onChange({ fieldKey: event.target.value })}
              className="mt-1 w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-white"
            />
          </label>
          <label className="block text-sm text-slate-300">
            Label
            <input
              value={field.label}
              onChange={(event) => onChange({ label: event.target.value })}
              className="mt-1 w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-white"
            />
          </label>
          <label className="block text-sm text-slate-300">
            Control
            <select
              value={field.control}
              onChange={(event) => onChange({ control: event.target.value as EmploymentApplicationFieldRequest['control'] })}
              className="mt-1 w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-white"
            >
              {controlOptions.map((option) => (
                <option key={option.value} value={option.value}>
                  {option.label}
                </option>
              ))}
            </select>
          </label>
          <label className="block text-sm text-slate-300">
            Mapping
            <select
              value={field.mappingMode}
              onChange={(event) =>
                onChange({ mappingMode: event.target.value as EmploymentApplicationFieldRequest['mappingMode'] })
              }
              className="mt-1 w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-white"
            >
              {MAPPING_OPTIONS.map((option) => (
                <option key={option} value={option}>
                  {option}
                </option>
              ))}
            </select>
          </label>
          <label className="block text-sm text-slate-300">
            Target field
            <select
              value={field.targetFieldKey ?? ''}
              onChange={(event) => onChange({ targetFieldKey: event.target.value || null })}
              className="mt-1 w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-white"
            >
              <option value="">Application only — do not map</option>
              {targetFieldGroups.map((group) => (
                <optgroup key={group.key} label={group.label}>
                  {group.fields.map((option) => (
                    <option key={option.value} value={option.value}>
                      {option.label} ({option.stage})
                    </option>
                  ))}
                </optgroup>
              ))}
            </select>
          </label>
          <label className="flex items-center gap-2 text-sm text-slate-300 pt-6">
            <input
              type="checkbox"
              checked={field.required}
              onChange={(event) => onChange({ required: event.target.checked })}
            />
            Required
          </label>
        </div>

        <button
          type="button"
          onClick={onDelete}
          className="inline-flex items-center gap-2 rounded-lg border border-rose-900/60 px-3 py-2 text-sm font-medium text-rose-100 hover:border-rose-500"
        >
          <Trash2 className="h-4 w-4" />
          Remove
        </button>
      </div>

      <div className="mt-4 grid gap-3 md:grid-cols-2">
        <label className="block text-sm text-slate-300">
          Help text
          <input
            value={field.helpText ?? ''}
            onChange={(event) => onChange({ helpText: event.target.value || null })}
            className="mt-1 w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-white"
          />
        </label>
        <label className="block text-sm text-slate-300">
          Placeholder
          <input
            value={field.placeholder ?? ''}
            onChange={(event) => onChange({ placeholder: event.target.value || null })}
            className="mt-1 w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-white"
          />
        </label>
      </div>

      {field.control === 'select' || field.control === 'multi_select' ? (
        <SelectOptionsEditor field={field} onChange={onChange} />
      ) : null}
    </div>
  )
}

function SelectOptionsEditor({
  field,
  onChange,
}: {
  field: EmploymentApplicationFieldRequest
  onChange: (patch: Partial<EmploymentApplicationFieldRequest>) => void
}) {
  const options = field.options ?? []
  const title = field.control === 'multi_select' ? 'Multi-select options' : 'Select options'
  const subtitle =
    field.control === 'multi_select'
      ? 'Multi-select fields need at least one option before publishing.'
      : 'Select controls need at least one option before publishing.'

  const updateOption = (index: number, patch: Partial<(typeof options)[number]>) => {
    onChange({
      options: options.map((option, optionIndex) => (optionIndex === index ? { ...option, ...patch } : option)),
    })
  }

  const addOption = () => {
    onChange({
      options: [...options, { value: `option_${crypto.randomUUID().slice(0, 6)}`, label: 'New option' }],
    })
  }

  const removeOption = (index: number) => {
    onChange({
      options: options.filter((_, optionIndex) => optionIndex !== index),
    })
  }

  return (
    <div className="mt-4 rounded-xl border border-slate-800 bg-slate-950/70 p-4">
      <div className="flex items-center justify-between gap-3">
        <div>
          <h3 className="text-sm font-semibold text-white">{title}</h3>
          <p className="text-xs text-[var(--color-text-muted)]">{subtitle}</p>
        </div>
        <button
          type="button"
          onClick={addOption}
          className="inline-flex items-center gap-2 rounded-lg border border-slate-700 px-3 py-2 text-xs font-medium text-slate-100 hover:border-cyan-400"
        >
          <Plus className="h-3.5 w-3.5" />
          Add option
        </button>
      </div>

      <div className="mt-3 space-y-3">
        {options.map((option, index) => (
          <div key={`${option.value}-${index}`} className="grid gap-2 md:grid-cols-[1fr_1fr_auto]">
            <input
              value={option.value}
              onChange={(event) => updateOption(index, { value: event.target.value })}
              className="rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-white"
              placeholder="Value"
            />
            <input
              value={option.label}
              onChange={(event) => updateOption(index, { label: event.target.value })}
              className="rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-white"
              placeholder="Label"
            />
            <button
              type="button"
              onClick={() => removeOption(index)}
              className="rounded-lg border border-slate-700 px-3 py-2 text-sm text-slate-300 hover:border-rose-500"
            >
              Remove
            </button>
          </div>
        ))}
      </div>
    </div>
  )
}
