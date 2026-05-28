import { render, waitFor } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { SiteSeo } from './SiteSeo'

describe('SiteSeo', () => {
  afterEach(() => {
    document.head.innerHTML = ''
    document.title = ''
    vi.unstubAllEnvs()
  })

  it('applies SEO tags on mount', async () => {
    vi.stubEnv('VITE_SITE_BASE_URL', 'https://stlcompliancesite.onrender.com')
    render(
      <SiteSeo
        title="Resources — STL Compliance"
        description="Public education resources"
        path="/resources"
      />,
    )

    await waitFor(() => {
      expect(document.title).toBe('Resources — STL Compliance')
    })
    expect(document.querySelector('meta[property="og:url"]')?.getAttribute('content')).toBe(
      'https://stlcompliancesite.onrender.com/resources',
    )
  })

  it('injects organization JSON-LD when requested', async () => {
    vi.stubEnv('VITE_SITE_BASE_URL', 'https://stlcompliancesite.onrender.com')
    const { rerender } = render(
      <SiteSeo
        title="Home"
        description="Homepage"
        path="/"
        includeOrganizationJsonLd
      />,
    )

    await waitFor(() => {
      expect(document.getElementById('stl-organization-jsonld')).toBeTruthy()
    })

    rerender(<SiteSeo title="Home" description="Homepage" path="/" />)
    await waitFor(() => {
      expect(document.getElementById('stl-organization-jsonld')).toBeNull()
    })
  })
})
