import { useMutation } from '@tanstack/react-query'
import { ApiErrorCallout, getErrorMessage } from '@stl/shared-ui'
import { useRef, useState } from 'react'
import type { ReactNode } from 'react'

import {
  getRulePackImport,
  getRulePackImportDiff,
  getRulePackImportTestResults,
  publishDraftRulePackImport,
  previewRulePackImport,
  rollbackRulePackImport,
  validateRulePackImport,
} from '../api/client'
import type {
  CsvImportResolutionOptions,
  RulePackImportDiffResponse,
  RulePackImportRollbackResponse,
  RulePackImportRunResponse,
  RulePackImportTestResultsResponse,
} from '../api/types'

interface RulePackImportWorkflowPanelProps {
  accessToken: string
  canManage: boolean
}

type ImportOptionsState = {
  regulatorySpineMode: string
  governingBodyKey: string
  governingBodyLabel: string
  governingBodyDescription: string
  jurisdictionKey: string
  jurisdictionLabel: string
  jurisdictionDescription: string
  programMappingsJson: string
}

const defaultOptions: ImportOptionsState = {
  regulatorySpineMode: 'strict',
  governingBodyKey: '',
  governingBodyLabel: '',
  governingBodyDescription: '',
  jurisdictionKey: '',
  jurisdictionLabel: '',
  jurisdictionDescription: '',
  programMappingsJson: '',
}

