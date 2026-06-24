import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import { RuleTestCasesPanel } from './RuleTestCasesPanel'
import * as client from '../api/client'

vi.mock('../api/client', () => ({
  listRuleTestCases: vi.fn(),
  createRuleTestCase: vi.fn(),
  patchRuleTestCase: vi.fn(),
  deleteRuleTestCase: vi.fn(),
  runRuleTestCase: vi.fn(),
}))

const rulePacks = [
  {
    rulePackId: 'pack-1',
    regulatoryProgramId: 'program-1',
    regulatoryProgramKey: 'fmcsa_safety',
    regulatoryProgramLabel: 'FMCSA Safety Compliance',
    packKey: 'driver_qualification',
    label: 'Driver Qualification Rules',
    description: 'Driver qualification rules',
    versionNumber: 2,
    status: 'draft',
    isActive: true,
    createdAt: '2026-06-01T00:00:00Z',
    updatedAt: '2026-06-01T00:00:00Z',
  },
]

describe('RuleTestCasesPanel', () => {
  afterEach(() => {
    cleanup()
    vi.clearAllMocks()
  })

  it('creates and runs saved test cases for the selected rule pack', async () => {
    vi.mocked(client.listRuleTestCases)
      .mockResolvedValueOnce([])
      .mockResolvedValueOnce([
        {
          ruleTestCaseId: 'tc-1',
          rulePackId: 'pack-1',
          rulePackKey: 'driver_qualification',
          rulePackVersion: 2,
          rulePackStatus: 'draft',
          ruleId: 'license_valid',
          ruleKey: 'license_valid',
          testKey: 'license_valid_happy_path',
          label: 'Valid driver license',
          description: 'Driver license is valid and the rule should pass.',
          expectedResult: 'pass',
          facts: { driver_license_valid: true },
          createdAt: '2026-06-01T00:00:00Z',
          updatedAt: '2026-06-01T00:00:00Z',
        },
      ])
    vi.mocked(client.createRuleTestCase).mockResolvedValueOnce({
      ruleTestCaseId: 'tc-1',
      rulePackId: 'pack-1',
      rulePackKey: 'driver_qualification',
      rulePackVersion: 2,
      rulePackStatus: 'draft',
      ruleId: 'license_valid',
      ruleKey: 'license_valid',
      testKey: 'license_valid_happy_path',
      label: 'Valid driver license',
      description: 'Driver license is valid and the rule should pass.',
      expectedResult: 'pass',
      facts: { driver_license_valid: true },
      createdAt: '2026-06-01T00:00:00Z',
      updatedAt: '2026-06-01T00:00:00Z',
    })
    vi.mocked(client.runRuleTestCase).mockResolvedValueOnce({
      ruleTestCaseId: 'tc-1',
      ruleId: 'license_valid',
      expectedResult: 'pass',
      actualResult: 'pass',
      passed: true,
      message: 'The saved test case matched the expected result.',
      evaluation: {
        ruleKey: 'license_valid',
        label: 'Valid driver license',
        result: 'pass',
        message: 'pass',
        nonWaivable: false,
        remediationRequired: false,
        reviewRequired: false,
      },
      evaluatedAt: '2026-06-01T00:00:00Z',
    })

    const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={queryClient}>
        <RuleTestCasesPanel
          accessToken="token"
          rulePacks={rulePacks}
          selectedRulePackId="pack-1"
          onSelectRulePack={() => undefined}
          canManage={true}
        />
      </QueryClientProvider>,
    )

    await screen.findByText(/No saved test cases yet/i)
    fireEvent.change(screen.getByLabelText(/Rule key/i), { target: { value: 'license_valid' } })
    fireEvent.change(screen.getByLabelText(/Test key/i), { target: { value: 'license_valid_happy_path' } })
    fireEvent.change(screen.getByLabelText(/Label/i), { target: { value: 'Valid driver license' } })
    fireEvent.change(screen.getByLabelText(/Expected result/i), { target: { value: 'pass' } })
    fireEvent.change(screen.getByLabelText(/Facts JSON/i), {
      target: { value: '{"driver_license_valid":true}' },
    })
    fireEvent.click(screen.getByRole('button', { name: /Create case/i }))

    await waitFor(() => expect(client.createRuleTestCase).toHaveBeenCalled())
    expect(client.createRuleTestCase).toHaveBeenCalledWith(
      'token',
      'pack-1',
      expect.objectContaining({
        ruleKey: 'license_valid',
        testKey: 'license_valid_happy_path',
        facts: { driver_license_valid: true },
      }),
    )

    expect(await screen.findByRole('button', { name: /Run test/i })).toBeInTheDocument()
    fireEvent.click(screen.getByRole('button', { name: /Run test/i }))
    await waitFor(() => expect(client.runRuleTestCase).toHaveBeenCalledWith('token', 'pack-1', 'tc-1'))
    expect(await screen.findByText(/The saved test case matched the expected result/i)).toBeInTheDocument()
    expect(screen.getByText('Evaluation details')).toBeInTheDocument()
    expect(screen.getByText('Advanced technical details')).toBeInTheDocument()
  })

  it('allows updating and deleting a loaded test case', async () => {
    vi.mocked(client.listRuleTestCases)
      .mockResolvedValueOnce([
        {
          ruleTestCaseId: 'tc-1',
          rulePackId: 'pack-1',
          rulePackKey: 'driver_qualification',
          rulePackVersion: 2,
          rulePackStatus: 'draft',
          ruleId: 'license_valid',
          ruleKey: 'license_valid',
          testKey: 'license_valid_happy_path',
          label: 'Valid driver license',
          description: 'Driver license is valid and the rule should pass.',
          expectedResult: 'pass',
          facts: { driver_license_valid: true },
          createdAt: '2026-06-01T00:00:00Z',
          updatedAt: '2026-06-01T00:00:00Z',
        },
      ])
      .mockResolvedValueOnce([
        {
          ruleTestCaseId: 'tc-1',
          rulePackId: 'pack-1',
          rulePackKey: 'driver_qualification',
          rulePackVersion: 2,
          rulePackStatus: 'draft',
          ruleId: 'license_valid',
          ruleKey: 'license_valid',
          testKey: 'license_valid_blocked_path',
          label: 'Blocked driver license',
          description: 'Updated description.',
          expectedResult: 'block',
          facts: { driver_license_valid: false },
          createdAt: '2026-06-01T00:00:00Z',
          updatedAt: '2026-06-01T00:00:00Z',
        },
      ])
      .mockResolvedValueOnce([])
    vi.mocked(client.patchRuleTestCase).mockResolvedValueOnce({
      ruleTestCaseId: 'tc-1',
      rulePackId: 'pack-1',
      rulePackKey: 'driver_qualification',
      rulePackVersion: 2,
      rulePackStatus: 'draft',
      ruleId: 'license_valid',
      ruleKey: 'license_valid',
      testKey: 'license_valid_blocked_path',
      label: 'Blocked driver license',
      description: 'Updated description.',
      expectedResult: 'block',
      facts: { driver_license_valid: false },
      createdAt: '2026-06-01T00:00:00Z',
      updatedAt: '2026-06-01T00:00:00Z',
    })

    const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={queryClient}>
        <RuleTestCasesPanel
          accessToken="token"
          rulePacks={rulePacks}
          selectedRulePackId="pack-1"
          onSelectRulePack={() => undefined}
          canManage={true}
        />
      </QueryClientProvider>,
    )

    expect(await screen.findByText('Valid driver license')).toBeInTheDocument()
    fireEvent.click(screen.getByRole('button', { name: /Valid driver license/i }))
    fireEvent.click(screen.getByRole('button', { name: /Save changes/i }))
    await waitFor(() =>
      expect(client.patchRuleTestCase).toHaveBeenCalledWith('token', 'pack-1', 'tc-1', expect.objectContaining({ expectedResult: 'pass' })),
    )

    fireEvent.click(screen.getByRole('button', { name: /Delete case/i }))
    await waitFor(() => expect(client.deleteRuleTestCase).toHaveBeenCalledWith('token', 'pack-1', 'tc-1'))
  })
})
