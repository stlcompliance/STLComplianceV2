import { useMutation, useQuery } from '@tanstack/react-query'
import { DetailBadge, QuestionnaireFlow, StaticSearchPicker, type PickerOption } from '@stl/shared-ui'
import { useMemo, useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { getSupplierDirectory } from '../../api/client'
import { createSupplierOrder } from '../../api/supplierOrderClient'
import {
  formatSupplierIdentitySummary,
  formatSupplierOperationalContext,
  humanizeSupplierUnitKind,
} from '../../utils/supplierPresentation'
import { useSupplyArrPageAccess } from './useSupplyArrPageAccess'
import { humanizeSupplierOrderValue } from './supplierOrderUi'

type SectionKey = 'basics' | 'locations' | 'timing' | 'review'

type CreateFormState = {
  supplierId: string
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
  supplierId: '',
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

function formatSupplierUnitOptionLabel(supplier: Awaited<ReturnType<typeof getSupplierDirectory>>[number]): string {
  return [
    humanizeSupplierUnitKind(supplier.unitKind),
    formatSupplierIdentitySummary({
      displayName: supplier.displayName,
      supplierKey: supplier.supplierKey,
      parentSupplierDisplayName: supplier.parentSupplierDisplayName,
      supplierUnitKind: supplier.unitKind,
    }),
    formatSupplierOperationalContext({
      supplierServiceTypes: supplier.serviceTypes,
      addressLine1: supplier.addressLine1,
      locality: supplier.locality,
      regionCode: supplier.regionCode,
      postalCode: supplier.postalCode,
    }),
  ]
    .filter(Boolean)
    .join(' · ')
}

export function SupplierOrderCreatePage() {
  const { session, meQuery, canCreateSupplierOrders } = useSupplyArrPageAccess()
  const navigate = useNavigate()
  const [form, setForm] = useState<CreateFormState>(INITIAL_FORM)
  const [expandedSection, setExpandedSection] = useState<SectionKey>('basics')
  const [questionnaireDraftId] = useState(() => crypto.randomUUID())
  const complianceCoreApiBase = import.meta.env.VITE_COMPLIANCECORE_API_BASE ?? ''

  if (!session) {
    return <p className="text-sm text-[var(--color-text-muted)]">Loading supplier-order create flow…</p>
  }

  if (meQuery.isLoading) {
    return <p className="text-sm text-[var(--color-text-muted)]">Loading create permissions…</p>
  }

  if (!canCreateSupplierOrders) {
    return (
      <section className="rounded-3xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-8">
        <h1 className="text-2xl font-bold text-[var(--color-text-primary)]">Create supplier order</h1>
        <p className="mt-3 text-sm text-[var(--color-text-secondary)]">
          You do not have permission to create SupplyArr supplier orders.
        </p>
      </section>
    )
  }

  const suppliersQuery = useQuery({
    queryKey: ['supplyarr-supplier-order-create-suppliers', session.accessToken],
    queryFn: () => getSupplierDirectory(session.accessToken),
  })

  const basicsComplete = Boolean(
    form.supplierId.trim() &&
      form.itemDescription.trim() &&
      Number(form.orderedQuantity) > 0 &&
      form.quantityUom.trim(),
  )
  const locationsComplete = Boolean(form.pickupAddressSnapshot.trim())
  const timingComplete = true
  const reviewReady = basicsComplete && locationsComplete && timingComplete

  const selectedSupplier = suppliersQuery.data?.find((supplier) => supplier.supplierId === form.supplierId) ?? null

  const createMutation = useMutation({
    mutationFn: () =>
      createSupplierOrder(session.accessToken, {
        supplierId: form.supplierId,
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
      await navigate(`/purchasing/supplier-orders/${created.supplierOrderId}`)
    },
  })

  const sections = useMemo(
    () => [
      {
        key: 'basics' as const,
        title: '1. Supplier & order basics',
        state: basicsComplete ? 'Complete' : 'Needs required fields',
        summary: basicsComplete
          ? `${selectedSupplier ? formatSupplierIdentitySummary({
              displayName: selectedSupplier.displayName,
              supplierKey: selectedSupplier.supplierKey,
              parentSupplierDisplayName: selectedSupplier.parentSupplierDisplayName,
              supplierUnitKind: selectedSupplier.unitKind,
            }) : 'Supplier'} · ${form.itemDescription} · ${form.orderedQuantity} ${form.quantityUom}`
          : 'Select the supplier identity or sub-unit, item description, quantity, and unit of measure.',
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
          ? 'Review the order before saving the draft supplier order.'
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
      selectedSupplier?.displayName,
      selectedSupplier?.parentSupplierDisplayName,
      selectedSupplier?.supplierKey,
      selectedSupplier?.unitKind,
      timingComplete,
    ],
  )

  return (
    <div className="grid gap-6 xl:grid-cols-[minmax(0,1fr)_22rem]">
      <div className="space-y-6">
        <section className="rounded-3xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-6 shadow-[var(--shadow-surface)]">
          <div className="flex flex-wrap items-start justify-between gap-4">
            <div>
              <div className="mb-3 flex flex-wrap gap-2">
                <DetailBadge label="SupplyArr" tone="info" />
                <DetailBadge label="Create supplier order" tone="warn" />
                <DetailBadge label={session.tenantDisplayName} tone="neutral" />
              </div>
              <h1 className="text-3xl font-bold text-[var(--color-text-primary)]">New supplier-order readiness workflow</h1>
              <p className="mt-3 max-w-3xl text-sm text-[var(--color-text-secondary)]">
                Create the order, then add timing and pickup details before sending it to the supplier.
              </p>
            </div>
            <Link
              to="/purchasing/supplier-orders"
              className="inline-flex rounded-xl border border-[var(--color-border-strong)] bg-[var(--color-bg-control)] px-4 py-3 text-sm font-semibold text-[var(--color-text-primary)] hover:bg-[var(--color-bg-control-hover)]"
            >
              Back to supplier orders
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
          sourceEntity="supplier_order"
          title="Compliance Core questionnaire"
          subtitle="Keep the supplier-order setup short and review missing trip facts."
          submitLabel="Save questionnaire answers"
        />

        <section className="space-y-4">
          {sections.map((section) => {
            const locked =
              (section.key === 'locations' && !basicsComplete) ||
              (section.key === 'timing' && !locationsComplete) ||
              (section.key === 'review' && !reviewReady)

            return (
              <div key={section.key} className="overflow-hidden rounded-2xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-elevated)]">
                <button
                  type="button"
                  className="flex w-full flex-wrap items-start justify-between gap-3 px-5 py-4 text-left"
                  disabled={locked}
                  onClick={() => setExpandedSection(section.key)}
                >
                  <div>
                    <h2 className="text-lg font-semibold text-[var(--color-text-primary)]">{section.title}</h2>
                    <p className="mt-1 text-sm text-[var(--color-text-secondary)]">{section.summary}</p>
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
                  <div className="border-t border-[var(--color-border-subtle)] px-5 py-5">
                    {section.key === 'basics' ? (
                      <BasicsSection
                        form={form}
                        onChange={setForm}
                        suppliersQuery={suppliersQuery}
                        selectedSupplier={selectedSupplier}
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
                        supplierSummary={
                          selectedSupplier
                            ? formatSupplierIdentitySummary({
                                displayName: selectedSupplier.displayName,
                                supplierKey: selectedSupplier.supplierKey,
                                parentSupplierDisplayName: selectedSupplier.parentSupplierDisplayName,
                                supplierUnitKind: selectedSupplier.unitKind,
                              })
                            : 'Supplier not selected'
                        }
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
        <section className="rounded-2xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-5">
          <h2 className="text-lg font-bold text-[var(--color-text-primary)]">Workflow guidance</h2>
          <ul className="mt-4 space-y-3 text-sm text-[var(--color-text-secondary)]">
            <li>Track supplier readiness and document updates here.</li>
            <li>Trips can use the order later for dispatch blocking and release checks.</li>
            <li>Pickup and destination fields on this page are snapshots, not master locations.</li>
            <li>Saving this record creates a draft only. Sending to the supplier happens later from the detail page.</li>
          </ul>
        </section>

        <section className="rounded-2xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-5">
          <h2 className="text-lg font-bold text-[var(--color-text-primary)]">Completion state</h2>
          <div className="mt-4 space-y-3 text-sm text-[var(--color-text-secondary)]">
            {sections.map((section) => (
              <div key={`summary-${section.key}`} className="rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-control)] p-3">
                <div className="flex items-center justify-between gap-2">
                  <span className="font-medium text-[var(--color-text-primary)]">{section.title}</span>
                  <DetailBadge
                    label={section.state}
                    tone={section.state === 'Complete' || section.state === 'Ready' ? 'good' : section.state === 'Locked' ? 'neutral' : 'warn'}
                  />
                </div>
                <p className="mt-2 text-xs text-[var(--color-text-muted)]">{section.summary}</p>
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
  suppliersQuery,
  selectedSupplier,
  onComplete,
  complete,
}: {
  form: CreateFormState
  onChange: (next: CreateFormState) => void
  suppliersQuery: { data?: Awaited<ReturnType<typeof getSupplierDirectory>> }
  selectedSupplier: Awaited<ReturnType<typeof getSupplierDirectory>>[number] | null
  onComplete: () => void
  complete: boolean
}) {
  const supplierOptions = (suppliersQuery.data ?? []).map((supplier) => ({
    value: supplier.supplierId!,
    label: formatSupplierUnitOptionLabel(supplier),
  }))
  const selectedSupplierOption = supplierOptions.find((option) => option.value === form.supplierId)
  return (
    <div className="grid gap-4 md:grid-cols-2">
      <div className="text-sm text-[var(--color-text-secondary)] md:col-span-2">
        <StaticSearchPicker
          id="supplier-order-supplier-unit"
          label="Supplier identity or sub-unit"
          value={form.supplierId}
          onChange={(value) => onChange({ ...form, supplierId: value })}
          options={supplierOptions}
          selectedOption={selectedSupplierOption}
          placeholder="Search supplier identities or sub-units…"
          testId="supplier-order-supplier-unit-picker"
        />
      </div>

      {selectedSupplier ? (
        <div className="rounded-2xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-elevated)] p-4 text-sm text-[var(--color-text-secondary)] md:col-span-2">
          <p className="font-medium text-[var(--color-text-primary)]">
            {formatSupplierIdentitySummary({
              displayName: selectedSupplier.displayName,
              supplierKey: selectedSupplier.supplierKey,
              parentSupplierDisplayName: selectedSupplier.parentSupplierDisplayName,
              supplierUnitKind: selectedSupplier.unitKind,
            })}
          </p>
          <p className="mt-2">
            {humanizeSupplierUnitKind(selectedSupplier.unitKind)} ·{' '}
            {formatSupplierOperationalContext({
              supplierServiceTypes: selectedSupplier.serviceTypes,
              addressLine1: selectedSupplier.addressLine1,
              locality: selectedSupplier.locality,
              regionCode: selectedSupplier.regionCode,
              postalCode: selectedSupplier.postalCode,
            })}
          </p>
        </div>
      ) : null}

      <label className="text-sm text-[var(--color-text-secondary)]">
        Broker order number snapshot
        <input
          className="mt-1 block w-full rounded-xl border border-[var(--color-border-strong)] bg-[var(--color-bg-control)] px-3 py-2 text-[var(--color-text-primary)]"
          value={form.brokerOrderNumberSnapshot}
          onChange={(event) => onChange({ ...form, brokerOrderNumberSnapshot: event.target.value })}
          placeholder="Optional for later reference"
        />
      </label>

      <label className="text-sm text-[var(--color-text-secondary)]">
        Quantity unit of measure
        <input
          className="mt-1 block w-full rounded-xl border border-[var(--color-border-strong)] bg-[var(--color-bg-control)] px-3 py-2 text-[var(--color-text-primary)]"
          value={form.quantityUom}
          onChange={(event) => onChange({ ...form, quantityUom: event.target.value })}
          placeholder="each"
        />
      </label>

      <label className="text-sm text-[var(--color-text-secondary)] md:col-span-2">
        Item description
        <textarea
          className="mt-1 block w-full rounded-xl border border-[var(--color-border-strong)] bg-[var(--color-bg-control)] px-3 py-2 text-[var(--color-text-primary)]"
          rows={4}
          value={form.itemDescription}
          onChange={(event) => onChange({ ...form, itemDescription: event.target.value })}
          placeholder="Describe the shipment or palletized order the supplier must complete."
        />
      </label>

      <label className="text-sm text-[var(--color-text-secondary)]">
        Ordered quantity
        <input
          className="mt-1 block w-full rounded-xl border border-[var(--color-border-strong)] bg-[var(--color-bg-control)] px-3 py-2 text-[var(--color-text-primary)]"
          inputMode="decimal"
          value={form.orderedQuantity}
          onChange={(event) => onChange({ ...form, orderedQuantity: event.target.value })}
        />
      </label>

      <div className="md:col-span-2">
        <button
          type="button"
          className="rounded-xl bg-[var(--color-accent)] px-4 py-2 text-sm font-semibold text-[var(--color-on-accent)] hover:bg-[var(--color-accent-hover)] disabled:opacity-50"
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
      <label className="text-sm text-[var(--color-text-secondary)]">
        Pickup location snapshot
        <input
          className="mt-1 block w-full rounded-xl border border-[var(--color-border-strong)] bg-[var(--color-bg-control)] px-3 py-2 text-[var(--color-text-primary)]"
          value={form.pickupLocationNameSnapshot}
          onChange={(event) => onChange({ ...form, pickupLocationNameSnapshot: event.target.value })}
          placeholder="Supplier yard, warehouse, or dock"
        />
      </label>

      <div className="text-sm text-[var(--color-text-secondary)]">
        <StaticSearchPicker
          id="supplier-order-customer-reference"
          label="Customer snapshot"
          value={form.customerIdSnapshot}
          onChange={(customerIdSnapshot) => onChange({ ...form, customerIdSnapshot })}
          options={customArrCustomerOptions}
          placeholder="Search customers"
        />
      </div>

      <label className="text-sm text-[var(--color-text-secondary)] md:col-span-2">
        Pickup address snapshot
        <textarea
          className="mt-1 block w-full rounded-xl border border-[var(--color-border-strong)] bg-[var(--color-bg-control)] px-3 py-2 text-[var(--color-text-primary)]"
          rows={3}
          value={form.pickupAddressSnapshot}
          onChange={(event) => onChange({ ...form, pickupAddressSnapshot: event.target.value })}
          placeholder="Required pickup snapshot for this order."
        />
      </label>

      <label className="text-sm text-[var(--color-text-secondary)]">
        Destination summary snapshot
        <input
          className="mt-1 block w-full rounded-xl border border-[var(--color-border-strong)] bg-[var(--color-bg-control)] px-3 py-2 text-[var(--color-text-primary)]"
          value={form.deliveryLocationNameSnapshot}
          onChange={(event) => onChange({ ...form, deliveryLocationNameSnapshot: event.target.value })}
          placeholder="Optional customer delivery label"
        />
      </label>

      <label className="text-sm text-[var(--color-text-secondary)] md:col-span-2">
        Delivery address snapshot
        <textarea
          className="mt-1 block w-full rounded-xl border border-[var(--color-border-strong)] bg-[var(--color-bg-control)] px-3 py-2 text-[var(--color-text-primary)]"
          rows={3}
          value={form.deliveryAddressSnapshot}
          onChange={(event) => onChange({ ...form, deliveryAddressSnapshot: event.target.value })}
          placeholder="Optional v1 destination summary"
        />
      </label>

      <div className="md:col-span-2">
        <button
          type="button"
          className="rounded-xl bg-[var(--color-accent)] px-4 py-2 text-sm font-semibold text-[var(--color-on-accent)] hover:bg-[var(--color-accent-hover)] disabled:opacity-50"
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
      <label className="text-sm text-[var(--color-text-secondary)]">
        Expected ready at
        <input
          type="datetime-local"
          className="mt-1 block w-full rounded-xl border border-[var(--color-border-strong)] bg-[var(--color-bg-control)] px-3 py-2 text-[var(--color-text-primary)]"
          value={form.expectedReadyAt}
          onChange={(event) => onChange({ ...form, expectedReadyAt: event.target.value })}
        />
      </label>

      <label className="text-sm text-[var(--color-text-secondary)]">
        Pickup window start
        <input
          type="datetime-local"
          className="mt-1 block w-full rounded-xl border border-[var(--color-border-strong)] bg-[var(--color-bg-control)] px-3 py-2 text-[var(--color-text-primary)]"
          value={form.pickupWindowStart}
          onChange={(event) => onChange({ ...form, pickupWindowStart: event.target.value })}
        />
      </label>

      <label className="text-sm text-[var(--color-text-secondary)]">
        Pickup window end
        <input
          type="datetime-local"
          className="mt-1 block w-full rounded-xl border border-[var(--color-border-strong)] bg-[var(--color-bg-control)] px-3 py-2 text-[var(--color-text-primary)]"
          value={form.pickupWindowEnd}
          onChange={(event) => onChange({ ...form, pickupWindowEnd: event.target.value })}
        />
      </label>

      <label className="text-sm text-[var(--color-text-secondary)] md:col-span-2">
        Pickup instructions
        <textarea
          className="mt-1 block w-full rounded-xl border border-[var(--color-border-strong)] bg-[var(--color-bg-control)] px-3 py-2 text-[var(--color-text-primary)]"
          rows={4}
          value={form.pickupInstructions}
          onChange={(event) => onChange({ ...form, pickupInstructions: event.target.value })}
          placeholder="Staging, forklift, dock, pallet-count, or contact instructions."
        />
      </label>

      <div className="md:col-span-2">
        <button
          type="button"
          className="rounded-xl bg-[var(--color-accent)] px-4 py-2 text-sm font-semibold text-[var(--color-on-accent)] hover:bg-[var(--color-accent-hover)]"
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
  supplierSummary,
  onCreate,
  createPending,
  createError,
}: {
  form: CreateFormState
  supplierSummary: string
  onCreate: () => void
  createPending: boolean
  createError: string | null
}) {
  return (
    <div className="space-y-4">
      <div className="grid gap-4 md:grid-cols-2">
        <ReviewCard label="Supplier" value={supplierSummary} />
        <ReviewCard label="Order quantity" value={`${form.orderedQuantity} ${form.quantityUom}`} />
        <ReviewCard label="Pickup snapshot" value={form.pickupLocationNameSnapshot || 'Pickup snapshot not labeled'} />
        <ReviewCard label="Expected ready" value={form.expectedReadyAt || 'Not scheduled'} />
      </div>

      <div className="rounded-2xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-elevated)] p-4 text-sm text-[var(--color-text-secondary)]">
        <h3 className="font-semibold text-[var(--color-text-primary)]">What happens next</h3>
        <ul className="mt-3 space-y-2">
          <li>The draft supplier order, status history, and supplier link are saved together.</li>
          <li>Trips can reference this order later by its order ID without changing transport responsibilities.</li>
          <li>Document records stay attached when files are added from this page or the supplier portal.</li>
        </ul>
      </div>

      {createError ? (
        <p className="text-sm text-[var(--tone-danger-text)]" role="alert">
          {createError}
        </p>
      ) : null}

      <button
        type="button"
        className="rounded-xl bg-[var(--color-accent)] px-4 py-3 text-sm font-semibold text-[var(--color-on-accent)] hover:bg-[var(--color-accent-hover)] disabled:opacity-50"
        disabled={createPending}
        onClick={onCreate}
      >
        {createPending ? 'Creating supplier order…' : 'Create supplier order draft'}
      </button>
    </div>
  )
}

function ReviewCard({ label, value }: { label: string; value: string }) {
  return (
    <div className="rounded-2xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-control)] p-4">
      <p className="text-xs uppercase tracking-wide text-[var(--color-text-muted)]">{label}</p>
      <p className="mt-2 text-sm font-medium text-[var(--color-text-primary)]">{value || humanizeSupplierOrderValue('not_recorded')}</p>
    </div>
  )
}
