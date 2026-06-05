import { cleanup, fireEvent, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { ProductSwitcher } from './ProductSwitcher'

describe('ProductSwitcher', () => {
  afterEach(() => {
    cleanup()
  })

  it('invokes onSelectProduct for handoff navigation', () => {
    const onSelectProduct = vi.fn()

    render(
      <ProductSwitcher
        currentProductKey="fieldcompanion"
        entitlements={['fieldcompanion', 'trainarr']}
        suiteHomeUrl="http://localhost:5174"
        productLaunchUrls={{
          fieldcompanion: 'http://localhost:5181/field-companion/launch',
          trainarr: 'http://localhost:5176/launch',
        }}
        onSelectProduct={onSelectProduct}
      />,
    )

    fireEvent.click(screen.getByRole('button', { name: /Field Companion/i }))
    fireEvent.click(screen.getByRole('menuitem', { name: /TrainArr/i }))

    expect(onSelectProduct).toHaveBeenCalledWith('trainarr')
  })

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

  it('exposes Field Companion as canonical UI', () => {
    const onSelectProduct = vi.fn()

    render(
      <ProductSwitcher
        currentProductKey="fieldcompanion"
        entitlements={['fieldcompanion', 'recordarr', 'reportarr', 'assurarr']}
        suiteHomeUrl="http://localhost:5174"
        onSelectProduct={onSelectProduct}
      />,
    )

    fireEvent.click(screen.getByRole('button', { name: /Field Companion/i }))

    expect(screen.getByRole('menuitem', { name: /Field Companion/i })).toHaveAttribute(
      'aria-current',
      'true',
    )
    expect(screen.getByRole('menuitem', { name: /RecordArr/i })).toBeInTheDocument()
    expect(screen.getByRole('menuitem', { name: /ReportArr/i })).toBeInTheDocument()
    expect(screen.getByRole('menuitem', { name: /AssurArr/i })).toBeInTheDocument()

    fireEvent.click(screen.getByRole('menuitem', { name: /AssurArr/i }))
    expect(onSelectProduct).toHaveBeenCalledWith('assurarr')
  })
})
