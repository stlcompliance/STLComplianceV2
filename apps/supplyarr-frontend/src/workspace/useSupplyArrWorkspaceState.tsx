import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'

import { useEffect, useState } from 'react'
import { Navigate, useSearchParams } from 'react-router-dom'


import {

  createPart,

  createInventoryBin,

  createInventoryLocation,

  createPartCatalog,

  createPartVendorLink,

  createPurchaseRequest,

  createPurchaseOrderFromPurchaseRequest,

  cancelBackorder,
  cancelVendorReturn,
  createBackorderFromPurchaseOrderLine,
  createReceivingException,
  createReceivingReceiptFromPurchaseOrder,
  cancelReceivingException,
  createVendorReturnFromPurchaseOrderLine,
  createVendorReturnFromStock,
  fulfillBackorder,
  getBackorders,
  getVendorReturns,
  postVendorReturn,
  getPricingSnapshots,
  getLeadTimeSnapshots,
  getAvailabilitySnapshots,
  createPricingSnapshot,
  createLeadTimeSnapshot,
  createAvailabilitySnapshot,
  getReorderEvaluation,
  upsertPartReorderPolicy,
  createPurchaseRequestFromReorder,
  createPurchaseRequestFromDemandRef,
  createStockReservation,
  fulfillStockReservation,
  getDemandRefs,
  getStockReservations,
  releaseStockReservation,

  createVendor,
  createSupplier,
  createDealer,
  updateParty,
  updatePartyApprovalStatus,
  updatePartyStatus,
  createPartyContact,
  getContractRecords,

  approvePurchaseRequest,

  approvePurchaseOrder,

  cancelPurchaseOrder,

  getDealers,

  getInventoryBins,
  transferStock,
  getStockLedger,

  getInventoryLocations,

  getMe,

  getPartCatalogs,

  getPartStockLevels,

  getParts,
  getSubstitutions,

  getPurchaseRequests,

  getPurchaseOrders,

  getReceivingReceipts,

  getSuppliers,

  issuePurchaseOrder,

  postReceivingReceipt,
  resolveReceivingException,
  reopenReceivingException,

  getVendors,

  rejectPurchaseRequest,

  submitPurchaseRequest,
  updateReceivingReceiptLine,

  upsertPartStockLevel,

} from '../api/client'

import type { UpdateExternalPartyRequest, WmsMovementResponse } from '../api/types'

import {
  canApprovePurchaseOrders,
  canApprovePurchaseRequests,
  canCreatePurchaseOrders,
  canCreatePurchaseRequests,
  canPerformReceiving,
  canReadVendorReports as userCanReadVendorReports,
  canExportVendorReports as userCanExportVendorReports,
  canReadPartsInventoryReports as userCanReadPartsInventoryReports,
  canExportPartsInventoryReports as userCanExportPartsInventoryReports,
  canReadPurchasingReports as userCanReadPurchasingReports,
  canExportPurchasingReports as userCanExportPurchasingReports,
  canReadComplianceReports as userCanReadComplianceReports,
  canExportComplianceReports as userCanExportComplianceReports,
  canUseForgivingSearch as userCanUseForgivingSearch,
  canReadAuditHistory as userCanReadAuditHistory,
  canReadSupplyReadiness as userCanReadSupplyReadiness,
  canManageInventory,
  canManageParties,
  canCreateEmergencyPurchase,
  canManagerOverrideEmergencyPurchase,
  canManageParts,
  canManageNotificationSettings,
  loadSession,
} from '../auth/sessionStorage'

