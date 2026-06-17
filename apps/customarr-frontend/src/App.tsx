import { useEffect, useMemo, useState, type ReactNode } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import {
  Building2,
  Clock3,
  Contact2,
  CreditCard,
  DatabaseZap,
  FileCheck2,
  FilePlus2,
  GitBranch,
  LayoutDashboard,
  MapPinned,
  PanelTopOpen,
  Route as RouteIcon,
  Search,
  Settings,
  ShieldCheck,
  Users,
} from 'lucide-react'
import { Navigate, Route, Routes, Link, useLocation, useNavigate, useParams } from 'react-router-dom'
import {
  ApiErrorCallout,
  ProductWorkspaceFrame,
  buildProductLaunchUrlMap,
  formatProductLaunchError,
  getErrorMessage,
  getLaunchCatalog,
  resolveProductWorkspaceBootstrapError,
  resolveSuiteHomeUrl,
  useProductWorkspaceLaunch,
  type ProductNavItem,
} from '@stl/shared-ui'
import { clearSession, loadSession, type StoredCustomArrSession } from './auth/sessionStorage'
import {
  cloneCustomers,
  demoCustomersSeed,
  demoRequirementCatalog,
  demoWorkspaceSession,
  type CustomArrCreateCustomerRequest,
  type CustomArrCustomerDetail,
  type CustomArrCustomerStatus,
  type CustomArrCustomerTier,
  type CustomArrRequirementCatalogItem,
} from './demoData'
import {
  createCustomer,
  getCustomer,
  getDashboard,
  getSessionBootstrap,
  listCustomers,
  listRequirements,
} from './api/client'
import { LaunchPage } from './LaunchPage'

const suiteHomeUrl = resolveSuiteHomeUrl(import.meta.env.VITE_SUITE_URL)
const productLaunchUrls = buildProductLaunchUrlMap(import.meta.env)
const apiBase = import.meta.env.VITE_CUSTOMARR_API_BASE ?? ''
const demoMode = import.meta.env.DEV

const navItems: ProductNavItem[] = [
  { label: 'Dashboard', to: '/dashboard', icon: LayoutDashboard as ProductNavItem['icon'] },
  { label: 'Customers', to: '/customers', icon: Users as ProductNavItem['icon'] },
  { label: 'Create', to: '/customers/create', icon: FilePlus2 as ProductNavItem['icon'] },
  { label: 'Hierarchy', to: '/hierarchy', icon: MapPinned as ProductNavItem['icon'] },
  { label: 'Requirements', to: '/requirements', icon: ShieldCheck as ProductNavItem['icon'] },
  { label: 'Contacts', to: '/contacts', icon: Contact2 as ProductNavItem['icon'] },
  { label: 'Settings', to: '/settings', icon: Settings as ProductNavItem['icon'], sectionBreakBefore: true },
]

function formatDate(value: string | null | undefined): string {
  if (!value) {
    return 'n/a'
  }
  const date = new Date(value)
  return Number.isNaN(date.getTime())
    ? value
    : date.toLocaleString(undefined, {
        month: 'short',
        day: 'numeric',
        hour: 'numeric',
        minute: '2-digit',
      })
}

function titleFromStatus(status: string): string {
  switch (status) {
    case 'prospect':
      return 'Prospect'
    case 'active':
      return 'Active'
    case 'on_hold':
      return 'On hold'
    case 'onboarding':
      return 'Onboarding'
    case 'watch':
      return 'Watch'
    case 'inactive':
      return 'Inactive'
    case 'archived':
      return 'Archived'
    case 'blocked':
      return 'Blocked'
    default:
      return humanizeKey(status)
  }
}

function toneForStatus(status: string): string {
  switch (status) {
    case 'active':
      return 'border-emerald-500/40 bg-emerald-500/10 text-emerald-100'
    case 'prospect':
    case 'onboarding':
    case 'watch':
    case 'on_hold':
      return 'border-amber-500/40 bg-amber-500/10 text-amber-100'
    case 'blocked':
      return 'border-rose-500/40 bg-rose-500/10 text-rose-100'
    case 'inactive':
    case 'archived':
      return 'border-slate-500/30 bg-slate-900/80 text-slate-200'
    default:
      return 'border-slate-500/30 bg-slate-900/80 text-slate-200'
  }
}

function humanizeKey(value: string | null | undefined): string {
  if (!value) {
    return 'n/a'
  }
  return value
    .split(/[_-]+/g)
    .filter(Boolean)
    .map((part) => part.charAt(0).toUpperCase() + part.slice(1))
    .join(' ')
}

function yesNo(value: boolean | null | undefined): string {
  return value ? 'Yes' : 'No'
}

function primaryLabel(value: string | null | undefined): string {
  return value?.trim() ? value : 'n/a'
}

function PageHeader({
  eyebrow,
  title,
  description,
  action,
}: {
  eyebrow: string
  title: string
  description: string
  action?: ReactNode
}) {
  return (
    <div className="flex flex-col gap-3 rounded-[1.5rem] border border-slate-700/70 bg-slate-950/80 p-5 shadow-2xl shadow-slate-950/20 lg:flex-row lg:items-end lg:justify-between">
      <div className="space-y-2">
        <p className="text-xs font-semibold uppercase tracking-[0.22em] text-cyan-300">{eyebrow}</p>
        <h1 className="text-2xl font-semibold text-slate-50">{title}</h1>
        <p className="max-w-3xl text-sm text-slate-300">{description}</p>
      </div>
      {action}
    </div>
  )
}

function SectionCard({
  title,
  icon,
  children,
  action,
}: {
  title: string
  icon: ReactNode
  children: ReactNode
  action?: ReactNode
}) {
  return (
    <div className="customarr-card">
      <div className="customarr-card-inner space-y-3">
        <div className="flex items-center justify-between gap-3">
          <div className="flex items-center gap-2">
            {icon}
            <h2 className="text-lg font-semibold text-slate-50">{title}</h2>
          </div>
          {action}
        </div>
        {children}
      </div>
    </div>
  )
}

function MetricCard({
  title,
  value,
  hint,
}: {
  title: string
  value: string | number
  hint: string
}) {
  return (
    <div className="customarr-card">
      <div className="customarr-card-inner">
        <p className="customarr-label">{title}</p>
        <p className="mt-2 text-3xl font-semibold text-slate-50">{value}</p>
        <p className="mt-2 text-sm text-slate-300">{hint}</p>
      </div>
    </div>
  )
}

function EmptyState({ title }: { title: string }) {
  return <div className="rounded-xl border border-dashed border-slate-700/80 p-4 text-sm text-slate-400">{title}</div>
}

function statusBadge(status: string) {
  return <span className={`rounded-full border px-3 py-1 text-xs font-semibold uppercase tracking-[0.18em] ${toneForStatus(status)}`}>{titleFromStatus(status)}</span>
}

function buildDemoDashboard(customers: CustomArrCustomerDetail[]) {
  const recentActivity = customers
    .flatMap((customer) =>
      customer.activity.map((activity) => ({
        ...activity,
        customerId: customer.customerId,
        customerNumber: customer.customerNumber,
      })),
    )
    .sort((left, right) => right.occurredAt.localeCompare(left.occurredAt))

  return {
    generatedAt: new Date().toISOString(),
    customerCount: customers.length,
    activeCustomerCount: customers.filter((customer) => (customer.statusKey ?? customer.status) === 'active').length,
    onboardingCustomerCount: customers.filter((customer) => ['prospect', 'onboarding'].includes(customer.statusKey ?? customer.status)).length,
    watchListCustomerCount: customers.filter((customer) => ['watch', 'on_hold', 'blocked'].includes(customer.statusKey ?? customer.status)).length,
    contactCount: customers.reduce((count, customer) => count + customer.contacts.length, 0),
    siteCount: customers.reduce((count, customer) => count + customer.siteCount, 0),
    requirementCount: customers.reduce((count, customer) => count + customer.requirements.length, 0),
    featuredCustomers: customers.slice(0, 3),
    recentActivity: recentActivity.slice(0, 6),
  }
}

function listAllContacts(customers: CustomArrCustomerDetail[]) {
  return customers.flatMap((customer) =>
    customer.contacts.map((contact) => ({
      ...contact,
      customerId: customer.customerId,
      customerNumber: customer.customerNumber,
      customerName: customer.displayName ?? customer.tradeName,
    })),
  )
}

