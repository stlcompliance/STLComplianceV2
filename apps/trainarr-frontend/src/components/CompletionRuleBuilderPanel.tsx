import { buildSemanticKey } from '@stl/shared-ui'
import { type FormEvent, useEffect, useMemo, useState } from 'react'
import type {
  TrainingCompletionRuleCatalogItemResponse,
  TrainingDefinitionCompletionRuleResponse,
  TrainingDefinitionResponse,
} from '../api/types'

interface CompletionRuleBuilderPanelProps {
  definitions: TrainingDefinitionResponse[]
  selectedDefinitionId: string | null
  catalog: TrainingCompletionRuleCatalogItemResponse[]
  rules: TrainingDefinitionCompletionRuleResponse[]
  isLoading: boolean
  canManage: boolean
  isSubmitting: boolean
  onSelectDefinition: (definitionId: string) => void
  onCreateRule: (request: {
    ruleKey: string
    ruleType: string
    label: string
    configJson: string
    sortOrder: number
  }) => Promise<void>
  onDeleteRule: (completionRuleId: string) => Promise<void>
}

function defaultConfigForType(
  catalog: TrainingCompletionRuleCatalogItemResponse[],
  ruleType: string,
): string {
  return catalog.find((item) => item.ruleType === ruleType)?.defaultConfigJson ?? '{}'
}

export function CompletionRuleBuilderPanel({
  definitions,
  selectedDefinitionId,
  catalog,
  rules,
  isLoading,
  canManage,
  isSubmitting,
  onSelectDefinition,
  onCreateRule,
  onDeleteRule,
}: CompletionRuleBuilderPanelProps) {
  const [label, setLabel] = useState('')
  const [ruleType, setRuleType] = useState('required_evaluator_pass')
  const [configJson, setConfigJson] = useState('{}')
  const [sortOrder, setSortOrder] = useState('0')

  const generatedRuleKey = useMemo(
    () =>
      buildSemanticKey({
        domain: 'train',
        kind: 'rule',
        title: label,
        existingKeys: rules.map((rule) => rule.ruleKey),
        maxLength: 64,
      }),
    [label, rules],
  )

  useEffect(() => {
    setSortOrder(String(rules.length))
  }, [rules.length, selectedDefinitionId])

  useEffect(() => {
    if (catalog.length > 0) {
      const initialType = catalog[0]?.ruleType ?? 'required_evaluator_pass'
      setRuleType(initialType)
      setConfigJson(defaultConfigForType(catalog, initialType))
    }
  }, [catalog])

  const handleRuleTypeChange = (value: string) => {
    setRuleType(value)
    setConfigJson(defaultConfigForType(catalog, value))
  }

  const handleSubmit = async (event: FormEvent) => {
    event.preventDefault()
    if (!selectedDefinitionId) {
      return
    }

    await onCreateRule({
      ruleKey: generatedRuleKey.trim(),
      ruleType,
      label: label.trim(),
      configJson,
      sortOrder: Number.parseInt(sortOrder, 10) || 0,
    })

    setLabel('')
    setSortOrder(String(rules.length + 1))
  }

  return (
    <section
      className="rounded-xl border border-slate-700 bg-slate-900/60 p-6"
      data-testid="completion-rule-builder-panel"
    >
      <header>
        <h2 className="text-sm font-medium text-slate-300">Completion rule builder</h2>
        <p className="mt-1 text-xs text-[var(--color-text-muted)]">
          docs/2.6 — define how TrainArr decides an assignment is complete. When no rules are configured, the legacy
          default requires passing evaluation plus trainee and trainer signoffs.
        </p>
      </header>

      <label htmlFor="completion-rule-definition" className="mt-4 block text-sm text-slate-300">
        Training definition
        <select
          id="completion-rule-definition"
          value={selectedDefinitionId ?? ''}
          onChange={(event) => onSelectDefinition(event.target.value)}
          className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
        >
          <option value="">Select definition…</option>
          {definitions.map((definition) => (
            <option key={definition.trainingDefinitionId} value={definition.trainingDefinitionId}>
              {definition.name}
            </option>
          ))}
        </select>
      </label>

      {selectedDefinitionId ? (
        <>
          {isLoading ? (
            <p className="mt-4 text-sm text-slate-400">Loading completion rules…</p>
          ) : rules.length === 0 ? (
            <p className="mt-4 text-sm text-slate-400">
              No custom completion rules yet — legacy defaults apply until you add at least one rule.
            </p>
          ) : (
            <ul className="mt-4 space-y-2">
              {rules.map((rule) => (
                <li
                  key={rule.completionRuleId}
                  className="flex flex-wrap items-center justify-between gap-2 rounded-lg border border-slate-800 bg-slate-950/40 px-3 py-2 text-sm"
                >
                  <div>
                    <p className="text-slate-100">
                      {rule.sortOrder}. {rule.label}{' '}
                      <span className="text-xs uppercase tracking-wide text-emerald-300">{rule.ruleType}</span>
                    </p>
                  </div>
                  {canManage ? (
                    <button
                      type="button"
                      className="text-xs text-rose-300 hover:text-rose-200"
                      onClick={() => onDeleteRule(rule.completionRuleId)}
                    >
                      Delete
                    </button>
                  ) : null}
                </li>
              ))}
            </ul>
          )}

          {canManage ? (
            <form className="mt-6 grid gap-4 md:grid-cols-2" onSubmit={handleSubmit}>
              <div className="md:col-span-2 text-xs text-slate-400">Completion rule reference is generated automatically from label.</div>
              <label htmlFor="completion-rule-label" className="block text-sm text-slate-300 md:col-span-2">
                Label
                <input
                  id="completion-rule-label"
                  value={label}
                  onChange={(event) => setLabel(event.target.value)}
                  className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
                  required
                  minLength={2}
                />
              </label>
              <label htmlFor="completion-rule-type" className="block text-sm text-slate-300">
                Rule type
                <select
                  id="completion-rule-type"
                  value={ruleType}
                  onChange={(event) => handleRuleTypeChange(event.target.value)}
                  className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
                >
                  {catalog.map((item) => (
                    <option key={item.ruleType} value={item.ruleType}>
                      {item.label}
                    </option>
                  ))}
                </select>
              </label>
              <label htmlFor="completion-rule-sort-order" className="block text-sm text-slate-300">
                Sort order
                <input
                  id="completion-rule-sort-order"
                  type="number"
                  value={sortOrder}
                  onChange={(event) => setSortOrder(event.target.value)}
                  className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
                />
              </label>
              <label htmlFor="completion-rule-config-json" className="block text-sm text-slate-300 md:col-span-2">
                Config JSON
                <textarea
                  id="completion-rule-config-json"
                  value={configJson}
                  onChange={(event) => setConfigJson(event.target.value)}
                  className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 font-mono text-xs text-slate-100"
                  required
                  rows={4}
                />
              </label>
              <div className="md:col-span-2">
                <button
                  type="submit"
                  disabled={isSubmitting || !generatedRuleKey}
                  className="rounded-md bg-emerald-600 px-4 py-2 text-sm font-medium text-white hover:bg-emerald-500 disabled:opacity-50"
                >
                  {isSubmitting ? 'Adding…' : 'Add completion rule'}
                </button>
              </div>
            </form>
          ) : null}
        </>
      ) : (
        <p className="mt-4 text-sm text-slate-400">Select a training definition to manage completion rules.</p>
      )}
    </section>
  )
}
