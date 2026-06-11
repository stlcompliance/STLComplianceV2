import { useEffect, useMemo, useState } from 'react'
import { useQuery } from '@tanstack/react-query'
import {
  Activity,
  AlertTriangle,
  Boxes,
  CheckCircle2,
  BarChart3,
  ClipboardCheck,
  ClipboardList,
  DatabaseZap,
  FileCheck2,
  MapPin,
  PackageCheck,
  PackagePlus,
  Route,
  Search,
  ShieldCheck,
  Truck,
  Warehouse,
} from 'lucide-react'
import { generatePath, matchPath, useLocation, useNavigate } from 'react-router-dom'
import {
  ControlledSelect,
  FormField,
  ProductWorkspaceFrame,
  buildProductLaunchUrlMap,
  formatProductLaunchError,
  getLaunchCatalog,
  resolveProductWorkspaceBootstrapError,
  resolveSuiteHomeUrl,
  useProductWorkspaceLaunch,
  type PickerOption,
  type ProductNavItem,
} from '@stl/shared-ui'
import { getLoadArrPermissionCatalog, getSessionBootstrap, loadArrFetch } from './api/client'
import { clearSession, loadSession } from './auth/sessionStorage'
import { ReportsPanel } from './components/ReportsPanel'

type LoadArrMetrics = {
  activeLocations: number
  quantityOnHand: number
  quantityCommitted: number
  quantityBlocked: number
  openTasks: number
  openHolds: number
  unexplainedInventory: number
}

type LoadArrLocation = {
  id: string
  staffarrSiteNameSnapshot: string
  staffarrSiteOrgUnitId: string
  name: string
  locationType: string
  path: string
  active: boolean
  complianceRestrictions: string[]
  capacityPercent: number
  notes: string
}

type LoadArrInventoryBalance = {
  id: string
  supplyarrItemId: string
  itemNameSnapshot: string
  unitOfMeasureSnapshot: string
  state: string
  locationId: string
  locationNameSnapshot: string
  quantityOnHand: number
  quantityReserved: number
  quantityAllocated: number
  quantityBlocked: number
  originEventType: string
  originReference: string
  traceTags: string[]
  notes: string
}

type LoadArrTask = {
  id: string
  taskType: string
  title: string
  priority: string
  status: string
  locationNameSnapshot: string
  assignedRole: string
  supplyarrItemId: string
  quantity: number
  dueAtUtc: string
  requiredSignals: string[]
}

type LoadArrHold = {
  id: string
  holdType: string
  supplyarrItemId: string
  locationNameSnapshot: string
  status: string
  reason: string
  sourceReference: string
  openedAtUtc: string
}

type LoadArrRouteHandoff = {
  id: string
  targetProduct: string
  targetReference: string
  locationNameSnapshot: string
  status: string
  quantity: number
  notes: string
}

type LoadArrExpectedReceipt = {
  id: string
  expectedReceiptNumber: string
  sourceProductKey: string
  sourceObjectRef: string
  supplierNameSnapshot: string
  warehouseLocationId: string
  locationNameSnapshot: string
  supplyarrItemId: string
  itemNameSnapshot: string
  expectedQuantity: number
  receivedQuantity: number
  status: string
  dueAtUtc: string
}

type LoadArrReservation = {
  id: string
  reservationNumber: string
  demandReference: string
  supplyarrItemId: string
  itemNameSnapshot: string
  locationNameSnapshot: string
  quantity: number
  status: string
  reservedAtUtc: string
}

type LoadArrWorkflowRecord = {
  id: string
  recordNumber: string
  subject: string
  status: string
  locationNameSnapshot: string
  quantity: number
  reasonCode: string
  updatedAtUtc: string
  notes: string
}

type LoadArrDiscrepancyRecord = {
  id: string
  discrepancyNumber: string
  sourceType: string
  status: string
  locationNameSnapshot: string
  itemNameSnapshot: string
  quantity: number
  reasonCode: string
  openedAtUtc: string
  notes: string
}

type LoadArrLedgerEntry = {
  id: string
  movementType: string
  sourceType: string
  status: string
  locationNameSnapshot: string
  itemNameSnapshot: string
  quantity: number
  unitOfMeasure: string
  occurredAtUtc: string
  reasonCode: string
}

type LoadArrBalanceRollup = {
  supplyarrItemId: string
  itemNameSnapshot: string
  unitOfMeasureSnapshot: string
  quantityOnHand: number
  quantityReserved: number
  quantityAllocated: number
  quantityBlocked: number
  locationCount: number
  locations: string[]
  activeStates: string[]
}

type LoadArrTruckStock = {
  id: string
  truckStockNumber: string
  staffarrSiteOrgUnitId: string
  staffarrSiteNameSnapshot: string
  truckLocationId: string
  truckLocationNameSnapshot: string
  supplyarrItemId: string
  itemNameSnapshot: string
  unitOfMeasure: string
  assignedPersonId: string
  assignedPersonNameSnapshot: string
  quantityOnHand: number
  minimumQuantity: number
  maximumQuantity: number
  status: string
  lastCountedAtUtc: string
  lastMovementAtUtc: string
  notes: string
  traceTags: string[]
}

type LoadArrTruckStockMutation = {
  truckStock: LoadArrTruckStock
  movement: {
    id: string
    movementType: string
    reasonCode: string
  } | null
  restockTask: LoadArrTask | null
}

type LoadArrKit = {
  id: string
  kitNumber: string
  staffarrSiteOrgUnitId: string
  staffarrSiteNameSnapshot: string
  locationId: string
  locationNameSnapshot: string
  primaryItemId: string
  kitNameSnapshot: string
  unitOfMeasure: string
  assignedPersonId: string
  assignedPersonNameSnapshot: string
  quantityOnHand: number
  minimumQuantity: number
  maximumQuantity: number
  status: string
  lastActionAtUtc: string
  lastMovementAtUtc: string
  notes: string
  traceTags: string[]
}

type LoadArrKitMutation = {
  kit: LoadArrKit
  movement: {
    id: string
    movementType: string
    reasonCode: string
  } | null
  followUpTask: LoadArrTask | null
}

type LoadArrEvidence = {
  id: string
  evidenceType: string
  locationNameSnapshot: string
  summary: string
  capturedAtUtc: string
  capturedBy: string
}

type LoadArrUnexplainedInventoryRecord = {
  id: string
  recordNumber: string
  status: string
  discoverySource: string
  staffarrSiteOrgUnitId: string
  staffarrSiteNameSnapshot: string
  warehouseLocationId: string
  locationNameSnapshot: string
  supplyarrItemId: string
  itemNameSnapshot: string
  expectedQuantity: number
  quantity: number
  varianceQuantity: number
  unitOfMeasure: string
  lotCode: string | null
  serialCode: string | null
  discoveredByPersonId: string
  reasonCode: string
  evidenceSummary: string
  complianceEvaluationId: string | null
  resolutionState: string
  discoveredAtUtc: string
  resolvedAtUtc: string | null
}

type SupplyArrItemReference = {
  supplyarrItemId: string
  itemNumberSnapshot: string
  itemNameSnapshot: string
  unitOfMeasureSnapshot: string
  itemTypeSnapshot: string
  isLotControlled: boolean
  isSerialControlled: boolean
  isHazardous: boolean
  requiresSds: boolean
  updatedAtUtc: string
}

type LoadArrLocationUtilization = {
  id: string
  name: string
  staffarrSiteOrgUnitId: string
  staffarrSiteNameSnapshot: string
  locationType: string
  active: boolean
  capacityPercent: number
  quantityOnHand: number
  quantityBlocked: number
  openTasks: number
  openHolds: number
  unexplainedInventory: number
  itemCount: number
  inventoryStates: string[]
  signals: string[]
  notes: string
  lastActivityAtUtc: string
}

type LoadArrCount = {
  id: string
  countNumber: string
  status: string
  countType: string
  staffarrSiteOrgUnitId: string
  staffarrSiteNameSnapshot: string
  warehouseLocationId: string
  locationNameSnapshot: string
  supplyarrItemId: string
  itemNameSnapshot: string
  expectedQuantity: number
  countedQuantity: number
  varianceQuantity: number
  unitOfMeasure: string
  countedByPersonId: string
  approvedByPersonId: string | null
  reasonCode: string
  inventoryAdjustmentId: string | null
  evidenceSummary: string
  createdAtUtc: string
  completedAtUtc: string | null
  approvedAtUtc: string | null
  updatedAtUtc: string
}

type LoadArrCountCompletion = {
  count: LoadArrCount
  adjustment: LoadArrAdjustment | null
  originEvent: {
    id: string
    originType: string
    supplyarrItemId: string
    quantity: number
    unitOfMeasure: string
    locationNameSnapshot: string
  } | null
  movement: {
    id: string
    movementType: string
    reasonCode: string
  } | null
}

type LoadArrAdjustment = {
  id: string
  adjustmentNumber: string
  status: string
  adjustmentType: string
  staffarrSiteOrgUnitId: string
  staffarrSiteNameSnapshot: string
  warehouseLocationId: string
  locationNameSnapshot: string
  supplyarrItemId: string
  itemNameSnapshot: string
  quantityDelta: number
  unitOfMeasure: string
  reasonCode: string
  createdByPersonId: string
  approvedByPersonId: string | null
  inventoryOriginEventId: string | null
  evidenceSummary: string
  createdAtUtc: string
  approvedAtUtc: string | null
  updatedAtUtc: string
}

type LoadArrAdjustmentMutation = {
  adjustment: LoadArrAdjustment
  originEvent: {
    id: string
    originType: string
    supplyarrItemId: string
    quantity: number
    unitOfMeasure: string
    locationNameSnapshot: string
  } | null
  movement: {
    id: string
    movementType: string
    reasonCode: string
  } | null
}

type LoadArrWorkspaceSummary = {
  generatedAt: string
  metrics: LoadArrMetrics
  locations: LoadArrLocation[]
  inventory: LoadArrInventoryBalance[]
  tasks: LoadArrTask[]
  holds: LoadArrHold[]
  routeHandoffs: LoadArrRouteHandoff[]
  evidence: LoadArrEvidence[]
  unexplainedInventory: LoadArrUnexplainedInventoryRecord[]
}

type LoadArrReceivingCompletion = {
  session: {
    receivingNumber: string
    status: string
  }
  originEvent: {
    id: string
    originType: string
    supplyarrItemId: string
    quantity: number
    unitOfMeasure: string
    locationNameSnapshot: string
  }
  movement: {
    id: string
    movementType: string
    reasonCode: string
  }
  balance: LoadArrInventoryBalance
  putawayTask: LoadArrTask
}

type LoadArrTransferCompletion = {
  transfer: {
    transferNumber: string
    status: string
  }
  movement: {
    id: string
    movementType: string
    reasonCode: string
  }
  sourceBalance: LoadArrInventoryBalance
  destinationBalance: LoadArrInventoryBalance
  transferTask: LoadArrTask
}

type LoadArrReceivingSession = {
  id: string
  receivingNumber: string
  receivingType: string
  status: string
  staffarrSiteOrgUnitId: string
  staffarrSiteNameSnapshot: string
  sourceProductKey: string
  sourceObjectType: string
  sourceObjectId: string
  supplierNameSnapshot: string
  startedByPersonId: string
  completedByPersonId: string | null
  startedAtUtc: string
  completedAtUtc: string | null
  lines: LoadArrReceivingLine[]
}

type LoadArrReceivingLine = {
  id: string
  supplyarrItemId: string
  itemNameSnapshot: string
  expectedQuantity: number
  receivedQuantity: number
  unitOfMeasure: string
  warehouseLocationId: string
  locationNameSnapshot: string
  lotCode: string | null
  serialCode: string | null
  condition: string
  status: string
  discrepancyReasonCode: string | null
  evidenceSummary: string | null
}

type LoadArrTransferOrder = {
  id: string
  transferNumber: string
  status: string
  transferType: string
  staffarrSiteOrgUnitId: string
  staffarrSiteNameSnapshot: string
  fromLocationId: string
  fromLocationNameSnapshot: string
  toLocationId: string
  toLocationNameSnapshot: string
  requestedByPersonId: string
  completedByPersonId: string | null
  reasonCode: string
  createdAtUtc: string
  completedAtUtc: string | null
  lines: LoadArrTransferLine[]
}

type LoadArrTransferLine = {
  id: string
  supplyarrItemId: string
  itemNameSnapshot: string
  quantity: number
  unitOfMeasure: string
  lotCode: string | null
  serialCode: string | null
  status: string
}

type LoadArrHoldMutation = {
  hold: {
    holdNumber: string
    status: string
    reasonCode: string
    quantity: number
    unitOfMeasure: string
    locationNameSnapshot: string
  }
  movement: {
    id: string
    movementType: string
    reasonCode: string
  }
  balance: LoadArrInventoryBalance
}

type LoadArrUnexplainedInventoryMutation = {
  record: LoadArrUnexplainedInventoryRecord
  originEvent: {
    id: string
    originType: string
    supplyarrItemId: string
    quantity: number
    unitOfMeasure: string
    locationNameSnapshot: string
  } | null
  movement: {
    id: string
    movementType: string
    reasonCode: string
  } | null
  reviewTask: LoadArrTask | null
}

type ReceivingFormState = {
  receivingType: string
  sourceProductKey: string
  sourceObjectType: string
  sourceObjectId: string
  supplierNameSnapshot: string
  completedByPersonId: string
  supplyarrItemId: string
  expectedQuantity: string
  receivedQuantity: string
  warehouseLocationId: string
  lotCode: string
  serialCode: string
  condition: string
  discrepancyReasonCode: string
  complianceEvaluationId: string
  evidenceSummary: string
}

type TransferFormState = {
  transferType: string
  fromLocationId: string
  toLocationId: string
  completedByPersonId: string
  supplyarrItemId: string
  quantity: string
  lotCode: string
  serialCode: string
  reasonCode: string
  complianceEvaluationId: string
  evidenceSummary: string
}

type HoldFormState = {
  holdType: string
  warehouseLocationId: string
  supplyarrItemId: string
  quantity: string
  reasonCode: string
  description: string
  createdByPersonId: string
  complianceEvaluationId: string
  evidenceSummary: string
}

type HoldReleaseFormState = {
  holdId: string
  reasonCode: string
  releasedByPersonId: string
  evidenceSummary: string
}

type UnexplainedInventoryFormState = {
  discoverySource: string
  warehouseLocationId: string
  supplyarrItemId: string
  expectedQuantity: string
  quantity: string
  lotCode: string
  serialCode: string
  discoveredByPersonId: string
  reasonCode: string
  evidenceSummary: string
  complianceEvaluationId: string
}

type UnexplainedInventoryResolutionFormState = {
  recordId: string
  reasonCode: string
  personId: string
  quarantineLocationId: string
  complianceEvaluationId: string
  evidenceSummary: string
}

type CountFormState = {
  countType: string
  warehouseLocationId: string
  supplyarrItemId: string
  expectedQuantity: string
  countedQuantity: string
  countedByPersonId: string
  reasonCode: string
  evidenceSummary: string
}

type AdjustmentFormState = {
  adjustmentType: string
  warehouseLocationId: string
  supplyarrItemId: string
  quantityDelta: string
  createdByPersonId: string
  reasonCode: string
  evidenceSummary: string
  approvedByPersonId: string
}

type TruckStockFormState = {
  truckStockId: string
  quantity: string
  personId: string
  reasonCode: string
  evidenceSummary: string
}

type KitFormState = {
  kitId: string
  operation: string
  quantity: string
  personId: string
  reasonCode: string
  evidenceSummary: string
  targetPersonId: string
  targetLocationId: string
}

type KitOperation = 'build' | 'reserve' | 'pick' | 'break' | 'replenish' | 'inspect' | 'assign' | 'return' | 'expire-components' | 'track-location'

type ViewKey =
  | 'dashboard'
  | 'inventory'
  | 'balances'
  | 'expected-receipts'
  | 'receiving'
  | 'putaway'
  | 'reservations'
  | 'picks'
  | 'issues'
  | 'returns'
  | 'transfers'
  | 'truckstock'
  | 'kits'
  | 'locations'
  | 'counts'
  | 'adjustments'
  | 'ledger'
  | 'discrepancies'
  | 'replenishment'
  | 'history'
  | 'tasks'
  | 'holds'
  | 'unexplained'
  | 'handoffs'
  | 'permissions'
  | 'settings'

type LoadArrRouteKind = 'section' | 'create' | 'detail' | 'filter'

type LoadArrRouteRegistration = {
  key: ViewKey
  path: string
  kind?: LoadArrRouteKind
  aliases?: string[]
}

type LoadArrRouteMatch = LoadArrRouteRegistration & {
  canonicalPath: string
  params: Record<string, string>
}

type NavIcon = NonNullable<ProductNavItem['icon']>

const loadarrRoutes: LoadArrRouteRegistration[] = [
  { key: 'dashboard', path: '/work/dashboard', aliases: ['/dashboard', '/work'] },
  { key: 'inventory', path: '/work/inventory', aliases: ['/inventory'] },
  { key: 'inventory', path: '/work/inventory/items/:itemRefId', kind: 'detail', aliases: ['/inventory/items/:itemRefId'] },
  {
    key: 'locations',
    path: '/work/inventory/locations/:locationProfileId',
    kind: 'detail',
    aliases: ['/inventory/locations/:locationProfileId', '/setup/warehouses-areas/:locationProfileId'],
  },
  {
    key: 'balances',
    path: '/work/balances',
    aliases: ['/balances', '/setup/item-references'],
  },
  { key: 'balances', path: '/setup/item-references' },
  {
    key: 'expected-receipts',
    path: '/work/expected-receipts',
    aliases: ['/expected-receipts', '/supply/purchase-order-receipts'],
  },
  {
    key: 'expected-receipts',
    path: '/work/expected-receipts/:expectedReceiptId',
    kind: 'detail',
    aliases: ['/expected-receipts/:expectedReceiptId'],
  },
  { key: 'expected-receipts', path: '/supply/purchase-order-receipts' },
  { key: 'receiving', path: '/work/receiving', aliases: ['/receiving'] },
  { key: 'receiving', path: '/work/receiving/new', kind: 'create', aliases: ['/receiving/new'] },
  {
    key: 'receiving',
    path: '/work/receiving/:receivingSessionId',
    kind: 'detail',
    aliases: ['/receiving/:receivingSessionId'],
  },
  { key: 'tasks', path: '/work/dock-schedule', aliases: ['/tasks'] },
  { key: 'putaway', path: '/work/putaway', aliases: ['/putaway'] },
  { key: 'putaway', path: '/work/putaway/:putawayTaskId', kind: 'detail', aliases: ['/putaway/:putawayTaskId'] },
  { key: 'reservations', path: '/work/reservations', aliases: ['/reservations'] },
  { key: 'reservations', path: '/work/reservations/new', kind: 'create', aliases: ['/reservations/new'] },
  {
    key: 'reservations',
    path: '/work/reservations/:reservationId',
    kind: 'detail',
    aliases: ['/reservations/:reservationId'],
  },
  { key: 'picks', path: '/work/picking', aliases: ['/picking', '/picks'] },
  { key: 'picks', path: '/work/picking/:pickTaskId', kind: 'detail', aliases: ['/picking/:pickTaskId'] },
  { key: 'transfers', path: '/work/transfers', aliases: ['/transfers'] },
  { key: 'transfers', path: '/work/transfers/new', kind: 'create', aliases: ['/transfers/new'] },
  { key: 'transfers', path: '/work/transfers/:transferId', kind: 'detail', aliases: ['/transfers/:transferId'] },
  { key: 'holds', path: '/work/holds', aliases: ['/holds'] },
  { key: 'holds', path: '/work/holds/:holdId', kind: 'detail', aliases: ['/holds/:holdId'] },
  { key: 'truckstock', path: '/work/staging', aliases: ['/staging', '/truck-stock'] },
  {
    key: 'truckstock',
    path: '/work/staging/:stagingAssignmentId',
    kind: 'detail',
    aliases: ['/staging/:stagingAssignmentId', '/truck-stock/:stagingAssignmentId'],
  },
  { key: 'handoffs', path: '/work/shipping', aliases: ['/shipping'] },
  { key: 'handoffs', path: '/work/shipping/:loadoutId', kind: 'detail', aliases: ['/shipping/:loadoutId'] },
  {
    key: 'counts',
    path: '/work/cycle-counts',
    aliases: ['/cycle-counts', '/records/count-history', '/counts'],
  },
  { key: 'counts', path: '/work/cycle-counts/new', kind: 'create', aliases: ['/cycle-counts/new', '/counts/new'] },
  {
    key: 'counts',
    path: '/work/cycle-counts/:countSessionId',
    kind: 'detail',
    aliases: ['/cycle-counts/:countSessionId', '/records/count-history/:countSessionId', '/counts/:countSessionId'],
  },
  { key: 'discrepancies', path: '/work/exceptions', aliases: ['/exceptions', '/work/issues'] },
  { key: 'discrepancies', path: '/work/exceptions/receiving', kind: 'filter', aliases: ['/exceptions/receiving'] },
  {
    key: 'discrepancies',
    path: '/work/exceptions/inventory-holds',
    kind: 'filter',
    aliases: ['/exceptions/inventory-holds'],
  },
  { key: 'discrepancies', path: '/work/exceptions/quarantine', kind: 'filter', aliases: ['/exceptions/quarantine'] },
  {
    key: 'discrepancies',
    path: '/work/exceptions/pending-quality-review',
    kind: 'filter',
    aliases: ['/exceptions/pending-quality-review'],
  },
  { key: 'discrepancies', path: '/work/exceptions/:exceptionId', kind: 'detail', aliases: ['/exceptions/:exceptionId'] },
  { key: 'returns', path: '/supply/vendor-returns', aliases: ['/returns', '/work/returns'] },
  { key: 'returns', path: '/work/returns' },
  {
    key: 'returns',
    path: '/supply/vendor-returns/:returnId',
    kind: 'detail',
    aliases: ['/returns/:returnId', '/work/returns/:returnId'],
  },
  { key: 'issues', path: '/supply/backorders', aliases: ['/issues', '/work/backorders'] },
  {
    key: 'issues',
    path: '/work/backorders/:issueId',
    kind: 'detail',
    aliases: ['/issues/:issueId'],
  },
  { key: 'issues', path: '/work/backorders' },
  { key: 'issues', path: '/supply/backorders' },
  { key: 'replenishment', path: '/supply', aliases: ['/supply/reorder-signals', '/replenishment'] },
  { key: 'replenishment', path: '/supply/reorder-signals' },
  { key: 'locations', path: '/setup/warehouses-areas', aliases: ['/locations', '/setup'] },
  { key: 'locations', path: '/setup', aliases: ['/setup/warehouses-areas'] },
  { key: 'adjustments', path: '/records/adjustment-history', aliases: ['/adjustments'] },
  {
    key: 'adjustments',
    path: '/records/adjustment-history/:adjustmentId',
    kind: 'detail',
    aliases: ['/adjustments/:adjustmentId'],
  },
  { key: 'ledger', path: '/records/stock-ledger', aliases: ['/ledger'] },
  {
    key: 'history',
    path: '/records',
    aliases: ['/history', '/records/receiving-history', '/records/movement-history'],
  },
  { key: 'history', path: '/records/receiving-history' },
  { key: 'history', path: '/records/movement-history' },
  {
    key: 'settings',
    path: '/admin/settings',
    aliases: ['/settings', '/admin', '/setup/location-rules', '/setup/inventory-policies', '/setup/devices-labels'],
  },
  { key: 'settings', path: '/setup/location-rules' },
  { key: 'settings', path: '/setup/inventory-policies' },
  { key: 'settings', path: '/setup/devices-labels' },
  { key: 'permissions', path: '/admin/permissions' },
  { key: 'handoffs', path: '/admin/integrations', aliases: ['/handoffs'] },
  { key: 'unexplained', path: '/work/unexplained', aliases: ['/unexplained'] },
  { key: 'unexplained', path: '/work/unexplained/:recordId', kind: 'detail', aliases: ['/unexplained/:recordId'] },
  { key: 'kits', path: '/work/kits', aliases: ['/kits'] },
  { key: 'kits', path: '/work/kits/:kitId', kind: 'detail', aliases: ['/kits/:kitId'] },
]

const productNavItems: ProductNavItem[] = [
  {
    label: 'Work',
    to: '/work',
    icon: Warehouse as NavIcon,
    children: [
      { label: 'Dashboard', to: '/work/dashboard', icon: BarChart3 as NavIcon },
      { label: 'Expected Receipts', to: '/work/expected-receipts', icon: PackagePlus as NavIcon },
      { label: 'Receiving', to: '/work/receiving', icon: PackagePlus as NavIcon },
      { label: 'Dock Schedule', to: '/work/dock-schedule', icon: ClipboardList as NavIcon },
      { label: 'Putaway', to: '/work/putaway', icon: PackagePlus as NavIcon },
      { label: 'Inventory', to: '/work/inventory', icon: Boxes as NavIcon },
      { label: 'Transfers', to: '/work/transfers', icon: Route as NavIcon },
      { label: 'Reservations', to: '/work/reservations', icon: ClipboardCheck as NavIcon },
      { label: 'Picking', to: '/work/picking', icon: ClipboardList as NavIcon },
      { label: 'Staging', to: '/work/staging', icon: Truck as NavIcon },
      { label: 'Shipping / Loadout', to: '/work/shipping', icon: Route as NavIcon },
      { label: 'Cycle Counts', to: '/work/cycle-counts', icon: Activity as NavIcon },
      { label: 'Exceptions', to: '/work/exceptions', icon: AlertTriangle as NavIcon },
      { label: 'Holds', to: '/work/holds', icon: ShieldCheck as NavIcon },
      { label: 'Unexplained', to: '/work/unexplained', icon: AlertTriangle as NavIcon },
    ],
  },
  {
    label: 'Supply Coordination',
    to: '/supply',
    icon: PackagePlus as NavIcon,
    sectionBreakBefore: true,
    children: [
      { label: 'Purchase Order Receipts', to: '/supply/purchase-order-receipts', icon: PackagePlus as NavIcon },
      { label: 'Vendor Returns', to: '/supply/vendor-returns', icon: Route as NavIcon },
      { label: 'Backorders', to: '/supply/backorders', icon: AlertTriangle as NavIcon },
      { label: 'Reorder Signals', to: '/supply/reorder-signals', icon: Truck as NavIcon },
    ],
  },
  {
    label: 'Setup',
    to: '/setup',
    icon: MapPin as NavIcon,
    sectionBreakBefore: true,
    children: [
      { label: 'Warehouses & Areas', to: '/setup/warehouses-areas', icon: Warehouse as NavIcon },
      { label: 'Location Rules', to: '/setup/location-rules', icon: ShieldCheck as NavIcon },
      { label: 'Item / Part References', to: '/setup/item-references', icon: Boxes as NavIcon },
      { label: 'Inventory Policies', to: '/setup/inventory-policies', icon: CheckCircle2 as NavIcon },
      { label: 'Devices & Labels', to: '/setup/devices-labels', icon: FileCheck2 as NavIcon },
    ],
  },
  {
    label: 'Records',
    to: '/records',
    icon: DatabaseZap as NavIcon,
    sectionBreakBefore: true,
    children: [
      { label: 'Stock Ledger', to: '/records/stock-ledger', icon: DatabaseZap as NavIcon },
      { label: 'Receiving History', to: '/records/receiving-history', icon: PackagePlus as NavIcon },
      { label: 'Movement History', to: '/records/movement-history', icon: Route as NavIcon },
      { label: 'Count History', to: '/records/count-history', icon: Activity as NavIcon },
      { label: 'Adjustment History', to: '/records/adjustment-history', icon: ClipboardCheck as NavIcon },
    ],
  },
  {
    label: 'Admin',
    to: '/admin',
    icon: ShieldCheck as NavIcon,
    sectionBreakBefore: true,
    children: [
      { label: 'LoadArr Settings', to: '/admin/settings', icon: Warehouse as NavIcon },
      { label: 'Integrations', to: '/admin/integrations', icon: FileCheck2 as NavIcon },
      { label: 'Permissions', to: '/admin/permissions', icon: ShieldCheck as NavIcon },
    ],
  },
]

function findLoadArrRoute(pathname: string): LoadArrRouteMatch | null {
  for (const route of loadarrRoutes) {
    for (const candidate of [route.path, ...(route.aliases ?? [])]) {
      const match = matchPath({ path: candidate, end: true }, pathname)
      if (!match) {
        continue
      }

      return {
        ...route,
        canonicalPath: generatePath(route.path, match.params),
        params: match.params as Record<string, string>,
      }
    }
  }

  return null
}

function selectLoadArrRouteRecord<T extends { id: string }>(
  records: T[],
  routeId: string | undefined,
  fallback: T | undefined,
): T | undefined {
  if (routeId) {
    const match = records.find((record) => record.id === routeId)
    if (match) {
      return match
    }
  }

  return fallback
}

