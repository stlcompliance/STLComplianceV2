import { describe, expect, it } from 'vitest'
import { RESOURCE_LINKS, resourcesByCategory } from './resources'

describe('RESOURCE_LINKS', () => {
  it('has unique ids and suite education entries', () => {
    const ids = RESOURCE_LINKS.map((link) => link.id)
    expect(new Set(ids).size).toBe(ids.length)
    expect(resourcesByCategory('suite').length).toBeGreaterThan(0)
  })
})
