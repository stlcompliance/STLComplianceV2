import { useEffect, useMemo, useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { ConfirmDialog, PageHeader, StaticSearchPicker, type PickerOption } from '@stl/shared-ui'
import {
  archiveRecruitingCandidate,
  archiveRecruitingInterviewStage,
  archiveRecruitingOffer,
  archiveRecruitingRequisition,
  convertEmploymentApplicationSubmissionToCandidate,
  createRecruitingRequisition,
  createRecruitingInterviewStage,
  createRecruitingOffer,
  getOrgUnits,
  getPeople,
  getStaffArrFieldset,
  hireRecruitingCandidate,
  listEmploymentApplicationSubmissions,
  listRecruitingCandidates,
  listRecruitingInterviewStages,
  listRecruitingOffers,
  listRecruitingRequisitions,
  updateRecruitingCandidate,
  updateRecruitingInterviewStage,
  updateRecruitingOffer,
  updateRecruitingRequisition,
} from '../../api/client'
import type {
  CreateStaffPersonRequest,
  EmploymentApplicationSubmissionListItemResponse,
  OrgUnitResponse,
  RecruitingCandidateResponse,
  StaffArrFieldsetResponse,
  StaffPersonSummaryResponse,
  UpsertRecruitingCandidateRequest,
  UpsertRecruitingRequisitionRequest,
  UpsertRecruitingInterviewStageRequest,
  UpsertRecruitingOfferRequest,
} from '../../api/types'
import { loadSession } from '../../auth/sessionStorage'

function emptyRequisitionDraft(): UpsertRecruitingRequisitionRequest {
  return {
    requisitionNumber: `REQ-${new Date().getFullYear()}-${Math.random().toString(36).slice(2, 6).toUpperCase()}`,
    title: 'New requisition',
    jobCode: 'JOB-000',
    jobFamily: 'General',
    departmentRef: null,
    siteRef: null,
    locationRef: null,
    hiringManagerPersonId: null,
    recruiterPersonId: null,
    status: 'open',
    headcountRequested: 1,
    filledCount: 0,
    openDate: new Date().toISOString().slice(0, 10),
    targetStartDate: null,
    sourceProductKey: 'staffarr.hiring',
    sourceRef: null,
  }
}

function emptyCandidateDraft(
  requisitionId: string | null,
  candidate?: RecruitingCandidateResponse | null,
): UpsertRecruitingCandidateRequest {
  return {
    recruitingRequisitionId: candidate?.recruitingRequisitionId ?? requisitionId,
    employmentApplicationSubmissionId: candidate?.employmentApplicationSubmissionId ?? null,
    personId: candidate?.personId ?? null,
    candidateName: candidate?.candidateName ?? '',
    candidateEmail: candidate?.candidateEmail ?? '',
    candidatePhone: candidate?.candidatePhone ?? null,
    sourceType: candidate?.sourceType ?? 'manual',
    stage: candidate?.stage ?? 'applied',
    status: candidate?.status ?? 'new',
    backgroundCheckStatus: candidate?.backgroundCheckStatus ?? null,
    drugScreenStatus: candidate?.drugScreenStatus ?? null,
    physicalStatus: candidate?.physicalStatus ?? null,
    offerStatus: candidate?.offerStatus ?? null,
    score: candidate?.score ?? null,
    notes: candidate?.notes ?? null,
    sourceProductKey: candidate?.sourceProductKey ?? 'staffarr.hiring',
    sourceRef: candidate?.sourceRef ?? null,
  }
}

function formatMaybeDate(value: string | null): string {
  return value ? new Date(value).toLocaleDateString() : 'n/a'
}

function stageBadge(stage: string): string {
  return stage.replaceAll('_', ' ')
}

type ArchiveTarget =
  | { kind: 'requisition'; id: string; message: string }
  | { kind: 'candidate'; id: string; message: string }
  | { kind: 'stage'; id: string; message: string }
  | { kind: 'offer'; id: string; message: string }

function emptyInterviewStageDraft(candidateId: string): UpsertRecruitingInterviewStageRequest {
  return {
    recruitingCandidateId: candidateId,
    stageName: 'Phone screen',
    status: 'scheduled',
    scheduledAt: null,
    completedAt: null,
    interviewerPersonId: null,
    score: null,
    recommendation: null,
    notes: null,
  }
}

function emptyOfferDraft(candidateId: string, candidateName: string | null): UpsertRecruitingOfferRequest {
  return {
    recruitingCandidateId: candidateId,
    status: 'draft',
    title: `Offer for ${candidateName ?? 'candidate'}`,
    payBasis: 'salary',
    annualSalary: 85000,
    hourlyRate: null,
    startDate: null,
    approvedAt: null,
    approvedByPersonId: null,
    acceptedAt: null,
    declinedAt: null,
    notes: null,
    sourceProductKey: 'staffarr.hiring',
    sourceRef: null,
  }
}

function splitCandidateName(name: string, fallbackEmail: string): { legalFirstName: string; legalLastName: string } {
  const trimmed = name.trim()
  const parts = trimmed.split(/\s+/).filter(Boolean)
  if (parts.length >= 2) {
    return {
      legalFirstName: parts[0],
      legalLastName: parts.slice(1).join(' '),
    }
  }

  if (parts.length === 1) {
    return {
      legalFirstName: parts[0],
      legalLastName: parts[0],
    }
  }

  const emailLocalPart = fallbackEmail.split('@')[0]?.trim()
  if (emailLocalPart) {
    return {
      legalFirstName: emailLocalPart,
      legalLastName: 'Candidate',
    }
  }

  return {
    legalFirstName: 'Applicant',
    legalLastName: 'Candidate',
  }
}

function emptyHireDraft(candidate?: RecruitingCandidateResponse | null): CreateStaffPersonRequest {
  const names = splitCandidateName(candidate?.candidateName ?? '', candidate?.candidateEmail ?? '')
  return {
    primaryEmail: candidate?.candidateEmail ?? '',
    legalFirstName: names.legalFirstName,
    legalLastName: names.legalLastName,
    preferredName: null,
    pronouns: null,
    givenName: names.legalFirstName,
    familyName: names.legalLastName,
    employmentStatus: 'pending_start',
    workRelationshipType: 'employee',
    employmentType: 'full_time',
    workerCategory: null,
    flsaStatus: null,
    positionNumber: null,
    currentEmploymentAction: null,
    currentEmploymentActionAt: null,
    leaveStatus: null,
    eligibleForRehire: true,
    alternateEmail: null,
    primaryPhone: candidate?.candidatePhone ?? null,
    alternatePhone: null,
    workPhone: null,
    startDate: null,
    expectedStartDate: null,
    primaryOrgUnitId: null,
    siteOrgUnitId: null,
    departmentOrgUnitId: null,
    teamOrgUnitId: null,
    positionOrgUnitId: null,
    managerPersonId: null,
    jobTitle: null,
    homeBaseLocationId: null,
    canLogin: false,
    temporaryPassword: null,
  }
}

function peopleOptions(people: StaffPersonSummaryResponse[]): PickerOption[] {
  return people.map((person) => ({
    value: person.personId,
    label: `${person.displayName} · ${person.primaryEmail}`,
  }))
}

function orgUnitOptions(orgUnits: OrgUnitResponse[]): PickerOption[] {
  return orgUnits
    .filter((orgUnit) => orgUnit.status === 'active')
    .map((orgUnit) => ({
      value: orgUnit.orgUnitId,
      label: `${orgUnit.unitType} · ${orgUnit.name}`,
    }))
}

function fieldOptions(fieldset: StaffArrFieldsetResponse | undefined, fieldKey: string): PickerOption[] {
  return (
    fieldset?.fields.find((field) => field.key === fieldKey)?.options.map((option) => ({
      value: option.value,
      label: option.label,
    })) ?? []
  )
}

function toDateTimeLocalValue(value: string | null | undefined): string {
  if (!value) {
    return ''
  }

  const date = new Date(value)
  if (Number.isNaN(date.getTime())) {
    return ''
  }

  return date.toISOString().slice(0, 16)
}

export function RecruitingPage() {
  const session = loadSession()
  const accessToken = session?.accessToken ?? ''
  const queryClient = useQueryClient()
  const [selectedRequisitionId, setSelectedRequisitionId] = useState<string | null>(null)
  const [selectedCandidateId, setSelectedCandidateId] = useState<string | null>(null)
  const [selectedStageId, setSelectedStageId] = useState<string | null>(null)
  const [selectedOfferId, setSelectedOfferId] = useState<string | null>(null)
  const [requisitionMode, setRequisitionMode] = useState<'create' | 'edit'>('edit')
  const [stageMode, setStageMode] = useState<'create' | 'edit'>('edit')
  const [offerMode, setOfferMode] = useState<'create' | 'edit'>('edit')
  const [selectedSubmissionId, setSelectedSubmissionId] = useState<string | null>(null)
  const [requisitionDraft, setRequisitionDraft] = useState<UpsertRecruitingRequisitionRequest>(emptyRequisitionDraft)
  const [candidateDraft, setCandidateDraft] = useState<UpsertRecruitingCandidateRequest>(
    emptyCandidateDraft(null),
  )
  const [interviewStageDraft, setInterviewStageDraft] = useState<UpsertRecruitingInterviewStageRequest>(
    emptyInterviewStageDraft(''),
  )
  const [offerDraft, setOfferDraft] = useState<UpsertRecruitingOfferRequest>(emptyOfferDraft('', null))
  const [hireDraft, setHireDraft] = useState<CreateStaffPersonRequest>(emptyHireDraft())
  const [localMessage, setLocalMessage] = useState<string | null>(null)
  const [pendingArchive, setPendingArchive] = useState<ArchiveTarget | null>(null)

  const requisitionsQuery = useQuery({
    queryKey: ['staffarr-recruiting-requisitions', accessToken],
    queryFn: () => listRecruitingRequisitions(accessToken),
    enabled: Boolean(accessToken),
  })

  const candidatesQuery = useQuery({
    queryKey: ['staffarr-recruiting-candidates', accessToken, selectedRequisitionId ?? 'all'],
    queryFn: () => listRecruitingCandidates(accessToken, selectedRequisitionId ?? undefined),
    enabled: Boolean(accessToken),
  })

  const submissionsQuery = useQuery({
    queryKey: ['staffarr-recruiting-submissions', accessToken],
    queryFn: () => listEmploymentApplicationSubmissions(accessToken, 25),
    enabled: Boolean(accessToken),
  })

  const interviewStagesQuery = useQuery({
    queryKey: ['staffarr-recruiting-interview-stages', accessToken, selectedCandidateId ?? 'none'],
    queryFn: () => listRecruitingInterviewStages(accessToken, selectedCandidateId ?? undefined),
    enabled: Boolean(accessToken && selectedCandidateId),
  })

  const offersQuery = useQuery({
    queryKey: ['staffarr-recruiting-offers', accessToken, selectedCandidateId ?? 'none'],
    queryFn: () => listRecruitingOffers(accessToken, selectedCandidateId ?? undefined),
    enabled: Boolean(accessToken && selectedCandidateId),
  })

  const peopleQuery = useQuery({
    queryKey: ['staffarr-hiring-people', accessToken],
    queryFn: () => getPeople(accessToken),
    enabled: Boolean(accessToken),
  })

  const orgUnitsQuery = useQuery({
    queryKey: ['staffarr-hiring-org-units', accessToken],
    queryFn: () => getOrgUnits(accessToken),
    enabled: Boolean(accessToken),
  })

  const fieldsetQuery = useQuery({
    queryKey: ['staffarr-hiring-fieldset', accessToken],
    queryFn: () => getStaffArrFieldset(accessToken, 'people/profile'),
    enabled: Boolean(accessToken),
  })

  const selectedRequisition = useMemo(
    () => requisitionsQuery.data?.find((item) => item.id === selectedRequisitionId) ?? null,
    [requisitionsQuery.data, selectedRequisitionId],
  )

  const selectedCandidate = useMemo(
    () => candidatesQuery.data?.find((item) => item.id === selectedCandidateId) ?? null,
    [candidatesQuery.data, selectedCandidateId],
  )

  const selectedStage = useMemo(
    () => interviewStagesQuery.data?.find((item) => item.id === selectedStageId) ?? null,
    [interviewStagesQuery.data, selectedStageId],
  )

  const selectedOffer = useMemo(
    () => offersQuery.data?.find((item) => item.id === selectedOfferId) ?? null,
    [offersQuery.data, selectedOfferId],
  )

  const managerOptions = useMemo(() => peopleOptions(peopleQuery.data ?? []), [peopleQuery.data])
  const orgUnitPickerOptions = useMemo(() => orgUnitOptions(orgUnitsQuery.data ?? []), [orgUnitsQuery.data])
  const employmentStatusOptions = useMemo(() => fieldOptions(fieldsetQuery.data, 'employmentStatus'), [fieldsetQuery.data])
  const workRelationshipOptions = useMemo(
    () => fieldOptions(fieldsetQuery.data, 'workRelationshipType'),
    [fieldsetQuery.data],
  )
  const employmentTypeOptions = useMemo(() => fieldOptions(fieldsetQuery.data, 'employmentType'), [fieldsetQuery.data])

  useEffect(() => {
    const firstRequisitionId = requisitionsQuery.data?.[0]?.id ?? null
    if (!selectedRequisitionId && firstRequisitionId && requisitionMode === 'edit') {
      setSelectedRequisitionId(firstRequisitionId)
    }
    if (selectedRequisitionId && requisitionsQuery.data && !requisitionsQuery.data.some((item) => item.id === selectedRequisitionId)) {
      setSelectedRequisitionId(firstRequisitionId)
    }
  }, [requisitionMode, requisitionsQuery.data, selectedRequisitionId])

  useEffect(() => {
    if (!selectedRequisition) {
      setRequisitionDraft(emptyRequisitionDraft())
      return
    }

    setRequisitionDraft({
      requisitionNumber: selectedRequisition.requisitionNumber,
      title: selectedRequisition.title,
      jobCode: selectedRequisition.jobCode,
      jobFamily: selectedRequisition.jobFamily,
      departmentRef: selectedRequisition.departmentRef,
      siteRef: selectedRequisition.siteRef,
      locationRef: selectedRequisition.locationRef,
      hiringManagerPersonId: selectedRequisition.hiringManagerPersonId,
      recruiterPersonId: selectedRequisition.recruiterPersonId,
      status: selectedRequisition.status,
      headcountRequested: selectedRequisition.headcountRequested,
      filledCount: selectedRequisition.filledCount,
      openDate: selectedRequisition.openDate,
      targetStartDate: selectedRequisition.targetStartDate,
      sourceProductKey: selectedRequisition.sourceProductKey,
      sourceRef: selectedRequisition.sourceRef,
    })
  }, [requisitionMode, selectedRequisition])

  useEffect(() => {
    const firstSubmissionId = submissionsQuery.data?.find((item) => !item.createdCandidateId)?.employmentApplicationSubmissionId ?? null
    if (!selectedSubmissionId && firstSubmissionId) {
      setSelectedSubmissionId(firstSubmissionId)
    }
  }, [selectedSubmissionId, submissionsQuery.data])

  useEffect(() => {
    if (!selectedSubmissionId) {
      return
    }
    if (submissionsQuery.data && !submissionsQuery.data.some((item) => item.employmentApplicationSubmissionId === selectedSubmissionId)) {
      setSelectedSubmissionId(submissionsQuery.data.find((item) => !item.createdCandidateId)?.employmentApplicationSubmissionId ?? null)
    }
  }, [selectedSubmissionId, submissionsQuery.data])

  useEffect(() => {
    const firstCandidateId = candidatesQuery.data?.[0]?.id ?? null
    if (!selectedCandidateId && firstCandidateId) {
      setSelectedCandidateId(firstCandidateId)
    }
    if (
      selectedCandidateId &&
      candidatesQuery.data &&
      !candidatesQuery.data.some((item) => item.id === selectedCandidateId)
    ) {
      setSelectedCandidateId(firstCandidateId)
    }
  }, [candidatesQuery.data, selectedCandidateId])

  useEffect(() => {
    if (!selectedCandidateId) {
      setCandidateDraft(emptyCandidateDraft(selectedRequisitionId))
      setInterviewStageDraft(emptyInterviewStageDraft(''))
      setOfferDraft(emptyOfferDraft('', null))
      setHireDraft(emptyHireDraft())
      return
    }

    setCandidateDraft(emptyCandidateDraft(selectedRequisitionId, selectedCandidate))
    setInterviewStageDraft(emptyInterviewStageDraft(selectedCandidateId))
    setOfferDraft(emptyOfferDraft(selectedCandidateId, selectedCandidate?.candidateName ?? null))
    setHireDraft((current) => ({
      ...emptyHireDraft(selectedCandidate),
      jobTitle: selectedRequisition?.title ?? selectedCandidate?.candidateName ?? current.jobTitle ?? null,
      primaryOrgUnitId: current.primaryOrgUnitId,
      managerPersonId: current.managerPersonId,
    }))
  }, [selectedCandidate, selectedCandidateId, selectedRequisition, selectedRequisitionId])

  useEffect(() => {
    const firstStageId = interviewStagesQuery.data?.[0]?.id ?? null
    if (!selectedStageId && firstStageId && stageMode === 'edit') {
      setSelectedStageId(firstStageId)
    }
    if (selectedStageId && interviewStagesQuery.data && !interviewStagesQuery.data.some((item) => item.id === selectedStageId)) {
      setSelectedStageId(firstStageId)
    }
  }, [interviewStagesQuery.data, selectedStageId, stageMode])

  useEffect(() => {
    if (!selectedStage) {
      return
    }

    setInterviewStageDraft({
      recruitingCandidateId: selectedStage.recruitingCandidateId,
      stageName: selectedStage.stageName,
      status: selectedStage.status,
      scheduledAt: selectedStage.scheduledAt,
      completedAt: selectedStage.completedAt,
      interviewerPersonId: selectedStage.interviewerPersonId,
      score: selectedStage.score,
      recommendation: selectedStage.recommendation,
      notes: selectedStage.notes,
    })
  }, [selectedStage, stageMode])

  useEffect(() => {
    const firstOfferId = offersQuery.data?.[0]?.id ?? null
    if (!selectedOfferId && firstOfferId && offerMode === 'edit') {
      setSelectedOfferId(firstOfferId)
    }
    if (selectedOfferId && offersQuery.data && !offersQuery.data.some((item) => item.id === selectedOfferId)) {
      setSelectedOfferId(firstOfferId)
    }
  }, [offerMode, offersQuery.data, selectedOfferId])

  useEffect(() => {
    if (!selectedOffer) {
      return
    }

    setOfferDraft({
      recruitingCandidateId: selectedOffer.recruitingCandidateId,
      status: selectedOffer.status,
      title: selectedOffer.title,
      payBasis: selectedOffer.payBasis,
      annualSalary: selectedOffer.annualSalary,
      hourlyRate: selectedOffer.hourlyRate,
      startDate: selectedOffer.startDate,
      approvedAt: selectedOffer.approvedAt,
      approvedByPersonId: selectedOffer.approvedByPersonId,
      acceptedAt: selectedOffer.acceptedAt,
      declinedAt: selectedOffer.declinedAt,
      notes: selectedOffer.notes,
      sourceProductKey: selectedOffer.sourceProductKey,
      sourceRef: selectedOffer.sourceRef,
    })
  }, [offerMode, selectedOffer])

  const refreshAll = async () => {
    await Promise.all([
      queryClient.invalidateQueries({ queryKey: ['staffarr-recruiting-requisitions', accessToken] }),
      queryClient.invalidateQueries({ queryKey: ['staffarr-recruiting-candidates', accessToken] }),
      queryClient.invalidateQueries({ queryKey: ['staffarr-recruiting-submissions', accessToken] }),
      queryClient.invalidateQueries({ queryKey: ['staffarr-recruiting-interview-stages', accessToken] }),
      queryClient.invalidateQueries({ queryKey: ['staffarr-recruiting-offers', accessToken] }),
      queryClient.invalidateQueries({ queryKey: ['staffarr-hiring-people', accessToken] }),
      queryClient.invalidateQueries({ queryKey: ['staffarr-hiring-org-units', accessToken] }),
    ])
  }

  const createRequisitionMutation = useMutation({
    mutationFn: (request: UpsertRecruitingRequisitionRequest) => createRecruitingRequisition(accessToken, request),
    onSuccess: async (created) => {
      await refreshAll()
      setSelectedRequisitionId(created.id)
      setRequisitionMode('edit')
      setRequisitionDraft(emptyRequisitionDraft())
      setLocalMessage(`Created requisition ${created.requisitionNumber}.`)
    },
  })

  const updateRequisitionMutation = useMutation({
    mutationFn: ({ requisitionId, request }: { requisitionId: string; request: UpsertRecruitingRequisitionRequest }) =>
      updateRecruitingRequisition(accessToken, requisitionId, request),
    onSuccess: async (updated) => {
      await refreshAll()
      setSelectedRequisitionId(updated.id)
      setRequisitionMode('edit')
      setLocalMessage(`Updated requisition ${updated.requisitionNumber}.`)
    },
  })

  const archiveRequisitionMutation = useMutation({
    mutationFn: (requisitionId: string) => archiveRecruitingRequisition(accessToken, requisitionId),
    onSuccess: async (updated) => {
      await refreshAll()
      setSelectedRequisitionId(updated.id)
      setRequisitionMode('edit')
      setLocalMessage(`Archived requisition ${updated.requisitionNumber}.`)
    },
  })

  const convertSubmissionMutation = useMutation({
    mutationFn: ({ submissionId, requisitionId }: { submissionId: string; requisitionId?: string | null }) =>
      convertEmploymentApplicationSubmissionToCandidate(accessToken, submissionId, requisitionId ?? null),
    onSuccess: async (candidate) => {
      await refreshAll()
      setLocalMessage(`Linked the application and created candidate ${candidate.candidateName}.`)
    },
  })

  const hireCandidateMutation = useMutation({
    mutationFn: ({ candidateId, request }: { candidateId: string; request: CreateStaffPersonRequest }) =>
      hireRecruitingCandidate(accessToken, candidateId, request),
    onSuccess: async (updated) => {
      await refreshAll()
      setSelectedCandidateId(updated.id)
      setLocalMessage(`Created person record for ${updated.candidateName}.`)
    },
  })

  const updateCandidateMutation = useMutation({
    mutationFn: ({ candidateId, request }: { candidateId: string; request: UpsertRecruitingCandidateRequest }) =>
      updateRecruitingCandidate(accessToken, candidateId, request),
    onSuccess: async (updated) => {
      await refreshAll()
      setSelectedCandidateId(updated.id)
      setLocalMessage(`Updated candidate ${updated.candidateName}.`)
    },
  })

  const archiveCandidateMutation = useMutation({
    mutationFn: (candidateId: string) => archiveRecruitingCandidate(accessToken, candidateId),
    onSuccess: async (updated) => {
      await refreshAll()
      setSelectedCandidateId(updated.id)
      setLocalMessage(`Archived candidate ${updated.candidateName}.`)
    },
  })

  const createInterviewStageMutation = useMutation({
    mutationFn: (request: UpsertRecruitingInterviewStageRequest) => createRecruitingInterviewStage(accessToken, request),
    onSuccess: async () => {
      await refreshAll()
      setInterviewStageDraft((current) => ({
        ...emptyInterviewStageDraft(current.recruitingCandidateId),
        stageName: current.stageName,
      }))
      setStageMode('edit')
      setLocalMessage('Created interview stage.')
    },
  })

  const updateInterviewStageMutation = useMutation({
    mutationFn: ({ stageId, request }: { stageId: string; request: UpsertRecruitingInterviewStageRequest }) =>
      updateRecruitingInterviewStage(accessToken, stageId, request),
    onSuccess: async (updated) => {
      await refreshAll()
      setSelectedStageId(updated.id)
      setStageMode('edit')
      setLocalMessage('Updated interview stage.')
    },
  })

  const archiveInterviewStageMutation = useMutation({
    mutationFn: (stageId: string) => archiveRecruitingInterviewStage(accessToken, stageId),
    onSuccess: async (updated) => {
      await refreshAll()
      setSelectedStageId(updated.id)
      setStageMode('edit')
      setLocalMessage('Archived interview stage.')
    },
  })

  const createOfferMutation = useMutation({
    mutationFn: (request: UpsertRecruitingOfferRequest) => createRecruitingOffer(accessToken, request),
    onSuccess: async () => {
      await refreshAll()
      setOfferDraft((current) => ({
        ...emptyOfferDraft(current.recruitingCandidateId, selectedCandidate?.candidateName ?? null),
        title: current.title,
      }))
      setOfferMode('edit')
      setLocalMessage('Created offer record.')
    },
  })

  const updateOfferMutation = useMutation({
    mutationFn: ({ offerId, request }: { offerId: string; request: UpsertRecruitingOfferRequest }) =>
      updateRecruitingOffer(accessToken, offerId, request),
    onSuccess: async (updated) => {
      await refreshAll()
      setSelectedOfferId(updated.id)
      setOfferMode('edit')
      setLocalMessage('Updated offer record.')
    },
  })

  const archiveOfferMutation = useMutation({
    mutationFn: (offerId: string) => archiveRecruitingOffer(accessToken, offerId),
    onSuccess: async (updated) => {
      await refreshAll()
      setSelectedOfferId(updated.id)
      setOfferMode('edit')
      setLocalMessage('Archived offer record.')
    },
  })

  if (!session) {
    return (
      <div className="space-y-6">
        <PageHeader title="Hiring" subtitle="Requisitions, candidates, interviews, offers, and applicant conversion" />
        <div className="rounded-2xl border border-slate-800 bg-slate-950/70 p-6 text-sm text-slate-300">
          Sign in to see hiring data.
        </div>
      </div>
    )
  }

  const requisitions = requisitionsQuery.data ?? []
  const candidates = candidatesQuery.data ?? []
  const submissions = submissionsQuery.data ?? []
  const interviewStages = interviewStagesQuery.data ?? []
  const offers = offersQuery.data ?? []
  const requisitionsById = useMemo(
    () => new Map(requisitions.map((requisition) => [requisition.id, requisition])),
    [requisitions],
  )
  const selectedSubmission = submissions.find((item) => item.employmentApplicationSubmissionId === selectedSubmissionId) ?? null

  return (
    <div className="space-y-6">
      <PageHeader
        title="Hiring"
        subtitle="Live requisitions, applicant bridges, interview stages, offers, and person conversion"
      />
      <ConfirmDialog
        open={pendingArchive !== null}
        title="Confirm archive"
        description={pendingArchive?.message ?? 'Confirm this archive action.'}
        confirmLabel="Archive"
        cancelLabel="Cancel"
        danger
        onConfirm={() => {
          if (!pendingArchive) return
          const archive = pendingArchive
          setPendingArchive(null)
          switch (archive.kind) {
            case 'requisition':
              archiveRequisitionMutation.mutate(archive.id)
              break
            case 'candidate':
              archiveCandidateMutation.mutate(archive.id)
              break
            case 'stage':
              archiveInterviewStageMutation.mutate(archive.id)
              break
            case 'offer':
              archiveOfferMutation.mutate(archive.id)
              break
          }
        }}
        onCancel={() => setPendingArchive(null)}
      />

      {localMessage ? (
        <div className="rounded-xl border border-emerald-500/20 bg-emerald-500/10 px-4 py-3 text-sm text-emerald-200">
          {localMessage}
        </div>
      ) : null}

      <section className="grid gap-4 lg:grid-cols-[1.3fr_1fr]">
        <div className="rounded-2xl border border-slate-800 bg-slate-950/70 p-5">
          <div className="flex items-start justify-between gap-3">
            <h2 className="text-base font-semibold text-slate-50">
              {requisitionMode === 'create' ? 'Create requisition' : 'Edit requisition'}
            </h2>
            <button
              type="button"
              className="rounded-lg border border-slate-700 bg-slate-900 px-3 py-1.5 text-xs font-medium text-slate-200 hover:border-slate-500"
              onClick={() => {
                setRequisitionMode('create')
                setSelectedRequisitionId(null)
                setRequisitionDraft(emptyRequisitionDraft())
              }}
            >
              New requisition
            </button>
          </div>
          <div className="mt-4 grid gap-3 md:grid-cols-2">
            <label className="text-sm text-slate-300">
              Requisition number
              <input
                className="mt-1 w-full rounded-lg border border-slate-700 bg-slate-900 px-3 py-2 text-slate-100"
                value={requisitionDraft.requisitionNumber}
                onChange={(e) => setRequisitionDraft((current) => ({ ...current, requisitionNumber: e.target.value }))}
              />
            </label>
            <label className="text-sm text-slate-300">
              Title
              <input
                className="mt-1 w-full rounded-lg border border-slate-700 bg-slate-900 px-3 py-2 text-slate-100"
                value={requisitionDraft.title}
                onChange={(e) => setRequisitionDraft((current) => ({ ...current, title: e.target.value }))}
              />
            </label>
            <label className="text-sm text-slate-300">
              Job code
              <input
                className="mt-1 w-full rounded-lg border border-slate-700 bg-slate-900 px-3 py-2 text-slate-100"
                value={requisitionDraft.jobCode}
                onChange={(e) => setRequisitionDraft((current) => ({ ...current, jobCode: e.target.value }))}
              />
            </label>
            <label className="text-sm text-slate-300">
              Job family
              <input
                className="mt-1 w-full rounded-lg border border-slate-700 bg-slate-900 px-3 py-2 text-slate-100"
                value={requisitionDraft.jobFamily}
                onChange={(e) => setRequisitionDraft((current) => ({ ...current, jobFamily: e.target.value }))}
              />
            </label>
            <label className="text-sm text-slate-300">
              Headcount
              <input
                type="number"
                min={1}
                className="mt-1 w-full rounded-lg border border-slate-700 bg-slate-900 px-3 py-2 text-slate-100"
                value={requisitionDraft.headcountRequested}
                onChange={(e) => setRequisitionDraft((current) => ({ ...current, headcountRequested: Number(e.target.value) || 1 }))}
              />
            </label>
            <label className="text-sm text-slate-300">
              Open date
              <input
                type="date"
                className="mt-1 w-full rounded-lg border border-slate-700 bg-slate-900 px-3 py-2 text-slate-100"
                value={requisitionDraft.openDate ?? ''}
                onChange={(e) => setRequisitionDraft((current) => ({ ...current, openDate: e.target.value || null }))}
              />
            </label>
          </div>
          <button
            type="button"
            className="mt-4 rounded-lg bg-sky-500 px-4 py-2 text-sm font-medium text-[var(--color-text-primary)] hover:bg-sky-400 disabled:opacity-50"
            disabled={createRequisitionMutation.isPending || updateRequisitionMutation.isPending}
            onClick={() => {
              if (requisitionMode === 'create' || !selectedRequisitionId) {
                createRequisitionMutation.mutate(requisitionDraft)
                return
              }

              updateRequisitionMutation.mutate({
                requisitionId: selectedRequisitionId,
                request: requisitionDraft,
              })
            }}
          >
            {requisitionMode === 'create' || !selectedRequisitionId ? 'Create requisition' : 'Save requisition'}
          </button>
            {selectedRequisitionId ? (
              <button
                type="button"
                className="mt-3 rounded-lg border border-amber-500/30 bg-amber-500/10 px-4 py-2 text-sm font-medium text-amber-200 hover:bg-amber-500/20 disabled:opacity-50"
                disabled={archiveRequisitionMutation.isPending}
                onClick={() => {
                  if (!selectedRequisitionId) return
                  setPendingArchive({
                    kind: 'requisition',
                    id: selectedRequisitionId,
                    message: 'Archive this requisition?',
                  })
                }}
              >
                Archive requisition
              </button>
          ) : null}
        </div>

        <div className="rounded-2xl border border-slate-800 bg-slate-950/70 p-5">
          <h2 className="text-base font-semibold text-slate-50">Bridge an applicant</h2>
          <p className="mt-2 text-sm text-slate-400">
            Link an applicant submission to a requisition on the application record, then create a candidate record.
          </p>
          <label className="mt-4 block text-sm text-slate-300">
            Submission
            <select
              className="mt-1 w-full rounded-lg border border-slate-700 bg-slate-900 px-3 py-2 text-slate-100"
              value={selectedSubmissionId ?? ''}
              onChange={(e) => setSelectedSubmissionId(e.target.value || null)}
            >
              <option value="">Select a submission</option>
              {submissions.map((submission) => (
                <option key={submission.employmentApplicationSubmissionId} value={submission.employmentApplicationSubmissionId}>
                  {submission.applicantDisplayName} - {submission.templateKey} - {submission.status}
                </option>
              ))}
            </select>
          </label>
          <button
            type="button"
            className="mt-4 rounded-lg bg-[var(--color-bg-control-hover)] px-4 py-2 text-sm font-medium text-[var(--color-text-primary)] hover:bg-[var(--color-bg-surface)] disabled:opacity-50"
            disabled={!selectedSubmissionId || convertSubmissionMutation.isPending}
            onClick={() => {
              if (!selectedSubmissionId) return
              convertSubmissionMutation.mutate({
                submissionId: selectedSubmissionId,
                requisitionId: selectedRequisitionId,
              })
            }}
          >
            Link application and create candidate
          </button>
          <div className="mt-4 grid gap-3 text-sm text-slate-300">
            <div className="rounded-xl border border-slate-800 bg-slate-900/60 p-3">
              <p className="text-xs uppercase tracking-[0.2em] text-[var(--color-text-muted)]">Application link</p>
              <p className="mt-1 font-medium text-slate-50">
                {selectedSubmission?.recruitingRequisitionId
                  ? requisitionsById.get(selectedSubmission.recruitingRequisitionId)?.title ??
                    selectedSubmission.recruitingRequisitionId
                  : selectedRequisition?.title ?? 'No requisition linked yet'}
              </p>
              <p className="text-xs text-slate-400">
                {selectedSubmission?.recruitingRequisitionId
                  ? `Stored on submission ${selectedSubmission.applicantDisplayName}.`
                  : selectedRequisition?.requisitionNumber ?? 'Pick a requisition to link this application.'}
              </p>
            </div>
            <div className="rounded-xl border border-slate-800 bg-slate-900/60 p-3">
              <p className="text-xs uppercase tracking-[0.2em] text-[var(--color-text-muted)]">Pipeline snapshot</p>
              <p className="mt-1 font-medium text-slate-50">{candidates.length} candidates</p>
              <p className="text-xs text-slate-400">{interviewStages.length} interview stages, {offers.length} offers</p>
            </div>
          </div>
        </div>
      </section>

      <section className="grid gap-4 xl:grid-cols-3">
        <div className="rounded-2xl border border-slate-800 bg-slate-950/70 p-5 xl:col-span-1">
          <h2 className="text-base font-semibold text-slate-50">Requisitions</h2>
          <div className="mt-4 space-y-3">
            {requisitions.map((requisition) => (
              <button
                key={requisition.id}
                type="button"
                className={`w-full rounded-xl border px-4 py-3 text-left transition ${
                  selectedRequisitionId === requisition.id
                    ? 'border-sky-500/50 bg-sky-500/10'
                    : 'border-slate-800 bg-slate-900/60 hover:border-slate-600'
                }`}
                onClick={() => {
                  setRequisitionMode('edit')
                  setSelectedRequisitionId(requisition.id)
                }}
              >
                <div className="flex items-center justify-between gap-3">
                  <p className="font-medium text-slate-50">{requisition.title}</p>
                  <span className="rounded-full bg-slate-800 px-2 py-0.5 text-[11px] uppercase tracking-[0.18em] text-slate-300">
                    {requisition.status}
                  </span>
                </div>
                <p className="mt-1 text-xs text-slate-400">
                  {requisition.requisitionNumber} - {requisition.jobCode} - {requisition.jobFamily}
                </p>
                <p className="mt-1 text-xs text-[var(--color-text-muted)]">
                  Headcount {requisition.filledCount}/{requisition.headcountRequested} - Opened {formatMaybeDate(requisition.openDate)}
                </p>
              </button>
            ))}
            {requisitions.length === 0 ? (
              <p className="text-sm text-slate-400">No requisitions yet.</p>
            ) : null}
          </div>
        </div>

        <div className="rounded-2xl border border-slate-800 bg-slate-950/70 p-5 xl:col-span-1">
          <h2 className="text-base font-semibold text-slate-50">Candidates</h2>
          <div className="mt-4 space-y-3">
            {candidates.map((candidate: RecruitingCandidateResponse) => (
              <button
                key={candidate.id}
                type="button"
                className={`w-full rounded-xl border p-4 text-left transition ${
                  selectedCandidateId === candidate.id
                    ? 'border-emerald-500/50 bg-emerald-500/10'
                    : 'border-slate-800 bg-slate-900/60 hover:border-slate-600'
                }`}
                onClick={() => {
                  setSelectedCandidateId(candidate.id)
                  setStageMode('edit')
                  setOfferMode('edit')
                }}
              >
                <div className="flex items-start justify-between gap-3">
                  <div>
                    <p className="font-medium text-slate-50">{candidate.candidateName}</p>
                    <p className="text-xs text-slate-400">{candidate.candidateEmail}</p>
                  </div>
                  <span className="rounded-full border border-emerald-500/30 bg-emerald-500/10 px-2 py-0.5 text-[11px] uppercase tracking-[0.18em] text-emerald-300">
                    {stageBadge(candidate.stage)}
                  </span>
                </div>
                <p className="mt-2 text-xs text-slate-400">
                  Status {candidate.status} - Source {candidate.sourceType} {candidate.offerStatus ? `- Offer ${candidate.offerStatus}` : ''}
                </p>
                {candidate.notes ? <p className="mt-2 text-xs text-[var(--color-text-muted)]">{candidate.notes}</p> : null}
              </button>
            ))}
            {candidates.length === 0 ? <p className="text-sm text-slate-400">No candidates yet.</p> : null}
          </div>

          <div className="mt-5 rounded-xl border border-slate-800 bg-slate-900/60 p-4">
            <div className="flex items-start justify-between gap-3">
              <div>
                <h3 className="text-sm font-semibold text-slate-50">Candidate details</h3>
                <p className="text-xs text-slate-400">
                  {selectedCandidate?.candidateName ?? 'Select a candidate to edit.'}
                </p>
              </div>
              <span className="rounded-full bg-slate-800 px-2 py-0.5 text-[11px] uppercase tracking-[0.18em] text-slate-300">
                {selectedCandidate?.status ?? 'n/a'}
              </span>
            </div>
            <div className="mt-3 grid gap-3">
              <label className="text-sm text-slate-300">
                Name
                <input
                  className="mt-1 w-full rounded-lg border border-slate-700 bg-slate-900 px-3 py-2 text-slate-100"
                  value={candidateDraft.candidateName}
                  disabled={!selectedCandidateId}
                  onChange={(e) => setCandidateDraft((current) => ({ ...current, candidateName: e.target.value }))}
                />
              </label>
              <label className="text-sm text-slate-300">
                Email
                <input
                  className="mt-1 w-full rounded-lg border border-slate-700 bg-slate-900 px-3 py-2 text-slate-100"
                  value={candidateDraft.candidateEmail}
                  disabled={!selectedCandidateId}
                  onChange={(e) => setCandidateDraft((current) => ({ ...current, candidateEmail: e.target.value }))}
                />
              </label>
              <label className="text-sm text-slate-300">
                Phone
                <input
                  className="mt-1 w-full rounded-lg border border-slate-700 bg-slate-900 px-3 py-2 text-slate-100"
                  value={candidateDraft.candidatePhone ?? ''}
                  disabled={!selectedCandidateId}
                  onChange={(e) => setCandidateDraft((current) => ({ ...current, candidatePhone: e.target.value || null }))}
                />
              </label>
              <label className="text-sm text-slate-300">
                Stage
                <input
                  className="mt-1 w-full rounded-lg border border-slate-700 bg-slate-900 px-3 py-2 text-slate-100"
                  value={candidateDraft.stage}
                  disabled={!selectedCandidateId}
                  onChange={(e) => setCandidateDraft((current) => ({ ...current, stage: e.target.value }))}
                />
              </label>
              <label className="text-sm text-slate-300">
                Status
                <input
                  className="mt-1 w-full rounded-lg border border-slate-700 bg-slate-900 px-3 py-2 text-slate-100"
                  value={candidateDraft.status}
                  disabled={!selectedCandidateId}
                  onChange={(e) => setCandidateDraft((current) => ({ ...current, status: e.target.value }))}
                />
              </label>
              <label className="text-sm text-slate-300">
                Score
                <input
                  type="number"
                  step="0.01"
                  className="mt-1 w-full rounded-lg border border-slate-700 bg-slate-900 px-3 py-2 text-slate-100"
                  value={candidateDraft.score ?? ''}
                  disabled={!selectedCandidateId}
                  onChange={(e) =>
                    setCandidateDraft((current) => ({
                      ...current,
                      score: e.target.value === '' ? null : Number(e.target.value),
                    }))
                  }
                />
              </label>
              <label className="text-sm text-slate-300">
                Notes
                <textarea
                  rows={3}
                  className="mt-1 w-full rounded-lg border border-slate-700 bg-slate-900 px-3 py-2 text-slate-100"
                  value={candidateDraft.notes ?? ''}
                  disabled={!selectedCandidateId}
                  onChange={(e) => setCandidateDraft((current) => ({ ...current, notes: e.target.value || null }))}
                />
              </label>
            </div>
            <button
              type="button"
              className="mt-4 rounded-lg bg-sky-500 px-4 py-2 text-sm font-medium text-[var(--color-text-primary)] hover:bg-sky-400 disabled:opacity-50"
              disabled={!selectedCandidateId || updateCandidateMutation.isPending}
              onClick={() => {
                if (!selectedCandidateId) return
                updateCandidateMutation.mutate({
                  candidateId: selectedCandidateId,
                  request: candidateDraft,
                })
              }}
            >
              Save candidate
            </button>
            {selectedCandidateId ? (
              <button
                type="button"
                className="mt-3 rounded-lg border border-amber-500/30 bg-amber-500/10 px-4 py-2 text-sm font-medium text-amber-200 hover:bg-amber-500/20 disabled:opacity-50"
                disabled={archiveCandidateMutation.isPending}
                onClick={() => {
                  if (!selectedCandidateId) return
                  setPendingArchive({
                    kind: 'candidate',
                    id: selectedCandidateId,
                    message: 'Archive this candidate?',
                  })
                }}
              >
                Archive candidate
              </button>
            ) : null}

            <div className="mt-5 rounded-xl border border-slate-800 bg-slate-950/60 p-4">
              <div className="flex items-start justify-between gap-3">
                <div>
                  <h3 className="text-sm font-semibold text-slate-50">Hire candidate</h3>
                  <p className="text-xs text-slate-400">
                    Create the StaffArr person record after the candidate is ready to join the workforce.
                  </p>
                </div>
                <span className="rounded-full border border-slate-700 bg-slate-900 px-2 py-0.5 text-[11px] uppercase tracking-[0.18em] text-slate-300">
                  {selectedCandidate?.personId ? 'Linked' : 'Ready'}
                </span>
              </div>

              {selectedCandidate?.personId ? (
                <div className="mt-3 rounded-lg border border-emerald-500/20 bg-emerald-500/10 px-3 py-2 text-sm text-emerald-200">
                  This candidate is already linked to person <span className="font-medium">{selectedCandidate.personId}</span>.
                </div>
              ) : (
                <>
                  <div className="mt-4 grid gap-3 md:grid-cols-2">
                    <label className="text-sm text-slate-300">
                      Legal first name
                      <input
                        className="mt-1 w-full rounded-lg border border-slate-700 bg-slate-900 px-3 py-2 text-slate-100"
                        value={hireDraft.legalFirstName ?? ''}
                        onChange={(e) => setHireDraft((current) => ({ ...current, legalFirstName: e.target.value }))}
                      />
                    </label>
                    <label className="text-sm text-slate-300">
                      Legal last name
                      <input
                        className="mt-1 w-full rounded-lg border border-slate-700 bg-slate-900 px-3 py-2 text-slate-100"
                        value={hireDraft.legalLastName ?? ''}
                        onChange={(e) => setHireDraft((current) => ({ ...current, legalLastName: e.target.value }))}
                      />
                    </label>
                    <label className="text-sm text-slate-300">
                      Primary email
                      <input
                        className="mt-1 w-full rounded-lg border border-slate-700 bg-slate-900 px-3 py-2 text-slate-100"
                        value={hireDraft.primaryEmail}
                        onChange={(e) => setHireDraft((current) => ({ ...current, primaryEmail: e.target.value }))}
                      />
                    </label>
                    <label className="text-sm text-slate-300">
                      Primary phone
                      <input
                        className="mt-1 w-full rounded-lg border border-slate-700 bg-slate-900 px-3 py-2 text-slate-100"
                        value={hireDraft.primaryPhone ?? ''}
                        onChange={(e) => setHireDraft((current) => ({ ...current, primaryPhone: e.target.value || null }))}
                      />
                    </label>
                    <label className="text-sm text-slate-300">
                      Job title
                      <input
                        className="mt-1 w-full rounded-lg border border-slate-700 bg-slate-900 px-3 py-2 text-slate-100"
                        value={hireDraft.jobTitle ?? ''}
                        onChange={(e) => setHireDraft((current) => ({ ...current, jobTitle: e.target.value || null }))}
                      />
                    </label>
                    <label className="text-sm text-slate-300">
                      Employment status
                      <StaticSearchPicker
                        value={hireDraft.employmentStatus}
                        onChange={(value) => setHireDraft((current) => ({ ...current, employmentStatus: value }))}
                        options={employmentStatusOptions}
                        placeholder="Search status"
                      />
                    </label>
                    <label className="text-sm text-slate-300">
                      Work relationship
                      <StaticSearchPicker
                        value={hireDraft.workRelationshipType ?? ''}
                        onChange={(value) => setHireDraft((current) => ({ ...current, workRelationshipType: value || null }))}
                        options={workRelationshipOptions}
                        placeholder="Search relationship"
                      />
                    </label>
                    <label className="text-sm text-slate-300">
                      Employment type
                      <StaticSearchPicker
                        value={hireDraft.employmentType ?? ''}
                        onChange={(value) => setHireDraft((current) => ({ ...current, employmentType: value || null }))}
                        options={employmentTypeOptions}
                        placeholder="Search employment type"
                      />
                    </label>
                    <label className="text-sm text-slate-300">
                      Primary org unit
                      <StaticSearchPicker
                        value={hireDraft.primaryOrgUnitId ?? ''}
                        onChange={(value) => setHireDraft((current) => ({ ...current, primaryOrgUnitId: value || null }))}
                        options={orgUnitPickerOptions}
                        placeholder="Search org units"
                      />
                    </label>
                    <label className="text-sm text-slate-300 md:col-span-2">
                      Manager
                      <StaticSearchPicker
                        value={hireDraft.managerPersonId ?? ''}
                        onChange={(value) => setHireDraft((current) => ({ ...current, managerPersonId: value || null }))}
                        options={managerOptions}
                        placeholder="Search managers"
                      />
                    </label>
                    <label className="text-sm text-slate-300">
                      Start date
                      <input
                        type="date"
                        className="mt-1 w-full rounded-lg border border-slate-700 bg-slate-900 px-3 py-2 text-slate-100"
                        value={hireDraft.startDate ?? ''}
                        onChange={(e) => setHireDraft((current) => ({ ...current, startDate: e.target.value || null }))}
                      />
                    </label>
                    <label className="text-sm text-slate-300">
                      Expected start date
                      <input
                        type="date"
                        className="mt-1 w-full rounded-lg border border-slate-700 bg-slate-900 px-3 py-2 text-slate-100"
                        value={hireDraft.expectedStartDate ?? ''}
                        onChange={(e) => setHireDraft((current) => ({ ...current, expectedStartDate: e.target.value || null }))}
                      />
                    </label>
                  </div>

                  <label className="mt-4 flex items-center gap-3 text-sm text-slate-300">
                    <input
                      type="checkbox"
                      checked={hireDraft.canLogin}
                      onChange={(e) => setHireDraft((current) => ({ ...current, canLogin: e.target.checked }))}
                    />
                    Create login on hire
                  </label>

                  {hireDraft.canLogin ? (
                    <label className="mt-3 block text-sm text-slate-300">
                      Temporary password
                      <input
                        type="password"
                        className="mt-1 w-full rounded-lg border border-slate-700 bg-slate-900 px-3 py-2 text-slate-100"
                        value={hireDraft.temporaryPassword ?? ''}
                        onChange={(e) => setHireDraft((current) => ({ ...current, temporaryPassword: e.target.value || null }))}
                      />
                    </label>
                  ) : null}

                  <button
                    type="button"
                    className="mt-4 rounded-lg bg-emerald-500 px-4 py-2 text-sm font-medium text-[var(--color-on-accent)] hover:bg-emerald-400 disabled:opacity-50"
                    disabled={
                      !selectedCandidateId ||
                      hireCandidateMutation.isPending ||
                      !hireDraft.primaryEmail.trim() ||
                      !hireDraft.legalFirstName?.trim() ||
                      !hireDraft.legalLastName?.trim() ||
                      (hireDraft.canLogin && !hireDraft.temporaryPassword?.trim())
                    }
                    onClick={() => {
                      if (!selectedCandidateId) return
                      hireCandidateMutation.mutate({
                        candidateId: selectedCandidateId,
                        request: hireDraft,
                      })
                    }}
                  >
                    {hireCandidateMutation.isPending ? 'Hiring...' : 'Create person record'}
                  </button>
                </>
              )}
            </div>
          </div>
        </div>

        <div className="rounded-2xl border border-slate-800 bg-slate-950/70 p-5 xl:col-span-1">
          <h2 className="text-base font-semibold text-slate-50">Applications</h2>
          <div className="mt-4 space-y-3">
            {submissions.map((submission: EmploymentApplicationSubmissionListItemResponse) => (
              <div key={submission.employmentApplicationSubmissionId} className="rounded-xl border border-slate-800 bg-slate-900/60 p-4">
                <p className="font-medium text-slate-50">{submission.applicantDisplayName}</p>
                <p className="text-xs text-slate-400">{submission.applicantEmail}</p>
                <p className="mt-2 text-xs text-slate-400">
                  Template {submission.templateKey} - {submission.status}
                </p>
                <p className="text-xs text-[var(--color-text-muted)]">
                  Submitted {new Date(submission.submittedAt).toLocaleString()}
                </p>
                <p className="mt-2 text-xs text-[var(--color-text-muted)]">
                  {submission.recruitingRequisitionId
                    ? `Requisition linked: ${requisitionsById.get(submission.recruitingRequisitionId)?.title ?? submission.recruitingRequisitionId}`
                    : 'No requisition linked yet'}
                </p>
                <p className="mt-1 text-xs text-[var(--color-text-muted)]">
                  {submission.createdCandidateId
                    ? `Candidate linked: ${submission.createdCandidateId}`
                    : 'Not yet converted to a candidate'}
                </p>
              </div>
            ))}
            {submissions.length === 0 ? <p className="text-sm text-slate-400">No applications yet.</p> : null}
          </div>
        </div>
      </section>

      <section className="grid gap-4 xl:grid-cols-2">
        <div className="rounded-2xl border border-slate-800 bg-slate-950/70 p-5">
          <div className="flex items-start justify-between gap-3">
            <div>
              <h2 className="text-base font-semibold text-slate-50">
                {stageMode === 'create' ? 'Create interview stage' : 'Edit interview stage'}
              </h2>
              <p className="mt-1 text-sm text-slate-400">
                Manage the selected candidate&apos;s pipeline steps.
              </p>
            </div>
            <button
              type="button"
              className="rounded-lg border border-slate-700 bg-slate-900 px-3 py-1.5 text-xs font-medium text-slate-200 hover:border-slate-500"
              onClick={() => {
                setStageMode('create')
                setSelectedStageId(null)
                setInterviewStageDraft(emptyInterviewStageDraft(selectedCandidateId ?? ''))
              }}
            >
              New stage
            </button>
          </div>

          <div className="mt-4 grid gap-3 md:grid-cols-2">
            <label className="text-sm text-slate-300">
              Stage name
              <input
                className="mt-1 w-full rounded-lg border border-slate-700 bg-slate-900 px-3 py-2 text-slate-100"
                value={interviewStageDraft.stageName}
                disabled={!selectedCandidateId}
                onChange={(e) => setInterviewStageDraft((current) => ({ ...current, stageName: e.target.value }))}
              />
            </label>
            <label className="text-sm text-slate-300">
              Status
              <input
                className="mt-1 w-full rounded-lg border border-slate-700 bg-slate-900 px-3 py-2 text-slate-100"
                value={interviewStageDraft.status}
                disabled={!selectedCandidateId}
                onChange={(e) => setInterviewStageDraft((current) => ({ ...current, status: e.target.value }))}
              />
            </label>
            <label className="text-sm text-slate-300">
              Scheduled at
              <input
                type="datetime-local"
                className="mt-1 w-full rounded-lg border border-slate-700 bg-slate-900 px-3 py-2 text-slate-100"
                value={toDateTimeLocalValue(interviewStageDraft.scheduledAt)}
                disabled={!selectedCandidateId}
                onChange={(e) =>
                  setInterviewStageDraft((current) => ({
                    ...current,
                    scheduledAt: e.target.value ? new Date(e.target.value).toISOString() : null,
                  }))
                }
              />
            </label>
            <label className="text-sm text-slate-300">
              Completed at
              <input
                type="datetime-local"
                className="mt-1 w-full rounded-lg border border-slate-700 bg-slate-900 px-3 py-2 text-slate-100"
                value={toDateTimeLocalValue(interviewStageDraft.completedAt)}
                disabled={!selectedCandidateId}
                onChange={(e) =>
                  setInterviewStageDraft((current) => ({
                    ...current,
                    completedAt: e.target.value ? new Date(e.target.value).toISOString() : null,
                  }))
                }
              />
            </label>
            <label className="text-sm text-slate-300">
              Interviewer person ID
              <input
                className="mt-1 w-full rounded-lg border border-slate-700 bg-slate-900 px-3 py-2 text-slate-100"
                value={interviewStageDraft.interviewerPersonId ?? ''}
                disabled={!selectedCandidateId}
                onChange={(e) =>
                  setInterviewStageDraft((current) => ({
                    ...current,
                    interviewerPersonId: e.target.value || null,
                  }))
                }
              />
            </label>
            <label className="text-sm text-slate-300">
              Score
              <input
                type="number"
                step="0.01"
                className="mt-1 w-full rounded-lg border border-slate-700 bg-slate-900 px-3 py-2 text-slate-100"
                value={interviewStageDraft.score ?? ''}
                disabled={!selectedCandidateId}
                onChange={(e) =>
                  setInterviewStageDraft((current) => ({
                    ...current,
                    score: e.target.value === '' ? null : Number(e.target.value),
                  }))
                }
              />
            </label>
            <label className="text-sm text-slate-300 md:col-span-2">
              Recommendation
              <input
                className="mt-1 w-full rounded-lg border border-slate-700 bg-slate-900 px-3 py-2 text-slate-100"
                value={interviewStageDraft.recommendation ?? ''}
                disabled={!selectedCandidateId}
                onChange={(e) =>
                  setInterviewStageDraft((current) => ({
                    ...current,
                    recommendation: e.target.value || null,
                  }))
                }
              />
            </label>
            <label className="text-sm text-slate-300 md:col-span-2">
              Notes
              <textarea
                rows={3}
                className="mt-1 w-full rounded-lg border border-slate-700 bg-slate-900 px-3 py-2 text-slate-100"
                value={interviewStageDraft.notes ?? ''}
                disabled={!selectedCandidateId}
                onChange={(e) => setInterviewStageDraft((current) => ({ ...current, notes: e.target.value || null }))}
              />
            </label>
          </div>

          <button
            type="button"
            className="mt-4 rounded-lg bg-sky-500 px-4 py-2 text-sm font-medium text-[var(--color-text-primary)] hover:bg-sky-400 disabled:opacity-50"
            disabled={!selectedCandidateId || createInterviewStageMutation.isPending || updateInterviewStageMutation.isPending}
            onClick={() => {
              if (!selectedCandidateId) return
              if (stageMode === 'create' || !selectedStageId) {
                createInterviewStageMutation.mutate({
                  ...interviewStageDraft,
                  recruitingCandidateId: selectedCandidateId,
                })
                return
              }

              updateInterviewStageMutation.mutate({
                stageId: selectedStageId,
                request: {
                  ...interviewStageDraft,
                  recruitingCandidateId: selectedCandidateId,
                },
              })
            }}
          >
            {stageMode === 'create' || !selectedStageId ? 'Create interview stage' : 'Save interview stage'}
          </button>
            {selectedStageId ? (
              <button
                type="button"
                className="mt-3 rounded-lg border border-amber-500/30 bg-amber-500/10 px-4 py-2 text-sm font-medium text-amber-200 hover:bg-amber-500/20 disabled:opacity-50"
                disabled={archiveInterviewStageMutation.isPending}
                onClick={() => {
                  if (!selectedStageId) return
                  setPendingArchive({
                    kind: 'stage',
                    id: selectedStageId,
                    message: 'Archive this interview stage?',
                  })
                }}
              >
                Archive interview stage
              </button>
          ) : null}

          <div className="mt-4 space-y-3">
            {interviewStages.map((stage) => (
              <button
                key={stage.id}
                type="button"
                className={`w-full rounded-xl border p-4 text-left transition ${
                  selectedStageId === stage.id
                    ? 'border-sky-500/50 bg-sky-500/10'
                    : 'border-slate-800 bg-slate-900/60 hover:border-slate-600'
                }`}
                onClick={() => {
                  setStageMode('edit')
                  setSelectedStageId(stage.id)
                }}
              >
                <div className="flex items-center justify-between gap-3">
                  <p className="font-medium text-slate-50">{stage.stageName}</p>
                  <span className="rounded-full bg-slate-800 px-2 py-0.5 text-[11px] uppercase tracking-[0.18em] text-slate-300">
                    {stageBadge(stage.status)}
                  </span>
                </div>
                <p className="mt-1 text-xs text-slate-400">
                  Scheduled {formatMaybeDate(stage.scheduledAt)} - Completed {formatMaybeDate(stage.completedAt)}
                </p>
                <p className="mt-1 text-xs text-[var(--color-text-muted)]">
                  Interviewer {stage.interviewerPersonId ?? 'n/a'} {stage.score != null ? `- Score ${stage.score}` : ''}
                </p>
              </button>
            ))}
            {interviewStages.length === 0 ? <p className="text-sm text-slate-400">No interview stages for the selected candidate.</p> : null}
          </div>
        </div>

        <div className="rounded-2xl border border-slate-800 bg-slate-950/70 p-5">
          <div className="flex items-start justify-between gap-3">
            <div>
              <h2 className="text-base font-semibold text-slate-50">
                {offerMode === 'create' ? 'Create offer' : 'Edit offer'}
              </h2>
              <p className="mt-1 text-sm text-slate-400">
                Draft and track the selected candidate&apos;s offer records.
              </p>
            </div>
            <button
              type="button"
              className="rounded-lg border border-slate-700 bg-slate-900 px-3 py-1.5 text-xs font-medium text-slate-200 hover:border-slate-500"
              onClick={() => {
                setOfferMode('create')
                setSelectedOfferId(null)
                setOfferDraft(emptyOfferDraft(selectedCandidateId ?? '', selectedCandidate?.candidateName ?? null))
              }}
            >
              New offer
            </button>
          </div>

          <div className="mt-4 grid gap-3 md:grid-cols-2">
            <label className="text-sm text-slate-300">
              Title
              <input
                className="mt-1 w-full rounded-lg border border-slate-700 bg-slate-900 px-3 py-2 text-slate-100"
                value={offerDraft.title}
                disabled={!selectedCandidateId}
                onChange={(e) => setOfferDraft((current) => ({ ...current, title: e.target.value }))}
              />
            </label>
            <label className="text-sm text-slate-300">
              Status
              <input
                className="mt-1 w-full rounded-lg border border-slate-700 bg-slate-900 px-3 py-2 text-slate-100"
                value={offerDraft.status}
                disabled={!selectedCandidateId}
                onChange={(e) => setOfferDraft((current) => ({ ...current, status: e.target.value }))}
              />
            </label>
            <label className="text-sm text-slate-300">
              Pay basis
              <input
                className="mt-1 w-full rounded-lg border border-slate-700 bg-slate-900 px-3 py-2 text-slate-100"
                value={offerDraft.payBasis}
                disabled={!selectedCandidateId}
                onChange={(e) => setOfferDraft((current) => ({ ...current, payBasis: e.target.value }))}
              />
            </label>
            <label className="text-sm text-slate-300">
              Start date
              <input
                type="date"
                className="mt-1 w-full rounded-lg border border-slate-700 bg-slate-900 px-3 py-2 text-slate-100"
                value={offerDraft.startDate ?? ''}
                disabled={!selectedCandidateId}
                onChange={(e) =>
                  setOfferDraft((current) => ({
                    ...current,
                    startDate: e.target.value || null,
                  }))
                }
              />
            </label>
            <label className="text-sm text-slate-300">
              Annual salary
              <input
                type="number"
                step="0.01"
                className="mt-1 w-full rounded-lg border border-slate-700 bg-slate-900 px-3 py-2 text-slate-100"
                value={offerDraft.annualSalary ?? ''}
                disabled={!selectedCandidateId}
                onChange={(e) =>
                  setOfferDraft((current) => ({
                    ...current,
                    annualSalary: e.target.value === '' ? null : Number(e.target.value),
                  }))
                }
              />
            </label>
            <label className="text-sm text-slate-300">
              Hourly rate
              <input
                type="number"
                step="0.01"
                className="mt-1 w-full rounded-lg border border-slate-700 bg-slate-900 px-3 py-2 text-slate-100"
                value={offerDraft.hourlyRate ?? ''}
                disabled={!selectedCandidateId}
                onChange={(e) =>
                  setOfferDraft((current) => ({
                    ...current,
                    hourlyRate: e.target.value === '' ? null : Number(e.target.value),
                  }))
                }
              />
            </label>
            <label className="text-sm text-slate-300">
              Approved at
              <input
                type="datetime-local"
                className="mt-1 w-full rounded-lg border border-slate-700 bg-slate-900 px-3 py-2 text-slate-100"
                value={toDateTimeLocalValue(offerDraft.approvedAt)}
                disabled={!selectedCandidateId}
                onChange={(e) =>
                  setOfferDraft((current) => ({
                    ...current,
                    approvedAt: e.target.value ? new Date(e.target.value).toISOString() : null,
                  }))
                }
              />
            </label>
            <label className="text-sm text-slate-300">
              Approved by person ID
              <input
                className="mt-1 w-full rounded-lg border border-slate-700 bg-slate-900 px-3 py-2 text-slate-100"
                value={offerDraft.approvedByPersonId ?? ''}
                disabled={!selectedCandidateId}
                onChange={(e) =>
                  setOfferDraft((current) => ({
                    ...current,
                    approvedByPersonId: e.target.value || null,
                  }))
                }
              />
            </label>
            <label className="text-sm text-slate-300">
              Notes
              <textarea
                rows={3}
                className="mt-1 w-full rounded-lg border border-slate-700 bg-slate-900 px-3 py-2 text-slate-100 md:col-span-2"
                value={offerDraft.notes ?? ''}
                disabled={!selectedCandidateId}
                onChange={(e) => setOfferDraft((current) => ({ ...current, notes: e.target.value || null }))}
              />
            </label>
          </div>

          <button
            type="button"
            className="mt-4 rounded-lg bg-[var(--color-bg-control-hover)] px-4 py-2 text-sm font-medium text-[var(--color-text-primary)] hover:bg-[var(--color-bg-surface)] disabled:opacity-50"
            disabled={!selectedCandidateId || createOfferMutation.isPending || updateOfferMutation.isPending}
            onClick={() => {
              if (!selectedCandidateId) return
              if (offerMode === 'create' || !selectedOfferId) {
                createOfferMutation.mutate({
                  ...offerDraft,
                  recruitingCandidateId: selectedCandidateId,
                })
                return
              }

              updateOfferMutation.mutate({
                offerId: selectedOfferId,
                request: {
                  ...offerDraft,
                  recruitingCandidateId: selectedCandidateId,
                },
              })
            }}
          >
            {offerMode === 'create' || !selectedOfferId ? 'Create offer' : 'Save offer'}
          </button>
            {selectedOfferId ? (
              <button
                type="button"
                className="mt-3 rounded-lg border border-amber-500/30 bg-amber-500/10 px-4 py-2 text-sm font-medium text-amber-200 hover:bg-amber-500/20 disabled:opacity-50"
                disabled={archiveOfferMutation.isPending}
                onClick={() => {
                  if (!selectedOfferId) return
                  setPendingArchive({
                    kind: 'offer',
                    id: selectedOfferId,
                    message: 'Archive this offer?',
                  })
                }}
              >
                Archive offer
              </button>
          ) : null}

          <div className="mt-4 space-y-3">
            {offers.map((offer) => (
              <button
                key={offer.id}
                type="button"
                className={`w-full rounded-xl border p-4 text-left transition ${
                  selectedOfferId === offer.id
                    ? 'border-[var(--color-border-default)] bg-[var(--color-bg-control-hover)]'
                    : 'border-slate-800 bg-slate-900/60 hover:border-slate-600'
                }`}
                onClick={() => {
                  setOfferMode('edit')
                  setSelectedOfferId(offer.id)
                }}
              >
                <div className="flex items-center justify-between gap-3">
                  <p className="font-medium text-slate-50">{offer.title}</p>
                  <span className="rounded-full bg-slate-800 px-2 py-0.5 text-[11px] uppercase tracking-[0.18em] text-slate-300">
                    {stageBadge(offer.status)}
                  </span>
                </div>
                <p className="mt-1 text-xs text-slate-400">
                  Basis {offer.payBasis} {offer.startDate ? `- Start ${formatMaybeDate(offer.startDate)}` : ''}
                </p>
                <p className="mt-1 text-xs text-[var(--color-text-muted)]">
                  Salary {offer.annualSalary ?? 'n/a'} - Rate {offer.hourlyRate ?? 'n/a'}
                </p>
              </button>
            ))}
            {offers.length === 0 ? <p className="text-sm text-slate-400">No offers for the selected candidate.</p> : null}
          </div>
        </div>
      </section>
    </div>
  )
}
