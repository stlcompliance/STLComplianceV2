import { useMutation, useQuery } from '@tanstack/react-query'
import { useState } from 'react'
import { ShieldCheck } from 'lucide-react'
import { ApiErrorCallout, getErrorMessage } from '@stl/shared-ui'

import {
  exportComplianceReportSummaryCsv,
  getCompliancePartyDetail,
  getComplianceReportSummary,
} from '../api/client'

interface ComplianceReportsPanelProps {
  accessToken: string
  canRead: boolean
  canExport: boolean
}

function formatPosture(posture: string): string {
  return posture.replace(/_/g, ' ')
}

export function ComplianceReportsPanel({
  accessToken,
  canRead,
  canExport,
}: ComplianceReportsPanelProps) {
  const [attentionOnly, setAttentionOnly] = useState(true)
  const [selectedPartyId, setSelectedPartyId] = useState<string | null>(null)

  const summaryQuery = useQuery({
    queryKey: ['supplyarr-compliance-report-summary', accessToken, attentionOnly],
    queryFn: () =>
      getComplianceReportSummary(accessToken, {
        attentionOnly: attentionOnly || undefined,
      }),
    enabled: canRead,
  })

  const partyDetailQuery = useQuery({
    queryKey: ['supplyarr-compliance-party-detail', accessToken, selectedPartyId],
    queryFn: () => getCompliancePartyDetail(accessToken, selectedPartyId!),
    enabled: canRead && Boolean(selectedPartyId),
  })

  const exportMutation = useMutation({
    mutationFn: () =>
      exportComplianceReportSummaryCsv(accessToken, {
        attentionOnly: attentionOnly || undefined,
      }),
    onSuccess: (blob) => {
      const url = URL.createObjectURL(blob)
      const anchor = document.createElement('a')
      anchor.href = url
      anchor.download = `supplyarr-compliance-${new Date().toISOString().slice(0, 10)}.csv`
      anchor.click()
      URL.revokeObjectURL(url)
    },
  })

  if (!canRead) {
    return null
  }

  return (
    <section
      className="rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-5 lg:col-span-2 shadow-[var(--shadow-surface)]"
      data-testid="compliance-reports-panel"
    >
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div className="flex gap-3">
          <ShieldCheck className="mt-0.5 h-5 w-5 text-[var(--color-success)]" aria-hidden />
          <div>
            <h2 className="text-lg font-semibold text-[var(--color-text-primary)]">Compliance reports</h2>
            <p className="mt-1 text-sm text-[var(--color-text-secondary)]">
              Supplier compliance documents, review status, and expiration rollups.
            </p>
          </div>
        </div>
        {canExport ? (
          <button
            type="button"
            className="rounded-md bg-[var(--color-accent)] px-3 py-1.5 text-sm font-medium text-[var(--color-on-accent)] hover:bg-[var(--color-accent-hover)] disabled:opacity-50"
            disabled={exportMutation.isPending}
            onClick={() => exportMutation.mutate()}
          >
            {exportMutation.isPending ? 'Exporting…' : 'Export CSV'}
          </button>
        ) : null}
      </div>

      <div className="mt-4 flex flex-wrap gap-3 text-sm">
        <label htmlFor="compliance-report-attention-only" className="flex items-center gap-2 text-[var(--color-text-secondary)]">
          <input
            id="compliance-report-attention-only"
            type="checkbox"
            checked={attentionOnly}
            onChange={(event) => setAttentionOnly(event.target.checked)}
          />
          Attention only (expired, expiring, pending review)
        </label>
      </div>

      {summaryQuery.isLoading && (
        <p className="mt-3 text-sm text-[var(--color-text-muted)]">Loading compliance summary…</p>
      )}

      {summaryQuery.isError && (
        <div className="mt-3">
          <ApiErrorCallout
            title="Compliance report unavailable"
            message={getErrorMessage(summaryQuery.error, 'Failed to load compliance report.')}
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
            message={getErrorMessage(exportMutation.error, 'Unable to export compliance report CSV.')}
          />
        </div>
      )}

      {summaryQuery.data && (
        <>
          <div className="mt-4 flex flex-wrap gap-2 text-xs">
            <span className="rounded-md border border-[var(--color-border-subtle)] bg-[var(--color-bg-control)] px-2 py-1 text-[var(--color-text-secondary)]">
              Parties: {summaryQuery.data.totals.partyCount}
            </span>
            <span className="rounded-md border border-[var(--color-border-subtle)] bg-[var(--color-bg-control)] px-2 py-1 text-[var(--color-text-secondary)]">
              Documents: {summaryQuery.data.totals.documentCount}
            </span>
            <span className="rounded-md border border-[var(--tone-danger-border)] bg-[var(--tone-danger-bg)] px-2 py-1 text-[var(--tone-danger-text)]">
              Expired: {summaryQuery.data.totals.expiredCount}
            </span>
            <span className="rounded-md border border-[var(--tone-warning-border)] bg-[var(--tone-warning-bg)] px-2 py-1 text-[var(--tone-warning-text)]">
              Expiring soon: {summaryQuery.data.totals.expiringSoonCount}
            </span>
            <span className="rounded-md border border-[var(--color-accent-border)] bg-[var(--color-accent-soft)] px-2 py-1 text-[var(--color-accent)]">
              Pending review: {summaryQuery.data.totals.reviewPendingCount}
            </span>
          </div>

          <div className="mt-4 overflow-x-auto">
            <table className="min-w-full text-left text-sm">
              <thead className="text-xs uppercase text-[var(--color-text-muted)]">
                <tr>
                  <th className="px-2 py-2">Party</th>
                  <th className="px-2 py-2">Posture</th>
                  <th className="px-2 py-2">Docs</th>
                  <th className="px-2 py-2">Expired</th>
                  <th className="px-2 py-2">Expiring</th>
                </tr>
              </thead>
              <tbody>
                {summaryQuery.data.parties.map((party) => (
                  <tr
                    key={party.externalPartyId}
                    className={`cursor-pointer border-t border-[var(--color-border-subtle)] hover:bg-[var(--color-bg-control-hover)] ${
                      selectedPartyId === party.externalPartyId ? 'bg-[var(--color-bg-control-hover)]' : ''
                    }`}
                    onClick={() => setSelectedPartyId(party.externalPartyId)}
                  >
                    <td className="px-2 py-2 text-[var(--color-text-primary)]">
                      {party.partyKey} · {party.displayName}
                    </td>
                    <td className="px-2 py-2 capitalize text-[var(--color-text-secondary)]">
                      {formatPosture(party.compliancePosture)}
                    </td>
                    <td className="px-2 py-2 text-[var(--color-text-muted)]">{party.documentCount}</td>
                    <td className="px-2 py-2 text-[var(--tone-danger-text)]">{party.expiredCount}</td>
                    <td className="px-2 py-2 text-[var(--tone-warning-text)]">{party.expiringSoonCount}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>

          <div className="mt-4 overflow-x-auto">
            <table className="min-w-full text-left text-sm">
              <thead className="text-xs uppercase text-[var(--color-text-muted)]">
                <tr>
                  <th className="px-2 py-2">Document</th>
                  <th className="px-2 py-2">Type</th>
                  <th className="px-2 py-2">Status</th>
                  <th className="px-2 py-2">Expires</th>
                </tr>
              </thead>
              <tbody>
                {summaryQuery.data.documents.map((doc) => (
                  <tr
                    key={doc.documentId}
                    className="border-t border-[var(--color-border-subtle)]"
                    onClick={() => setSelectedPartyId(doc.externalPartyId)}
                  >
                    <td className="px-2 py-2 text-[var(--color-text-primary)]">
                      {doc.documentKey} · {doc.title}
                    </td>
                    <td className="px-2 py-2 text-[var(--color-text-muted)]">{doc.documentTypeKey}</td>
                    <td className="px-2 py-2 capitalize text-[var(--color-text-secondary)]">{doc.effectiveStatus}</td>
                    <td className="px-2 py-2 text-[var(--color-text-muted)]">
                      {doc.expiresAt ? new Date(doc.expiresAt).toLocaleDateString() : '—'}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </>
      )}

      {selectedPartyId && partyDetailQuery.data && (
        <div className="mt-6 rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-elevated)] p-4">
          <h3 className="text-sm font-semibold text-[var(--color-text-primary)]">
            {partyDetailQuery.data.summary.partyKey} · {partyDetailQuery.data.summary.displayName}
          </h3>
          <p className="mt-1 text-xs text-[var(--color-text-muted)]">
            Approval: {partyDetailQuery.data.summary.approvalStatus} · Posture:{' '}
            {formatPosture(partyDetailQuery.data.summary.compliancePosture)}
          </p>
          <ul className="mt-3 space-y-2 text-sm">
            {partyDetailQuery.data.documents.map((doc) => (
              <li key={doc.documentId} className="rounded-md border border-[var(--color-border-subtle)] bg-[var(--color-bg-control)] px-3 py-2">
                <span className="font-medium text-[var(--color-text-primary)]">
                  {doc.documentKey} v{doc.version}
                </span>
                <span className="ml-2 capitalize text-[var(--color-text-muted)]">{doc.effectiveStatus}</span>
                {doc.expiresAt ? (
                  <span className="ml-2 text-[var(--color-text-muted)]">
                    expires {new Date(doc.expiresAt).toLocaleDateString()}
                  </span>
                ) : null}
              </li>
            ))}
          </ul>
        </div>
      )}
    </section>
  )
}