export function RulePackImportWorkflowPanel({ accessToken, canManage }: RulePackImportWorkflowPanelProps) {
  const fileInputRef = useRef<HTMLInputElement>(null)
  const [options, setOptions] = useState<ImportOptionsState>(defaultOptions)
  const [selectedFileNames, setSelectedFileNames] = useState<string[]>([])
  const [latestRun, setLatestRun] = useState<RulePackImportRunResponse | null>(null)
  const [latestDiff, setLatestDiff] = useState<RulePackImportDiffResponse | null>(null)
  const [latestTests, setLatestTests] = useState<RulePackImportTestResultsResponse | null>(null)
  const [latestRollback, setLatestRollback] = useState<RulePackImportRollbackResponse | null>(null)

  const runImport = async (submit: (files: FileList, options: CsvImportResolutionOptions) => Promise<RulePackImportRunResponse>) => {
    const files = fileInputRef.current?.files
    if (!files?.length) {
      throw new Error('Select at least one CSV or ZIP file first.')
    }

    const resolvedOptions = buildResolutionOptions(options)
    const run = await submit(files, resolvedOptions)
    const [nextRun, nextDiff, nextTests] = await Promise.all([
      getRulePackImport(accessToken, run.importId),
      getRulePackImportDiff(accessToken, run.importId),
      getRulePackImportTestResults(accessToken, run.importId),
    ])

    setLatestRun(nextRun)
    setLatestDiff(nextDiff)
    setLatestTests(nextTests)
    setLatestRollback(null)
    return nextRun
  }

  const previewMutation = useMutation({
    mutationFn: () => runImport((files, resolvedOptions) => previewRulePackImport(accessToken, files, resolvedOptions)),
  })

  const validateMutation = useMutation({
    mutationFn: () => runImport((files, resolvedOptions) => validateRulePackImport(accessToken, files, resolvedOptions)),
  })

  const publishMutation = useMutation({
    mutationFn: () =>
      runImport((files, resolvedOptions) => publishDraftRulePackImport(accessToken, files, resolvedOptions)),
  })

  const rollbackMutation = useMutation({
    mutationFn: async () => {
      if (!latestRun) {
        throw new Error('Run a preview or publish a draft first.')
      }

      const rollback = await rollbackRulePackImport(accessToken, latestRun.importId)
      setLatestRollback(rollback)
      return rollback
    },
  })

  return (
    <section
      data-testid="rule-pack-import-workflow-panel"
      className="space-y-5 rounded-lg border border-slate-700 bg-slate-900/80 p-5"
    >
      <header>
        <p className="text-xs font-semibold uppercase tracking-wide text-emerald-300">Rule pack imports</p>
        <h2 className="mt-1 text-xl font-semibold text-slate-50">Preview, validate, publish, and rollback</h2>
        <p className="mt-1 text-sm text-slate-400">
          Exercise the first-class rule-pack import lifecycle routes that sit alongside the CSV bundle tools.
        </p>
      </header>

      {canManage ? (
        <div className="space-y-4 rounded-lg border border-slate-800 bg-slate-950/40 p-4">
          <div className="grid gap-3 lg:grid-cols-2">
            <label className="text-sm text-slate-300">
              Bundle files
              <input
                ref={fileInputRef}
                type="file"
                accept=".csv,.zip"
                multiple
                onChange={(event) =>
                  setSelectedFileNames(Array.from(event.currentTarget.files ?? []).map((file) => file.name))
                }
                className="mt-1 block w-full text-sm text-slate-300 file:mr-3 file:rounded-md file:border-0 file:bg-slate-700 file:px-3 file:py-1.5 file:text-sm file:text-slate-100"
              />
            </label>
            <label className="text-sm text-slate-300">
              Registry resolution
              <select
                value={options.regulatorySpineMode}
                onChange={(event) => setOptions((current) => ({ ...current, regulatorySpineMode: event.target.value }))}
                className="mt-1 block w-full rounded-md border border-slate-700 bg-slate-900 px-3 py-2 text-sm text-slate-100"
              >
                <option value="strict">Strict match</option>
                <option value="create_missing">Create missing</option>
                <option value="map_existing">Map existing</option>
                <option value="create_or_map">Create or map</option>
              </select>
            </label>
          </div>

          <div className="grid gap-3 md:grid-cols-2">
            <TextInput label="Governing body key" value={options.governingBodyKey} onChange={(value) => setOptions((current) => ({ ...current, governingBodyKey: value }))} />
            <TextInput label="Governing body label" value={options.governingBodyLabel} onChange={(value) => setOptions((current) => ({ ...current, governingBodyLabel: value }))} />
            <TextInput label="Governing body description" value={options.governingBodyDescription} onChange={(value) => setOptions((current) => ({ ...current, governingBodyDescription: value }))} />
            <TextInput label="Jurisdiction key" value={options.jurisdictionKey} onChange={(value) => setOptions((current) => ({ ...current, jurisdictionKey: value }))} />
            <TextInput label="Jurisdiction label" value={options.jurisdictionLabel} onChange={(value) => setOptions((current) => ({ ...current, jurisdictionLabel: value }))} />
            <TextInput label="Jurisdiction description" value={options.jurisdictionDescription} onChange={(value) => setOptions((current) => ({ ...current, jurisdictionDescription: value }))} />
          </div>

          <label className="block text-sm text-slate-300">
            Program mappings JSON
            <textarea
              value={options.programMappingsJson}
              onChange={(event) => setOptions((current) => ({ ...current, programMappingsJson: event.target.value }))}
              placeholder='{"external_fmcsa":"fmcsa_safety"}'
              rows={3}
              className="mt-1 block w-full rounded-md border border-slate-700 bg-slate-900 px-3 py-2 font-mono text-sm text-slate-100"
            />
          </label>

          <div className="flex flex-wrap gap-2">
            <ActionButton
              label={previewMutation.isPending ? 'Previewing…' : 'Preview import'}
              disabled={previewMutation.isPending}
              onClick={() => previewMutation.mutate()}
            />
            <ActionButton
              label={validateMutation.isPending ? 'Validating…' : 'Validate import'}
              disabled={validateMutation.isPending}
              onClick={() => validateMutation.mutate()}
            />
            <ActionButton
              label={publishMutation.isPending ? 'Publishing…' : 'Publish draft'}
              disabled={publishMutation.isPending}
              onClick={() => publishMutation.mutate()}
            />
            <ActionButton
              label={rollbackMutation.isPending ? 'Rolling back…' : 'Rollback latest import'}
              disabled={rollbackMutation.isPending || !latestRun}
              onClick={() => rollbackMutation.mutate()}
            />
          </div>

          {selectedFileNames.length > 0 ? (
            <p className="text-xs text-[var(--color-text-muted)]">
              Selected files: <span className="font-mono text-slate-300">{selectedFileNames.join(', ')}</span>
            </p>
          ) : null}
        </div>
      ) : (
        <p className="text-sm text-[var(--color-text-muted)]">Rule-pack import preview and publish require compliance admin access.</p>
      )}

      {latestRun ? (
        <div className="space-y-4 rounded-lg border border-slate-800 bg-slate-950/40 p-4">
          <div className="grid gap-3 md:grid-cols-4">
            <Metric label="Status" value={latestRun.status} />
            <Metric label="Dry run" value={latestRun.dryRun ? 'yes' : 'no'} tone={latestRun.dryRun ? 'warn' : 'ok'} />
            <Metric label="Files" value={String(latestRun.result.files.length)} />
            <Metric label="Issues" value={String(latestRun.result.issues.length)} tone={latestRun.result.issues.length ? 'warn' : 'ok'} />
          </div>
          <p className="text-xs text-[var(--color-text-muted)]">
            Import ID <span className="font-mono text-slate-300">{latestRun.importId}</span> created at{' '}
            <span className="font-mono text-slate-300">{formatDateTime(latestRun.createdAt)}</span>
          </p>
          <div className="grid gap-3 md:grid-cols-5">
            <Metric label="Changed files" value={String(latestDiff?.filesWithChanges ?? 0)} />
            <Metric label="Created" value={String(latestDiff?.createdCount ?? 0)} />
            <Metric label="Updated" value={String(latestDiff?.updatedCount ?? 0)} />
            <Metric label="Deactivated" value={String(latestDiff?.deactivatedCount ?? 0)} />
            <Metric label="Test issues" value={String(latestTests?.issueCount ?? 0)} tone={latestTests?.passed ? 'ok' : 'warn'} />
          </div>
          <div className="grid gap-4 lg:grid-cols-2">
            <SummaryCard title="CSV file summaries">
              <ul className="space-y-1 text-sm text-slate-300">
                {latestRun.result.files.map((file) => (
                  <li key={file.fileName} className="flex flex-wrap items-center justify-between gap-2 border-b border-slate-800 pb-1 last:border-b-0">
                    <span className="font-mono text-slate-100">{file.fileName}</span>
                    <span className="text-slate-400">
                      {file.rowCount} rows, {file.created} created, {file.updated} updated, {file.deactivated} deactivated
                    </span>
                  </li>
                ))}
              </ul>
            </SummaryCard>
            <SummaryCard title="Issues">
              {latestRun.result.issues.length > 0 ? (
                <ul className="space-y-1 text-sm text-amber-200">
                  {latestRun.result.issues.map((issue, index) => (
                    <li key={`${issue.fileName}-${issue.lineNumber}-${index}`}>
                      <span className="font-mono text-slate-100">{issue.fileName}</span>
                      {issue.lineNumber ? <span className="text-[var(--color-text-muted)]">:{issue.lineNumber}</span> : null} - {issue.message}
                    </li>
                  ))}
                </ul>
              ) : (
                <p className="text-sm text-slate-400">No import issues reported.</p>
              )}
            </SummaryCard>
          </div>
          <SummaryCard title="Test results">
            <p className="text-sm text-slate-300">
              {latestTests?.passed ? 'Passed' : 'Not passed'} - {latestTests?.issueCount ?? 0} issue(s)
            </p>
          </SummaryCard>
        </div>
      ) : null}

      {latestRollback ? (
        <div className="rounded-lg border border-slate-800 bg-slate-950/40 p-4">
          <p className="text-sm text-slate-300">
            Rollback status: <span className="font-semibold text-slate-100">{latestRollback.status}</span> for import{' '}
            <span className="font-mono text-slate-300">{latestRollback.importId}</span>
          </p>
        </div>
      ) : null}

      {previewMutation.isError ? (
        <ApiErrorCallout title="Preview failed" message={getErrorMessage(previewMutation.error, 'Preview failed.')} />
      ) : null}
      {validateMutation.isError ? (
        <ApiErrorCallout title="Validation failed" message={getErrorMessage(validateMutation.error, 'Validation failed.')} />
      ) : null}
      {publishMutation.isError ? (
        <ApiErrorCallout title="Publish failed" message={getErrorMessage(publishMutation.error, 'Publish failed.')} />
      ) : null}
      {rollbackMutation.isError ? (
        <ApiErrorCallout title="Rollback failed" message={getErrorMessage(rollbackMutation.error, 'Rollback failed.')} />
      ) : null}
    </section>
  )
}

