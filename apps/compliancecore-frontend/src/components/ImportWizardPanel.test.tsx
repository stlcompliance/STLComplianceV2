import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, fireEvent, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import { ImportWizardPanel } from './ImportWizardPanel'
import * as clientApi from '../api/client'

vi.mock('../api/client', () => ({
  addImportWizardSupportingEvidence: vi.fn(),
  bulkConfirmImportWizardMappings: vi.fn(),
  commitImportWizard: vi.fn(),
  confirmImportWizardItem: vi.fn(),
  createImportWizardExceptionExemption: vi.fn(),
  createImportSession: vi.fn().mockResolvedValue({
    importSessionId: 'session-1',
  }),
  createImportWizardTarget: vi.fn(),
  forceMapImportWizardItem: vi.fn(),
  generateImportMappingCandidates: vi.fn(),
  getImportCommitPreview: vi.fn(),
  getImportWizardSummary: vi.fn(),
  getNextImportWizardItem: vi.fn(),
  mapImportWizardAsExceptionProof: vi.fn(),
  mapImportWizardAsExemptionProof: vi.fn(),
  mapImportWizardAsNormalEvidence: vi.fn(),
  mapImportWizardAsSpecialPermitApprovalProof: vi.fn(),
  markImportWizardExceptionNotApplicable: vi.fn(),
  markImportWizardNoDocumentRequired: vi.fn(),
  markImportWizardNotApplicable: vi.fn(),
  markImportWizardReferenceOnly: vi.fn(),
  parseImportSession: vi.fn().mockResolvedValue({
    session: {
      importSessionId: 'session-1',
      status: 'parsed',
      validationStatus: 'passed',
      mappingStatus: 'draft',
    },
  }),
  rejectImportWizardItem: vi.fn(),
  selectImportWizardExceptionExemption: vi.fn(),
  selectImportWizardEvidenceOption: vi.fn(),
  selectImportWizardTarget: vi.fn(),
  skipImportWizardItem: vi.fn(),
  uploadImportSessionBundle: vi.fn().mockResolvedValue({
    session: {
      importSessionId: 'session-1',
      status: 'uploaded',
      validationStatus: 'pending',
      mappingStatus: 'draft',
    },
  }),
  validateImportSession: vi.fn().mockResolvedValue({
    validationStatus: 'passed',
    validRows: 1,
    totalRows: 1,
    invalidRows: 0,
    files: [],
  }),
}))

describe('ImportWizardPanel', () => {
  afterEach(() => {
    cleanup()
  })

  it('renders the staged import wizard controls for managers', () => {
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={client}>
        <ImportWizardPanel accessToken="token" canManage={true} />
      </QueryClientProvider>,
    )

    expect(screen.getByText(/Staged Import and Evidence Mapping Wizard/)).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /Upload and validate/i })).toBeEnabled()
    expect(screen.getByRole('button', { name: /Generate candidates/i })).toBeDisabled()
  })

  it('blocks upload for read-only users', () => {
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={client}>
        <ImportWizardPanel accessToken="token" canManage={false} />
      </QueryClientProvider>,
    )

    expect(screen.getByRole('button', { name: /Upload and validate/i })).toBeDisabled()
  })

  it('renders candidate generation failures in shared callout', async () => {
    vi.mocked(clientApi.generateImportMappingCandidates).mockRejectedValueOnce(
      new Error('candidate generation failed'),
    )

    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={client}>
        <ImportWizardPanel accessToken="token" canManage={true} />
      </QueryClientProvider>,
    )

    const input = screen.getByLabelText(/Compliance Core CSV bundle/i) as HTMLInputElement
    fireEvent.change(input, {
      target: { files: [new File(['a,b\n1,2'], 'bundle.csv', { type: 'text/csv' })] },
    })
    fireEvent.click(screen.getByRole('button', { name: /Upload and validate/i }))

    const generate = await screen.findByRole('button', { name: /Generate candidates/i })
    expect(generate).toBeEnabled()
    fireEvent.click(generate)

    expect(await screen.findByText('candidate generation failed')).toBeInTheDocument()
    expect(screen.getByRole('alert')).toBeInTheDocument()
  })
})
