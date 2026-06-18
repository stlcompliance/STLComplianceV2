import { describe, expect, it } from 'vitest'
import { ledgArrNavItems } from './ledgarrNav'

describe('ledgArrNavItems', () => {
  it('matches the high-level LedgArr shell destinations', () => {
    expect(ledgArrNavItems.map((item) => item.label)).toEqual([
      'Dashboard',
      'General Ledger',
      'Payables',
      'Receivables',
      'Billing',
      'Banking',
      'Budgets',
      'Fixed Assets',
      'Tax',
      'Intercompany',
      'Close',
      'Reports',
      'Settings',
    ])

    expect(ledgArrNavItems.map((item) => item.to)).toEqual([
      '/dashboard',
      '/general-ledger',
      '/payables',
      '/receivables',
      '/billing',
      '/banking',
      '/budgets',
      '/fixed-assets',
      '/tax',
      '/intercompany',
      '/close',
      '/reports',
      '/settings',
    ])
  })
})
