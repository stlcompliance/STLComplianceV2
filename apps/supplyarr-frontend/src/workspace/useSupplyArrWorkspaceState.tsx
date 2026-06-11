import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'

import { useState } from 'react'
import { Navigate, useSearchParams } from 'react-router-dom'


import {

  createPart,
  createPartCatalog,

  createPartVendorLink,

  createPurchaseRequest,

  createPurchaseOrderFromPurchaseRequest,

  getBackorders,
  getVendorReturns,
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

  getMe,

  getPartCatalogs,

  getParts,
  getSubstitutions,

  getPurchaseRequests,

  getPurchaseOrders,

  getSuppliers,

  issuePurchaseOrder,

  getVendors,

  rejectPurchaseRequest,

  submitPurchaseRequest,

} from '../api/client'

import type { UpdateExternalPartyRequest } from '../api/types'

import {
  canApprovePurchaseOrders,
  canApprovePurchaseRequests,
  canCreatePurchaseOrders,
  canCreatePurchaseRequests,
  canReadParties as userCanReadParties,
  canReadPartSubstitutions as userCanReadPartSubstitutions,
  canReadProcurementRecords as userCanReadProcurementRecords,
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



  const backordersQuery = useQuery({

    queryKey: ['supplyarr-backorders', session?.accessToken],

    queryFn: () =>

      getBackorders(session!.accessToken, {

        status: undefined,

      }),

    enabled: Boolean(session?.accessToken) && meQuery.isSuccess,

  })



  const vendorReturnsQuery = useQuery({

    queryKey: ['supplyarr-returns', session?.accessToken],

    queryFn: () =>

      getVendorReturns(session!.accessToken, {

        status: undefined,

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

  const canManageNotifications = me
    ? canManageNotificationSettings(me.tenantRoleKey, me.isPlatformAdmin)
    : false

  const canReadParties = me
    ? userCanReadParties(me.tenantRoleKey, me.isPlatformAdmin)
    : false

  const canReadPartSubstitutions = me
    ? userCanReadPartSubstitutions(me.tenantRoleKey, me.isPlatformAdmin)
    : false

  const substitutionsQuery = useQuery({
    queryKey: ['supplyarr-substitutions', session?.accessToken, substitutionPartId],
    queryFn: () => getSubstitutions(session!.accessToken, substitutionPartId || undefined),
    enabled: Boolean(session?.accessToken) && meQuery.isSuccess && canReadPartSubstitutions,
  })

  const canReadProcurementRecords = me
    ? userCanReadProcurementRecords(me.tenantRoleKey, me.isPlatformAdmin)
    : false

  const contractsQuery = useQuery({

    queryKey: ['supplyarr-contract-records', session?.accessToken],

    queryFn: () => getContractRecords(session!.accessToken, { limit: 100 }),

    enabled: Boolean(session?.accessToken) && meQuery.isSuccess && canReadProcurementRecords,

  })

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

  const vendors = vendorsQuery.data ?? []

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
    purchaseRequestsQuery,
    purchaseOrdersQuery,
    backordersQuery,
    vendorReturnsQuery,
    pricingSnapshotsQuery,
    leadTimeSnapshotsQuery,
    availabilitySnapshotsQuery,
    reorderEvaluationQuery,
    contractsQuery,
    demandRefsQuery,
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
    createPurchaseRequestMutation,
    submitPurchaseRequestMutation,
    approvePurchaseRequestMutation,
    rejectPurchaseRequestMutation,
    createPurchaseOrderMutation,
    approvePurchaseOrderMutation,
    issuePurchaseOrderMutation,
    cancelPurchaseOrderMutation,
    createPricingSnapshotMutation,
    createLeadTimeSnapshotMutation,
    createAvailabilitySnapshotMutation,
    upsertReorderPolicyMutation,
    createPurchaseRequestFromReorderMutation,
    createPurchaseRequestFromDemandRefMutation,
    canManage,
    canManageCatalog,
    canManageInv,
    canCreatePr,
    canApprovePr,
    canCreateEmergencyPurchase: canCreateEmergencyPurchaseFlag,
    canManagerOverrideEmergencyPurchase: canManagerOverrideEmergencyPurchaseFlag,
    canCreatePo,
    canApprovePo,
    canManageNotifications,
    canReadParties,
    canReadPartSubstitutions,
    canUseForgivingSearch,
    canReadAuditHistory,
    canReadSupplyReadiness,
    approvedPurchaseRequests,
    issuedPurchaseOrders,
    vendors,
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