function buildCustomerRequest(form: CustomerFormState): CustomArrCreateCustomerRequest {
  const tags = form.segment
    .split(',')
    .map((tag) => tag.trim())
    .filter(Boolean)

  return {
    legalName: form.legalName.trim(),
    tradeName: form.tradeName.trim(),
    status: form.status,
    tier: form.tier,
    segment: form.segment.trim(),
    ownerPersonId: form.ownerPersonId.trim(),
    parentCustomerId: form.parentCustomerId.trim(),
    primaryContactName: form.primaryContactName.trim(),
    primaryContactEmail: form.primaryContactEmail.trim(),
    primaryContactPhone: form.primaryContactPhone.trim(),
    billingCity: form.billingCity.trim(),
    billingState: form.billingState.trim(),
    shippingCity: form.shippingCity.trim(),
    shippingState: form.shippingState.trim(),
    notes: form.notes.trim(),
    displayName: form.displayName.trim() || form.tradeName.trim() || form.legalName.trim(),
    dbaName: form.dbaName.trim(),
    customerTypeKey: form.tier,
    statusKey: form.status,
    accountOwnerPersonId: form.ownerPersonId.trim(),
    assignedTeamId: form.assignedTeamId.trim(),
    customerSinceDate: form.customerSinceDate || null,
    sourceKey: form.sourceKey,
    tags,
    portalEnabled: form.portalEnabled,
    portalDisplayName: form.portalDisplayName.trim(),
    paymentTermsKey: form.paymentTermsKey,
    defaultOrderTypeKey: form.defaultOrderTypeKey,
    defaultServiceLevelKey: form.defaultServiceLevelKey,
    requiresAppointment: form.requiresAppointment,
    requiresProofOfDelivery: form.requiresProofOfDelivery,
    requiresCustomerReference: form.requiresCustomerReference,
    customerReferenceLabel: form.customerReferenceLabel.trim(),
    defaultInstructions: form.defaultInstructions.trim(),
    notificationPreferenceKey: form.notificationPreferenceKey,
  }
}

type CustomerFormState = {
  legalName: string
  tradeName: string
  displayName: string
  dbaName: string
  status: CustomArrCustomerStatus
  tier: CustomArrCustomerTier
  segment: string
  ownerPersonId: string
  assignedTeamId: string
  customerSinceDate: string
  sourceKey: string
  parentCustomerId: string
  primaryContactName: string
  primaryContactEmail: string
  primaryContactPhone: string
  billingCity: string
  billingState: string
  shippingCity: string
  shippingState: string
  notes: string
  portalEnabled: boolean
  portalDisplayName: string
  paymentTermsKey: string
  defaultOrderTypeKey: string
  defaultServiceLevelKey: string
  requiresAppointment: boolean
  requiresProofOfDelivery: boolean
  requiresCustomerReference: boolean
  customerReferenceLabel: string
  defaultInstructions: string
  notificationPreferenceKey: string
}

const staffOwnerOptions = [
  { personId: 'person-101', displayName: 'Maria Jensen', role: 'Account manager' },
  { personId: 'person-102', displayName: 'Derek Holt', role: 'Customer success lead' },
  { personId: 'person-103', displayName: 'Nina Rao', role: 'Operations liaison' },
  { personId: 'person-999', displayName: 'Demo Admin', role: 'Workspace admin' },
]

const staffTeamOptions = [
  { teamId: 'team-customer-success', displayName: 'Customer success' },
  { teamId: 'team-operations', displayName: 'Operations' },
  { teamId: 'team-compliance', displayName: 'Compliance' },
  { teamId: 'team-dispatch', displayName: 'Dispatch coordination' },
]

function staffPersonLabel(personId?: string | null) {
  if (!personId) {
    return 'Unassigned'
  }

  const person = staffOwnerOptions.find((option) => option.personId === personId)
  return person ? `${person.displayName} · ${person.role}` : 'StaffArr person'
}

function staffTeamLabel(teamId?: string | null) {
  if (!teamId) {
    return 'Unassigned'
  }

  const team = staffTeamOptions.find((option) => option.teamId === teamId)
  return team?.displayName ?? 'StaffArr team'
}

const initialCustomerForm: CustomerFormState = {
  legalName: '',
  tradeName: '',
  displayName: '',
  dbaName: '',
  status: 'prospect',
  tier: 'business',
  segment: '',
  ownerPersonId: 'person-101',
  assignedTeamId: 'team-customer-success',
  customerSinceDate: '',
  sourceKey: 'manual',
  parentCustomerId: '',
  primaryContactName: '',
  primaryContactEmail: '',
  primaryContactPhone: '',
  billingCity: '',
  billingState: '',
  shippingCity: '',
  shippingState: '',
  notes: '',
  portalEnabled: false,
  portalDisplayName: '',
  paymentTermsKey: 'net_30',
  defaultOrderTypeKey: 'customer_order',
  defaultServiceLevelKey: 'standard',
  requiresAppointment: false,
  requiresProofOfDelivery: true,
  requiresCustomerReference: false,
  customerReferenceLabel: 'PO Number',
  defaultInstructions: '',
  notificationPreferenceKey: 'email',
}

