import { useSearchParams } from 'react-router-dom'
import { OrgSection } from './OrgSection'
import { LocationsAdminSection } from './LocationsAdminSection'
import type { StaffArrWorkspaceState } from '../useStaffArrWorkspaceState'

type Props = { state: StaffArrWorkspaceState }

type OrganizationStructureTab = 'organization' | 'locations'

export function OrganizationStructureSection({ state }: Props) {
  const [searchParams, setSearchParams] = useSearchParams()
  const tab = searchParams.get('tab') === 'locations' ? 'locations' : 'organization'

  const setTab = (nextTab: OrganizationStructureTab) => {
    const nextParams = new URLSearchParams(searchParams)
    nextParams.set('tab', nextTab)
    setSearchParams(nextParams, { replace: true })
  }

  return (
    <section className="space-y-6">
      <div className="flex flex-wrap items-center gap-2 rounded-xl border border-slate-800 bg-slate-950/60 p-2">
        <button
          type="button"
          onClick={() => setTab('organization')}
          className={`rounded-lg px-4 py-2 text-sm font-medium transition ${
            tab === 'organization'
              ? 'bg-sky-500/15 text-sky-200 ring-1 ring-sky-500/30'
              : 'text-slate-400 hover:bg-slate-900 hover:text-slate-200'
          }`}
        >
          Organization
        </button>
        <button
          type="button"
          onClick={() => setTab('locations')}
          className={`rounded-lg px-4 py-2 text-sm font-medium transition ${
            tab === 'locations'
              ? 'bg-sky-500/15 text-sky-200 ring-1 ring-sky-500/30'
              : 'text-slate-400 hover:bg-slate-900 hover:text-slate-200'
          }`}
        >
          Locations
        </button>
      </div>

      {tab === 'organization' ? (
        <OrgSection state={state} />
      ) : (
        <LocationsAdminSection state={state} />
      )}
    </section>
  )
}
