import { cleanup, fireEvent, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { RulePackRequirementPanel } from './RulePackRequirementPanel'

describe('RulePackRequirementPanel', () => {
  afterEach(() => {
    cleanup()
  })

  it('renders save form for managers', () => {
    render(
      <RulePackRequirementPanel
        mode="create"
        title="Definition rule packs"
        requirements={[]}
        rulePackKeyInput=""
        rulePackOptions={[
          { value: 'driver_qualification', label: 'driver_qualification' },
        ]}
        onRulePackKeyChange={vi.fn()}
        onSave={vi.fn()}
        onRemove={vi.fn()}
        isSaving={false}
        isRemovingId={null}
        canManage
        validateWithComplianceCore
        onValidateWithComplianceCoreChange={vi.fn()}
      />,
    )

    expect(screen.getByRole('button', { name: /save rule pack requirement/i })).toBeDisabled()
    expect(screen.getByTestId('rule-pack-requirement-key')).toBeInTheDocument()
  })

  it('lists linked requirements with metadata', () => {
    render(
      <RulePackRequirementPanel
        mode="details"
        title="Program rule packs"
        requirements={[
          {
            requirementId: 'r1',
            entityType: 'training_program',
            entityId: 'p1',
            rulePackKey: 'driver_qualification',
            createdAt: '2026-05-27T00:00:00Z',
            updatedAt: '2026-05-27T00:00:00Z',
            metadata: {
              label: 'Driver qualification rules',
              description: 'CDL qualification checks.',
              regulatoryProgramKey: 'driver_compliance',
              regulatoryProgramLabel: 'Driver Compliance',
              versionNumber: 1,
              status: 'published',
              isActive: true,
            },
          },
        ]}
        rulePackKeyInput=""
        rulePackOptions={[
          { value: 'driver_qualification', label: 'driver_qualification' },
        ]}
        onRulePackKeyChange={vi.fn()}
        onSave={vi.fn()}
        onRemove={vi.fn()}
        isSaving={false}
        isRemovingId={null}
        canManage
        validateWithComplianceCore={false}
        onValidateWithComplianceCoreChange={vi.fn()}
      />,
    )

    expect(screen.getAllByText('driver_qualification').length).toBeGreaterThan(0)
    expect(screen.getByText('Driver qualification rules')).toBeInTheDocument()
    fireEvent.click(screen.getByRole('button', { name: /remove/i }))
  })
})
