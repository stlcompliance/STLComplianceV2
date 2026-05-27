import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'

import { useEffect, useState } from 'react'

import { Navigate } from 'react-router-dom'

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
  getDemandRefs,

  createVendor,

  approvePurchaseRequest,

  approvePurchaseOrder,

  getDealers,

  getInventoryBins,

  getInventoryLocations,

  getMe,

  getPartCatalogs,

  getPartStockLevels,

  getParts,

  getPurchaseRequests,

  getPurchaseOrders,

  getReceivingReceipts,

  getSuppliers,

  issuePurchaseOrder,

  postReceivingReceipt,
  resolveReceivingException,

  getVendors,

  rejectPurchaseRequest,

  submitPurchaseRequest,
  updateReceivingReceiptLine,

  upsertPartStockLevel,

} from '../api/client'

import {
  canApprovePurchaseOrders,
  canApprovePurchaseRequests,
  canCreatePurchaseOrders,
  canCreatePurchaseRequests,
  canPerformReceiving,
  canManageInventory,
  canManageParties,
  canManageParts,
  clearSession,
  loadSession,
} from '../auth/sessionStorage'

import { InventoryPanel } from '../components/InventoryPanel'

import { PurchaseOrderPanel } from '../components/PurchaseOrderPanel'

import { BackordersPanel } from '../components/BackordersPanel'
import { ReturnsPanel } from '../components/ReturnsPanel'
import { PricingLeadTimePanel } from '../components/PricingLeadTimePanel'
import { AvailabilitySnapshotsPanel } from '../components/AvailabilitySnapshotsPanel'
import { DemandRefsPanel } from '../components/DemandRefsPanel'
import { ReorderEvaluationPanel } from '../components/ReorderEvaluationPanel'
import { ReceivingPanel } from '../components/ReceivingPanel'

import { PurchaseRequestPanel } from '../components/PurchaseRequestPanel'

import { PartCatalogPanel } from '../components/PartCatalogPanel'

import { PartyRegistryPanel } from '../components/PartyRegistryPanel'



