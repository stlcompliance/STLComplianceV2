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
import { useLocation, useNavigate } from 'react-router-dom'
import {
  ControlledSelect,
  FormField,
  ProductAppShell,
  buildProductLaunchUrlMap,
  formatProductLaunchError,
  getLaunchCatalog,
  resolveProductWorkspaceBootstrapError,
  resolveSuiteHomeUrl,
  useProductWorkspaceLaunch,
  type PickerOption,
  type ProductNavItem,
} from '@stl/shared-ui'
import { getSessionBootstrap } from './api/client'
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
  | 'settings'

type LoadArrRoute = {
  key: ViewKey
  label: string
  path: string
  icon: NonNullable<ProductNavItem['icon']>
  sectionBreakBefore?: boolean
  showInSidebar?: boolean
}

type NavIcon = NonNullable<ProductNavItem['icon']>

const loadarrRoutes: LoadArrRoute[] = [
  { key: 'dashboard', label: 'Dashboard', path: '/dashboard', icon: BarChart3 as NavIcon },
  { key: 'inventory', label: 'Inventory', path: '/inventory', icon: Boxes as NavIcon },
  { key: 'balances', label: 'Balances', path: '/balances', icon: PackageCheck as NavIcon, sectionBreakBefore: true },
  { key: 'expected-receipts', label: 'Expected Receipts', path: '/expected-receipts', icon: PackagePlus as NavIcon },
  { key: 'receiving', label: 'Receiving', path: '/receiving', icon: PackagePlus as NavIcon },
  { key: 'putaway', label: 'Putaway', path: '/putaway', icon: PackagePlus as NavIcon },
  { key: 'reservations', label: 'Reservations', path: '/reservations', icon: ClipboardCheck as NavIcon },
  { key: 'picks', label: 'Picks', path: '/picks', icon: ClipboardList as NavIcon },
  { key: 'issues', label: 'Issues', path: '/issues', icon: AlertTriangle as NavIcon },
  { key: 'returns', label: 'Returns', path: '/returns', icon: Route as NavIcon },
  { key: 'transfers', label: 'Transfers', path: '/transfers', icon: Route as NavIcon },
  { key: 'truckstock', label: 'Truck Stock', path: '/truck-stock', icon: Truck as NavIcon },
  { key: 'locations', label: 'Locations', path: '/locations', icon: MapPin as NavIcon },
  { key: 'counts', label: 'Counts', path: '/counts', icon: Activity as NavIcon },
  { key: 'adjustments', label: 'Adjustments', path: '/adjustments', icon: ClipboardCheck as NavIcon },
  { key: 'ledger', label: 'Stock Ledger', path: '/ledger', icon: DatabaseZap as NavIcon, sectionBreakBefore: true },
  { key: 'discrepancies', label: 'Discrepancies', path: '/discrepancies', icon: AlertTriangle as NavIcon },
  { key: 'replenishment', label: 'Replenishment', path: '/replenishment', icon: Truck as NavIcon },
  { key: 'history', label: 'History', path: '/history', icon: BarChart3 as NavIcon },
  {
    key: 'tasks',
    label: 'Tasks',
    path: '/tasks',
    icon: ClipboardList as NavIcon,
    sectionBreakBefore: true,
  },
  { key: 'holds', label: 'Holds', path: '/holds', icon: ShieldCheck as NavIcon },
  { key: 'unexplained', label: 'Unexplained', path: '/unexplained', icon: AlertTriangle as NavIcon },
  { key: 'handoffs', label: 'Handoffs', path: '/handoffs', icon: Route as NavIcon },
  { key: 'settings', label: 'Settings', path: '/settings', icon: Warehouse as NavIcon, sectionBreakBefore: true },
  {
    key: 'kits',
    label: 'Kits',
    path: '/kits',
    icon: Boxes as NavIcon,
    showInSidebar: false,
  },
]

const productNavItems: ProductNavItem[] = loadarrRoutes
  .filter((route) => route.showInSidebar !== false)
  .map((route) => ({
    label: route.label,
    to: route.path,
    icon: route.icon,
    sectionBreakBefore: route.sectionBreakBefore,
  }))

const loadarrRouteByPath = new Map(loadarrRoutes.map((route) => [route.path, route.key]))

const suiteHomeUrl = resolveSuiteHomeUrl(import.meta.env.VITE_SUITE_URL)
const productLaunchUrls = buildProductLaunchUrlMap(import.meta.env)
const apiBase = import.meta.env.VITE_LOADARR_API_BASE ?? ''

