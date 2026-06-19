import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { getVendorOrderSettings, upsertVendorOrderSettings } from '../api/vendorOrderClient'

export function VendorOrderSettingsPanel({
  accessToken,
  canManage,
}: {
  accessToken: string
  canManage: boolean
}) {
  const queryClient = useQueryClient()
  const settingsQuery = useQuery({
    queryKey: ['supplyarr-vendor-order-settings', accessToken],
    queryFn: () => getVendorOrderSettings(accessToken),
    enabled: canManage,
  })

  const mutation = useMutation({
    mutationFn: (payload: { allowDestinationSummaryInVendorPortal: boolean; magicLinkTtlHours: number }) =>
      upsertVendorOrderSettings(accessToken, payload),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['supplyarr-vendor-order-settings', accessToken] })
    },
  })

  if (!canManage) {
    return null
  }

  if (settingsQuery.isLoading || !settingsQuery.data) {
    return (
      <section className="rounded-xl border border-slate-700 bg-slate-900/60 p-5">
        <h2 className="text-lg font-semibold text-white">Vendor-order portal settings</h2>
        <p className="mt-3 text-sm text-slate-400">Loading vendor-order settings…</p>
      </section>
    )
  }

  const settings = settingsQuery.data

  return (
    <section className="rounded-xl border border-slate-700 bg-slate-900/60 p-5" data-testid="vendor-order-settings-panel">
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div>
          <h2 className="text-lg font-semibold text-white">Vendor-order portal settings</h2>
          <p className="mt-1 text-sm text-slate-400">
            Control destination visibility in the vendor portal and the default magic-link lifetime.
          </p>
        </div>
        {settings.updatedAt ? (
          <span className="text-xs text-[var(--color-text-muted)]">Updated {new Date(settings.updatedAt).toLocaleString()}</span>
        ) : null}
      </div>

      <div className="mt-4 space-y-4">
        <label className="flex items-start gap-3 rounded-lg border border-slate-700 bg-slate-950/40 p-4 text-sm text-slate-300">
          <input
            type="checkbox"
            checked={settings.allowDestinationSummaryInVendorPortal}
            onChange={(event) =>
              mutation.mutate({
                allowDestinationSummaryInVendorPortal: event.target.checked,
                magicLinkTtlHours: settings.magicLinkTtlHours,
              })
            }
          />
          <span>Allow destination summary in the vendor portal.</span>
        </label>

        <label className="block text-sm text-slate-300">
          Magic-link TTL hours
          <input
            type="number"
            min={1}
            max={240}
            className="mt-1 block w-full max-w-xs rounded border border-slate-700 bg-slate-950 px-3 py-2 text-white"
            value={settings.magicLinkTtlHours}
            onChange={(event) =>
              mutation.mutate({
                allowDestinationSummaryInVendorPortal: settings.allowDestinationSummaryInVendorPortal,
                magicLinkTtlHours: Number(event.target.value) || 72,
              })
            }
          />
        </label>
      </div>

      {mutation.error instanceof Error ? (
        <p className="mt-3 text-sm text-red-300">{mutation.error.message}</p>
      ) : null}
    </section>
  )
}
