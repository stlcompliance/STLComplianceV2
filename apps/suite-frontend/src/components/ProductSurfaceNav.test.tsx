import { describe, expect, it } from 'vitest'
import { render, screen } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import { ProductSurfaceNav } from './ProductSurfaceNav'
import type { NavigationSurfaceItem } from '../api/types'

const surfaces: NavigationSurfaceItem[] = [
  {
    surfaceKey: 'overview',
    label: 'Overview',
    relativePath: '',
    iconKey: 'dashboard',
    sortOrder: 0,
    isEnabled: true,
    permissionHint: null,
  },
  {
    surfaceKey: 'dispatch',
    label: 'Dispatch',
    relativePath: 'dispatch',
    iconKey: 'fleet',
    sortOrder: 10,
    isEnabled: true,
    permissionHint: null,
  },
  {
    surfaceKey: 'launch',
    label: 'Open RoutArr app',
    relativePath: 'launch',
    iconKey: 'fleet',
    sortOrder: 90,
    isEnabled: false,
    permissionHint: 'Requires entitlement',
  },
]

describe('ProductSurfaceNav', () => {
  it('renders only enabled surfaces', () => {
    render(
      <MemoryRouter>
        <ProductSurfaceNav productKey="routarr" surfaces={surfaces} />
      </MemoryRouter>,
    )

    expect(screen.getByRole('link', { name: /overview/i })).toBeInTheDocument()
    expect(screen.getByRole('link', { name: /dispatch/i })).toHaveAttribute('href', '/app/routarr/dispatch')
    expect(screen.queryByRole('link', { name: /open routarr app/i })).not.toBeInTheDocument()
  })
})
