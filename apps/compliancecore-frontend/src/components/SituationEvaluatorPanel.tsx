import { useMutation, useQuery } from '@tanstack/react-query'
import {
  AlertTriangle,
  CheckCircle2,
  ClipboardCheck,
  Copy,
  Download,
  FileQuestion,
  GitBranch,
  Play,
  RotateCcw,
  Save,
} from 'lucide-react'
import { useMemo, useState } from 'react'

import {
  createTheoreticalSituation,
  duplicateTheoreticalSituationFromTemplate,
  evaluateTheoreticalSituation,
  getTheoreticalContextFields,
  getTheoreticalEvidenceStates,
  getTheoreticalIncidentOptions,
  getTheoreticalMaterialClasses,
  getTheoreticalNextContext,
  getTheoreticalSituationKinds,
  getTheoreticalSituationSimulationReport,
  resolveTheoreticalApplicability,
  saveTheoreticalSituationTemplate,
  setTheoreticalSituationContext,
  setTheoreticalSituationFacts,
  setTheoreticalSituationIncidents,
} from '../api/client'
import type {
  FactRequirementResponse,
  TheoreticalApplicabilityResultResponse,
  TheoreticalContextFieldResponse,
  TheoreticalSituationEvaluationResponse,
  TheoreticalSituationResponse,
} from '../api/types'

interface SituationEvaluatorPanelProps {
  accessToken: string
  canEvaluate: boolean
  factRequirements: FactRequirementResponse[]
}

type WizardStep = 'kind' | 'context' | 'facts' | 'review' | 'result'

const incidentKinds = new Set([
  'hazmat_incident_reporting',
  'accident_post_accident_testing',
  'incident_event_outcome',
])

function resultTone(result: string) {
  if (result === 'compliant') return 'compliant'
  if (result === 'allowed_with_override' || result === 'allowed_with_warning') return 'warning'
  if (result === 'insufficient_information') return 'info'
  return 'non_compliant'
}

