import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useEffect, useState } from 'react'
import { ApiErrorCallout, getErrorMessage } from '@stl/shared-ui'

import {
  getIntegrationProbes,
  getIntegrationSettings,
  upsertIntegrationSettings,
} from '../api/client'

interface IntegrationSettingsPanelProps {
  accessToken: string
  canManage: boolean
}

export function IntegrationSettingsPanel({ accessToken, canManage }: IntegrationSettingsPanelProps) {
  const queryClient = useQueryClient()
  const [initialized, setInitialized] = useState(false)
  const [staffArrIntegrationEnabled, setStaffArrIntegrationEnabled] = useState(true)
  const [staffArrIncidentIntakeEnabled, setStaffArrIncidentIntakeEnabled] = useState(true)
  const [staffArrPublicationDeliveryEnabled, setStaffArrPublicationDeliveryEnabled] = useState(true)
  const [complianceCoreIntegrationEnabled, setComplianceCoreIntegrationEnabled] = useState(true)
  const [complianceCoreQualificationChecksEnabled, setComplianceCoreQualificationChecksEnabled] = useState(true)
  const [routarrIntegrationEnabled, setRoutarrIntegrationEnabled] = useState(true)
  const [routarrQualificationDispatchEnabled, setRoutarrQualificationDispatchEnabled] = useState(true)

  const settingsQuery = useQuery({
    queryKey: ['trainarr-integration-settings', accessToken],
    queryFn: () => getIntegrationSettings(accessToken),
    enabled: canManage,
  })

  const probesQuery = useQuery({
    queryKey: ['trainarr-integration-probes', accessToken],
    queryFn: () => getIntegrationProbes(accessToken),
    enabled: canManage,
  })

  useEffect(() => {
    if (initialized || settingsQuery.isLoading || !settingsQuery.data) {
      return
    }
    const data = settingsQuery.data
    setStaffArrIntegrationEnabled(data.staffArrIntegrationEnabled)
    setStaffArrIncidentIntakeEnabled(data.staffArrIncidentIntakeEnabled)
    setStaffArrPublicationDeliveryEnabled(data.staffArrPublicationDeliveryEnabled)
    setComplianceCoreIntegrationEnabled(data.complianceCoreIntegrationEnabled)
    setComplianceCoreQualificationChecksEnabled(data.complianceCoreQualificationChecksEnabled)
    setRoutarrIntegrationEnabled(data.routarrIntegrationEnabled)
    setRoutarrQualificationDispatchEnabled(data.routarrQualificationDispatchEnabled)
    setInitialized(true)
  }, [initialized, settingsQuery.data, settingsQuery.isLoading])

  const saveMutation = useMutation({
    mutationFn: () =>
      upsertIntegrationSettings(accessToken, {
        staffArrIntegrationEnabled,
        staffArrIncidentIntakeEnabled,
        staffArrPublicationDeliveryEnabled,
        complianceCoreIntegrationEnabled,
        complianceCoreQualificationChecksEnabled,
        routarrIntegrationEnabled,
        routarrQualificationDispatchEnabled,
      }),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ['trainarr-integration-settings', accessToken] })
      void queryClient.invalidateQueries({ queryKey: ['trainarr-integration-probes', accessToken] })
    },
  })

  if (!canManage) {
    return null
  }

  return (
    <section className="rounded-lg border border-border bg-card p-4 shadow-sm" data-testid="integration-settings-panel">
      <h2 className="text-lg font-semibold text-foreground">Integration settings</h2>
      <p className="mt-1 text-sm text-muted-foreground">
        Control which integrations are active for this tenant. Disabled integrations reject inbound service-token calls and skip outbound delivery attempts.
      </p>

      {settingsQuery.isError && (
        <div className="mt-3">
          <ApiErrorCallout
            title="Integration settings unavailable"
            message={getErrorMessage(settingsQuery.error, 'Failed to load integration settings.')}
            retryLabel="Retry settings"
            onRetry={() => {
              void settingsQuery.refetch()
            }}
          />
        </div>
      )}

      <div className="mt-4 space-y-4">
        <fieldset className="space-y-2 rounded border border-border p-3">
          <legend className="px-1 text-sm font-semibold">People</legend>
          <label htmlFor="integration-staffarr-enabled" className="flex items-center gap-2 text-sm">
            <input
              id="integration-staffarr-enabled"
              type="checkbox"
              checked={staffArrIntegrationEnabled}
              onChange={(event) => setStaffArrIntegrationEnabled(event.target.checked)}
              data-testid="integration-staffarr-enabled"
            />
            Enable people integration
          </label>
          <label htmlFor="integration-staffarr-incident-intake" className="flex items-center gap-2 text-sm">
            <input
              id="integration-staffarr-incident-intake"
              type="checkbox"
              checked={staffArrIncidentIntakeEnabled}
              disabled={!staffArrIntegrationEnabled}
              onChange={(event) => setStaffArrIncidentIntakeEnabled(event.target.checked)}
              data-testid="integration-staffarr-incident-intake"
            />
            Accept incident remediation intake
          </label>
          <label htmlFor="integration-staffarr-publication-delivery" className="flex items-center gap-2 text-sm">
            <input
              id="integration-staffarr-publication-delivery"
              type="checkbox"
              checked={staffArrPublicationDeliveryEnabled}
              disabled={!staffArrIntegrationEnabled}
              onChange={(event) => setStaffArrPublicationDeliveryEnabled(event.target.checked)}
              data-testid="integration-staffarr-publication-delivery"
            />
            Deliver certification publications
          </label>
        </fieldset>

        <fieldset className="space-y-2 rounded border border-border p-3">
          <legend className="px-1 text-sm font-semibold">Compliance Core</legend>
          <label htmlFor="integration-compliancecore-enabled" className="flex items-center gap-2 text-sm">
            <input
              id="integration-compliancecore-enabled"
              type="checkbox"
              checked={complianceCoreIntegrationEnabled}
              onChange={(event) => setComplianceCoreIntegrationEnabled(event.target.checked)}
              data-testid="integration-compliancecore-enabled"
            />
            Enable Compliance Core integration
          </label>
          <label htmlFor="integration-compliancecore-qualification-checks" className="flex items-center gap-2 text-sm">
            <input
              id="integration-compliancecore-qualification-checks"
              type="checkbox"
              checked={complianceCoreQualificationChecksEnabled}
              disabled={!complianceCoreIntegrationEnabled}
              onChange={(event) => setComplianceCoreQualificationChecksEnabled(event.target.checked)}
              data-testid="integration-compliancecore-qualification-checks"
            />
            Run Compliance Core rule evaluation during qualification checks
          </label>
        </fieldset>

        <fieldset className="space-y-2 rounded border border-border p-3">
          <legend className="px-1 text-sm font-semibold">Dispatch</legend>
          <label htmlFor="integration-routarr-enabled" className="flex items-center gap-2 text-sm">
            <input
              id="integration-routarr-enabled"
              type="checkbox"
              checked={routarrIntegrationEnabled}
              onChange={(event) => setRoutarrIntegrationEnabled(event.target.checked)}
              data-testid="integration-routarr-enabled"
            />
            Enable dispatch integration
          </label>
          <label htmlFor="integration-routarr-qualification-dispatch" className="flex items-center gap-2 text-sm">
            <input
              id="integration-routarr-qualification-dispatch"
              type="checkbox"
              checked={routarrQualificationDispatchEnabled}
              disabled={!routarrIntegrationEnabled}
              onChange={(event) => setRoutarrQualificationDispatchEnabled(event.target.checked)}
              data-testid="integration-routarr-qualification-dispatch"
            />
            Accept qualification check dispatch
          </label>
        </fieldset>

        <button
          type="button"
          className="rounded-md bg-primary px-4 py-2 text-sm font-medium text-primary-foreground disabled:opacity-50"
          disabled={saveMutation.isPending}
          onClick={() => saveMutation.mutate()}
          data-testid="integration-settings-save"
        >
          {saveMutation.isPending ? 'Saving…' : 'Save integration settings'}
        </button>
        {saveMutation.isError && (
          <ApiErrorCallout
            title="Save failed"
            message={getErrorMessage(saveMutation.error, 'Failed to save integration settings.')}
          />
        )}
      </div>

      <div className="mt-6">
        <h3 className="text-sm font-semibold text-foreground">Connectivity probes</h3>
        {probesQuery.isLoading && <p className="mt-2 text-sm text-muted-foreground">Probing integrations…</p>}
        {probesQuery.isError && (
          <ApiErrorCallout
            title="Connectivity probes unavailable"
            message={getErrorMessage(probesQuery.error, 'Failed to load integration connectivity probes.')}
            retryLabel="Retry probes"
            onRetry={() => {
              void probesQuery.refetch()
            }}
          />
        )}
        {probesQuery.data && (
          <ul className="mt-2 space-y-2 text-sm" data-testid="integration-probes-list">
            {probesQuery.data.items.map((item) => (
              <li key={item.integrationKey} className="rounded border border-border px-3 py-2">
                <div className="font-medium">
                  {item.displayName} · {item.status}
                </div>
                {item.httpStatusCode != null && (
                  <div className="text-muted-foreground">HTTP {item.httpStatusCode}</div>
                )}
                {item.message && <div className="text-muted-foreground">{item.message}</div>}
              </li>
            ))}
          </ul>
        )}
      </div>
    </section>
  )
}
