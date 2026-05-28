import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'

import { useEffect, useState } from 'react'

import { Navigate, useSearchParams } from 'react-router-dom'

import {

  createCitation,

  createComplianceKey,

  createFactDefinition,

  createFactRequirement,

  createFactSource,

  createGoverningBody,

  createJurisdiction,

  createMaterialKey,

  createRegulatoryMapping,

  createRegulatoryProgram,

  createRulePack,

  createVocabularyTerm,

  checkWorkflowGate,
  checkWorkflowGateBatch,
  createWorkflowGate,
  evaluateRulePack,
  evaluateRulePackBatch,
  getFindings,

  getCitations,

  getComplianceKeys,

  getFactDefinitions,

  getFactRequirements,

  getFactSources,

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

} from '../api/client'

import type {
  EvaluateRulePackBatchResponse,
  RuleEvaluationRunResponse,
  RulePackContentBody,
  WorkflowGateBatchCheckResponse,
  WorkflowGateCheckResponse,
} from '../api/types'

import { canExportAuditPackage, canManageVocabulary, loadSession } from '../auth/sessionStorage'

import { CitationFactCatalogPanel } from '../components/CitationFactCatalogPanel'

import { FactSourcesPanel } from '../components/FactSourcesPanel'

import { RegulatoryMappingsPanel } from '../components/RegulatoryMappingsPanel'

import { RegulatoryRegistryPanel } from '../components/RegulatoryRegistryPanel'

import { AuditPackageExportPanel } from '../components/AuditPackageExportPanel'
import { CsvImportExportPanel } from '../components/CsvImportExportPanel'
import { FindingsWorkflowGatesPanel } from '../components/FindingsWorkflowGatesPanel'
import { OperatorDashboardPanel } from '../components/OperatorDashboardPanel'
import { RuleEvaluationPanel } from '../components/RuleEvaluationPanel'

import { VocabularyPanel } from '../components/VocabularyPanel'

