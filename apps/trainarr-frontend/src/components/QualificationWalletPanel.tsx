import { useMutation, useQuery } from '@tanstack/react-query'
import { useEffect, useMemo, useState, type ReactNode } from 'react'
import { ApiErrorCallout, getErrorMessage } from '@stl/shared-ui'
import {
  getQualificationWalletCredential,
  verifyQualificationWalletCredential,
} from '../api/client'
import type {
  QualificationIssueListItemResponse,
  QualificationWalletCredentialResponse,
} from '../api/types'

interface QualificationWalletPanelProps {
  accessToken: string
  issues: QualificationIssueListItemResponse[]
  selectedIssueId: string | null
  canManage: boolean
}

function formatDateTime(value: string | null | undefined): string {
  if (!value) {
    return '—'
  }

  return new Date(value).toLocaleString()
}

function SectionCard({ title, children }: { title: string; children: ReactNode }) {
  return (
    <div className="rounded-lg border border-slate-700 bg-slate-950/40 p-4">
      <h3 className="text-xs font-semibold uppercase tracking-wide text-[var(--color-text-muted)]">{title}</h3>
      <div className="mt-3">{children}</div>
    </div>
  )
}

function MetricCard({ label, value }: { label: string; value: string }) {
  return (
    <div className="rounded border border-slate-700 bg-slate-900/60 px-3 py-2">
      <p className="text-xs text-[var(--color-text-muted)]">{label}</p>
      <p className="text-sm font-medium text-slate-100">{value}</p>
    </div>
  )
}

function credentialToJson(credential: QualificationWalletCredentialResponse): string {
  return JSON.stringify(credential, null, 2)
}

