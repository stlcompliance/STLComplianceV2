import { useMutation, useQuery } from '@tanstack/react-query'
import { ApiErrorCallout, getErrorMessage } from '@stl/shared-ui'
import { useEffect, useState } from 'react'

import {
  createRuleTestCase,
  deleteRuleTestCase,
  listRuleTestCases,
  patchRuleTestCase,
  runRuleTestCase,
} from '../api/client'
import type {
  CreateRuleTestCaseRequest,
  PatchRuleTestCaseRequest,
  RulePackResponse,
  RuleTestCaseRunResponse,
} from '../api/types'

interface RuleTestCasesPanelProps {
  accessToken: string
  rulePacks: RulePackResponse[]
  selectedRulePackId: string
  onSelectRulePack: (rulePackId: string) => void
  canManage: boolean
}

const defaultFactsJson = '{\n  "driver_license_valid": true\n}'

export function RuleTestCasesPanel({
  accessToken,
  rulePacks,
  selectedRulePackId,
  onSelectRulePack,
  canManage,
}: RuleTestCasesPanelProps) {
  const [selectedTestCaseId, setSelectedTestCaseId] = useState('')
  const [ruleKey, setRuleKey] = useState('license_valid')
  const [testKey, setTestKey] = useState('license_valid_happy_path')
  const [label, setLabel] = useState('Valid driver license')
  const [description, setDescription] = useState('Driver license is valid and the rule should pass.')
  const [expectedResult, setExpectedResult] = useState('pass')
  const [factsJson, setFactsJson] = useState(defaultFactsJson)
  const [runResult, setRunResult] = useState<RuleTestCaseRunResponse | null>(null)

  const testCasesQuery = useQuery({
    queryKey: ['compliancecore-rule-test-cases', accessToken, selectedRulePackId],
    queryFn: () => listRuleTestCases(accessToken, selectedRulePackId),
    enabled: Boolean(selectedRulePackId),
  })

  const selectedCase = testCasesQuery.data?.find((testCase) => testCase.ruleTestCaseId === selectedTestCaseId) ?? null

  useEffect(() => {
    if (!testCasesQuery.data?.length) {
      setSelectedTestCaseId('')
      return
    }

    if (!selectedTestCaseId || !testCasesQuery.data.some((testCase) => testCase.ruleTestCaseId === selectedTestCaseId)) {
      setSelectedTestCaseId(testCasesQuery.data[0].ruleTestCaseId)
    }
  }, [selectedTestCaseId, testCasesQuery.data])

  useEffect(() => {
    if (!selectedCase) {
      return
    }

    setRuleKey(selectedCase.ruleKey)
    setTestKey(selectedCase.testKey)
    setLabel(selectedCase.label)
    setDescription(selectedCase.description)
    setExpectedResult(selectedCase.expectedResult)
    setFactsJson(JSON.stringify(selectedCase.facts, null, 2))
  }, [selectedCase])

  const createMutation = useMutation({
    mutationFn: () =>
      createRuleTestCase(accessToken, selectedRulePackId, {
        ruleKey,
        testKey,
        label,
        description,
        expectedResult,
        facts: parseFactsJson(factsJson),
      } satisfies CreateRuleTestCaseRequest),
    onSuccess: (created) => {
      setSelectedTestCaseId(created.ruleTestCaseId)
      setRunResult(null)
      testCasesQuery.refetch()
    },
  })

  const patchMutation = useMutation({
    mutationFn: () => {
      if (!selectedCase) {
        throw new Error('Select a test case first.')
      }

      const payload: PatchRuleTestCaseRequest = {
        ruleKey,
        testKey,
        label,
        description,
        expectedResult,
        facts: parseFactsJson(factsJson),
      }
      return patchRuleTestCase(accessToken, selectedRulePackId, selectedCase.ruleTestCaseId, payload)
    },
    onSuccess: (updated) => {
      setSelectedTestCaseId(updated.ruleTestCaseId)
      setRunResult(null)
      testCasesQuery.refetch()
    },
  })

  const deleteMutation = useMutation({
    mutationFn: async () => {
      if (!selectedCase) {
        throw new Error('Select a test case first.')
      }

      await deleteRuleTestCase(accessToken, selectedRulePackId, selectedCase.ruleTestCaseId)
    },
    onSuccess: async () => {
      setSelectedTestCaseId('')
      setRunResult(null)
      await testCasesQuery.refetch()
    },
  })

  const runMutation = useMutation({
    mutationFn: async () => {
      if (!selectedCase) {
        throw new Error('Select a test case first.')
      }

      return runRuleTestCase(accessToken, selectedRulePackId, selectedCase.ruleTestCaseId)
    },
    onSuccess: setRunResult,
  })

  const selectedPack = rulePacks.find((pack) => pack.rulePackId === selectedRulePackId) ?? null

  return (
    <section data-testid="rule-test-cases-panel" className="space-y-5 rounded-lg border border-slate-700 bg-slate-900/80 p-5">
      <header className="flex flex-wrap items-start justify-between gap-4">
        <div>
          <p className="text-xs font-semibold uppercase tracking-wide text-emerald-300">Rule test cases</p>
          <h2 className="mt-1 text-xl font-semibold text-slate-50">Saved regression cases for rule packs</h2>
          <p className="mt-1 text-sm text-slate-400">
            Create, run, update, and delete deterministic rule evaluations from the registry workspace.
          </p>
        </div>
        {selectedPack ? (
          <div className="text-right text-xs text-slate-400">
            <p className="font-mono text-slate-300">{selectedPack.packKey}</p>
            <p>
              v{selectedPack.versionNumber} / {selectedPack.status}
            </p>
          </div>
        ) : null}
      </header>

      <label className="block text-sm text-slate-300">
        Rule pack
        <select
          value={selectedRulePackId}
          onChange={(event) => onSelectRulePack(event.target.value)}
          className="mt-1 block w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
        >
          <option value="">Select a rule pack…</option>
          {rulePacks.map((pack) => (
            <option key={pack.rulePackId} value={pack.rulePackId}>
              {pack.packKey} (v{pack.versionNumber}, {pack.status})
            </option>
          ))}
        </select>
      </label>

      {selectedRulePackId ? (
        <div className="grid gap-4 lg:grid-cols-[minmax(0,1fr)_minmax(0,1.2fr)]">
          <section className="rounded-lg border border-slate-800 bg-slate-950/40 p-4">
            <div className="flex items-center justify-between gap-3">
              <h3 className="text-sm font-medium text-slate-200">Saved cases</h3>
              <span className="text-xs text-[var(--color-text-muted)]">{testCasesQuery.data?.length ?? 0} total</span>
            </div>
            {testCasesQuery.data?.length ? (
              <ul className="mt-3 space-y-2">
                {testCasesQuery.data.map((testCase) => (
                  <li key={testCase.ruleTestCaseId}>
                    <button
                      type="button"
                      onClick={() => setSelectedTestCaseId(testCase.ruleTestCaseId)}
                      className={`w-full rounded-lg border px-3 py-2 text-left text-sm transition ${
                        testCase.ruleTestCaseId === selectedTestCaseId
                          ? 'border-emerald-500 bg-emerald-500/10 text-emerald-100'
                          : 'border-slate-800 bg-slate-900/60 text-slate-200 hover:border-slate-700'
                      }`}
                    >
                      <div className="flex items-center justify-between gap-3">
                        <span className="font-medium">{testCase.label}</span>
                        <span className="font-mono text-xs text-slate-400">{testCase.expectedResult}</span>
                      </div>
                      <div className="mt-1 flex items-center justify-between gap-3 text-xs text-[var(--color-text-muted)]">
                        <span className="font-mono">{testCase.ruleKey}</span>
                        <span className="font-mono">{testCase.testKey}</span>
                      </div>
                    </button>
                  </li>
                ))}
              </ul>
            ) : (
              <p className="mt-3 text-sm text-[var(--color-text-muted)]">No saved test cases yet.</p>
            )}
          </section>

          <section className="space-y-4 rounded-lg border border-slate-800 bg-slate-950/40 p-4">
            <h3 className="text-sm font-medium text-slate-200">{selectedCase ? 'Edit test case' : 'Create test case'}</h3>
            <div className="grid gap-3 md:grid-cols-2">
              <TextInput label="Rule key" value={ruleKey} onChange={setRuleKey} />
              <TextInput label="Test key" value={testKey} onChange={setTestKey} />
              <TextInput label="Label" value={label} onChange={setLabel} />
              <TextInput label="Expected result" value={expectedResult} onChange={setExpectedResult} />
            </div>
            <label className="block text-sm text-slate-300">
              Description
              <textarea
                value={description}
                onChange={(event) => setDescription(event.target.value)}
                rows={3}
                className="mt-1 block w-full rounded-md border border-slate-700 bg-slate-900 px-3 py-2 text-sm text-slate-100"
              />
            </label>
            <label className="block text-sm text-slate-300">
              Facts JSON
              <textarea
                value={factsJson}
                onChange={(event) => setFactsJson(event.target.value)}
                rows={6}
                className="mt-1 block w-full rounded-md border border-slate-700 bg-slate-900 px-3 py-2 font-mono text-sm text-slate-100"
              />
            </label>

            <div className="flex flex-wrap gap-2">
              <ActionButton
                label={createMutation.isPending ? 'Creating…' : 'Create case'}
                disabled={!canManage || createMutation.isPending || !selectedRulePackId}
                onClick={() => createMutation.mutate()}
              />
              <ActionButton
                label={patchMutation.isPending ? 'Saving…' : 'Save changes'}
                disabled={!canManage || patchMutation.isPending || !selectedCase}
                onClick={() => patchMutation.mutate()}
              />
              <ActionButton
                label={runMutation.isPending ? 'Running…' : 'Run test'}
                disabled={runMutation.isPending || !selectedCase}
                onClick={() => runMutation.mutate()}
              />
              <ActionButton
                label={deleteMutation.isPending ? 'Deleting…' : 'Delete case'}
                disabled={!canManage || deleteMutation.isPending || !selectedCase}
                onClick={() => deleteMutation.mutate()}
                tone="danger"
              />
            </div>
          </section>
        </div>
      ) : (
        <p className="text-sm text-[var(--color-text-muted)]">Select a rule pack to view and manage saved test cases.</p>
      )}

      {runResult ? (
        <div className="rounded-lg border border-slate-800 bg-slate-950/40 p-4">
          <div className="grid gap-3 md:grid-cols-4">
            <Metric label="Passed" value={runResult.passed ? 'yes' : 'no'} tone={runResult.passed ? 'ok' : 'warn'} />
            <Metric label="Expected" value={runResult.expectedResult} />
            <Metric label="Actual" value={runResult.actualResult} />
            <Metric label="Evaluated" value={formatDate(runResult.evaluatedAt)} />
          </div>
          <p className="mt-3 text-sm text-slate-300">{runResult.message}</p>
          <div className="mt-3 space-y-3 rounded-lg border border-slate-800 bg-slate-900/60 p-3">
            <h3 className="text-xs font-semibold uppercase tracking-wide text-slate-400">Evaluation details</h3>
            <dl className="grid gap-2 md:grid-cols-2">
              {buildEvaluationSummaryEntries(runResult).map((entry) => (
                <div key={entry.label} className="rounded-md border border-slate-800 bg-slate-950/60 p-2">
                  <dt className="text-[11px] uppercase tracking-wide text-[var(--color-text-muted)]">{entry.label}</dt>
                  <dd className="mt-1 break-words text-sm text-slate-100">{entry.value}</dd>
                </div>
              ))}
            </dl>
            <details className="rounded-md border border-slate-800 bg-slate-950/60 p-2">
              <summary className="cursor-pointer text-[11px] font-semibold uppercase tracking-wide text-slate-100">
                Advanced technical details
              </summary>
              <pre className="mt-2 overflow-auto rounded-md border border-slate-800 bg-slate-900 p-3 text-xs text-slate-200">
                {JSON.stringify(runResult.evaluation, null, 2)}
              </pre>
            </details>
          </div>
        </div>
      ) : null}

      {testCasesQuery.isError ? (
        <ApiErrorCallout
          title="Failed to load rule test cases"
          message={getErrorMessage(testCasesQuery.error, 'Failed to load rule test cases.')}
        />
      ) : null}
      {createMutation.isError ? (
        <ApiErrorCallout title="Create failed" message={getErrorMessage(createMutation.error, 'Create failed.')} />
      ) : null}
      {patchMutation.isError ? (
        <ApiErrorCallout title="Save failed" message={getErrorMessage(patchMutation.error, 'Save failed.')} />
      ) : null}
      {runMutation.isError ? (
        <ApiErrorCallout title="Run failed" message={getErrorMessage(runMutation.error, 'Run failed.')} />
      ) : null}
      {deleteMutation.isError ? (
        <ApiErrorCallout title="Delete failed" message={getErrorMessage(deleteMutation.error, 'Delete failed.')} />
      ) : null}
    </section>
  )
}

