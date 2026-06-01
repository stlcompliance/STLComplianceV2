import { chromium } from '../tests/e2e-playwright/node_modules/@playwright/test/index.mjs'
import fs from 'node:fs/promises'
import path from 'node:path'

const runId = new Date().toISOString().replace(/[-:T.Z]/g, '').slice(0, 14)
const artifactRoot = path.resolve(process.cwd(), '..', '..', 'artifacts', `deployed-product-crawl-${runId}`)
const screenshotDir = path.join(artifactRoot, 'screenshots')

const suiteBaseUrl = 'https://suite-frontend.onrender.com'
const nexarrApiUrl = 'https://nexarr-api-3zlb.onrender.com'

const demoCredentials = {
  email: 'admin@demo.stl',
  password: 'ChangeMe!Demo2026',
  tenantId: '11111111-1111-1111-1111-111111111101',
}

const productConfigs = [
  {
    key: 'staffarr',
    name: 'StaffArr',
    frontendUrl: 'https://staffarr-frontend.onrender.com',
    apiUrl: 'https://staffarr-api-58w6.onrender.com',
    seed: seedStaffArr,
    routes: [
      ['/', 'Root redirect'],
      ['/me', 'Me portal'],
      ['/my-team', 'My team'],
      ['/people', 'People directory'],
      ['/people/drawer', 'People drawer'],
      ['/people/details', 'People details'],
      ['/people/create', 'People create'],
      ['/org', 'Org chart'],
      ['/permissions', 'Permissions'],
      ['/readiness', 'Readiness'],
      ['/incidents', 'Incidents'],
      ['/training-acknowledgements', 'Training acknowledgements'],
      ['/certifications', 'Certifications'],
      ['/reports', 'Reports'],
      ['/admin', 'Admin'],
    ],
  },
  {
    key: 'trainarr',
    name: 'TrainArr',
    frontendUrl: 'https://trainarr-frontend.onrender.com',
    apiUrl: 'https://trainarr-api-ieni.onrender.com',
    seed: seedTrainArr,
    routes: [
      ['/', 'Root redirect'],
      ['/programs', 'Programs'],
      ['/programs/drawer', 'Programs drawer'],
      ['/programs/details', 'Programs details'],
      ['/programs/create', 'Programs create'],
      ['/assignments', 'Assignments'],
      ['/assignments/manual', 'Manual assignments'],
      ['/assignments/queue', 'Assignment queue'],
      ['/assignments/evaluation', 'Assignment evaluation'],
      ['/assignments/{trainingAssignmentId}', 'Assignment detail'],
      ['/assignments/{trainingAssignmentId}/evidence', 'Assignment evidence'],
      ['/remediation', 'Remediation'],
      ['/citations', 'Citations'],
      ['/rule-packs', 'Rule packs'],
      ['/rule-packs/drawer', 'Rule packs drawer'],
      ['/rule-packs/details', 'Rule packs details'],
      ['/rule-packs/create', 'Rule packs create'],
      ['/qualifications', 'Qualifications'],
      ['/reports', 'Reports'],
      ['/settings', 'Settings'],
    ],
  },
  {
    key: 'maintainarr',
    name: 'MaintainArr',
    frontendUrl: 'https://maintainarr-frontend.onrender.com',
    apiUrl: 'https://maintainarr-api-gx03.onrender.com',
    seed: seedMaintainArr,
    routes: [
      ['/', 'Root redirect'],
      ['/overview', 'Overview'],
      ['/assets', 'Assets'],
      ['/assets/drawer', 'Assets drawer'],
      ['/assets/details', 'Assets details'],
      ['/assets/create', 'Assets create'],
      ['/pm-programs', 'PM programs'],
      ['/pm-programs/drawer', 'PM programs drawer'],
      ['/pm-programs/details', 'PM programs details'],
      ['/pm-programs/create', 'PM programs create'],
      ['/meters', 'Meters'],
      ['/meters/drawer', 'Meters drawer'],
      ['/meters/details', 'Meters details'],
      ['/meters/create', 'Meters create'],
      ['/work-orders', 'Work orders'],
      ['/work-orders/drawer', 'Work orders drawer'],
      ['/work-orders/details', 'Work orders details'],
      ['/work-orders/create', 'Work orders create'],
      ['/work-orders/{workOrderId}', 'Work order detail'],
      ['/defects', 'Defects'],
      ['/defects/drawer', 'Defects drawer'],
      ['/defects/details', 'Defects details'],
      ['/defects/create', 'Defects create'],
      ['/inspections', 'Inspections'],
      ['/inspections/drawer', 'Inspections drawer'],
      ['/inspections/details', 'Inspections details'],
      ['/inspections/create', 'Inspections create'],
      ['/inspection-templates', 'Inspection templates'],
      ['/inspection-templates/drawer', 'Inspection templates drawer'],
      ['/inspection-templates/details', 'Inspection templates details'],
      ['/inspection-templates/create', 'Inspection templates create'],
      ['/history', 'History'],
      ['/downtime', 'Downtime'],
      ['/reports', 'Reports'],
      ['/reports/compliance', 'Compliance reports'],
      ['/reports/executive', 'Executive reports'],
      ['/reports/maintenance', 'Maintenance reports'],
      ['/reports/exports', 'Report exports'],
      ['/settings', 'Settings'],
    ],
  },
  {
    key: 'routarr',
    name: 'RoutArr',
    frontendUrl: 'https://routarr-frontend.onrender.com',
    apiUrl: 'https://routarr-api-nmwr.onrender.com',
    seed: seedRoutArr,
    routes: [
      ['/', 'Root redirect'],
      ['/dispatch', 'Dispatch'],
      ['/driver-portal', 'Driver portal'],
      ['/trips', 'Trips'],
      ['/trips/drawer', 'Trips drawer'],
      ['/trips/details', 'Trips details'],
      ['/trips/create', 'Trips create'],
      ['/trips/{tripId}', 'Trip detail'],
      ['/routes', 'Routes'],
      ['/routes/drawer', 'Routes drawer'],
      ['/routes/details', 'Routes details'],
      ['/routes/create', 'Routes create'],
      ['/availability', 'Availability'],
      ['/calendar', 'Calendar'],
      ['/reports', 'Reports'],
      ['/settings', 'Settings'],
    ],
  },
  {
    key: 'supplyarr',
    name: 'SupplyArr',
    frontendUrl: 'https://supplyarr-frontend.onrender.com',
    apiUrl: 'https://supplyarr-api-gavo.onrender.com',
    seed: seedSupplyArr,
    routes: [
      ['/', 'Root redirect'],
      ['/parties', 'Parties'],
      ['/parties/drawer', 'Parties drawer'],
      ['/parties/details', 'Parties details'],
      ['/parties/create', 'Parties create'],
      ['/catalog', 'Catalog'],
      ['/inventory', 'Inventory'],
      ['/purchasing', 'Purchasing'],
      ['/purchasing/procurement', 'Procurement'],
      ['/purchasing/approvals', 'Approvals'],
      ['/purchasing/exceptions', 'Exceptions'],
      ['/receiving', 'Receiving'],
      ['/receiving/{receivingReceiptId}', 'Receiving detail'],
      ['/pricing', 'Pricing'],
      ['/planning', 'Planning'],
      ['/readiness', 'Readiness'],
      ['/reports', 'Reports'],
      ['/settings', 'Settings'],
    ],
  },
  {
    key: 'compliancecore',
    name: 'Compliance Core',
    frontendUrl: 'https://compliancecore-frontend.onrender.com',
    apiUrl: 'https://compliancecore-api-h69n.onrender.com',
    seed: seedComplianceCore,
    routes: [
      ['/', 'Root redirect'],
      ['/registry', 'Registry'],
      ['/registry/drawer', 'Registry drawer'],
      ['/registry/details', 'Registry details'],
      ['/registry/create', 'Registry create'],
      ['/mappings', 'Mappings'],
      ['/findings', 'Findings'],
      ['/evaluation', 'Evaluation'],
      ['/fact-sources', 'Fact sources'],
      ['/operator', 'Operator'],
      ['/reports', 'Reports'],
      ['/admin', 'Admin'],
    ],
  },
  {
    key: 'companion',
    name: 'Companion',
    frontendUrl: 'https://companion-frontend-b2nm.onrender.com',
    apiUrl: null,
    seed: null,
    routes: [
      ['/', 'Home'],
      ['/launch', 'Launch'],
    ],
  },
]

