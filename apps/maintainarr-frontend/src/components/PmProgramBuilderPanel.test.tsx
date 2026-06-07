import { render, screen, fireEvent } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'

vi.mock('@stl/shared-ui', async (importOriginal) => {
  const mod = await importOriginal<typeof import('@stl/shared-ui')>()
  return {
    ...mod,
    StaticSearchPicker: ({
      label,
      value,
      onChange,
      options,
      testId,
    }: {
      label?: string
      value: string
      onChange: (value: string) => void
      options: { value: string; label: string }[]
      testId?: string
    }) => (
      <label htmlFor={testId ?? 'mock-static-search-picker'}>
        {label}
        <input
          id={testId ?? 'mock-static-search-picker'}
          aria-label={label}
          data-testid={testId}
          value={value}
          onChange={(event) => onChange(event.target.value)}
        />
        <ul>
          {options.map((option) => (
            <li key={option.value}>{option.label}</li>
          ))}
        </ul>
      </label>
    ),
  }
})
import { PmProgramBuilderPanel } from './PmProgramBuilderPanel'

describe('PmProgramBuilderPanel', () => {
  const baseProps = {
    mode: 'details' as const,
    canManage: true,
    programs: [
      {
        pmProgramId: '11111111-1111-1111-1111-111111111111',
        programKey: 'forklift-pm',
        name: 'Forklift PM Program',
        scopeType: 'asset_type',
        assetTypeId: '22222222-2222-2222-2222-222222222222',
        assetTypeName: 'Forklift',
        assetId: null,
        assetTag: null,
        status: 'draft',
        scheduleCount: 2,
        createdAt: '2026-01-01T00:00:00Z',
        updatedAt: '2026-05-27T12:00:00Z',
      },
    ],
    selectedProgram: {
      pmProgramId: '11111111-1111-1111-1111-111111111111',
      programKey: 'forklift-pm',
      name: 'Forklift PM Program',
      description: 'Standard forklift preventive maintenance',
      scopeType: 'asset_type',
      assetTypeId: '22222222-2222-2222-2222-222222222222',
      assetTypeKey: 'forklift',
      assetTypeName: 'Forklift',
      assetId: null,
      assetTag: null,
      assetName: null,
      status: 'draft',
      autoGenerateWorkOrder: true,
      defaultWorkOrderTemplateRef: null,
      autoGenerateInspection: false,
      inspectionTemplateId: null,
      inspectionTemplateKey: null,
      inspectionTemplateName: null,
      schedules: [
        {
          pmScheduleId: '33333333-3333-3333-3333-333333333333',
          scheduleKey: 'oil-change',
          name: 'Oil Change',
          assetTag: 'FL-001',
          assetName: 'Forklift 1',
          dueStatus: 'scheduled',
          status: 'active',
          sortOrder: 0,
        },
      ],
      createdAt: '2026-01-01T00:00:00Z',
      updatedAt: '2026-05-27T12:00:00Z',
      categoryKey: null,
      workTypeKey: null,
      priorityKey: null,
      owningSiteRef: null,
      owningTeamRef: null,
      owningDepartmentRef: null,
      ownerPersonId: null,
      ownerRoleKey: null,
      tags: null,
      activatedAt: null,
      pausedAt: null,
      retiredAt: null,
    },
    assetTypes: [
      {
        assetTypeId: '22222222-2222-2222-2222-222222222222',
        assetClassId: '44444444-4444-4444-4444-444444444444',
        classKey: 'vehicles',
        className: 'Vehicles',
        typeKey: 'forklift',
        name: 'Forklift',
        description: '',
        status: 'active',
        createdAt: '2026-01-01T00:00:00Z',
      },
    ],
    assets: [],
    availableSchedules: [
      {
        pmScheduleId: '33333333-3333-3333-3333-333333333333',
        assetId: '55555555-5555-5555-5555-555555555555',
        assetTag: 'FL-001',
        assetName: 'Forklift 1',
        scheduleKey: 'oil-change',
        name: 'Oil Change',
        description: '',
        scheduleMode: 'calendar',
        assetMeterId: null,
        meterKey: null,
        meterUnit: null,
        intervalUsage: null,
        nextDueAtUsage: null,
        lastCompletedUsage: null,
        intervalDays: 90,
        nextDueAt: '2026-06-01T00:00:00Z',
        lastCompletedAt: null,
        dueStatus: 'scheduled',
        status: 'active',
        lastDueScanAt: null,
        linkedWorkOrderId: null,
        linkedWorkOrderNumber: null,
        linkedWorkOrderStatus: null,
        createdAt: '2026-01-01T00:00:00Z',
        updatedAt: '2026-01-01T00:00:00Z',
      },
    ],
    isLoading: false,
    isDetailLoading: false,
    isSchedulesLoading: false,
    programKey: '',
    programName: '',
    programDescription: '',
    scopeType: 'asset_type',
    selectedAssetTypeId: '',
    selectedAssetId: '',
    selectedProgramId: '11111111-1111-1111-1111-111111111111',
    selectedScheduleIds: ['33333333-3333-3333-3333-333333333333'],
    onProgramKeyChange: vi.fn(),
    onProgramNameChange: vi.fn(),
    onProgramDescriptionChange: vi.fn(),
    onScopeTypeChange: vi.fn(),
    onSelectedAssetTypeIdChange: vi.fn(),
    onSelectedAssetIdChange: vi.fn(),
    onSelectedProgramIdChange: vi.fn(),
    onSelectedScheduleIdsChange: vi.fn(),
    onCreateProgram: vi.fn(),
    onSaveSchedules: vi.fn(),
    onActivateProgram: vi.fn(),
    isCreatingProgram: false,
    isSavingSchedules: false,
  }

  it('renders program list and selected program schedules', () => {
    render(<PmProgramBuilderPanel {...baseProps} />)

    expect(screen.getByText('PM program builder')).toBeInTheDocument()
    expect(screen.getAllByText('Forklift PM Program').length).toBeGreaterThan(0)
    expect(screen.getByText('Assigned schedules')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Save schedule assignments' })).toBeInTheDocument()
  })

  it('shows empty state when no programs exist', () => {
    render(
      <PmProgramBuilderPanel
        {...baseProps}
        programs={[]}
        selectedProgram={null}
        selectedProgramId=""
      />,
    )

    expect(screen.getByText('No PM programs yet.')).toBeInTheDocument()
  })

  it('uses a searchable picker for asset type scoping', () => {
    const onSelectedAssetTypeIdChange = vi.fn()

    render(
      <PmProgramBuilderPanel
        {...baseProps}
        mode="create"
        programName="Forklift PM"
        programDescription="desc"
        selectedAssetTypeId=""
        selectedAssetId=""
        scopeType="asset_type"
        assets={[]}
        onSelectedAssetTypeIdChange={onSelectedAssetTypeIdChange}
        onSelectedAssetIdChange={vi.fn()}
      />,
    )

    expect(screen.getByTestId('pmprogrambuilder-asset-type-picker')).toBeInTheDocument()
    fireEvent.change(screen.getByTestId('pmprogrambuilder-asset-type-picker'), {
      target: { value: '22222222-2222-2222-2222-222222222222' },
    })
    expect(onSelectedAssetTypeIdChange).toHaveBeenCalledWith('22222222-2222-2222-2222-222222222222')
  })

  it('uses a searchable picker for single-asset scoping', () => {
    const onSelectedAssetIdChange = vi.fn()

    render(
      <PmProgramBuilderPanel
        {...baseProps}
        mode="create"
        programName="Forklift PM"
        programDescription="desc"
        scopeType="asset"
        selectedAssetTypeId=""
        selectedAssetId=""
        assets={[
        {
          assetId: '55555555-5555-5555-5555-555555555555',
          assetTypeId: '22222222-2222-2222-2222-222222222222',
          typeKey: 'forklift',
          typeName: 'Forklift',
          classKey: 'vehicles',
          className: 'Vehicles',
          assetTag: 'FL-001',
          name: 'Forklift 1',
          description: '',
          lifecycleStatus: 'active',
          siteRef: null,
          createdAt: '2026-01-01T00:00:00Z',
          updatedAt: '2026-01-01T00:00:00Z',
        },
        ]}
        onSelectedAssetTypeIdChange={vi.fn()}
        onSelectedAssetIdChange={onSelectedAssetIdChange}
      />,
    )

    expect(screen.getByTestId('pmprogrambuilder-asset-picker')).toBeInTheDocument()
    fireEvent.change(screen.getByTestId('pmprogrambuilder-asset-picker'), {
      target: { value: '55555555-5555-5555-5555-555555555555' },
    })
    expect(onSelectedAssetIdChange).toHaveBeenCalledWith('55555555-5555-5555-5555-555555555555')
  })
})