const fallbackSummary: LoadArrWorkspaceSummary = {
  generatedAt: '2026-06-03T14:00:00Z',
  metrics: {
    activeLocations: 4,
    quantityOnHand: 64,
    quantityCommitted: 23,
    quantityBlocked: 14,
    openTasks: 3,
    openHolds: 2,
    unexplainedInventory: 2,
  },
  locations: [
    {
      id: 'loc-dock-01',
      staffarrSiteNameSnapshot: 'STL North Yard',
      staffarrSiteOrgUnitId: 'staff-site-stl-north',
      name: 'Receiving Dock 1',
      locationType: 'dock',
      path: 'STL North Yard / Main Warehouse / Dock 1',
      active: true,
      complianceRestrictions: ['ambient', 'forklift'],
      capacityPercent: 78,
      notes: 'Open for receipts and outbound staging',
    },
    {
      id: 'loc-haz-01',
      staffarrSiteNameSnapshot: 'STL North Yard',
      staffarrSiteOrgUnitId: 'staff-site-stl-north',
      name: 'Hazmat Cage A',
      locationType: 'hazmat_cage',
      path: 'STL North Yard / Secure Storage / Hazmat Cage A',
      active: true,
      complianceRestrictions: ['hazmat', 'controlled_access', 'inspection_required'],
      capacityPercent: 63,
      notes: 'Authorized staff and current hazmat training required',
    },
    {
      id: 'loc-quarantine-01',
      staffarrSiteNameSnapshot: 'STL North Yard',
      staffarrSiteOrgUnitId: 'staff-site-stl-north',
      name: 'Quarantine Bay',
      locationType: 'quarantine_area',
      path: 'STL North Yard / Quality / Quarantine Bay',
      active: true,
      complianceRestrictions: ['quality_hold', 'blocked'],
      capacityPercent: 41,
      notes: 'Blocked from allocation until investigation closes',
    },
    {
      id: 'loc-truck-17',
      staffarrSiteNameSnapshot: 'South Service Depot',
      staffarrSiteOrgUnitId: 'staff-site-south-depot',
      name: 'Truck Stock 17',
      locationType: 'service_truck',
      path: 'South Service Depot / Mobile Stock / Truck 17',
      active: true,
      complianceRestrictions: ['mobile_stock', 'route_ready'],
      capacityPercent: 55,
      notes: 'Assigned to field maintenance route handoff',
    },
  ],
  inventory: [
    {
      id: 'bal-valve-kit-a',
      supplyarrItemId: 'SUP-VALVE-KIT-A',
      itemNameSnapshot: 'Valve repair kit A',
      unitOfMeasureSnapshot: 'each',
      state: 'available',
      locationId: 'loc-dock-01',
      locationNameSnapshot: 'Receiving Dock 1',
      quantityOnHand: 38,
      quantityReserved: 11,
      quantityAllocated: 4,
      quantityBlocked: 0,
      originEventType: 'purchase_receipt',
      originReference: 'PO-10492 / RR-24018',
      traceTags: ['lot:L2405-77', 'supplyarr:part:SUP-VALVE-KIT-A'],
      notes: 'Ready for putaway',
    },
    {
      id: 'bal-adhesive-haz',
      supplyarrItemId: 'SUP-ADH-49',
      itemNameSnapshot: 'Regulated adhesive cartridge',
      unitOfMeasureSnapshot: 'case',
      state: 'pending_inspection',
      locationId: 'loc-haz-01',
      locationNameSnapshot: 'Hazmat Cage A',
      quantityOnHand: 14,
      quantityReserved: 0,
      quantityAllocated: 0,
      quantityBlocked: 14,
      originEventType: 'vendor_consignment_receipt',
      originReference: 'ASN-8834',
      traceTags: ['hazmat', 'sds:current', 'lot:ADH-991'],
      notes: 'SDS and label check required',
    },
    {
      id: 'bal-brake-rotor',
      supplyarrItemId: 'SUP-BR-ROTOR-22',
      itemNameSnapshot: 'Brake rotor assembly',
      unitOfMeasureSnapshot: 'each',
      state: 'reserved',
      locationId: 'loc-truck-17',
      locationNameSnapshot: 'Truck Stock 17',
      quantityOnHand: 12,
      quantityReserved: 2,
      quantityAllocated: 6,
      quantityBlocked: 0,
      originEventType: 'route_return',
      originReference: 'RoutArr trip RT-7781',
      traceTags: ['maintainarr:work-order:WO-5530', 'truck_stock'],
      notes: 'Reserved for maintenance work orders',
    },
  ],
  tasks: [
    {
      id: 'task-receive-24018',
      taskType: 'receive',
      title: 'Receive PO-10492',
      priority: 'high',
      status: 'ready',
      locationNameSnapshot: 'Receiving Dock 1',
      assignedRole: 'Inventory Clerk',
      supplyarrItemId: 'SUP-VALVE-KIT-A',
      quantity: 38,
      dueAtUtc: '2026-06-03T15:00:00Z',
      requiredSignals: ['purchase_receipt', 'packing_slip_attached'],
    },
    {
      id: 'task-inspect-adh-49',
      taskType: 'quality_inspection',
      title: 'Inspect regulated adhesive lot',
      priority: 'urgent',
      status: 'blocked_by_training',
      locationNameSnapshot: 'Hazmat Cage A',
      assignedRole: 'Hazmat-qualified reviewer',
      supplyarrItemId: 'SUP-ADH-49',
      quantity: 14,
      dueAtUtc: '2026-06-03T18:00:00Z',
      requiredSignals: ['hazmat', 'trainarr_required', 'compliancecore_sds_check'],
    },
    {
      id: 'task-pick-wo-5530',
      taskType: 'pick',
      title: 'Pick parts for WO-5530',
      priority: 'normal',
      status: 'in_progress',
      locationNameSnapshot: 'Truck Stock 17',
      assignedRole: 'Route Stock Lead',
      supplyarrItemId: 'SUP-BR-ROTOR-22',
      quantity: 6,
      dueAtUtc: '2026-06-04T13:30:00Z',
      requiredSignals: ['maintainarr', 'route_ready'],
    },
  ],
  holds: [
    {
      id: 'hold-adh-49',
      holdType: 'quality_hold',
      supplyarrItemId: 'SUP-ADH-49',
      locationNameSnapshot: 'Hazmat Cage A',
      status: 'Open',
      reason: 'SDS label mismatch requires Compliance Core review',
      sourceReference: 'ComplianceCore rule title49.hazmat.labeling',
      openedAtUtc: '2026-06-02T21:10:00Z',
    },
    {
      id: 'hold-count-rotor',
      holdType: 'investigation',
      supplyarrItemId: 'SUP-BR-ROTOR-22',
      locationNameSnapshot: 'Truck Stock 17',
      status: 'Review',
      reason: 'Cycle count variance above mobile-stock threshold',
      sourceReference: 'Count CC-8021',
      openedAtUtc: '2026-06-02T19:40:00Z',
    },
  ],
  routeHandoffs: [
    {
      id: 'handoff-rt-7781',
      targetProduct: 'RoutArr',
      targetReference: 'RT-7781',
      locationNameSnapshot: 'Truck Stock 17',
      status: 'ready',
      quantity: 6,
      notes: 'WO-5530 parts staged for mobile maintenance route',
    },
    {
      id: 'handoff-out-1204',
      targetProduct: 'SupplyArr',
      targetReference: 'OUT-1204',
      locationNameSnapshot: 'Receiving Dock 1',
      status: 'waiting_on_pick',
      quantity: 11,
      notes: 'Outbound stock movement pending pick confirmation',
    },
  ],
  evidence: [
    {
      id: 'ev-rr-24018-photo',
      evidenceType: 'photo',
      locationNameSnapshot: 'Receiving Dock 1',
      summary: 'Dock receipt photo attached to RR-24018',
      capturedAtUtc: '2026-06-02T20:16:00Z',
      capturedBy: 'Inventory Clerk',
    },
    {
      id: 'ev-adh-sds-check',
      evidenceType: 'rule_evaluation',
      locationNameSnapshot: 'Hazmat Cage A',
      summary: 'SDS and label rule check opened',
      capturedAtUtc: '2026-06-02T21:12:00Z',
      capturedBy: 'ComplianceCore',
    },
    {
      id: 'ev-count-8021',
      evidenceType: 'cycle_count',
      locationNameSnapshot: 'Truck Stock 17',
      summary: 'Mobile stock variance captured for review',
      capturedAtUtc: '2026-06-02T19:42:00Z',
      capturedBy: 'Route Stock Lead',
    },
  ],
  unexplainedInventory: [
    {
      id: 'unexplained-count-8021',
      recordNumber: 'UNX-8021',
      status: 'needs_approval',
      discoverySource: 'cycle_count_variance',
      staffarrSiteOrgUnitId: 'staff-site-south-depot',
      staffarrSiteNameSnapshot: 'South Service Depot',
      warehouseLocationId: 'loc-truck-17',
      locationNameSnapshot: 'Truck Stock 17',
      supplyarrItemId: 'SUP-BR-ROTOR-22',
      itemNameSnapshot: 'Brake rotor assembly',
      expectedQuantity: 10,
      quantity: 12,
      varianceQuantity: 2,
      unitOfMeasure: 'each',
      lotCode: null,
      serialCode: 'BR-SN-7781',
      discoveredByPersonId: 'person-route-stock-lead',
      reasonCode: 'cycle_count_variance',
      evidenceSummary: 'Positive mobile-stock variance captured; stock is not trusted available until supervisor approval.',
      complianceEvaluationId: null,
      resolutionState: 'not_trusted_available',
      discoveredAtUtc: '2026-06-02T19:45:00Z',
      resolvedAtUtc: null,
    },
    {
      id: 'unexplained-dock-adh',
      recordNumber: 'UNX-ADH-49',
      status: 'needs_quarantine',
      discoverySource: 'damaged_freight_receipt',
      staffarrSiteOrgUnitId: 'staff-site-stl-north',
      staffarrSiteNameSnapshot: 'STL North Yard',
      warehouseLocationId: 'loc-quarantine-01',
      locationNameSnapshot: 'Quarantine Bay',
      supplyarrItemId: 'SUP-ADH-49',
      itemNameSnapshot: 'Regulated adhesive cartridge',
      expectedQuantity: 0,
      quantity: 1,
      varianceQuantity: 1,
      unitOfMeasure: 'case',
      lotCode: 'ADH-991',
      serialCode: null,
      discoveredByPersonId: 'person-inventory-clerk',
      reasonCode: 'unknown_origin_review',
      evidenceSummary: 'One extra case found beside freight paperwork; moved to StaffArr quarantine location for review.',
      complianceEvaluationId: 'cc-eval-adh-extra',
      resolutionState: 'quarantined_untrusted',
      discoveredAtUtc: '2026-06-02T20:50:00Z',
      resolvedAtUtc: null,
    },
  ],
}