function buildDemoCustomer(
  form: CustomerFormState,
  existingCustomers: CustomArrCustomerDetail[],
): CustomArrCustomerDetail {
  const request = buildCustomerRequest(form)
  const nextNumber = existingCustomers.length + 1001
  const parent = existingCustomers.find((customer) => customer.customerId === request.parentCustomerId) ?? null
  const tradeName = request.displayName || request.tradeName || request.legalName
  const customerId = `cust-${crypto.randomUUID().slice(0, 8)}`
  const contactId = `ct-${crypto.randomUUID().slice(0, 8)}`
  const locationId = `loc-${crypto.randomUUID().slice(0, 8)}`
  const billingAddressId = `addr-${crypto.randomUUID().slice(0, 8)}`
  const shippingAddressId = locationId
  const now = new Date().toISOString()

  return {
    customerId,
    tenantId: 'demo-tenant',
    customerNumber: `CUS-${nextNumber}`,
    customerCode: `CUS-${nextNumber}`,
    legalName: request.legalName,
    displayName: tradeName,
    dbaName: request.dbaName || request.tradeName || null,
    customerTypeKey: request.customerTypeKey ?? request.tier,
    statusKey: request.statusKey as CustomArrCustomerStatus,
    tradeName,
    status: request.status,
    tier: request.tier,
    segment: request.segment,
    ownerPersonId: request.ownerPersonId,
    parentCustomerId: parent?.customerId ?? null,
    parentCustomerName: parent?.tradeName ?? null,
    primaryContactName: request.primaryContactName,
    primaryContactEmail: request.primaryContactEmail,
    siteCount: 1,
    contactCount: 1,
    requirementCount: 3,
    holdStatus: 'clear',
    lastActivityAt: now,
    updatedAt: now,
    hierarchyPath: [...(parent?.hierarchyPath ?? []), request.legalName],
    billingAddress: [request.billingCity, request.billingState].filter(Boolean).join(', '),
    shippingAddress: [request.shippingCity, request.shippingState].filter(Boolean).join(', '),
    taxId: 'pending',
    paymentTerms: request.paymentTermsKey ?? 'net_30',
    riskRating: ['prospect', 'onboarding'].includes(request.status) ? 'medium' : 'low',
    primaryContactId: contactId,
    primaryBillingAddressId: billingAddressId,
    primaryShippingAddressId: shippingAddressId,
    primaryServiceAddressId: shippingAddressId,
    assignedTeamId: request.assignedTeamId || null,
    customerSinceDate: request.customerSinceDate ?? null,
    sourceKey: request.sourceKey ?? 'manual',
    tags: request.tags?.length ? request.tags : request.segment ? [request.segment] : ['standard'],
    notes: request.notes ? [request.notes] : ['Created in demo mode.'],
    contacts: [
      {
        contactId,
        name: request.primaryContactName,
        role: 'Primary contact',
        email: request.primaryContactEmail,
        phone: request.primaryContactPhone,
        isPrimary: true,
        firstName: request.primaryContactName.split(' ')[0] ?? '',
        lastName: request.primaryContactName.split(' ').slice(1).join(' '),
        displayName: request.primaryContactName,
        title: 'Primary contact',
        preferredContactMethodKey: 'email',
        isBillingContact: true,
        isOrderingContact: true,
        isShippingContact: true,
        portalAccessEnabled: Boolean(request.portalEnabled),
        portalRoleKey: request.portalEnabled ? 'portal_admin' : null,
        statusKey: 'active',
        lastVerifiedAt: now,
      },
    ],
    locations: [
      {
        locationId: billingAddressId,
        addressId: billingAddressId,
        label: 'Billing',
        locationName: 'Billing',
        type: 'billing',
        addressTypeKey: 'billing',
        city: request.billingCity,
        addressCity: request.billingCity,
        state: request.billingState,
        stateProvince: request.billingState,
        postalCode: '',
        countryCode: 'US',
        isDefaultBilling: true,
        statusKey: 'active',
      },
      {
        locationId,
        addressId: locationId,
        label: 'Primary location',
        locationName: 'Primary location',
        type: 'service',
        addressTypeKey: 'service',
        city: request.shippingCity || request.billingCity,
        addressCity: request.shippingCity || request.billingCity,
        state: request.shippingState || request.billingState,
        stateProvince: request.shippingState || request.billingState,
        postalCode: '',
        countryCode: 'US',
        appointmentRequired: Boolean(request.requiresAppointment),
        isDefaultShipping: true,
        isDefaultService: true,
        statusKey: 'active',
      },
    ],
    addresses: [],
    identifiers: [
      {
        identifierId: `id-${customerId}`,
        identifierTypeKey: 'customer_legacy_id',
        identifierValue: 'pending',
        jurisdictionKey: null,
        issuingAuthority: null,
        effectiveDate: null,
        expirationDate: null,
        verificationStatusKey: 'unverified',
        recordArrDocumentId: null,
      },
    ],
    billingProfiles: [
      {
        billingProfileId: `bill-${customerId}`,
        billingContactId: contactId,
        billingAddressId,
        paymentTermsKey: request.paymentTermsKey ?? 'net_30',
        invoiceDeliveryMethodKey: 'email',
        billingEmail: request.primaryContactEmail || null,
        purchaseOrderRequired: Boolean(request.requiresCustomerReference),
        taxExempt: false,
        taxExemptionRecordId: null,
        currencyCode: 'USD',
        creditStatusKey: 'good_standing',
        creditLimit: null,
        externalAccountingCustomerRef: null,
      },
    ],
    portalSettings: {
      portalEnabled: Boolean(request.portalEnabled),
      portalDisplayName: request.portalDisplayName || tradeName,
      allowPortalOrderCreate: Boolean(request.portalEnabled),
      allowPortalDocumentUpload: Boolean(request.portalEnabled),
      allowPortalStatusView: Boolean(request.portalEnabled),
      defaultPortalContactId: request.portalEnabled ? contactId : null,
      portalInviteStatusKey: request.portalEnabled ? 'invited' : 'not_invited',
      portalTermsAcceptedAt: null,
      portalTermsAcceptedByPersonId: null,
      portalNotes: null,
    },
    operationalPreferences: {
      defaultOrderTypeKey: request.defaultOrderTypeKey ?? 'customer_order',
      defaultServiceLevelKey: request.defaultServiceLevelKey ?? 'standard',
      defaultPickupAddressId: shippingAddressId,
      defaultDeliveryAddressId: shippingAddressId,
      defaultContactId: contactId,
      requiresAppointment: Boolean(request.requiresAppointment),
      requiresProofOfDelivery: Boolean(request.requiresProofOfDelivery),
      requiresCustomerReference: Boolean(request.requiresCustomerReference),
      customerReferenceLabel: request.customerReferenceLabel || null,
      defaultInstructions: request.defaultInstructions || null,
      restrictedServiceNotes: null,
      notificationPreferenceKey: request.notificationPreferenceKey ?? 'email',
      orderConfirmationRequired: false,
    },
    requirements: demoRequirementCatalog.slice(0, 3).map((item, index) => ({
      requirementKey: `${customerId}-${item.requirementKey}`,
      title: item.title,
      owner: item.ownerTeam,
      status: index === 0 ? 'pending' : 'watch',
      dueAt: null,
      requirementId: `${customerId}-${item.requirementKey}`,
      requirementTypeKey: item.requirementKey,
      requirementName: item.title,
      description: item.description,
      requiredBeforeKey: index === 0 ? 'before_activation' : 'before_order_creation',
      recordArrDocumentId: null,
      statusKey: index === 0 ? 'missing' : 'pending_review',
      ownerTeam: item.ownerTeam,
    })),
    externalRefs: [],
    relationships: parent
      ? [
          {
            relationshipId: `rel-${crypto.randomUUID().slice(0, 8)}`,
            relatedCustomerId: parent.customerId,
            relatedCustomerName: parent.displayName ?? parent.tradeName,
            relationshipTypeKey: 'parent',
            effectiveDate: now,
            endDate: null,
          },
        ]
      : [],
    customFieldValues: [],
    activity: [
      {
        activityId: `act-${crypto.randomUUID().slice(0, 8)}`,
        kind: 'created',
        message: 'Customer created in demo mode.',
        occurredAt: now,
        sourceProductKey: 'customarr',
        actorPersonId: request.ownerPersonId,
      },
    ],
    accountOwnerPersonId: request.ownerPersonId,
    createdAt: now,
    createdByPersonId: request.ownerPersonId,
    updatedByPersonId: request.ownerPersonId,
    archivedAt: null,
    archivedByPersonId: null,
    rowVersion: 1,
  }
}

function WorkspaceBootstrap({
  accessToken,
  bootstrapError,
  workspaceSession,
  switcherEntitlements,
  isBootstrapping,
  onSelectProduct,
  onSignOut,
  isProductLaunchPending,
  productLaunchError,
  children,
}: {
  accessToken: string
  bootstrapError: 'forbidden' | 'expired' | null
  workspaceSession: { userDisplayName: string; tenantDisplayName: string; tenantSlug: string } | null
  switcherEntitlements: readonly string[]
  isBootstrapping: boolean
  onSelectProduct?: (productKey: string) => void
  onSignOut?: () => void
  isProductLaunchPending?: boolean
  productLaunchError?: string | null
  children: ReactNode
}) {
  return (
    <ProductWorkspaceFrame
      productName="CustomArr"
      productKey="customarr"
      workspaceSubtitle="Customer master, hierarchy, contacts, and requirements"
      navItems={navItems}
      entitlements={switcherEntitlements}
      suiteHomeUrl={suiteHomeUrl}
      productLaunchUrls={productLaunchUrls}
      onSelectProduct={onSelectProduct}
      onSignOut={onSignOut}
      isProductLaunchPending={isProductLaunchPending}
      productLaunchError={productLaunchError}
      aiAssistance={accessToken ? { apiBase, accessToken } : undefined}
      workspaceSession={workspaceSession}
      isBootstrapping={isBootstrapping}
      bootstrapError={bootstrapError}
    >
      {children}
    </ProductWorkspaceFrame>
  )
}

function DashboardPage({
  accessToken,
  customers,
}: {
  accessToken: string
  customers: CustomArrCustomerDetail[]
}) {
  const dashboardQuery = useQuery({
    queryKey: ['customarr', 'dashboard'],
    queryFn: () => getDashboard(accessToken),
    enabled: Boolean(accessToken),
    staleTime: 20_000,
  })

  const dashboard = dashboardQuery.data ?? buildDemoDashboard(customers)

  return (
    <div className="customarr-page">
      <PageHeader
        eyebrow="CustomArr"
        title="Customer master control center"
        description="Maintain the customer master, roll up hierarchy health, review onboarding requirements, and keep contacts aligned with the active record."
        action={
          <span className="customarr-pill">
            <DatabaseZap className="h-4 w-4" />
            Updated {formatDate(dashboard.generatedAt)}
          </span>
        }
      />
      {dashboardQuery.isError ? (
        <ApiErrorCallout
          title="Unable to load dashboard"
          message={getErrorMessage(dashboardQuery.error, 'Failed to load CustomArr dashboard.')}
        />
      ) : null}
      <div className="customarr-grid cols-3">
        <MetricCard title="Customers" value={dashboard.customerCount} hint={`${dashboard.activeCustomerCount} active, ${dashboard.onboardingCustomerCount} onboarding`} />
        <MetricCard title="Contacts" value={dashboard.contactCount} hint="Primary and secondary customer contacts" />
        <MetricCard title="Requirements" value={dashboard.requirementCount} hint="Required business and compliance checks" />
        <MetricCard title="Sites" value={dashboard.siteCount} hint="Billing, shipping, and service locations" />
        <MetricCard title="Watch list" value={dashboard.watchListCustomerCount} hint="Records needing attention" />
        <MetricCard title="Demo mode" value={accessToken ? 'No' : 'Yes'} hint="Local preview mode without a live session" />
      </div>

      <div className="customarr-grid cols-2">
        <SectionCard title="Featured customers" icon={<Users className="h-4 w-4 text-cyan-300" />}>
          <div className="space-y-3">
            {dashboard.featuredCustomers.map((customer) => (
              <Link key={customer.customerId} to={`/customers/${customer.customerId}`} className="block rounded-2xl border border-slate-700/70 bg-slate-900/70 p-4 transition hover:border-cyan-400/60 hover:bg-slate-900">
                <div className="flex items-center justify-between gap-3">
                  <div>
                    <strong className="text-slate-50">{customer.customerNumber}</strong>
                    <p className="mt-1 text-sm text-slate-300">{customer.displayName ?? customer.tradeName}</p>
                  </div>
                  {statusBadge(customer.statusKey ?? customer.status)}
                </div>
                <p className="mt-2 text-xs text-slate-400">
                  {customer.segment} · {humanizeKey(customer.customerTypeKey ?? customer.tier)} · {customer.contactCount} contacts
                </p>
              </Link>
            ))}
          </div>
        </SectionCard>
        <SectionCard title="Recent activity" icon={<Clock3 className="h-4 w-4 text-cyan-300" />}>
          <div className="space-y-3">
            {dashboard.recentActivity.map((activity) => (
              <div key={activity.activityId} className="rounded-2xl border border-slate-700/70 bg-slate-900/70 p-4">
                <div className="flex items-center justify-between gap-3">
                  <strong className="text-sm text-slate-50">{activity.customerNumber}</strong>
                  <span className="customarr-pill text-[0.7rem]">{activity.kind}</span>
                </div>
                <p className="mt-1 text-sm text-slate-300">{activity.message}</p>
                <p className="mt-2 text-xs text-slate-400">{formatDate(activity.occurredAt)}</p>
              </div>
            ))}
          </div>
        </SectionCard>
      </div>
    </div>
  )
}

