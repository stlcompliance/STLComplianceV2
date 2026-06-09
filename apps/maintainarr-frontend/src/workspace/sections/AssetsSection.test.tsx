import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, fireEvent, render, screen } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import { afterEach, describe, expect, it, vi } from 'vitest'

import type { MaintainArrWorkspaceState } from '../useMaintainArrWorkspaceState'
import { AssetsSection } from './AssetsSection'

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

const state = {
  assetsQuery: {
    data: [
      {
        assetId: 'asset-1',
        assetTag: 'TRK-01',
        name: 'Truck 01',
      },
    ],
    isLoading: false,
  },
  assetReadinessFleetQuery: { data: [], isLoading: false },
  classesQuery: { data: [], isLoading: false },
  typesQuery: { data: [], isLoading: false },
  selectedAssetId: null,
  setSelectedAssetId: vi.fn(),
  assetReadinessDetailQuery: { data: null, isLoading: false },
  assetReadinessHistoryQuery: { data: null, isLoading: false },
  assetFieldContextQuery: { data: null, isLoading: false },
  assetInstalledComponentsQuery: { data: [], isLoading: false },
} as unknown as MaintainArrWorkspaceState

describe('AssetsSection', () => {
  afterEach(() => {
    cleanup()
    vi.clearAllMocks()
  })

  it('renders asset details picker when the details route has no selected asset', () => {
    const queryClient = new QueryClient({
      defaultOptions: { queries: { retry: false } },
    })
    render(
      <MemoryRouter initialEntries={['/assets/details/missing']}>
        <QueryClientProvider client={queryClient}>
          <AssetsSection state={state} />
        </QueryClientProvider>
      </MemoryRouter>,
    )

    expect(screen.getByTestId('maintainarr-assets-workspace')).toBeInTheDocument()
    expect(screen.getByTestId('maintainarr-assets-picker')).toBeInTheDocument()

    fireEvent.change(screen.getByTestId('maintainarr-assets-picker'), {
      target: { value: 'asset-1' },
    })

    expect(state.setSelectedAssetId).toHaveBeenCalledWith('asset-1')
  })
})
