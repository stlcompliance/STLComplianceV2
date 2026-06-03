import { cleanup, render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { MemoryRouter } from 'react-router-dom'
import { afterEach, describe, expect, it } from 'vitest'
import App from './App'

describe('App routing', () => {
  afterEach(() => {
    cleanup()
    window.localStorage.clear()
  })

  it('renders TrainArr product page', async () => {
    render(
      <MemoryRouter initialEntries={['/products/trainarr']}>
        <App />
      </MemoryRouter>,
    )
    expect(await screen.findByRole('heading', { name: 'TrainArr' })).toBeInTheDocument()
    expect(screen.getByText(/Best for/i)).toBeInTheDocument()
    expect(screen.getByTestId('ownership-source-doc')).toHaveTextContent(/secure suite login/i)
  })

  it('renders Compliance Core education block', async () => {
    render(
      <MemoryRouter initialEntries={['/products/compliancecore']}>
        <App />
      </MemoryRouter>,
    )
    expect(await screen.findByRole('heading', { name: 'Compliance Core' })).toBeInTheDocument()
    const education = screen.getByTestId('compliance-core-education')
    expect(education).toBeInTheDocument()
    expect(education).toHaveTextContent(/Rules and proof/i)
    expect(education).toHaveTextContent(/evidence expectations/i)
  })

  it('renders compare page with workflow checklist sections', async () => {
    render(
      <MemoryRouter initialEntries={['/compare']}>
        <App />
      </MemoryRouter>,
    )
    expect(
      await screen.findByRole('heading', {
        name: /Compare the whole workflow/i,
      }),
    ).toBeInTheDocument()
    expect(screen.getByTestId('usual-stack-table')).toHaveTextContent(/What still falls between/i)
    expect(screen.getByTestId('feature-checklist-table')).toHaveTextContent(
      /Qualification controls work eligibility/i,
    )
    expect(screen.getByTestId('can-work-start-list')).toHaveTextContent(/Asset is inspected/i)
    expect(screen.getByTestId('product-stack-table')).toHaveTextContent(/Compliance Core/i)
  })

  it('renders pricing page without checkout', async () => {
    render(
      <MemoryRouter initialEntries={['/pricing']}>
        <App />
      </MemoryRouter>,
    )
    expect(
      await screen.findByRole('heading', { name: /Start with the products/i }),
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
      await screen.findByRole('link', { name: /Products hub/i }),
    ).toBeInTheDocument()
  })

  it('renders privacy policy with company contact details', async () => {
    render(
      <MemoryRouter initialEntries={['/privacy']}>
        <App />
      </MemoryRouter>,
    )
    expect(await screen.findByRole('heading', { name: 'Privacy Policy' })).toBeInTheDocument()
    expect(screen.getAllByText(/STL Compliance LLC/i)[0]).toBeInTheDocument()
    expect(screen.getByText(/303 N Sparta St, Steeleville, IL 62288/i)).toBeInTheDocument()
    expect(screen.getAllByText(/privacy@stlcompliance.com/i)[0]).toBeInTheDocument()
  })

  it('renders terms and conditions with company contact details', async () => {
    render(
      <MemoryRouter initialEntries={['/terms']}>
        <App />
      </MemoryRouter>,
    )
    expect(
      await screen.findByRole('heading', { name: 'Terms and Conditions' }),
    ).toBeInTheDocument()
    expect(screen.getAllByText(/STL Compliance LLC/i)[0]).toBeInTheDocument()
    expect(screen.getByText(/303 N Sparta St, Steeleville, IL 62288/i)).toBeInTheDocument()
    expect(screen.getByText(/hello@stlcompliance.com/i)).toBeInTheDocument()
  })

  it('shows dismissible cookie notice with privacy link', async () => {
    const user = userEvent.setup()
    render(
      <MemoryRouter initialEntries={['/']}>
        <App />
      </MemoryRouter>,
    )
    const notice = await screen.findByTestId('cookie-notice')
    expect(notice).toHaveTextContent(/necessary cookies for login, security, and session management/i)
    expect(notice).toHaveTextContent(/limited usage data to fix bugs/i)
    expect(screen.getByRole('link', { name: 'Privacy Policy' })).toHaveAttribute('href', '/privacy')

    await user.click(screen.getByRole('button', { name: 'Got it' }))
    expect(screen.queryByTestId('cookie-notice')).toBeNull()
    expect(window.localStorage.getItem('stl-cookie-notice-dismissed')).toBe('true')
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
