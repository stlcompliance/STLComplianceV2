import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'

import type { ExternalVinDecodeResponse } from '../api/types'
import { AssetVinDecodePanel } from './AssetVinDecodePanel'

const decodeResult = {
  providerKey: 'nhtsa',
  vin: '1FTFW1E58MFA00001',
  normalizedVin: '1FTFW1E58MFA00001',
  modelYear: 2021,
  isPartial: true,
  searchCriteria: 'VIN:1FTFW1E58MFA00001',
  message: 'Decode completed with partial reference coverage.',
  errorCode: null,
  errorText: null,
  additionalErrorText: null,
  decodedFields: {
    Make: 'Ford',
    Model: 'F-150',
    ModelYear: '2021',
    Manufacturer: 'Ford Motor Company',
    BodyClass: 'Pickup',
    VehicleType: 'Truck',
    PlantCompanyName: 'Dearborn',
    FuelTypePrimary: 'Gasoline',
  },
  suggestions: [
    {
      suggestionId: 'suggestion-1',
      assetId: 'asset-1',
      snapshotId: 'snapshot-1',
      providerKey: 'nhtsa',
      fieldKey: 'make',
      fieldLabel: 'Make',
      currentValue: 'Unknown',
      proposedValue: 'Ford',
      reason: 'VIN decode identified the manufacturer.',
      confidence: 0.94,
      status: 'pending',
      reviewedByPersonId: null,
      reviewedAt: null,
      createdAt: '2026-06-01T00:00:00Z',
      updatedAt: '2026-06-01T00:00:00Z',
    },
  ],
  identifiers: [
    {
      identifierId: 'identifier-1',
      assetId: 'asset-1',
      sourceSystem: 'nhtsa',
      identifierType: 'vin',
      identifierValue: '1FTFW1E58MFA00001',
      normalizedValue: '1FTFW1E58MFA00001',
      isPrimary: true,
      isVerified: true,
      metadata: { make: 'Ford' },
      observedAt: '2026-06-01T00:00:00Z',
      createdAt: '2026-06-01T00:00:00Z',
      updatedAt: '2026-06-01T00:00:00Z',
    },
  ],
  snapshotId: 'snapshot-1',
  capturedAt: '2026-06-01T00:00:00Z',
} satisfies ExternalVinDecodeResponse

describe('AssetVinDecodePanel', () => {
  it('renders decode results and preview metadata', () => {
    render(
      <AssetVinDecodePanel
        vin="1FTFW1E58MFA00001"
        modelYear={2021}
        result={decodeResult}
        isLoading={false}
        error={null}
      />,
    )

    expect(screen.getByTestId('asset-vin-decode-panel')).toBeInTheDocument()
    expect(screen.getByText('VIN intelligence preview')).toBeInTheDocument()
    expect(screen.getByText('Search criteria: VIN:1FTFW1E58MFA00001')).toBeInTheDocument()
    expect(screen.getByText('Partial decode')).toBeInTheDocument()
    expect(screen.getByText('Suggested field values')).toBeInTheDocument()
    expect(screen.getAllByText('Make')).toHaveLength(2)
    expect(screen.getByText('Ford')).toBeInTheDocument()
    expect(screen.getByText('Identifiers')).toBeInTheDocument()
    expect(screen.getByText('Snapshot ID: snapshot-1')).toBeInTheDocument()
  })
})
