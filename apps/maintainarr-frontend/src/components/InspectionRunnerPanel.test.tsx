import { render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'
import { InspectionRunnerPanel } from './InspectionRunnerPanel'

describe('InspectionRunnerPanel', () => {
  const baseProps = {
    canExecute: true,
    viewAllRuns: true,
    assets: [
      {
        assetId: '11111111-1111-1111-1111-111111111111',
        assetTypeId: '22222222-2222-2222-2222-222222222222',
        typeKey: 'forklift',
        typeName: 'Forklift',
        classKey: 'vehicles',
        className: 'Vehicles',
        assetTag: 'FL-001',
        name: 'Yard Forklift',
        description: '',
        lifecycleStatus: 'active',
        siteRef: 'yard-a',
        createdAt: '2026-01-01T00:00:00Z',
        updatedAt: '2026-01-01T00:00:00Z',
      },
    ],
    activeTemplates: [
      {
        inspectionTemplateId: '33333333-3333-3333-3333-333333333333',
        templateKey: 'pre-trip',
        name: 'Pre-Trip',
        description: '',
        version: 1,
        status: 'active',
        categoryCount: 0,
        checklistItemCount: 1,
        linkedAssetTypeCount: 1,
        createdAt: '2026-01-01T00:00:00Z',
        updatedAt: '2026-01-01T00:00:00Z',
      },
    ],
    runs: [
      {
        inspectionRunId: '44444444-4444-4444-4444-444444444444',
        assetId: '11111111-1111-1111-1111-111111111111',
        assetTag: 'FL-001',
        assetName: 'Yard Forklift',
        inspectionTemplateId: '33333333-3333-3333-3333-333333333333',
        templateKey: 'pre-trip',
        templateName: 'Pre-Trip',
        templateVersion: 1,
        status: 'in_progress',
        result: null,
        startedByUserId: '55555555-5555-5555-5555-555555555555',
        startedAt: '2026-05-27T12:00:00Z',
        completedAt: null,
        answerCount: 0,
        requiredItemCount: 1,
      },
    ],
    activeRun: {
      inspectionRunId: '44444444-4444-4444-4444-444444444444',
      assetId: '11111111-1111-1111-1111-111111111111',
      assetTag: 'FL-001',
      assetName: 'Yard Forklift',
      inspectionTemplateId: '33333333-3333-3333-3333-333333333333',
      templateKey: 'pre-trip',
      templateName: 'Pre-Trip',
      templateVersion: 1,
      status: 'in_progress',
      result: null,
      startedByUserId: '55555555-5555-5555-5555-555555555555',
      startedAt: '2026-05-27T12:00:00Z',
      completedAt: null,
      updatedAt: '2026-05-27T12:00:00Z',
      checklistItems: [
        {
          checklistItemId: '66666666-6666-6666-6666-666666666666',
          categoryId: null,
          categoryKey: null,
          itemKey: 'brakes-ok',
          prompt: 'Brakes operate correctly',
          itemType: 'pass_fail',
          isRequired: true,
          sortOrder: 10,
        },
      ],
      answers: [],
    },
    selectedAssetId: '',
    selectedTemplateId: '',
    selectedRunId: '44444444-4444-4444-4444-444444444444',
    answerDrafts: {},
    isLoading: false,
    isRunLoading: false,
    isStarting: false,
    isSubmitting: false,
    isCompleting: false,
    isCreatingDefects: false,
    voiceGuidanceEnabled: false,
    voiceGuidanceSupported: false,
    voiceGuidanceLoading: false,
    currentVoicePrompt: null,
    voiceStatusMessage: null,
    isVoiceListening: false,
    onVoiceGuidanceEnabledChange: vi.fn(),
    onReadCurrentPrompt: vi.fn(),
    onListenForAnswer: vi.fn(),
    onSelectedAssetIdChange: vi.fn(),
    onSelectedTemplateIdChange: vi.fn(),
    onSelectedRunIdChange: vi.fn(),
    onAnswerDraftChange: vi.fn(),
    onStartRun: vi.fn(),
    onSubmitAnswers: vi.fn(),
    onCompleteRun: vi.fn(),
    onCreateDefectsFromRun: vi.fn(),
  }

  it('renders run list and active checklist', () => {
    render(<InspectionRunnerPanel {...baseProps} />)

    expect(screen.getByText('Run inspection')).toBeInTheDocument()
    expect(screen.getByText('FL-001')).toBeInTheDocument()
    expect(screen.getByText('Brakes operate correctly')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Complete run' })).toBeInTheDocument()
  })

  it('shows empty runs message when no history', () => {
    render(<InspectionRunnerPanel {...baseProps} runs={[]} activeRun={null} selectedRunId="" />)

    expect(screen.getByText('No inspection runs yet.')).toBeInTheDocument()
  })

  it('shows capture defects action for failed completed runs', () => {
    render(
      <InspectionRunnerPanel
        {...baseProps}
        activeRun={{
          ...baseProps.activeRun!,
          status: 'completed',
          result: 'failed',
          completedAt: '2026-05-27T12:30:00Z',
        }}
      />,
    )

    expect(screen.getByRole('button', { name: 'Capture defects from run' })).toBeInTheDocument()
  })
})
