import { cleanup, fireEvent, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import { LoadArrHandoffPanel } from './LoadArrHandoffPanel'
import { createProductHandoff } from '@stl/shared-ui'

vi.mock('@stl/shared-ui', async (importOriginal) => {
  const actual = await importOriginal<typeof import('@stl/shared-ui')>()
  return {
    ...actual,
    createProductHandoff: vi.fn(),
  }
})

describe('LoadArrHandoffPanel', () => {
  afterEach(() => {
    cleanup()
    vi.clearAllMocks()
  })

  it('shows a safe fallback when the LoadArr handoff fails', async () => {
    vi.mocked(createProductHandoff).mockRejectedValueOnce(new Error('handoff down'))
    const consoleError = vi.spyOn(console, 'error').mockImplementation(() => undefined)

    render(
      <LoadArrHandoffPanel
        accessToken="token"
        title="Open LoadArr"
        description="Launch supply execution."
        metrics={[{ label: 'Open', value: 1 }]}
      />,
    )

    fireEvent.click(screen.getByRole('button', { name: 'Open in LoadArr' }))

    expect(await screen.findByText('LoadArr is temporarily unavailable. Please try again.')).toBeInTheDocument()
    expect(consoleError).toHaveBeenCalled()

    consoleError.mockRestore()
  })
})
