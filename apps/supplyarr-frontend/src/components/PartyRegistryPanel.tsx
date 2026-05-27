import type { ExternalPartyResponse } from '../api/types'

interface PartyRegistryPanelProps {
  title: string
  parties: ExternalPartyResponse[]
  canManage: boolean
  isLoading: boolean
  partyKey: string
  displayName: string
  legalName: string
  taxIdentifier: string
  notes: string
  onPartyKeyChange: (value: string) => void
  onDisplayNameChange: (value: string) => void
  onLegalNameChange: (value: string) => void
  onTaxIdentifierChange: (value: string) => void
  onNotesChange: (value: string) => void
  onCreate: () => void
  isCreating: boolean
}

function approvalBadgeClass(status: string): string {
  switch (status) {
    case 'approved':
      return 'bg-emerald-500/20 text-emerald-300 ring-emerald-500/40'
    case 'restricted':
      return 'bg-amber-500/20 text-amber-200 ring-amber-500/40'
    case 'inactive':
      return 'bg-slate-500/20 text-slate-300 ring-slate-500/40'
    default:
      return 'bg-sky-500/20 text-sky-200 ring-sky-500/40'
  }
}

export function PartyRegistryPanel({
  title,
  parties,
  canManage,
  isLoading,
  partyKey,
  displayName,
  legalName,
  taxIdentifier,
  notes,
  onPartyKeyChange,
  onDisplayNameChange,
  onLegalNameChange,
  onTaxIdentifierChange,
  onNotesChange,
  onCreate,
  isCreating,
}: PartyRegistryPanelProps) {
  if (isLoading) {
    return <p className="text-sm text-slate-400">Loading {title.toLowerCase()}…</p>
  }

  return (
    <section className="rounded-xl border border-slate-700 bg-slate-900/60 p-5">
      <h2 className="text-lg font-medium text-white">{title}</h2>
      <ul className="mt-4 space-y-2 text-sm">
        {parties.length === 0 ? (
          <li className="text-slate-400">No records yet.</li>
        ) : (
          parties.map((party) => (
            <li key={party.partyId} className="rounded-lg border border-slate-800 p-3">
              <div className="flex items-start justify-between gap-3">
                <div>
                  <div className="font-medium">{party.displayName}</div>
                  <div className="text-slate-400">{party.partyKey}</div>
                  {party.legalName ? <div className="mt-1 text-slate-500">{party.legalName}</div> : null}
                </div>
                <span
                  className={`rounded-full px-2 py-0.5 text-xs ring-1 ${approvalBadgeClass(party.approvalStatus)}`}
                >
                  {party.approvalStatus}
                </span>
              </div>
              {party.contacts.length > 0 ? (
                <p className="mt-2 text-slate-400">
                  Primary contact: {party.contacts.find((c) => c.isPrimary)?.contactName ?? party.contacts[0].contactName}
                </p>
              ) : null}
            </li>
          ))
        )}
      </ul>
      {canManage ? (
        <div className="mt-4 space-y-2">
          <input
            className="w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-sm"
            placeholder="Party key"
            value={partyKey}
            onChange={(e) => onPartyKeyChange(e.target.value)}
          />
          <input
            className="w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-sm"
            placeholder="Display name"
            value={displayName}
            onChange={(e) => onDisplayNameChange(e.target.value)}
          />
          <input
            className="w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-sm"
            placeholder="Legal name"
            value={legalName}
            onChange={(e) => onLegalNameChange(e.target.value)}
          />
          <input
            className="w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-sm"
            placeholder="Tax identifier (optional)"
            value={taxIdentifier}
            onChange={(e) => onTaxIdentifierChange(e.target.value)}
          />
          <textarea
            className="w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-sm"
            placeholder="Notes"
            rows={2}
            value={notes}
            onChange={(e) => onNotesChange(e.target.value)}
          />
          <button
            type="button"
            className="rounded-lg bg-sky-600 px-4 py-2 text-sm font-medium text-white hover:bg-sky-500 disabled:opacity-50"
            disabled={isCreating || !partyKey.trim() || !displayName.trim()}
            onClick={onCreate}
          >
            {isCreating ? 'Creating…' : `Add ${title.slice(0, -1).toLowerCase()}`}
          </button>
        </div>
      ) : null}
    </section>
  )
}
