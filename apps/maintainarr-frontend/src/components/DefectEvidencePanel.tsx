import { ControlledSelect } from '@stl/shared-ui'

import type { DefectEvidenceResponse } from '../api/types'
import { DEFECT_EVIDENCE_TYPE_OPTIONS } from './formOptions'

interface DefectEvidencePanelProps {
  defectId: string | null
  defectTitle: string | null
  defectStatus: string | null
  evidence: DefectEvidenceResponse[]
  canUpload: boolean
  evidenceTypeKey: string
  evidenceNotes: string
  selectedFileName: string | null
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

function canUploadToDefect(status: string | null): boolean {
  return status === 'open' || status === 'acknowledged' || status === 'in_repair'
}

export function DefectEvidencePanel({
  defectId,
  defectTitle,
  defectStatus,
  evidence,
  canUpload,
  evidenceTypeKey,
  evidenceNotes,
  selectedFileName,
  onEvidenceTypeKeyChange,
  onEvidenceNotesChange,
  onSelectFile,
  onUploadEvidence,
  isUploadingEvidence,
  isLoading,
}: DefectEvidencePanelProps) {
  if (!defectId) {
    return (
      <div
        className="mt-6 rounded-lg border border-slate-800 bg-slate-950/40 p-4"
        data-testid="defect-evidence-panel"
      >
        <p className="text-sm text-slate-400" data-testid="defect-evidence-empty">
          Select a defect to review or upload evidence photos and documents.
        </p>
      </div>
    )
  }

  const editable = canUploadToDefect(defectStatus)

  return (
    <div
      className="mt-6 rounded-lg border border-slate-800 bg-slate-950/40 p-4"
      data-testid="defect-evidence-panel"
    >
      <header className="mb-3">
        <h3 className="text-sm font-semibold text-white">Defect evidence</h3>
        {defectTitle ? (
          <p className="mt-1 text-xs text-slate-400">
            {defectTitle} · {defectStatus}
          </p>
        ) : null}
        <p className="mt-1 text-xs text-[var(--color-text-muted)]">
          docs/15 — attach photos or documents to support defect capture and repair proof.
        </p>
      </header>

      {isLoading ? (
        <p className="text-sm text-slate-400" data-testid="defect-evidence-loading">
          Loading evidence…
        </p>
      ) : (
        <>
          {evidence.length === 0 ? (
            <p className="text-sm text-slate-400">No evidence uploaded for this defect.</p>
          ) : (
            <ul className="space-y-2">
              {evidence.map((item) => (
                <li
                  key={item.evidenceId}
                  className="rounded border border-slate-700 bg-slate-950/40 px-3 py-2 text-sm"
                  data-testid="defect-evidence-item"
                >
                  <div className="font-medium text-slate-100">{item.fileName}</div>
                  <div className="text-xs text-slate-400">
                    {item.evidenceTypeKey} · {formatBytes(item.sizeBytes)} ·{' '}
                    {new Date(item.createdAt).toLocaleString()}
                  </div>
                  {item.notes ? <p className="mt-1 text-xs text-[var(--color-text-muted)]">{item.notes}</p> : null}
                </li>
              ))}
            </ul>
          )}

          {canUpload && editable ? (
            <div className="mt-3 space-y-2" data-testid="defect-evidence-upload">
              <ControlledSelect
                label="Evidence type"
                value={evidenceTypeKey}
                onChange={onEvidenceTypeKeyChange}
                options={DEFECT_EVIDENCE_TYPE_OPTIONS}
                emptyLabel="Select evidence type…"
                testId="defect-evidence-type"
                className="w-full rounded border border-slate-600 bg-slate-950 px-2 py-1 text-sm text-white"
              />
              <label className="block text-sm text-slate-300" htmlFor="defect-evidence-file">
                Evidence file
                <input
                  id="defect-evidence-file"
                  type="file"
                  className="mt-1 block w-full text-sm text-slate-300"
                  data-testid="defect-evidence-file"
                  onChange={(event) => onSelectFile(event.target.files?.[0] ?? null)}
                />
              </label>
              {selectedFileName ? <p className="text-xs text-[var(--color-text-muted)]">{selectedFileName}</p> : null}
              <label className="block text-sm text-slate-300" htmlFor="defect-evidence-notes">
                Notes (optional)
                <input
                  id="defect-evidence-notes"
                  className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-2 py-1 text-sm text-white"
                  value={evidenceNotes}
                  onChange={(event) => onEvidenceNotesChange(event.target.value)}
                />
              </label>
              <button
                type="button"
                className="rounded bg-amber-800 px-3 py-1 text-sm text-white hover:bg-amber-700 disabled:opacity-50"
                data-testid="defect-evidence-upload-button"
                disabled={!selectedFileName || !evidenceTypeKey.trim() || isUploadingEvidence}
                onClick={onUploadEvidence}
              >
                {isUploadingEvidence ? 'Uploading…' : 'Upload evidence'}
              </button>
            </div>
          ) : (
            !editable && (
              <p className="mt-2 text-xs text-amber-300">
                Evidence capture is closed for resolved or closed defects.
              </p>
            )
          )}
        </>
      )}
    </div>
  )
}
