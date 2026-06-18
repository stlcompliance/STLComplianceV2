import { type FormEvent, useMemo, useState } from 'react'
import { ApiErrorCallout, StaticSearchPicker, type PickerOption } from '@stl/shared-ui'
import type {
  CertificationDefinitionResponse,
  PersonCertificationResponse,
} from '../api/types'

interface CertificationPanelProps {
  personId: string
  personDisplayName: string
  definitions: CertificationDefinitionResponse[]
  certifications: PersonCertificationResponse[]
  isLoading?: boolean
  isError?: boolean
  readErrorMessage?: string | null
  onRetryRead?: () => void
  canManage: boolean
  isSubmitting: boolean
  actionErrorMessage: string | null
  onGrantCertification: (request: {
    certificationDefinitionId: string
    grantedAt: string | null
    expiresAt: string | null
    notes: string | null
  }) => Promise<void>
  onUpdateCertification: (
    personCertificationId: string,
    request: {
      status: 'active' | 'expired' | 'revoked'
      expiresAt: string | null
      notes: string | null
    },
  ) => Promise<void>
}

function formatStatusLabel(status: string): string {
  switch (status) {
    case 'active':
      return 'Active'
    case 'expired':
      return 'Expired'
    case 'revoked':
      return 'Revoked'
    case 'suspended':
      return 'Suspended'
    default:
      return status
  }
}

function formatSourceLabel(sourceType: string): string {
  switch (sourceType) {
    case 'trainarr_publication':
      return 'TrainArr qualification'
    case 'manual':
      return 'Manual grant'
    default:
      return sourceType.replaceAll('_', ' ')
  }
}

function daysUntil(value: string | null): number | null {
  if (!value) {
    return null
  }

  const timestamp = Date.parse(value)
  if (!Number.isFinite(timestamp)) {
    return null
  }

  return Math.ceil((timestamp - Date.now()) / 86_400_000)
}

function certificationRiskLabel(certification: PersonCertificationResponse): string | null {
  if (certification.effectiveStatus !== 'active') {
    return null
  }

  const remainingDays = daysUntil(certification.expiresAt)
  if (remainingDays == null) {
    return null
  }

  if (remainingDays < 0) {
    return 'Expired'
  }

  if (remainingDays <= 60) {
    return 'Expiring soon'
  }

  return null
}

export function formatCertificationMutationError(errorMessage: string | null): string | null {
  if (!errorMessage) {
    return null
  }

  const normalized = errorMessage.toLowerCase()
  if (normalized.includes('"status":403') || normalized.includes('forbidden')) {
    return `Forbidden: ${errorMessage}`
  }

  if (normalized.includes('"status":409') || normalized.includes('conflict')) {
    return `Conflict: ${errorMessage}`
  }

  if (normalized.includes('"status":400') || normalized.includes('validation')) {
    return `Validation: ${errorMessage}`
  }

  return errorMessage
}

