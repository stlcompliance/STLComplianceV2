import { useMutation, useQuery } from '@tanstack/react-query'
import { useMemo, useState, type ReactNode } from 'react'
import { ApiErrorCallout, StaticSearchPicker, getErrorMessage, type PickerOption } from '@stl/shared-ui'
import {
 exportQualificationReportSummaryCsv,
 getPointInTimeQualificationReport,
  listQualificationIssuesForReport,
  getQualificationReportSummary,
} from '../api/client'
import type { QualificationPointInTimeReportResponse } from '../api/types'

interface QualificationReportsPanelProps {
  accessToken: string
  canRead: boolean
  canExport: boolean
}

function MetricCard({ label, value }: { label: string; value: string }) {
  return (
    <div className="rounded-md border border-border bg-card px-3 py-2">
      <p className="text-xs text-muted-foreground">{label}</p>
      <p className="text-lg font-semibold text-foreground">{value}</p>
    </div>
  )
}

function SectionCard({
  title,
  children,
}: {
  title: string
  children: ReactNode
}) {
  return (
    <div className="rounded-lg border border-border bg-background/60 p-4">
      <h4 className="text-sm font-semibold text-foreground">{title}</h4>
      <div className="mt-3">{children}</div>
    </div>
  )
}

function formatDateInputValue(date: Date): string {
  const offsetMinutes = date.getTimezoneOffset()
  const localDate = new Date(date.getTime() - offsetMinutes * 60_000)
  return localDate.toISOString().slice(0, 10)
}

function formatDateTime(value: string | null | undefined): string {
  if (!value) {
    return '—'
  }

  return new Date(value).toLocaleString()
}

function renderResponseTitle(report: QualificationPointInTimeReportResponse): string {
  return report.isQualified ? 'Qualified' : 'Not qualified'
}

const staffPersonOptions: PickerOption[] = [
  { value: 'person-training-lead', label: 'Riley Chen - Training lead' },
  { value: 'person-hazmat-driver', label: 'Morgan Ellis - Hazmat driver' },
  { value: 'person-field-technician', label: 'Sam Patel - Field technician' },
  { value: 'person-compliance-reviewer', label: 'Taylor Nguyen - Compliance reviewer' },
]

