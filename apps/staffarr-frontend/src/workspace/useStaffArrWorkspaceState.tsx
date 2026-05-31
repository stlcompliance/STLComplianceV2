import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useEffect, useState } from 'react'
import { Navigate, useSearchParams } from 'react-router-dom'
import {
  createPerson,
  createPersonOrgAssignment,
  createPersonRoleAssignment,
  createRoleTemplate,
  createOrgUnit,
  getCertificationDefinitions,
  getEffectivePermissions,
  getManagerChain,
  getMe,
  getOrgUnits,
  getPermissionTemplates,
  getPermissionHistoryTimeline,
  getPersonTimeline,
  getPersonHistorySummary,
  getPersonTrainarrTrainingHistory,
  getWorkforceOnboardingJourney,
  getPersonOffboarding,
  startPersonOffboarding,
  executePersonOffboarding,
  getPersonCertifications,
  clearPersonReadinessOverride,
  createPersonnelIncident,
  routePersonnelIncidentToTrainarr,
  getPersonnelIncident,
  listPersonnelNotes,
  getPersonnelNote,
  createPersonnelNote,
  listPersonnelDocuments,
  getPersonnelDocument,
  createPersonnelDocument,
  personnelDocumentContentUrl,
  getPersonReadiness,
  getPersonLookup,
  getReadinessRollupMembers,
  getSiteReadinessRollups,
  getTeamReadinessRollups,
  grantPersonReadinessOverride,
  listPersonnelIncidents,
  getPersonOrgAssignments,
  getPersonRoleAssignments,
  getRoleTemplates,
  getSubordinateDetail,
  getSubordinates,
  getPeople,
  getPerson,
  updatePerson,
  updatePersonEmploymentStatus,
  grantPersonCertification,
  updatePersonManager,
  updatePersonOrgAssignment,
  updatePersonOrgAssignmentStatus,
  updatePersonRoleAssignmentStatus,
  updatePersonCertification,
  updateRoleTemplate,
  upsertPermissionTemplate,
  updateOrgUnit,
  updateOrgUnitStatus,
} from '../api/client'
import { loadSession, canExportAuditPackage, canReadReports as userCanReadReports } from '../auth/sessionStorage'
import { canManagePeople } from '../components/PersonProfileEditorPanel'
import { canManageIncidents } from '../components/IncidentsPanel'
import { canManagePersonnelNotes } from '../components/PersonnelNotesPanel'
import { canManagePersonnelDocuments } from '../components/PersonnelDocumentsPanel'
import { canOverrideReadiness } from '../components/ReadinessPanel'
import { canViewReadinessRollups } from '../components/ReadinessRollupSupervisorPanel'
import { filterPeopleDirectory } from '../lib/peopleDirectoryFilter'
import { canManageOrgHierarchy } from '../components/OrgHierarchyManager'
import type { PersonTimelineCategoryFilter } from '../components/PersonTimelinePanel'
import type { ReadinessRollupSelection } from '../api/types'

