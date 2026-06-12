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
  onAdvanceRulePack,
  isAdvancingRulePack,
}: RegulatoryRegistryPanelProps) {
  return (
    <div className="grid gap-6 lg:grid-cols-2">
      <section className="rounded-xl border border-slate-700 bg-slate-900/60 p-5">
        <h2 className="text-lg font-medium text-white">Governing bodies</h2>
        <ul className="mt-4 space-y-2 text-sm">
          {governingBodies.length === 0 ? (
            <li className="text-slate-400">No governing bodies registered yet.</li>
          ) : (
            governingBodies.map((body) => (
              <li key={body.governingBodyId} className="rounded-lg border border-slate-800 p-3">
                <div className="font-medium text-slate-100">{body.label}</div>
                <div className="mt-1 font-mono text-xs text-violet-300">{body.bodyKey}</div>
              </li>
            ))
          )}
        </ul>
      </section>

      <section className="rounded-xl border border-slate-700 bg-slate-900/60 p-5">
        <h2 className="text-lg font-medium text-white">Jurisdictions</h2>
        <ul className="mt-4 space-y-2 text-sm">
          {jurisdictions.length === 0 ? (
            <li className="text-slate-400">No jurisdictions registered yet.</li>
          ) : (
            jurisdictions.map((jurisdiction) => (
              <li key={jurisdiction.jurisdictionId} className="rounded-lg border border-slate-800 p-3">
                <div className="font-medium text-slate-100">{jurisdiction.label}</div>
                <div className="mt-1 font-mono text-xs text-sky-300">{jurisdiction.jurisdictionKey}</div>
                <div className="mt-1 text-xs text-slate-400">{jurisdiction.governingBodyLabel}</div>
              </li>
            ))
          )}
        </ul>
      </section>

      <section className="rounded-xl border border-slate-700 bg-slate-900/60 p-5">
        <h2 className="text-lg font-medium text-white">Regulatory programs</h2>
        <ul className="mt-4 space-y-2 text-sm">
          {programs.length === 0 ? (
            <li className="text-slate-400">No regulatory programs registered yet.</li>
          ) : (
            programs.map((program) => (
              <li key={program.regulatoryProgramId} className="rounded-lg border border-slate-800 p-3">
                <div className="font-medium text-slate-100">{program.label}</div>
                <div className="mt-1 font-mono text-xs text-emerald-300">{program.programKey}</div>
                <div className="mt-1 text-xs text-slate-400">{program.jurisdictionLabel}</div>
              </li>
            ))
          )}
        </ul>
      </section>

      <section className="rounded-xl border border-slate-700 bg-slate-900/60 p-5">
        <h2 className="text-lg font-medium text-white">Rule packs</h2>
        <ul className="mt-4 space-y-2 text-sm">
          {rulePacks.length === 0 ? (
            <li className="text-slate-400">No rule packs defined yet.</li>
          ) : (
            rulePacks.map((pack) => (
              <li key={pack.rulePackId} className="rounded-lg border border-slate-800 p-3">
                <div className="flex items-start justify-between gap-3">
                  <div>
                    <div className="font-medium text-slate-100">{pack.label}</div>
                    <div className="mt-1 font-mono text-xs text-amber-300">{pack.packKey}</div>
                  </div>
                  <span className={`rounded px-2 py-0.5 text-xs uppercase ${statusBadgeClass(pack.status)}`}>
                    {pack.status}
                  </span>
                </div>
                <div className="mt-2 text-xs text-slate-400">
                  v{pack.versionNumber} · {pack.regulatoryProgramLabel}
                </div>
                {canManage && pack.status === 'draft' && (
                  <button
                    type="button"
                    onClick={() => onAdvanceRulePack(pack.rulePackId, 'review')}
                    disabled={isAdvancingRulePack}
                    className="mt-3 inline-flex items-center rounded-md border border-slate-700 bg-slate-800 px-3 py-1.5 text-xs text-slate-200 hover:border-slate-600 hover:bg-slate-700 disabled:cursor-not-allowed disabled:opacity-50"
                  >
                    Submit for review
                  </button>
                )}
                {canManage && pack.status === 'review' && (
                  <button
                    type="button"
                    onClick={() => onAdvanceRulePack(pack.rulePackId, 'published')}
                    disabled={isAdvancingRulePack}
                    className="mt-3 inline-flex items-center rounded-md border border-emerald-700 bg-emerald-800 px-3 py-1.5 text-xs text-emerald-100 hover:border-emerald-600 hover:bg-emerald-700 disabled:cursor-not-allowed disabled:opacity-50"
                  >
                    Publish
                  </button>
                )}
              </li>
            ))
          )}
        </ul>
      </section>
    </div>
  )
}