const createdData = []
const dataErrors = []
const networkConsoleErrors = []
const consoleWarnings = []
const routeResults = []
let routeCounter = 0

function nowIso() {
  return new Date().toISOString()
}

function slugify(value) {
  return value
    .toLowerCase()
    .replace(/[^a-z0-9]+/g, '-')
    .replace(/^-|-$/g, '')
    .slice(0, 110)
}

async function ensureDirs() {
  await fs.mkdir(screenshotDir, { recursive: true })
}

async function readJson(response, label) {
  if (!response.ok) {
    const body = await response.text().catch(() => '')
    throw new Error(`${label} failed with HTTP ${response.status}: ${body.slice(0, 700)}`)
  }
  return response.json()
}

async function loginNexArrApi() {
  const response = await fetch(`${nexarrApiUrl}/api/auth/login`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(demoCredentials),
  })
  const payload = await readJson(response, 'NexArr login')
  return payload.accessToken
}

async function redeemProductApiToken(product) {
  if (!product.apiUrl) {
    throw new Error(`No API URL configured for ${product.key}`)
  }
  const nexarrToken = await loginNexArrApi()
  const handoffResponse = await fetch(`${nexarrApiUrl}/api/v1/launch/handoff`, {
    method: 'POST',
    headers: {
      Authorization: `Bearer ${nexarrToken}`,
      'Content-Type': 'application/json',
    },
    body: JSON.stringify({
      productKey: product.key,
      callbackUrl: `${product.frontendUrl}/launch`,
    }),
  })
  const handoff = await readJson(handoffResponse, `${product.name} handoff create`)
  const redeemResponse = await fetch(`${product.apiUrl}/api/auth/handoff/redeem`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ handoffCode: handoff.handoffCode }),
  })
  const redeem = await readJson(redeemResponse, `${product.name} handoff redeem`)
  return redeem.accessToken
}

