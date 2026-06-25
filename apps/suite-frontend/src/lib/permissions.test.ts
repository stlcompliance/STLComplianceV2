import { describe, expect, it } from 'vitest'
import type { LaunchContextResponse, MeResponse } from '../api/types'
import {
  canLaunchFromContext,
  isInSuiteProduct,
  isPlatformAdmin,
} from './permissions'

describe('canLaunchFromContext', () => {
  it('reflects server canLaunch flag', () => {
    const allowed = {
      canLaunch: true,
    } as LaunchContextResponse
    const denied = {
      canLaunch: false,
    } as LaunchContextResponse
    expect(canLaunchFromContext(allowed)).toBe(true)
    expect(canLaunchFromContext(denied)).toBe(false)
  })
})

describe('isPlatformAdmin', () => {
  it('reads me profile flag', () => {
    expect(isPlatformAdmin({ isPlatformAdmin: true } as MeResponse)).toBe(true)
    expect(isPlatformAdmin({ isPlatformAdmin: false } as MeResponse)).toBe(false)
    expect(isPlatformAdmin(undefined)).toBe(false)
  })
})

describe('isInSuiteProduct', () => {
  it('only treats nexarr as in-suite', () => {
    expect(isInSuiteProduct('nexarr')).toBe(true)
    expect(isInSuiteProduct('NexArr')).toBe(true)
    expect(isInSuiteProduct('nex-arr')).toBe(true)
    expect(isInSuiteProduct('staffarr')).toBe(false)
  })
})
