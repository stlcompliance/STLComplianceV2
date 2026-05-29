import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useEffect, useMemo, useState } from 'react'
import { Navigate, useSearchParams } from 'react-router-dom'
import {
  completeTrainingAssignment,
  createQualificationCheck,
  createBatchQualificationCheck,
  listQualificationChecks,
  createTrainingAssignment,
  createTrainingEvidence,
  createTrainingProgram,
  createTrainingDefinitionStep,
  deleteTrainingDefinitionStep,
  getTrainingDefinitionSteps,
  createTrainingMatrixEntry,
  deleteTrainingMatrixEntry,
  getTrainingMatrix,
  getTrainingRequirementBuilderView,
  createTrainingApplicabilityProfile,
  deleteTrainingApplicabilityProfile,
  createTrainingRequirement,
  deleteTrainingRequirement,
  syncTrainingRequirementToMatrix,
  getTrainingProgram,
  getTrainingProgramVersions,
  listQualificationIssues,
  startTrainingProgramRevision,
  updateTrainingProgram,
  expireQualificationIssue,
  revokeQualificationIssue,
  submitTrainingEvaluation,
  submitTrainingSignoff,
  suspendQualificationIssue,
  getTrainingEvaluationHistory,
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
  const [manualAssignmentPersonId, setManualAssignmentPersonId] = useState('')
  const [manualAssignmentDefinitionId, setManualAssignmentDefinitionId] = useState('')
  const [manualQualificationCheck, setManualQualificationCheck] = useState<
    import('../api/types').QualificationCheckResponse | null
  >(null)
  const [operationsCheckPersonId, setOperationsCheckPersonId] = useState('')
  const [operationsCheckDefinitionId, setOperationsCheckDefinitionId] = useState('')
  const [operationsQualificationCheck, setOperationsQualificationCheck] = useState<
    import('../api/types').QualificationCheckResponse | null
  >(null)
  const [batchQualificationKey, setBatchQualificationKey] = useState('hazmat_endorsement')
  const [selectedBatchPersonIds, setSelectedBatchPersonIds] = useState<string[]>([])
  const [selectedBatchRemediationPersonIds, setSelectedBatchRemediationPersonIds] = useState<string[]>([])
  const [batchQualificationCheck, setBatchQualificationCheck] = useState<
    import('../api/types').BatchQualificationCheckResponse | null
  >(null)
  const [matrixApplicabilityKey, setMatrixApplicabilityKey] = useState('')
  const [matrixApplicabilityLabel, setMatrixApplicabilityLabel] = useState('')
  const [matrixTargetType, setMatrixTargetType] = useState<'program' | 'definition'>('program')
  const [matrixTargetId, setMatrixTargetId] = useState('')
  const [matrixRequirementLevel, setMatrixRequirementLevel] = useState('required')
  const [matrixSortOrder, setMatrixSortOrder] = useState('0')
  const [deletingMatrixEntryId, setDeletingMatrixEntryId] = useState<string | null>(null)
  const [profileLabel, setProfileLabel] = useState('')
  const [profileScopeType, setProfileScopeType] = useState('role_template')
  const [profileScopeKey, setProfileScopeKey] = useState('')
  const [profileDescription, setProfileDescription] = useState('')
  const [requirementKey, setRequirementKey] = useState('')
  const [requirementLabel, setRequirementLabel] = useState('')
  const [requirementSource, setRequirementSource] = useState('internal')
  const [requirementSourceKey, setRequirementSourceKey] = useState('')
  const [requirementTargetType, setRequirementTargetType] = useState<'program' | 'definition'>('program')
  const [requirementTargetId, setRequirementTargetId] = useState('')
  const [requirementProfileId, setRequirementProfileId] = useState('')
  const [requirementLevel, setRequirementLevel] = useState('required')
  const [deletingApplicabilityProfileId, setDeletingApplicabilityProfileId] = useState<string | null>(null)
  const [deletingRequirementId, setDeletingRequirementId] = useState<string | null>(null)
  const [syncingRequirementId, setSyncingRequirementId] = useState<string | null>(null)
  const [qualificationStatusFilter, setQualificationStatusFilter] = useState('')
  const [selectedQualificationIssueId, setSelectedQualificationIssueId] = useState<string | null>(null)

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

  const programDetailQuery = useQuery({
    queryKey: ['trainarr-program-detail', session?.accessToken, selectedProgramId],
    queryFn: () => getTrainingProgram(session!.accessToken, selectedProgramId!),
    enabled: Boolean(session?.accessToken && selectedProgramId),
  })

  const programVersionsQuery = useQuery({
    queryKey: ['trainarr-program-versions', session?.accessToken, selectedProgramId],
    queryFn: () => getTrainingProgramVersions(session!.accessToken, selectedProgramId!),
    enabled: Boolean(session?.accessToken && selectedProgramId),
  })

  const trainingMatrixQuery = useQuery({
    queryKey: ['trainarr-training-matrix', session?.accessToken],
    queryFn: () => getTrainingMatrix(session!.accessToken),
    enabled: Boolean(session?.accessToken) && meQuery.isSuccess,
  })

  const requirementBuilderQuery = useQuery({
    queryKey: ['trainarr-requirement-builder', session?.accessToken],
    queryFn: () => getTrainingRequirementBuilderView(session!.accessToken),
    enabled: Boolean(session?.accessToken) && meQuery.isSuccess,
  })

  const qualificationIssuesQuery = useQuery({
    queryKey: ['trainarr-qualification-issues', session?.accessToken, qualificationStatusFilter],
    queryFn: () =>
      listQualificationIssues(
        session!.accessToken,
        qualificationStatusFilter.trim() || undefined,
      ),
    enabled: Boolean(session?.accessToken) && meQuery.isSuccess,
  })

  const qualificationCheckHistoryQuery = useQuery({
    queryKey: [
      'trainarr-qualification-check-history',
      session?.accessToken,
      operationsCheckPersonId,
    ],
    queryFn: () =>
      listQualificationChecks(session!.accessToken, {
        staffarrPersonId: operationsCheckPersonId.trim() || undefined,
        limit: 25,
      }),
    enabled: Boolean(session?.accessToken) && meQuery.isSuccess,
  })

  useEffect(() => {
    const detail = programDetailQuery.data
    if (!detail || !selectedProgramId) {
      return
    }
    setProgramName(detail.name)
    setProgramDescription(detail.description)
    setSelectedProgramDefinitionIds(detail.definitions.map((d) => d.trainingDefinitionId))
  }, [programDetailQuery.data, selectedProgramId])

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

  const evaluationHistoryQuery = useQuery({
    queryKey: ['trainarr-evaluation-history', session?.accessToken, selectedAssignmentId],
    queryFn: () => getTrainingEvaluationHistory(session!.accessToken, selectedAssignmentId!),
    enabled: Boolean(session?.accessToken && selectedAssignmentId),
  })

  const definitionCitationsQuery = useQuery({
    queryKey: ['trainarr-definition-citations', session?.accessToken, selectedDefinitionIdForCitations],
    queryFn: () => getTrainingDefinitionCitations(session!.accessToken, selectedDefinitionIdForCitations!),
    enabled: Boolean(session?.accessToken && selectedDefinitionIdForCitations),
  })

  const definitionStepsQuery = useQuery({
    queryKey: ['trainarr-definition-steps', session?.accessToken, selectedDefinitionIdForCitations],
    queryFn: () => getTrainingDefinitionSteps(session!.accessToken, selectedDefinitionIdForCitations!),
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
      const personIds = [...new Set([...selectedBatchRemediationPersonIds, ...selectedBatchPersonIds])]
      if (personIds.length === 0) {
        throw new Error('Select at least one StaffArr person or remediation subject.')
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

  const manualQualificationCheckMutation = useMutation({
    mutationFn: async () => {
      const definition = definitionsQuery.data?.find(
        (d) => d.trainingDefinitionId === manualAssignmentDefinitionId,
      )
      if (!manualAssignmentPersonId.trim() || !definition) {
        throw new Error('Enter a person id and select a training definition before running a check.')
      }
      return createQualificationCheck(session!.accessToken, {
        staffarrPersonId: manualAssignmentPersonId.trim(),
        qualificationKey: definition.qualificationKey,
        rulePackKey: rulePackKey.trim() || null,
        trainingDefinitionId: manualAssignmentDefinitionId,
        context: null,
      })
    },
    onSuccess: (result) => {
      setManualQualificationCheck(result)
      void queryClient.invalidateQueries({ queryKey: ['trainarr-qualification-check-history'] })
    },
  })

  const operationsQualificationCheckMutation = useMutation({
    mutationFn: async () => {
      const definition = definitionsQuery.data?.find(
        (d) => d.trainingDefinitionId === operationsCheckDefinitionId,
      )
      if (!operationsCheckPersonId.trim() || !definition) {
        throw new Error('Enter a person id and select a training definition before running a check.')
      }
      return createQualificationCheck(session!.accessToken, {
        staffarrPersonId: operationsCheckPersonId.trim(),
        qualificationKey: definition.qualificationKey,
        rulePackKey: rulePackKey.trim() || null,
        trainingDefinitionId: operationsCheckDefinitionId,
        context: null,
      })
    },
    onSuccess: (result) => {
      setOperationsQualificationCheck(result)
      void queryClient.invalidateQueries({ queryKey: ['trainarr-qualification-check-history'] })
    },
  })

  const createAssignmentMutation = useMutation({
    mutationFn: async () => {
      const remediation = remediationsQuery.data?.find((r) => r.remediationId === selectedRemediationId)
      if (!remediation || !selectedDefinitionId || !qualificationCheck) {
        throw new Error('Select a remediation, training definition, and run an authorization check.')
      }
      return createTrainingAssignment(session!.accessToken, {
        staffarrPersonId: remediation.staffarrPersonId,
        trainingDefinitionId: selectedDefinitionId,
        staffarrIncidentRemediationId: remediation.remediationId,
        assignmentReason: 'incident_remediation',
        authorizationQualificationCheckId: qualificationCheck.checkId,
      })
    },
    onSuccess: (created) => {
      setSelectedAssignmentId(created.assignmentId)
      setSelectedRemediationId(null)
      setQualificationCheck(null)
      void queryClient.invalidateQueries({ queryKey: ['trainarr-assignments'] })
      void queryClient.invalidateQueries({ queryKey: ['trainarr-remediations'] })
      void queryClient.invalidateQueries({ queryKey: ['trainarr-qualification-check-history'] })
    },
  })

  const createManualAssignmentMutation = useMutation({
    mutationFn: async () => {
      if (!manualAssignmentPersonId.trim() || !manualAssignmentDefinitionId || !manualQualificationCheck) {
        throw new Error('Enter person, definition, and run an authorization check before creating the assignment.')
      }
      return createTrainingAssignment(session!.accessToken, {
        staffarrPersonId: manualAssignmentPersonId.trim(),
        trainingDefinitionId: manualAssignmentDefinitionId,
        assignmentReason: 'manual',
        authorizationQualificationCheckId: manualQualificationCheck.checkId,
      })
    },
    onSuccess: (created) => {
      setSelectedAssignmentId(created.assignmentId)
      setManualAssignmentPersonId('')
      setManualAssignmentDefinitionId('')
      setManualQualificationCheck(null)
      void queryClient.invalidateQueries({ queryKey: ['trainarr-assignments'] })
      void queryClient.invalidateQueries({ queryKey: ['trainarr-qualification-check-history'] })
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
    onSuccess: (created) => {
      setProgramKey('')
      setProgramName('')
      setProgramDescription('')
      setSelectedProgramDefinitionIds([])
      setSelectedProgramId(created.programId)
      void queryClient.invalidateQueries({ queryKey: ['trainarr-programs'] })
    },
  })

  const createDefinitionStepMutation = useMutation({
    mutationFn: async (payload: {
      stepKey: string
      name: string
      description: string
      stepType: 'content' | 'quiz' | 'practical'
      configJson: string
      sortOrder: number
    }) => {
      if (!selectedDefinitionIdForCitations) {
        throw new Error('Select a training definition first.')
      }
      return createTrainingDefinitionStep(session!.accessToken, selectedDefinitionIdForCitations, payload)
    },
    onSuccess: () => {
      void queryClient.invalidateQueries({
        queryKey: ['trainarr-definition-steps', session?.accessToken, selectedDefinitionIdForCitations],
      })
    },
  })

  const deleteDefinitionStepMutation = useMutation({
    mutationFn: async (stepId: string) => {
      if (!selectedDefinitionIdForCitations) {
        throw new Error('Select a training definition first.')
      }
      await deleteTrainingDefinitionStep(session!.accessToken, selectedDefinitionIdForCitations, stepId)
    },
    onSuccess: () => {
      void queryClient.invalidateQueries({
        queryKey: ['trainarr-definition-steps', session?.accessToken, selectedDefinitionIdForCitations],
      })
    },
  })

  const saveProgramMutation = useMutation({
    mutationFn: async () => {
      if (!selectedProgramId) {
        throw new Error('Select a program to save.')
      }
      return updateTrainingProgram(session!.accessToken, selectedProgramId, {
        name: programName,
        description: programDescription,
        status: 'draft',
        trainingDefinitionIds: selectedProgramDefinitionIds,
      })
    },
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ['trainarr-programs'] })
      void queryClient.invalidateQueries({
        queryKey: ['trainarr-program-detail', session?.accessToken, selectedProgramId],
      })
    },
  })

  const publishProgramMutation = useMutation({
    mutationFn: async () => {
      if (!selectedProgramId) {
        throw new Error('Select a program to publish.')
      }
      return updateTrainingProgram(session!.accessToken, selectedProgramId, {
        name: programName,
        description: programDescription,
        status: 'published',
        trainingDefinitionIds: selectedProgramDefinitionIds,
      })
    },
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ['trainarr-programs'] })
      void queryClient.invalidateQueries({
        queryKey: ['trainarr-program-detail', session?.accessToken, selectedProgramId],
      })
      void queryClient.invalidateQueries({
        queryKey: ['trainarr-program-versions', session?.accessToken, selectedProgramId],
      })
    },
  })

  const startRevisionMutation = useMutation({
    mutationFn: async () => {
      if (!selectedProgramId) {
        throw new Error('Select a program to revise.')
      }
      return startTrainingProgramRevision(session!.accessToken, {
        trainingProgramId: selectedProgramId,
      })
    },
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ['trainarr-programs'] })
      void queryClient.invalidateQueries({
        queryKey: ['trainarr-program-detail', session?.accessToken, selectedProgramId],
      })
    },
  })

  const createMatrixEntryMutation = useMutation({
    mutationFn: async () =>
      createTrainingMatrixEntry(session!.accessToken, {
        applicabilityKey: matrixApplicabilityKey.trim(),
        applicabilityLabel: matrixApplicabilityLabel.trim(),
        trainingProgramId: matrixTargetType === 'program' ? matrixTargetId : null,
        trainingDefinitionId: matrixTargetType === 'definition' ? matrixTargetId : null,
        requirementLevel: matrixRequirementLevel,
        sortOrder: Number.parseInt(matrixSortOrder, 10) || 0,
      }),
    onSuccess: () => {
      setMatrixApplicabilityKey('')
      setMatrixApplicabilityLabel('')
      setMatrixTargetId('')
      void queryClient.invalidateQueries({ queryKey: ['trainarr-training-matrix'] })
    },
  })

  const deleteMatrixEntryMutation = useMutation({
    mutationFn: async (matrixEntryId: string) => {
      setDeletingMatrixEntryId(matrixEntryId)
      await deleteTrainingMatrixEntry(session!.accessToken, matrixEntryId)
    },
    onSettled: () => {
      setDeletingMatrixEntryId(null)
      void queryClient.invalidateQueries({ queryKey: ['trainarr-training-matrix'] })
    },
  })

  const createApplicabilityProfileMutation = useMutation({
    mutationFn: async () =>
      createTrainingApplicabilityProfile(session!.accessToken, {
        label: profileLabel.trim(),
        scopeType: profileScopeType,
        scopeKey: profileScopeKey.trim(),
        description: profileDescription.trim() || null,
        sourceProduct: profileScopeType === 'role_template' || profileScopeType === 'org_unit' ? 'StaffArr' : null,
      }),
    onSuccess: () => {
      setProfileLabel('')
      setProfileScopeKey('')
      setProfileDescription('')
      void queryClient.invalidateQueries({ queryKey: ['trainarr-requirement-builder'] })
    },
  })

  const deleteApplicabilityProfileMutation = useMutation({
    mutationFn: async (profileId: string) => {
      setDeletingApplicabilityProfileId(profileId)
      await deleteTrainingApplicabilityProfile(session!.accessToken, profileId)
    },
    onSettled: () => {
      setDeletingApplicabilityProfileId(null)
      void queryClient.invalidateQueries({ queryKey: ['trainarr-requirement-builder'] })
    },
  })

  const createRequirementMutation = useMutation({
    mutationFn: async () =>
      createTrainingRequirement(session!.accessToken, {
        requirementKey: requirementKey.trim(),
        label: requirementLabel.trim(),
        requirementSource,
        sourceKey: requirementSourceKey.trim() || null,
        trainingProgramId: requirementTargetType === 'program' ? requirementTargetId : null,
        trainingDefinitionId: requirementTargetType === 'definition' ? requirementTargetId : null,
        applicabilityProfileId: requirementProfileId,
        requirementLevel,
        sortOrder: 0,
      }),
    onSuccess: () => {
      setRequirementKey('')
      setRequirementLabel('')
      setRequirementSourceKey('')
      setRequirementTargetId('')
      void queryClient.invalidateQueries({ queryKey: ['trainarr-requirement-builder'] })
    },
  })

  const deleteRequirementMutation = useMutation({
    mutationFn: async (requirementId: string) => {
      setDeletingRequirementId(requirementId)
      await deleteTrainingRequirement(session!.accessToken, requirementId)
    },
    onSettled: () => {
      setDeletingRequirementId(null)
      void queryClient.invalidateQueries({ queryKey: ['trainarr-requirement-builder'] })
    },
  })

  const syncRequirementToMatrixMutation = useMutation({
    mutationFn: async (requirementId: string) => {
      setSyncingRequirementId(requirementId)
      return syncTrainingRequirementToMatrix(session!.accessToken, requirementId)
    },
    onSettled: () => {
      setSyncingRequirementId(null)
      void queryClient.invalidateQueries({ queryKey: ['trainarr-training-matrix'] })
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
      void queryClient.invalidateQueries({ queryKey: ['trainarr-evaluation-history', session?.accessToken, selectedAssignmentId] })
      void queryClient.invalidateQueries({ queryKey: ['trainarr-evaluation-review-timeline', session?.accessToken] })
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
      void queryClient.invalidateQueries({ queryKey: ['trainarr-qualification-issues'] })
      void queryClient.invalidateQueries({ queryKey: ['trainarr-assignments'] })
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
      void queryClient.invalidateQueries({ queryKey: ['trainarr-qualification-issues'] })
      void queryClient.invalidateQueries({ queryKey: ['trainarr-assignments'] })
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
      void queryClient.invalidateQueries({ queryKey: ['trainarr-qualification-issues'] })
      void queryClient.invalidateQueries({ queryKey: ['trainarr-assignments'] })
    },
  })

  const rulePackOptions = useMemo(() => {
    const keys = new Set<string>(['driver_qualification'])
    if (rulePackKey.trim()) {
      keys.add(rulePackKey.trim())
    }
    for (const item of definitionRulePackRequirementsQuery.data ?? []) {
      keys.add(item.rulePackKey)
    }
    for (const item of programRulePackRequirementsQuery.data ?? []) {
      keys.add(item.rulePackKey)
    }
    return [...keys].sort().map((value) => ({ value, label: value }))
  }, [rulePackKey, definitionRulePackRequirementsQuery.data, programRulePackRequirementsQuery.data])

  const personPickerOptions = useMemo(() => {
    const options = new Map<string, { value: string; label: string }>()
    for (const issue of qualificationIssuesQuery.data ?? []) {
      options.set(issue.staffarrPersonId, {
        value: issue.staffarrPersonId,
        label: `${issue.qualificationKey} · ${issue.staffarrPersonId.slice(0, 8)}…`,
      })
    }
    for (const assignment of assignmentsQuery.data ?? []) {
      if (!options.has(assignment.staffarrPersonId)) {
        options.set(assignment.staffarrPersonId, {
          value: assignment.staffarrPersonId,
          label: `Assignment · ${assignment.staffarrPersonId.slice(0, 8)}…`,
        })
      }
    }
    return [...options.values()].sort((left, right) => left.label.localeCompare(right.label))
  }, [qualificationIssuesQuery.data, assignmentsQuery.data])

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
    manualAssignmentPersonId,
    manualAssignmentDefinitionId,
    manualQualificationCheck,
    operationsCheckPersonId,
    operationsCheckDefinitionId,
    operationsQualificationCheck,
    batchQualificationKey,
    selectedBatchPersonIds,
    selectedBatchRemediationPersonIds,
    batchQualificationCheck,
    rulePackOptions,
    personPickerOptions,
    meQuery,
    definitionsQuery,
    programsQuery,
    programDetailQuery,
    programVersionsQuery,
    trainingMatrixQuery,
    requirementBuilderQuery,
    qualificationIssuesQuery,
    qualificationCheckHistoryQuery,
    assignmentsQuery,
    remediationsQuery,
    assignmentDetailQuery,
    evaluationHistoryQuery,
    definitionCitationsQuery,
    definitionStepsQuery,
    programCitationsQuery,
    definitionRulePackRequirementsQuery,
    programRulePackRequirementsQuery,
    evidenceQuery,
    batchQualificationCheckMutation,
    qualificationCheckMutation,
    manualQualificationCheckMutation,
    operationsQualificationCheckMutation,
    createAssignmentMutation,
    createManualAssignmentMutation,
    attachDefinitionCitationMutation,
    attachProgramCitationMutation,
    upsertDefinitionRulePackMutation,
    upsertProgramRulePackMutation,
    createProgramMutation,
    saveProgramMutation,
    publishProgramMutation,
    startRevisionMutation,
    createMatrixEntryMutation,
    deleteMatrixEntryMutation,
    createApplicabilityProfileMutation,
    deleteApplicabilityProfileMutation,
    createRequirementMutation,
    deleteRequirementMutation,
    syncRequirementToMatrixMutation,
    createDefinitionStepMutation,
    deleteDefinitionStepMutation,
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
    setManualAssignmentPersonId,
    setManualAssignmentDefinitionId,
    setManualQualificationCheck,
    setOperationsCheckPersonId,
    setOperationsCheckDefinitionId,
    setOperationsQualificationCheck,
    setBatchQualificationKey,
    setSelectedBatchPersonIds,
    setSelectedBatchRemediationPersonIds,
    setBatchQualificationCheck,
    matrixApplicabilityKey,
    matrixApplicabilityLabel,
    matrixTargetType,
    matrixTargetId,
    matrixRequirementLevel,
    matrixSortOrder,
    deletingMatrixEntryId,
    qualificationStatusFilter,
    selectedQualificationIssueId,
    setMatrixApplicabilityKey,
    setMatrixApplicabilityLabel,
    setMatrixTargetType,
    setMatrixTargetId,
    setMatrixRequirementLevel,
    setMatrixSortOrder,
    profileLabel,
    profileScopeType,
    profileScopeKey,
    profileDescription,
    requirementKey,
    requirementLabel,
    requirementSource,
    requirementSourceKey,
    requirementTargetType,
    requirementTargetId,
    requirementProfileId,
    requirementLevel,
    deletingApplicabilityProfileId,
    deletingRequirementId,
    syncingRequirementId,
    setProfileLabel,
    setProfileScopeType,
    setProfileScopeKey,
    setProfileDescription,
    setRequirementKey,
    setRequirementLabel,
    setRequirementSource,
    setRequirementSourceKey,
    setRequirementTargetType,
    setRequirementTargetId,
    setRequirementProfileId,
    setRequirementLevel,
    setQualificationStatusFilter,
    setSelectedQualificationIssueId,
  }
}

export type TrainArrWorkspaceState = ReturnType<typeof useTrainArrWorkspaceState>
