import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import { ImportWizardPanel } from './ImportWizardPanel'

vi.mock('../api/client', () => ({
  addImportWizardSupportingEvidence: vi.fn(),
  bulkConfirmImportWizardMappings: vi.fn(),
  commitImportWizard: vi.fn(),
  confirmImportWizardItem: vi.fn(),
  createImportWizardExceptionExemption: vi.fn(),
  createImportSession: vi.fn(),
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
  parseImportSession: vi.fn(),
  rejectImportWizardItem: vi.fn(),
  selectImportWizardExceptionExemption: vi.fn(),
  selectImportWizardEvidenceOption: vi.fn(),
  selectImportWizardTarget: vi.fn(),
  skipImportWizardItem: vi.fn(),
  uploadImportSessionBundle: vi.fn(),
  validateImportSession: vi.fn(),
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
})
