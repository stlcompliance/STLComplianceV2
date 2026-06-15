import { useMutation, useQuery } from '@tanstack/react-query'
import { CheckCircle2, Clock3, FileUp, HelpCircle, Loader2, AlertTriangle } from 'lucide-react'
import { useMemo, useState } from 'react'

export interface QuestionnaireResolveRequest {
  tenantId: string
  productKey: string
  workflowKey: string
  subjectType: string
  subjectId?: string | null
  subjectLabel?: string | null
  sourceRecordId?: string | null
  sourceEntity?: string | null
  knownFacts?: Record<string, string>
  sourceRecordContext?: Record<string, string>
  persistRun?: boolean
}

export interface QuestionnaireAnswerRequest {
  questionKey: string
  selectedOptionKey?: string | null
  answerText?: string | null
  documentUrl?: string | null
  storageKey?: string | null
  fileName?: string | null
  fileHash?: string | null
  evidenceId?: string | null
  effectiveAt?: string | null
}

export interface QuestionnaireAnswerOptionResponse {
  key: string
  label: string
  description: string
  answerKind: string
  isDefault: boolean
}

export interface QuestionnaireQuestionResponse {
  questionKey: string
  sectionKey: string
  sectionLabel: string
  prompt: string
  helpText: string | null
  whyItMatters: string | null
  answerKind: string
  factKey: string
  factValueType: string
  required: boolean
  priority: number
  defaultOptionKey: string | null
  options: QuestionnaireAnswerOptionResponse[]
  applicableAreas: string[]
  recommendedNextActions: string[]
}

export interface QuestionnaireTenantProfileResponse {
  businessProfile: string
  transportationExposure: string[]
  workforceExposure: string[]
  locationExposure: string[]
  materialHazmatExposure: string[]
  recordDocumentMaturity: string
  likelyRulePacks: string[]
  initialAssumptions: string[]
  setupChecklist: string[]
}

export interface QuestionnaireExceptionResponse {
  exceptionKey: string
  label: string
  reason: string
  severity: string
}

export interface QuestionnaireFollowUpResponse {
  followUpKey: string
  prompt: string
  reason: string
  triggerFactKey: string
  priority: string
}

export interface QuestionnaireResultSummaryResponse {
  summary: string
  likelyApplicableAreas: string[]
  missingFacts: string[]
  recommendedNextActions: string[]
  generatedExceptions: QuestionnaireExceptionResponse[]
  followUps: QuestionnaireFollowUpResponse[]
  requiresMoreFacts: boolean
  riskGateStatus: string
}

export interface QuestionnaireRunResponse {
  questionnaireRunId: string
  productKey: string
  workflowKey: string
  subjectType: string
  subjectId: string
  sourceRecordId: string
  sourceEntity: string
  status: string
  templateKey: string
  createdAt: string
  updatedAt: string
  resolvedAt: string | null
  submittedAt: string | null
}

export interface QuestionnaireResolutionResponse {
  run: QuestionnaireRunResponse
  questions: QuestionnaireQuestionResponse[]
  tenantProfile: QuestionnaireTenantProfileResponse
  summary: QuestionnaireResultSummaryResponse
}

export interface QuestionnaireAnswerResponse {
  questionnaireAnswerId: string
  questionKey: string
  selectedOptionKey: string
  answerText: string
  documentUrl: string
  storageKey: string
  fileName: string
  fileHash: string
  normalizedFactKey: string
  normalizedFactValue: string
  normalizedFactValueType: string
  reviewStatus: string
  confidence: number
  effectiveAt: string
  evidenceReferenceId: string | null
  evidenceId: string | null
}

export interface QuestionnaireFactResponse {
  factAssertionId: string
  factKey: string
  subjectKind: string
  subjectId: string
  value: string
  valueType: string
  sourceProduct: string
  sourceRecordId: string
  reviewStatus: string
  confidence: number
  assertedAt: string
  effectiveAt: string | null
  expiresAt: string | null
}

export interface QuestionnaireSubmissionResponse {
  run: QuestionnaireRunResponse
  tenantProfile: QuestionnaireTenantProfileResponse
  summary: QuestionnaireResultSummaryResponse
  answers: QuestionnaireAnswerResponse[]
  createdFacts: QuestionnaireFactResponse[]
}

export interface QuestionnaireFlowProps {
  apiBase: string
  accessToken: string
  tenantId: string
  productKey: string
  workflowKey: string
  subjectType: string
  subjectId?: string | null
  subjectLabel?: string | null
  sourceRecordId?: string | null
  sourceEntity?: string | null
  knownFacts?: Record<string, string>
  sourceRecordContext?: Record<string, string>
  title: string
  subtitle?: string
  submitLabel?: string
  onSubmitted?: (response: QuestionnaireSubmissionResponse) => void
}

