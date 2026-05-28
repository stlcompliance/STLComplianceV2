import { fireEvent, render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import { ProductSwitcher } from './ProductSwitcher'

describe('ProductSwitcher', () => {
  it('opens an entitlement-aware dropdown with launch URLs', () => {
    render(
      <ProductSwitcher
        currentProductKey="staffarr"
        entitlements={['staffarr', 'trainarr']}
        suiteHomeUrl="http://localhost:5174"
        productLaunchUrls={{
          staffarr: 'http://localhost:5175/launch',
          trainarr: 'http://localhost:5176/launch',
        }}
      />,
    )

    expect(screen.getByRole('button', { name: /StaffArr/i })).toBeInTheDocument()

    fireEvent.click(screen.getByRole('button', { name: /StaffArr/i }))

    expect(screen.getByRole('menuitem', { name: /StaffArr/i })).toHaveAttribute('aria-current', 'true')
    expect(screen.getByRole('menuitem', { name: /TrainArr/i })).toHaveAttribute(
      'href',
      'http://localhost:5176/launch',
    )
  })
})
