import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useEffect, useState } from 'react'
import { Navigate, useSearchParams } from 'react-router-dom'
import { ApiErrorCallout, PageHeader, getErrorMessage } from '@stl/shared-ui'
import {
  createPersonOrgAssignment,
  createPerson,
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
  getPersonHistorySummary,
  getPersonLookup,
  getPersonTrainarrTrainingHistory,
  getPersonTimeline,
  getWorkforceOnboardingJourney,
  getPersonCertifications,
  getPersonOffboarding,
  clearPersonReadinessOverride,
  createPersonnelIncident,
  createPersonnelDocument,
  createPersonnelNote,
  executePersonOffboarding,
  routePersonnelIncidentToTrainarr,
  getPersonnelIncident,
  getPersonnelDocument,
  getPersonnelNote,
  getPersonReadiness,
  getReadinessRollupMembers,
  getSiteReadinessRollups,
  getTeamReadinessRollups,
  grantPersonReadinessOverride,
  listPersonnelIncidents,
  listPersonnelDocuments,
  listPersonnelNotes,
  personnelDocumentContentUrl,
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
  StaffArrApiError,
  updatePersonManager,
  updatePersonOrgAssignment,
  updatePersonOrgAssignmentStatus,
  updatePersonRoleAssignmentStatus,
  updatePersonCertification,
  updateRoleTemplate,
  startPersonOffboarding,
  upsertPermissionTemplate,
  updateOrgUnit,
  updateOrgUnitStatus,
} from '../api/client'
import { clearSession, loadSession, canExportAuditPackage, canReadReports } from '../auth/sessionStorage'
import { filterPeopleDirectory } from '../lib/peopleDirectoryFilter'
import { CertificationPanel } from '../components/CertificationPanel'
import { AuditPackageExportPanel } from '../components/AuditPackageExportPanel'
import { CreatePersonPanel } from '../components/CreatePersonPanel'
import { PersonBulkImportPanel } from '../components/PersonBulkImportPanel'
import { PersonExportPanel } from '../components/PersonExportPanel'
import { canManagePeople, PersonProfileEditorPanel } from '../components/PersonProfileEditorPanel'
import { canManageIncidents, IncidentsPanel } from '../components/IncidentsPanel'
import { canOverrideReadiness, ReadinessPanel } from '../components/ReadinessPanel'
import {
  canViewReadinessRollups,
  ReadinessRollupSupervisorPanel,
} from '../components/ReadinessRollupSupervisorPanel'
import { ManagerHierarchyPanel } from '../components/ManagerHierarchyPanel'
import { canManageOrgHierarchy, OrgHierarchyManager } from '../components/OrgHierarchyManager'
import { PersonOrgAssignmentsManager } from '../components/PersonOrgAssignmentsManager'
import { PermissionProjectionTimelinePanel } from '../components/PermissionProjectionTimelinePanel'
import { PersonHistorySummaryPanel } from '../components/PersonHistorySummaryPanel'
import { PersonLookupPanel } from '../components/PersonLookupPanel'
import { PersonOffboardingPanel } from '../components/PersonOffboardingPanel'
import { canManagePersonnelDocuments, PersonnelDocumentsPanel } from '../components/PersonnelDocumentsPanel'
import { canManagePersonnelNotes, PersonnelNotesPanel } from '../components/PersonnelNotesPanel'
import { PersonTrainarrTrainingHistoryPanel } from '../components/PersonTrainarrTrainingHistoryPanel'
import { PersonTimelinePanel } from '../components/PersonTimelinePanel'
import type { PersonTimelineCategoryFilter } from '../components/PersonTimelinePanel'
import { RoleTemplateAssignmentPanel } from '../components/RoleTemplateAssignmentPanel'
import { WorkforceOnboardingJourneyPanel } from '../components/WorkforceOnboardingJourneyPanel'
import type { ReadinessRollupSelection } from '../api/types'

