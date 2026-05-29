import { PartyRegistryPanel } from '../../components/PartyRegistryPanel'
import { SupplierOnboardingPanel } from '../../components/SupplierOnboardingPanel'
import { VendorRestrictionsPanel } from '../../components/VendorRestrictionsPanel'
import { SupplierIncidentsPanel } from '../../components/SupplierIncidentsPanel'
import type { CreatePartyContactRequest, UpdateExternalPartyRequest } from '../../api/types'
import type { SupplyArrWorkspaceState } from '../useSupplyArrWorkspaceState'

type Props = { state: SupplyArrWorkspaceState }

function partyRegistryHandlers(
  s: SupplyArrWorkspaceState,
  route: 'vendors' | 'suppliers' | 'dealers',
) {
  return {
    onUpdateParty: (partyId: string, request: UpdateExternalPartyRequest) =>
      s.updatePartyMutation.mutate({ route, partyId, request }),
    onUpdateApprovalStatus: (partyId: string, approvalStatus: string) =>
      s.updatePartyApprovalMutation.mutate({ route, partyId, approvalStatus }),
    onUpdateStatus: (partyId: string, status: string) =>
      s.updatePartyStatusMutation.mutate({ route, partyId, status }),
    onAddContact: (partyId: string, request: CreatePartyContactRequest) =>
      s.addPartyContactMutation.mutate({ route, partyId, request }),
    isUpdating: s.updatePartyMutation.isPending,
    isUpdatingApproval: s.updatePartyApprovalMutation.isPending,
    isUpdatingStatus: s.updatePartyStatusMutation.isPending,
    isAddingContact: s.addPartyContactMutation.isPending,
  }
}

export function PartiesSection({ state: s }: Props) {
  const onboardableParties = [
    ...s.vendors,
    ...(s.suppliersQuery.data ?? []),
  ].filter((p) => p.partyType === 'vendor' || p.partyType === 'supplier')

  const vendorHandlers = partyRegistryHandlers(s, 'vendors')
  const supplierHandlers = partyRegistryHandlers(s, 'suppliers')
  const dealerHandlers = partyRegistryHandlers(s, 'dealers')

  return (
    <div className="grid gap-6 lg:grid-cols-2" data-testid="supplyarr-party-registry-workspace">
      <SupplierOnboardingPanel
        accessToken={s.accessToken}
        canManage={s.canManage}
        canReview={s.canApprovePr}
        onboardableParties={onboardableParties}
      />
      <VendorRestrictionsPanel
        accessToken={s.accessToken}
        canManage={s.canManage}
        restrictableParties={onboardableParties}
      />
      <SupplierIncidentsPanel
        accessToken={s.accessToken}
        canManage={s.canManage}
        incidentParties={onboardableParties}
      />
      <PartyRegistryPanel
        title="Vendors"
        partyType="vendors"
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
        {...vendorHandlers}
      />
      <PartyRegistryPanel
        title="Suppliers"
        partyType="suppliers"
        parties={s.suppliersQuery.data ?? []}
        canManage={s.canManage}
        isLoading={s.suppliersQuery.isLoading}
        partyKey={s.supplierKey}
        displayName={s.supplierName}
        legalName={s.supplierLegalName}
        taxIdentifier={s.supplierTaxId}
        notes={s.supplierNotes}
        onPartyKeyChange={s.setSupplierKey}
        onDisplayNameChange={s.setSupplierName}
        onLegalNameChange={s.setSupplierLegalName}
        onTaxIdentifierChange={s.setSupplierTaxId}
        onNotesChange={s.setSupplierNotes}
        onCreate={() => s.createSupplierMutation.mutate()}
        isCreating={s.createSupplierMutation.isPending}
        {...supplierHandlers}
      />
      <PartyRegistryPanel
        title="Dealers"
        partyType="dealers"
        parties={s.dealersQuery.data ?? []}
        canManage={s.canManage}
        isLoading={s.dealersQuery.isLoading}
        partyKey={s.dealerKey}
        displayName={s.dealerName}
        legalName={s.dealerLegalName}
        taxIdentifier={s.dealerTaxId}
        notes={s.dealerNotes}
        onPartyKeyChange={s.setDealerKey}
        onDisplayNameChange={s.setDealerName}
        onLegalNameChange={s.setDealerLegalName}
        onTaxIdentifierChange={s.setDealerTaxId}
        onNotesChange={s.setDealerNotes}
        onCreate={() => s.createDealerMutation.mutate()}
        isCreating={s.createDealerMutation.isPending}
        {...dealerHandlers}
      />
    </div>
  )
}
