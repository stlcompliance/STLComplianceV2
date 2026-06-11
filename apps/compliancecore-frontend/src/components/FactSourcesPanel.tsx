import type { FactDefinitionResponse, FactSourceResponse } from '../api/types'

interface FactSourcesPanelProps {
  factDefinitions: FactDefinitionResponse[]
  factSources: FactSourceResponse[]
}

export function FactSourcesPanel({
  factDefinitions,
  factSources,
}: FactSourcesPanelProps) {
  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between gap-3">
        <p className="text-sm text-slate-400">
          Fact sources bind catalog facts to static configuration or product API references. Compliance Core resolves
          registered sources via the internal resolve API for entitled service tokens.
        </p>
      </div>

      <div className="grid gap-6 lg:grid-cols-2">
        <section className="rounded-xl border border-slate-700 bg-slate-900/60 p-4">
          <h2 className="text-sm font-semibold uppercase tracking-wide text-slate-400">Catalog facts</h2>
          {factDefinitions.length === 0 ? (
            <p className="mt-3 text-sm text-slate-400">No fact definitions yet. Seed the citations & facts catalog first.</p>
          ) : (
            <ul className="mt-3 space-y-2">
              {factDefinitions.map((fact) => (
                <li key={fact.factDefinitionId} className="rounded-lg border border-slate-700 bg-slate-950/60 p-3">
                  <p className="font-medium text-slate-100">{fact.label}</p>
                  <p className="font-mono text-xs text-sky-300">{fact.factKey}</p>
                  <p className="mt-1 text-xs text-slate-500">{fact.valueType}</p>
                </li>
              ))}
            </ul>
          )}
        </section>

        <section className="rounded-xl border border-slate-700 bg-slate-900/60 p-4">
          <h2 className="text-sm font-semibold uppercase tracking-wide text-slate-400">Registered sources</h2>
          {factSources.length === 0 ? (
            <p className="mt-3 text-sm text-slate-400">No fact sources registered yet.</p>
          ) : (
            <ul className="mt-3 space-y-2">
              {factSources.map((source) => (
                <li key={source.factSourceId} className="rounded-lg border border-slate-700 bg-slate-950/60 p-3">
                  <div className="flex items-start justify-between gap-2">
                    <p className="font-medium text-slate-100">{source.label}</p>
                    <span className="rounded bg-slate-800 px-2 py-0.5 text-xs text-slate-400">{source.sourceType}</span>
                  </div>
                  <p className="font-mono text-xs text-violet-300">{source.sourceKey}</p>
                  <p className="mt-1 text-xs text-slate-500">
                    {source.factKey}
                    {source.productKey ? ` · ${source.productKey}` : ''}
                  </p>
                  {source.productReference && (
                    <p className="mt-1 font-mono text-xs text-slate-500">{source.productReference}</p>
                  )}
                </li>
              ))}
            </ul>
          )}
        </section>
      </div>
    </div>
  )
}
