import { useEffect, useMemo, useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Copy, Plus, Save, Sparkles, Trash2 } from 'lucide-react'
import {
  cloneEmploymentApplicationTemplate,
  createEmploymentApplicationTemplate,
  listEmploymentApplicationSubmissions,
  listEmploymentApplicationTemplates,
  publishEmploymentApplicationTemplate,
  updateEmploymentApplicationTemplate,
} from '../../api/client'
import type {
  EmploymentApplicationFieldRequest,
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
  'number',
]

const MAPPING_OPTIONS: EmploymentApplicationFieldRequest['mappingMode'][] = ['create', 'eventual', 'unmapped']

const TARGET_FIELD_OPTIONS = [
  { value: 'legalFirstName', label: 'Legal first name', stage: 'create' },
  { value: 'legalLastName', label: 'Legal last name', stage: 'create' },
  { value: 'primaryEmail', label: 'Primary email', stage: 'create' },
  { value: 'primaryPhone', label: 'Primary phone', stage: 'create' },
  { value: 'preferredName', label: 'Preferred name', stage: 'eventual' },
  { value: 'pronouns', label: 'Pronouns', stage: 'eventual' },
  { value: 'workRelationshipType', label: 'Work relationship', stage: 'create' },
  { value: 'employmentType', label: 'Employment type', stage: 'create' },
  { value: 'expectedStartDate', label: 'Desired start date', stage: 'create' },
  { value: 'jobTitle', label: 'Position applying for', stage: 'eventual' },
  { value: 'alternateEmail', label: 'Alternate email', stage: 'create' },
  { value: 'alternatePhone', label: 'Alternate phone', stage: 'create' },
  { value: 'workPhone', label: 'Work phone', stage: 'create' },
  { value: 'managerPersonId', label: 'Manager person', stage: 'eventual' },
  { value: 'homeBaseLocationId', label: 'Home base location', stage: 'eventual' },
] as const

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
  return 'Employment application'
}

function defaultTemplateKey(): string {
  return `employment-application-${crypto.randomUUID().slice(0, 8)}`
}