function authHeaders(token) {
  return { Authorization: `Bearer ${token}` }
}

function jsonHeaders(token) {
  return { Authorization: `Bearer ${token}`, 'Content-Type': 'application/json' }
}

async function seedStaffArr(product, token) {
  const suffix = `crawl-${Date.now()}`
  const person = await readJson(
    await fetch(`${product.apiUrl}/api/people`, {
      method: 'POST',
      headers: jsonHeaders(token),
      body: JSON.stringify({
        givenName: 'Route',
        familyName: `Crawl ${suffix}`,
        primaryEmail: `route.crawl.${Date.now()}@example.test`,
        employmentStatus: 'active',
        primaryOrgUnitId: null,
        managerPersonId: null,
        jobTitle: 'QA route auditor',
      }),
    }),
    'StaffArr person create',
  )
  return [{ type: 'staffarr-person', productKey: product.key, personId: person.personId, displayName: person.displayName }]
}

async function seedTrainArr(product, token) {
  const journey = await readJson(
    await fetch(`${product.apiUrl}/api/load-test-journey/seed`, {
      method: 'POST',
      headers: authHeaders(token),
    }),
    'TrainArr journey seed',
  )
  const assignment = await readJson(
    await fetch(`${product.apiUrl}/api/training-assignments`, {
      method: 'POST',
      headers: jsonHeaders(token),
      body: JSON.stringify({
        staffarrPersonId: '22222222-2222-2222-2222-222222222201',
        trainingDefinitionId: journey.trainingDefinitionId,
        staffarrIncidentRemediationId: null,
        assignmentReason: 'recertification',
        dueAt: null,
      }),
    }),
    'TrainArr assignment create',
  )
  return [
    { type: 'trainarr-journey-seed', productKey: product.key, ...journey },
    { type: 'trainarr-training-assignment', productKey: product.key, trainingAssignmentId: assignment.assignmentId },
  ]
}

