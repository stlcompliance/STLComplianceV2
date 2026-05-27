import type { TrainingCitationAttachmentResponse } from '../api/types'

interface CitationAttachmentPanelProps {
  title: string
  citations: TrainingCitationAttachmentResponse[]
  citationIdInput: string
  citationKeyInput: string
  onCitationIdChange: (value: string) => void
  onCitationKeyChange: (value: string) => void
  onAttach: () => void
  onRemove: (attachmentId: string) => void
  isAttaching: boolean
  isRemovingId: string | null
  canManage: boolean
  validateWithComplianceCore: boolean
  onValidateWithComplianceCoreChange: (value: boolean) => void
}

export function CitationAttachmentPanel({
  title,
  citations,
  citationIdInput,
  citationKeyInput,
  onCitationIdChange,
  onCitationKeyChange,
  onAttach,
  onRemove,
  isAttaching,
  isRemovingId,
  canManage,
  validateWithComplianceCore,
  onValidateWithComplianceCoreChange,
}: CitationAttachmentPanelProps) {
  return (
    <section className="rounded-xl border border-slate-700 bg-slate-900/60 p-4">
      <h2 className="text-sm font-semibold uppercase tracking-wide text-slate-400">{title}</h2>
      <p className="mt-1 text-xs text-slate-500">
        References Compliance Core citation keys only — TrainArr does not own regulatory citations.
      </p>

      {canManage ? (
        <div className="mt-4 grid gap-3 sm:grid-cols-2">
          <label className="block text-xs text-slate-400">
            Compliance Core citation id
            <input
              className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-2 py-1 font-mono text-xs text-slate-100"
              value={citationIdInput}
              onChange={(e) => onCitationIdChange(e.target.value)}
              placeholder="00000000-0000-0000-0000-000000000000"
            />
          </label>
          <label className="block text-xs text-slate-400">
            Citation key
            <input
              className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-2 py-1 text-sm text-slate-100"
              value={citationKeyInput}
              onChange={(e) => onCitationKeyChange(e.target.value)}
              placeholder="cfr_391_11"
            />
          </label>
          <label className="flex items-center gap-2 text-xs text-slate-400 sm:col-span-2">
            <input
              type="checkbox"
              checked={validateWithComplianceCore}
              onChange={(e) => onValidateWithComplianceCoreChange(e.target.checked)}
            />
            Validate against Compliance Core when attaching
          </label>
          <button
            type="button"
            className="rounded bg-violet-700 px-3 py-1.5 text-sm font-medium text-white hover:bg-violet-600 disabled:opacity-50 sm:col-span-2 sm:justify-self-start"
            disabled={isAttaching || !citationIdInput.trim() || !citationKeyInput.trim()}
            onClick={onAttach}
          >
            {isAttaching ? 'Attaching…' : 'Attach citation'}
          </button>
        </div>
      ) : (
        <p className="mt-3 text-sm text-slate-400">Citation management requires trainarr admin access.</p>
      )}

      {citations.length === 0 ? (
        <p className="mt-4 text-sm text-slate-500">No regulatory citations attached.</p>
      ) : (
        <ul className="mt-4 space-y-2">
          {citations.map((citation) => (
            <li
              key={citation.attachmentId}
              className="rounded-lg border border-slate-700 bg-slate-950/40 p-3 text-sm"
            >
              <div className="flex flex-wrap items-start justify-between gap-2">
                <div>
                  <p className="font-medium text-slate-100">{citation.citationKey}</p>
                  {citation.metadata ? (
                    <p className="mt-1 text-slate-300">{citation.metadata.label}</p>
                  ) : null}
                  <p className="mt-1 font-mono text-xs text-slate-500">
                    {citation.complianceCoreCitationId} · v{citation.citationVersion}
                  </p>
                  {citation.metadata ? (
                    <p className="mt-1 text-xs text-slate-400">{citation.metadata.sourceReference}</p>
                  ) : null}
                </div>
                {canManage ? (
                  <button
                    type="button"
                    className="rounded border border-slate-600 px-2 py-1 text-xs text-slate-300 hover:bg-slate-800 disabled:opacity-50"
                    disabled={isRemovingId === citation.attachmentId}
                    onClick={() => onRemove(citation.attachmentId)}
                  >
                    {isRemovingId === citation.attachmentId ? 'Removing…' : 'Remove'}
                  </button>
                ) : null}
              </div>
            </li>
          ))}
        </ul>
      )}
    </section>
  )
}
