import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { MemoryRouter } from 'react-router-dom'
import { describe, expect, it } from 'vitest'
import App from './App'

describe('App routing', () => {
  it('renders TrainArr product ownership page', async () => {
    render(
      <MemoryRouter initialEntries={['/products/trainarr']}>
        <App />
      </MemoryRouter>,
    )
    expect(await screen.findByRole('heading', { name: 'TrainArr' })).toBeInTheDocument()
    expect(screen.getByText(/Owns/i)).toBeInTheDocument()
  })

  it('shows 404 for unknown paths', () => {
    render(
      <MemoryRouter initialEntries={['/unknown-path']}>
        <App />
      </MemoryRouter>,
    )
    expect(screen.getByRole('heading', { name: '404' })).toBeInTheDocument()
  })
})

describe('DemoContactPage via App', () => {
  it('shows thank-you without API calls', async () => {
    const user = userEvent.setup()
    render(
      <MemoryRouter initialEntries={['/demo']}>
        <App />
      </MemoryRouter>,
    )
    await user.type(screen.getByLabelText(/Name/i), 'Alex Operator')
    await user.type(screen.getByLabelText(/Work email/i), 'alex@example.com')
    await user.type(screen.getByLabelText(/What would you like/i), 'Suite walkthrough')
    await user.click(screen.getByRole('button', { name: /Submit request/i }))
    expect(await screen.findByTestId('demo-thank-you')).toBeInTheDocument()
  })
})
