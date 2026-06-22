import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useEffect, useState } from 'react'

import {
  getCertificationDefinitions,
  getMyTeamDashboard,
  getPersonCertifications,
  getPersonReadiness,
  reviewMyTeamPersonnelUpdateRequest,
} from '../../api/client'
import type { ReviewPersonnelUpdateRequest } from '../../api/types'
import { loadSession } from '../../auth/sessionStorage'
import { CertificationPanel } from '../../components/CertificationPanel'
import { ReadinessPanel } from '../../components/ReadinessPanel'
import { MyTeamPanel } from '../../components/MyTeamPanel'

export function MyTeamPage() {
  const session = loadSession()
  const queryClient = useQueryClient()
  const [reviewErrorMessage, setReviewErrorMessage] = useState<string | null>(null)
  const [selectedPersonId, setSelectedPersonId] = useState<string | null>(null)

  const teamQuery = useQuery({
    queryKey: ['staffarr-my-team', session?.accessToken],
    queryFn: () => getMyTeamDashboard(session!.accessToken),
    enabled: Boolean(session?.accessToken),
  })

  useEffect(() => {
    if (
      selectedPersonId &&
      teamQuery.data?.members.some((member) => member.summary.personId === selectedPersonId)
    ) {
      return
    }

    setSelectedPersonId(teamQuery.data?.members[0]?.summary.personId ?? null)
  }, [selectedPersonId, teamQuery.data])

  const readinessQuery = useQuery({
    queryKey: ['staffarr-my-team-readiness', session?.accessToken, selectedPersonId],
    queryFn: () => getPersonReadiness(session!.accessToken, selectedPersonId!),
    enabled: Boolean(session?.accessToken && selectedPersonId),
  })
  const certificationDefinitionsQuery = useQuery({
    queryKey: ['staffarr-my-team-certification-definitions', session?.accessToken],
    queryFn: () => getCertificationDefinitions(session!.accessToken),
    enabled: Boolean(session?.accessToken),
  })
  const selectedPersonCertificationsQuery = useQuery({
    queryKey: ['staffarr-my-team-person-certifications', session?.accessToken, selectedPersonId],
    queryFn: () => getPersonCertifications(session!.accessToken, selectedPersonId!),
    enabled: Boolean(session?.accessToken && selectedPersonId),
  })

  const reviewMutation = useMutation({
    mutationFn: ({
      requestId,
      review,
    }: {
      requestId: string
      review: ReviewPersonnelUpdateRequest
    }) => reviewMyTeamPersonnelUpdateRequest(session!.accessToken, requestId, review),
    onSuccess: async () => {
      setReviewErrorMessage(null)
      await queryClient.invalidateQueries({ queryKey: ['staffarr-my-team'] })
    },
    onError: (error) => {
      console.error('My team review request failed', error)
      setReviewErrorMessage('Review failed. Please try again.')
    },
  })

  const apiError = teamQuery.error ? 'Failed to load direct reports.' : null
  const readinessErrorMessage = readinessQuery.error ? 'Failed to load readiness status.' : null
  const selectedPerson = teamQuery.data?.members.find((member) => member.summary.personId === selectedPersonId) ?? null

  useEffect(() => {
    if (teamQuery.error) {
      console.error('My team dashboard load failed', teamQuery.error)
    }
  }, [teamQuery.error])

  useEffect(() => {
    if (readinessQuery.error) {
      console.error('My team readiness load failed', readinessQuery.error)
    }
  }, [readinessQuery.error])

  useEffect(() => {
    if (certificationDefinitionsQuery.error || selectedPersonCertificationsQuery.error) {
      console.error(
        'My team certification load failed',
        certificationDefinitionsQuery.error ?? selectedPersonCertificationsQuery.error,
      )
    }
  }, [certificationDefinitionsQuery.error, selectedPersonCertificationsQuery.error])

  const handleReviewRequest = async (
    requestId: string,
    review: ReviewPersonnelUpdateRequest,
  ) => {
    setReviewErrorMessage(null)
    await reviewMutation.mutateAsync({ requestId, review })
  }

  return (
    <div className="mx-auto max-w-6xl px-4 py-6">
      <header className="mb-6">
        <h1 className="text-xl font-semibold text-[var(--color-text-primary)]">My team</h1>
        <p className="mt-1 text-sm text-[var(--color-text-muted)]">
          Review direct report readiness, certifications, incidents, and pending personnel actions.
        </p>
      </header>

      <MyTeamPanel
        dashboard={teamQuery.data ?? null}
        isLoading={teamQuery.isLoading}
        errorMessage={apiError}
        reviewingRequestId={reviewMutation.isPending ? reviewMutation.variables?.requestId : null}
        reviewErrorMessage={reviewErrorMessage}
        selectedPersonId={selectedPersonId}
        onReviewRequest={handleReviewRequest}
        onSelectPerson={setSelectedPersonId}
      />

      {selectedPerson ? (
        <div className="space-y-6">
          <ReadinessPanel
            personId={selectedPerson.summary.personId}
            personDisplayName={selectedPerson.summary.displayName}
            readiness={readinessQuery.data ?? null}
            isLoading={readinessQuery.isLoading}
            isError={Boolean(readinessQuery.error)}
            readErrorMessage={readinessErrorMessage}
            onRetryRead={() => void readinessQuery.refetch()}
            canOverride={false}
            isSubmittingOverride={false}
            overrideErrorMessage={null}
            onGrantOverride={async () => {}}
            onClearOverride={async () => {}}
          />
          <CertificationPanel
            personId={selectedPerson.summary.personId}
            personDisplayName={selectedPerson.summary.displayName}
            definitions={certificationDefinitionsQuery.data ?? []}
            certifications={selectedPersonCertificationsQuery.data ?? []}
            isLoading={certificationDefinitionsQuery.isLoading || selectedPersonCertificationsQuery.isLoading}
            isError={certificationDefinitionsQuery.isError || selectedPersonCertificationsQuery.isError}
            readErrorMessage={
              certificationDefinitionsQuery.error || selectedPersonCertificationsQuery.error
                ? 'Failed to load certifications.'
                  : null
            }
            onRetryRead={() => {
              void certificationDefinitionsQuery.refetch()
              void selectedPersonCertificationsQuery.refetch()
            }}
            canManage={false}
            isSubmitting={false}
            actionErrorMessage={null}
            onGrantCertification={async () => {}}
            onUpdateCertification={async () => {}}
          />
        </div>
      ) : (
        <section className="mt-6 rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-6">
          <p className="text-sm text-[var(--color-text-muted)]">
            Select a direct report to view their missing and expiring certifications.
          </p>
        </section>
      )}
    </div>
  )
}