function CustomersPage({
  customers,
}: {
  customers: CustomArrCustomerDetail[]
}) {
  const [search, setSearch] = useState('')
  const filteredCustomers = useMemo(() => {
    const query = search.trim().toLowerCase()
    if (!query) {
      return customers
    }
    return customers.filter((customer) =>
      [customer.customerNumber, customer.customerCode, customer.displayName, customer.tradeName, customer.legalName, customer.segment, customer.primaryContactName]
        .join(' ')
        .toLowerCase()
        .includes(query),
    )
  }, [customers, search])

  return (
    <div className="customarr-page">
      <PageHeader
        eyebrow="Customers"
        title="Customer register"
        description="Search the customer master, inspect hierarchy ownership, and jump straight into the detail record for a customer."
      />
      <div className="customarr-card">
        <div className="customarr-card-inner flex flex-col gap-3 lg:flex-row lg:items-center lg:justify-between">
          <div className="relative flex-1">
            <Search className="pointer-events-none absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-slate-400" />
            <input className="customarr-input pl-10" value={search} onChange={(event) => setSearch(event.target.value)} placeholder="Search by number, name, segment, or contact..." />
          </div>
          <Link to="/customers/create" className="customarr-button">
            <FilePlus2 className="h-4 w-4" />
            Create customer
          </Link>
        </div>
      </div>

      <div className="customarr-grid cols-2">
        {filteredCustomers.map((customer) => (
          <Link key={customer.customerId} to={`/customers/${customer.customerId}`} className="customarr-card transition hover:-translate-y-0.5 hover:border-cyan-400/40">
            <div className="customarr-card-inner space-y-3">
              <div className="flex items-start justify-between gap-3">
                <div>
                  <p className="text-xs uppercase tracking-[0.2em] text-cyan-300">{customer.customerNumber}</p>
                  <h2 className="mt-1 text-xl font-semibold text-slate-50">{customer.displayName ?? customer.tradeName}</h2>
                  <p className="mt-1 text-sm text-slate-300">{customer.legalName}</p>
                </div>
                {statusBadge(customer.statusKey ?? customer.status)}
              </div>
              <div className="grid gap-2 text-sm text-slate-300 md:grid-cols-2">
                <p><strong className="text-slate-100">Type:</strong> {humanizeKey(customer.customerTypeKey ?? customer.tier)}</p>
                <p><strong className="text-slate-100">Tags:</strong> {(customer.tags?.length ? customer.tags.map(humanizeKey).join(', ') : customer.segment) || 'n/a'}</p>
                <p><strong className="text-slate-100">Primary contact:</strong> {customer.primaryContactName}</p>
                <p><strong className="text-slate-100">Hierarchy:</strong> {customer.parentCustomerName ?? 'Top-level'}</p>
              </div>
              <p className="text-xs text-slate-400">
                {customer.contactCount} contacts · {customer.siteCount} sites · {customer.requirementCount} requirements
              </p>
            </div>
          </Link>
        ))}
        {filteredCustomers.length === 0 ? <EmptyState title="No customers matched your search." /> : null}
      </div>
    </div>
  )
}

