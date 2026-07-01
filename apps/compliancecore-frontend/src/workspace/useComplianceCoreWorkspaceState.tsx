import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'

import { useEffect, useState } from 'react'

import { Navigate, useLocation, useSearchParams } from 'react-router-dom'

import {

  checkWorkflowGate,
  checkWorkflowGateBatch,
  evaluateRulePack,
  evaluateRulePackBatch,
  getFindings,

  getCitations,

  getComplianceKeys,

  getFactDefinitions,

  getFactRequirements,

  getFactSources,

  createFactSource,

  getGoverningBodies,

  getJurisdictions,

  getMaterialKeys,

  getMe,

  getRegulatoryMappings,

  getRegulatoryPrograms,

  getRuleEvaluations,

  getRulePackContent,

  getRulePacks,
  getWorkflowGates,

  getVocabularyTerms,

  getVocabularyTypes,

  updateRulePackStatus,

  updateRulePackContent,

  updateFactSource,

} from '../api/client'

import type {
  CreateFactSourceRequest,
  EvaluateRulePackBatchResponse,
  RuleEvaluationRunResponse,
  RulePackContentBody,
  UpdateFactSourceRequest,
  WorkflowGateBatchCheckResponse,
  WorkflowGateCheckResponse,
} from '../api/types'

import { loadSession } from '../auth/sessionStorage'