const fallbackCounts: LoadArrCount[] = [
  {
    id: 'count-8021',
    countNumber: 'CNT-260602-1945',
    status: 'variance_pending_approval',
    countType: 'cycle_count',
    staffarrSiteOrgUnitId: 'staff-site-south-depot',
    staffarrSiteNameSnapshot: 'South Service Depot',
    warehouseLocationId: 'loc-truck-17',
    locationNameSnapshot: 'Truck Stock 17',
    supplyarrItemId: 'SUP-BR-ROTOR-22',
    itemNameSnapshot: 'Brake rotor assembly',
    expectedQuantity: 10,
    countedQuantity: 12,
    varianceQuantity: 2,
    unitOfMeasure: 'each',
    countedByPersonId: 'person-route-stock-lead',
    approvedByPersonId: null,
    reasonCode: 'cycle_count_variance',
    inventoryAdjustmentId: null,
    evidenceSummary: 'Positive variance waiting supervisor approval',
    createdAtUtc: '2026-06-02T19:45:00Z',
    completedAtUtc: '2026-06-02T19:54:00Z',
    approvedAtUtc: null,
    updatedAtUtc: '2026-06-02T19:54:00Z',
  },
  {
    id: 'count-adh-49',
    countNumber: 'CNT-260602-2110',
    status: 'approved',
    countType: 'compliance',
    staffarrSiteOrgUnitId: 'staff-site-stl-north',
    staffarrSiteNameSnapshot: 'STL North Yard',
    warehouseLocationId: 'loc-haz-01',
    locationNameSnapshot: 'Hazmat Cage A',
    supplyarrItemId: 'SUP-ADH-49',
    itemNameSnapshot: 'Regulated adhesive cartridge',
    expectedQuantity: 14,
    countedQuantity: 14,
    varianceQuantity: 0,
    unitOfMeasure: 'case',
    countedByPersonId: 'person-hazmat-reviewer',
    approvedByPersonId: 'person-hazmat-supervisor',
    reasonCode: 'sds_label_mismatch',
    inventoryAdjustmentId: 'adj-count-adh-49',
    evidenceSummary: 'SDS and label rule check completed',
    createdAtUtc: '2026-06-02T21:10:00Z',
    completedAtUtc: '2026-06-02T21:18:00Z',
    approvedAtUtc: '2026-06-02T21:20:00Z',
    updatedAtUtc: '2026-06-02T21:20:00Z',
  },
]

const fallbackAdjustments: LoadArrAdjustment[] = [
  {
    id: 'adj-count-adh-49',
    adjustmentNumber: 'ADJ-260602-2118',
    status: 'approved',
    adjustmentType: 'loss',
    staffarrSiteOrgUnitId: 'staff-site-stl-north',
    staffarrSiteNameSnapshot: 'STL North Yard',
    warehouseLocationId: 'loc-haz-01',
    locationNameSnapshot: 'Hazmat Cage A',
    supplyarrItemId: 'SUP-ADH-49',
    itemNameSnapshot: 'Regulated adhesive cartridge',
    quantityDelta: -0.5,
    unitOfMeasure: 'case',
    reasonCode: 'sds_label_mismatch',
    createdByPersonId: 'person-hazmat-reviewer',
    approvedByPersonId: 'person-hazmat-supervisor',
    inventoryOriginEventId: null,
    evidenceSummary: 'SDS and label review correction',
    createdAtUtc: '2026-06-02T21:18:00Z',
    approvedAtUtc: '2026-06-02T21:20:00Z',
    updatedAtUtc: '2026-06-02T21:20:00Z',
  },
  {
    id: 'adj-count-8021',
    adjustmentNumber: 'ADJ-260602-1954',
    status: 'open',
    adjustmentType: 'gain',
    staffarrSiteOrgUnitId: 'staff-site-south-depot',
    staffarrSiteNameSnapshot: 'South Service Depot',
    warehouseLocationId: 'loc-truck-17',
    locationNameSnapshot: 'Truck Stock 17',
    supplyarrItemId: 'SUP-BR-ROTOR-22',
    itemNameSnapshot: 'Brake rotor assembly',
    quantityDelta: 2,
    unitOfMeasure: 'each',
    reasonCode: 'cycle_count_variance',
    createdByPersonId: 'person-route-stock-lead',
    approvedByPersonId: null,
    inventoryOriginEventId: null,
    evidenceSummary: 'Supervisor approval pending',
    createdAtUtc: '2026-06-02T19:54:00Z',
    approvedAtUtc: null,
    updatedAtUtc: '2026-06-02T19:54:00Z',
  },
]

