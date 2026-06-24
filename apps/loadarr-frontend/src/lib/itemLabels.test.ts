import { describe, expect, it } from 'vitest'

import { resolveLoadArrItemLabel } from './itemLabels'

describe('resolveLoadArrItemLabel', () => {
  it('returns the item number and item name when a reference exists', () => {
    expect(
      resolveLoadArrItemLabel('item-123', [
        {
          supplyarrItemId: 'item-123',
          itemNumberSnapshot: 'PO-2001',
          itemNameSnapshot: 'Hydraulic Filter',
        },
      ]),
    ).toBe('PO-2001 · Hydraulic Filter')
  })

  it('falls back to Unknown item when a reference is missing', () => {
    expect(resolveLoadArrItemLabel('missing-item', [])).toBe('Unknown item')
  })
})
