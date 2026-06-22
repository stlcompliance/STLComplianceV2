import { describe, expect, it } from 'vitest'
import { ledgArrNavItems } from './ledgarrNav'

describe('ledgArrNavItems', () => {
  it('matches the high-level LedgArr shell destinations', () => {
    expect(ledgArrNavItems.map((item) => item.label)).toEqual([
      'Dashboard',
      'Legal Entities',
      'General Ledger',
      'Payables',
      'Receivables',
      'Cash & Bank',
      'Budgets',
      'Cost Accounting',
      'Projects & Jobs',
      'Fixed Assets',
      'Payroll Financials',
      'Taxes',
      'Intercompany',
      'Consolidation',
      'Close',
      'Reports',
      'Settings',
    ])

    expect(ledgArrNavItems.map((item) => item.to)).toEqual([
      '/dashboard',
      '/legal-entities',
      '/general-ledger',
      '/payables',
      '/receivables',
      '/cash-and-bank',
      '/budgets',
      '/cost-accounting',
      '/projects-jobs',
      '/fixed-assets',
      '/payroll-financials',
      '/taxes',
      '/intercompany',
      '/consolidation',
      '/close',
      '/reports',
      '/settings',
    ])
  })
})
