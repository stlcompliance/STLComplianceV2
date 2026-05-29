import { describe, expect, it } from 'vitest'

import { buildDowntimeDeepLinkPath, parseDowntimeDeepLink } from './downtimeDeepLink'

describe('downtimeDeepLink', () => {
  it('parses downtime context from search params', () => {
    expect(
      parseDowntimeDeepLink(
        '?assetId=11111111-1111-1111-1111-111111111111&workOrderId=22222222-2222-2222-2222-222222222222',
      ),
    ).toEqual({
      assetId: '11111111-1111-1111-1111-111111111111',
      workOrderId: '22222222-2222-2222-2222-222222222222',
      defectId: null,
      eventId: null,
    })
  })

  it('builds downtime deep link path', () => {
    expect(
      buildDowntimeDeepLinkPath({
        assetId: '11111111-1111-1111-1111-111111111111',
        workOrderId: null,
        defectId: '33333333-3333-3333-3333-333333333333',
        eventId: '44444444-4444-4444-4444-444444444444',
      }),
    ).toBe(
      '/downtime?assetId=11111111-1111-1111-1111-111111111111&defectId=33333333-3333-3333-3333-333333333333&eventId=44444444-4444-4444-4444-444444444444',
    )
  })
})
