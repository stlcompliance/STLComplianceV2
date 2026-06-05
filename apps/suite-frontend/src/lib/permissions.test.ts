import { describe, expect, it } from 'vitest'
import type { LaunchContextResponse, MeResponse } from '../api/types'
import {
  canAccessProductRoute,
  canLaunchFromContext,
  hasProductEntitlement,
  isInSuiteProduct,
  isPlatformAdmin,
} from './permissions'

describe('hasProductEntitlement', () => {
  it('matches case-insensitively', () => {
    expect(hasProductEntitlement(['StaffArr', 'nexarr'], 'staffarr')).toBe(true)
    expect(hasProductEntitlement(['staffarr'], 'trainarr')).toBe(false)
  })

  it('matches Field Companion through canonical and legacy keys', () => {
    expect(hasProductEntitlement(['fieldcompanion'], 'fieldcompanion')).toBe(true)
    expect(hasProductEntitlement(['field-companion'], 'fieldcompanion')).toBe(true)
  })
})

describe('canAccessProductRoute', () => {
  it('requires entitlement for route access', () => {
    expect(canAccessProductRoute(['staffarr'], 'staffarr')).toBe(true)
    expect(canAccessProductRoute(['staffarr'], 'trainarr')).toBe(false)
  })
})

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