export function HomePage() {
  const [searchParams] = useSearchParams()
  const handoff = searchParams.get('handoff')
  if (handoff) {
    return <Navigate to={`/launch?handoff=${encodeURIComponent(handoff)}`} replace />
  }

  const session = loadSession()
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

  if (!session) {
    return <p className="text-sm text-slate-400">Loading workspace data…</p>
  }

  if (meQuery.isError) {
    if (meQuery.error instanceof StaffArrApiError && (meQuery.error.status === 401 || meQuery.error.status === 403)) {
      clearSession()
    }

    return (
      <div className="rounded-xl border border-red-800/60 bg-red-950/30 p-6">
        <ApiErrorCallout
          title="Session data unavailable"
          message={getErrorMessage(meQuery.error, 'Failed to load current user context.')}
          onRetry={() => void meQuery.refetch()}
          retryLabel="Retry session"
        />
      </div>
    )
  }

  if (!meQuery.data) {
    return <p className="text-sm text-slate-400">Loading workspace data…</p>
  }

  if (peopleQuery.isError || orgUnitsQuery.isError) {
    const directoryError =
      peopleQuery.error ??
      orgUnitsQuery.error
    if (directoryError instanceof StaffArrApiError && (directoryError.status === 401 || directoryError.status === 403)) {
      clearSession()
    }

    return (
      <div className="rounded-xl border border-red-800/60 bg-red-950/30 p-6">
        <ApiErrorCallout
          title="People directory unavailable"
          message={getErrorMessage(directoryError, 'Could not load people directory data.')}
          onRetry={() => {
            void peopleQuery.refetch()
            void orgUnitsQuery.refetch()
          }}
          retryLabel="Retry directory data"
        />
      </div>
    )
  }

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
  const canManageOrgUnits = canManageOrgHierarchy(me.tenantRoleKey, me.isPlatformAdmin)
  const canManageHierarchy = canManageOrgUnits
  const canOverridePersonReadiness = canOverrideReadiness(me.tenantRoleKey, me.isPlatformAdmin)
  const canManagePersonIncidents = canManageIncidents(me.tenantRoleKey, me.isPlatformAdmin)
  const canManagePersonNotes = canManagePersonnelNotes(me.tenantRoleKey, me.isPlatformAdmin)
  const canManagePersonDocuments = canManagePersonnelDocuments(me.tenantRoleKey, me.isPlatformAdmin)
  const canManagePeopleProfiles = canManagePeople(me.tenantRoleKey, me.isPlatformAdmin)
  const canExportAudit = canExportAuditPackage(me.tenantRoleKey, me.isPlatformAdmin)
  const canReadAudit = canReadReports(me.tenantRoleKey, me.isPlatformAdmin)
  const personIncidents = personIncidentsQuery.data ?? []
  const personNotes = personNotesQuery.data ?? []
  const personDocuments = personDocumentsQuery.data ?? []
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
  const offboardingMutationError =
    startOffboardingMutation.error ?? executeOffboardingMutation.error ?? null
  const personProfileMutationError =
    updatePersonMutation.error ?? updateEmploymentStatusMutation.error ?? null

  return (
    <div className="mx-auto max-w-6xl space-y-6">
      <PageHeader
        title="People directory"
        subtitle="Profiles, org structure, permissions, readiness, and incidents"
      />

      {canViewReadinessRollupSummaries ? (
        <ReadinessRollupSupervisorPanel
          teamRollups={teamReadinessRollupsQuery.data ?? []}
          siteRollups={siteReadinessRollupsQuery.data ?? []}
          siteFilterOrgUnitId={readinessRollupSiteFilterId}
          onSiteFilterChange={setReadinessRollupSiteFilterId}
          memberReadinessFilter={readinessRollupMemberFilter}
          onMemberReadinessFilterChange={setReadinessRollupMemberFilter}
          selectedRollup={selectedReadinessRollup}
          onSelectRollup={setSelectedReadinessRollup}
          rollupMembers={readinessRollupMembersQuery.data ?? null}
          rollupMembersLoading={readinessRollupMembersQuery.isLoading}
          rollupMembersReadErrorMessage={
            readinessRollupMembersQuery.isError
              ? getErrorMessage(
                  readinessRollupMembersQuery.error,
                  'Failed to load readiness rollup members.',
                )
              : null
          }
          onRetryRollupMembersRead={() => void readinessRollupMembersQuery.refetch()}
          onSelectPerson={setSelectedPersonId}
          isLoading={teamReadinessRollupsQuery.isLoading || siteReadinessRollupsQuery.isLoading}
          readErrorMessage={
            teamReadinessRollupsQuery.isError
              ? getErrorMessage(
                  teamReadinessRollupsQuery.error,
                  'Failed to load team readiness rollups.',
                )
              : siteReadinessRollupsQuery.isError
                ? getErrorMessage(
                    siteReadinessRollupsQuery.error,
                    'Failed to load site readiness rollups.',
                  )
                : null
          }
          onRetryRead={() => {
            void teamReadinessRollupsQuery.refetch()
            void siteReadinessRollupsQuery.refetch()
          }}
        />
      ) : null}

      <section className="mt-8 grid gap-6 lg:grid-cols-3">
        <div className="rounded-xl border border-slate-700 bg-slate-900/60 p-6">
          <h2 className="text-sm font-medium text-slate-300">Session context</h2>
          <dl className="mt-4 grid gap-3 text-sm">
            <div className="flex justify-between gap-4">
              <dt className="text-slate-500">Signed in</dt>
              <dd className="text-right text-white">{me.displayName}</dd>
            </div>
            <div className="flex justify-between gap-4">
              <dt className="text-slate-500">Role</dt>
              <dd className="text-right text-sky-300">{me.tenantRoleKey || 'tenant_member'}</dd>
            </div>
            <div className="flex justify-between gap-4">
              <dt className="text-slate-500">Org unit</dt>
              <dd className="text-right text-slate-200">{me.primaryOrgUnitName ?? 'Unassigned'}</dd>
            </div>
            <div className="flex justify-between gap-4">
              <dt className="text-slate-500">Job title</dt>
              <dd className="text-right text-slate-200">{me.jobTitle ?? 'Unspecified'}</dd>
            </div>
            <div className="flex justify-between gap-4">
              <dt className="text-slate-500">Person ID</dt>
              <dd className="text-right font-mono text-xs text-slate-300">{me.personId}</dd>
            </div>
          </dl>
        </div>

        <div className="rounded-xl border border-slate-700 bg-slate-900/60 p-6 lg:col-span-2">
          <h2 className="text-sm font-medium text-slate-300">People directory</h2>
          <div className="mt-3 space-y-2">
            <label className="block text-xs font-medium uppercase tracking-wide text-slate-400" htmlFor="directory-filter">
              Quick filter
            </label>
            <div className="flex items-center gap-2">
              <input
                id="directory-filter"
                type="search"
                aria-label="People quick filter"
                data-testid="people-directory-filter"
                value={peopleDirectoryQuery}
                onChange={(event) => setPeopleDirectoryQuery(event.target.value)}
                onKeyDown={(event) => {
                  if (event.key === 'Escape' && peopleDirectoryQuery) {
                    event.preventDefault()
                    setPeopleDirectoryQuery('')
                    return
                  }
                  if ((event.key === 'ArrowDown' || event.key === 'ArrowUp') && peopleDirectoryQuery.trim() && filteredPeople.length > 0) {
                    event.preventDefault()
                    const anchorId = activeDirectoryPersonId ?? selectedPerson?.personId ?? filteredPeople[0]!.personId
                    const currentIndex = filteredPeople.findIndex((person) => person.personId === anchorId)
                    const startIndex = currentIndex >= 0 ? currentIndex : 0
                    const nextIndex =
                      event.key === 'ArrowDown'
                        ? (startIndex + 1) % filteredPeople.length
                        : (startIndex - 1 + filteredPeople.length) % filteredPeople.length
                    setActiveDirectoryPersonId(filteredPeople[nextIndex]!.personId)
                    return
                  }
                  if (event.key === 'Enter' && peopleDirectoryQuery.trim() && filteredPeople.length > 0) {
                    event.preventDefault()
                    setSelectedPersonId(activeDirectoryPersonId ?? filteredPeople[0]!.personId)
                  }
                }}
                placeholder="Search by name, email, title, org unit, or status"
                className="w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-white placeholder:text-slate-500 focus:border-sky-500 focus:outline-none"
              />
              {peopleDirectoryQuery ? (
                <button
                  type="button"
                  onClick={() => setPeopleDirectoryQuery('')}
                  className="rounded-md border border-slate-700 px-3 py-2 text-xs text-slate-300 hover:border-slate-500 hover:text-white"
                >
                  Clear
                </button>
              ) : null}
            </div>
            {!peopleQuery.isLoading && people.length > 0 ? (
              <p className="text-xs text-slate-500" aria-live="polite">
                Showing {filteredPeople.length} of {people.length} people
              </p>
            ) : null}
            {!peopleQuery.isLoading && peopleDirectoryQuery.trim() && filteredPeople.length > 0 ? (
              <p className="text-xs text-slate-500">Use ↑/↓ to move through results, then press Enter to select.</p>
            ) : null}
            {selectedPersonHiddenByFilter ? (
              <div className="rounded-md border border-amber-700/60 bg-amber-950/20 p-2 text-xs text-amber-200">
                The selected person is hidden by the current filter.
                <button
                  type="button"
                  onClick={() => setPeopleDirectoryQuery('')}
                  className="ml-2 underline decoration-amber-400/70 underline-offset-2 hover:text-amber-100"
                >
                  Clear filter to show selection
                </button>
              </div>
            ) : null}
          </div>
          {peopleQuery.isLoading ? (
            <p className="mt-4 text-sm text-slate-400">Loading people…</p>
          ) : people.length === 0 ? (
            <p className="mt-4 text-sm text-slate-400">No people have been added yet for this tenant.</p>
          ) : filteredPeople.length === 0 ? (
            <p className="mt-4 text-sm text-slate-400" aria-live="polite">
              No people match the current filter. Try a different name, email, or status.
            </p>
          ) : (
            <ul className="mt-4 divide-y divide-slate-700">
              {filteredPeople.map((person) => {
                const isSelected = effectivePersonId === person.personId
                const isActive = Boolean(peopleDirectoryQuery.trim()) && activeDirectoryPersonId === person.personId
                const buttonClass = isSelected
                  ? 'w-full rounded-md px-1 py-1 text-left text-sky-200'
                  : isActive
                    ? 'w-full rounded-md px-1 py-1 text-left text-slate-100 ring-1 ring-sky-500/70'
                    : 'w-full rounded-md px-1 py-1 text-left'

                return (
                <li key={person.personId} className="flex items-center justify-between py-3">
                  <button
                    type="button"
                    onMouseEnter={() => setActiveDirectoryPersonId(person.personId)}
                    onClick={() => {
                      setActiveDirectoryPersonId(person.personId)
                      setSelectedPersonId(person.personId)
                    }}
                    className={buttonClass}
                  >
                    <p className="text-sm text-white">{person.displayName}</p>
                    <p className="text-xs text-slate-400">
                      {person.jobTitle ?? 'No title'} · {person.primaryEmail}
                    </p>
                  </button>
                  <span className="text-xs uppercase tracking-wide text-slate-500">{person.employmentStatus}</span>
                </li>
                )
              })}
            </ul>
          )}
        </div>
      </section>

      <CreatePersonPanel
        orgUnits={orgUnits}
        peopleOptions={people.map((person) => ({
          personId: person.personId,
          displayName: person.displayName,
        }))}
        canManage={canManagePeopleProfiles}
        isSubmitting={createPersonMutation.isPending}
        errorMessage={
          createPersonMutation.error
            ? getErrorMessage(createPersonMutation.error, 'Failed to create person profile.')
            : null
        }
        onCreate={async (request) => {
          await createPersonMutation.mutateAsync(request)
        }}
      />

      <section className="mt-6 rounded-xl border border-slate-700 bg-slate-900/60 p-6">
        <h2 className="text-sm font-medium text-slate-300">Selected profile</h2>
        {personProfileQuery.isLoading ? (
          <p className="mt-4 text-sm text-slate-400">Loading selected profile…</p>
        ) : !profile ? (
          <p className="mt-4 text-sm text-slate-400">No profile selected.</p>
        ) : (
          <dl className="mt-4 grid gap-3 text-sm md:grid-cols-2">
            <div className="flex justify-between gap-4">
              <dt className="text-slate-500">Name</dt>
              <dd className="text-right text-white">{profile.displayName}</dd>
            </div>
            <div className="flex justify-between gap-4">
              <dt className="text-slate-500">Email</dt>
              <dd className="text-right text-white">{profile.primaryEmail}</dd>
            </div>
            <div className="flex justify-between gap-4">
              <dt className="text-slate-500">Org unit</dt>
              <dd className="text-right text-white">{profile.primaryOrgUnitName ?? 'Unassigned'}</dd>
            </div>
            <div className="flex justify-between gap-4">
              <dt className="text-slate-500">Manager</dt>
              <dd className="text-right font-mono text-xs text-slate-300">{profile.managerPersonId ?? 'None'}</dd>
            </div>
            <div className="flex justify-between gap-4">
              <dt className="text-slate-500">Created</dt>
              <dd className="text-right text-slate-200">{new Date(profile.createdAt).toLocaleDateString()}</dd>
            </div>
            <div className="flex justify-between gap-4">
              <dt className="text-slate-500">Updated</dt>
              <dd className="text-right text-slate-200">{new Date(profile.updatedAt).toLocaleDateString()}</dd>
            </div>
          </dl>
        )}
      </section>

      {profile ? (
        <PersonProfileEditorPanel
          profile={profile}
          orgUnits={orgUnits}
          peopleOptions={people.map((person) => ({
            personId: person.personId,
            displayName: person.displayName,
          }))}
          canManage={canManagePeopleProfiles}
          isSubmitting={updatePersonMutation.isPending || updateEmploymentStatusMutation.isPending}
          errorMessage={
            personProfileMutationError
              ? getErrorMessage(personProfileMutationError, 'Failed to update person profile.')
              : null
          }
          onUpdate={async (request) => {
            await updatePersonMutation.mutateAsync({
              personId: profile.personId,
              ...request,
            })
          }}
          onEmploymentStatusChange={async (request) => {
            await updateEmploymentStatusMutation.mutateAsync({
              personId: profile.personId,
              ...request,
            })
          }}
        />
      ) : null}

      {selectedPerson ? (
        <PersonOrgAssignmentsManager
          personId={selectedPerson.personId}
          personDisplayName={selectedPerson.displayName}
          orgUnits={orgUnits}
          assignments={assignments}
          isLoading={assignmentQuery.isLoading || orgUnitsQuery.isLoading}
          isError={assignmentQuery.isError || orgUnitsQuery.isError}
          readErrorMessage={
            assignmentQuery.isError
              ? getErrorMessage(
                  assignmentQuery.error,
                  'Failed to load person org assignments.',
                )
              : orgUnitsQuery.isError
                ? getErrorMessage(
                    (orgUnitsQuery as { error: unknown }).error,
                    'Failed to load org unit options.',
                  )
                : null
          }
          onRetryRead={() => {
            void assignmentQuery.refetch()
            void orgUnitsQuery.refetch()
          }}
          canManage={canManageOrgUnits}
          isSubmitting={
            createAssignmentMutation.isPending || updateAssignmentMutation.isPending || updateAssignmentStatusMutation.isPending
          }
          actionErrorMessage={
            assignmentMutationError ? getErrorMessage(assignmentMutationError, 'Failed to save org assignments.') : null
          }
          onCreate={async (payload) => {
            await createAssignmentMutation.mutateAsync({ personId: selectedPerson.personId, ...payload })
          }}
          onUpdate={async (assignmentId, payload) => {
            await updateAssignmentMutation.mutateAsync({
              personId: selectedPerson.personId,
              assignmentId,
              ...payload,
            })
          }}
          onStatusChange={async (assignmentId, status) => {
            await updateAssignmentStatusMutation.mutateAsync({
              personId: selectedPerson.personId,
              assignmentId,
              status,
            })
          }}
        />
      ) : null}

      {selectedPerson ? (
        <ManagerHierarchyPanel
          selectedPersonId={selectedPerson.personId}
          selectedPersonDisplayName={selectedPerson.displayName}
          people={people}
          managerChain={managerChain}
          subordinates={subordinates}
          selectedSubordinateId={selectedSubordinateId}
          selectedSubordinate={selectedSubordinateDetail}
          isLoading={managerChainQuery.isLoading || subordinatesQuery.isLoading}
          isError={managerChainQuery.isError || subordinatesQuery.isError}
          readErrorMessage={
            managerChainQuery.isError
              ? getErrorMessage(
                  managerChainQuery.error,
                  'Failed to load manager chain.',
                )
              : subordinatesQuery.isError
                ? getErrorMessage(
                    subordinatesQuery.error,
                    'Failed to load subordinate hierarchy.',
                  )
                : null
          }
          onRetryRead={() => {
            void managerChainQuery.refetch()
            void subordinatesQuery.refetch()
          }}
          isLoadingSubordinateDetail={subordinateDetailQuery.isLoading}
          isSubordinateDetailError={subordinateDetailQuery.isError}
          subordinateDetailErrorMessage={
            subordinateDetailQuery.isError
              ? getErrorMessage(
                  subordinateDetailQuery.error,
                  'Failed to load subordinate detail.',
                )
              : null
          }
          onRetrySubordinateDetail={() => void subordinateDetailQuery.refetch()}
          canManage={canManageHierarchy}
          isSubmitting={updateManagerMutation.isPending}
          actionErrorMessage={
            managerMutationError ? getErrorMessage(managerMutationError, 'Failed to update manager hierarchy.') : null
          }
          onSelectSubordinate={(subordinatePersonId) => setSelectedSubordinateId(subordinatePersonId)}
          onUpdateManager={async (managerPersonId) => {
            await updateManagerMutation.mutateAsync({
              personId: selectedPerson.personId,
              managerPersonId,
            })
          }}
        />
      ) : null}

      {selectedPerson ? (
        <RoleTemplateAssignmentPanel
          personId={selectedPerson.personId}
          personDisplayName={selectedPerson.displayName}
          orgUnits={orgUnits}
          permissionTemplates={permissionTemplates}
          roleTemplates={roleTemplates}
          roleAssignments={roleAssignments}
          isLoading={
            permissionTemplatesQuery.isLoading
            || roleTemplatesQuery.isLoading
            || roleAssignmentsQuery.isLoading
          }
          isError={
            permissionTemplatesQuery.isError
            || roleTemplatesQuery.isError
            || roleAssignmentsQuery.isError
          }
          readErrorMessage={
            permissionTemplatesQuery.isError
              ? getErrorMessage(
                  permissionTemplatesQuery.error,
                  'Failed to load permission templates.',
                )
              : roleTemplatesQuery.isError
                ? getErrorMessage(
                    roleTemplatesQuery.error,
                    'Failed to load role templates.',
                  )
                : roleAssignmentsQuery.isError
                  ? getErrorMessage(
                      roleAssignmentsQuery.error,
                      'Failed to load role assignments.',
                    )
                  : null
          }
          onRetryRead={() => {
            void permissionTemplatesQuery.refetch()
            void roleTemplatesQuery.refetch()
            void roleAssignmentsQuery.refetch()
          }}
          canManage={canManageHierarchy}
          isSubmitting={
            upsertPermissionTemplateMutation.isPending ||
            createRoleTemplateMutation.isPending ||
            updateRoleTemplateMutation.isPending ||
            createRoleAssignmentMutation.isPending ||
            updateRoleAssignmentStatusMutation.isPending
          }
          actionErrorMessage={
            roleTemplateMutationError
              ? getErrorMessage(roleTemplateMutationError, 'Failed to update role templates or assignments.')
              : null
          }
          onUpsertPermissionTemplate={async (payload) => {
            await upsertPermissionTemplateMutation.mutateAsync(payload)
          }}
          onCreateRoleTemplate={async (payload) => {
            await createRoleTemplateMutation.mutateAsync(payload)
          }}
          onUpdateRoleTemplateStatus={async (roleTemplateId, status) => {
            const existing = roleTemplates.find((role) => role.roleTemplateId === roleTemplateId)
            if (!existing) {
              return
            }

            await updateRoleTemplateMutation.mutateAsync({
              roleTemplateId,
              name: existing.name,
              description: existing.description,
              status,
              permissions: existing.permissions.map((mapping) => ({
                permissionTemplateId: mapping.permissionTemplateId,
                scopeType: mapping.scopeType,
                scopeValue: mapping.scopeValue,
              })),
            })
          }}
          onCreateRoleAssignment={async (payload) => {
            await createRoleAssignmentMutation.mutateAsync({
              personId: selectedPerson.personId,
              ...payload,
            })
          }}
          onUpdateRoleAssignmentStatus={async (assignmentId, status) => {
            await updateRoleAssignmentStatusMutation.mutateAsync({
              personId: selectedPerson.personId,
              assignmentId,
              status,
            })
          }}
        />
      ) : null}

      {selectedPerson ? (
        <PermissionProjectionTimelinePanel
          personDisplayName={selectedPerson.displayName}
          orgUnits={orgUnits}
          projection={effectivePermissions}
          timeline={permissionHistory}
          isLoading={effectivePermissionsQuery.isLoading || permissionHistoryQuery.isLoading}
          isError={effectivePermissionsQuery.isError || permissionHistoryQuery.isError}
          readErrorMessage={
            effectivePermissionsQuery.isError
              ? getErrorMessage(
                  effectivePermissionsQuery.error,
                  'Failed to load effective permission projection.',
                )
              : permissionHistoryQuery.isError
                ? getErrorMessage(
                    permissionHistoryQuery.error,
                    'Failed to load permission history timeline.',
                  )
                : null
          }
          onRetryRead={() => {
            void effectivePermissionsQuery.refetch()
            void permissionHistoryQuery.refetch()
          }}
        />
      ) : null}

      {selectedPerson ? (
        <ReadinessPanel
          personId={selectedPerson.personId}
          personDisplayName={selectedPerson.displayName}
          readiness={personReadinessQuery.data ?? null}
          isLoading={personReadinessQuery.isLoading}
          isError={personReadinessQuery.isError}
          readErrorMessage={
            personReadinessQuery.isError
              ? getErrorMessage(
                  personReadinessQuery.error,
                  'Failed to load readiness status for this person.',
                )
              : null
          }
          onRetryRead={() => void personReadinessQuery.refetch()}
          canOverride={canOverridePersonReadiness}
          isSubmittingOverride={
            grantReadinessOverrideMutation.isPending || clearReadinessOverrideMutation.isPending
          }
          overrideErrorMessage={
            readinessOverrideMutationError
              ? getErrorMessage(readinessOverrideMutationError, 'Failed to update readiness override.')
              : null
          }
          onGrantOverride={async (payload) => {
            await grantReadinessOverrideMutation.mutateAsync({
              personId: selectedPerson.personId,
              ...payload,
            })
          }}
          onClearOverride={async () => {
            await clearReadinessOverrideMutation.mutateAsync(selectedPerson.personId)
          }}
        />
      ) : null}

      {selectedPerson ? (
        <IncidentsPanel
          personId={selectedPerson.personId}
          personDisplayName={selectedPerson.displayName}
          incidents={personIncidents}
          selectedIncidentId={selectedIncidentId}
          selectedIncident={incidentDetailQuery.data ?? null}
          isLoading={personIncidentsQuery.isLoading}
          isError={personIncidentsQuery.isError}
          readErrorMessage={
            personIncidentsQuery.isError
              ? getErrorMessage(
                  personIncidentsQuery.error,
                  'Failed to load personnel incidents.',
                )
              : null
          }
          onRetryRead={() => void personIncidentsQuery.refetch()}
          isLoadingDetail={incidentDetailQuery.isLoading}
          isDetailError={incidentDetailQuery.isError}
          detailErrorMessage={
            incidentDetailQuery.isError
              ? getErrorMessage(
                  incidentDetailQuery.error,
                  'Failed to load incident detail.',
                )
              : null
          }
          onRetryDetail={() => void incidentDetailQuery.refetch()}
          canManage={canManagePersonIncidents}
          isSubmitting={createIncidentMutation.isPending}
          isRouting={routeIncidentToTrainarrMutation.isPending}
          actionErrorMessage={
            incidentMutationError
              ? getErrorMessage(incidentMutationError, 'Failed to save personnel incident changes.')
              : null
          }
          onSelectIncident={setSelectedIncidentId}
          onCreateIncident={async (payload) => {
            await createIncidentMutation.mutateAsync(payload)
          }}
          onRouteToTrainarr={async (incidentId) => {
            await routeIncidentToTrainarrMutation.mutateAsync(incidentId)
          }}
        />
      ) : null}

      {selectedPerson ? (
        <PersonnelNotesPanel
          personId={selectedPerson.personId}
          personDisplayName={selectedPerson.displayName}
          notes={personNotes}
          selectedNoteId={selectedNoteId}
          selectedNote={noteDetailQuery.data ?? null}
          isLoading={personNotesQuery.isLoading}
          isError={personNotesQuery.isError}
          readErrorMessage={
            personNotesQuery.isError
              ? getErrorMessage(
                  personNotesQuery.error,
                  'Failed to load personnel notes.',
                )
              : null
          }
          onRetryRead={() => void personNotesQuery.refetch()}
          isLoadingDetail={noteDetailQuery.isLoading}
          isDetailError={noteDetailQuery.isError}
          detailErrorMessage={
            noteDetailQuery.isError
              ? getErrorMessage(
                  noteDetailQuery.error,
                  'Failed to load note detail.',
                )
              : null
          }
          onRetryDetail={() => void noteDetailQuery.refetch()}
          canManage={canManagePersonNotes}
          isSubmitting={createNoteMutation.isPending}
          actionErrorMessage={
            noteMutationError ? getErrorMessage(noteMutationError, 'Failed to save personnel note.') : null
          }
          onSelectNote={setSelectedNoteId}
          onCreateNote={async (payload) => {
            await createNoteMutation.mutateAsync(payload)
          }}
        />
      ) : null}

      {selectedPerson ? (
        <PersonnelDocumentsPanel
          personId={selectedPerson.personId}
          personDisplayName={selectedPerson.displayName}
          accessToken={session!.accessToken}
          documents={personDocuments}
          selectedDocumentId={selectedDocumentId}
          selectedDocument={documentDetailQuery.data ?? null}
          isLoading={personDocumentsQuery.isLoading}
          isError={personDocumentsQuery.isError}
          readErrorMessage={
            personDocumentsQuery.isError
              ? getErrorMessage(
                  personDocumentsQuery.error,
                  'Failed to load personnel documents.',
                )
              : null
          }
          onRetryRead={() => void personDocumentsQuery.refetch()}
          isLoadingDetail={documentDetailQuery.isLoading}
          isDetailError={documentDetailQuery.isError}
          detailErrorMessage={
            documentDetailQuery.isError
              ? getErrorMessage(
                  documentDetailQuery.error,
                  'Failed to load document detail.',
                )
              : null
          }
          onRetryDetail={() => void documentDetailQuery.refetch()}
          canManage={canManagePersonDocuments}
          isSubmitting={uploadDocumentMutation.isPending}
          actionErrorMessage={
            documentMutationError ? getErrorMessage(documentMutationError, 'Failed to upload personnel document.') : null
          }
          onSelectDocument={setSelectedDocumentId}
          onUploadDocument={async (payload) => {
            await uploadDocumentMutation.mutateAsync(payload)
          }}
          contentUrlFor={(documentId) =>
            personnelDocumentContentUrl(selectedPerson.personId, documentId)
          }
        />
      ) : null}

      {effectivePersonId && (selectedPerson ?? personProfileQuery.data) ? (
        <WorkforceOnboardingJourneyPanel
          accessToken={session!.accessToken}
          personDisplayName={selectedPerson?.displayName ?? personProfileQuery.data!.displayName}
          journey={workforceOnboardingJourneyQuery.data ?? null}
          isLoading={workforceOnboardingJourneyQuery.isLoading}
          isError={workforceOnboardingJourneyQuery.isError}
          readErrorMessage={
            workforceOnboardingJourneyQuery.isError
              ? getErrorMessage(
                  workforceOnboardingJourneyQuery.error,
                  'Failed to load workforce onboarding journey.',
                )
              : null
          }
          onRetryRead={() => void workforceOnboardingJourneyQuery.refetch()}
        />
      ) : null}

      {selectedPerson ? (
        <PersonOffboardingPanel
          personId={selectedPerson.personId}
          personDisplayName={selectedPerson.displayName}
          peopleOptions={people.map((person) => ({
            personId: person.personId,
            displayName: person.displayName,
          }))}
          offboarding={personOffboardingQuery.data ?? null}
          isLoading={personOffboardingQuery.isLoading}
          isError={personOffboardingQuery.isError}
          readErrorMessage={
            personOffboardingQuery.isError
              ? getErrorMessage(
                  personOffboardingQuery.error,
                  'Failed to load offboarding workflow state.',
                )
              : null
          }
          onRetryRead={() => void personOffboardingQuery.refetch()}
          canManage={canManagePeopleProfiles}
          isSubmitting={startOffboardingMutation.isPending || executeOffboardingMutation.isPending}
          actionErrorMessage={
            offboardingMutationError
              ? getErrorMessage(offboardingMutationError, 'Failed to update offboarding workflow.')
              : null
          }
          onStart={async (request) => {
            await startOffboardingMutation.mutateAsync({
              personId: selectedPerson.personId,
              ...request,
            })
          }}
          onExecute={async (request) => {
            const offboarding = personOffboardingQuery.data
            if (!offboarding) {
              return
            }

            await executeOffboardingMutation.mutateAsync({
              personId: selectedPerson.personId,
              offboardingId: offboarding.offboardingId,
              ...request,
            })
          }}
        />
      ) : null}

      {selectedPerson ? (
        <PersonLookupPanel
          personId={selectedPerson.personId}
          personDisplayName={selectedPerson.displayName}
          lookup={personLookupQuery.data ?? null}
          isLoading={personLookupQuery.isLoading}
          isError={personLookupQuery.isError}
          readErrorMessage={
            personLookupQuery.isError
              ? getErrorMessage(
                  personLookupQuery.error,
                  'Failed to load person identity and placement details.',
                )
              : null
          }
          onRetryRead={() => void personLookupQuery.refetch()}
        />
      ) : null}

      {selectedPerson ? (
        <PersonHistorySummaryPanel
          personDisplayName={selectedPerson.displayName}
          summary={personHistorySummaryQuery.data ?? null}
          isLoading={personHistorySummaryQuery.isLoading}
          isError={personHistorySummaryQuery.isError}
          readErrorMessage={
            personHistorySummaryQuery.isError
              ? getErrorMessage(
                  personHistorySummaryQuery.error,
                  'Failed to load personnel history summary.',
                )
              : null
          }
          onRetryRead={() => void personHistorySummaryQuery.refetch()}
        />
      ) : null}

      {selectedPerson ? (
        <PersonTimelinePanel
          personDisplayName={selectedPerson.displayName}
          entries={personTimelineEntries}
          totalCount={personTimelineTotalCount}
          page={personTimelinePage}
          pageSize={personTimelinePageSize}
          hasNextPage={personTimelineHasNextPage}
          categoryFilter={personTimelineCategoryFilter}
          isLoading={personTimelineQuery.isLoading}
          isError={personTimelineQuery.isError}
          readErrorMessage={
            personTimelineQuery.isError
              ? getErrorMessage(
                  personTimelineQuery.error,
                  'Failed to load person timeline events.',
                )
              : null
          }
          onRetryRead={() => void personTimelineQuery.refetch()}
          onCategoryFilterChange={setPersonTimelineCategoryFilter}
          onPageChange={setPersonTimelinePage}
          onPageSizeChange={setPersonTimelinePageSize}
        />
      ) : null}

      {selectedPerson ? (
        <PersonTrainarrTrainingHistoryPanel
          personDisplayName={selectedPerson.displayName}
          history={trainarrTrainingHistoryQuery.data ?? null}
          isLoading={trainarrTrainingHistoryQuery.isLoading}
          isError={trainarrTrainingHistoryQuery.isError}
          readErrorMessage={
            trainarrTrainingHistoryQuery.isError
              ? getErrorMessage(
                  trainarrTrainingHistoryQuery.error,
                  'Failed to load TrainArr training history.',
                )
              : null
          }
          onRetryRead={() => void trainarrTrainingHistoryQuery.refetch()}
        />
      ) : null}

      {selectedPerson ? (
        <CertificationPanel
          personId={selectedPerson.personId}
          personDisplayName={selectedPerson.displayName}
          definitions={certificationDefinitions}
          certifications={personCertifications}
          isLoading={certificationDefinitionsQuery.isLoading || personCertificationsQuery.isLoading}
          isError={certificationDefinitionsQuery.isError || personCertificationsQuery.isError}
          readErrorMessage={
            certificationDefinitionsQuery.isError
              ? getErrorMessage(
                  certificationDefinitionsQuery.error,
                  'Failed to load certification definitions.',
                )
              : personCertificationsQuery.isError
                ? getErrorMessage(
                    personCertificationsQuery.error,
                    'Failed to load person certifications.',
                  )
                : null
          }
          onRetryRead={() => {
            void certificationDefinitionsQuery.refetch()
            void personCertificationsQuery.refetch()
          }}
          canManage={canManageHierarchy}
          isSubmitting={grantCertificationMutation.isPending || updateCertificationMutation.isPending}
          actionErrorMessage={
            certificationMutationError
              ? getErrorMessage(certificationMutationError, 'Failed to update certification records.')
              : null
          }
          onGrantCertification={async (payload) => {
            await grantCertificationMutation.mutateAsync({
              personId: selectedPerson.personId,
              ...payload,
            })
          }}
          onUpdateCertification={async (personCertificationId, payload) => {
            await updateCertificationMutation.mutateAsync({
              personId: selectedPerson.personId,
              personCertificationId,
              ...payload,
            })
          }}
        />
      ) : null}

      <OrgHierarchyManager
        orgUnits={orgUnits}
        isLoading={orgUnitsQuery.isLoading}
        isError={orgUnitsQuery.isError}
        readErrorMessage={
          orgUnitsQuery.isError
            ? getErrorMessage(
                (orgUnitsQuery as { error: unknown }).error,
                'Failed to load org hierarchy data.',
              )
            : null
        }
        onRetryRead={() => void orgUnitsQuery.refetch()}
        canManage={canManageOrgUnits}
        isSubmitting={
          createOrgUnitMutation.isPending || updateOrgUnitMutation.isPending || updateOrgUnitStatusMutation.isPending
        }
        actionErrorMessage={
          orgMutationError ? getErrorMessage(orgMutationError, 'Failed to update org hierarchy.') : null
        }
        onCreate={async (payload) => {
          await createOrgUnitMutation.mutateAsync(payload)
        }}
        onUpdate={async (orgUnitId, payload) => {
          await updateOrgUnitMutation.mutateAsync({ orgUnitId, ...payload })
        }}
        onStatusChange={async (orgUnitId, status) => {
          await updateOrgUnitStatusMutation.mutateAsync({ orgUnitId, status })
        }}
      />

      <PersonBulkImportPanel
        accessToken={session!.accessToken}
        canImport={canManagePeopleProfiles}
        onComplete={() => {
          void queryClient.invalidateQueries({ queryKey: ['staffarr-people', session?.accessToken] })
        }}
      />

      <PersonExportPanel accessToken={session!.accessToken} canExport={canManagePeopleProfiles} />

      <AuditPackageExportPanel
        accessToken={session!.accessToken}
        canRead={canReadAudit}
        canExport={canExportAudit}
      />

      <section className="mt-6 rounded-xl border border-slate-700 bg-slate-900/60 p-6">
        <h2 className="text-sm font-medium text-slate-300">Signed in</h2>
        <dl className="mt-4 grid gap-3 text-sm">
          <div className="flex justify-between gap-4">
            <dt className="text-slate-500">Name</dt>
            <dd className="text-right text-white">{me.displayName}</dd>
          </div>
          <div className="flex justify-between gap-4">
            <dt className="text-slate-500">Email</dt>
            <dd className="text-right text-white">{me.email}</dd>
          </div>
          <div className="flex justify-between gap-4">
            <dt className="text-slate-500">Tenant</dt>
            <dd className="text-right font-mono text-xs text-slate-300">{session.tenantSlug}</dd>
          </div>
          <div className="flex justify-between gap-4">
            <dt className="text-slate-500">Org units loaded</dt>
            <dd className="text-right text-white">{orgUnits.length}</dd>
          </div>
          <div className="flex justify-between gap-4">
            <dt className="text-slate-500">StaffArr entitlement</dt>
            <dd className="text-right text-emerald-400">
              {me.hasStaffArrEntitlement ? 'Active' : 'Missing'}
            </dd>
          </div>
        </dl>
      </section>
    </div>
  )
}
