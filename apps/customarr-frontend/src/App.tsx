import { useEffect, useMemo, useState, type ReactNode } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import {
  Building2,
  Clock3,
  Contact2,
  DatabaseZap,
  FilePlus2,
  LayoutDashboard,
  MapPinned,
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
    case 'active':
      return 'Active'
    case 'onboarding':
      return 'Onboarding'
    case 'watch':
      return 'Watch'
    case 'inactive':
      return 'Inactive'
    default:
      return status
  }
}

function toneForStatus(status: string): string {
  switch (status) {
    case 'active':
      return 'border-emerald-500/40 bg-emerald-500/10 text-emerald-100'
    case 'onboarding':
    case 'watch':
      return 'border-amber-500/40 bg-amber-500/10 text-amber-100'
    case 'inactive':
      return 'border-slate-500/30 bg-slate-900/80 text-slate-200'
    default:
      return 'border-slate-500/30 bg-slate-900/80 text-slate-200'
  }
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
    activeCustomerCount: customers.filter((customer) => customer.status === 'active').length,
    onboardingCustomerCount: customers.filter((customer) => customer.status === 'onboarding').length,
    watchListCustomerCount: customers.filter((customer) => customer.status === 'watch').length,
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
      customerName: customer.tradeName,
    })),
  )
}

function buildCustomerRequest(form: CustomerFormState): CustomArrCreateCustomerRequest {
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
  }
}

type CustomerFormState = {
  legalName: string
  tradeName: string
  status: CustomArrCustomerStatus
  tier: CustomArrCustomerTier
  segment: string
  ownerPersonId: string
  parentCustomerId: string
  primaryContactName: string
  primaryContactEmail: string
  primaryContactPhone: string
  billingCity: string
  billingState: string
  shippingCity: string
  shippingState: string
  notes: string
}

const initialCustomerForm: CustomerFormState = {
  legalName: '',
  tradeName: '',
  status: 'onboarding',
  tier: 'core',
  segment: '',
  ownerPersonId: 'person-999',
  parentCustomerId: '',
  primaryContactName: '',
  primaryContactEmail: '',
  primaryContactPhone: '',
  billingCity: '',
  billingState: '',
  shippingCity: '',
  shippingState: '',
  notes: '',
}

