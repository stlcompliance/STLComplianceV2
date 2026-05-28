import { StaffArrApiError } from '../../api/client'
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
      canManage={s.canManagePeopleProfiles}
      isSubmitting={s.grantCertificationMutation.isPending || s.updateCertificationMutation.isPending}
      errorMessage={
        s.certificationMutationError instanceof StaffArrApiError
          ? s.certificationMutationError.body || s.certificationMutationError.message
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
