import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useEffect, useState } from 'react'
import { ApiErrorCallout, getErrorMessage } from '@stl/shared-ui'

import {
  getIntegrationEventSettings,
  listIntegrationOutboxEvents,
  RoutArrApiError,
  upsertIntegrationEventSettings,
} from '../api/client'

function resolveSaveErrorMessage(error: unknown): string {
  if (error instanceof RoutArrApiError) {
    try {
      const payload = JSON.parse(error.body) as { message?: string }
      if (payload.message) {
        return payload.message
      }
    } catch {
      if (error.message && !error.message.startsWith('{')) {
        return error.message
      }
    }
  }

  return 'Failed to save integration event settings.'
}

function resolveRetrySettingsError(
  maxAttempts: number,
  retryIntervalMinutes: number,
): string | null {
  if (!Number.isFinite(maxAttempts) || maxAttempts < 1 || maxAttempts > 20) {
    return 'Max attempts must be between 1 and 20.'
  }

  if (!Number.isFinite(retryIntervalMinutes) || retryIntervalMinutes < 1 || retryIntervalMinutes > 1440) {
    return 'Retry interval must be between 1 and 1440 minutes.'
  }

  return null
}

interface IntegrationEventSettingsPanelProps {
  accessToken: string
  canManage: boolean
}