const fallbackTruckStock: LoadArrTruckStock[] = [
  {
    id: 'truck-stock-17-rotor',
    truckStockNumber: 'TRK-17-ROTOR',
    staffarrSiteOrgUnitId: 'staff-site-south-depot',
    staffarrSiteNameSnapshot: 'South Service Depot',
    truckLocationId: 'loc-truck-17',
    truckLocationNameSnapshot: 'Truck Stock 17',
    supplyarrItemId: 'SUP-BR-ROTOR-22',
    itemNameSnapshot: 'Brake rotor assembly',
    unitOfMeasure: 'each',
    assignedPersonId: 'person-route-stock-lead',
    assignedPersonNameSnapshot: 'Jordan Reed',
    quantityOnHand: 12,
    minimumQuantity: 6,
    maximumQuantity: 18,
    status: 'ready',
    lastCountedAtUtc: '2026-06-02T19:40:00Z',
    lastMovementAtUtc: '2026-06-02T19:48:00Z',
    notes: 'Reserved for maintenance work orders and route returns.',
    traceTags: ['truck_stock', 'route_ready', 'maintainarr:work-order:WO-5530'],
  },
  {
    id: 'truck-stock-17-kit',
    truckStockNumber: 'TRK-17-KIT',
    staffarrSiteOrgUnitId: 'staff-site-south-depot',
    staffarrSiteNameSnapshot: 'South Service Depot',
    truckLocationId: 'loc-truck-17',
    truckLocationNameSnapshot: 'Truck Stock 17',
    supplyarrItemId: 'SUP-VALVE-KIT-A',
    itemNameSnapshot: 'Valve repair kit A',
    unitOfMeasure: 'each',
    assignedPersonId: 'person-route-stock-lead',
    assignedPersonNameSnapshot: 'Jordan Reed',
    quantityOnHand: 4,
    minimumQuantity: 3,
    maximumQuantity: 10,
    status: 'low_stock',
    lastCountedAtUtc: '2026-06-02T18:15:00Z',
    lastMovementAtUtc: '2026-06-02T18:50:00Z',
    notes: 'Needs restock after route replenishment and technician issue.',
    traceTags: ['truck_stock', 'restock_required', 'route_replenishment'],
  },
]

const fallbackKits: LoadArrKit[] = [
  {
    id: 'kit-pm-emergency-17',
    kitNumber: 'KIT-PM-17',
    staffarrSiteOrgUnitId: 'staff-site-south-depot',
    staffarrSiteNameSnapshot: 'South Service Depot',
    locationId: 'loc-truck-17',
    locationNameSnapshot: 'Truck Stock 17',
    primaryItemId: 'SUP-VALVE-KIT-A',
    kitNameSnapshot: 'Valve repair kit A',
    unitOfMeasure: 'each',
    assignedPersonId: 'person-route-stock-lead',
    assignedPersonNameSnapshot: 'Jordan Reed',
    quantityOnHand: 4,
    minimumQuantity: 3,
    maximumQuantity: 10,
    status: 'built',
    lastActionAtUtc: '2026-06-02T18:50:00Z',
    lastMovementAtUtc: '2026-06-02T18:50:00Z',
    notes: 'Maintenance and route response kit assigned to mobile stock.',
    traceTags: ['kit', 'mobile_stock', 'pm_ready'],
  },
  {
    id: 'kit-ppe-hazmat-04',
    kitNumber: 'KIT-PPE-04',
    staffarrSiteOrgUnitId: 'staff-site-stl-north',
    staffarrSiteNameSnapshot: 'STL North Yard',
    locationId: 'loc-dock-01',
    locationNameSnapshot: 'Receiving Dock 1',
    primaryItemId: 'SUP-ADH-49',
    kitNameSnapshot: 'Regulated adhesive cartridge kit',
    unitOfMeasure: 'case',
    assignedPersonId: 'person-hazmat-reviewer',
    assignedPersonNameSnapshot: 'Taylor Brooks',
    quantityOnHand: 1,
    minimumQuantity: 1,
    maximumQuantity: 4,
    status: 'needs_replenishment',
    lastActionAtUtc: '2026-06-02T20:30:00Z',
    lastMovementAtUtc: '2026-06-02T20:30:00Z',
    notes: 'Hazmat response kit pending replenishment after inspection.',
    traceTags: ['kit', 'hazmat', 'inspection'],
  },
]

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

const kitPersonOptions: PickerOption[] = [
  { value: 'person-route-stock-lead', label: 'Jordan Reed' },
  { value: 'person-hazmat-reviewer', label: 'Taylor Brooks' },
  { value: 'person-hazmat-supervisor', label: 'Riley Quinn' },
  { value: 'person-inventory-clerk', label: 'Avery Morgan' },
  { value: 'person-inventory-supervisor', label: 'Morgan Patel' },
]

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

const fallbackSupplyArrItemReferences: SupplyArrItemReference[] = [
  {
    supplyarrItemId: 'SUP-VALVE-KIT-A',
    itemNumberSnapshot: 'VALVE-KIT-A',
    itemNameSnapshot: 'Valve repair kit A',
    unitOfMeasureSnapshot: 'each',
    itemTypeSnapshot: 'maintenance_part',
    isLotControlled: true,
    isSerialControlled: false,
    isHazardous: false,
    requiresSds: false,
    updatedAtUtc: '2026-06-01T12:00:00Z',
  },
  {
    supplyarrItemId: 'SUP-ADH-49',
    itemNumberSnapshot: 'ADH-49',
    itemNameSnapshot: 'Regulated adhesive cartridge',
    unitOfMeasureSnapshot: 'case',
    itemTypeSnapshot: 'regulated_consumable',
    isLotControlled: true,
    isSerialControlled: false,
    isHazardous: true,
    requiresSds: true,
    updatedAtUtc: '2026-06-01T12:00:00Z',
  },
  {
    supplyarrItemId: 'SUP-BR-ROTOR-22',
    itemNumberSnapshot: 'BR-ROTOR-22',
    itemNameSnapshot: 'Brake rotor assembly',
    unitOfMeasureSnapshot: 'each',
    itemTypeSnapshot: 'maintenance_part',
    isLotControlled: false,
    isSerialControlled: true,
    isHazardous: false,
    requiresSds: false,
    updatedAtUtc: '2026-06-01T12:00:00Z',
  },
]

