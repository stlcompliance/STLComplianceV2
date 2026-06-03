export type MarketProductComparison = {
  id: string
  category: string
  product: string
  bestAt: string
  buyerFit: string
  stlDifference: string
  sourceLabel: string
  sourceHref: string
}

export type MarketChecklistRow = {
  id: string
  feature: string
  stlCoverage: string
  typicalMarketCoverage: string
  stlAdvantage: string
  competitorExamples: string
}

export const MARKET_COMPARISON_LEAD =
  'Use this as a buying checklist. Category tools usually solve one department deeply. STL Compliance is biased toward the cross-functional features that decide whether work can start, continue, and be proven later.'

export const MARKET_PRODUCT_COMPARISONS: MarketProductComparison[] = [
  {
    id: 'manhattan-active-wm',
    category: 'WMS',
    product: 'Manhattan Active Warehouse Management',
    bestAt:
      'High-volume warehouse execution, order streaming, distribution center flexibility, and cloud-native WMS scale.',
    buyerFit:
      'Best fit when the main buying problem is deep warehouse control, fulfillment throughput, labor orchestration, and DC complexity.',
    stlDifference:
      'STL is broader around readiness. LoadArr covers inventory and warehouse proof, but the suite differentiates when warehouse work depends on worker qualifications, maintenance release, supplier evidence, dispatch readiness, and audit packaging.',
    sourceLabel: 'Manhattan WMS',
    sourceHref: 'https://www.manh.com/products/warehouse-management',
  },
  {
    id: 'blue-yonder-wms',
    category: 'WMS',
    product: 'Blue Yonder Warehouse Management',
    bestAt:
      'Tier-1 warehouse management across receiving, putaway, picking, packing, shipping, labor, and automation-heavy distribution.',
    buyerFit:
      'Best fit when the buyer needs a mature enterprise WMS for complex distribution center operations.',
    stlDifference:
      'STL should not pretend to be a robotics-first WMS. Its stronger story is connecting inventory and warehouse proof to workforce, maintenance, supply, dispatch, and compliance decisions.',
    sourceLabel: 'Blue Yonder WMS',
    sourceHref: 'https://blueyonder.com/solutions/warehouse-management',
  },
  {
    id: 'ibm-maximo',
    category: 'EAM / CMMS',
    product: 'IBM Maximo Application Suite',
    bestAt:
      'Enterprise asset management, inspections, maintenance, reliability planning, asset monitoring, and predictive maintenance.',
    buyerFit:
      'Best fit for asset-intensive organizations where maintenance depth, reliability engineering, and asset lifecycle management are the center of gravity.',
    stlDifference:
      'MaintainArr covers maintenance readiness, but STL’s difference is the operational gate around the asset: worker qualification, part availability, route use, compliance rules, and evidence export in one suite.',
    sourceLabel: 'IBM Maximo',
    sourceHref: 'https://www.ibm.com/products/maximo',
  },
  {
    id: 'upkeep',
    category: 'CMMS',
    product: 'UpKeep Maintenance / Asset Operations',
    bestAt:
      'Mobile-first work orders, preventive maintenance, inspections, asset tracking, parts inventory, and frontline maintenance execution.',
    buyerFit:
      'Best fit when the buyer wants a practical maintenance platform with strong technician adoption and CMMS workflows.',
    stlDifference:
      'STL competes when maintenance is only one readiness input. MaintainArr can connect repairs and inspections to dispatch, purchasing, workforce eligibility, and compliance proof.',
    sourceLabel: 'UpKeep CMMS',
    sourceHref: 'https://upkeep.com/product/cmms-software/',
  },
  {
    id: 'cornerstone-learning',
    category: 'LMS',
    product: 'Cornerstone Learning Management',
    bestAt:
      'Enterprise learning, content subscriptions, compliance monitoring, ILT management, learning assignment, and learning ecosystem integrations.',
    buyerFit:
      'Best fit when the main problem is learning scale, content administration, skill development, and enterprise learning operations.',
    stlDifference:
      'TrainArr is stronger as an operational qualification system than a content marketplace. STL turns training proof into assignment, dispatch, maintenance, and compliance readiness signals.',
    sourceLabel: 'Cornerstone Learning',
    sourceHref: 'https://www.cornerstoneondemand.com/platform/learning/',
  },
  {
    id: 'docebo',
    category: 'LMS',
    product: 'Docebo Learning Platform',
    bestAt:
      'Compliance training, enrollment rules, learning analytics, certification and retraining workflows, and internal or external education programs.',
    buyerFit:
      'Best fit when the buyer needs a dedicated LMS for employee, partner, customer, or government-scale learning programs.',
    stlDifference:
      'STL is not trying to out-LMS Docebo. Its advantage is tying certificates, signoffs, evaluations, and qualifications to real operational gates.',
    sourceLabel: 'Docebo Compliance Training',
    sourceHref: 'https://www.docebo.com/?solution=compliance-training',
  },
  {
    id: 'ukg-pro-wfm',
    category: 'WFM / HCM',
    product: 'UKG Pro Workforce Management',
    bestAt:
      'Workforce management, scheduling, time, people data, workforce visibility, and HR/payroll-adjacent controls.',
    buyerFit:
      'Best fit when labor planning, scheduling, time, payroll alignment, and workforce operations are the budget center.',
    stlDifference:
      'StaffArr is not a payroll-grade WFM replacement. STL focuses on whether the person is operationally qualified and compliant for the work, then connects that answer to training, assets, dispatch, and evidence.',
    sourceLabel: 'UKG Pro',
    sourceHref: 'https://www.ukg.com/products/ukg-pro',
  },
  {
    id: 'dayforce-wfm',
    category: 'WFM / HCM',
    product: 'Dayforce Workforce Management',
    bestAt:
      'HCM, workforce management, labor decisions, HR communication, reporting, analytics, and payroll-adjacent workforce data.',
    buyerFit:
      'Best fit when the buyer needs broad HCM/WFM with labor cost, HR, pay, time, and people analytics in one people platform.',
    stlDifference:
      'STL complements this kind of system where work authorization depends on more than HR status: training proof, asset readiness, vendor documents, route conditions, inventory, and compliance rules.',
    sourceLabel: 'Dayforce WFM',
    sourceHref: 'https://www.dayforce.com/how-we-help/dayforce',
  },
  {
    id: 'samsara',
    category: 'Fleet / telematics',
    product: 'Samsara Connected Operations',
    bestAt:
      'Telematics, ELD, fleet safety, driver app workflows, dispatch and routing, vehicle maintenance, equipment monitoring, and real-time operational data.',
    buyerFit:
      'Best fit when the center of gravity is fleet visibility, safety, driver behavior, ELD/HOS compliance, GPS, cameras, and connected physical operations.',
    stlDifference:
      'RoutArr manages dispatch readiness and trip proof, but STL is not a telematics device cloud. STL’s role is to combine telematics or route signals with worker qualification, asset readiness, inventory, and audit evidence.',
    sourceLabel: 'Samsara ELD / Connected Operations',
    sourceHref: 'https://www.samsara.com/products/apps-and-workflows/eld/',
  },
  {
    id: 'oracle-transportation',
    category: 'TMS',
    product: 'Oracle Transportation Management',
    bestAt:
      'Transportation planning, logistics operations, shipment optimization, freight cost control, service levels, and enterprise transportation execution.',
    buyerFit:
      'Best fit for enterprise shippers and logistics teams with deep transportation planning, optimization, carrier, freight, and execution requirements.',
    stlDifference:
      'RoutArr is better framed as route readiness and dispatch proof inside STL’s compliance operating model, not a replacement for enterprise transportation optimization.',
    sourceLabel: 'Oracle Transportation Management',
    sourceHref: 'https://www.oracle.com/scm/logistics/transportation-management/',
  },
]