type DraftAnswer = {
  selectedOptionKey?: string
  answerText?: string
  document?: File
  documentUrl?: string
  storageKey?: string
  fileName?: string
  fileHash?: string
}

async function parseJsonResponse<T>(response: Response, fallbackMessage: string): Promise<T> {
  if (!response.ok) {
    const body = await response.text()
    throw new Error(body || `${fallbackMessage} (${response.status})`)
  }

  return (await response.json()) as T
}

function authHeaders(accessToken: string): HeadersInit {
  return {
    Authorization: `Bearer ${accessToken}`,
    'Content-Type': 'application/json',
  }
}

export async function resolveQuestionnaire(
  apiBase: string,
  accessToken: string,
  request: QuestionnaireResolveRequest,
): Promise<QuestionnaireResolutionResponse> {
  const response = await fetch(`${apiBase}/api/v1/questionnaires/resolve`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  return parseJsonResponse<QuestionnaireResolutionResponse>(response, 'Failed to resolve questionnaire')
}

export async function submitQuestionnaire(
  apiBase: string,
  accessToken: string,
  runId: string,
  answers: QuestionnaireAnswerRequest[],
  sourceRecordContext?: Record<string, string>,
): Promise<QuestionnaireSubmissionResponse> {
  const response = await fetch(`${apiBase}/api/v1/questionnaires/${encodeURIComponent(runId)}/submit`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify({ answers, sourceRecordContext }),
  })
  return parseJsonResponse<QuestionnaireSubmissionResponse>(response, 'Failed to submit questionnaire')
}

