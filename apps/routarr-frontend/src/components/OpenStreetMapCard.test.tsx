import { cleanup, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it } from 'vitest'

import { buildOpenStreetMapUrl, OpenStreetMapCard } from './OpenStreetMapCard'

describe('OpenStreetMapCard', () => {
  afterEach(() => {
    cleanup()
  })

  it('uses a stop address as the primary OpenStreetMap target when both address and coordinates exist', () => {
    render(
      <OpenStreetMapCard
        latitude={40.0001}
        longitude={-105.0002}
        label="Warehouse gate"
        addressQuery="123 Main St, Boulder, CO"
      />,
    )

    expect(screen.getByRole('link', { name: 'Open address' })).toHaveAttribute(
      'href',
      'https://www.openstreetmap.org/search?query=123%20Main%20St%2C%20Boulder%2C%20CO',
    )
    expect(screen.getByRole('link', { name: 'Open coordinates' })).toHaveAttribute(
      'href',
      expect.stringContaining('mlat=40.0001'),
    )
    expect(screen.getByText(/Open map uses the RoutArr stop address/i)).toBeInTheDocument()
  })

  it('allows an address-only OpenStreetMap search without geofence coordinates', () => {
    render(
      <OpenStreetMapCard
        latitude={null}
        longitude={null}
        label="Customer dock"
        addressQuery="500 Market St, Saint Louis, MO"
      />,
    )

    expect(screen.getByRole('link', { name: 'Open address' })).toHaveAttribute(
      'href',
      'https://www.openstreetmap.org/search?query=500%20Market%20St%2C%20Saint%20Louis%2C%20MO',
    )
    expect(screen.queryByTitle(/OpenStreetMap preview/i)).not.toBeInTheDocument()
    expect(screen.getByText(/Geofence coordinates are optional/i)).toBeInTheDocument()
  })

  it('falls back to coordinates when no stop address exists', () => {
    expect(
      buildOpenStreetMapUrl({
        latitude: 38.627,
        longitude: -90.1994,
        addressQuery: '',
      }),
    ).toEqual({
      source: 'coordinates',
      url: 'https://www.openstreetmap.org/?mlat=38.627&mlon=-90.1994#map=16/38.627/-90.1994',
    })
  })
})
