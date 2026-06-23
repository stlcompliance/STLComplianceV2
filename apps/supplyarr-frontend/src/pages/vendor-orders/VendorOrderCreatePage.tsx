import { useMutation, useQuery } from '@tanstack/react-query'
import { DetailBadge, QuestionnaireFlow, StaticSearchPicker, type PickerOption } from '@stl/shared-ui'
import { useMemo, useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { getVendors } from '../../api/client'
import { createVendorOrder } from '../../api/vendorOrderClient'
import { useSupplyArrPageAccess } from './useSupplyArrPageAccess'
import { humanizeVendorOrderValue } from './vendorOrderUi'

type SectionKey = 'basics' | 'locations' | 'timing' | 'review'

type CreateFormState = {
  vendorId: string
  brokerOrderNumberSnapshot: string
  itemDescription: string
  orderedQuantity: string
  quantityUom: string
  pickupLocationNameSnapshot: string
  pickupAddressSnapshot: string
  customerIdSnapshot: string
  deliveryLocationNameSnapshot: string
  deliveryAddressSnapshot: string
  expectedReadyAt: string
  pickupWindowStart: string
  pickupWindowEnd: string
  pickupInstructions: string
}

const INITIAL_FORM: CreateFormState = {
  vendorId: '',
  brokerOrderNumberSnapshot: '',
  itemDescription: '',
  orderedQuantity: '',
  quantityUom: 'each',
  pickupLocationNameSnapshot: '',
  pickupAddressSnapshot: '',
  customerIdSnapshot: '',
  deliveryLocationNameSnapshot: '',
  deliveryAddressSnapshot: '',
  expectedReadyAt: '',
  pickupWindowStart: '',
  pickupWindowEnd: '',
  pickupInstructions: '',
}

const customArrCustomerOptions: PickerOption[] = [
  { value: 'cust-1001', label: 'CUS-1001 - Acme Freight Systems LLC' },
  { value: 'cust-1002', label: 'CUS-1002 - Northwind Components Inc.' },
  { value: 'cust-1003', label: 'CUS-1003 - South Ridge Logistics Partners' },
]

export function VendorOrderCreatePage() {
  const { session, meQuery, canCreateVendorOrders } = useSupplyArrPageAccess()
  const navigate = useNavigate()
  const [form, setForm] = useState<CreateFormState>(INITIAL_FORM)
  const [expandedSection, setExpandedSection] = useState<SectionKey>('basics')
  const [questionnaireDraftId] = useState(() => crypto.randomUUID())
  const complianceCoreApiBase = import.meta.env.VITE_COMPLIANCECORE_API_BASE ?? ''

  if (!session) {
    return <p className="text-sm text-slate-400">Loading vendor-order create flow…</p>
  }

  if (meQuery.isLoading) {
    return <p className="text-sm text-slate-400">Loading create permissions…</p>
  }

  if (!canCreateVendorOrders) {
    return (
      <section className="rounded-3xl border border-slate-800 bg-slate-950/70 p-8">
        <h1 className="text-2xl font-bold text-white">Create vendor order</h1>
        <p className="mt-3 text-sm text-slate-400">
          You do not have permission to create SupplyArr vendor orders.
        </p>
      </section>
    )
  }

  const vendorsQuery = useQuery({
    queryKey: ['supplyarr-vendor-order-create-vendors', session.accessToken],
    queryFn: () => getVendors(session.accessToken),
  })

  const basicsComplete = Boolean(
    form.vendorId.trim() &&
      form.itemDescription.trim() &&
      Number(form.orderedQuantity) > 0 &&
      form.quantityUom.trim(),
  )
  const locationsComplete = Boolean(form.pickupAddressSnapshot.trim())
  const timingComplete = true
  const reviewReady = basicsComplete && locationsComplete && timingComplete

  const selectedVendor = vendorsQuery.data?.find((vendor) => vendor.partyId === form.vendorId) ?? null

  const createMutation = useMutation({
    mutationFn: () =>
      createVendorOrder(session.accessToken, {
        vendorId: form.vendorId,
        brokerOrderNumberSnapshot: form.brokerOrderNumberSnapshot || null,
        pickupLocationNameSnapshot: form.pickupLocationNameSnapshot || null,
        pickupAddressSnapshot: form.pickupAddressSnapshot,
        customerIdSnapshot: form.customerIdSnapshot || null,
        deliveryLocationNameSnapshot: form.deliveryLocationNameSnapshot || null,
        deliveryAddressSnapshot: form.deliveryAddressSnapshot || null,
        itemDescription: form.itemDescription,
        orderedQuantity: Number(form.orderedQuantity),
        quantityUom: form.quantityUom || 'each',
        expectedReadyAt: form.expectedReadyAt || null,
        pickupWindowStart: form.pickupWindowStart || null,
        pickupWindowEnd: form.pickupWindowEnd || null,
        pickupInstructions: form.pickupInstructions || null,
      }),
    onSuccess: async (created) => {
      await navigate(`/purchasing/vendor-orders/${created.vendorOrderId}`)
    },
  })

  const sections = useMemo(
    () => [
      {
        key: 'basics' as const,
        title: '1. Vendor & order basics',
        state: basicsComplete ? 'Complete' : 'Needs required fields',
        summary: basicsComplete
          ? `${selectedVendor?.displayName ?? 'Vendor'} · ${form.itemDescription} · ${form.orderedQuantity} ${form.quantityUom}`
          : 'Select vendor, item description, quantity, and unit of measure.',
      },
      {
        key: 'locations' as const,
        title: '2. Pickup & destination snapshots',
        state: !basicsComplete ? 'Locked' : locationsComplete ? 'Complete' : 'Needs required fields',
        summary: locationsComplete
          ? `${form.pickupLocationNameSnapshot || 'Pickup snapshot'} · ${form.pickupAddressSnapshot}`
          : 'Capture the pickup snapshot and optional destination summary.',
      },
      {
        key: 'timing' as const,
        title: '3. Timing & instructions',
        state: !locationsComplete ? 'Locked' : timingComplete ? 'Optional' : 'In progress',
        summary:
          form.expectedReadyAt || form.pickupWindowStart || form.pickupInstructions
            ? 'Timing and pickup notes captured.'
            : 'Expected-ready date, pickup window, and instructions are optional in v1.',
      },
      {
        key: 'review' as const,
        title: '4. Review & create',
        state: reviewReady ? 'Ready' : 'Locked',
        summary: reviewReady
          ? 'Review the order before saving the draft vendor order.'
          : 'Complete the required basics and pickup snapshot first.',
      },
    ],
    [
      basicsComplete,
      form.expectedReadyAt,
      form.itemDescription,
      form.orderedQuantity,
      form.pickupAddressSnapshot,
      form.pickupInstructions,
      form.pickupLocationNameSnapshot,
      form.pickupWindowStart,
      form.quantityUom,
      locationsComplete,
      reviewReady,
      selectedVendor?.displayName,
      timingComplete,
    ],
  )

  return (
    <div className="grid gap-6 xl:grid-cols-[minmax(0,1fr)_22rem]">
      <div className="space-y-6">
        <section className="rounded-3xl border border-slate-800 bg-slate-950/80 p-6 shadow-2xl shadow-sky-950/20">
          <div className="flex flex-wrap items-start justify-between gap-4">
            <div>
              <div className="mb-3 flex flex-wrap gap-2">
                <DetailBadge label="SupplyArr" tone="info" />
                <DetailBadge label="Create vendor order" tone="warn" />
                <DetailBadge label={session.tenantDisplayName} tone="neutral" />
              </div>
              <h1 className="text-3xl font-bold text-white">New vendor-order readiness workflow</h1>
              <p className="mt-3 max-w-3xl text-sm text-slate-300">
                Create the order, then add timing and pickup details before sending it to the vendor.
              </p>
            </div>
            <Link
              to="/purchasing/vendor-orders"
              className="inline-flex rounded-xl border border-slate-700 bg-slate-900 px-4 py-3 text-sm font-semibold text-slate-200 hover:bg-slate-800"
            >
              Back to vendor orders
            </Link>
          </div>
        </section>

        <QuestionnaireFlow
          apiBase={complianceCoreApiBase}
          accessToken={session.accessToken}
          tenantId={session.tenantId}
          productKey="supplyarr"
          workflowKey="route_order_create"
          subjectType="trip"
          sourceRecordId={questionnaireDraftId}
          sourceEntity="vendor_order"
          title="Compliance Core questionnaire"
          subtitle="Keep the vendor-order setup short and review missing trip facts."
          submitLabel="Save questionnaire answers"
        />

        <section className="space-y-4">
          {sections.map((section) => {
            const locked =
              (section.key === 'locations' && !basicsComplete) ||
              (section.key === 'timing' && !locationsComplete) ||
              (section.key === 'review' && !reviewReady)

            return (
              <div key={section.key} className="overflow-hidden rounded-2xl border border-slate-800 bg-slate-950/70">
                <button
                  type="button"
                  className="flex w-full flex-wrap items-start justify-between gap-3 px-5 py-4 text-left"
                  disabled={locked}
                  onClick={() => setExpandedSection(section.key)}
                >
                  <div>
                    <h2 className="text-lg font-semibold text-white">{section.title}</h2>
                    <p className="mt-1 text-sm text-slate-400">{section.summary}</p>
                  </div>
                  <DetailBadge
                    label={section.state}
                    tone={
                      section.state === 'Complete' || section.state === 'Ready'
                        ? 'good'
                        : section.state === 'Locked'
                          ? 'neutral'
                          : 'warn'
                    }
                  />
                </button>

                {expandedSection === section.key ? (
                  <div className="border-t border-slate-800 px-5 py-5">
                    {section.key === 'basics' ? (
                      <BasicsSection
                        form={form}
                        onChange={setForm}
                        vendorsQuery={vendorsQuery}
                        onComplete={() => setExpandedSection('locations')}
                        complete={basicsComplete}
                      />
                    ) : null}

                    {section.key === 'locations' ? (
                      <LocationsSection
                        form={form}
                        onChange={setForm}
                        onComplete={() => setExpandedSection('timing')}
                        complete={locationsComplete}
                      />
                    ) : null}

                    {section.key === 'timing' ? (
                      <TimingSection
                        form={form}
                        onChange={setForm}
                        onComplete={() => setExpandedSection('review')}
                      />
                    ) : null}

                    {section.key === 'review' ? (
                      <ReviewSection
                        form={form}
                        vendorName={selectedVendor?.displayName ?? 'Vendor not selected'}
                        onCreate={() => createMutation.mutate()}
                        createPending={createMutation.isPending}
                        createError={createMutation.error instanceof Error ? createMutation.error.message : null}
                      />
                    ) : null}
                  </div>
                ) : null}
              </div>
            )
          })}
        </section>
      </div>

      <aside className="space-y-6">
        <section className="rounded-2xl border border-slate-800 bg-slate-950/70 p-5">
          <h2 className="text-lg font-bold text-white">Workflow guidance</h2>
          <ul className="mt-4 space-y-3 text-sm text-slate-300">
            <li>Track vendor readiness and document updates here.</li>
            <li>Trips can use the order later for dispatch blocking and release checks.</li>
            <li>Pickup and destination fields on this page are snapshots, not master locations.</li>
            <li>Saving this record creates a draft only. Sending to the vendor happens later from the detail page.</li>
          </ul>
        </section>

        <section className="rounded-2xl border border-slate-800 bg-slate-950/70 p-5">
          <h2 className="text-lg font-bold text-white">Completion state</h2>
          <div className="mt-4 space-y-3 text-sm text-slate-300">
            {sections.map((section) => (
              <div key={`summary-${section.key}`} className="rounded-xl border border-slate-800 bg-slate-900/70 p-3">
                <div className="flex items-center justify-between gap-2">
                  <span className="font-medium text-white">{section.title}</span>
                  <DetailBadge
                    label={section.state}
                    tone={section.state === 'Complete' || section.state === 'Ready' ? 'good' : section.state === 'Locked' ? 'neutral' : 'warn'}
                  />
                </div>
                <p className="mt-2 text-xs text-slate-400">{section.summary}</p>
              </div>
            ))}
          </div>
        </section>
      </aside>
    </div>
  )
}

function BasicsSection({
  form,
  onChange,
  vendorsQuery,
  onComplete,
  complete,
}: {
  form: CreateFormState
  onChange: (next: CreateFormState) => void
  vendorsQuery: { data?: Awaited<ReturnType<typeof getVendors>> }
  onComplete: () => void
  complete: boolean
}) {
  return (
    <div className="grid gap-4 md:grid-cols-2">
      <label className="text-sm text-slate-300 md:col-span-2">
        Vendor
        <select
          className="mt-1 block w-full rounded-xl border border-slate-700 bg-slate-950 px-3 py-2 text-white"
          value={form.vendorId}
          onChange={(event) => onChange({ ...form, vendorId: event.target.value })}
        >
          <option value="">Select a vendor</option>
          {(vendorsQuery.data ?? []).map((vendor) => (
            <option key={vendor.partyId} value={vendor.partyId}>
              {vendor.displayName}
            </option>
          ))}
        </select>
      </label>

      <label className="text-sm text-slate-300">
        Broker order number snapshot
        <input
          className="mt-1 block w-full rounded-xl border border-slate-700 bg-slate-950 px-3 py-2 text-white"
          value={form.brokerOrderNumberSnapshot}
          onChange={(event) => onChange({ ...form, brokerOrderNumberSnapshot: event.target.value })}
          placeholder="Optional for later reference"
        />
      </label>

      <label className="text-sm text-slate-300">
        Quantity unit of measure
        <input
          className="mt-1 block w-full rounded-xl border border-slate-700 bg-slate-950 px-3 py-2 text-white"
          value={form.quantityUom}
          onChange={(event) => onChange({ ...form, quantityUom: event.target.value })}
          placeholder="each"
        />
      </label>

      <label className="text-sm text-slate-300 md:col-span-2">
        Item description
        <textarea
          className="mt-1 block w-full rounded-xl border border-slate-700 bg-slate-950 px-3 py-2 text-white"
          rows={4}
          value={form.itemDescription}
          onChange={(event) => onChange({ ...form, itemDescription: event.target.value })}
          placeholder="Describe the shipment or palletized order the vendor must complete."
        />
      </label>

      <label className="text-sm text-slate-300">
        Ordered quantity
        <input
          className="mt-1 block w-full rounded-xl border border-slate-700 bg-slate-950 px-3 py-2 text-white"
          inputMode="decimal"
          value={form.orderedQuantity}
          onChange={(event) => onChange({ ...form, orderedQuantity: event.target.value })}
        />
      </label>

      <div className="md:col-span-2">
        <button
          type="button"
          className="rounded-xl bg-sky-500 px-4 py-2 text-sm font-semibold text-[var(--color-text-primary)] hover:bg-sky-400 disabled:opacity-50"
          disabled={!complete}
          onClick={onComplete}
        >
          Continue to pickup snapshots
        </button>
      </div>
    </div>
  )
}

function LocationsSection({
  form,
  onChange,
  onComplete,
  complete,
}: {
  form: CreateFormState
  onChange: (next: CreateFormState) => void
  onComplete: () => void
  complete: boolean
}) {
  return (
    <div className="grid gap-4 md:grid-cols-2">
      <label className="text-sm text-slate-300">
        Pickup location snapshot
        <input
          className="mt-1 block w-full rounded-xl border border-slate-700 bg-slate-950 px-3 py-2 text-white"
          value={form.pickupLocationNameSnapshot}
          onChange={(event) => onChange({ ...form, pickupLocationNameSnapshot: event.target.value })}
          placeholder="Vendor yard, warehouse, or dock"
        />
      </label>

      <div className="text-sm text-slate-300">
        <StaticSearchPicker
          id="vendor-order-customer-reference"
          label="Customer snapshot"
          value={form.customerIdSnapshot}
          onChange={(customerIdSnapshot) => onChange({ ...form, customerIdSnapshot })}
          options={customArrCustomerOptions}
          placeholder="Search customers"
        />
      </div>

      <label className="text-sm text-slate-300 md:col-span-2">
        Pickup address snapshot
        <textarea
          className="mt-1 block w-full rounded-xl border border-slate-700 bg-slate-950 px-3 py-2 text-white"
          rows={3}
          value={form.pickupAddressSnapshot}
          onChange={(event) => onChange({ ...form, pickupAddressSnapshot: event.target.value })}
          placeholder="Required pickup snapshot for this order."
        />
      </label>

      <label className="text-sm text-slate-300">
        Destination summary snapshot
        <input
          className="mt-1 block w-full rounded-xl border border-slate-700 bg-slate-950 px-3 py-2 text-white"
          value={form.deliveryLocationNameSnapshot}
          onChange={(event) => onChange({ ...form, deliveryLocationNameSnapshot: event.target.value })}
          placeholder="Optional customer delivery label"
        />
      </label>

      <label className="text-sm text-slate-300 md:col-span-2">
        Delivery address snapshot
        <textarea
          className="mt-1 block w-full rounded-xl border border-slate-700 bg-slate-950 px-3 py-2 text-white"
          rows={3}
          value={form.deliveryAddressSnapshot}
          onChange={(event) => onChange({ ...form, deliveryAddressSnapshot: event.target.value })}
          placeholder="Optional v1 destination summary"
        />
      </label>

      <div className="md:col-span-2">
        <button
          type="button"
          className="rounded-xl bg-sky-500 px-4 py-2 text-sm font-semibold text-[var(--color-text-primary)] hover:bg-sky-400 disabled:opacity-50"
          disabled={!complete}
          onClick={onComplete}
        >
          Continue to timing
        </button>
      </div>
    </div>
  )
}

function TimingSection({
  form,
  onChange,
  onComplete,
}: {
  form: CreateFormState
  onChange: (next: CreateFormState) => void
  onComplete: () => void
}) {
  return (
    <div className="grid gap-4 md:grid-cols-2">
      <label className="text-sm text-slate-300">
        Expected ready at
        <input
          type="datetime-local"
          className="mt-1 block w-full rounded-xl border border-slate-700 bg-slate-950 px-3 py-2 text-white"
          value={form.expectedReadyAt}
          onChange={(event) => onChange({ ...form, expectedReadyAt: event.target.value })}
        />
      </label>

      <label className="text-sm text-slate-300">
        Pickup window start
        <input
          type="datetime-local"
          className="mt-1 block w-full rounded-xl border border-slate-700 bg-slate-950 px-3 py-2 text-white"
          value={form.pickupWindowStart}
          onChange={(event) => onChange({ ...form, pickupWindowStart: event.target.value })}
        />
      </label>

      <label className="text-sm text-slate-300">
        Pickup window end
        <input
          type="datetime-local"
          className="mt-1 block w-full rounded-xl border border-slate-700 bg-slate-950 px-3 py-2 text-white"
          value={form.pickupWindowEnd}
          onChange={(event) => onChange({ ...form, pickupWindowEnd: event.target.value })}
        />
      </label>

      <label className="text-sm text-slate-300 md:col-span-2">
        Pickup instructions
        <textarea
          className="mt-1 block w-full rounded-xl border border-slate-700 bg-slate-950 px-3 py-2 text-white"
          rows={4}
          value={form.pickupInstructions}
          onChange={(event) => onChange({ ...form, pickupInstructions: event.target.value })}
          placeholder="Staging, forklift, dock, pallet-count, or contact instructions."
        />
      </label>

      <div className="md:col-span-2">
        <button
          type="button"
          className="rounded-xl bg-sky-500 px-4 py-2 text-sm font-semibold text-[var(--color-text-primary)] hover:bg-sky-400"
          onClick={onComplete}
        >
          Continue to review
        </button>
      </div>
    </div>
  )
}

function ReviewSection({
  form,
  vendorName,
  onCreate,
  createPending,
  createError,
}: {
  form: CreateFormState
  vendorName: string
  onCreate: () => void
  createPending: boolean
  createError: string | null
}) {
  return (
    <div className="space-y-4">
      <div className="grid gap-4 md:grid-cols-2">
        <ReviewCard label="Vendor" value={vendorName} />
        <ReviewCard label="Order quantity" value={`${form.orderedQuantity} ${form.quantityUom}`} />
        <ReviewCard label="Pickup snapshot" value={form.pickupLocationNameSnapshot || 'Pickup snapshot not labeled'} />
        <ReviewCard label="Expected ready" value={form.expectedReadyAt || 'Not scheduled'} />
      </div>

      <div className="rounded-2xl border border-slate-800 bg-slate-900/60 p-4 text-sm text-slate-300">
        <h3 className="font-semibold text-white">What happens next</h3>
        <ul className="mt-3 space-y-2">
          <li>The draft vendor order, status history, and vendor link are saved together.</li>
          <li>Trips can reference this order later by its order ID without changing transport responsibilities.</li>
          <li>Document records stay attached when files are added from this page or the vendor portal.</li>
        </ul>
      </div>

      {createError ? (
        <p className="text-sm text-red-300" role="alert">
          {createError}
        </p>
      ) : null}

      <button
        type="button"
        className="rounded-xl bg-emerald-500 px-4 py-3 text-sm font-semibold text-[var(--color-text-primary)] hover:bg-emerald-400 disabled:opacity-50"
        disabled={createPending}
        onClick={onCreate}
      >
        {createPending ? 'Creating vendor order…' : 'Create vendor order draft'}
      </button>
    </div>
  )
}

function ReviewCard({ label, value }: { label: string; value: string }) {
  return (
    <div className="rounded-2xl border border-slate-800 bg-slate-950/60 p-4">
      <p className="text-xs uppercase tracking-wide text-[var(--color-text-muted)]">{label}</p>
      <p className="mt-2 text-sm font-medium text-white">{value || humanizeVendorOrderValue('not_recorded')}</p>
    </div>
  )
}