async function seedMaintainArr(product, token) {
  const suffix = Date.now()
  const assetClass = await readJson(
    await fetch(`${product.apiUrl}/api/asset-classes`, {
      method: 'POST',
      headers: jsonHeaders(token),
      body: JSON.stringify({
        classKey: `route-crawl-class-${suffix}`,
        name: 'Route crawl asset class',
        description: 'Created by deployed route crawl',
      }),
    }),
    'MaintainArr asset class create',
  )
  const assetType = await readJson(
    await fetch(`${product.apiUrl}/api/asset-types`, {
      method: 'POST',
      headers: jsonHeaders(token),
      body: JSON.stringify({
        assetClassId: assetClass.assetClassId,
        typeKey: `route-crawl-type-${suffix}`,
        name: 'Route crawl asset type',
        description: 'Created by deployed route crawl',
      }),
    }),
    'MaintainArr asset type create',
  )
  const asset = await readJson(
    await fetch(`${product.apiUrl}/api/assets`, {
      method: 'POST',
      headers: jsonHeaders(token),
      body: JSON.stringify({
        assetTypeId: assetType.assetTypeId,
        assetTag: `RC-${suffix}`,
        name: 'Route crawl asset',
        locationLabel: 'QA bay',
        notes: null,
      }),
    }),
    'MaintainArr asset create',
  )
  const workOrder = await readJson(
    await fetch(`${product.apiUrl}/api/work-orders`, {
      method: 'POST',
      headers: jsonHeaders(token),
      body: JSON.stringify({
        assetId: asset.assetId,
        title: `Route crawl work order ${suffix}`,
        description: 'Created by deployed route crawl',
        priority: 'medium',
        assignedTechnicianPersonId: null,
        pmScheduleId: null,
      }),
    }),
    'MaintainArr work order create',
  )
  return [
    { type: 'maintainarr-asset-class', productKey: product.key, assetClassId: assetClass.assetClassId },
    { type: 'maintainarr-asset-type', productKey: product.key, assetTypeId: assetType.assetTypeId },
    { type: 'maintainarr-asset', productKey: product.key, assetId: asset.assetId },
    { type: 'maintainarr-work-order', productKey: product.key, workOrderId: workOrder.workOrderId, workOrderNumber: workOrder.workOrderNumber },
  ]
}

async function seedRoutArr(product, token) {
  const seed = await readJson(
    await fetch(`${product.apiUrl}/api/load-test-journey/seed`, {
      method: 'POST',
      headers: authHeaders(token),
    }),
    'RoutArr journey seed',
  )
  return [{ type: 'routarr-journey-seed', productKey: product.key, ...seed }]
}

async function seedSupplyArr(product, token) {
  const suffix = Date.now()
  const vendor = await readJson(
    await fetch(`${product.apiUrl}/api/vendors`, {
      method: 'POST',
      headers: jsonHeaders(token),
      body: JSON.stringify({
        partyKey: `route-crawl-vendor-${suffix}`,
        displayName: 'Route crawl vendor',
        legalName: 'Route Crawl Vendor LLC',
        contactEmail: null,
        notes: '',
      }),
    }),
    'SupplyArr vendor create',
  )
  const part = await readJson(
    await fetch(`${product.apiUrl}/api/parts`, {
      method: 'POST',
      headers: jsonHeaders(token),
      body: JSON.stringify({
        partKey: `route-crawl-part-${suffix}`,
        catalogId: null,
        displayName: 'Route crawl part',
        description: '',
        categoryKey: 'general',
        unitOfMeasure: 'each',
        manufacturerName: '',
        manufacturerPartNumber: '',
      }),
    }),
    'SupplyArr part create',
  )
  const location = await readJson(
    await fetch(`${product.apiUrl}/api/inventory/locations`, {
      method: 'POST',
      headers: jsonHeaders(token),
      body: JSON.stringify({
        locationKey: `route-crawl-loc-${suffix}`,
        name: 'Route crawl warehouse',
        locationType: 'warehouse',
        addressLine: 'Dock',
      }),
    }),
    'SupplyArr location create',
  )
  const bin = await readJson(
    await fetch(`${product.apiUrl}/api/inventory/locations/${location.locationId}/bins`, {
      method: 'POST',
      headers: jsonHeaders(token),
      body: JSON.stringify({ binKey: `route-crawl-bin-${suffix}`, name: 'Route crawl bin' }),
    }),
    'SupplyArr bin create',
  )
  const purchaseRequest = await readJson(
    await fetch(`${product.apiUrl}/api/purchase-requests`, {
      method: 'POST',
      headers: jsonHeaders(token),
      body: JSON.stringify({
        requestKey: `route-crawl-pr-${suffix}`,
        title: 'Route crawl receiving PR',
        notes: '',
        vendorPartyId: vendor.partyId,
        lines: [{ partId: part.partId, quantityRequested: 3, notes: '' }],
      }),
    }),
    'SupplyArr purchase request create',
  )
  await readJson(
    await fetch(`${product.apiUrl}/api/purchase-requests/${purchaseRequest.purchaseRequestId}/submit`, {
      method: 'POST',
      headers: authHeaders(token),
    }),
    'SupplyArr purchase request submit',
  )
  await readJson(
    await fetch(`${product.apiUrl}/api/purchase-requests/${purchaseRequest.purchaseRequestId}/approve`, {
      method: 'POST',
      headers: authHeaders(token),
    }),
    'SupplyArr purchase request approve',
  )
  const purchaseOrder = await readJson(
    await fetch(`${product.apiUrl}/api/purchase-orders/from-purchase-request/${purchaseRequest.purchaseRequestId}`, {
      method: 'POST',
      headers: jsonHeaders(token),
      body: JSON.stringify({
        orderKey: `route-crawl-po-${suffix}`,
        notes: null,
        requestedDeliveryAt: null,
      }),
    }),
    'SupplyArr purchase order create',
  )
  await readJson(
    await fetch(`${product.apiUrl}/api/purchase-orders/${purchaseOrder.purchaseOrderId}/approve`, {
      method: 'POST',
      headers: authHeaders(token),
    }),
    'SupplyArr purchase order approve',
  )
  await readJson(
    await fetch(`${product.apiUrl}/api/purchase-orders/${purchaseOrder.purchaseOrderId}/issue`, {
      method: 'POST',
      headers: authHeaders(token),
    }),
    'SupplyArr purchase order issue',
  )
  const receipt = await readJson(
    await fetch(`${product.apiUrl}/api/receiving/from-purchase-order/${purchaseOrder.purchaseOrderId}`, {
      method: 'POST',
      headers: jsonHeaders(token),
      body: JSON.stringify({
        receiptKey: `route-crawl-rcpt-${suffix}`,
        inventoryBinId: bin.binId,
        notes: 'Created by deployed route crawl',
      }),
    }),
    'SupplyArr receiving receipt create',
  )
  return [
    { type: 'supplyarr-vendor', productKey: product.key, partyId: vendor.partyId },
    { type: 'supplyarr-part', productKey: product.key, partId: part.partId },
    { type: 'supplyarr-location', productKey: product.key, locationId: location.locationId },
    { type: 'supplyarr-bin', productKey: product.key, binId: bin.binId },
    { type: 'supplyarr-purchase-request', productKey: product.key, purchaseRequestId: purchaseRequest.purchaseRequestId },
    { type: 'supplyarr-purchase-order', productKey: product.key, purchaseOrderId: purchaseOrder.purchaseOrderId },
    { type: 'supplyarr-receiving-receipt', productKey: product.key, receivingReceiptId: receipt.receivingReceiptId },
  ]
}

