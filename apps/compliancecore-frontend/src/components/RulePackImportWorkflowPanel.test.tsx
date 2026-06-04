import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import { RulePackImportWorkflowPanel } from './RulePackImportWorkflowPanel'
import * as client from '../api/client'

vi.mock('../api/client', () => ({
  previewRulePackImport: vi.fn(),
  validateRulePackImport: vi.fn(),
  publishDraftRulePackImport: vi.fn(),
  getRulePackImport: vi.fn(),
  getRulePackImportDiff: vi.fn(),
  getRulePackImportTestResults: vi.fn(),
  rollbackRulePackImport: vi.fn(),
}))

describe('RulePackImportWorkflowPanel', () => {
  afterEach(() => {
    cleanup()
    vi.clearAllMocks()
  })

  it('previews a rule pack import and shows the returned summary artifacts', async () => {
    vi.mocked(client.previewRulePackImport).mockResolvedValueOnce({
      importId: 'import-1',
      status: 'validated',
      dryRun: true,
      createdAt: '2026-06-01T12:00:00Z',
      result: {
        dryRun: true,
        applied: false,
        files: [{ fileName: 'rule_packs.csv', rowCount: 1, created: 1, updated: 0, deactivated: 0 }],
        issues: [],
      },
    })
    vi.mocked(client.getRulePackImport).mockResolvedValueOnce({
      importId: 'import-1',
      status: 'validated',
      dryRun: true,
      createdAt: '2026-06-01T12:00:00Z',
      result: {
        dryRun: true,
        applied: false,
        files: [{ fileName: 'rule_packs.csv', rowCount: 1, created: 1, updated: 0, deactivated: 0 }],
        issues: [],
      },
    })
    vi.mocked(client.getRulePackImportDiff).mockResolvedValueOnce({
      importId: 'import-1',
      filesWithChanges: 1,
      createdCount: 1,
      updatedCount: 0,
      deactivatedCount: 0,
      issueCount: 0,
    })
    vi.mocked(client.getRulePackImportTestResults).mockResolvedValueOnce({
      importId: 'import-1',
      passed: true,
      issueCount: 0,
      issues: [],
    })

    const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={queryClient}>
        <RulePackImportWorkflowPanel accessToken="token" canManage={true} />
      </QueryClientProvider>,
    )

    fireEvent.change(screen.getByLabelText(/Bundle files/i), {
      target: { files: [new File(['pack_key'], 'rule_packs.csv', { type: 'text/csv' })] },
    })
    fireEvent.change(screen.getByLabelText(/Registry resolution/i), { target: { value: 'create_missing' } })
    fireEvent.change(screen.getByLabelText(/Governing body key/i), { target: { value: 'osha' } })
    fireEvent.change(screen.getByLabelText(/Jurisdiction key/i), { target: { value: 'us_workplace' } })
    fireEvent.change(screen.getByLabelText(/Program mappings JSON/i), {
      target: { value: '{"external_fmcsa":"fmcsa_safety"}' },
    })

    fireEvent.click(screen.getByRole('button', { name: /Preview import/i }))

    await waitFor(() => expect(client.previewRulePackImport).toHaveBeenCalled())
    expect(client.previewRulePackImport).toHaveBeenCalledWith(
      'token',
      expect.anything(),
      expect.objectContaining({
        regulatorySpineMode: 'create_missing',
        governingBodyKey: 'osha',
        jurisdictionKey: 'us_workplace',
        programMappings: { external_fmcsa: 'fmcsa_safety' },
      }),
    )
    expect(await screen.findByText(/import-1/)).toBeInTheDocument()
    expect(screen.getByText(/Changed files/)).toBeInTheDocument()
    expect(screen.getByText(/No import issues reported/i)).toBeInTheDocument()
    expect(screen.getByText(/Passed - 0 issue\(s\)/i)).toBeInTheDocument()
  })

  it('publishes a draft import and allows rollback from the latest import', async () => {
    vi.mocked(client.publishDraftRulePackImport).mockResolvedValueOnce({
      importId: 'import-2',
      status: 'applied',
      dryRun: false,
      createdAt: '2026-06-01T13:00:00Z',
      result: {
        dryRun: false,
        applied: true,
        files: [],
        issues: [],
      },
    })
    vi.mocked(client.getRulePackImport).mockResolvedValue({
      importId: 'import-2',
      status: 'applied',
      dryRun: false,
      createdAt: '2026-06-01T13:00:00Z',
      result: {
        dryRun: false,
        applied: true,
        files: [],
        issues: [],
      },
    })
    vi.mocked(client.getRulePackImportDiff).mockResolvedValue({
      importId: 'import-2',
      filesWithChanges: 0,
      createdCount: 0,
      updatedCount: 0,
      deactivatedCount: 0,
      issueCount: 0,
    })
    vi.mocked(client.getRulePackImportTestResults).mockResolvedValue({
      importId: 'import-2',
      passed: true,
      issueCount: 0,
      issues: [],
    })
    vi.mocked(client.rollbackRulePackImport).mockResolvedValueOnce({
      importId: 'import-2',
      rolledBack: true,
      status: 'rolled_back',
    })

    const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={queryClient}>
        <RulePackImportWorkflowPanel accessToken="token" canManage={true} />
      </QueryClientProvider>,
    )

    fireEvent.change(screen.getByLabelText(/Bundle files/i), {
      target: { files: [new File(['pack_key'], 'rule_packs.csv', { type: 'text/csv' })] },
    })
    fireEvent.click(screen.getByRole('button', { name: /Publish draft/i }))

    await waitFor(() => expect(client.publishDraftRulePackImport).toHaveBeenCalled())
    fireEvent.click(screen.getByRole('button', { name: /Rollback latest import/i }))
    await waitFor(() => expect(client.rollbackRulePackImport).toHaveBeenCalledWith('token', 'import-2'))
    expect(await screen.findByText(/rolled_back/)).toBeInTheDocument()
  })
})
