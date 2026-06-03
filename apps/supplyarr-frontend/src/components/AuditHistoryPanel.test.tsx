import { render, screen } from '@testing-library/react'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { describe, expect, it, vi } from 'vitest'

vi.mock('@stl/shared-ui', async () => {
  const actual = await vi.importActual<typeof import('@stl/shared-ui')>('@stl/shared-ui')
  return {
    ...actual,
    StaticSearchPicker: ({
      label,
      value,
      options,
      onChange,
      placeholder,
      testId,
    }: {
      label?: string
      value: string
      options: Array<{ value: string; label: string }>
      onChange: (value: string) => void
      placeholder?: string
      testId?: string
    }) => (
      <label>
        {label ? <span>{label}</span> : null}
        <select
          aria-label={label ?? placeholder ?? 'Static search picker'}
          data-testid={testId}
          value={value}
          onChange={(event) => onChange(event.target.value)}
        >
          <option value="">{placeholder ?? 'Select…'}</option>
          {options.map((option) => (
            <option key={option.value} value={option.value}>
              {option.label}
            </option>
          ))}
        </select>
      </label>
    ),
  }
})

import { AuditHistoryPanel } from './AuditHistoryPanel'

vi.mock('../api/client', () => ({
  listAuditHistory: vi.fn().mockResolvedValue({
    items: [
      {
        id: 'audit-1',
        actorUserId: 'user-1',
        action: 'supplyarr.parties.create',
        targetType: 'external_party',
        targetId: 'party-1',
        result: 'success',
        reasonCode: null,
        correlationId: 'corr-1',
        occurredAt: new Date().toISOString(),
      },
    ],
    nextCursor: null,
    hasMore: false,
  }),
}))

describe('AuditHistoryPanel', () => {
  it('renders audit history rows', async () => {
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={client}>
        <AuditHistoryPanel accessToken="token" canRead={true} />
      </QueryClientProvider>,
    )

    expect(await screen.findByTestId('audit-history-panel')).toBeInTheDocument()
    expect(screen.getByLabelText('Target type')).toBeInTheDocument()
    expect(screen.getByLabelText('Target id')).toBeInTheDocument()
    expect(await screen.findByText('supplyarr.parties.create')).toBeInTheDocument()
  })

  it('returns null when user cannot read audit history', () => {
    const client = new QueryClient()
    const { container } = render(
      <QueryClientProvider client={client}>
        <AuditHistoryPanel accessToken="token" canRead={false} />
      </QueryClientProvider>,
    )

    expect(container).toBeEmptyDOMElement()
  })

  it('shows retry callout when audit history fails', async () => {
    const { listAuditHistory } = await import('../api/client')
    vi.mocked(listAuditHistory).mockRejectedValueOnce(new Error('audit down'))
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={client}>
        <AuditHistoryPanel accessToken="token" canRead={true} />
      </QueryClientProvider>,
    )

    expect(await screen.findByText('Audit history unavailable')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Retry history' })).toBeInTheDocument()
  })
})