async function seedComplianceCore(product, token) {
  const seed = await readJson(
    await fetch(`${product.apiUrl}/api/load-test-journey/seed`, {
      method: 'POST',
      headers: authHeaders(token),
    }),
    'Compliance Core journey seed',
  )
  return [{ type: 'compliancecore-journey-seed', productKey: product.key, ...seed }]
}

async function seedProducts() {
  for (const product of productConfigs) {
    if (!product.seed) {
      createdData.push({ type: 'data-create-skipped', productKey: product.key, reason: 'No direct product API seed path configured for this crawler' })
      continue
    }
    try {
      const token = await redeemProductApiToken(product)
      const created = await product.seed(product, token)
      createdData.push(...created)
    } catch (error) {
      dataErrors.push({
        at: nowIso(),
        productKey: product.key,
        message: error instanceof Error ? error.message : String(error),
      })
    }
  }
}

async function signInFromSuite(page) {
  await page.goto(`${suiteBaseUrl}/login`, { waitUntil: 'domcontentloaded' })
  await page.getByLabel('Email').fill(demoCredentials.email)
  await page.getByLabel('Password').fill(demoCredentials.password)
  await page.getByRole('button', { name: 'Sign in' }).click()
  await page.getByRole('heading', { name: /Welcome,/ }).waitFor({ timeout: 30_000 })
}

function attachMonitoring(page) {
  page.on('requestfailed', (request) => {
    const failure = request.failure()
    networkConsoleErrors.push({
      at: nowIso(),
      kind: 'requestfailed',
      method: request.method(),
      url: request.url(),
      resourceType: request.resourceType(),
      failureText: failure?.errorText ?? 'unknown',
      pageUrl: page.url(),
    })
  })
  page.on('response', (response) => {
    if (response.status() < 400) {
      return
    }
    const request = response.request()
    networkConsoleErrors.push({
      at: nowIso(),
      kind: 'http-status',
      status: response.status(),
      statusText: response.statusText(),
      method: request.method(),
      url: response.url(),
      resourceType: request.resourceType(),
      pageUrl: page.url(),
    })
  })
  page.on('console', (message) => {
    const type = message.type()
    if (type !== 'error' && type !== 'warning') {
      return
    }
    const item = {
      at: nowIso(),
      kind: 'console',
      type,
      text: message.text(),
      location: message.location(),
      pageUrl: page.url(),
    }
    if (type === 'error') {
      networkConsoleErrors.push(item)
    } else {
      consoleWarnings.push(item)
    }
  })
  page.on('pageerror', (error) => {
    networkConsoleErrors.push({
      at: nowIso(),
      kind: 'pageerror',
      message: error.message,
      stack: error.stack,
      pageUrl: page.url(),
    })
  })
}

