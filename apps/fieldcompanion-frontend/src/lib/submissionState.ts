export const SUBMISSION_STATE_STORAGE_KEY = 'stl-fieldcompanion-submission-state-v1'

export type SubmissionKind = 'acknowledge' | 'clock' | 'evidence' | 'dvir' | 'inspection' | 'work-order' | 'receiving'
export type LocalSubmissionPhase = 'queued' | 'syncing' | 'uploading' | 'synced' | 'failed'

export interface LocalSubmissionEntry {
  taskKey: string
  kind: SubmissionKind
  phase: LocalSubmissionPhase
  message?: string
  updatedAt: string
}

export type SubmissionToastTone = 'success' | 'error' | 'info'

export interface SubmissionToast {
  id: string
  tone: SubmissionToastTone
  message: string
  createdAt: string
}

interface SubmissionStateSnapshot {
  entries: LocalSubmissionEntry[]
  toasts: SubmissionToast[]
}

const listeners = new Set<() => void>()

function notifyListeners(): void {
  listeners.forEach((listener) => listener())
}

export function subscribeSubmissionState(listener: () => void): () => void {
  listeners.add(listener)
  return () => listeners.delete(listener)
}

function readSnapshot(): SubmissionStateSnapshot {
  if (typeof window === 'undefined') {
    return { entries: [], toasts: [] }
  }

  try {
    const raw = window.localStorage.getItem(SUBMISSION_STATE_STORAGE_KEY)
    if (!raw) {
      return { entries: [], toasts: [] }
    }

    const parsed = JSON.parse(raw) as SubmissionStateSnapshot
    return {
      entries: Array.isArray(parsed.entries) ? parsed.entries : [],
      toasts: Array.isArray(parsed.toasts) ? parsed.toasts : [],
    }
  } catch {
    return { entries: [], toasts: [] }
  }
}

function writeSnapshot(snapshot: SubmissionStateSnapshot): void {
  window.localStorage.setItem(SUBMISSION_STATE_STORAGE_KEY, JSON.stringify(snapshot))
  notifyListeners()
}

function entryKey(taskKey: string, kind: SubmissionKind): string {
  return `${taskKey}\0${kind}`
}

export function setLocalSubmission(input: {
  taskKey: string
  kind: SubmissionKind
  phase: LocalSubmissionPhase
  message?: string
}): void {
  const snapshot = readSnapshot()
  const key = entryKey(input.taskKey, input.kind)
  const without = snapshot.entries.filter(
    (entry) => entryKey(entry.taskKey, entry.kind) !== key,
  )
  without.push({
    taskKey: input.taskKey,
    kind: input.kind,
    phase: input.phase,
    message: input.message,
    updatedAt: new Date().toISOString(),
  })
  snapshot.entries = without
  writeSnapshot(snapshot)
}

export function getLocalSubmission(
  taskKey: string,
  kind: SubmissionKind,
): LocalSubmissionEntry | undefined {
  return readSnapshot().entries.find(
    (entry) => entry.taskKey === taskKey && entry.kind === kind,
  )
}

export function getSubmissionToasts(): SubmissionToast[] {
  return readSnapshot().toasts
}

export function pushSubmissionToast(input: {
  tone: SubmissionToastTone
  message: string
}): SubmissionToast {
  const snapshot = readSnapshot()
  const toast: SubmissionToast = {
    id: crypto.randomUUID(),
    tone: input.tone,
    message: input.message,
    createdAt: new Date().toISOString(),
  }
  snapshot.toasts = [toast, ...snapshot.toasts].slice(0, 8)
  writeSnapshot(snapshot)
  return toast
}

export function dismissSubmissionToast(id: string): void {
  const snapshot = readSnapshot()
  snapshot.toasts = snapshot.toasts.filter((toast) => toast.id !== id)
  writeSnapshot(snapshot)
}

export function clearSubmissionStateForTests(): void {
  if (typeof window !== 'undefined') {
    window.localStorage.removeItem(SUBMISSION_STATE_STORAGE_KEY)
    notifyListeners()
  }
}

export interface MergedSubmissionChip {
  kind: SubmissionKind
  label: string
  tone: 'neutral' | 'pending' | 'progress' | 'success' | 'error'
  detail?: string
}

