import { cleanup, fireEvent, render, screen } from '@testing-library/react'
import { useState } from 'react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import { AssignmentLaborPanel } from './AssignmentLaborPanel'

function Harness({
  laborEntries = [],
  onAddLaborEntry,
  onRemoveLaborEntry,
}: {
  laborEntries?: Array<{
    laborEntryId: string
    trainingAssignmentId: string
    laborTypeKey: string
    hoursWorked: number
    costPerHour: number
    totalCost: number
    notes: string | null
    loggedByUserId: string | null
    loggedAt: string
    createdAt: string
  }>
  onAddLaborEntry: () => void
  onRemoveLaborEntry: (laborEntryId: string) => Promise<void>
}) {
  const [laborTypeKey, setLaborTypeKey] = useState('delivery')
  const [hoursWorked, setHoursWorked] = useState('1')
  const [costPerHour, setCostPerHour] = useState('50')
  const [notes, setNotes] = useState('Travel time')

  return (
    <AssignmentLaborPanel
      laborEntries={laborEntries}
      canManage
      laborTypeKey={laborTypeKey}
      hoursWorked={hoursWorked}
      costPerHour={costPerHour}
      notes={notes}
      onLaborTypeKeyChange={setLaborTypeKey}
      onHoursWorkedChange={setHoursWorked}
      onCostPerHourChange={setCostPerHour}
      onNotesChange={setNotes}
      onAddLaborEntry={onAddLaborEntry}
      onRemoveLaborEntry={onRemoveLaborEntry}
      isAdding={false}
      removingId={null}
    />
  )
}

describe('AssignmentLaborPanel', () => {
  afterEach(() => {
    cleanup()
    vi.clearAllMocks()
  })

  it('renders labor totals and add controls', () => {
    const onAddLaborEntry = vi.fn()

    render(
      <Harness
        laborEntries={[
          {
            laborEntryId: 'lab-1',
            trainingAssignmentId: 'asg-1',
            laborTypeKey: 'delivery',
            hoursWorked: 1.5,
            costPerHour: 50,
            totalCost: 75,
            notes: 'Prep time',
            loggedByUserId: 'user-1',
            loggedAt: '2026-05-27T00:00:00Z',
            createdAt: '2026-05-27T00:00:00Z',
          },
        ]}
        onAddLaborEntry={onAddLaborEntry}
        onRemoveLaborEntry={vi.fn().mockResolvedValue(undefined)}
      />,
    )

    expect(screen.getByText('Training labor')).toBeInTheDocument()
    expect(screen.getByText('1.50')).toBeInTheDocument()
    expect(screen.getByText('$75.00')).toBeInTheDocument()
    fireEvent.click(screen.getByRole('button', { name: /add labor entry/i }))
    expect(onAddLaborEntry).toHaveBeenCalledOnce()
  })
})
