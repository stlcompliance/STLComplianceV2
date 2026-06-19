import type {
  FactDefinitionResponse,
  FactRequirementResponse,
  RegulatoryCitationResponse,
} from '../api/types'

interface CitationFactCatalogPanelProps {
  citations: RegulatoryCitationResponse[]
  factDefinitions: FactDefinitionResponse[]
  factRequirements: FactRequirementResponse[]
}

export function CitationFactCatalogPanel({
  citations,
  factDefinitions,
  factRequirements,
}: CitationFactCatalogPanelProps) {
  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between gap-3">
        <p className="text-sm text-slate-400">
          Regulatory citations linked to programs and rule packs, plus fact definitions and requirements for rule evaluation.
        </p>
      </div>

      <div className="grid gap-6 lg:grid-cols-2">
        <section className="rounded-xl border border-slate-700 bg-slate-900/60 p-4">
          <h2 className="text-sm font-semibold uppercase tracking-wide text-slate-400">Citations</h2>
          {citations.length === 0 ? (
            <p className="mt-3 text-sm text-slate-400">No regulatory citations registered yet.</p>
          ) : (
            <ul className="mt-3 space-y-2">
              {citations.map((citation) => (
                <li key={citation.citationId} className="rounded-lg border border-slate-700 bg-slate-950/60 p-3">
                  <div className="flex items-start justify-between gap-2">
                    <p className="font-medium text-slate-100">{citation.label}</p>
                    <span className="rounded bg-slate-800 px-2 py-0.5 text-xs text-slate-400">v{citation.versionNumber}</span>
                  </div>
                  <p className="font-mono text-xs text-amber-300">{citation.sourceReference}</p>
                  <p className="mt-1 font-mono text-xs text-violet-300">{citation.citationKey}</p>
                  <p className="mt-1 text-xs text-[var(--color-text-muted)]">
                    {citation.regulatoryProgramLabel}
                    {citation.rulePackLabel ? ` · ${citation.rulePackLabel}` : ''}
                  </p>
                </li>
              ))}
            </ul>
          )}
        </section>

        <section className="rounded-xl border border-slate-700 bg-slate-900/60 p-4">
          <h2 className="text-sm font-semibold uppercase tracking-wide text-slate-400">Fact definitions</h2>
          {factDefinitions.length === 0 ? (
            <p className="mt-3 text-sm text-slate-400">No fact definitions in the catalog yet.</p>
          ) : (
            <ul className="mt-3 space-y-2">
              {factDefinitions.map((fact) => (
                <li key={fact.factDefinitionId} className="rounded-lg border border-slate-700 bg-slate-950/60 p-3">
                  <p className="font-medium text-slate-100">{fact.label}</p>
                  <p className="font-mono text-xs text-sky-300">{fact.factKey}</p>
                  <p className="mt-1 text-xs text-[var(--color-text-muted)]">{fact.valueType}</p>
                </li>
              ))}
            </ul>
          )}
        </section>

        <section className="rounded-xl border border-slate-700 bg-slate-900/60 p-4 lg:col-span-2">
          <h2 className="text-sm font-semibold uppercase tracking-wide text-slate-400">Fact requirements</h2>
          {factRequirements.length === 0 ? (
            <p className="mt-3 text-sm text-slate-400">No fact requirements linked to rule packs or citations yet.</p>
          ) : (
            <ul className="mt-3 space-y-2">
              {factRequirements.map((requirement) => (
                <li key={requirement.factRequirementId} className="rounded-lg border border-slate-700 bg-slate-950/60 p-3">
                  <div className="flex items-start justify-between gap-2">
                    <p className="font-medium text-slate-100">{requirement.label}</p>
                    <span
                      className={`rounded px-2 py-0.5 text-xs ${
                        requirement.isRequired ? 'bg-rose-900/60 text-rose-200' : 'bg-slate-800 text-slate-400'
                      }`}
                    >
                      {requirement.isRequired ? 'required' : 'optional'}
                    </span>
                  </div>
                  <p className="font-mono text-xs text-emerald-300">{requirement.factKey}</p>
                  <p className="mt-1 text-xs text-[var(--color-text-muted)]">
                    {requirement.rulePackKey
                      ? `Rule pack: ${requirement.rulePackKey}`
                      : requirement.citationKey
                        ? `Citation: ${requirement.citationKey}`
                        : 'Unlinked'}
                  </p>
                </li>
              ))}
            </ul>
          )}
        </section>
      </div>
    </div>
  )
}
