import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useEffect, useState } from 'react'
import { ApiErrorCallout, getErrorMessage } from '@stl/shared-ui'

import { getTripExecutionSettings, upsertTripExecutionSettings } from '../api/client'

interface TripExecutionSettingsPanelProps {
  accessToken: string
  canManage: boolean
}

export function TripExecutionSettingsPanel({
  accessToken,
  canManage,
}: TripExecutionSettingsPanelProps) {
  const queryClient = useQueryClient()
  const [initialized, setInitialized] = useState(false)
  const [requirePreTripDvirBeforeStart, setRequirePreTripDvirBeforeStart] = useState(true)
  const [requirePostTripDvirBeforeComplete, setRequirePostTripDvirBeforeComplete] = useState(false)
  const [requireDeliveryProofBeforeComplete, setRequireDeliveryProofBeforeComplete] = useState(false)
  const [requirePickupProofBeforeStart, setRequirePickupProofBeforeStart] = useState(false)
  const [blockTripStartOnDvirFail, setBlockTripStartOnDvirFail] = useState(true)
  const [blockTripCompleteOnDvirFail, setBlockTripCompleteOnDvirFail] = useState(true)
  const [requirePickupProofPhotoBeforeStart, setRequirePickupProofPhotoBeforeStart] = useState(false)
  const [requireDeliveryProofPhotoBeforeComplete, setRequireDeliveryProofPhotoBeforeComplete] =
    useState(false)
  const [requireDeliverySignatureBeforeComplete, setRequireDeliverySignatureBeforeComplete] =
    useState(false)
  const [requirePreTripDvirPhotoBeforeStart, setRequirePreTripDvirPhotoBeforeStart] =
    useState(false)
  const [requirePostTripDvirPhotoBeforeComplete, setRequirePostTripDvirPhotoBeforeComplete] =
    useState(false)

  const settingsQuery = useQuery({
    queryKey: ['routarr-trip-execution-settings', accessToken],
    queryFn: () => getTripExecutionSettings(accessToken),
    enabled: canManage,
  })

  useEffect(() => {
    if (initialized || settingsQuery.isLoading || !settingsQuery.data) {
      return
    }
    const data = settingsQuery.data
    setRequirePreTripDvirBeforeStart(data.requirePreTripDvirBeforeStart)
    setRequirePostTripDvirBeforeComplete(data.requirePostTripDvirBeforeComplete)
    setRequireDeliveryProofBeforeComplete(data.requireDeliveryProofBeforeComplete)
    setRequirePickupProofBeforeStart(data.requirePickupProofBeforeStart)
    setBlockTripStartOnDvirFail(data.blockTripStartOnDvirFail)
    setBlockTripCompleteOnDvirFail(data.blockTripCompleteOnDvirFail)
    setRequirePickupProofPhotoBeforeStart(data.requirePickupProofPhotoBeforeStart)
    setRequireDeliveryProofPhotoBeforeComplete(data.requireDeliveryProofPhotoBeforeComplete)
    setRequireDeliverySignatureBeforeComplete(data.requireDeliverySignatureBeforeComplete)
    setRequirePreTripDvirPhotoBeforeStart(data.requirePreTripDvirPhotoBeforeStart)
    setRequirePostTripDvirPhotoBeforeComplete(data.requirePostTripDvirPhotoBeforeComplete)
    setInitialized(true)
  }, [initialized, settingsQuery.data, settingsQuery.isLoading])

  const saveMutation = useMutation({
    mutationFn: () =>
      upsertTripExecutionSettings(accessToken, {
        requirePreTripDvirBeforeStart,
        requirePostTripDvirBeforeComplete,
        requireDeliveryProofBeforeComplete,
        requirePickupProofBeforeStart,
        blockTripStartOnDvirFail,
        blockTripCompleteOnDvirFail,
        requirePickupProofPhotoBeforeStart,
        requireDeliveryProofPhotoBeforeComplete,
        requireDeliverySignatureBeforeComplete,
        requirePreTripDvirPhotoBeforeStart,
        requirePostTripDvirPhotoBeforeComplete,
      }),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ['routarr-trip-execution-settings', accessToken] })
      void queryClient.invalidateQueries({ queryKey: ['driver-portal-schedule'] })
      void queryClient.invalidateQueries({ queryKey: ['driver-portal-capture-readiness'] })
    },
  })

  if (!canManage) {
    return null
  }

  return (
    <section
      className="rounded-lg border border-slate-700 bg-slate-900/80 p-4 shadow-sm"
      data-testid="trip-execution-settings-panel"
    >
      <h2 className="text-lg font-semibold text-slate-50">Trip proof &amp; DVIR capture policy</h2>
      <p className="mt-1 text-sm text-slate-400">
        Tenant rules for driver portal pickup/delivery proof and pre/post-trip DVIR before start or
        complete.
      </p>

      {settingsQuery.isError ? (
        <ApiErrorCallout
          className="mt-3"
          message={getErrorMessage(settingsQuery.error, 'Failed to load trip execution settings.')}
          onRetry={() => void settingsQuery.refetch()}
          retryLabel="Retry settings"
        />
      ) : null}

      <div className="mt-4 space-y-2 text-sm text-slate-200">
        <label className="flex items-center gap-2" htmlFor="trip-execution-require-pre-trip-dvir">
          <input
            id="trip-execution-require-pre-trip-dvir"
            type="checkbox"
            checked={requirePreTripDvirBeforeStart}
            onChange={(e) => setRequirePreTripDvirBeforeStart(e.target.checked)}
          />
          Require pre-trip DVIR before start
        </label>
        <label className="flex items-center gap-2" htmlFor="trip-execution-require-pickup-proof">
          <input
            id="trip-execution-require-pickup-proof"
            type="checkbox"
            checked={requirePickupProofBeforeStart}
            onChange={(e) => setRequirePickupProofBeforeStart(e.target.checked)}
          />
          Require pickup proof before start
        </label>
        <label className="flex items-center gap-2" htmlFor="trip-execution-block-start-on-dvir-fail">
          <input
            id="trip-execution-block-start-on-dvir-fail"
            type="checkbox"
            checked={blockTripStartOnDvirFail}
            onChange={(e) => setBlockTripStartOnDvirFail(e.target.checked)}
          />
          Block start when pre-trip DVIR is fail
        </label>
        <label className="flex items-center gap-2" htmlFor="trip-execution-require-post-trip-dvir">
          <input
            id="trip-execution-require-post-trip-dvir"
            type="checkbox"
            checked={requirePostTripDvirBeforeComplete}
            onChange={(e) => setRequirePostTripDvirBeforeComplete(e.target.checked)}
          />
          Require post-trip DVIR before complete
        </label>
        <label className="flex items-center gap-2" htmlFor="trip-execution-require-delivery-proof">
          <input
            id="trip-execution-require-delivery-proof"
            type="checkbox"
            checked={requireDeliveryProofBeforeComplete}
            onChange={(e) => setRequireDeliveryProofBeforeComplete(e.target.checked)}
          />
          Require delivery proof before complete
        </label>
        <label className="flex items-center gap-2" htmlFor="trip-execution-block-complete-on-dvir-fail">
          <input
            id="trip-execution-block-complete-on-dvir-fail"
            type="checkbox"
            checked={blockTripCompleteOnDvirFail}
            onChange={(e) => setBlockTripCompleteOnDvirFail(e.target.checked)}
          />
          Block complete when post-trip DVIR is fail
        </label>
        <p className="pt-2 text-xs uppercase tracking-wide text-[var(--color-text-muted)]">Attachment requirements</p>
        <label className="flex items-center gap-2" htmlFor="trip-execution-require-pickup-photo">
          <input
            id="trip-execution-require-pickup-photo"
            type="checkbox"
            checked={requirePickupProofPhotoBeforeStart}
            onChange={(e) => setRequirePickupProofPhotoBeforeStart(e.target.checked)}
          />
          Require pickup proof photo before start
        </label>
        <label className="flex items-center gap-2" htmlFor="trip-execution-require-pre-trip-dvir-photo">
          <input
            id="trip-execution-require-pre-trip-dvir-photo"
            type="checkbox"
            checked={requirePreTripDvirPhotoBeforeStart}
            onChange={(e) => setRequirePreTripDvirPhotoBeforeStart(e.target.checked)}
          />
          Require pre-trip DVIR photo before start
        </label>
        <label className="flex items-center gap-2" htmlFor="trip-execution-require-delivery-photo">
          <input
            id="trip-execution-require-delivery-photo"
            type="checkbox"
            checked={requireDeliveryProofPhotoBeforeComplete}
            onChange={(e) => setRequireDeliveryProofPhotoBeforeComplete(e.target.checked)}
          />
          Require delivery proof photo before complete
        </label>
        <label className="flex items-center gap-2" htmlFor="trip-execution-require-delivery-signature">
          <input
            id="trip-execution-require-delivery-signature"
            type="checkbox"
            checked={requireDeliverySignatureBeforeComplete}
            onChange={(e) => setRequireDeliverySignatureBeforeComplete(e.target.checked)}
          />
          Require delivery signature before complete
        </label>
        <label className="flex items-center gap-2" htmlFor="trip-execution-require-post-trip-dvir-photo">
          <input
            id="trip-execution-require-post-trip-dvir-photo"
            type="checkbox"
            checked={requirePostTripDvirPhotoBeforeComplete}
            onChange={(e) => setRequirePostTripDvirPhotoBeforeComplete(e.target.checked)}
          />
          Require post-trip DVIR photo before complete
        </label>
      </div>

      <button
        type="button"
        className="mt-4 rounded bg-indigo-700 px-3 py-1.5 text-sm text-white disabled:opacity-50"
        disabled={saveMutation.isPending || settingsQuery.isLoading}
        onClick={() => saveMutation.mutate()}
      >
        Save capture policy
      </button>
      {saveMutation.isError ? (
        <ApiErrorCallout
          className="mt-2"
          message={getErrorMessage(saveMutation.error, 'Failed to save trip execution settings.')}
        />
      ) : null}
    </section>
  )
}
