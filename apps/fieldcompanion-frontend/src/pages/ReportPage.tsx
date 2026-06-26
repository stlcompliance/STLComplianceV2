import { Bell, ShieldAlert, Wrench, Truck, GraduationCap, PackageOpen } from 'lucide-react'
import { PageHeader, buildProductLaunchUrlMap, resolveSuiteHomeUrl } from '@stl/shared-ui'

import { useFieldCompanionProductLaunch } from '../hooks/useFieldCompanionProductLaunch'
import { useFieldCompanionWorkspace } from '../hooks/useFieldCompanionWorkspace'

const suiteHomeUrl = resolveSuiteHomeUrl(import.meta.env.VITE_SUITE_URL)
const productLaunchUrls = buildProductLaunchUrlMap(import.meta.env)

const REPORT_TARGETS = [
  {
    productKey: 'staffarr',
    title: 'Incident report',
    description: 'Capture a safety, HR, or personnel issue in the owning people workflow.',
    icon: Bell,
  },
  {
    productKey: 'maintainarr',
    title: 'Maintenance note',
    description: 'Open the maintenance workspace for defect, work order, or equipment reporting.',
    icon: Wrench,
  },
  {
    productKey: 'assurarr',
    title: 'Quality / CAPA',
    description: 'Route nonconformance, hold, and corrective action work to AssurArr.',
    icon: ShieldAlert,
  },
  {
    productKey: 'routarr',
    title: 'Route exception',
    description: 'Open the route workspace for delivery, pickup, or dispatch exceptions.',
    icon: Truck,
  },
  {
    productKey: 'trainarr',
    title: 'Training issue',
    description: 'Launch the training workspace when an assignment needs acknowledgement or signoff.',
    icon: GraduationCap,
  },
  {
    productKey: 'loadarr',
    title: 'Receiving issue',
    description: 'Open the warehouse workspace for receiving, picking, or inventory-related issues.',
    icon: PackageOpen,
  },
] as const

export function ReportPage() {
  const { session, meQuery } = useFieldCompanionWorkspace()
  const productLaunch = useFieldCompanionProductLaunch({
    accessToken: session?.accessToken ?? '',
    suiteHomeUrl,
    productLaunchUrls,
  })

  if (!session || !meQuery.data) {
    return <p className="text-sm text-slate-400">Loading report shortcuts…</p>
  }

  const availableProducts = new Set(meQuery.data.fieldProductKeys)

  return (
    <div className="mx-auto max-w-5xl space-y-5">
      <PageHeader
        title="Report"
        subtitle="Send issues to the right workflow. Field Companion stays focused on capture and handoff."
      />

      <section className="rounded-2xl border border-slate-700 bg-slate-900/80 p-5">
        <p className="text-sm text-slate-400">
          These shortcuts route you into the correct workspace for the report. They stay lightweight so you can move quickly.
        </p>

        <div className="mt-4 grid gap-3 sm:grid-cols-2 xl:grid-cols-3">
          {REPORT_TARGETS.filter((target) => availableProducts.has(target.productKey)).map((target) => {
            const Icon = target.icon
            return (
              <article key={target.productKey} className="rounded-2xl border border-slate-700 bg-slate-950/60 p-4">
                <div className="flex items-center gap-2">
                  <Icon className="h-5 w-5 text-teal-300" aria-hidden />
                  <h2 className="text-base font-semibold text-white">{target.title}</h2>
                </div>
                <p className="mt-2 text-sm text-slate-300">{target.description}</p>
                <button
                  type="button"
                  className="mt-3 inline-flex min-h-11 items-center rounded-lg bg-teal-600 px-4 py-2 text-sm font-medium text-white hover:bg-teal-500 disabled:opacity-50"
                  disabled={productLaunch.isPending}
                  onClick={() => {
                    void productLaunch.mutateAsync(target.productKey)
                  }}
                >
                  Open {target.title.replace(' report', '').replace(' note', '')}
                </button>
              </article>
            )
          })}
        </div>
      </section>
    </div>
  )
}
