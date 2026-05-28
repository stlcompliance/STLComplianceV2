import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'
import { Navigate, useSearchParams } from 'react-router-dom'
import { PageHeader } from '@stl/shared-ui'
import {
  completeTrainingAssignment,
  createQualificationCheck,
  createBatchQualificationCheck,
  createTrainingAssignment,
  createTrainingEvidence,
  createTrainingProgram,
  expireQualificationIssue,
  revokeQualificationIssue,
  submitTrainingEvaluation,
  submitTrainingSignoff,
  suspendQualificationIssue,
  getIncidentRemediations,
  getMe,
  getTrainingAssignment,
  getTrainingAssignments,
  getTrainingDefinitions,
  getTrainingEvidence,
  getTrainingPrograms,
  getTrainingDefinitionCitations,
  getTrainingProgramCitations,
  attachTrainingDefinitionCitation,
  attachTrainingProgramCitation,
  removeTrainingDefinitionCitation,
  removeTrainingProgramCitation,
  getTrainingDefinitionRulePackRequirements,
  getTrainingProgramRulePackRequirements,
  upsertTrainingDefinitionRulePackRequirement,
  upsertTrainingProgramRulePackRequirement,
  removeTrainingDefinitionRulePackRequirement,
  removeTrainingProgramRulePackRequirement,
  assessRulePackImpact,
} from '../api/client'
import {
  canCompleteAssignment,
  canManageAssignments,
  canManagePrograms,
  canAssessRulePackImpact,
  canManageNotificationSettings,
  canManageQualifications,
  canRunBatchQualificationChecks,
  canSubmitEvaluation,
  canSubmitTraineeSignoff,
  canSubmitTrainerSignoff,
  canUploadEvidence,
  loadSession,
} from '../auth/sessionStorage'
import { AssignmentsPanel } from '../components/AssignmentsPanel'
import { EvidenceCapturePanel } from '../components/EvidenceCapturePanel'
import { ProgramBuilderPanel } from '../components/ProgramBuilderPanel'
import { SignoffEvaluationPanel } from '../components/SignoffEvaluationPanel'
import { RemediationAssignmentPanel } from '../components/RemediationAssignmentPanel'
import { BatchQualificationCheckPanel } from '../components/BatchQualificationCheckPanel'
import { CitationAttachmentPanel } from '../components/CitationAttachmentPanel'
import { RulePackRequirementPanel } from '../components/RulePackRequirementPanel'
import { RulePackImpactPanel } from '../components/RulePackImpactPanel'
import { NotificationSettingsPanel } from '../components/NotificationSettingsPanel'

const personIdPattern =
  /[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}/gi

function parsePersonIdsFromText(text: string): string[] {
  const matches = text.match(personIdPattern) ?? []
  return [...new Set(matches.map((id) => id.toLowerCase()))]
}

async function fileToBase64(file: File): Promise<string> {
  const buffer = await file.arrayBuffer()
  const bytes = new Uint8Array(buffer)
  let binary = ''
  bytes.forEach((byte) => {
    binary += String.fromCharCode(byte)
  })
  return btoa(binary)
}

