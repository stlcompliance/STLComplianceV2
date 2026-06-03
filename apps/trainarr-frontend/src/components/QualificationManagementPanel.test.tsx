import { render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'
import { QualificationManagementPanel } from './QualificationManagementPanel'

describe('QualificationManagementPanel', () => {
  it('lists issues and lifecycle actions for managers', () => {
    render(
      <QualificationManagementPanel
        issues={[
          {
            qualificationIssueId: 'q1',
            trainingAssignmentId: 'a1',
            staffarrPersonId: '11111111-1111-1111-1111-111111111101',
            qualificationKey: 'hazmat',
            qualificationName: 'Hazmat endorsement',
            status: 'issued',
            issuedAt: '2026-05-28T12:00:00Z',
            expiresAt: null,
            statusChangedAt: null,
            lifecycleReason: null,
          },
        ]}
        statusFilter=""
        lifecycleReason=""
        selectedIssueId="q1"
        onStatusFilterChange={vi.fn()}
        onLifecycleReasonChange={vi.fn()}
        onSelectIssue={vi.fn()}
        onSuspend={vi.fn()}
        onRevoke={vi.fn()}
        onExpire={vi.fn()}
        isSuspending={false}
        isRevoking={false}
        isExpiring={false}
        canManage
        history={[]}
        isLoadingHistory={false}
      />,
    )
    expect(screen.getByTestId('qualification-management-panel')).toBeInTheDocument()
    expect(screen.getAllByText('Hazmat endorsement').length).toBeGreaterThan(0)
    expect(screen.getByRole('button', { name: /suspend/i })).toBeInTheDocument()
  })

  it('renders selected qualification history entries', () => {
    render(
      <QualificationManagementPanel
        issues={[
          {
            qualificationIssueId: 'q1',
            trainingAssignmentId: 'a1',
            staffarrPersonId: '11111111-1111-1111-1111-111111111101',
            qualificationKey: 'hazmat',
            qualificationName: 'Hazmat endorsement',
            status: 'issued',
            issuedAt: '2026-05-28T12:00:00Z',
            expiresAt: null,
            statusChangedAt: null,
            lifecycleReason: null,
          },
        ]}
        statusFilter=""
        lifecycleReason=""
        selectedIssueId="q1"
        onStatusFilterChange={vi.fn()}
        onLifecycleReasonChange={vi.fn()}
        onSelectIssue={vi.fn()}
        onSuspend={vi.fn()}
        onRevoke={vi.fn()}
        onExpire={vi.fn()}
        isSuspending={false}
        isRevoking={false}
        isExpiring={false}
        canManage
        history={[
          {
            occurredAt: '2026-05-28T13:00:00Z',
            eventType: 'qualification_issue.revoke',
            status: 'revoked',
            reason: 'policy',
            actorUserId: 'actor-1',
          },
        ]}
        isLoadingHistory={false}
      />,
    )
    expect(screen.getAllByText('Qualification issue history').length).toBeGreaterThan(0)
    expect(screen.getByText('qualification_issue.revoke')).toBeInTheDocument()
  })
})