export function useSupplyArrWorkspaceState() {
  const [searchParams] = useSearchParams()
  const handoff = searchParams.get('handoff')
  const handoffRedirect = handoff
    ? <Navigate to={`/launch?handoff=${encodeURIComponent(handoff)}`} replace />
    : null

  const session = loadSession()
  const accessToken = session?.accessToken ?? ''
  const [apiError] = useState<string | null>(null)

  const queryClient = useQueryClient()



  const [vendorKey, setVendorKey] = useState('')

  const [vendorName, setVendorName] = useState('')

  const [vendorLegalName, setVendorLegalName] = useState('')

  const [vendorTaxId, setVendorTaxId] = useState('')

  const [vendorNotes, setVendorNotes] = useState('')

  const [supplierKey, setSupplierKey] = useState('')
  const [supplierName, setSupplierName] = useState('')
  const [supplierLegalName, setSupplierLegalName] = useState('')
  const [supplierTaxId, setSupplierTaxId] = useState('')
  const [supplierNotes, setSupplierNotes] = useState('')

  const [dealerKey, setDealerKey] = useState('')
  const [dealerName, setDealerName] = useState('')
  const [dealerLegalName, setDealerLegalName] = useState('')
  const [dealerTaxId, setDealerTaxId] = useState('')
  const [dealerNotes, setDealerNotes] = useState('')



  const [catalogKey, setCatalogKey] = useState('')

  const [catalogName, setCatalogName] = useState('')

  const [catalogDescription, setCatalogDescription] = useState('')

  const [partKey, setPartKey] = useState('')

  const [partName, setPartName] = useState('')

  const [partCategory, setPartCategory] = useState('general')

  const [partUom, setPartUom] = useState('each')

  const [partManufacturer, setPartManufacturer] = useState('')

  const [partMfgNumber, setPartMfgNumber] = useState('')

  const [selectedCatalogId, setSelectedCatalogId] = useState('')

  const [selectedPartId, setSelectedPartId] = useState('')

  const [substitutionPartId, setSubstitutionPartId] = useState('')

  const [selectedVendorId, setSelectedVendorId] = useState('')

  const [vendorPartNumber, setVendorPartNumber] = useState('')

  const [invLocationKey, setInvLocationKey] = useState('')

  const [invLocationName, setInvLocationName] = useState('')

  const [invLocationType, setInvLocationType] = useState('warehouse')

  const [invAddressLine, setInvAddressLine] = useState('')

  const [invBinKey, setInvBinKey] = useState('')

  const [invBinName, setInvBinName] = useState('')

  const [selectedInvLocationId, setSelectedInvLocationId] = useState('')

  const [selectedStockPartId, setSelectedStockPartId] = useState('')

  const [selectedStockBinId, setSelectedStockBinId] = useState('')

  const [stockQuantity, setStockQuantity] = useState('')

  const [transferKey, setTransferKey] = useState('')
  const [transferPartId, setTransferPartId] = useState('')
  const [transferFromBinId, setTransferFromBinId] = useState('')
  const [transferToBinId, setTransferToBinId] = useState('')
  const [transferQuantity, setTransferQuantity] = useState('')
  const [transferNotes, setTransferNotes] = useState('')
  const [lastTransferResult, setLastTransferResult] = useState<WmsMovementResponse | null>(null)

  const [reservationKey, setReservationKey] = useState('')

  const [selectedReservationId, setSelectedReservationId] = useState('')

  const [selectedReservationPartId, setSelectedReservationPartId] = useState('')

  const [selectedReservationBinId, setSelectedReservationBinId] = useState('')

  const [reservationQuantity, setReservationQuantity] = useState('')

  const [reservationNotes, setReservationNotes] = useState('')

  const [reservationReleaseReason, setReservationReleaseReason] = useState('')

  const [reservationStatusFilter, setReservationStatusFilter] = useState('active')

  const [prRequestKey, setPrRequestKey] = useState('')

  const [prTitle, setPrTitle] = useState('')

  const [prNotes, setPrNotes] = useState('')

  const [prVendorId, setPrVendorId] = useState('')

  const [prPartId, setPrPartId] = useState('')

  const [prLineQty, setPrLineQty] = useState('')

  const [prLineNotes, setPrLineNotes] = useState('')

  const [prRejectionReason, setPrRejectionReason] = useState('')

  const [selectedPurchaseRequestId, setSelectedPurchaseRequestId] = useState('')

  const [poOrderKey, setPoOrderKey] = useState('')

  const [poSourcePurchaseRequestId, setPoSourcePurchaseRequestId] = useState('')

  const [selectedPurchaseOrderId, setSelectedPurchaseOrderId] = useState('')

  const [poCancellationReason, setPoCancellationReason] = useState('')

  const [receiptKey, setReceiptKey] = useState('')

  const [receiveSourcePurchaseOrderId, setReceiveSourcePurchaseOrderId] = useState('')

  const [selectedReceivingReceiptId, setSelectedReceivingReceiptId] = useState('')

  const [receiveBinId, setReceiveBinId] = useState('')

  const [selectedReceiveLineId, setSelectedReceiveLineId] = useState('')

  const [lineQuantityReceived, setLineQuantityReceived] = useState('')

  const [exceptionType, setExceptionType] = useState('short')

  const [exceptionQuantity, setExceptionQuantity] = useState('')

  const [exceptionNotes, setExceptionNotes] = useState('')

  const [exceptionCancelReason, setExceptionCancelReason] = useState('')

  const [exceptionReopenReason, setExceptionReopenReason] = useState('')

  const [backorderKey, setBackorderKey] = useState('')

  const [selectedBackorderId, setSelectedBackorderId] = useState('')

  const [selectedBackorderPoLineId, setSelectedBackorderPoLineId] = useState('')

  const [backorderQuantity, setBackorderQuantity] = useState('')

  const [backorderNotes, setBackorderNotes] = useState('')

  const [backorderCancelReason, setBackorderCancelReason] = useState('')

  const [backorderStatusFilter, setBackorderStatusFilter] = useState('open')

  const [returnKey, setReturnKey] = useState('')

  const [selectedReturnId, setSelectedReturnId] = useState('')

  const [selectedReturnVendorId, setSelectedReturnVendorId] = useState('')

  const [selectedReturnBinId, setSelectedReturnBinId] = useState('')

  const [selectedReturnPoLineId, setSelectedReturnPoLineId] = useState('')

  const [selectedReturnPartId, setSelectedReturnPartId] = useState('')

  const [returnQuantity, setReturnQuantity] = useState('')

  const [rmaNumber, setRmaNumber] = useState('')

  const [returnNotes, setReturnNotes] = useState('')

  const [returnCancelReason, setReturnCancelReason] = useState('')

  const [returnStatusFilter, setReturnStatusFilter] = useState('')

  const [returnSource, setReturnSource] = useState<'stock' | 'purchase_order_line'>('stock')

  const [pricingSnapshotKey, setPricingSnapshotKey] = useState('')

  const [leadTimeSnapshotKey, setLeadTimeSnapshotKey] = useState('')

  const [selectedSnapshotVendorLinkId, setSelectedSnapshotVendorLinkId] = useState('')

  const [snapshotUnitPrice, setSnapshotUnitPrice] = useState('')

  const [snapshotCurrencyCode, setSnapshotCurrencyCode] = useState('USD')

  const [snapshotMinimumOrderQty, setSnapshotMinimumOrderQty] = useState('')

  const [snapshotLeadTimeDays, setSnapshotLeadTimeDays] = useState('')

  const [snapshotNotes, setSnapshotNotes] = useState('')

  const [snapshotCurrentOnly, setSnapshotCurrentOnly] = useState(true)

  const [availabilitySnapshotKey, setAvailabilitySnapshotKey] = useState('')

  const [selectedAvailabilityVendorLinkId, setSelectedAvailabilityVendorLinkId] = useState('')

  const [availabilityQuantity, setAvailabilityQuantity] = useState('')

  const [availabilityStatus, setAvailabilityStatus] = useState('in_stock')

  const [availabilityNotes, setAvailabilityNotes] = useState('')

  const [availabilityCurrentOnly, setAvailabilityCurrentOnly] = useState(true)

  const [reorderPolicyPartId, setReorderPolicyPartId] = useState('')

  const [reorderPoint, setReorderPoint] = useState('')

  const [reorderQuantity, setReorderQuantity] = useState('')

  const [selectedReorderPartIds, setSelectedReorderPartIds] = useState<string[]>([])

  const [reorderPrRequestKey, setReorderPrRequestKey] = useState('')

  const [reorderPrTitle, setReorderPrTitle] = useState('')

  const [reorderPrNotes, setReorderPrNotes] = useState('')

  const [selectedDemandRefId, setSelectedDemandRefId] = useState('')
  const [demandPrRequestKey, setDemandPrRequestKey] = useState('')
  const [demandPrTitle, setDemandPrTitle] = useState('')
  const [demandPrNotes, setDemandPrNotes] = useState('')



  const meQuery = useQuery({

    queryKey: ['supplyarr-me', session?.accessToken],

    queryFn: () => getMe(session!.accessToken),

    enabled: Boolean(session?.accessToken),

  })



  const vendorsQuery = useQuery({

    queryKey: ['supplyarr-vendors', session?.accessToken],

    queryFn: () => getVendors(session!.accessToken),

    enabled: Boolean(session?.accessToken) && meQuery.isSuccess,

  })



  const suppliersQuery = useQuery({

    queryKey: ['supplyarr-suppliers', session?.accessToken],

    queryFn: () => getSuppliers(session!.accessToken),

    enabled: Boolean(session?.accessToken) && meQuery.isSuccess,

  })



  const dealersQuery = useQuery({

    queryKey: ['supplyarr-dealers', session?.accessToken],

    queryFn: () => getDealers(session!.accessToken),

    enabled: Boolean(session?.accessToken) && meQuery.isSuccess,

  })



  const catalogsQuery = useQuery({

    queryKey: ['supplyarr-catalogs', session?.accessToken],

    queryFn: () => getPartCatalogs(session!.accessToken),

    enabled: Boolean(session?.accessToken) && meQuery.isSuccess,

  })



  const partsQuery = useQuery({

    queryKey: ['supplyarr-parts', session?.accessToken],

    queryFn: () => getParts(session!.accessToken),

    enabled: Boolean(session?.accessToken) && meQuery.isSuccess,

  })



  const locationsQuery = useQuery({

    queryKey: ['supplyarr-inventory-locations', session?.accessToken],

    queryFn: () => getInventoryLocations(session!.accessToken),

    enabled: Boolean(session?.accessToken) && meQuery.isSuccess,

  })



  const binsQuery = useQuery({

    queryKey: ['supplyarr-inventory-bins', session?.accessToken, selectedInvLocationId],

    queryFn: () => getInventoryBins(session!.accessToken, selectedInvLocationId),

    enabled: Boolean(session?.accessToken) && meQuery.isSuccess && Boolean(selectedInvLocationId),

  })

  const allBinsQuery = useQuery({

    queryKey: ['supplyarr-inventory-bins-all', session?.accessToken],

    queryFn: () => getInventoryBins(session!.accessToken),

    enabled: Boolean(session?.accessToken) && meQuery.isSuccess,

  })



  const purchaseRequestsQuery = useQuery({

    queryKey: ['supplyarr-purchase-requests', session?.accessToken],

    queryFn: () => getPurchaseRequests(session!.accessToken),

    enabled: Boolean(session?.accessToken) && meQuery.isSuccess,

  })



  const purchaseOrdersQuery = useQuery({

    queryKey: ['supplyarr-purchase-orders', session?.accessToken],

    queryFn: () => getPurchaseOrders(session!.accessToken),

    enabled: Boolean(session?.accessToken) && meQuery.isSuccess,

  })



  const receivingReceiptsQuery = useQuery({

    queryKey: ['supplyarr-receiving', session?.accessToken],

    queryFn: () => getReceivingReceipts(session!.accessToken),

    enabled: Boolean(session?.accessToken) && meQuery.isSuccess,

  })



  const backordersQuery = useQuery({

    queryKey: ['supplyarr-backorders', session?.accessToken, backorderStatusFilter],

    queryFn: () =>

      getBackorders(session!.accessToken, {

        status: backorderStatusFilter || undefined,

      }),

    enabled: Boolean(session?.accessToken) && meQuery.isSuccess,

  })



  const vendorReturnsQuery = useQuery({

    queryKey: ['supplyarr-returns', session?.accessToken, returnStatusFilter],

    queryFn: () =>

      getVendorReturns(session!.accessToken, {

        status: returnStatusFilter || undefined,

      }),

    enabled: Boolean(session?.accessToken) && meQuery.isSuccess,

  })



  const pricingSnapshotsQuery = useQuery({

    queryKey: ['supplyarr-pricing-snapshots', session?.accessToken],

    queryFn: () => getPricingSnapshots(session!.accessToken),

    enabled: Boolean(session?.accessToken) && meQuery.isSuccess,

  })



  const leadTimeSnapshotsQuery = useQuery({

    queryKey: ['supplyarr-lead-time-snapshots', session?.accessToken],

    queryFn: () => getLeadTimeSnapshots(session!.accessToken),

    enabled: Boolean(session?.accessToken) && meQuery.isSuccess,

  })



  const availabilitySnapshotsQuery = useQuery({

    queryKey: ['supplyarr-availability-snapshots', session?.accessToken],

    queryFn: () => getAvailabilitySnapshots(session!.accessToken),

    enabled: Boolean(session?.accessToken) && meQuery.isSuccess,

  })



  const reorderEvaluationQuery = useQuery({

    queryKey: ['supplyarr-reorder-evaluation', session?.accessToken],

    queryFn: () => getReorderEvaluation(session!.accessToken),

    enabled: Boolean(session?.accessToken) && meQuery.isSuccess,

  })



  const demandRefsQuery = useQuery({

    queryKey: ['supplyarr-demand-refs', session?.accessToken],

    queryFn: () => getDemandRefs(session!.accessToken),

    enabled: Boolean(session?.accessToken) && meQuery.isSuccess,

  })



  const stockQuery = useQuery({

    queryKey: ['supplyarr-inventory-stock', session?.accessToken, selectedInvLocationId],

    queryFn: () =>

      getPartStockLevels(session!.accessToken, {

        locationId: selectedInvLocationId || undefined,

      }),

    enabled: Boolean(session?.accessToken) && meQuery.isSuccess,

  })

  const stockLedgerQuery = useQuery({

    queryKey: [
      'supplyarr-stock-ledger',
      session?.accessToken,
      selectedInvLocationId,
      selectedStockBinId,
      selectedStockPartId,
    ],

    queryFn: () =>
      getStockLedger(session!.accessToken, {
        locationId: selectedInvLocationId || undefined,
        binId: selectedStockBinId || undefined,
        partId: selectedStockPartId || undefined,
      }),

    enabled: Boolean(session?.accessToken) && meQuery.isSuccess,

  })



  const stockReservationsQuery = useQuery({

    queryKey: [
      'supplyarr-stock-reservations',
      session?.accessToken,
      reservationStatusFilter,
      selectedInvLocationId,
    ],

    queryFn: () =>
      getStockReservations(session!.accessToken, {
        status: reservationStatusFilter || undefined,
      }),

    enabled: Boolean(session?.accessToken) && meQuery.isSuccess,

  })



  const createVendorMutation = useMutation({

    mutationFn: () =>

      createVendor(session!.accessToken, {

        partyKey: vendorKey,

        displayName: vendorName,

        legalName: vendorLegalName,

        taxIdentifier: vendorTaxId || null,

        notes: vendorNotes,

      }),

    onSuccess: async () => {

      setVendorKey('')

      setVendorName('')

      setVendorLegalName('')

      setVendorTaxId('')

      setVendorNotes('')

      await queryClient.invalidateQueries({ queryKey: ['supplyarr-vendors'] })

    },

  })

  const createSupplierMutation = useMutation({
    mutationFn: () =>
      createSupplier(session!.accessToken, {
        partyKey: supplierKey,
        displayName: supplierName,
        legalName: supplierLegalName,
        taxIdentifier: supplierTaxId || null,
        notes: supplierNotes,
      }),
    onSuccess: async () => {
      setSupplierKey('')
      setSupplierName('')
      setSupplierLegalName('')
      setSupplierTaxId('')
      setSupplierNotes('')
      await queryClient.invalidateQueries({ queryKey: ['supplyarr-suppliers'] })
    },
  })

  const createDealerMutation = useMutation({
    mutationFn: () =>
      createDealer(session!.accessToken, {
        partyKey: dealerKey,
        displayName: dealerName,
        legalName: dealerLegalName,
        taxIdentifier: dealerTaxId || null,
        notes: dealerNotes,
      }),
    onSuccess: async () => {
      setDealerKey('')
      setDealerName('')
      setDealerLegalName('')
      setDealerTaxId('')
      setDealerNotes('')
      await queryClient.invalidateQueries({ queryKey: ['supplyarr-dealers'] })
    },
  })

  const updatePartyMutation = useMutation({
    mutationFn: ({
      route,
      partyId,
      request,
    }: {
      route: 'vendors' | 'suppliers' | 'dealers'
      partyId: string
      request: UpdateExternalPartyRequest
    }) => updateParty(session!.accessToken, route, partyId, request),
    onSuccess: async (_data, variables) => {
      await queryClient.invalidateQueries({ queryKey: [`supplyarr-${variables.route}`] })
    },
  })

  const updatePartyApprovalMutation = useMutation({
    mutationFn: ({
      route,
      partyId,
      approvalStatus,
    }: {
      route: 'vendors' | 'suppliers' | 'dealers'
      partyId: string
      approvalStatus: string
    }) =>
      updatePartyApprovalStatus(session!.accessToken, route, partyId, {
        approvalStatus,
      }),
    onSuccess: async (_data, variables) => {
      await queryClient.invalidateQueries({ queryKey: [`supplyarr-${variables.route}`] })
    },
  })

  const updatePartyStatusMutation = useMutation({
    mutationFn: ({
      route,
      partyId,
      status,
    }: {
      route: 'vendors' | 'suppliers' | 'dealers'
      partyId: string
      status: string
    }) => updatePartyStatus(session!.accessToken, route, partyId, { status }),
    onSuccess: async (_data, variables) => {
      await queryClient.invalidateQueries({ queryKey: [`supplyarr-${variables.route}`] })
    },
  })

  const addPartyContactMutation = useMutation({
    mutationFn: ({
      route,
      partyId,
      request,
    }: {
      route: 'vendors' | 'suppliers' | 'dealers'
      partyId: string
      request: {
        contactName: string
        email: string
        phone: string
        roleLabel: string
        isPrimary: boolean
      }
    }) => createPartyContact(session!.accessToken, route, partyId, request),
    onSuccess: async (_data, variables) => {
      await queryClient.invalidateQueries({ queryKey: [`supplyarr-${variables.route}`] })
    },
  })



  const createCatalogMutation = useMutation({

    mutationFn: () =>

      createPartCatalog(session!.accessToken, {

        catalogKey,

        name: catalogName,

        description: catalogDescription,

      }),

    onSuccess: async () => {

      setCatalogKey('')

      setCatalogName('')

      setCatalogDescription('')

      await queryClient.invalidateQueries({ queryKey: ['supplyarr-catalogs'] })

    },

  })



  const createPartMutation = useMutation({

    mutationFn: () =>

      createPart(session!.accessToken, {

        partKey,

        catalogId: selectedCatalogId || null,

        displayName: partName,

        description: '',

        categoryKey: partCategory,

        unitOfMeasure: partUom,

        manufacturerName: partManufacturer,

        manufacturerPartNumber: partMfgNumber,

      }),

    onSuccess: async () => {

      setPartKey('')

      setPartName('')

      setPartCategory('general')

      setPartUom('each')

      setPartManufacturer('')

      setPartMfgNumber('')

      await queryClient.invalidateQueries({ queryKey: ['supplyarr-parts'] })

    },

  })



  const linkVendorMutation = useMutation({

    mutationFn: () =>

      createPartVendorLink(session!.accessToken, selectedPartId, {

        partyId: selectedVendorId,

        vendorPartNumber,

        isPreferred: true,

      }),

    onSuccess: async () => {

      setVendorPartNumber('')

      await queryClient.invalidateQueries({ queryKey: ['supplyarr-parts'] })

    },

  })



  const createLocationMutation = useMutation({

    mutationFn: () =>

      createInventoryLocation(session!.accessToken, {

        locationKey: invLocationKey,

        name: invLocationName,

        locationType: invLocationType,

        addressLine: invAddressLine,

      }),

    onSuccess: async (created) => {

      setInvLocationKey('')

      setInvLocationName('')

      setInvAddressLine('')

      setSelectedInvLocationId(created.locationId)

      await queryClient.invalidateQueries({ queryKey: ['supplyarr-inventory-locations'] })

    },

  })



  const createBinMutation = useMutation({

    mutationFn: () =>

      createInventoryBin(session!.accessToken, selectedInvLocationId, {

        binKey: invBinKey,

        name: invBinName,

      }),

    onSuccess: async () => {

      setInvBinKey('')

      setInvBinName('')

      await queryClient.invalidateQueries({ queryKey: ['supplyarr-inventory-bins'] })

      await queryClient.invalidateQueries({ queryKey: ['supplyarr-inventory-locations'] })

    },

  })



  const createPurchaseRequestMutation = useMutation({

    mutationFn: () =>

      createPurchaseRequest(session!.accessToken, {

        requestKey: prRequestKey,

        title: prTitle,

        notes: prNotes,

        vendorPartyId: prVendorId || null,

        lines: [

          {

            partId: prPartId,

            quantityRequested: Number(prLineQty),

            notes: prLineNotes,

          },

        ],

      }),

    onSuccess: async (created) => {

      setPrRequestKey('')

      setPrTitle('')

      setPrNotes('')

      setPrVendorId('')

      setPrPartId('')

      setPrLineQty('')

      setPrLineNotes('')

      setSelectedPurchaseRequestId(created.purchaseRequestId)

      await queryClient.invalidateQueries({ queryKey: ['supplyarr-purchase-requests'] })

    },

  })



  const submitPurchaseRequestMutation = useMutation({

    mutationFn: () => submitPurchaseRequest(session!.accessToken, selectedPurchaseRequestId),

    onSuccess: async () => {

      await queryClient.invalidateQueries({ queryKey: ['supplyarr-purchase-requests'] })

    },

  })



  const approvePurchaseRequestMutation = useMutation({

    mutationFn: () => approvePurchaseRequest(session!.accessToken, selectedPurchaseRequestId),

    onSuccess: async () => {

      await queryClient.invalidateQueries({ queryKey: ['supplyarr-purchase-requests'] })

    },

  })



  const rejectPurchaseRequestMutation = useMutation({

    mutationFn: () =>

      rejectPurchaseRequest(session!.accessToken, selectedPurchaseRequestId, {

        reason: prRejectionReason,

      }),

    onSuccess: async () => {

      setPrRejectionReason('')

      await queryClient.invalidateQueries({ queryKey: ['supplyarr-purchase-requests'] })

    },

  })



  const createPurchaseOrderMutation = useMutation({

    mutationFn: () =>

      createPurchaseOrderFromPurchaseRequest(

        session!.accessToken,

        poSourcePurchaseRequestId,

        { orderKey: poOrderKey },

      ),

    onSuccess: async (created) => {

      setPoOrderKey('')

      setSelectedPurchaseOrderId(created.purchaseOrderId)

      await queryClient.invalidateQueries({ queryKey: ['supplyarr-purchase-orders'] })

    },

  })



  const approvePurchaseOrderMutation = useMutation({

    mutationFn: () => approvePurchaseOrder(session!.accessToken, selectedPurchaseOrderId),

    onSuccess: async () => {

      await queryClient.invalidateQueries({ queryKey: ['supplyarr-purchase-orders'] })

    },

  })



  const issuePurchaseOrderMutation = useMutation({

    mutationFn: () => issuePurchaseOrder(session!.accessToken, selectedPurchaseOrderId),

    onSuccess: async () => {

      await queryClient.invalidateQueries({ queryKey: ['supplyarr-purchase-orders'] })

    },

  })



  const cancelPurchaseOrderMutation = useMutation({

    mutationFn: () =>

      cancelPurchaseOrder(session!.accessToken, selectedPurchaseOrderId, {

        reason: poCancellationReason,

      }),

    onSuccess: async () => {

      setPoCancellationReason('')

      await queryClient.invalidateQueries({ queryKey: ['supplyarr-purchase-orders'] })

    },

  })



  const createReceivingReceiptMutation = useMutation({

    mutationFn: () =>

      createReceivingReceiptFromPurchaseOrder(

        session!.accessToken,

        receiveSourcePurchaseOrderId,

        { receiptKey, inventoryBinId: receiveBinId },

      ),

    onSuccess: async (created) => {

      setReceiptKey('')

      setSelectedReceivingReceiptId(created.receivingReceiptId)

      await queryClient.invalidateQueries({ queryKey: ['supplyarr-receiving'] })

    },

  })



  const postReceivingReceiptMutation = useMutation({

    mutationFn: () => postReceivingReceipt(session!.accessToken, selectedReceivingReceiptId),

    onSuccess: async () => {

      await queryClient.invalidateQueries({ queryKey: ['supplyarr-receiving'] })

      await queryClient.invalidateQueries({ queryKey: ['supplyarr-inventory-stock'] })

      await queryClient.invalidateQueries({ queryKey: ['supplyarr-purchase-orders'] })

      await queryClient.invalidateQueries({ queryKey: ['supplyarr-backorders'] })

    },

  })



  const updateReceivingLineMutation = useMutation({

    mutationFn: () =>

      updateReceivingReceiptLine(

        session!.accessToken,

        selectedReceivingReceiptId,

        selectedReceiveLineId,

        { quantityReceived: Number(lineQuantityReceived) },

      ),

    onSuccess: async () => {

      await queryClient.invalidateQueries({ queryKey: ['supplyarr-receiving'] })

    },

  })



  const createReceivingExceptionMutation = useMutation({

    mutationFn: () =>

      createReceivingException(

        session!.accessToken,

        selectedReceivingReceiptId,

        selectedReceiveLineId,

        {

          exceptionType,

          quantity: Number(exceptionQuantity),

          notes: exceptionNotes || null,

        },

      ),

    onSuccess: async () => {

      setExceptionQuantity('')

      setExceptionNotes('')

      await queryClient.invalidateQueries({ queryKey: ['supplyarr-receiving'] })

    },

  })



  const resolveReceivingExceptionMutation = useMutation({

    mutationFn: (receivingExceptionId: string) =>

      resolveReceivingException(session!.accessToken, receivingExceptionId),

    onSuccess: async () => {

      await queryClient.invalidateQueries({ queryKey: ['supplyarr-receiving'] })

    },

  })

  const cancelReceivingExceptionMutation = useMutation({

    mutationFn: ({ receivingExceptionId, reason }: { receivingExceptionId: string; reason: string }) =>

      cancelReceivingException(session!.accessToken, receivingExceptionId, { reason }),

    onSuccess: async () => {

      setExceptionCancelReason('')

      await queryClient.invalidateQueries({ queryKey: ['supplyarr-receiving'] })

    },

  })

  const reopenReceivingExceptionMutation = useMutation({

    mutationFn: ({ receivingExceptionId, reason }: { receivingExceptionId: string; reason: string }) =>

      reopenReceivingException(session!.accessToken, receivingExceptionId, { reason }),

    onSuccess: async () => {

      setExceptionReopenReason('')

      await queryClient.invalidateQueries({ queryKey: ['supplyarr-receiving'] })

    },

  })



  const createBackorderMutation = useMutation({

    mutationFn: () =>

      createBackorderFromPurchaseOrderLine(

        session!.accessToken,

        selectedBackorderPoLineId,

        {

          backorderKey,

          quantityBackordered: backorderQuantity ? Number(backorderQuantity) : null,

          notes: backorderNotes || null,

        },

      ),

    onSuccess: async (created) => {

      setBackorderKey('')

      setBackorderQuantity('')

      setBackorderNotes('')

      setSelectedBackorderId(created.backorderId)

      await queryClient.invalidateQueries({ queryKey: ['supplyarr-backorders'] })

    },

  })



  const fulfillBackorderMutation = useMutation({

    mutationFn: () => fulfillBackorder(session!.accessToken, selectedBackorderId),

    onSuccess: async () => {

      await queryClient.invalidateQueries({ queryKey: ['supplyarr-backorders'] })

    },

  })



  const cancelBackorderMutation = useMutation({

    mutationFn: () =>

      cancelBackorder(session!.accessToken, selectedBackorderId, {

        reason: backorderCancelReason,

      }),

    onSuccess: async () => {

      setBackorderCancelReason('')

      await queryClient.invalidateQueries({ queryKey: ['supplyarr-backorders'] })

    },

  })



  const createReturnMutation = useMutation({

    mutationFn: () => {

      const quantity = Number(returnQuantity)

      if (returnSource === 'stock') {

        return createVendorReturnFromStock(session!.accessToken, {

          returnKey,

          vendorPartyId: selectedReturnVendorId,

          inventoryBinId: selectedReturnBinId,

          rmaNumber: rmaNumber || null,

          notes: returnNotes || null,

          lines: [

            {

              partId: selectedReturnPartId,

              quantity,

              notes: returnNotes || null,

            },

          ],

        })

      }

      return createVendorReturnFromPurchaseOrderLine(

        session!.accessToken,

        selectedReturnPoLineId,

        {

          returnKey,

          inventoryBinId: selectedReturnBinId,

          quantity,

          rmaNumber: rmaNumber || null,

          notes: returnNotes || null,

        },

      )

    },

    onSuccess: async (created) => {

      setReturnKey('')

      setReturnQuantity('')

      setReturnNotes('')

      setRmaNumber('')

      setSelectedReturnId(created.returnId)

      await queryClient.invalidateQueries({ queryKey: ['supplyarr-returns'] })

    },

  })



  const postReturnMutation = useMutation({

    mutationFn: () => postVendorReturn(session!.accessToken, selectedReturnId),

    onSuccess: async () => {

      await queryClient.invalidateQueries({ queryKey: ['supplyarr-returns'] })

      await queryClient.invalidateQueries({ queryKey: ['supplyarr-inventory-stock'] })

    },

  })



  const cancelReturnMutation = useMutation({

    mutationFn: () =>

      cancelVendorReturn(session!.accessToken, selectedReturnId, {

        reason: returnCancelReason,

      }),

    onSuccess: async () => {

      setReturnCancelReason('')

      await queryClient.invalidateQueries({ queryKey: ['supplyarr-returns'] })

    },

  })



  const createPricingSnapshotMutation = useMutation({

    mutationFn: () =>

      createPricingSnapshot(session!.accessToken, {

        snapshotKey: pricingSnapshotKey,

        partVendorLinkId: selectedSnapshotVendorLinkId,

        unitPrice: Number(snapshotUnitPrice),

        currencyCode: snapshotCurrencyCode || 'USD',

        minimumOrderQuantity: snapshotMinimumOrderQty

          ? Number(snapshotMinimumOrderQty)

          : null,

        notes: snapshotNotes || null,

        source: 'manual',

      }),

    onSuccess: async () => {

      setPricingSnapshotKey('')

      setSnapshotUnitPrice('')

      setSnapshotMinimumOrderQty('')

      await queryClient.invalidateQueries({ queryKey: ['supplyarr-pricing-snapshots'] })

    },

  })



  const createLeadTimeSnapshotMutation = useMutation({

    mutationFn: () =>

      createLeadTimeSnapshot(session!.accessToken, {

        snapshotKey: leadTimeSnapshotKey,

        partVendorLinkId: selectedSnapshotVendorLinkId,

        leadTimeDays: Number(snapshotLeadTimeDays),

        notes: snapshotNotes || null,

        source: 'manual',

      }),

    onSuccess: async () => {

      setLeadTimeSnapshotKey('')

      setSnapshotLeadTimeDays('')

      await queryClient.invalidateQueries({ queryKey: ['supplyarr-lead-time-snapshots'] })

    },

  })



  const createAvailabilitySnapshotMutation = useMutation({

    mutationFn: () =>

      createAvailabilitySnapshot(session!.accessToken, {

        snapshotKey: availabilitySnapshotKey,

        partVendorLinkId: selectedAvailabilityVendorLinkId,

        quantityAvailable: availabilityQuantity ? Number(availabilityQuantity) : null,

        availabilityStatus,

        notes: availabilityNotes || null,

        source: 'manual',

      }),

    onSuccess: async () => {

      setAvailabilitySnapshotKey('')

      setAvailabilityQuantity('')

      await queryClient.invalidateQueries({ queryKey: ['supplyarr-availability-snapshots'] })

    },

  })



  const upsertReorderPolicyMutation = useMutation({

    mutationFn: () =>

      upsertPartReorderPolicy(session!.accessToken, reorderPolicyPartId, {

        reorderPoint: reorderPoint ? Number(reorderPoint) : null,

        reorderQuantity: reorderQuantity ? Number(reorderQuantity) : null,

      }),

    onSuccess: async () => {

      await queryClient.invalidateQueries({ queryKey: ['supplyarr-parts'] })

      await queryClient.invalidateQueries({ queryKey: ['supplyarr-reorder-evaluation'] })

    },

  })



  const createPurchaseRequestFromReorderMutation = useMutation({

    mutationFn: () =>

      createPurchaseRequestFromReorder(session!.accessToken, {

        requestKey: reorderPrRequestKey,

        title: reorderPrTitle,

        notes: reorderPrNotes,

        partIds: selectedReorderPartIds,

      }),

    onSuccess: async () => {

      setSelectedReorderPartIds([])

      setReorderPrRequestKey('')

      setReorderPrTitle('')

      setReorderPrNotes('')

      await queryClient.invalidateQueries({ queryKey: ['supplyarr-purchase-requests'] })

      await queryClient.invalidateQueries({ queryKey: ['supplyarr-reorder-evaluation'] })

    },

  })



  const createPurchaseRequestFromDemandRefMutation = useMutation({

    mutationFn: () =>

      createPurchaseRequestFromDemandRef(session!.accessToken, selectedDemandRefId, {

        requestKey: demandPrRequestKey,

        title: demandPrTitle,

        notes: demandPrNotes,

      }),

    onSuccess: async () => {

      setDemandPrRequestKey('')

      setDemandPrTitle('')

      setDemandPrNotes('')

      await queryClient.invalidateQueries({ queryKey: ['supplyarr-demand-refs'] })

      await queryClient.invalidateQueries({ queryKey: ['supplyarr-purchase-requests'] })

    },

  })



  const upsertStockMutation = useMutation({

    mutationFn: () =>

      upsertPartStockLevel(session!.accessToken, {

        partId: selectedStockPartId,

        binId: selectedStockBinId,

        quantityOnHand: Number(stockQuantity),

      }),

    onSuccess: async () => {

      setStockQuantity('')

      await queryClient.invalidateQueries({ queryKey: ['supplyarr-inventory-stock'] })
      await queryClient.invalidateQueries({ queryKey: ['supplyarr-stock-ledger'] })

    },

  })

  const transferStockMutation = useMutation({

    mutationFn: () =>
      transferStock(session!.accessToken, {
        idempotencyKey: transferKey,
        partId: transferPartId,
        fromBinId: transferFromBinId,
        toBinId: transferToBinId,
        quantity: Number(transferQuantity),
        notes: transferNotes || null,
      }),

    onSuccess: async (result) => {

      setTransferKey('')
      setTransferPartId('')
      setTransferFromBinId('')
      setTransferToBinId('')
      setTransferQuantity('')
      setTransferNotes('')
      setLastTransferResult(result)

      await queryClient.invalidateQueries({ queryKey: ['supplyarr-inventory-stock'] })
      await queryClient.invalidateQueries({ queryKey: ['supplyarr-inventory-bins'] })
      await queryClient.invalidateQueries({ queryKey: ['supplyarr-inventory-bins-all'] })
      await queryClient.invalidateQueries({ queryKey: ['supplyarr-stock-ledger'] })

    },

  })

  const createStockReservationMutation = useMutation({
    mutationFn: () =>
      createStockReservation(session!.accessToken, {
        reservationKey,
        partId: selectedReservationPartId,
        binId: selectedReservationBinId,
        quantity: Number(reservationQuantity),
        sourceType: 'manual',
        notes: reservationNotes || null,
      }),
    onSuccess: async (created) => {
      setReservationKey('')
      setReservationQuantity('')
      setReservationNotes('')
      setSelectedReservationId(created.reservationId)
      await queryClient.invalidateQueries({ queryKey: ['supplyarr-stock-reservations'] })
      await queryClient.invalidateQueries({ queryKey: ['supplyarr-inventory-stock'] })
      await queryClient.invalidateQueries({ queryKey: ['supplyarr-stock-ledger'] })
    },
  })

  const releaseStockReservationMutation = useMutation({
    mutationFn: () =>
      releaseStockReservation(session!.accessToken, selectedReservationId, {
        reason: reservationReleaseReason || null,
      }),
    onSuccess: async () => {
      setReservationReleaseReason('')
      await queryClient.invalidateQueries({ queryKey: ['supplyarr-stock-reservations'] })
      await queryClient.invalidateQueries({ queryKey: ['supplyarr-inventory-stock'] })
      await queryClient.invalidateQueries({ queryKey: ['supplyarr-stock-ledger'] })
    },
  })

  const fulfillStockReservationMutation = useMutation({
    mutationFn: () => fulfillStockReservation(session!.accessToken, selectedReservationId),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['supplyarr-stock-reservations'] })
      await queryClient.invalidateQueries({ queryKey: ['supplyarr-inventory-stock'] })
      await queryClient.invalidateQueries({ queryKey: ['supplyarr-stock-ledger'] })
    },
  })

  const me = meQuery.data

  const canManage = me ? canManageParties(me.tenantRoleKey, me.isPlatformAdmin) : false

  const canManageCatalog = me ? canManageParts(me.tenantRoleKey, me.isPlatformAdmin) : false

  const canManageInv = me ? canManageInventory(me.tenantRoleKey, me.isPlatformAdmin) : false

  const canCreatePr = me ? canCreatePurchaseRequests(me.tenantRoleKey, me.isPlatformAdmin) : false

  const canApprovePr = me ? canApprovePurchaseRequests(me.tenantRoleKey, me.isPlatformAdmin) : false

  const canCreateEmergencyPurchaseFlag = me
    ? canCreateEmergencyPurchase(me.tenantRoleKey, me.isPlatformAdmin)
    : false

  const canManagerOverrideEmergencyPurchaseFlag = me
    ? canManagerOverrideEmergencyPurchase(me.tenantRoleKey, me.isPlatformAdmin)
    : false

  const canCreatePo = me ? canCreatePurchaseOrders(me.tenantRoleKey, me.isPlatformAdmin) : false

  const canApprovePo = me ? canApprovePurchaseOrders(me.tenantRoleKey, me.isPlatformAdmin) : false

  const canReceive = me ? canPerformReceiving(me.tenantRoleKey, me.isPlatformAdmin) : false

  const canManageNotifications = me
    ? canManageNotificationSettings(me.tenantRoleKey, me.isPlatformAdmin)
    : false

  const canReadVendorReports = me
    ? userCanReadVendorReports(me.tenantRoleKey, me.isPlatformAdmin)
    : false

  const canExportVendorReports = me
    ? userCanExportVendorReports(me.tenantRoleKey, me.isPlatformAdmin)
    : false

  const canReadPartsInventoryReports = me
    ? userCanReadPartsInventoryReports(me.tenantRoleKey, me.isPlatformAdmin)
    : false

  const substitutionsQuery = useQuery({
    queryKey: ['supplyarr-substitutions', session?.accessToken, substitutionPartId],
    queryFn: () => getSubstitutions(session!.accessToken, substitutionPartId || undefined),
    enabled: Boolean(session?.accessToken) && meQuery.isSuccess && canReadPartsInventoryReports,
  })

  const canExportPartsInventoryReports = me
    ? userCanExportPartsInventoryReports(me.tenantRoleKey, me.isPlatformAdmin)
    : false

  const canReadPurchasingReports = me
    ? userCanReadPurchasingReports(me.tenantRoleKey, me.isPlatformAdmin)
    : false

  const contractsQuery = useQuery({

    queryKey: ['supplyarr-contract-records', session?.accessToken],

    queryFn: () => getContractRecords(session!.accessToken, { limit: 100 }),

    enabled: Boolean(session?.accessToken) && meQuery.isSuccess && canReadPurchasingReports,

  })

  const canExportPurchasingReports = me
    ? userCanExportPurchasingReports(me.tenantRoleKey, me.isPlatformAdmin)
    : false

  const canReadComplianceReports = me
    ? userCanReadComplianceReports(me.tenantRoleKey, me.isPlatformAdmin)
    : false

  const canExportComplianceReports = me
    ? userCanExportComplianceReports(me.tenantRoleKey, me.isPlatformAdmin)
    : false

  const canUseForgivingSearch = me
    ? userCanUseForgivingSearch(me.tenantRoleKey, me.isPlatformAdmin)
    : false

  const canReadAuditHistory = me
    ? userCanReadAuditHistory(me.tenantRoleKey, me.isPlatformAdmin)
    : false

  const canReadSupplyReadiness = me
    ? userCanReadSupplyReadiness(me.tenantRoleKey, me.isPlatformAdmin)
    : false

  const approvedPurchaseRequests =

    purchaseRequestsQuery.data?.filter((pr) => pr.status === 'approved' && pr.vendorPartyId) ?? []

  const issuedPurchaseOrders =

    purchaseOrdersQuery.data?.filter(

      (po) => po.status === 'issued' && po.lines.some((l) => l.quantityRemaining > 0),

    ) ?? []

  const issuedPurchaseOrdersWithReceived =

    purchaseOrdersQuery.data?.filter(

      (po) => po.status === 'issued' && po.lines.some((l) => l.quantityReceived > 0),

    ) ?? []

  const returnInventoryBins = (binsQuery.data ?? []).map((bin) => ({

    binId: bin.binId,

    binKey: bin.binKey,

    name: bin.name,

    label: `${bin.binKey} · ${bin.name}`,

  }))

  const selectedReceivingReceipt =

    receivingReceiptsQuery.data?.find(

      (receipt) => receipt.receivingReceiptId === selectedReceivingReceiptId,

    ) ?? null

  const selectedReceivingLine =

    selectedReceivingReceipt?.lines.find((line) => line.lineId === selectedReceiveLineId) ?? null



  useEffect(() => {

    if (!selectedReceivingReceipt) {

      setSelectedReceiveLineId('')

      setLineQuantityReceived('')

      return

    }



    const firstLine = selectedReceivingReceipt.lines[0]

    if (!selectedReceiveLineId && firstLine) {

      setSelectedReceiveLineId(firstLine.lineId)

      setLineQuantityReceived(String(firstLine.quantityReceived))

      return

    }



    if (selectedReceivingLine) {

      setLineQuantityReceived(String(selectedReceivingLine.quantityReceived))

    }

  }, [selectedReceivingReceipt, selectedReceiveLineId, selectedReceivingLine])



  const vendors = vendorsQuery.data ?? []

  const locations = locationsQuery.data ?? []

  return {
    handoffRedirect,
    ready: Boolean(session && meQuery.data),
    loadingMessage: 'Loading supply workspace…',
    me: meQuery.data!,
    session: session!,
    accessToken,
    apiError,
    vendorKey,
    vendorName,
    vendorLegalName,
    vendorTaxId,
    vendorNotes,
    supplierKey,
    supplierName,
    supplierLegalName,
    supplierTaxId,
    supplierNotes,
    dealerKey,
    dealerName,
    dealerLegalName,
    dealerTaxId,
    dealerNotes,
    catalogKey,
    catalogName,
    catalogDescription,
    partKey,
    partName,
    partCategory,
    partUom,
    partManufacturer,
    partMfgNumber,
    selectedCatalogId,
    selectedPartId,
    substitutionPartId,
    selectedVendorId,
    vendorPartNumber,
    invLocationKey,
    invLocationName,
    invLocationType,
    invAddressLine,
    invBinKey,
    invBinName,
    selectedInvLocationId,
    selectedStockPartId,
    selectedStockBinId,
    stockQuantity,
    transferKey,
    transferPartId,
    transferFromBinId,
    transferToBinId,
    transferQuantity,
    transferNotes,
    lastTransferResult,
    reservationKey,
    selectedReservationId,
    selectedReservationPartId,
    selectedReservationBinId,
    reservationQuantity,
    reservationNotes,
    reservationReleaseReason,
    reservationStatusFilter,
    prRequestKey,
    prTitle,
    prNotes,
    prVendorId,
    prPartId,
    prLineQty,
    prLineNotes,
    prRejectionReason,
    selectedPurchaseRequestId,
    poOrderKey,
    poSourcePurchaseRequestId,
    selectedPurchaseOrderId,
    poCancellationReason,
    receiptKey,
    receiveSourcePurchaseOrderId,
    selectedReceivingReceiptId,
    receiveBinId,
    selectedReceiveLineId,
    lineQuantityReceived,
    exceptionType,
    exceptionQuantity,
    exceptionNotes,
    exceptionCancelReason,
    exceptionReopenReason,
    backorderKey,
    selectedBackorderId,
    selectedBackorderPoLineId,
    backorderQuantity,
    backorderNotes,
    backorderCancelReason,
    backorderStatusFilter,
    returnKey,
    selectedReturnId,
    selectedReturnVendorId,
    selectedReturnBinId,
    selectedReturnPoLineId,
    selectedReturnPartId,
    returnQuantity,
    rmaNumber,
    returnNotes,
    returnCancelReason,
    returnStatusFilter,
    returnSource,
    pricingSnapshotKey,
    leadTimeSnapshotKey,
    selectedSnapshotVendorLinkId,
    snapshotUnitPrice,
    snapshotCurrencyCode,
    snapshotMinimumOrderQty,
    snapshotLeadTimeDays,
    snapshotNotes,
    snapshotCurrentOnly,
    availabilitySnapshotKey,
    selectedAvailabilityVendorLinkId,
    availabilityQuantity,
    availabilityStatus,
    availabilityNotes,
    availabilityCurrentOnly,
    reorderPolicyPartId,
    reorderPoint,
    reorderQuantity,
    selectedReorderPartIds,
    reorderPrRequestKey,
    reorderPrTitle,
    reorderPrNotes,
    selectedDemandRefId,
    demandPrRequestKey,
    demandPrTitle,
    demandPrNotes,
    meQuery,
    vendorsQuery,
    suppliersQuery,
    dealersQuery,
    catalogsQuery,
    partsQuery,
    substitutionsQuery,
    locationsQuery,
    binsQuery,
    allBinsQuery,
    purchaseRequestsQuery,
    purchaseOrdersQuery,
    receivingReceiptsQuery,
    backordersQuery,
    vendorReturnsQuery,
    pricingSnapshotsQuery,
    leadTimeSnapshotsQuery,
    availabilitySnapshotsQuery,
    reorderEvaluationQuery,
    contractsQuery,
    demandRefsQuery,
    stockQuery,
    stockLedgerQuery,
    stockReservationsQuery,
    createVendorMutation,
    createSupplierMutation,
    createDealerMutation,
    updatePartyMutation,
    updatePartyApprovalMutation,
    updatePartyStatusMutation,
    addPartyContactMutation,
    createCatalogMutation,
    createPartMutation,
    linkVendorMutation,
    createLocationMutation,
    createBinMutation,
    createPurchaseRequestMutation,
    submitPurchaseRequestMutation,
    approvePurchaseRequestMutation,
    rejectPurchaseRequestMutation,
    createPurchaseOrderMutation,
    approvePurchaseOrderMutation,
    issuePurchaseOrderMutation,
    cancelPurchaseOrderMutation,
    createReceivingReceiptMutation,
    postReceivingReceiptMutation,
    updateReceivingLineMutation,
    createReceivingExceptionMutation,
    resolveReceivingExceptionMutation,
    cancelReceivingExceptionMutation,
    reopenReceivingExceptionMutation,
    createBackorderMutation,
    fulfillBackorderMutation,
    cancelBackorderMutation,
    createReturnMutation,
    postReturnMutation,
    cancelReturnMutation,
    createPricingSnapshotMutation,
    createLeadTimeSnapshotMutation,
    createAvailabilitySnapshotMutation,
    upsertReorderPolicyMutation,
    createPurchaseRequestFromReorderMutation,
    createPurchaseRequestFromDemandRefMutation,
    upsertStockMutation,
    transferStockMutation,
    createStockReservationMutation,
    releaseStockReservationMutation,
    fulfillStockReservationMutation,
    canManage,
    canManageCatalog,
    canManageInv,
    canCreatePr,
    canApprovePr,
    canCreateEmergencyPurchase: canCreateEmergencyPurchaseFlag,
    canManagerOverrideEmergencyPurchase: canManagerOverrideEmergencyPurchaseFlag,
    canCreatePo,
    canApprovePo,
    canReceive,
    canManageNotifications,
    canReadVendorReports,
    canExportVendorReports,
    canReadPartsInventoryReports,
    canExportPartsInventoryReports,
    canReadPurchasingReports,
    canExportPurchasingReports,
    canReadComplianceReports,
    canExportComplianceReports,
    canUseForgivingSearch,
    canReadAuditHistory,
    canReadSupplyReadiness,
    approvedPurchaseRequests,
    issuedPurchaseOrders,
    issuedPurchaseOrdersWithReceived,
    returnInventoryBins,
    stockLedgerEntries: stockLedgerQuery.data ?? [],
    selectedReceivingReceipt,
    selectedReceivingLine,
    vendors,
    locations,
    setVendorKey,
    setVendorName,
    setVendorLegalName,
    setVendorTaxId,
    setVendorNotes,
    setSupplierKey,
    setSupplierName,
    setSupplierLegalName,
    setSupplierTaxId,
    setSupplierNotes,
    setDealerKey,
    setDealerName,
    setDealerLegalName,
    setDealerTaxId,
    setDealerNotes,
    setCatalogKey,
    setCatalogName,
    setCatalogDescription,
    setPartKey,
    setPartName,
    setPartCategory,
    setPartUom,
    setPartManufacturer,
    setPartMfgNumber,
    setSelectedCatalogId,
    setSelectedPartId,
    setSubstitutionPartId,
    setSelectedVendorId,
    setVendorPartNumber,
    setInvLocationKey,
    setInvLocationName,
    setInvLocationType,
    setInvAddressLine,
    setInvBinKey,
    setInvBinName,
    setSelectedInvLocationId,
    setSelectedStockPartId,
    setSelectedStockBinId,
    setStockQuantity,
    setTransferKey,
    setTransferPartId,
    setTransferFromBinId,
    setTransferToBinId,
    setTransferQuantity,
    setTransferNotes,
    setReservationKey,
    setSelectedReservationId,
    setSelectedReservationPartId,
    setSelectedReservationBinId,
    setReservationQuantity,
    setReservationNotes,
    setReservationReleaseReason,
    setReservationStatusFilter,
    setPrRequestKey,
    setPrTitle,
    setPrNotes,
    setPrVendorId,
    setPrPartId,
    setPrLineQty,
    setPrLineNotes,
    setPrRejectionReason,
    setSelectedPurchaseRequestId,
    setPoOrderKey,
    setPoSourcePurchaseRequestId,
    setSelectedPurchaseOrderId,
    setPoCancellationReason,
    setReceiptKey,
    setReceiveSourcePurchaseOrderId,
    setSelectedReceivingReceiptId,
    setReceiveBinId,
    setSelectedReceiveLineId,
    setLineQuantityReceived,
    setExceptionType,
    setExceptionQuantity,
    setExceptionNotes,
    setExceptionCancelReason,
    setExceptionReopenReason,
    setBackorderKey,
    setSelectedBackorderId,
    setSelectedBackorderPoLineId,
    setBackorderQuantity,
    setBackorderNotes,
    setBackorderCancelReason,
    setBackorderStatusFilter,
    setReturnKey,
    setSelectedReturnId,
    setSelectedReturnVendorId,
    setSelectedReturnBinId,
    setSelectedReturnPoLineId,
    setSelectedReturnPartId,
    setReturnQuantity,
    setRmaNumber,
    setReturnNotes,
    setReturnCancelReason,
    setReturnStatusFilter,
    setReturnSource,
    setPricingSnapshotKey,
    setLeadTimeSnapshotKey,
    setSelectedSnapshotVendorLinkId,
    setSnapshotUnitPrice,
    setSnapshotCurrencyCode,
    setSnapshotMinimumOrderQty,
    setSnapshotLeadTimeDays,
    setSnapshotNotes,
    setSnapshotCurrentOnly,
    setAvailabilitySnapshotKey,
    setSelectedAvailabilityVendorLinkId,
    setAvailabilityQuantity,
    setAvailabilityStatus,
    setAvailabilityNotes,
    setAvailabilityCurrentOnly,
    setReorderPolicyPartId,
    setReorderPoint,
    setReorderQuantity,
    setSelectedReorderPartIds,
    setReorderPrRequestKey,
    setReorderPrTitle,
    setReorderPrNotes,
    setSelectedDemandRefId,
    setDemandPrRequestKey,
    setDemandPrTitle,
    setDemandPrNotes,
  }
}

export type SupplyArrWorkspaceState = ReturnType<typeof useSupplyArrWorkspaceState>
