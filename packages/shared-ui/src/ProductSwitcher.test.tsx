import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import { ProductSwitcher } from './ProductSwitcher'

describe('ProductSwitcher', () => {
  it('lists entitled products with current product highlighted', () => {
    render(
      <ProductSwitcher
        currentProductKey="staffarr"
        entitlements={['staffarr', 'trainarr']}
        suiteHomeUrl="http://localhost:5174"
      />,
    )

    expect(screen.getByRole('link', { name: /Suite home/i })).toHaveAttribute(
      'href',
      'http://localhost:5174/app',
    )
    expect(screen.getByRole('link', { name: /StaffArr/i })).toHaveAttribute('aria-current', 'page')
    expect(screen.getByRole('link', { name: /TrainArr/i })).toHaveAttribute(
      'href',
      'http://localhost:5174/app/trainarr',
    )
  })
})
