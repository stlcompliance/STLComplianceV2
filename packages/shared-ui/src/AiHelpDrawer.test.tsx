import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import { AiHelpButton, AiHelpDrawer } from './AiHelpDrawer'

describe('AiHelpDrawer', () => {
  afterEach(() => {
    cleanup()
  })

  it('opens as a product-aware drawer and sends trimmed messages', async () => {
    const onSend = vi.fn()

    render(
      <AiHelpDrawer
        open
        productKey="staffarr"
        route="/staffarr/people"
        messages={[{ id: 'a1', role: 'assistant', text: 'Check the destination record before committing.' }]}
        onClose={vi.fn()}
        onSend={onSend}
      />,
    )

    expect(screen.getByText('staffarr · /staffarr/people')).toBeInTheDocument()
    expect(screen.getByText('Check the destination record before committing.')).toBeInTheDocument()

    fireEvent.change(screen.getByLabelText('Message'), { target: { value: '  explain this validation  ' } })
    fireEvent.click(screen.getByRole('button', { name: 'Send' }))

    await waitFor(() => expect(onSend).toHaveBeenCalledWith('explain this validation'))
  })

  it('renders a compact icon button for shell entrypoints', () => {
    const onClick = vi.fn()

    render(<AiHelpButton label="Open AI assistance" onClick={onClick} />)
    fireEvent.click(screen.getByRole('button', { name: 'Open AI assistance' }))

    expect(onClick).toHaveBeenCalledTimes(1)
  })

  it('sends the draft on Enter while preserving Shift+Enter for multiline drafts', async () => {
    const onSend = vi.fn()

    render(
      <AiHelpDrawer
        open
        productKey="staffarr"
        route="/roles"
        messages={[]}
        onClose={vi.fn()}
        onSend={onSend}
      />,
    )

    const input = screen.getByLabelText('Message')
    fireEvent.change(input, { target: { value: 'line one' } })
    await waitFor(() => expect(screen.getByRole('button', { name: 'Send' })).not.toBeDisabled())
    fireEvent.keyDown(input, { key: 'Enter', shiftKey: true })
    expect(onSend).not.toHaveBeenCalled()

    fireEvent.keyDown(input, { key: 'Enter' })

    await waitFor(() => expect(onSend).toHaveBeenCalledWith('line one'))
  })

  it('scrolls to the latest message when chat content changes', async () => {
    const scrollIntoView = vi.fn()
    const originalScrollIntoView = Element.prototype.scrollIntoView
    Object.defineProperty(Element.prototype, 'scrollIntoView', {
      configurable: true,
      value: scrollIntoView,
    })

    try {
      const { rerender } = render(
        <AiHelpDrawer
          open
          productKey="nexarr"
          route="/app/imports"
          messages={[{ id: 'a1', role: 'assistant', text: 'First answer.' }]}
          onClose={vi.fn()}
          onSend={vi.fn()}
        />,
      )

      await waitFor(() => expect(scrollIntoView).toHaveBeenCalled())
      scrollIntoView.mockClear()

      rerender(
        <AiHelpDrawer
          open
          productKey="nexarr"
          route="/app/imports"
          messages={[
            { id: 'a1', role: 'assistant', text: 'First answer.' },
            { id: 'a2', role: 'assistant', text: 'Second answer.' },
          ]}
          onClose={vi.fn()}
          onSend={vi.fn()}
        />,
      )

      await waitFor(() => expect(scrollIntoView).toHaveBeenCalledWith({ block: 'end' }))
    } finally {
      Object.defineProperty(Element.prototype, 'scrollIntoView', {
        configurable: true,
        value: originalScrollIntoView,
      })
    }
  })

  it('renders safe URLs in assistant messages as links', () => {
    render(
      <AiHelpDrawer
        open
        productKey="nexarr"
        route="/app/imports"
        messages={[
          {
            id: 'a1',
            role: 'assistant',
            text: 'Open StaffArr roles: https://app.stlcompliance.com/staffarr/roles.',
          },
        ]}
        onClose={vi.fn()}
        onSend={vi.fn()}
      />,
    )

    expect(screen.getByRole('link', { name: 'https://app.stlcompliance.com/staffarr/roles' })).toHaveAttribute(
      'href',
      'https://app.stlcompliance.com/staffarr/roles',
    )
  })
})