async function launchProductFromSuite(page, product) {
  await page.goto(`${suiteBaseUrl}/app/${product.key}/launch`, { waitUntil: 'domcontentloaded' })
  const launchButton = page.getByRole('button', { name: 'Launch product (handoff)' })
  await launchButton.waitFor({ state: 'visible', timeout: 30_000 })
  await Promise.all([
    page.waitForURL((url) => url.href.startsWith(product.frontendUrl), { timeout: 60_000 }),
    launchButton.click(),
  ])
  await page.waitForFunction(() => !window.location.href.includes('handoff='), undefined, {
    timeout: 45_000,
  })
  await page.waitForLoadState('networkidle', { timeout: 12_000 }).catch(() => {})
}

function routePathForProduct(rawPath) {
  let routePath = rawPath
  for (const item of createdData) {
    for (const [key, value] of Object.entries(item)) {
      if (key.endsWith('Id') && typeof value === 'string') {
        routePath = routePath.replaceAll(`{${key}}`, value)
      }
    }
  }
  return routePath
}

async function detectVisibleErrors(page) {
  return page.evaluate(() => {
    const text = document.body?.innerText ?? ''
    const patterns = [
      /Application error[^\n]*/i,
      /Something went wrong[^\n]*/i,
      /Failed to load[^\n]*/i,
      /Unable to load[^\n]*/i,
      /Not found[^\n]*/i,
      /404[^\n]*/i,
      /500[^\n]*/i,
      /profile missing[^\n]*/i,
      /error loading[^\n]*/i,
      /Cannot launch[^\n]*/i,
    ]
    const matches = []
    for (const pattern of patterns) {
      const match = text.match(pattern)
      if (match?.[0]) {
        matches.push(match[0].slice(0, 240))
      }
    }
    return Array.from(new Set(matches)).slice(0, 5)
  })
}

async function captureRoute(page, product, routePath, label) {
  routeCounter += 1
  const normalizedPath = routePathForProduct(routePath)
  const unresolved = /\{[^}]+\}/.test(normalizedPath)
  const filename = `${String(routeCounter).padStart(3, '0')}-${slugify(product.key)}-${slugify(label || normalizedPath)}.png`
  const screenshotPath = path.join(screenshotDir, filename)
  if (unresolved) {
    routeResults.push({
      index: routeCounter,
      productKey: product.key,
      label,
      path: routePath,
      status: 'skipped',
      reason: 'Route contains an unresolved seeded id placeholder',
    })
    return
  }

  const routeErrorsStart = networkConsoleErrors.length
  const routeWarningsStart = consoleWarnings.length
  const targetUrl = new URL(normalizedPath, product.frontendUrl).toString()
  const result = {
    index: routeCounter,
    productKey: product.key,
    label,
    path: normalizedPath,
    url: targetUrl,
    finalUrl: null,
    screenshot: `screenshots/${filename}`,
    visibleErrors: [],
    status: 'ok',
    message: null,
    newErrorCount: 0,
    newWarningCount: 0,
  }

  try {
    await page.goto(targetUrl, { waitUntil: 'domcontentloaded', timeout: 45_000 })
    await page.waitForLoadState('networkidle', { timeout: 12_000 }).catch(() => {})
    await page.waitForTimeout(600)
    result.finalUrl = page.url()
    result.visibleErrors = await detectVisibleErrors(page)
    if (result.visibleErrors.length > 0) {
      result.status = 'visible-error'
    }
    await page.screenshot({ path: screenshotPath, fullPage: true })
  } catch (error) {
    result.status = 'navigation-error'
    result.message = error instanceof Error ? error.message : String(error)
    try {
      await page.screenshot({ path: screenshotPath, fullPage: true })
    } catch {}
  } finally {
    result.newErrorCount = networkConsoleErrors.length - routeErrorsStart
    result.newWarningCount = consoleWarnings.length - routeWarningsStart
    routeResults.push(result)
  }
}

