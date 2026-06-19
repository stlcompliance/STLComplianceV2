import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useEffect, useRef, useState } from 'react'
import { Link, Navigate, useParams, useSearchParams } from 'react-router-dom'
import {
  AlertTriangle,
  BookOpen,
  CheckCircle2,
  ClipboardCheck,
  FileText,
  GraduationCap,
  History,
  ShieldCheck,
  Wrench,
} from 'lucide-react'
import {
  DetailBadge,
  DetailEmptyState,
  ProfileDetailsLayout,
  type DetailRailSectionConfig,
  type DetailTone,
} from '@stl/shared-ui'
import {
  completeTrainingAssignment,
  createTrainingAssignmentMaterialDemandLine,
  createTrainingAssignmentLaborEntry,
  createTrainingEvidence,
  getMe,
  getTrainingAssignment,
  getTrainingAssignmentMaterialDemand,
  getTrainingAssignmentMaterialDemandStatusEvents,
  getTrainingAssignmentLaborEntries,
  getTrainingAssignmentSteps,
  getTrainingEvidence,
  getTrainingEvaluationHistory,
  publishTrainingAssignmentMaterialDemand,
  removeTrainingAssignmentLaborEntry,
  submitTrainingAssignmentStep,
  submitTrainingEvaluation,
  submitTrainingSignoff,
} from '../api/client'
import {
  canCompleteAssignment,
  canManageAssignments,
  canSubmitEvaluation,
  canSubmitTraineeSignoff,
  canSubmitTrainerSignoff,
  canUploadEvidence,
  loadSession,
} from '../auth/sessionStorage'
import { AssignmentMaterialDemandPanel } from '../components/AssignmentMaterialDemandPanel'
import { AssignmentLaborPanel } from '../components/AssignmentLaborPanel'
import { AssignmentStepsPanel } from '../components/AssignmentStepsPanel'
import { EvidenceCapturePanel } from '../components/EvidenceCapturePanel'
import { SignoffEvaluationPanel } from '../components/SignoffEvaluationPanel'

async function fileToBase64(file: File): Promise<string> {
  const buffer = await file.arrayBuffer()
  const bytes = new Uint8Array(buffer)
  let binary = ''
  bytes.forEach((byte) => {
    binary += String.fromCharCode(byte)
  })
  return btoa(binary)
}

interface AssignmentWorkspacePageProps {
  focus?: 'evidence'
}

function humanize(value: string | null | undefined): string {
  if (!value) return 'Not recorded'
  return value.replace(/[_-]+/g, ' ').replace(/\b\w/g, (char) => char.toUpperCase())
}

function formatDateTime(value: string | null | undefined): string {
  if (!value) return 'Not recorded'
  const date = new Date(value)
  if (Number.isNaN(date.getTime())) return 'Not recorded'
  return date.toLocaleString()
}

function statusTone(value: string | null | undefined): DetailTone {
  const normalized = value?.toLowerCase() ?? ''
  if (['completed', 'complete', 'published', 'acknowledged'].includes(normalized)) return 'good'
  if (['in_progress', 'pending', 'draft', 'review'].includes(normalized)) return 'warn'
  if (['cancelled', 'failed', 'blocked', 'expired'].includes(normalized)) return 'bad'
  return 'neutral'
}