function CustomerDetailPage({
  accessToken,
  customers,
}: {
  accessToken: string
  customers: CustomArrCustomerDetail[]
}) {
  const params = useParams()
  const customerId = params.customerId ?? ''
  const query = useQuery({
    queryKey: ['customarr', 'customer', customerId],
    queryFn: () => getCustomer(accessToken, customerId),
    enabled: Boolean(accessToken && customerId),
    staleTime: 20_000,
  })

  const customer = query.data ?? customers.find((entry) => entry.customerId === customerId) ?? null

  if (!customer) {
    return <EmptyState title="Customer not found." />
  }

  const customerStatus = customer.statusKey ?? customer.status
  const customerName = customer.displayName ?? customer.tradeName
  const addresses = customer.addresses?.length ? customer.addresses : customer.locations
  const billingProfile = customer.billingProfiles?.[0]
  const portalSettings = customer.portalSettings
  const operations = customer.operationalPreferences
  const identifiers = customer.identifiers ?? []
  const externalRefs = customer.externalRefs ?? []
  const relationships = customer.relationships ?? []
  const customFields = customer.customFieldValues ?? []

  return (
    <div className="customarr-page">
      <PageHeader
        eyebrow="Customer detail"
        title={customerName}
        description={`${customer.customerNumber} · ${customer.legalName}`}
        action={statusBadge(customerStatus)}
      />
      {query.isError ? (
        <ApiErrorCallout
          title="Unable to load customer"
          message={getErrorMessage(query.error, 'Failed to load customer detail.')}
        />
      ) : null}

      <div className="customarr-grid cols-2">
        <SectionCard title="Overview" icon={<Building2 className="h-4 w-4 text-cyan-300" />}>
          <div className="grid gap-3 md:grid-cols-2">
            <Field label="Legal name" value={customer.legalName} />
            <Field label="Display name" value={customerName} />
            <Field label="DBA name" value={primaryLabel(customer.dbaName)} />
            <Field label="Customer type" value={humanizeKey(customer.customerTypeKey ?? customer.tier)} />
            <Field label="Status" value={titleFromStatus(customerStatus)} />
            <Field label="Account owner" value={staffPersonLabel(customer.accountOwnerPersonId ?? customer.ownerPersonId)} />
            <Field label="Customer team" value={staffTeamLabel(customer.assignedTeamId)} />
            <Field label="Customer since" value={formatDate(customer.customerSinceDate)} />
            <Field label="Source" value={humanizeKey(customer.sourceKey)} />
            <Field label="Tags" value={customer.tags?.length ? customer.tags.map(humanizeKey).join(', ') : primaryLabel(customer.segment)} wide />
            <Field label="Notes" value={customer.notes.length ? customer.notes.join(' ') : 'n/a'} wide />
          </div>
        </SectionCard>
        <SectionCard title="Hierarchy" icon={<MapPinned className="h-4 w-4 text-cyan-300" />}>
          <div className="space-y-3">
            <p className="text-sm text-slate-300">
              Parent: <strong className="text-slate-100">{customer.parentCustomerName ?? 'None'}</strong>
            </p>
            <div className="space-y-2">
              {customer.hierarchyPath.map((level, index) => (
                <div key={`${level}-${index}`} className="rounded-2xl border border-slate-700/70 bg-slate-900/70 px-4 py-3 text-sm text-slate-200">
                  {index + 1}. {level}
                </div>
              ))}
            </div>
            <p className="text-xs text-slate-400">Hold status: {humanizeKey(customer.holdStatus)} · Risk: {humanizeKey(customer.riskRating)}</p>
          </div>
        </SectionCard>
      </div>

      <div className="customarr-grid cols-2">
        <SectionCard title="Contacts" icon={<Contact2 className="h-4 w-4 text-cyan-300" />}>
          <div className="space-y-3">
            {customer.contacts.map((contact) => (
              <div key={contact.contactId} className="rounded-2xl border border-slate-700/70 bg-slate-900/70 p-4">
                <div className="flex items-center justify-between gap-3">
                  <strong className="text-slate-50">{contact.displayName ?? contact.name}</strong>
                  {contact.isPrimary || contact.primary ? <span className="customarr-pill">Primary</span> : null}
                </div>
                <p className="mt-1 text-sm text-slate-300">{contact.title ?? contact.role}</p>
                <p className="mt-1 text-sm text-slate-300">{contact.email}</p>
                <p className="mt-1 text-xs text-slate-400">{contact.phone}</p>
                <p className="mt-2 text-xs text-slate-400">
                  {contact.isBillingContact ? 'Billing · ' : ''}
                  {contact.isOrderingContact ? 'Ordering · ' : ''}
                  {contact.isShippingContact ? 'Shipping · ' : ''}
                  Portal {contact.portalAccessEnabled ? humanizeKey(contact.portalRoleKey ?? 'enabled') : 'disabled'}
                </p>
              </div>
            ))}
          </div>
        </SectionCard>
        <SectionCard title="Locations" icon={<MapPinned className="h-4 w-4 text-cyan-300" />}>
          <div className="space-y-3">
            {addresses.map((address) => (
              <div key={address.addressId ?? address.locationId} className="rounded-2xl border border-slate-700/70 bg-slate-900/70 p-4">
                <div className="flex items-center justify-between gap-3">
                  <strong className="text-slate-50">{address.locationName ?? address.label}</strong>
                  <span className="customarr-pill">{humanizeKey(address.addressTypeKey ?? address.type)}</span>
                </div>
                <p className="mt-1 text-sm text-slate-300">
                  {[address.line1, address.addressCity ?? address.city, address.stateProvince ?? address.state, address.postalCode].filter(Boolean).join(', ') || 'Address pending'}
                </p>
                <p className="mt-2 text-xs text-slate-400">
                  {address.isDefaultBilling ? 'Default billing · ' : ''}
                  {address.isDefaultShipping ? 'Default shipping · ' : ''}
                  {address.isDefaultService ? 'Default service · ' : ''}
                  Appointment {yesNo(address.appointmentRequired)}
                </p>
                {address.receivingHours ? <p className="mt-1 text-xs text-slate-400">{address.receivingHours}</p> : null}
                {address.deliveryInstructions ? <p className="mt-1 text-xs text-slate-400">{address.deliveryInstructions}</p> : null}
              </div>
            ))}
          </div>
        </SectionCard>
      </div>

      <div className="customarr-grid cols-2">
        <SectionCard title="Portal access" icon={<PanelTopOpen className="h-4 w-4 text-cyan-300" />}>
          <div className="grid gap-3 md:grid-cols-2">
            <Field label="Portal enabled" value={yesNo(portalSettings?.portalEnabled)} />
            <Field label="Invite status" value={humanizeKey(portalSettings?.portalInviteStatusKey)} />
            <Field label="Portal display" value={primaryLabel(portalSettings?.portalDisplayName)} />
            <Field label="Default portal contact" value={primaryLabel(portalSettings?.defaultPortalContactId)} />
            <Field label="Orders" value={yesNo(portalSettings?.allowPortalOrderCreate)} />
            <Field label="Document upload" value={yesNo(portalSettings?.allowPortalDocumentUpload)} />
            <Field label="Status view" value={yesNo(portalSettings?.allowPortalStatusView)} />
            <Field label="Terms accepted" value={formatDate(portalSettings?.portalTermsAcceptedAt)} />
          </div>
        </SectionCard>
        <SectionCard title="Billing & terms" icon={<CreditCard className="h-4 w-4 text-cyan-300" />}>
          <div className="grid gap-3 md:grid-cols-2">
            <Field label="Payment terms" value={humanizeKey(billingProfile?.paymentTermsKey ?? customer.paymentTerms)} />
            <Field label="Invoice delivery" value={humanizeKey(billingProfile?.invoiceDeliveryMethodKey)} />
            <Field label="Billing email" value={primaryLabel(billingProfile?.billingEmail)} />
            <Field label="PO required" value={yesNo(billingProfile?.purchaseOrderRequired)} />
            <Field label="Tax exempt" value={yesNo(billingProfile?.taxExempt)} />
            <Field label="Currency" value={primaryLabel(billingProfile?.currencyCode)} />
            <Field label="Credit status" value={humanizeKey(billingProfile?.creditStatusKey)} />
            <Field label="Accounting ref" value={primaryLabel(billingProfile?.externalAccountingCustomerRef)} />
          </div>
        </SectionCard>
      </div>

      <div className="customarr-grid cols-2">
        <SectionCard title="Operational defaults" icon={<RouteIcon className="h-4 w-4 text-cyan-300" />}>
          <div className="grid gap-3 md:grid-cols-2">
            <Field label="Order type" value={humanizeKey(operations?.defaultOrderTypeKey)} />
            <Field label="Service level" value={humanizeKey(operations?.defaultServiceLevelKey)} />
            <Field label="Pickup address" value={primaryLabel(operations?.defaultPickupAddressId)} />
            <Field label="Delivery address" value={primaryLabel(operations?.defaultDeliveryAddressId)} />
            <Field label="Default contact" value={primaryLabel(operations?.defaultContactId)} />
            <Field label="Appointment required" value={yesNo(operations?.requiresAppointment)} />
            <Field label="POD required" value={yesNo(operations?.requiresProofOfDelivery)} />
            <Field label="Reference required" value={yesNo(operations?.requiresCustomerReference)} />
            <Field label="Reference label" value={primaryLabel(operations?.customerReferenceLabel)} />
            <Field label="Notifications" value={humanizeKey(operations?.notificationPreferenceKey)} />
            <Field label="Instructions" value={primaryLabel(operations?.defaultInstructions)} wide />
          </div>
        </SectionCard>
        <SectionCard title="Identifiers" icon={<FileCheck2 className="h-4 w-4 text-cyan-300" />}>
          <div className="space-y-3">
            {identifiers.map((identifier) => (
              <div key={identifier.identifierId} className="rounded-2xl border border-slate-700/70 bg-slate-900/70 p-4">
                <div className="flex items-center justify-between gap-3">
                  <strong className="text-slate-50">{humanizeKey(identifier.identifierTypeKey)}</strong>
                  <span className="customarr-pill">{humanizeKey(identifier.verificationStatusKey)}</span>
                </div>
                <p className="mt-1 text-sm text-slate-300">{identifier.identifierValue}</p>
                <p className="mt-1 text-xs text-slate-400">{primaryLabel(identifier.jurisdictionKey)} · {primaryLabel(identifier.issuingAuthority)}</p>
              </div>
            ))}
            {identifiers.length === 0 ? <EmptyState title="No business identifiers recorded." /> : null}
          </div>
        </SectionCard>
      </div>

      <div className="customarr-grid cols-2">
        <SectionCard title="Requirements & documents" icon={<ShieldCheck className="h-4 w-4 text-cyan-300" />}>
          <div className="space-y-3">
            {customer.requirements.map((requirement) => (
              <div key={requirement.requirementKey} className="rounded-2xl border border-slate-700/70 bg-slate-900/70 p-4">
                <div className="flex items-center justify-between gap-3">
                  <strong className="text-slate-50">{requirement.requirementName ?? requirement.title}</strong>
                  {statusBadge(requirement.status)}
                </div>
                <p className="mt-1 text-sm text-slate-300">{requirement.description ?? `Owner: ${requirement.owner}`}</p>
                <p className="mt-1 text-xs text-slate-400">
                  {humanizeKey(requirement.requiredBeforeKey)} · RecordArr {primaryLabel(requirement.recordArrDocumentId)} · Expires {formatDate(requirement.expirationDate ?? requirement.dueAt)}
                </p>
              </div>
            ))}
          </div>
        </SectionCard>
        <SectionCard title="Relationships" icon={<GitBranch className="h-4 w-4 text-cyan-300" />}>
          <div className="space-y-3">
            {relationships.map((relationship) => (
              <div key={relationship.relationshipId} className="rounded-2xl border border-slate-700/70 bg-slate-900/70 p-4">
                <div className="flex items-center justify-between gap-3">
                  <strong className="text-slate-50">{relationship.relatedCustomerName ?? relationship.relatedCustomerId}</strong>
                  <span className="customarr-pill">{humanizeKey(relationship.relationshipTypeKey)}</span>
                </div>
                <p className="mt-1 text-xs text-slate-400">Effective {formatDate(relationship.effectiveDate)} · Ends {formatDate(relationship.endDate)}</p>
              </div>
            ))}
            {externalRefs.map((externalRef) => (
              <div key={externalRef.externalRefId} className="rounded-2xl border border-slate-700/70 bg-slate-900/70 p-4">
                <div className="flex items-center justify-between gap-3">
                  <strong className="text-slate-50">{humanizeKey(externalRef.systemKey)}</strong>
                  <span className="customarr-pill">{humanizeKey(externalRef.syncStatusKey)}</span>
                </div>
                <p className="mt-1 text-sm text-slate-300">{externalRef.externalCode ?? externalRef.externalId}</p>
                <p className="mt-1 text-xs text-slate-400">Synced {formatDate(externalRef.lastSyncedAt)}</p>
              </div>
            ))}
            {customFields.map((field) => (
              <div key={field.fieldValueId} className="rounded-2xl border border-slate-700/70 bg-slate-900/70 p-4">
                <strong className="text-slate-50">{humanizeKey(field.fieldDefinitionId)}</strong>
                <p className="mt-1 text-sm text-slate-300">{field.valueText ?? field.valueOptionKey ?? field.valueNumber ?? field.valueDate ?? 'n/a'}</p>
              </div>
            ))}
            {relationships.length + externalRefs.length + customFields.length === 0 ? <EmptyState title="No relationships or external references recorded." /> : null}
          </div>
        </SectionCard>
      </div>

      <SectionCard title="Activity & audit" icon={<Clock3 className="h-4 w-4 text-cyan-300" />}>
        <div className="space-y-3">
          {customer.activity.map((item) => (
            <div key={item.activityId} className="rounded-2xl border border-slate-700/70 bg-slate-900/70 p-4">
              <div className="flex items-center justify-between gap-3">
                <strong className="text-slate-50">{item.kind}</strong>
                <span className="customarr-pill">{formatDate(item.occurredAt)}</span>
              </div>
              <p className="mt-2 text-sm text-slate-300">{item.message}</p>
              <p className="mt-1 text-xs text-slate-400">{item.sourceProductKey ?? 'customarr'} · {primaryLabel(item.actorPersonId)}</p>
            </div>
          ))}
          <div className="rounded-2xl border border-slate-700/70 bg-slate-900/70 p-4 text-sm text-slate-300">
            Created {formatDate(customer.createdAt)} by {primaryLabel(customer.createdByPersonId)} · Updated by {primaryLabel(customer.updatedByPersonId)} · Version {customer.rowVersion ?? 1}
          </div>
        </div>
      </SectionCard>
    </div>
  )
}

