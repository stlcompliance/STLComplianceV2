import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'

import { AssetExternalIntelligencePanel } from './AssetExternalIntelligencePanel'

describe('AssetExternalIntelligencePanel', () => {
  it('renders labeled summaries and keeps raw technical details behind disclosures', () => {
    render(
      <AssetExternalIntelligencePanel
        isLoading={false}
        overview={{
          assetId: 'asset-1',
          vin: '1HGBH41JXMN109186',
          providers: [
            {
              providerKey: 'nhtsa',
              displayName: 'NHTSA',
              description: 'Government recall data',
              sourceOfTruth: 'NHTSA API',
              status: 'active',
              supportsVinDecode: true,
              supportsRecallLookup: true,
              supportsComplaintLookup: false,
              supportsReferenceLookups: true,
              supportsEquipmentReferences: false,
              lastCheckedAt: '2026-06-10T11:30:00Z',
              lastSuccessfulAt: '2026-06-10T12:00:00Z',
              lastError: null,
            },
          ],
          summary: {
            identifierCount: 1,
            snapshotCount: 1,
            suggestionCount: 0,
            activeRecallCount: 0,
            complaintCount: 0,
            lastRefreshedAt: '2026-06-10T12:00:00Z',
          },
          identifiers: [
            {
              identifierId: 'identifier-1',
              assetId: 'asset-1',
              sourceSystem: 'VIN decoder',
              identifierType: 'VIN',
              identifierValue: '1HGBH41JXMN109186',
              normalizedValue: '1HGBH41JXMN109186',
              isPrimary: true,
              isVerified: true,
              metadata: {
                serialNumber: 'ABC123',
                source: 'inspection',
              },
              observedAt: '2026-06-10T10:00:00Z',
              createdAt: '2026-06-10T10:00:00Z',
              updatedAt: '2026-06-10T10:00:00Z',
            },
          ],
          snapshots: [
            {
              snapshotId: 'snapshot-1',
              assetId: 'asset-1',
              providerKey: 'nhtsa',
              snapshotType: 'vin_decode',
              sourceObjectRef: 'vin:1HGBH41JXMN109186',
              summary: 'VIN decode succeeded',
              details: {
                make: 'Ford',
                model: 'F-150',
              },
              capturedAt: '2026-06-10T12:00:00Z',
              createdAt: '2026-06-10T12:00:00Z',
              updatedAt: '2026-06-10T12:00:00Z',
            },
          ],
          suggestions: [],
          recalls: [],
          complaints: [],
        }}
      />,
    )

    expect(screen.getByText('NHTSA / external intelligence')).toBeInTheDocument()
    expect(screen.getByText('serialNumber')).toBeInTheDocument()
    expect(screen.getByText('ABC123')).toBeInTheDocument()
    expect(screen.getByText('make')).toBeInTheDocument()
    expect(screen.getByText('Ford')).toBeInTheDocument()
    expect(screen.getAllByText(/Advanced technical details/i)).toHaveLength(2)
  })
})
