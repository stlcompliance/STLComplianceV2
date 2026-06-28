import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import * as client from '../api/client'
import type { TrainArrTenantSettingsPayload, TrainArrTenantSettingsResponse } from '../api/types'
import { TenantSettingsPanel } from './TenantSettingsPanel'

vi.mock('../api/client', async (importOriginal) => {
  const actual = await importOriginal<typeof import('../api/client')>()
  return {
    ...actual,
    getTrainArrTenantSettings: vi.fn(),
    getTrainArrTenantSettingsDefaults: vi.fn(),
    putTrainArrTenantSettings: vi.fn(),
  }
})

describe('TenantSettingsPanel', () => {
  afterEach(() => {
    cleanup()
    vi.clearAllMocks()
  })

  it('renders all canonical tenant settings sections', async () => {
    mockSettings()
    renderPanel()

    expect(await screen.findByTestId('trainarr-tenant-settings-panel')).toBeInTheDocument()
    const sections = [
      'assignment',
      'program-versioning',
      'certifications',
      'completion-and-signoff',
      'evaluations',
      'remediation',
      'evidence-and-records',
      'notifications',
      'enforcement',
      'external-training',
      'trainers-and-evaluators',
      'compliance-core',
      'audit-and-retention',
    ]
    for (const section of sections) {
      expect(screen.getByTestId(`tenant-settings-section-${section}`)).toBeInTheDocument()
    }
  })

  it('saves edited settings with the current row version', async () => {
    mockSettings()
    vi.mocked(client.putTrainArrTenantSettings).mockResolvedValue(
      buildResponse(defaultSettings({ assignmentDueDays: 21 }), 2),
    )
    renderPanel()

    fireEvent.change(await screen.findByLabelText(/Default due days/i), {
      target: { value: '21' },
    })
    fireEvent.click(screen.getByTestId('tenant-settings-save'))

    await waitFor(() => {
      expect(client.putTrainArrTenantSettings).toHaveBeenCalledWith(
        'token',
        expect.objectContaining({
          rowVersion: 1,
          settings: expect.objectContaining({
            assignment: expect.objectContaining({ defaultAssignmentDueDays: 21 }),
          }),
        }),
      )
    })
  })

  it('resets the draft to canonical defaults without exposing raw JSON', async () => {
    mockSettings({ current: defaultSettings({ assignmentDueDays: 30 }) })
    renderPanel()

    const defaultDueDays = await screen.findByLabelText(/Default due days/i)
    expect(defaultDueDays).toHaveValue(30)

    fireEvent.click(screen.getByTestId('tenant-settings-reset'))

    expect(screen.getByLabelText(/Default due days/i)).toHaveValue(14)
    expect(screen.queryByText(/raw json/i)).not.toBeInTheDocument()
  })

  it('lets managers read settings but disables write actions', async () => {
    mockSettings()
    renderPanel({ canManage: false })

    expect(await screen.findByText(/only TrainArr tenant administrators can change them/i)).toBeInTheDocument()
    expect(screen.getByTestId('tenant-settings-save')).toBeDisabled()
    expect(screen.getByTestId('tenant-settings-reset')).toBeDisabled()
  })

  it('shows validation save errors from the API', async () => {
    mockSettings()
    vi.mocked(client.putTrainArrTenantSettings).mockRejectedValue(new Error('validation failed'))
    renderPanel()

    fireEvent.change(await screen.findByLabelText(/Default due days/i), {
      target: { value: '22' },
    })
    fireEvent.click(screen.getByTestId('tenant-settings-save'))

    expect(await screen.findByText('Save failed')).toBeInTheDocument()
    expect(await screen.findByText('validation failed')).toBeInTheDocument()
  })
})

function renderPanel({ canManage = true }: { canManage?: boolean } = {}) {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  render(
    <QueryClientProvider client={queryClient}>
      <TenantSettingsPanel accessToken="token" canRead canManage={canManage} />
    </QueryClientProvider>,
  )
}

function mockSettings({
  current = defaultSettings(),
  defaults = defaultSettings(),
}: {
  current?: TrainArrTenantSettingsPayload
  defaults?: TrainArrTenantSettingsPayload
} = {}) {
  vi.mocked(client.getTrainArrTenantSettings).mockResolvedValue(buildResponse(current, 1))
  vi.mocked(client.getTrainArrTenantSettingsDefaults).mockResolvedValue({
    productKey: 'trainarr',
    scope: 'tenant',
    schemaVersion: 1,
    settings: defaults,
  })
}

function buildResponse(settings: TrainArrTenantSettingsPayload, rowVersion: number): TrainArrTenantSettingsResponse {
  return {
    productKey: 'trainarr',
    scope: 'tenant',
    schemaVersion: 1,
    settings,
    updatedByDisplayName: 'TrainArr administrator',
    createdAt: '2026-06-18T00:00:00Z',
    updatedAt: '2026-06-18T00:00:00Z',
    rowVersion,
  }
}