export function SituationEvaluatorPanel({
  accessToken,
  canEvaluate,
  factRequirements,
}: SituationEvaluatorPanelProps) {
  const [step, setStep] = useState<WizardStep>('kind')
  const [selectedKind, setSelectedKind] = useState('driver_dispatch_readiness')
  const [situation, setSituation] = useState<TheoreticalSituationResponse | null>(null)
  const [contextValues, setContextValues] = useState<Record<string, string>>({})
  const [factStates, setFactStates] = useState<Record<string, string>>({})
  const [incidentType, setIncidentType] = useState('accident')
  const [applicability, setApplicability] = useState<TheoreticalApplicabilityResultResponse[]>([])
  const [evaluation, setEvaluation] = useState<TheoreticalSituationEvaluationResponse | null>(null)

  const kindQuery = useQuery({
    queryKey: ['tse-situation-kinds', accessToken],
    queryFn: () => getTheoreticalSituationKinds(accessToken),
    enabled: Boolean(accessToken),
  })
  const contextQuery = useQuery({
    queryKey: ['tse-context-fields', accessToken, selectedKind],
    queryFn: () => getTheoreticalContextFields(accessToken, selectedKind),
    enabled: Boolean(accessToken) && Boolean(selectedKind),
  })
  const evidenceStatesQuery = useQuery({
    queryKey: ['tse-evidence-states', accessToken],
    queryFn: () => getTheoreticalEvidenceStates(accessToken),
    enabled: Boolean(accessToken),
  })
  const materialClassesQuery = useQuery({
    queryKey: ['tse-material-classes', accessToken],
    queryFn: () => getTheoreticalMaterialClasses(accessToken),
    enabled: Boolean(accessToken),
  })
  const incidentOptionsQuery = useQuery({
    queryKey: ['tse-incidents', accessToken],
    queryFn: () => getTheoreticalIncidentOptions(accessToken),
    enabled: Boolean(accessToken),
  })

  const contextFields = useMemo(() => {
    const fields = contextQuery.data ?? []
    return fields.map((field) =>
      field.contextKey === 'material_class'
        ? { ...field, values: materialClassesQuery.data ?? field.values }
        : field,
    )
  }, [contextQuery.data, materialClassesQuery.data])

  const applicablePacks = useMemo(
    () =>
      new Set(
        applicability
          .filter((item) => ['primary', 'likely'].includes(item.applicabilityBand))
          .map((item) => item.packKey),
      ),
    [applicability],
  )
  const applicableRequirements = useMemo(() => {
    const rows = factRequirements
      .filter((requirement) => requirement.isActive)
      .filter((requirement) => requirement.rulePackKey && applicablePacks.has(requirement.rulePackKey))
    return rows.length > 0 ? rows : factRequirements.filter((requirement) => requirement.isActive).slice(0, 6)
  }, [applicablePacks, factRequirements])

  const createMutation = useMutation({
    mutationFn: () => createTheoreticalSituation(accessToken, { situationKind: selectedKind }),
    onSuccess: async (created) => {
      const nextContext = await getTheoreticalNextContext(accessToken, created.situationId)
      const defaults = Object.fromEntries(
        (contextQuery.data ?? nextContext.questions).map((field) => [
          field.contextKey,
          defaultValue(field),
        ]),
      )
      setSituation(created)
      setContextValues(defaults)
      setApplicability([])
      setEvaluation(null)
      setStep('context')
    },
  })

  const contextMutation = useMutation({
    mutationFn: async () => {
      if (!situation) throw new Error('Start a situation first.')
      return setTheoreticalSituationContext(accessToken, situation.situationId, {
        values: Object.entries(contextValues).map(([contextKey, contextValueKey]) => ({
          contextKey,
          contextValueKey,
        })),
      })
    },
    onSuccess: async () => {
      if (situation && incidentKinds.has(situation.situationKind)) {
        await setTheoreticalSituationIncidents(accessToken, situation.situationId, {
          incidents: [
            {
              incidentTypeKey: incidentType,
              severityKey: 'major',
              involvedSubjectKind: 'driver',
              involvedSubjectState: 'active',
              triggerKey: 'reportability',
              triggerValue: 'unknown',
              reportabilityState: 'unknown',
              remediationState: 'open',
            },
          ],
        })
      }
      if (situation) {
        const results = await resolveTheoreticalApplicability(accessToken, situation.situationId)
        setApplicability(results)
      }
      setStep('facts')
    },
  })

  const factsMutation = useMutation({
    mutationFn: async () => {
      if (!situation) throw new Error('Start a situation first.')
      return setTheoreticalSituationFacts(accessToken, situation.situationId, {
        facts: applicableRequirements.map((requirement) => ({
          factKey: requirement.factKey,
          requirementKey: requirement.requirementKey,
          citationKey: requirement.citationKey,
          packKey: requirement.rulePackKey,
          simulatedState: factStates[requirement.requirementKey] ?? 'unknown',
          valueType: 'boolean',
        })),
      })
    },
    onSuccess: () => setStep('review'),
  })

  const evaluateMutation = useMutation({
    mutationFn: async () => {
      if (!situation) throw new Error('Start a situation first.')
      return evaluateTheoreticalSituation(accessToken, situation.situationId)
    },
    onSuccess: (result) => {
      setEvaluation(result)
      setStep('result')
    },
  })

  const templateMutation = useMutation({
    mutationFn: async () => {
      if (!situation) throw new Error('Start a situation first.')
      return saveTheoreticalSituationTemplate(accessToken, situation.situationId)
    },
    onSuccess: setSituation,
  })

  const duplicateMutation = useMutation({
    mutationFn: async () => {
      if (!situation) throw new Error('Save this situation as a template first.')
      return duplicateTheoreticalSituationFromTemplate(accessToken, situation.situationId)
    },
    onSuccess: (copy) => {
      setSituation(copy)
      setEvaluation(null)
      setStep('context')
    },
  })

  const exportMutation = useMutation({
    mutationFn: async () => {
      if (!situation) throw new Error('Start a situation first.')
      return getTheoreticalSituationSimulationReport(accessToken, situation.situationId)
    },
    onSuccess: (report) => {
      const blob = new Blob([JSON.stringify(report, null, 2)], { type: 'application/json' })
      const url = window.URL.createObjectURL(blob)
      const link = document.createElement('a')
      link.href = url
      link.download = `theoretical-situation-${report.situationId}-simulation-report.json`
      document.body.appendChild(link)
      link.click()
      link.remove()
      window.URL.revokeObjectURL(url)
    },
  })

  const startOver = () => {
    setSituation(null)
    setContextValues({})
    setFactStates({})
    setApplicability([])
    setEvaluation(null)
    setStep('kind')
  }

  return (
    <div className="space-y-6" data-testid="situation-evaluator-panel">
      <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <h2 className="text-lg font-semibold text-slate-100">Situation Evaluator</h2>
          <p className="text-sm text-slate-400">
            Build a structured hypothetical situation and evaluate it without changing operational records.
          </p>
        </div>
        <div className="flex gap-2">
          <button
            type="button"
            onClick={startOver}
            className="inline-flex items-center gap-2 rounded-md border border-slate-700 px-3 py-2 text-sm text-slate-200 hover:bg-slate-800"
          >
            <RotateCcw size={16} /> Revise
          </button>
          <button
            type="button"
            onClick={() => templateMutation.mutate()}
            disabled={!canEvaluate || !situation || templateMutation.isPending}
            className="inline-flex items-center gap-2 rounded-md border border-slate-700 px-3 py-2 text-sm text-slate-200 hover:bg-slate-800 disabled:opacity-50"
          >
            <Save size={16} /> Save
          </button>
          <button
            type="button"
            onClick={() => duplicateMutation.mutate()}
            disabled={!canEvaluate || !situation?.savedAsTemplate || duplicateMutation.isPending}
            className="inline-flex items-center gap-2 rounded-md border border-slate-700 px-3 py-2 text-sm text-slate-200 hover:bg-slate-800 disabled:opacity-50"
          >
            <Copy size={16} /> Duplicate
          </button>
          <button
            type="button"
            onClick={() => exportMutation.mutate()}
            disabled={!situation || exportMutation.isPending}
            className="inline-flex items-center gap-2 rounded-md border border-slate-700 px-3 py-2 text-sm text-slate-200 hover:bg-slate-800 disabled:opacity-50"
          >
            <Download size={16} /> Export Report
          </button>
        </div>
      </div>

      <StepRail step={step} />

      {step === 'kind' && (
        <section className="rounded-lg border border-slate-700 bg-slate-900/70 p-4">
          <h3 className="text-sm font-semibold uppercase tracking-wide text-slate-400">
            What are you evaluating?
          </h3>
          <div className="mt-4 grid gap-2 sm:grid-cols-2 lg:grid-cols-3">
            {(kindQuery.data ?? []).map((kind) => (
              <button
                key={kind.key}
                type="button"
                onClick={() => setSelectedKind(kind.key)}
                className={`min-h-24 rounded-lg border p-3 text-left transition ${
                  selectedKind === kind.key
                    ? 'border-cyan-400 bg-cyan-950/40 text-cyan-50'
                    : 'border-slate-700 bg-slate-950 text-slate-200 hover:border-slate-500'
                }`}
              >
                <span className="block text-sm font-medium">{kind.label}</span>
                <span className="mt-2 block text-xs text-slate-400">{kind.description}</span>
              </button>
            ))}
          </div>
          <button
            type="button"
            onClick={() => createMutation.mutate()}
            disabled={!canEvaluate || createMutation.isPending}
            className="mt-4 inline-flex items-center gap-2 rounded-md bg-cyan-600 px-4 py-2 text-sm font-medium text-white hover:bg-cyan-500 disabled:opacity-50"
          >
            <FileQuestion size={16} /> Start Situation
          </button>
        </section>
      )}

      {step === 'context' && (
        <section className="rounded-lg border border-slate-700 bg-slate-900/70 p-4">
          <h3 className="text-sm font-semibold uppercase tracking-wide text-slate-400">
            Operational context
          </h3>
          <div className="mt-4 grid gap-4 md:grid-cols-2">
            {contextFields.map((field) => (
              <ContextControl
                key={field.contextKey}
                field={field}
                value={contextValues[field.contextKey] ?? defaultValue(field)}
                onChange={(value) =>
                  setContextValues((current) => ({ ...current, [field.contextKey]: value }))
                }
              />
            ))}
            {incidentKinds.has(selectedKind) && (
              <label className="flex flex-col gap-1 text-sm text-slate-300">
                Incident type
                <select
                  value={incidentType}
                  onChange={(event) => setIncidentType(event.target.value)}
                  className="rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-slate-100"
                >
                  {(incidentOptionsQuery.data ?? []).map((option) => (
                    <option key={option.key} value={option.key}>
                      {option.label}
                    </option>
                  ))}
                </select>
              </label>
            )}
          </div>
          <button
            type="button"
            onClick={() => contextMutation.mutate()}
            disabled={!canEvaluate || contextMutation.isPending}
            className="mt-4 inline-flex items-center gap-2 rounded-md bg-cyan-600 px-4 py-2 text-sm font-medium text-white hover:bg-cyan-500 disabled:opacity-50"
          >
            <GitBranch size={16} /> Resolve Applicability
          </button>
        </section>
      )}

      {step === 'facts' && (
        <section className="rounded-lg border border-slate-700 bg-slate-900/70 p-4">
          <h3 className="text-sm font-semibold uppercase tracking-wide text-slate-400">
            Evidence and fact states
          </h3>
          <ApplicabilitySummary results={applicability} />
          <div className="mt-4 space-y-3">
            {applicableRequirements.map((requirement) => (
              <label
                key={requirement.requirementKey}
                className="grid gap-2 rounded-lg border border-slate-800 bg-slate-950 p-3 md:grid-cols-[1fr_260px]"
              >
                <span>
                  <span className="block text-sm font-medium text-slate-100">
                    {requirement.label || requirement.factLabel}
                  </span>
                  <span className="mt-1 block text-xs text-slate-400">
                    {requirement.description || requirement.factLabel}
                  </span>
                </span>
                <select
                  value={factStates[requirement.requirementKey] ?? 'unknown'}
                  onChange={(event) =>
                    setFactStates((current) => ({
                      ...current,
                      [requirement.requirementKey]: event.target.value,
                    }))
                  }
                  className="rounded-md border border-slate-700 bg-slate-900 px-3 py-2 text-sm text-slate-100"
                >
                  {(evidenceStatesQuery.data ?? []).map((state) => (
                    <option key={state.key} value={state.key}>
                      {state.label}
                    </option>
                  ))}
                </select>
              </label>
            ))}
          </div>
          <button
            type="button"
            onClick={() => factsMutation.mutate()}
            disabled={!canEvaluate || factsMutation.isPending}
            className="mt-4 inline-flex items-center gap-2 rounded-md bg-cyan-600 px-4 py-2 text-sm font-medium text-white hover:bg-cyan-500 disabled:opacity-50"
          >
            <ClipboardCheck size={16} /> Review Situation
          </button>
        </section>
      )}

      {step === 'review' && (
        <section className="rounded-lg border border-slate-700 bg-slate-900/70 p-4">
          <h3 className="text-sm font-semibold uppercase tracking-wide text-slate-400">
            Review hypothetical situation
          </h3>
          <div className="mt-4 flex flex-wrap gap-2">
            {Object.entries(contextValues).map(([key, value]) => (
              <span key={key} className="rounded-full bg-slate-800 px-3 py-1 text-xs text-slate-200">
                {formatKey(key)}: {formatKey(value)}
              </span>
            ))}
          </div>
          <div className="mt-4 grid gap-3 md:grid-cols-3">
            <Metric label="Primary" value={applicability.filter((x) => x.applicabilityBand === 'primary').length} />
            <Metric label="Likely" value={applicability.filter((x) => x.applicabilityBand === 'likely').length} />
            <Metric label="Collapsed Edge Cases" value={applicability.filter((x) => x.edgeCase).length} />
          </div>
          <button
            type="button"
            onClick={() => evaluateMutation.mutate()}
            disabled={!canEvaluate || evaluateMutation.isPending}
            className="mt-4 inline-flex items-center gap-2 rounded-md bg-emerald-600 px-4 py-2 text-sm font-medium text-white hover:bg-emerald-500 disabled:opacity-50"
          >
            <Play size={16} /> Evaluate
          </button>
        </section>
      )}

      {step === 'result' && evaluation && (
        <section className="rounded-lg border border-slate-700 bg-slate-900/70 p-4">
          <div className="flex flex-col gap-3 sm:flex-row sm:items-start sm:justify-between">
            <div>
              <span
                className="stl-tone-badge inline-flex rounded-full border px-3 py-1 text-xs font-semibold uppercase"
                data-tone={resultTone(evaluation.result)}
              >
                {formatKey(evaluation.result)}
              </span>
              <h3 className="mt-3 text-xl font-semibold text-slate-100">{evaluation.summary}</h3>
            </div>
            <div className="grid grid-cols-3 gap-2 text-center">
              <Metric label="Pass" value={evaluation.passCount} />
              <Metric label="Fail" value={evaluation.failCount + evaluation.blockedCount} />
              <Metric label="Unknown" value={evaluation.unknownCount} />
            </div>
          </div>
          <div className="mt-5 space-y-3">
            {evaluation.details.map((detail) => (
              <div key={detail.detailId} className="rounded-lg border border-slate-800 bg-slate-950 p-3">
                <div className="flex flex-wrap items-center gap-2">
                  {detail.result === 'compliant' ? (
                    <CheckCircle2 size={16} className="text-emerald-300" />
                  ) : (
                    <AlertTriangle size={16} className="text-amber-300" />
                  )}
                  <span className="font-medium text-slate-100">{detail.auditQuestion}</span>
                  <span
                    className="stl-tone-badge rounded-full border px-2 py-0.5 text-xs"
                    data-tone={resultTone(detail.result)}
                  >
                    {formatKey(detail.result)}
                  </span>
                </div>
                <p className="mt-2 text-sm text-slate-300">{detail.explanation}</p>
                {detail.exceptionExemptionConsidered ? (
                  <div className="mt-3 border-l border-cyan-500/70 bg-cyan-950/20 px-3 py-2 text-xs text-slate-300">
                    <p className="font-medium text-cyan-100">Exception/exemption logic</p>
                    <div className="mt-2 grid gap-2 sm:grid-cols-2 lg:grid-cols-3">
                      <span>Normal result: {formatKey(detail.normalRuleResult || detail.resultBeforeException)}</span>
                      <span>
                        Legal relief:{' '}
                        {detail.exceptionExemptionLabel || detail.exceptionExemptionKey || formatKey(detail.simulatedState)}
                      </span>
                      <span>Type: {formatKey(detail.exceptionExemptionType || 'unknown')}</span>
                      <span>Applies: {detail.exceptionExemptionApplies ? 'yes' : 'no'}</span>
                      <span>Proof required: {detail.exceptionExemptionProofRequired ? 'yes' : 'no'}</span>
                      <span>Proof valid: {detail.exceptionExemptionProofValid ? 'yes' : 'no'}</span>
                      <span>After exception: {formatKey(detail.resultAfterException || detail.result)}</span>
                      <span>Final: {formatKey(detail.finalComplianceResult || detail.result)}</span>
                    </div>
                  </div>
                ) : null}
                <p className="mt-1 text-xs text-[var(--color-text-muted)]">{detail.suggestedNextAction}</p>
                <details className="mt-2 text-xs text-[var(--color-text-muted)]">
                  <summary className="cursor-pointer text-slate-400">Citation and fact details</summary>
                  <div className="mt-2 flex flex-wrap gap-2">
                    <span>{detail.citationKey || 'citation unavailable'}</span>
                    <span>{detail.factKey}</span>
                    <span>{detail.failureSeverity}</span>
                    {detail.overrideAllowed ? <span>override eligible</span> : <span>override blocked</span>}
                  </div>
                </details>
              </div>
            ))}
          </div>
          {evaluation.edgeCases.length > 0 && (
            <details className="mt-4 rounded-lg border border-slate-800 bg-slate-950 p-3">
              <summary className="cursor-pointer text-sm font-medium text-slate-200">
                Other contexts could add requirements
              </summary>
              <ul className="mt-3 space-y-1 text-sm text-slate-400">
                {evaluation.edgeCases.map((edgeCase) => (
                  <li key={edgeCase}>{edgeCase}</li>
                ))}
              </ul>
            </details>
          )}
        </section>
      )}
    </div>
  )
}

