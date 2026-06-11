import { fireEvent, render, screen, waitFor } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'

import { AiHelpButton, AiHelpDrawer } from './AiHelpDrawer'

describe('AiHelpDrawer', () => {
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
})