export function HomePage() {

  const session = loadSession()

  const queryClient = useQueryClient()



  const [vendorKey, setVendorKey] = useState('')

  const [vendorName, setVendorName] = useState('')

  const [vendorLegalName, setVendorLegalName] = useState('')

  const [vendorTaxId, setVendorTaxId] = useState('')

  const [vendorNotes, setVendorNotes] = useState('')



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

  const [receiptKey, setReceiptKey] = useState('')

  const [receiveSourcePurchaseOrderId, setReceiveSourcePurchaseOrderId] = useState('')

  const [selectedReceivingReceiptId, setSelectedReceivingReceiptId] = useState('')

  const [receiveBinId, setReceiveBinId] = useState('')

  const [selectedReceiveLineId, setSelectedReceiveLineId] = useState('')

  const [lineQuantityReceived, setLineQuantityReceived] = useState('')

  const [exceptionType, setExceptionType] = useState('short')

  const [exceptionQuantity, setExceptionQuantity] = useState('')

  const [exceptionNotes, setExceptionNotes] = useState('')

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

    },

  })



  if (!session) {

    return <Navigate to="/launch" replace />

  }



  if (meQuery.isError) {

    clearSession()

    return <Navigate to="/launch" replace />

  }



  const me = meQuery.data

  const canManage = me ? canManageParties(me.tenantRoleKey, me.isPlatformAdmin) : false

  const canManageCatalog = me ? canManageParts(me.tenantRoleKey, me.isPlatformAdmin) : false

  const canManageInv = me ? canManageInventory(me.tenantRoleKey, me.isPlatformAdmin) : false

  const canCreatePr = me ? canCreatePurchaseRequests(me.tenantRoleKey, me.isPlatformAdmin) : false

  const canApprovePr = me ? canApprovePurchaseRequests(me.tenantRoleKey, me.isPlatformAdmin) : false

  const canCreatePo = me ? canCreatePurchaseOrders(me.tenantRoleKey, me.isPlatformAdmin) : false

  const canApprovePo = me ? canApprovePurchaseOrders(me.tenantRoleKey, me.isPlatformAdmin) : false

  const canReceive = me ? canPerformReceiving(me.tenantRoleKey, me.isPlatformAdmin) : false

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



  return (

    <main className="mx-auto max-w-6xl p-6">

      <header className="mb-8 flex flex-wrap items-center justify-between gap-4 border-b border-slate-800 pb-6">

        <div>

          <h1 className="text-2xl font-semibold text-white">SupplyArr</h1>

          <p className="mt-1 text-sm text-slate-400">

            Vendor registry, part catalog, inventory, procurement, receiving, backorders, and
            returns.

          </p>

        </div>

        {me ? (

          <div className="text-right text-sm text-slate-400">

            <div className="text-slate-200">{me.displayName}</div>

            <div>{me.tenantRoleKey}</div>

          </div>

        ) : null}

      </header>



      <div className="grid gap-6 lg:grid-cols-2">

        <ReceivingPanel

          receivingReceipts={receivingReceiptsQuery.data ?? []}

          issuedPurchaseOrders={issuedPurchaseOrders}

          bins={binsQuery.data ?? []}

          canPerform={canReceive}

          isLoading={receivingReceiptsQuery.isLoading}

          receiptKey={receiptKey}

          selectedPurchaseOrderId={receiveSourcePurchaseOrderId}

          selectedReceivingReceiptId={selectedReceivingReceiptId}

          selectedBinId={receiveBinId}

          selectedLineId={selectedReceiveLineId}

          lineQuantityReceived={lineQuantityReceived}

          exceptionType={exceptionType}

          exceptionQuantity={exceptionQuantity}

          exceptionNotes={exceptionNotes}

          onReceiptKeyChange={setReceiptKey}

          onSelectedPurchaseOrderIdChange={setReceiveSourcePurchaseOrderId}

          onSelectedReceivingReceiptIdChange={setSelectedReceivingReceiptId}

          onSelectedBinIdChange={setReceiveBinId}

          onSelectedLineIdChange={setSelectedReceiveLineId}

          onLineQuantityReceivedChange={setLineQuantityReceived}

          onExceptionTypeChange={setExceptionType}

          onExceptionQuantityChange={setExceptionQuantity}

          onExceptionNotesChange={setExceptionNotes}

          onCreateFromPurchaseOrder={() => createReceivingReceiptMutation.mutate()}

          onUpdateLineQuantity={() => updateReceivingLineMutation.mutate()}

          onCreateException={() => createReceivingExceptionMutation.mutate()}

          onResolveException={(id) => resolveReceivingExceptionMutation.mutate(id)}

          onPost={() => postReceivingReceiptMutation.mutate()}

          isCreating={createReceivingReceiptMutation.isPending}

          isUpdatingLine={updateReceivingLineMutation.isPending}

          isCreatingException={createReceivingExceptionMutation.isPending}

          isPosting={postReceivingReceiptMutation.isPending}

        />



        <BackordersPanel

          backorders={backordersQuery.data ?? []}

          issuedPurchaseOrders={issuedPurchaseOrders}

          canManage={canReceive}

          isLoading={backordersQuery.isLoading}

          backorderKey={backorderKey}

          selectedBackorderId={selectedBackorderId}

          selectedPurchaseOrderLineId={selectedBackorderPoLineId}

          backorderQuantity={backorderQuantity}

          backorderNotes={backorderNotes}

          cancelReason={backorderCancelReason}

          statusFilter={backorderStatusFilter}

          onBackorderKeyChange={setBackorderKey}

          onSelectedBackorderIdChange={setSelectedBackorderId}

          onSelectedPurchaseOrderLineIdChange={setSelectedBackorderPoLineId}

          onBackorderQuantityChange={setBackorderQuantity}

          onBackorderNotesChange={setBackorderNotes}

          onCancelReasonChange={setBackorderCancelReason}

          onStatusFilterChange={setBackorderStatusFilter}

          onCreateFromPurchaseOrderLine={() => createBackorderMutation.mutate()}

          onFulfill={() => fulfillBackorderMutation.mutate()}

          onCancel={() => cancelBackorderMutation.mutate()}

          isCreating={createBackorderMutation.isPending}

          isFulfilling={fulfillBackorderMutation.isPending}

          isCancelling={cancelBackorderMutation.isPending}

        />



        <ReturnsPanel

          returns={vendorReturnsQuery.data ?? []}

          vendors={vendors}

          parts={partsQuery.data ?? []}

          issuedPurchaseOrders={issuedPurchaseOrdersWithReceived}

          inventoryBins={returnInventoryBins}

          canManage={canReceive}

          isLoading={vendorReturnsQuery.isLoading}

          returnKey={returnKey}

          selectedReturnId={selectedReturnId}

          selectedVendorPartyId={selectedReturnVendorId}

          selectedInventoryBinId={selectedReturnBinId}

          selectedReturnPoLineId={selectedReturnPoLineId}

          selectedReturnPartId={selectedReturnPartId}

          returnQuantity={returnQuantity}

          rmaNumber={rmaNumber}

          returnNotes={returnNotes}

          cancelReason={returnCancelReason}

          statusFilter={returnStatusFilter}

          returnSource={returnSource}

          onReturnKeyChange={setReturnKey}

          onSelectedReturnIdChange={setSelectedReturnId}

          onSelectedVendorPartyIdChange={setSelectedReturnVendorId}

          onSelectedInventoryBinIdChange={setSelectedReturnBinId}

          onSelectedReturnPoLineIdChange={setSelectedReturnPoLineId}

          onSelectedReturnPartIdChange={setSelectedReturnPartId}

          onReturnQuantityChange={setReturnQuantity}

          onRmaNumberChange={setRmaNumber}

          onReturnNotesChange={setReturnNotes}

          onCancelReasonChange={setReturnCancelReason}

          onStatusFilterChange={setReturnStatusFilter}

          onReturnSourceChange={setReturnSource}

          onCreate={() => createReturnMutation.mutate()}

          onPost={() => postReturnMutation.mutate()}

          onCancel={() => cancelReturnMutation.mutate()}

          isCreating={createReturnMutation.isPending}

          isPosting={postReturnMutation.isPending}

          isCancelling={cancelReturnMutation.isPending}

        />



        <PricingLeadTimePanel

          parts={partsQuery.data ?? []}

          pricingSnapshots={pricingSnapshotsQuery.data ?? []}

          leadTimeSnapshots={leadTimeSnapshotsQuery.data ?? []}

          canManage={canManageCatalog}

          isLoading={pricingSnapshotsQuery.isLoading || leadTimeSnapshotsQuery.isLoading}

          pricingSnapshotKey={pricingSnapshotKey}

          leadTimeSnapshotKey={leadTimeSnapshotKey}

          selectedVendorLinkId={selectedSnapshotVendorLinkId}

          unitPrice={snapshotUnitPrice}

          currencyCode={snapshotCurrencyCode}

          minimumOrderQuantity={snapshotMinimumOrderQty}

          leadTimeDays={snapshotLeadTimeDays}

          snapshotNotes={snapshotNotes}

          currentOnlyFilter={snapshotCurrentOnly}

          onPricingSnapshotKeyChange={setPricingSnapshotKey}

          onLeadTimeSnapshotKeyChange={setLeadTimeSnapshotKey}

          onSelectedVendorLinkIdChange={setSelectedSnapshotVendorLinkId}

          onUnitPriceChange={setSnapshotUnitPrice}

          onCurrencyCodeChange={setSnapshotCurrencyCode}

          onMinimumOrderQuantityChange={setSnapshotMinimumOrderQty}

          onLeadTimeDaysChange={setSnapshotLeadTimeDays}

          onSnapshotNotesChange={setSnapshotNotes}

          onCurrentOnlyFilterChange={setSnapshotCurrentOnly}

          onCreatePricingSnapshot={() => createPricingSnapshotMutation.mutate()}

          onCreateLeadTimeSnapshot={() => createLeadTimeSnapshotMutation.mutate()}

          isCreatingPricing={createPricingSnapshotMutation.isPending}

          isCreatingLeadTime={createLeadTimeSnapshotMutation.isPending}

        />



        <AvailabilitySnapshotsPanel

          parts={partsQuery.data ?? []}

          availabilitySnapshots={availabilitySnapshotsQuery.data ?? []}

          canManage={canManageCatalog}

          isLoading={availabilitySnapshotsQuery.isLoading}

          snapshotKey={availabilitySnapshotKey}

          selectedVendorLinkId={selectedAvailabilityVendorLinkId}

          quantityAvailable={availabilityQuantity}

          availabilityStatus={availabilityStatus}

          snapshotNotes={availabilityNotes}

          currentOnlyFilter={availabilityCurrentOnly}

          onSnapshotKeyChange={setAvailabilitySnapshotKey}

          onSelectedVendorLinkIdChange={setSelectedAvailabilityVendorLinkId}

          onQuantityAvailableChange={setAvailabilityQuantity}

          onAvailabilityStatusChange={setAvailabilityStatus}

          onSnapshotNotesChange={setAvailabilityNotes}

          onCurrentOnlyFilterChange={setAvailabilityCurrentOnly}

          onCreateAvailabilitySnapshot={() => createAvailabilitySnapshotMutation.mutate()}

          isCreating={createAvailabilitySnapshotMutation.isPending}

        />



        <ReorderEvaluationPanel

          suggestions={reorderEvaluationQuery.data?.suggestions ?? []}

          parts={partsQuery.data ?? []}

          canManagePolicy={canManageInv}

          canCreatePurchaseRequest={canCreatePr}

          isLoading={reorderEvaluationQuery.isLoading}

          selectedPartId={reorderPolicyPartId}

          reorderPoint={reorderPoint}

          reorderQuantity={reorderQuantity}

          selectedSuggestionPartIds={selectedReorderPartIds}

          prRequestKey={reorderPrRequestKey}

          prTitle={reorderPrTitle}

          prNotes={reorderPrNotes}

          onSelectedPartIdChange={setReorderPolicyPartId}

          onReorderPointChange={setReorderPoint}

          onReorderQuantityChange={setReorderQuantity}

          onSelectedSuggestionPartIdsChange={setSelectedReorderPartIds}

          onPrRequestKeyChange={setReorderPrRequestKey}

          onPrTitleChange={setReorderPrTitle}

          onPrNotesChange={setReorderPrNotes}

          onSavePolicy={() => upsertReorderPolicyMutation.mutate()}

          onRefreshEvaluation={() => reorderEvaluationQuery.refetch()}

          onCreatePurchaseRequest={() => createPurchaseRequestFromReorderMutation.mutate()}

          isSavingPolicy={upsertReorderPolicyMutation.isPending}

          isCreatingPurchaseRequest={createPurchaseRequestFromReorderMutation.isPending}

        />



        <DemandRefsPanel

          demandRefs={demandRefsQuery.data ?? []}

          parts={partsQuery.data ?? []}

          canCreatePurchaseRequest={canCreatePr}

          isLoading={demandRefsQuery.isLoading}

          selectedDemandRefId={selectedDemandRefId}

          prRequestKey={demandPrRequestKey}

          prTitle={demandPrTitle}

          prNotes={demandPrNotes}

          onSelectedDemandRefIdChange={setSelectedDemandRefId}

          onPrRequestKeyChange={setDemandPrRequestKey}

          onPrTitleChange={setDemandPrTitle}

          onPrNotesChange={setDemandPrNotes}

          onCreatePurchaseRequest={() => createPurchaseRequestFromDemandRefMutation.mutate()}

          isCreatingPurchaseRequest={createPurchaseRequestFromDemandRefMutation.isPending}

        />



        <PurchaseOrderPanel

          purchaseOrders={purchaseOrdersQuery.data ?? []}

          approvedPurchaseRequests={approvedPurchaseRequests}

          canCreate={canCreatePo}

          canApprove={canApprovePo}

          isLoading={purchaseOrdersQuery.isLoading}

          orderKey={poOrderKey}

          selectedPurchaseRequestId={poSourcePurchaseRequestId}

          selectedPurchaseOrderId={selectedPurchaseOrderId}

          onOrderKeyChange={setPoOrderKey}

          onSelectedPurchaseRequestIdChange={setPoSourcePurchaseRequestId}

          onSelectedPurchaseOrderIdChange={setSelectedPurchaseOrderId}

          onCreateFromPurchaseRequest={() => createPurchaseOrderMutation.mutate()}

          onApprove={() => approvePurchaseOrderMutation.mutate()}

          onIssue={() => issuePurchaseOrderMutation.mutate()}

          isCreating={createPurchaseOrderMutation.isPending}

          isApproving={approvePurchaseOrderMutation.isPending}

          isIssuing={issuePurchaseOrderMutation.isPending}

        />



        <PurchaseRequestPanel

          purchaseRequests={purchaseRequestsQuery.data ?? []}

          parts={partsQuery.data ?? []}

          vendors={vendors.map((v) => ({

            partyId: v.partyId,

            displayName: v.displayName,

            partyKey: v.partyKey,

          }))}

          canCreate={canCreatePr}

          canApprove={canApprovePr}

          isLoading={purchaseRequestsQuery.isLoading}

          requestKey={prRequestKey}

          title={prTitle}

          notes={prNotes}

          selectedVendorId={prVendorId}

          selectedPartId={prPartId}

          lineQuantity={prLineQty}

          lineNotes={prLineNotes}

          rejectionReason={prRejectionReason}

          selectedPurchaseRequestId={selectedPurchaseRequestId}

          onRequestKeyChange={setPrRequestKey}

          onTitleChange={setPrTitle}

          onNotesChange={setPrNotes}

          onSelectedVendorIdChange={setPrVendorId}

          onSelectedPartIdChange={setPrPartId}

          onLineQuantityChange={setPrLineQty}

          onLineNotesChange={setPrLineNotes}

          onRejectionReasonChange={setPrRejectionReason}

          onSelectedPurchaseRequestIdChange={setSelectedPurchaseRequestId}

          onCreate={() => createPurchaseRequestMutation.mutate()}

          onSubmit={() => submitPurchaseRequestMutation.mutate()}

          onApprove={() => approvePurchaseRequestMutation.mutate()}

          onReject={() => rejectPurchaseRequestMutation.mutate()}

          isCreating={createPurchaseRequestMutation.isPending}

          isSubmitting={submitPurchaseRequestMutation.isPending}

          isApproving={approvePurchaseRequestMutation.isPending}

          isRejecting={rejectPurchaseRequestMutation.isPending}

        />

        <InventoryPanel

          locations={locations}

          bins={binsQuery.data ?? []}

          stockLevels={stockQuery.data ?? []}

          parts={partsQuery.data ?? []}

          canManage={canManageInv}

          isLoading={locationsQuery.isLoading || stockQuery.isLoading}

          locationKey={invLocationKey}

          locationName={invLocationName}

          locationType={invLocationType}

          addressLine={invAddressLine}

          binKey={invBinKey}

          binName={invBinName}

          selectedLocationId={selectedInvLocationId}

          selectedPartId={selectedStockPartId}

          selectedBinId={selectedStockBinId}

          stockQuantity={stockQuantity}

          onLocationKeyChange={setInvLocationKey}

          onLocationNameChange={setInvLocationName}

          onLocationTypeChange={setInvLocationType}

          onAddressLineChange={setInvAddressLine}

          onBinKeyChange={setInvBinKey}

          onBinNameChange={setInvBinName}

          onSelectedLocationIdChange={setSelectedInvLocationId}

          onSelectedPartIdChange={setSelectedStockPartId}

          onSelectedBinIdChange={setSelectedStockBinId}

          onStockQuantityChange={setStockQuantity}

          onCreateLocation={() => createLocationMutation.mutate()}

          onCreateBin={() => createBinMutation.mutate()}

          onUpsertStock={() => upsertStockMutation.mutate()}

          isCreatingLocation={createLocationMutation.isPending}

          isCreatingBin={createBinMutation.isPending}

          isUpsertingStock={upsertStockMutation.isPending}

        />

        <PartCatalogPanel

          catalogs={catalogsQuery.data ?? []}

          parts={partsQuery.data ?? []}

          canManage={canManageCatalog}

          isLoading={catalogsQuery.isLoading || partsQuery.isLoading}

          catalogKey={catalogKey}

          catalogName={catalogName}

          catalogDescription={catalogDescription}

          partKey={partKey}

          partName={partName}

          partCategory={partCategory}

          partUom={partUom}

          partManufacturer={partManufacturer}

          partMfgNumber={partMfgNumber}

          selectedCatalogId={selectedCatalogId}

          vendorPartNumber={vendorPartNumber}

          selectedPartId={selectedPartId}

          selectedVendorId={selectedVendorId}

          vendors={vendors.map((v) => ({

            partyId: v.partyId,

            displayName: v.displayName,

            partyKey: v.partyKey,

          }))}

          onCatalogKeyChange={setCatalogKey}

          onCatalogNameChange={setCatalogName}

          onCatalogDescriptionChange={setCatalogDescription}

          onPartKeyChange={setPartKey}

          onPartNameChange={setPartName}

          onPartCategoryChange={setPartCategory}

          onPartUomChange={setPartUom}

          onPartManufacturerChange={setPartManufacturer}

          onPartMfgNumberChange={setPartMfgNumber}

          onSelectedCatalogIdChange={setSelectedCatalogId}

          onVendorPartNumberChange={setVendorPartNumber}

          onSelectedPartIdChange={setSelectedPartId}

          onSelectedVendorIdChange={setSelectedVendorId}

          onCreateCatalog={() => createCatalogMutation.mutate()}

          onCreatePart={() => createPartMutation.mutate()}

          onLinkVendor={() => linkVendorMutation.mutate()}

          isCreatingCatalog={createCatalogMutation.isPending}

          isCreatingPart={createPartMutation.isPending}

          isLinkingVendor={linkVendorMutation.isPending}

        />

        <PartyRegistryPanel

          title="Vendors"

          parties={vendors}

          canManage={canManage}

          isLoading={vendorsQuery.isLoading}

          partyKey={vendorKey}

          displayName={vendorName}

          legalName={vendorLegalName}

          taxIdentifier={vendorTaxId}

          notes={vendorNotes}

          onPartyKeyChange={setVendorKey}

          onDisplayNameChange={setVendorName}

          onLegalNameChange={setVendorLegalName}

          onTaxIdentifierChange={setVendorTaxId}

          onNotesChange={setVendorNotes}

          onCreate={() => createVendorMutation.mutate()}

          isCreating={createVendorMutation.isPending}

        />

        <PartyRegistryPanel

          title="Suppliers"

          parties={suppliersQuery.data ?? []}

          canManage={false}

          isLoading={suppliersQuery.isLoading}

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

          parties={dealersQuery.data ?? []}

          canManage={false}

          isLoading={dealersQuery.isLoading}

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

    </main>

  )

}