export function QualificationWalletPanel({
  accessToken,
  issues,
  selectedIssueId,
  canManage,
}: QualificationWalletPanelProps) {
  const selected = useMemo(
    () => issues.find((item) => item.qualificationIssueId === selectedIssueId) ?? null,
    [issues, selectedIssueId],
  )
  const [verificationToken, setVerificationToken] = useState('')

  const credentialQuery = useQuery({
    queryKey: ['trainarr-qualification-wallet-credential', accessToken, selectedIssueId],
    queryFn: () => getQualificationWalletCredential(accessToken, selectedIssueId!),
    enabled: Boolean(canManage && selectedIssueId),
  })

  const verifyMutation = useMutation({
    mutationFn: () =>
      verifyQualificationWalletCredential(accessToken, {
        credentialToken: verificationToken.trim(),
      }),
  })

  useEffect(() => {
    if (credentialQuery.data) {
      setVerificationToken(credentialQuery.data.credentialToken)
    }
  }, [credentialQuery.data])

  if (!canManage) {
    return (
      <section className="rounded-xl border border-slate-700 bg-slate-900/60 p-4">
        <h2 className="text-sm font-semibold uppercase tracking-wide text-slate-400">Digital wallet certificates</h2>
        <p className="mt-3 text-sm text-slate-400">
          Qualification wallet credentials require trainarr qualifications manage access.
        </p>
      </section>
    )
  }

  const credentialJson = credentialQuery.data ? credentialToJson(credentialQuery.data) : ''
  const canVerify = verificationToken.trim().length > 0

  return (
    <section
      className="rounded-xl border border-slate-700 bg-slate-900/60 p-4"
      data-testid="qualification-wallet-panel"
    >
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div>
          <h2 className="text-sm font-semibold uppercase tracking-wide text-slate-400">Digital wallet certificates</h2>
          <p className="mt-1 text-xs text-[var(--color-text-muted)]">
            Issue a signed credential token for the selected qualification and verify it like a smart badge scan.
          </p>
        </div>
      </div>

      {selected ? (
        <div className="mt-4 rounded border border-slate-700 bg-slate-950/40 p-3 text-sm text-slate-300">
          <p>
            Selected: <span className="text-slate-100">{selected.qualificationName}</span>
          </p>
          <p className="mt-1 text-xs text-[var(--color-text-muted)]">
            {selected.status} · {selected.qualificationName} · Person selected
          </p>
        </div>
      ) : (
        <p className="mt-4 text-sm text-slate-400">Select a qualification issue in the management panel to issue a wallet credential.</p>
      )}

      {credentialQuery.isError ? (
        <div className="mt-4">
          <ApiErrorCallout
            title="Credential unavailable"
            message={getErrorMessage(credentialQuery.error, 'Failed to load qualification wallet credential.')}
            retryLabel="Retry credential"
            onRetry={() => {
              void credentialQuery.refetch()
            }}
          />
        </div>
      ) : null}

      {selected && credentialQuery.isLoading ? (
        <p className="mt-4 text-sm text-slate-400">Loading credential token…</p>
      ) : null}

      {selected && credentialQuery.data ? (
        <div className="mt-4 space-y-4">
          <SectionCard title="Credential card">
            <div className="grid gap-3 md:grid-cols-2 xl:grid-cols-4">
              <MetricCard label="Credential" value={credentialQuery.data.displayLabel} />
              <MetricCard label="Issued" value={formatDateTime(credentialQuery.data.issuedAt)} />
              <MetricCard label="Expires" value={formatDateTime(credentialQuery.data.expiresAt)} />
              <MetricCard label="Status" value={credentialQuery.data.status} />
            </div>
            <div className="mt-4 grid gap-4 xl:grid-cols-2">
              <div className="space-y-2">
                <label htmlFor="wallet-verification-url" className="block text-xs font-semibold uppercase tracking-wide text-[var(--color-text-muted)]">
                  Verification URL
                </label>
                <input
                  id="wallet-verification-url"
                  readOnly
                  value={credentialQuery.data.verificationUrl}
                  className="w-full rounded border border-slate-700 bg-slate-950 px-3 py-2 font-mono text-xs text-slate-100"
                />
              </div>
              <div className="space-y-2">
                <label htmlFor="wallet-credential-token" className="block text-xs font-semibold uppercase tracking-wide text-[var(--color-text-muted)]">
                  Signed token
                </label>
                <textarea
                  id="wallet-credential-token"
                  readOnly
                  value={credentialQuery.data.credentialToken}
                  rows={6}
                  className="w-full rounded border border-slate-700 bg-slate-950 px-3 py-2 font-mono text-[11px] text-slate-100"
                />
              </div>
            </div>
            <div className="mt-4 flex flex-wrap gap-2">
              <button
                type="button"
                className="rounded border border-slate-600 px-3 py-1.5 text-xs text-slate-100 hover:bg-slate-800"
                onClick={async () => {
                  await navigator.clipboard.writeText(credentialQuery.data.credentialToken)
                }}
              >
                Copy token
              </button>
              <button
                type="button"
                className="rounded border border-slate-600 px-3 py-1.5 text-xs text-slate-100 hover:bg-slate-800"
                onClick={() => {
                  const blob = new Blob([credentialJson], { type: 'application/json' })
                  const url = URL.createObjectURL(blob)
                  const anchor = document.createElement('a')
                  anchor.href = url
                  anchor.download = `trainarr-wallet-credential-${credentialQuery.data.qualificationIssueId}.json`
                  anchor.click()
                  URL.revokeObjectURL(url)
                }}
              >
                Download JSON
              </button>
            </div>
          </SectionCard>

          <SectionCard title="Smart badge verification">
            <div className="space-y-3">
              <label htmlFor="wallet-verification-token" className="grid gap-1 text-sm text-slate-200">
                Credential token
                <textarea
                  id="wallet-verification-token"
                  value={verificationToken}
                  onChange={(event) => setVerificationToken(event.target.value)}
                  rows={5}
                  className="rounded border border-slate-700 bg-slate-950 px-3 py-2 font-mono text-[11px] text-slate-100"
                />
              </label>
              <div className="flex flex-wrap gap-2">
                <button
                  type="button"
                  disabled={!canVerify || verifyMutation.isPending}
                  className="rounded border border-violet-700 px-3 py-1.5 text-xs text-violet-100 hover:bg-violet-950/40 disabled:opacity-50"
                  onClick={() => verifyMutation.mutate()}
                >
                  {verifyMutation.isPending ? 'Verifying…' : 'Verify credential'}
                </button>
              </div>
            </div>

            {verifyMutation.isError ? (
              <div className="mt-4">
                <ApiErrorCallout
                  title="Verification failed"
                  message={getErrorMessage(verifyMutation.error, 'Failed to verify wallet credential.')}
                />
              </div>
            ) : null}

            {verifyMutation.data ? (
              <div className="mt-4 space-y-4 rounded border border-slate-700 bg-slate-950/40 p-4">
                <div className="flex flex-wrap items-start justify-between gap-3">
                  <div>
                    <p className="text-xs uppercase tracking-wide text-[var(--color-text-muted)]">Verification result</p>
                    <p className="mt-1 text-xl font-semibold text-slate-100">
                      {verifyMutation.data.isValid ? 'Valid badge' : 'Invalid badge'}
                    </p>
                    <p className="mt-1 text-sm text-slate-400">{verifyMutation.data.message}</p>
                  </div>
                  <div className="rounded-full border border-slate-700 px-3 py-1 text-sm text-slate-200">
                    {new Date(verifyMutation.data.verifiedAt).toLocaleString()}
                  </div>
                </div>

                {verifyMutation.data.report ? (
                  <>
                    <div className="grid gap-3 md:grid-cols-2 xl:grid-cols-4">
                      <MetricCard label="Person" value="Selected person" />
                      <MetricCard label="Qualification" value={verifyMutation.data.report.qualificationName} />
                      <MetricCard label="Status on date" value={verifyMutation.data.report.statusOnDate} />
                      <MetricCard label="As of" value={new Date(verifyMutation.data.report.asOfUtc).toLocaleDateString()} />
                    </div>

                    <div className="grid gap-4 xl:grid-cols-2">
                      <SectionCard title="Source certificate">
                        {verifyMutation.data.report.sourceCertificate ? (
                          <dl className="grid gap-2 text-sm">
                            <div className="flex flex-wrap justify-between gap-2">
                              <dt className="text-[var(--color-text-muted)]">Issue</dt>
                              <dd className="font-mono text-slate-100">
                                {verifyMutation.data.report.sourceCertificate.qualificationIssueId}
                              </dd>
                            </div>
                            <div className="flex flex-wrap justify-between gap-2">
                              <dt className="text-[var(--color-text-muted)]">Grant publication</dt>
                              <dd className="font-mono text-slate-100">
                                {verifyMutation.data.report.sourceCertificate.grantPublicationId}
                              </dd>
                            </div>
                            <div className="flex flex-wrap justify-between gap-2">
                              <dt className="text-[var(--color-text-muted)]">Issued</dt>
                              <dd className="text-slate-100">
                                {formatDateTime(verifyMutation.data.report.sourceCertificate.issuedAt)}
                              </dd>
                            </div>
                            <div className="flex flex-wrap justify-between gap-2">
                              <dt className="text-[var(--color-text-muted)]">Expires</dt>
                              <dd className="text-slate-100">
                                {formatDateTime(verifyMutation.data.report.sourceCertificate.expiresAt)}
                              </dd>
                            </div>
                            <div className="flex flex-wrap justify-between gap-2">
                              <dt className="text-[var(--color-text-muted)]">Lifecycle</dt>
                              <dd className="text-slate-100">{verifyMutation.data.report.sourceCertificate.statusOnDate}</dd>
                            </div>
                          </dl>
                        ) : (
                          <p className="text-sm text-slate-400">No source certificate was resolved for this credential.</p>
                        )}
                      </SectionCard>

                      <SectionCard title="Restrictions">
                        {verifyMutation.data.report.restrictions.length === 0 ? (
                          <p className="text-sm text-slate-400">No restrictions were recorded for this credential.</p>
                        ) : (
                          <ul className="space-y-2 text-sm text-slate-100">
                            {verifyMutation.data.report.restrictions.map((restriction) => (
                              <li key={restriction} className="rounded border border-slate-700 bg-slate-900/60 px-3 py-2">
                                {restriction}
                              </li>
                            ))}
                          </ul>
                        )}
                      </SectionCard>
                    </div>
                  </>
                ) : null}
              </div>
            ) : null}
          </SectionCard>
        </div>
      ) : null}
    </section>
  )
}
