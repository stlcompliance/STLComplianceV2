import { describe, expect, it } from 'vitest'

import { RESOURCE_LINKS, resourcesByCategory } from './resources'



describe('RESOURCE_LINKS', () => {

  it('has unique ids and suite education entries', () => {

    const ids = RESOURCE_LINKS.map((link) => link.id)

    expect(new Set(ids).size).toBe(ids.length)

    expect(resourcesByCategory('suite').length).toBeGreaterThan(0)
    expect(RESOURCE_LINKS.some((link) => link.id === 'compare-approaches')).toBe(true)
    expect(RESOURCE_LINKS.some((link) => link.id === 'products-hub')).toBe(true)

  })

})