export function QualificationReportsPanel({
  accessToken,
  canRead,
  canExport,
}: QualificationReportsPanelProps) {
  const [status, setStatus] = useState('all')
  const [staffarrPersonId, setStaffarrPersonId] = useState('')
  const [qualificationKey, setQualificationKey] = useState('')
  const [actionTask, setActionTask] = useState('')
  const [asOfDate, setAsOfDate] = useState(() => formatDateInputValue(new Date()))

  const summaryQuery = useQuery({
    queryKey: ['trainarr-qualification-report-summary', accessToken, status],
    queryFn: () =>
      getQualificationReportSummary(accessToken, {
        status: status === 'all' ? undefined : status,
      }),
    enabled: canRead,
  })

  const exportMutation = useMutation({
    mutationFn: () =>
      exportQualificationReportSummaryCsv(accessToken, {
        status: status === 'all' ? undefined : status,
      }),
    onSuccess: (blob) => {
      const url = URL.createObjectURL(blob)
      const anchor = document.createElement('a')
      anchor.href = url
      anchor.download = `trainarr-qualification-report-${new Date().toISOString().slice(0, 10)}.csv`
      anchor.click()
      URL.revokeObjectURL(url)
    },
  })

  const pointInTimeMutation = useMutation({
    mutationFn: () =>
      getPointInTimeQualificationReport(accessToken, {
        staffarrPersonId: staffarrPersonId.trim(),
        qualificationKey: qualificationKey.trim(),
        actionTask: actionTask.trim(),
        asOfDate,
      }),
  })

  const qualificationOptionsQuery = useQuery({
    queryKey: ['trainarr-qualification-report-qualification-options', accessToken],
    queryFn: () => listQualificationIssuesForReport(accessToken),
    enabled: canRead,
  })

  const qualificationOptions = useMemo<PickerOption[]>(
    () =>
      Array.from(
        new Map(
          (qualificationOptionsQuery.data ?? [])
            .filter((issue) => issue.qualificationKey.trim().length > 0)
            .map((issue) => [
              issue.qualificationKey,
              {
                value: issue.qualificationKey,
                label: `${issue.qualificationName} (${issue.qualificationKey})`,
              },
            ]),
        ).values(),
      ),
    [qualificationOptionsQuery.data],
  )

  if (!canRead) {
    return null
  }

  const canRunPointInTimeReport =
    staffarrPersonId.trim().length > 0
    && qualificationKey.trim().length > 0
    && actionTask.trim().length > 0
    && asOfDate.trim().length > 0

  return (
    <section
      className="mt-6 rounded-xl border border-border bg-card p-5"
      data-testid="qualification-reports-panel"
    >
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div>
          <h2 className="text-lg font-semibold text-foreground">Qualification reports</h2>
          <p className="mt-1 text-sm text-muted-foreground">
            Issued qualification lifecycle metrics, expiring credentials, point-in-time checks, and status rollups.
          </p>
        </div>
        {canExport ? (
          <button
            type="button"
            className="rounded-md bg-primary px-3 py-1.5 text-sm font-medium text-primary-foreground hover:opacity-90 disabled:opacity-50"
            disabled={exportMutation.isPending}
            onClick={() => exportMutation.mutate()}
          >
            {exportMutation.isPending ? 'Exporting…' : 'Export CSV'}
          </button>
        ) : null}
      </div>

      <div className="mt-4 flex flex-wrap gap-4 text-sm text-foreground">
        <label htmlFor="qualification-reports-status" className="flex items-center gap-2">
          <span>Status</span>
          <select
            id="qualification-reports-status"
            className="rounded border border-border bg-background px-2 py-1"
            value={status}
            onChange={(event) => setStatus(event.target.value)}
          >
            <option value="all">All</option>
            <option value="issued">Issued</option>
            <option value="expired">Expired</option>
            <option value="suspended">Suspended</option>
            <option value="revoked">Revoked</option>
          </select>
        </label>
      </div>

      {summaryQuery.isLoading && (
        <p className="mt-3 text-sm text-muted-foreground">Loading qualification report summary…</p>
      )}

      {summaryQuery.isError && (
        <div className="mt-3">
          <ApiErrorCallout
            title="Qualification report unavailable"
            message={getErrorMessage(summaryQuery.error, 'Failed to load qualification report summary.')}
            retryLabel="Retry summary"
            onRetry={() => {
              void summaryQuery.refetch()
            }}
          />
        </div>
      )}

      {exportMutation.isError && (
        <div className="mt-3">
          <ApiErrorCallout
            title="CSV export failed"
            message={getErrorMessage(exportMutation.error, 'Unable to export qualification report CSV.')}
          />
        </div>
      )}

      {summaryQuery.data && (
        <>
          <div className="mt-4 grid gap-3 text-sm sm:grid-cols-2 lg:grid-cols-4">
            <MetricCard label="Qualifications in scope" value={String(summaryQuery.data.totalQualifications)} />
            <MetricCard label="Issued" value={String(summaryQuery.data.issuedCount)} />
            <MetricCard label="Expired" value={String(summaryQuery.data.expiredCount)} />
            <MetricCard label="Expiring within 30 days" value={String(summaryQuery.data.expiringWithin30Days)} />
          </div>

          {summaryQuery.data.recentQualifications.length === 0 ? (
            <p className="mt-4 text-sm text-muted-foreground">No qualifications match this filter.</p>
          ) : (
            <div className="mt-4 overflow-x-auto">
              <table className="min-w-full text-sm">
                <thead>
                  <tr className="border-b border-border text-left text-muted-foreground">
                    <th className="px-2 py-2">Qualification</th>
                    <th className="px-2 py-2">Status</th>
                    <th className="px-2 py-2">Expires</th>
                  </tr>
                </thead>
                <tbody>
                  {summaryQuery.data.recentQualifications.slice(0, 8).map((item) => (
                    <tr key={item.qualificationIssueId} className="border-b border-border/60">
                      <td className="px-2 py-2">{item.qualificationName}</td>
                      <td className="px-2 py-2">{item.status}</td>
                      <td className="px-2 py-2">
                        {item.expiresAt ? new Date(item.expiresAt).toLocaleDateString() : '—'}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </>
      )}

      <div className="mt-6 rounded-xl border border-border bg-card/70 p-4" data-testid="point-in-time-qualification-report">
        <div className="flex flex-wrap items-start justify-between gap-3">
          <div>
            <h3 className="text-base font-semibold text-foreground">Point-in-time qualification report</h3>
            <p className="mt-1 text-sm text-muted-foreground">
              Was this person qualified for this action on this date?
            </p>
          </div>
        </div>

        <div className="mt-4 grid gap-4 lg:grid-cols-2">
          <div className="grid gap-1 text-sm text-foreground">
            <StaticSearchPicker
              id="point-in-time-person-id"
              label="Person - StaffArr"
              value={staffarrPersonId}
              onChange={setStaffarrPersonId}
              options={staffPersonOptions}
              placeholder="Search StaffArr people..."
              testId="point-in-time-person-picker"
            />
          </div>

          <div className="grid gap-1 text-sm text-foreground">
            <StaticSearchPicker
              id="point-in-time-qualification-key"
              label="Qualification key"
              value={qualificationKey}
              onChange={setQualificationKey}
              options={qualificationOptions}
              placeholder="Search qualifications…"
              testId="point-in-time-qualification-picker"
            />
            {qualificationOptionsQuery.isLoading ? (
              <p className="text-xs text-muted-foreground">Loading qualification options…</p>
            ) : null}
          </div>

          <label className="grid gap-1 text-sm text-foreground lg:col-span-2" htmlFor="point-in-time-action-task">
            Action / task
            <input
              id="point-in-time-action-task"
              className="rounded border border-border bg-background px-3 py-2 text-sm"
              value={actionTask}
              onChange={(event) => setActionTask(event.target.value)}
              placeholder="Route dispatch, inspection, material handling, etc."
            />
          </label>

          <label className="grid gap-1 text-sm text-foreground" htmlFor="point-in-time-as-of">
            As of date
            <input
              id="point-in-time-as-of"
              type="date"
              className="rounded border border-border bg-background px-3 py-2 text-sm"
              value={asOfDate}
              onChange={(event) => setAsOfDate(event.target.value)}
            />
          </label>

          <div className="flex items-end">
            <button
              type="button"
              className="rounded-md bg-primary px-4 py-2 text-sm font-medium text-primary-foreground hover:opacity-90 disabled:opacity-50"
              disabled={!canRunPointInTimeReport || pointInTimeMutation.isPending}
              onClick={() => pointInTimeMutation.mutate()}
            >
              {pointInTimeMutation.isPending ? 'Running report…' : 'Run report'}
            </button>
          </div>
        </div>

        {pointInTimeMutation.isError ? (
          <div className="mt-4">
            <ApiErrorCallout
              title="Point-in-time report unavailable"
              message={getErrorMessage(
                pointInTimeMutation.error,
                'Failed to load point-in-time qualification report.',
              )}
            />
          </div>
        ) : null}

        {pointInTimeMutation.data ? (
          <div className="mt-4 space-y-4">
            <div className="rounded-lg border border-border bg-background/70 p-4">
              <div className="flex flex-wrap items-center justify-between gap-3">
                <div>
                  <p className="text-xs uppercase tracking-wide text-muted-foreground">Outcome</p>
                  <p className="text-xl font-semibold text-foreground">{renderResponseTitle(pointInTimeMutation.data)}</p>
                  <p className="mt-1 text-sm text-muted-foreground">{pointInTimeMutation.data.qualificationMessage}</p>
                </div>
                <div className="rounded-full border border-border px-3 py-1 text-sm font-medium text-foreground">
                  {pointInTimeMutation.data.statusOnDate}
                </div>
              </div>

              <div className="mt-4 grid gap-3 text-sm md:grid-cols-2 xl:grid-cols-4">
                <MetricCard label="Person" value={pointInTimeMutation.data.staffarrPersonId} />
                <MetricCard label="Qualification" value={pointInTimeMutation.data.qualificationName} />
                <MetricCard label="Action" value={pointInTimeMutation.data.actionTask} />
                <MetricCard label="As of" value={new Date(pointInTimeMutation.data.asOfUtc).toLocaleDateString()} />
              </div>
            </div>

            <div className="grid gap-4 xl:grid-cols-2">
              <SectionCard title="Source certificate">
                {pointInTimeMutation.data.sourceCertificate ? (
                  <dl className="grid gap-2 text-sm">
                    <div className="flex flex-wrap justify-between gap-2">
                      <dt className="text-muted-foreground">Qualification issue</dt>
                      <dd className="font-mono text-foreground">{pointInTimeMutation.data.sourceCertificate.qualificationIssueId}</dd>
                    </div>
                    <div className="flex flex-wrap justify-between gap-2">
                      <dt className="text-muted-foreground">Training assignment</dt>
                      <dd className="font-mono text-foreground">{pointInTimeMutation.data.sourceCertificate.trainingAssignmentId}</dd>
                    </div>
                    <div className="flex flex-wrap justify-between gap-2">
                      <dt className="text-muted-foreground">Grant publication</dt>
                      <dd className="font-mono text-foreground">{pointInTimeMutation.data.sourceCertificate.grantPublicationId}</dd>
                    </div>
                    <div className="flex flex-wrap justify-between gap-2">
                      <dt className="text-muted-foreground">Issued</dt>
                      <dd className="text-foreground">{formatDateTime(pointInTimeMutation.data.sourceCertificate.issuedAt)}</dd>
                    </div>
                    <div className="flex flex-wrap justify-between gap-2">
                      <dt className="text-muted-foreground">Expires</dt>
                      <dd className="text-foreground">{formatDateTime(pointInTimeMutation.data.sourceCertificate.expiresAt)}</dd>
                    </div>
                    <div className="flex flex-wrap justify-between gap-2">
                      <dt className="text-muted-foreground">Status on date</dt>
                      <dd className="text-foreground">{pointInTimeMutation.data.sourceCertificate.statusOnDate}</dd>
                    </div>
                    <div className="flex flex-wrap justify-between gap-2">
                      <dt className="text-muted-foreground">Lifecycle reason</dt>
                      <dd className="text-foreground">{pointInTimeMutation.data.sourceCertificate.lifecycleReason ?? '—'}</dd>
                    </div>
                  </dl>
                ) : (
                  <p className="text-sm text-muted-foreground">No source certificate found for this date.</p>
                )}
              </SectionCard>

              <SectionCard title="Program version">
                {pointInTimeMutation.data.programVersion ? (
                  <dl className="grid gap-2 text-sm">
                    <div className="flex flex-wrap justify-between gap-2">
                      <dt className="text-muted-foreground">Program</dt>
                      <dd className="text-foreground">{pointInTimeMutation.data.programVersion.programName}</dd>
                    </div>
                    <div className="flex flex-wrap justify-between gap-2">
                      <dt className="text-muted-foreground">Program key</dt>
                      <dd className="font-mono text-foreground">{pointInTimeMutation.data.programVersion.programKey}</dd>
                    </div>
                    <div className="flex flex-wrap justify-between gap-2">
                      <dt className="text-muted-foreground">Version</dt>
                      <dd className="text-foreground">#{pointInTimeMutation.data.programVersion.versionNumber}</dd>
                    </div>
                    <div className="flex flex-wrap justify-between gap-2">
                      <dt className="text-muted-foreground">Status</dt>
                      <dd className="text-foreground">{pointInTimeMutation.data.programVersion.status}</dd>
                    </div>
                    <div className="flex flex-wrap justify-between gap-2">
                      <dt className="text-muted-foreground">Published</dt>
                      <dd className="text-foreground">{formatDateTime(pointInTimeMutation.data.programVersion.publishedAt)}</dd>
                    </div>
                    <div className="flex flex-wrap justify-between gap-2">
                      <dt className="text-muted-foreground">Training definition</dt>
                      <dd className="text-foreground">{pointInTimeMutation.data.programVersion.definitionName}</dd>
                    </div>
                  </dl>
                ) : (
                  <p className="text-sm text-muted-foreground">No program version snapshot could be resolved.</p>
                )}
              </SectionCard>
            </div>

            <div className="grid gap-4 xl:grid-cols-2">
              <SectionCard title="Expiration state">
                <dl className="grid gap-2 text-sm">
                  <div className="flex flex-wrap justify-between gap-2">
                    <dt className="text-muted-foreground">Expires at</dt>
                    <dd className="text-foreground">{formatDateTime(pointInTimeMutation.data.expirationState.expiresAt)}</dd>
                  </div>
                  <div className="flex flex-wrap justify-between gap-2">
                    <dt className="text-muted-foreground">Is expired</dt>
                    <dd className="text-foreground">{pointInTimeMutation.data.expirationState.isExpired ? 'Yes' : 'No'}</dd>
                  </div>
                  <div className="flex flex-wrap justify-between gap-2">
                    <dt className="text-muted-foreground">Days until expiration</dt>
                    <dd className="text-foreground">
                      {pointInTimeMutation.data.expirationState.daysUntilExpiration ?? '—'}
                    </dd>
                  </div>
                  <div className="flex flex-wrap justify-between gap-2">
                    <dt className="text-muted-foreground">Message</dt>
                    <dd className="text-foreground">{pointInTimeMutation.data.expirationState.message}</dd>
                  </div>
                </dl>
              </SectionCard>

              <SectionCard title="Restrictions">
                {pointInTimeMutation.data.restrictions.length === 0 ? (
                  <p className="text-sm text-muted-foreground">No restrictions were recorded for this date.</p>
                ) : (
                  <ul className="space-y-2 text-sm text-foreground">
                    {pointInTimeMutation.data.restrictions.map((restriction) => (
                      <li key={restriction} className="rounded border border-border bg-card px-3 py-2">
                        {restriction}
                      </li>
                    ))}
                  </ul>
                )}
              </SectionCard>
            </div>

            <div className="grid gap-4 xl:grid-cols-2">
              <SectionCard title="Evidence">
                {pointInTimeMutation.data.evidence.length === 0 ? (
                  <p className="text-sm text-muted-foreground">No evidence was available on or before the requested date.</p>
                ) : (
                  <div className="overflow-x-auto">
                    <table className="min-w-full text-sm">
                      <thead>
                        <tr className="border-b border-border text-left text-muted-foreground">
                          <th className="px-2 py-2">File</th>
                          <th className="px-2 py-2">Type</th>
                          <th className="px-2 py-2">Created</th>
                        </tr>
                      </thead>
                      <tbody>
                        {pointInTimeMutation.data.evidence.map((item) => (
                          <tr key={item.evidenceId} className="border-b border-border/60">
                            <td className="px-2 py-2">
                              <p className="font-medium text-foreground">{item.fileName}</p>
                              <p className="text-xs text-muted-foreground">{item.notes ?? 'No notes'}</p>
                            </td>
                            <td className="px-2 py-2">{item.evidenceTypeKey}</td>
                            <td className="px-2 py-2">{formatDateTime(item.createdAt)}</td>
                          </tr>
                        ))}
                      </tbody>
                    </table>
                  </div>
                )}
              </SectionCard>

              <SectionCard title="Signoffs">
                {pointInTimeMutation.data.signoffs.length === 0 ? (
                  <p className="text-sm text-muted-foreground">No signoffs were recorded on or before the requested date.</p>
                ) : (
                  <div className="overflow-x-auto">
                    <table className="min-w-full text-sm">
                      <thead>
                        <tr className="border-b border-border text-left text-muted-foreground">
                          <th className="px-2 py-2">Role</th>
                          <th className="px-2 py-2">Signed by</th>
                          <th className="px-2 py-2">Signed at</th>
                        </tr>
                      </thead>
                      <tbody>
                        {pointInTimeMutation.data.signoffs.map((item) => (
                          <tr key={item.signoffId} className="border-b border-border/60">
                            <td className="px-2 py-2">
                              <p className="font-medium text-foreground">{item.signoffRole}</p>
                              <p className="text-xs text-muted-foreground">{item.notes ?? 'No notes'}</p>
                            </td>
                            <td className="px-2 py-2 font-mono text-xs">{item.signedByUserId}</td>
                            <td className="px-2 py-2">{formatDateTime(item.signedAt)}</td>
                          </tr>
                        ))}
                      </tbody>
                    </table>
                  </div>
                )}
              </SectionCard>
            </div>

            <SectionCard title="Audit trail">
              {pointInTimeMutation.data.auditTrail.length === 0 ? (
                <p className="text-sm text-muted-foreground">No audit events were available on or before the requested date.</p>
              ) : (
                <div className="overflow-x-auto">
                  <table className="min-w-full text-sm">
                    <thead>
                      <tr className="border-b border-border text-left text-muted-foreground">
                        <th className="px-2 py-2">When</th>
                        <th className="px-2 py-2">Action</th>
                        <th className="px-2 py-2">Target</th>
                        <th className="px-2 py-2">Result</th>
                      </tr>
                    </thead>
                    <tbody>
                      {pointInTimeMutation.data.auditTrail.map((item) => (
                        <tr key={item.auditEventId} className="border-b border-border/60">
                          <td className="px-2 py-2">{formatDateTime(item.occurredAt)}</td>
                          <td className="px-2 py-2">
                            <p className="font-medium text-foreground">{item.action}</p>
                            <p className="text-xs text-muted-foreground">{item.reasonCode ?? '—'}</p>
                          </td>
                          <td className="px-2 py-2">
                            <p className="text-foreground">{item.targetType}</p>
                            <p className="font-mono text-xs text-muted-foreground">{item.targetId ?? '—'}</p>
                          </td>
                          <td className="px-2 py-2">
                            <p className="text-foreground">{item.result}</p>
                            <p className="font-mono text-xs text-muted-foreground">{item.actorUserId ?? 'system'}</p>
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              )}
            </SectionCard>
          </div>
        ) : (
          <p className="mt-4 text-sm text-muted-foreground">
            Enter a person, qualification, action, and date to run a point-in-time qualification report.
          </p>
        )}
      </div>
    </section>
  )
}
