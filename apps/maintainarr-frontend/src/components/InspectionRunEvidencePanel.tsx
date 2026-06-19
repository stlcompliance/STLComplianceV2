import { ControlledSelect } from '@stl/shared-ui'

import type { InspectionRunEvidenceResponse } from '../api/types'
import { INSPECTION_EVIDENCE_TYPE_OPTIONS } from './formOptions'

interface InspectionRunEvidencePanelProps {
  inspectionRunId: string | null
  runStatus: string | null
  evidence: InspectionRunEvidenceResponse[]
  checklistItemId: string
  checklistOptions: Array<{ value: string; label: string }>
  canUpload: boolean
  evidenceTypeKey: string
  evidenceNotes: string
  selectedFileName: string | null
  onChecklistItemIdChange: (value: string) => void
  onEvidenceTypeKeyChange: (value: string) => void
  onEvidenceNotesChange: (value: string) => void
  onSelectFile: (file: File | null) => void
  onUploadEvidence: () => void
  isUploadingEvidence: boolean
  isLoading: boolean
}

function formatBytes(sizeBytes: number): string {
  if (sizeBytes < 1024) return `${sizeBytes} B`
  if (sizeBytes < 1024 * 1024) return `${(sizeBytes / 1024).toFixed(1)} KB`
  return `${(sizeBytes / (1024 * 1024)).toFixed(1)} MB`
}

export function InspectionRunEvidencePanel({
  inspectionRunId,
  runStatus,
  evidence,
  checklistItemId,
  checklistOptions,
  canUpload,
  evidenceTypeKey,
  evidenceNotes,
  selectedFileName,
  onChecklistItemIdChange,
  onEvidenceTypeKeyChange,
  onEvidenceNotesChange,
  onSelectFile,
  onUploadEvidence,
  isUploadingEvidence,
  isLoading,
}: InspectionRunEvidencePanelProps) {
  if (!inspectionRunId) {
    return null
  }

  const editable = runStatus === 'in_progress'

  return (
    <div
      className="mt-6 rounded-lg border border-slate-800 bg-slate-950/40 p-4"
      data-testid="inspection-run-evidence-panel"
    >
      <header className="mb-3">
        <h3 className="text-sm font-semibold text-white">Inspection evidence</h3>
        <p className="mt-1 text-xs text-[var(--color-text-muted)]">
          Attach photos or documents during an in-progress run to support failed-item proof.
        </p>
      </header>

      {isLoading ? (
        <p className="text-sm text-slate-400" data-testid="inspection-run-evidence-loading">
          Loading evidence…
        </p>
      ) : (
        <>
          {evidence.length === 0 ? (
            <p className="text-sm text-slate-400">No evidence uploaded for this run.</p>
          ) : (
            <ul className="space-y-2">
              {evidence.map((item) => (
                <li
                  key={item.evidenceId}
                  className="rounded border border-slate-700 bg-slate-950/40 px-3 py-2 text-sm"
                  data-testid="inspection-run-evidence-item"
                >
                  <div className="font-medium text-slate-100">{item.fileName}</div>
                  <div className="text-xs text-slate-400">
                    {item.evidenceTypeKey}
                    {item.checklistItemId ? ' · checklist item' : ''} · {formatBytes(item.sizeBytes)} ·{' '}
                    {new Date(item.createdAt).toLocaleString()}
                  </div>
                  {item.notes ? <p className="mt-1 text-xs text-[var(--color-text-muted)]">{item.notes}</p> : null}
                </li>
              ))}
            </ul>
          )}

          {canUpload && editable ? (
            <div className="mt-3 space-y-2" data-testid="inspection-run-evidence-upload">
              <ControlledSelect
                label="Checklist item (optional)"
                value={checklistItemId}
                onChange={onChecklistItemIdChange}
                options={checklistOptions}
                emptyLabel="Run-level evidence"
                testId="inspection-evidence-checklist-item"
                className="w-full rounded border border-slate-600 bg-slate-950 px-2 py-1 text-sm text-white"
              />
              <ControlledSelect
                label="Evidence type"
                value={evidenceTypeKey}
                onChange={onEvidenceTypeKeyChange}
                options={INSPECTION_EVIDENCE_TYPE_OPTIONS}
                emptyLabel="Select evidence type…"
                testId="inspection-evidence-type"
                className="w-full rounded border border-slate-600 bg-slate-950 px-2 py-1 text-sm text-white"
              />
              <label className="block text-sm text-slate-300" htmlFor="inspection-evidence-file">
                Inspection evidence file
                <input
                  id="inspection-evidence-file"
                  type="file"
                  className="mt-1 block w-full text-sm text-slate-300"
                  data-testid="inspection-evidence-file"
                  onChange={(event) => onSelectFile(event.target.files?.[0] ?? null)}
                />
              </label>
              {selectedFileName ? <p className="text-xs text-[var(--color-text-muted)]">{selectedFileName}</p> : null}
              <label className="block text-sm text-slate-300" htmlFor="inspection-evidence-notes">
                Notes (optional)
                <input
                  id="inspection-evidence-notes"
                  className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-2 py-1 text-sm text-white"
                  value={evidenceNotes}
                  onChange={(event) => onEvidenceNotesChange(event.target.value)}
                />
              </label>
              <button
                type="button"
                className="rounded bg-emerald-800 px-3 py-1 text-sm text-white hover:bg-emerald-700 disabled:opacity-50"
                data-testid="inspection-evidence-upload-button"
                disabled={!selectedFileName || !evidenceTypeKey.trim() || isUploadingEvidence}
                onClick={onUploadEvidence}
              >
                {isUploadingEvidence ? 'Uploading…' : 'Upload evidence'}
              </button>
            </div>
          ) : (
            !editable && (
              <p className="mt-2 text-xs text-amber-300">
                Evidence capture is available only while the inspection run is in progress.
              </p>
            )
          )}
        </>
      )}
    </div>
  )
}
