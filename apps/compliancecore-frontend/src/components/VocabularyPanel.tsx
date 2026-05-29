import type { ComplianceKeyResponse, MaterialKeyResponse, VocabularyTermResponse, VocabularyTypeResponse } from '../api/types'

interface VocabularyPanelProps {
  types: VocabularyTypeResponse[]
  terms: VocabularyTermResponse[]
  complianceKeys: ComplianceKeyResponse[]
  materialKeys: MaterialKeyResponse[]
  selectedTypeKey: string
  onSelectType: (typeKey: string) => void
  canManage: boolean
  onCreateTerm: () => void
  isCreatingTerm: boolean
}

export function VocabularyPanel({
  types,
  terms,
  complianceKeys,
  materialKeys,
  selectedTypeKey,
  onSelectType,
  canManage,
  onCreateTerm,
  isCreatingTerm,
}: VocabularyPanelProps) {
  return (
    <div className="grid gap-6 lg:grid-cols-2">
      <section className="rounded-xl border border-slate-700 bg-slate-900/60 p-4">
        <div className="flex items-center justify-between gap-3">
          <h2 className="text-sm font-semibold uppercase tracking-wide text-slate-400">Controlled vocabulary</h2>
          {canManage && (
            <button
              type="button"
              onClick={onCreateTerm}
              disabled={isCreatingTerm || !selectedTypeKey}
              className="rounded-md bg-violet-600 px-3 py-1.5 text-xs font-medium text-white hover:bg-violet-500 disabled:opacity-50"
            >
              {isCreatingTerm ? 'Creating…' : 'Add sample term'}
            </button>
          )}
        </div>

        <label htmlFor="vocabulary-type-filter" className="mt-4 block text-xs text-slate-500">
          Vocabulary type filter
          <select
            id="vocabulary-type-filter"
            value={selectedTypeKey}
            onChange={(event) => onSelectType(event.target.value)}
            className="mt-1 w-full rounded-md border border-slate-600 bg-slate-950 px-3 py-2 text-sm text-slate-100"
          >
            <option value="">All types ({types.length})</option>
            {types.map((type) => (
              <option key={type.typeKey} value={type.typeKey}>
                {type.label}
              </option>
            ))}
          </select>
        </label>

        {terms.length === 0 ? (
          <p className="mt-4 text-sm text-slate-400">No vocabulary terms yet for this filter.</p>
        ) : (
          <ul className="mt-4 space-y-2">
            {terms.map((term) => (
              <li key={term.termId} className="rounded-lg border border-slate-700 bg-slate-950/60 p-3">
                <div className="flex items-start justify-between gap-2">
                  <div>
                    <p className="font-medium text-slate-100">{term.label}</p>
                    <p className="font-mono text-xs text-violet-300">{term.termKey}</p>
                  </div>
                  <span className="rounded bg-slate-800 px-2 py-0.5 text-xs text-slate-400">{term.vocabularyTypeKey}</span>
                </div>
                <p className="mt-2 text-sm text-slate-400">{term.description}</p>
                {term.aliases.length > 0 && (
                  <p className="mt-2 text-xs text-slate-500">Aliases: {term.aliases.join(', ')}</p>
                )}
              </li>
            ))}
          </ul>
        )}
      </section>

      <div className="space-y-6">
        <section className="rounded-xl border border-slate-700 bg-slate-900/60 p-4">
          <h2 className="text-sm font-semibold uppercase tracking-wide text-slate-400">Compliance keys</h2>
          {complianceKeys.length === 0 ? (
            <p className="mt-3 text-sm text-slate-400">No compliance keys defined yet.</p>
          ) : (
            <ul className="mt-3 space-y-2">
              {complianceKeys.map((key) => (
                <li key={key.complianceKeyId} className="rounded-lg border border-slate-700 bg-slate-950/60 p-3">
                  <p className="font-medium text-slate-100">{key.label}</p>
                  <p className="font-mono text-xs text-emerald-300">{key.key}</p>
                  <p className="mt-1 text-xs text-slate-500">{key.category}</p>
                </li>
              ))}
            </ul>
          )}
        </section>

        <section className="rounded-xl border border-slate-700 bg-slate-900/60 p-4">
          <h2 className="text-sm font-semibold uppercase tracking-wide text-slate-400">Material keys</h2>
          {materialKeys.length === 0 ? (
            <p className="mt-3 text-sm text-slate-400">No material keys defined yet.</p>
          ) : (
            <ul className="mt-3 space-y-2">
              {materialKeys.map((key) => (
                <li key={key.materialKeyId} className="rounded-lg border border-slate-700 bg-slate-950/60 p-3">
                  <p className="font-medium text-slate-100">{key.label}</p>
                  <p className="font-mono text-xs text-amber-300">{key.key}</p>
                  <p className="mt-1 text-xs text-slate-500">{key.category}</p>
                </li>
              ))}
            </ul>
          )}
        </section>
      </div>
    </div>
  )
}
