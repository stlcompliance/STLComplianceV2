import { ControlledSelect } from '@stl/shared-ui'

import type { TrainingCitationAttachmentResponse } from '../api/types'

interface CitationReferenceOption {
  citationId: string
  citationKey: string
  label: string
}

interface CitationAttachmentPanelProps {
  title: string
  citations: TrainingCitationAttachmentResponse[]
  citationOptions: CitationReferenceOption[]
  citationIdInput: string
  citationKeyInput: string
  onCitationSelectionChange: (value: CitationReferenceOption | null) => void
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
  citationOptions,
  citationIdInput,
  citationKeyInput,
  onCitationSelectionChange,
  onAttach,
  onRemove,
  isAttaching,
  isRemovingId,
  canManage,
  validateWithComplianceCore,
  onValidateWithComplianceCoreChange,
}: CitationAttachmentPanelProps) {
  const selectedCitationOption =
    citationOptions.find((option) => option.citationId === citationIdInput.trim()) ?? null

  return (
    <section className="rounded-xl border border-slate-700 bg-slate-900/60 p-4">
      <h2 className="text-sm font-semibold uppercase tracking-wide text-slate-400">{title}</h2>
      <p className="mt-1 text-xs text-[var(--color-text-muted)]">
        References rule citation keys only. Regulatory citations stay in the rules workspace.
      </p>

      {canManage ? (
        <div className="mt-4 grid gap-3 sm:grid-cols-2">
          <ControlledSelect
            label="Compliance Core citation"
            value={citationIdInput}
            onChange={(value) => {
              const selectedOption =
                citationOptions.find((option) => option.citationId === value) ?? null
              onCitationSelectionChange(selectedOption)
            }}
            options={citationOptions.map((option) => ({
              value: option.citationId,
              label: option.label,
            }))}
            emptyLabel="Select citation…"
            testId="citation-attachment-citation-id"
            className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-2 py-1 text-sm text-slate-100"
          />
          <div className="block text-xs text-slate-400" data-testid="citation-attachment-citation-key">
            Citation key
            <p className="mt-1 rounded border border-slate-600 bg-slate-950 px-2 py-1 text-sm text-slate-100">
              {(selectedCitationOption?.citationKey ?? citationKeyInput) || 'Select a citation to populate'}
            </p>
          </div>
          {citationOptions.length === 0 ? (
            <p className="text-xs text-amber-300 sm:col-span-2">
              No citation references are available to select. Create and publish citations in Compliance Core first.
            </p>
          ) : null}
          <label htmlFor="citation-attachment-validate-compliance-core" className="flex items-center gap-2 text-xs text-slate-400 sm:col-span-2">
            <input
              id="citation-attachment-validate-compliance-core"
              type="checkbox"
              data-testid="citation-attachment-validate-compliance-core"
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
        <p className="mt-4 text-sm text-[var(--color-text-muted)]">No regulatory citations attached.</p>
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
                  <p className="mt-1 font-mono text-xs text-[var(--color-text-muted)]">
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
