import { PageHeader, QuestionnaireFlow } from '@stl/shared-ui'
import { useComplianceCoreWorkspaceState } from '../../workspace/useComplianceCoreWorkspaceState'

export function QuestionnairesPage() {
  const state = useComplianceCoreWorkspaceState()
  if (state.handoffRedirect) return state.handoffRedirect
  if (!state.ready) return <p className="text-sm text-slate-400">{state.loadingMessage}</p>

  const complianceCoreApiBase = import.meta.env.VITE_COMPLIANCECORE_API_BASE ?? ''

  return (
    <div className="space-y-6">
      <PageHeader
        title="Questionnaires"
        subtitle="Capture fallback facts and tenant assumptions without making another product own regulatory meaning."
      />
      <section className="rounded-2xl border border-slate-800 bg-slate-950/70 p-5">
        <h2 className="text-lg font-semibold text-white">Tenant onboarding questionnaire</h2>
        <p className="mt-2 max-w-3xl text-sm text-slate-300">
          Questionnaire answers become reviewable Compliance Core facts. They can fill unknowns,
          but they do not replace mapped source data, RecordArr evidence, or product-owned
          operational records.
        </p>
        <div className="mt-5">
          <QuestionnaireFlow
            apiBase={complianceCoreApiBase}
            accessToken={state.accessToken}
            tenantId={state.session.tenantId}
            productKey="compliancecore"
            workflowKey="tenant_onboarding"
            subjectType="tenant"
            sourceRecordId={`tenant-${state.session.tenantId}`}
            sourceEntity="tenant"
            title="Compliance Core onboarding questionnaire"
            subtitle="Capture a plain-language tenant profile and seed the first compliance facts."
            submitLabel="Save questionnaire answers"
          />
        </div>
      </section>
    </div>
  )
}