export function IntegrationEventSettingsPanel({
  accessToken,
  canManage,
}: IntegrationEventSettingsPanelProps) {
  const queryClient = useQueryClient()
  const [initialized, setInitialized] = useState(false)
  const [isEnabled, setIsEnabled] = useState(true)
  const [maxAttempts, setMaxAttempts] = useState(5)
  const [retryIntervalMinutes, setRetryIntervalMinutes] = useState(15)
  const [validationError, setValidationError] = useState<string | null>(null)
  const [saveError, setSaveError] = useState<string | null>(null)

  const settingsQuery = useQuery({
    queryKey: ['routarr-integration-event-settings', accessToken],
    queryFn: () => getIntegrationEventSettings(accessToken),
    enabled: canManage,
  })

  const outboxQuery = useQuery({
    queryKey: ['routarr-integration-outbox-events', accessToken],
    queryFn: () => listIntegrationOutboxEvents(accessToken, 10),
    enabled: canManage,
  })

  useEffect(() => {
    if (initialized || settingsQuery.isLoading || !settingsQuery.data) {
      return
    }
    const data = settingsQuery.data
    setIsEnabled(data.isEnabled)
    setMaxAttempts(data.maxAttempts)
    setRetryIntervalMinutes(data.retryIntervalMinutes)
    setInitialized(true)
  }, [initialized, settingsQuery.data, settingsQuery.isLoading])

  const saveMutation = useMutation({
    mutationFn: () =>
      upsertIntegrationEventSettings(accessToken, {
        isEnabled,
        maxAttempts,
        retryIntervalMinutes,
      }),
    onSuccess: () => {
      setValidationError(null)
      setSaveError(null)
      void queryClient.invalidateQueries({ queryKey: ['routarr-integration-event-settings', accessToken] })
      void queryClient.invalidateQueries({ queryKey: ['routarr-integration-outbox-events', accessToken] })
    },
    onError: (error) => {
      setSaveError(resolveSaveErrorMessage(error))
    },
  })

  const handleSave = () => {
    const error = resolveRetrySettingsError(maxAttempts, retryIntervalMinutes)
    if (error) {
      setValidationError(error)
      setSaveError(null)
      return
    }

    setValidationError(null)
    setSaveError(null)
    saveMutation.mutate()
  }

  if (!canManage) {
    return null
  }

  return (
    <section
      className="rounded-lg border border-slate-700 bg-slate-900/80 p-4 shadow-sm"
      data-testid="integration-event-settings-panel"
    >
      <h2 className="text-lg font-semibold text-slate-50">Cross-product integration events</h2>
      <p className="mt-1 text-sm text-slate-400">
        Control RoutArr outbox publishing for trip dispatch, assignment, completion, and exception
        events consumed by other Arr products. Processing runs on a scheduled worker.
      </p>

      {settingsQuery.isError && (
        <div className="mt-3">
          <ApiErrorCallout
            title="Integration settings unavailable"
            message={getErrorMessage(settingsQuery.error, 'Failed to load integration event settings.')}
            retryLabel="Retry settings"
            onRetry={() => {
              void settingsQuery.refetch()
            }}
          />
        </div>
      )}

      <div className="mt-4 space-y-3">
        <label className="flex items-center gap-2 text-sm text-slate-200" htmlFor="integration-event-settings-enabled">
          <input
            id="integration-event-settings-enabled"
            type="checkbox"
            checked={isEnabled}
            onChange={(event) => {
              setIsEnabled(event.target.checked)
              setValidationError(null)
              setSaveError(null)
            }}
            data-testid="integration-event-settings-enabled"
          />
          Enable integration event publishing
        </label>

        <div className="grid gap-3 sm:grid-cols-2">
          <label className="block text-sm text-slate-200" htmlFor="integration-event-max-attempts">
            <span className="font-medium">Max processing attempts</span>
            <input
              id="integration-event-max-attempts"
              className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
              type="number"
              min={1}
              max={20}
              value={maxAttempts}
              onChange={(event) => {
                setMaxAttempts(Number(event.target.value))
                setValidationError(null)
                setSaveError(null)
              }}
              data-testid="integration-event-max-attempts"
            />
          </label>

          <label className="block text-sm text-slate-200" htmlFor="integration-event-retry-interval">
            <span className="font-medium">Retry interval (minutes)</span>
            <input
              id="integration-event-retry-interval"
              className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
              type="number"
              min={1}
              max={1440}
              value={retryIntervalMinutes}
              onChange={(event) => {
                setRetryIntervalMinutes(Number(event.target.value))
                setValidationError(null)
                setSaveError(null)
              }}
              data-testid="integration-event-retry-interval"
            />
          </label>
        </div>

        {validationError ? (
          <p className="text-sm text-rose-400" data-testid="integration-event-settings-validation-error">
            {validationError}
          </p>
        ) : null}

        <button
          type="button"
          className="rounded-md bg-sky-600 px-4 py-2 text-sm font-medium text-white hover:bg-sky-500 disabled:opacity-50"
          disabled={saveMutation.isPending || settingsQuery.isLoading}
          onClick={handleSave}
          data-testid="integration-event-settings-save"
        >
          {saveMutation.isPending ? 'Saving…' : 'Save integration event settings'}
        </button>

        {saveError ? (
          <ApiErrorCallout title="Save failed" message={saveError} />
        ) : null}
      </div>

      <div className="mt-6">
        <h3 className="text-sm font-semibold text-slate-200">Recent outbox events</h3>
        {outboxQuery.isLoading && (
          <p className="mt-2 text-sm text-slate-500">Loading outbox history…</p>
        )}
        {outboxQuery.data && outboxQuery.data.items.length === 0 && (
          <p className="mt-2 text-sm text-slate-500" data-testid="integration-outbox-empty">
            No integration outbox events yet.
          </p>
        )}
        {outboxQuery.data && outboxQuery.data.items.length > 0 && (
          <ul
            className="mt-2 divide-y divide-slate-800 rounded-md border border-slate-800 text-sm"
            data-testid="integration-outbox-list"
          >
            {outboxQuery.data.items.map((item) => (
              <li
                key={item.outboxEventId}
                className="px-3 py-2"
                data-testid={`integration-outbox-row-${item.outboxEventId}`}
              >
                <div className="flex flex-wrap items-center justify-between gap-2">
                  <span className="font-medium text-slate-100">{item.eventKind}</span>
                  <span className="text-slate-500">{item.processingStatus}</span>
                </div>
                <div className="text-xs text-slate-500">
                  {item.relatedEntityType} {item.relatedEntityId}
                  {item.attemptCount > 0 ? ` · ${item.attemptCount} attempt(s)` : ''}
                  {item.errorMessage ? ` · ${item.errorMessage}` : ''}
                </div>
              </li>
            ))}
          </ul>
        )}
      </div>
    </section>
  )
}
