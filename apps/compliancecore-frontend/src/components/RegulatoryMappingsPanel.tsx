import type { RegulatoryMappingResponse } from '../api/types'

interface RegulatoryMappingsPanelProps {
  mappings: RegulatoryMappingResponse[]
}

export function RegulatoryMappingsPanel({
  mappings,
}: RegulatoryMappingsPanelProps) {
  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between gap-3">
        <p className="text-sm text-slate-400">
          Links compliance and material keys to regulatory programs, rule packs, citations, and fact definitions for
          cross-product evaluation.
        </p>
      </div>

      <section className="rounded-xl border border-slate-700 bg-slate-900/60 p-4">
        <h2 className="text-sm font-semibold uppercase tracking-wide text-slate-400">Regulatory mappings</h2>
        {mappings.length === 0 ? (
          <p className="mt-3 text-sm text-slate-400">No regulatory mappings registered yet.</p>
        ) : (
          <ul className="mt-3 space-y-2">
            {mappings.map((mapping) => (
              <li key={mapping.regulatoryMappingId} className="rounded-lg border border-slate-700 bg-slate-950/60 p-3">
                <div className="flex items-start justify-between gap-2">
                  <p className="font-medium text-slate-100">{mapping.label}</p>
                  <span className="rounded bg-slate-800 px-2 py-0.5 text-xs text-slate-400">{mapping.targetKind}</span>
                </div>
                <p className="font-mono text-xs text-violet-300">{mapping.mappingKey}</p>
                <p className="mt-2 text-xs text-slate-500">
                  Target:{' '}
                  <span className="font-mono text-emerald-300">
                    {mapping.complianceKey ?? mapping.materialKey ?? '—'}
                  </span>
                </p>
                <p className="mt-1 text-xs text-slate-500">
                  Program: {mapping.regulatoryProgramLabel}
                  {mapping.rulePackKey ? ` · Pack: ${mapping.rulePackKey}` : ''}
                  {mapping.citationKey ? ` · Citation: ${mapping.citationKey}` : ''}
                  {mapping.factKey ? ` · Fact: ${mapping.factKey}` : ''}
                </p>
              </li>
            ))}
          </ul>
        )}
      </section>
    </div>
  )
}