export function QuestionnaireFlow({
  apiBase,
  accessToken,
  tenantId,
  productKey,
  workflowKey,
  subjectType,
  subjectId,
  subjectLabel,
  sourceRecordId,
  sourceEntity,
  knownFacts,
  sourceRecordContext,
  title,
  subtitle,
  submitLabel = 'Save answers',
  onSubmitted,
}: QuestionnaireFlowProps) {
  const [drafts, setDrafts] = useState<Record<string, DraftAnswer>>({})
  const resolveQuery = useQuery({
    queryKey: [
      'questionnaire-resolve',
      apiBase,
      tenantId,
      productKey,
      workflowKey,
      subjectType,
      subjectId ?? '',
      sourceRecordId ?? '',
    ],
    queryFn: () =>
      resolveQuestionnaire(apiBase, accessToken, {
        tenantId,
        productKey,
        workflowKey,
        subjectType,
        subjectId,
        subjectLabel,
        sourceRecordId,
        sourceEntity,
        knownFacts,
        sourceRecordContext,
        persistRun: true,
      }),
    enabled: Boolean(apiBase && accessToken),
    retry: false,
  })

  const submitMutation = useMutation({
    mutationFn: async () => {
      if (!resolveQuery.data) {
        throw new Error('Questionnaire is still loading.')
      }

      const answers = resolveQuery.data.questions.map((question) => {
        const draft = drafts[question.questionKey]
        const document = draft?.document
        return {
          questionKey: question.questionKey,
          selectedOptionKey: draft?.selectedOptionKey ?? question.defaultOptionKey ?? null,
          answerText: draft?.answerText ?? null,
          documentUrl: draft?.documentUrl ?? (document ? URL.createObjectURL(document) : null),
          storageKey: draft?.storageKey ?? null,
          fileName: draft?.fileName ?? document?.name ?? null,
          fileHash: draft?.fileHash ?? (document ? `${document.name}:${document.size}` : null),
        } satisfies QuestionnaireAnswerRequest
      })

      return submitQuestionnaire(apiBase, accessToken, resolveQuery.data.run.questionnaireRunId, answers, sourceRecordContext)
    },
    onSuccess: (response) => {
      onSubmitted?.(response)
    },
  })

  const sortedQuestions = useMemo(
    () => [...(resolveQuery.data?.questions ?? [])].sort((left, right) => left.priority - right.priority),
    [resolveQuery.data?.questions],
  )

  return (
    <section className="rounded-3xl border border-slate-800 bg-slate-950/70 p-5 shadow-2xl shadow-slate-950/40">
      <div className="flex flex-wrap items-start justify-between gap-4">
        <div className="min-w-0">
          <h2 className="text-lg font-semibold text-white">{title}</h2>
          {subtitle ? <p className="mt-1 text-sm text-slate-400">{subtitle}</p> : null}
          {subjectLabel ? <p className="mt-2 text-xs text-slate-500">{subjectLabel}</p> : null}
        </div>
        <div className="flex items-center gap-2 rounded-full border border-slate-800 bg-slate-900 px-3 py-1 text-xs text-slate-400">
          <Clock3 className="h-3.5 w-3.5" />
          {resolveQuery.data?.summary.riskGateStatus ?? 'loading'}
        </div>
      </div>

      {!apiBase ? (
        <p className="mt-4 rounded-xl border border-slate-800 bg-slate-900/60 p-3 text-sm text-slate-400">
          Compliance Core questionnaire API is not configured for this product yet.
        </p>
      ) : null}

      {resolveQuery.isLoading ? (
        <div className="mt-4 flex items-center gap-3 text-sm text-slate-300">
          <Loader2 className="h-4 w-4 animate-spin" />
          Loading questionnaire...
        </div>
      ) : null}

      {resolveQuery.isError ? (
        <p className="mt-4 rounded-xl border border-rose-900/70 bg-rose-950/40 p-3 text-sm text-rose-100">
          {(resolveQuery.error as Error).message}
        </p>
      ) : null}

      {resolveQuery.data ? (
        <div className="mt-5 space-y-4">
          <div className="grid gap-3 md:grid-cols-3">
            <ProfileCard
              label="Likely areas"
              value={resolveQuery.data.summary.likelyApplicableAreas.join(', ') || 'None yet'}
            />
            <ProfileCard
              label="Missing facts"
              value={resolveQuery.data.summary.missingFacts.length.toString()}
            />
            <ProfileCard
              label="Next actions"
              value={resolveQuery.data.summary.recommendedNextActions.length.toString()}
            />
          </div>

          {resolveQuery.data.summary.followUps.length > 0 ? (
            <div className="rounded-2xl border border-amber-900/50 bg-amber-950/20 p-4">
              <div className="flex items-center gap-2 text-sm font-medium text-amber-100">
                <AlertTriangle className="h-4 w-4" />
                Follow-ups
              </div>
              <ul className="mt-3 space-y-2 text-sm text-amber-50/90">
                {resolveQuery.data.summary.followUps.map((followUp) => (
                  <li key={followUp.followUpKey} className="rounded-xl border border-amber-900/30 bg-amber-950/30 px-3 py-2">
                    <div className="font-medium">{followUp.prompt}</div>
                    <div className="mt-1 text-xs text-amber-100/70">{followUp.reason}</div>
                  </li>
                ))}
              </ul>
            </div>
          ) : null}

          <div className="space-y-4">
            {sortedQuestions.map((question) => (
              <QuestionCard
                key={question.questionKey}
                question={question}
                value={drafts[question.questionKey]}
                onChange={(draft) => setDrafts((current) => ({ ...current, [question.questionKey]: draft }))}
              />
            ))}
          </div>

          <div className="flex flex-wrap items-center justify-between gap-3 rounded-2xl border border-slate-800 bg-slate-900/70 px-4 py-3">
            <div className="text-sm text-slate-400">
              {resolveQuery.data.summary.riskGateStatus === 'blocked'
                ? 'This workflow needs more facts before it can proceed.'
                : 'You can submit partial answers, and Compliance Core will keep unknowns reviewable.'}
            </div>
            <button
              type="button"
              onClick={() => submitMutation.mutate()}
              disabled={submitMutation.isPending}
              className="inline-flex items-center gap-2 rounded-xl bg-cyan-600 px-4 py-2 text-sm font-semibold text-white hover:bg-cyan-500 disabled:opacity-50"
            >
              {submitMutation.isPending ? 'Saving...' : submitLabel}
            </button>
          </div>

          {submitMutation.isError ? (
            <p className="rounded-xl border border-rose-900/70 bg-rose-950/40 p-3 text-sm text-rose-100">
              {(submitMutation.error as Error).message}
            </p>
          ) : null}

          {submitMutation.data ? (
            <div className="rounded-2xl border border-emerald-900/40 bg-emerald-950/20 p-4">
              <div className="flex items-center gap-2 text-sm font-medium text-emerald-100">
                <CheckCircle2 className="h-4 w-4" />
                {submitMutation.data.summary.summary}
              </div>
              <p className="mt-2 text-sm text-emerald-50/80">
                {submitMutation.data.summary.likelyApplicableAreas.join(', ') || 'No major applicability areas yet.'}
              </p>
            </div>
          ) : null}
        </div>
      ) : null}
    </section>
  )
}

