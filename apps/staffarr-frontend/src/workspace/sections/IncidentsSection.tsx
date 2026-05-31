import { getErrorMessage } from '@stl/shared-ui'
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
      selectedIncidentId={s.selectedIncidentId}
      selectedIncident={s.incidentDetailQuery.data ?? null}
      isLoading={s.personIncidentsQuery.isLoading}
      isError={s.personIncidentsQuery.isError}
      readErrorMessage={
        s.personIncidentsQuery.isError
          ? getErrorMessage(
              s.personIncidentsQuery.error,
              'Failed to load personnel incidents.',
            )
          : null
      }
      onRetryRead={() => void s.personIncidentsQuery.refetch()}
      isLoadingDetail={s.incidentDetailQuery.isLoading}
      isDetailError={s.incidentDetailQuery.isError}
      detailErrorMessage={
        s.incidentDetailQuery.isError
          ? getErrorMessage(
              s.incidentDetailQuery.error,
              'Failed to load incident detail.',
            )
          : null
      }
      onRetryDetail={() => void s.incidentDetailQuery.refetch()}
      canManage={s.canManagePersonIncidents}
      isSubmitting={s.createIncidentMutation.isPending}
      isRouting={s.routeIncidentToTrainarrMutation.isPending}
      actionErrorMessage={
        s.incidentMutationError
          ? getErrorMessage(s.incidentMutationError, 'Failed to save personnel incident changes.')
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
