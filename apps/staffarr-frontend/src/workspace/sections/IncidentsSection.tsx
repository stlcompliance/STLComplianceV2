import { StaffArrApiError } from '../../api/client'
import { IncidentsPanel } from '../../components/IncidentsPanel'
import type { StaffArrWorkspaceState } from '../useStaffArrWorkspaceState'

type Props = { state: StaffArrWorkspaceState }

export function IncidentsSection({ state }: Props) {
  const s = state
  if (!s.selectedPerson) {
    return <p className="text-sm text-slate-400">Select a person on the People page to manage incidents.</p>
  }

  return (
    <IncidentsPanel
      personId={s.selectedPerson.personId}
      personDisplayName={s.selectedPerson.displayName}
      incidents={s.personIncidents}
      selectedIncident={s.incidentDetailQuery.data ?? null}
      isLoading={s.personIncidentsQuery.isLoading}
      isLoadingDetail={s.incidentDetailQuery.isLoading}
      canManage={s.canManagePersonIncidents}
      isSubmitting={s.createIncidentMutation.isPending}
      isRouting={s.routeIncidentToTrainarrMutation.isPending}
      errorMessage={
        s.incidentMutationError instanceof StaffArrApiError
          ? s.incidentMutationError.body || s.incidentMutationError.message
          : null
      }
      onSelectIncident={s.setSelectedIncidentId}
      onCreateIncident={async (payload) => {
        await s.createIncidentMutation.mutateAsync(payload)
      }}
      onRouteToTrainarr={async (incidentId) => {
        await s.routeIncidentToTrainarrMutation.mutateAsync(incidentId)
      }}
    />
  )
}
