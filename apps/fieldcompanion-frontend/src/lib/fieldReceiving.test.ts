import { describe, expect, it } from 'vitest'
import {
  parseQuantityInput,
  receivingEditable,
  receivingPostActionLabel,
  receivingPostReady,
} from './fieldReceiving'

describe('fieldReceiving', () => {
  it('treats draft receipts as editable', () => {
    expect(receivingEditable('draft')).toBe(true)
    expect(receivingEditable('posted')).toBe(false)
  })

  it('parses non-negative quantity input', () => {
    expect(parseQuantityInput('4')).toBe(4)
    expect(parseQuantityInput('0')).toBe(0)
    expect(parseQuantityInput('-1')).toBeNull()
    expect(parseQuantityInput('')).toBeNull()
  })

  it('determines post readiness and label', () => {
    expect(receivingPostReady([{ quantityReceived: 0 }, { quantityReceived: 2 }])).toBe(true)
    expect(receivingPostReady([{ quantityReceived: 0 }])).toBe(false)
    expect(receivingPostActionLabel('draft')).toBe('Post receiving')
    expect(receivingPostActionLabel('posted')).toBeNull()
  })
})
