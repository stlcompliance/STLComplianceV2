import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, fireEvent, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import { VendorRestrictionsPanel } from './VendorRestrictionsPanel'

vi.mock('@stl/shared-ui', async (importOriginal) => {
  const actual = await importOriginal<typeof import('@stl/shared-ui')>()

  return {
    ...actual,
    StaticSearchPicker: ({
      label,
      value,
      onChange,
      options,
      placeholder,
      testId,
    }: {
      label: string
      value: string
      onChange: (value: string) => void
      options: Array<{ value: string; label: string }>
      placeholder?: string
      testId?: string
    }) => (
      <label>
        <span>{label}</span>
        <input
          aria-label={label}
          data-testid={testId}
          placeholder={placeholder}
          value={value}
          onChange={(event) => onChange(event.target.value)}
        />
        <div data-testid={`${testId ?? 'picker'}-options`}>
          {options.map((option) => (
            <span key={option.value}>{option.label}</span>
          ))}
        </div>
      </label>
    ),
  }
})

vi.mock('../api/client', () => ({
  listVendorRestrictions: vi.fn().mockResolvedValue([]),
  listPartyVendorRestrictions: vi.fn().mockResolvedValue([]),
  getPartyVendorRestrictionEnforcement: vi.fn().mockResolvedValue({
    externalPartyId: 'party-1',
    isBlocked: false,
    blockReason: null,
    activeScopes: [],
  }),
  createPartyVendorRestriction: vi.fn(),
  liftVendorRestriction: vi.fn(),
}))

describe('VendorRestrictionsPanel', () => {
  afterEach(() => {
    cleanup()
  })

  it('renders when user can manage', async () => {
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={client}>
        <VendorRestrictionsPanel
          accessToken="token"
          canManage={true}
          restrictableParties={[
            {
              partyId: 'party-1',
              partyKey: 'V-1',
              partyType: 'vendor',
              unitKind: 'identity',
              displayName: 'Test Vendor',
              legalName: '',
              taxIdentifier: null,
              approvalStatus: 'approved',
              status: 'active',
              notes: '',
              contacts: [],
              createdAt: '',
              updatedAt: '',
            },
          ]}
        />
      </QueryClientProvider>,
    )
    expect(await screen.findByTestId('vendor-restrictions-panel')).toBeInTheDocument()
    expect(screen.getByText('Supplier restrictions')).toBeInTheDocument()
  })

  it('uses a searchable picker for supplier hierarchy selection', async () => {
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={client}>
        <VendorRestrictionsPanel
          accessToken="token"
          canManage={true}
          restrictableParties={[
            {
              partyId: 'party-1',
              partyKey: 'V-1',
              partyType: 'vendor',
              unitKind: 'identity',
              displayName: 'Test Vendor',
              legalName: '',
              taxIdentifier: null,
              approvalStatus: 'approved',
              status: 'active',
              notes: '',
              contacts: [],
              createdAt: '',
              updatedAt: '',
            },
            {
              partyId: 'party-2',
              partyKey: 'S-1',
              partyType: 'supplier',
              unitKind: 'sub_unit',
              displayName: 'Second Supplier',
              legalName: '',
              taxIdentifier: null,
              approvalStatus: 'approved',
              status: 'active',
              notes: '',
              contacts: [],
              createdAt: '',
              updatedAt: '',
            },
          ]}
        />
      </QueryClientProvider>,
    )

    expect(await screen.findByTestId('vendor-restrictions-panel')).toBeInTheDocument()
    expect(screen.getByTestId('vendor-restriction-party-picker-options')).toHaveTextContent(
      'supplier identity · V-1 · Test Vendor',
    )
    expect(screen.getByTestId('vendor-restriction-party-picker-options')).toHaveTextContent(
      'sub-unit · S-1 · Second Supplier',
    )

    fireEvent.change(screen.getByTestId('vendor-restriction-party-picker'), {
      target: { value: 'party-2' },
    })

    expect(screen.getByLabelText(/Restriction reason/i)).toBeInTheDocument()
  })

  it('returns null when user cannot manage', () => {
    const client = new QueryClient()
    const { container } = render(
      <QueryClientProvider client={client}>
        <VendorRestrictionsPanel accessToken="token" canManage={false} restrictableParties={[]} />
      </QueryClientProvider>,
    )
    expect(container).toBeEmptyDOMElement()
  })
})