function CreateCustomerPage({
  accessToken,
  customers,
  onCreateDemoCustomer,
}: {
  accessToken: string
  customers: CustomArrCustomerDetail[]
  onCreateDemoCustomer: (request: CustomArrCreateCustomerRequest) => CustomArrCustomerDetail
}) {
  const queryClient = useQueryClient()
  const navigate = useNavigate()
  const [form, setForm] = useState<CustomerFormState>(initialCustomerForm)
  const createMutation = useMutation({
    mutationFn: async () => {
      const request = buildCustomerRequest(form)
      if (accessToken) {
        return createCustomer(accessToken, request)
      }
      return onCreateDemoCustomer(request)
    },
    onSuccess: async (customer) => {
      await queryClient.invalidateQueries({ queryKey: ['customarr'] })
      navigate(`/customers/${customer.customerId}`)
    },
  })

  return (
    <div className="customarr-page">
      <PageHeader
        eyebrow="Create customer"
        title="New customer record"
        description="Capture the customer master record once, then let downstream workflows consume the same canonical record."
        action={<span className="customarr-pill">{accessToken ? 'Live create' : 'Demo create'}</span>}
      />
      <div className="customarr-card">
        <div className="customarr-card-inner space-y-4">
          <div className="grid gap-3 md:grid-cols-2">
            <Field label="Legal name"><input className="customarr-input" value={form.legalName} onChange={(event) => setForm({ ...form, legalName: event.target.value })} /></Field>
            <Field label="Display name"><input className="customarr-input" value={form.displayName} onChange={(event) => setForm({ ...form, displayName: event.target.value })} /></Field>
            <Field label="DBA name"><input className="customarr-input" value={form.dbaName} onChange={(event) => setForm({ ...form, dbaName: event.target.value })} /></Field>
            <Field label="Legacy trade name"><input className="customarr-input" value={form.tradeName} onChange={(event) => setForm({ ...form, tradeName: event.target.value })} /></Field>
            <Field label="Status">
              <select className="customarr-select" value={form.status} onChange={(event) => setForm({ ...form, status: event.target.value as CustomerFormState['status'] })}>
                <option value="prospect">Prospect</option>
                <option value="active">Active</option>
                <option value="on_hold">On hold</option>
                <option value="inactive">Inactive</option>
                <option value="blocked">Blocked</option>
              </select>
            </Field>
            <Field label="Customer type">
              <select className="customarr-select" value={form.tier} onChange={(event) => setForm({ ...form, tier: event.target.value as CustomerFormState['tier'] })}>
                <option value="business">Business</option>
                <option value="individual">Individual</option>
                <option value="government">Government</option>
                <option value="internal">Internal</option>
                <option value="broker">Broker</option>
                <option value="carrier">Carrier</option>
              </select>
            </Field>
            <Field label="Tags"><input className="customarr-input" value={form.segment} onChange={(event) => setForm({ ...form, segment: event.target.value })} placeholder="strategic, enterprise logistics" /></Field>
            <Field label="Account owner">
              <select className="customarr-select" value={form.ownerPersonId} onChange={(event) => setForm({ ...form, ownerPersonId: event.target.value })}>
                {staffOwnerOptions.map((person) => (
                  <option key={person.personId} value={person.personId}>
                    {person.displayName} - {person.role}
                  </option>
                ))}
              </select>
            </Field>
            <Field label="Customer team">
              <select className="customarr-select" value={form.assignedTeamId} onChange={(event) => setForm({ ...form, assignedTeamId: event.target.value })}>
                <option value="">Unassigned</option>
                {staffTeamOptions.map((team) => (
                  <option key={team.teamId} value={team.teamId}>
                    {team.displayName}
                  </option>
                ))}
              </select>
            </Field>
            <Field label="Customer since"><input className="customarr-input" type="date" value={form.customerSinceDate} onChange={(event) => setForm({ ...form, customerSinceDate: event.target.value })} /></Field>
            <Field label="Source">
              <select className="customarr-select" value={form.sourceKey} onChange={(event) => setForm({ ...form, sourceKey: event.target.value })}>
                <option value="manual">Manual</option>
                <option value="import">Import</option>
                <option value="portal_signup">Portal signup</option>
                <option value="api">API</option>
                <option value="migration">Migration</option>
              </select>
            </Field>
            <Field label="Parent customer">
              <select className="customarr-select" value={form.parentCustomerId} onChange={(event) => setForm({ ...form, parentCustomerId: event.target.value })}>
                <option value="">Top-level customer</option>
                {customers.map((customer) => (
                  <option key={customer.customerId} value={customer.customerId}>
                    {customer.customerNumber} - {customer.displayName ?? customer.tradeName}
                  </option>
                ))}
              </select>
            </Field>
            <Field label="Primary contact name"><input className="customarr-input" value={form.primaryContactName} onChange={(event) => setForm({ ...form, primaryContactName: event.target.value })} /></Field>
            <Field label="Primary contact email"><input className="customarr-input" value={form.primaryContactEmail} onChange={(event) => setForm({ ...form, primaryContactEmail: event.target.value })} /></Field>
            <Field label="Primary contact phone"><input className="customarr-input" value={form.primaryContactPhone} onChange={(event) => setForm({ ...form, primaryContactPhone: event.target.value })} /></Field>
            <Field label="Billing city"><input className="customarr-input" value={form.billingCity} onChange={(event) => setForm({ ...form, billingCity: event.target.value })} /></Field>
            <Field label="Billing state"><input className="customarr-input" value={form.billingState} onChange={(event) => setForm({ ...form, billingState: event.target.value })} /></Field>
            <Field label="Shipping city"><input className="customarr-input" value={form.shippingCity} onChange={(event) => setForm({ ...form, shippingCity: event.target.value })} /></Field>
            <Field label="Shipping state"><input className="customarr-input" value={form.shippingState} onChange={(event) => setForm({ ...form, shippingState: event.target.value })} /></Field>
            <Field label="Payment terms">
              <select className="customarr-select" value={form.paymentTermsKey} onChange={(event) => setForm({ ...form, paymentTermsKey: event.target.value })}>
                <option value="net_15">Net 15</option>
                <option value="net_30">Net 30</option>
                <option value="net_45">Net 45</option>
                <option value="due_on_receipt">Due on receipt</option>
                <option value="prepaid">Prepaid</option>
              </select>
            </Field>
            <Field label="Service level">
              <select className="customarr-select" value={form.defaultServiceLevelKey} onChange={(event) => setForm({ ...form, defaultServiceLevelKey: event.target.value })}>
                <option value="standard">Standard</option>
                <option value="expedited">Expedited</option>
                <option value="scheduled">Scheduled</option>
                <option value="recurring">Recurring</option>
              </select>
            </Field>
            <Field label="Reference label"><input className="customarr-input" value={form.customerReferenceLabel} onChange={(event) => setForm({ ...form, customerReferenceLabel: event.target.value })} /></Field>
            <Field label="Notification preference">
              <select className="customarr-select" value={form.notificationPreferenceKey} onChange={(event) => setForm({ ...form, notificationPreferenceKey: event.target.value })}>
                <option value="email">Email</option>
                <option value="portal">Portal</option>
                <option value="sms">SMS</option>
                <option value="api_webhook">API/webhook</option>
              </select>
            </Field>
            <label className="flex items-center gap-3 rounded-xl border border-slate-700/70 bg-slate-900/70 px-4 py-3 text-sm text-slate-200">
              <input type="checkbox" checked={form.portalEnabled} onChange={(event) => setForm({ ...form, portalEnabled: event.target.checked })} />
              Portal enabled
            </label>
            <label className="flex items-center gap-3 rounded-xl border border-slate-700/70 bg-slate-900/70 px-4 py-3 text-sm text-slate-200">
              <input type="checkbox" checked={form.requiresAppointment} onChange={(event) => setForm({ ...form, requiresAppointment: event.target.checked })} />
              Requires appointment
            </label>
            <label className="flex items-center gap-3 rounded-xl border border-slate-700/70 bg-slate-900/70 px-4 py-3 text-sm text-slate-200">
              <input type="checkbox" checked={form.requiresProofOfDelivery} onChange={(event) => setForm({ ...form, requiresProofOfDelivery: event.target.checked })} />
              Requires POD
            </label>
            <label className="flex items-center gap-3 rounded-xl border border-slate-700/70 bg-slate-900/70 px-4 py-3 text-sm text-slate-200">
              <input type="checkbox" checked={form.requiresCustomerReference} onChange={(event) => setForm({ ...form, requiresCustomerReference: event.target.checked })} />
              Requires customer reference
            </label>
            <Field label="Portal display name"><input className="customarr-input" value={form.portalDisplayName} onChange={(event) => setForm({ ...form, portalDisplayName: event.target.value })} /></Field>
            <Field label="Default instructions" wide>
              <textarea className="customarr-textarea min-h-28" value={form.defaultInstructions} onChange={(event) => setForm({ ...form, defaultInstructions: event.target.value })} />
            </Field>
            <Field label="Notes" wide>
              <textarea className="customarr-textarea min-h-36" value={form.notes} onChange={(event) => setForm({ ...form, notes: event.target.value })} />
            </Field>
          </div>
          <button type="button" className="customarr-button" onClick={() => createMutation.mutate()} disabled={createMutation.isPending}>
            {createMutation.isPending ? 'Creating...' : 'Create customer'}
          </button>
        </div>
      </div>
    </div>
  )
}

