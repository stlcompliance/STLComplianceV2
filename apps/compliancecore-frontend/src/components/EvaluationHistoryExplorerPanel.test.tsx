import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, render, screen, fireEvent, waitFor } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import { EvaluationHistoryExplorerPanel } from './EvaluationHistoryExplorerPanel'
import { getRuleEvaluationAuditExport } from '../api/client'

vi.mock('../api/client', () => ({
  getRuleEvaluationAuditExport: vi.fn(),
}))

describe('EvaluationHistoryExplorerPanel', () => {
  afterEach(() => {
    cleanup()
    vi.resetAllMocks()
  })

  function renderPanel() {
    const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })

    render(
      <QueryClientProvider client={queryClient}>
        <EvaluationHistoryExplorerPanel
          accessToken="token"
          rulePacks={[
            {
              rulePackId: 'rp-1',
              regulatoryProgramId: 'prog-1',
              regulatoryProgramKey: 'fmcsa_safety',
              regulatoryProgramLabel: 'FMCSA Safety Compliance',
              packKey: 'driver_qualification',
              label: 'Driver Qualification Rules',
              description: 'Baseline driver qualification rules.',
              versionNumber: 1,
              status: 'published',
              isActive: true,
              createdAt: '2026-05-27T00:00:00Z',
              updatedAt: '2026-05-27T00:00:00Z',
            },
            {
              rulePackId: 'rp-2',
              regulatoryProgramId: 'prog-1',
              regulatoryProgramKey: 'fmcsa_safety',
              regulatoryProgramLabel: 'FMCSA Safety Compliance',
              packKey: 'medical_cert',
              label: 'Medical Certificate Rules',
              description: 'Baseline medical certificate rules.',
              versionNumber: 1,
              status: 'published',
              isActive: true,
              createdAt: '2026-05-27T00:00:00Z',
              updatedAt: '2026-05-27T00:00:00Z',
            },
            {
              rulePackId: 'rp-3',
              regulatoryProgramId: 'prog-1',
              regulatoryProgramKey: 'fmcsa_safety',
              regulatoryProgramLabel: 'FMCSA Safety Compliance',
              packKey: 'legacy_rules',
              label: 'Legacy Rule Pack',
              description: 'Historical legacy rules.',
              versionNumber: 1,
              status: 'archived',
              isActive: false,
              createdAt: '2026-05-26T00:00:00Z',
              updatedAt: '2026-05-26T00:00:00Z',
            },
          ]}
          evaluationRuns={[
            {
              evaluationRunId: 'run-0',
              rulePackId: 'rp-3',
              packKey: 'legacy_rules',
              packLabel: 'Legacy Rule Pack',
              versionNumber: 1,
              status: 'completed',
              overallResult: 'warn',
              factInputs: { legacy_fact: true },
              ruleResults: [
                {
                  ruleKey: 'legacy_check',
                  label: 'Legacy check',
                  result: 'warn',
                  message: 'Legacy rule triggered.',
                },
              ],
              createdAt: '2026-05-26T09:00:00Z',
            },
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
              createdAt: '2026-05-27T12:00:00Z',
            },
            {
              evaluationRunId: 'run-2',
              rulePackId: 'rp-2',
              packKey: 'medical_cert',
              packLabel: 'Medical Certificate Rules',
              versionNumber: 1,
              status: 'completed',
              overallResult: 'block',
              factInputs: { medical_cert_on_file: false },
              ruleResults: [
                {
                  ruleKey: 'med_cert',
                  label: 'Medical certificate on file',
                  result: 'block',
                  message: 'Missing certificate.',
                  nonWaivable: true,
                },
              ],
              createdAt: '2026-05-27T13:00:00Z',
            },
            {
              evaluationRunId: 'run-3',
              rulePackId: 'rp-1',
              packKey: 'driver_qualification',
              packLabel: 'Driver Qualification Rules',
              versionNumber: 1,
              status: 'completed',
              overallResult: 'pass',
              factInputs: { driver_license_valid: true, refresher_complete: true },
              ruleResults: [
                {
                  ruleKey: 'license_valid',
                  label: 'Valid driver license',
                  result: 'pass',
                  message: 'Fact matched.',
                },
              ],
              createdAt: '2026-05-28T08:00:00Z',
            },
          ]}
          canExportAudit={true}
          onFocusRulePack={vi.fn()}
        />
      </QueryClientProvider>,
    )
  }

  it('filters runs, shows details, and loads audit export snapshot', async () => {
    vi.mocked(getRuleEvaluationAuditExport).mockResolvedValue({
      exportId: 'export-1',
      tenantId: 'tenant-1',
      generatedAt: '2026-05-27T14:00:00Z',
      evaluationRun: {
        evaluationRunId: 'run-2',
        rulePackId: 'rp-2',
        packKey: 'medical_cert',
        actorUserId: 'user-1',
        status: 'completed',
        overallResult: 'block',
        factInputsJson: '{"medical_cert_on_file":false}',
        ruleResultsJson: '[{"ruleKey":"med_cert"}]',
        appliedWaiverId: null,
        appliedWaiverKey: null,
        createdAt: '2026-05-27T13:00:00Z',
      },
      workflowGateChecks: [
        {
          checkResultId: 'check-1',
          gateKey: 'driver_assignment',
          rulePackId: 'rp-2',
          packKey: 'medical_cert',
          ruleEvaluationRunId: 'run-2',
          outcome: 'block',
          reasonCode: 'missing_fact',
          message: 'Medical certificate missing.',
          appliedWaiverId: null,
          appliedWaiverKey: null,
          checkedAt: '2026-05-27T13:00:00Z',
        },
      ],
      findings: [
        {
          findingId: 'finding-1',
          rulePackId: 'rp-2',
          packKey: 'medical_cert',
          ruleEvaluationRunId: 'run-2',
          findingKey: 'med_cert_missing',
          severity: 'high',
          status: 'open',
          ruleKey: 'med_cert',
          factKey: 'medical_cert_on_file',
          title: 'Medical certificate missing',
          message: 'Driver does not have a current medical certificate.',
          reasonCode: 'missing_fact',
          createdAt: '2026-05-27T13:00:00Z',
        },
      ],
      waivers: [],
    })

    renderPanel()

    expect(screen.getByTestId('evaluation-history-explorer-panel')).toBeTruthy()
    expect(screen.getAllByText('Driver Qualification Rules').length).toBeGreaterThan(0)
    expect(screen.getAllByText('Medical Certificate Rules').length).toBeGreaterThan(0)
    expect(screen.getAllByText('Legacy Rule Pack').length).toBeGreaterThan(0)

    fireEvent.change(screen.getByLabelText(/Search/i), { target: { value: 'medical' } })

    await waitFor(() => {
      expect(screen.queryAllByText('Driver Qualification Rules').length).toBe(0)
      expect(screen.queryAllByText('Medical Certificate Rules').length).toBeGreaterThan(0)
    })

    fireEvent.change(screen.getByLabelText(/Search/i), { target: { value: '' } })
    fireEvent.change(screen.getByLabelText(/Start date/i), { target: { value: '2026-05-27' } })
    fireEvent.change(screen.getByLabelText(/End date/i), { target: { value: '2026-05-27' } })

    await waitFor(() => {
      expect(screen.queryAllByText('Legacy Rule Pack').length).toBe(0)
      expect(screen.queryAllByText('Medical Certificate Rules').length).toBeGreaterThan(0)
      expect(screen.queryAllByText('Driver Qualification Rules').length).toBeGreaterThan(0)
      expect(screen.getByText(/Matching runs/i).parentElement).toHaveTextContent('2')
    })

    fireEvent.click(screen.getByRole('button', { name: /Load audit export snapshot/i }))

    await waitFor(() => {
      expect(screen.getByRole('heading', { name: /Audit export snapshot/i })).toBeInTheDocument()
      expect(screen.getByText(/export-1/i)).toBeInTheDocument()
      expect(screen.getByText(/1 gate checks/i)).toBeInTheDocument()
      expect(screen.getByText(/1 findings/i)).toBeInTheDocument()
    })
  })
})
