import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useEffect, useRef, useState } from 'react'
import { Link, Navigate, useParams, useSearchParams } from 'react-router-dom'
import { PageHeader } from '@stl/shared-ui'
import {
  completeTrainingAssignment,
  createTrainingEvidence,
  getMe,
  getTrainingAssignment,
  getTrainingEvidence,
  submitTrainingEvaluation,
  submitTrainingSignoff,
} from '../api/client'
import {
  canCompleteAssignment,
  canSubmitEvaluation,
  canSubmitTraineeSignoff,
  canSubmitTrainerSignoff,
  canUploadEvidence,
  loadSession,
} from '../auth/sessionStorage'
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
    canUploadEvidence(me.tenantRoleKey, me.isPlatformAdmin, assignment.staffarrPersonId, me.personId)
  const canComplete =
    assignment &&
    canCompleteAssignment(me.tenantRoleKey, me.isPlatformAdmin, assignment.staffarrPersonId, me.personId)
    && assignment.completionRequirementsMet
  const canTraineeSign =
    assignment &&
    canSubmitTraineeSignoff(me.tenantRoleKey, me.isPlatformAdmin, assignment.staffarrPersonId, me.personId)
  const canEvaluate = canSubmitEvaluation(me.tenantRoleKey, me.isPlatformAdmin)
  const canTrainerSign = canSubmitTrainerSignoff(me.tenantRoleKey, me.isPlatformAdmin)

  return (
    <div className="mx-auto max-w-3xl space-y-6" data-testid="assignment-workspace">
      <PageHeader
        title="Training assignment"
        subtitle={assignment?.trainingDefinitionName ?? 'Loading assignment…'}
      />

      <p className="text-sm">
        <Link to="/" className="text-violet-300 hover:text-violet-200">
          ← Back to training workspace
        </Link>
      </p>

      {assignmentDetailQuery.isError ? (
        <p className="rounded-lg border border-rose-800/60 bg-rose-950/40 px-4 py-3 text-sm text-rose-200">
          Unable to load this assignment. It may have been completed or you may not have access.
        </p>
      ) : null}

      {assignment ? (
        <>
          <section className="rounded-xl border border-slate-700 bg-slate-900/60 p-4">
            <h2 className="text-sm font-semibold uppercase tracking-wide text-slate-400">Assignment detail</h2>
            <dl className="mt-3 space-y-2 text-sm">
              <div>
                <dt className="text-slate-500">Qualification</dt>
                <dd className="text-slate-100">{assignment.qualificationName}</dd>
              </div>
              <div>
                <dt className="text-slate-500">Status</dt>
                <dd className="text-slate-100">{assignment.status}</dd>
              </div>
              <div>
                <dt className="text-slate-500">Evidence on file</dt>
                <dd className="text-slate-100">{assignment.evidenceCount}</dd>
              </div>
              <div>
                <dt className="text-slate-500">Completion gate</dt>
                <dd className={assignment.completionRequirementsMet ? 'text-emerald-300' : 'text-amber-300'}>
                  {assignment.completionRequirementsMet
                    ? 'Ready to complete'
                    : 'Evaluation + signoffs required'}
                </dd>
              </div>
            </dl>
            {canComplete && assignment.status !== 'completed' ? (
              <button
                type="button"
                className="mt-4 rounded-md bg-emerald-600 px-4 py-2 text-sm font-medium text-white hover:bg-emerald-500 disabled:opacity-50"
                disabled={completeAssignmentMutation.isPending}
                onClick={() => completeAssignmentMutation.mutate()}
              >
                {completeAssignmentMutation.isPending ? 'Completing…' : 'Mark assignment complete'}
              </button>
            ) : null}
          </section>

          <div ref={evidenceSectionRef} id="assignment-evidence" data-testid="assignment-evidence-section">
            <EvidenceCapturePanel
              assignment={assignment}
              evidence={evidenceQuery.data ?? []}
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
            assignment={assignment}
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
        </>
      ) : null}
    </div>
  )
}