function HierarchyPage({
  customers,
}: {
  customers: CustomArrCustomerDetail[]
}) {
  const roots = customers.filter((customer) => !customer.parentCustomerId)
  const childrenByParent = new Map<string, CustomArrCustomerDetail[]>()
  customers.forEach((customer) => {
    if (!customer.parentCustomerId) {
      return
    }
    const current = childrenByParent.get(customer.parentCustomerId) ?? []
    current.push(customer)
    childrenByParent.set(customer.parentCustomerId, current)
  })

  return (
    <div className="customarr-page">
      <PageHeader
        eyebrow="Hierarchy"
        title="Customer hierarchy map"
        description="See the parent-child relationships that define customer groups, umbrella accounts, and shared commercial ownership."
      />
      <div className="customarr-grid cols-2">
        {roots.map((customer) => (
          <SectionCard key={customer.customerId} title={customer.displayName ?? customer.tradeName} icon={<Building2 className="h-4 w-4 text-cyan-300" />}>
            <div className="space-y-3">
              <p className="text-sm text-slate-300">{customer.customerNumber} · {customer.segment}</p>
              {childrenByParent.get(customer.customerId)?.length ? (
                <div className="space-y-2">
                  {childrenByParent.get(customer.customerId)?.map((child) => (
                    <Link key={child.customerId} to={`/customers/${child.customerId}`} className="block rounded-2xl border border-slate-700/70 bg-slate-900/70 px-4 py-3 text-sm text-slate-200">
                      {child.customerNumber} · {child.displayName ?? child.tradeName} · {titleFromStatus(child.statusKey ?? child.status)}
                    </Link>
                  ))}
                </div>
              ) : (
                <EmptyState title="No downstream child accounts." />
              )}
            </div>
          </SectionCard>
        ))}
      </div>
    </div>
  )
}

function RequirementsPage({
  requirements,
  customers,
}: {
  requirements: CustomArrRequirementCatalogItem[]
  customers: CustomArrCustomerDetail[]
}) {
  const coverage = requirements.map((requirement) => ({
    ...requirement,
    count: customers.filter((customer) =>
      customer.requirements.some((customerRequirement) => customerRequirement.title === requirement.title),
    ).length,
  }))

  return (
    <div className="customarr-page">
      <PageHeader
        eyebrow="Requirements"
        title="Requirement catalog"
        description="Track the business requirements that every customer must satisfy before activation and keep a quick view of coverage."
      />
      <div className="customarr-grid cols-2">
        {coverage.map((requirement) => (
          <SectionCard key={requirement.requirementKey} title={requirement.title} icon={<ShieldCheck className="h-4 w-4 text-cyan-300" />}>
            <div className="space-y-2">
              <p className="text-sm text-slate-300">{requirement.description}</p>
              <p className="text-sm text-slate-300">Owner team: {requirement.ownerTeam}</p>
              <p className="text-sm text-slate-300">Applies to: {requirement.appliesTo.join(', ')}</p>
              <p className="text-xs text-slate-400">{requirement.count} customers carry this requirement in the current workspace snapshot.</p>
            </div>
          </SectionCard>
        ))}
      </div>
    </div>
  )
}

function ContactsPage({
  customers,
}: {
  customers: CustomArrCustomerDetail[]
}) {
  const contacts = listAllContacts(customers)

  return (
    <div className="customarr-page">
      <PageHeader
        eyebrow="Contacts"
        title="Customer contact directory"
        description="A quick view of the people who keep billing, operations, and onboarding moving for each customer."
      />
      <div className="customarr-grid cols-2">
        {contacts.map((contact) => (
          <SectionCard key={contact.contactId} title={contact.name} icon={<Contact2 className="h-4 w-4 text-cyan-300" />}>
            <div className="space-y-2 text-sm text-slate-300">
              <p>{contact.role}</p>
              <p>{contact.email}</p>
              <p>{contact.phone}</p>
              <p className="text-xs text-slate-400">
                {contact.customerNumber} · {contact.customerName}
              </p>
              <p>{contact.isPrimary ? 'Primary contact' : 'Secondary contact'}</p>
            </div>
          </SectionCard>
        ))}
      </div>
    </div>
  )
}