function defaultSettings({
  assignmentDueDays = 14,
}: {
  assignmentDueDays?: number
} = {}): TrainArrTenantSettingsPayload {
  return {
    assignment: {
      autoAssignOnHire: true,
      autoAssignOnPositionChange: true,
      autoAssignOnSiteChange: true,
      autoAssignOnDepartmentChange: true,
      allowManagerAssignment: true,
      allowSelfEnrollment: true,
      optionalEnrollmentRequiresApproval: false,
      defaultAssignmentDueDays: assignmentDueDays,
      assignmentGracePeriodDays: 3,
      assignmentPriorityDefault: 'normal',
    },
    programVersioning: {
      programVersionChangePolicy: 'expired_or_incomplete',
      reassignOnMajorVersion: true,
      reassignOnMinorVersion: false,
      allowInProgressVersionCompletion: true,
      requireReasonForProgramPublish: true,
      archiveSupersededPrograms: true,
    },
    certifications: {
      defaultCertificateValidityDays: 365,
      defaultRenewalWindowDays: 60,
      defaultExpirationWarningDays: [90, 60, 30, 14, 7, 1],
      allowEarlyRenewal: true,
      allowExpiredRenewal: true,
      expiredQualificationBlocksWork: true,
      certificateNumberFormat: 'TRN-{tenantCode}-{yyyy}-{sequence}',
      requireCertificatePdf: true,
      certificateDisplayNameFormat: null,
    },
    completionSignoff: {
      defaultCompletionMode: 'trainer',
      requireTrainerSignoff: true,
      requireTraineeAcknowledgement: true,
      requireManagerApproval: false,
      allowBulkCompletion: true,
      bulkCompletionRequiresReason: true,
      allowBackdatedCompletion: true,
      backdatedCompletionMaxDays: 30,
      requireReasonForBackdating: true,
      completionEditPolicy: 'admin_correction_only',
    },
    evaluations: {
      defaultPassingScorePercent: 80,
      allowRetakes: true,
      maxRetakeAttempts: 3,
      retakeCooldownHours: 24,
      randomizeQuestionOrder: false,
      randomizeAnswerOrder: false,
      showCorrectAnswersAfterAttempt: false,
      requireEvaluatorCommentOnFail: true,
      requireEvaluatorCommentOnOverride: true,
    },
    remediation: {
      acceptIncidentRetrainingRequests: true,
      incidentRetrainingDefaultDueDays: 7,
      incidentRetrainingRequiresReview: true,
      autoAssignRemediationOnIncident: false,
      repeatIncidentEscalationThreshold: 2,
      repeatIncidentLookbackDays: 180,
      notifyManagerOnRemediation: true,
      blockQualificationDuringRemediation: false,
    },
    evidenceRecords: {
      requireEvidenceForCompletion: false,
      allowedEvidenceTypes: [
        'pdf',
        'image',
        'video',
        'external_url',
        'signature',
        'form',
        'completion_certificate',
        'evaluation_sheet',
        'signoff_form',
        'practical_demo',
        'attendance_roster',
        'quiz_result',
      ],
      maxEvidenceFileSizeMb: 25,
      evidenceRetentionYears: 7,
      allowExternalEvidenceUrl: true,
      requireEvidenceReview: false,
      allowTraineeEvidenceUpload: false,
      allowTrainerEvidenceUpload: true,
      sendFinalRecordsToRecordArr: true,
    },
    notifications: {
      notifyOnAssignmentCreated: true,
      notifyOnDueSoon: true,
      dueSoonReminderDays: [14, 7, 1],
      notifyOnOverdue: true,
      overdueReminderCadenceDays: 7,
      notifyManagerOnOverdue: true,
      notifyAdminOnCriticalOverdue: true,
      notifyOnCertificateIssued: true,
      notifyOnCertificateExpiring: true,
      certificateExpirationWarningDays: [90, 60, 30, 14, 7, 1],
    },
    enforcement: {
      exposeQualificationStatusToProducts: true,
      allowProductsToBlockWork: true,
      defaultWorkBlockMode: 'manager_override_required',
      allowManagerOverrideOfBlock: true,
      overrideRequiresReason: true,
      overrideDurationHours: 24,
      publishQualificationEvents: true,
    },
    externalTraining: {
      allowExternalTrainingProvider: true,
      externalCompletionRequiresReview: true,
      externalCertificateRequiresEvidence: true,
      trustedProviderIds: [],
      allowManualExternalCompletionEntry: true,
      externalRecordConfidenceDefault: 'medium',
    },
    trainersEvaluators: {
      trainerMustBeQualified: true,
      trainerQualificationRequiredDays: 0,
      allowTrainerSelfSignoff: false,
      allowManagerAsEvaluator: true,
      requireDifferentTrainerAndEvaluator: false,
      evaluatorConflictPolicy: 'warn',
      trainerRosterSource: 'both',
    },
    complianceCore: {
      complianceCoreEnabled: true,
      requireComplianceCoreProgramMapping: false,
      allowUnmappedInternalPrograms: true,
      citationDisplayMode: 'trainer_and_admin',
      regulatoryChangeReviewRequired: true,
      autoCreateReviewTasksFromRuleChanges: true,
    },
    auditCorrection: {
      requireCorrectionReason: true,
      requireAdminReasonForDeletion: true,
      allowCertificateRevocation: true,
      revocationRequiresReason: true,
      retainVoidedRecords: true,
      auditEventRetentionYears: 7,
    },
  }
}