async function crawlProducts(page) {
  for (const product of productConfigs) {
    try {
      await launchProductFromSuite(page, product)
      routeResults.push({
        index: null,
        productKey: product.key,
        label: 'Launch handoff',
        path: '/launch',
        url: page.url(),
        status: 'launch-ok',
      })
    } catch (error) {
      routeResults.push({
        index: null,
        productKey: product.key,
        label: 'Launch handoff',
        path: '/launch',
        status: 'launch-error',
        message: error instanceof Error ? error.message : String(error),
      })
      continue
    }

    for (const [routePath, label] of product.routes) {
      await captureRoute(page, product, routePath, label)
    }
  }
}

function groupByProduct(results) {
  return productConfigs.map((product) => {
    const scoped = results.filter((result) => result.productKey === product.key && result.index !== null)
    return {
      productKey: product.key,
      name: product.name,
      routes: scoped.length,
      ok: scoped.filter((result) => result.status === 'ok').length,
      visibleErrors: scoped.filter((result) => result.status === 'visible-error').length,
      navigationErrors: scoped.filter((result) => result.status === 'navigation-error').length,
      skipped: scoped.filter((result) => result.status === 'skipped').length,
      networkConsoleErrors: scoped.reduce((sum, result) => sum + (result.newErrorCount ?? 0), 0),
    }
  })
}

async function writeJson(name, value) {
  await fs.writeFile(path.join(artifactRoot, name), JSON.stringify(value, null, 2))
}

async function writeReport() {
  const screenshots = routeResults.filter((result) => result.screenshot)
  const grouped = groupByProduct(routeResults)
  const lines = [
    '# STL Suite product crawl',
    '',
    `Run: ${path.basename(artifactRoot).replace('deployed-product-crawl-', '')}`,
    `Suite base: ${suiteBaseUrl}`,
    `Generated: ${nowIso()}`,
    '',
    '## Summary',
    '',
    `- Product routes/screenshots: ${screenshots.length}`,
    `- Product/network/console errors: ${networkConsoleErrors.length}`,
    `- Console warnings: ${consoleWarnings.length}`,
    `- Created records: ${createdData.filter((item) => item.type !== 'data-create-skipped').length}`,
    `- Data creation errors: ${dataErrors.length}`,
    '',
    '## Products',
    '',
    '| Product | Routes | OK | Visible errors | Navigation errors | Skipped | Route error events |',
    '| --- | ---: | ---: | ---: | ---: | ---: | ---: |',
    ...grouped.map((item) =>
      `| ${item.name} | ${item.routes} | ${item.ok} | ${item.visibleErrors} | ${item.navigationErrors} | ${item.skipped} | ${item.networkConsoleErrors} |`,
    ),
    '',
    '## Created Data',
    '',
    ...createdData.map((item) => `- ${item.type}: ${JSON.stringify(item)}`),
    ...(createdData.length === 0 ? ['- None'] : []),
    '',
    '## Data Creation Errors',
    '',
    ...dataErrors.map((item) => `- ${item.productKey}: ${item.message}`),
    ...(dataErrors.length === 0 ? ['- None'] : []),
    '',
    '## Route Screenshots',
    '',
    ...routeResults.map((result) => {
      if (result.index === null) {
        return `- ${result.productKey}: ${result.label} -> ${result.status}${result.message ? ` [${result.message}]` : ''}`
      }
      const bits = [`${String(result.index).padStart(3, '0')}. ${result.productKey} / ${result.label} (${result.path})`]
      if (result.screenshot) bits.push(`-> ${result.screenshot}`)
      if (result.status !== 'ok') bits.push(`[${result.status}]`)
      if (result.visibleErrors?.length) bits.push(`[visible error: ${result.visibleErrors.join(' | ')}]`)
      if (result.message) bits.push(`[${result.message}]`)
      if (result.newErrorCount) bits.push(`[route error events: ${result.newErrorCount}]`)
      return `- ${bits.join(' ')}`
    }),
    '',
    '## Network / Console / Page Errors',
    '',
    ...networkConsoleErrors.map((item) => `- ${item.kind}${item.status ? ` ${item.status}` : ''}: ${item.url ?? item.text ?? item.message} (${item.pageUrl ?? ''})`),
    ...(networkConsoleErrors.length === 0 ? ['- None'] : []),
    '',
  ]
  await fs.writeFile(path.join(artifactRoot, 'report.md'), lines.join('\n'))
}

function imageDimensionsHtml(src) {
  return `<img src="${src}" loading="lazy">`
}

