import { render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'
import { TrainingMatrixPanel } from './TrainingMatrixPanel'

describe('TrainingMatrixPanel', () => {
  it('renders matrix rows and admin form', () => {
    render(
      <TrainingMatrixPanel
        entries={[
          {
            matrixEntryId: 'm1',
            applicabilityKey: 'driver',
            applicabilityLabel: 'Driver',
            trainingProgramId: null,
            trainingProgramName: null,
            trainingDefinitionId: 'd1',
            trainingDefinitionName: 'CDL renewal',
            requirementLevel: 'required',
            sortOrder: 0,
            createdAt: '2026-05-28T12:00:00Z',
            updatedAt: '2026-05-28T12:00:00Z',
          },
        ]}
        programs={[]}
        definitions={[]}
        applicabilityKey=""
        applicabilityLabel=""
        targetType="definition"
        targetId=""
        requirementLevel="required"
        sortOrder="0"
        onApplicabilityKeyChange={vi.fn()}
        onApplicabilityLabelChange={vi.fn()}
        onTargetTypeChange={vi.fn()}
        onTargetIdChange={vi.fn()}
        onRequirementLevelChange={vi.fn()}
        onSortOrderChange={vi.fn()}
        onCreateEntry={vi.fn()}
        onDeleteEntry={vi.fn()}
        isCreating={false}
        deletingEntryId={null}
        canManage
      />,
    )
    expect(screen.getByTestId('training-matrix-panel')).toBeInTheDocument()
    expect(screen.getByText(/CDL renewal/)).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /add matrix row/i })).toBeInTheDocument()
  })
})