function defaultTemplateRequest(): EmploymentApplicationTemplateCreateRequest {
  return {
    templateKey: defaultTemplateKey(),
    templateName: defaultTemplateName(),
    title: 'Employment application',
    subtitle: 'Tell us a little about yourself so we can build your applicant profile in StaffArr.',
    submitLabel: 'Submit application',
    publicLinkExpiresAt: new Date(Date.now() + 90 * 24 * 60 * 60 * 1000).toISOString(),
    fields: [
      {
        fieldKey: 'legalFirstName',
        label: 'Legal first name',
        control: 'text',
        required: true,
        mappingMode: 'create',
        targetFieldKey: 'legalFirstName',
        helpText: 'Matches the person record.',
        placeholder: 'First name',
        options: [],
      },
      {
        fieldKey: 'legalLastName',
        label: 'Legal last name',
        control: 'text',
        required: true,
        mappingMode: 'create',
        targetFieldKey: 'legalLastName',
        helpText: 'Matches the person record.',
        placeholder: 'Last name',
        options: [],
      },
      {
        fieldKey: 'primaryEmail',
        label: 'Email',
        control: 'email',
        required: true,
        mappingMode: 'create',
        targetFieldKey: 'primaryEmail',
        helpText: 'This is the login/contact email.',
        placeholder: 'name@example.com',
        options: [],
      },
      {
        fieldKey: 'primaryPhone',
        label: 'Phone',
        control: 'phone',
        required: false,
        mappingMode: 'create',
        targetFieldKey: 'primaryPhone',
        helpText: null,
        placeholder: '(555) 123-4567',
        options: [],
      },
      {
        fieldKey: 'preferredName',
        label: 'Preferred name',
        control: 'text',
        required: false,
        mappingMode: 'eventual',
        targetFieldKey: 'preferredName',
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
  const [selectedTemplateId, setSelectedTemplateId] = useState<string | null>(null)
  const [draft, setDraft] = useState<EmploymentApplicationTemplateUpsertRequest | null>(null)
  const [savedDraft, setSavedDraft] = useState<EmploymentApplicationTemplateUpsertRequest | null>(null)
  const [newTemplateName, setNewTemplateName] = useState(defaultTemplateName())
  const [newTemplateKey, setNewTemplateKey] = useState(defaultTemplateKey())
  const [localError, setLocalError] = useState<string | null>(null)
  const [successMessage, setSuccessMessage] = useState<string | null>(null)

  const templatesQuery = useQuery({
    queryKey: ['staffarr-employment-application-templates', state.accessToken],
    queryFn: () => listEmploymentApplicationTemplates(state.accessToken),
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

  const groupedTemplates = useMemo(() => {
    const groups = new Map<string, EmploymentApplicationTemplateResponse[]>()
    for (const template of templatesQuery.data ?? []) {
      const entries = groups.get(template.templateKey) ?? []
      entries.push(template)
      groups.set(template.templateKey, entries)
    }

    return Array.from(groups.entries())
      .map(([templateKey, entries]) => ({
        templateKey,
        templates: entries.sort((left, right) => right.version - left.version || right.updatedAt.localeCompare(left.updatedAt)),
      }))
      .sort((left, right) => left.templateKey.localeCompare(right.templateKey))
  }, [templatesQuery.data])

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
      setLocalError(null)
      setSuccessMessage('Employment application template created.')
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
      setSuccessMessage('Employment application template saved.')
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
    <section className="space-y-6">
      <div className="rounded-2xl border border-cyan-500/30 bg-gradient-to-br from-slate-950 via-slate-950 to-cyan-950/30 p-6 shadow-lg shadow-cyan-950/10">
        <div className="flex flex-wrap items-start justify-between gap-4">
          <div>
            <div className="inline-flex items-center gap-2 rounded-full border border-cyan-500/30 bg-cyan-500/10 px-3 py-1 text-xs font-semibold text-cyan-200">
              <Sparkles className="h-3.5 w-3.5" />
              Versioned template builder
            </div>
            <h1 className="mt-4 text-3xl font-semibold tracking-tight text-white">Employment applications</h1>
            <p className="mt-2 max-w-3xl text-sm text-slate-300">
              Create multiple application templates, clone new versions from any existing one, and publish a public
              link that creates a StaffArr person record with intelligent field mappings.
            </p>
          </div>
          <div className="rounded-xl border border-slate-700 bg-slate-950/70 px-4 py-3 text-sm text-slate-300">
            <div className="font-medium text-white">Public link</div>
            <div className="mt-1 max-w-md break-all text-xs text-cyan-200">
              {publicUrl || 'Publish a template to generate a public link.'}
            </div>
            {selectedTemplate?.publicLinkExpiresAt ? (
              <div className="mt-2 text-xs text-slate-500">
                Expires {new Date(selectedTemplate.publicLinkExpiresAt).toLocaleString()}
              </div>
            ) : null}
          </div>
        </div>
      </div>

      {localError ? (
        <p className="rounded-xl border border-rose-900/60 bg-rose-950/40 px-4 py-3 text-sm text-rose-100">
          {localError}
        </p>
      ) : null}

      {successMessage ? (
        <p className="rounded-xl border border-emerald-900/40 bg-emerald-950/30 px-4 py-3 text-sm text-emerald-100">
          {successMessage}
        </p>
      ) : null}

      {templatesQuery.isLoading ? <p className="text-sm text-slate-400">Loading employment application templates...</p> : null}

      {templatesQuery.data ? (
        <div className="grid gap-6 xl:grid-cols-[0.95fr_1.4fr]">
          <div className="space-y-6">
            <div className="rounded-2xl border border-slate-800 bg-slate-950/70 p-5">
              <div className="flex items-center justify-between gap-3">
                <div>
                  <h2 className="text-lg font-semibold text-white">Templates</h2>
                  <p className="mt-1 text-sm text-slate-400">Select a template key, then pick one of its versions.</p>
                </div>
                <span className="rounded-full border border-slate-700 px-3 py-1 text-xs text-slate-300">
                  {templatesQuery.data.length} total
                </span>
              </div>

              <div className="mt-4 space-y-4">
                {groupedTemplates.map((group) => {
                  const activeTemplate = group.templates.find((template) => template.status === 'published') ?? group.templates[0]
                  return (
                    <div key={group.templateKey} className="rounded-2xl border border-slate-800 bg-slate-900/50 p-3">
                      <div className="flex flex-wrap items-start justify-between gap-3 px-1">
                        <div>
                          <div className="text-sm font-semibold text-white">{group.templates[0].templateName}</div>
                          <div className="mt-1 text-xs text-slate-400">
                            {group.templateKey} · {group.templates.length} version{group.templates.length === 1 ? '' : 's'}
                          </div>
                        </div>
                        <button
                          type="button"
                          onClick={() => setSelectedTemplateId(activeTemplate.employmentApplicationTemplateId)}
                          className="rounded-lg border border-slate-700 px-3 py-1 text-xs font-medium text-slate-200 hover:border-cyan-400"
                        >
                          Open latest
                        </button>
                      </div>

                      <div className="mt-3 space-y-2">
                        {group.templates.map((template) => (
                          <button
                            key={template.employmentApplicationTemplateId}
                            type="button"
                            onClick={() => setSelectedTemplateId(template.employmentApplicationTemplateId)}
                            className={`w-full rounded-xl border px-4 py-3 text-left transition ${
                              selectedTemplateId === template.employmentApplicationTemplateId
                                ? 'border-cyan-400 bg-cyan-500/10 text-white'
                                : 'border-slate-800 bg-slate-950/70 text-slate-200 hover:border-slate-600'
                            }`}
                          >
                            <div className="flex items-center justify-between gap-3">
                              <div className="font-medium">Version {template.version}</div>
                              <div className="flex items-center gap-2">
                                {template.status === 'published' ? (
                                  <span className="rounded-full border border-emerald-500/30 bg-emerald-500/10 px-2 py-0.5 text-[11px] uppercase tracking-wide text-emerald-200">
                                    Published
                                  </span>
                                ) : template.status === 'draft' ? (
                                  <span className="rounded-full border border-amber-500/30 bg-amber-500/10 px-2 py-0.5 text-[11px] uppercase tracking-wide text-amber-200">
                                    Draft
                                  </span>
                                ) : (
                                  <span className="rounded-full border border-slate-700 px-2 py-0.5 text-[11px] uppercase tracking-wide text-slate-400">
                                    Retired
                                  </span>
                                )}
                              </div>
                            </div>
                            <div className="mt-1 text-xs text-slate-400">
                              {template.title}
                            </div>
                          </button>
                        ))}
                      </div>
                    </div>
                  )
                })}
              </div>
            </div>

            <div className="rounded-2xl border border-slate-800 bg-slate-950/70 p-5">
              <div className="flex flex-wrap items-center justify-between gap-3">
                <div>
                  <h2 className="text-lg font-semibold text-white">Create template</h2>
                  <p className="mt-1 text-sm text-slate-400">Start a brand-new template key with a fresh draft.</p>
                </div>
                <button
                  type="button"
                  onClick={handleCreate}
                  disabled={createMutation.isPending}
                  className="inline-flex items-center gap-2 rounded-lg bg-cyan-600 px-3 py-2 text-sm font-semibold text-white hover:bg-cyan-500 disabled:opacity-50"
                >
                  <Plus className="h-4 w-4" />
                  {createMutation.isPending ? 'Creating...' : 'Create'}
                </button>
              </div>

              <div className="mt-4 grid gap-3">
                <label className="block text-sm text-slate-300">
                  Template name
                  <input
                    value={newTemplateName}
                    onChange={(event) => setNewTemplateName(event.target.value)}
                    className="mt-1 w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-white"
                  />
                </label>
                <label className="block text-sm text-slate-300">
                  Template key
                  <input
                    value={newTemplateKey}
                    onChange={(event) => setNewTemplateKey(event.target.value)}
                    className="mt-1 w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-white"
                  />
                </label>
                <p className="text-xs text-slate-500">
                  The key groups versions together, similar to MaintainArr inspection templates.
                </p>
              </div>
            </div>

            <div className="rounded-2xl border border-slate-800 bg-slate-950/70 p-5">
              <h2 className="text-lg font-semibold text-white">Recent submissions</h2>
              <p className="mt-1 text-sm text-slate-400">Public applicants are created as StaffArr people with applicant status.</p>
              <div className="mt-4 space-y-3">
                {(submissionsQuery.data ?? []).map((submission) => (
                  <div key={submission.employmentApplicationSubmissionId} className="rounded-xl border border-slate-800 bg-slate-900/70 p-4 text-sm">
                    <div className="flex items-center justify-between gap-3">
                      <div className="font-medium text-white">{submission.applicantDisplayName || submission.applicantEmail}</div>
                      <div className="text-xs text-slate-500">{submission.status}</div>
                    </div>
                    <div className="mt-1 text-xs text-slate-400">
                      {submission.templateKey} v{submission.templateVersion} · {new Date(submission.submittedAt).toLocaleString()}
                    </div>
                  </div>
                ))}
                {(submissionsQuery.data ?? []).length === 0 ? (
                  <p className="rounded-xl border border-slate-800 bg-slate-900/50 p-4 text-sm text-slate-500">
                    No submissions yet.
                  </p>
                ) : null}
              </div>
            </div>
          </div>

          <div className="space-y-6">
            {selectedTemplate && draft ? (
              <>
                <div className="rounded-2xl border border-slate-800 bg-slate-950/70 p-5">
                  <div className="flex flex-wrap items-start justify-between gap-3">
                    <div>
                      <h2 className="text-lg font-semibold text-white">Editor</h2>
                      <p className="mt-1 text-sm text-slate-400">
                        {selectedTemplate.templateKey} · version {selectedTemplate.version} · {selectedTemplate.status}
                      </p>
                    </div>
                    <div className="flex flex-wrap gap-2">
                      <button
                        type="button"
                        onClick={() => cloneMutation.mutate()}
                        disabled={cloneMutation.isPending}
                        className="inline-flex items-center gap-2 rounded-lg border border-slate-700 px-3 py-2 text-sm font-medium text-slate-100 hover:border-cyan-400 disabled:opacity-50"
                      >
                        <Copy className="h-4 w-4" />
                        {cloneMutation.isPending ? 'Cloning...' : 'Clone version'}
                      </button>
                      <button
                        type="button"
                        onClick={() => publishMutation.mutate()}
                        disabled={publishMutation.isPending || draft.fields.length === 0}
                        className="inline-flex items-center gap-2 rounded-lg bg-emerald-600 px-3 py-2 text-sm font-semibold text-white hover:bg-emerald-500 disabled:opacity-50"
                      >
                        {publishMutation.isPending ? 'Publishing...' : 'Publish'}
                      </button>
                    </div>
                  </div>

                  <div className="mt-5 grid gap-4 md:grid-cols-2">
                    <label className="block text-sm text-slate-300 md:col-span-2">
                      Template name
                      <input
                        value={draft.templateName}
                        onChange={(event) => setDraft((current) => current ? { ...current, templateName: event.target.value } : current)}
                        className="mt-1 w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-white"
                      />
                    </label>
                    <label className="block text-sm text-slate-300 md:col-span-2">
                      Application title
                      <input
                        value={draft.title}
                        onChange={(event) => setDraft((current) => current ? { ...current, title: event.target.value } : current)}
                        className="mt-1 w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-white"
                      />
                    </label>
                    <label className="block text-sm text-slate-300 md:col-span-2">
                      Intro copy
                      <textarea
                        rows={3}
                        value={draft.subtitle}
                        onChange={(event) => setDraft((current) => current ? { ...current, subtitle: event.target.value } : current)}
                        className="mt-1 w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-white"
                      />
                    </label>
                    <label className="block text-sm text-slate-300">
                      Submit button label
                      <input
                        value={draft.submitLabel}
                        onChange={(event) => setDraft((current) => current ? { ...current, submitLabel: event.target.value } : current)}
                        className="mt-1 w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-white"
                      />
                    </label>
                    <label className="block text-sm text-slate-300">
                      Public link expiry
                      <input
                        type="date"
                        value={draft.publicLinkExpiresAt ? new Date(draft.publicLinkExpiresAt).toISOString().slice(0, 10) : ''}
                        onChange={(event) =>
                          setDraft((current) =>
                            current
                              ? {
                                  ...current,
                                  publicLinkExpiresAt: event.target.value
                                    ? new Date(`${event.target.value}T23:59:59.999Z`).toISOString()
                                    : null,
                                }
                              : current,
                          )
                        }
                        className="mt-1 w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-white"
                      />
                    </label>
                  </div>
                </div>

                <div className="rounded-2xl border border-slate-800 bg-slate-950/70 p-5">
                  <div className="flex flex-wrap items-center justify-between gap-3">
                    <div>
                      <h2 className="text-lg font-semibold text-white">Fields</h2>
                      <p className="mt-1 text-sm text-slate-400">
                        Create-mapped fields seed StaffArr immediately; eventual fields are stored for later review.
                      </p>
                    </div>
                    <button
                      type="button"
                      onClick={addField}
                      className="inline-flex items-center gap-2 rounded-lg border border-slate-700 px-3 py-2 text-sm font-medium text-slate-100 hover:border-cyan-400"
                    >
                      <Plus className="h-4 w-4" />
                      Add field
                    </button>
                  </div>

                  <div className="mt-5 space-y-4">
                    {draft.fields.map((field) => (
                      <FieldEditor
                        key={field.fieldKey}
                        field={field}
                        onChange={(patch) => updateField(field.fieldKey, patch)}
                        onDelete={() => removeField(field.fieldKey)}
                      />
                    ))}
                  </div>
                </div>

                <div className="rounded-2xl border border-slate-800 bg-slate-950/70 p-5">
                  <div className="flex items-center justify-between gap-3">
                    <div>
                      <h2 className="text-lg font-semibold text-white">Preview</h2>
                      <p className="mt-1 text-sm text-slate-400">This is close to what applicants will see.</p>
                    </div>
                    <span className="rounded-full border border-slate-700 px-3 py-1 text-xs text-slate-300">
                      {draft.fields.length} fields
                    </span>
                  </div>
                  <div className="mt-4 space-y-3">
                    {draft.fields.map((field) => (
                      <div key={field.fieldKey} className="rounded-xl border border-slate-800 bg-slate-900/70 p-4">
                        <div className="flex items-center justify-between gap-3">
                          <div className="text-sm font-medium text-white">{field.label}</div>
                          <div className="rounded-full border border-slate-700 px-2 py-0.5 text-[11px] uppercase tracking-wide text-slate-400">
                            {field.mappingMode}
                          </div>
                        </div>
                        {field.helpText ? <p className="mt-1 text-xs text-slate-400">{field.helpText}</p> : null}
                        <p className="mt-2 text-xs text-slate-500">Target: {field.targetFieldKey ?? 'none'}</p>
                      </div>
                    ))}
                  </div>
                </div>
              </>
            ) : (
              <div className="rounded-2xl border border-slate-800 bg-slate-950/70 p-6 text-sm text-slate-400">
                No template selected.
              </div>
            )}
          </div>
        </div>
      ) : null}

      <div className="flex flex-wrap items-center justify-between gap-3 border-t border-slate-800 pt-4">
        <p className="text-xs text-slate-500">
          Fields marked <span className="font-semibold text-cyan-200">create</span> are applied immediately. Fields marked{' '}
          <span className="font-semibold text-amber-200">eventual</span> stay in the applicant profile draft for later review.
        </p>
        <button
          type="button"
          onClick={handleSave}
          disabled={saveMutation.isPending || !dirty || !selectedTemplate || selectedTemplate.status !== 'draft'}
          className="inline-flex items-center gap-2 rounded-lg bg-cyan-600 px-4 py-2 text-sm font-semibold text-white hover:bg-cyan-500 disabled:opacity-50"
        >
          <Save className="h-4 w-4" />
          {saveMutation.isPending ? 'Saving...' : dirty ? 'Save template' : 'Saved'}
        </button>
      </div>
    </section>
  )
}

function FieldEditor({
  field,
  onChange,
  onDelete,
}: {
  field: EmploymentApplicationFieldRequest
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
              {CONTROL_OPTIONS.map((option) => (
                <option key={option} value={option}>
                  {option}
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
              <option value="">None</option>
              {TARGET_FIELD_OPTIONS.map((option) => (
                <option key={option.value} value={option.value}>
                  {option.label} ({option.stage})
                </option>
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

      {field.control === 'select' ? (
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
          <h3 className="text-sm font-semibold text-white">Select options</h3>
          <p className="text-xs text-slate-500">Select controls need at least one option before publishing.</p>
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
