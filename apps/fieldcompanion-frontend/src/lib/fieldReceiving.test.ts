import { describe, expect, it } from 'vitest'
import {
  receivingPostActionLabel,
  receivingPostReady,
} from './fieldReceiving'

describe('fieldReceiving', () => {
  it('determines completion readiness and label for loadarr sessions', () => {
    expect(receivingPostReady([{ quantityReceived: 0 }, { quantityReceived: 2 }])).toBe(true)
    expect(receivingPostReady([{ quantityReceived: 0 }])).toBe(false)
    expect(receivingPostActionLabel('open', 'loadarr')).toBe('Complete receiving')
    expect(receivingPostActionLabel('inspection_required', 'loadarr', 'Compliance hold')).toBeNull()
    expect(receivingPostActionLabel('draft', 'supplyarr')).toBeNull()
  })
})
