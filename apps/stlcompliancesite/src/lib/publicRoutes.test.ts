import { describe, expect, it } from 'vitest'

import { buildStaticPublicPaths, MARKETING_PRODUCT_KEYS } from './publicRoutes'



describe('publicRoutes', () => {

  it('includes core marketing paths and every product page', () => {

    const paths = buildStaticPublicPaths()

    expect(paths).toContain('/')

    expect(paths).toContain('/products')

    expect(paths).toContain('/resources')
    expect(paths).toContain('/compare')
    expect(paths).toContain('/maturity')
    expect(paths).toContain('/pricing')

    for (const key of MARKETING_PRODUCT_KEYS) {

      expect(paths).toContain(`/products/${key}`)

    }

    expect(new Set(paths).size).toBe(paths.length)

  })

})


