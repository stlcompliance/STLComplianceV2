import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import {
  getMePortalSummary,
  listMyPersonnelIncidents,
  listMyPersonnelUpdateRequests,
  submitPersonnelUpdateRequest,
  submitSelfReportedPersonnelIncident,
} from '../../api/client'
import { loadSession } from '../../auth/sessionStorage'
import { MeSelfServicePortalPanel } from '../../components/MeSelfServicePortalPanel'
import type {
  SubmitPersonnelUpdateRequest,
  SubmitSelfReportedPersonnelIncidentRequest,
} from '../../api/types'

export function MePage() {
  const session = loadSession()
  const queryClient = useQueryClient()

  const portalQuery = useQuery({
    queryKey: ['staffarr-me-portal', session?.accessToken],
    queryFn: () => getMePortalSummary(session!.accessToken),
    enabled: Boolean(session?.accessToken),
  })

  const requestsQuery = useQuery({
    queryKey: ['staffarr-me-update-requests', session?.accessToken],
    queryFn: () => listMyPersonnelUpdateRequests(session!.accessToken),
    enabled: Boolean(session?.accessToken),
  })

  const incidentsQuery = useQuery({
    queryKey: ['staffarr-me-incidents', session?.accessToken],
    queryFn: () => listMyPersonnelIncidents(session!.accessToken),
    enabled: Boolean(session?.accessToken),
  })

  const submitUpdateMutation = useMutation({
    mutationFn: (request: SubmitPersonnelUpdateRequest) =>
      submitPersonnelUpdateRequest(session!.accessToken, request),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['staffarr-me-update-requests'] })
    },
  })

  const submitIncidentMutation = useMutation({
    mutationFn: (request: SubmitSelfReportedPersonnelIncidentRequest) =>
      submitSelfReportedPersonnelIncident(session!.accessToken, request),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['staffarr-me-incidents'] })
    },
  })

  const apiError =
    portalQuery.error instanceof Error
      ? portalQuery.error.message
      : submitUpdateMutation.error instanceof Error
        ? submitUpdateMutation.error.message
        : submitIncidentMutation.error instanceof Error
          ? submitIncidentMutation.error.message
          : null

  return (
    <div className="mx-auto max-w-5xl px-4 py-6">
      <header className="mb-6">
        <h1 className="text-xl font-semibold text-slate-100">My workforce profile</h1>
        <p className="mt-1 text-sm text-slate-400">
          View your assignments, readiness, certifications, submit personnel update requests, and
          report incidents.
        </p>
      </header>

      <MeSelfServicePortalPanel
        summary={portalQuery.data ?? null}
        updateRequests={requestsQuery.data ?? []}
        incidentReports={incidentsQuery.data ?? []}
        isLoading={portalQuery.isLoading}
        isSubmittingUpdate={submitUpdateMutation.isPending}
        isSubmittingIncident={submitIncidentMutation.isPending}
        errorMessage={apiError}
        onSubmitUpdateRequest={async (request) => {
          await submitUpdateMutation.mutateAsync(request)
        }}
        onSubmitIncidentReport={async (request) => {
          await submitIncidentMutation.mutateAsync(request)
        }}
      />
    </div>
  )
}
