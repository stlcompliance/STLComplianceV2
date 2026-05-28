import { describe, expect, it } from 'vitest'
import { contactMailto, siteConfig } from './siteConfig'

describe('siteConfig', () => {
  it('defaults suite login to local suite port', () => {
    expect(siteConfig.suiteLoginUrl).toContain('/login')
  })

  it('builds mailto with subject', () => {
    expect(contactMailto('Demo')).toMatch(/^mailto:/)
    expect(contactMailto('Demo')).toContain('subject=Demo')
  })
})