function SettingsPage({
  accessToken,
  session,
  customers,
}: {
  accessToken: string
  session: StoredCustomArrSession | null
  customers: CustomArrCustomerDetail[]
}) {
  return (
    <div className="customarr-page">
      <PageHeader
        eyebrow="Settings"
        title="Workspace settings"
        description="Launch routing, API wiring, and ownership reminders for the customer master."
        action={<span className="customarr-pill">{demoMode ? 'Demo enabled' : 'Live only'}</span>}
      />
      <div className="customarr-grid cols-2">
        <SectionCard title="Runtime wiring" icon={<DatabaseZap className="h-4 w-4 text-cyan-300" />}>
          <div className="space-y-2 text-sm text-slate-300">
            <p><strong className="text-slate-100">API base:</strong> <span className="customarr-pill">{apiBase || '/api proxy'}</span></p>
            <p><strong className="text-slate-100">Preview port:</strong> <span className="customarr-pill">5186</span></p>
            <p><strong className="text-slate-100">API port:</strong> <span className="customarr-pill">5111</span></p>
            <p><strong className="text-slate-100">Suite home:</strong> <span className="customarr-pill">{suiteHomeUrl}</span></p>
            <p><strong className="text-slate-100">Access token present:</strong> <span className="customarr-pill">{accessToken ? 'yes' : 'no'}</span></p>
            <p><strong className="text-slate-100">Current tenant:</strong> {session?.tenantDisplayName ?? demoWorkspaceSession.tenantDisplayName}</p>
          </div>
        </SectionCard>
        <SectionCard title="Ownership reminders" icon={<ShieldCheck className="h-4 w-4 text-cyan-300" />}>
          <div className="space-y-2 text-sm text-slate-300">
            <p>CustomArr owns the customer master, customer hierarchy, contact records, and customer-level requirement tracking.</p>
            <p>Other products may reference customer data, but they do not become the source of truth for the customer domain.</p>
            <p>{customers.length} customer records are available in the current workspace snapshot.</p>
          </div>
        </SectionCard>
      </div>
    </div>
  )
}

export default function App() {
  const location = useLocation()
  const session = loadSession()
  const [demoCustomers, setDemoCustomers] = useState(() => cloneCustomers(demoCustomersSeed))

  const sessionQuery = useQuery({
    queryKey: ['customarr', 'session', session?.accessToken],
    queryFn: () => getSessionBootstrap(session!.accessToken),
    enabled: Boolean(session?.accessToken),
    retry: false,
  })

  const launchCatalogQuery = useQuery({
    queryKey: ['customarr', 'launch-catalog', session?.accessToken],
    queryFn: () => getLaunchCatalog(apiBase, session!.accessToken, 'customarr'),
    enabled: Boolean(session?.accessToken),
    retry: false,
  })

  const customersQuery = useQuery({
    queryKey: ['customarr', 'customers'],
    queryFn: () => listCustomers(session!.accessToken),
    enabled: Boolean(session?.accessToken),
    staleTime: 20_000,
  })

  const requirementsQuery = useQuery({
    queryKey: ['customarr', 'requirements'],
    queryFn: () => listRequirements(session!.accessToken),
    enabled: Boolean(session?.accessToken),
    staleTime: 30_000,
  })

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

  const bootstrapError = sessionQuery.isError
    ? resolveProductWorkspaceBootstrapError(sessionQuery.error)
    : launchCatalogQuery.isError
      ? resolveProductWorkspaceBootstrapError(launchCatalogQuery.error)
      : null

  const liveCustomers = customersQuery.data ?? []
  const workspaceCustomers = session?.accessToken ? liveCustomers : demoCustomers
  const requirementCatalog = requirementsQuery.data ?? demoRequirementCatalog
  const workspaceSession =
    session && sessionQuery.data && !bootstrapError
      ? {
          userDisplayName: session.displayName,
          tenantDisplayName: session.tenantDisplayName,
          tenantSlug: session.tenantSlug,
        }
      : demoMode
        ? demoWorkspaceSession
        : null

  const switcherEntitlements =
    launchCatalogQuery.data?.products.map((product) => product.productKey) ??
    sessionQuery.data?.entitlements ??
    ['customarr']

  const launch = useProductWorkspaceLaunch({
    apiBase,
    accessToken: session?.accessToken ?? '',
    currentProductKey: 'customarr',
    suiteHomeUrl,
    productLaunchUrls,
  })

  const currentTitle = (() => {
    const path = location.pathname.replace(/\/+$/, '') || '/'
    if (path.startsWith('/customers/') && path !== '/customers/create') return 'Customer detail'
    if (path.startsWith('/customers/create')) return 'Create customer'
    if (path.startsWith('/customers')) return 'Customers'
    if (path.startsWith('/hierarchy')) return 'Hierarchy'
    if (path.startsWith('/requirements')) return 'Requirements'
    if (path.startsWith('/contacts')) return 'Contacts'
    if (path.startsWith('/settings')) return 'Settings'
    return 'Dashboard'
  })()

  const createDemoCustomer = (request: CustomArrCreateCustomerRequest): CustomArrCustomerDetail => {
    const created = buildDemoCustomer(
      {
        legalName: request.legalName,
        tradeName: request.tradeName,
        displayName: request.displayName ?? request.tradeName,
        dbaName: request.dbaName ?? '',
        status: request.status,
        tier: request.tier,
        segment: request.segment,
        ownerPersonId: request.ownerPersonId,
        assignedTeamId: request.assignedTeamId ?? '',
        customerSinceDate: request.customerSinceDate ?? '',
        sourceKey: request.sourceKey ?? 'manual',
        parentCustomerId: request.parentCustomerId,
        primaryContactName: request.primaryContactName,
        primaryContactEmail: request.primaryContactEmail,
        primaryContactPhone: request.primaryContactPhone,
        billingCity: request.billingCity,
        billingState: request.billingState,
        shippingCity: request.shippingCity,
        shippingState: request.shippingState,
        notes: request.notes,
        portalEnabled: Boolean(request.portalEnabled),
        portalDisplayName: request.portalDisplayName ?? '',
        paymentTermsKey: request.paymentTermsKey ?? 'net_30',
        defaultOrderTypeKey: request.defaultOrderTypeKey ?? 'customer_order',
        defaultServiceLevelKey: request.defaultServiceLevelKey ?? 'standard',
        requiresAppointment: Boolean(request.requiresAppointment),
        requiresProofOfDelivery: Boolean(request.requiresProofOfDelivery),
        requiresCustomerReference: Boolean(request.requiresCustomerReference),
        customerReferenceLabel: request.customerReferenceLabel ?? 'PO Number',
        defaultInstructions: request.defaultInstructions ?? '',
        notificationPreferenceKey: request.notificationPreferenceKey ?? 'email',
      },
      demoCustomers,
    )
    setDemoCustomers((previous) => [created, ...previous])
    return created
  }

  if (location.pathname === '/launch' || location.pathname === '/handoff') {
    return <LaunchPage />
  }

  return (
    <WorkspaceBootstrap
      accessToken={session?.accessToken ?? ''}
      bootstrapError={bootstrapError}
      workspaceSession={workspaceSession}
      switcherEntitlements={switcherEntitlements}
      isBootstrapping={Boolean(session?.accessToken) && (sessionQuery.isLoading || launchCatalogQuery.isLoading)}
      onSelectProduct={
        session?.accessToken
          ? (productKey) => {
              void launch.mutate(productKey)
            }
          : undefined
      }
      onSignOut={
        session
          ? () => {
              clearSession()
              window.location.assign(suiteHomeUrl)
            }
          : undefined
      }
      isProductLaunchPending={launch.isPending}
      productLaunchError={launch.isError ? formatProductLaunchError(launch.error) : null}
    >
      <Routes>
        <Route index element={<Navigate to="/dashboard" replace />} />
        <Route path="/dashboard" element={<DashboardPage accessToken={session?.accessToken ?? ''} customers={workspaceCustomers} />} />
        <Route path="/customers" element={<CustomersPage customers={workspaceCustomers} />} />
        <Route path="/customers/create" element={<CreateCustomerPage accessToken={session?.accessToken ?? ''} customers={workspaceCustomers} onCreateDemoCustomer={createDemoCustomer} />} />
        <Route path="/customers/:customerId" element={<CustomerDetailPage accessToken={session?.accessToken ?? ''} customers={workspaceCustomers} />} />
        <Route path="/hierarchy" element={<HierarchyPage customers={workspaceCustomers} />} />
        <Route path="/requirements" element={<RequirementsPage requirements={requirementCatalog} customers={workspaceCustomers} />} />
        <Route path="/contacts" element={<ContactsPage customers={workspaceCustomers} />} />
        <Route path="/settings" element={<SettingsPage accessToken={session?.accessToken ?? ''} session={session} customers={workspaceCustomers} />} />
        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
      <p className="mt-6 text-sm text-slate-400">Current view: {currentTitle}</p>
    </WorkspaceBootstrap>
  )
}

type FieldProps = {
  label: string
  value?: ReactNode
  children?: ReactNode
  wide?: boolean
}

function Field({ label, value, children, wide }: FieldProps) {
  return (
    <label className={wide ? 'md:col-span-2' : ''}>
      <div className="customarr-label mb-2">{label}</div>
      {children ?? <div className="rounded-xl border border-slate-700/70 bg-slate-900/70 px-4 py-3 text-sm text-slate-200">{value}</div>}
    </label>
  )
}