export function AssignmentWorkspacePage({ focus }: AssignmentWorkspacePageProps) {
  const { assignmentId } = useParams<{ assignmentId: string }>()
  const [searchParams] = useSearchParams()
  const handoff = searchParams.get('handoff')
  if (handoff) {
    return <Navigate to={`/launch?handoff=${encodeURIComponent(handoff)}`} replace />
  }

  const session = loadSession()
  const queryClient = useQueryClient()
  const evidenceSectionRef = useRef<HTMLDivElement>(null)

  const [evidenceTypeKey, setEvidenceTypeKey] = useState('completion_certificate')
  const [evidenceNotes, setEvidenceNotes] = useState('')
  const [evidenceFile, setEvidenceFile] = useState<File | null>(null)
  const [evaluationResult, setEvaluationResult] = useState('pass')
  const [evaluationScore, setEvaluationScore] = useState('')
  const [evaluationNotes, setEvaluationNotes] = useState('')
  const [signoffNotes, setSignoffNotes] = useState('')
  const [demandPartNumber, setDemandPartNumber] = useState('')
  const [demandSupplyarrPartId, setDemandSupplyarrPartId] = useState('')
  const [demandQuantity, setDemandQuantity] = useState('1')
  const [demandUnitOfMeasure, setDemandUnitOfMeasure] = useState('each')
  const [demandNotes, setDemandNotes] = useState('')
  const [createPurchaseRequestDraft, setCreatePurchaseRequestDraft] = useState(false)
  const [laborTypeKey, setLaborTypeKey] = useState('delivery')
  const [laborHoursWorked, setLaborHoursWorked] = useState('1')
  const [laborCostPerHour, setLaborCostPerHour] = useState('0')
  const [laborNotes, setLaborNotes] = useState('')
  const [removingLaborEntryId, setRemovingLaborEntryId] = useState<string | null>(null)

  const meQuery = useQuery({
    queryKey: ['trainarr-me', session?.accessToken],
    queryFn: () => getMe(session!.accessToken),
    enabled: Boolean(session?.accessToken),
    retry: false,
  })

  const assignmentDetailQuery = useQuery({
    queryKey: ['trainarr-assignment', session?.accessToken, assignmentId],
    queryFn: () => getTrainingAssignment(session!.accessToken, assignmentId!),
    enabled: Boolean(session?.accessToken && assignmentId),
  })

  const evidenceQuery = useQuery({
    queryKey: ['trainarr-evidence', session?.accessToken, assignmentId],
    queryFn: () => getTrainingEvidence(session!.accessToken, assignmentId!),
    enabled: Boolean(session?.accessToken && assignmentId),
  })

  const assignmentStepsQuery = useQuery({
    queryKey: ['trainarr-assignment-steps', session?.accessToken, assignmentId],
    queryFn: () => getTrainingAssignmentSteps(session!.accessToken, assignmentId!),
    enabled: Boolean(session?.accessToken && assignmentId),
  })

  const materialDemandQuery = useQuery({
    queryKey: ['trainarr-material-demand', session?.accessToken, assignmentId],
    queryFn: () => getTrainingAssignmentMaterialDemand(session!.accessToken, assignmentId!),
    enabled: Boolean(session?.accessToken && assignmentId),
  })

  const materialDemandStatusEventsQuery = useQuery({
    queryKey: ['trainarr-material-demand-status-events', session?.accessToken, assignmentId],
    queryFn: () =>
      getTrainingAssignmentMaterialDemandStatusEvents(session!.accessToken, assignmentId!),
    enabled: Boolean(session?.accessToken && assignmentId),
  })

  const laborQuery = useQuery({
    queryKey: ['trainarr-assignment-labor', session?.accessToken, assignmentId],
    queryFn: () => getTrainingAssignmentLaborEntries(session!.accessToken, assignmentId!),
    enabled: Boolean(session?.accessToken && assignmentId),
  })

  const evaluationHistoryQuery = useQuery({
    queryKey: ['trainarr-evaluation-history', session?.accessToken, assignmentId],
    queryFn: () => getTrainingEvaluationHistory(session!.accessToken, assignmentId!),
    enabled: Boolean(session?.accessToken && assignmentId),
  })

  const uploadEvidenceMutation = useMutation({
    mutationFn: async () => {
      if (!assignmentId || !evidenceFile) {
        throw new Error('Select a file to upload.')
      }
      const contentBase64 = await fileToBase64(evidenceFile)
      return createTrainingEvidence(session!.accessToken, assignmentId, {
        evidenceTypeKey,
        fileName: evidenceFile.name,
        contentType: evidenceFile.type || 'application/octet-stream',
        contentBase64,
        notes: evidenceNotes || null,
      })
    },
    onSuccess: () => {
      setEvidenceFile(null)
      setEvidenceNotes('')
      void queryClient.invalidateQueries({ queryKey: ['trainarr-evidence', session?.accessToken, assignmentId] })
      void queryClient.invalidateQueries({ queryKey: ['trainarr-assignment', session?.accessToken, assignmentId] })
      void queryClient.invalidateQueries({ queryKey: ['trainarr-assignments'] })
    },
  })

  const submitEvaluationMutation = useMutation({
    mutationFn: async () => {
      if (!assignmentId) {
        throw new Error('Assignment is required.')
      }
      const score = evaluationScore.trim() ? Number(evaluationScore) : null
      return submitTrainingEvaluation(session!.accessToken, assignmentId, {
        trainingAssignmentId: assignmentId,
        result: evaluationResult,
        score: Number.isFinite(score) ? score : null,
        notes: evaluationNotes || null,
      })
    },
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ['trainarr-assignment', session?.accessToken, assignmentId] })
      void queryClient.invalidateQueries({ queryKey: ['trainarr-assignments'] })
      void queryClient.invalidateQueries({ queryKey: ['trainarr-evaluation-history', session?.accessToken, assignmentId] })
      void queryClient.invalidateQueries({ queryKey: ['trainarr-evaluation-review-timeline', session?.accessToken] })
    },
  })

  const submitTraineeSignoffMutation = useMutation({
    mutationFn: async () => {
      if (!assignmentId) {
        throw new Error('Assignment is required.')
      }
      return submitTrainingSignoff(session!.accessToken, assignmentId, {
        trainingAssignmentId: assignmentId,
        signoffRole: 'trainee',
        notes: signoffNotes || null,
      })
    },
    onSuccess: () => {
      setSignoffNotes('')
      void queryClient.invalidateQueries({ queryKey: ['trainarr-assignment', session?.accessToken, assignmentId] })
    },
  })

  const submitTrainerSignoffMutation = useMutation({
    mutationFn: async () => {
      if (!assignmentId) {
        throw new Error('Assignment is required.')
      }
      return submitTrainingSignoff(session!.accessToken, assignmentId, {
        trainingAssignmentId: assignmentId,
        signoffRole: 'trainer',
        notes: signoffNotes || null,
      })
    },
    onSuccess: () => {
      setSignoffNotes('')
      void queryClient.invalidateQueries({ queryKey: ['trainarr-assignment', session?.accessToken, assignmentId] })
    },
  })

  const completeAssignmentMutation = useMutation({
    mutationFn: () => completeTrainingAssignment(session!.accessToken, assignmentId!),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ['trainarr-assignments'] })
      void queryClient.invalidateQueries({ queryKey: ['trainarr-assignment', session?.accessToken, assignmentId] })
    },
  })

  const submitAssignmentStepMutation = useMutation({
    mutationFn: async ({
      stepId,
      payload,
    }: {
      stepId: string
      payload: {
        selectedOptionIndexes?: number[]
        practicalResult?: string
        notes?: string
        contentAcknowledged?: boolean
        practicalObservationNotes?: string
        safetyCriticalFailure?: boolean
        failureComments?: string
        traineeAcknowledged?: boolean
        retestRequired?: boolean
      }
    }) => submitTrainingAssignmentStep(session!.accessToken, assignmentId!, stepId, payload),
    onSuccess: () => {
      void queryClient.invalidateQueries({
        queryKey: ['trainarr-assignment-steps', session?.accessToken, assignmentId],
      })
      void queryClient.invalidateQueries({ queryKey: ['trainarr-assignment', session?.accessToken, assignmentId] })
    },
  })

  const invalidateMaterialDemand = () => {
    void queryClient.invalidateQueries({
      queryKey: ['trainarr-material-demand', session?.accessToken, assignmentId],
    })
    void queryClient.invalidateQueries({
      queryKey: ['trainarr-material-demand-status-events', session?.accessToken, assignmentId],
    })
  }

  const addMaterialDemandLineMutation = useMutation({
    mutationFn: async () => {
      const quantity = Number(demandQuantity)
      if (!Number.isFinite(quantity) || quantity <= 0) {
        throw new Error('Quantity must be greater than zero.')
      }
      const supplyarrPartId = demandSupplyarrPartId.trim()
      return createTrainingAssignmentMaterialDemandLine(session!.accessToken, assignmentId!, {
        supplyarrPartId: supplyarrPartId ? supplyarrPartId : null,
        partNumber: demandPartNumber.trim() || null,
        quantityRequested: quantity,
        unitOfMeasure: demandUnitOfMeasure.trim() || 'each',
        notes: demandNotes.trim() || null,
      })
    },
    onSuccess: () => {
      setDemandPartNumber('')
      setDemandSupplyarrPartId('')
      setDemandQuantity('1')
      setDemandNotes('')
      invalidateMaterialDemand()
    },
  })

  const publishMaterialDemandMutation = useMutation({
    mutationFn: () =>
      publishTrainingAssignmentMaterialDemand(session!.accessToken, assignmentId!, {
        createPurchaseRequestDraft,
      }),
    onSuccess: () => invalidateMaterialDemand(),
  })

  const addLaborEntryMutation = useMutation({
    mutationFn: async () => {
      if (!assignmentId) {
        throw new Error('Assignment is required.')
      }
      const hoursWorked = Number(laborHoursWorked)
      const costPerHour = Number(laborCostPerHour)
      if (!Number.isFinite(hoursWorked) || hoursWorked <= 0) {
        throw new Error('Labor hours must be greater than zero.')
      }
      if (!Number.isFinite(costPerHour) || costPerHour < 0) {
        throw new Error('Cost per hour must be zero or greater.')
      }
      return createTrainingAssignmentLaborEntry(session!.accessToken, assignmentId, {
        laborTypeKey,
        hoursWorked,
        costPerHour,
        notes: laborNotes.trim() || null,
      })
    },
    onSuccess: () => {
      setLaborHoursWorked('1')
      setLaborCostPerHour('0')
      setLaborNotes('')
      void queryClient.invalidateQueries({ queryKey: ['trainarr-assignment-labor', session?.accessToken, assignmentId] })
      void queryClient.invalidateQueries({ queryKey: ['trainarr-assignment-report-summary', session?.accessToken] })
      void queryClient.invalidateQueries({ queryKey: ['trainarr-assignment', session?.accessToken, assignmentId] })
    },
  })

  const removeLaborEntryMutation = useMutation({
    mutationFn: async (laborEntryId: string) => {
      if (!assignmentId) {
        throw new Error('Assignment is required.')
      }
      setRemovingLaborEntryId(laborEntryId)
      await removeTrainingAssignmentLaborEntry(session!.accessToken, assignmentId, laborEntryId)
    },
    onSettled: () => {
      setRemovingLaborEntryId(null)
      void queryClient.invalidateQueries({ queryKey: ['trainarr-assignment-labor', session?.accessToken, assignmentId] })
      void queryClient.invalidateQueries({ queryKey: ['trainarr-assignment-report-summary', session?.accessToken] })
      void queryClient.invalidateQueries({ queryKey: ['trainarr-assignment', session?.accessToken, assignmentId] })
    },
  })

  const assignment = assignmentDetailQuery.data

  useEffect(() => {
    if (focus !== 'evidence' || !assignment) {
      return
    }

    if (typeof evidenceSectionRef.current?.scrollIntoView === 'function') {
      evidenceSectionRef.current.scrollIntoView({ behavior: 'smooth', block: 'start' })
    }
  }, [focus, assignment])

  if (!session || !meQuery.data) {
    return <p className="text-sm text-slate-400">Loading training assignment…</p>
  }

  if (!assignmentId) {
    return <Navigate to="/" replace />
  }

  const me = meQuery.data
  const canUploadForAssignment =
    assignment &&
    !assignment.staffarrAcknowledgementRequired &&
    canUploadEvidence(me.tenantRoleKey, me.isPlatformAdmin, assignment.staffarrPersonId, me.personId)
  const canSubmitSteps =
    assignment &&
    canCompleteAssignment(me.tenantRoleKey, me.isPlatformAdmin, assignment.staffarrPersonId, me.personId)
  const canComplete =
    assignment &&
    canCompleteAssignment(me.tenantRoleKey, me.isPlatformAdmin, assignment.staffarrPersonId, me.personId)
    && assignment.completionRequirementsMet
  const canTraineeSign =
    assignment &&
    canSubmitTraineeSignoff(me.tenantRoleKey, me.isPlatformAdmin, assignment.staffarrPersonId, me.personId)
  const canEvaluate = canSubmitEvaluation(me.tenantRoleKey, me.isPlatformAdmin)
  const canTrainerSign = canSubmitTrainerSignoff(me.tenantRoleKey, me.isPlatformAdmin)
  const canManageDemand = canManageAssignments(me.tenantRoleKey, me.isPlatformAdmin)

  const detail = assignment
  const steps = assignmentStepsQuery.data ?? []
  const evidence = evidenceQuery.data ?? []
  const laborEntries = laborQuery.data ?? []
  const demandLines = materialDemandQuery.data ?? []
  const statusEvents = materialDemandStatusEventsQuery.data ?? []
  const evaluationHistory = evaluationHistoryQuery.data?.items ?? []
  const blocked = !detail || detail.status === 'cancelled' || !detail.completionRequirementsMet
  const readinessTone: DetailTone = blocked ? 'warn' : 'good'
  const railSections: DetailRailSectionConfig[] = [
    {
      title: 'Readiness summary',
      icon: <ShieldCheck className="h-5 w-5" />,
      content: detail ? (
        <div className="space-y-3">
          <div className="flex flex-wrap items-center gap-2">
            <DetailBadge
              label={detail.completionRequirementsMet ? 'Ready to complete' : 'Needs evaluation'}
              tone={detail.completionRequirementsMet ? 'good' : 'warn'}
            />
            <DetailBadge
              label={detail.staffarrAcknowledgementRequired ? 'StaffArr acknowledgement required' : 'Acknowledgement complete'}
              tone={detail.staffarrAcknowledgementRequired ? 'warn' : 'good'}
            />
          </div>
          <p className="text-sm text-slate-300">
            {detail.completionRequirementsMet
              ? 'Evaluation, signoffs, and evidence support normal assignment closeout.'
              : 'Evaluation, signoffs, and evidence are still required before closeout.'}
          </p>
          <div className="grid gap-3 sm:grid-cols-2">
            <div className="rounded-xl border border-slate-800 bg-slate-900/70 p-4">
              <p className="text-xs text-slate-400">Evidence on file</p>
              <p className="mt-1 text-xl font-semibold text-white">{detail.evidenceCount}</p>
            </div>
            <div className="rounded-xl border border-slate-800 bg-slate-900/70 p-4">
              <p className="text-xs text-slate-400">Qualification</p>
              <p className="mt-1 text-sm text-white">{detail.qualificationName}</p>
            </div>
          </div>
        </div>
      ) : (
        <DetailEmptyState text="No assignment has been loaded yet." />
      ),
    },
    {
      title: 'Workflow summary',
      icon: <ClipboardCheck className="h-5 w-5" />,
      content: (
        <div className="space-y-3 text-sm text-slate-300">
          <div className="grid gap-3 sm:grid-cols-2">
            <div className="rounded-xl border border-slate-800 bg-slate-900/70 p-4">
              <p className="text-xs text-slate-400">Steps</p>
              <p className="mt-1 text-lg font-semibold text-white">{steps.length}</p>
            </div>
            <div className="rounded-xl border border-slate-800 bg-slate-900/70 p-4">
              <p className="text-xs text-slate-400">Evidence</p>
              <p className="mt-1 text-lg font-semibold text-white">{evidence.length}</p>
            </div>
          </div>
          <p>
            {steps.length > 0
              ? 'Complete steps in order, then finalize evaluation and signoffs before closing the assignment.'
              : 'No structured steps were configured for this training definition.'}
          </p>
        </div>
      ),
    },
    {
      title: 'Activity',
      icon: <History className="h-5 w-5" />,
      content: (
        <div className="space-y-3 text-sm text-slate-300">
          <div className="rounded-xl border border-slate-800 bg-slate-900/70 p-4">
            <p className="text-xs text-slate-400">Evaluation history</p>
            <p className="mt-1 text-lg font-semibold text-white">{evaluationHistory.length}</p>
          </div>
          <div className="rounded-xl border border-slate-800 bg-slate-900/70 p-4">
            <p className="text-xs text-slate-400">Labor entries</p>
            <p className="mt-1 text-lg font-semibold text-white">{laborEntries.length}</p>
          </div>
          <div className="rounded-xl border border-slate-800 bg-slate-900/70 p-4">
            <p className="text-xs text-slate-400">Material demand lines</p>
            <p className="mt-1 text-lg font-semibold text-white">{demandLines.length}</p>
          </div>
        </div>
      ),
    },
  ]

  return (
    <ProfileDetailsLayout
      testId="assignment-workspace"
      backLabel="Training workspace"
      backTo="/"
      breadcrumbs={[detail?.qualificationName ?? 'Assignment', detail?.trainingDefinitionName ?? 'Loading assignment…']}
      icon={<GraduationCap className="h-9 w-9" />}
      title={detail?.trainingDefinitionName ?? 'Training assignment'}
      subtitle={
        <span className="flex flex-wrap items-center gap-2">
          <span>{detail?.qualificationName ?? 'Qualification not loaded'}</span>
          <span className="text-[var(--color-text-muted)]">-</span>
          <span>{detail ? humanize(detail.status) : 'Loading…'}</span>
          <span className="text-[var(--color-text-muted)]">-</span>
          <span>{detail?.staffarrPersonId ?? 'No StaffArr person'}</span>
        </span>
      }
      badges={[
        { label: detail?.status ?? 'Loading', tone: statusTone(detail?.status) },
        { label: detail?.completionRequirementsMet ? 'Ready' : 'In progress', tone: detail?.completionRequirementsMet ? 'good' : 'warn' },
        { label: detail?.staffarrAcknowledgementRequired ? 'StaffArr ack required' : 'Ack not required', tone: detail?.staffarrAcknowledgementRequired ? 'warn' : 'neutral' },
      ]}
      actions={
        <>
          {detail && canComplete && detail.status !== 'completed' ? (
            <button
              type="button"
              className="inline-flex items-center gap-2 rounded-xl bg-emerald-600 px-4 py-3 text-sm font-bold text-white hover:bg-emerald-500 disabled:opacity-50"
              disabled={completeAssignmentMutation.isPending}
              onClick={() => completeAssignmentMutation.mutate()}
            >
              {completeAssignmentMutation.isPending ? 'Completing…' : 'Mark complete'}
            </button>
          ) : null}
          <Link
            to="/"
            className="inline-flex items-center gap-2 rounded-xl border border-slate-700 bg-slate-900 px-4 py-3 text-sm font-bold text-white hover:bg-slate-800"
          >
            Back to workspace
          </Link>
        </>
      }
      metrics={[
        {
          label: 'Status',
          value: detail ? humanize(detail.status) : 'Loading',
          hint: detail?.trainingDefinitionName ?? 'Training definition',
          icon: <ShieldCheck className="h-5 w-5" />,
          tone: detail ? statusTone(detail.status) : 'neutral',
        },
        {
          label: 'Evidence',
          value: detail?.evidenceCount ?? 0,
          hint: 'Evidence on file',
          icon: <FileText className="h-5 w-5" />,
          tone: (detail?.evidenceCount ?? 0) > 0 ? 'good' : 'warn',
        },
        {
          label: 'Steps',
          value: steps.length,
          hint: 'Structured workflow steps',
          icon: <BookOpen className="h-5 w-5" />,
          tone: steps.length > 0 ? 'good' : 'neutral',
        },
        {
          label: 'Labor',
          value: laborEntries.length,
          hint: 'Training labor entries',
          icon: <Wrench className="h-5 w-5" />,
          tone: laborEntries.length > 0 ? 'info' : 'neutral',
        },
      ]}
      tabs={['Overview', 'Steps', 'Evidence', 'Labor', 'Demand', 'History']}
      snapshotTitle="Assignment snapshot"
      snapshotSubtitle="Assignment identity, qualification, completion gate, StaffArr acknowledgement, and source labels."
      snapshotFields={[
        { label: 'Assignment ID', value: detail?.assignmentId ?? 'Loading', source: 'TrainArr source of truth' },
        { label: 'Training definition', value: detail?.trainingDefinitionName ?? 'Loading', source: 'Training definition' },
        { label: 'Qualification', value: detail?.qualificationName ?? 'Loading', source: 'Qualification record' },
        { label: 'Person', value: detail?.staffarrPersonId ?? 'Not recorded', source: 'StaffArr personId' },
        { label: 'Status', value: detail ? humanize(detail.status) : 'Loading', source: 'Assignment lifecycle' },
        { label: 'Evidence count', value: detail?.evidenceCount ?? 0, source: 'Evidence records' },
        { label: 'Completion gate', value: detail?.completionRequirementsMet ? 'Ready to complete' : 'Requires evaluation and signoffs', source: 'Completion readiness' },
        { label: 'Created', value: formatDateTime(detail?.createdAt), source: 'Audit trail' },
        { label: 'Updated', value: formatDateTime(detail?.updatedAt), source: 'Audit trail' },
      ]}
      mainContent={
        <div className="space-y-6">
          {assignmentDetailQuery.isError ? (
            <p className="rounded-lg border border-rose-800/60 bg-rose-950/40 px-4 py-3 text-sm text-rose-200">
              Unable to load this assignment. It may have been completed or you may not have access.
            </p>
          ) : null}

          {!detail ? (
            <DetailEmptyState text={assignmentDetailQuery.isLoading ? 'Loading assignment…' : 'Assignment not found.'} />
          ) : (
            <>
              {detail.staffarrAcknowledgementRequired ? (
                <p
                  className="rounded-lg border border-amber-800/60 bg-amber-950/40 px-4 py-3 text-sm text-amber-100"
                  data-testid="staffarr-acknowledgement-required"
                >
                  Acknowledge this training assignment in StaffArr before uploading evidence. Current StaffArr status:{' '}
                  <span className="font-medium">{detail.staffarrAcknowledgementStatus ?? 'pending'}</span>.
                </p>
              ) : null}

              {detail.staffarrAcknowledgementStatus === 'acknowledged' ? (
                <p className="rounded-lg border border-emerald-800/50 bg-emerald-950/30 px-4 py-2 text-sm text-emerald-200">
                  StaffArr acknowledgement recorded
                  {detail.staffarrAcknowledgementAt ? ` at ${formatDateTime(detail.staffarrAcknowledgementAt)}` : ''}.
                </p>
              ) : null}

              <section className="rounded-xl border border-slate-700 bg-slate-900/60 p-4">
                <h2 className="text-sm font-semibold uppercase tracking-wide text-slate-400">Assignment detail</h2>
                <dl className="mt-3 space-y-2 text-sm">
                  <div>
                    <dt className="text-[var(--color-text-muted)]">Qualification</dt>
                    <dd className="text-slate-100">{detail.qualificationName}</dd>
                  </div>
                  <div>
                    <dt className="text-[var(--color-text-muted)]">Status</dt>
                    <dd className="text-slate-100">{detail.status}</dd>
                  </div>
                  <div>
                    <dt className="text-[var(--color-text-muted)]">Evidence on file</dt>
                    <dd className="text-slate-100">{detail.evidenceCount}</dd>
                  </div>
                  <div>
                    <dt className="text-[var(--color-text-muted)]">Completion gate</dt>
                    <dd className={detail.completionRequirementsMet ? 'text-emerald-300' : 'text-amber-300'}>
                      {detail.completionRequirementsMet ? 'Ready to complete' : 'Evaluation + signoffs required'}
                    </dd>
                  </div>
                </dl>
              </section>

              <AssignmentStepsPanel
                steps={steps}
                isLoading={assignmentStepsQuery.isLoading}
                canComplete={Boolean(canSubmitSteps)}
                canEvaluate={canEvaluate}
                isSubmitting={submitAssignmentStepMutation.isPending}
                onSubmitStep={async (stepId, payload) => {
                  await submitAssignmentStepMutation.mutateAsync({ stepId, payload })
                }}
              />

              <div ref={evidenceSectionRef} id="assignment-evidence" data-testid="assignment-evidence-section">
                <EvidenceCapturePanel
                  assignment={detail}
                  evidence={evidence}
                  evidenceTypeKey={evidenceTypeKey}
                  notes={evidenceNotes}
                  selectedFileName={evidenceFile?.name ?? null}
                  onEvidenceTypeKeyChange={setEvidenceTypeKey}
                  onNotesChange={setEvidenceNotes}
                  onSelectFile={setEvidenceFile}
                  onUploadEvidence={() => uploadEvidenceMutation.mutate()}
                  isUploading={uploadEvidenceMutation.isPending}
                  canUpload={Boolean(canUploadForAssignment)}
                />
              </div>

              <SignoffEvaluationPanel
                assignment={detail}
                evaluationHistory={evaluationHistory}
                isLoadingHistory={evaluationHistoryQuery.isLoading}
                evaluationResult={evaluationResult}
                evaluationScore={evaluationScore}
                evaluationNotes={evaluationNotes}
                signoffNotes={signoffNotes}
                onEvaluationResultChange={setEvaluationResult}
                onEvaluationScoreChange={setEvaluationScore}
                onEvaluationNotesChange={setEvaluationNotes}
                onSignoffNotesChange={setSignoffNotes}
                onSubmitEvaluation={() => submitEvaluationMutation.mutate()}
                onSubmitTraineeSignoff={() => submitTraineeSignoffMutation.mutate()}
                onSubmitTrainerSignoff={() => submitTrainerSignoffMutation.mutate()}
                isSubmittingEvaluation={submitEvaluationMutation.isPending}
                isSubmittingTraineeSignoff={submitTraineeSignoffMutation.isPending}
                isSubmittingTrainerSignoff={submitTrainerSignoffMutation.isPending}
                canSubmitEvaluation={canEvaluate}
                canSubmitTraineeSignoff={Boolean(canTraineeSign)}
                canSubmitTrainerSignoff={canTrainerSign}
              />

              <AssignmentLaborPanel
                laborEntries={laborEntries}
                canManage={Boolean(canManageDemand)}
                laborTypeKey={laborTypeKey}
                hoursWorked={laborHoursWorked}
                costPerHour={laborCostPerHour}
                notes={laborNotes}
                onLaborTypeKeyChange={setLaborTypeKey}
                onHoursWorkedChange={setLaborHoursWorked}
                onCostPerHourChange={setLaborCostPerHour}
                onNotesChange={setLaborNotes}
                onAddLaborEntry={() => addLaborEntryMutation.mutate()}
                onRemoveLaborEntry={async (laborEntryId) => {
                  await removeLaborEntryMutation.mutateAsync(laborEntryId)
                }}
                isAdding={addLaborEntryMutation.isPending}
                removingId={removingLaborEntryId}
              />

              <AssignmentMaterialDemandPanel
                assignment={detail}
                demandLines={demandLines}
                statusEvents={statusEvents}
                canManage={canManageDemand}
                partNumber={demandPartNumber}
                supplyarrPartId={demandSupplyarrPartId}
                quantityRequested={demandQuantity}
                unitOfMeasure={demandUnitOfMeasure}
                notes={demandNotes}
                createPurchaseRequestDraft={createPurchaseRequestDraft}
                onPartNumberChange={setDemandPartNumber}
                onSupplyarrPartIdChange={setDemandSupplyarrPartId}
                onQuantityRequestedChange={setDemandQuantity}
                onUnitOfMeasureChange={setDemandUnitOfMeasure}
                onNotesChange={setDemandNotes}
                onCreatePurchaseRequestDraftChange={setCreatePurchaseRequestDraft}
                onAddDemandLine={() => addMaterialDemandLineMutation.mutate()}
                onPublishDemand={() => publishMaterialDemandMutation.mutate()}
                isAdding={addMaterialDemandLineMutation.isPending}
                isPublishing={publishMaterialDemandMutation.isPending}
              />
            </>
          )}
        </div>
      }
      decisionTitle="Assignment decision"
      decisionBadge={{ label: blocked ? 'Needs work' : 'Ready', tone: readinessTone }}
      decisionIcon={
        blocked ? (
          <AlertTriangle className="h-5 w-5 text-amber-300" />
        ) : (
          <CheckCircle2 className="h-5 w-5 text-emerald-300" />
        )
      }
      decisionSummary={blocked ? 'Assignment needs workflow attention' : 'Assignment can be completed'}
      decisionDetail={
        blocked
          ? 'Evaluation, signoffs, or acknowledgement work still needs to be completed before the assignment can close.'
          : 'Evidence, evaluation, and signoffs support normal completion.'
      }
      allowedChecks={[
        Boolean(detail?.completionRequirementsMet),
        Boolean(detail?.evidenceCount),
        Boolean(detail?.staffarrAcknowledgementStatus === 'acknowledged' || !detail?.staffarrAcknowledgementRequired),
      ].filter(Boolean).length}
      blockedChecks={[
        !detail?.completionRequirementsMet,
        detail?.staffarrAcknowledgementRequired && detail?.staffarrAcknowledgementStatus !== 'acknowledged',
      ].filter(Boolean).length}
      railSections={railSections}
    />
  )
}