function buildResolutionOptions(options: ImportOptionsState): CsvImportResolutionOptions {
  const programMappings = parseProgramMappings(options.programMappingsJson)
  return {
    regulatorySpineMode: options.regulatorySpineMode,
    governingBodyKey: options.governingBodyKey,
    governingBodyLabel: options.governingBodyLabel,
    governingBodyDescription: options.governingBodyDescription,
    jurisdictionKey: options.jurisdictionKey,
    jurisdictionLabel: options.jurisdictionLabel,
    jurisdictionDescription: options.jurisdictionDescription,
    programMappings,
  }
}

function parseProgramMappings(raw: string): Record<string, string> | undefined {
  if (!raw.trim()) {
    return undefined
  }

  const parsed = JSON.parse(raw) as unknown
  if (!parsed || typeof parsed !== 'object' || Array.isArray(parsed)) {
    throw new Error('Program mappings JSON must be an object.')
  }

  const entries = Object.entries(parsed as Record<string, unknown>).map(([key, value]) => {
    if (typeof value !== 'string') {
      throw new Error('Program mappings JSON values must be strings.')
    }

    return [key, value] as const
  })

  return Object.fromEntries(entries)
}

function formatDateTime(value: string) {
  return new Date(value).toLocaleString()
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
  const id = `rule-pack-import-${label.toLowerCase().replaceAll(' ', '-')}`
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
}: {
  label: string
  onClick: () => void
  disabled?: boolean
}) {
  return (
    <button
      type="button"
      onClick={onClick}
      disabled={disabled}
      className="rounded-md bg-emerald-600 px-4 py-2 text-sm font-medium text-white hover:bg-emerald-500 disabled:opacity-50"
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

function SummaryCard({ title, children }: { title: string; children: ReactNode }) {
  return (
    <div className="rounded-lg border border-slate-800 bg-slate-900/60 p-4">
      <h3 className="text-sm font-medium text-slate-200">{title}</h3>
      <div className="mt-2">{children}</div>
    </div>
  )
}