const suiteHomeUrl = resolveSuiteHomeUrl(import.meta.env.VITE_SUITE_URL)
const productLaunchUrls = buildProductLaunchUrlMap(import.meta.env)
const apiBase = import.meta.env.VITE_LOADARR_API_BASE ?? ''

const fallbackSummary: LoadArrWorkspaceSummary = {
  generatedAt: new Date(0).toISOString(),
  metrics: {
    activeLocations: 0,
    quantityOnHand: 0,
    quantityCommitted: 0,
    quantityBlocked: 0,
    openTasks: 0,
    openHolds: 0,
    unexplainedInventory: 0,
  },
  locations: [],
  inventory: [],
  tasks: [],
  holds: [],
  routeHandoffs: [],
  evidence: [],
  unexplainedInventory: [],
}
const fallbackReceivingSessions: LoadArrReceivingSession[] = []

const fallbackTransferOrders: LoadArrTransferOrder[] = []

const fallbackCounts: LoadArrCount[] = []

const fallbackAdjustments: LoadArrAdjustment[] = []

const fallbackTruckStock: LoadArrTruckStock[] = []

const fallbackKits: LoadArrKit[] = []

const formatNumber = new Intl.NumberFormat('en-US', { maximumFractionDigits: 1 })
const fieldClassName = 'field-block'
const wideFieldClassName = 'field-block wide'
const fieldLabelClassName = 'field-label'
const fieldControlClassName = 'field-control'

const receivingTypeOptions: PickerOption[] = [
  { value: 'manual', label: 'Manual' },
  { value: 'purchase_order', label: 'Purchase order' },
  { value: 'transfer', label: 'Transfer' },
  { value: 'return', label: 'Return' },
  { value: 'vendor_consignment', label: 'Vendor consignment' },
  { value: 'production_output', label: 'Production output' },
]

const receivingConditionOptions: PickerOption[] = [
  { value: 'new', label: 'New' },
  { value: 'pending_inspection', label: 'Pending inspection' },
  { value: 'damaged', label: 'Damaged' },
  { value: 'quarantined', label: 'Quarantined' },
]

const transferTypeOptions: PickerOption[] = [
  { value: 'bin_to_bin', label: 'Bin to bin' },
  { value: 'zone_to_zone', label: 'Zone to zone' },
  { value: 'site_to_site', label: 'Site to site' },
  { value: 'warehouse_to_truck', label: 'Warehouse to truck' },
  { value: 'truck_to_warehouse', label: 'Truck to warehouse' },
  { value: 'quarantine_transfer', label: 'Quarantine transfer' },
  { value: 'return_to_stock', label: 'Return to stock' },
  { value: 'scrap_transfer', label: 'Scrap transfer' },
]

const transferReasonOptions: PickerOption[] = [
  { value: 'putaway', label: 'Putaway' },
  { value: 'quality_inspection', label: 'Quality inspection' },
  { value: 'route_replenishment', label: 'Route replenishment' },
  { value: 'quarantine_move', label: 'Quarantine move' },
  { value: 'return_to_stock', label: 'Return to stock' },
  { value: 'cycle_count_correction', label: 'Cycle count correction' },
]

const kitReasonOptions: PickerOption[] = [
  { value: 'kit_build', label: 'Kit build' },
  { value: 'kit_break', label: 'Kit break' },
  { value: 'kit_replenish', label: 'Kit replenish' },
  { value: 'maintenance_cycle', label: 'Maintenance cycle' },
  { value: 'pm_readiness', label: 'PM readiness' },
  { value: 'emergency_response', label: 'Emergency response' },
  { value: 'inspection_release', label: 'Inspection release' },
  { value: 'route_replenishment', label: 'Route replenishment' },
]

const kitOperationOptions: PickerOption[] = [
  { value: 'build', label: 'Build' },
  { value: 'reserve', label: 'Reserve' },
  { value: 'pick', label: 'Pick' },
  { value: 'break', label: 'Break' },
  { value: 'replenish', label: 'Replenish' },
  { value: 'inspect', label: 'Inspect' },
  { value: 'assign', label: 'Assign' },
  { value: 'return', label: 'Return' },
  { value: 'expire-components', label: 'Expire components' },
  { value: 'track-location', label: 'Track location' },
]

const kitPersonOptions: PickerOption[] = []

const holdTypeOptions: PickerOption[] = [
  { value: 'compliance', label: 'Compliance' },
  { value: 'quality', label: 'Quality' },
  { value: 'damage', label: 'Damage' },
  { value: 'recall', label: 'Recall' },
  { value: 'expired', label: 'Expired' },
  { value: 'missing_sds', label: 'Missing SDS' },
  { value: 'receiving_discrepancy', label: 'Receiving discrepancy' },
  { value: 'training_qualification', label: 'Training qualification' },
  { value: 'investigation', label: 'Investigation' },
  { value: 'unknown_origin', label: 'Unknown origin' },
]

const holdReasonOptions: PickerOption[] = [
  { value: 'sds_label_mismatch', label: 'SDS label mismatch' },
  { value: 'damage_observed', label: 'Damage observed' },
  { value: 'cycle_count_variance', label: 'Cycle count variance' },
  { value: 'missing_documentation', label: 'Missing documentation' },
  { value: 'qualification_required', label: 'Qualification required' },
  { value: 'unknown_origin_review', label: 'Unknown origin review' },
]

const holdReleaseReasonOptions: PickerOption[] = [
  { value: 'compliance_review_cleared', label: 'Compliance review cleared' },
  { value: 'quality_review_cleared', label: 'Quality review cleared' },
  { value: 'documentation_received', label: 'Documentation received' },
  { value: 'cycle_count_reconciled', label: 'Cycle count reconciled' },
]

const unexplainedDiscoverySourceOptions: PickerOption[] = [
  { value: 'cycle_count_variance', label: 'Cycle count variance' },
  { value: 'dock_found_stock', label: 'Dock found stock' },
  { value: 'damaged_freight_receipt', label: 'Damaged freight receipt' },
  { value: 'route_return_variance', label: 'Route return variance' },
]

const unexplainedReasonOptions: PickerOption[] = [
  { value: 'cycle_count_variance', label: 'Cycle count variance' },
  { value: 'unknown_origin_review', label: 'Unknown origin review' },
  { value: 'paperwork_mismatch', label: 'Paperwork mismatch' },
  { value: 'freight_overage', label: 'Freight overage' },
]

const unexplainedResolutionReasonOptions: PickerOption[] = [
  { value: 'supervisor_approved_valid_stock', label: 'Approved valid stock' },
  { value: 'quarantine_pending_investigation', label: 'Quarantine investigation' },
  { value: 'scrap_unknown_origin', label: 'Scrap unknown origin' },
]

const fallbackSupplyArrItemReferences: SupplyArrItemReference[] = []

const initialReceivingForm: ReceivingFormState = {
  receivingType: 'manual',
  sourceProductKey: 'supplyarr',
  sourceObjectType: 'purchase_order',
  sourceObjectId: '',
  supplierNameSnapshot: '',
  completedByPersonId: '',
  supplyarrItemId: '',
  expectedQuantity: '',
  receivedQuantity: '',
  warehouseLocationId: '',
  lotCode: '',
  serialCode: '',
  condition: 'new',
  discrepancyReasonCode: '',
  complianceEvaluationId: '',
  evidenceSummary: '',
}

const initialTransferForm: TransferFormState = {
  transferType: 'bin_to_bin',
  fromLocationId: '',
  toLocationId: '',
  completedByPersonId: '',
  supplyarrItemId: '',
  quantity: '',
  lotCode: '',
  serialCode: '',
  reasonCode: 'quality_inspection',
  complianceEvaluationId: '',
  evidenceSummary: '',
}

const initialHoldForm: HoldFormState = {
  holdType: 'quality',
  warehouseLocationId: '',
  supplyarrItemId: '',
  quantity: '',
  reasonCode: 'sds_label_mismatch',
  description: '',
  createdByPersonId: '',
  complianceEvaluationId: '',
  evidenceSummary: '',
}

const initialHoldReleaseForm: HoldReleaseFormState = {
  holdId: '',
  reasonCode: 'compliance_review_cleared',
  releasedByPersonId: '',
  evidenceSummary: '',
}

const initialUnexplainedInventoryForm: UnexplainedInventoryFormState = {
  discoverySource: 'cycle_count_variance',
  warehouseLocationId: '',
  supplyarrItemId: '',
  expectedQuantity: '',
  quantity: '',
  lotCode: '',
  serialCode: '',
  discoveredByPersonId: '',
  reasonCode: 'cycle_count_variance',
  evidenceSummary: '',
  complianceEvaluationId: '',
}

const initialUnexplainedResolutionForm: UnexplainedInventoryResolutionFormState = {
  recordId: '',
  reasonCode: 'supervisor_approved_valid_stock',
  personId: '',
  quarantineLocationId: '',
  complianceEvaluationId: '',
  evidenceSummary: '',
}

const initialCountForm: CountFormState = {
  countType: 'cycle_count',
  warehouseLocationId: '',
  supplyarrItemId: '',
  expectedQuantity: '',
  countedQuantity: '',
  countedByPersonId: '',
  reasonCode: 'cycle_count_variance',
  evidenceSummary: '',
}

const initialAdjustmentForm: AdjustmentFormState = {
  adjustmentType: 'gain',
  warehouseLocationId: '',
  supplyarrItemId: '',
  quantityDelta: '',
  createdByPersonId: '',
  reasonCode: 'cycle_count_variance',
  evidenceSummary: '',
  approvedByPersonId: '',
}

const initialTruckStockForm: TruckStockFormState = {
  truckStockId: '',
  quantity: '',
  personId: '',
  reasonCode: 'route_replenishment',
  evidenceSummary: '',
}

const initialKitForm: KitFormState = {
  kitId: '',
  operation: 'build',
  quantity: '',
  personId: '',
  reasonCode: 'kit_build',
  evidenceSummary: '',
  targetPersonId: '',
  targetLocationId: '',
}