const initialReceivingForm: ReceivingFormState = {
  receivingType: 'manual',
  sourceProductKey: 'supplyarr',
  sourceObjectType: 'purchase_order',
  sourceObjectId: 'PO-10492',
  supplierNameSnapshot: 'Midwest Fleet Supply',
  completedByPersonId: 'person-inventory-clerk',
  supplyarrItemId: 'SUP-VALVE-KIT-A',
  expectedQuantity: '38',
  receivedQuantity: '38',
  warehouseLocationId: 'loc-dock-01',
  lotCode: 'L2405-77',
  serialCode: '',
  condition: 'new',
  discrepancyReasonCode: '',
  complianceEvaluationId: '',
  evidenceSummary: 'Dock receipt photo attached',
}

const initialTransferForm: TransferFormState = {
  transferType: 'bin_to_bin',
  fromLocationId: 'loc-dock-01',
  toLocationId: 'loc-quarantine-01',
  completedByPersonId: 'person-inventory-clerk',
  supplyarrItemId: 'SUP-VALVE-KIT-A',
  quantity: '4',
  lotCode: 'L2405-77',
  serialCode: '',
  reasonCode: 'quality_inspection',
  complianceEvaluationId: '',
  evidenceSummary: 'Moved to StaffArr quarantine location for inspection',
}

const initialHoldForm: HoldFormState = {
  holdType: 'quality',
  warehouseLocationId: 'loc-haz-01',
  supplyarrItemId: 'SUP-ADH-49',
  quantity: '4',
  reasonCode: 'sds_label_mismatch',
  description: 'Hold pending SDS label verification',
  createdByPersonId: 'person-hazmat-reviewer',
  complianceEvaluationId: 'cc-eval-adh-49',
  evidenceSummary: 'SDS and label rule check opened',
}

const initialHoldReleaseForm: HoldReleaseFormState = {
  holdId: 'hold-adh-49',
  reasonCode: 'compliance_review_cleared',
  releasedByPersonId: 'person-hazmat-reviewer',
  evidenceSummary: 'Compliance Core label review cleared',
}

const initialUnexplainedInventoryForm: UnexplainedInventoryFormState = {
  discoverySource: 'cycle_count_variance',
  warehouseLocationId: 'loc-truck-17',
  supplyarrItemId: 'SUP-BR-ROTOR-22',
  expectedQuantity: '10',
  quantity: '12',
  lotCode: '',
  serialCode: 'BR-SN-7781',
  discoveredByPersonId: 'person-route-stock-lead',
  reasonCode: 'cycle_count_variance',
  evidenceSummary: 'Cycle count found two additional rotors; keep unavailable until supervisor approval.',
  complianceEvaluationId: '',
}

const initialUnexplainedResolutionForm: UnexplainedInventoryResolutionFormState = {
  recordId: 'unexplained-count-8021',
  reasonCode: 'supervisor_approved_valid_stock',
  personId: 'person-inventory-supervisor',
  quarantineLocationId: 'loc-quarantine-01',
  complianceEvaluationId: '',
  evidenceSummary: 'Supervisor reviewed count evidence and approved trusted stock resolution.',
}

const initialCountForm: CountFormState = {
  countType: 'cycle_count',
  warehouseLocationId: 'loc-truck-17',
  supplyarrItemId: 'SUP-BR-ROTOR-22',
  expectedQuantity: '10',
  countedQuantity: '12',
  countedByPersonId: 'person-route-stock-lead',
  reasonCode: 'cycle_count_variance',
  evidenceSummary: 'Positive variance waiting supervisor approval',
}

const initialAdjustmentForm: AdjustmentFormState = {
  adjustmentType: 'gain',
  warehouseLocationId: 'loc-truck-17',
  supplyarrItemId: 'SUP-BR-ROTOR-22',
  quantityDelta: '2',
  createdByPersonId: 'person-route-stock-lead',
  reasonCode: 'cycle_count_variance',
  evidenceSummary: 'Create adjustment from cycle count variance',
  approvedByPersonId: 'person-inventory-supervisor',
}

const initialTruckStockForm: TruckStockFormState = {
  truckStockId: 'truck-stock-17-rotor',
  quantity: '1',
  personId: 'person-route-stock-lead',
  reasonCode: 'route_replenishment',
  evidenceSummary: 'Mobile stock movement captured from the warehouse shell',
}

const initialKitForm: KitFormState = {
  kitId: 'kit-pm-emergency-17',
  operation: 'build',
  quantity: '1',
  personId: 'person-route-stock-lead',
  reasonCode: 'kit_build',
  evidenceSummary: 'Kit lifecycle action captured from the warehouse shell',
  targetPersonId: 'person-hazmat-reviewer',
  targetLocationId: 'loc-truck-17',
}