function ProfileCard({ label, value }: { label: string; value: string }) {
  return (
    <div className="rounded-2xl border border-slate-800 bg-slate-900/70 p-4">
      <div className="text-xs uppercase tracking-wide text-slate-500">{label}</div>
      <div className="mt-2 text-sm text-slate-100">{value}</div>
    </div>
  )
}

function QuestionCard({
  question,
  value,
  onChange,
}: {
  question: QuestionnaireQuestionResponse
  value: DraftAnswer | undefined
  onChange: (draft: DraftAnswer) => void
}) {
  const [showDocumentHint, setShowDocumentHint] = useState(false)

  return (
    <article className="rounded-2xl border border-slate-800 bg-slate-950 p-4">
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div className="min-w-0">
          <div className="flex flex-wrap items-center gap-2">
            <h3 className="text-sm font-semibold text-white">{question.prompt}</h3>
            {question.required ? (
              <span className="rounded-full bg-sky-500/15 px-2 py-0.5 text-[11px] font-semibold uppercase tracking-wide text-sky-200">
                Required
              </span>
            ) : null}
          </div>
          {question.whyItMatters ? <p className="mt-1 text-xs text-slate-400">{question.whyItMatters}</p> : null}
          {question.helpText ? <p className="mt-1 text-xs text-slate-500">{question.helpText}</p> : null}
        </div>
        {question.applicableAreas.length > 0 ? (
          <div className="text-right text-xs text-slate-500">{question.applicableAreas.join(' · ')}</div>
        ) : null}
      </div>

      {question.answerKind === 'document' ? (
        <label className="mt-4 flex cursor-pointer items-center justify-between rounded-xl border border-dashed border-slate-700 bg-slate-900/50 px-4 py-3 text-sm text-slate-300 hover:border-slate-500">
          <span className="inline-flex items-center gap-2">
            <FileUp className="h-4 w-4" />
            {value?.fileName || 'Choose a file'}
          </span>
          <input
            type="file"
            className="hidden"
            onChange={(event) => {
              const file = event.target.files?.[0]
              if (!file) return
              const objectUrl = window.URL.createObjectURL(file)
              onChange({
                ...value,
                document: file,
                documentUrl: objectUrl,
                fileName: file.name,
                fileHash: `${file.name}:${file.size}`,
              })
              setShowDocumentHint(true)
            }}
          />
        </label>
      ) : question.options.length > 0 ? (
        <div className="mt-4 grid gap-2 sm:grid-cols-2 lg:grid-cols-3">
          {question.options.map((option) => {
            const selected = value?.selectedOptionKey
              ? value.selectedOptionKey === option.key
              : question.defaultOptionKey === option.key
            return (
              <button
                key={option.key}
                type="button"
                onClick={() => onChange({ ...value, selectedOptionKey: option.key })}
                className={`rounded-xl border px-3 py-3 text-left transition ${
                  selected
                    ? 'border-cyan-400 bg-cyan-500/10 text-cyan-50'
                    : 'border-slate-800 bg-slate-900/70 text-slate-200 hover:border-slate-600'
                }`}
              >
                <div className="flex items-center gap-2 text-sm font-medium">
                  {selected ? <CheckCircle2 className="h-4 w-4 text-cyan-300" /> : <HelpCircle className="h-4 w-4 text-slate-500" />}
                  {option.label}
                </div>
                <p className="mt-2 text-xs text-slate-400">{option.description}</p>
              </button>
            )
          })}
        </div>
      ) : (
        <div className="mt-4 text-sm text-slate-400">No answer options configured.</div>
      )}

      {question.answerKind !== 'document' ? (
        <div className="mt-3 flex flex-wrap gap-2">
          {question.options.some((option) => option.key === 'not_sure') ? (
            <button
              type="button"
              onClick={() => onChange({ ...value, selectedOptionKey: 'not_sure' })}
              className="rounded-full border border-slate-700 px-3 py-1 text-xs text-slate-300 hover:border-slate-500"
            >
              Not sure
            </button>
          ) : null}
          {question.options.some((option) => option.key === 'skip_for_now') ? (
            <button
              type="button"
              onClick={() => onChange({ ...value, selectedOptionKey: 'skip_for_now' })}
              className="rounded-full border border-slate-700 px-3 py-1 text-xs text-slate-300 hover:border-slate-500"
            >
              Skip for now
            </button>
          ) : null}
        </div>
      ) : null}

      {showDocumentHint ? (
        <p className="mt-3 text-xs text-slate-500">
          Uploading a document creates a reviewable evidence reference.
        </p>
      ) : null}
    </article>
  )
}