export function useComplianceCoreWorkspaceState() {


  const [searchParams] = useSearchParams()

  const handoff = searchParams.get('handoff')
  const handoffRedirect = handoff
    ? <Navigate to={`/launch?handoff=${encodeURIComponent(handoff)}`} replace />
    : null

  const session = loadSession()
  const accessToken = session?.accessToken ?? ''

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



  const regulatoryMappingsQuery = useQuery({

    queryKey: ['compliancecore-regulatory-mappings', session?.accessToken],

    queryFn: () => getRegulatoryMappings(session!.accessToken),

    enabled: Boolean(session?.accessToken) && meQuery.isSuccess,

  })



  const rulePackContentQuery = useQuery({

    queryKey: ['compliancecore-rule-pack-content', session?.accessToken, selectedRulePackId],

    queryFn: () => getRulePackContent(session!.accessToken, selectedRulePackId),

    enabled: Boolean(session?.accessToken) && meQuery.isSuccess && Boolean(selectedRulePackId),

  })



  const ruleEvaluationsQuery = useQuery({

    queryKey: ['compliancecore-rule-evaluations', session?.accessToken, selectedRulePackId],

    queryFn: () => getRuleEvaluations(session!.accessToken, selectedRulePackId || undefined),

    enabled: Boolean(session?.accessToken) && meQuery.isSuccess && Boolean(selectedRulePackId),

  })



  const findingsQuery = useQuery({

    queryKey: ['compliancecore-findings', session?.accessToken, selectedRulePackId],

    queryFn: () => getFindings(session!.accessToken, selectedRulePackId || undefined),

    enabled: Boolean(session?.accessToken) && meQuery.isSuccess,

  })



  const workflowGatesQuery = useQuery({

    queryKey: ['compliancecore-workflow-gates', session?.accessToken],

    queryFn: () => getWorkflowGates(session!.accessToken),

    enabled: Boolean(session?.accessToken) && meQuery.isSuccess,

  })



  const seedMutation = useMutation({

    mutationFn: async () => {

      await createVocabularyTerm(session!.accessToken, {

        termKey: `sample_${Date.now()}`,

        label: 'Sample Hazard Term',

        vocabularyTypeKey: selectedTypeKey || 'material_hazard',

        description: 'Sample controlled vocabulary term created from the admin UI.',

      })



      if ((complianceKeysQuery.data ?? []).length === 0) {

        await createComplianceKey(session!.accessToken, {

          key: 'vehicle_inspection',

          label: 'Vehicle Inspection',

          category: 'compliance_domain',

          description: 'Inspection requirement domain for fleet compliance.',

        })

      }



      if ((materialKeysQuery.data ?? []).length === 0) {

        await createMaterialKey(session!.accessToken, {

          key: 'flammable',

          label: 'Flammable',

          category: 'material_hazard',

          description: 'Can ignite under defined conditions.',

        })

      }

    },

    onSuccess: async () => {

      await queryClient.invalidateQueries({ queryKey: ['compliancecore-vocabulary-terms'] })

      await queryClient.invalidateQueries({ queryKey: ['compliancecore-compliance-keys'] })

      await queryClient.invalidateQueries({ queryKey: ['compliancecore-material-keys'] })

    },

  })



  const seedRegistryMutation = useMutation({

    mutationFn: async () => {

      let bodyId = governingBodiesQuery.data?.[0]?.governingBodyId

      if (!bodyId) {

        const body = await createGoverningBody(session!.accessToken, {

          bodyKey: 'dot',

          label: 'U.S. Department of Transportation',

          description: 'Federal transportation safety and compliance authority.',

        })

        bodyId = body.governingBodyId

      }



      let jurisdictionId = jurisdictionsQuery.data?.[0]?.jurisdictionId

      if (!jurisdictionId) {

        const jurisdiction = await createJurisdiction(session!.accessToken, {

          governingBodyId: bodyId,

          jurisdictionKey: 'us_federal',

          label: 'United States Federal',

          description: 'Federal jurisdiction for interstate transportation rules.',

        })

        jurisdictionId = jurisdiction.jurisdictionId

      }



      let programId = programsQuery.data?.[0]?.regulatoryProgramId

      if (!programId) {

        const program = await createRegulatoryProgram(session!.accessToken, {

          jurisdictionId,

          programKey: 'fmcsa_safety',

          label: 'FMCSA Safety Compliance',

          description: 'Federal motor carrier safety compliance program.',

        })

        programId = program.regulatoryProgramId

      }



      if ((rulePacksQuery.data ?? []).length === 0) {

        await createRulePack(session!.accessToken, {

          regulatoryProgramId: programId,

          packKey: 'driver_qualification',

          label: 'Driver Qualification Rules',

          description: 'Baseline driver qualification rule pack for fleet operators.',

        })

      }

    },

    onSuccess: async () => {

      await queryClient.invalidateQueries({ queryKey: ['compliancecore-governing-bodies'] })

      await queryClient.invalidateQueries({ queryKey: ['compliancecore-jurisdictions'] })

      await queryClient.invalidateQueries({ queryKey: ['compliancecore-regulatory-programs'] })

      await queryClient.invalidateQueries({ queryKey: ['compliancecore-rule-packs'] })

    },

  })



  const advanceRulePackMutation = useMutation({

    mutationFn: async ({ rulePackId, status }: { rulePackId: string; status: string }) => {

      await updateRulePackStatus(session!.accessToken, rulePackId, { status })

    },

    onSuccess: async () => {

      await queryClient.invalidateQueries({ queryKey: ['compliancecore-rule-packs'] })

    },

  })



  const seedCatalogMutation = useMutation({

    mutationFn: async () => {

      let programId = programsQuery.data?.[0]?.regulatoryProgramId

      let rulePackId = rulePacksQuery.data?.[0]?.rulePackId

      if (!programId || !rulePackId) {

        await seedRegistryMutation.mutateAsync()

        const programs = await getRegulatoryPrograms(session!.accessToken)

        const packs = await getRulePacks(session!.accessToken)

        programId = programs[0]?.regulatoryProgramId

        rulePackId = packs[0]?.rulePackId

      }

      if (!programId || !rulePackId) {

        return

      }

      let citationId = citationsQuery.data?.[0]?.citationId

      if (!citationId) {

        const citation = await createCitation(session!.accessToken, {

          regulatoryProgramId: programId,

          rulePackId,

          citationKey: 'cfr_391_11',

          label: 'General qualifications of drivers',

          sourceReference: '49 CFR 391.11',

          description: 'General driver qualification requirements under FMCSA.',

        })

        citationId = citation.citationId

      }

      let factDefinitionId = factDefinitionsQuery.data?.[0]?.factDefinitionId

      if (!factDefinitionId) {

        const fact = await createFactDefinition(session!.accessToken, {

          factKey: 'driver_license_valid',

          label: 'Valid driver license',

          description: 'Driver holds a valid commercial driver license.',

          valueType: 'boolean',

        })

        factDefinitionId = fact.factDefinitionId

      }

      if ((factRequirementsQuery.data ?? []).length === 0) {

        await createFactRequirement(session!.accessToken, {

          factDefinitionId,

          rulePackId,

          requirementKey: 'dq_license_check',

          label: 'License validity check',

          description: 'Driver license must be valid for driver qualification.',

          isRequired: true,

        })

      }

    },

    onSuccess: async () => {

      await queryClient.invalidateQueries({ queryKey: ['compliancecore-citations'] })

      await queryClient.invalidateQueries({ queryKey: ['compliancecore-fact-definitions'] })

      await queryClient.invalidateQueries({ queryKey: ['compliancecore-fact-requirements'] })

      await queryClient.invalidateQueries({ queryKey: ['compliancecore-fact-sources'] })

      await queryClient.invalidateQueries({ queryKey: ['compliancecore-governing-bodies'] })

      await queryClient.invalidateQueries({ queryKey: ['compliancecore-jurisdictions'] })

      await queryClient.invalidateQueries({ queryKey: ['compliancecore-regulatory-programs'] })

      await queryClient.invalidateQueries({ queryKey: ['compliancecore-rule-packs'] })

    },

  })



  const seedSourcesMutation = useMutation({

    mutationFn: async () => {

      if ((factDefinitionsQuery.data ?? []).length === 0) {

        await seedCatalogMutation.mutateAsync()

      }

      const facts = await getFactDefinitions(session!.accessToken)

      const licenseFact = facts.find((fact) => fact.factKey === 'driver_license_valid') ?? facts[0]

      if (!licenseFact) {

        return

      }

      const existing = await getFactSources(session!.accessToken)

      if (!existing.some((source) => source.factKey === licenseFact.factKey)) {

        await createFactSource(session!.accessToken, {

          factDefinitionId: licenseFact.factDefinitionId,

          sourceKey: 'default_license_flag',

          sourceType: 'static_config',

          label: 'Default license valid',

          description: 'Static default used for rule evaluation and internal resolve.',

          configJson: '{"booleanValue":true}',

          priority: 0,

        })

      }

      const medicalFact = facts.find((fact) => fact.factKey === 'medical_cert_on_file')

      if (medicalFact && !existing.some((source) => source.factKey === medicalFact.factKey)) {

        await createFactSource(session!.accessToken, {

          factDefinitionId: medicalFact.factDefinitionId,

          sourceKey: 'staffarr_med_cert',

          sourceType: 'product_api',

          label: 'StaffArr medical certificate',

          description: 'Resolved from StaffArr caller context until product fetch is implemented.',

          productKey: 'staffarr',

          productReference: '/api/people/{personId}/certifications',

          configJson: '{}',

          priority: 0,

        })

      }

    },

    onSuccess: async () => {

      await queryClient.invalidateQueries({ queryKey: ['compliancecore-fact-sources'] })

    },

  })



  const seedMappingsMutation = useMutation({

    mutationFn: async () => {

      let programId = programsQuery.data?.[0]?.regulatoryProgramId

      let rulePackId = rulePacksQuery.data?.[0]?.rulePackId

      if (!programId || !rulePackId) {

        await seedRegistryMutation.mutateAsync()

        const programs = await getRegulatoryPrograms(session!.accessToken)

        const packs = await getRulePacks(session!.accessToken)

        programId = programs[0]?.regulatoryProgramId

        rulePackId = packs[0]?.rulePackId

      }

      if (!programId || !rulePackId) {

        return

      }



      let complianceKeyId = complianceKeysQuery.data?.find((k) => k.key === 'vehicle_inspection')?.complianceKeyId

      if (!complianceKeyId) {

        const key = await createComplianceKey(session!.accessToken, {

          key: 'vehicle_inspection',

          label: 'Vehicle Inspection',

          category: 'compliance_domain',

          description: 'Inspection requirement domain for fleet compliance.',

        })

        complianceKeyId = key.complianceKeyId

      }



      if ((regulatoryMappingsQuery.data ?? []).length === 0) {

        await createRegulatoryMapping(session!.accessToken, {

          mappingKey: 'dq_vehicle_inspection',

          label: 'Vehicle inspection under driver qualification',

          description: 'Maps vehicle inspection compliance key to FMCSA driver qualification rule pack.',

          targetKind: 'compliance_key',

          regulatoryProgramId: programId,

          rulePackId,

          complianceKeyId,

        })

      }

    },

    onSuccess: async () => {

      await queryClient.invalidateQueries({ queryKey: ['compliancecore-regulatory-mappings'] })

      await queryClient.invalidateQueries({ queryKey: ['compliancecore-compliance-keys'] })

      await queryClient.invalidateQueries({ queryKey: ['compliancecore-governing-bodies'] })

      await queryClient.invalidateQueries({ queryKey: ['compliancecore-jurisdictions'] })

      await queryClient.invalidateQueries({ queryKey: ['compliancecore-regulatory-programs'] })

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



  const seedRuleContentMutation = useMutation({

    mutationFn: async () => {

      let rulePackId = selectedRulePackId || rulePacksQuery.data?.[0]?.rulePackId

      if (!rulePackId) {

        await seedRegistryMutation.mutateAsync()

        const packs = await getRulePacks(session!.accessToken)

        rulePackId = packs[0]?.rulePackId ?? ''

      }

      if (!rulePackId) {

        return

      }

      setSelectedRulePackId(rulePackId)

      await updateRulePackContent(session!.accessToken, rulePackId, {

        content: {

          schemaVersion: 1,

          logic: 'all',

          rules: [

            {

              ruleKey: 'license_valid',

              label: 'Valid driver license',

              type: 'fact_boolean',

              factKey: 'driver_license_valid',

              expectedValue: true,

            },

            {

              ruleKey: 'med_cert',

              label: 'Medical certificate on file',

              type: 'fact_boolean',

              factKey: 'medical_cert_on_file',

              expectedValue: true,

            },

          ],

        },

      })

    },

    onSuccess: async () => {

      await queryClient.invalidateQueries({ queryKey: ['compliancecore-rule-pack-content'] })

      await queryClient.invalidateQueries({ queryKey: ['compliancecore-rule-packs'] })

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



  const seedWorkflowGateMutation = useMutation({

    mutationFn: async () => {

      let rulePackId = selectedRulePackId || rulePacksQuery.data?.[0]?.rulePackId

      if (!rulePackId) {

        await seedRegistryMutation.mutateAsync()

        const packs = await getRulePacks(session!.accessToken)

        rulePackId = packs[0]?.rulePackId ?? ''

      }

      if (!rulePackId) {

        return

      }

      setSelectedRulePackId(rulePackId)

      const gates = await getWorkflowGates(session!.accessToken)

      if (!gates.some((gate) => gate.gateKey === 'driver_assignment')) {

        await createWorkflowGate(session!.accessToken, {

          gateKey: 'driver_assignment',

          label: 'Driver assignment gate',

          description: 'Blocks assignment when driver qualification rules fail.',

          rulePackId,

        })

      }

    },

    onSuccess: async () => {

      await queryClient.invalidateQueries({ queryKey: ['compliancecore-workflow-gates'] })

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

    if (rulePacksQuery.data?.length && !selectedRulePackId) {

      setSelectedRulePackId(rulePacksQuery.data[0].rulePackId)

    }

  }, [rulePacksQuery.data, selectedRulePackId])

  const ready = Boolean(session && meQuery.data)
  const me = meQuery.data!
  const canManage = meQuery.data
    ? canManageVocabulary(me.tenantRoleKey, me.isPlatformAdmin)
    : false
  const canExportAudit = meQuery.data
    ? canExportAuditPackage(me.tenantRoleKey, me.isPlatformAdmin)
    : false
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
    selectedRulePackId,
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
    regulatoryMappingsQuery,
    rulePackContentQuery,
    ruleEvaluationsQuery,
    findingsQuery,
    workflowGatesQuery,
    seedMutation,
    seedRegistryMutation,
    advanceRulePackMutation,
    seedCatalogMutation,
    seedSourcesMutation,
    seedMappingsMutation,
    saveRuleContentMutation,
    seedRuleContentMutation,
    evaluateRulePackMutation,
    evaluateRulePackBatchMutation,
    seedWorkflowGateMutation,
    checkWorkflowGateMutation,
    checkWorkflowGateBatchMutation,
    canManage,
    canExportAudit,
  }
}

export type ComplianceCoreWorkspaceState = ReturnType<typeof useComplianceCoreWorkspaceState>
