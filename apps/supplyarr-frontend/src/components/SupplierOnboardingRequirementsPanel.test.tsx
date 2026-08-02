import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { fireEvent, render, screen, waitFor } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'

import { SupplierOnboardingRequirementsPanel } from './SupplierOnboardingRequirementsPanel'
import * as clientApi from '../api/client'

vi.mock('../api/client', () => ({
  getSupplierOnboardingDocumentRequirements: vi.fn().mockResolvedValue({
    requirements: [
      { documentTypeKey: 'w9', label: 'W-9 tax form', isRequired: true },
      { documentTypeKey: 'insurance_certificate', label: 'Certificate of insurance', isRequired: true },
    ],
  }),
  upsertSupplierOnboardingDocumentRequirements: vi.fn().mockResolvedValue({
    requirements: [
      { documentTypeKey: 'w9', label: 'W-9 tax form', isRequired: true },
    ],
  }),
}))

describe('SupplierOnboardingRequirementsPanel', () => {
  it('saves the selected requirement keys', async () => {
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={client}>
        <SupplierOnboardingRequirementsPanel accessToken="token" canManage={true} />
      </QueryClientProvider>,
    )

    expect(await screen.findByTestId('supplier-onboarding-requirements-panel')).toBeInTheDocument()
    fireEvent.click(screen.getAllByRole('checkbox')[1])
    fireEvent.click(screen.getByTestId('supplier-onboarding-requirements-save'))

    await waitFor(() => {
      expect(vi.mocked(clientApi.upsertSupplierOnboardingDocumentRequirements)).toHaveBeenCalledWith(
        'token',
        { requiredDocumentTypeKeys: ['w9'] },
      )
    })
  })
})