export function CertificationPanel({
  personId,
  personDisplayName,
  definitions,
  certifications,
  isLoading = false,
  isError = false,
  readErrorMessage = null,
  onRetryRead,
  canManage,
  isSubmitting,
  actionErrorMessage,
  onGrantCertification,
  onUpdateCertification,
}: CertificationPanelProps) {
  const [selectedDefinitionId, setSelectedDefinitionId] = useState('')
  const [grantNotes, setGrantNotes] = useState('')
  const [expiresAt, setExpiresAt] = useState('')

  const activeDefinitions = useMemo(
    () => definitions.filter((definition) => definition.status === 'active'),
    [definitions],
  )

  const grantableDefinitions = useMemo(() => {
    const activeCertificationDefinitionIds = new Set(
      certifications
        .filter(
          (certification) =>
            certification.effectiveStatus === 'active' || certification.status === 'active',
        )
        .map((certification) => certification.certificationDefinitionId),
    )

    return activeDefinitions.filter(
      (definition) => !activeCertificationDefinitionIds.has(definition.certificationDefinitionId),
    )
  }, [activeDefinitions, certifications])
  const definitionOptions = useMemo<PickerOption[]>(
    () =>
      grantableDefinitions.map((definition) => ({
        value: definition.certificationDefinitionId,
        label: `${definition.name} (${definition.certificationKey})`,
      })),
    [grantableDefinitions],
  )
  const selectedDefinitionOption = definitionOptions.find((option) => option.value === selectedDefinitionId)

  const panelClassName =
    'mt-6 rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-6'
  const panelHeadingClassName = 'text-sm font-medium text-[var(--color-text-secondary)]'
  const panelCopyClassName = 'text-sm text-[var(--color-text-muted)]'
  const secondaryTextClassName = 'text-[var(--color-text-secondary)]'
  const primaryTextClassName = 'text-[var(--color-text-primary)]'
  const cardClassName =
    'rounded border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-elevated)] px-3 py-2'

  async function handleGrantSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    if (!selectedDefinitionId) {
      return
    }

    await onGrantCertification({
      certificationDefinitionId: selectedDefinitionId,
      grantedAt: null,
      expiresAt: expiresAt ? new Date(expiresAt).toISOString() : null,
      notes: grantNotes.trim() ? grantNotes.trim() : null,
    })

    setSelectedDefinitionId('')
    setGrantNotes('')
    setExpiresAt('')
  }

  return (
    <section className={panelClassName}>
      <h2 className={panelHeadingClassName}>Certifications for {personDisplayName}</h2>
      <p className={`mt-1 text-xs ${panelCopyClassName}`}>
        Readiness-linked certification visibility and manual grant records for person {personId}.
      </p>

      {actionErrorMessage ? (
        <div className="mt-4">
          <ApiErrorCallout
            title="Certification update failed"
            message={formatCertificationMutationError(actionErrorMessage) ?? actionErrorMessage}
          />
        </div>
      ) : null}

      {isLoading ? (
        <p className={`mt-4 ${panelCopyClassName}`}>Loading certifications…</p>
      ) : isError ? (
        <div className="mt-4">
          <ApiErrorCallout
            title="Certifications unavailable"
            message={readErrorMessage ?? 'Failed to load certifications and readiness catalog.'}
            onRetry={onRetryRead}
            retryLabel="Retry certifications"
          />
        </div>
      ) : certifications.length === 0 ? (
        <p className={`mt-4 ${panelCopyClassName}`}>No certification records yet for this person.</p>
      ) : (
        <ul className="mt-4 divide-y divide-[var(--color-border-subtle)]">
          {certifications.map((certification) => (
            <li key={certification.personCertificationId} className="py-3">
              <div className="flex flex-wrap items-start justify-between gap-3">
                <div>
                  <div className="flex flex-wrap items-center gap-2">
                    <p className={`text-sm ${primaryTextClassName}`}>{certification.certificationName}</p>
                    {certificationRiskLabel(certification) ? (
                      <span className="rounded-full bg-amber-500/15 px-2 py-0.5 text-[11px] font-medium uppercase tracking-wide text-amber-200">
                        {certificationRiskLabel(certification)}
                      </span>
                    ) : null}
                  </div>
                  <p className={`text-xs ${secondaryTextClassName}`}>
                    {certification.certificationKey} · {certification.category} ·{' '}
                    {formatSourceLabel(certification.sourceType)}
                  </p>
                  {certification.externalPublicationId ? (
                    <p className="mt-1 font-mono text-xs text-violet-600/90 dark:text-violet-300/90">
                      TrainArr publication {certification.externalPublicationId}
                    </p>
                  ) : null}
                  {certification.sourceType === 'trainarr_publication' &&
                  certification.effectiveStatus !== 'active' ? (
                    <p className="mt-1 text-xs text-violet-700/90 dark:text-violet-200/90">
                      TrainArr lifecycle: {formatStatusLabel(certification.effectiveStatus)}
                    </p>
                  ) : null}
                  <p className={`mt-1 text-xs ${panelCopyClassName}`}>
                    Granted {new Date(certification.grantedAt).toLocaleDateString()}
                    {certification.expiresAt
                      ? ` · Expires ${new Date(certification.expiresAt).toLocaleDateString()}`
                      : ' · No expiration'}
                  </p>
                  {certification.notes ? (
                    <p className={`mt-1 text-xs ${secondaryTextClassName}`}>{certification.notes}</p>
                  ) : null}
                </div>
                <div className="text-right">
                  <span
                    className={
                      certification.effectiveStatus === 'active'
                        ? 'text-xs uppercase tracking-wide text-emerald-400'
                        : certification.effectiveStatus === 'revoked'
                          ? 'text-xs uppercase tracking-wide text-red-300'
                          : 'text-xs uppercase tracking-wide text-amber-300'
                    }
                  >
                    {formatStatusLabel(certification.effectiveStatus)}
                  </span>
                  {canManage ? (
                    <div className="mt-2 flex flex-wrap justify-end gap-2">
                      {certification.effectiveStatus !== 'revoked' ? (
                        <button
                          type="button"
                          disabled={isSubmitting}
                          className="rounded border border-[var(--color-border-subtle)] px-2 py-1 text-xs text-[var(--color-text-secondary)] hover:bg-[var(--color-bg-surface-elevated)] disabled:opacity-50"
                          onClick={() =>
                            onUpdateCertification(certification.personCertificationId, {
                              status: 'revoked',
                              expiresAt: certification.expiresAt,
                              notes: certification.notes,
                            })
                          }
                        >
                          Revoke
                        </button>
                      ) : null}
                    </div>
                  ) : null}
                </div>
              </div>
            </li>
          ))}
        </ul>
      )}

      {canManage && !isLoading && !isError ? (
        <form className="mt-6 grid gap-3 border-t border-[var(--color-border-subtle)] pt-4 md:grid-cols-2" onSubmit={handleGrantSubmit}>
          <label className={`grid gap-1 text-xs ${secondaryTextClassName} md:col-span-2`}>
            Certification definition
            <div className="mt-1">
              <StaticSearchPicker
                value={selectedDefinitionId}
                onChange={setSelectedDefinitionId}
                options={definitionOptions}
                selectedOption={selectedDefinitionOption}
                placeholder="Select definition"
                testId="certification-grant-definition"
              />
            </div>
          </label>
          <label htmlFor="certification-grant-expires-at" className={`grid gap-1 text-xs ${secondaryTextClassName}`}>
            Expiration override (optional)
            <input
              id="certification-grant-expires-at"
              type="date"
              value={expiresAt}
              onChange={(event) => setExpiresAt(event.target.value)}
              className="rounded border border-[var(--color-border-subtle)] bg-[var(--color-bg-control)] px-3 py-2 text-sm text-[var(--color-text-primary)]"
            />
          </label>
          <label htmlFor="certification-grant-notes" className={`grid gap-1 text-xs ${secondaryTextClassName} md:col-span-2`}>
            Grant notes (optional)
            <textarea
              id="certification-grant-notes"
              value={grantNotes}
              onChange={(event) => setGrantNotes(event.target.value)}
              rows={2}
              className="rounded border border-[var(--color-border-subtle)] bg-[var(--color-bg-control)] px-3 py-2 text-sm text-[var(--color-text-primary)]"
            />
          </label>
          <div className="md:col-span-2">
            <button
              type="submit"
              disabled={isSubmitting || grantableDefinitions.length === 0}
              className="rounded bg-sky-600 px-4 py-2 text-sm text-white hover:bg-sky-500 disabled:opacity-50"
            >
              Grant manual certification
            </button>
          </div>
        </form>
      ) : !isLoading && !isError ? (
        <p className="mt-4 text-xs text-[var(--color-text-muted)]">
          Certification grants require staffarr.certifications.manage scope.
        </p>
      ) : null}

      <div className="mt-6 border-t border-[var(--color-border-subtle)] pt-4">
        <h3 className="text-xs font-medium uppercase tracking-wide text-[var(--color-text-muted)]">Readiness catalog</h3>
        {isLoading ? (
          <p className={`mt-2 ${panelCopyClassName}`}>Loading readiness catalog…</p>
        ) : isError ? (
          <p className={`mt-2 ${panelCopyClassName}`}>Readiness catalog unavailable.</p>
        ) : definitions.length === 0 ? (
          <p className={`mt-2 ${panelCopyClassName}`}>No certification definitions loaded.</p>
        ) : (
          <ul className="mt-2 grid gap-2 md:grid-cols-2">
            {definitions.map((definition) => (
              <li
                key={definition.certificationDefinitionId}
                className={cardClassName}
              >
                <p className={`text-sm ${primaryTextClassName}`}>{definition.name}</p>
                <p className={`text-[var(--color-text-muted)]`}>
                  {definition.certificationKey} · {definition.category}
                  {definition.defaultValidityDays ? ` · ${definition.defaultValidityDays} day default` : ''}
                </p>
              </li>
            ))}
          </ul>
        )}
      </div>
    </section>
  )
}
