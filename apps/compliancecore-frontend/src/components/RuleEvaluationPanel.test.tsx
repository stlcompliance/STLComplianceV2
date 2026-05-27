import { cleanup, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it } from 'vitest'
import { RuleEvaluationPanel } from './RuleEvaluationPanel'

describe('RuleEvaluationPanel', () => {
  afterEach(() => {
    cleanup()
  })

  it('renders empty states when no rule packs exist', () => {
    render(
      <RuleEvaluationPanel
        rulePacks={[]}
        factDefinitions={[]}
        selectedRulePackId=""
        onSelectRulePack={() => undefined}
        content={null}
        hasContent={false}
        evaluationRuns={[]}
        canManage={false}
        onSaveContent={() => undefined}
        isSavingContent={false}
        onSeedContent={() => undefined}
        isSeedingContent={false}
        onEvaluate={() => undefined}
        isEvaluating={false}
        lastEvaluation={null}
        onEvaluateBatch={() => undefined}
        isEvaluatingBatch={false}
        lastBatchEvaluation={null}
      />,
    )

    expect(screen.getByText(/Create a rule pack on the Regulatory tab first/)).toBeInTheDocument()
    expect(screen.getByText(/No evaluation runs yet/)).toBeInTheDocument()
  })

  it('renders rule pack selector, evaluation form, and history rows', () => {
    render(
      <RuleEvaluationPanel
        rulePacks={[
          {
            rulePackId: 'rp-1',
            regulatoryProgramId: 'p-1',
            regulatoryProgramKey: 'fmcsa_safety',
            regulatoryProgramLabel: 'FMCSA Safety Compliance',
            packKey: 'driver_qualification',
            label: 'Driver Qualification Rules',
            description: 'Baseline driver qualification rules.',
            versionNumber: 1,
            status: 'draft',
            isActive: true,
            createdAt: '2026-05-27T00:00:00Z',
            updatedAt: '2026-05-27T00:00:00Z',
          },
        ]}
        factDefinitions={[
          {
            factDefinitionId: 'fd-1',
            factKey: 'driver_license_valid',
            label: 'Valid driver license',
            description: 'Driver holds a valid license.',
            valueType: 'boolean',
            isActive: true,
            createdAt: '2026-05-27T00:00:00Z',
            updatedAt: '2026-05-27T00:00:00Z',
          },
        ]}
        selectedRulePackId="rp-1"
        onSelectRulePack={() => undefined}
        content={{
          schemaVersion: 1,
          logic: 'all',
          rules: [
            {
              ruleKey: 'license_valid',
              label: 'Valid driver license',
              type: 'fact_boolean',
              factKey: 'driver_license_valid',
              expectedValue: true,
            },
          ],
        }}
        hasContent={true}
        evaluationRuns={[
          {
            evaluationRunId: 'run-1',
            rulePackId: 'rp-1',
            packKey: 'driver_qualification',
            packLabel: 'Driver Qualification Rules',
            versionNumber: 1,
            status: 'completed',
            overallResult: 'pass',
            factInputs: { driver_license_valid: true },
            ruleResults: [
              {
                ruleKey: 'license_valid',
                label: 'Valid driver license',
                result: 'pass',
                message: 'Fact matched.',
              },
            ],
            createdAt: '2026-05-27T00:00:00Z',
          },
        ]}
        canManage={true}
        onSaveContent={() => undefined}
        isSavingContent={false}
        onSeedContent={() => undefined}
        isSeedingContent={false}
        onEvaluate={() => undefined}
        isEvaluating={false}
        lastEvaluation={null}
        onEvaluateBatch={() => undefined}
        isEvaluatingBatch={false}
        lastBatchEvaluation={null}
      />,
    )

    expect(screen.getAllByText('Driver Qualification Rules').length).toBeGreaterThan(0)
    expect(screen.getByRole('button', { name: /Run batch evaluation \(0 packs\)/i })).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Seed sample rule content' })).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Evaluate rule pack' })).toBeInTheDocument()
    expect(screen.getByText('pass')).toBeInTheDocument()
  })
})
