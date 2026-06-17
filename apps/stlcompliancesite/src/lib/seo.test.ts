import { afterEach, describe, expect, it, vi } from 'vitest'

import { applyPageSeo, absoluteUrl, siteBaseUrl, upsertMeta } from './seo'



describe('seo helpers', () => {

  afterEach(() => {

    document.head.innerHTML = ''

    document.title = ''

    vi.unstubAllEnvs()

  })



  it('builds absolute URLs from configured base', () => {

    vi.stubEnv('VITE_SITE_BASE_URL', 'https://example.com/')

    expect(siteBaseUrl()).toBe('https://example.com')

    expect(absoluteUrl('/products')).toBe('https://example.com/products')

    expect(absoluteUrl('/')).toBe('https://example.com')

  })



  it('applyPageSeo sets title, description, canonical, and Open Graph tags', () => {

    vi.stubEnv('VITE_SITE_BASE_URL', 'https://stlcompliance.com')

    applyPageSeo({

      title: 'Products — STL Compliance',

      description: 'Suite product overview',

      path: '/products',

    })



    expect(document.title).toBe('Products — STL Compliance')

    expect(document.querySelector('meta[name="description"]')?.getAttribute('content')).toBe(

      'Suite product overview',

    )

    expect(document.querySelector('link[rel="canonical"]')?.getAttribute('href')).toBe(

      'https://stlcompliance.com/products',

    )

    expect(document.querySelector('meta[property="og:title"]')?.getAttribute('content')).toBe(

      'Products — STL Compliance',

    )

    expect(document.querySelector('meta[name="twitter:card"]')?.getAttribute('content')).toBe(

      'summary_large_image',

    )

  })



  it('upsertMeta updates existing tags', () => {

    upsertMeta('name', 'description', 'first')

    upsertMeta('name', 'description', 'second')

    expect(document.querySelectorAll('meta[name="description"]').length).toBe(1)

    expect(document.querySelector('meta[name="description"]')?.getAttribute('content')).toBe(

      'second',

    )

  })

})


