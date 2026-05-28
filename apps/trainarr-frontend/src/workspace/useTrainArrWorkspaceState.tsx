import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'
import { Navigate, useSearchParams } from 'react-router-dom'
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
  getTrainingDefinitionRulePackRequirements,
  getTrainingProgramRulePackRequirements,
  upsertTrainingDefinitionRulePackRequirement,
  upsertTrainingProgramRulePackRequirement,
  assessRulePackImpact,
} from '../api/client'
import {
  canManageAssignments,
  canManagePrograms,
  canAssessRulePackImpact,
  canManageNotificationSettings,
  canExportAuditPackage,
  canReadAuditPackage,
  canManageQualifications,
  canRunBatchQualificationChecks,
  canSubmitEvaluation,
  canSubmitTraineeSignoff,
  canSubmitTrainerSignoff,
  canUploadEvidence,
  loadSession,
} from '../auth/sessionStorage'

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

export function useTrainArrWorkspaceState() {

  const [searchParams] = useSearchParams()
  const handoff = searchParams.get('handoff')
  const handoffRedirect = handoff
    ? <Navigate to={`/launch?handoff=${encodeURIComponent(handoff)}`} replace />
    : null

  const session = loadSession()
  const accessToken = session?.accessToken ?? ''
  const [apiError] = useState<string | null>(null)
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

const me = meQuery.data
  const canManage = me ? canManageAssignments(me.tenantRoleKey, me.isPlatformAdmin) : false
  const canBatchQualification = me ? canRunBatchQualificationChecks(me.tenantRoleKey, me.isPlatformAdmin) : false
  const canQualifications = me ? canManageQualifications(me.tenantRoleKey, me.isPlatformAdmin) : false
  const canPrograms = me ? canManagePrograms(me.tenantRoleKey, me.isPlatformAdmin) : false
  const canImpact = me ? canAssessRulePackImpact(me.tenantRoleKey, me.isPlatformAdmin) : false
  const canNotifications = me ? canManageNotificationSettings(me.tenantRoleKey, me.isPlatformAdmin) : false
  const canExportAudit = me ? canExportAuditPackage(me.tenantRoleKey, me.isPlatformAdmin) : false
  const canReadAudit = me ? canReadAuditPackage(me.tenantRoleKey, me.isPlatformAdmin) : false
  const assignments = assignmentsQuery.data ?? []
  const selectedAssignment = assignmentDetailQuery.data
  const canUploadForAssignment =
    me &&
    selectedAssignment &&
    canUploadEvidence(
      me.tenantRoleKey,
      me.isPlatformAdmin,
      selectedAssignment.staffarrPersonId,
      me.personId,
    )

  const canEvaluate = me ? canSubmitEvaluation(me.tenantRoleKey, me.isPlatformAdmin) : false
  const canTraineeSign =
    me &&
    selectedAssignment &&
    canSubmitTraineeSignoff(
      me.tenantRoleKey,
      me.isPlatformAdmin,
      selectedAssignment.staffarrPersonId,
      me.personId,
    )
  const canTrainerSign = me ? canSubmitTrainerSignoff(me.tenantRoleKey, me.isPlatformAdmin) : false

  const toggleProgramDefinition = (definitionId: string) => {
    setSelectedProgramDefinitionIds((current) =>
      current.includes(definitionId) ? current.filter((id) => id !== definitionId) : [...current, definitionId],
    )
  }

  return {
    handoffRedirect,
    ready: Boolean(session && meQuery.data),
    loadingMessage: 'Loading training workspace…',
    me: meQuery.data!,
    session: session!,
    accessToken,
    apiError,
    searchParams,
    selectedAssignmentId,
    selectedRemediationId,
    selectedDefinitionId,
    programKey,
    programName,
    programDescription,
    selectedProgramDefinitionIds,
    selectedProgramId,
    selectedDefinitionIdForCitations,
    citationIdInput,
    citationKeyInput,
    validateCitationWithComplianceCore,
    rulePackKeyInput,
    validateRulePackWithComplianceCore,
    impactRulePackKeyInput,
    rulePackImpactAssessment,
    removingCitationId,
    removingRulePackRequirementId,
    evidenceTypeKey,
    evidenceNotes,
    evidenceFile,
    evaluationResult,
    evaluationScore,
    evaluationNotes,
    signoffNotes,
    lifecycleReason,
    rulePackKey,
    qualificationCheck,
    batchQualificationKey,
    batchPersonIdsText,
    selectedBatchRemediationPersonIds,
    batchQualificationCheck,
    meQuery,
    definitionsQuery,
    programsQuery,
    assignmentsQuery,
    remediationsQuery,
    assignmentDetailQuery,
    definitionCitationsQuery,
    programCitationsQuery,
    definitionRulePackRequirementsQuery,
    programRulePackRequirementsQuery,
    evidenceQuery,
    batchQualificationCheckMutation,
    qualificationCheckMutation,
    createAssignmentMutation,
    attachDefinitionCitationMutation,
    attachProgramCitationMutation,
    upsertDefinitionRulePackMutation,
    upsertProgramRulePackMutation,
    createProgramMutation,
    rulePackImpactMutation,
    uploadEvidenceMutation,
    submitEvaluationMutation,
    submitTraineeSignoffMutation,
    submitTrainerSignoffMutation,
    completeAssignmentMutation,
    suspendQualificationMutation,
    revokeQualificationMutation,
    expireQualificationMutation,
    canManage,
    canBatchQualification,
    canQualifications,
    canPrograms,
    canImpact,
    canNotifications,
    canExportAudit,
    canReadAudit,
    assignments,
    selectedAssignment,
    canUploadForAssignment,
    canEvaluate,
    canTraineeSign,
    canTrainerSign,
    toggleProgramDefinition,
    queryClient,
    setSelectedAssignmentId,
    setSelectedRemediationId,
    setSelectedDefinitionId,
    setProgramKey,
    setProgramName,
    setProgramDescription,
    setSelectedProgramId,
    setSelectedDefinitionIdForCitations,
    setCitationIdInput,
    setCitationKeyInput,
    setValidateCitationWithComplianceCore,
    setRulePackKeyInput,
    setValidateRulePackWithComplianceCore,
    setImpactRulePackKeyInput,
    setRulePackImpactAssessment,
    setRemovingCitationId,
    setRemovingRulePackRequirementId,
    setEvidenceTypeKey,
    setEvidenceNotes,
    setEvidenceFile,
    setEvaluationResult,
    setEvaluationScore,
    setEvaluationNotes,
    setSignoffNotes,
    setLifecycleReason,
    setRulePackKey,
    setQualificationCheck,
    setBatchQualificationKey,
    setBatchPersonIdsText,
    setSelectedBatchRemediationPersonIds,
    setBatchQualificationCheck,
  }
}

export type TrainArrWorkspaceState = ReturnType<typeof useTrainArrWorkspaceState>