export function App() {
  const location = useLocation()
  const navigate = useNavigate()
  const session = loadSession()
  const accessToken = session?.accessToken
  const [summary, setSummary] = useState<LoadArrWorkspaceSummary>(fallbackSummary)
  const [loadState, setLoadState] = useState<'loading' | 'live' | 'offline'>('loading')
  const [query, setQuery] = useState('')
  const [receivingForm, setReceivingForm] = useState<ReceivingFormState>(initialReceivingForm)
  const [receivingStatus, setReceivingStatus] = useState<'idle' | 'submitting' | 'completed' | 'failed'>('idle')
  const [receivingCompletion, setReceivingCompletion] = useState<LoadArrReceivingCompletion | null>(null)
  const [transferForm, setTransferForm] = useState<TransferFormState>(initialTransferForm)
  const [transferStatus, setTransferStatus] = useState<'idle' | 'submitting' | 'completed' | 'failed'>('idle')
  const [transferCompletion, setTransferCompletion] = useState<LoadArrTransferCompletion | null>(null)
  const [receivingSessions, setReceivingSessions] = useState<LoadArrReceivingSession[]>(fallbackReceivingSessions)
  const [transferOrders, setTransferOrders] = useState<LoadArrTransferOrder[]>(fallbackTransferOrders)
  const [holdForm, setHoldForm] = useState<HoldFormState>(initialHoldForm)
  const [holdStatus, setHoldStatus] = useState<'idle' | 'submitting' | 'completed' | 'failed'>('idle')
  const [holdMutation, setHoldMutation] = useState<LoadArrHoldMutation | null>(null)
  const [holdReleaseForm, setHoldReleaseForm] = useState<HoldReleaseFormState>(initialHoldReleaseForm)
  const [holdReleaseStatus, setHoldReleaseStatus] = useState<'idle' | 'submitting' | 'completed' | 'failed'>('idle')
  const [unexplainedForm, setUnexplainedForm] = useState<UnexplainedInventoryFormState>(
    initialUnexplainedInventoryForm,
  )
  const [unexplainedResolutionForm, setUnexplainedResolutionForm] =
    useState<UnexplainedInventoryResolutionFormState>(initialUnexplainedResolutionForm)
  const [unexplainedStatus, setUnexplainedStatus] = useState<'idle' | 'submitting' | 'completed' | 'failed'>('idle')
  const [unexplainedMutation, setUnexplainedMutation] =
    useState<LoadArrUnexplainedInventoryMutation | null>(null)
  const [supplyArrItemReferences, setSupplyArrItemReferences] = useState<SupplyArrItemReference[]>(
    fallbackSupplyArrItemReferences,
  )
  const [counts, setCounts] = useState<LoadArrCount[]>(fallbackCounts)
  const [adjustments, setAdjustments] = useState<LoadArrAdjustment[]>(fallbackAdjustments)
  const [locationUtilization, setLocationUtilization] = useState<LoadArrLocationUtilization | null>(null)
  const [selectedLocationId, setSelectedLocationId] = useState(fallbackSummary.locations[0]?.id ?? '')
  const [countForm, setCountForm] = useState<CountFormState>(initialCountForm)
  const [countStatus, setCountStatus] = useState<'idle' | 'submitting' | 'completed' | 'failed'>('idle')
  const [countResult, setCountResult] = useState<LoadArrCountCompletion | null>(null)
  const [adjustmentForm, setAdjustmentForm] = useState<AdjustmentFormState>(initialAdjustmentForm)
  const [adjustmentStatus, setAdjustmentStatus] = useState<'idle' | 'submitting' | 'completed' | 'failed'>('idle')
  const [adjustmentResult, setAdjustmentResult] = useState<LoadArrAdjustmentMutation | null>(null)
  const [truckStockRecords, setTruckStockRecords] = useState<LoadArrTruckStock[]>(fallbackTruckStock)
  const [truckStockForm, setTruckStockForm] = useState<TruckStockFormState>(initialTruckStockForm)
  const [truckStockStatus, setTruckStockStatus] = useState<'idle' | 'submitting' | 'completed' | 'failed'>('idle')
  const [truckStockResult, setTruckStockResult] = useState<LoadArrTruckStockMutation | null>(null)
  const [kitRecords, setKitRecords] = useState<LoadArrKit[]>(fallbackKits)
  const [kitForm, setKitForm] = useState<KitFormState>(initialKitForm)
  const [kitStatus, setKitStatus] = useState<'idle' | 'submitting' | 'completed' | 'failed'>('idle')
  const [kitResult, setKitResult] = useState<LoadArrKitMutation | null>(null)
  const sessionQuery = useQuery({
    queryKey: ['loadarr-session', accessToken],
    queryFn: () => getSessionBootstrap(accessToken!),
    enabled: Boolean(accessToken),
    retry: false,
  })

  const launchCatalogQuery = useQuery({
    queryKey: ['loadarr-launch-catalog', accessToken],
    queryFn: () => getLaunchCatalog(apiBase, accessToken!, 'loadarr'),
    enabled: Boolean(accessToken),
    retry: false,
  })

  const routerBasename = import.meta.env.VITE_ROUTER_BASENAME?.replace(/\/+$/, '') ?? ''
  const normalizedPathname = (() => {
    const pathname = location.pathname.replace(/\/+$/, '') || '/'
    if (routerBasename && pathname.startsWith(routerBasename)) {
      const stripped = pathname.slice(routerBasename.length)
      return stripped || '/'
    }
    return pathname
  })()

  const permissionsQuery = useQuery({
    queryKey: ['loadarr-permission-catalog', accessToken, normalizedPathname],
    queryFn: () => getLoadArrPermissionCatalog(accessToken!),
    enabled: Boolean(accessToken) && normalizedPathname === '/admin/permissions',
    retry: false,
  })

  const activeRoute = useMemo(() => {
    return findLoadArrRoute(normalizedPathname === '/' ? '/work' : normalizedPathname)
  }, [normalizedPathname])

  const activeView = activeRoute?.key ?? 'dashboard'
  useEffect(() => {
    if (normalizedPathname === '/') {
      navigate('/work', { replace: true })
      return
    }

    if (!activeRoute) {
      navigate('/work', { replace: true })
      return
    }

    if (normalizedPathname !== activeRoute.canonicalPath) {
      navigate(activeRoute.canonicalPath, { replace: true })
    }
  }, [activeRoute, navigate, normalizedPathname])

  useEffect(() => {
    const routeLocationId = activeRoute?.params.locationProfileId
    if (!routeLocationId) {
      return
    }

    if (summary.locations.some((location) => location.id === routeLocationId)) {
      setSelectedLocationId(routeLocationId)
    }
  }, [activeRoute?.params.locationProfileId, summary.locations])

  useEffect(() => {
    const routeTruckStockId = activeRoute?.params.stagingAssignmentId
    if (!routeTruckStockId) {
      return
    }

    const record = truckStockRecords.find((candidate) => candidate.id === routeTruckStockId)
    if (record) {
      setTruckStockForm((current) =>
        current.truckStockId === record.id && current.personId === record.assignedPersonId
          ? current
          : {
              ...current,
              truckStockId: record.id,
              personId: record.assignedPersonId,
            },
      )
    }
  }, [activeRoute?.params.stagingAssignmentId, truckStockRecords])

  useEffect(() => {
    const routeKitId = activeRoute?.params.kitId
    if (!routeKitId) {
      return
    }

    const record = kitRecords.find((candidate) => candidate.id === routeKitId)
    if (record) {
      setKitForm((current) =>
        current.kitId === record.id && current.personId === record.assignedPersonId
          ? current
          : {
              ...current,
              kitId: record.id,
              personId: record.assignedPersonId,
              targetPersonId: record.assignedPersonId,
              targetLocationId: record.locationId,
            },
      )
    }
  }, [activeRoute?.params.kitId, kitRecords])

  useEffect(() => {
    const routeHoldId = activeRoute?.params.holdId
    if (!routeHoldId) {
      return
    }

    const record = summary.holds.find((candidate) => candidate.id === routeHoldId)
    if (record) {
      setHoldReleaseForm((current) => (current.holdId === record.id ? current : { ...current, holdId: record.id }))
    }
  }, [activeRoute?.params.holdId, summary.holds])

  useEffect(() => {
    const routeRecordId = activeRoute?.params.recordId
    if (!routeRecordId) {
      return
    }

    const record = summary.unexplainedInventory.find((candidate) => candidate.id === routeRecordId)
    if (record) {
      setUnexplainedResolutionForm((current) =>
        current.recordId === record.id ? current : { ...current, recordId: record.id },
      )
    }
  }, [activeRoute?.params.recordId, summary.unexplainedInventory])

  useEffect(() => {
    if (sessionQuery.isError && resolveProductWorkspaceBootstrapError(sessionQuery.error)) {
      clearSession()
    }
  }, [sessionQuery.error, sessionQuery.isError])

  useEffect(() => {
    if (launchCatalogQuery.isError && resolveProductWorkspaceBootstrapError(launchCatalogQuery.error)) {
      clearSession()
    }
  }, [launchCatalogQuery.error, launchCatalogQuery.isError])

  useEffect(() => {
    const controller = new AbortController()

    loadArrFetch('/api/v1/workspace/summary', accessToken, {
      signal: controller.signal,
      headers: { Accept: 'application/json' },
    })
      .then((response) => {
        if (!response.ok) {
          throw new Error(`Workspace request failed: ${response.status}`)
        }

        return response.json() as Promise<LoadArrWorkspaceSummary>
      })
      .then((data) => {
        setSummary(data)
        setLoadState('live')
      })
      .catch(() => {
        if (!controller.signal.aborted) {
          setSummary(fallbackSummary)
          setLoadState('offline')
        }
      })

    return () => controller.abort()
  }, [accessToken])

  useEffect(() => {
    const controller = new AbortController()

    loadArrFetch('/api/v1/counts', accessToken, {
      signal: controller.signal,
      headers: { Accept: 'application/json' },
    })
      .then((response) => {
        if (!response.ok) {
          throw new Error(`Count request failed: ${response.status}`)
        }

        return response.json() as Promise<{ items: LoadArrCount[] }>
      })
      .then((data) => {
        if (data.items.length > 0) {
          setCounts(data.items)
        }
      })
      .catch(() => {
        if (!controller.signal.aborted) {
          setCounts(fallbackCounts)
        }
      })

    return () => controller.abort()
  }, [accessToken])

  useEffect(() => {
    const controller = new AbortController()

    loadArrFetch('/api/v1/receiving', accessToken, {
      signal: controller.signal,
      headers: { Accept: 'application/json' },
    })
      .then((response) => {
        if (!response.ok) {
          throw new Error(`Receiving request failed: ${response.status}`)
        }

        return response.json() as Promise<{ items: LoadArrReceivingSession[] }>
      })
      .then((data) => {
        if (data.items.length > 0) {
          setReceivingSessions(data.items)
        }
      })
      .catch(() => {
        if (!controller.signal.aborted) {
          setReceivingSessions(fallbackReceivingSessions)
        }
      })

    return () => controller.abort()
  }, [accessToken])

  useEffect(() => {
    const controller = new AbortController()

    loadArrFetch('/api/v1/transfers', accessToken, {
      signal: controller.signal,
      headers: { Accept: 'application/json' },
    })
      .then((response) => {
        if (!response.ok) {
          throw new Error(`Transfer request failed: ${response.status}`)
        }

        return response.json() as Promise<{ items: LoadArrTransferOrder[] }>
      })
      .then((data) => {
        if (data.items.length > 0) {
          setTransferOrders(data.items)
        }
      })
      .catch(() => {
        if (!controller.signal.aborted) {
          setTransferOrders(fallbackTransferOrders)
        }
      })

    return () => controller.abort()
  }, [accessToken])

  useEffect(() => {
    const controller = new AbortController()

    loadArrFetch('/api/v1/adjustments', accessToken, {
      signal: controller.signal,
      headers: { Accept: 'application/json' },
    })
      .then((response) => {
        if (!response.ok) {
          throw new Error(`Adjustment request failed: ${response.status}`)
        }

        return response.json() as Promise<{ items: LoadArrAdjustment[] }>
      })
      .then((data) => {
        if (data.items.length > 0) {
          setAdjustments(data.items)
        }
      })
      .catch(() => {
        if (!controller.signal.aborted) {
          setAdjustments(fallbackAdjustments)
        }
      })

    return () => controller.abort()
  }, [accessToken])

  useEffect(() => {
    const controller = new AbortController()

    loadArrFetch('/api/v1/truck-stock', accessToken, {
      signal: controller.signal,
      headers: { Accept: 'application/json' },
    })
      .then((response) => {
        if (!response.ok) {
          throw new Error(`Truck stock request failed: ${response.status}`)
        }

        return response.json() as Promise<{ items: LoadArrTruckStock[] }>
      })
      .then((data) => {
        if (data.items.length > 0) {
          setTruckStockRecords(data.items)
        }
      })
      .catch(() => {
        if (!controller.signal.aborted) {
          setTruckStockRecords(fallbackTruckStock)
        }
      })

    return () => controller.abort()
  }, [accessToken])

  useEffect(() => {
    const controller = new AbortController()

    loadArrFetch('/api/v1/kits', accessToken, {
      signal: controller.signal,
      headers: { Accept: 'application/json' },
    })
      .then((response) => {
        if (!response.ok) {
          throw new Error(`Kit request failed: ${response.status}`)
        }

        return response.json() as Promise<{ items: LoadArrKit[] }>
      })
      .then((data) => {
        if (data.items.length > 0) {
          setKitRecords(data.items)
        }
      })
      .catch(() => {
        if (!controller.signal.aborted) {
          setKitRecords(fallbackKits)
        }
      })

    return () => controller.abort()
  }, [accessToken])

  useEffect(() => {
    if (!selectedLocationId) {
      setLocationUtilization(null)
      return
    }

    const controller = new AbortController()

    loadArrFetch(`/api/v1/locations/${selectedLocationId}/utilization`, accessToken, {
      signal: controller.signal,
      headers: { Accept: 'application/json' },
    })
      .then((response) => {
        if (!response.ok) {
          throw new Error(`Location utilization request failed: ${response.status}`)
        }

        return response.json() as Promise<LoadArrLocationUtilization>
      })
      .then((data) => {
        setLocationUtilization(data)
      })
      .catch(() => {
        if (!controller.signal.aborted) {
          const location = fallbackSummary.locations.find((candidate) => candidate.id === selectedLocationId)
          setLocationUtilization(location ? createLocalLocationUtilization(location, fallbackSummary) : null)
        }
      })

    return () => controller.abort()
  }, [selectedLocationId, accessToken])

  useEffect(() => {
    const controller = new AbortController()

    loadArrFetch('/api/v1/workspace/supplyarr-item-references', accessToken, {
      signal: controller.signal,
      headers: { Accept: 'application/json' },
    })
      .then((response) => {
        if (!response.ok) {
          throw new Error(`SupplyArr reference request failed: ${response.status}`)
        }

        return response.json() as Promise<{ items: SupplyArrItemReference[] }>
      })
      .then((data) => {
        if (data.items.length > 0) {
          setSupplyArrItemReferences(data.items)
        }
      })
      .catch(() => {
        if (!controller.signal.aborted) {
          setSupplyArrItemReferences(fallbackSupplyArrItemReferences)
        }
      })

    return () => controller.abort()
  }, [accessToken])

  const normalizedQuery = query.trim().toLowerCase()
  const filteredInventory = useMemo(
    () =>
      summary.inventory.filter((item) =>
        [
          item.itemNameSnapshot,
          item.supplyarrItemId,
          item.state,
          item.locationNameSnapshot,
          item.originReference,
          ...item.traceTags,
        ]
          .join(' ')
          .toLowerCase()
          .includes(normalizedQuery),
      ),
    [normalizedQuery, summary.inventory],
  )

  const filteredLocations = useMemo(
    () =>
      summary.locations.filter((location) =>
        [
          location.name,
          location.locationType,
          location.path,
          location.staffarrSiteNameSnapshot,
          ...location.complianceRestrictions,
        ]
          .join(' ')
          .toLowerCase()
          .includes(normalizedQuery),
      ),
    [normalizedQuery, summary.locations],
  )

  const filteredTasks = useMemo(
    () =>
      summary.tasks.filter((task) =>
        [
          task.title,
          task.taskType,
          task.status,
          task.priority,
          task.locationNameSnapshot,
          task.supplyarrItemId,
          ...task.requiredSignals,
        ]
          .join(' ')
          .toLowerCase()
          .includes(normalizedQuery),
      ),
    [normalizedQuery, summary.tasks],
  )

  const filteredPutawayTasks = useMemo(
    () =>
      [
        receivingCompletion?.putawayTask,
        ...summary.tasks.filter((task) => task.taskType === 'putaway'),
      ].filter((task): task is LoadArrTask => Boolean(task)),
    [receivingCompletion?.putawayTask, summary.tasks],
  )

  const filteredHolds = useMemo(
    () =>
      summary.holds.filter((hold) =>
        [
          hold.holdType,
          hold.status,
          hold.reason,
          hold.sourceReference,
          hold.locationNameSnapshot,
          hold.supplyarrItemId,
        ]
          .join(' ')
          .toLowerCase()
          .includes(normalizedQuery),
      ),
    [normalizedQuery, summary.holds],
  )

  const replenishmentRows = useMemo(
    () => [
      ...truckStockRecords.filter((record) => record.quantityOnHand < record.minimumQuantity),
      ...kitRecords.filter((record) => record.quantityOnHand < record.minimumQuantity),
      ...summary.routeHandoffs.filter((handoff) => handoff.status !== 'ready'),
    ],
    [kitRecords, summary.routeHandoffs, truckStockRecords],
  )

  const showInventoryOverview = activeView === 'inventory' || activeView === 'dashboard'

  const filteredTruckStock = useMemo(
    () =>
      truckStockRecords.filter((record) =>
        [
          record.truckStockNumber,
          record.itemNameSnapshot,
          record.status,
          record.truckLocationNameSnapshot,
          record.assignedPersonNameSnapshot,
          ...record.traceTags,
        ]
          .join(' ')
          .toLowerCase()
          .includes(normalizedQuery),
      ),
    [normalizedQuery, truckStockRecords],
  )

  const selectedTruckStock = useMemo(
    () => truckStockRecords.find((record) => record.id === truckStockForm.truckStockId) ?? null,
    [truckStockForm.truckStockId, truckStockRecords],
  )

  useEffect(() => {
    if (truckStockRecords.length === 0) {
      return
    }

    if (!truckStockRecords.some((record) => record.id === truckStockForm.truckStockId)) {
      const fallback = truckStockRecords[0]!
      setTruckStockForm((current) => ({
        ...current,
        truckStockId: fallback.id,
        personId: fallback.assignedPersonId,
      }))
    }
  }, [truckStockForm.truckStockId, truckStockRecords])

  const filteredKits = useMemo(
    () =>
      kitRecords.filter((record) =>
        [
          record.kitNumber,
          record.kitNameSnapshot,
          record.status,
          record.locationNameSnapshot,
          record.assignedPersonNameSnapshot,
          ...record.traceTags,
        ]
          .join(' ')
          .toLowerCase()
          .includes(normalizedQuery),
      ),
    [kitRecords, normalizedQuery],
  )

  const selectedKit = useMemo(
    () => kitRecords.find((record) => record.id === kitForm.kitId) ?? null,
    [kitForm.kitId, kitRecords],
  )

  useEffect(() => {
    if (kitRecords.length === 0) {
      return
    }

    if (!kitRecords.some((record) => record.id === kitForm.kitId)) {
      const fallback = kitRecords[0]!
      setKitForm((current) => ({
        ...current,
        kitId: fallback.id,
        personId: fallback.assignedPersonId,
        targetPersonId: fallback.assignedPersonId,
        targetLocationId: fallback.locationId,
      }))
    }
  }, [kitForm.kitId, kitRecords])

  const filteredHandoffs = useMemo(
    () =>
      summary.routeHandoffs.filter((handoff) =>
        [
          handoff.targetProduct,
          handoff.targetReference,
          handoff.status,
          handoff.locationNameSnapshot,
          handoff.notes,
        ]
          .join(' ')
          .toLowerCase()
          .includes(normalizedQuery),
      ),
    [normalizedQuery, summary.routeHandoffs],
  )

  const filteredUnexplainedInventory = useMemo(
    () =>
      summary.unexplainedInventory.filter((record) =>
        [
          record.recordNumber,
          record.status,
          record.discoverySource,
          record.resolutionState,
          record.locationNameSnapshot,
          record.staffarrSiteNameSnapshot,
          record.supplyarrItemId,
          record.itemNameSnapshot,
          record.reasonCode,
          record.evidenceSummary,
        ]
          .join(' ')
          .toLowerCase()
          .includes(normalizedQuery),
      ),
    [normalizedQuery, summary.unexplainedInventory],
  )

  const selectedLocation = useMemo(
    () => summary.locations.find((location) => location.id === selectedLocationId) ?? summary.locations[0],
    [selectedLocationId, summary.locations],
  )

  const selectedReceivingLocation = useMemo(
    () =>
      summary.locations.find((location) => location.id === receivingForm.warehouseLocationId) ??
      summary.locations[0],
    [receivingForm.warehouseLocationId, summary.locations],
  )

  const selectedSupplyArrItem = useMemo(
    () =>
      supplyArrItemReferences.find((item) => item.supplyarrItemId === receivingForm.supplyarrItemId) ??
      supplyArrItemReferences[0],
    [receivingForm.supplyarrItemId, supplyArrItemReferences],
  )

  const selectedTransferSourceLocation = useMemo(
    () => summary.locations.find((location) => location.id === transferForm.fromLocationId) ?? summary.locations[0],
    [summary.locations, transferForm.fromLocationId],
  )

  const selectedTransferDestinationLocation = useMemo(
    () => summary.locations.find((location) => location.id === transferForm.toLocationId) ?? summary.locations[1],
    [summary.locations, transferForm.toLocationId],
  )

  const selectedTransferItem = useMemo(
    () =>
      supplyArrItemReferences.find((item) => item.supplyarrItemId === transferForm.supplyarrItemId) ??
      supplyArrItemReferences[0],
    [supplyArrItemReferences, transferForm.supplyarrItemId],
  )

  const selectedTransferSourceBalance = useMemo(
    () =>
      summary.inventory.find(
        (item) =>
          item.supplyarrItemId === transferForm.supplyarrItemId &&
          item.locationId === transferForm.fromLocationId,
      ),
    [summary.inventory, transferForm.fromLocationId, transferForm.supplyarrItemId],
  )

  const selectedHoldLocation = useMemo(
    () => summary.locations.find((location) => location.id === holdForm.warehouseLocationId) ?? summary.locations[0],
    [holdForm.warehouseLocationId, summary.locations],
  )

  const selectedHoldItem = useMemo(
    () =>
      supplyArrItemReferences.find((item) => item.supplyarrItemId === holdForm.supplyarrItemId) ??
      supplyArrItemReferences[0],
    [holdForm.supplyarrItemId, supplyArrItemReferences],
  )

  const selectedHoldBalance = useMemo(
    () =>
      summary.inventory.find(
        (item) =>
          item.supplyarrItemId === holdForm.supplyarrItemId &&
          item.locationId === holdForm.warehouseLocationId,
      ),
    [holdForm.supplyarrItemId, holdForm.warehouseLocationId, summary.inventory],
  )

  const selectedReleaseHold = useMemo(
    () => summary.holds.find((hold) => hold.id === holdReleaseForm.holdId) ?? summary.holds[0],
    [holdReleaseForm.holdId, summary.holds],
  )

  const selectedReleaseBalance = useMemo(
    () =>
      summary.inventory.find(
        (item) =>
          item.supplyarrItemId === selectedReleaseHold?.supplyarrItemId &&
          item.locationNameSnapshot === selectedReleaseHold?.locationNameSnapshot,
      ),
    [selectedReleaseHold, summary.inventory],
  )

  const selectedUnexplainedLocation = useMemo(
    () => summary.locations.find((location) => location.id === unexplainedForm.warehouseLocationId) ?? summary.locations[0],
    [summary.locations, unexplainedForm.warehouseLocationId],
  )

  const selectedUnexplainedItem = useMemo(
    () =>
      supplyArrItemReferences.find((item) => item.supplyarrItemId === unexplainedForm.supplyarrItemId) ??
      supplyArrItemReferences[0],
    [supplyArrItemReferences, unexplainedForm.supplyarrItemId],
  )

  const selectedUnexplainedRecord = useMemo(
    () =>
      summary.unexplainedInventory.find((record) => record.id === unexplainedResolutionForm.recordId) ??
      summary.unexplainedInventory[0],
    [summary.unexplainedInventory, unexplainedResolutionForm.recordId],
  )

  const selectedReceivingSession = useMemo(
    () =>
      receivingSessions.find((session) => session.id === activeRoute?.params.receivingSessionId) ??
      receivingSessions[0],
    [activeRoute?.params.receivingSessionId, receivingSessions],
  )

  const selectedTransferOrder = useMemo(
    () =>
      transferOrders.find((order) => order.id === activeRoute?.params.transferId) ??
      transferOrders[0],
    [activeRoute?.params.transferId, transferOrders],
  )

  const selectedCount = useMemo(
    () =>
      counts.find((count) => count.id === countResult?.count.id) ??
      counts.find((count) => count.id === activeRoute?.params.countSessionId) ??
      counts[0],
    [activeRoute?.params.countSessionId, countResult?.count.id, counts],
  )

  const selectedAdjustment = useMemo(
    () =>
      adjustments.find((adjustment) => adjustment.id === adjustmentResult?.adjustment.id) ??
      adjustments.find((adjustment) => adjustment.id === activeRoute?.params.adjustmentId) ??
      adjustments[0],
    [activeRoute?.params.adjustmentId, adjustmentResult?.adjustment.id, adjustments],
  )

  const selectedCountRecord = countResult?.count ?? selectedCount ?? null
  const selectedAdjustmentRecord = adjustmentResult?.adjustment ?? selectedAdjustment ?? null

  const selectedInventoryItem = useMemo(
    () => {
      const routeItemRefId = activeRoute?.params.itemRefId
      if (routeItemRefId) {
        return (
          filteredInventory.find((item) => item.supplyarrItemId === routeItemRefId) ??
          summary.inventory.find((item) => item.supplyarrItemId === routeItemRefId) ??
          filteredInventory[0] ??
          summary.inventory[0] ??
          null
        )
      }

      return filteredInventory[0] ?? summary.inventory[0] ?? null
    },
    [activeRoute?.params.itemRefId, filteredInventory, summary.inventory],
  )

  const selectedPutawayTask = useMemo(
    () => selectLoadArrRouteRecord(filteredPutawayTasks, activeRoute?.params.putawayTaskId, filteredPutawayTasks[0]),
    [activeRoute?.params.putawayTaskId, filteredPutawayTasks],
  )

  const selectedHandoff = useMemo(
    () => selectLoadArrRouteRecord(summary.routeHandoffs, activeRoute?.params.loadoutId, summary.routeHandoffs[0]),
    [activeRoute?.params.loadoutId, summary.routeHandoffs],
  )

  const selectedHoldRecord = useMemo(
    () => selectLoadArrRouteRecord(summary.holds, activeRoute?.params.holdId, summary.holds[0]),
    [activeRoute?.params.holdId, summary.holds],
  )

  const inventoryBalancesByItem = useMemo<LoadArrBalanceRollup[]>(
    () => {
      const rollups = new Map<
        string,
        {
          supplyarrItemId: string
          itemNameSnapshot: string
          unitOfMeasureSnapshot: string
          quantityOnHand: number
          quantityReserved: number
          quantityAllocated: number
          quantityBlocked: number
          locations: Set<string>
          states: Set<string>
        }
      >()

      for (const item of summary.inventory) {
        const current = rollups.get(item.supplyarrItemId)
        if (current) {
          current.quantityOnHand += item.quantityOnHand
          current.quantityReserved += item.quantityReserved
          current.quantityAllocated += item.quantityAllocated
          current.quantityBlocked += item.quantityBlocked
          current.locations.add(item.locationNameSnapshot)
          current.states.add(item.state)
          continue
        }

        rollups.set(item.supplyarrItemId, {
          supplyarrItemId: item.supplyarrItemId,
          itemNameSnapshot: item.itemNameSnapshot,
          unitOfMeasureSnapshot: item.unitOfMeasureSnapshot,
          quantityOnHand: item.quantityOnHand,
          quantityReserved: item.quantityReserved,
          quantityAllocated: item.quantityAllocated,
          quantityBlocked: item.quantityBlocked,
          locations: new Set([item.locationNameSnapshot]),
          states: new Set([item.state]),
        })
      }

      return [...rollups.values()].map((entry) => ({
        supplyarrItemId: entry.supplyarrItemId,
        itemNameSnapshot: entry.itemNameSnapshot,
        unitOfMeasureSnapshot: entry.unitOfMeasureSnapshot,
        quantityOnHand: entry.quantityOnHand,
        quantityReserved: entry.quantityReserved,
        quantityAllocated: entry.quantityAllocated,
        quantityBlocked: entry.quantityBlocked,
        locationCount: entry.locations.size,
        locations: [...entry.locations],
        activeStates: [...entry.states],
      }))
    },
    [summary.inventory],
  )

  const expectedReceiptRows = useMemo<LoadArrExpectedReceipt[]>(
    () =>
      summary.tasks
        .filter((task) => task.taskType === 'receive')
        .map((task) => ({
          id: task.id,
          expectedReceiptNumber: `EXP-${task.id.slice(-4).toUpperCase()}`,
          sourceProductKey: task.requiredSignals.includes('purchase_receipt') ? 'supplyarr' : 'routarr',
          sourceObjectRef: task.title,
          supplierNameSnapshot: task.assignedRole,
          warehouseLocationId:
            summary.locations.find((location) => location.name === task.locationNameSnapshot)?.id ?? summary.locations[0]?.id ?? 'location-001',
          locationNameSnapshot: task.locationNameSnapshot,
          supplyarrItemId: task.supplyarrItemId,
          itemNameSnapshot:
            summary.inventory.find((item) => item.supplyarrItemId === task.supplyarrItemId)?.itemNameSnapshot ??
            task.supplyarrItemId,
          expectedQuantity: task.quantity,
          receivedQuantity: 0,
          status: task.status,
          dueAtUtc: task.dueAtUtc,
        })),
    [summary.inventory, summary.locations, summary.tasks],
  )

  const reservationRows = useMemo<LoadArrReservation[]>(
    () =>
      summary.inventory
        .filter((item) => item.quantityReserved > 0)
        .map((item) => ({
          id: item.id,
          reservationNumber: `RSV-${item.id.slice(-4).toUpperCase()}`,
          demandReference: item.originReference,
          supplyarrItemId: item.supplyarrItemId,
          itemNameSnapshot: item.itemNameSnapshot,
          locationNameSnapshot: item.locationNameSnapshot,
          quantity: item.quantityReserved,
          status: item.state === 'reserved' ? 'reserved' : 'partially_reserved',
          reservedAtUtc: summary.generatedAt,
        })),
    [summary.generatedAt, summary.inventory],
  )

  const pickRows = useMemo<LoadArrWorkflowRecord[]>(
    () =>
      summary.tasks
        .filter((task) => task.taskType === 'pick')
        .map((task) => ({
          id: task.id,
          recordNumber: `PICK-${task.id.slice(-4).toUpperCase()}`,
          subject: task.title,
          status: task.status,
          locationNameSnapshot: task.locationNameSnapshot,
          quantity: task.quantity,
          reasonCode: task.requiredSignals[0] ?? 'pick_request',
          updatedAtUtc: task.dueAtUtc,
          notes: `Assigned to ${task.assignedRole}`,
        })),
    [summary.tasks],
  )

  const issueRows = useMemo<LoadArrWorkflowRecord[]>(
    () =>
      truckStockRecords.map((record) => ({
        id: record.id,
        recordNumber: record.truckStockNumber,
        subject: record.itemNameSnapshot,
        status: record.status,
        locationNameSnapshot: record.truckLocationNameSnapshot,
        quantity: record.quantityOnHand,
        reasonCode: 'issue_to_service_truck',
        updatedAtUtc: record.lastMovementAtUtc,
        notes: `Assigned to ${record.assignedPersonNameSnapshot}`,
      })),
    [truckStockRecords],
  )

  const returnRows = useMemo<LoadArrWorkflowRecord[]>(
    () =>
      summary.inventory
        .filter((item) => item.originEventType === 'route_return')
        .map((item) => ({
          id: item.id,
          recordNumber: `RET-${item.id.slice(-4).toUpperCase()}`,
          subject: item.itemNameSnapshot,
          status: item.state,
          locationNameSnapshot: item.locationNameSnapshot,
          quantity: item.quantityOnHand,
          reasonCode: 'route_return',
          updatedAtUtc: summary.generatedAt,
          notes: item.originReference,
        })),
    [summary.generatedAt, summary.inventory],
  )

  const selectedIssue = useMemo(
    () => selectLoadArrRouteRecord(issueRows, activeRoute?.params.issueId, issueRows[0]),
    [activeRoute?.params.issueId, issueRows],
  )

  const selectedReturn = useMemo(
    () => selectLoadArrRouteRecord(returnRows, activeRoute?.params.returnId, returnRows[0]),
    [activeRoute?.params.returnId, returnRows],
  )

  const discrepancyRows = useMemo<LoadArrDiscrepancyRecord[]>(
    () =>
      summary.unexplainedInventory.map((record) => ({
        id: record.id,
        discrepancyNumber: record.recordNumber,
        sourceType: record.discoverySource,
        status: record.status,
        locationNameSnapshot: record.locationNameSnapshot,
        itemNameSnapshot: record.itemNameSnapshot,
        quantity: record.varianceQuantity,
        reasonCode: record.reasonCode,
        openedAtUtc: record.discoveredAtUtc,
        notes: record.evidenceSummary,
      })),
    [summary.unexplainedInventory],
  )

  const selectedExpectedReceipt = useMemo(
    () =>
      selectLoadArrRouteRecord(expectedReceiptRows, activeRoute?.params.expectedReceiptId, expectedReceiptRows[0]),
    [activeRoute?.params.expectedReceiptId, expectedReceiptRows],
  )

  const selectedReservation = useMemo(
    () => selectLoadArrRouteRecord(reservationRows, activeRoute?.params.reservationId, reservationRows[0]),
    [activeRoute?.params.reservationId, reservationRows],
  )

  const selectedPick = useMemo(
    () => selectLoadArrRouteRecord(pickRows, activeRoute?.params.pickTaskId, pickRows[0]),
    [activeRoute?.params.pickTaskId, pickRows],
  )

  const exceptionRows = useMemo<LoadArrDiscrepancyRecord[]>(() => {
    const routePath = activeRoute?.canonicalPath
    if (!routePath || routePath === '/work/exceptions') {
      return discrepancyRows
    }

    const matches = (record: LoadArrDiscrepancyRecord, needle: string) => {
      const haystacks = [
        record.sourceType,
        record.status,
        record.reasonCode,
        record.locationNameSnapshot,
        record.itemNameSnapshot,
        record.notes,
      ].map((value) => value.toLowerCase())

      return haystacks.some((value) => value.includes(needle))
    }

    switch (routePath) {
      case '/work/exceptions/receiving':
        return discrepancyRows.filter((record) => matches(record, 'receiv') || matches(record, 'receipt'))
      case '/work/exceptions/inventory-holds':
        return discrepancyRows.filter(
          (record) => matches(record, 'hold') || matches(record, 'variance') || matches(record, 'trust'),
        )
      case '/work/exceptions/quarantine':
        return discrepancyRows.filter((record) => matches(record, 'quarantine'))
      case '/work/exceptions/pending-quality-review':
        return discrepancyRows.filter((record) => matches(record, 'review') || matches(record, 'quality'))
      default:
        return discrepancyRows
    }
  }, [activeRoute?.canonicalPath, discrepancyRows])

  const selectedException = useMemo(
    () => selectLoadArrRouteRecord(exceptionRows, activeRoute?.params.exceptionId, exceptionRows[0]),
    [activeRoute?.params.exceptionId, exceptionRows],
  )

  const ledgerRows = useMemo<LoadArrLedgerEntry[]>(
    () => [
      ...summary.inventory.map((item) => ({
        id: `bal-${item.id}`,
        movementType: item.originEventType,
        sourceType: item.originReference,
        status: item.state,
        locationNameSnapshot: item.locationNameSnapshot,
        itemNameSnapshot: item.itemNameSnapshot,
        quantity: item.quantityOnHand,
        unitOfMeasure: item.unitOfMeasureSnapshot,
        occurredAtUtc: summary.generatedAt,
        reasonCode: item.notes,
      })),
      ...counts.map((count) => ({
        id: `cnt-${count.id}`,
        movementType: count.varianceQuantity === 0 ? 'count' : 'count_variance',
        sourceType: count.countNumber,
        status: count.status,
        locationNameSnapshot: count.locationNameSnapshot,
        itemNameSnapshot: count.itemNameSnapshot,
        quantity: Math.abs(count.varianceQuantity),
        unitOfMeasure: count.unitOfMeasure,
        occurredAtUtc: count.updatedAtUtc,
        reasonCode: count.reasonCode,
      })),
      ...adjustments.map((adjustment) => ({
        id: `adj-${adjustment.id}`,
        movementType: 'adjustment',
        sourceType: adjustment.adjustmentNumber,
        status: adjustment.status,
        locationNameSnapshot: adjustment.locationNameSnapshot,
        itemNameSnapshot: adjustment.itemNameSnapshot,
        quantity: adjustment.quantityDelta,
        unitOfMeasure: adjustment.unitOfMeasure,
        occurredAtUtc: adjustment.updatedAtUtc,
        reasonCode: adjustment.reasonCode,
      })),
    ].sort((left, right) => right.occurredAtUtc.localeCompare(left.occurredAtUtc)),
    [adjustments, counts, summary.generatedAt, summary.inventory],
  )

  const settingRows = useMemo(
    () => [
      { key: 'canonical_location_owner', label: 'Canonical location owner', value: 'StaffArr' },
      { key: 'wms_behavior_owner', label: 'WMS behavior owner', value: 'LoadArr' },
      { key: 'inventory_owner', label: 'Inventory balances', value: 'LoadArr' },
      { key: 'stock_ledger_owner', label: 'Stock ledger truth', value: 'LoadArr' },
      { key: 'quality_hold_owner', label: 'Hold and release decisions', value: 'AssurArr' },
      { key: 'route_handoff_owner', label: 'Route handoff consumer', value: 'RoutArr' },
      { key: 'suite_home', label: 'Suite home', value: suiteHomeUrl },
    ],
    [suiteHomeUrl],
  )

  const quarantineLocationOptions = useMemo<PickerOption[]>(
    () =>
      summary.locations
        .filter((location) => location.locationType.includes('quarantine'))
        .map((location) => ({ value: location.id, label: location.name })),
    [summary.locations],
  )

  const receivingLocationOptions = useMemo<PickerOption[]>(
    () => summary.locations.map((location) => ({ value: location.id, label: location.name })),
    [summary.locations],
  )

  const holdOptions = useMemo<PickerOption[]>(
    () =>
      summary.holds.map((hold) => ({
        value: hold.id,
        label: `${hold.supplyarrItemId} · ${hold.locationNameSnapshot}`,
      })),
    [summary.holds],
  )

  const unexplainedRecordOptions = useMemo<PickerOption[]>(
    () =>
      summary.unexplainedInventory.map((record) => ({
        value: record.id,
        label: `${record.recordNumber} · ${record.itemNameSnapshot}`,
      })),
    [summary.unexplainedInventory],
  )

  const supplyArrItemOptions = useMemo<PickerOption[]>(
    () =>
      supplyArrItemReferences.map((item) => ({
        value: item.supplyarrItemId,
        label: `${item.itemNumberSnapshot} · ${item.itemNameSnapshot}`,
      })),
    [supplyArrItemReferences],
  )

  const updateReceivingForm = (field: keyof ReceivingFormState, value: string) => {
    setReceivingForm((current) => ({ ...current, [field]: value }))
  }

  const updateTransferForm = (field: keyof TransferFormState, value: string) => {
    setTransferForm((current) => ({ ...current, [field]: value }))
  }

  const updateHoldForm = (field: keyof HoldFormState, value: string) => {
    setHoldForm((current) => ({ ...current, [field]: value }))
  }

  const updateHoldReleaseForm = (field: keyof HoldReleaseFormState, value: string) => {
    setHoldReleaseForm((current) => ({ ...current, [field]: value }))
  }

  const updateUnexplainedForm = (field: keyof UnexplainedInventoryFormState, value: string) => {
    setUnexplainedForm((current) => ({ ...current, [field]: value }))
  }

  const updateUnexplainedResolutionForm = (
    field: keyof UnexplainedInventoryResolutionFormState,
    value: string,
  ) => {
    setUnexplainedResolutionForm((current) => ({ ...current, [field]: value }))
  }

  const updateCountForm = (field: keyof CountFormState, value: string) => {
    setCountForm((current) => ({ ...current, [field]: value }))
  }

  const updateAdjustmentForm = (field: keyof AdjustmentFormState, value: string) => {
    setAdjustmentForm((current) => ({ ...current, [field]: value }))
  }

  const completeReceiving = async () => {
    setReceivingStatus('submitting')
    const payload = toReceivingPayload(receivingForm)

    try {
      const response = await loadArrFetch('/api/v1/receiving/draft/complete', accessToken, {
        method: 'POST',
        headers: {
          Accept: 'application/json',
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(payload),
      })

      if (!response.ok) {
        throw new Error(`Receiving completion failed: ${response.status}`)
      }

      const data = (await response.json()) as LoadArrReceivingCompletion
      setReceivingCompletion(data)
      setReceivingSessions((current) => [
        createLocalReceivingSessionRecord(receivingForm, selectedReceivingLocation, selectedSupplyArrItem, data),
        ...current.filter((record) => record.receivingNumber !== data.session.receivingNumber),
      ])
      setReceivingStatus('completed')
    } catch {
      const fallback = createLocalPreview(() => {
        const completion = createLocalReceivingCompletion(
          receivingForm,
          selectedReceivingLocation,
          selectedSupplyArrItem,
        )
        return {
          completion,
          record: createLocalReceivingSessionRecord(
            receivingForm,
            selectedReceivingLocation,
            selectedSupplyArrItem,
            completion,
          ),
        }
      })

      if (!fallback) {
        setReceivingStatus('failed')
        return
      }

      setReceivingCompletion(fallback.completion)
      setReceivingSessions((current) => [
        fallback.record,
        ...current.filter((record) => record.receivingNumber !== fallback.completion.session.receivingNumber),
      ])
      setReceivingStatus('completed')
    }
  }

  const completeTransfer = async () => {
    setTransferStatus('submitting')
    const payload = toTransferPayload(transferForm)

    try {
      const response = await loadArrFetch('/api/v1/transfers/draft/complete', accessToken, {
        method: 'POST',
        headers: {
          Accept: 'application/json',
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(payload),
      })

      if (!response.ok) {
        throw new Error(`Transfer completion failed: ${response.status}`)
      }

      const data = (await response.json()) as LoadArrTransferCompletion
      setTransferCompletion(data)
      setTransferOrders((current) => [
        createLocalTransferOrderRecord(
          transferForm,
          selectedTransferSourceLocation,
          selectedTransferDestinationLocation,
          selectedTransferItem,
          data,
        ),
        ...current.filter((record) => record.transferNumber !== data.transfer.transferNumber),
      ])
      setTransferStatus('completed')
    } catch {
      const fallback = createLocalPreview(() => {
        const completion = createLocalTransferCompletion(
          transferForm,
          selectedTransferSourceLocation,
          selectedTransferDestinationLocation,
          selectedTransferItem,
          selectedTransferSourceBalance,
        )
        return {
          completion,
          record: createLocalTransferOrderRecord(
            transferForm,
            selectedTransferSourceLocation,
            selectedTransferDestinationLocation,
            selectedTransferItem,
            completion,
          ),
        }
      })

      if (!fallback) {
        setTransferStatus('failed')
        return
      }

      setTransferCompletion(fallback.completion)
      setTransferOrders((current) => [
        fallback.record,
        ...current.filter((record) => record.transferNumber !== fallback.completion.transfer.transferNumber),
      ])
      setTransferStatus('completed')
    }
  }

  const createHold = async () => {
    setHoldStatus('submitting')
    const payload = toHoldPayload(holdForm)

    try {
      const response = await loadArrFetch('/api/v1/holds', accessToken, {
        method: 'POST',
        headers: {
          Accept: 'application/json',
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(payload),
      })

      if (!response.ok) {
        throw new Error(`Hold creation failed: ${response.status}`)
      }

      const data = (await response.json()) as LoadArrHoldMutation
      setHoldMutation(data)
      setHoldStatus('completed')
    } catch {
      const fallback = createLocalPreview(() =>
        createLocalHoldMutation(
          holdForm,
          selectedHoldLocation,
          selectedHoldItem,
          selectedHoldBalance,
        ),
      )
      if (!fallback) {
        setHoldStatus('failed')
        return
      }
      setHoldMutation(fallback)
      setHoldStatus('completed')
    }
  }

  const releaseHold = async () => {
    setHoldReleaseStatus('submitting')
    const payload = toHoldReleasePayload(holdReleaseForm)

    try {
      const response = await loadArrFetch(`/api/v1/holds/${holdReleaseForm.holdId}/release`, accessToken, {
        method: 'POST',
        headers: {
          Accept: 'application/json',
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(payload),
      })

      if (!response.ok) {
        throw new Error(`Hold release failed: ${response.status}`)
      }

      const data = (await response.json()) as LoadArrHoldMutation
      setHoldMutation(data)
      setHoldReleaseStatus('completed')
    } catch {
      const fallback = createLocalPreview(() =>
        createLocalHoldReleaseMutation(
          holdReleaseForm,
          selectedReleaseHold,
          selectedReleaseBalance,
        ),
      )
      if (!fallback) {
        setHoldReleaseStatus('failed')
        return
      }
      setHoldMutation(fallback)
      setHoldReleaseStatus('completed')
    }
  }

  const createUnexplainedInventory = async () => {
    setUnexplainedStatus('submitting')
    const payload = toUnexplainedInventoryPayload(unexplainedForm)

    try {
      const response = await loadArrFetch('/api/v1/unexplained-inventory', accessToken, {
        method: 'POST',
        headers: {
          Accept: 'application/json',
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(payload),
      })

      if (!response.ok) {
        throw new Error(`Unexplained inventory creation failed: ${response.status}`)
      }

      const data = (await response.json()) as LoadArrUnexplainedInventoryMutation
      setUnexplainedMutation(data)
      setUnexplainedStatus('completed')
    } catch {
      const fallback = createLocalPreview(() =>
        createLocalUnexplainedInventoryMutation(
          unexplainedForm,
          selectedUnexplainedLocation,
          selectedUnexplainedItem,
        ),
      )
      if (!fallback) {
        setUnexplainedStatus('failed')
        return
      }
      setUnexplainedMutation(fallback)
      setUnexplainedStatus('completed')
    }
  }

  const mutateUnexplainedInventory = async (action: 'resolve' | 'quarantine' | 'scrap') => {
    setUnexplainedStatus('submitting')
    const payload = toUnexplainedResolutionPayload(unexplainedResolutionForm, action)

    try {
      const response = await loadArrFetch(
        `/api/v1/unexplained-inventory/${unexplainedResolutionForm.recordId}/${action}`,
        accessToken,
        {
          method: 'POST',
          headers: {
            Accept: 'application/json',
            'Content-Type': 'application/json',
          },
          body: JSON.stringify(payload),
        },
      )

      if (!response.ok) {
        throw new Error(`Unexplained inventory ${action} failed: ${response.status}`)
      }

      const data = (await response.json()) as LoadArrUnexplainedInventoryMutation
      setUnexplainedMutation(data)
      setUnexplainedStatus('completed')
    } catch {
      const fallback = createLocalPreview(() =>
        createLocalUnexplainedResolutionMutation(
          unexplainedResolutionForm,
          selectedUnexplainedRecord,
          action,
        ),
      )
      if (!fallback) {
        setUnexplainedStatus('failed')
        return
      }
      setUnexplainedMutation(fallback)
      setUnexplainedStatus('completed')
    }
  }

  const performTruckStockAction = async (action: 'issue' | 'return' | 'count') => {
    const record = selectedTruckStock
    if (!record) {
      return
    }

    setTruckStockStatus('submitting')

    const payload = {
      truckStockId: truckStockForm.truckStockId,
      quantity: toPositiveNumber(truckStockForm.quantity),
      personId: truckStockForm.personId,
      reasonCode: truckStockForm.reasonCode,
      evidenceSummary: truckStockForm.evidenceSummary || null,
    }

    try {
      const response = await loadArrFetch(`/api/v1/truck-stock/${record.id}/${action}`, accessToken, {
        method: 'POST',
        headers: {
          Accept: 'application/json',
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(payload),
      })

      if (!response.ok) {
        throw new Error(`Truck stock ${action} failed: ${response.status}`)
      }

      const data = (await response.json()) as LoadArrTruckStockMutation
      setTruckStockRecords((current) =>
        current.map((item) => (item.id === data.truckStock.id ? data.truckStock : item)),
      )
      setTruckStockResult(data)
      setTruckStockStatus('completed')
    } catch {
      const fallback = createLocalPreview(() => createLocalTruckStockMutation(action, truckStockForm, record))
      if (!fallback) {
        setTruckStockStatus('failed')
        return
      }
      setTruckStockRecords((current) =>
        current.map((item) => (item.id === fallback.truckStock.id ? fallback.truckStock : item)),
      )
      setTruckStockResult(fallback)
      setTruckStockStatus('completed')
    }
  }

  const performKitAction = async () => {
    const record = selectedKit
    if (!record) {
      return
    }

    setKitStatus('submitting')

    const operation = kitForm.operation as KitOperation
    const quantity = toPositiveNumber(kitForm.quantity)
    const targetPersonNameSnapshot =
      kitPersonOptions.find((option) => option.value === kitForm.targetPersonId)?.label ?? kitForm.targetPersonId
    const targetLocationNameSnapshot =
      summary.locations.find((location) => location.id === kitForm.targetLocationId)?.name ?? kitForm.targetLocationId

    const payload =
      operation === 'assign'
        ? {
            personId: kitForm.personId,
            targetPersonId: kitForm.targetPersonId,
            targetPersonNameSnapshot,
            reasonCode: kitForm.reasonCode,
            evidenceSummary: kitForm.evidenceSummary || null,
          }
        : operation === 'track-location'
          ? {
              personId: kitForm.personId,
              targetLocationId: kitForm.targetLocationId,
              reasonCode: kitForm.reasonCode,
              evidenceSummary: kitForm.evidenceSummary || null,
            }
          : {
              personId: kitForm.personId,
              quantity,
              reasonCode: kitForm.reasonCode,
              evidenceSummary: kitForm.evidenceSummary || null,
            }

    try {
      const response = await loadArrFetch(`/api/v1/kits/${record.id}/${operation}`, accessToken, {
        method: 'POST',
        headers: {
          Accept: 'application/json',
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(payload),
      })

      if (!response.ok) {
        throw new Error(`Kit ${operation} failed: ${response.status}`)
      }

      const data = (await response.json()) as LoadArrKitMutation
      setKitRecords((current) => current.map((item) => (item.id === data.kit.id ? data.kit : item)))
      setKitResult(data)
      setKitStatus('completed')
    } catch {
      const fallback = createLocalPreview(() =>
        createLocalKitMutation(operation, kitForm, record, targetPersonNameSnapshot, targetLocationNameSnapshot),
      )
      if (!fallback) {
        setKitStatus('failed')
        return
      }
      setKitRecords((current) => current.map((item) => (item.id === fallback.kit.id ? fallback.kit : item)))
      setKitResult(fallback)
      setKitStatus('completed')
    }
  }

  const performCount = async () => {
    setCountStatus('submitting')
    const createPayload = toCountCreatePayload(countForm)
    const completePayload = toCountCompletionPayload(countForm)

    try {
      const createResponse = await loadArrFetch('/api/v1/counts', accessToken, {
        method: 'POST',
        headers: {
          Accept: 'application/json',
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(createPayload),
      })

      if (!createResponse.ok) {
        throw new Error(`Count creation failed: ${createResponse.status}`)
      }

      const createdCount = (await createResponse.json()) as LoadArrCount

      const completeResponse = await loadArrFetch(`/api/v1/counts/${createdCount.id}/complete`, accessToken, {
        method: 'POST',
        headers: {
          Accept: 'application/json',
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(completePayload),
      })

      if (!completeResponse.ok) {
        throw new Error(`Count completion failed: ${completeResponse.status}`)
      }

      const data = (await completeResponse.json()) as LoadArrCountCompletion
      setCountResult(data)
      setCountStatus('completed')
    } catch {
      const fallback = createLocalPreview(() =>
        createLocalCountCompletion(countForm, selectedLocation, selectedCount, supplyArrItemReferences),
      )
      if (!fallback) {
        setCountStatus('failed')
        return
      }
      setCountResult(fallback)
      setCountStatus('completed')
    }
  }

  const approveCountVariance = async () => {
    const countId = countResult?.count.id ?? selectedCount?.id
    if (!countId) {
      return
    }

    setCountStatus('submitting')
    const payload = {
      countType: countForm.countType,
      staffarrSiteOrgUnitId: selectedLocation?.staffarrSiteOrgUnitId ?? '',
      warehouseLocationId: countForm.warehouseLocationId,
      supplyarrItemId: countForm.supplyarrItemId,
      expectedQuantity: toNonNegativeNumber(countForm.expectedQuantity),
      approvedByPersonId: countForm.countedByPersonId,
      countedQuantity: toNonNegativeNumber(countForm.countedQuantity),
      reasonCode: countForm.reasonCode,
      evidenceSummary: countForm.evidenceSummary,
      complianceEvaluationId: '',
    }

    try {
      const response = await loadArrFetch(`/api/v1/counts/${countId}/approve-variance`, accessToken, {
        method: 'POST',
        headers: {
          Accept: 'application/json',
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(payload),
      })

      if (!response.ok) {
        throw new Error(`Count variance approval failed: ${response.status}`)
      }

      const data = (await response.json()) as LoadArrCountCompletion
      setCountResult(data)
      setCountStatus('completed')
    } catch {
      const fallback = createLocalPreview(() =>
        createLocalCountVarianceApproval(
          countResult ?? createLocalCountCompletion(countForm, selectedLocation, selectedCount, supplyArrItemReferences),
        ),
      )
      if (!fallback) {
        setCountStatus('failed')
        return
      }
      setCountResult(fallback)
      setCountStatus('completed')
    }
  }

  const createAdjustment = async () => {
    setAdjustmentStatus('submitting')
    const payload = toAdjustmentCreatePayload(adjustmentForm)

    try {
      const response = await loadArrFetch('/api/v1/adjustments', accessToken, {
        method: 'POST',
        headers: {
          Accept: 'application/json',
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(payload),
      })

      if (!response.ok) {
        throw new Error(`Adjustment creation failed: ${response.status}`)
      }

      const data = (await response.json()) as LoadArrAdjustment
      setAdjustmentResult({ adjustment: data, originEvent: null, movement: null })
      setAdjustmentStatus('completed')
    } catch {
      const fallback = createLocalPreview(() =>
        createLocalAdjustmentMutation(
          adjustmentForm,
          selectedLocation,
          selectedAdjustment,
          selectedCount,
          supplyArrItemReferences,
        ),
      )
      if (!fallback) {
        setAdjustmentStatus('failed')
        return
      }
      setAdjustmentResult(fallback)
      setAdjustmentStatus('completed')
    }
  }

  const approveAdjustment = async () => {
    const adjustmentId = adjustmentResult?.adjustment.id ?? selectedAdjustment?.id
    if (!adjustmentId) {
      return
    }

    setAdjustmentStatus('submitting')
    const payload = {
      adjustmentType: adjustmentForm.adjustmentType,
      staffarrSiteOrgUnitId: selectedLocation?.staffarrSiteOrgUnitId ?? '',
      warehouseLocationId: adjustmentForm.warehouseLocationId,
      supplyarrItemId: adjustmentForm.supplyarrItemId,
      quantityDelta: toSignedNumber(adjustmentForm.quantityDelta),
      createdByPersonId: adjustmentForm.createdByPersonId,
      approvedByPersonId: adjustmentForm.approvedByPersonId,
      reasonCode: adjustmentForm.reasonCode,
      evidenceSummary: adjustmentForm.evidenceSummary,
      complianceEvaluationId: '',
    }

    try {
      const response = await loadArrFetch(`/api/v1/adjustments/${adjustmentId}/approve`, accessToken, {
        method: 'POST',
        headers: {
          Accept: 'application/json',
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(payload),
      })

      if (!response.ok) {
        throw new Error(`Adjustment approval failed: ${response.status}`)
      }

      const data = (await response.json()) as LoadArrAdjustmentMutation
      setAdjustmentResult(data)
      setAdjustmentStatus('completed')
    } catch {
      const fallback = createLocalPreview(() =>
        createLocalAdjustmentApproval(
          adjustmentResult
            ?? createLocalAdjustmentMutation(
              adjustmentForm,
              selectedLocation,
              selectedAdjustment,
              selectedCount,
              supplyArrItemReferences,
            ),
        ),
      )
      if (!fallback) {
        setAdjustmentStatus('failed')
        return
      }
      setAdjustmentResult(fallback)
      setAdjustmentStatus('completed')
    }
  }

  const sessionBootstrapError = sessionQuery.isError
    ? resolveProductWorkspaceBootstrapError(sessionQuery.error)
    : null
  const launchBootstrapError = launchCatalogQuery.isError
    ? resolveProductWorkspaceBootstrapError(launchCatalogQuery.error)
    : null
  const bootstrapError = sessionBootstrapError ?? launchBootstrapError

  const workspaceSession =
    session && sessionQuery.data && !bootstrapError
      ? {
          userDisplayName: session.displayName,
          tenantDisplayName: session.tenantDisplayName,
          tenantSlug: session.tenantSlug,
        }
      : null

  const switcherEntitlements =
    launchCatalogQuery.data?.products.map((product) => product.productKey) ??
    sessionQuery.data?.entitlements ??
    []

  const productLaunch = useProductWorkspaceLaunch({
    apiBase,
    accessToken: session?.accessToken ?? '',
    currentProductKey: 'loadarr',
    suiteHomeUrl,
    productLaunchUrls,
  })

  return (
    <ProductWorkspaceFrame
      productName="LoadArr"
      productKey="loadarr"
      workspaceSubtitle="Warehouse execution and inventory custody"
      navItems={productNavItems}
      entitlements={switcherEntitlements}
      suiteHomeUrl={suiteHomeUrl}
      productLaunchUrls={productLaunchUrls}
      onSelectProduct={(productKey) => {
        if (session?.accessToken) {
          void productLaunch.mutate(productKey)
        }
      }}
      onSignOut={() => {
        clearSession()
        window.location.assign(suiteHomeUrl)
      }}
      isProductLaunchPending={productLaunch.isPending}
      productLaunchError={productLaunch.isError ? formatProductLaunchError(productLaunch.error) : null}
      aiAssistance={
        session?.accessToken ? { apiBase, accessToken: session.accessToken } : undefined
      }
      workspaceSession={workspaceSession}
      isBootstrapping={
        Boolean(session?.accessToken) && (sessionQuery.isLoading || launchCatalogQuery.isLoading)
      }
      bootstrapError={bootstrapError}
    >
      <div className="shell">
        <section className="workspace" aria-label="LoadArr workspace">
          <header className="header">
            <div>
              <p className="eyebrow">LoadArr</p>
              <h1>Warehouse execution</h1>
            </div>
            <div className={`status-pill ${loadState}`}>
              <Warehouse aria-hidden="true" />
              <span>{loadState === 'live' ? 'API live' : loadState === 'loading' ? 'Loading' : 'Local snapshot'}</span>
            </div>
          </header>

          <section className="metrics" aria-label="Warehouse metrics">
            <Metric icon={Warehouse} label="Active locations" value={summary.metrics.activeLocations} />
            <Metric icon={PackageCheck} label="On hand" value={summary.metrics.quantityOnHand} />
            <Metric icon={Truck} label="Committed" value={summary.metrics.quantityCommitted} />
            <Metric icon={AlertTriangle} label="Blocked" value={summary.metrics.quantityBlocked} tone="warning" />
            <Metric icon={ClipboardCheck} label="Open tasks" value={summary.metrics.openTasks} />
            <Metric icon={ShieldCheck} label="Open holds" value={summary.metrics.openHolds} tone="warning" />
            <Metric icon={AlertTriangle} label="Unexplained" value={summary.metrics.unexplainedInventory} tone="warning" />
          </section>

          <section className="control-band" aria-label="Workspace controls">
            <label className="search-field">
              <Search aria-hidden="true" />
              <span className="sr-only">Search workspace records</span>
              <input
                value={query}
                onChange={(event) => setQuery(event.target.value)}
                placeholder="Search item, site, hold, task, or route"
              />
            </label>
          </section>

        {showInventoryOverview && (
          <section className="receiving-layout" aria-label="Inventory balances and item detail">
            <article className="workflow-panel">
              <div className="section-heading">
                <Boxes aria-hidden="true" />
                <h2>Inventory balances</h2>
              </div>

              <section className="data-grid inventory-grid" aria-label="Inventory balance list">
                {filteredInventory.map((item) => (
                  <article className="panel" key={item.id}>
                    <div className="panel-title-row">
                      <div>
                        <span className="kicker">{item.supplyarrItemId}</span>
                        <h2>{item.itemNameSnapshot}</h2>
                      </div>
                      <StatusChip value={item.state} />
                    </div>
                    <dl className="quantity-grid">
                      <Quantity label="On hand" value={item.quantityOnHand} />
                      <Quantity label="Reserved" value={item.quantityReserved} />
                      <Quantity label="Allocated" value={item.quantityAllocated} />
                      <Quantity label="Blocked" value={item.quantityBlocked} />
                    </dl>
                    <div className="detail-line">
                      <MapPin aria-hidden="true" />
                      <span>{item.locationNameSnapshot}</span>
                    </div>
                    <div className="detail-line">
                      <DatabaseZap aria-hidden="true" />
                      <span>
                        {item.originEventType} · {item.originReference}
                      </span>
                    </div>
                    <TagList tags={item.traceTags} />
                    <p className="notes">{item.notes}</p>
                  </article>
                ))}
              </section>
            </article>

            <aside className="side-panel" aria-label="Inventory item detail">
              <div className="section-heading">
                <FileCheck2 aria-hidden="true" />
                <h2>Item detail</h2>
              </div>

              {selectedInventoryItem ? (
                <div className="completion-stack">
                  <AuditFact label="Item" value={`${selectedInventoryItem.supplyarrItemId} · ${selectedInventoryItem.itemNameSnapshot}`} />
                  <AuditFact label="Location" value={selectedInventoryItem.locationNameSnapshot} />
                  <AuditFact label="Status" value={selectedInventoryItem.state} />
                  <AuditFact
                    label="Balance"
                    value={`${formatNumber.format(selectedInventoryItem.quantityOnHand)} on hand, ${formatNumber.format(selectedInventoryItem.quantityReserved)} reserved`}
                  />
                  <AuditFact
                    label="Availability"
                    value={
                      selectedInventoryItem.quantityBlocked > 0
                        ? `${formatNumber.format(selectedInventoryItem.quantityBlocked)} blocked`
                        : 'Available for work'
                    }
                  />
                  <TagList tags={selectedInventoryItem.traceTags} />
                  <p className="notes">{selectedInventoryItem.notes}</p>
                </div>
              ) : (
                <div className="empty-state">
                  <strong>No inventory selected</strong>
                  <span>Inventory balances are currently empty.</span>
                </div>
              )}
            </aside>
          </section>
        )}

        {activeView === 'receiving' && (
          <section className="receiving-layout" aria-label="Guided receiving workflow">
            <article className="workflow-panel">
              <div className="section-heading">
                <PackagePlus aria-hidden="true" />
                <h2>Manual receiving</h2>
              </div>

              <div className="form-grid">
                <FormField label="Receiving type" className={fieldClassName} labelClassName={fieldLabelClassName}>
                  <ControlledSelect
                    value={receivingForm.receivingType}
                    onChange={(value) => updateReceivingForm('receivingType', value)}
                    options={receivingTypeOptions}
                    className={fieldControlClassName}
                  />
                </FormField>

                <FormField label="StaffArr site" className={fieldClassName} labelClassName={fieldLabelClassName}>
                  <input
                    className={fieldControlClassName}
                    value={selectedReceivingLocation?.staffarrSiteNameSnapshot ?? ''}
                    readOnly
                  />
                </FormField>

                <FormField label="Receiving location" className={fieldClassName} labelClassName={fieldLabelClassName}>
                  <ControlledSelect
                    value={receivingForm.warehouseLocationId}
                    onChange={(value) => updateReceivingForm('warehouseLocationId', value)}
                    options={receivingLocationOptions}
                    className={fieldControlClassName}
                  />
                </FormField>

                <FormField label="Source reference" className={fieldClassName} labelClassName={fieldLabelClassName}>
                  <input
                    className={fieldControlClassName}
                    value={receivingForm.sourceObjectId}
                    onChange={(event) => updateReceivingForm('sourceObjectId', event.target.value)}
                  />
                </FormField>

                <FormField label="Supplier snapshot" className={fieldClassName} labelClassName={fieldLabelClassName}>
                  <input
                    className={fieldControlClassName}
                    value={receivingForm.supplierNameSnapshot}
                    onChange={(event) => updateReceivingForm('supplierNameSnapshot', event.target.value)}
                  />
                </FormField>

                <FormField label="SupplyArr item" className={fieldClassName} labelClassName={fieldLabelClassName}>
                  <ControlledSelect
                    value={receivingForm.supplyarrItemId}
                    onChange={(value) => updateReceivingForm('supplyarrItemId', value)}
                    options={supplyArrItemOptions}
                    className={fieldControlClassName}
                  />
                </FormField>

                <FormField label="Item name snapshot" className={fieldClassName} labelClassName={fieldLabelClassName}>
                  <input
                    className={fieldControlClassName}
                    value={selectedSupplyArrItem?.itemNameSnapshot ?? ''}
                    readOnly
                  />
                </FormField>

                <FormField label="Expected quantity" className={fieldClassName} labelClassName={fieldLabelClassName}>
                  <input
                    className={fieldControlClassName}
                    inputMode="decimal"
                    value={receivingForm.expectedQuantity}
                    onChange={(event) => updateReceivingForm('expectedQuantity', event.target.value)}
                  />
                </FormField>

                <FormField label="Received quantity" className={fieldClassName} labelClassName={fieldLabelClassName}>
                  <input
                    className={fieldControlClassName}
                    inputMode="decimal"
                    value={receivingForm.receivedQuantity}
                    onChange={(event) => updateReceivingForm('receivedQuantity', event.target.value)}
                  />
                </FormField>

                <FormField label="UOM" className={fieldClassName} labelClassName={fieldLabelClassName}>
                  <input
                    className={fieldControlClassName}
                    value={selectedSupplyArrItem?.unitOfMeasureSnapshot ?? ''}
                    readOnly
                  />
                </FormField>

                <FormField
                  label={selectedSupplyArrItem?.isLotControlled ? 'Lot code required' : 'Lot code'}
                  className={fieldClassName}
                  labelClassName={fieldLabelClassName}
                >
                  <input
                    className={fieldControlClassName}
                    value={receivingForm.lotCode}
                    onChange={(event) => updateReceivingForm('lotCode', event.target.value)}
                  />
                </FormField>

                <FormField
                  label={selectedSupplyArrItem?.isSerialControlled ? 'Serial required' : 'Serial'}
                  className={fieldClassName}
                  labelClassName={fieldLabelClassName}
                >
                  <input
                    className={fieldControlClassName}
                    value={receivingForm.serialCode}
                    onChange={(event) => updateReceivingForm('serialCode', event.target.value)}
                  />
                </FormField>

                <FormField label="Condition" className={fieldClassName} labelClassName={fieldLabelClassName}>
                  <ControlledSelect
                    value={receivingForm.condition}
                    onChange={(value) => updateReceivingForm('condition', value)}
                    options={receivingConditionOptions}
                    className={fieldControlClassName}
                  />
                </FormField>

                <FormField
                  label="Evidence summary"
                  className={wideFieldClassName}
                  labelClassName={fieldLabelClassName}
                >
                  <textarea
                    className={fieldControlClassName}
                    value={receivingForm.evidenceSummary}
                    onChange={(event) => updateReceivingForm('evidenceSummary', event.target.value)}
                  />
                </FormField>
              </div>

              <button
                type="button"
                className="primary-action"
                onClick={() => void completeReceiving()}
                disabled={receivingStatus === 'submitting'}
              >
                <CheckCircle2 aria-hidden="true" />
                <span>{receivingStatus === 'submitting' ? 'Completing' : 'Complete receiving'}</span>
              </button>
            </article>

            <aside className="side-panel" aria-label="Receiving completion audit">
              <div className="section-heading">
                <FileCheck2 aria-hidden="true" />
                <h2>Completion audit</h2>
              </div>

              {receivingCompletion ? (
                <div className="completion-stack">
                  <AuditFact
                    label="Origin event"
                    value={`${receivingCompletion.originEvent.originType} · ${receivingCompletion.originEvent.id}`}
                  />
                  <AuditFact
                    label="Movement"
                    value={`${receivingCompletion.movement.movementType} · ${receivingCompletion.movement.reasonCode}`}
                  />
                  <AuditFact
                    label="Balance"
                    value={`${formatNumber.format(receivingCompletion.balance.quantityOnHand)} ${receivingCompletion.balance.unitOfMeasureSnapshot} available at ${receivingCompletion.balance.locationNameSnapshot}`}
                  />
                  <AuditFact
                    label="Putaway task"
                    value={`${receivingCompletion.putawayTask.title} · ${receivingCompletion.putawayTask.status}`}
                  />
                </div>
              ) : selectedReceivingSession ? (
                <div className="completion-stack">
                  <AuditFact
                    label="Receiving"
                    value={`${selectedReceivingSession.receivingNumber} · ${selectedReceivingSession.status}`}
                  />
                  <AuditFact
                    label="Source"
                    value={`${selectedReceivingSession.sourceProductKey} · ${selectedReceivingSession.sourceObjectType} · ${selectedReceivingSession.sourceObjectId}`}
                  />
                  <AuditFact
                    label="Location"
                    value={`${selectedReceivingSession.staffarrSiteNameSnapshot} · ${selectedReceivingSession.lines[0]?.locationNameSnapshot ?? 'Unknown location'}`}
                  />
                  <AuditFact
                    label="Lines"
                    value={`${selectedReceivingSession.lines.length} line(s) · ${selectedReceivingSession.lines[0]?.itemNameSnapshot ?? 'No lines available'}`}
                  />
                  <AuditFact label="Started" value={formatDate(selectedReceivingSession.startedAtUtc)} />
                </div>
              ) : (
                <div className="empty-state">
                  <strong>Awaiting completion</strong>
                  <span>Completing receiving will create the origin, movement, balance, and putaway task records.</span>
                </div>
              )}
            </aside>
          </section>
        )}

        {activeView === 'balances' && (
          <section className="receiving-layout" aria-label="Inventory balances by item">
            <article className="workflow-panel">
              <div className="section-heading">
                <PackageCheck aria-hidden="true" />
                <h2>Balance rollup</h2>
              </div>

              <div className="data-grid inventory-grid">
                {inventoryBalancesByItem.map((item) => (
                  <article className="panel" key={item.supplyarrItemId}>
                    <div className="panel-title-row">
                      <div>
                        <span className="kicker">{item.supplyarrItemId}</span>
                        <h2>{item.itemNameSnapshot}</h2>
                      </div>
                      <StatusChip value={item.activeStates.join(' / ')} />
                    </div>
                    <dl className="quantity-grid">
                      <Quantity label="Locations" value={item.locationCount} />
                      <Quantity label="On hand" value={item.quantityOnHand} />
                      <Quantity label="Reserved" value={item.quantityReserved} />
                      <Quantity label="Allocated" value={item.quantityAllocated} />
                      <Quantity label="Blocked" value={item.quantityBlocked} />
                    </dl>
                    <TagList tags={item.locations} />
                  </article>
                ))}
              </div>
            </article>

            <aside className="side-panel" aria-label="Balance summary">
              <div className="section-heading">
                <BarChart3 aria-hidden="true" />
                <h2>Balance snapshot</h2>
              </div>
              <div className="completion-stack">
                <AuditFact label="Items tracked" value={inventoryBalancesByItem.length.toString()} />
                <AuditFact label="Open tasks" value={summary.metrics.openTasks.toString()} />
                <AuditFact label="Open holds" value={summary.metrics.openHolds.toString()} />
                <AuditFact label="Unexplained" value={summary.metrics.unexplainedInventory.toString()} />
                <AuditFact label="Last refresh" value={formatDate(summary.generatedAt)} />
              </div>
            </aside>
          </section>
        )}

        {activeView === 'expected-receipts' && (
          <section className="receiving-layout" aria-label="Expected receipts">
            <article className="workflow-panel">
              <div className="section-heading">
                <PackagePlus aria-hidden="true" />
                <h2>Expected receipt watchlist</h2>
              </div>

              <div className="queue compact-queue">
                {expectedReceiptRows.map((receipt) => (
                  <article className="queue-row" key={receipt.id}>
                    <PackagePlus aria-hidden="true" />
                    <div>
                      <div className="row-heading">
                        <h2>{receipt.expectedReceiptNumber}</h2>
                        <StatusChip value={receipt.status} />
                      </div>
                      <p>
                        {receipt.itemNameSnapshot} · expected {formatNumber.format(receipt.expectedQuantity)}{' '}
                        {selectedSupplyArrItem?.unitOfMeasureSnapshot ?? 'each'} · {receipt.locationNameSnapshot}
                      </p>
                      <TagList tags={[receipt.supplierNameSnapshot, receipt.sourceObjectRef, receipt.sourceProductKey]} />
                    </div>
                    <time dateTime={receipt.dueAtUtc}>{formatDate(receipt.dueAtUtc)}</time>
                  </article>
                ))}
              </div>
            </article>

            <aside className="side-panel" aria-label="Expected receipt detail">
              <div className="section-heading">
                <FileCheck2 aria-hidden="true" />
                <h2>Receipt detail</h2>
              </div>
              {selectedExpectedReceipt ? (
                <div className="completion-stack">
                  <AuditFact label="Receipt" value={selectedExpectedReceipt.expectedReceiptNumber} />
                  <AuditFact label="Source" value={`${selectedExpectedReceipt.sourceProductKey} · ${selectedExpectedReceipt.sourceObjectRef}`} />
                  <AuditFact label="Location" value={selectedExpectedReceipt.locationNameSnapshot} />
                  <AuditFact
                    label="Quantity"
                    value={`${formatNumber.format(selectedExpectedReceipt.expectedQuantity)} expected, ${formatNumber.format(selectedExpectedReceipt.receivedQuantity)} received`}
                  />
                  <AuditFact label="Due" value={formatDate(selectedExpectedReceipt.dueAtUtc)} />
                </div>
              ) : (
                <div className="empty-state">
                  <strong>No expected receipts</strong>
                  <span>LoadArr exposes the expected-receipt watchlist for inbound receiving readiness.</span>
                </div>
              )}
            </aside>
          </section>
        )}

        {activeView === 'reservations' && (
          <section className="receiving-layout" aria-label="Reservations">
            <article className="workflow-panel">
              <div className="section-heading">
                <ClipboardCheck aria-hidden="true" />
                <h2>Reservation queue</h2>
              </div>

              <div className="queue compact-queue">
                {reservationRows.map((reservation) => (
                  <article className="queue-row" key={reservation.id}>
                    <ClipboardCheck aria-hidden="true" />
                    <div>
                      <div className="row-heading">
                        <h2>{reservation.reservationNumber}</h2>
                        <StatusChip value={reservation.status} />
                      </div>
                      <p>
                        {reservation.itemNameSnapshot} · {formatNumber.format(reservation.quantity)} reserved ·{' '}
                        {reservation.locationNameSnapshot}
                      </p>
                      <TagList tags={[reservation.demandReference, reservation.supplyarrItemId]} />
                    </div>
                    <time dateTime={reservation.reservedAtUtc}>{formatDate(reservation.reservedAtUtc)}</time>
                  </article>
                ))}
              </div>
            </article>

            <aside className="side-panel" aria-label="Reservation detail">
              <div className="section-heading">
                <ClipboardCheck aria-hidden="true" />
                <h2>Reservation detail</h2>
              </div>
              {selectedReservation ? (
                <div className="completion-stack">
                  <AuditFact label="Reservation" value={selectedReservation.reservationNumber} />
                  <AuditFact label="Demand" value={selectedReservation.demandReference} />
                  <AuditFact label="Item" value={selectedReservation.itemNameSnapshot} />
                  <AuditFact label="Quantity" value={formatNumber.format(selectedReservation.quantity)} />
                  <AuditFact label="Status" value={selectedReservation.status} />
                </div>
              ) : (
                <div className="empty-state">
                  <strong>No reservations</strong>
                  <span>Reserved quantities will appear here for inbound demand and fulfillment demand.</span>
                </div>
              )}
            </aside>
          </section>
        )}

        {activeView === 'picks' && (
          <section className="receiving-layout" aria-label="Picking">
            <article className="workflow-panel">
              <div className="section-heading">
                <ClipboardList aria-hidden="true" />
                <h2>Picking</h2>
              </div>

              <div className="queue compact-queue">
                {pickRows.map((pick) => (
                  <article className="queue-row" key={pick.id}>
                    <ClipboardList aria-hidden="true" />
                    <div>
                      <div className="row-heading">
                        <h2>{pick.recordNumber}</h2>
                        <StatusChip value={pick.status} />
                      </div>
                      <p>
                        {pick.subject} · {formatNumber.format(pick.quantity)} · {pick.locationNameSnapshot}
                      </p>
                      <TagList tags={[pick.reasonCode, pick.notes]} />
                    </div>
                    <time dateTime={pick.updatedAtUtc}>{formatDate(pick.updatedAtUtc)}</time>
                  </article>
                ))}
              </div>
            </article>

            <aside className="side-panel" aria-label="Picking detail">
              <div className="section-heading">
                <ClipboardCheck aria-hidden="true" />
                <h2>Picking detail</h2>
              </div>
              {selectedPick ? (
                <div className="completion-stack">
                  <AuditFact label="Pick" value={selectedPick.recordNumber} />
                  <AuditFact label="Subject" value={selectedPick.subject} />
                  <AuditFact label="Quantity" value={formatNumber.format(selectedPick.quantity)} />
                  <AuditFact label="Location" value={selectedPick.locationNameSnapshot} />
                  <AuditFact label="Status" value={selectedPick.status} />
                </div>
              ) : (
                <div className="empty-state">
                  <strong>No pick tasks</strong>
                  <span>Pick tasks appear here for routed or order-driven demand.</span>
                </div>
              )}
            </aside>
          </section>
        )}

        {activeView === 'issues' && (
          <section className="receiving-layout" aria-label="Backorders and issues">
            <article className="workflow-panel">
              <div className="section-heading">
                <AlertTriangle aria-hidden="true" />
                <h2>Backorders</h2>
              </div>

              <div className="queue compact-queue">
                {issueRows.map((issue) => (
                  <article className="queue-row" key={issue.id}>
                    <AlertTriangle aria-hidden="true" />
                    <div>
                      <div className="row-heading">
                        <h2>{issue.recordNumber}</h2>
                        <StatusChip value={issue.status} />
                      </div>
                      <p>
                        {issue.subject} · {formatNumber.format(issue.quantity)} · {issue.locationNameSnapshot}
                      </p>
                      <TagList tags={[issue.reasonCode, issue.notes]} />
                    </div>
                    <time dateTime={issue.updatedAtUtc}>{formatDate(issue.updatedAtUtc)}</time>
                  </article>
                ))}
              </div>
            </article>

            <aside className="side-panel" aria-label="Backorder detail">
              <div className="section-heading">
                <Truck aria-hidden="true" />
                <h2>Backorder detail</h2>
              </div>
              {selectedIssue ? (
                <div className="completion-stack">
                  <AuditFact label="Issue" value={selectedIssue.recordNumber} />
                  <AuditFact label="Item" value={selectedIssue.subject} />
                  <AuditFact label="Quantity" value={formatNumber.format(selectedIssue.quantity)} />
                  <AuditFact label="Location" value={selectedIssue.locationNameSnapshot} />
                  <AuditFact label="Status" value={selectedIssue.status} />
                </div>
              ) : (
                <div className="empty-state">
                  <strong>No issues</strong>
                  <span>Issued stock for truck stock and mobile operations shows up here.</span>
                </div>
              )}
            </aside>
          </section>
        )}

        {activeView === 'returns' && (
          <section className="receiving-layout" aria-label="Vendor returns">
            <article className="workflow-panel">
              <div className="section-heading">
                <Route aria-hidden="true" />
                <h2>Vendor returns</h2>
              </div>

              <div className="queue compact-queue">
                {returnRows.map((ret) => (
                  <article className="queue-row" key={ret.id}>
                    <Route aria-hidden="true" />
                    <div>
                      <div className="row-heading">
                        <h2>{ret.recordNumber}</h2>
                        <StatusChip value={ret.status} />
                      </div>
                      <p>
                        {ret.subject} · {formatNumber.format(ret.quantity)} · {ret.locationNameSnapshot}
                      </p>
                      <TagList tags={[ret.reasonCode, ret.notes]} />
                    </div>
                    <time dateTime={ret.updatedAtUtc}>{formatDate(ret.updatedAtUtc)}</time>
                  </article>
                ))}
              </div>
            </article>

            <aside className="side-panel" aria-label="Vendor return detail">
              <div className="section-heading">
                <Route aria-hidden="true" />
                <h2>Vendor return detail</h2>
              </div>
              {selectedReturn ? (
                <div className="completion-stack">
                  <AuditFact label="Return" value={selectedReturn.recordNumber} />
                  <AuditFact label="Item" value={selectedReturn.subject} />
                  <AuditFact label="Quantity" value={formatNumber.format(selectedReturn.quantity)} />
                  <AuditFact label="Location" value={selectedReturn.locationNameSnapshot} />
                  <AuditFact label="Reference" value={selectedReturn.notes} />
                </div>
              ) : (
                <div className="empty-state">
                  <strong>No returns</strong>
                  <span>Returned stock and route returns appear here.</span>
                </div>
              )}
            </aside>
          </section>
        )}

        {activeView === 'ledger' && (
          <section className="receiving-layout" aria-label="Stock ledger">
            <article className="workflow-panel">
              <div className="section-heading">
                <DatabaseZap aria-hidden="true" />
                <h2>Stock ledger</h2>
              </div>

              <div className="queue compact-queue">
                {ledgerRows.map((entry) => (
                  <article className="queue-row" key={entry.id}>
                    <DatabaseZap aria-hidden="true" />
                    <div>
                      <div className="row-heading">
                        <h2>{entry.movementType.replaceAll('_', ' ')}</h2>
                        <StatusChip value={entry.status} />
                      </div>
                      <p>
                        {entry.itemNameSnapshot} · {formatNumber.format(entry.quantity)} {entry.unitOfMeasure} ·{' '}
                        {entry.locationNameSnapshot}
                      </p>
                      <TagList tags={[entry.sourceType, entry.reasonCode]} />
                    </div>
                    <time dateTime={entry.occurredAtUtc}>{formatDate(entry.occurredAtUtc)}</time>
                  </article>
                ))}
              </div>
            </article>

            <aside className="side-panel" aria-label="Ledger detail">
              <div className="section-heading">
                <BarChart3 aria-hidden="true" />
                <h2>Ledger summary</h2>
              </div>
              <div className="completion-stack">
                <AuditFact label="Entries" value={ledgerRows.length.toString()} />
                <AuditFact label="Current balances" value={summary.inventory.length.toString()} />
                <AuditFact label="Counts" value={counts.length.toString()} />
                <AuditFact label="Adjustments" value={adjustments.length.toString()} />
              </div>
            </aside>
          </section>
        )}

        {activeView === 'discrepancies' && (
          <section className="receiving-layout" aria-label="Warehouse exceptions">
            <article className="workflow-panel">
              <div className="section-heading">
                <AlertTriangle aria-hidden="true" />
                <h2>Exceptions</h2>
              </div>

              <div className="queue compact-queue">
                {exceptionRows.map((discrepancy) => (
                  <article className="queue-row" key={discrepancy.id}>
                    <AlertTriangle aria-hidden="true" />
                    <div>
                      <div className="row-heading">
                        <h2>{discrepancy.discrepancyNumber}</h2>
                        <StatusChip value={discrepancy.status} />
                      </div>
                      <p>
                        {discrepancy.itemNameSnapshot} · {formatNumber.format(discrepancy.quantity)} ·{' '}
                        {discrepancy.locationNameSnapshot}
                      </p>
                      <TagList tags={[discrepancy.sourceType, discrepancy.reasonCode]} />
                    </div>
                    <time dateTime={discrepancy.openedAtUtc}>{formatDate(discrepancy.openedAtUtc)}</time>
                  </article>
                ))}
              </div>
            </article>

            <aside className="side-panel" aria-label="Exception detail">
              <div className="section-heading">
                <ShieldCheck aria-hidden="true" />
                <h2>Exception detail</h2>
              </div>
              {selectedException ? (
                <div className="completion-stack">
                  <AuditFact label="Discrepancy" value={selectedException.discrepancyNumber} />
                  <AuditFact label="Item" value={selectedException.itemNameSnapshot} />
                  <AuditFact label="Variance" value={formatNumber.format(selectedException.quantity)} />
                  <AuditFact label="Status" value={selectedException.status} />
                  <AuditFact label="Notes" value={selectedException.notes} />
                </div>
              ) : (
                <div className="empty-state">
                  <strong>No discrepancies</strong>
                  <span>Inventory anomalies, holds, and unexplained stock can be tracked here.</span>
                </div>
              )}
            </aside>
          </section>
        )}

        {activeView === 'settings' && (
          <section className="receiving-layout" aria-label="LoadArr admin settings">
            <article className="workflow-panel">
              <div className="section-heading">
                <Warehouse aria-hidden="true" />
                <h2>Admin settings</h2>
              </div>

              <div className="data-grid inventory-grid">
                {settingRows.map((setting) => (
                  <article className="panel" key={setting.key}>
                    <div className="panel-title-row">
                      <div>
                        <span className="kicker">{setting.key}</span>
                        <h2>{setting.label}</h2>
                      </div>
                      <StatusChip value="active" />
                    </div>
                    <p className="notes">{setting.value}</p>
                  </article>
                ))}
              </div>
            </article>

            <aside className="side-panel" aria-label="Workspace configuration summary">
              <div className="section-heading">
                <FileCheck2 aria-hidden="true" />
                <h2>Configuration</h2>
              </div>
              <div className="completion-stack">
                <AuditFact label="Product" value="LoadArr" />
                <AuditFact label="Launch URL" value={productLaunchUrls.loadarr ?? 'Not configured'} />
                <AuditFact label="Suite home" value={suiteHomeUrl} />
                <AuditFact label="Site source" value="StaffArr-owned locations" />
              </div>
            </aside>
          </section>
        )}

        {activeView === 'permissions' && (
          <section className="receiving-layout" aria-label="LoadArr permissions">
            <article className="workflow-panel">
              <div className="section-heading">
                <ShieldCheck aria-hidden="true" />
                <h2>Permission catalog</h2>
              </div>

              {permissionsQuery.isLoading ? (
                <div className="empty-state">
                  <strong>Loading permission catalog</strong>
                  <span>Fetching the canonical LoadArr permission keys from the API.</span>
                </div>
              ) : permissionsQuery.isError ? (
                <div className="empty-state">
                  <strong>Permission catalog unavailable</strong>
                  <span>
                    {permissionsQuery.error instanceof Error
                      ? permissionsQuery.error.message
                      : 'Unable to load the LoadArr permission catalog.'}
                  </span>
                </div>
              ) : (permissionsQuery.data?.permissions.length ?? 0) > 0 ? (
                <div className="data-grid inventory-grid">
                  {permissionsQuery.data!.permissions.map((permission) => (
                    <article className="panel" key={permission.permissionKey}>
                      <div className="panel-title-row">
                        <div>
                          <span className="kicker">{permission.scope} · {permission.sensitivity}</span>
                          <h2>{permission.permissionKey}</h2>
                        </div>
                        <StatusChip value={permission.status} />
                      </div>
                      <p className="notes">{permission.label}</p>
                      {permission.description ? <p className="notes">{permission.description}</p> : null}
                    </article>
                  ))}
                </div>
              ) : (
                <div className="empty-state">
                  <strong>No permissions published</strong>
                  <span>The LoadArr permission catalog is empty.</span>
                </div>
              )}
            </article>

            <aside className="side-panel" aria-label="Permission mapping summary">
              <div className="section-heading">
                <FileCheck2 aria-hidden="true" />
                <h2>Role mapping</h2>
              </div>
              <div className="completion-stack">
                <AuditFact label="Catalog source" value="LoadArr API" />
                <AuditFact label="Role authority" value="StaffArr" />
                <AuditFact label="Launch URL" value={productLaunchUrls.staffarr ?? suiteHomeUrl} />
              </div>
              <div className="empty-state">
                <strong>Read-only surface</strong>
                <span>Permission editing and role assignment remain in StaffArr. Use the link above for role mapping.</span>
              </div>
              <a className="secondary-action" href={productLaunchUrls.staffarr ?? suiteHomeUrl} rel="noreferrer" target="_blank">
                <ShieldCheck aria-hidden="true" />
                <span>Open StaffArr</span>
              </a>
            </aside>
          </section>
        )}

        {activeView === 'putaway' && (
          <section className="receiving-layout" aria-label="Putaway">
            <article className="workflow-panel">
              <div className="section-heading">
                <PackagePlus aria-hidden="true" />
                <h2>Putaway tasks</h2>
              </div>

              <div className="queue compact-queue">
                {filteredPutawayTasks.map((task) => (
                  <article className="queue-row" key={task.id}>
                    <PackagePlus aria-hidden="true" />
                    <div>
                      <div className="row-heading">
                        <h2>{task.title}</h2>
                        <StatusChip value={task.status} />
                      </div>
                      <p>
                        {task.locationNameSnapshot} · {formatNumber.format(task.quantity)} · {task.assignedRole}
                      </p>
                      <TagList tags={task.requiredSignals} />
                    </div>
                    <time dateTime={task.dueAtUtc}>{formatDate(task.dueAtUtc)}</time>
                  </article>
                ))}
              </div>
            </article>

            <aside className="side-panel" aria-label="Putaway detail">
              <div className="section-heading">
                <FileCheck2 aria-hidden="true" />
                <h2>Putaway completion</h2>
              </div>
              {receivingCompletion ? (
                <div className="completion-stack">
                  <AuditFact label="Receipt" value={receivingCompletion.session.receivingNumber} />
                  <AuditFact label="Putaway task" value={receivingCompletion.putawayTask.title} />
                  <AuditFact label="Status" value={receivingCompletion.putawayTask.status} />
                  <AuditFact label="Location" value={receivingCompletion.putawayTask.locationNameSnapshot} />
                  <AuditFact label="Signals" value={receivingCompletion.putawayTask.requiredSignals.join(' · ')} />
                </div>
              ) : selectedPutawayTask ? (
                <div className="completion-stack">
                  <AuditFact label="Task" value={selectedPutawayTask.title} />
                  <AuditFact label="Status" value={selectedPutawayTask.status} />
                  <AuditFact label="Location" value={selectedPutawayTask.locationNameSnapshot} />
                  <AuditFact label="Quantity" value={formatNumber.format(selectedPutawayTask.quantity)} />
                  <AuditFact label="Signals" value={selectedPutawayTask.requiredSignals.join(' · ')} />
                </div>
              ) : (
                <div className="empty-state">
                  <strong>No putaway completion</strong>
                  <span>Receiving completion produces the putaway task that this surface summarizes.</span>
                </div>
              )}
            </aside>
          </section>
        )}

        {activeView === 'transfers' && (
          <section className="receiving-layout" aria-label="Guided transfer workflow">
            <article className="workflow-panel">
              <div className="section-heading">
                <Route aria-hidden="true" />
                <h2>Controlled transfer</h2>
              </div>

              <div className="form-grid">
                <FormField label="Transfer type" className={fieldClassName} labelClassName={fieldLabelClassName}>
                  <ControlledSelect
                    value={transferForm.transferType}
                    onChange={(value) => updateTransferForm('transferType', value)}
                    options={transferTypeOptions}
                    className={fieldControlClassName}
                  />
                </FormField>

                <FormField label="StaffArr site" className={fieldClassName} labelClassName={fieldLabelClassName}>
                  <input
                    className={fieldControlClassName}
                    value={selectedTransferSourceLocation?.staffarrSiteNameSnapshot ?? ''}
                    readOnly
                  />
                </FormField>

                <FormField label="Reason code" className={fieldClassName} labelClassName={fieldLabelClassName}>
                  <ControlledSelect
                    value={transferForm.reasonCode}
                    onChange={(value) => updateTransferForm('reasonCode', value)}
                    options={transferReasonOptions}
                    className={fieldControlClassName}
                  />
                </FormField>

                <FormField
                  label="From StaffArr location"
                  className={fieldClassName}
                  labelClassName={fieldLabelClassName}
                >
                  <ControlledSelect
                    value={transferForm.fromLocationId}
                    onChange={(value) => updateTransferForm('fromLocationId', value)}
                    options={receivingLocationOptions}
                    className={fieldControlClassName}
                  />
                </FormField>

                <FormField
                  label="To StaffArr location"
                  className={fieldClassName}
                  labelClassName={fieldLabelClassName}
                >
                  <ControlledSelect
                    value={transferForm.toLocationId}
                    onChange={(value) => updateTransferForm('toLocationId', value)}
                    options={receivingLocationOptions}
                    className={fieldControlClassName}
                  />
                </FormField>

                <FormField label="SupplyArr item" className={fieldClassName} labelClassName={fieldLabelClassName}>
                  <ControlledSelect
                    value={transferForm.supplyarrItemId}
                    onChange={(value) => updateTransferForm('supplyarrItemId', value)}
                    options={supplyArrItemOptions}
                    className={fieldControlClassName}
                  />
                </FormField>

                <FormField label="Item name snapshot" className={fieldClassName} labelClassName={fieldLabelClassName}>
                  <input
                    className={fieldControlClassName}
                    value={selectedTransferItem?.itemNameSnapshot ?? ''}
                    readOnly
                  />
                </FormField>

                <FormField label="Available at source" className={fieldClassName} labelClassName={fieldLabelClassName}>
                  <input
                    className={fieldControlClassName}
                    value={
                      selectedTransferSourceBalance
                        ? `${formatNumber.format(selectedTransferSourceBalance.quantityOnHand)} ${selectedTransferSourceBalance.unitOfMeasureSnapshot}`
                        : 'No matching source balance'
                    }
                    readOnly
                  />
                </FormField>

                <FormField label="Transfer quantity" className={fieldClassName} labelClassName={fieldLabelClassName}>
                  <input
                    className={fieldControlClassName}
                    inputMode="decimal"
                    value={transferForm.quantity}
                    onChange={(event) => updateTransferForm('quantity', event.target.value)}
                  />
                </FormField>

                <FormField
                  label={selectedTransferItem?.isLotControlled ? 'Lot code required' : 'Lot code'}
                  className={fieldClassName}
                  labelClassName={fieldLabelClassName}
                >
                  <input
                    className={fieldControlClassName}
                    value={transferForm.lotCode}
                    onChange={(event) => updateTransferForm('lotCode', event.target.value)}
                  />
                </FormField>

                <FormField
                  label={selectedTransferItem?.isSerialControlled ? 'Serial required' : 'Serial'}
                  className={fieldClassName}
                  labelClassName={fieldLabelClassName}
                >
                  <input
                    className={fieldControlClassName}
                    value={transferForm.serialCode}
                    onChange={(event) => updateTransferForm('serialCode', event.target.value)}
                  />
                </FormField>

                <FormField
                  label="Evidence summary"
                  className={wideFieldClassName}
                  labelClassName={fieldLabelClassName}
                >
                  <textarea
                    className={fieldControlClassName}
                    value={transferForm.evidenceSummary}
                    onChange={(event) => updateTransferForm('evidenceSummary', event.target.value)}
                  />
                </FormField>
              </div>

              <button
                type="button"
                className="primary-action"
                onClick={() => void completeTransfer()}
                disabled={transferStatus === 'submitting'}
              >
                <CheckCircle2 aria-hidden="true" />
                <span>{transferStatus === 'submitting' ? 'Completing' : 'Complete transfer'}</span>
              </button>
            </article>

            <aside className="side-panel" aria-label="Transfer completion audit">
              <div className="section-heading">
                <FileCheck2 aria-hidden="true" />
                <h2>Transfer audit</h2>
              </div>

              {transferCompletion ? (
                <div className="completion-stack">
                  <AuditFact
                    label="Transfer"
                    value={`${transferCompletion.transfer.transferNumber} · ${transferCompletion.transfer.status}`}
                  />
                  <AuditFact
                    label="Movement"
                    value={`${transferCompletion.movement.movementType} · ${transferCompletion.movement.reasonCode}`}
                  />
                  <AuditFact
                    label="Source balance"
                    value={`${formatNumber.format(transferCompletion.sourceBalance.quantityOnHand)} ${transferCompletion.sourceBalance.unitOfMeasureSnapshot} at ${transferCompletion.sourceBalance.locationNameSnapshot}`}
                  />
                  <AuditFact
                    label="Destination balance"
                    value={`${formatNumber.format(transferCompletion.destinationBalance.quantityOnHand)} ${transferCompletion.destinationBalance.unitOfMeasureSnapshot} at ${transferCompletion.destinationBalance.locationNameSnapshot}`}
                  />
                  <AuditFact
                    label="Task"
                    value={`${transferCompletion.transferTask.title} · ${transferCompletion.transferTask.status}`}
                  />
                </div>
              ) : selectedTransferOrder ? (
                <div className="completion-stack">
                  <AuditFact label="Transfer" value={`${selectedTransferOrder.transferNumber} · ${selectedTransferOrder.status}`} />
                  <AuditFact
                    label="Route"
                    value={`${selectedTransferOrder.fromLocationNameSnapshot} → ${selectedTransferOrder.toLocationNameSnapshot}`}
                  />
                  <AuditFact label="Type" value={selectedTransferOrder.transferType} />
                  <AuditFact
                    label="Lines"
                    value={`${selectedTransferOrder.lines.length} line(s) · ${selectedTransferOrder.lines[0]?.itemNameSnapshot ?? 'No lines available'}`}
                  />
                  <AuditFact label="Created" value={formatDate(selectedTransferOrder.createdAtUtc)} />
                </div>
              ) : (
                <div className="empty-state">
                  <strong>Awaiting transfer</strong>
                  <span>
                    Completing transfer will create a movement and update balances across StaffArr-owned
                    locations used by LoadArr.
                  </span>
                </div>
              )}
            </aside>
          </section>
        )}

        {activeView === 'truckstock' && (
          <section className="receiving-layout" aria-label="Staging and truck stock workflow">
            <article className="workflow-panel">
              <div className="section-heading">
                <Truck aria-hidden="true" />
                <h2>Staging</h2>
              </div>

              <div className="form-grid">
                <FormField label="Truck stock record" className={fieldClassName} labelClassName={fieldLabelClassName}>
                  <ControlledSelect
                    value={truckStockForm.truckStockId}
                    onChange={(value) =>
                      setTruckStockForm((current) => ({
                        ...current,
                        truckStockId: value,
                      }))
                    }
                    options={truckStockRecords.map((record) => ({
                      value: record.id,
                      label: `${record.truckStockNumber} · ${record.itemNameSnapshot}`,
                    }))}
                    className={fieldControlClassName}
                  />
                </FormField>

                <FormField label="Truck / site" className={fieldClassName} labelClassName={fieldLabelClassName}>
                  <input className={fieldControlClassName} value={selectedTruckStock?.truckLocationNameSnapshot ?? ''} readOnly />
                </FormField>

                <FormField label="Assigned person" className={fieldClassName} labelClassName={fieldLabelClassName}>
                  <input className={fieldControlClassName} value={selectedTruckStock?.assignedPersonNameSnapshot ?? ''} readOnly />
                </FormField>

                <FormField label="Quantity" className={fieldClassName} labelClassName={fieldLabelClassName}>
                  <input
                    className={fieldControlClassName}
                    inputMode="decimal"
                    value={truckStockForm.quantity}
                    onChange={(event) => setTruckStockForm((current) => ({ ...current, quantity: event.target.value }))}
                  />
                </FormField>

                <FormField label="Reason code" className={fieldClassName} labelClassName={fieldLabelClassName}>
                  <ControlledSelect
                    value={truckStockForm.reasonCode}
                    onChange={(value) =>
                      setTruckStockForm((current) => ({
                        ...current,
                        reasonCode: value,
                      }))
                    }
                    options={transferReasonOptions}
                    className={fieldControlClassName}
                  />
                </FormField>

                <FormField label="Evidence summary" className={wideFieldClassName} labelClassName={fieldLabelClassName}>
                  <textarea
                    className={fieldControlClassName}
                    rows={3}
                    value={truckStockForm.evidenceSummary}
                    onChange={(event) =>
                      setTruckStockForm((current) => ({
                        ...current,
                        evidenceSummary: event.target.value,
                      }))
                    }
                  />
                </FormField>
              </div>

              <div className="button-row">
                <button type="button" className="primary-action" onClick={() => void performTruckStockAction('issue')} disabled={truckStockStatus === 'submitting'}>
                  <PackageCheck aria-hidden="true" />
                  <span>{truckStockStatus === 'submitting' ? 'Issuing' : 'Issue from truck'}</span>
                </button>
                <button type="button" className="secondary-action" onClick={() => void performTruckStockAction('return')} disabled={truckStockStatus === 'submitting'}>
                  <PackagePlus aria-hidden="true" />
                  <span>{truckStockStatus === 'submitting' ? 'Returning' : 'Return to truck'}</span>
                </button>
                <button type="button" className="secondary-action" onClick={() => void performTruckStockAction('count')} disabled={truckStockStatus === 'submitting'}>
                  <ClipboardCheck aria-hidden="true" />
                  <span>{truckStockStatus === 'submitting' ? 'Counting' : 'Count stock'}</span>
                </button>
              </div>
            </article>

            <aside className="side-panel" aria-label="Staging audit">
              <div className="section-heading">
                <ClipboardCheck aria-hidden="true" />
                <h2>Staging audit</h2>
              </div>

              {truckStockResult ? (
                <div className="completion-stack">
                  <AuditFact
                    label="Stock record"
                    value={`${truckStockResult.truckStock.truckStockNumber} · ${truckStockResult.truckStock.status}`}
                  />
                  <AuditFact
                    label="Quantity on hand"
                    value={`${formatNumber.format(truckStockResult.truckStock.quantityOnHand)} ${truckStockResult.truckStock.unitOfMeasure}`}
                  />
                  <AuditFact
                    label="Movement"
                    value={truckStockResult.movement ? `${truckStockResult.movement.movementType} · ${truckStockResult.movement.reasonCode}` : 'No movement created'}
                  />
                  <AuditFact
                    label="Restock task"
                    value={truckStockResult.restockTask ? `${truckStockResult.restockTask.title} · ${truckStockResult.restockTask.status}` : 'No restock task required'}
                  />
                </div>
              ) : (
                <div className="empty-state">
                  <strong>Awaiting truck stock action</strong>
                  <span>Truck stock records can be issued, returned, or counted from the mobile inventory workflow.</span>
                </div>
              )}

              <div className="queue compact-queue">
                {filteredTruckStock.map((record) => (
                  <article className="queue-row" key={record.id}>
                    <div>
                      <div className="queue-row-header">
                        <h2>{record.truckStockNumber}</h2>
                        <StatusChip value={record.status} />
                      </div>
                      <p className="queue-subtitle">
                        {record.itemNameSnapshot} · {formatNumber.format(record.quantityOnHand)} {record.unitOfMeasure}
                      </p>
                      <TagList tags={[record.truckLocationNameSnapshot, record.assignedPersonNameSnapshot, ...record.traceTags]} />
                    </div>
                    <time dateTime={record.lastMovementAtUtc}>{formatDate(record.lastMovementAtUtc)}</time>
                  </article>
                ))}
              </div>
            </aside>
          </section>
        )}

        {activeView === 'kits' && (
          <section className="receiving-layout" aria-label="Kitting workflow">
            <article className="workflow-panel">
              <div className="section-heading">
                <Boxes aria-hidden="true" />
                <h2>Kit operations</h2>
              </div>

              <div className="form-grid">
                <FormField label="Kit record" className={fieldClassName} labelClassName={fieldLabelClassName}>
                  <ControlledSelect
                    value={kitForm.kitId}
                    onChange={(value) =>
                      setKitForm((current) => ({
                        ...current,
                        kitId: value,
                      }))
                    }
                    options={kitRecords.map((record) => ({
                      value: record.id,
                      label: `${record.kitNumber} · ${record.kitNameSnapshot}`,
                    }))}
                    className={fieldControlClassName}
                  />
                </FormField>

                <FormField label="Operation" className={fieldClassName} labelClassName={fieldLabelClassName}>
                  <ControlledSelect
                    value={kitForm.operation}
                    onChange={(value) =>
                      setKitForm((current) => ({
                        ...current,
                        operation: value,
                      }))
                    }
                    options={kitOperationOptions}
                    className={fieldControlClassName}
                  />
                </FormField>

                <FormField label="Kit location" className={fieldClassName} labelClassName={fieldLabelClassName}>
                  <input className={fieldControlClassName} value={selectedKit?.locationNameSnapshot ?? ''} readOnly />
                </FormField>

                <FormField label="Assigned person" className={fieldClassName} labelClassName={fieldLabelClassName}>
                  <input className={fieldControlClassName} value={selectedKit?.assignedPersonNameSnapshot ?? ''} readOnly />
                </FormField>

                <FormField label="Quantity" className={fieldClassName} labelClassName={fieldLabelClassName}>
                  <input
                    className={fieldControlClassName}
                    inputMode="decimal"
                    value={kitForm.quantity}
                    onChange={(event) => setKitForm((current) => ({ ...current, quantity: event.target.value }))}
                  />
                </FormField>

                {(kitForm.operation === 'assign' || kitForm.operation === 'track-location') && (
                  <FormField label={kitForm.operation === 'assign' ? 'Target person' : 'Target location'} className={fieldClassName} labelClassName={fieldLabelClassName}>
                    {kitForm.operation === 'assign' ? (
                      <ControlledSelect
                        value={kitForm.targetPersonId}
                        onChange={(value) =>
                          setKitForm((current) => ({
                            ...current,
                            targetPersonId: value,
                          }))
                        }
                        options={kitPersonOptions}
                        className={fieldControlClassName}
                      />
                    ) : (
                      <ControlledSelect
                        value={kitForm.targetLocationId}
                        onChange={(value) =>
                          setKitForm((current) => ({
                            ...current,
                            targetLocationId: value,
                          }))
                        }
                        options={receivingLocationOptions}
                        className={fieldControlClassName}
                      />
                    )}
                  </FormField>
                )}

                <FormField label="Reason code" className={fieldClassName} labelClassName={fieldLabelClassName}>
                  <ControlledSelect
                    value={kitForm.reasonCode}
                    onChange={(value) =>
                      setKitForm((current) => ({
                        ...current,
                        reasonCode: value,
                      }))
                    }
                    options={kitReasonOptions}
                    className={fieldControlClassName}
                  />
                </FormField>

                <FormField label="Evidence summary" className={wideFieldClassName} labelClassName={fieldLabelClassName}>
                  <textarea
                    className={fieldControlClassName}
                    rows={3}
                    value={kitForm.evidenceSummary}
                    onChange={(event) =>
                      setKitForm((current) => ({
                        ...current,
                        evidenceSummary: event.target.value,
                      }))
                    }
                  />
                </FormField>
              </div>

              <div className="button-row">
                <button type="button" className="primary-action" onClick={() => void performKitAction()} disabled={kitStatus === 'submitting'}>
                  <PackageCheck aria-hidden="true" />
                  <span>
                    {kitStatus === 'submitting'
                      ? 'Working'
                      : kitOperationOptions.find((option) => option.value === kitForm.operation)?.label ?? 'Run kit action'}
                  </span>
                </button>
              </div>
            </article>

            <aside className="side-panel" aria-label="Kit audit">
              <div className="section-heading">
                <ClipboardCheck aria-hidden="true" />
                <h2>Kit audit</h2>
              </div>

              {kitResult ? (
                <div className="completion-stack">
                  <AuditFact
                    label="Kit record"
                    value={`${kitResult.kit.kitNumber} · ${kitResult.kit.status}`}
                  />
                  <AuditFact
                    label="Quantity on hand"
                    value={`${formatNumber.format(kitResult.kit.quantityOnHand)} ${kitResult.kit.unitOfMeasure}`}
                  />
                  <AuditFact
                    label="Movement"
                    value={kitResult.movement ? `${kitResult.movement.movementType} · ${kitResult.movement.reasonCode}` : 'No movement created'}
                  />
                  <AuditFact
                    label="Follow-up task"
                    value={kitResult.followUpTask ? `${kitResult.followUpTask.title} · ${kitResult.followUpTask.status}` : 'No follow-up task required'}
                  />
                </div>
              ) : (
                <div className="empty-state">
                  <strong>Awaiting kit action</strong>
                  <span>Kit records can be built, broken, or replenished from the mobile kitting workflow.</span>
                </div>
              )}

              <div className="queue compact-queue">
                {filteredKits.map((record) => (
                  <article className="queue-row" key={record.id}>
                    <div>
                      <div className="queue-row-header">
                        <h2>{record.kitNumber}</h2>
                        <StatusChip value={record.status} />
                      </div>
                      <p className="queue-subtitle">
                        {record.kitNameSnapshot} · {formatNumber.format(record.quantityOnHand)} {record.unitOfMeasure}
                      </p>
                      <TagList tags={[record.locationNameSnapshot, record.assignedPersonNameSnapshot, ...record.traceTags]} />
                    </div>
                    <time dateTime={record.lastActionAtUtc}>{formatDate(record.lastActionAtUtc)}</time>
                  </article>
                ))}
              </div>
            </aside>
          </section>
        )}

        {activeView === 'locations' && (
          <section className="receiving-layout" aria-label="Warehouses and areas">
            <article className="workflow-panel">
              <div className="section-heading">
                <MapPin aria-hidden="true" />
                <h2>Warehouses &amp; areas</h2>
              </div>

              <div className="form-grid single-column">
                <FormField label="Selected location" className={fieldClassName} labelClassName={fieldLabelClassName}>
                  <ControlledSelect
                    value={selectedLocationId}
                    onChange={(value) => setSelectedLocationId(value)}
                    options={receivingLocationOptions}
                    className={fieldControlClassName}
                  />
                </FormField>
              </div>

              <section className="data-grid location-grid" aria-label="Location list">
                {filteredLocations.map((location) => (
                  <article
                    className={`panel ${location.id === selectedLocationId ? 'selected-panel' : ''}`}
                    key={location.id}
                    onClick={() => setSelectedLocationId(location.id)}
                    role="button"
                    tabIndex={0}
                    onKeyDown={(event) => {
                      if (event.key === 'Enter' || event.key === ' ') {
                        event.preventDefault()
                        setSelectedLocationId(location.id)
                      }
                    }}
                  >
                    <div className="panel-title-row">
                      <div>
                        <span className="kicker">{location.staffarrSiteNameSnapshot}</span>
                        <h2>{location.name}</h2>
                      </div>
                      <StatusChip value={location.locationType} />
                    </div>
                    <p className="path">{location.path}</p>
                    <div className="capacity">
                      <div>
                        <span>Capacity</span>
                        <strong>{location.capacityPercent}%</strong>
                      </div>
                      <progress value={location.capacityPercent} max={100} />
                    </div>
                    <TagList tags={location.complianceRestrictions} />
                    <p className="notes">{location.notes}</p>
                  </article>
                ))}
              </section>
            </article>

            <aside className="side-panel" aria-label="Location utilization">
              <div className="section-heading">
                <Activity aria-hidden="true" />
                <h2>Utilization detail</h2>
              </div>

              {locationUtilization ? (
                <div className="completion-stack">
                  <AuditFact label="Selected location" value={locationUtilization.name} />
                  <AuditFact
                    label="Site"
                    value={`${locationUtilization.staffarrSiteNameSnapshot} · ${locationUtilization.staffarrSiteOrgUnitId}`}
                  />
                  <AuditFact
                    label="Inventory"
                    value={`${formatNumber.format(locationUtilization.quantityOnHand)} on hand, ${formatNumber.format(locationUtilization.quantityBlocked)} blocked`}
                  />
                  <AuditFact
                    label="Work queue"
                    value={`${locationUtilization.openTasks} task(s), ${locationUtilization.openHolds} hold(s)`}
                  />
                  <AuditFact
                    label="Unexplained"
                    value={`${locationUtilization.unexplainedInventory} record(s)`}
                  />
                  <AuditFact
                    label="Last activity"
                    value={formatDate(locationUtilization.lastActivityAtUtc)}
                  />
                  <div className="capacity">
                    <div>
                      <span>Capacity</span>
                      <strong>{locationUtilization.capacityPercent}%</strong>
                    </div>
                    <progress value={locationUtilization.capacityPercent} max={100} />
                  </div>
                  <TagList tags={[...locationUtilization.inventoryStates, ...locationUtilization.signals]} />
                  <p className="notes">{locationUtilization.notes}</p>
                </div>
              ) : (
                <div className="empty-state">
                  <strong>Awaiting utilization</strong>
                  <span>LoadArr resolves location utilization from the StaffArr-owned location and inventory state.</span>
                </div>
              )}
            </aside>
          </section>
        )}

        {activeView === 'counts' && (
          <section className="receiving-layout" aria-label="Cycle counts and adjustments">
            <article className="workflow-panel">
              <div className="section-heading">
                <Activity aria-hidden="true" />
                <h2>Cycle counts</h2>
              </div>

              <div className="queue compact-queue">
                {counts
                  .filter((count) =>
                    [
                      count.countNumber,
                      count.countType,
                      count.status,
                      count.locationNameSnapshot,
                      count.itemNameSnapshot,
                      count.reasonCode,
                    ]
                      .join(' ')
                      .toLowerCase()
                      .includes(normalizedQuery),
                  )
                  .map((count) => (
                    <article className="queue-row" key={count.id}>
                      <ClipboardCheck aria-hidden="true" />
                      <div>
                        <div className="row-heading">
                          <h2>{count.countNumber}</h2>
                          <StatusChip value={count.status} />
                        </div>
                        <p>
                          {count.itemNameSnapshot} · variance {formatNumber.format(count.varianceQuantity)}{' '}
                          {count.unitOfMeasure} · {count.locationNameSnapshot}
                        </p>
                        <TagList tags={[count.countType, count.reasonCode, count.staffarrSiteNameSnapshot]} />
                      </div>
                      <time dateTime={count.createdAtUtc}>{formatDate(count.createdAtUtc)}</time>
                    </article>
                  ))}
              </div>

              <div className="panel-divider" />

              <div className="form-grid">
                <FormField label="Count type" className={fieldClassName} labelClassName={fieldLabelClassName}>
                  <ControlledSelect
                    value={countForm.countType}
                    onChange={(value) => updateCountForm('countType', value)}
                    options={[
                      { value: 'cycle_count', label: 'Cycle count' },
                      { value: 'compliance', label: 'Compliance count' },
                      { value: 'investigation', label: 'Investigation count' },
                      { value: 'recount', label: 'Recount' },
                    ]}
                    className={fieldControlClassName}
                  />
                </FormField>

                <FormField label="StaffArr location" className={fieldClassName} labelClassName={fieldLabelClassName}>
                  <ControlledSelect
                    value={countForm.warehouseLocationId}
                    onChange={(value) => updateCountForm('warehouseLocationId', value)}
                    options={receivingLocationOptions}
                    className={fieldControlClassName}
                  />
                </FormField>

                <FormField label="SupplyArr item" className={fieldClassName} labelClassName={fieldLabelClassName}>
                  <ControlledSelect
                    value={countForm.supplyarrItemId}
                    onChange={(value) => updateCountForm('supplyarrItemId', value)}
                    options={supplyArrItemOptions}
                    className={fieldControlClassName}
                  />
                </FormField>

                <FormField label="Expected quantity" className={fieldClassName} labelClassName={fieldLabelClassName}>
                  <input
                    className={fieldControlClassName}
                    inputMode="decimal"
                    value={countForm.expectedQuantity}
                    onChange={(event) => updateCountForm('expectedQuantity', event.target.value)}
                  />
                </FormField>

                <FormField label="Counted quantity" className={fieldClassName} labelClassName={fieldLabelClassName}>
                  <input
                    className={fieldControlClassName}
                    inputMode="decimal"
                    value={countForm.countedQuantity}
                    onChange={(event) => updateCountForm('countedQuantity', event.target.value)}
                  />
                </FormField>

                <FormField label="Counted by" className={fieldClassName} labelClassName={fieldLabelClassName}>
                  <input
                    className={fieldControlClassName}
                    value={countForm.countedByPersonId}
                    onChange={(event) => updateCountForm('countedByPersonId', event.target.value)}
                  />
                </FormField>

                <FormField label="Reason code" className={fieldClassName} labelClassName={fieldLabelClassName}>
                  <ControlledSelect
                    value={countForm.reasonCode}
                    onChange={(value) => updateCountForm('reasonCode', value)}
                    options={[
                      { value: 'cycle_count_variance', label: 'Cycle count variance' },
                      { value: 'paperwork_mismatch', label: 'Paperwork mismatch' },
                      { value: 'inventory_damage', label: 'Inventory damage' },
                      { value: 'migration_correction', label: 'Migration correction' },
                    ]}
                    className={fieldControlClassName}
                  />
                </FormField>

                <FormField label="Evidence summary" className={wideFieldClassName} labelClassName={fieldLabelClassName}>
                  <textarea
                    className={fieldControlClassName}
                    value={countForm.evidenceSummary}
                    onChange={(event) => updateCountForm('evidenceSummary', event.target.value)}
                  />
                </FormField>
              </div>

              <button type="button" className="primary-action" onClick={() => void performCount()} disabled={countStatus === 'submitting'}>
                <CheckCircle2 aria-hidden="true" />
                <span>{countStatus === 'submitting' ? 'Recording' : 'Record count'}</span>
              </button>
            </article>

            <aside className="side-panel" aria-label="Count approval and adjustment detail">
              <div className="section-heading">
                <FileCheck2 aria-hidden="true" />
                <h2>Variance review</h2>
              </div>

              {countResult || selectedCountRecord ? (
                <div className="completion-stack">
                  <AuditFact
                    label="Count"
                    value={`${(countResult?.count ?? selectedCountRecord).countNumber} · ${(countResult?.count ?? selectedCountRecord).status}`}
                  />
                  <AuditFact
                    label="Variance"
                    value={`${formatNumber.format((countResult?.count ?? selectedCountRecord).varianceQuantity)} ${(countResult?.count ?? selectedCountRecord).unitOfMeasure}`}
                  />
                  <AuditFact
                    label="Location"
                    value={(countResult?.count ?? selectedCountRecord).locationNameSnapshot}
                  />
                  <AuditFact
                    label="Adjustment"
                    value={
                      countResult?.adjustment
                        ? `${countResult.adjustment.adjustmentNumber} · ${countResult.adjustment.status}`
                        : selectedAdjustmentRecord
                          ? `${selectedAdjustmentRecord.adjustmentNumber} · ${selectedAdjustmentRecord.status}`
                          : 'No adjustment yet'
                    }
                  />
                  <AuditFact
                    label="Movement"
                    value={countResult?.movement ? `${countResult.movement.movementType} · ${countResult.movement.reasonCode}` : 'No movement yet'}
                  />
                </div>
              ) : (
                <div className="empty-state">
                  <strong>Awaiting count</strong>
                  <span>Recording a count creates the audit record that variance approval can resolve into an adjustment.</span>
                </div>
              )}

              <div className="panel-divider" />

              <button
                type="button"
                className="secondary-action"
                onClick={() => void approveCountVariance()}
                disabled={countStatus === 'submitting' || !countResult || countResult.count.varianceQuantity === 0}
              >
                <ShieldCheck aria-hidden="true" />
                <span>{countStatus === 'submitting' ? 'Approving' : 'Approve variance'}</span>
              </button>

              <div className="panel-divider" />

              <div className="section-heading">
                <Activity aria-hidden="true" />
                <h2>Adjustments</h2>
              </div>

              <div className="queue compact-queue">
                {adjustments
                  .filter((adjustment) =>
                    [
                      adjustment.adjustmentNumber,
                      adjustment.adjustmentType,
                      adjustment.status,
                      adjustment.locationNameSnapshot,
                      adjustment.itemNameSnapshot,
                      adjustment.reasonCode,
                    ]
                      .join(' ')
                      .toLowerCase()
                      .includes(normalizedQuery),
                  )
                  .map((adjustment) => (
                    <article className="queue-row" key={adjustment.id}>
                      <ClipboardCheck aria-hidden="true" />
                      <div>
                        <div className="row-heading">
                          <h2>{adjustment.adjustmentNumber}</h2>
                          <StatusChip value={adjustment.status} />
                        </div>
                        <p>
                          {adjustment.itemNameSnapshot} · delta {formatNumber.format(adjustment.quantityDelta)}{' '}
                          {adjustment.unitOfMeasure} · {adjustment.locationNameSnapshot}
                        </p>
                        <TagList tags={[adjustment.adjustmentType, adjustment.reasonCode, adjustment.staffarrSiteNameSnapshot]} />
                      </div>
                      <time dateTime={adjustment.createdAtUtc}>{formatDate(adjustment.createdAtUtc)}</time>
                    </article>
                  ))}
              </div>

              <div className="panel-divider" />

              <div className="form-grid single-column">
                <FormField label="Adjustment type" className={fieldClassName} labelClassName={fieldLabelClassName}>
                  <ControlledSelect
                    value={adjustmentForm.adjustmentType}
                    onChange={(value) => updateAdjustmentForm('adjustmentType', value)}
                    options={[
                      { value: 'gain', label: 'Gain' },
                      { value: 'loss', label: 'Loss' },
                      { value: 'status_correction', label: 'Status correction' },
                      { value: 'condition_correction', label: 'Condition correction' },
                      { value: 'unit_of_measure_correction', label: 'UOM correction' },
                      { value: 'migration_correction', label: 'Migration correction' },
                    ]}
                    className={fieldControlClassName}
                  />
                </FormField>

                <FormField label="StaffArr location" className={fieldClassName} labelClassName={fieldLabelClassName}>
                  <ControlledSelect
                    value={adjustmentForm.warehouseLocationId}
                    onChange={(value) => updateAdjustmentForm('warehouseLocationId', value)}
                    options={receivingLocationOptions}
                    className={fieldControlClassName}
                  />
                </FormField>

                <FormField label="SupplyArr item" className={fieldClassName} labelClassName={fieldLabelClassName}>
                  <ControlledSelect
                    value={adjustmentForm.supplyarrItemId}
                    onChange={(value) => updateAdjustmentForm('supplyarrItemId', value)}
                    options={supplyArrItemOptions}
                    className={fieldControlClassName}
                  />
                </FormField>

                <FormField label="Quantity delta" className={fieldClassName} labelClassName={fieldLabelClassName}>
                  <input
                    className={fieldControlClassName}
                    inputMode="decimal"
                    value={adjustmentForm.quantityDelta}
                    onChange={(event) => updateAdjustmentForm('quantityDelta', event.target.value)}
                  />
                </FormField>

                <FormField label="Created by" className={fieldClassName} labelClassName={fieldLabelClassName}>
                  <input
                    className={fieldControlClassName}
                    value={adjustmentForm.createdByPersonId}
                    onChange={(event) => updateAdjustmentForm('createdByPersonId', event.target.value)}
                  />
                </FormField>

                <FormField label="Reason code" className={fieldClassName} labelClassName={fieldLabelClassName}>
                  <ControlledSelect
                    value={adjustmentForm.reasonCode}
                    onChange={(value) => updateAdjustmentForm('reasonCode', value)}
                    options={[
                      { value: 'cycle_count_variance', label: 'Cycle count variance' },
                      { value: 'migration_correction', label: 'Migration correction' },
                      { value: 'condition_correction', label: 'Condition correction' },
                      { value: 'inventory_damage', label: 'Inventory damage' },
                    ]}
                    className={fieldControlClassName}
                  />
                </FormField>

                <FormField label="Evidence summary" className={wideFieldClassName} labelClassName={fieldLabelClassName}>
                  <textarea
                    className={fieldControlClassName}
                    value={adjustmentForm.evidenceSummary}
                    onChange={(event) => updateAdjustmentForm('evidenceSummary', event.target.value)}
                  />
                </FormField>
              </div>

              <button
                type="button"
                className="primary-action"
                onClick={() => void createAdjustment()}
                disabled={adjustmentStatus === 'submitting'}
              >
                <CheckCircle2 aria-hidden="true" />
                <span>{adjustmentStatus === 'submitting' ? 'Creating' : 'Create adjustment'}</span>
              </button>

              <div className="panel-divider" />

              <button
                type="button"
                className="secondary-action"
                onClick={() => void approveAdjustment()}
                disabled={adjustmentStatus === 'submitting' || (!adjustmentResult && adjustments.length === 0)}
              >
                <ShieldCheck aria-hidden="true" />
                <span>{adjustmentStatus === 'submitting' ? 'Approving' : 'Approve adjustment'}</span>
              </button>
            </aside>
          </section>
        )}

        {activeView === 'adjustments' && (
          <section className="receiving-layout" aria-label="Adjustment history">
            <article className="workflow-panel">
              <div className="section-heading">
                <ClipboardCheck aria-hidden="true" />
                <h2>Adjustment history</h2>
              </div>

              <div className="queue compact-queue">
                {adjustments.map((adjustment) => (
                  <article className="queue-row" key={adjustment.id}>
                    <ClipboardCheck aria-hidden="true" />
                    <div>
                      <div className="row-heading">
                        <h2>{adjustment.adjustmentNumber}</h2>
                        <StatusChip value={adjustment.status} />
                      </div>
                      <p>
                        {adjustment.itemNameSnapshot} · {formatNumber.format(adjustment.quantityDelta)}{' '}
                        {adjustment.unitOfMeasure} · {adjustment.locationNameSnapshot}
                      </p>
                      <TagList tags={[adjustment.adjustmentType, adjustment.reasonCode, adjustment.staffarrSiteNameSnapshot]} />
                    </div>
                    <time dateTime={adjustment.updatedAtUtc}>{formatDate(adjustment.updatedAtUtc)}</time>
                  </article>
                ))}
              </div>
            </article>

            <aside className="side-panel" aria-label="Adjustment detail">
              <div className="section-heading">
                <FileCheck2 aria-hidden="true" />
                <h2>Adjustment detail</h2>
              </div>
              {selectedAdjustmentRecord ? (
                <div className="completion-stack">
                  <AuditFact label="Adjustment" value={selectedAdjustmentRecord.adjustmentNumber} />
                  <AuditFact label="Item" value={selectedAdjustmentRecord.itemNameSnapshot} />
                  <AuditFact label="Quantity" value={formatNumber.format(selectedAdjustmentRecord.quantityDelta)} />
                  <AuditFact label="Status" value={selectedAdjustmentRecord.status} />
                  <AuditFact label="Reason" value={selectedAdjustmentRecord.reasonCode} />
                </div>
              ) : (
                <div className="empty-state">
                  <strong>No adjustments</strong>
                  <span>Approval-ready inventory adjustments will appear here.</span>
                </div>
              )}
            </aside>
          </section>
        )}

        {activeView === 'replenishment' && (
          <section className="receiving-layout" aria-label="Supply coordination">
            <article className="workflow-panel">
              <div className="section-heading">
                <Truck aria-hidden="true" />
                <h2>Supply coordination</h2>
              </div>

              <div className="queue compact-queue">
                {replenishmentRows.map((record, index) => (
                  <article
                    className="queue-row"
                    key={`${index}-${'truckStockNumber' in record ? record.truckStockNumber : 'kitNumber' in record ? record.kitNumber : record.targetReference}`}
                  >
                    <Truck aria-hidden="true" />
                    <div>
                      <div className="row-heading">
                        <h2>
                          {'truckStockNumber' in record
                            ? record.truckStockNumber
                            : 'kitNumber' in record
                              ? record.kitNumber
                              : record.targetReference}
                        </h2>
                        <StatusChip value={'status' in record ? record.status : 'ready'} />
                      </div>
                      <p>
                        {'quantityOnHand' in record
                          ? 'truckStockNumber' in record
                            ? `${record.itemNameSnapshot} · ${formatNumber.format(record.quantityOnHand)} on hand`
                            : `${record.kitNameSnapshot} · ${formatNumber.format(record.quantityOnHand)} on hand`
                          : `${record.locationNameSnapshot} · ${formatNumber.format(record.quantity)} handoff quantity`}
                      </p>
                      <TagList
                        tags={
                          'traceTags' in record
                            ? [...record.traceTags]
                            : 'notes' in record
                              ? [record.notes]
                              : ['route_handoff']
                        }
                      />
                    </div>
                  </article>
                ))}
              </div>
            </article>

            <aside className="side-panel" aria-label="Supply coordination detail">
              <div className="section-heading">
                <PackagePlus aria-hidden="true" />
                <h2>Supply coordination summary</h2>
              </div>
              <div className="completion-stack">
                <AuditFact label="Truck stock rows" value={truckStockRecords.filter((record) => record.quantityOnHand < record.minimumQuantity).length.toString()} />
                <AuditFact label="Kit rows" value={kitRecords.filter((record) => record.quantityOnHand < record.minimumQuantity).length.toString()} />
                <AuditFact label="Route handoffs" value={summary.routeHandoffs.filter((handoff) => handoff.status !== 'ready').length.toString()} />
              </div>
            </aside>
          </section>
        )}

        {activeView === 'history' && (
          <ReportsPanel summary={summary} counts={counts} adjustments={adjustments} />
        )}

        {activeView === 'tasks' && (
          <section className="queue" aria-label="Dock schedule and warehouse tasks">
            <div className="section-heading">
              <ClipboardList aria-hidden="true" />
              <h2>Dock schedule</h2>
            </div>
            {filteredTasks.map((task) => (
              <article className="queue-row" key={task.id}>
                <ClipboardList aria-hidden="true" />
                <div>
                  <div className="row-heading">
                    <h2>{task.title}</h2>
                    <StatusChip value={task.priority} />
                  </div>
                  <p>
                    {task.taskType} · {task.status} · {task.quantity} units · {task.locationNameSnapshot}
                  </p>
                  <TagList tags={[task.assignedRole, task.supplyarrItemId, ...task.requiredSignals]} />
                </div>
                <time dateTime={task.dueAtUtc}>{formatDate(task.dueAtUtc)}</time>
              </article>
            ))}
          </section>
        )}

        {activeView === 'holds' && (
          <section className="split-layout" aria-label="Holds and quarantine">
            <div className="queue">
              {filteredHolds.map((hold) => (
                <article className="queue-row" key={hold.id}>
                  <ShieldCheck aria-hidden="true" />
                  <div>
                    <div className="row-heading">
                      <h2>{hold.supplyarrItemId}</h2>
                      <StatusChip value={hold.status} />
                    </div>
                    <p>{hold.reason}</p>
                    <TagList tags={[hold.holdType, hold.locationNameSnapshot, hold.sourceReference]} />
                  </div>
                  <time dateTime={hold.openedAtUtc}>{formatDate(hold.openedAtUtc)}</time>
                </article>
              ))}
            </div>
            <aside className="side-panel" aria-label="Create inventory hold">
              <div className="section-heading">
                <ShieldCheck aria-hidden="true" />
                <h2>Create hold</h2>
              </div>
              {selectedHoldRecord ? (
                <div className="completion-stack">
                  <AuditFact label="Hold" value={selectedHoldRecord.holdType} />
                  <AuditFact label="Item" value={selectedHoldRecord.supplyarrItemId} />
                  <AuditFact label="Location" value={selectedHoldRecord.locationNameSnapshot} />
                  <AuditFact label="Status" value={selectedHoldRecord.status} />
                  <AuditFact label="Reason" value={selectedHoldRecord.reason} />
                </div>
              ) : null}
              <div className="form-grid single-column">
                <FormField label="Hold type" className={fieldClassName} labelClassName={fieldLabelClassName}>
                  <ControlledSelect
                    value={holdForm.holdType}
                    onChange={(value) => updateHoldForm('holdType', value)}
                    options={holdTypeOptions}
                    className={fieldControlClassName}
                  />
                </FormField>

                <FormField label="Reason code" className={fieldClassName} labelClassName={fieldLabelClassName}>
                  <ControlledSelect
                    value={holdForm.reasonCode}
                    onChange={(value) => updateHoldForm('reasonCode', value)}
                    options={holdReasonOptions}
                    className={fieldControlClassName}
                  />
                </FormField>

                <FormField
                  label="StaffArr location"
                  className={fieldClassName}
                  labelClassName={fieldLabelClassName}
                >
                  <ControlledSelect
                    value={holdForm.warehouseLocationId}
                    onChange={(value) => updateHoldForm('warehouseLocationId', value)}
                    options={receivingLocationOptions}
                    className={fieldControlClassName}
                  />
                </FormField>

                <FormField label="StaffArr site" className={fieldClassName} labelClassName={fieldLabelClassName}>
                  <input
                    className={fieldControlClassName}
                    value={selectedHoldLocation?.staffarrSiteNameSnapshot ?? ''}
                    readOnly
                  />
                </FormField>

                <FormField label="SupplyArr item" className={fieldClassName} labelClassName={fieldLabelClassName}>
                  <ControlledSelect
                    value={holdForm.supplyarrItemId}
                    onChange={(value) => updateHoldForm('supplyarrItemId', value)}
                    options={supplyArrItemOptions}
                    className={fieldControlClassName}
                  />
                </FormField>

                <FormField label="Available at location" className={fieldClassName} labelClassName={fieldLabelClassName}>
                  <input
                    className={fieldControlClassName}
                    value={
                      selectedHoldBalance
                        ? `${formatNumber.format(selectedHoldBalance.quantityOnHand)} ${selectedHoldBalance.unitOfMeasureSnapshot}`
                        : 'No matching balance'
                    }
                    readOnly
                  />
                </FormField>

                <FormField label="Hold quantity" className={fieldClassName} labelClassName={fieldLabelClassName}>
                  <input
                    className={fieldControlClassName}
                    inputMode="decimal"
                    value={holdForm.quantity}
                    onChange={(event) => updateHoldForm('quantity', event.target.value)}
                  />
                </FormField>

                <FormField label="Description" className={fieldClassName} labelClassName={fieldLabelClassName}>
                  <textarea
                    className={fieldControlClassName}
                    value={holdForm.description}
                    onChange={(event) => updateHoldForm('description', event.target.value)}
                  />
                </FormField>

                <FormField label="Evidence summary" className={fieldClassName} labelClassName={fieldLabelClassName}>
                  <textarea
                    className={fieldControlClassName}
                    value={holdForm.evidenceSummary}
                    onChange={(event) => updateHoldForm('evidenceSummary', event.target.value)}
                  />
                </FormField>
              </div>

              <button
                type="button"
                className="primary-action"
                onClick={() => void createHold()}
                disabled={holdStatus === 'submitting'}
              >
                <CheckCircle2 aria-hidden="true" />
                <span>{holdStatus === 'submitting' ? 'Creating' : 'Create hold'}</span>
              </button>

              <div className="panel-divider" />

              <div className="section-heading">
                <CheckCircle2 aria-hidden="true" />
                <h2>Release hold</h2>
              </div>
              <div className="form-grid single-column">
                <FormField label="Existing hold" className={fieldClassName} labelClassName={fieldLabelClassName}>
                  <ControlledSelect
                    value={holdReleaseForm.holdId}
                    onChange={(value) => updateHoldReleaseForm('holdId', value)}
                    options={holdOptions}
                    className={fieldControlClassName}
                  />
                </FormField>

                <FormField
                  label="StaffArr location"
                  className={fieldClassName}
                  labelClassName={fieldLabelClassName}
                >
                  <input
                    className={fieldControlClassName}
                    value={selectedReleaseHold?.locationNameSnapshot ?? ''}
                    readOnly
                  />
                </FormField>

                <FormField label="Release reason" className={fieldClassName} labelClassName={fieldLabelClassName}>
                  <ControlledSelect
                    value={holdReleaseForm.reasonCode}
                    onChange={(value) => updateHoldReleaseForm('reasonCode', value)}
                    options={holdReleaseReasonOptions}
                    className={fieldControlClassName}
                  />
                </FormField>

                <FormField label="Released by" className={fieldClassName} labelClassName={fieldLabelClassName}>
                  <input
                    className={fieldControlClassName}
                    value={holdReleaseForm.releasedByPersonId}
                    onChange={(event) => updateHoldReleaseForm('releasedByPersonId', event.target.value)}
                  />
                </FormField>

                <FormField label="Evidence summary" className={fieldClassName} labelClassName={fieldLabelClassName}>
                  <textarea
                    className={fieldControlClassName}
                    value={holdReleaseForm.evidenceSummary}
                    onChange={(event) => updateHoldReleaseForm('evidenceSummary', event.target.value)}
                  />
                </FormField>
              </div>

              <button
                type="button"
                className="secondary-action"
                onClick={() => void releaseHold()}
                disabled={holdReleaseStatus === 'submitting' || !selectedReleaseHold}
              >
                <CheckCircle2 aria-hidden="true" />
                <span>{holdReleaseStatus === 'submitting' ? 'Releasing' : 'Release hold'}</span>
              </button>

              {holdMutation ? (
                <div className="completion-stack">
                  <AuditFact
                    label="Hold action"
                    value={`${holdMutation.hold.holdNumber} · ${holdMutation.hold.status} · ${holdMutation.hold.reasonCode}`}
                  />
                  <AuditFact
                    label="Movement"
                    value={`${holdMutation.movement.movementType} · ${holdMutation.movement.reasonCode}`}
                  />
                  <AuditFact
                    label="Blocked balance"
                    value={`${formatNumber.format(holdMutation.balance.quantityBlocked)} ${holdMutation.balance.unitOfMeasureSnapshot} blocked at ${holdMutation.balance.locationNameSnapshot}`}
                  />
                </div>
              ) : (
                <div className="empty-state">
                  <strong>Awaiting hold</strong>
                  <span>Creating a hold blocks quantity and records a warehouse movement audit.</span>
                </div>
              )}
            </aside>
          </section>
        )}

        {activeView === 'unexplained' && (
          <section className="receiving-layout" aria-label="Unexplained inventory workflow">
            <article className="workflow-panel">
              <div className="section-heading">
                <AlertTriangle aria-hidden="true" />
                <h2>Unexplained inventory</h2>
              </div>

              <div className="queue compact-queue">
                {filteredUnexplainedInventory.map((record) => (
                  <article className="queue-row" key={record.id}>
                    <AlertTriangle aria-hidden="true" />
                    <div>
                      <div className="row-heading">
                        <h2>{record.recordNumber}</h2>
                        <StatusChip value={record.status} />
                      </div>
                      <p>
                        {record.itemNameSnapshot} · variance {formatNumber.format(record.varianceQuantity)}{' '}
                        {record.unitOfMeasure} · {record.locationNameSnapshot}
                      </p>
                      <TagList
                        tags={[
                          record.discoverySource,
                          record.resolutionState,
                          record.supplyarrItemId,
                          record.staffarrSiteNameSnapshot,
                        ]}
                      />
                    </div>
                    <time dateTime={record.discoveredAtUtc}>{formatDate(record.discoveredAtUtc)}</time>
                  </article>
                ))}
              </div>

              <div className="panel-divider" />

              <div className="form-grid">
                <FormField label="Discovery source" className={fieldClassName} labelClassName={fieldLabelClassName}>
                  <ControlledSelect
                    value={unexplainedForm.discoverySource}
                    onChange={(value) => updateUnexplainedForm('discoverySource', value)}
                    options={unexplainedDiscoverySourceOptions}
                    className={fieldControlClassName}
                  />
                </FormField>

                <FormField label="StaffArr location" className={fieldClassName} labelClassName={fieldLabelClassName}>
                  <ControlledSelect
                    value={unexplainedForm.warehouseLocationId}
                    onChange={(value) => updateUnexplainedForm('warehouseLocationId', value)}
                    options={receivingLocationOptions}
                    className={fieldControlClassName}
                  />
                </FormField>

                <FormField label="StaffArr site" className={fieldClassName} labelClassName={fieldLabelClassName}>
                  <input
                    className={fieldControlClassName}
                    value={selectedUnexplainedLocation?.staffarrSiteNameSnapshot ?? ''}
                    readOnly
                  />
                </FormField>

                <FormField label="SupplyArr item" className={fieldClassName} labelClassName={fieldLabelClassName}>
                  <ControlledSelect
                    value={unexplainedForm.supplyarrItemId}
                    onChange={(value) => updateUnexplainedForm('supplyarrItemId', value)}
                    options={supplyArrItemOptions}
                    className={fieldControlClassName}
                  />
                </FormField>

                <FormField label="Expected quantity" className={fieldClassName} labelClassName={fieldLabelClassName}>
                  <input
                    className={fieldControlClassName}
                    inputMode="decimal"
                    value={unexplainedForm.expectedQuantity}
                    onChange={(event) => updateUnexplainedForm('expectedQuantity', event.target.value)}
                  />
                </FormField>

                <FormField label="Found quantity" className={fieldClassName} labelClassName={fieldLabelClassName}>
                  <input
                    className={fieldControlClassName}
                    inputMode="decimal"
                    value={unexplainedForm.quantity}
                    onChange={(event) => updateUnexplainedForm('quantity', event.target.value)}
                  />
                </FormField>

                <FormField label="Reason code" className={fieldClassName} labelClassName={fieldLabelClassName}>
                  <ControlledSelect
                    value={unexplainedForm.reasonCode}
                    onChange={(value) => updateUnexplainedForm('reasonCode', value)}
                    options={unexplainedReasonOptions}
                    className={fieldControlClassName}
                  />
                </FormField>

                <FormField label="Discovered by" className={fieldClassName} labelClassName={fieldLabelClassName}>
                  <input
                    className={fieldControlClassName}
                    value={unexplainedForm.discoveredByPersonId}
                    onChange={(event) => updateUnexplainedForm('discoveredByPersonId', event.target.value)}
                  />
                </FormField>

                <FormField label="Resolution guard" className={fieldClassName} labelClassName={fieldLabelClassName}>
                  <input className={fieldControlClassName} value="Not trusted available until resolved" readOnly />
                </FormField>

                <FormField
                  label="Evidence summary"
                  className={wideFieldClassName}
                  labelClassName={fieldLabelClassName}
                >
                  <textarea
                    className={fieldControlClassName}
                    value={unexplainedForm.evidenceSummary}
                    onChange={(event) => updateUnexplainedForm('evidenceSummary', event.target.value)}
                  />
                </FormField>
              </div>

              <button
                type="button"
                className="primary-action"
                onClick={() => void createUnexplainedInventory()}
                disabled={unexplainedStatus === 'submitting'}
              >
                <AlertTriangle aria-hidden="true" />
                <span>{unexplainedStatus === 'submitting' ? 'Recording' : 'Record unexplained inventory'}</span>
              </button>
            </article>

            <aside className="side-panel" aria-label="Unexplained inventory resolution">
              <div className="section-heading">
                <FileCheck2 aria-hidden="true" />
                <h2>Resolution</h2>
              </div>

              <div className="form-grid single-column">
                <FormField label="Queue record" className={fieldClassName} labelClassName={fieldLabelClassName}>
                  <ControlledSelect
                    value={unexplainedResolutionForm.recordId}
                    onChange={(value) => updateUnexplainedResolutionForm('recordId', value)}
                    options={unexplainedRecordOptions}
                    className={fieldControlClassName}
                  />
                </FormField>

                <FormField label="Current state" className={fieldClassName} labelClassName={fieldLabelClassName}>
                  <input
                    className={fieldControlClassName}
                    value={selectedUnexplainedRecord?.resolutionState ?? ''}
                    readOnly
                  />
                </FormField>

                <FormField label="Reason code" className={fieldClassName} labelClassName={fieldLabelClassName}>
                  <ControlledSelect
                    value={unexplainedResolutionForm.reasonCode}
                    onChange={(value) => updateUnexplainedResolutionForm('reasonCode', value)}
                    options={unexplainedResolutionReasonOptions}
                    className={fieldControlClassName}
                  />
                </FormField>

                <FormField label="Reviewer" className={fieldClassName} labelClassName={fieldLabelClassName}>
                  <input
                    className={fieldControlClassName}
                    value={unexplainedResolutionForm.personId}
                    onChange={(event) => updateUnexplainedResolutionForm('personId', event.target.value)}
                  />
                </FormField>

                <FormField label="Quarantine location" className={fieldClassName} labelClassName={fieldLabelClassName}>
                  <ControlledSelect
                    value={unexplainedResolutionForm.quarantineLocationId}
                    onChange={(value) => updateUnexplainedResolutionForm('quarantineLocationId', value)}
                    options={quarantineLocationOptions.length > 0 ? quarantineLocationOptions : receivingLocationOptions}
                    className={fieldControlClassName}
                  />
                </FormField>

                <FormField label="Evidence summary" className={fieldClassName} labelClassName={fieldLabelClassName}>
                  <textarea
                    className={fieldControlClassName}
                    value={unexplainedResolutionForm.evidenceSummary}
                    onChange={(event) => updateUnexplainedResolutionForm('evidenceSummary', event.target.value)}
                  />
                </FormField>
              </div>

              <div className="action-row">
                <button
                  type="button"
                  className="primary-action"
                  onClick={() => void mutateUnexplainedInventory('resolve')}
                  disabled={unexplainedStatus === 'submitting' || !selectedUnexplainedRecord}
                >
                  <CheckCircle2 aria-hidden="true" />
                  <span>Resolve</span>
                </button>
                <button
                  type="button"
                  className="secondary-action"
                  onClick={() => void mutateUnexplainedInventory('quarantine')}
                  disabled={unexplainedStatus === 'submitting' || !selectedUnexplainedRecord}
                >
                  <ShieldCheck aria-hidden="true" />
                  <span>Quarantine</span>
                </button>
                <button
                  type="button"
                  className="secondary-action"
                  onClick={() => void mutateUnexplainedInventory('scrap')}
                  disabled={unexplainedStatus === 'submitting' || !selectedUnexplainedRecord}
                >
                  <AlertTriangle aria-hidden="true" />
                  <span>Scrap</span>
                </button>
              </div>

              {unexplainedMutation ? (
                <div className="completion-stack">
                  <AuditFact
                    label="Record"
                    value={`${unexplainedMutation.record.recordNumber} · ${unexplainedMutation.record.status} · ${unexplainedMutation.record.resolutionState}`}
                  />
                  <AuditFact
                    label="Origin event"
                    value={
                      unexplainedMutation.originEvent
                        ? `${unexplainedMutation.originEvent.originType} · ${unexplainedMutation.originEvent.id}`
                        : 'No trusted origin created'
                    }
                  />
                  <AuditFact
                    label="Movement"
                    value={
                      unexplainedMutation.movement
                        ? `${unexplainedMutation.movement.movementType} · ${unexplainedMutation.movement.reasonCode}`
                        : 'Awaiting resolution movement'
                    }
                  />
                  <AuditFact
                    label="Task"
                    value={
                      unexplainedMutation.reviewTask
                        ? `${unexplainedMutation.reviewTask.title} · ${unexplainedMutation.reviewTask.status}`
                        : 'Resolution complete'
                    }
                  />
                </div>
              ) : (
                <div className="empty-state">
                  <strong>Awaiting resolution</strong>
                  <span>Unexplained stock remains unavailable until approval, quarantine, or scrap is recorded.</span>
                </div>
              )}
            </aside>
          </section>
        )}

        {activeView === 'handoffs' && (
          <section className="receiving-layout" aria-label="Route and product handoffs">
            <article className="workflow-panel">
              <div className="section-heading">
                <Route aria-hidden="true" />
                <h2>Route and product handoffs</h2>
              </div>

              <div className="data-grid handoff-grid">
                {filteredHandoffs.map((handoff) => (
                  <article className="panel" key={handoff.id}>
                    <div className="panel-title-row">
                      <div>
                        <span className="kicker">{handoff.targetProduct}</span>
                        <h2>{handoff.targetReference}</h2>
                      </div>
                      <StatusChip value={handoff.status} />
                    </div>
                    <div className="handoff-route">
                      <Route aria-hidden="true" />
                      <span>{handoff.locationNameSnapshot}</span>
                    </div>
                    <dl className="quantity-grid compact">
                      <Quantity label="Quantity" value={handoff.quantity} />
                    </dl>
                    <p className="notes">{handoff.notes}</p>
                  </article>
                ))}
              </div>
            </article>

            <aside className="side-panel" aria-label="Handoff detail">
              <div className="section-heading">
                <FileCheck2 aria-hidden="true" />
                <h2>Handoff detail</h2>
              </div>
              {selectedHandoff ? (
                <div className="completion-stack">
                  <AuditFact label="Target" value={`${selectedHandoff.targetProduct} · ${selectedHandoff.targetReference}`} />
                  <AuditFact label="Location" value={selectedHandoff.locationNameSnapshot} />
                  <AuditFact label="Quantity" value={formatNumber.format(selectedHandoff.quantity)} />
                  <AuditFact label="Status" value={selectedHandoff.status} />
                  <AuditFact label="Notes" value={selectedHandoff.notes} />
                </div>
              ) : (
                <div className="empty-state">
                  <strong>No handoffs</strong>
                  <span>Route handoffs show transfers between LoadArr and RoutArr or SupplyArr follow-up paths.</span>
                </div>
              )}
            </aside>
          </section>
        )}

        <footer className="workspace-footer">
          <Activity aria-hidden="true" />
          <span>Generated {formatDate(summary.generatedAt)}</span>
        </footer>
      </section>
      </div>
    </ProductWorkspaceFrame>
  )
}

function Metric({
  icon: Icon,
  label,
  value,
  tone = 'neutral',
}: {
  icon: typeof Boxes
  label: string
  value: number
  tone?: 'neutral' | 'warning'
}) {
  return (
    <article className={`metric ${tone}`}>
      <Icon aria-hidden="true" />
      <div>
        <span>{label}</span>
        <strong>{formatNumber.format(value)}</strong>
      </div>
    </article>
  )
}

function Quantity({ label, value }: { label: string; value: number }) {
  return (
    <div>
      <dt>{label}</dt>
      <dd>{formatNumber.format(value)}</dd>
    </div>
  )
}

function StatusChip({ value }: { value: string }) {
  return <span className={`chip ${value.toLowerCase().replace(/[^a-z0-9]+/g, '-')}`}>{value.replaceAll('_', ' ')}</span>
}

function TagList({ tags }: { tags: string[] }) {
  return (
    <div className="tags">
      {tags.map((tag) => (
        <span key={tag}>{tag}</span>
      ))}
    </div>
  )
}

function AuditFact({ label, value }: { label: string; value: string }) {
  return (
    <div className="audit-fact">
      <span>{label}</span>
      <strong>{value}</strong>
    </div>
  )
}

function toReceivingPayload(form: ReceivingFormState) {
  return {
    receivingType: form.receivingType,
    sourceProductKey: form.sourceProductKey,
    sourceObjectType: form.sourceObjectType,
    sourceObjectId: form.sourceObjectId,
    supplierNameSnapshot: form.supplierNameSnapshot,
    completedByPersonId: form.completedByPersonId,
    supplyarrItemId: form.supplyarrItemId,
    expectedQuantity: toPositiveNumber(form.expectedQuantity),
    receivedQuantity: toPositiveNumber(form.receivedQuantity),
    warehouseLocationId: form.warehouseLocationId,
    lotCode: form.lotCode || null,
    serialCode: form.serialCode || null,
    condition: form.condition,
    discrepancyReasonCode: form.discrepancyReasonCode || null,
    complianceEvaluationId: form.complianceEvaluationId || null,
    evidenceSummary: form.evidenceSummary || null,
  }
}

function toTransferPayload(form: TransferFormState) {
  return {
    transferType: form.transferType,
    fromLocationId: form.fromLocationId,
    toLocationId: form.toLocationId,
    completedByPersonId: form.completedByPersonId,
    supplyarrItemId: form.supplyarrItemId,
    quantity: toPositiveNumber(form.quantity),
    lotCode: form.lotCode || null,
    serialCode: form.serialCode || null,
    reasonCode: form.reasonCode,
    complianceEvaluationId: form.complianceEvaluationId || null,
    evidenceSummary: form.evidenceSummary || null,
  }
}

function toHoldPayload(form: HoldFormState) {
  return {
    holdType: form.holdType,
    warehouseLocationId: form.warehouseLocationId,
    supplyarrItemId: form.supplyarrItemId,
    quantity: toPositiveNumber(form.quantity),
    reasonCode: form.reasonCode,
    description: form.description,
    createdByPersonId: form.createdByPersonId,
    complianceEvaluationId: form.complianceEvaluationId || null,
    evidenceSummary: form.evidenceSummary || null,
  }
}

function toHoldReleasePayload(form: HoldReleaseFormState) {
  return {
    releasedByPersonId: form.releasedByPersonId,
    reasonCode: form.reasonCode,
    evidenceSummary: form.evidenceSummary || null,
  }
}

function toUnexplainedInventoryPayload(form: UnexplainedInventoryFormState) {
  return {
    discoverySource: form.discoverySource,
    warehouseLocationId: form.warehouseLocationId,
    supplyarrItemId: form.supplyarrItemId,
    expectedQuantity: toNonNegativeNumber(form.expectedQuantity),
    quantity: toPositiveNumber(form.quantity),
    lotCode: form.lotCode || null,
    serialCode: form.serialCode || null,
    discoveredByPersonId: form.discoveredByPersonId,
    reasonCode: form.reasonCode,
    evidenceSummary: form.evidenceSummary,
    complianceEvaluationId: form.complianceEvaluationId || null,
  }
}

function toUnexplainedResolutionPayload(
  form: UnexplainedInventoryResolutionFormState,
  action: 'resolve' | 'quarantine' | 'scrap',
) {
  if (action === 'resolve') {
    return {
      approvedByPersonId: form.personId,
      reasonCode: form.reasonCode,
      complianceEvaluationId: form.complianceEvaluationId || null,
      evidenceSummary: form.evidenceSummary || null,
    }
  }

  if (action === 'quarantine') {
    return {
      quarantineLocationId: form.quarantineLocationId,
      quarantinedByPersonId: form.personId,
      reasonCode: form.reasonCode,
      evidenceSummary: form.evidenceSummary || null,
    }
  }

  return {
    scrappedByPersonId: form.personId,
    reasonCode: form.reasonCode,
    evidenceSummary: form.evidenceSummary || null,
  }
}

function toCountCreatePayload(form: CountFormState) {
  return {
    countType: form.countType,
    staffarrSiteOrgUnitId: '',
    warehouseLocationId: form.warehouseLocationId,
    supplyarrItemId: form.supplyarrItemId,
    expectedQuantity: toNonNegativeNumber(form.expectedQuantity),
    countedByPersonId: form.countedByPersonId,
    reasonCode: form.reasonCode,
    evidenceSummary: form.evidenceSummary || null,
  }
}

function toCountCompletionPayload(form: CountFormState) {
  return {
    countType: form.countType,
    staffarrSiteOrgUnitId: '',
    warehouseLocationId: form.warehouseLocationId,
    supplyarrItemId: form.supplyarrItemId,
    expectedQuantity: toNonNegativeNumber(form.expectedQuantity),
    countedQuantity: toNonNegativeNumber(form.countedQuantity),
    countedByPersonId: form.countedByPersonId,
    reasonCode: form.reasonCode,
    evidenceSummary: form.evidenceSummary || null,
    complianceEvaluationId: null,
  }
}

function toAdjustmentCreatePayload(form: AdjustmentFormState) {
  return {
    adjustmentType: form.adjustmentType,
    staffarrSiteOrgUnitId: '',
    warehouseLocationId: form.warehouseLocationId,
    supplyarrItemId: form.supplyarrItemId,
    quantityDelta: toSignedNumber(form.quantityDelta),
    createdByPersonId: form.createdByPersonId,
    reasonCode: form.reasonCode,
    evidenceSummary: form.evidenceSummary || null,
  }
}

function createLocalReceivingCompletion(
  form: ReceivingFormState,
  location: LoadArrLocation | undefined,
  item: SupplyArrItemReference | undefined,
): LoadArrReceivingCompletion {
  const receivedQuantity = toPositiveNumber(form.receivedQuantity)
  const locationSnapshot = requireLocalValue(location)
  const itemSnapshot = requireLocalValue(item)
  const locationName = locationSnapshot.name
  const now = Date.now().toString(36)

  return {
    session: {
      receivingNumber: `RCV-${now.toUpperCase()}`,
      status: 'completed',
    },
    originEvent: {
      id: `origin-${now}`,
      originType: form.receivingType === 'manual' ? 'purchase_receipt' : form.receivingType,
      supplyarrItemId: itemSnapshot.supplyarrItemId,
      quantity: receivedQuantity,
      unitOfMeasure: itemSnapshot.unitOfMeasureSnapshot,
      locationNameSnapshot: locationName,
    },
    movement: {
      id: `move-${now}`,
      movementType: 'receive',
      reasonCode: 'manual_receiving_complete',
    },
    balance: {
      id: `bal-${now}`,
      supplyarrItemId: itemSnapshot.supplyarrItemId,
      itemNameSnapshot: itemSnapshot.itemNameSnapshot,
      unitOfMeasureSnapshot: itemSnapshot.unitOfMeasureSnapshot,
      state: 'available',
      locationId: form.warehouseLocationId,
      locationNameSnapshot: locationName,
      quantityOnHand: receivedQuantity,
      quantityReserved: 0,
      quantityAllocated: 0,
      quantityBlocked: 0,
      originEventType: form.receivingType === 'manual' ? 'purchase_receipt' : form.receivingType,
      originReference: form.sourceObjectId,
      traceTags: [`receiving:${now}`, `origin:origin-${now}`],
      notes: 'Created from local receiving completion preview',
    },
    putawayTask: {
      id: `task-${now}`,
      taskType: 'putaway',
      title: `Put away ${itemSnapshot.itemNameSnapshot}`,
      priority: 'normal',
      status: 'ready',
      locationNameSnapshot: locationName,
      assignedRole: 'Warehouse Associate',
      supplyarrItemId: itemSnapshot.supplyarrItemId,
      quantity: receivedQuantity,
      dueAtUtc: new Date(Date.now() + 4 * 60 * 60 * 1000).toISOString(),
      requiredSignals: ['origin_event_created', 'movement_recorded', 'location_scan_required'],
    },
  }
}

function createLocalTransferCompletion(
  form: TransferFormState,
  fromLocation: LoadArrLocation | undefined,
  toLocation: LoadArrLocation | undefined,
  item: SupplyArrItemReference | undefined,
  sourceBalance: LoadArrInventoryBalance | undefined,
): LoadArrTransferCompletion {
  const quantity = toPositiveNumber(form.quantity)
  const itemSnapshot = requireLocalValue(item)
  const fromLocationSnapshot = requireLocalValue(fromLocation)
  const toLocationSnapshot = requireLocalValue(toLocation)
  const fromLocationName = fromLocationSnapshot.name
  const toLocationName = toLocationSnapshot.name
  const sourceQuantity = Math.max(0, (sourceBalance?.quantityOnHand ?? quantity) - quantity)
  const now = Date.now().toString(36)

  return {
    transfer: {
      transferNumber: `TRF-${now.toUpperCase()}`,
      status: 'completed',
    },
    movement: {
      id: `move-${now}`,
      movementType: 'transfer',
      reasonCode: form.reasonCode,
    },
    sourceBalance: {
      id: sourceBalance?.id ?? `bal-source-${now}`,
      supplyarrItemId: itemSnapshot.supplyarrItemId,
      itemNameSnapshot: itemSnapshot.itemNameSnapshot,
      unitOfMeasureSnapshot: itemSnapshot.unitOfMeasureSnapshot,
      state: sourceBalance?.state ?? 'available',
      locationId: form.fromLocationId,
      locationNameSnapshot: fromLocationName,
      quantityOnHand: sourceQuantity,
      quantityReserved: sourceBalance?.quantityReserved ?? 0,
      quantityAllocated: sourceBalance?.quantityAllocated ?? 0,
      quantityBlocked: sourceBalance?.quantityBlocked ?? 0,
      originEventType: sourceBalance?.originEventType ?? 'purchase_receipt',
      originReference: sourceBalance?.originReference ?? 'Existing trusted LoadArr balance',
      traceTags: [...(sourceBalance?.traceTags ?? []), `transfer-out:${now}`],
      notes: `Transferred ${quantity} ${itemSnapshot.unitOfMeasureSnapshot} to ${toLocationName}`,
    },
    destinationBalance: {
      id: `bal-destination-${now}`,
      supplyarrItemId: itemSnapshot.supplyarrItemId,
      itemNameSnapshot: itemSnapshot.itemNameSnapshot,
      unitOfMeasureSnapshot: itemSnapshot.unitOfMeasureSnapshot,
      state: 'available',
      locationId: form.toLocationId,
      locationNameSnapshot: toLocationName,
      quantityOnHand: quantity,
      quantityReserved: 0,
      quantityAllocated: 0,
      quantityBlocked: 0,
      originEventType: sourceBalance?.originEventType ?? 'purchase_receipt',
      originReference: sourceBalance?.originReference ?? 'Existing trusted LoadArr balance',
      traceTags: [`transfer-in:${now}`, `movement:move-${now}`],
      notes: 'Created from controlled transfer across StaffArr-owned locations',
    },
    transferTask: {
      id: `task-${now}`,
      taskType: 'transfer',
      title: `Move ${itemSnapshot.itemNameSnapshot} to ${toLocationName}`,
      priority: 'normal',
      status: 'completed',
      locationNameSnapshot: toLocationName,
      assignedRole: 'Warehouse Associate',
      supplyarrItemId: itemSnapshot.supplyarrItemId,
      quantity,
      dueAtUtc: new Date().toISOString(),
      requiredSignals: ['source_scan_required', 'destination_scan_required', 'movement_recorded'],
    },
  }
}

function createLocalReceivingSessionRecord(
  form: ReceivingFormState,
  location: LoadArrLocation | undefined,
  item: SupplyArrItemReference | undefined,
  completion: LoadArrReceivingCompletion,
): LoadArrReceivingSession {
  const now = Date.now().toString(36)
  const itemSnapshot = requireLocalValue(item)
  const locationSnapshot = requireLocalValue(location)
  const quantity = toPositiveNumber(form.receivedQuantity) || completion.balance.quantityOnHand
  const timestamp = new Date().toISOString()

  return {
    id: `recv-${now}`,
    receivingNumber: completion.session.receivingNumber,
    receivingType: form.receivingType,
    status: completion.session.status,
    staffarrSiteOrgUnitId: locationSnapshot.staffarrSiteOrgUnitId,
    staffarrSiteNameSnapshot: locationSnapshot.staffarrSiteNameSnapshot,
    sourceProductKey: form.sourceProductKey,
    sourceObjectType: form.sourceObjectType,
    sourceObjectId: form.sourceObjectId,
    supplierNameSnapshot: form.supplierNameSnapshot,
    startedByPersonId: form.completedByPersonId,
    completedByPersonId: form.completedByPersonId,
    startedAtUtc: timestamp,
    completedAtUtc: timestamp,
    lines: [
      {
        id: `line-${now}`,
        supplyarrItemId: itemSnapshot.supplyarrItemId,
        itemNameSnapshot: itemSnapshot.itemNameSnapshot,
        expectedQuantity: toNonNegativeNumber(form.expectedQuantity) || quantity,
        receivedQuantity: quantity,
        unitOfMeasure: itemSnapshot.unitOfMeasureSnapshot,
        warehouseLocationId: form.warehouseLocationId,
        locationNameSnapshot: locationSnapshot.name,
        lotCode: form.lotCode || null,
        serialCode: form.serialCode || null,
        condition: form.condition,
        status: completion.putawayTask.status,
        discrepancyReasonCode: form.discrepancyReasonCode || null,
        evidenceSummary: form.evidenceSummary || null,
      },
    ],
  }
}

function createLocalTransferOrderRecord(
  form: TransferFormState,
  fromLocation: LoadArrLocation | undefined,
  toLocation: LoadArrLocation | undefined,
  item: SupplyArrItemReference | undefined,
  completion: LoadArrTransferCompletion,
): LoadArrTransferOrder {
  const now = Date.now().toString(36)
  const itemSnapshot = requireLocalValue(item)
  const fromLocationSnapshot = requireLocalValue(fromLocation)
  const toLocationSnapshot = requireLocalValue(toLocation)
  const timestamp = new Date().toISOString()

  return {
    id: `xfer-${now}`,
    transferNumber: completion.transfer.transferNumber,
    status: completion.transfer.status,
    transferType: form.transferType,
    staffarrSiteOrgUnitId: fromLocationSnapshot.staffarrSiteOrgUnitId,
    staffarrSiteNameSnapshot: fromLocationSnapshot.staffarrSiteNameSnapshot,
    fromLocationId: form.fromLocationId,
    fromLocationNameSnapshot: fromLocationSnapshot.name,
    toLocationId: form.toLocationId,
    toLocationNameSnapshot: toLocationSnapshot.name,
    requestedByPersonId: form.completedByPersonId,
    completedByPersonId: form.completedByPersonId,
    reasonCode: form.reasonCode,
    createdAtUtc: timestamp,
    completedAtUtc: timestamp,
    lines: [
      {
        id: `xfer-line-${now}`,
        supplyarrItemId: itemSnapshot.supplyarrItemId,
        itemNameSnapshot: itemSnapshot.itemNameSnapshot,
        quantity: toPositiveNumber(form.quantity) || completion.destinationBalance.quantityOnHand,
        unitOfMeasure: itemSnapshot.unitOfMeasureSnapshot,
        lotCode: form.lotCode || null,
        serialCode: form.serialCode || null,
        status: completion.transferTask.status,
      },
    ],
  }
}

function createLocalTruckStockMutation(
  action: 'issue' | 'return' | 'count',
  form: TruckStockFormState,
  record: LoadArrTruckStock | undefined,
): LoadArrTruckStockMutation {
  const current = requireLocalValue(record)
  const quantity = toPositiveNumber(form.quantity) || 1
  const now = Date.now().toString(36)
  const timestamp = new Date().toISOString()

  const nextQuantity =
    action === 'issue'
      ? Math.max(0, current.quantityOnHand - quantity)
      : action === 'return'
        ? current.quantityOnHand + quantity
        : quantity

  const status =
    nextQuantity === 0
      ? 'empty'
      : nextQuantity < current.minimumQuantity
        ? 'low_stock'
        : nextQuantity > current.maximumQuantity
          ? 'overflow'
          : 'ready'

  const truckStock: LoadArrTruckStock = {
    ...current,
    quantityOnHand: nextQuantity,
    status,
    lastCountedAtUtc: action === 'count' ? timestamp : current.lastCountedAtUtc,
    lastMovementAtUtc: timestamp,
    notes:
      action === 'issue'
        ? `Issued ${quantity} ${current.unitOfMeasure} from truck stock.`
        : action === 'return'
          ? `Returned ${quantity} ${current.unitOfMeasure} to truck stock.`
          : `Counted at ${timestamp}.`,
    traceTags: [...current.traceTags, `truck_stock:${action}:${now}`],
  }

  return {
    truckStock,
    movement: {
      id: `move-${now}`,
      movementType: `truck_stock_${action}`,
      reasonCode: form.reasonCode,
    },
    restockTask:
      status === 'low_stock'
        ? {
            id: `task-${now}`,
            taskType: 'replenish',
            title: `Restock ${current.itemNameSnapshot} on ${current.truckStockNumber}`,
            priority: 'normal',
            status: 'ready',
            locationNameSnapshot: current.truckLocationNameSnapshot,
            assignedRole: 'Truck Stock User',
            supplyarrItemId: current.supplyarrItemId,
            quantity: Math.max(0, current.minimumQuantity - nextQuantity),
            dueAtUtc: new Date(Date.now() + 2 * 60 * 60 * 1000).toISOString(),
            requiredSignals: ['truck_stock_low', 'restock_requested'],
      }
        : null,
  }
}

function createLocalKitMutation(
  action: KitOperation,
  form: KitFormState,
  record: LoadArrKit | undefined,
  targetPersonNameSnapshot?: string,
  targetLocationNameSnapshot?: string,
): LoadArrKitMutation {
  const current = requireLocalValue(record)
  const quantity = toPositiveNumber(form.quantity) || 1
  const now = Date.now().toString(36)
  const timestamp = new Date().toISOString()

  const nextQuantity =
    action === 'break' || action === 'pick' || action === 'reserve'
      ? Math.max(0, current.quantityOnHand - quantity)
      : action === 'expire-components'
        ? 0
        : action === 'return' || action === 'replenish' || action === 'build'
          ? current.quantityOnHand + quantity
          : current.quantityOnHand

  const status =
    action === 'assign'
      ? 'assigned'
      : action === 'track-location'
        ? 'tracked'
        : action === 'inspect'
          ? (current.quantityOnHand < current.minimumQuantity ? 'needs_replenishment' : 'inspected')
          : action === 'expire-components'
            ? 'expired'
            : action === 'reserve'
              ? (nextQuantity < current.minimumQuantity ? 'needs_replenishment' : 'reserved')
              : action === 'pick'
                ? (nextQuantity < current.minimumQuantity ? 'needs_replenishment' : 'picked')
                : action === 'return'
                  ? (nextQuantity < current.minimumQuantity ? 'needs_replenishment' : 'returned')
                  : action === 'replenish'
                    ? (nextQuantity < current.minimumQuantity ? 'needs_replenishment' : 'built')
                    : action === 'build'
                      ? (nextQuantity < current.minimumQuantity ? 'needs_replenishment' : 'built')
                      : action === 'break'
                        ? (nextQuantity === 0 ? 'broken' : nextQuantity < current.minimumQuantity ? 'needs_replenishment' : 'built')
                        : 'tracked'

  const kit: LoadArrKit = {
    ...current,
    quantityOnHand: nextQuantity,
    status,
    lastActionAtUtc: timestamp,
    lastMovementAtUtc: timestamp,
    notes:
      action === 'build'
        ? `Built ${quantity} kit(s) from LoadArr components.`
        : action === 'break'
          ? `Broke down ${quantity} kit(s) for component recovery.`
          : action === 'replenish'
            ? `Replenished ${quantity} kit(s) at the warehouse.`
            : action === 'reserve'
              ? `Reserved ${quantity} kit(s) for controlled use.`
              : action === 'pick'
                ? `Picked ${quantity} kit(s) from controlled stock.`
                : action === 'inspect'
                  ? `Inspected by ${form.personId} for readiness and condition.`
                  : action === 'assign'
                    ? `Assigned kit to ${targetPersonNameSnapshot ?? form.targetPersonId}.`
                    : action === 'return'
                      ? `Returned ${quantity} kit(s) to stock.`
                      : action === 'expire-components'
                        ? 'Expired kit components as of controlled review.'
                        : `Tracked kit location to ${targetLocationNameSnapshot ?? current.locationNameSnapshot}.`,
    traceTags: [...current.traceTags, `kit:${action}:${now}`],
    assignedPersonId:
      action === 'assign' ? form.targetPersonId : current.assignedPersonId,
    assignedPersonNameSnapshot:
      action === 'assign' ? (targetPersonNameSnapshot ?? form.targetPersonId) : current.assignedPersonNameSnapshot,
    locationId:
      action === 'track-location' ? (form.targetLocationId || current.locationId) : current.locationId,
    locationNameSnapshot:
      action === 'track-location' ? (targetLocationNameSnapshot ?? current.locationNameSnapshot) : current.locationNameSnapshot,
  }

  return {
    kit,
    movement: {
      id: `move-${now}`,
      movementType: `kit_${action}`,
      reasonCode: form.reasonCode,
    },
    followUpTask:
      status === 'needs_replenishment'
        ? {
            id: `task-${now}`,
            taskType: 'replenish',
            title: `Replenish ${current.kitNameSnapshot}`,
            priority: 'normal',
            status: 'ready',
            locationNameSnapshot: current.locationNameSnapshot,
            assignedRole: 'Kit Coordinator',
            supplyarrItemId: current.primaryItemId,
            quantity: Math.max(0, current.minimumQuantity - nextQuantity),
            dueAtUtc: new Date(Date.now() + 2 * 60 * 60 * 1000).toISOString(),
            requiredSignals: ['kit_low', 'replenish_requested'],
          }
        : null,
  }
}

function createLocalHoldMutation(
  form: HoldFormState,
  location: LoadArrLocation | undefined,
  item: SupplyArrItemReference | undefined,
  balance: LoadArrInventoryBalance | undefined,
): LoadArrHoldMutation {
  const quantity = toPositiveNumber(form.quantity)
  const itemSnapshot = requireLocalValue(item)
  const locationSnapshot = requireLocalValue(location)
  const locationName = locationSnapshot.name
  const now = Date.now().toString(36)
  const blockedQuantity = (balance?.quantityBlocked ?? 0) + quantity

  return {
    hold: {
      holdNumber: `HLD-${now.toUpperCase()}`,
      status: 'open',
      reasonCode: form.reasonCode,
      quantity,
      unitOfMeasure: itemSnapshot.unitOfMeasureSnapshot,
      locationNameSnapshot: locationName,
    },
    movement: {
      id: `move-${now}`,
      movementType: 'hold',
      reasonCode: form.reasonCode,
    },
    balance: {
      id: balance?.id ?? `bal-hold-${now}`,
      supplyarrItemId: itemSnapshot.supplyarrItemId,
      itemNameSnapshot: itemSnapshot.itemNameSnapshot,
      unitOfMeasureSnapshot: itemSnapshot.unitOfMeasureSnapshot,
      state: balance?.state ?? 'available',
      locationId: form.warehouseLocationId,
      locationNameSnapshot: locationName,
      quantityOnHand: balance?.quantityOnHand ?? quantity,
      quantityReserved: balance?.quantityReserved ?? 0,
      quantityAllocated: balance?.quantityAllocated ?? 0,
      quantityBlocked: blockedQuantity,
      originEventType: balance?.originEventType ?? 'purchase_receipt',
      originReference: balance?.originReference ?? 'Existing trusted LoadArr balance',
      traceTags: [...(balance?.traceTags ?? []), `hold:${now}`, `movement:move-${now}`],
      notes: `Held ${quantity} ${itemSnapshot.unitOfMeasureSnapshot}: ${form.reasonCode}`,
    },
  }
}

function createLocalHoldReleaseMutation(
  form: HoldReleaseFormState,
  hold: LoadArrHold | undefined,
  balance: LoadArrInventoryBalance | undefined,
): LoadArrHoldMutation {
  const now = Date.now().toString(36)
  const holdSnapshot = requireLocalValue(hold)
  const balanceSnapshot = requireLocalValue(balance)
  const heldQuantity = Math.max(0, balanceSnapshot.quantityBlocked)
  const releasedQuantity = heldQuantity || 1
  const itemId = holdSnapshot.supplyarrItemId
  const locationName = holdSnapshot.locationNameSnapshot

  return {
    hold: {
      holdNumber: `HLD-${now.toUpperCase()}`,
      status: 'released',
      reasonCode: form.reasonCode,
      quantity: releasedQuantity,
      unitOfMeasure: balanceSnapshot.unitOfMeasureSnapshot,
      locationNameSnapshot: locationName,
    },
    movement: {
      id: `move-${now}`,
      movementType: 'release_hold',
      reasonCode: form.reasonCode,
    },
    balance: {
      id: balanceSnapshot.id,
      supplyarrItemId: itemId,
      itemNameSnapshot: balanceSnapshot.itemNameSnapshot,
      unitOfMeasureSnapshot: balanceSnapshot.unitOfMeasureSnapshot,
      state: balanceSnapshot.state,
      locationId: balanceSnapshot.locationId,
      locationNameSnapshot: locationName,
      quantityOnHand: balanceSnapshot.quantityOnHand,
      quantityReserved: balanceSnapshot.quantityReserved,
      quantityAllocated: balanceSnapshot.quantityAllocated,
      quantityBlocked: Math.max(0, heldQuantity - releasedQuantity),
      originEventType: balanceSnapshot.originEventType,
      originReference: balanceSnapshot.originReference,
      traceTags: [...balanceSnapshot.traceTags, `released-hold:${form.holdId}`, `movement:move-${now}`],
      notes: `Released hold ${form.holdId}: ${form.reasonCode}`,
    },
  }
}

function createLocalUnexplainedInventoryMutation(
  form: UnexplainedInventoryFormState,
  location: LoadArrLocation | undefined,
  item: SupplyArrItemReference | undefined,
): LoadArrUnexplainedInventoryMutation {
  const now = Date.now().toString(36)
  const itemSnapshot = requireLocalValue(item)
  const locationSnapshot = requireLocalValue(location)
  const foundQuantity = toPositiveNumber(form.quantity)
  const expectedQuantity = toNonNegativeNumber(form.expectedQuantity)
  const varianceQuantity = foundQuantity - expectedQuantity

  return {
    record: {
      id: `unexplained-${now}`,
      recordNumber: `UNX-${now.toUpperCase()}`,
      status: varianceQuantity > 0 ? 'needs_approval' : 'needs_review',
      discoverySource: form.discoverySource,
      staffarrSiteOrgUnitId: locationSnapshot.staffarrSiteOrgUnitId,
      staffarrSiteNameSnapshot: locationSnapshot.staffarrSiteNameSnapshot,
      warehouseLocationId: form.warehouseLocationId,
      locationNameSnapshot: locationSnapshot.name,
      supplyarrItemId: itemSnapshot.supplyarrItemId,
      itemNameSnapshot: itemSnapshot.itemNameSnapshot,
      expectedQuantity,
      quantity: foundQuantity,
      varianceQuantity,
      unitOfMeasure: itemSnapshot.unitOfMeasureSnapshot,
      lotCode: form.lotCode || null,
      serialCode: form.serialCode || null,
      discoveredByPersonId: form.discoveredByPersonId,
      reasonCode: form.reasonCode,
      evidenceSummary: form.evidenceSummary,
      complianceEvaluationId: form.complianceEvaluationId || null,
      resolutionState: 'not_trusted_available',
      discoveredAtUtc: new Date().toISOString(),
      resolvedAtUtc: null,
    },
    originEvent: null,
    movement: null,
    reviewTask: {
      id: `task-${now}`,
      taskType: 'unexplained_inventory_review',
      title: `Resolve unexplained ${itemSnapshot.itemNameSnapshot}`,
      priority: 'urgent',
      status: 'ready',
      locationNameSnapshot: locationSnapshot.name,
      assignedRole: 'Inventory Supervisor',
      supplyarrItemId: itemSnapshot.supplyarrItemId,
      quantity: foundQuantity,
      dueAtUtc: new Date(Date.now() + 2 * 60 * 60 * 1000).toISOString(),
      requiredSignals: ['approval_required', 'origin_unknown', 'stock_not_available'],
    },
  }
}

function createLocalUnexplainedResolutionMutation(
  form: UnexplainedInventoryResolutionFormState,
  record: LoadArrUnexplainedInventoryRecord | undefined,
  action: 'resolve' | 'quarantine' | 'scrap',
): LoadArrUnexplainedInventoryMutation {
  const now = Date.now().toString(36)
  const baseRecord = requireLocalValue(record)
  const status =
    action === 'resolve' ? 'resolved_valid_stock' : action === 'scrap' ? 'resolved_scrap' : 'needs_quarantine'
  const resolutionState =
    action === 'resolve' ? 'trusted_available' : action === 'scrap' ? 'scrapped' : 'quarantined_untrusted'
  const movementType = action === 'resolve' ? 'adjust' : action
  const resolvedRecord = {
    ...baseRecord,
    status,
    resolutionState,
    resolvedAtUtc: action === 'quarantine' ? null : new Date().toISOString(),
  }

  return {
    record: resolvedRecord,
    originEvent:
      action === 'resolve'
        ? {
            id: `origin-${now}`,
            originType: 'unexplained_inventory_resolution',
            supplyarrItemId: baseRecord.supplyarrItemId,
            quantity: baseRecord.quantity,
            unitOfMeasure: baseRecord.unitOfMeasure,
            locationNameSnapshot: baseRecord.locationNameSnapshot,
          }
        : null,
    movement: {
      id: `move-${now}`,
      movementType,
      reasonCode: form.reasonCode,
    },
    reviewTask: null,
  }
}

function createLocalLocationUtilization(
  location: LoadArrLocation,
  workspace: LoadArrWorkspaceSummary,
): LoadArrLocationUtilization {
  const inventory = workspace.inventory.filter((item) => item.locationId === location.id)
  const tasks = workspace.tasks.filter((task) => task.locationNameSnapshot === location.name)
  const holds = workspace.holds.filter((hold) => hold.locationNameSnapshot === location.name)
  const unexplained = workspace.unexplainedInventory.filter((record) => record.warehouseLocationId === location.id)

  return {
    id: location.id,
    name: location.name,
    staffarrSiteOrgUnitId: location.staffarrSiteOrgUnitId,
    staffarrSiteNameSnapshot: location.staffarrSiteNameSnapshot,
    locationType: location.locationType,
    active: location.active,
    capacityPercent: location.capacityPercent,
    quantityOnHand: inventory.reduce((total, item) => total + item.quantityOnHand, 0),
    quantityBlocked: inventory.reduce((total, item) => total + item.quantityBlocked, 0),
    openTasks: tasks.length,
    openHolds: holds.length,
    unexplainedInventory: unexplained.length,
    itemCount: inventory.length,
    inventoryStates: [...new Set(inventory.map((item) => item.state))],
    signals: [
      location.complianceRestrictions.length > 0
        ? location.complianceRestrictions.join(', ')
        : 'no restrictions',
      `${tasks.length} task(s) open`,
      `${holds.length} hold(s) active`,
    ],
    notes: location.notes,
    lastActivityAtUtc:
      [...workspace.evidence.map((item) => item.capturedAtUtc), ...holds.map((hold) => hold.openedAtUtc), ...unexplained.map((record) => record.discoveredAtUtc)]
        .sort((left, right) => right.localeCompare(left))[0] ?? workspace.generatedAt,
  }
}

function createLocalCountCompletion(
  form: CountFormState,
  location: LoadArrLocation | undefined,
  selectedCount: LoadArrCount | undefined,
  items: SupplyArrItemReference[],
): LoadArrCountCompletion {
  const locationSnapshot = requireLocalValue(location)
  const itemSnapshot = requireLocalValue(items.find((item) => item.supplyarrItemId === form.supplyarrItemId))
  const expectedQuantity = toNonNegativeNumber(form.expectedQuantity) || selectedCount?.expectedQuantity || 0
  const countedQuantity = toNonNegativeNumber(form.countedQuantity)
  const varianceQuantity = countedQuantity - expectedQuantity
  const now = Date.now().toString(36)
  const timestamp = new Date().toISOString()
  const count: LoadArrCount = {
    id: selectedCount?.id ?? `count-${now}`,
    countNumber: selectedCount?.countNumber ?? `CNT-${now.toUpperCase()}`,
    status: varianceQuantity === 0 ? 'completed' : 'variance_pending_approval',
    countType: form.countType,
    staffarrSiteOrgUnitId: locationSnapshot.staffarrSiteOrgUnitId,
    staffarrSiteNameSnapshot: locationSnapshot.staffarrSiteNameSnapshot,
    warehouseLocationId: locationSnapshot.id,
    locationNameSnapshot: locationSnapshot.name,
    supplyarrItemId: itemSnapshot.supplyarrItemId,
    itemNameSnapshot: itemSnapshot.itemNameSnapshot,
    expectedQuantity,
    countedQuantity,
    varianceQuantity,
    unitOfMeasure: itemSnapshot.unitOfMeasureSnapshot,
    countedByPersonId: form.countedByPersonId,
    approvedByPersonId: null,
    reasonCode: form.reasonCode,
    inventoryAdjustmentId: varianceQuantity === 0 ? null : `adj-${now}`,
    evidenceSummary: form.evidenceSummary,
    createdAtUtc: timestamp,
    completedAtUtc: timestamp,
    approvedAtUtc: null,
    updatedAtUtc: timestamp,
  }

  return {
    count,
    adjustment: null,
    originEvent: null,
    movement: null,
  }
}

function createLocalCountVarianceApproval(result: LoadArrCountCompletion): LoadArrCountCompletion {
  const now = Date.now().toString(36)
  const approvedAtUtc = new Date().toISOString()
  const adjustmentType = result.count.varianceQuantity > 0 ? 'gain' : 'loss'
  const adjustment: LoadArrAdjustment = {
    id: result.count.inventoryAdjustmentId ?? `adj-${now}`,
    adjustmentNumber: `ADJ-${now.toUpperCase()}`,
    status: 'approved',
    adjustmentType,
    staffarrSiteOrgUnitId: result.count.staffarrSiteOrgUnitId,
    staffarrSiteNameSnapshot: result.count.staffarrSiteNameSnapshot,
    warehouseLocationId: result.count.warehouseLocationId,
    locationNameSnapshot: result.count.locationNameSnapshot,
    supplyarrItemId: result.count.supplyarrItemId,
    itemNameSnapshot: result.count.itemNameSnapshot,
    quantityDelta: result.count.varianceQuantity,
    unitOfMeasure: result.count.unitOfMeasure,
    reasonCode: result.count.reasonCode,
    createdByPersonId: result.count.countedByPersonId,
    approvedByPersonId: result.count.countedByPersonId,
    inventoryOriginEventId: result.count.varianceQuantity > 0 ? `origin-${now}` : null,
    evidenceSummary: result.count.evidenceSummary,
    createdAtUtc: approvedAtUtc,
    approvedAtUtc,
    updatedAtUtc: approvedAtUtc,
  }

  return {
    count: {
      ...result.count,
      status: 'approved',
      approvedByPersonId: result.count.countedByPersonId,
      approvedAtUtc,
      inventoryAdjustmentId: adjustment.id,
      updatedAtUtc: approvedAtUtc,
    },
    adjustment,
    originEvent:
      result.count.varianceQuantity > 0
        ? {
            id: `origin-${now}`,
            originType: 'cycle_count_gain',
            supplyarrItemId: result.count.supplyarrItemId,
            quantity: Math.abs(result.count.varianceQuantity),
            unitOfMeasure: result.count.unitOfMeasure,
            locationNameSnapshot: result.count.locationNameSnapshot,
          }
        : null,
    movement: {
      id: `move-${now}`,
      movementType: result.count.varianceQuantity > 0 ? 'count_gain' : 'count_loss',
      reasonCode: result.count.reasonCode,
    },
  }
}

function createLocalAdjustmentMutation(
  form: AdjustmentFormState,
  location: LoadArrLocation | undefined,
  selectedAdjustment: LoadArrAdjustment | undefined,
  selectedCount: LoadArrCount | undefined,
  items: SupplyArrItemReference[],
): LoadArrAdjustmentMutation {
  const locationSnapshot = requireLocalValue(location)
  const itemSnapshot = requireLocalValue(items.find((item) => item.supplyarrItemId === form.supplyarrItemId))
  const now = Date.now().toString(36)
  const timestamp = new Date().toISOString()
  const adjustment: LoadArrAdjustment = {
    id: selectedAdjustment?.id ?? `adj-${now}`,
    adjustmentNumber: selectedAdjustment?.adjustmentNumber ?? `ADJ-${now.toUpperCase()}`,
    status: 'open',
    adjustmentType: form.adjustmentType,
    staffarrSiteOrgUnitId: locationSnapshot.staffarrSiteOrgUnitId,
    staffarrSiteNameSnapshot: locationSnapshot.staffarrSiteNameSnapshot,
    warehouseLocationId: locationSnapshot.id,
    locationNameSnapshot: locationSnapshot.name,
    supplyarrItemId: itemSnapshot.supplyarrItemId,
    itemNameSnapshot: itemSnapshot.itemNameSnapshot,
    quantityDelta: toSignedNumber(form.quantityDelta) || selectedCount?.varianceQuantity || 1,
    unitOfMeasure: itemSnapshot.unitOfMeasureSnapshot,
    reasonCode: form.reasonCode,
    createdByPersonId: form.createdByPersonId,
    approvedByPersonId: null,
    inventoryOriginEventId: null,
    evidenceSummary: form.evidenceSummary,
    createdAtUtc: timestamp,
    approvedAtUtc: null,
    updatedAtUtc: timestamp,
  }

  return {
    adjustment,
    originEvent: null,
    movement: null,
  }
}

function createLocalAdjustmentApproval(result: LoadArrAdjustmentMutation): LoadArrAdjustmentMutation {
  const now = Date.now().toString(36)
  const approvedAtUtc = new Date().toISOString()

  return {
    adjustment: {
      ...result.adjustment,
      status: 'approved',
      approvedByPersonId: result.adjustment.createdByPersonId,
      inventoryOriginEventId: result.adjustment.quantityDelta > 0 ? `origin-${now}` : result.adjustment.inventoryOriginEventId,
      approvedAtUtc,
      updatedAtUtc: approvedAtUtc,
    },
    originEvent:
      result.adjustment.quantityDelta > 0
        ? {
            id: `origin-${now}`,
            originType: 'manual_adjustment',
            supplyarrItemId: result.adjustment.supplyarrItemId,
            quantity: Math.abs(result.adjustment.quantityDelta),
            unitOfMeasure: result.adjustment.unitOfMeasure,
            locationNameSnapshot: result.adjustment.locationNameSnapshot,
          }
        : null,
    movement: {
      id: `move-${now}`,
      movementType: result.adjustment.quantityDelta >= 0 ? 'count_gain' : 'count_loss',
      reasonCode: result.adjustment.reasonCode,
    },
  }
}

function toPositiveNumber(value: string) {
  const parsed = Number.parseFloat(value)

  if (!Number.isFinite(parsed) || parsed <= 0) {
    return 0
  }

  return parsed
}

function toNonNegativeNumber(value: string) {
  const parsed = Number.parseFloat(value)

  if (!Number.isFinite(parsed) || parsed < 0) {
    return 0
  }

  return parsed
}

function toSignedNumber(value: string) {
  const parsed = Number.parseFloat(value)

  if (!Number.isFinite(parsed)) {
    return 0
  }

  return parsed
}

function requireLocalValue<T>(value: T | null | undefined): T {
  if (value === null || value === undefined) {
    throw new Error('Required source data is unavailable.')
  }

  return value
}

function createLocalPreview<T>(factory: () => T): T | null {
  try {
    return factory()
  } catch {
    return null
  }
}

function formatDate(value: string) {
  const date = new Date(value)

  if (Number.isNaN(date.getTime())) {
    return value
  }

  return date.toLocaleString(undefined, {
    month: 'short',
    day: 'numeric',
    hour: 'numeric',
    minute: '2-digit',
  })
}
