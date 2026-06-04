import { render, screen } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import { describe, expect, it, vi } from 'vitest'

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

import { WorkOrderProfile } from './MaintenanceDetailProfiles'

describe('WorkOrderProfile', () => {
  it('renders supply readiness inside the work order detail profile', () => {
    const state = {
      selectedWorkOrderId: 'wo-1',
      workOrdersQuery: {
        data: [
          {
            workOrderId: 'wo-1',
            workOrderNumber: 'WO-100',
            assetTag: 'FLT-01',
            assetName: 'Forklift 01',
            title: 'Replace brake pads',
            priority: 'high',
            status: 'open',
            source: 'defect',
            assignedTechnicianPersonId: 'person-1',
            defectId: 'def-1',
            pmScheduleId: null,
            createdAt: '2026-05-29T00:00:00Z',
          },
        ],
      },
      workOrderDetailQuery: {
        data: {
          workOrderId: 'wo-1',
          workOrderNumber: 'WO-100',
          assetId: 'asset-1',
          assetTag: 'FLT-01',
          assetName: 'Forklift 01',
          defectId: 'def-1',
          defectTitle: 'Brake issue',
          pmScheduleId: null,
          pmScheduleName: null,
          title: 'Replace brake pads',
          description: 'Replace pads and inspect calipers',
          priority: 'high',
          status: 'open',
          source: 'defect',
          assignedTechnicianPersonId: 'person-1',
          createdByUserId: 'user-1',
          createdAt: '2026-05-29T00:00:00Z',
          updatedAt: '2026-05-29T00:00:00Z',
          startedAt: null,
          completedAt: null,
          cancelledAt: null,
        },
      },
      workOrderTasksQuery: {
        data: [
          {
            taskLineId: 'task-1',
            title: 'Inspect pads',
            status: 'completed',
          },
        ],
      },
      workOrderLaborQuery: {
        data: [],
      },
      workOrderCommentsQuery: {
        data: [],
      },
      workOrderTimelineQuery: {
        data: [],
      },
      workOrderEvidenceQuery: {
        data: [],
      },
      canExecuteInspections: true,
      session: {
        personId: 'person-1',
      },
      technicianRefs: [
        {
          personId: 'person-1',
          displayName: 'Alex Tech',
          activeStatus: 'active',
          primarySite: 'Central Maintenance',
        },
      ],
      woCommentBody: '',
      woCommentVisibility: 'internal',
      woCommentPinned: false,
      setWoCommentBody: () => {},
      setWoCommentVisibility: () => {},
      setWoCommentPinned: () => {},
      addWorkOrderCommentMutation: {
        isPending: false,
        mutate: () => {},
      },
      woTaskTitle: '',
      setWoTaskTitle: () => {},
      woLaborHours: '1',
      setWoLaborHours: () => {},
      woLaborTypeKey: 'regular',
      setWoLaborTypeKey: () => {},
      woLaborPersonId: 'person-1',
      setWoLaborPersonId: () => {},
      woSelectedTaskLineId: '',
      setWoSelectedTaskLineId: () => {},
      woEvidenceTypeKey: 'completion_photo',
      setWoEvidenceTypeKey: () => {},
      woEvidenceNotes: '',
      setWoEvidenceNotes: () => {},
      woEvidenceFile: null,
      setWoEvidenceFile: () => {},
      addWorkOrderTaskMutation: {
        isPending: false,
        mutate: () => {},
      },
      logWorkOrderLaborMutation: {
        isPending: false,
        mutate: () => {},
      },
      uploadWorkOrderEvidenceMutation: {
        isPending: false,
        mutate: () => {},
      },
      workOrderPartsDemandQuery: {
        data: [
          {
            demandLineId: 'demand-1',
          },
        ],
      },
      workOrderSupplyReadinessQuery: {
        isLoading: false,
        data: {
          workOrderId: 'wo-1',
          workOrderNumber: 'WO-100',
          generatedAt: '2026-05-29T00:00:00Z',
          overallReadinessStatus: 'not_ready',
          totalDemandLines: 1,
          linesChecked: 1,
          linesReady: 0,
          linesBlocked: 1,
          linesSkipped: 0,
          lines: [
            {
              demandLineId: 'demand-1',
              lineNumber: 1,
              supplyarrPartId: 'part-1',
              partNumber: 'BRK-001',
              quantityRequested: 2,
              lineStatus: 'pending',
              readinessStatus: 'not_ready',
              readinessBasis: 'availability',
              skipReason: null,
              quantityAvailable: 0,
              calculatedAt: '2026-05-29T00:00:00Z',
              blockers: [
                {
                  reasonCode: 'part_stockout',
                  message: 'Insufficient available quantity.',
                  sourceEntityType: 'part_stock',
                  sourceEntityId: 'part-1',
                  relatedEntityId: null,
                },
              ],
            },
          ],
        },
      },
    } as any

    render(
      <MemoryRouter>
        <WorkOrderProfile state={state} />
      </MemoryRouter>,
    )

    expect(screen.getByText('Supply readiness')).toBeInTheDocument()
    expect(screen.getByText('Labor and evidence')).toBeInTheDocument()
    expect(screen.getByTestId('work-order-supply-readiness-panel')).toBeInTheDocument()
    expect(screen.getByTestId('work-order-supply-readiness-overall')).toHaveTextContent('Supply blocked')
  })
})
