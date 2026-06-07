import { cleanup, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

vi.mock('@stl/shared-ui', async () => {
  const actual = await vi.importActual<typeof import('@stl/shared-ui')>('@stl/shared-ui')
  return {
    ...actual,
    StaticSearchPicker: ({
      label,
      value,
      options,
      onChange,
      placeholder,
      testId,
    }: {
      label?: string
      value: string
      options: Array<{ value: string; label: string }>
      onChange: (value: string) => void
      placeholder?: string
      testId?: string
    }) => (
      <label>
        {label ? <span>{label}</span> : null}
        <select
          aria-label={label ?? placeholder ?? 'Static search picker'}
          data-testid={testId}
          value={value}
          onChange={(event) => onChange(event.target.value)}
        >
          <option value="">{placeholder ?? 'Select…'}</option>
          {options.map((option) => (
            <option key={option.value} value={option.value}>
              {option.label}
            </option>
          ))}
        </select>
      </label>
    ),
  }
})

import { InspectionRunnerPanel } from './InspectionRunnerPanel'

describe('InspectionRunnerPanel', () => {
  afterEach(() => {
    cleanup()
  })

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
          controlledOptions: [],
          isRequired: true,
          sortOrder: 10,
        },
      ],
      answers: [],
      pauseEvents: [],
    },
    selectedAssetId: '',
    selectedTemplateId: '',
    selectedRunId: '44444444-4444-4444-4444-444444444444',
    answerDrafts: {},
    pauseReason: 'interrupted',
    pauseNotes: '',
    isLoading: false,
    isRunLoading: false,
    isStarting: false,
    isSubmitting: false,
    isPausing: false,
    isResuming: false,
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
    onPauseReasonChange: vi.fn(),
    onPauseNotesChange: vi.fn(),
    onStartRun: vi.fn(),
    onSubmitAnswers: vi.fn(),
    onPauseRun: vi.fn(),
    onResumeRun: vi.fn(),
    onCompleteRun: vi.fn(),
    onCreateDefectsFromRun: vi.fn(),
    runEvidence: [],
    evidenceChecklistItemId: '',
    evidenceTypeKey: 'inspection_photo',
    evidenceNotes: '',
    selectedEvidenceFileName: null,
    isEvidenceLoading: false,
    isUploadingEvidence: false,
    onEvidenceChecklistItemIdChange: vi.fn(),
    onEvidenceTypeKeyChange: vi.fn(),
    onEvidenceNotesChange: vi.fn(),
    onSelectEvidenceFile: vi.fn(),
    onUploadEvidence: vi.fn(),
  }

  it('renders run list and active checklist', () => {
    render(<InspectionRunnerPanel {...baseProps} />)

    expect(screen.getByText('Run inspection')).toBeInTheDocument()
    expect(screen.getByText('FL-001')).toBeInTheDocument()
    expect(screen.getAllByText('Brakes operate correctly').length).toBeGreaterThan(0)
    expect(screen.getByRole('button', { name: 'Pause run' })).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Complete run' })).toBeInTheDocument()
    expect(screen.getByTestId('inspection-run-evidence-panel')).toBeInTheDocument()
    expect(screen.getByLabelText('Asset for inspection')).toBeInTheDocument()
    expect(screen.getByLabelText('Active inspection template')).toBeInTheDocument()
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

  it('shows resume controls and pause history for paused runs', () => {
    render(
      <InspectionRunnerPanel
        {...baseProps}
        activeRun={{
          ...baseProps.activeRun!,
          status: 'paused',
          pauseEvents: [
            {
              pauseEventId: '88888888-8888-8888-8888-888888888888',
              pausedAt: '2026-05-27T12:10:00Z',
              resumedAt: null,
              durationMinutes: null,
              reason: 'waiting_parts',
              notes: 'Waiting on replacement filter.',
              pausedByUserId: '55555555-5555-5555-5555-555555555555',
              resumedByUserId: null,
            },
          ],
        }}
      />,
    )

    expect(screen.getByRole('button', { name: 'Resume run' })).toBeInTheDocument()
    expect(screen.getByText('Pause history')).toBeInTheDocument()
    expect(screen.getAllByText(/waiting parts/i).length).toBeGreaterThan(0)
  })

  it('renders yes/no checklist items with a yes/no control', () => {
    render(
      <InspectionRunnerPanel
        {...baseProps}
        activeRun={{
          ...baseProps.activeRun!,
          checklistItems: [
            {
              ...baseProps.activeRun!.checklistItems[0],
              itemType: 'yes_no',
              prompt: 'Seat belt fastened',
              itemKey: 'seat-belt-fastened',
            },
          ],
        }}
      />,
    )

    expect(screen.getByRole('combobox', { name: /Seat belt fastened/ })).toBeInTheDocument()
  })

  it('renders selectable checklist items with controlled options', () => {
    render(
      <InspectionRunnerPanel
        {...baseProps}
        activeRun={{
          ...baseProps.activeRun!,
          checklistItems: [
            {
              ...baseProps.activeRun!.checklistItems[0],
              itemType: 'select',
              prompt: 'Cab door position',
              itemKey: 'cab-door-position',
              controlledOptions: ['Open', 'Closed'],
            },
          ],
        }}
      />,
    )

    expect(screen.getByRole('combobox', { name: /Cab door position/ })).toBeInTheDocument()
  })

  it('renders meter reading checklist items with unit and range hints', () => {
    render(
      <InspectionRunnerPanel
        {...baseProps}
        activeRun={{
          ...baseProps.activeRun!,
          checklistItems: [
            {
              ...baseProps.activeRun!.checklistItems[0],
              itemType: 'meter_reading',
              prompt: 'Engine hour meter',
              itemKey: 'engine-hour-meter',
              unitOfMeasure: 'hours',
              acceptableRangeMin: 100,
              acceptableRangeMax: 500,
            },
          ],
        }}
      />,
    )

    expect(screen.getByRole('spinbutton', { name: /Engine hour meter/ })).toBeInTheDocument()
    expect(screen.getByText('hours')).toBeInTheDocument()
    expect(screen.getByText(/Min 100/)).toBeInTheDocument()
  })

  it('renders evidence guidance for photo and signature checklist items', () => {
    render(
      <InspectionRunnerPanel
        {...baseProps}
        activeRun={{
          ...baseProps.activeRun!,
          checklistItems: [
            {
              ...baseProps.activeRun!.checklistItems[0],
              itemType: 'photo',
              prompt: 'Upload a photo of the data plate',
              itemKey: 'data-plate-photo',
            },
          ],
        }}
        runEvidence={[
          {
            evidenceId: '77777777-7777-7777-7777-777777777777',
            inspectionRunId: '44444444-4444-4444-4444-444444444444',
            checklistItemId: '66666666-6666-6666-6666-666666666666',
            evidenceTypeKey: 'inspection_photo',
            fileName: 'data-plate.jpg',
            contentType: 'image/jpeg',
            sizeBytes: 1024,
            notes: null,
            uploadedByUserId: '55555555-5555-5555-5555-555555555555',
            createdAt: '2026-05-27T12:05:00Z',
          },
        ]}
      />,
    )

    expect(screen.getByText(/Use the inspection evidence panel below to upload a photo/)).toBeInTheDocument()
    expect(screen.getAllByText('data-plate.jpg').length).toBeGreaterThan(0)
  })
})
