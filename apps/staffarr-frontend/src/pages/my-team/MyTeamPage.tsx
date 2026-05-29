import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'
import { getMyTeamDashboard, reviewMyTeamPersonnelUpdateRequest } from '../../api/client'
import type { ReviewPersonnelUpdateRequest } from '../../api/types'
import { loadSession } from '../../auth/sessionStorage'
import { MyTeamPanel } from '../../components/MyTeamPanel'

export function MyTeamPage() {
  const session = loadSession()
  const queryClient = useQueryClient()
  const [reviewErrorMessage, setReviewErrorMessage] = useState<string | null>(null)

  const teamQuery = useQuery({
    queryKey: ['staffarr-my-team', session?.accessToken],
    queryFn: () => getMyTeamDashboard(session!.accessToken),
    enabled: Boolean(session?.accessToken),
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
      setReviewErrorMessage(error instanceof Error ? error.message : 'Review failed.')
    },
  })

  const apiError = teamQuery.error instanceof Error ? teamQuery.error.message : null

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
        <h1 className="text-xl font-semibold text-slate-100">My team</h1>
        <p className="mt-1 text-sm text-slate-400">
          Review direct report readiness, certifications, incidents, and pending personnel actions.
        </p>
      </header>

      <MyTeamPanel
        dashboard={teamQuery.data ?? null}
        isLoading={teamQuery.isLoading}
        errorMessage={apiError}
        reviewingRequestId={reviewMutation.isPending ? reviewMutation.variables?.requestId : null}
        reviewErrorMessage={reviewErrorMessage}
        onReviewRequest={handleReviewRequest}
      />
    </div>
  )
}
