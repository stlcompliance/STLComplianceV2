import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import { PmDuePanel } from './PmDuePanel'

describe('PmDuePanel', () => {
  it('renders due and overdue schedules', () => {
    render(
      <PmDuePanel
        isLoading={false}
        dueSchedules={[
          {
            pmScheduleId: '11111111-1111-1111-1111-111111111111',
            assetId: '22222222-2222-2222-2222-222222222222',
            assetTag: 'EX-1001',
            assetName: 'Excavator 1001',
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
            nextDueAt: '2026-05-20T00:00:00Z',
            lastCompletedAt: null,
            dueStatus: 'due',
            status: 'active',
            lastDueScanAt: '2026-05-27T12:00:00Z',
            linkedWorkOrderId: '55555555-5555-5555-5555-555555555555',
            linkedWorkOrderNumber: 'WO-20260527-ABC12345',
            linkedWorkOrderStatus: 'open',
            createdAt: '2026-01-01T00:00:00Z',
            updatedAt: '2026-05-27T12:00:00Z',
          },
          {
            pmScheduleId: '33333333-3333-3333-3333-333333333333',
            assetId: '44444444-4444-4444-4444-444444444444',
            assetTag: 'FL-2002',
            assetName: 'Forklift 2002',
            scheduleKey: 'safety-check',
            name: 'Safety Check',
            description: '',
            scheduleMode: 'calendar',
            assetMeterId: null,
            meterKey: null,
            meterUnit: null,
            intervalUsage: null,
            nextDueAtUsage: null,
            lastCompletedUsage: null,
            intervalDays: 30,
            nextDueAt: '2026-05-10T00:00:00Z',
            lastCompletedAt: null,
            dueStatus: 'overdue',
            status: 'active',
            lastDueScanAt: '2026-05-27T12:00:00Z',
            linkedWorkOrderId: null,
            linkedWorkOrderNumber: null,
            linkedWorkOrderStatus: null,
            createdAt: '2026-01-01T00:00:00Z',
            updatedAt: '2026-05-27T12:00:00Z',
          },
        ]}
      />,
    )

    expect(screen.getByText('Due preventive maintenance')).toBeInTheDocument()
    expect(screen.getByText('EX-1001')).toBeInTheDocument()
    expect(screen.getByText('FL-2002')).toBeInTheDocument()
    expect(screen.getByText('Due')).toBeInTheDocument()
    expect(screen.getByText('Overdue')).toBeInTheDocument()
    expect(screen.getByText('WO-20260527-ABC12345')).toBeInTheDocument()
    expect(screen.getByText('Pending generation')).toBeInTheDocument()
  })

  it('shows empty state when no schedules are due', () => {
    render(<PmDuePanel isLoading={false} dueSchedules={[]} />)
    expect(screen.getByText('No PM schedules are currently due.')).toBeInTheDocument()
  })
})