const inFlightPhases: ReadonlySet<LocalSubmissionPhase> = new Set([
  'queued',
  'syncing',
  'uploading',
])

export function mergeSubmissionChips(input: {
  taskKey: string
  acknowledgeLocal?: LocalSubmissionEntry
  evidenceLocal?: LocalSubmissionEntry
  dvirLocal?: LocalSubmissionEntry
  inspectionLocal?: LocalSubmissionEntry
  workOrderLocal?: LocalSubmissionEntry
  receivingLocal?: LocalSubmissionEntry
  serverItems: ReadonlyArray<{
    submissionKind: string
    status: string
    detailMessage: string | null
  }>
}): MergedSubmissionChip[] {
  const chips: MergedSubmissionChip[] = []

  const acknowledgeServer = input.serverItems.find((item) => item.submissionKind === 'acknowledge')
  const evidenceServer = input.serverItems.find((item) => item.submissionKind === 'evidence')
  const dvirServer = input.serverItems.find((item) => item.submissionKind === 'dvir')
  const inspectionServer = input.serverItems.find((item) => item.submissionKind === 'inspection')
  const workOrderServer = input.serverItems.find((item) => item.submissionKind === 'work-order')
  const receivingServer = input.serverItems.find((item) => item.submissionKind === 'receiving')

  const acknowledgeChip = buildChip(
    'acknowledge',
    'Acknowledgment',
    input.acknowledgeLocal,
    acknowledgeServer,
  )
  if (acknowledgeChip) {
    chips.push(acknowledgeChip)
  }

  const evidenceChip = buildChip('evidence', 'Evidence', input.evidenceLocal, evidenceServer)
  if (evidenceChip) {
    chips.push(evidenceChip)
  }

  const dvirChip = buildChip('dvir', 'DVIR', input.dvirLocal, dvirServer)
  if (dvirChip) {
    chips.push(dvirChip)
  }

  const inspectionChip = buildChip(
    'inspection',
    'Inspection',
    input.inspectionLocal,
    inspectionServer,
  )
  if (inspectionChip) {
    chips.push(inspectionChip)
  }

  const workOrderChip = buildChip(
    'work-order',
    'Work order',
    input.workOrderLocal,
    workOrderServer,
  )
  if (workOrderChip) {
    chips.push(workOrderChip)
  }

  const receivingChip = buildChip(
    'receiving',
    'Receiving',
    input.receivingLocal,
    receivingServer,
  )
  if (receivingChip) {
    chips.push(receivingChip)
  }

  return chips
}

function buildChip(
  kind: SubmissionKind,
  prefix: string,
  local: LocalSubmissionEntry | undefined,
  server:
    | {
        status: string
        detailMessage: string | null
      }
    | undefined,
): MergedSubmissionChip | undefined {
  if (local && inFlightPhases.has(local.phase)) {
    return {
      kind,
      label: localPhaseLabel(prefix, local.phase),
      tone: local.phase === 'queued' ? 'pending' : 'progress',
      detail: local.message,
    }
  }

  if (local?.phase === 'failed') {
    return {
      kind,
      label: `${prefix} failed`,
      tone: 'error',
      detail: local.message,
    }
  }

  if (local?.phase === 'synced') {
    return {
      kind,
      label: `${prefix} submitted`,
      tone: 'success',
      detail: local.message,
    }
  }

  if (server?.status === 'synced') {
    return {
      kind,
      label: `${prefix} submitted`,
      tone: 'success',
      detail: server.detailMessage ?? undefined,
    }
  }

  if (server?.status === 'failed') {
    return {
      kind,
      label: `${prefix} failed`,
      tone: 'error',
      detail: server.detailMessage ?? undefined,
    }
  }

  return undefined
}

function localPhaseLabel(prefix: string, phase: LocalSubmissionPhase): string {
  switch (phase) {
    case 'queued':
      return `${prefix} queued`
    case 'syncing':
      return `${prefix} syncing`
    case 'uploading':
      return `${prefix} uploading`
    case 'synced':
      return `${prefix} submitted`
    case 'failed':
      return `${prefix} failed`
    default:
      return prefix
  }
}
