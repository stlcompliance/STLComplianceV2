import { describe, expect, it } from 'vitest'

import { buildStaticPublicPaths, MARKETING_PRODUCT_KEYS, productPath } from './publicRoutes'



describe('publicRoutes', () => {

  it('includes core marketing paths and every product page', () => {

    const paths = buildStaticPublicPaths()

    expect(paths).toContain('/')
    expect(paths).toContain('/platform-overview')
    expect(paths).toContain('/products')
    expect(paths).toContain('/industries')
    expect(paths).toContain('/use-cases')
    expect(paths).toContain('/compliance')
    expect(paths).toContain('/why-stl-compliance')
    expect(paths).toContain('/about-founder')
    expect(paths).toContain('/compare')
    expect(paths).toContain('/pricing')
    expect(paths).toContain('/request-access')
    expect(paths).toContain('/contact')
    expect(paths).toContain('/faq')
    expect(paths).toContain('/resources')
    expect(paths).toContain('/security')
    expect(paths).toContain('/data-ownership')
    expect(paths).toContain('/demo')
    expect(paths).toContain('/privacy')
    expect(paths).toContain('/terms')

    for (const key of MARKETING_PRODUCT_KEYS) {
      expect(paths).toContain(productPath(key))
    }

    expect(paths).toContain('/products/field-companion')

    expect(new Set(paths).size).toBe(paths.length)

  })

})