function parseFactsJson(raw: string): Record<string, boolean> {
  if (!raw.trim()) {
    return {}
  }

  const parsed = JSON.parse(raw) as unknown
  if (!parsed || typeof parsed !== 'object' || Array.isArray(parsed)) {
    throw new Error('Facts JSON must be an object.')
  }

  return Object.fromEntries(
    Object.entries(parsed as Record<string, unknown>).map(([key, value]) => {
      if (typeof value !== 'boolean') {
        throw new Error('Facts JSON values must be booleans.')
      }

      return [key, value] as const
    }),
  )
}

function formatDate(value: string) {
  return new Date(value).toLocaleString()
}

function formatEvaluationFlags(evaluation: RuleTestCaseRunResponse['evaluation']): string {
  const flags = [
    evaluation.nonWaivable ? 'non-waivable' : null,
    evaluation.remediationRequired ? 'remediation required' : null,
    evaluation.reviewRequired ? 'review required' : null,
  ].filter((value): value is string => Boolean(value))

  return flags.length > 0 ? flags.join(' · ') : 'Standard'
}

function buildEvaluationSummaryEntries(runResult: RuleTestCaseRunResponse) {
  return [
    { label: 'Rule key', value: runResult.evaluation.ruleKey },
    { label: 'Rule label', value: runResult.evaluation.label },
    { label: 'Rule result', value: runResult.evaluation.result },
    { label: 'Evaluation message', value: runResult.evaluation.message },
    { label: 'Flags', value: formatEvaluationFlags(runResult.evaluation) },
  ]
}

