import { cleanup, render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { MemoryRouter } from 'react-router-dom'
import { afterEach, describe, expect, it } from 'vitest'
import App from './App'

describe('App routing', () => {
  afterEach(() => {
    cleanup()
  })

  it('renders TrainArr product ownership page', async () => {
    render(
      <MemoryRouter initialEntries={['/products/trainarr']}>
        <App />
      </MemoryRouter>,
    )
    expect(await screen.findByRole('heading', { name: 'TrainArr' })).toBeInTheDocument()
    expect(screen.getByText(/Owns/i)).toBeInTheDocument()
    expect(screen.getByTestId('ownership-source-doc')).toHaveTextContent(/docs\/02/)
  })

  it('renders Compliance Core authority education block', async () => {
    render(
      <MemoryRouter initialEntries={['/products/compliancecore']}>
        <App />
      </MemoryRouter>,
    )
    expect(await screen.findByRole('heading', { name: 'Compliance Core' })).toBeInTheDocument()
    const education = screen.getByTestId('compliance-core-education')
    expect(education).toBeInTheDocument()
    expect(education).toHaveTextContent(/Authority layer/i)
    expect(education).toHaveTextContent(/entitled product APIs/i)
  })

  it('renders maturity page with program snapshot', async () => {
    render(
      <MemoryRouter initialEntries={['/maturity']}>
        <App />
      </MemoryRouter>,
    )
    expect(
      await screen.findByRole('heading', { name: /V1 maturity by product and milestone/i }),
    ).toBeInTheDocument()
    expect(screen.getByTestId('maturity-disclaimer')).toBeInTheDocument()
    expect(screen.getByTestId('program-milestone-table')).toBeInTheDocument()
    expect(screen.getByTestId('maturity-product-companion')).toBeInTheDocument()
  })

  it('renders compare page with honest disclaimer', async () => {
    render(
      <MemoryRouter initialEntries={['/compare']}>
        <App />
      </MemoryRouter>,
    )
    expect(
      await screen.findByRole('heading', {
        name: /Spreadsheets, point tools, or a bounded product suite/i,
      }),
    ).toBeInTheDocument()
    expect(screen.getByTestId('compare-disclaimer')).toBeInTheDocument()
    expect(screen.getByTestId('compare-dimensions-table')).toBeInTheDocument()
  })

  it('renders pricing page without checkout', async () => {
    render(
      <MemoryRouter initialEntries={['/pricing']}>
        <App />
      </MemoryRouter>,
    )
    expect(
      await screen.findByRole('heading', { name: /Suite licensing through NexArr/i }),
    ).toBeInTheDocument()
    expect(screen.getByTestId('pricing-disclaimer')).toBeInTheDocument()
    expect(screen.queryByRole('button', { name: /checkout|buy now|add to cart/i })).toBeNull()
  })

  it('renders resources hub', async () => {
    render(
      <MemoryRouter initialEntries={['/resources']}>
        <App />
      </MemoryRouter>,
    )
    expect(
      await screen.findByRole('link', { name: /Products hub and ownership map/i }),
    ).toBeInTheDocument()
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
