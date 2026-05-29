import type { PublicCapabilityMaturity } from './products'
import { DOCS_11_ACCEPTANCE_NOTE } from './ownershipBoundaries'

/** Mirrors `docs/implementation-status.md` and worker slice state — update each marketing slice. */
export const PROGRAM_SNAPSHOT = {
  lastUpdatedLabel: 'Worker 4 (2026-05-29)',
  completedWorkersThrough: 4,
  latestSliceSummary: 'STLComplianceSite ownership copy + maturity honesty vs docs/02 and docs/11',
  latestCommitShort: 'uncommitted',
  sliceStateDoc: 'docs/implementation/worker-slices/00_SLICE_STATE.md',
  statusDoc: 'docs/implementation-status.md',
} as const

export const MATURITY_DISCLAIMER =
  'Public transparency only. Labels summarize implementation evidence in the private program docs — not a contractual SLA, not live production telemetry, and not proof that every docs/11 FEATURESET row is complete. Entitled customers validate scope in product UIs and agreements.'

export const MATURITY_LEAD =
  'V1 ships real APIs, PostgreSQL databases, workers, and authenticated product UIs per Arr product. Milestones below reflect program progress from the implementation masterplan; per-product gap canvases track remaining FEATURESET items until docs/11 sign-off.'

export type MilestonePosture = 'substantial' | 'partial' | 'in-progress'

export type ProgramMilestoneRow = {
  id: string
  title: string
  posture: MilestonePosture
  summary: string
  primaryProducts: string
}

export const MILESTONE_POSTURE_LABELS: Record<MilestonePosture, string> = {
  substantial: 'Substantial V1 progress',
  partial: 'Partial — active slices',
  'in-progress': 'In progress',
}

export const PROGRAM_MILESTONES: ProgramMilestoneRow[] = [
  {
    id: 'M0',
    title: 'Masterplan lock and traceability',
    posture: 'substantial',
    summary: 'Product ownership, milestone matrix, and ship-gate definitions are frozen in repo docs.',
    primaryProducts: 'Program',
  },
  {
    id: 'M1',
    title: 'Render and repo foundation',
    posture: 'substantial',
    summary: 'Dockerized .NET APIs, workers, Compose, CI, health checks, and per-product PostgreSQL baselines.',
    primaryProducts: 'All products',
  },
  {
    id: 'M2',
    title: 'NexArr platform access spine',
    posture: 'substantial',
    summary: 'Login, tenants, entitlements, service tokens, launch context, handoffs, and platform admin surfaces.',
    primaryProducts: 'NexArr',
  },
  {
    id: 'M3',
    title: 'Suite frontend and design system',
    posture: 'partial',
    summary: 'Suite AppShell, product switcher, shared UI, product workspace frames, and STLComplianceSite marketing spine.',
    primaryProducts: 'Suite, STLComplianceSite',
  },
  {
    id: 'M4',
    title: 'StaffArr workforce spine',
    posture: 'partial',
    summary: 'People, org, permissions, readiness, certifications, incidents, timeline, and audit export patterns.',
    primaryProducts: 'StaffArr',
  },
  {
    id: 'M5',
    title: 'Compliance Core vocabulary and rules',
    posture: 'partial',
    summary: 'Vocabulary, keys, mappings, rule packs, evaluations, findings, gates, and 9-CSV import/export.',
    primaryProducts: 'Compliance Core',
  },
  {
    id: 'M6',
    title: 'TrainArr qualification spine',
    posture: 'partial',
    summary: 'Programs, assignments, evidence, signoffs, qualifications, StaffArr publication, and field deep links.',
    primaryProducts: 'TrainArr',
  },
  {
    id: 'M7',
    title: 'MaintainArr maintenance spine',
    posture: 'partial',
    summary: 'Assets, inspections, defects, work orders, PM schedules, readiness, and async audit packages.',
    primaryProducts: 'MaintainArr',
  },
  {
    id: 'M8',
    title: 'SupplyArr procurement spine',
    posture: 'partial',
    summary: 'Vendors, parts, purchasing, receiving, inventory, notification settings, and MaintainArr demand intake.',
    primaryProducts: 'SupplyArr',
  },
  {
    id: 'M9',
    title: 'RoutArr dispatch spine',
    posture: 'partial',
    summary: 'Routes, trips, dispatch assignment, execution status, notification hooks, and driver eligibility integration.',
    primaryProducts: 'RoutArr',
  },
  {
    id: 'M10',
    title: 'Closed-loop cross-product workflows',
    posture: 'partial',
    summary: 'Qualification checks, workflow gates, demand signals, and publication paths across product APIs.',
    primaryProducts: 'TrainArr, StaffArr, MaintainArr, RoutArr, SupplyArr, Compliance Core',
  },
  {
    id: 'M11',
    title: 'Companion field execution',
    posture: 'partial',
    summary: 'Field inbox aggregation, notification settings, and deep links into entitled products — not workflow authority.',
    primaryProducts: 'Companion',
  },
  {
    id: 'M12',
    title: 'Reporting, exports, and audit packages',
    posture: 'partial',
    summary: 'Audit package export/generation across products, scheduled exports, and platform-admin audit export.',
    primaryProducts: 'StaffArr, Compliance Core, MaintainArr, NexArr, SupplyArr, TrainArr',
  },
  {
    id: 'M13',
    title: 'Hardening, performance, and ship gate',
    posture: 'in-progress',
    summary: 'k6 load scenarios, Playwright handoff/deep-link smokes, DR restore drill, and ongoing E2E catalog expansion.',
    primaryProducts: 'Program',
  },
]

export const VERIFICATION_HIGHLIGHTS = [
  '900+ Release .NET tests (Category≠Live), including cross-product integration and Playwright spec catalog tests.',
  'Five k6 scenarios: health probes plus authenticated login/me and handoff bootstrap.',
  'Playwright: suite login, product handoff smokes, and docs/23 journey proofs with per-frontend skip semantics.',
  'DR: nightly live restore drill validates all seven product PostgreSQL databases.',
] as const

export const OPEN_HONESTY_NOTES = [
  DOCS_11_ACCEPTANCE_NOTE,
  '“Substantial” and “partial” describe program milestone posture — not every backlog row in the feature matrix is complete.',
  'Companion remains V1 partial: inbox and deep links exist; authority stays in owning product APIs.',
  'Marketing pages never grant entitlements or display tenant operational data.',
] as const

export function maturityBadgeClass(maturity: PublicCapabilityMaturity): string {
  return maturity === 'v1-operational'
    ? 'rounded-full bg-teal-950/80 px-2 py-0.5 text-xs font-semibold text-teal-200'
    : 'rounded-full bg-amber-950/60 px-2 py-0.5 text-xs font-semibold text-amber-200'
}

export function milestonePostureBadgeClass(posture: MilestonePosture): string {
  if (posture === 'substantial') {
    return 'rounded-full bg-teal-950/80 px-2 py-0.5 text-xs font-semibold text-teal-200'
  }
  if (posture === 'in-progress') {
    return 'rounded-full bg-slate-800 px-2 py-0.5 text-xs font-semibold text-slate-200'
  }
  return 'rounded-full bg-amber-950/60 px-2 py-0.5 text-xs font-semibold text-amber-200'
}