export const MARKET_CHECKLIST_ROWS: MarketChecklistRow[] = [
  {
    id: 'cross-functional-readiness',
    feature: 'Cross-functional readiness decision',
    stlCoverage:
      'Combines worker readiness, qualification proof, asset condition, route context, supply availability, warehouse state, and compliance rules.',
    typicalMarketCoverage:
      'Usually split across WFM, LMS, CMMS, WMS, TMS, procurement, and compliance tools.',
    stlAdvantage:
      'STL gives the operator one operational answer: who can work, what can run, what stock is available, what route can release, and what proof exists.',
    competitorExamples:
      'UKG/Dayforce for workforce, Cornerstone/Docebo for learning, Maximo/UpKeep for maintenance, Manhattan/Blue Yonder for WMS, Oracle/Samsara for transport.',
  },
  {
    id: 'qualification-gated-work',
    feature: 'Qualification-gated work',
    stlCoverage:
      'TrainArr creates qualification proof; StaffArr exposes readiness; MaintainArr and RoutArr can depend on qualifications before assigning jobs or trips.',
    typicalMarketCoverage:
      'LMS tools prove training completion, while maintenance and dispatch systems often need integration to use that proof operationally.',
    stlAdvantage:
      'STL turns training into an operating gate, not a separate record someone checks manually.',
    competitorExamples: 'Cornerstone, Docebo, IBM Maximo, UpKeep, Samsara, Oracle Transportation Management.',
  },
  {
    id: 'asset-release',
    feature: 'Asset and vehicle release',
    stlCoverage:
      'MaintainArr tracks inspections, defects, work orders, repairs, part usage, and asset readiness; RoutArr consumes vehicle readiness before dispatch.',
    typicalMarketCoverage:
      'CMMS/EAM systems handle maintenance depth; fleet/TMS tools handle dispatch depth; the release decision often crosses both.',
    stlAdvantage:
      'STL links maintenance proof to dispatch action so a vehicle or asset is not just repaired, but operationally cleared for the next job.',
    competitorExamples: 'IBM Maximo, UpKeep, Samsara, Oracle Transportation Management.',
  },
  {
    id: 'inventory-aware-dispatch',
    feature: 'Inventory-aware dispatch and work execution',
    stlCoverage:
      'LoadArr tracks stock, reservations, picks, shipments, counts, and inventory history; RoutArr and MaintainArr can use that state for route, load, and parts readiness.',
    typicalMarketCoverage:
      'WMS tools know inventory; TMS and maintenance tools may not naturally see the warehouse state without integration.',
    stlAdvantage:
      'STL connects stock reality to the work decision: dispatch can depend on load/stock status, and maintenance can depend on parts availability.',
    competitorExamples: 'Manhattan Active WM, Blue Yonder WMS, Oracle Transportation Management, UpKeep.',
  },
  {
    id: 'vendor-procurement-gates',
    feature: 'Vendor, purchasing, and approval gates',
    stlCoverage:
      'SupplyArr manages vendors, customers, parts, purchase requests, approvals, receiving, restrictions, incidents, and procurement exceptions.',
    typicalMarketCoverage:
      'Procurement tools manage purchasing depth; operations tools often see only whether something arrived or not.',
    stlAdvantage:
      'STL keeps vendor and approval evidence connected to maintenance, warehouse, dispatch, and compliance decisions.',
    competitorExamples: 'ERP/procurement suites, WMS, CMMS, TMS platforms.',
  },
  {
    id: 'audit-evidence',
    feature: 'Workflow-native audit evidence',
    stlCoverage:
      'StaffArr, TrainArr, MaintainArr, RoutArr, SupplyArr, LoadArr, and Compliance Core preserve evidence where the work happens.',
    typicalMarketCoverage:
      'Specialists export their own slice; cross-domain evidence usually needs reporting work, middleware, or manual assembly.',
    stlAdvantage:
      'STL packages the story across people, training, equipment, route, supply, warehouse, rules, and approvals.',
    competitorExamples: 'All named specialists can contribute records; STL is built around the joined evidence chain.',
  },
  {
    id: 'field-handoffs',
    feature: 'Field task handoffs',
    stlCoverage:
      'Companion gives workers a focused inbox and sends them to the product that owns the record.',
    typicalMarketCoverage:
      'Point tools often have their own mobile experience, but field work across departments fragments quickly.',
    stlAdvantage:
      'STL keeps field action tied to the owning workflow without turning the inbox into another system of record.',
    competitorExamples: 'Samsara driver apps, UpKeep mobile CMMS, LMS mobile apps, WMS handheld workflows.',
  },
]
