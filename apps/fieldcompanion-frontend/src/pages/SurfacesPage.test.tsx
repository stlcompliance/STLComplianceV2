import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

vi.mock('../api/client', () => ({
  getFieldInbox: vi.fn(),
}))

vi.mock('../hooks/useFieldCompanionWorkspace', () => ({
  useFieldCompanionWorkspace: vi.fn(() => ({
    accessToken: 'token',
    meQuery: {
      data: {
        fieldProductKeys: ['maintainarr', 'routarr'],
      },
    },
    session: {
      accessToken: 'token',
    },
  })),
}))

const mutateAsync = vi.fn().mockResolvedValue(undefined)

vi.mock('../hooks/useFieldCompanionProductLaunch', () => ({
  useFieldCompanionProductLaunch: vi.fn(() => ({
    isPending: false,
    mutateAsync,
  })),
}))

vi.stubEnv('VITE_MAINTAINARR_FRONTEND_BASE', 'https://maintainarr.example.com')
vi.stubEnv('VITE_ROUTARR_FRONTEND_BASE', 'https://routarr.example.com')
vi.stubEnv('VITE_SUITE_URL', 'https://suite.example.com')

const api = await import('../api/client')
const { SurfacesPage } = await import('./SurfacesPage')

describe('SurfacesPage', () => {
  afterEach(() => {
    cleanup()
    mutateAsync.mockReset()
    vi.mocked(api.getFieldInbox).mockReset()
    vi.unstubAllEnvs()
  })

  function renderPage() {
    const client = new QueryClient({
      defaultOptions: {
        queries: {
          retry: false,
        },
      },
    })

    render(
      <QueryClientProvider client={client}>
        <SurfacesPage />
      </QueryClientProvider>,
    )
  }

  it('renders the workspace launcher cards with inbox counts and direct links', async () => {
    vi.mocked(api.getFieldInbox).mockResolvedValue({
      summary: {
        totalCount: 3,
        blockedCount: 0,
        countByProduct: {
          maintainarr: 2,
          routarr: 1,
        },
      },
      items: [],
      sources: [],
    } as never)

    renderPage()

    expect(await screen.findByText('Available workspaces')).toBeInTheDocument()
    expect(screen.getByText('MaintainArr')).toBeInTheDocument()
    expect(screen.getByText('RoutArr')).toBeInTheDocument()
    await waitFor(() => {
      expect(screen.getByText('2 tasks waiting in your field inbox.')).toBeInTheDocument()
      expect(screen.getByText('1 task waiting in your field inbox.')).toBeInTheDocument()
    })

    const directLinks = screen.getAllByRole('link', { name: 'Direct link' })
    expect(directLinks).toHaveLength(2)
    expect(directLinks[0]).toHaveAttribute('href', 'https://maintainarr.example.com/')
    expect(directLinks[1]).toHaveAttribute('href', 'https://routarr.example.com/')

    fireEvent.click(screen.getAllByRole('button', { name: 'Open app' })[0]!)

    await waitFor(() => {
      expect(mutateAsync).toHaveBeenCalledWith('maintainarr')
    })
  })
})
