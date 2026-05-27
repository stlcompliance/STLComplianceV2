import { cleanup, fireEvent, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { RulePackImpactPanel } from './RulePackImpactPanel'

describe('RulePackImpactPanel', () => {
  afterEach(() => {
    cleanup()
  })

  it('renders assess form for admins', () => {
    render(
      <RulePackImpactPanel
        rulePackKeyInput=""
        onRulePackKeyChange={vi.fn()}
        onAssess={vi.fn()}
        isAssessing={false}
        canAssess
        assessment={null}
      />,
    )

    expect(screen.getByRole('button', { name: /run impact assessment/i })).toBeDisabled()
    expect(screen.getByPlaceholderText('driver_qualification')).toBeInTheDocument()
  })

  it('shows assessment summary and recommended actions', () => {
    render(
      <RulePackImpactPanel
        rulePackKeyInput="driver_qualification"
        onRulePackKeyChange={vi.fn()}
        onAssess={vi.fn()}
        isAssessing={false}
        canAssess
        assessment={{
          assessmentId: 'a1',
          rulePackKey: 'driver_qualification',
          assessedAt: '2026-05-27T00:00:00Z',
          triggers: ['version_drift'],
          currentState: {
            label: 'Driver qualification rules',
            description: 'CDL checks',
            regulatoryProgramKey: 'driver_compliance',
            regulatoryProgramLabel: 'Driver Compliance',
            versionNumber: 2,
            status: 'published',
            isActive: true,
          },
          drift: {
            hasVersionDrift: true,
            baselineVersionNumber: 1,
            currentVersionNumber: 2,
            hasStatusDrift: false,
            baselineStatus: 'published',
            currentStatus: 'published',
            packInactive: false,
            packNotFound: false,
          },
          affectedDefinitions: [],
          affectedPrograms: [],
          affectedAssignments: [
            {
              assignmentId: 'as1',
              staffarrPersonId: '11111111-1111-1111-1111-111111111111',
              trainingDefinitionId: 'd1',
              trainingDefinitionName: 'Hazmat training',
              status: 'in_progress',
              assignmentReason: 'manual',
              createdAt: '2026-05-27T00:00:00Z',
            },
          ],
          affectedQualifications: [],
          recommendedActions: [
            {
              actionType: 'review_requirements',
              priority: 'high',
              message: 'Review linked requirements after version change.',
              entityType: null,
              entityId: null,
            },
          ],
          summary: {
            requirementCount: 1,
            definitionCount: 1,
            programCount: 0,
            activeAssignmentCount: 1,
            activeQualificationCount: 0,
            hasDrift: true,
            requiresAttention: true,
          },
        }}
      />,
    )

    expect(screen.getByText('Attention required')).toBeInTheDocument()
    expect(screen.getByText(/Version drift/)).toBeInTheDocument()
    expect(screen.getByText(/review requirements/i)).toBeInTheDocument()
    expect(screen.getByText(/Hazmat training/)).toBeInTheDocument()
    fireEvent.click(screen.getByRole('button', { name: /run impact assessment/i }))
  })
})
