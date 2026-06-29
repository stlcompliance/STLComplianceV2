import { Link } from 'react-router-dom'

import { ApprovalRemindersPanel } from '../../components/ApprovalRemindersPanel'
import { DemandProcessingPanel } from '../../components/DemandProcessingPanel'
import { ProcurementCoordinationPanel } from '../../components/ProcurementCoordinationPanel'
import { SupplyReadinessDashboardPanel } from '../../components/SupplyReadinessDashboardPanel'
import type { SupplyArrWorkspaceState } from '../useSupplyArrWorkspaceState'

type Props = { state: SupplyArrWorkspaceState }

function combineParties(state: SupplyArrWorkspaceState) {
  return state.supplierDirectory
}

function countOpenRequests(state: SupplyArrWorkspaceState): number {
  return (
    state.purchaseRequestsQuery.data?.filter(
      (request) => !['approved', 'rejected', 'cancelled'].includes(request.status),
    ).length ?? 0
  )
}

function countOpenOrders(state: SupplyArrWorkspaceState): number {
  return (
    state.purchaseOrdersQuery.data?.filter(
      (order) => !['issued', 'cancelled', 'received', 'closed'].includes(order.status),
    ).length ?? 0
  )
}

function countWatchParties(state: SupplyArrWorkspaceState): number {
  return combineParties(state).filter(
    (party) => party.status !== 'active' || party.approvalStatus !== 'approved',
  ).length
}

function MetricCard({
  label,
  value,
  note,
}: {
  label: string
  value: string
  note: string
}) {
  return (
    <div className="rounded-2xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-4">
      <p className="text-xs uppercase tracking-wide text-[var(--color-text-muted)]">{label}</p>
      <p className="mt-2 text-3xl font-semibold text-[var(--color-text-primary)]">{value}</p>
      <p className="mt-2 text-xs text-[var(--color-text-muted)]">{note}</p>
    </div>
  )
}

export function DashboardSection({ state: s }: Props) {
  const parties = combineParties(s)
  const approvedParties = parties.filter(
    (party) => party.status === 'active' && party.approvalStatus === 'approved',
  ).length
  const watchParties = countWatchParties(s)
  const openPurchaseRequests = countOpenRequests(s)
  const openPurchaseOrders = countOpenOrders(s)
  const issuedPurchaseOrders =
    s.purchaseOrdersQuery.data?.filter((order) => order.status === 'issued').length ?? 0
  const backorderCount = s.backordersQuery.data?.length ?? 0
  const contractCount = s.contractsQuery.data?.length ?? 0
  const expiringLeadTimes = s.leadTimeSnapshotsQuery.data?.length ?? 0

  return (
    <div className="space-y-6">
      <section className="rounded-3xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-6 shadow-[var(--shadow-surface)]">
        <div className="flex flex-wrap items-start justify-between gap-5">
          <div className="max-w-3xl">
            <p className="text-xs uppercase tracking-[0.25em] text-[var(--color-accent)]">Command surface</p>
            <h2 className="mt-2 text-3xl font-semibold text-[var(--color-text-primary)]">
              Supplier risk, procurement blockers, and live sourcing activity.
            </h2>
            <p className="mt-3 text-sm text-[var(--color-text-secondary)]">
              This dashboard keeps supplier and purchasing work organized, with document posture and readiness signals that affect buying decisions.
            </p>
          </div>
          <div className="flex flex-wrap gap-2">
            <Link
              to="/suppliers/drawer"
              className="rounded-xl border border-[var(--color-border-default)] bg-[var(--color-bg-control)] px-4 py-2 text-sm font-medium text-[var(--color-text-primary)] hover:border-[var(--color-accent-border)]"
            >
              Review suppliers
            </Link>
            <Link
              to="/onboarding"
              className="rounded-xl bg-[var(--color-accent)] px-4 py-2 text-sm font-semibold text-[var(--color-on-accent)] hover:bg-[var(--color-accent-hover)]"
            >
              Open onboarding
            </Link>
            <Link
              to="/rfqs"
              className="rounded-xl border border-[var(--color-border-default)] bg-[var(--color-bg-control)] px-4 py-2 text-sm font-medium text-[var(--color-text-primary)] hover:border-[var(--color-accent-border)]"
            >
              Start RFQ
            </Link>
          </div>
        </div>
      </section>

      <section className="grid gap-4 md:grid-cols-2 xl:grid-cols-4">
        <MetricCard
          label="Approved suppliers"
          value={approvedParties.toString()}
          note={`${watchParties} suppliers need attention or review`}
        />
        <MetricCard
          label="Open purchase requests"
          value={openPurchaseRequests.toString()}
          note="Draft and submitted procurement requests"
        />
        <MetricCard
          label="Open purchase orders"
          value={openPurchaseOrders.toString()}
          note={`${issuedPurchaseOrders} issued orders are in flight`}
        />
        <MetricCard
          label="Backorders"
          value={backorderCount.toString()}
          note="LoadArr / supplier fulfillment gaps"
        />
        <MetricCard
          label="Contracts"
          value={contractCount.toString()}
          note="Tracked supplier agreements and renewal metadata"
        />
        <MetricCard
          label="Lead-time snapshots"
          value={expiringLeadTimes.toString()}
          note="Pricing, lead-time, and availability history"
        />
      </section>

      <section className="grid gap-6 lg:grid-cols-2">
        <SupplyReadinessDashboardPanel
          accessToken={s.accessToken}
          canRead={s.canReadSupplyReadiness}
        />
        <ProcurementCoordinationPanel
          accessToken={s.accessToken}
          canRead={s.canCreatePr || s.canApprovePr || s.canCreatePo}
        />
        <ApprovalRemindersPanel
          accessToken={s.accessToken}
          canRead={s.canCreatePr || s.canApprovePr || s.canCreatePo}
        />
        <DemandProcessingPanel
          accessToken={s.accessToken}
          canRead={s.canCreatePr || s.canApprovePr}
          canOperate={s.canCreatePr}
        />
      </section>

      <section className="rounded-2xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-5">
        <div className="flex flex-wrap items-start justify-between gap-3">
          <div>
            <h3 className="text-lg font-semibold text-[var(--color-text-primary)]">Dashboard scope</h3>
            <p className="mt-1 text-sm text-[var(--color-text-muted)]">
              Track suppliers, supplier relationships, purchasing context, procurement records, and supplier risk signals.
            </p>
          </div>
          <div className="flex flex-wrap gap-2 text-sm">
            <Link to="/risk" className="rounded-lg border border-[var(--color-border-default)] px-3 py-2 text-[var(--color-text-primary)] hover:border-[var(--color-accent-border)]">
              Review risk
            </Link>
            <Link to="/performance" className="rounded-lg border border-[var(--color-border-default)] px-3 py-2 text-[var(--color-text-primary)] hover:border-[var(--color-accent-border)]">
              Open performance
            </Link>
          </div>
        </div>
      </section>
    </div>
  )
}
