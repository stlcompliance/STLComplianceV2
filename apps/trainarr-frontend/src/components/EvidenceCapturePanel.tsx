import { ControlledSelect, type PickerOption } from '@stl/shared-ui'

import type { TrainingAssignmentDetailResponse, TrainingEvidenceResponse } from '../api/types'
import { EVIDENCE_TYPE_OPTIONS } from './formOptions'

interface EvidenceCapturePanelProps {
  assignment: TrainingAssignmentDetailResponse | null
  evidence: TrainingEvidenceResponse[]
  evidenceTypeKey: string
  notes: string
  selectedFileName: string | null
  onEvidenceTypeKeyChange: (value: string) => void
  onNotesChange: (value: string) => void
  onSelectFile: (file: File | null) => void
  onUploadEvidence: () => void
  isUploading: boolean
  canUpload: boolean
}

function formatBytes(sizeBytes: number): string {
  if (sizeBytes < 1024) return `${sizeBytes} B`
  if (sizeBytes < 1024 * 1024) return `${(sizeBytes / 1024).toFixed(1)} KB`
  return `${(sizeBytes / (1024 * 1024)).toFixed(1)} MB`
}

export function EvidenceCapturePanel({
  assignment,
  evidence,
  evidenceTypeKey,
  notes,
  selectedFileName,
  onEvidenceTypeKeyChange,
  onNotesChange,
  onSelectFile,
  onUploadEvidence,
  isUploading,
  canUpload,
}: EvidenceCapturePanelProps) {
  if (!assignment) {
    return (
      <section className="rounded-xl border border-slate-700 bg-slate-900/60 p-4">
        <h2 className="text-sm font-semibold uppercase tracking-wide text-slate-400">Evidence</h2>
        <p className="mt-3 text-sm text-slate-400">Select an assignment to capture evidence.</p>
      </section>
    )
  }

  const assignmentOpen = assignment.status === 'assigned' || assignment.status === 'in_progress'
  const evidenceTypeOptionsByKey = new Map<string, PickerOption>()
  for (const option of EVIDENCE_TYPE_OPTIONS) {
    evidenceTypeOptionsByKey.set(option.value, option)
  }
  for (const item of evidence) {
    if (!evidenceTypeOptionsByKey.has(item.evidenceTypeKey)) {
      evidenceTypeOptionsByKey.set(item.evidenceTypeKey, {
        value: item.evidenceTypeKey,
        label: item.evidenceTypeKey,
      })
    }
  }
  if (evidenceTypeKey.trim() && !evidenceTypeOptionsByKey.has(evidenceTypeKey.trim())) {
    evidenceTypeOptionsByKey.set(evidenceTypeKey.trim(), {
      value: evidenceTypeKey.trim(),
      label: evidenceTypeKey.trim(),
    })
  }
  const evidenceTypeOptions = [...evidenceTypeOptionsByKey.values()].sort((left, right) =>
    left.label.localeCompare(right.label),
  )

  return (
    <section className="rounded-xl border border-slate-700 bg-slate-900/60 p-4">
      <h2 className="text-sm font-semibold uppercase tracking-wide text-slate-400">Evidence capture</h2>
      <p className="mt-1 text-xs text-[var(--color-text-muted)]">
        {assignment.evidenceCount} file(s) on record · status {assignment.status.replace('_', ' ')}
      </p>

      {evidence.length === 0 ? (
        <p className="mt-3 text-sm text-slate-400">No evidence uploaded yet.</p>
      ) : (
        <ul className="mt-3 space-y-2">
          {evidence.map((item) => (
            <li key={item.evidenceId} className="rounded-lg border border-slate-700 bg-slate-950/40 p-3 text-sm">
              <p className="font-medium text-slate-100">{item.fileName}</p>
              <p className="mt-1 text-xs text-slate-400">
                {item.evidenceTypeKey} · {formatBytes(item.sizeBytes)} ·{' '}
                {new Date(item.createdAt).toLocaleString()}
              </p>
              {item.notes && <p className="mt-1 text-xs text-slate-300">{item.notes}</p>}
            </li>
          ))}
        </ul>
      )}

      {canUpload && assignmentOpen && (
        <div className="mt-4 space-y-3 border-t border-slate-700 pt-4">
          <ControlledSelect
            id="evidence-capture-type"
            label="Evidence type"
            value={evidenceTypeKey}
            onChange={onEvidenceTypeKeyChange}
            options={evidenceTypeOptions}
            emptyLabel="Select evidence type…"
            testId="evidence-capture-type"
            className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-2 py-1 text-sm text-slate-100"
          />
          <label htmlFor="evidence-capture-file" className="block text-xs text-slate-400">
            File
            <input
              id="evidence-capture-file"
              type="file"
              className="mt-1 block w-full text-sm text-slate-300"
              onChange={(e) => onSelectFile(e.target.files?.[0] ?? null)}
            />
            {selectedFileName && <p className="mt-1 text-xs text-[var(--color-text-muted)]">{selectedFileName}</p>}
          </label>
          <label htmlFor="evidence-capture-notes" className="block text-xs text-slate-400">
            Notes (optional)
            <input
              id="evidence-capture-notes"
              className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-2 py-1 text-sm text-slate-100"
              value={notes}
              onChange={(e) => onNotesChange(e.target.value)}
            />
          </label>
          <button
            type="button"
            className="rounded bg-violet-700 px-3 py-1.5 text-sm font-medium text-white hover:bg-violet-600 disabled:opacity-50"
            disabled={isUploading || !selectedFileName}
            onClick={onUploadEvidence}
          >
            {isUploading ? 'Uploading…' : 'Upload evidence'}
          </button>
        </div>
      )}

      {!assignmentOpen && (
        <p className="mt-3 text-xs text-amber-300">Evidence upload is closed for completed assignments.</p>
      )}
    </section>
  )
}
