import { PartyRegistryPanel } from '../../components/PartyRegistryPanel'
import type { SupplyArrWorkspaceState } from '../useSupplyArrWorkspaceState'

type Props = { state: SupplyArrWorkspaceState }

export function PartiesSection({ state: s }: Props) {
  return (
    <div className="grid gap-6 lg:grid-cols-2">
      <PartyRegistryPanel
        title="Vendors"
        parties={s.vendors}
        canManage={s.canManage}
        isLoading={s.vendorsQuery.isLoading}
        partyKey={s.vendorKey}
        displayName={s.vendorName}
        legalName={s.vendorLegalName}
        taxIdentifier={s.vendorTaxId}
        notes={s.vendorNotes}
        onPartyKeyChange={s.setVendorKey}
        onDisplayNameChange={s.setVendorName}
        onLegalNameChange={s.setVendorLegalName}
        onTaxIdentifierChange={s.setVendorTaxId}
        onNotesChange={s.setVendorNotes}
        onCreate={() => s.createVendorMutation.mutate()}
        isCreating={s.createVendorMutation.isPending}
      />
      <PartyRegistryPanel
        title="Suppliers"
        parties={s.suppliersQuery.data ?? []}
        canManage={false}
        isLoading={s.suppliersQuery.isLoading}
        partyKey=""
        displayName=""
        legalName=""
        taxIdentifier=""
        notes=""
        onPartyKeyChange={() => {}}
        onDisplayNameChange={() => {}}
        onLegalNameChange={() => {}}
        onTaxIdentifierChange={() => {}}
        onNotesChange={() => {}}
        onCreate={() => {}}
        isCreating={false}
      />
      <PartyRegistryPanel
        title="Dealers"
        parties={s.dealersQuery.data ?? []}
        canManage={false}
        isLoading={s.dealersQuery.isLoading}
        partyKey=""
        displayName=""
        legalName=""
        taxIdentifier=""
        notes=""
        onPartyKeyChange={() => {}}
        onDisplayNameChange={() => {}}
        onLegalNameChange={() => {}}
        onTaxIdentifierChange={() => {}}
        onNotesChange={() => {}}
        onCreate={() => {}}
        isCreating={false}
      />
    </div>
  )
}
