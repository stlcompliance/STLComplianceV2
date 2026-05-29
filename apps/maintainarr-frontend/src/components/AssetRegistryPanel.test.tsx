import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import { AssetRegistryPanel } from './AssetRegistryPanel'

describe('AssetRegistryPanel', () => {
  it('renders asset registry lists', () => {
    render(
      <AssetRegistryPanel
        canManage={false}
        classes={[
          {
            assetClassId: '11111111-1111-1111-1111-111111111111',
            classKey: 'heavy-equipment',
            name: 'Heavy Equipment',
            description: '',
            status: 'active',
            createdAt: '2026-05-27T00:00:00Z',
          },
        ]}
        types={[]}
        readinessByAssetId={{
          '22222222-2222-2222-2222-222222222222': {
            assetId: '22222222-2222-2222-2222-222222222222',
            assetTag: 'EX-1001',
            assetName: 'Excavator 1001',
            lifecycleStatus: 'active',
            readinessStatus: 'not_ready',
            blockerCount: 1,
            primaryBlockerMessage: 'Critical defect open: Hydraulic leak',
          },
        }}
        selectedAssetId={null}
        onSelectAsset={() => {}}
        isReadinessLoading={false}
        assets={[
          {
            assetId: '22222222-2222-2222-2222-222222222222',
            assetTypeId: '33333333-3333-3333-3333-333333333333',
            typeKey: 'excavator',
            typeName: 'Excavator',
            classKey: 'heavy-equipment',
            className: 'Heavy Equipment',
            assetTag: 'EX-1001',
            name: 'Excavator 1001',
            description: '',
            lifecycleStatus: 'active',
            siteRef: null,
            createdAt: '2026-05-27T00:00:00Z',
            updatedAt: '2026-05-27T00:00:00Z',
          },
        ]}
        isLoading={false}
        className=""
        classDescription=""
        classKeyManualOverride=""
        confirmedClassKey={null}
        selectedClassId=""
        typeName=""
        typeDescription=""
        typeKeyManualOverride=""
        confirmedTypeKey={null}
        selectedTypeId=""
        assetTag=""
        assetName=""
        assetDescription=""
        siteRef=""
        onClassNameChange={() => {}}
        onClassDescriptionChange={() => {}}
        onClassKeyManualOverrideChange={() => {}}
        onSelectedClassIdChange={() => {}}
        onTypeNameChange={() => {}}
        onTypeDescriptionChange={() => {}}
        onTypeKeyManualOverrideChange={() => {}}
        onSelectedTypeIdChange={() => {}}
        onAssetTagChange={() => {}}
        onAssetNameChange={() => {}}
        onAssetDescriptionChange={() => {}}
        onSiteRefChange={() => {}}
        onCreateClass={() => {}}
        onCreateType={() => {}}
        onCreateAsset={() => {}}
        isCreatingClass={false}
        isCreatingType={false}
        isCreatingAsset={false}
      />,
    )

    expect(screen.getByText('Heavy Equipment')).toBeInTheDocument()
    expect(screen.getByText('EX-1001')).toBeInTheDocument()
    expect(screen.getByText('Not ready (1)')).toBeInTheDocument()
    expect(screen.getByText('Critical defect open: Hydraulic leak')).toBeInTheDocument()
  })
})
