import { getErrorMessage } from '@stl/shared-ui'
import { CertificationPanel } from '../../components/CertificationPanel'
import type { StaffArrWorkspaceState } from '../useStaffArrWorkspaceState'

type Props = { state: StaffArrWorkspaceState }

export function CertificationsSection({ state }: Props) {
  const s = state
  if (!s.selectedPerson) {
    return <p className="text-sm text-slate-400">Select a person on the People page to manage certifications.</p>
  }

  return (
    <CertificationPanel
      personId={s.selectedPerson.personId}
      personDisplayName={s.selectedPerson.displayName}
      definitions={s.certificationDefinitions}
      certifications={s.personCertifications}
      isLoading={s.certificationDefinitionsQuery.isLoading || s.personCertificationsQuery.isLoading}
      isError={s.certificationDefinitionsQuery.isError || s.personCertificationsQuery.isError}
      readErrorMessage={
        s.certificationDefinitionsQuery.isError
          ? getErrorMessage(
              s.certificationDefinitionsQuery.error,
              'Failed to load certification definitions.',
            )
          : s.personCertificationsQuery.isError
            ? getErrorMessage(
                s.personCertificationsQuery.error,
                'Failed to load person certifications.',
              )
            : null
      }
      onRetryRead={() => {
        void s.certificationDefinitionsQuery.refetch()
        void s.personCertificationsQuery.refetch()
      }}
      canManage={s.canManagePeopleProfiles}
      isSubmitting={s.grantCertificationMutation.isPending || s.updateCertificationMutation.isPending}
      actionErrorMessage={
        s.certificationMutationError
          ? getErrorMessage(s.certificationMutationError, 'Failed to update certification records.')
          : null
      }
      onGrantCertification={async (payload) => {
        await s.grantCertificationMutation.mutateAsync({
          personId: s.selectedPerson!.personId,
          ...payload,
        })
      }}
      onUpdateCertification={async (personCertificationId, payload) => {
        await s.updateCertificationMutation.mutateAsync({
          personId: s.selectedPerson!.personId,
          personCertificationId,
          ...payload,
        })
      }}
    />
  )
}