export function App() {
  const location = useLocation()
  const navigate = useNavigate()
  const session = loadSession()
  const [summary, setSummary] = useState<LoadArrWorkspaceSummary>(fallbackSummary)
  const [loadState, setLoadState] = useState<'loading' | 'live' | 'offline'>('loading')
  const [query, setQuery] = useState('')
  const [receivingForm, setReceivingForm] = useState<ReceivingFormState>(initialReceivingForm)
  const [receivingStatus, setReceivingStatus] = useState<'idle' | 'submitting' | 'completed' | 'failed'>('idle')
  const [receivingCompletion, setReceivingCompletion] = useState<LoadArrReceivingCompletion | null>(null)
  const [transferForm, setTransferForm] = useState<TransferFormState>(initialTransferForm)
  const [transferStatus, setTransferStatus] = useState<'idle' | 'submitting' | 'completed' | 'failed'>('idle')
  const [transferCompletion, setTransferCompletion] = useState<LoadArrTransferCompletion | null>(null)
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
    queryKey: ['loadarr-session', session?.accessToken],
    queryFn: () => getSessionBootstrap(session!.accessToken),
    enabled: Boolean(session?.accessToken),
    retry: false,
  })

  const launchCatalogQuery = useQuery({
    queryKey: ['loadarr-launch-catalog', session?.accessToken],
    queryFn: () => getLaunchCatalog(apiBase, session!.accessToken, 'loadarr'),
    enabled: Boolean(session?.accessToken),
    retry: false,
  })

  const normalizedPathname = location.pathname.replace(/\/+$/, '') || '/'
  const activeView = useMemo<ViewKey>(() => {
    if (normalizedPathname === '/') {
      return 'dashboard'
    }

    return loadarrRouteByPath.get(normalizedPathname) ?? 'dashboard'
  }, [normalizedPathname])

  useEffect(() => {
    if (normalizedPathname === '/') {
      navigate('/dashboard', { replace: true })
      return
    }

    if (!loadarrRouteByPath.has(normalizedPathname)) {
      navigate('/dashboard', { replace: true })
    }
  }, [navigate, normalizedPathname])

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

    fetch('/api/v1/workspace/summary', {
      credentials: 'include',
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
  }, [])

  useEffect(() => {
    const controller = new AbortController()

    fetch('/api/v1/counts', {
      credentials: 'include',
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
  }, [])

  useEffect(() => {
    const controller = new AbortController()

    fetch('/api/v1/adjustments', {
      credentials: 'include',
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
  }, [])

  useEffect(() => {
    const controller = new AbortController()

    fetch('/api/v1/truck-stock', {
      credentials: 'include',
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
  }, [])

  useEffect(() => {
    const controller = new AbortController()

    fetch('/api/v1/kits', {
      credentials: 'include',
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
  }, [])

  useEffect(() => {
    if (!selectedLocationId) {
      setLocationUtilization(null)
      return
    }

    const controller = new AbortController()

    fetch(`/api/v1/locations/${selectedLocationId}/utilization`, {
      credentials: 'include',
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
  }, [selectedLocationId])

  useEffect(() => {
    const controller = new AbortController()

    fetch('/api/v1/workspace/supplyarr-item-references', {
      credentials: 'include',
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
  }, [])

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

  const selectedCount = useMemo(
    () => counts.find((count) => count.id === countResult?.count.id) ?? counts[0],
    [countResult?.count.id, counts],
  )

  const selectedAdjustment = useMemo(
    () => adjustments.find((adjustment) => adjustment.id === adjustmentResult?.adjustment.id) ?? adjustments[0],
    [adjustmentResult?.adjustment.id, adjustments],
  )

  const selectedInventoryItem = useMemo(
    () => filteredInventory[0] ?? summary.inventory[0] ?? null,
    [filteredInventory, summary.inventory],
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
      const response = await fetch('/api/v1/receiving/draft/complete', {
        method: 'POST',
        credentials: 'include',
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
      setReceivingStatus('completed')
    } catch {
      const fallback = createLocalReceivingCompletion(
        receivingForm,
        selectedReceivingLocation,
        selectedSupplyArrItem,
      )
      setReceivingCompletion(fallback)
      setReceivingStatus('completed')
    }
  }

  const completeTransfer = async () => {
    setTransferStatus('submitting')
    const payload = toTransferPayload(transferForm)

    try {
      const response = await fetch('/api/v1/transfers/draft/complete', {
        method: 'POST',
        credentials: 'include',
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
      setTransferStatus('completed')
    } catch {
      const fallback = createLocalTransferCompletion(
        transferForm,
        selectedTransferSourceLocation,
        selectedTransferDestinationLocation,
        selectedTransferItem,
        selectedTransferSourceBalance,
      )
      setTransferCompletion(fallback)
      setTransferStatus('completed')
    }
  }

  const createHold = async () => {
    setHoldStatus('submitting')
    const payload = toHoldPayload(holdForm)

    try {
      const response = await fetch('/api/v1/holds', {
        method: 'POST',
        credentials: 'include',
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
      const fallback = createLocalHoldMutation(
        holdForm,
        selectedHoldLocation,
        selectedHoldItem,
        selectedHoldBalance,
      )
      setHoldMutation(fallback)
      setHoldStatus('completed')
    }
  }

  const releaseHold = async () => {
    setHoldReleaseStatus('submitting')
    const payload = toHoldReleasePayload(holdReleaseForm)

    try {
      const response = await fetch(`/api/v1/holds/${holdReleaseForm.holdId}/release`, {
        method: 'POST',
        credentials: 'include',
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
      const fallback = createLocalHoldReleaseMutation(
        holdReleaseForm,
        selectedReleaseHold,
        selectedReleaseBalance,
      )
      setHoldMutation(fallback)
      setHoldReleaseStatus('completed')
    }
  }

  const createUnexplainedInventory = async () => {
    setUnexplainedStatus('submitting')
    const payload = toUnexplainedInventoryPayload(unexplainedForm)

    try {
      const response = await fetch('/api/v1/unexplained-inventory', {
        method: 'POST',
        credentials: 'include',
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
      setUnexplainedMutation(createLocalUnexplainedInventoryMutation(
        unexplainedForm,
        selectedUnexplainedLocation,
        selectedUnexplainedItem,
      ))
      setUnexplainedStatus('completed')
    }
  }

  const mutateUnexplainedInventory = async (action: 'resolve' | 'quarantine' | 'scrap') => {
    setUnexplainedStatus('submitting')
    const payload = toUnexplainedResolutionPayload(unexplainedResolutionForm, action)

    try {
      const response = await fetch(`/api/v1/unexplained-inventory/${unexplainedResolutionForm.recordId}/${action}`, {
        method: 'POST',
        credentials: 'include',
        headers: {
          Accept: 'application/json',
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(payload),
      })

      if (!response.ok) {
        throw new Error(`Unexplained inventory ${action} failed: ${response.status}`)
      }

      const data = (await response.json()) as LoadArrUnexplainedInventoryMutation
      setUnexplainedMutation(data)
      setUnexplainedStatus('completed')
    } catch {
      setUnexplainedMutation(createLocalUnexplainedResolutionMutation(
        unexplainedResolutionForm,
        selectedUnexplainedRecord,
        action,
      ))
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
      const response = await fetch(`/api/v1/truck-stock/${record.id}/${action}`, {
        method: 'POST',
        credentials: 'include',
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
      const fallback = createLocalTruckStockMutation(action, truckStockForm, record)
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
      const response = await fetch(`/api/v1/kits/${record.id}/${operation}`, {
        method: 'POST',
        credentials: 'include',
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
      const fallback = createLocalKitMutation(operation, kitForm, record, targetPersonNameSnapshot, targetLocationNameSnapshot)
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
      const createResponse = await fetch('/api/v1/counts', {
        method: 'POST',
        credentials: 'include',
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

      const completeResponse = await fetch(`/api/v1/counts/${createdCount.id}/complete`, {
        method: 'POST',
        credentials: 'include',
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
      const fallback = createLocalCountCompletion(countForm, selectedLocation, selectedCount, supplyArrItemReferences)
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
      const response = await fetch(`/api/v1/counts/${countId}/approve-variance`, {
        method: 'POST',
        credentials: 'include',
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
      setCountResult(createLocalCountVarianceApproval(countResult ?? createLocalCountCompletion(countForm, selectedLocation, selectedCount, supplyArrItemReferences)))
      setCountStatus('completed')
    }
  }

  const createAdjustment = async () => {
    setAdjustmentStatus('submitting')
    const payload = toAdjustmentCreatePayload(adjustmentForm)

    try {
      const response = await fetch('/api/v1/adjustments', {
        method: 'POST',
        credentials: 'include',
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
      setAdjustmentResult(createLocalAdjustmentMutation(adjustmentForm, selectedLocation, selectedAdjustment, selectedCount, supplyArrItemReferences))
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
      const response = await fetch(`/api/v1/adjustments/${adjustmentId}/approve`, {
        method: 'POST',
        credentials: 'include',
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
      setAdjustmentResult(createLocalAdjustmentApproval(adjustmentResult ?? createLocalAdjustmentMutation(adjustmentForm, selectedLocation, selectedAdjustment, selectedCount, supplyArrItemReferences)))
      setAdjustmentStatus('completed')
    }
  }

  const bootstrapError = sessionQuery.isError
    ? resolveProductWorkspaceBootstrapError(sessionQuery.error)
    : null

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
    <ProductAppShell
      productName="LoadArr"
      productKey="loadarr"
      workspaceSubtitle="Warehouse execution and inventory custody"
      navItems={productNavItems}
      tenantDisplayName={workspaceSession?.tenantDisplayName}
      tenantSlug={workspaceSession?.tenantSlug}
      userDisplayName={workspaceSession?.userDisplayName}
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
              {expectedReceiptRows[0] ? (
                <div className="completion-stack">
                  <AuditFact label="Receipt" value={expectedReceiptRows[0].expectedReceiptNumber} />
                  <AuditFact label="Source" value={`${expectedReceiptRows[0].sourceProductKey} · ${expectedReceiptRows[0].sourceObjectRef}`} />
                  <AuditFact label="Location" value={expectedReceiptRows[0].locationNameSnapshot} />
                  <AuditFact
                    label="Quantity"
                    value={`${formatNumber.format(expectedReceiptRows[0].expectedQuantity)} expected, ${formatNumber.format(expectedReceiptRows[0].receivedQuantity)} received`}
                  />
                  <AuditFact label="Due" value={formatDate(expectedReceiptRows[0].dueAtUtc)} />
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
              {reservationRows[0] ? (
                <div className="completion-stack">
                  <AuditFact label="Reservation" value={reservationRows[0].reservationNumber} />
                  <AuditFact label="Demand" value={reservationRows[0].demandReference} />
                  <AuditFact label="Item" value={reservationRows[0].itemNameSnapshot} />
                  <AuditFact label="Quantity" value={formatNumber.format(reservationRows[0].quantity)} />
                  <AuditFact label="Status" value={reservationRows[0].status} />
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
          <section className="receiving-layout" aria-label="Pick tasks">
            <article className="workflow-panel">
              <div className="section-heading">
                <ClipboardList aria-hidden="true" />
                <h2>Pick task list</h2>
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

            <aside className="side-panel" aria-label="Pick detail">
              <div className="section-heading">
                <ClipboardCheck aria-hidden="true" />
                <h2>Pick detail</h2>
              </div>
              {pickRows[0] ? (
                <div className="completion-stack">
                  <AuditFact label="Pick" value={pickRows[0].recordNumber} />
                  <AuditFact label="Subject" value={pickRows[0].subject} />
                  <AuditFact label="Quantity" value={formatNumber.format(pickRows[0].quantity)} />
                  <AuditFact label="Location" value={pickRows[0].locationNameSnapshot} />
                  <AuditFact label="Status" value={pickRows[0].status} />
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
          <section className="receiving-layout" aria-label="Issues">
            <article className="workflow-panel">
              <div className="section-heading">
                <AlertTriangle aria-hidden="true" />
                <h2>Issue queue</h2>
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

            <aside className="side-panel" aria-label="Issue detail">
              <div className="section-heading">
                <Truck aria-hidden="true" />
                <h2>Issue detail</h2>
              </div>
              {issueRows[0] ? (
                <div className="completion-stack">
                  <AuditFact label="Issue" value={issueRows[0].recordNumber} />
                  <AuditFact label="Item" value={issueRows[0].subject} />
                  <AuditFact label="Quantity" value={formatNumber.format(issueRows[0].quantity)} />
                  <AuditFact label="Location" value={issueRows[0].locationNameSnapshot} />
                  <AuditFact label="Status" value={issueRows[0].status} />
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
          <section className="receiving-layout" aria-label="Returns">
            <article className="workflow-panel">
              <div className="section-heading">
                <Route aria-hidden="true" />
                <h2>Return queue</h2>
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

            <aside className="side-panel" aria-label="Return detail">
              <div className="section-heading">
                <Route aria-hidden="true" />
                <h2>Return detail</h2>
              </div>
              {returnRows[0] ? (
                <div className="completion-stack">
                  <AuditFact label="Return" value={returnRows[0].recordNumber} />
                  <AuditFact label="Item" value={returnRows[0].subject} />
                  <AuditFact label="Quantity" value={formatNumber.format(returnRows[0].quantity)} />
                  <AuditFact label="Location" value={returnRows[0].locationNameSnapshot} />
                  <AuditFact label="Reference" value={returnRows[0].notes} />
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
          <section className="receiving-layout" aria-label="Discrepancies">
            <article className="workflow-panel">
              <div className="section-heading">
                <AlertTriangle aria-hidden="true" />
                <h2>Discrepancy queue</h2>
              </div>

              <div className="queue compact-queue">
                {discrepancyRows.map((discrepancy) => (
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

            <aside className="side-panel" aria-label="Discrepancy detail">
              <div className="section-heading">
                <ShieldCheck aria-hidden="true" />
                <h2>Discrepancy detail</h2>
              </div>
              {discrepancyRows[0] ? (
                <div className="completion-stack">
                  <AuditFact label="Discrepancy" value={discrepancyRows[0].discrepancyNumber} />
                  <AuditFact label="Item" value={discrepancyRows[0].itemNameSnapshot} />
                  <AuditFact label="Variance" value={formatNumber.format(discrepancyRows[0].quantity)} />
                  <AuditFact label="Status" value={discrepancyRows[0].status} />
                  <AuditFact label="Notes" value={discrepancyRows[0].notes} />
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
          <section className="receiving-layout" aria-label="LoadArr settings">
            <article className="workflow-panel">
              <div className="section-heading">
                <Warehouse aria-hidden="true" />
                <h2>Workspace settings</h2>
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
          <section className="receiving-layout" aria-label="Truck stock workflow">
            <article className="workflow-panel">
              <div className="section-heading">
                <Truck aria-hidden="true" />
                <h2>Truck stock operations</h2>
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

            <aside className="side-panel" aria-label="Truck stock audit">
              <div className="section-heading">
                <ClipboardCheck aria-hidden="true" />
                <h2>Truck stock audit</h2>
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
          <section className="receiving-layout" aria-label="StaffArr locations used by LoadArr">
            <article className="workflow-panel">
              <div className="section-heading">
                <MapPin aria-hidden="true" />
                <h2>Location list</h2>
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
          <section className="receiving-layout" aria-label="Inventory count and adjustment workflow">
            <article className="workflow-panel">
              <div className="section-heading">
                <Activity aria-hidden="true" />
                <h2>Count list</h2>
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

              {countResult ? (
                <div className="completion-stack">
                  <AuditFact
                    label="Count"
                    value={`${countResult.count.countNumber} · ${countResult.count.status}`}
                  />
                  <AuditFact
                    label="Variance"
                    value={`${formatNumber.format(countResult.count.varianceQuantity)} ${countResult.count.unitOfMeasure}`}
                  />
                  <AuditFact
                    label="Location"
                    value={countResult.count.locationNameSnapshot}
                  />
                  <AuditFact
                    label="Adjustment"
                    value={countResult.adjustment ? `${countResult.adjustment.adjustmentNumber} · ${countResult.adjustment.status}` : 'No adjustment yet'}
                  />
                  <AuditFact
                    label="Movement"
                    value={countResult.movement ? `${countResult.movement.movementType} · ${countResult.movement.reasonCode}` : 'No movement yet'}
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
          <section className="receiving-layout" aria-label="Adjustments">
            <article className="workflow-panel">
              <div className="section-heading">
                <ClipboardCheck aria-hidden="true" />
                <h2>Adjustment queue</h2>
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
              {adjustments[0] ? (
                <div className="completion-stack">
                  <AuditFact label="Adjustment" value={adjustments[0].adjustmentNumber} />
                  <AuditFact label="Item" value={adjustments[0].itemNameSnapshot} />
                  <AuditFact label="Quantity" value={formatNumber.format(adjustments[0].quantityDelta)} />
                  <AuditFact label="Status" value={adjustments[0].status} />
                  <AuditFact label="Reason" value={adjustments[0].reasonCode} />
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
          <section className="receiving-layout" aria-label="Replenishment">
            <article className="workflow-panel">
              <div className="section-heading">
                <Truck aria-hidden="true" />
                <h2>Replenishment signals</h2>
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

            <aside className="side-panel" aria-label="Replenishment detail">
              <div className="section-heading">
                <PackagePlus aria-hidden="true" />
                <h2>Replenishment summary</h2>
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
          <section className="queue" aria-label="Warehouse tasks">
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
          <section className="data-grid handoff-grid" aria-label="Route and product handoffs">
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
          </section>
        )}

        <footer className="workspace-footer">
          <Activity aria-hidden="true" />
          <span>Generated {formatDate(summary.generatedAt)}</span>
        </footer>
      </section>
      </div>
    </ProductAppShell>
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
  const locationName = location?.name ?? 'Receiving location'
  const itemSnapshot = item ?? fallbackSupplyArrItemReferences[0]
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
  const itemSnapshot = item ?? fallbackSupplyArrItemReferences[0]
  const fromLocationName = fromLocation?.name ?? 'StaffArr source location'
  const toLocationName = toLocation?.name ?? 'StaffArr destination location'
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

function createLocalTruckStockMutation(
  action: 'issue' | 'return' | 'count',
  form: TruckStockFormState,
  record: LoadArrTruckStock | undefined,
): LoadArrTruckStockMutation {
  const current = record ?? fallbackTruckStock[0]
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
  const current = record ?? fallbackKits[0]
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
  const itemSnapshot = item ?? fallbackSupplyArrItemReferences[0]
  const locationName = location?.name ?? 'StaffArr location'
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
  const heldQuantity = Math.max(0, balance?.quantityBlocked ?? 0)
  const releasedQuantity = heldQuantity || 1
  const itemId = hold?.supplyarrItemId ?? balance?.supplyarrItemId ?? 'SUP-ADH-49'
  const locationName = hold?.locationNameSnapshot ?? balance?.locationNameSnapshot ?? 'StaffArr location'

  return {
    hold: {
      holdNumber: `HLD-${now.toUpperCase()}`,
      status: 'released',
      reasonCode: form.reasonCode,
      quantity: releasedQuantity,
      unitOfMeasure: balance?.unitOfMeasureSnapshot ?? 'each',
      locationNameSnapshot: locationName,
    },
    movement: {
      id: `move-${now}`,
      movementType: 'release_hold',
      reasonCode: form.reasonCode,
    },
    balance: {
      id: balance?.id ?? `bal-release-${now}`,
      supplyarrItemId: itemId,
      itemNameSnapshot: balance?.itemNameSnapshot ?? itemId,
      unitOfMeasureSnapshot: balance?.unitOfMeasureSnapshot ?? 'each',
      state: balance?.state ?? 'available',
      locationId: balance?.locationId ?? 'staffarr-location',
      locationNameSnapshot: locationName,
      quantityOnHand: balance?.quantityOnHand ?? releasedQuantity,
      quantityReserved: balance?.quantityReserved ?? 0,
      quantityAllocated: balance?.quantityAllocated ?? 0,
      quantityBlocked: Math.max(0, heldQuantity - releasedQuantity),
      originEventType: balance?.originEventType ?? 'purchase_receipt',
      originReference: balance?.originReference ?? hold?.sourceReference ?? 'Existing trusted LoadArr balance',
      traceTags: [...(balance?.traceTags ?? []), `released-hold:${form.holdId}`, `movement:move-${now}`],
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
  const itemSnapshot = item ?? fallbackSupplyArrItemReferences[0]
  const foundQuantity = toPositiveNumber(form.quantity)
  const expectedQuantity = toNonNegativeNumber(form.expectedQuantity)
  const varianceQuantity = foundQuantity - expectedQuantity

  return {
    record: {
      id: `unexplained-${now}`,
      recordNumber: `UNX-${now.toUpperCase()}`,
      status: varianceQuantity > 0 ? 'needs_approval' : 'needs_review',
      discoverySource: form.discoverySource,
      staffarrSiteOrgUnitId: location?.staffarrSiteOrgUnitId ?? 'staffarr-site',
      staffarrSiteNameSnapshot: location?.staffarrSiteNameSnapshot ?? 'StaffArr site',
      warehouseLocationId: form.warehouseLocationId,
      locationNameSnapshot: location?.name ?? 'StaffArr location',
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
      locationNameSnapshot: location?.name ?? 'StaffArr location',
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
  const baseRecord = record ?? fallbackSummary.unexplainedInventory[0]
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
  const locationSnapshot = location ?? fallbackSummary.locations[0]
  const itemSnapshot = items.find((item) => item.supplyarrItemId === form.supplyarrItemId) ?? items[0]
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
  const locationSnapshot = location ?? fallbackSummary.locations[0]
  const itemSnapshot = items.find((item) => item.supplyarrItemId === form.supplyarrItemId) ?? items[0]
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