export function HomePage() {
  const [searchParams] = useSearchParams()
  const handoff = searchParams.get('handoff')
  if (handoff) {
    return <Navigate to={`/launch?handoff=${encodeURIComponent(handoff)}`} replace />
  }

  const session = loadSession()
  const queryClient = useQueryClient()
  const [selectedAssignmentId, setSelectedAssignmentId] = useState<string | null>(null)
  const [selectedRemediationId, setSelectedRemediationId] = useState<string | null>(null)
  const [selectedDefinitionId, setSelectedDefinitionId] = useState('')
  const [programKey, setProgramKey] = useState('')
  const [programName, setProgramName] = useState('')
  const [programDescription, setProgramDescription] = useState('')
  const [selectedProgramDefinitionIds, setSelectedProgramDefinitionIds] = useState<string[]>([])
  const [selectedProgramId, setSelectedProgramId] = useState<string | null>(null)
  const [selectedDefinitionIdForCitations, setSelectedDefinitionIdForCitations] = useState<string | null>(null)
  const [citationIdInput, setCitationIdInput] = useState('')
  const [citationKeyInput, setCitationKeyInput] = useState('')
  const [validateCitationWithComplianceCore, setValidateCitationWithComplianceCore] = useState(true)
  const [rulePackKeyInput, setRulePackKeyInput] = useState('')
  const [validateRulePackWithComplianceCore, setValidateRulePackWithComplianceCore] = useState(true)
  const [impactRulePackKeyInput, setImpactRulePackKeyInput] = useState('driver_qualification')
  const [rulePackImpactAssessment, setRulePackImpactAssessment] = useState<
    import('../api/types').RulePackImpactAssessmentResponse | null
  >(null)
  const [removingCitationId, setRemovingCitationId] = useState<string | null>(null)
  const [removingRulePackRequirementId, setRemovingRulePackRequirementId] = useState<string | null>(null)
  const [evidenceTypeKey, setEvidenceTypeKey] = useState('completion_certificate')
  const [evidenceNotes, setEvidenceNotes] = useState('')
  const [evidenceFile, setEvidenceFile] = useState<File | null>(null)
  const [evaluationResult, setEvaluationResult] = useState('pass')
  const [evaluationScore, setEvaluationScore] = useState('')
  const [evaluationNotes, setEvaluationNotes] = useState('')
  const [signoffNotes, setSignoffNotes] = useState('')
  const [lifecycleReason, setLifecycleReason] = useState('')
  const [rulePackKey, setRulePackKey] = useState('driver_qualification')
  const [qualificationCheck, setQualificationCheck] = useState<
    import('../api/types').QualificationCheckResponse | null
  >(null)
  const [batchQualificationKey, setBatchQualificationKey] = useState('hazmat_endorsement')
  const [batchPersonIdsText, setBatchPersonIdsText] = useState('')
  const [selectedBatchRemediationPersonIds, setSelectedBatchRemediationPersonIds] = useState<string[]>([])
  const [batchQualificationCheck, setBatchQualificationCheck] = useState<
    import('../api/types').BatchQualificationCheckResponse | null
  >(null)

  const meQuery = useQuery({
    queryKey: ['trainarr-me', session?.accessToken],
    queryFn: () => getMe(session!.accessToken),
    enabled: Boolean(session?.accessToken),
    retry: false,
  })

  const definitionsQuery = useQuery({
    queryKey: ['trainarr-definitions', session?.accessToken],
    queryFn: () => getTrainingDefinitions(session!.accessToken),
    enabled: Boolean(session?.accessToken) && meQuery.isSuccess,
  })

  const programsQuery = useQuery({
    queryKey: ['trainarr-programs', session?.accessToken],
    queryFn: () => getTrainingPrograms(session!.accessToken),
    enabled: Boolean(session?.accessToken) && meQuery.isSuccess,
  })

  const assignmentsQuery = useQuery({
    queryKey: ['trainarr-assignments', session?.accessToken],
    queryFn: () => getTrainingAssignments(session!.accessToken),
    enabled: Boolean(session?.accessToken) && meQuery.isSuccess,
  })

  const remediationsQuery = useQuery({
    queryKey: ['trainarr-remediations', session?.accessToken],
    queryFn: () => getIncidentRemediations(session!.accessToken, 'intake_received'),
    enabled: Boolean(session?.accessToken) && meQuery.isSuccess,
  })

  const assignmentDetailQuery = useQuery({
    queryKey: ['trainarr-assignment', session?.accessToken, selectedAssignmentId],
    queryFn: () => getTrainingAssignment(session!.accessToken, selectedAssignmentId!),
    enabled: Boolean(session?.accessToken && selectedAssignmentId),
  })

  const definitionCitationsQuery = useQuery({
    queryKey: ['trainarr-definition-citations', session?.accessToken, selectedDefinitionIdForCitations],
    queryFn: () => getTrainingDefinitionCitations(session!.accessToken, selectedDefinitionIdForCitations!),
    enabled: Boolean(session?.accessToken && selectedDefinitionIdForCitations),
  })

  const programCitationsQuery = useQuery({
    queryKey: ['trainarr-program-citations', session?.accessToken, selectedProgramId],
    queryFn: () => getTrainingProgramCitations(session!.accessToken, selectedProgramId!),
    enabled: Boolean(session?.accessToken && selectedProgramId),
  })

  const definitionRulePackRequirementsQuery = useQuery({
    queryKey: ['trainarr-definition-rule-packs', session?.accessToken, selectedDefinitionIdForCitations],
    queryFn: () =>
      getTrainingDefinitionRulePackRequirements(session!.accessToken, selectedDefinitionIdForCitations!),
    enabled: Boolean(session?.accessToken && selectedDefinitionIdForCitations),
  })

  const programRulePackRequirementsQuery = useQuery({
    queryKey: ['trainarr-program-rule-packs', session?.accessToken, selectedProgramId],
    queryFn: () => getTrainingProgramRulePackRequirements(session!.accessToken, selectedProgramId!),
    enabled: Boolean(session?.accessToken && selectedProgramId),
  })

  const evidenceQuery = useQuery({
    queryKey: ['trainarr-evidence', session?.accessToken, selectedAssignmentId],
    queryFn: () => getTrainingEvidence(session!.accessToken, selectedAssignmentId!),
    enabled: Boolean(session?.accessToken && selectedAssignmentId),
  })

  const batchQualificationCheckMutation = useMutation({
    mutationFn: async () => {
      const pastedIds = parsePersonIdsFromText(batchPersonIdsText)
      const personIds = [...new Set([...selectedBatchRemediationPersonIds, ...pastedIds])]
      if (personIds.length === 0) {
        throw new Error('Add at least one StaffArr person id or select people from remediations.')
      }
      const qualificationKey = batchQualificationKey.trim()
      if (!qualificationKey) {
        throw new Error('Qualification key is required for batch checks.')
      }
      return createBatchQualificationCheck(session!.accessToken, {
        qualificationKey,
        rulePackKey: rulePackKey.trim() || null,
        trainingDefinitionId: selectedDefinitionId || null,
        subjects: personIds.map((staffarrPersonId) => ({ staffarrPersonId, context: null })),
      })
    },
    onSuccess: (result) => {
      setBatchQualificationCheck(result)
    },
  })

  const qualificationCheckMutation = useMutation({
    mutationFn: async () => {
      const remediation = remediationsQuery.data?.find((r) => r.remediationId === selectedRemediationId)
      const definition = definitionsQuery.data?.find((d) => d.trainingDefinitionId === selectedDefinitionId)
      if (!remediation || !definition) {
        throw new Error('Select a remediation and training definition before running a check.')
      }
      return createQualificationCheck(session!.accessToken, {
        staffarrPersonId: remediation.staffarrPersonId,
        qualificationKey: definition.qualificationKey,
        rulePackKey: rulePackKey.trim() || null,
        trainingDefinitionId: selectedDefinitionId,
        context: null,
      })
    },
    onSuccess: (result) => {
      setQualificationCheck(result)
    },
  })

  const createAssignmentMutation = useMutation({
    mutationFn: async () => {
      const remediation = remediationsQuery.data?.find((r) => r.remediationId === selectedRemediationId)
      if (!remediation || !selectedDefinitionId) {
        throw new Error('Select a remediation and training definition.')
      }
      return createTrainingAssignment(session!.accessToken, {
        staffarrPersonId: remediation.staffarrPersonId,
        trainingDefinitionId: selectedDefinitionId,
        staffarrIncidentRemediationId: remediation.remediationId,
        assignmentReason: 'incident_remediation',
      })
    },
    onSuccess: (created) => {
      setSelectedAssignmentId(created.assignmentId)
      setSelectedRemediationId(null)
      void queryClient.invalidateQueries({ queryKey: ['trainarr-assignments'] })
      void queryClient.invalidateQueries({ queryKey: ['trainarr-remediations'] })
    },
  })

  const attachDefinitionCitationMutation = useMutation({
    mutationFn: async () => {
      if (!selectedDefinitionIdForCitations) {
        throw new Error('Select a training definition for citation attachment.')
      }
      return attachTrainingDefinitionCitation(
        session!.accessToken,
        selectedDefinitionIdForCitations,
        {
          complianceCoreCitationId: citationIdInput.trim(),
          citationKey: citationKeyInput.trim(),
        },
        validateCitationWithComplianceCore,
      )
    },
    onSuccess: () => {
      setCitationIdInput('')
      setCitationKeyInput('')
      void queryClient.invalidateQueries({
        queryKey: ['trainarr-definition-citations', session?.accessToken, selectedDefinitionIdForCitations],
      })
    },
  })

  const attachProgramCitationMutation = useMutation({
    mutationFn: async () => {
      if (!selectedProgramId) {
        throw new Error('Select a training program for citation attachment.')
      }
      return attachTrainingProgramCitation(
        session!.accessToken,
        selectedProgramId,
        {
          complianceCoreCitationId: citationIdInput.trim(),
          citationKey: citationKeyInput.trim(),
        },
        validateCitationWithComplianceCore,
      )
    },
    onSuccess: () => {
      setCitationIdInput('')
      setCitationKeyInput('')
      void queryClient.invalidateQueries({
        queryKey: ['trainarr-program-citations', session?.accessToken, selectedProgramId],
      })
    },
  })

  const upsertDefinitionRulePackMutation = useMutation({
    mutationFn: async () => {
      if (!selectedDefinitionIdForCitations) {
        throw new Error('Select a training definition for rule pack requirements.')
      }
      return upsertTrainingDefinitionRulePackRequirement(
        session!.accessToken,
        selectedDefinitionIdForCitations,
        { rulePackKey: rulePackKeyInput.trim() },
        validateRulePackWithComplianceCore,
      )
    },
    onSuccess: () => {
      setRulePackKeyInput('')
      void queryClient.invalidateQueries({
        queryKey: ['trainarr-definition-rule-packs', session?.accessToken, selectedDefinitionIdForCitations],
      })
    },
  })

  const upsertProgramRulePackMutation = useMutation({
    mutationFn: async () => {
      if (!selectedProgramId) {
        throw new Error('Select a training program for rule pack requirements.')
      }
      return upsertTrainingProgramRulePackRequirement(
        session!.accessToken,
        selectedProgramId,
        { rulePackKey: rulePackKeyInput.trim() },
        validateRulePackWithComplianceCore,
      )
    },
    onSuccess: () => {
      setRulePackKeyInput('')
      void queryClient.invalidateQueries({
        queryKey: ['trainarr-program-rule-packs', session?.accessToken, selectedProgramId],
      })
    },
  })

  const createProgramMutation = useMutation({
    mutationFn: async () =>
      createTrainingProgram(session!.accessToken, {
        programKey,
        name: programName,
        description: programDescription,
        trainingDefinitionIds: selectedProgramDefinitionIds,
      }),
    onSuccess: () => {
      setProgramKey('')
      setProgramName('')
      setProgramDescription('')
      setSelectedProgramDefinitionIds([])
      void queryClient.invalidateQueries({ queryKey: ['trainarr-programs'] })
    },
  })

  const rulePackImpactMutation = useMutation({
    mutationFn: async () =>
      assessRulePackImpact(session!.accessToken, {
        rulePackKey: impactRulePackKeyInput.trim(),
      }),
    onSuccess: (result) => {
      setRulePackImpactAssessment(result)
    },
  })

  const uploadEvidenceMutation = useMutation({
    mutationFn: async () => {
      if (!selectedAssignmentId || !evidenceFile) {
        throw new Error('Select an assignment and file.')
      }
      const contentBase64 = await fileToBase64(evidenceFile)
      return createTrainingEvidence(session!.accessToken, selectedAssignmentId, {
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
      void queryClient.invalidateQueries({ queryKey: ['trainarr-evidence', session?.accessToken, selectedAssignmentId] })
      void queryClient.invalidateQueries({ queryKey: ['trainarr-assignment', session?.accessToken, selectedAssignmentId] })
      void queryClient.invalidateQueries({ queryKey: ['trainarr-assignments'] })
    },
  })

  const submitEvaluationMutation = useMutation({
    mutationFn: async () => {
      if (!selectedAssignmentId) {
        throw new Error('Select an assignment.')
      }
      const score = evaluationScore.trim() ? Number(evaluationScore) : null
      return submitTrainingEvaluation(session!.accessToken, selectedAssignmentId, {
        trainingAssignmentId: selectedAssignmentId,
        result: evaluationResult,
        score: Number.isFinite(score) ? score : null,
        notes: evaluationNotes || null,
      })
    },
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ['trainarr-assignment', session?.accessToken, selectedAssignmentId] })
      void queryClient.invalidateQueries({ queryKey: ['trainarr-assignments'] })
    },
  })

  const submitTraineeSignoffMutation = useMutation({
    mutationFn: async () => {
      if (!selectedAssignmentId) {
        throw new Error('Select an assignment.')
      }
      return submitTrainingSignoff(session!.accessToken, selectedAssignmentId, {
        trainingAssignmentId: selectedAssignmentId,
        signoffRole: 'trainee',
        notes: signoffNotes || null,
      })
    },
    onSuccess: () => {
      setSignoffNotes('')
      void queryClient.invalidateQueries({ queryKey: ['trainarr-assignment', session?.accessToken, selectedAssignmentId] })
    },
  })

  const submitTrainerSignoffMutation = useMutation({
    mutationFn: async () => {
      if (!selectedAssignmentId) {
        throw new Error('Select an assignment.')
      }
      return submitTrainingSignoff(session!.accessToken, selectedAssignmentId, {
        trainingAssignmentId: selectedAssignmentId,
        signoffRole: 'trainer',
        notes: signoffNotes || null,
      })
    },
    onSuccess: () => {
      setSignoffNotes('')
      void queryClient.invalidateQueries({ queryKey: ['trainarr-assignment', session?.accessToken, selectedAssignmentId] })
    },
  })

  const completeAssignmentMutation = useMutation({
    mutationFn: (assignmentId: string) => completeTrainingAssignment(session!.accessToken, assignmentId),
    onSuccess: (_, assignmentId) => {
      void queryClient.invalidateQueries({ queryKey: ['trainarr-assignments'] })
      void queryClient.invalidateQueries({ queryKey: ['trainarr-assignment', session?.accessToken, assignmentId] })
    },
  })

  const suspendQualificationMutation = useMutation({
    mutationFn: (qualificationIssueId: string) =>
      suspendQualificationIssue(session!.accessToken, qualificationIssueId, {
        reason: lifecycleReason.trim() || null,
      }),
    onSuccess: () => {
      setLifecycleReason('')
      void queryClient.invalidateQueries({ queryKey: ['trainarr-assignment', session?.accessToken, selectedAssignmentId] })
    },
  })

  const revokeQualificationMutation = useMutation({
    mutationFn: (qualificationIssueId: string) =>
      revokeQualificationIssue(session!.accessToken, qualificationIssueId, {
        reason: lifecycleReason.trim() || null,
      }),
    onSuccess: () => {
      setLifecycleReason('')
      void queryClient.invalidateQueries({ queryKey: ['trainarr-assignment', session?.accessToken, selectedAssignmentId] })
    },
  })

  const expireQualificationMutation = useMutation({
    mutationFn: (qualificationIssueId: string) =>
      expireQualificationIssue(session!.accessToken, qualificationIssueId, {
        reason: lifecycleReason.trim() || null,
      }),
    onSuccess: () => {
      setLifecycleReason('')
      void queryClient.invalidateQueries({ queryKey: ['trainarr-assignment', session?.accessToken, selectedAssignmentId] })
    },
  })

  if (!session || !meQuery.data) {
    return <p className="text-sm text-slate-400">Loading training workspaceΓÇª</p>
  }

  const me = meQuery.data
  const canManage = canManageAssignments(me.tenantRoleKey, me.isPlatformAdmin)
  const canBatchQualification = canRunBatchQualificationChecks(me.tenantRoleKey, me.isPlatformAdmin)
  const canQualifications = canManageQualifications(me.tenantRoleKey, me.isPlatformAdmin)
  const canPrograms = canManagePrograms(me.tenantRoleKey, me.isPlatformAdmin)
  const canImpact = canAssessRulePackImpact(me.tenantRoleKey, me.isPlatformAdmin)
  const canNotifications = canManageNotificationSettings(me.tenantRoleKey, me.isPlatformAdmin)
  const assignments = assignmentsQuery.data ?? []
  const selectedAssignment = assignmentDetailQuery.data
  const canUploadForAssignment =
    selectedAssignment &&
    canUploadEvidence(
      me.tenantRoleKey,
      me.isPlatformAdmin,
      selectedAssignment.staffarrPersonId,
      me.personId,
    )

  const canEvaluate = canSubmitEvaluation(me.tenantRoleKey, me.isPlatformAdmin)
  const canTraineeSign =
    selectedAssignment &&
    canSubmitTraineeSignoff(
      me.tenantRoleKey,
      me.isPlatformAdmin,
      selectedAssignment.staffarrPersonId,
      me.personId,
    )
  const canTrainerSign = canSubmitTrainerSignoff(me.tenantRoleKey, me.isPlatformAdmin)

  const toggleProgramDefinition = (definitionId: string) => {
    setSelectedProgramDefinitionIds((current) =>
      current.includes(definitionId) ? current.filter((id) => id !== definitionId) : [...current, definitionId],
    )
  }

  return (
    <div className="mx-auto max-w-5xl space-y-6">
      <PageHeader
        title="Training qualification workspace"
        subtitle={`${me.displayName} ┬╖ ${me.tenantRoleKey.replace('_', ' ')}`}
      />

      <ProgramBuilderPanel
        programs={programsQuery.data ?? []}
        definitions={definitionsQuery.data ?? []}
        selectedDefinitionIds={selectedProgramDefinitionIds}
        selectedProgramId={selectedProgramId}
        selectedDefinitionIdForCitations={selectedDefinitionIdForCitations}
        onSelectProgram={setSelectedProgramId}
        onSelectDefinitionForCitations={setSelectedDefinitionIdForCitations}
        programKey={programKey}
        programName={programName}
        programDescription={programDescription}
        onProgramKeyChange={setProgramKey}
        onProgramNameChange={setProgramName}
        onProgramDescriptionChange={setProgramDescription}
        onToggleDefinition={toggleProgramDefinition}
        onCreateProgram={() => createProgramMutation.mutate()}
        isCreating={createProgramMutation.isPending}
        canManage={canPrograms}
      />

      {selectedDefinitionIdForCitations ? (
        <CitationAttachmentPanel
          title="Training definition citations"
          citations={definitionCitationsQuery.data ?? []}
          citationIdInput={citationIdInput}
          citationKeyInput={citationKeyInput}
          onCitationIdChange={setCitationIdInput}
          onCitationKeyChange={setCitationKeyInput}
          onAttach={() => attachDefinitionCitationMutation.mutate()}
          onRemove={async (attachmentId) => {
            setRemovingCitationId(attachmentId)
            try {
              await removeTrainingDefinitionCitation(
                session!.accessToken,
                selectedDefinitionIdForCitations,
                attachmentId,
              )
              await queryClient.invalidateQueries({
                queryKey: ['trainarr-definition-citations', session?.accessToken, selectedDefinitionIdForCitations],
              })
            } finally {
              setRemovingCitationId(null)
            }
          }}
          isAttaching={attachDefinitionCitationMutation.isPending}
          isRemovingId={removingCitationId}
          canManage={canPrograms}
          validateWithComplianceCore={validateCitationWithComplianceCore}
          onValidateWithComplianceCoreChange={setValidateCitationWithComplianceCore}
        />
      ) : null}

      {selectedProgramId ? (
        <CitationAttachmentPanel
          title="Training program citations"
          citations={programCitationsQuery.data ?? []}
          citationIdInput={citationIdInput}
          citationKeyInput={citationKeyInput}
          onCitationIdChange={setCitationIdInput}
          onCitationKeyChange={setCitationKeyInput}
          onAttach={() => attachProgramCitationMutation.mutate()}
          onRemove={async (attachmentId) => {
            setRemovingCitationId(attachmentId)
            try {
              await removeTrainingProgramCitation(session!.accessToken, selectedProgramId, attachmentId)
              await queryClient.invalidateQueries({
                queryKey: ['trainarr-program-citations', session?.accessToken, selectedProgramId],
              })
            } finally {
              setRemovingCitationId(null)
            }
          }}
          isAttaching={attachProgramCitationMutation.isPending}
          isRemovingId={removingCitationId}
          canManage={canPrograms}
          validateWithComplianceCore={validateCitationWithComplianceCore}
          onValidateWithComplianceCoreChange={setValidateCitationWithComplianceCore}
        />
      ) : null}

      {selectedDefinitionIdForCitations ? (
        <RulePackRequirementPanel
          title="Training definition rule pack requirements"
          requirements={definitionRulePackRequirementsQuery.data ?? []}
          rulePackKeyInput={rulePackKeyInput}
          onRulePackKeyChange={setRulePackKeyInput}
          onSave={() => upsertDefinitionRulePackMutation.mutate()}
          onRemove={async (requirementId) => {
            setRemovingRulePackRequirementId(requirementId)
            try {
              await removeTrainingDefinitionRulePackRequirement(
                session!.accessToken,
                selectedDefinitionIdForCitations,
                requirementId,
              )
              await queryClient.invalidateQueries({
                queryKey: ['trainarr-definition-rule-packs', session?.accessToken, selectedDefinitionIdForCitations],
              })
            } finally {
              setRemovingRulePackRequirementId(null)
            }
          }}
          isSaving={upsertDefinitionRulePackMutation.isPending}
          isRemovingId={removingRulePackRequirementId}
          canManage={canPrograms}
          validateWithComplianceCore={validateRulePackWithComplianceCore}
          onValidateWithComplianceCoreChange={setValidateRulePackWithComplianceCore}
        />
      ) : null}

      {selectedProgramId ? (
        <RulePackRequirementPanel
          title="Training program rule pack requirements"
          requirements={programRulePackRequirementsQuery.data ?? []}
          rulePackKeyInput={rulePackKeyInput}
          onRulePackKeyChange={setRulePackKeyInput}
          onSave={() => upsertProgramRulePackMutation.mutate()}
          onRemove={async (requirementId) => {
            setRemovingRulePackRequirementId(requirementId)
            try {
              await removeTrainingProgramRulePackRequirement(
                session!.accessToken,
                selectedProgramId,
                requirementId,
              )
              await queryClient.invalidateQueries({
                queryKey: ['trainarr-program-rule-packs', session?.accessToken, selectedProgramId],
              })
            } finally {
              setRemovingRulePackRequirementId(null)
            }
          }}
          isSaving={upsertProgramRulePackMutation.isPending}
          isRemovingId={removingRulePackRequirementId}
          canManage={canPrograms}
          validateWithComplianceCore={validateRulePackWithComplianceCore}
          onValidateWithComplianceCoreChange={setValidateRulePackWithComplianceCore}
        />
      ) : null}

      {canImpact ? (
        <RulePackImpactPanel
          rulePackKeyInput={impactRulePackKeyInput}
          onRulePackKeyChange={(value) => {
            setImpactRulePackKeyInput(value)
            setRulePackImpactAssessment(null)
          }}
          onAssess={() => rulePackImpactMutation.mutate()}
          isAssessing={rulePackImpactMutation.isPending}
          canAssess={canImpact}
          assessment={rulePackImpactAssessment}
        />
      ) : null}

      {canNotifications ? (
        <NotificationSettingsPanel accessToken={session.accessToken} canManage={canNotifications} />
      ) : null}

      {canBatchQualification && (
        <BatchQualificationCheckPanel
          batch={batchQualificationCheck}
          isChecking={batchQualificationCheckMutation.isPending}
          onRunBatch={() => batchQualificationCheckMutation.mutate()}
          canRun={
            Boolean(batchQualificationKey.trim()) &&
            (parsePersonIdsFromText(batchPersonIdsText).length > 0 ||
              selectedBatchRemediationPersonIds.length > 0)
          }
          qualificationKey={batchQualificationKey}
          onQualificationKeyChange={setBatchQualificationKey}
          rulePackKey={rulePackKey}
          onRulePackKeyChange={setRulePackKey}
          personIdsText={batchPersonIdsText}
          onPersonIdsTextChange={setBatchPersonIdsText}
          selectedRemediationPersonIds={selectedBatchRemediationPersonIds}
          onToggleRemediationPerson={(personId) => {
            setSelectedBatchRemediationPersonIds((current) =>
              current.includes(personId) ? current.filter((id) => id !== personId) : [...current, personId],
            )
            setBatchQualificationCheck(null)
          }}
          remediationPersonOptions={(remediationsQuery.data ?? []).map((remediation) => ({
            remediationId: remediation.remediationId,
            staffarrPersonId: remediation.staffarrPersonId,
            label: `${remediation.reasonCategoryKey} ┬╖ ${remediation.remediationId.slice(0, 8)}`,
          }))}
        />
      )}

      <RemediationAssignmentPanel
        remediations={remediationsQuery.data ?? []}
        definitions={definitionsQuery.data ?? []}
        selectedRemediationId={selectedRemediationId}
        selectedDefinitionId={selectedDefinitionId}
        onSelectRemediation={(id) => {
          setSelectedRemediationId(id)
          setQualificationCheck(null)
        }}
        onSelectDefinition={(id) => {
          setSelectedDefinitionId(id)
          setQualificationCheck(null)
        }}
        onCreateAssignment={() => createAssignmentMutation.mutate()}
        isCreating={createAssignmentMutation.isPending}
        canManage={canManage}
        qualificationCheck={qualificationCheck}
        isCheckingQualification={qualificationCheckMutation.isPending}
        onRunQualificationCheck={() => qualificationCheckMutation.mutate()}
        rulePackKey={rulePackKey}
        onRulePackKeyChange={setRulePackKey}
      />

      <div className="grid gap-6 lg:grid-cols-2">
        <AssignmentsPanel
          assignments={assignments}
          selectedAssignmentId={selectedAssignmentId}
          onSelectAssignment={setSelectedAssignmentId}
          canManage={canManage}
          canCompleteForAssignment={(assignment) =>
            canCompleteAssignment(me.tenantRoleKey, me.isPlatformAdmin, assignment.staffarrPersonId, me.personId) &&
            (assignment.assignmentId !== selectedAssignment?.assignmentId ||
              Boolean(selectedAssignment?.completionRequirementsMet))
          }
          onComplete={(assignmentId) => completeAssignmentMutation.mutate(assignmentId)}
          completingAssignmentId={
            completeAssignmentMutation.isPending ? completeAssignmentMutation.variables ?? null : null
          }
        />

        <div className="space-y-6">
          <section className="rounded-xl border border-slate-700 bg-slate-900/60 p-4">
            <h2 className="text-sm font-semibold uppercase tracking-wide text-slate-400">Assignment detail</h2>
            {!selectedAssignment ? (
              <p className="mt-3 text-sm text-slate-400">Select an assignment to view details.</p>
            ) : (
              <dl className="mt-3 space-y-2 text-sm">
                <div>
                  <dt className="text-slate-500">Training</dt>
                  <dd className="text-slate-100">{selectedAssignment.trainingDefinitionName}</dd>
                </div>
                <div>
                  <dt className="text-slate-500">Qualification</dt>
                  <dd className="text-slate-100">{selectedAssignment.qualificationName}</dd>
                </div>
                <div>
                  <dt className="text-slate-500">Status</dt>
                  <dd className="text-slate-100">{selectedAssignment.status}</dd>
                </div>
                <div>
                  <dt className="text-slate-500">Evidence on file</dt>
                  <dd className="text-slate-100">{selectedAssignment.evidenceCount}</dd>
                </div>
                <div>
                  <dt className="text-slate-500">Completion gate</dt>
                  <dd className={selectedAssignment.completionRequirementsMet ? 'text-emerald-300' : 'text-amber-300'}>
                    {selectedAssignment.completionRequirementsMet ? 'Ready to complete' : 'Evaluation + signoffs required'}
                  </dd>
                </div>
                <div>
                  <dt className="text-slate-500">Person</dt>
                  <dd className="font-mono text-xs text-slate-300">{selectedAssignment.staffarrPersonId}</dd>
                </div>
                {selectedAssignment.staffarrIncidentRemediationId && (
                  <div>
                    <dt className="text-slate-500">Remediation</dt>
                    <dd className="font-mono text-xs text-violet-300">
                      {selectedAssignment.staffarrIncidentRemediationId}
                    </dd>
                  </div>
                )}
                {selectedAssignment.blockerPublicationId && (
                  <div>
                    <dt className="text-slate-500">StaffArr blocker publication</dt>
                    <dd className="font-mono text-xs text-slate-300">{selectedAssignment.blockerPublicationId}</dd>
                  </div>
                )}
                {selectedAssignment.completedAt && (
                  <div>
                    <dt className="text-slate-500">Completed</dt>
                    <dd className="text-slate-100">{new Date(selectedAssignment.completedAt).toLocaleString()}</dd>
                  </div>
                )}
                {selectedAssignment.qualificationIssue && (
                  <div
                    className={
                      selectedAssignment.qualificationIssue.status === 'issued'
                        ? 'rounded-lg border border-emerald-800/60 bg-emerald-950/30 p-3'
                        : selectedAssignment.qualificationIssue.status === 'suspended'
                          ? 'rounded-lg border border-amber-800/60 bg-amber-950/30 p-3'
                          : 'rounded-lg border border-red-800/60 bg-red-950/30 p-3'
                    }
                  >
                    <dt className="text-xs font-semibold uppercase tracking-wide text-emerald-400">
                      Qualification {selectedAssignment.qualificationIssue.status.replace('_', ' ')}
                    </dt>
                    <dd className="mt-1 text-sm text-emerald-100">
                      {selectedAssignment.qualificationIssue.qualificationName} ┬╖{' '}
                      {new Date(selectedAssignment.qualificationIssue.issuedAt).toLocaleString()}
                    </dd>
                    {selectedAssignment.qualificationIssue.statusChangedAt ? (
                      <dd className="mt-1 text-xs text-slate-300">
                        Status changed {new Date(selectedAssignment.qualificationIssue.statusChangedAt).toLocaleString()}
                      </dd>
                    ) : null}
                    {selectedAssignment.qualificationIssue.lifecycleReason ? (
                      <dd className="mt-1 text-xs text-slate-400">
                        {selectedAssignment.qualificationIssue.lifecycleReason}
                      </dd>
                    ) : null}
                    <dd className="mt-1 font-mono text-xs text-emerald-300/80">
                      StaffArr grant publication {selectedAssignment.qualificationIssue.grantPublicationId}
                    </dd>
                    {selectedAssignment.qualificationIssue.lifecyclePublicationId ? (
                      <dd className="mt-1 font-mono text-xs text-violet-300/80">
                        Lifecycle publication {selectedAssignment.qualificationIssue.lifecyclePublicationId}
                      </dd>
                    ) : null}
                    {canQualifications &&
                    ['issued', 'suspended'].includes(selectedAssignment.qualificationIssue.status) ? (
                      <div className="mt-3 space-y-2 border-t border-slate-700/60 pt-3">
                        <label className="grid gap-1 text-xs text-slate-400">
                          Lifecycle reason (optional)
                          <textarea
                            value={lifecycleReason}
                            onChange={(event) => setLifecycleReason(event.target.value)}
                            rows={2}
                            className="rounded border border-slate-600 bg-slate-950 px-2 py-1 text-sm text-white"
                          />
                        </label>
                        <div className="flex flex-wrap gap-2">
                          {selectedAssignment.qualificationIssue.status === 'issued' ? (
                            <button
                              type="button"
                              disabled={suspendQualificationMutation.isPending}
                              className="rounded border border-amber-700 px-2 py-1 text-xs text-amber-100 hover:bg-amber-950/40 disabled:opacity-50"
                              onClick={() =>
                                suspendQualificationMutation.mutate(
                                  selectedAssignment.qualificationIssue!.qualificationIssueId,
                                )
                              }
                            >
                              Suspend
                            </button>
                          ) : null}
                          <button
                            type="button"
                            disabled={revokeQualificationMutation.isPending}
                            className="rounded border border-red-700 px-2 py-1 text-xs text-red-100 hover:bg-red-950/40 disabled:opacity-50"
                            onClick={() =>
                              revokeQualificationMutation.mutate(
                                selectedAssignment.qualificationIssue!.qualificationIssueId,
                              )
                            }
                          >
                            Revoke
                          </button>
                          <button
                            type="button"
                            disabled={expireQualificationMutation.isPending}
                            className="rounded border border-slate-600 px-2 py-1 text-xs text-slate-200 hover:bg-slate-800 disabled:opacity-50"
                            onClick={() =>
                              expireQualificationMutation.mutate(
                                selectedAssignment.qualificationIssue!.qualificationIssueId,
                              )
                            }
                          >
                            Expire
                          </button>
                        </div>
                      </div>
                    ) : null}
                  </div>
                )}
              </dl>
            )}
          </section>

          <EvidenceCapturePanel
            assignment={selectedAssignment ?? null}
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

          <SignoffEvaluationPanel
            assignment={selectedAssignment ?? null}
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
        </div>
      </div>
    </div>
  )
}
