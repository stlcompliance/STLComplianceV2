import { StaticSearchPicker, type PickerOption } from '@stl/shared-ui'

import type { TrainingProgramContentReferenceResponse } from '../api/types'

interface ProgramContentReferencePanelProps {
  title: string
  contentReferences: TrainingProgramContentReferenceResponse[]
  contentTypeKey: string
  contentTitle: string
  contentReferenceValue: string
  contentNotes: string
  contentLocaleTag: string
  onContentTypeKeyChange: (value: string) => void
  onContentTitleChange: (value: string) => void
  onContentReferenceValueChange: (value: string) => void
  onContentNotesChange: (value: string) => void
  onContentLocaleTagChange: (value: string) => void
  onAttach: () => void
  onRemove: (contentReferenceId: string) => Promise<void>
  isAttaching: boolean
  isRemovingId: string | null
  canManage: boolean
}

const contentTypeOptions: PickerOption[] = [
  { value: 'uploaded_pdf', label: 'Uploaded PDF' },
  { value: 'uploaded_video', label: 'Uploaded video' },
  { value: 'external_url', label: 'External URL' },
  { value: 'internal_document_reference', label: 'Internal document reference' },
  { value: 'policy_document', label: 'Policy document' },
  { value: 'compliance_core_citation', label: 'Compliance Core citation' },
  { value: 'maintainarr_asset_procedure', label: 'MaintainArr asset procedure' },
  { value: 'staffarr_policy', label: 'StaffArr policy' },
  { value: 'supplyarr_vendor_document', label: 'SupplyArr vendor document' },
  { value: 'embedded_text_lesson', label: 'Embedded text lesson' },
  { value: 'quiz_bank', label: 'Quiz bank' },
]

function formatContentType(value: string): string {
  return (
    contentTypeOptions.find((option) => option.value === value)?.label ??
    value.replace(/[_-]+/g, ' ')
  )
}

export function ProgramContentReferencePanel({
  title,
  contentReferences,
  contentTypeKey,
  contentTitle,
  contentReferenceValue,
  contentNotes,
  contentLocaleTag,
  onContentTypeKeyChange,
  onContentTitleChange,
  onContentReferenceValueChange,
  onContentNotesChange,
  onContentLocaleTagChange,
  onAttach,
  onRemove,
  isAttaching,
  isRemovingId,
  canManage,
}: ProgramContentReferencePanelProps) {
  const selectedContentTypeOption =
    contentTypeOptions.find((option) => option.value === contentTypeKey.trim()) ?? null

  return (
    <section className="rounded-xl border border-slate-700 bg-slate-900/60 p-4">
      <h2 className="text-sm font-semibold uppercase tracking-wide text-slate-400">{title}</h2>
      <p className="mt-1 text-xs text-[var(--color-text-muted)]">
        Course content references can point to PDFs, videos, URLs, policy docs, citations, quiz banks, and
        external product references, with optional locale tags for multilingual content.
      </p>

      {canManage ? (
        <div className="mt-4 grid gap-3 sm:grid-cols-2">
          <StaticSearchPicker
            id="program-content-reference-type"
            label="Content type"
            value={contentTypeKey}
            onChange={onContentTypeKeyChange}
            options={contentTypeOptions}
            selectedOption={selectedContentTypeOption ?? undefined}
            placeholder="Search content types…"
            testId="program-content-reference-type"
          />
          <label htmlFor="program-content-reference-title" className="block text-sm text-slate-300">
            Title
            <input
              id="program-content-reference-title"
              value={contentTitle}
              onChange={(event) => onContentTitleChange(event.target.value)}
              className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
              required
              minLength={2}
            />
          </label>
          <label htmlFor="program-content-reference-value" className="block text-sm text-slate-300 sm:col-span-2">
            Reference value
            <input
              id="program-content-reference-value"
              value={contentReferenceValue}
              onChange={(event) => onContentReferenceValueChange(event.target.value)}
              className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
              placeholder="URL, document key, citation key, procedure reference, or file reference"
              required
              minLength={2}
            />
          </label>
          <label htmlFor="program-content-reference-locale" className="block text-sm text-slate-300">
            Locale tag
            <input
              id="program-content-reference-locale"
              value={contentLocaleTag}
              onChange={(event) => onContentLocaleTagChange(event.target.value)}
              className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
              placeholder="e.g. en, es, en-us"
            />
          </label>
          <label htmlFor="program-content-reference-notes" className="block text-sm text-slate-300 sm:col-span-2">
            Notes
            <textarea
              id="program-content-reference-notes"
              value={contentNotes}
              onChange={(event) => onContentNotesChange(event.target.value)}
              className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
              rows={2}
            />
          </label>
          <button
            type="button"
            className="rounded bg-sky-700 px-3 py-1.5 text-sm font-medium text-white hover:bg-sky-600 disabled:opacity-50 sm:col-span-2 sm:justify-self-start"
            disabled={isAttaching || !contentTypeKey.trim() || !contentTitle.trim() || !contentReferenceValue.trim()}
            onClick={onAttach}
          >
            {isAttaching ? 'Adding…' : 'Add content reference'}
          </button>
        </div>
      ) : (
        <p className="mt-3 text-sm text-slate-400">Content reference management requires trainarr admin access.</p>
      )}

      {contentReferences.length === 0 ? (
        <p className="mt-4 text-sm text-[var(--color-text-muted)]">No training content references attached.</p>
      ) : (
        <ul className="mt-4 space-y-2">
          {contentReferences.map((reference) => (
            <li
              key={reference.contentReferenceId}
              className="rounded-lg border border-slate-700 bg-slate-950/40 p-3 text-sm"
            >
              <div className="flex flex-wrap items-start justify-between gap-2">
                <div>
                  <p className="font-medium text-slate-100">{reference.title}</p>
                  <p className="mt-1 text-xs uppercase tracking-wide text-sky-300">
                    {formatContentType(reference.contentType)}
                  </p>
                  <p className="mt-1 font-mono text-xs text-[var(--color-text-muted)]">{reference.referenceValue}</p>
                  {reference.localeTag ? (
                    <p className="mt-1 text-xs uppercase tracking-wide text-emerald-300">
                      Locale {reference.localeTag}
                    </p>
                  ) : null}
                  {reference.notes ? <p className="mt-1 text-xs text-slate-400">{reference.notes}</p> : null}
                </div>
                {canManage ? (
                  <button
                    type="button"
                    className="rounded border border-slate-600 px-2 py-1 text-xs text-slate-300 hover:bg-slate-800 disabled:opacity-50"
                    disabled={isRemovingId === reference.contentReferenceId}
                    onClick={() => void onRemove(reference.contentReferenceId)}
                  >
                    {isRemovingId === reference.contentReferenceId ? 'Removing…' : 'Remove'}
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