function buildDemoCustomer(
  form: CustomerFormState,
  existingCustomers: CustomArrCustomerDetail[],
): CustomArrCustomerDetail {
  const request = buildCustomerRequest(form)
  const nextNumber = existingCustomers.length + 1001
  const parent = existingCustomers.find((customer) => customer.customerId === request.parentCustomerId) ?? null
  const tradeName = request.tradeName || request.legalName
  const customerId = `cust-${crypto.randomUUID().slice(0, 8)}`
  const contactId = `ct-${crypto.randomUUID().slice(0, 8)}`
  const locationId = `loc-${crypto.randomUUID().slice(0, 8)}`
  const now = new Date().toISOString()

  return {
    customerId,
    customerNumber: `CUS-${nextNumber}`,
    legalName: request.legalName,
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
    paymentTerms: 'Net 30',
    riskRating: request.status === 'onboarding' ? 'medium' : 'low',
    notes: request.notes ? [request.notes] : ['Created in demo mode.'],
    contacts: [
      {
        contactId,
        name: request.primaryContactName,
        role: 'Primary contact',
        email: request.primaryContactEmail,
        phone: request.primaryContactPhone,
        isPrimary: true,
      },
    ],
    locations: [
      {
        locationId,
        label: 'Primary location',
        type: 'service',
        city: request.shippingCity || request.billingCity,
        state: request.shippingState || request.billingState,
      },
    ],
    requirements: demoRequirementCatalog.slice(0, 3).map((item, index) => ({
      requirementKey: `${customerId}-${item.requirementKey}`,
      title: item.title,
      owner: item.ownerTeam,
      status: index === 0 ? 'pending' : 'watch',
      dueAt: null,
    })),
    activity: [
      {
        activityId: `act-${crypto.randomUUID().slice(0, 8)}`,
        kind: 'created',
        message: 'Customer created in demo mode.',
        occurredAt: now,
      },
    ],
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
                    <p className="mt-1 text-sm text-slate-300">{customer.tradeName}</p>
                  </div>
                  {statusBadge(customer.status)}
                </div>
                <p className="mt-2 text-xs text-slate-400">
                  {customer.segment} · {customer.tier} · {customer.contactCount} contacts
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
      [customer.customerNumber, customer.tradeName, customer.legalName, customer.segment, customer.primaryContactName]
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
                  <h2 className="mt-1 text-xl font-semibold text-slate-50">{customer.tradeName}</h2>
                  <p className="mt-1 text-sm text-slate-300">{customer.legalName}</p>
                </div>
                {statusBadge(customer.status)}
              </div>
              <div className="grid gap-2 text-sm text-slate-300 md:grid-cols-2">
                <p><strong className="text-slate-100">Tier:</strong> {customer.tier}</p>
                <p><strong className="text-slate-100">Segment:</strong> {customer.segment}</p>
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

  return (
    <div className="customarr-page">
      <PageHeader
        eyebrow="Customer detail"
        title={customer.tradeName}
        description={`${customer.customerNumber} · ${customer.legalName}`}
        action={<span className="customarr-pill">{statusBadge(customer.status)}</span>}
      />
      {query.isError ? (
        <ApiErrorCallout
          title="Unable to load customer"
          message={getErrorMessage(query.error, 'Failed to load customer detail.')}
        />
      ) : null}

      <div className="customarr-grid cols-2">
        <SectionCard title="Profile" icon={<Building2 className="h-4 w-4 text-cyan-300" />}>
          <div className="grid gap-3 md:grid-cols-2">
            <Field label="Legal name" value={customer.legalName} />
            <Field label="Trade name" value={customer.tradeName} />
            <Field label="Tier" value={customer.tier} />
            <Field label="Segment" value={customer.segment} />
            <Field label="Owner" value={customer.ownerPersonId} />
            <Field label="Risk rating" value={customer.riskRating} />
            <Field label="Billing address" value={customer.billingAddress} wide />
            <Field label="Shipping address" value={customer.shippingAddress} wide />
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
            <p className="text-xs text-slate-400">Hold status: {customer.holdStatus} · Payment terms: {customer.paymentTerms}</p>
          </div>
        </SectionCard>
      </div>

      <div className="customarr-grid cols-2">
        <SectionCard title="Contacts" icon={<Contact2 className="h-4 w-4 text-cyan-300" />}>
          <div className="space-y-3">
            {customer.contacts.map((contact) => (
              <div key={contact.contactId} className="rounded-2xl border border-slate-700/70 bg-slate-900/70 p-4">
                <div className="flex items-center justify-between gap-3">
                  <strong className="text-slate-50">{contact.name}</strong>
                  {contact.isPrimary ? <span className="customarr-pill">Primary</span> : null}
                </div>
                <p className="mt-1 text-sm text-slate-300">{contact.role}</p>
                <p className="mt-1 text-sm text-slate-300">{contact.email}</p>
                <p className="mt-1 text-xs text-slate-400">{contact.phone}</p>
              </div>
            ))}
          </div>
        </SectionCard>
        <SectionCard title="Requirements" icon={<ShieldCheck className="h-4 w-4 text-cyan-300" />}>
          <div className="space-y-3">
            {customer.requirements.map((requirement) => (
              <div key={requirement.requirementKey} className="rounded-2xl border border-slate-700/70 bg-slate-900/70 p-4">
                <div className="flex items-center justify-between gap-3">
                  <strong className="text-slate-50">{requirement.title}</strong>
                  {statusBadge(requirement.status)}
                </div>
                <p className="mt-1 text-sm text-slate-300">Owner: {requirement.owner}</p>
                <p className="mt-1 text-xs text-slate-400">Due {formatDate(requirement.dueAt)}</p>
              </div>
            ))}
          </div>
        </SectionCard>
      </div>

      <SectionCard title="Activity" icon={<Clock3 className="h-4 w-4 text-cyan-300" />}>
        <div className="space-y-3">
          {customer.activity.map((item) => (
            <div key={item.activityId} className="rounded-2xl border border-slate-700/70 bg-slate-900/70 p-4">
              <div className="flex items-center justify-between gap-3">
                <strong className="text-slate-50">{item.kind}</strong>
                <span className="customarr-pill">{formatDate(item.occurredAt)}</span>
              </div>
              <p className="mt-2 text-sm text-slate-300">{item.message}</p>
            </div>
          ))}
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
            <Field label="Trade name"><input className="customarr-input" value={form.tradeName} onChange={(event) => setForm({ ...form, tradeName: event.target.value })} /></Field>
            <Field label="Status">
              <select className="customarr-select" value={form.status} onChange={(event) => setForm({ ...form, status: event.target.value as CustomerFormState['status'] })}>
                <option value="onboarding">Onboarding</option>
                <option value="active">Active</option>
                <option value="watch">Watch</option>
                <option value="inactive">Inactive</option>
              </select>
            </Field>
            <Field label="Tier">
              <select className="customarr-select" value={form.tier} onChange={(event) => setForm({ ...form, tier: event.target.value as CustomerFormState['tier'] })}>
                <option value="strategic">Strategic</option>
                <option value="core">Core</option>
                <option value="standard">Standard</option>
              </select>
            </Field>
            <Field label="Segment"><input className="customarr-input" value={form.segment} onChange={(event) => setForm({ ...form, segment: event.target.value })} /></Field>
            <Field label="Owner person id"><input className="customarr-input" value={form.ownerPersonId} onChange={(event) => setForm({ ...form, ownerPersonId: event.target.value })} /></Field>
            <Field label="Parent customer">
              <select className="customarr-select" value={form.parentCustomerId} onChange={(event) => setForm({ ...form, parentCustomerId: event.target.value })}>
                <option value="">Top-level customer</option>
                {customers.map((customer) => (
                  <option key={customer.customerId} value={customer.customerId}>
                    {customer.customerNumber} - {customer.tradeName}
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
          <SectionCard key={customer.customerId} title={customer.tradeName} icon={<Building2 className="h-4 w-4 text-cyan-300" />}>
            <div className="space-y-3">
              <p className="text-sm text-slate-300">{customer.customerNumber} · {customer.segment}</p>
              {childrenByParent.get(customer.customerId)?.length ? (
                <div className="space-y-2">
                  {childrenByParent.get(customer.customerId)?.map((child) => (
                    <Link key={child.customerId} to={`/customers/${child.customerId}`} className="block rounded-2xl border border-slate-700/70 bg-slate-900/70 px-4 py-3 text-sm text-slate-200">
                      {child.customerNumber} · {child.tradeName} · {child.status}
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
        status: request.status,
        tier: request.tier,
        segment: request.segment,
        ownerPersonId: request.ownerPersonId,
        parentCustomerId: request.parentCustomerId,
        primaryContactName: request.primaryContactName,
        primaryContactEmail: request.primaryContactEmail,
        primaryContactPhone: request.primaryContactPhone,
        billingCity: request.billingCity,
        billingState: request.billingState,
        shippingCity: request.shippingCity,
        shippingState: request.shippingState,
        notes: request.notes,
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
