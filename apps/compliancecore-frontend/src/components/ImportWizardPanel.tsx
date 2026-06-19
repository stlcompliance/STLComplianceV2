import { useMutation } from '@tanstack/react-query'
import { ApiErrorCallout, getErrorMessage } from '@stl/shared-ui'
import {
  AlertTriangle,
  CheckCircle2,
  FileCheck2,
  FileSearch,
  GitMerge,
  ListChecks,
  Plus,
  Search,
  ShieldAlert,
  SkipForward,
  Upload,
  XCircle,
} from 'lucide-react'
import { useRef, useState } from 'react'
import type { ReactNode } from 'react'

import {
  addImportWizardSupportingEvidence,
  bulkConfirmImportWizardMappings,
  commitImportWizard,
  confirmImportWizardItem,
  createImportWizardExceptionExemption,
  createImportSession,
  createImportWizardTarget,
  forceMapImportWizardItem,
  generateImportMappingCandidates,
  getImportCommitPreview,
  getImportWizardSummary,
  getNextImportWizardItem,
  mapImportWizardAsExceptionProof,
  mapImportWizardAsExemptionProof,
  mapImportWizardAsNormalEvidence,
  mapImportWizardAsSpecialPermitApprovalProof,
  markImportWizardExceptionNotApplicable,
  markImportWizardNoDocumentRequired,
  markImportWizardNotApplicable,
  markImportWizardReferenceOnly,
  parseImportSession,
  rejectImportWizardItem,
  selectImportWizardExceptionExemption,
  selectImportWizardEvidenceOption,
  selectImportWizardTarget,
  skipImportWizardItem,
  uploadImportSessionBundle,
  validateImportSession,
} from '../api/client'
import type {
  CommitPreviewResponse,
  ImportCompletionReportResponse,
  ImportSessionResponse,
  ImportValidationResultsResponse,
  MappingCandidateResponse,
  WizardItemResponse,
  WizardSummaryResponse,
} from '../api/types'

interface ImportWizardPanelProps {
  accessToken: string
  canManage: boolean
}

type CandidateFilters = {
  confidenceBand: string
  targetKind: string
  sourceFile: string
  riskOnly: boolean
}

const emptyFilters: CandidateFilters = {
  confidenceBand: 'all',
  targetKind: 'all',
  sourceFile: 'all',
  riskOnly: false,
}

