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
  question: string
  strongestMarketExamples: string
  marketReality: string
  stlAnswer: string
}

export const MARKET_COMPARISON_LEAD =
  'STL Compliance is not positioned as a deeper WMS, CMMS, LMS, WFM, TMS, or telematics product than the category leaders. The comparison below shows the real tradeoff: specialists usually win inside one department, while STL is built for cross-functional operational readiness and compliance proof.'

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
    id: 'single-category-depth',
    question: 'Who wins when the buyer needs deep single-category functionality?',
    strongestMarketExamples:
      'Manhattan or Blue Yonder for WMS; IBM Maximo or UpKeep for maintenance; Cornerstone or Docebo for LMS; UKG or Dayforce for WFM; Oracle or Samsara for transport/fleet.',
    marketReality:
      'Category leaders are deeper in their native domains and often bring larger ecosystems, implementation partner networks, and specialized workflows.',
    stlAnswer:
      'STL wins only when the buying problem crosses departments: people, training, assets, routes, supply, warehouse work, and compliance proof need to agree before work starts.',
  },
  {
    id: 'work-readiness',
    question: 'Can the platform answer “should this work start now?” across people, equipment, supply, route, and rules?',
    strongestMarketExamples:
      'A best-of-breed stack can answer this through integrations, data warehouses, middleware, and manual operating rules.',
    marketReality:
      'The more systems involved, the more the final readiness answer often depends on brittle handoffs, duplicate records, or local spreadsheets.',
    stlAnswer:
      'STL is designed around the cross-functional readiness question: qualified worker, ready asset, available supply, dispatch context, and evidence expectations in one operating model.',
  },
  {
    id: 'audit-package',
    question: 'Can the audit package come from the workflow history rather than a scramble after the fact?',
    strongestMarketExamples:
      'Specialist products can export their own records; enterprise platforms often have reporting and analytics modules.',
    marketReality:
      'Cross-domain evidence still has to be assembled when training, maintenance, dispatch, purchasing, and compliance facts live in different systems.',
    stlAnswer:
      'STL’s audit story is cross-product: evidence originates in the workflow that created it and can be packaged around the operational event or compliance question.',
  },
  {
    id: 'replacement-vs-layer',
    question: 'Should STL replace every specialist?',
    strongestMarketExamples:
      'No. Deep WMS, EAM, LMS, WFM, TMS, and telematics systems are valid when their domain is the center of gravity.',
    marketReality:
      'Large buyers may keep best-of-breed systems because category depth, existing contracts, devices, integrations, or established operating process matter.',
    stlAnswer:
      'STL should be sold as the integrated compliance and readiness layer where operational authorization, evidence, and cross-functional handoffs matter most.',
  },
]
