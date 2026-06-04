import type {
  WorkOrderDetailResponse,
  WorkOrderEvidenceResponse,
  WorkOrderLaborEntryResponse,
  WorkOrderTaskLineResponse,
} from '../api/types'

const LIFECYCLE_STEPS = ['open', 'in_progress', 'completed', 'cancelled'] as const

type LifecycleStep = (typeof LIFECYCLE_STEPS)[number]

interface WorkOrderLifecyclePanelProps {
  workOrder: WorkOrderDetailResponse | null
  tasks: WorkOrderTaskLineResponse[]
  labor: WorkOrderLaborEntryResponse[]
  evidence: WorkOrderEvidenceResponse[]
  isDetailLoading: boolean
}

function stepLabel(step: LifecycleStep): string {
  if (step === 'in_progress') return 'In progress'
  if (step === 'open') return 'Open'
  if (step === 'completed') return 'Completed'
  return 'Cancelled'
}

function stepIndex(status: string): number {
  const idx = LIFECYCLE_STEPS.indexOf(status as LifecycleStep)
  return idx >= 0 ? idx : 0
}

function formatTimestamp(value: string | null): string {
  if (!value) return '—'
  return new Date(value).toLocaleString()
}

function totalLaborHours(labor: WorkOrderLaborEntryResponse[]): number {
  return labor.reduce((sum, entry) => sum + entry.hoursWorked, 0)
}

function formatEvidenceLabel(
  evidenceId: string,
  evidence: WorkOrderEvidenceResponse[],
): string {
  const item = evidence.find((entry) => entry.evidenceId === evidenceId)
  if (!item) {
    return `${evidenceId.slice(0, 8)}`
  }

  return `${item.fileName} (${item.evidenceTypeKey.replaceAll('_', ' ')})`
}

function splitReferenceList(value: string | null): string[] {
  if (!value) {
    return []
  }

  return value
    .split(/[\n,;]+/)
    .map((part) => part.trim())
    .filter((part) => part.length > 0)
}