export function useStaffArrWorkspaceState() {

  const [searchParams] = useSearchParams()
  const handoff = searchParams.get('handoff')
  const handoffRedirect = handoff
    ? <Navigate to={`/launch?handoff=${encodeURIComponent(handoff)}`} replace />
    : null

  const session = loadSession()
  const accessToken = session?.accessToken ?? ''
  const [apiError] = useState<string | null>(null)
  const queryClient = useQueryClient()

  const meQuery = useQuery({
    queryKey: ['staffarr-me', session?.accessToken],
    queryFn: () => getMe(session!.accessToken),
    enabled: Boolean(session?.accessToken),
  })
  const peopleQuery = useQuery({
    queryKey: ['staffarr-people', session?.accessToken],
    queryFn: () => getPeople(session!.accessToken),
    enabled: Boolean(session?.accessToken),
  })
  const orgUnitsQuery = useQuery({
    queryKey: ['staffarr-org-units', session?.accessToken],
    queryFn: () => getOrgUnits(session!.accessToken),
    enabled: Boolean(session?.accessToken),
  })
  const [selectedPersonId, setSelectedPersonId] = useState<string | null>(null)
  const [activeDirectoryPersonId, setActiveDirectoryPersonId] = useState<string | null>(null)
  const [peopleDirectoryQuery, setPeopleDirectoryQuery] = useState('')
  const [personTimelinePage, setPersonTimelinePage] = useState(1)
  const [personTimelinePageSize, setPersonTimelinePageSize] = useState(25)
  const [personTimelineCategoryFilter, setPersonTimelineCategoryFilter] =
    useState<PersonTimelineCategoryFilter>('')
  const fallbackPersonId = peopleQuery.data?.[0]?.personId ?? meQuery.data?.personId ?? null
  useEffect(() => {
    if (!selectedPersonId && fallbackPersonId) {
      setSelectedPersonId(fallbackPersonId)
    }
  }, [fallbackPersonId, selectedPersonId])

  const effectivePersonId = selectedPersonId ?? fallbackPersonId

  useEffect(() => {
    setPersonTimelinePage(1)
  }, [effectivePersonId, personTimelineCategoryFilter, personTimelinePageSize])
  const personProfileQuery = useQuery({
    queryKey: ['staffarr-person', session?.accessToken, effectivePersonId],
    queryFn: () => getPerson(session!.accessToken, effectivePersonId!),
    enabled: Boolean(session?.accessToken && effectivePersonId),
  })
  const assignmentQuery = useQuery({
    queryKey: ['staffarr-org-assignments', session?.accessToken, effectivePersonId],
    queryFn: () => getPersonOrgAssignments(session!.accessToken, effectivePersonId!),
    enabled: Boolean(session?.accessToken && effectivePersonId),
  })
  const [selectedSubordinateId, setSelectedSubordinateId] = useState<string | null>(null)
  useEffect(() => {
    setSelectedSubordinateId(null)
  }, [effectivePersonId])
  const managerChainQuery = useQuery({
    queryKey: ['staffarr-manager-chain', session?.accessToken, effectivePersonId],
    queryFn: () => getManagerChain(session!.accessToken, effectivePersonId!),
    enabled: Boolean(session?.accessToken && effectivePersonId),
  })
  const subordinatesQuery = useQuery({
    queryKey: ['staffarr-subordinates', session?.accessToken, effectivePersonId],
    queryFn: () => getSubordinates(session!.accessToken, effectivePersonId!, true, 200),
    enabled: Boolean(session?.accessToken && effectivePersonId),
  })
  const subordinateDetailQuery = useQuery({
    queryKey: ['staffarr-subordinate-detail', session?.accessToken, effectivePersonId, selectedSubordinateId],
    queryFn: () => getSubordinateDetail(session!.accessToken, effectivePersonId!, selectedSubordinateId!),
    enabled: Boolean(session?.accessToken && effectivePersonId && selectedSubordinateId),
  })
  const permissionTemplatesQuery = useQuery({
    queryKey: ['staffarr-permission-templates', session?.accessToken],
    queryFn: () => getPermissionTemplates(session!.accessToken),
    enabled: Boolean(session?.accessToken),
  })
  const roleTemplatesQuery = useQuery({
    queryKey: ['staffarr-role-templates', session?.accessToken],
    queryFn: () => getRoleTemplates(session!.accessToken),
    enabled: Boolean(session?.accessToken),
  })
  const roleAssignmentsQuery = useQuery({
    queryKey: ['staffarr-role-assignments', session?.accessToken, effectivePersonId],
    queryFn: () => getPersonRoleAssignments(session!.accessToken, effectivePersonId!),
    enabled: Boolean(session?.accessToken && effectivePersonId),
  })
  const effectivePermissionsQuery = useQuery({
    queryKey: ['staffarr-effective-permissions', session?.accessToken, effectivePersonId],
    queryFn: () => getEffectivePermissions(session!.accessToken, effectivePersonId!),
    enabled: Boolean(session?.accessToken && effectivePersonId),
  })
  const permissionHistoryQuery = useQuery({
    queryKey: ['staffarr-permission-history', session?.accessToken, effectivePersonId],
    queryFn: () => getPermissionHistoryTimeline(session!.accessToken, effectivePersonId!, 100),
    enabled: Boolean(session?.accessToken && effectivePersonId),
  })
  const personTimelineQuery = useQuery({
    queryKey: [
      'staffarr-person-timeline',
      session?.accessToken,
      effectivePersonId,
      personTimelinePage,
      personTimelinePageSize,
      personTimelineCategoryFilter,
    ],
    queryFn: () =>
      getPersonTimeline(
        session!.accessToken,
        effectivePersonId!,
        personTimelinePage,
        personTimelinePageSize,
        personTimelineCategoryFilter || undefined,
      ),
    enabled: Boolean(session?.accessToken && effectivePersonId),
  })
  const personHistorySummaryQuery = useQuery({
    queryKey: ['staffarr-person-history-summary', session?.accessToken, effectivePersonId],
    queryFn: () => getPersonHistorySummary(session!.accessToken, effectivePersonId!),
    enabled: Boolean(session?.accessToken && effectivePersonId),
  })
  const trainarrTrainingHistoryQuery = useQuery({
    queryKey: ['staffarr-trainarr-training-history', session?.accessToken, effectivePersonId],
    queryFn: () => getPersonTrainarrTrainingHistory(session!.accessToken, effectivePersonId!),
    enabled: Boolean(session?.accessToken && effectivePersonId),
  })
  const workforceOnboardingJourneyQuery = useQuery({
    queryKey: ['staffarr-workforce-onboarding-journey', session?.accessToken, effectivePersonId],
    queryFn: () => getWorkforceOnboardingJourney(session!.accessToken, effectivePersonId!),
    enabled: Boolean(session?.accessToken && effectivePersonId),
  })
  const personOffboardingQuery = useQuery({
    queryKey: ['staffarr-person-offboarding', session?.accessToken, effectivePersonId],
    queryFn: () => getPersonOffboarding(session!.accessToken, effectivePersonId!),
    enabled: Boolean(session?.accessToken && effectivePersonId),
  })
  const certificationDefinitionsQuery = useQuery({
    queryKey: ['staffarr-certification-definitions', session?.accessToken],
    queryFn: () => getCertificationDefinitions(session!.accessToken),
    enabled: Boolean(session?.accessToken),
  })
  const personCertificationsQuery = useQuery({
    queryKey: ['staffarr-person-certifications', session?.accessToken, effectivePersonId],
    queryFn: () => getPersonCertifications(session!.accessToken, effectivePersonId!),
    enabled: Boolean(session?.accessToken && effectivePersonId),
  })
  const personReadinessQuery = useQuery({
    queryKey: ['staffarr-person-readiness', session?.accessToken, effectivePersonId],
    queryFn: () => getPersonReadiness(session!.accessToken, effectivePersonId!),
    enabled: Boolean(session?.accessToken && effectivePersonId),
  })
  const personLookupQuery = useQuery({
    queryKey: ['staffarr-person-lookup', session?.accessToken, effectivePersonId],
    queryFn: () => getPersonLookup(session!.accessToken, effectivePersonId!),
    enabled: Boolean(session?.accessToken && effectivePersonId),
  })
  const canViewReadinessRollupSummaries =
    meQuery.data != null &&
    canViewReadinessRollups(meQuery.data.tenantRoleKey, meQuery.data.isPlatformAdmin)
  const [readinessRollupSiteFilterId, setReadinessRollupSiteFilterId] = useState<string | null>(null)
  const [selectedReadinessRollup, setSelectedReadinessRollup] = useState<ReadinessRollupSelection | null>(null)
  const [readinessRollupMemberFilter, setReadinessRollupMemberFilter] = useState<'all' | 'not_ready'>('all')
  useEffect(() => {
    setSelectedReadinessRollup(null)
  }, [readinessRollupSiteFilterId])
  const teamReadinessRollupsQuery = useQuery({
    queryKey: ['staffarr-team-readiness-rollups', session?.accessToken, readinessRollupSiteFilterId],
    queryFn: () =>
      getTeamReadinessRollups(session!.accessToken, readinessRollupSiteFilterId ?? undefined),
    enabled: Boolean(session?.accessToken && canViewReadinessRollupSummaries),
  })
  const siteReadinessRollupsQuery = useQuery({
    queryKey: ['staffarr-site-readiness-rollups', session?.accessToken],
    queryFn: () => getSiteReadinessRollups(session!.accessToken),
    enabled: Boolean(session?.accessToken && canViewReadinessRollupSummaries),
  })
  const readinessRollupMembersQuery = useQuery({
    queryKey: [
      'staffarr-readiness-rollup-members',
      session?.accessToken,
      selectedReadinessRollup?.scopeType,
      selectedReadinessRollup?.orgUnitId,
      readinessRollupMemberFilter,
    ],
    queryFn: () =>
      getReadinessRollupMembers(
        session!.accessToken,
        selectedReadinessRollup!.scopeType,
        selectedReadinessRollup!.orgUnitId,
        readinessRollupMemberFilter === 'not_ready' ? 'not_ready' : undefined,
      ),
    enabled: Boolean(session?.accessToken && canViewReadinessRollupSummaries && selectedReadinessRollup),
  })
  const [selectedIncidentId, setSelectedIncidentId] = useState<string | null>(null)
  useEffect(() => {
    setSelectedIncidentId(null)
  }, [effectivePersonId])
  const personIncidentsQuery = useQuery({
    queryKey: ['staffarr-person-incidents', session?.accessToken, effectivePersonId],
    queryFn: () => listPersonnelIncidents(session!.accessToken, effectivePersonId!),
    enabled: Boolean(session?.accessToken && effectivePersonId),
  })
  const incidentDetailQuery = useQuery({
    queryKey: ['staffarr-incident-detail', session?.accessToken, selectedIncidentId],
    queryFn: () => getPersonnelIncident(session!.accessToken, selectedIncidentId!),
    enabled: Boolean(session?.accessToken && selectedIncidentId),
  })
  const [selectedNoteId, setSelectedNoteId] = useState<string | null>(null)
  useEffect(() => {
    setSelectedNoteId(null)
  }, [effectivePersonId])
  const personNotesQuery = useQuery({
    queryKey: ['staffarr-person-notes', session?.accessToken, effectivePersonId],
    queryFn: () => listPersonnelNotes(session!.accessToken, effectivePersonId!),
    enabled: Boolean(session?.accessToken && effectivePersonId),
  })
  const noteDetailQuery = useQuery({
    queryKey: ['staffarr-note-detail', session?.accessToken, effectivePersonId, selectedNoteId],
    queryFn: () => getPersonnelNote(session!.accessToken, effectivePersonId!, selectedNoteId!),
    enabled: Boolean(session?.accessToken && effectivePersonId && selectedNoteId),
  })
  const [selectedDocumentId, setSelectedDocumentId] = useState<string | null>(null)
  useEffect(() => {
    setSelectedDocumentId(null)
  }, [effectivePersonId])
  const personDocumentsQuery = useQuery({
    queryKey: ['staffarr-person-documents', session?.accessToken, effectivePersonId],
    queryFn: () => listPersonnelDocuments(session!.accessToken, effectivePersonId!),
    enabled: Boolean(session?.accessToken && effectivePersonId),
  })
  const documentDetailQuery = useQuery({
    queryKey: ['staffarr-document-detail', session?.accessToken, effectivePersonId, selectedDocumentId],
    queryFn: () => getPersonnelDocument(session!.accessToken, effectivePersonId!, selectedDocumentId!),
    enabled: Boolean(session?.accessToken && effectivePersonId && selectedDocumentId),
  })

  const createOrgUnitMutation = useMutation({
    mutationFn: (payload: { unitType: string; name: string; parentOrgUnitId: string | null }) =>
      createOrgUnit(session!.accessToken, payload),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['staffarr-org-units', session?.accessToken] })
    },
  })

  const updateOrgUnitMutation = useMutation({
    mutationFn: (payload: { orgUnitId: string; unitType: string; name: string; parentOrgUnitId: string | null }) =>
      updateOrgUnit(session!.accessToken, payload.orgUnitId, {
        unitType: payload.unitType,
        name: payload.name,
        parentOrgUnitId: payload.parentOrgUnitId,
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['staffarr-org-units', session?.accessToken] })
    },
  })

  const updateOrgUnitStatusMutation = useMutation({
    mutationFn: (payload: { orgUnitId: string; status: 'active' | 'inactive' }) =>
      updateOrgUnitStatus(session!.accessToken, payload.orgUnitId, { status: payload.status }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['staffarr-org-units', session?.accessToken] })
    },
  })

  const createAssignmentMutation = useMutation({
    mutationFn: (payload: {
      personId: string
      siteOrgUnitId: string
      departmentOrgUnitId: string
      teamOrgUnitId: string
      positionOrgUnitId: string
    }) =>
      createPersonOrgAssignment(session!.accessToken, payload.personId, {
        siteOrgUnitId: payload.siteOrgUnitId,
        departmentOrgUnitId: payload.departmentOrgUnitId,
        teamOrgUnitId: payload.teamOrgUnitId,
        positionOrgUnitId: payload.positionOrgUnitId,
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['staffarr-org-assignments', session?.accessToken] })
    },
  })

  const updateAssignmentMutation = useMutation({
    mutationFn: (payload: {
      personId: string
      assignmentId: string
      siteOrgUnitId: string
      departmentOrgUnitId: string
      teamOrgUnitId: string
      positionOrgUnitId: string
    }) =>
      updatePersonOrgAssignment(session!.accessToken, payload.personId, payload.assignmentId, {
        siteOrgUnitId: payload.siteOrgUnitId,
        departmentOrgUnitId: payload.departmentOrgUnitId,
        teamOrgUnitId: payload.teamOrgUnitId,
        positionOrgUnitId: payload.positionOrgUnitId,
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['staffarr-org-assignments', session?.accessToken] })
    },
  })

  const updateAssignmentStatusMutation = useMutation({
    mutationFn: (payload: { personId: string; assignmentId: string; status: 'active' | 'inactive' }) =>
      updatePersonOrgAssignmentStatus(session!.accessToken, payload.personId, payload.assignmentId, { status: payload.status }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['staffarr-org-assignments', session?.accessToken] })
    },
  })
  const updateManagerMutation = useMutation({
    mutationFn: (payload: { personId: string; managerPersonId: string | null }) =>
      updatePersonManager(session!.accessToken, payload.personId, {
        managerPersonId: payload.managerPersonId,
      }),
    onSuccess: async () => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: ['staffarr-people', session?.accessToken] }),
        queryClient.invalidateQueries({ queryKey: ['staffarr-person', session?.accessToken] }),
        queryClient.invalidateQueries({ queryKey: ['staffarr-manager-chain', session?.accessToken] }),
        queryClient.invalidateQueries({ queryKey: ['staffarr-subordinates', session?.accessToken] }),
        queryClient.invalidateQueries({ queryKey: ['staffarr-subordinate-detail', session?.accessToken] }),
      ])
    },
  })
  const upsertPermissionTemplateMutation = useMutation({
    mutationFn: (payload: { permissionKey: string; name: string; description: string | null }) =>
      upsertPermissionTemplate(session!.accessToken, payload),
    onSuccess: async () => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: ['staffarr-permission-templates', session?.accessToken] }),
        queryClient.invalidateQueries({ queryKey: ['staffarr-effective-permissions', session?.accessToken] }),
      ])
    },
  })
  const createRoleTemplateMutation = useMutation({
    mutationFn: (payload: {
      roleKey: string
      name: string
      description: string | null
      permissions: Array<{
        permissionTemplateId: string
        scopeType: 'tenant' | 'site' | 'department' | 'team' | 'position'
        scopeValue: string | null
      }>
    }) => createRoleTemplate(session!.accessToken, payload),
    onSuccess: async () => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: ['staffarr-role-templates', session?.accessToken] }),
        queryClient.invalidateQueries({ queryKey: ['staffarr-effective-permissions', session?.accessToken] }),
      ])
    },
  })
  const updateRoleTemplateMutation = useMutation({
    mutationFn: (payload: {
      roleTemplateId: string
      name: string
      description: string | null
      status: 'active' | 'inactive'
      permissions: Array<{
        permissionTemplateId: string
        scopeType: 'tenant' | 'site' | 'department' | 'team' | 'position'
        scopeValue: string | null
      }>
    }) =>
      updateRoleTemplate(session!.accessToken, payload.roleTemplateId, {
        name: payload.name,
        description: payload.description,
        status: payload.status,
        permissions: payload.permissions,
      }),
    onSuccess: async () => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: ['staffarr-role-templates', session?.accessToken] }),
        queryClient.invalidateQueries({ queryKey: ['staffarr-effective-permissions', session?.accessToken] }),
        queryClient.invalidateQueries({ queryKey: ['staffarr-permission-history', session?.accessToken] }),
        queryClient.invalidateQueries({ queryKey: ['staffarr-person-timeline', session?.accessToken] }),
      ])
    },
  })
  const createRoleAssignmentMutation = useMutation({
    mutationFn: (payload: {
      personId: string
      roleTemplateId: string
      scopeType: 'tenant' | 'site' | 'department' | 'team' | 'position'
      scopeValue: string | null
    }) => createPersonRoleAssignment(session!.accessToken, payload.personId, payload),
    onSuccess: async () => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: ['staffarr-role-assignments', session?.accessToken] }),
        queryClient.invalidateQueries({ queryKey: ['staffarr-effective-permissions', session?.accessToken] }),
        queryClient.invalidateQueries({ queryKey: ['staffarr-permission-history', session?.accessToken] }),
        queryClient.invalidateQueries({ queryKey: ['staffarr-person-timeline', session?.accessToken] }),
      ])
    },
  })
  const updateRoleAssignmentStatusMutation = useMutation({
    mutationFn: (payload: { personId: string; assignmentId: string; status: 'active' | 'inactive' }) =>
      updatePersonRoleAssignmentStatus(session!.accessToken, payload.personId, payload.assignmentId, payload.status),
    onSuccess: async () => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: ['staffarr-role-assignments', session?.accessToken] }),
        queryClient.invalidateQueries({ queryKey: ['staffarr-effective-permissions', session?.accessToken] }),
        queryClient.invalidateQueries({ queryKey: ['staffarr-permission-history', session?.accessToken] }),
        queryClient.invalidateQueries({ queryKey: ['staffarr-person-timeline', session?.accessToken] }),
      ])
    },
  })
  const grantCertificationMutation = useMutation({
    mutationFn: (payload: {
      personId: string
      certificationDefinitionId: string
      grantedAt: string | null
      expiresAt: string | null
      notes: string | null
    }) =>
      grantPersonCertification(session!.accessToken, payload.personId, {
        certificationDefinitionId: payload.certificationDefinitionId,
        grantedAt: payload.grantedAt,
        expiresAt: payload.expiresAt,
        notes: payload.notes,
      }),
    onSuccess: async () => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: ['staffarr-person-certifications', session?.accessToken] }),
        queryClient.invalidateQueries({ queryKey: ['staffarr-person-readiness', session?.accessToken] }),
        queryClient.invalidateQueries({ queryKey: ['staffarr-person-timeline', session?.accessToken] }),
      ])
    },
  })
  const updateCertificationMutation = useMutation({
    mutationFn: (payload: {
      personId: string
      personCertificationId: string
      status: 'active' | 'expired' | 'revoked'
      expiresAt: string | null
      notes: string | null
    }) =>
      updatePersonCertification(session!.accessToken, payload.personId, payload.personCertificationId, {
        status: payload.status,
        expiresAt: payload.expiresAt,
        notes: payload.notes,
      }),
    onSuccess: async () => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: ['staffarr-person-certifications', session?.accessToken] }),
        queryClient.invalidateQueries({ queryKey: ['staffarr-person-readiness', session?.accessToken] }),
        queryClient.invalidateQueries({ queryKey: ['staffarr-person-timeline', session?.accessToken] }),
      ])
    },
  })
  const createPersonMutation = useMutation({
    mutationFn: (payload: {
      givenName: string
      familyName: string
      primaryEmail: string
      employmentStatus: string
      primaryOrgUnitId: string | null
      managerPersonId: string | null
      jobTitle: string | null
    }) => createPerson(session!.accessToken, payload),
    onSuccess: async (created) => {
      setSelectedPersonId(created.personId)
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: ['staffarr-people', session?.accessToken] }),
        queryClient.invalidateQueries({ queryKey: ['staffarr-person', session?.accessToken, created.personId] }),
        queryClient.invalidateQueries({ queryKey: ['staffarr-person-timeline', session?.accessToken] }),
        queryClient.invalidateQueries({ queryKey: ['staffarr-workforce-onboarding-journey', session?.accessToken] }),
      ])
    },
  })
  const updatePersonMutation = useMutation({
    mutationFn: (payload: {
      personId: string
      givenName: string
      familyName: string
      primaryEmail: string
      primaryOrgUnitId: string | null
      managerPersonId: string | null
      jobTitle: string | null
    }) =>
      updatePerson(session!.accessToken, payload.personId, {
        givenName: payload.givenName,
        familyName: payload.familyName,
        primaryEmail: payload.primaryEmail,
        primaryOrgUnitId: payload.primaryOrgUnitId,
        managerPersonId: payload.managerPersonId,
        jobTitle: payload.jobTitle,
      }),
    onSuccess: async (_, payload) => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: ['staffarr-people', session?.accessToken] }),
        queryClient.invalidateQueries({ queryKey: ['staffarr-person', session?.accessToken, payload.personId] }),
        queryClient.invalidateQueries({ queryKey: ['staffarr-manager-chain', session?.accessToken] }),
        queryClient.invalidateQueries({ queryKey: ['staffarr-subordinates', session?.accessToken] }),
        queryClient.invalidateQueries({ queryKey: ['staffarr-person-lookup', session?.accessToken, payload.personId] }),
        queryClient.invalidateQueries({
          queryKey: ['staffarr-person-history-summary', session?.accessToken, payload.personId],
        }),
        queryClient.invalidateQueries({
          queryKey: ['staffarr-workforce-onboarding-journey', session?.accessToken, payload.personId],
        }),
        queryClient.invalidateQueries({ queryKey: ['staffarr-person-timeline', session?.accessToken] }),
      ])
    },
  })
  const updateEmploymentStatusMutation = useMutation({
    mutationFn: (payload: { personId: string; employmentStatus: string; reason: string | null }) =>
      updatePersonEmploymentStatus(session!.accessToken, payload.personId, {
        employmentStatus: payload.employmentStatus,
        reason: payload.reason,
      }),
    onSuccess: async (_, payload) => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: ['staffarr-people', session?.accessToken] }),
        queryClient.invalidateQueries({ queryKey: ['staffarr-person', session?.accessToken, payload.personId] }),
        queryClient.invalidateQueries({ queryKey: ['staffarr-person-lookup', session?.accessToken, payload.personId] }),
        queryClient.invalidateQueries({
          queryKey: ['staffarr-person-history-summary', session?.accessToken, payload.personId],
        }),
        queryClient.invalidateQueries({
          queryKey: ['staffarr-workforce-onboarding-journey', session?.accessToken, payload.personId],
        }),
        queryClient.invalidateQueries({ queryKey: ['staffarr-person-timeline', session?.accessToken] }),
      ])
    },
  })
  const startOffboardingMutation = useMutation({
    mutationFn: (payload: {
      personId: string
      separationDate: string
      separationReason: string | null
      targetEmploymentStatus: string
      disableLoginRequested: boolean
      newManagerPersonIdForReports: string | null
    }) =>
      startPersonOffboarding(session!.accessToken, {
        personId: payload.personId,
        separationDate: payload.separationDate,
        separationReason: payload.separationReason,
        targetEmploymentStatus: payload.targetEmploymentStatus,
        disableLoginRequested: payload.disableLoginRequested,
        newManagerPersonIdForReports: payload.newManagerPersonIdForReports,
      }),
    onSuccess: async (_, payload) => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: ['staffarr-person-offboarding', session?.accessToken, payload.personId] }),
        queryClient.invalidateQueries({ queryKey: ['staffarr-person-lookup', session?.accessToken, payload.personId] }),
        queryClient.invalidateQueries({
          queryKey: ['staffarr-person-history-summary', session?.accessToken, payload.personId],
        }),
        queryClient.invalidateQueries({
          queryKey: ['staffarr-workforce-onboarding-journey', session?.accessToken, payload.personId],
        }),
        queryClient.invalidateQueries({ queryKey: ['staffarr-person-timeline', session?.accessToken] }),
      ])
    },
  })
  const executeOffboardingMutation = useMutation({
    mutationFn: (payload: {
      personId: string
      offboardingId: string
      newManagerPersonIdForReports: string | null
    }) =>
      executePersonOffboarding(session!.accessToken, payload.offboardingId, {
        newManagerPersonIdForReports: payload.newManagerPersonIdForReports,
      }),
    onSuccess: async (_, payload) => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: ['staffarr-person-offboarding', session?.accessToken, payload.personId] }),
        queryClient.invalidateQueries({ queryKey: ['staffarr-people', session?.accessToken] }),
        queryClient.invalidateQueries({ queryKey: ['staffarr-person', session?.accessToken, payload.personId] }),
        queryClient.invalidateQueries({ queryKey: ['staffarr-org-assignments', session?.accessToken] }),
        queryClient.invalidateQueries({ queryKey: ['staffarr-role-assignments', session?.accessToken] }),
        queryClient.invalidateQueries({ queryKey: ['staffarr-effective-permissions', session?.accessToken] }),
        queryClient.invalidateQueries({ queryKey: ['staffarr-person-readiness', session?.accessToken] }),
        queryClient.invalidateQueries({ queryKey: ['staffarr-person-lookup', session?.accessToken, payload.personId] }),
        queryClient.invalidateQueries({
          queryKey: ['staffarr-person-history-summary', session?.accessToken, payload.personId],
        }),
        queryClient.invalidateQueries({
          queryKey: ['staffarr-workforce-onboarding-journey', session?.accessToken, payload.personId],
        }),
        queryClient.invalidateQueries({ queryKey: ['staffarr-person-timeline', session?.accessToken] }),
        queryClient.invalidateQueries({ queryKey: ['staffarr-subordinates', session?.accessToken] }),
      ])
    },
  })
  const grantReadinessOverrideMutation = useMutation({
    mutationFn: (payload: { personId: string; reason: string; expiresAt: string | null }) =>
      grantPersonReadinessOverride(session!.accessToken, payload.personId, {
        reason: payload.reason,
        expiresAt: payload.expiresAt,
      }),
    onSuccess: async () => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: ['staffarr-person-readiness', session?.accessToken] }),
        queryClient.invalidateQueries({ queryKey: ['staffarr-person-timeline', session?.accessToken] }),
      ])
    },
  })
  const clearReadinessOverrideMutation = useMutation({
    mutationFn: (personId: string) => clearPersonReadinessOverride(session!.accessToken, personId),
    onSuccess: async () => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: ['staffarr-person-readiness', session?.accessToken] }),
        queryClient.invalidateQueries({ queryKey: ['staffarr-person-timeline', session?.accessToken] }),
      ])
    },
  })
  const createIncidentMutation = useMutation({
    mutationFn: (payload: Parameters<typeof createPersonnelIncident>[1]) =>
      createPersonnelIncident(session!.accessToken, payload),
    onSuccess: async (created) => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: ['staffarr-person-incidents', session?.accessToken] }),
        queryClient.invalidateQueries({ queryKey: ['staffarr-person-timeline', session?.accessToken] }),
      ])
      setSelectedIncidentId(created.incidentId)
    },
  })
  const routeIncidentToTrainarrMutation = useMutation({
    mutationFn: (incidentId: string) =>
      routePersonnelIncidentToTrainarr(session!.accessToken, incidentId),
    onSuccess: async () => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: ['staffarr-person-incidents', session?.accessToken] }),
        queryClient.invalidateQueries({ queryKey: ['staffarr-incident-detail', session?.accessToken] }),
        queryClient.invalidateQueries({ queryKey: ['staffarr-person-timeline', session?.accessToken] }),
      ])
    },
  })
  const createNoteMutation = useMutation({
    mutationFn: (payload: Parameters<typeof createPersonnelNote>[2]) =>
      createPersonnelNote(session!.accessToken, effectivePersonId!, payload),
    onSuccess: async (created) => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: ['staffarr-person-notes', session?.accessToken] }),
        queryClient.invalidateQueries({ queryKey: ['staffarr-person-timeline', session?.accessToken] }),
      ])
      setSelectedNoteId(created.noteId)
    },
  })
  const uploadDocumentMutation = useMutation({
    mutationFn: (payload: Parameters<typeof createPersonnelDocument>[2]) =>
      createPersonnelDocument(session!.accessToken, effectivePersonId!, payload),
    onSuccess: async (created) => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: ['staffarr-person-documents', session?.accessToken] }),
        queryClient.invalidateQueries({ queryKey: ['staffarr-person-timeline', session?.accessToken] }),
      ])
      setSelectedDocumentId(created.documentId)
    },
  })

  const me = meQuery.data
  const people = peopleQuery.data ?? []
  const filteredPeople = filterPeopleDirectory(people, peopleDirectoryQuery)
  const orgUnits = orgUnitsQuery.data ?? []
  const profile = personProfileQuery.data
  const selectedPerson = people.find((person) => person.personId === effectivePersonId) ?? null
  const selectedPersonHiddenByFilter =
    Boolean(peopleDirectoryQuery.trim()) &&
    Boolean(selectedPerson) &&
    !filteredPeople.some((person) => person.personId === selectedPerson?.personId)
  const assignments = assignmentQuery.data ?? []
  const managerChain = managerChainQuery.data ?? []
  const subordinates = subordinatesQuery.data ?? []
  const selectedSubordinateDetail = subordinateDetailQuery.data ?? null
  const permissionTemplates = permissionTemplatesQuery.data ?? []
  const roleTemplates = roleTemplatesQuery.data ?? []
  const roleAssignments = roleAssignmentsQuery.data ?? []
  const effectivePermissions = effectivePermissionsQuery.data ?? null
  const permissionHistory = permissionHistoryQuery.data ?? []
  const personTimelineEntries = personTimelineQuery.data?.items ?? []
  const personTimelineTotalCount = personTimelineQuery.data?.totalCount ?? 0
  const personTimelineHasNextPage = personTimelineQuery.data?.hasNextPage ?? false
  const certificationDefinitions = certificationDefinitionsQuery.data ?? []
  const personCertifications = personCertificationsQuery.data ?? []
  const canManageOrgUnits = me ? canManageOrgHierarchy(me.tenantRoleKey, me.isPlatformAdmin) : false
  const canManageHierarchy = canManageOrgUnits
  const canOverridePersonReadiness = me ? canOverrideReadiness(me.tenantRoleKey, me.isPlatformAdmin) : false
  const canManagePersonIncidents = me ? canManageIncidents(me.tenantRoleKey, me.isPlatformAdmin) : false
  const canManagePersonNotes = me ? canManagePersonnelNotes(me.tenantRoleKey, me.isPlatformAdmin) : false
  const canManagePersonDocuments = me ? canManagePersonnelDocuments(me.tenantRoleKey, me.isPlatformAdmin) : false
  const canManagePeopleProfiles = me ? canManagePeople(me.tenantRoleKey, me.isPlatformAdmin) : false
  const canExportAudit = me ? canExportAuditPackage(me.tenantRoleKey, me.isPlatformAdmin) : false
  const canReadReports = me ? userCanReadReports(me.tenantRoleKey, me.isPlatformAdmin) : false
  const personIncidents = personIncidentsQuery.data ?? []
  const orgMutationError =
    createOrgUnitMutation.error ?? updateOrgUnitMutation.error ?? updateOrgUnitStatusMutation.error ?? null
  const assignmentMutationError =
    createAssignmentMutation.error ?? updateAssignmentMutation.error ?? updateAssignmentStatusMutation.error ?? null
  const managerMutationError = updateManagerMutation.error
  const roleTemplateMutationError =
    upsertPermissionTemplateMutation.error ??
    createRoleTemplateMutation.error ??
    updateRoleTemplateMutation.error ??
    createRoleAssignmentMutation.error ??
    updateRoleAssignmentStatusMutation.error ??
    null
  const certificationMutationError =
    grantCertificationMutation.error ?? updateCertificationMutation.error ?? null
  const readinessOverrideMutationError =
    grantReadinessOverrideMutation.error ?? clearReadinessOverrideMutation.error ?? null
  const incidentMutationError =
    createIncidentMutation.error ?? routeIncidentToTrainarrMutation.error ?? null
  const noteMutationError = createNoteMutation.error ?? null
  const documentMutationError = uploadDocumentMutation.error ?? null
  const personProfileMutationError =
    updatePersonMutation.error ??
    updateEmploymentStatusMutation.error ??
    null
  const offboardingMutationError =
    startOffboardingMutation.error ?? executeOffboardingMutation.error ?? null

  return {
    handoffRedirect,
    ready: Boolean(session && meQuery.data),
    loadingMessage: 'Loading workforce workspace…',
    me: meQuery.data!,
    session: session!,
    accessToken,
    apiError,
    searchParams,
    selectedPersonId,
    setSelectedPersonId,
    activeDirectoryPersonId,
    setActiveDirectoryPersonId,
    peopleDirectoryQuery,
    setPeopleDirectoryQuery,
    selectedSubordinateId,
    setSelectedSubordinateId,
    selectedIncidentId,
    setSelectedIncidentId,
    selectedNoteId,
    setSelectedNoteId,
    selectedDocumentId,
    setSelectedDocumentId,
    queryClient,
    meQuery,
    peopleQuery,
    orgUnitsQuery,
    fallbackPersonId,
    effectivePersonId,
    personProfileQuery,
    assignmentQuery,
    managerChainQuery,
    subordinatesQuery,
    subordinateDetailQuery,
    permissionTemplatesQuery,
    roleTemplatesQuery,
    roleAssignmentsQuery,
    effectivePermissionsQuery,
    permissionHistoryQuery,
    personTimelineQuery,
    personHistorySummaryQuery,
    trainarrTrainingHistoryQuery,
    workforceOnboardingJourneyQuery,
    personOffboardingQuery,
    certificationDefinitionsQuery,
    personCertificationsQuery,
    personReadinessQuery,
    personLookupQuery,
    canViewReadinessRollupSummaries,
    readinessRollupSiteFilterId,
    setReadinessRollupSiteFilterId,
    selectedReadinessRollup,
    setSelectedReadinessRollup,
    readinessRollupMemberFilter,
    setReadinessRollupMemberFilter,
    teamReadinessRollupsQuery,
    siteReadinessRollupsQuery,
    readinessRollupMembersQuery,
    personIncidentsQuery,
    incidentDetailQuery,
    createOrgUnitMutation,
    updateOrgUnitMutation,
    updateOrgUnitStatusMutation,
    createAssignmentMutation,
    updateAssignmentMutation,
    updateAssignmentStatusMutation,
    updateManagerMutation,
    upsertPermissionTemplateMutation,
    createRoleTemplateMutation,
    updateRoleTemplateMutation,
    createRoleAssignmentMutation,
    updateRoleAssignmentStatusMutation,
    grantCertificationMutation,
    updateCertificationMutation,
    createPersonMutation,
    updatePersonMutation,
    updateEmploymentStatusMutation,
    startOffboardingMutation,
    executeOffboardingMutation,
    grantReadinessOverrideMutation,
    clearReadinessOverrideMutation,
    createIncidentMutation,
    routeIncidentToTrainarrMutation,
    createNoteMutation,
    uploadDocumentMutation,
    personNotesQuery,
    noteDetailQuery,
    personDocumentsQuery,
    documentDetailQuery,
    people,
    filteredPeople,
    orgUnits,
    profile,
    selectedPerson,
    selectedPersonHiddenByFilter,
    assignments,
    managerChain,
    subordinates,
    selectedSubordinateDetail,
    permissionTemplates,
    roleTemplates,
    roleAssignments,
    effectivePermissions,
    permissionHistory,
    personTimelineEntries,
    personTimelineTotalCount,
    personTimelineHasNextPage,
    personTimelinePage,
    setPersonTimelinePage,
    personTimelinePageSize,
    setPersonTimelinePageSize,
    personTimelineCategoryFilter,
    setPersonTimelineCategoryFilter,
    certificationDefinitions,
    personCertifications,
    canManageOrgUnits,
    canManageHierarchy,
    canOverridePersonReadiness,
    canManagePersonIncidents,
    canManagePersonNotes,
    canManagePersonDocuments,
    canManagePeopleProfiles,
    canExportAudit,
    canReadReports,
    personIncidents,
    personNotes: personNotesQuery.data ?? [],
    personDocuments: personDocumentsQuery.data ?? [],
    orgMutationError,
    assignmentMutationError,
    managerMutationError,
    roleTemplateMutationError,
    certificationMutationError,
    readinessOverrideMutationError,
    incidentMutationError,
    noteMutationError,
    documentMutationError,
    personProfileMutationError,
    offboardingMutationError,
    personnelDocumentContentUrl,
  }
}

export type StaffArrWorkspaceState = ReturnType<typeof useStaffArrWorkspaceState>