function TextInput({
  label,
  value,
  onChange,
}: {
  label: string
  value: string
  onChange: (value: string) => void
}) {
  const id = `rule-test-case-${label.toLowerCase().replaceAll(' ', '-')}`
  return (
    <label htmlFor={id} className="block text-sm text-slate-300">
      {label}
      <input
        id={id}
        type="text"
        value={value}
        onChange={(event) => onChange(event.target.value)}
        className="mt-1 block w-full rounded-md border border-slate-700 bg-slate-900 px-3 py-2 text-sm text-slate-100"
      />
    </label>
  )
}

function ActionButton({
  label,
  onClick,
  disabled,
  tone = 'primary',
}: {
  label: string
  onClick: () => void
  disabled?: boolean
  tone?: 'primary' | 'danger'
}) {
  const className =
    tone === 'danger'
      ? 'bg-red-600 hover:bg-red-500'
      : 'bg-emerald-600 hover:bg-emerald-500'

  return (
    <button
      type="button"
      onClick={onClick}
      disabled={disabled}
      className={`rounded-md px-4 py-2 text-sm font-medium text-white disabled:opacity-50 ${className}`}
    >
      {label}
    </button>
  )
}

function Metric({ label, value, tone }: { label: string; value: string; tone?: 'ok' | 'warn' }) {
  const color = tone === 'warn' ? 'text-amber-200' : tone === 'ok' ? 'text-emerald-200' : 'text-slate-100'
  return (
    <div className="border-l border-slate-700 pl-3">
      <p className="text-xs text-[var(--color-text-muted)]">{label}</p>
      <p className={`mt-1 truncate text-sm font-semibold ${color}`}>{value}</p>
    </div>
  )
}