export function WorkOrderLifecyclePanel({
  workOrder,
  tasks,
  labor,
  evidence,
  isDetailLoading,
}: WorkOrderLifecyclePanelProps) {
  if (!workOrder) {
    return (
      <section
        className="rounded-lg border border-dashed border-slate-700 bg-slate-950/30 p-4"
        data-testid="work-order-lifecycle-panel"
      >
        <h3 className="text-sm font-semibold text-white">Work order lifecycle</h3>
        <p className="mt-2 text-sm text-slate-400" data-testid="work-order-lifecycle-empty">
          Select a work order to review status progression, labor, and evidence capture.
        </p>
      </section>
    )
  }

  const currentIdx = stepIndex(workOrder.status)
  const laborHoursTotal = totalLaborHours(labor)
  const completionReady =
    workOrder.status === 'in_progress' && tasks.length > 0 && labor.length > 0 && evidence.length > 0
  const closeout = workOrder.closeout
  const acceptedEvidenceLabels = closeout?.evidenceRecordRefs?.map((evidenceId) =>
    formatEvidenceLabel(evidenceId, evidence),
  ) ?? []
  const unresolvedDefectRefs = splitReferenceList(closeout?.unresolvedDefectRefs ?? null)
  const followUpWorkOrderRefs = splitReferenceList(closeout?.followUpWorkOrderRefs ?? null)

  return (
    <section
      className="rounded-lg border border-slate-800 bg-slate-950/50 p-4"
      data-testid="work-order-lifecycle-panel"
    >
      <h3 className="text-sm font-semibold text-white">Work order lifecycle</h3>
      <p className="mt-1 text-xs text-slate-500">
        docs/15 — open through completion with labor and evidence for defensible maintenance records.
      </p>

      {isDetailLoading ? (
        <p className="mt-3 text-sm text-slate-400" data-testid="work-order-lifecycle-loading">
          Loading lifecycle…
        </p>
      ) : (
        <div className="mt-4 space-y-4" data-testid="work-order-lifecycle-content">
          <ol
            className="flex flex-wrap gap-2"
            data-testid="work-order-lifecycle-stepper"
            aria-label="Work order status progression"
          >
            {LIFECYCLE_STEPS.map((step, index) => {
              const isCurrent = workOrder.status === step
              const isPast = index < currentIdx && workOrder.status !== 'cancelled'
              const isTerminal = workOrder.status === 'cancelled' && step === 'cancelled'
              const active = isCurrent || isPast || isTerminal
              return (
                <li
                  key={step}
                  data-testid={`work-order-lifecycle-step-${step}`}
                  className={`rounded-full px-3 py-1 text-xs ring-1 ring-inset ${
                    isCurrent
                      ? 'bg-sky-500/20 text-sky-200 ring-sky-500/50'
                      : active
                        ? 'bg-emerald-500/10 text-emerald-200 ring-emerald-500/30'
                        : 'bg-slate-800/80 text-slate-500 ring-slate-600/50'
                  }`}
                >
                  {stepLabel(step)}
                </li>
              )
            })}
          </ol>

          <dl
            className="grid gap-2 text-sm sm:grid-cols-2"
            data-testid="work-order-lifecycle-timestamps"
          >
            <div>
              <dt className="text-slate-500">Created</dt>
              <dd className="text-slate-200">{formatTimestamp(workOrder.createdAt)}</dd>
            </div>
            <div>
              <dt className="text-slate-500">Started</dt>
              <dd className="text-slate-200">{formatTimestamp(workOrder.startedAt)}</dd>
            </div>
            <div>
              <dt className="text-slate-500">Completed</dt>
              <dd className="text-slate-200">{formatTimestamp(workOrder.completedAt)}</dd>
            </div>
            <div>
              <dt className="text-slate-500">Cancelled</dt>
              <dd className="text-slate-200">{formatTimestamp(workOrder.cancelledAt)}</dd>
            </div>
          </dl>

          <div data-testid="work-order-completion-signals">
            <h4 className="text-xs font-semibold uppercase tracking-wide text-slate-400">
              Completion signals
            </h4>
            <ul className="mt-2 grid gap-2 text-sm sm:grid-cols-3">
              <li
                className="rounded border border-slate-700 bg-slate-950/40 px-3 py-2"
                data-testid="work-order-signal-tasks"
              >
                <span className="text-slate-500">Tasks</span>
                <span className="ml-2 font-medium text-white">{tasks.length}</span>
              </li>
              <li
                className="rounded border border-slate-700 bg-slate-950/40 px-3 py-2"
                data-testid="work-order-signal-labor"
              >
                <span className="text-slate-500">Labor hours</span>
                <span className="ml-2 font-medium text-white">{laborHoursTotal.toFixed(2)}</span>
              </li>
              <li
                className="rounded border border-slate-700 bg-slate-950/40 px-3 py-2"
                data-testid="work-order-signal-evidence"
              >
                <span className="text-slate-500">Evidence</span>
                <span className="ml-2 font-medium text-white">{evidence.length}</span>
              </li>
            </ul>
          </div>

          {closeout ? (
            <section
              className="rounded-lg border border-slate-700 bg-slate-950/40 p-3"
              data-testid="work-order-closeout-summary"
            >
              <h4 className="text-xs font-semibold uppercase tracking-wide text-slate-400">
                Closeout
              </h4>
              <p className="mt-2 text-sm text-slate-200">{closeout.completionSummary}</p>
              <dl className="mt-2 grid gap-2 text-sm sm:grid-cols-2">
                <div>
                  <dt className="text-slate-500">Final status</dt>
                  <dd className="text-slate-200">{closeout.finalStatus ?? '—'}</dd>
                </div>
                <div>
                  <dt className="text-slate-500">Root cause</dt>
                  <dd className="text-slate-200">{closeout.rootCause ?? '—'}</dd>
                </div>
                <div>
                  <dt className="text-slate-500">Corrective action</dt>
                  <dd className="text-slate-200">{closeout.correctiveAction ?? '—'}</dd>
                </div>
                <div>
                  <dt className="text-slate-500">Preventive recommendation</dt>
                  <dd className="text-slate-200">{closeout.preventiveActionRecommendation ?? '—'}</dd>
                </div>
                <div>
                  <dt className="text-slate-500">Evidence accepted</dt>
                  <dd className="text-slate-200">{closeout.evidenceAccepted ? 'Yes' : 'No'}</dd>
                </div>
                <div>
                  <dt className="text-slate-500">Returned to service</dt>
                  <dd className="text-slate-200">
                    {closeout.assetReturnedToService
                      ? formatTimestamp(closeout.returnToServiceAt)
                      : 'No'}
                  </dd>
                </div>
                <div>
                  <dt className="text-slate-500">Returned by</dt>
                  <dd className="text-slate-200">{closeout.returnToServiceByPersonId ?? '—'}</dd>
                </div>
                <div>
                  <dt className="text-slate-500">Readiness</dt>
                  <dd className="text-slate-200">{closeout.finalAssetReadinessStatus ?? '—'}</dd>
                </div>
                <div>
                  <dt className="text-slate-500">Post-repair inspection</dt>
                  <dd className="text-slate-200">
                    {closeout.postRepairInspectionRequired
                      ? closeout.postRepairInspectionRef ?? 'Required'
                      : 'Not required'}
                  </dd>
                </div>
                <div>
                  <dt className="text-slate-500">Supervisor review</dt>
                  <dd className="text-slate-200">
                    {closeout.supervisorReviewRequired
                      ? `${closeout.supervisorReviewedByPersonId ?? 'Required'}${closeout.supervisorReviewedAt ? ` on ${formatTimestamp(closeout.supervisorReviewedAt)}` : ''}`
                      : 'Not required'}
                  </dd>
                </div>
                <div>
                  <dt className="text-slate-500">Compliance review</dt>
                  <dd className="text-slate-200">
                    {closeout.complianceReviewRequired
                      ? `${closeout.complianceReviewedByPersonId ?? 'Required'}${closeout.complianceReviewedAt ? ` on ${formatTimestamp(closeout.complianceReviewedAt)}` : ''}`
                      : 'Not required'}
                  </dd>
                </div>
                <div>
                  <dt className="text-slate-500">Quality review</dt>
                  <dd className="text-slate-200">
                    {closeout.qualityReviewRequired
                      ? `${closeout.qualityReviewedByPersonId ?? 'Required'}${closeout.qualityReviewedAt ? ` on ${formatTimestamp(closeout.qualityReviewedAt)}` : ''}`
                      : 'Not required'}
                  </dd>
                </div>
                <div className="sm:col-span-2">
                  <dt className="text-slate-500">Customer impact</dt>
                  <dd className="text-slate-200">{closeout.customerImpactSummary ?? '—'}</dd>
                </div>
                <div className="sm:col-span-2">
                  <dt className="text-slate-500">Downtime summary</dt>
                  <dd className="text-slate-200">{closeout.downtimeSummary ?? '—'}</dd>
                </div>
              </dl>
              <div className="mt-3 grid gap-3 sm:grid-cols-2">
                <section data-testid="work-order-closeout-unresolved-defects">
                  <h5 className="text-xs font-semibold uppercase tracking-wide text-slate-400">
                    Unresolved defects
                  </h5>
                  {unresolvedDefectRefs.length > 0 ? (
                    <ul className="mt-2 flex flex-wrap gap-2">
                      {unresolvedDefectRefs.map((ref) => (
                        <li
                          key={`${closeout.closeoutId}-defect-${ref}`}
                          className="rounded-full border border-slate-700 bg-slate-900 px-3 py-1 text-xs text-slate-200"
                        >
                          {ref}
                        </li>
                      ))}
                    </ul>
                  ) : (
                    <p className="mt-2 text-xs text-slate-500">None</p>
                  )}
                </section>
                <section data-testid="work-order-closeout-followups">
                  <h5 className="text-xs font-semibold uppercase tracking-wide text-slate-400">
                    Follow-up work orders
                  </h5>
                  {followUpWorkOrderRefs.length > 0 ? (
                    <ul className="mt-2 flex flex-wrap gap-2">
                      {followUpWorkOrderRefs.map((ref) => (
                        <li
                          key={`${closeout.closeoutId}-followup-${ref}`}
                          className="rounded-full border border-slate-700 bg-slate-900 px-3 py-1 text-xs text-slate-200"
                        >
                          {ref}
                        </li>
                      ))}
                    </ul>
                  ) : (
                    <p className="mt-2 text-xs text-slate-500">None</p>
                  )}
                </section>
              </div>
              <div className="mt-3" data-testid="work-order-closeout-evidence">
                <h5 className="text-xs font-semibold uppercase tracking-wide text-slate-400">
                  Accepted evidence
                </h5>
                {acceptedEvidenceLabels.length > 0 ? (
                  <ul className="mt-2 flex flex-wrap gap-2">
                    {acceptedEvidenceLabels.map((label, index) => (
                      <li
                        key={`${closeout.closeoutId}-evidence-${index}`}
                        className="rounded-full border border-slate-700 bg-slate-900 px-3 py-1 text-xs text-slate-200"
                      >
                        {label}
                      </li>
                    ))}
                  </ul>
                ) : (
                  <p className="mt-2 text-xs text-slate-500">No closeout evidence was attached.</p>
                )}
              </div>
            </section>
          ) : null}

          {workOrder.status === 'in_progress' ? (
            <p
              className={`text-xs ${completionReady ? 'text-emerald-300' : 'text-amber-300'}`}
              data-testid="work-order-lifecycle-completion-hint"
            >
              {completionReady
                ? 'Tasks, labor, and evidence captured — ready to mark completed from the list.'
                : 'Add at least one task, labor entry, and evidence file before completing the work order.'}
            </p>
          ) : null}
        </div>
      )}
    </section>
  )
}
