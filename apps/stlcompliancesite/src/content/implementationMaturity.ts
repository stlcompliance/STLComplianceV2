import type { PublicCapabilityMaturity } from './products'
import { DOCS_11_ACCEPTANCE_NOTE } from './ownershipBoundaries'

export const PROGRAM_SNAPSHOT = {
  lastUpdatedLabel: 'June 2026',
  completedWorkersThrough: 4,
  latestSliceSummary: 'Public site copy and product status refresh',
  latestCommitShort: 'current',
  sliceStateDoc: 'internal rollout notes',
  statusDoc: 'product status review',
} as const

export const MATURITY_DISCLAIMER =
  'Status labels are a plain snapshot for evaluation conversations. They are not a contract, a guarantee that every roadmap item is finished, or a replacement for your STL Compliance agreement.'

export const MATURITY_LEAD =
  'STL Compliance is already usable across the core product suite, with deeper workflow coverage continuing to expand. This page gives buyers a clear view of what is available now and what is still growing.'

export type MilestonePosture = 'substantial' | 'partial' | 'in-progress'

export type ProgramMilestoneRow = {
  id: string
  title: string
  posture: MilestonePosture
  summary: string
  primaryProducts: string
}

export const MILESTONE_POSTURE_LABELS: Record<MilestonePosture, string> = {
  substantial: 'Available now',
  partial: 'Expanding',
  'in-progress': 'In progress',
}

export const PROGRAM_MILESTONES: ProgramMilestoneRow[] = [
  {
    id: 'M0',
    title: 'Suite foundation',
    posture: 'substantial',
    summary: 'The product family, rollout model, and suite positioning are established.',
    primaryProducts: 'Program',
  },
  {
    id: 'M1',
    title: 'Cloud deployment foundation',
    posture: 'substantial',
    summary: 'The suite is prepared for cloud deployment with health checks and service packaging.',
    primaryProducts: 'All products',
  },
  {
    id: 'M2',
    title: 'Secure access',
    posture: 'substantial',
    summary: 'Users can sign in, reach available products, and be managed through NexArr.',
    primaryProducts: 'NexArr',
  },
  {
    id: 'M3',
    title: 'Shared suite experience',
    posture: 'partial',
    summary: 'The suite shell, product switching, and public site are in place and improving.',
    primaryProducts: 'Suite, STLComplianceSite',
  },
  {
    id: 'M4',
    title: 'Workforce readiness',
    posture: 'partial',
    summary: 'People, roles, readiness, certifications, incidents, and history are active focus areas.',
    primaryProducts: 'StaffArr',
  },
  {
    id: 'M5',
    title: 'Compliance rules and evidence',
    posture: 'partial',
    summary: 'Rules, vocabulary, evidence expectations, findings, and checks continue to expand.',
    primaryProducts: 'Compliance Core',
  },
  {
    id: 'M6',
    title: 'Training and qualification',
    posture: 'partial',
    summary: 'Programs, assignments, signoffs, evidence, and qualification proof are active.',
    primaryProducts: 'TrainArr',
  },
  {
    id: 'M7',
    title: 'Maintenance control',
    posture: 'partial',
    summary: 'Assets, inspections, defects, work orders, preventive maintenance, and readiness are active.',
    primaryProducts: 'MaintainArr',
  },
  {
    id: 'M8',
    title: 'Supply and purchasing',
    posture: 'partial',
    summary: 'Vendors, parts, purchase requests, receiving, and supply records continue to expand.',
    primaryProducts: 'SupplyArr',
  },
  {
    id: 'M9',
    title: 'Routes and dispatch',
    posture: 'partial',
    summary: 'Routes, trips, assignments, trip status, and driver readiness checks continue to expand.',
    primaryProducts: 'RoutArr',
  },
  {
    id: 'M10',
    title: 'Connected workflows',
    posture: 'partial',
    summary: 'The suite is connecting training, readiness, maintenance, dispatch, supply, and compliance proof.',
    primaryProducts: 'TrainArr, StaffArr, MaintainArr, RoutArr, SupplyArr, Compliance Core',
  },
  {
    id: 'M11',
    title: 'Field work',
    posture: 'partial',
    summary: 'The field inbox and quick handoffs are available in early form.',
    primaryProducts: 'Companion',
  },
  {
    id: 'M12',
    title: 'Reporting and audit packages',
    posture: 'partial',
    summary: 'Exports, reports, and audit packages continue to expand across products.',
    primaryProducts: 'StaffArr, Compliance Core, MaintainArr, NexArr, SupplyArr, TrainArr',
  },
  {
    id: 'M13',
    title: 'Hardening and launch readiness',
    posture: 'in-progress',
    summary: 'Performance, reliability, walkthrough coverage, and deployment checks remain active.',
    primaryProducts: 'Program',
  },
]

export const VERIFICATION_HIGHLIGHTS = [
  'Core product workflows are available across the suite.',
  'Secure sign-in and product launch are part of the platform.',
  'Product walkthroughs and connected workflow checks continue to expand.',
  'Cloud deployment checks are kept aligned with the Render configuration.',
] as const

export const OPEN_HONESTY_NOTES = [
  DOCS_11_ACCEPTANCE_NOTE,
  'Available now and expanding describe product status, not a promise that every future feature is finished.',
  'Companion remains early access: field inbox and handoffs exist, with more workflow depth still growing.',
  'This public site explains the platform. Customer records and daily work happen after secure sign-in.',
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