export function useComplianceCoreWorkspaceState() {


  const [searchParams] = useSearchParams()
  const location = useLocation()

  const handoff = searchParams.get('handoff')
  const handoffRedirect = handoff
    ? <Navigate to={`/launch?handoff=${encodeURIComponent(handoff)}`} replace />
    : null

  const session = loadSession()
  const accessToken = session?.accessToken ?? ''
  const [apiError] = useState<string | null>(null)
  const shouldLoadRulePackDetailQueries =
    location.pathname.startsWith('/registry')
    || location.pathname.startsWith('/findings')
    || location.pathname.startsWith('/evaluation')
    || location.pathname.startsWith('/rulepacks')
    || location.pathname.startsWith('/requirements')

  const queryClient = useQueryClient()

  const [selectedTypeKey, setSelectedTypeKey] = useState('material_hazard')

  const [selectedRulePackId, setSelectedRulePackId] = useState('')

  const [lastEvaluation, setLastEvaluation] = useState<RuleEvaluationRunResponse | null>(null)
  const [lastBatchEvaluation, setLastBatchEvaluation] = useState<EvaluateRulePackBatchResponse | null>(null)
  const [lastGateCheck, setLastGateCheck] = useState<WorkflowGateCheckResponse | null>(null)
  const [lastGateBatch, setLastGateBatch] = useState<WorkflowGateBatchCheckResponse | null>(null)



  const meQuery = useQuery({

    queryKey: ['compliancecore-me', session?.accessToken],

    queryFn: () => getMe(session!.accessToken),

    enabled: Boolean(session?.accessToken),

    retry: false,

  })



  const typesQuery = useQuery({

    queryKey: ['compliancecore-vocabulary-types', session?.accessToken],

    queryFn: () => getVocabularyTypes(session!.accessToken),

    enabled: Boolean(session?.accessToken) && meQuery.isSuccess,

  })



  const termsQuery = useQuery({

    queryKey: ['compliancecore-vocabulary-terms', session?.accessToken, selectedTypeKey],

    queryFn: () => getVocabularyTerms(session!.accessToken, selectedTypeKey || undefined),

    enabled: Boolean(session?.accessToken) && meQuery.isSuccess,

  })



  const complianceKeysQuery = useQuery({

    queryKey: ['compliancecore-compliance-keys', session?.accessToken],

    queryFn: () => getComplianceKeys(session!.accessToken),

    enabled: Boolean(session?.accessToken) && meQuery.isSuccess,

  })



  const materialKeysQuery = useQuery({

    queryKey: ['compliancecore-material-keys', session?.accessToken],

    queryFn: () => getMaterialKeys(session!.accessToken),

    enabled: Boolean(session?.accessToken) && meQuery.isSuccess,

  })



  const governingBodiesQuery = useQuery({

    queryKey: ['compliancecore-governing-bodies', session?.accessToken],

    queryFn: () => getGoverningBodies(session!.accessToken),

    enabled: Boolean(session?.accessToken) && meQuery.isSuccess,

  })



  const jurisdictionsQuery = useQuery({

    queryKey: ['compliancecore-jurisdictions', session?.accessToken],

    queryFn: () => getJurisdictions(session!.accessToken),

    enabled: Boolean(session?.accessToken) && meQuery.isSuccess,

  })



  const programsQuery = useQuery({

    queryKey: ['compliancecore-regulatory-programs', session?.accessToken],

    queryFn: () => getRegulatoryPrograms(session!.accessToken),

    enabled: Boolean(session?.accessToken) && meQuery.isSuccess,

  })



  const rulePacksQuery = useQuery({

    queryKey: ['compliancecore-rule-packs', session?.accessToken],

    queryFn: () => getRulePacks(session!.accessToken),

    enabled: Boolean(session?.accessToken) && meQuery.isSuccess,

  })



  const citationsQuery = useQuery({

    queryKey: ['compliancecore-citations', session?.accessToken],

    queryFn: () => getCitations(session!.accessToken),

    enabled: Boolean(session?.accessToken) && meQuery.isSuccess,

  })



  const factDefinitionsQuery = useQuery({

    queryKey: ['compliancecore-fact-definitions', session?.accessToken],

    queryFn: () => getFactDefinitions(session!.accessToken),

    enabled: Boolean(session?.accessToken) && meQuery.isSuccess,

  })



  const factRequirementsQuery = useQuery({

    queryKey: ['compliancecore-fact-requirements', session?.accessToken],

    queryFn: () => getFactRequirements(session!.accessToken),

    enabled: Boolean(session?.accessToken) && meQuery.isSuccess,

  })



  const factSourcesQuery = useQuery({

    queryKey: ['compliancecore-fact-sources', session?.accessToken],

    queryFn: () => getFactSources(session!.accessToken),

    enabled: Boolean(session?.accessToken) && meQuery.isSuccess,

  })


  const createFactSourceMutation = useMutation({

    mutationFn: async (payload: CreateFactSourceRequest) =>
      createFactSource(session!.accessToken, payload),

    onSuccess: async () => {

      await queryClient.invalidateQueries({ queryKey: ['compliancecore-fact-sources'] })

    },

  })



  const updateFactSourceMutation = useMutation({

    mutationFn: async ({
      factSourceId,
      payload,
    }: {
      factSourceId: string
      payload: UpdateFactSourceRequest
    }) => updateFactSource(session!.accessToken, factSourceId, payload),

    onSuccess: async () => {

      await queryClient.invalidateQueries({ queryKey: ['compliancecore-fact-sources'] })

    },

  })



  const regulatoryMappingsQuery = useQuery({

    queryKey: ['compliancecore-regulatory-mappings', session?.accessToken],

    queryFn: () => getRegulatoryMappings(session!.accessToken),

    enabled: Boolean(session?.accessToken) && meQuery.isSuccess,

  })



  const rulePackContentQuery = useQuery({

    queryKey: ['compliancecore-rule-pack-content', session?.accessToken, selectedRulePackId],

    queryFn: () => getRulePackContent(session!.accessToken, selectedRulePackId),

    enabled:
      Boolean(session?.accessToken)
      && meQuery.isSuccess
      && Boolean(selectedRulePackId)
      && shouldLoadRulePackDetailQueries,

  })



  const ruleEvaluationsQuery = useQuery({

    queryKey: ['compliancecore-rule-evaluations', session?.accessToken, selectedRulePackId],

    queryFn: () => getRuleEvaluations(session!.accessToken, selectedRulePackId || undefined),

    enabled:
      Boolean(session?.accessToken)
      && meQuery.isSuccess
      && Boolean(selectedRulePackId)
      && shouldLoadRulePackDetailQueries,

  })

  const allRuleEvaluationsQuery = useQuery({

    queryKey: ['compliancecore-rule-evaluations', session?.accessToken, 'all'],

    queryFn: () => getRuleEvaluations(session!.accessToken),

    enabled: Boolean(session?.accessToken) && meQuery.isSuccess,

  })



  const findingsQuery = useQuery({

    queryKey: ['compliancecore-findings', session?.accessToken, selectedRulePackId],

    queryFn: () => getFindings(session!.accessToken, selectedRulePackId || undefined),

    enabled: Boolean(session?.accessToken) && meQuery.isSuccess && shouldLoadRulePackDetailQueries,

  })



  const workflowGatesQuery = useQuery({

    queryKey: ['compliancecore-workflow-gates', session?.accessToken],

    queryFn: () => getWorkflowGates(session!.accessToken),

    enabled: Boolean(session?.accessToken) && meQuery.isSuccess,

  })



  const advanceRulePackMutation = useMutation({

    mutationFn: async ({ rulePackId, status }: { rulePackId: string; status: string }) => {

      await updateRulePackStatus(session!.accessToken, rulePackId, { status })

    },

    onSuccess: async () => {

      await queryClient.invalidateQueries({ queryKey: ['compliancecore-rule-packs'] })

    },

  })



  const saveRuleContentMutation = useMutation({

    mutationFn: async (content: RulePackContentBody) => {

      if (!selectedRulePackId) {

        return

      }

      await updateRulePackContent(session!.accessToken, selectedRulePackId, { content })

    },

    onSuccess: async () => {

      await queryClient.invalidateQueries({ queryKey: ['compliancecore-rule-pack-content'] })

    },

  })



  const evaluateRulePackMutation = useMutation({

    mutationFn: async (facts: Record<string, boolean>) => {

      if (!selectedRulePackId) {

        return null

      }

      return evaluateRulePack(session!.accessToken, selectedRulePackId, { facts })

    },

    onSuccess: async (result) => {

      if (result) {

        setLastEvaluation(result)

      }

      await queryClient.invalidateQueries({ queryKey: ['compliancecore-rule-evaluations'] })

    },

  })

  const evaluateRulePackBatchMutation = useMutation({
    mutationFn: async ({
      rulePackKeys,
      facts,
      emitFindings,
    }: {
      rulePackKeys: string[]
      facts: Record<string, boolean>
      emitFindings: boolean
    }) =>
      evaluateRulePackBatch(session!.accessToken, {
        items: rulePackKeys.map((rulePackKey) => ({ rulePackKey })),
        facts,
        emitFindings,
      }),
    onSuccess: async (result) => {
      setLastBatchEvaluation(result)
      await queryClient.invalidateQueries({ queryKey: ['compliancecore-rule-evaluations'] })
      await queryClient.invalidateQueries({ queryKey: ['compliancecore-findings'] })
    },
  })



  const checkWorkflowGateMutation = useMutation({

    mutationFn: async ({

      gateKey,

      facts,

      emitFindings,

    }: {

      gateKey: string

      facts: Record<string, boolean>

      emitFindings: boolean

    }) => checkWorkflowGate(session!.accessToken, { gateKey, facts, emitFindings }),

    onSuccess: async (result) => {

      setLastGateCheck(result)

      await queryClient.invalidateQueries({ queryKey: ['compliancecore-findings'] })

    },

  })

  const checkWorkflowGateBatchMutation = useMutation({

    mutationFn: async ({

      gateKeys,

      facts,

      emitFindings,

    }: {

      gateKeys: string[]

      facts: Record<string, boolean>

      emitFindings: boolean

    }) =>
      checkWorkflowGateBatch(session!.accessToken, {
        items: gateKeys.map((gateKey) => ({ gateKey })),
        facts,
        emitFindings,
      }),

    onSuccess: async (result) => {

      setLastGateBatch(result)

      await queryClient.invalidateQueries({ queryKey: ['compliancecore-findings'] })

    },

  })



  useEffect(() => {

    if (shouldLoadRulePackDetailQueries && rulePacksQuery.data?.length && !selectedRulePackId) {

      setSelectedRulePackId(rulePacksQuery.data[0].rulePackId)

    }

  }, [rulePacksQuery.data, selectedRulePackId, shouldLoadRulePackDetailQueries])

  const ready = Boolean(session && meQuery.data)
  const me = meQuery.data!
  const canManage = meQuery.data ? me.canManageVocabulary : false
  const canExportAudit = meQuery.data ? me.canExportAuditPackage : false
  const canReadOrchestration = canExportAudit
  const canEvaluateRisk = meQuery.data ? me.canEvaluateRiskScores : false
  const canEvaluateMissingEvidence = meQuery.data ? me.canEvaluateMissingEvidenceWarnings : false
  const canEvaluateControlEffectivenessFlag = meQuery.data
    ? me.canEvaluateControlEffectiveness
    : false
  const canEvaluateReadinessForecastFlag = meQuery.data ? me.canEvaluateReadinessForecast : false
  const loadingMessage = 'Loading compliance registry…'

  return {
    handoffRedirect,
    ready,
    loadingMessage,
    me,
    session: session!,
    accessToken,
    apiError,
    searchParams,
    selectedTypeKey,
    setSelectedTypeKey,
    selectedRulePackId,
    setSelectedRulePackId,
    lastEvaluation,
    lastBatchEvaluation,
    lastGateCheck,
    lastGateBatch,
    meQuery,
    typesQuery,
    termsQuery,
    complianceKeysQuery,
    materialKeysQuery,
    governingBodiesQuery,
    jurisdictionsQuery,
    programsQuery,
    rulePacksQuery,
    citationsQuery,
    factDefinitionsQuery,
    factRequirementsQuery,
    factSourcesQuery,
    createFactSourceMutation,
    updateFactSourceMutation,
    regulatoryMappingsQuery,
    rulePackContentQuery,
    ruleEvaluationsQuery,
    allRuleEvaluationsQuery,
    findingsQuery,
    workflowGatesQuery,
    advanceRulePackMutation,
    saveRuleContentMutation,
    evaluateRulePackMutation,
    evaluateRulePackBatchMutation,
    checkWorkflowGateMutation,
    checkWorkflowGateBatchMutation,
    canManage,
    canExportAudit,
    canReadOrchestration,
    canEvaluateRisk,
    canEvaluateMissingEvidence,
    canEvaluateControlEffectiveness: canEvaluateControlEffectivenessFlag,
    canEvaluateReadinessForecast: canEvaluateReadinessForecastFlag,
  }
}

export type ComplianceCoreWorkspaceState = ReturnType<typeof useComplianceCoreWorkspaceState>
