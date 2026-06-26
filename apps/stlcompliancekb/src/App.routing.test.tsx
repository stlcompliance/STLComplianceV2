import { cleanup, render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { MemoryRouter } from 'react-router-dom'
import { afterEach, describe, expect, it } from 'vitest'
import App from './App'

describe('KB app routing', () => {
  afterEach(() => {
    cleanup()
  })

  it('renders the KB home and searches articles', async () => {
    const user = userEvent.setup()
    render(
      <MemoryRouter initialEntries={['/']}>
        <App />
      </MemoryRouter>,
    )

    expect(screen.getByRole('heading', { name: /Find the next right step/i })).toBeInTheDocument()
    expect(screen.queryByText(/platform admin/i)).toBeNull()
    expect(screen.getByRole('link', { name: /I can't sign in/i })).toBeInTheDocument()

    await user.type(screen.getByPlaceholderText(/Search login/i), 'work order')
    expect(await screen.findByRole('link', { name: /How to create a work order/i })).toBeInTheDocument()
  })

  it('supports global search from article pages', async () => {
    const user = userEvent.setup()
    render(
      <MemoryRouter initialEntries={['/articles/getting-started--first-login']}>
        <App />
      </MemoryRouter>,
    )

    await user.type(screen.getByLabelText(/Search the knowledge base/i), 'certificate')
    await user.keyboard('{Enter}')
    expect(await screen.findByRole('heading', { name: /matching articles/i })).toBeInTheDocument()
    expect(screen.getByRole('link', { name: /How to issue a certificate/i })).toBeInTheDocument()
  })

  it('renders section pages', () => {
    render(
      <MemoryRouter initialEntries={['/sections/how-to']}>
        <App />
      </MemoryRouter>,
    )

    expect(screen.getByRole('heading', { name: 'How-To' })).toBeInTheDocument()
    expect(screen.getByRole('link', { name: 'AssurArr' })).toBeInTheDocument()
    expect(screen.getByRole('link', { name: /How to place or release a quality hold/i })).toBeInTheDocument()
    expect(screen.getByRole('link', { name: 'LoadArr' })).toBeInTheDocument()
    expect(screen.getByRole('link', { name: /How to receive inbound goods/i })).toBeInTheDocument()
    expect(screen.queryByText(/Start here for this section/i)).toBeNull()
  })

  it('renders article pages from docs/user markdown', () => {
    render(
      <MemoryRouter initialEntries={['/articles/getting-started--first-login']}>
        <App />
      </MemoryRouter>,
    )

    expect(screen.getByRole('heading', { name: 'First Login' })).toBeInTheDocument()
    expect(screen.getByText(/Before you start/i)).toBeInTheDocument()
    expect(screen.getByText(/Open the STL Compliance sign-in page/i)).toBeInTheDocument()
    expect(screen.getByText(/All ordinary products are available to active tenant members/i)).toBeInTheDocument()
    expect(screen.queryByText(/platform admin/i)).toBeNull()
  })

  it('normalizes plain list item casing on role pages', () => {
    render(
      <MemoryRouter initialEntries={['/articles/roles--dispatcher-guide']}>
        <App />
      </MemoryRouter>,
    )

    expect(screen.getByRole('heading', { name: 'Dispatcher Guide' })).toBeInTheDocument()
    expect(screen.getByText('Trip')).toBeInTheDocument()
    expect(screen.getByText('Dispatch exceptions')).toBeInTheDocument()
    expect(screen.queryByText('trip')).toBeNull()
    expect(screen.queryByText('dispatch exceptions')).toBeNull()
  })

  it('does not route Compliance Core UI walkthroughs in the end-user KB', () => {
    render(
      <MemoryRouter initialEntries={['/articles/how-to--compliance-core--how-to-import-rule-reference-data']}>
        <App />
      </MemoryRouter>,
    )

    expect(screen.getByRole('heading', { name: /That KB page is not available/i })).toBeInTheDocument()
    expect(screen.queryByText(/How to import rule reference data/i)).toBeNull()
  })

  it('does not route section index docs as articles', () => {
    render(
      <MemoryRouter initialEntries={['/articles/compliance']}>
        <App />
      </MemoryRouter>,
    )

    expect(screen.getByRole('heading', { name: /That KB page is not available/i })).toBeInTheDocument()
    expect(screen.queryByText(/Audit and Compliance Guides/i)).toBeNull()
    expect(screen.queryByText(/Start here for this section/i)).toBeNull()
  })
})
