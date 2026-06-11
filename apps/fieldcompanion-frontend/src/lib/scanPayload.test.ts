import { describe, expect, it } from 'vitest'
import { normalizeScanPayload } from './scanPayload'

const assignmentId = 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa'
const loadArrTaskId = '11111111-1111-1111-1111-111111111111'

describe('normalizeScanPayload', () => {
  it('passes through task keys', () => {
    const raw = `trainarr:assignment:${assignmentId}`
    expect(normalizeScanPayload(raw)).toBe(raw)
  })

  it('passes through hyphenated task keys', () => {
    const raw = `maintainarr:work-order:${assignmentId}`
    expect(normalizeScanPayload(raw)).toBe(raw)
  })

  it('maps assignment deep link paths', () => {
    expect(normalizeScanPayload(`/assignments/${assignmentId}`)).toBe(
      `trainarr:assignment:${assignmentId}`,
    )
  })

  it('unwraps stl-field-task prefix', () => {
    expect(normalizeScanPayload(`stl-field-task:trainarr:assignment:${assignmentId}`)).toBe(
      `trainarr:assignment:${assignmentId}`,
    )
  })

  it('extracts loadarr task key from relative deep link query', () => {
    expect(
      normalizeScanPayload(
        `/work/receiving/recv-24018?taskKey=loadarr:receiving:${loadArrTaskId}`,
      ),
    ).toBe(`loadarr:receiving:${loadArrTaskId}`)
  })

  it('does not remap legacy supplyarr receiving paths', () => {
    expect(normalizeScanPayload(`/receiving/${assignmentId}`)).toBe(`/receiving/${assignmentId}`)
  })
})
