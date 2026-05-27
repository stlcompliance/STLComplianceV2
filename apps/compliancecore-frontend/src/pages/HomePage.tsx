import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'

import { useEffect, useState } from 'react'

import { Navigate, useSearchParams } from 'react-router-dom'

import {

  ComplianceCoreApiError,

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

import { canExportAuditPackage, canManageVocabulary, clearSession, loadSession } from '../auth/sessionStorage'

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



type AdminTab =
  | 'dashboard'
  | 'vocabulary'
  | 'regulatory'
  | 'citations'
  | 'sources'
  | 'mappings'
  | 'evaluation'
  | 'findings'
  | 'csv'
  | 'audit'



export function HomePage() {

  const [searchParams] = useSearchParams()

  const handoff = searchParams.get('handoff')

  if (handoff) {

    return <Navigate to={`/launch?handoff=${encodeURIComponent(handoff)}`} replace />

  }



  const session = loadSession()

  const queryClient = useQueryClient()

  const [selectedTypeKey, setSelectedTypeKey] = useState('material_hazard')

  const [activeTab, setActiveTab] = useState<AdminTab>('dashboard')

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



  if (!session) {

    return (

      <main className="mx-auto max-w-3xl p-6">

        <div className="rounded-xl border border-slate-700 bg-slate-900/80 p-8 text-center">

          <h1 className="text-xl font-semibold">Compliance Core</h1>

          <p className="mt-3 text-sm text-slate-400">Launch Compliance Core from the suite to sign in.</p>

        </div>

      </main>

    )

  }



  if (meQuery.isError) {

    const err = meQuery.error

    if (err instanceof ComplianceCoreApiError && (err.status === 401 || err.status === 403)) {

      clearSession()

    }

    return (

      <main className="mx-auto max-w-3xl p-6">

        <div className="rounded-xl border border-red-800 bg-red-950/30 p-8 text-center">

          <p className="text-sm text-red-200">Session expired or not entitled. Relaunch Compliance Core from the suite.</p>

        </div>

      </main>

    )

  }



  if (meQuery.isLoading || !meQuery.data) {

    return (

      <main className="mx-auto max-w-3xl p-6">

        <p className="text-slate-400">Loading Compliance Core…</p>

      </main>

    )

  }



  const me = meQuery.data

  const canManage = canManageVocabulary(me.tenantRoleKey, me.isPlatformAdmin)
  const canExportAudit = canExportAuditPackage(me.tenantRoleKey, me.isPlatformAdmin)



  return (

    <main className="mx-auto max-w-5xl space-y-6 p-6">

      <header className="rounded-xl border border-slate-700 bg-slate-900/80 p-5">

        <p className="text-xs uppercase tracking-wide text-slate-500">Compliance Core</p>

        <h1 className="text-2xl font-semibold text-slate-50">Compliance authority registry</h1>

        <p className="mt-1 text-sm text-slate-400">

          {me.displayName} · {me.tenantRoleKey.replace('_', ' ')} · {typesQuery.data?.length ?? 0} vocabulary types ·{' '}

          {rulePacksQuery.data?.length ?? 0} rule packs · {citationsQuery.data?.length ?? 0} citations ·{' '}

          {regulatoryMappingsQuery.data?.length ?? 0} mappings ·{' '}

          {factSourcesQuery.data?.length ?? 0} fact sources ·{' '}

          {ruleEvaluationsQuery.data?.length ?? 0} evaluations · {findingsQuery.data?.length ?? 0} findings ·{' '}

          {workflowGatesQuery.data?.length ?? 0} gates

        </p>

        <nav className="mt-4 flex flex-wrap gap-2">

          <button

            type="button"

            onClick={() => setActiveTab('dashboard')}

            className={`rounded-md px-3 py-1.5 text-sm ${

              activeTab === 'dashboard'

                ? 'bg-violet-600 text-white'

                : 'bg-slate-800 text-slate-300 hover:bg-slate-700'

            }`}

          >

            Dashboard

          </button>

          <button

            type="button"

            onClick={() => setActiveTab('vocabulary')}

            className={`rounded-md px-3 py-1.5 text-sm ${

              activeTab === 'vocabulary'

                ? 'bg-violet-600 text-white'

                : 'bg-slate-800 text-slate-300 hover:bg-slate-700'

            }`}

          >

            Vocabulary & keys

          </button>

          <button

            type="button"

            onClick={() => setActiveTab('regulatory')}

            className={`rounded-md px-3 py-1.5 text-sm ${

              activeTab === 'regulatory'

                ? 'bg-violet-600 text-white'

                : 'bg-slate-800 text-slate-300 hover:bg-slate-700'

            }`}

          >

            Regulatory & rule packs

          </button>

          <button

            type="button"

            onClick={() => setActiveTab('citations')}

            className={`rounded-md px-3 py-1.5 text-sm ${

              activeTab === 'citations'

                ? 'bg-violet-600 text-white'

                : 'bg-slate-800 text-slate-300 hover:bg-slate-700'

            }`}

          >

            Citations & facts

          </button>

          <button

            type="button"

            onClick={() => setActiveTab('sources')}

            className={`rounded-md px-3 py-1.5 text-sm ${

              activeTab === 'sources'

                ? 'bg-violet-600 text-white'

                : 'bg-slate-800 text-slate-300 hover:bg-slate-700'

            }`}

          >

            Fact sources

          </button>

          <button

            type="button"

            onClick={() => setActiveTab('mappings')}

            className={`rounded-md px-3 py-1.5 text-sm ${

              activeTab === 'mappings'

                ? 'bg-violet-600 text-white'

                : 'bg-slate-800 text-slate-300 hover:bg-slate-700'

            }`}

          >

            Regulatory mappings

          </button>

          <button

            type="button"

            onClick={() => setActiveTab('evaluation')}

            className={`rounded-md px-3 py-1.5 text-sm ${

              activeTab === 'evaluation'

                ? 'bg-violet-600 text-white'

                : 'bg-slate-800 text-slate-300 hover:bg-slate-700'

            }`}

          >

            Rule evaluation

          </button>

          <button

            type="button"

            onClick={() => setActiveTab('findings')}

            className={`rounded-md px-3 py-1.5 text-sm ${

              activeTab === 'findings'

                ? 'bg-violet-600 text-white'

                : 'bg-slate-800 text-slate-300 hover:bg-slate-700'

            }`}

          >

            Findings & gates

          </button>

          <button

            type="button"

            onClick={() => setActiveTab('csv')}

            className={`rounded-md px-3 py-1.5 text-sm ${

              activeTab === 'csv'

                ? 'bg-violet-600 text-white'

                : 'bg-slate-800 text-slate-300 hover:bg-slate-700'

            }`}

          >

            CSV bundle

          </button>

          <button

            type="button"

            onClick={() => setActiveTab('audit')}

            className={`rounded-md px-3 py-1.5 text-sm ${

              activeTab === 'audit'

                ? 'bg-violet-600 text-white'

                : 'bg-slate-800 text-slate-300 hover:bg-slate-700'

            }`}

          >

            Audit package

          </button>

        </nav>

      </header>



      {activeTab === 'dashboard' ? (

        <OperatorDashboardPanel accessToken={session!.accessToken} />

      ) : activeTab === 'audit' ? (

        <AuditPackageExportPanel accessToken={session!.accessToken} canExport={canExportAudit} />

      ) : activeTab === 'csv' ? (

        <CsvImportExportPanel accessToken={session!.accessToken} canManage={canManage} />

      ) : activeTab === 'findings' ? (

        <FindingsWorkflowGatesPanel

          rulePacks={rulePacksQuery.data ?? []}

          factDefinitions={factDefinitionsQuery.data ?? []}

          rulePackContent={rulePackContentQuery.data?.content ?? null}

          findings={findingsQuery.data ?? []}

          workflowGates={workflowGatesQuery.data ?? []}

          canManage={canManage}

          onSeedGate={() => seedWorkflowGateMutation.mutate()}

          isSeedingGate={seedWorkflowGateMutation.isPending}

          onCheckGate={(gateKey, facts, emitFindings) =>

            checkWorkflowGateMutation.mutate({ gateKey, facts, emitFindings })

          }

          isCheckingGate={checkWorkflowGateMutation.isPending}

          lastGateCheck={lastGateCheck}

          onCheckGateBatch={(gateKeys, facts, emitFindings) =>
            checkWorkflowGateBatchMutation.mutate({ gateKeys, facts, emitFindings })
          }

          isCheckingGateBatch={checkWorkflowGateBatchMutation.isPending}

          lastGateBatch={lastGateBatch}

        />

      ) : activeTab === 'evaluation' ? (

        <RuleEvaluationPanel

          rulePacks={rulePacksQuery.data ?? []}

          factDefinitions={factDefinitionsQuery.data ?? []}

          selectedRulePackId={selectedRulePackId}

          onSelectRulePack={setSelectedRulePackId}

          content={rulePackContentQuery.data?.content ?? null}

          hasContent={rulePackContentQuery.data?.hasContent ?? false}

          evaluationRuns={ruleEvaluationsQuery.data ?? []}

          canManage={canManage}

          onSaveContent={(content) => saveRuleContentMutation.mutate(content)}

          isSavingContent={saveRuleContentMutation.isPending}

          onSeedContent={() => seedRuleContentMutation.mutate()}

          isSeedingContent={seedRuleContentMutation.isPending}

          onEvaluate={(facts) => evaluateRulePackMutation.mutate(facts)}

          isEvaluating={evaluateRulePackMutation.isPending}

          lastEvaluation={lastEvaluation}

          onEvaluateBatch={(rulePackKeys, facts, emitFindings) =>
            evaluateRulePackBatchMutation.mutate({ rulePackKeys, facts, emitFindings })
          }

          isEvaluatingBatch={evaluateRulePackBatchMutation.isPending}

          lastBatchEvaluation={lastBatchEvaluation}

        />

      ) : activeTab === 'sources' ? (

        <FactSourcesPanel

          factDefinitions={factDefinitionsQuery.data ?? []}

          factSources={factSourcesQuery.data ?? []}

          canManage={canManage}

          onSeedSources={() => seedSourcesMutation.mutate()}

          isSeeding={seedSourcesMutation.isPending}

        />

      ) : activeTab === 'mappings' ? (

        <RegulatoryMappingsPanel

          mappings={regulatoryMappingsQuery.data ?? []}

          canManage={canManage}

          onSeedMappings={() => seedMappingsMutation.mutate()}

          isSeeding={seedMappingsMutation.isPending}

        />

      ) : activeTab === 'citations' ? (

        <CitationFactCatalogPanel

          citations={citationsQuery.data ?? []}

          factDefinitions={factDefinitionsQuery.data ?? []}

          factRequirements={factRequirementsQuery.data ?? []}

          canManage={canManage}

          onSeedCatalog={() => seedCatalogMutation.mutate()}

          isSeeding={seedCatalogMutation.isPending}

        />

      ) : activeTab === 'vocabulary' ? (

        <VocabularyPanel

          types={typesQuery.data ?? []}

          terms={termsQuery.data ?? []}

          complianceKeys={complianceKeysQuery.data ?? []}

          materialKeys={materialKeysQuery.data ?? []}

          selectedTypeKey={selectedTypeKey}

          onSelectType={setSelectedTypeKey}

          canManage={canManage}

          onCreateTerm={() => seedMutation.mutate()}

          isCreatingTerm={seedMutation.isPending}

        />

      ) : (

        <RegulatoryRegistryPanel

          governingBodies={governingBodiesQuery.data ?? []}

          jurisdictions={jurisdictionsQuery.data ?? []}

          programs={programsQuery.data ?? []}

          rulePacks={rulePacksQuery.data ?? []}

          canManage={canManage}

          onSeedRegistry={() => seedRegistryMutation.mutate()}

          isSeeding={seedRegistryMutation.isPending}

          onAdvanceRulePack={(rulePackId, status) => advanceRulePackMutation.mutate({ rulePackId, status })}

          isAdvancingRulePack={advanceRulePackMutation.isPending}

        />

      )}

    </main>

  )

}