export function ImportWizardPanel({ accessToken, canManage }: ImportWizardPanelProps) {
  const fileInputRef = useRef<HTMLInputElement>(null)
  const [session, setSession] = useState<ImportSessionResponse | null>(null)
  const [validation, setValidation] = useState<ImportValidationResultsResponse | null>(null)
  const [candidates, setCandidates] = useState<MappingCandidateResponse[]>([])
  const [summary, setSummary] = useState<WizardSummaryResponse | null>(null)
  const [item, setItem] = useState<WizardItemResponse | null>(null)
  const [preview, setPreview] = useState<CommitPreviewResponse | null>(null)
  const [report, setReport] = useState<ImportCompletionReportResponse | null>(null)
  const [filters, setFilters] = useState<CandidateFilters>(emptyFilters)
  const [targetKind, setTargetKind] = useState('existing_document_type')
  const [targetKey, setTargetKey] = useState('')
  const [targetLabel, setTargetLabel] = useState('')
  const [overrideReason, setOverrideReason] = useState('')
  const [overrideAck, setOverrideAck] = useState(false)

  const refreshWizard = async (sessionId: string) => {
    const [nextSummary, nextItem] = await Promise.all([
      getImportWizardSummary(accessToken, sessionId),
      getNextImportWizardItem(accessToken, sessionId),
    ])
    setSummary(nextSummary)
    setItem(nextItem)
  }

  const uploadMutation = useMutation({
    mutationFn: async (files: FileList) => {
      const created = await createImportSession(accessToken)
      const uploaded = await uploadImportSessionBundle(accessToken, created.importSessionId, files)
      const parsed = await parseImportSession(accessToken, created.importSessionId)
      const validated = await validateImportSession(accessToken, created.importSessionId)
      return { session: parsed.session ?? uploaded.session, validation: validated }
    },
    onSuccess: ({ session: nextSession, validation: nextValidation }) => {
      setSession(nextSession)
      setValidation(nextValidation)
      setCandidates([])
      setSummary(null)
      setItem(null)
      setPreview(null)
      setReport(null)
    },
  })

  const candidateMutation = useMutation({
    mutationFn: async () => {
      if (!session) {
        throw new Error('Create and validate an import session first.')
      }
      return generateImportMappingCandidates(accessToken, session.importSessionId)
    },
    onSuccess: async (nextCandidates) => {
      setCandidates(nextCandidates)
      if (session) {
        await refreshWizard(session.importSessionId)
      }
    },
  })

  const decisionMutation = useMutation({
    mutationFn: async (action: () => Promise<unknown>) => action(),
    onSuccess: async () => {
      if (session) {
        await refreshWizard(session.importSessionId)
        setPreview(null)
      }
    },
  })

  const previewMutation = useMutation({
    mutationFn: async () => {
      if (!session) {
        throw new Error('No import session is active.')
      }
      return getImportCommitPreview(accessToken, session.importSessionId)
    },
    onSuccess: setPreview,
  })

  const commitMutation = useMutation({
    mutationFn: async () => {
      if (!session) {
        throw new Error('No import session is active.')
      }
      return commitImportWizard(accessToken, session.importSessionId)
    },
    onSuccess: setReport,
  })

  const filteredCandidates = candidates.filter((candidate) => {
    if (filters.confidenceBand !== 'all' && candidate.confidenceBand !== filters.confidenceBand) {
      return false
    }
    if (filters.targetKind !== 'all' && candidate.targetKind !== filters.targetKind) {
      return false
    }
    if (filters.sourceFile !== 'all' && candidate.stagedSourceFile !== filters.sourceFile) {
      return false
    }
    if (filters.riskOnly && candidate.riskFlags.length === 0) {
      return false
    }
    return true
  })

  const sourceFiles = Array.from(new Set(candidates.map((candidate) => candidate.stagedSourceFile))).sort()
  const targetKinds = Array.from(new Set(candidates.map((candidate) => candidate.targetKind))).sort()
  const currentItemId = item?.itemId ?? ''
  const canAct = Boolean(session && item && canManage)
  const exceptionTargetKind = legalReliefTargetKindOr(targetKind, 'exception_exemption')

  return (
    <section data-testid="import-wizard-panel" className="space-y-5 rounded-lg border border-slate-700 bg-slate-900/80 p-5">
      <header className="flex flex-wrap items-start justify-between gap-4">
        <div>
          <p className="text-xs font-semibold uppercase tracking-wide text-emerald-300">Import Wizard</p>
          <h2 className="mt-1 text-xl font-semibold text-slate-50">Staged Import and Evidence Mapping Wizard</h2>
        </div>
        {session ? (
          <div className="text-right text-xs text-slate-400">
            <p className="font-mono text-slate-300">{session.importSessionId.slice(0, 8)}</p>
            <p>
              {session.status} / {session.validationStatus} / {session.mappingStatus}
            </p>
          </div>
        ) : null}
      </header>

      <div className="border-t border-slate-800 pt-4">
        <div className="flex flex-wrap items-end gap-3">
          <label htmlFor="import-wizard-files" className="min-w-72 flex-1 text-sm text-slate-300">
            Compliance Core CSV bundle
            <input
              id="import-wizard-files"
              ref={fileInputRef}
              type="file"
              accept=".csv,.zip"
              multiple
              className="mt-1 block w-full text-sm text-slate-300 file:mr-3 file:rounded-md file:border-0 file:bg-slate-700 file:px-3 file:py-1.5 file:text-sm file:text-slate-100"
            />
          </label>
          <IconButton
            icon={<Upload size={16} />}
            label={uploadMutation.isPending ? 'Staging...' : 'Upload and validate'}
            disabled={!canManage || uploadMutation.isPending}
            onClick={() => {
              const files = fileInputRef.current?.files
              if (files?.length) {
                uploadMutation.mutate(files)
              }
            }}
          />
          <IconButton
            icon={<FileSearch size={16} />}
            label={candidateMutation.isPending ? 'Generating...' : 'Generate candidates'}
            disabled={!session || validation?.validationStatus !== 'passed' || candidateMutation.isPending}
            onClick={() => candidateMutation.mutate()}
          />
        </div>
        {uploadMutation.isError || candidateMutation.isError ? (
          <ApiErrorCallout
            className="mt-2"
            title="Import wizard action failed"
            message={getErrorMessage(uploadMutation.error || candidateMutation.error, 'Import wizard action failed.')}
          />
        ) : null}
      </div>

      {validation ? (
        <div className="grid gap-3 border-t border-slate-800 pt-4 md:grid-cols-4">
          <Metric label="Validation" value={validation.validationStatus} />
          <Metric label="Rows" value={`${validation.validRows}/${validation.totalRows} valid`} />
          <Metric label="Invalid" value={String(validation.invalidRows)} tone={validation.invalidRows > 0 ? 'warn' : 'ok'} />
          <Metric label="Files" value={String(validation.files.length)} />
        </div>
      ) : null}

      {summary ? (
        <div className="grid gap-3 border-t border-slate-800 pt-4 md:grid-cols-5">
          <Metric label="Pending" value={String(summary.pendingItems)} />
          <Metric label="Confirmed" value={String(summary.confirmedItems)} tone="ok" />
          <Metric label="Changed" value={String(summary.changedItems)} />
          <Metric label="Risk flagged" value={String(summary.riskFlaggedItems)} tone={summary.riskFlaggedItems ? 'warn' : 'ok'} />
          <Metric label="Rejected" value={String(summary.rejectedItems)} />
        </div>
      ) : null}

      {candidates.length > 0 ? (
        <div className="space-y-3 border-t border-slate-800 pt-4">
          <div className="flex flex-wrap gap-3">
            <FilterSelect
              label="Confidence"
              value={filters.confidenceBand}
              options={['all', 'exact', 'high', 'medium', 'low', 'no_match']}
              onChange={(value) => setFilters((current) => ({ ...current, confidenceBand: value }))}
            />
            <FilterSelect
              label="Target"
              value={filters.targetKind}
              options={['all', ...targetKinds]}
              onChange={(value) => setFilters((current) => ({ ...current, targetKind: value }))}
            />
            <FilterSelect
              label="Source file"
              value={filters.sourceFile}
              options={['all', ...sourceFiles]}
              onChange={(value) => setFilters((current) => ({ ...current, sourceFile: value }))}
            />
            <label className="flex items-center gap-2 pt-6 text-sm text-slate-300">
              <input
                type="checkbox"
                checked={filters.riskOnly}
                onChange={(event) => setFilters((current) => ({ ...current, riskOnly: event.target.checked }))}
              />
              Risk only
            </label>
          </div>

          <div className="flex flex-wrap gap-2">
            <IconButton
              icon={<CheckCircle2 size={16} />}
              label="Confirm exact no-risk"
              disabled={!session || !canManage}
              onClick={() =>
                session &&
                decisionMutation.mutate(() => bulkConfirmImportWizardMappings(accessToken, session.importSessionId, 'exact'))
              }
            />
            <IconButton
              icon={<ListChecks size={16} />}
              label="Confirm high no-risk"
              disabled={!session || !canManage}
              onClick={() =>
                session &&
                decisionMutation.mutate(() =>
                  bulkConfirmImportWizardMappings(accessToken, session.importSessionId, 'high', true),
                )
              }
            />
          </div>

          <div className="max-h-56 overflow-auto border border-slate-800">
            <table className="w-full min-w-[760px] text-left text-xs text-slate-300">
              <thead className="bg-slate-950 text-slate-400">
                <tr>
                  <th className="px-3 py-2 font-medium">Requirement</th>
                  <th className="px-3 py-2 font-medium">Evidence path</th>
                  <th className="px-3 py-2 font-medium">Target</th>
                  <th className="px-3 py-2 font-medium">Confidence</th>
                  <th className="px-3 py-2 font-medium">Risk</th>
                </tr>
              </thead>
              <tbody>
                {filteredCandidates.map((candidate) => (
                  <tr key={candidate.mappingCandidateId} className="border-t border-slate-800">
                    <td className="px-3 py-2 font-mono">{candidate.sourceKey}</td>
                    <td className="px-3 py-2">{candidate.evidenceOptionLabel}</td>
                    <td className="px-3 py-2">{candidate.targetLabel || candidate.targetKey}</td>
                    <td className="px-3 py-2">
                      {candidate.confidenceBand} ({Math.round(candidate.confidenceScore * 100)}%)
                    </td>
                    <td className="px-3 py-2">{candidate.riskFlags.length ? candidate.riskFlags.length : 'none'}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      ) : null}

      {item ? (
        <div className="space-y-4 border-t border-slate-800 pt-4">
          <div className="flex flex-wrap items-center justify-between gap-3">
            <div>
              <p className="text-sm font-medium text-slate-100">{item.confirmationPrompt}</p>
              <p className="mt-1 font-mono text-xs text-slate-400">{item.requirementKey}</p>
            </div>
            <span className={bandClass(item.confidenceBand)}>
              {item.confidenceBand} / {Math.round(item.confidenceScore * 100)}%
            </span>
          </div>

          <div className="grid gap-4 lg:grid-cols-2">
            <div className="space-y-3">
              <InfoGrid
                rows={[
                  ['Evidence key', item.evidenceKey],
                  ['Rule pack', item.rulePackKey],
                  ['Citation', item.citationKey],
                  ['Domain', item.complianceKeyOrDomain],
                  ['Evidence kind', item.requiredEvidenceKind],
                  ['Logic', item.evidenceLogic],
                  ['Source', `${item.sourceProduct} / ${item.sourceEntity}`],
                  ['Field or record', item.sourceFieldOrRecordType],
                ]}
              />
              <p className="text-sm text-slate-300">{item.auditQuestion}</p>
            </div>
            <div className="space-y-3">
              <InfoGrid
                rows={[
                  ['Suggested path', item.suggestedEvidencePath.evidenceOptionLabel],
                  ['Suggested target', item.suggestedTarget],
                  ['Target kind', item.targetKind],
                  ['Will do', item.whatWillHappenIfConfirmed],
                  ['Proof question', item.exceptionProofPrompt],
                  ['Override', item.overrideAllowed ? 'allowed' : 'blocked'],
                  ['Remediation', item.remediationRequired ? 'required' : 'not required'],
                ]}
              />
            </div>
          </div>

          <ReasonList title="Match reasons" items={item.matchReasons} />
          <ReasonList title="Risk flags" items={item.riskFlags} tone="warn" emptyLabel="No risk flags" />

          {item.otherAcceptableEvidencePaths.length > 0 ? (
            <div className="space-y-2">
              <h3 className="text-sm font-medium text-slate-200">Other acceptable evidence paths</h3>
              <div className="flex flex-wrap gap-2">
                {item.otherAcceptableEvidencePaths.map((option) => (
                  <button
                    key={option.evidenceOptionKey}
                    type="button"
                    disabled={!canAct || decisionMutation.isPending}
                    onClick={() =>
                      session &&
                      decisionMutation.mutate(() =>
                        selectImportWizardEvidenceOption(
                          accessToken,
                          session.importSessionId,
                          currentItemId,
                          option.evidenceOptionKey,
                        ),
                      )
                    }
                    className="rounded-md border border-slate-700 px-3 py-2 text-sm text-slate-200 hover:border-emerald-400 disabled:opacity-50"
                  >
                    {option.evidenceOptionLabel}
                  </button>
                ))}
              </div>
            </div>
          ) : null}

          <div className="grid gap-3 lg:grid-cols-3">
            <label className="text-sm text-slate-300">
              Target kind
              <input
                value={targetKind}
                onChange={(event) => setTargetKind(event.target.value)}
                className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-slate-100"
              />
            </label>
            <label className="text-sm text-slate-300">
              Target key
              <input
                value={targetKey}
                onChange={(event) => setTargetKey(event.target.value)}
                className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-slate-100"
              />
            </label>
            <label className="text-sm text-slate-300">
              Target label
              <input
                value={targetLabel}
                onChange={(event) => setTargetLabel(event.target.value)}
                className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-slate-100"
              />
            </label>
          </div>

          <div className="flex flex-wrap gap-2">
            <IconButton
              icon={<CheckCircle2 size={16} />}
              label="Confirm"
              disabled={!canAct || decisionMutation.isPending}
              onClick={() =>
                session &&
                decisionMutation.mutate(() => confirmImportWizardItem(accessToken, session.importSessionId, currentItemId))
              }
            />
            <IconButton
              icon={<Search size={16} />}
              label="Select target"
              disabled={!canAct || !targetKey || decisionMutation.isPending}
              onClick={() =>
                session &&
                decisionMutation.mutate(() =>
                  selectImportWizardTarget(accessToken, session.importSessionId, currentItemId, {
                    targetKind,
                    targetId: targetKey,
                    targetKey,
                    targetLabel: targetLabel || targetKey,
                  }),
                )
              }
            />
            <IconButton
              icon={<Plus size={16} />}
              label="Create target"
              disabled={!canAct || !targetKey || decisionMutation.isPending}
              onClick={() =>
                session &&
                decisionMutation.mutate(() =>
                  createImportWizardTarget(accessToken, session.importSessionId, currentItemId, {
                    targetKind,
                    payload: { stableKey: targetKey, label: targetLabel || targetKey },
                  }),
                )
              }
            />
            <IconButton
              icon={<FileCheck2 size={16} />}
              label="No document"
              disabled={!canAct || decisionMutation.isPending}
              onClick={() =>
                session &&
                decisionMutation.mutate(() =>
                  markImportWizardNoDocumentRequired(accessToken, session.importSessionId, currentItemId),
                )
              }
            />
            <IconButton
              icon={<GitMerge size={16} />}
              label="Supporting evidence"
              disabled={!canAct || !targetKey || decisionMutation.isPending}
              onClick={() =>
                session &&
                decisionMutation.mutate(() =>
                  addImportWizardSupportingEvidence(accessToken, session.importSessionId, currentItemId, {
                    targetKind,
                    targetKey,
                    targetLabel: targetLabel || targetKey,
                  }),
                )
              }
            />
            <IconButton
              icon={<FileCheck2 size={16} />}
              label="Normal evidence"
              disabled={!canAct || decisionMutation.isPending}
              onClick={() =>
                session &&
                decisionMutation.mutate(() =>
                  mapImportWizardAsNormalEvidence(accessToken, session.importSessionId, currentItemId),
                )
              }
            />
            <IconButton
              icon={<FileSearch size={16} />}
              label="Exception proof"
              disabled={!canAct || !targetKey || decisionMutation.isPending}
              onClick={() =>
                session &&
                decisionMutation.mutate(() =>
                  mapImportWizardAsExceptionProof(accessToken, session.importSessionId, currentItemId, {
                    exceptionExemptionKey: targetKey,
                    targetKind: exceptionTargetKind,
                    targetKey,
                    targetLabel: targetLabel || targetKey,
                  }),
                )
              }
            />
            <IconButton
              icon={<FileSearch size={16} />}
              label="Exemption proof"
              disabled={!canAct || !targetKey || decisionMutation.isPending}
              onClick={() =>
                session &&
                decisionMutation.mutate(() =>
                  mapImportWizardAsExemptionProof(accessToken, session.importSessionId, currentItemId, {
                    exceptionExemptionKey: targetKey,
                    targetKind: exceptionTargetKind,
                    targetKey,
                    targetLabel: targetLabel || targetKey,
                  }),
                )
              }
            />
            <IconButton
              icon={<FileCheck2 size={16} />}
              label="Permit proof"
              disabled={!canAct || !targetKey || decisionMutation.isPending}
              onClick={() =>
                session &&
                decisionMutation.mutate(() =>
                  mapImportWizardAsSpecialPermitApprovalProof(accessToken, session.importSessionId, currentItemId, {
                    exceptionExemptionKey: targetKey,
                    targetKind: legalReliefTargetKindOr(targetKind, 'special_permit'),
                    targetKey,
                    targetLabel: targetLabel || targetKey,
                  }),
                )
              }
            />
            <IconButton
              icon={<Plus size={16} />}
              label="Create exception"
              disabled={!canAct || !targetKey || decisionMutation.isPending}
              onClick={() =>
                session &&
                decisionMutation.mutate(() =>
                  createImportWizardExceptionExemption(accessToken, session.importSessionId, currentItemId, {
                    exceptionExemptionKey: targetKey,
                    targetKind: exceptionTargetKind,
                    targetKey,
                    targetLabel: targetLabel || targetKey,
                    payload: {
                      key: targetKey,
                      label: targetLabel || targetKey,
                      type: exceptionTypeForTargetKind(exceptionTargetKind),
                      effectType: exceptionEffectForTargetKind(exceptionTargetKind),
                    },
                  }),
                )
              }
            />
            <IconButton
              icon={<Search size={16} />}
              label="Select exception"
              disabled={!canAct || !targetKey || decisionMutation.isPending}
              onClick={() =>
                session &&
                decisionMutation.mutate(() =>
                  selectImportWizardExceptionExemption(accessToken, session.importSessionId, currentItemId, {
                    exceptionExemptionKey: targetKey,
                    targetKind: exceptionTargetKind,
                    targetKey,
                    targetLabel: targetLabel || targetKey,
                  }),
                )
              }
            />
            <IconButton
              icon={<FileCheck2 size={16} />}
              label="Exception N/A"
              disabled={!canAct || decisionMutation.isPending}
              onClick={() =>
                session &&
                decisionMutation.mutate(() =>
                  markImportWizardExceptionNotApplicable(accessToken, session.importSessionId, currentItemId),
                )
              }
            />
            <IconButton
              icon={<SkipForward size={16} />}
              label="Skip"
              disabled={!canAct || decisionMutation.isPending}
              onClick={() =>
                session && decisionMutation.mutate(() => skipImportWizardItem(accessToken, session.importSessionId, currentItemId))
              }
            />
            <IconButton
              icon={<XCircle size={16} />}
              label="Reject"
              disabled={!canAct || decisionMutation.isPending}
              onClick={() =>
                session &&
                decisionMutation.mutate(() => rejectImportWizardItem(accessToken, session.importSessionId, currentItemId))
              }
              tone="danger"
            />
            <IconButton
              icon={<FileCheck2 size={16} />}
              label="N/A"
              disabled={!canAct || decisionMutation.isPending}
              onClick={() =>
                session &&
                decisionMutation.mutate(() => markImportWizardNotApplicable(accessToken, session.importSessionId, currentItemId))
              }
            />
            <IconButton
              icon={<FileSearch size={16} />}
              label="Reference only"
              disabled={!canAct || decisionMutation.isPending}
              onClick={() =>
                session &&
                decisionMutation.mutate(() => markImportWizardReferenceOnly(accessToken, session.importSessionId, currentItemId))
              }
            />
          </div>

          <div className="space-y-2 border-l-2 border-amber-400 pl-3">
            <div className="grid gap-3 lg:grid-cols-[1fr_auto]">
              <label className="text-sm text-slate-300">
                Override reason
                <input
                  value={overrideReason}
                  onChange={(event) => setOverrideReason(event.target.value)}
                  className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-slate-100"
                />
              </label>
              <label className="flex items-center gap-2 pt-6 text-sm text-slate-300">
                <input
                  type="checkbox"
                  checked={overrideAck}
                  onChange={(event) => setOverrideAck(event.target.checked)}
                />
                Risk acknowledged
              </label>
            </div>
            <IconButton
              icon={<ShieldAlert size={16} />}
              label="Force map"
              disabled={!canAct || !targetKey || !overrideReason || !overrideAck || decisionMutation.isPending}
              onClick={() =>
                session &&
                decisionMutation.mutate(() =>
                  forceMapImportWizardItem(accessToken, session.importSessionId, currentItemId, {
                    targetKind,
                    targetId: targetKey,
                    targetKey,
                    targetLabel: targetLabel || targetKey,
                    overrideReason,
                    riskAcknowledged: overrideAck,
                  }),
                )
              }
              tone="warn"
            />
          </div>

          <details className="text-sm text-slate-400">
            <summary className="cursor-pointer text-slate-300">Advanced source and target details</summary>
            <div className="mt-3 grid gap-4 lg:grid-cols-2">
              <KeyValueTable rows={item.sourceRow} />
              <KeyValueTable rows={item.targetRecord} />
            </div>
          </details>
        </div>
      ) : candidates.length > 0 ? (
        <p className="border-t border-slate-800 pt-4 text-sm text-emerald-300">Mapping review queue is clear.</p>
      ) : null}

      {decisionMutation.isError ? (
        <ApiErrorCallout
          title="Mapping decision failed"
          message={getErrorMessage(decisionMutation.error, 'Mapping decision failed.')}
        />
      ) : null}

      {session ? (
        <div className="flex flex-wrap gap-2 border-t border-slate-800 pt-4">
          <IconButton
            icon={<ListChecks size={16} />}
            label={previewMutation.isPending ? 'Previewing...' : 'Commit preview'}
            disabled={previewMutation.isPending}
            onClick={() => previewMutation.mutate()}
          />
          <IconButton
            icon={<CheckCircle2 size={16} />}
            label={commitMutation.isPending ? 'Committing...' : 'Commit import'}
            disabled={!preview || preview.unresolvedBlockers.length > 0 || commitMutation.isPending || !canManage}
            onClick={() => commitMutation.mutate()}
          />
        </div>
      ) : null}

      {preview ? (
        <div className="space-y-3 border-t border-slate-800 pt-4">
          <div className="grid gap-3 md:grid-cols-6">
            <Metric label="Decisions" value={String(preview.totalDecisions)} />
            <Metric label="Evidence refs" value={String(preview.evidenceReferencesToCreateOrUpdate)} />
            <Metric label="Exception proof" value={String(preview.exceptionProofMappings)} />
            <Metric label="Relief records" value={String(preview.exceptionExemptionRecordsToCreateOrUpdate)} />
            <Metric label="Overrides" value={String(preview.overridesUsed)} tone={preview.overridesUsed ? 'warn' : 'ok'} />
            <Metric label="Blockers" value={String(preview.unresolvedBlockers.length)} tone={preview.unresolvedBlockers.length ? 'warn' : 'ok'} />
          </div>
          {preview.unresolvedBlockers.length > 0 ? <ReasonList title="Unresolved blockers" items={preview.unresolvedBlockers} tone="warn" /> : null}
          <div className="max-h-48 overflow-auto border border-slate-800">
            <table className="w-full min-w-[720px] text-left text-xs text-slate-300">
              <thead className="bg-slate-950 text-slate-400">
                <tr>
                  <th className="px-3 py-2 font-medium">Action</th>
                  <th className="px-3 py-2 font-medium">Source</th>
                  <th className="px-3 py-2 font-medium">Target</th>
                  <th className="px-3 py-2 font-medium">Purpose</th>
                  <th className="px-3 py-2 font-medium">Legal relief</th>
                  <th className="px-3 py-2 font-medium">Override</th>
                </tr>
              </thead>
              <tbody>
                {preview.actions.map((action, index) => (
                  <tr key={`${action.sourceKey}-${index}`} className="border-t border-slate-800">
                    <td className="px-3 py-2">{action.action}</td>
                    <td className="px-3 py-2 font-mono">{action.sourceKey}</td>
                    <td className="px-3 py-2">
                      {action.targetKind}: {action.targetKey}
                    </td>
                    <td className="px-3 py-2">{formatPurpose(action.evidenceMappingPurpose)}</td>
                    <td className="px-3 py-2">{action.exceptionExemptionKey || 'none'}</td>
                    <td className="px-3 py-2">{action.overrideUsed ? 'yes' : 'no'}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      ) : null}

      {report ? (
        <div className="grid gap-3 border-t border-slate-800 pt-4 md:grid-cols-4">
          <Metric label="Status" value={report.status} tone="ok" />
          <Metric label="Created" value={String(report.createdCount)} />
          <Metric label="Updated" value={String(report.updatedCount)} />
          <Metric label="Audit log" value={report.auditLogReference} />
        </div>
      ) : null}
    </section>
  )
}

function IconButton({
  icon,
  label,
  onClick,
  disabled,
  tone = 'primary',
}: {
  icon: ReactNode
  label: string
  onClick: () => void
  disabled?: boolean
  tone?: 'primary' | 'danger' | 'warn'
}) {
  const color =
    tone === 'danger'
      ? 'bg-red-600 hover:bg-red-500'
      : tone === 'warn'
        ? 'bg-amber-600 hover:bg-amber-500'
        : 'bg-emerald-600 hover:bg-emerald-500'
  return (
    <button
      type="button"
      onClick={onClick}
      disabled={disabled}
      className={`inline-flex items-center gap-2 rounded-md px-3 py-2 text-sm font-medium text-white ${color} disabled:cursor-not-allowed disabled:opacity-50`}
    >
      {icon}
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

function FilterSelect({
  label,
  value,
  options,
  onChange,
}: {
  label: string
  value: string
  options: string[]
  onChange: (value: string) => void
}) {
  return (
    <label className="text-sm text-slate-300">
      {label}
      <select
        value={value}
        onChange={(event) => onChange(event.target.value)}
        className="mt-1 block rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-slate-100"
      >
        {options.map((option) => (
          <option key={option} value={option}>
            {option}
          </option>
        ))}
      </select>
    </label>
  )
}

function InfoGrid({ rows }: { rows: Array<[string, string | boolean | number]> }) {
  return (
    <dl className="grid gap-2 text-sm sm:grid-cols-2">
      {rows.map(([label, value]) => (
        <div key={label}>
          <dt className="text-xs text-[var(--color-text-muted)]">{label}</dt>
          <dd className="mt-0.5 break-words text-slate-200">{String(value || 'none')}</dd>
        </div>
      ))}
    </dl>
  )
}

function ReasonList({
  title,
  items,
  tone,
  emptyLabel = 'No entries',
}: {
  title: string
  items: string[]
  tone?: 'warn'
  emptyLabel?: string
}) {
  const icon = tone === 'warn' ? <AlertTriangle size={14} /> : <CheckCircle2 size={14} />
  return (
    <div>
      <h3 className="text-sm font-medium text-slate-200">{title}</h3>
      {items.length > 0 ? (
        <ul className="mt-2 space-y-1 text-sm text-slate-300">
          {items.map((item) => (
            <li key={item} className="flex gap-2">
              <span className={tone === 'warn' ? 'mt-0.5 text-amber-300' : 'mt-0.5 text-emerald-300'}>{icon}</span>
              <span>{item}</span>
            </li>
          ))}
        </ul>
      ) : (
        <p className="mt-2 text-sm text-[var(--color-text-muted)]">{emptyLabel}</p>
      )}
    </div>
  )
}

function KeyValueTable({ rows }: { rows: Record<string, string> }) {
  return (
    <table className="w-full text-left text-xs text-slate-300">
      <tbody>
        {Object.entries(rows).map(([key, value]) => (
          <tr key={key} className="border-t border-slate-800">
            <th className="w-1/3 px-2 py-1.5 font-medium text-[var(--color-text-muted)]">{key}</th>
            <td className="px-2 py-1.5">{value || 'none'}</td>
          </tr>
        ))}
      </tbody>
    </table>
  )
}

function bandClass(band: string) {
  const base = 'rounded-md px-2 py-1 text-xs font-semibold'
  if (band === 'exact' || band === 'high') {
    return `${base} bg-emerald-500/15 text-emerald-200`
  }
  if (band === 'medium') {
    return `${base} bg-sky-500/15 text-sky-200`
  }
  if (band === 'low') {
    return `${base} bg-amber-500/15 text-amber-200`
  }
  return `${base} bg-slate-700 text-slate-200`
}

const legalReliefTargetKinds = new Set([
  'exception_exemption',
  'waiver',
  'variance',
  'special_permit',
  'approval',
  'alternate_compliance_path',
  'conditional_exclusion',
])

function legalReliefTargetKindOr(value: string, fallback: string) {
  return legalReliefTargetKinds.has(value) ? value : fallback
}

function exceptionTypeForTargetKind(targetKind: string) {
  if (targetKind === 'waiver') return 'waiver'
  if (targetKind === 'variance') return 'variance'
  if (targetKind === 'special_permit') return 'special_permit'
  if (targetKind === 'approval') return 'approval'
  if (targetKind === 'alternate_compliance_path') return 'alternate_compliance_path'
  if (targetKind === 'conditional_exclusion') return 'conditional_exclusion'
  return 'regulatory_exemption'
}

function exceptionEffectForTargetKind(targetKind: string) {
  if (targetKind === 'conditional_exclusion') return 'makes_requirement_not_applicable'
  if (targetKind === 'alternate_compliance_path') return 'allows_alternate_evidence'
  if (targetKind === 'special_permit' || targetKind === 'approval') return 'authorizes_otherwise_blocked_action'
  return 'allows_alternate_evidence'
}

function formatPurpose(value: string) {
  return value ? value.replaceAll('_', ' ') : 'normal requirement'
}
