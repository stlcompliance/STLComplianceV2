import { ScanLine } from 'lucide-react'
import { PageHeader, buildProductLaunchUrlMap, resolveSuiteHomeUrl } from '@stl/shared-ui'

import { useFieldCompanionProductLaunch } from '../hooks/useFieldCompanionProductLaunch'
import { FieldScanPanel } from '../components/FieldScanPanel'
import { useFieldCompanionWorkspace } from '../hooks/useFieldCompanionWorkspace'

const suiteHomeUrl = resolveSuiteHomeUrl(import.meta.env.VITE_SUITE_URL)
const productLaunchUrls = buildProductLaunchUrlMap(import.meta.env)

export function ScanPage() {
  const { session, accessToken, meQuery } = useFieldCompanionWorkspace()
  const productLaunch = useFieldCompanionProductLaunch({
    accessToken,
    suiteHomeUrl,
    productLaunchUrls,
  })

  if (!session || !meQuery.data) {
    return <p className="text-sm text-slate-400">Loading scan tools…</p>
  }

  return (
    <div className="mx-auto max-w-3xl space-y-5">
      <PageHeader
        title="Scan / capture"
        subtitle="Scan a QR code or barcode, then continue into the correct product workflow with the right context."
      />

      <section className="rounded-2xl border border-slate-700 bg-slate-900/80 p-5">
        <div className="flex items-center gap-2">
          <ScanLine className="h-5 w-5 text-teal-300" aria-hidden />
          <h2 className="text-lg font-semibold text-white">Task resolver</h2>
        </div>
        <p className="mt-2 text-sm text-slate-400">
          Camera and manual scan support are both enabled here. If a code resolves to a task, the
          owning product still validates the action before anything is considered final.
        </p>
        <div className="mt-4">
          <FieldScanPanel accessToken={accessToken} onResolved={() => undefined} />
        </div>
      </section>

      <section className="rounded-2xl border border-slate-700 bg-slate-900/80 p-5">
        <h2 className="text-lg font-semibold text-white">Secure upload handoff</h2>
        <p className="mt-2 text-sm text-slate-400">
          When the source product creates a scoped upload session, Field Companion can hand off to
          the owning workspace so RecordArr can store the file or signature artifact with the right
          tenant and source context.
        </p>
        <div className="mt-4 flex flex-wrap gap-2">
          <button
            type="button"
            className="inline-flex min-h-11 items-center rounded-lg bg-teal-600 px-4 py-2 text-sm font-medium text-white hover:bg-teal-500 disabled:opacity-50"
            disabled={productLaunch.isPending}
            onClick={() => {
              void productLaunch.mutateAsync('recordarr')
            }}
          >
            Open RecordArr capture
          </button>
          <p className="inline-flex min-h-11 items-center text-sm text-[var(--color-text-muted)]">
            Secure uploads remain owned by the source product and RecordArr storage.
          </p>
        </div>
      </section>
    </div>
  )
}