async function writeGallery(browser) {
  const screenshotResults = routeResults.filter((result) => result.screenshot)
  const cards = screenshotResults
    .map((result) => {
      const src = result.screenshot.replaceAll('\\', '/')
      const title = `${String(result.index).padStart(3, '0')} ${result.productKey} ${result.label}`
      const statusClass = result.status === 'ok' ? 'ok' : 'bad'
      return `<article class="card ${statusClass}"><h2>${escapeHtml(title)}</h2><p>${escapeHtml(result.path)}</p>${imageDimensionsHtml(src)}</article>`
    })
    .join('\n')
  const html = `<!doctype html>
<html>
<head>
  <meta charset="utf-8">
  <title>Product crawl screenshots</title>
  <style>
    body { margin: 0; font-family: Arial, sans-serif; background: #f5f7fb; color: #172033; }
    header { padding: 24px 28px 10px; }
    h1 { margin: 0 0 8px; font-size: 24px; }
    .grid { display: grid; grid-template-columns: repeat(3, minmax(0, 1fr)); gap: 14px; padding: 18px 28px 28px; }
    .card { background: white; border: 1px solid #d7deea; border-radius: 8px; overflow: hidden; box-shadow: 0 2px 8px rgba(23,32,51,0.08); }
    .card.bad { border-color: #d85b66; }
    .card h2 { margin: 0; padding: 10px 12px 2px; font-size: 13px; line-height: 1.25; }
    .card p { margin: 0; padding: 0 12px 10px; font-size: 11px; color: #5d6a7e; }
    .card img { display: block; width: 100%; height: 320px; object-fit: contain; object-position: top center; border-top: 1px solid #e6ebf3; background: #ffffff; }
  </style>
</head>
<body>
  <header>
    <h1>STL Suite deployed product crawl</h1>
    <div>${screenshotResults.length} screenshots · ${networkConsoleErrors.length} errors · ${createdData.filter((item) => item.type !== 'data-create-skipped').length} created records</div>
  </header>
  <main class="grid">${cards}</main>
</body>
</html>`
  const galleryPath = path.join(artifactRoot, 'screenshot-gallery.html')
  await fs.writeFile(galleryPath, html)

  const page = await browser.newPage({ viewport: { width: 1800, height: 2400 } })
  await page.goto(`file://${galleryPath.replaceAll('\\', '/')}`)
  await page.screenshot({ path: path.join(artifactRoot, 'screenshot-gallery.png'), fullPage: true })
  await page.close()
}

function escapeHtml(value) {
  return String(value)
    .replaceAll('&', '&amp;')
    .replaceAll('<', '&lt;')
    .replaceAll('>', '&gt;')
    .replaceAll('"', '&quot;')
    .replaceAll("'", '&#39;')
}

async function main() {
  await ensureDirs()
  await seedProducts()

  const browser = await chromium.launch({ headless: true })
  const context = await browser.newContext({
    viewport: { width: 1440, height: 1000 },
    ignoreHTTPSErrors: true,
  })
  const page = await context.newPage()
  attachMonitoring(page)
  await signInFromSuite(page)
  await crawlProducts(page)
  await writeJson('created-data.json', createdData)
  await writeJson('data-errors.json', dataErrors)
  await writeJson('network-console-errors.json', networkConsoleErrors)
  await writeJson('console-warnings.json', consoleWarnings)
  await writeJson('route-results.json', routeResults)
  await writeJson('summary.json', {
    artifactRoot,
    suiteBaseUrl,
    screenshots: routeResults.filter((result) => result.screenshot).length,
    routeResults: routeResults.length,
    networkConsoleErrors: networkConsoleErrors.length,
    consoleWarnings: consoleWarnings.length,
    createdRecords: createdData.filter((item) => item.type !== 'data-create-skipped').length,
    dataErrors: dataErrors.length,
    products: groupByProduct(routeResults),
  })
  await writeReport()
  await writeGallery(browser)
  await browser.close()

  console.log(JSON.stringify({
    artifactRoot,
    screenshots: routeResults.filter((result) => result.screenshot).length,
    networkConsoleErrors: networkConsoleErrors.length,
    consoleWarnings: consoleWarnings.length,
    createdRecords: createdData.filter((item) => item.type !== 'data-create-skipped').length,
    dataErrors: dataErrors.length,
  }, null, 2))
}

main().catch((error) => {
  console.error(error)
  process.exitCode = 1
})
