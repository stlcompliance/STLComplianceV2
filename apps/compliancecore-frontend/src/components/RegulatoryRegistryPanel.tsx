import type {
  GoverningBodyResponse,
  JurisdictionResponse,
  RegulatoryProgramResponse,
  RulePackResponse,
} from '../api/types'

interface RegulatoryRegistryPanelProps {
  governingBodies: GoverningBodyResponse[]
  jurisdictions: JurisdictionResponse[]
  programs: RegulatoryProgramResponse[]
  rulePacks: RulePackResponse[]
  canManage: boolean
  onSeedRegistry: () => void
  isSeeding: boolean
  onAdvanceRulePack: (rulePackId: string, status: string) => void
  isAdvancingRulePack: boolean
}

function statusBadgeClass(status: string): string {
  switch (status) {
    case 'published':
      return 'bg-emerald-900/60 text-emerald-200'
    case 'review':
      return 'bg-amber-900/60 text-amber-200'
    case 'archived':
      return 'bg-slate-800 text-slate-400'
    default:
      return 'bg-slate-800 text-slate-300'
  }
}

export function RegulatoryRegistryPanel({
  governingBodies,
  jurisdictions,
  programs,
  rulePacks,
  canManage,
  onSeedRegistry,
  isSeeding,
  onAdvanceRulePack,
  isAdvancingRulePack,
}: RegulatoryRegistryPanelProps) {
  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between gap-3">
        <p className="text-sm text-slate-400">
          Governing body → jurisdiction → regulatory program → rule pack hierarchy for compliance rule authority.
        </p>
        {canManage && (
          <button
            type="button"
            onClick={onSeedRegistry}
            disabled={isSeeding}
            className="rounded-md bg-violet-600 px-3 py-1.5 text-xs font-medium text-white hover:bg-violet-500 disabled:opacity-50"
          >
            {isSeeding ? 'Creating…' : 'Seed sample registry'}
          </button>
        )}
      </div>

      <div className="grid gap-6 lg:grid-cols-2">
        <section className="rounded-xl border border-slate-700 bg-slate-900/60 p-4">
          <h2 className="text-sm font-semibold uppercase tracking-wide text-slate-400">Governing bodies</h2>
          {governingBodies.length === 0 ? (
            <p className="mt-3 text-sm text-slate-400">No governing bodies registered yet.</p>
          ) : (
            <ul className="mt-3 space-y-2">
              {governingBodies.map((body) => (
                <li key={body.governingBodyId} className="rounded-lg border border-slate-700 bg-slate-950/60 p-3">
                  <p className="font-medium text-slate-100">{body.label}</p>
                  <p className="font-mono text-xs text-violet-300">{body.bodyKey}</p>
                </li>
              ))}
            </ul>
          )}
        </section>

        <section className="rounded-xl border border-slate-700 bg-slate-900/60 p-4">
          <h2 className="text-sm font-semibold uppercase tracking-wide text-slate-400">Jurisdictions</h2>
          {jurisdictions.length === 0 ? (
            <p className="mt-3 text-sm text-slate-400">No jurisdictions registered yet.</p>
          ) : (
            <ul className="mt-3 space-y-2">
              {jurisdictions.map((jurisdiction) => (
                <li key={jurisdiction.jurisdictionId} className="rounded-lg border border-slate-700 bg-slate-950/60 p-3">
                  <p className="font-medium text-slate-100">{jurisdiction.label}</p>
                  <p className="font-mono text-xs text-sky-300">{jurisdiction.jurisdictionKey}</p>
                  <p className="mt-1 text-xs text-slate-500">{jurisdiction.governingBodyLabel}</p>
                </li>
              ))}
            </ul>
          )}
        </section>

        <section className="rounded-xl border border-slate-700 bg-slate-900/60 p-4">
          <h2 className="text-sm font-semibold uppercase tracking-wide text-slate-400">Regulatory programs</h2>
          {programs.length === 0 ? (
            <p className="mt-3 text-sm text-slate-400">No regulatory programs registered yet.</p>
          ) : (
            <ul className="mt-3 space-y-2">
              {programs.map((program) => (
                <li key={program.regulatoryProgramId} className="rounded-lg border border-slate-700 bg-slate-950/60 p-3">
                  <p className="font-medium text-slate-100">{program.label}</p>
                  <p className="font-mono text-xs text-emerald-300">{program.programKey}</p>
                  <p className="mt-1 text-xs text-slate-500">{program.jurisdictionLabel}</p>
                </li>
              ))}
            </ul>
          )}
        </section>

        <section className="rounded-xl border border-slate-700 bg-slate-900/60 p-4">
          <h2 className="text-sm font-semibold uppercase tracking-wide text-slate-400">Rule packs</h2>
          {rulePacks.length === 0 ? (
            <p className="mt-3 text-sm text-slate-400">No rule packs defined yet.</p>
          ) : (
            <ul className="mt-3 space-y-2">
              {rulePacks.map((pack) => (
                <li key={pack.rulePackId} className="rounded-lg border border-slate-700 bg-slate-950/60 p-3">
                  <div className="flex items-start justify-between gap-2">
                    <div>
                      <p className="font-medium text-slate-100">{pack.label}</p>
                      <p className="font-mono text-xs text-amber-300">{pack.packKey}</p>
                    </div>
                    <span className={`rounded px-2 py-0.5 text-xs uppercase ${statusBadgeClass(pack.status)}`}>
                      {pack.status}
                    </span>
                  </div>
                  <p className="mt-2 text-xs text-slate-500">
                    v{pack.versionNumber} · {pack.regulatoryProgramLabel}
                  </p>
                  {canManage && pack.status === 'draft' && (
                    <button
                      type="button"
                      onClick={() => onAdvanceRulePack(pack.rulePackId, 'review')}
                      disabled={isAdvancingRulePack}
                      className="mt-2 rounded bg-slate-800 px-2 py-1 text-xs text-slate-200 hover:bg-slate-700 disabled:opacity-50"
                    >
                      Submit for review
                    </button>
                  )}
                  {canManage && pack.status === 'review' && (
                    <button
                      type="button"
                      onClick={() => onAdvanceRulePack(pack.rulePackId, 'published')}
                      disabled={isAdvancingRulePack}
                      className="mt-2 rounded bg-emerald-800 px-2 py-1 text-xs text-emerald-100 hover:bg-emerald-700 disabled:opacity-50"
                    >
                      Publish
                    </button>
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