function StepRail({ step }: { step: WizardStep }) {
  const steps: Array<[WizardStep, string]> = [
    ['kind', 'Evaluate'],
    ['context', 'Context'],
    ['facts', 'States'],
    ['review', 'Review'],
    ['result', 'Result'],
  ]
  const activeIndex = steps.findIndex(([key]) => key === step)
  return (
    <div className="grid grid-cols-5 gap-2">
      {steps.map(([key, label], index) => (
        <div
          key={key}
          className={`h-2 rounded-full ${index <= activeIndex ? 'bg-[var(--color-accent)]' : 'bg-[var(--color-bg-surface-elevated)]'}`}
          aria-label={label}
        />
      ))}
    </div>
  )
}

function ContextControl({
  field,
  value,
  onChange,
}: {
  field: TheoreticalContextFieldResponse
  value: string
  onChange: (value: string) => void
}) {
  if (field.controlType === 'yes_no_unknown') {
    return (
      <div className="flex flex-col gap-2 text-sm text-slate-300">
        <span>{field.label}</span>
        <div className="grid grid-cols-3 overflow-hidden rounded-md border border-slate-700">
          {field.values.map((option) => (
            <button
              key={option.key}
              type="button"
              onClick={() => onChange(option.key)}
              className={`px-3 py-2 text-sm ${
                value === option.key ? 'bg-cyan-600 text-white' : 'bg-slate-950 text-slate-300'
              }`}
            >
              {option.label}
            </button>
          ))}
        </div>
      </div>
    )
  }

  return (
    <label className="flex flex-col gap-1 text-sm text-slate-300">
      {field.label}
      <select
        value={value}
        onChange={(event) => onChange(event.target.value)}
        className="rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-slate-100"
      >
        {field.values.map((option) => (
          <option key={option.key} value={option.key}>
            {option.label}
          </option>
        ))}
      </select>
    </label>
  )
}

function ApplicabilitySummary({ results }: { results: TheoreticalApplicabilityResultResponse[] }) {
  const primary = results.filter((item) => item.applicabilityBand === 'primary').length
  const likely = results.filter((item) => item.applicabilityBand === 'likely').length
  const edge = results.filter((item) => item.edgeCase).length
  return (
    <div className="mt-4 grid gap-3 md:grid-cols-3">
      <Metric label="Primary" value={primary} />
      <Metric label="Likely" value={likely} />
      <Metric label="Collapsed Edge Cases" value={edge} />
    </div>
  )
}

function Metric({ label, value }: { label: string; value: number }) {
  return (
    <div className="rounded-lg border border-slate-800 bg-slate-950 px-3 py-2">
      <div className="text-lg font-semibold text-slate-100">{value}</div>
      <div className="text-xs text-[var(--color-text-muted)]">{label}</div>
    </div>
  )
}

function defaultValue(field: TheoreticalContextFieldResponse) {
  return field.values.find((value) => value.key === 'unknown')?.key ?? field.values[0]?.key ?? ''
}

function formatKey(value: string) {
  return value.replaceAll('_', ' ')
}
