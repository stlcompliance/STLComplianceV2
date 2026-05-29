import { render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'
import { WorkOrderLaborEvidencePanel } from './WorkOrderLaborEvidencePanel'

const baseProps = {
  tasks: [],
  labor: [],
  evidence: [],
  canPerform: true,
  sessionPersonId: 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb',
  technicianRefs: [],
  taskTitle: '',
  laborHours: '1',
  laborTypeKey: 'regular',
  laborPersonId: '',
  selectedTaskLineId: '',
  evidenceTypeKey: 'before_photo',
  evidenceNotes: '',
  selectedFileName: null,
  onTaskTitleChange: vi.fn(),
  onLaborHoursChange: vi.fn(),
  onLaborTypeKeyChange: vi.fn(),
  onLaborPersonIdChange: vi.fn(),
  onSelectedTaskLineIdChange: vi.fn(),
  onEvidenceTypeKeyChange: vi.fn(),
  onEvidenceNotesChange: vi.fn(),
  onSelectFile: vi.fn(),
  onAddTask: vi.fn(),
  onLogLabor: vi.fn(),
  onUploadEvidence: vi.fn(),
  isAddingTask: false,
  isLoggingLabor: false,
  isUploadingEvidence: false,
}

describe('WorkOrderLaborEvidencePanel', () => {
  it('prompts to select a work order when none selected', () => {
    render(<WorkOrderLaborEvidencePanel {...baseProps} workOrder={null} />)
    expect(screen.getByText(/Select a work order to manage tasks/i)).toBeInTheDocument()
  })

  it('shows labor and evidence sections for open work order', () => {
    render(
      <WorkOrderLaborEvidencePanel
        {...baseProps}
        workOrder={{
          workOrderId: '33333333-3333-3333-3333-333333333333',
          workOrderNumber: 'WO-1',
          assetId: '11111111-1111-1111-1111-111111111111',
          assetTag: 'FL-1',
          assetName: 'Forklift',
          defectId: null,
          defectTitle: null,
          pmScheduleId: null,
          pmScheduleName: null,
          title: 'Repair',
          description: '',
          priority: 'medium',
          status: 'open',
          source: 'manual',
          assignedTechnicianPersonId: null,
          createdByUserId: '55555555-5555-5555-5555-555555555555',
          createdAt: '2026-05-27T12:00:00Z',
          updatedAt: '2026-05-27T12:00:00Z',
          startedAt: null,
          completedAt: null,
          cancelledAt: null,
        }}
      />,
    )

    expect(screen.getByText('Task lines')).toBeInTheDocument()
    expect(screen.getByText('Labor')).toBeInTheDocument()
    expect(screen.getByText('Evidence')).toBeInTheDocument()
    expect(screen.getByTestId('work-order-evidence-type')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Log labor' })).toBeInTheDocument()
  })
})
