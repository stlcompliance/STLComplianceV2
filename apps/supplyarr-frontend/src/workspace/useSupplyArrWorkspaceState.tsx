import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'

import { useState } from 'react'
import { Navigate, useSearchParams } from 'react-router-dom'


import {

  createPart,
  createPartCatalog,
  createPartSource,

  createPartSupplierLink,

  createPurchaseRequest,

  createPurchaseOrderFromPurchaseRequest,

  getBackorders,
  getSupplierReturns,
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

  createSupplier,
  createSupplierContact,
  getContractRecords,

  approvePurchaseRequest,

  approvePurchaseOrder,

  cancelPurchaseOrder,

  getMe,

  getPartCatalogs,

  getParts,
  getSubstitutions,

  getPurchaseRequests,

  getPurchaseOrders,

  getSupplierDirectory,
  updateSupplier,
  updateSupplierApprovalStatus,
  updateSupplierStatus,

  issuePurchaseOrder,

  rejectPurchaseRequest,

  submitPurchaseRequest,

} from '../api/client'

import type { UpdateSupplierRequest } from '../api/types'

import {
  canApprovePurchaseOrders,
  canApprovePurchaseRequests,
  canCreatePurchaseOrders,
  canCreatePurchaseRequests,
  canReadSuppliers as userCanReadSuppliers,
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

  const [supplierKey, setSupplierKey] = useState('')
  const [supplierName, setSupplierName] = useState('')
  const [supplierLegalName, setSupplierLegalName] = useState('')
  const [supplierTaxId, setSupplierTaxId] = useState('')
  const [supplierNotes, setSupplierNotes] = useState('')
  const [supplierParentUnitId, setSupplierParentUnitId] = useState('')
  const [supplierUnitKind, setSupplierUnitKind] = useState('identity')
  const [supplierServiceTypes, setSupplierServiceTypes] = useState('products,parts')
  const [supplierAddressLine1, setSupplierAddressLine1] = useState('')
  const [supplierLocality, setSupplierLocality] = useState('')
  const [supplierRegionCode, setSupplierRegionCode] = useState('')
  const [supplierPostalCode, setSupplierPostalCode] = useState('')
  const [supplierCountryCode, setSupplierCountryCode] = useState('US')
  const [catalogKey, setCatalogKey] = useState('')

  const [catalogName, setCatalogName] = useState('')

  const [catalogDescription, setCatalogDescription] = useState('')

  const [partKey, setPartKey] = useState('')

  const [partName, setPartName] = useState('')

  const [partCategory, setPartCategory] = useState('general')

  const [partUom, setPartUom] = useState('each')

  const [partManufacturer, setPartManufacturer] = useState('')

  const [partMfgNumber, setPartMfgNumber] = useState('')

  const [partIsTrackable, setPartIsTrackable] = useState(true)

  const [partIsStocked, setPartIsStocked] = useState(true)

  const [selectedCatalogId, setSelectedCatalogId] = useState('')

  const [selectedPartId, setSelectedPartId] = useState('')

  const [substitutionPartId, setSubstitutionPartId] = useState('')

  const [selectedSourcePartId, setSelectedSourcePartId] = useState('')

  const [partSourceType, setPartSourceType] = useState('unknown')

  const [partSourceLabel, setPartSourceLabel] = useState('')

  const [partSourceNotes, setPartSourceNotes] = useState('')

  const [selectedSupplierUnitId, setSelectedSupplierUnitId] = useState('')

  const [supplierPartNumber, setSupplierPartNumber] = useState('')

  const [prRequestKey, setPrRequestKey] = useState('')

  const [prTitle, setPrTitle] = useState('')

  const [prNotes, setPrNotes] = useState('')

  const [prSupplierUnitId, setPrSupplierUnitId] = useState('')

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

  const [selectedPricingSourceLinkId, setSelectedPricingSourceLinkId] = useState('')

  const [snapshotUnitPrice, setSnapshotUnitPrice] = useState('')

  const [snapshotCurrencyCode, setSnapshotCurrencyCode] = useState('USD')

  const [snapshotMinimumOrderQty, setSnapshotMinimumOrderQty] = useState('')

  const [snapshotLeadTimeDays, setSnapshotLeadTimeDays] = useState('')

  const [snapshotNotes, setSnapshotNotes] = useState('')

  const [snapshotCurrentOnly, setSnapshotCurrentOnly] = useState(true)

  const [availabilitySnapshotKey, setAvailabilitySnapshotKey] = useState('')

  const [selectedAvailabilitySourceLinkId, setSelectedAvailabilitySourceLinkId] = useState('')

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

  const suppliersQuery = useQuery({

    queryKey: ['supplyarr-suppliers', session?.accessToken],

    queryFn: () => getSupplierDirectory(session!.accessToken),

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



  const supplierReturnsQuery = useQuery({

    queryKey: ['supplyarr-returns', session?.accessToken],

    queryFn: () =>

      getSupplierReturns(session!.accessToken, {

        status: undefined,

        supplierId: undefined,

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

  const createSupplierMutation = useMutation({
    mutationFn: () =>
      createSupplier(session!.accessToken, {
        supplierKey,
        parentSupplierId: supplierParentUnitId || null,
        unitKind: supplierUnitKind,
        displayName: supplierName,
        legalName: supplierLegalName,
        taxIdentifier: supplierTaxId || null,
        notes: supplierNotes,
        serviceTypes: supplierServiceTypes
          .split(',')
          .map((value) => value.trim())
          .filter(Boolean),
        addressLine1: supplierAddressLine1 || null,
        locality: supplierLocality || null,
        regionCode: supplierRegionCode || null,
        postalCode: supplierPostalCode || null,
        countryCode: supplierCountryCode || null,
      }),
    onSuccess: async () => {
      setSupplierKey('')
      setSupplierName('')
      setSupplierLegalName('')
      setSupplierTaxId('')
      setSupplierNotes('')
      setSupplierParentUnitId('')
      setSupplierUnitKind('identity')
      setSupplierServiceTypes('products,parts')
      setSupplierAddressLine1('')
      setSupplierLocality('')
      setSupplierRegionCode('')
      setSupplierPostalCode('')
      setSupplierCountryCode('US')
      await queryClient.invalidateQueries({ queryKey: ['supplyarr-suppliers'] })
    },
  })
  const updateSupplierMutation = useMutation({
    mutationFn: ({
      supplierId,
      request,
    }: {
      supplierId: string
      request: UpdateSupplierRequest
    }) => updateSupplier(session!.accessToken, supplierId, request),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['supplyarr-suppliers'] })
    },
  })

  const updateSupplierApprovalMutation = useMutation({
    mutationFn: ({
      supplierId,
      approvalStatus,
    }: {
      supplierId: string
      approvalStatus: string
    }) =>
      updateSupplierApprovalStatus(session!.accessToken, supplierId, {
        approvalStatus,
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['supplyarr-suppliers'] })
    },
  })

  const updateSupplierStatusMutation = useMutation({
    mutationFn: ({
      supplierId,
      status,
    }: {
      supplierId: string
      status: string
    }) => updateSupplierStatus(session!.accessToken, supplierId, { status }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['supplyarr-suppliers'] })
    },
  })

  const addSupplierContactMutation = useMutation({
    mutationFn: ({
      supplierId,
      request,
    }: {
      supplierId: string
      request: {
        contactName: string
        email: string
        phone: string
        roleLabel: string
        isPrimary: boolean
      }
    }) => createSupplierContact(session!.accessToken, supplierId, request),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['supplyarr-suppliers'] })
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

        isTrackable: partIsTrackable,

        isStocked: partIsStocked,

      }),

    onSuccess: async () => {

      setPartKey('')

      setPartName('')

      setPartCategory('general')

      setPartUom('each')

      setPartManufacturer('')

      setPartMfgNumber('')

      setPartIsTrackable(true)

      setPartIsStocked(true)

      await queryClient.invalidateQueries({ queryKey: ['supplyarr-parts'] })

    },

  })

  const createPartSourceMutation = useMutation({

    mutationFn: () =>

      createPartSource(session!.accessToken, selectedSourcePartId, {

        sourceType: partSourceType,

        label: partSourceLabel,

        notes: partSourceNotes,

      }),

    onSuccess: async () => {

      setPartSourceType('unknown')

      setPartSourceLabel('')

      setPartSourceNotes('')

      await queryClient.invalidateQueries({ queryKey: ['supplyarr-parts'] })

    },

  })



  const linkSupplierSourceMutation = useMutation({

    mutationFn: () =>

      createPartSupplierLink(session!.accessToken, selectedPartId, {
        supplierUnitId: selectedSupplierUnitId,
        supplierPartNumber: supplierPartNumber,

        isPreferred: true,

      }),

    onSuccess: async () => {

      setSupplierPartNumber('')

      await queryClient.invalidateQueries({ queryKey: ['supplyarr-parts'] })

    },

  })



  const createPurchaseRequestMutation = useMutation({

    mutationFn: () =>

      createPurchaseRequest(session!.accessToken, {

        requestKey: prRequestKey,

        title: prTitle,

        notes: prNotes,

        supplierUnitId: prSupplierUnitId || null,

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

      setPrSupplierUnitId('')

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

        partSupplierLinkId: selectedPricingSourceLinkId,
        

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

        partSupplierLinkId: selectedPricingSourceLinkId,

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

        partSupplierLinkId: selectedAvailabilitySourceLinkId,

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

  const canReadSuppliers = me
    ? userCanReadSuppliers(me.tenantRoleKey, me.isPlatformAdmin)
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

    purchaseRequestsQuery.data?.filter((pr) => pr.status === 'approved' && pr.supplierId) ?? []

  const issuedPurchaseOrders =

    purchaseOrdersQuery.data?.filter(

      (po) => po.status === 'issued' && po.lines.some((l) => l.quantityRemaining > 0),

    ) ?? []
  const supplierDirectory = suppliersQuery.data ?? []

  return {
    handoffRedirect,
    ready: Boolean(session && meQuery.data),
    loadingMessage: 'Loading supply workspace…',
    me: meQuery.data!,
    session: session!,
    accessToken,
    apiError,
    supplierKey,
    supplierName,
    supplierLegalName,
    supplierTaxId,
    supplierNotes,
    supplierParentUnitId,
    supplierUnitKind,
    supplierServiceTypes,
    supplierAddressLine1,
    supplierLocality,
    supplierRegionCode,
    supplierPostalCode,
    supplierCountryCode,
    catalogKey,
    catalogName,
    catalogDescription,
    partKey,
    partName,
    partCategory,
    partUom,
    partManufacturer,
    partMfgNumber,
    partIsTrackable,
    partIsStocked,
    selectedCatalogId,
    selectedPartId,
    substitutionPartId,
    selectedSourcePartId,
    partSourceType,
    partSourceLabel,
    partSourceNotes,
    selectedSupplierUnitId,
    supplierPartNumber,
    prRequestKey,
    prTitle,
    prNotes,
    prSupplierUnitId,
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
    selectedPricingSourceLinkId,
    snapshotUnitPrice,
    snapshotCurrencyCode,
    snapshotMinimumOrderQty,
    snapshotLeadTimeDays,
    snapshotNotes,
    snapshotCurrentOnly,
    availabilitySnapshotKey,
    selectedAvailabilitySourceLinkId,
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
    suppliersQuery,
    supplierDirectory,
    catalogsQuery,
    partsQuery,
    substitutionsQuery,
    purchaseRequestsQuery,
    purchaseOrdersQuery,
    backordersQuery,
    supplierReturnsQuery,
    pricingSnapshotsQuery,
    leadTimeSnapshotsQuery,
    availabilitySnapshotsQuery,
    reorderEvaluationQuery,
    contractsQuery,
    demandRefsQuery,
    createSupplierMutation,
    updateSupplierMutation,
    updateSupplierApprovalMutation,
    updateSupplierStatusMutation,
    addSupplierContactMutation,
    createCatalogMutation,
    createPartMutation,
    createPartSourceMutation,
    linkSupplierSourceMutation,
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
    canReadSuppliers,
    canReadPartSubstitutions,
    canUseForgivingSearch,
    canReadAuditHistory,
    canReadSupplyReadiness,
    approvedPurchaseRequests,
    issuedPurchaseOrders,
    setSupplierKey,
    setSupplierName,
    setSupplierLegalName,
    setSupplierTaxId,
    setSupplierNotes,
    setSupplierParentUnitId,
    setSupplierUnitKind,
    setSupplierServiceTypes,
    setSupplierAddressLine1,
    setSupplierLocality,
    setSupplierRegionCode,
    setSupplierPostalCode,
    setSupplierCountryCode,
    setCatalogKey,
    setCatalogName,
    setCatalogDescription,
    setPartKey,
    setPartName,
    setPartCategory,
    setPartUom,
    setPartManufacturer,
    setPartMfgNumber,
    setPartIsTrackable,
    setPartIsStocked,
    setSelectedCatalogId,
    setSelectedPartId,
    setSubstitutionPartId,
    setSelectedSourcePartId,
    setPartSourceType,
    setPartSourceLabel,
    setPartSourceNotes,
    setSelectedSupplierUnitId,
    setSupplierPartNumber,
    setPrRequestKey,
    setPrTitle,
    setPrNotes,
    setPrSupplierUnitId,
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
    setSelectedPricingSourceLinkId,
    setSnapshotUnitPrice,
    setSnapshotCurrencyCode,
    setSnapshotMinimumOrderQty,
    setSnapshotLeadTimeDays,
    setSnapshotNotes,
    setSnapshotCurrentOnly,
    setAvailabilitySnapshotKey,
    setSelectedAvailabilitySourceLinkId,
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
