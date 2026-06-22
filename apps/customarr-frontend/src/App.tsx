import { useEffect, useMemo, useState, type ReactNode } from 'react'
import { useMutation, useQueries, useQuery, useQueryClient } from '@tanstack/react-query'
import {
  Activity,
  BriefcaseBusiness,
  Building2,
  ClipboardCheck,
  Clock3,
  Contact2,
  CreditCard,
  DatabaseZap,
  FileCheck2,
  FilePlus2,
  GitBranch,
  Handshake,
  HeartPulse,
  LayoutDashboard,
  LifeBuoy,
  MapPinned,
  PanelTopOpen,
  PlugZap,
  Route as RouteIcon,
  Search,
  Settings,
  ShieldCheck,
  UploadCloud,
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
import { clearSession, loadSession } from './auth/sessionStorage'
import {
  type CustomArrCreateCustomerRequest,
  type CustomArrCrmRecord,
  type CustomArrCustomerDetail,
  type CustomArrCustomerStatus,
  type CustomArrCustomerTier,
  type CustomArrRequirementCatalogItem,
} from './demoData'
import {
  createCustomer,
  getCrmOverview,
  getCustomer,
  getCustomerCreateMetadata,
  getDashboard,
  getSessionBootstrap,
  getTenantSettings,
  listCrmRecords,
  listCustomers,
  listRequirements,
  updateTenantSettings,
  type CustomArrCustomerCreateMetadataResponse,
  type CustomArrCrmModuleRoute,
  type CustomArrTenantSettingsResponse,
  type CustomerAddressTypeItem,
  type CustomerClassificationCatalogItem,
  type CustomerContactRoleItem,
  type CustomerCustomFieldDefinitionItem,
  type CustomerDocumentRequirementItem,
  type CustomerDuplicateDetectionRuleItem,
  type CustomerNotificationRuleItem,
  type CustomerRequiredFieldRuleItem,
} from './api/client'
import { LaunchPage } from './LaunchPage'

const suiteHomeUrl = resolveSuiteHomeUrl(import.meta.env.VITE_SUITE_URL)
const productLaunchUrls = buildProductLaunchUrlMap(import.meta.env)
const apiBase = import.meta.env.VITE_CUSTOMARR_API_BASE ?? ''

const navItems: ProductNavItem[] = [
  { label: 'Dashboard', to: '/dashboard', icon: LayoutDashboard as ProductNavItem['icon'] },
  { label: 'Accounts', to: '/accounts', icon: Users as ProductNavItem['icon'] },
  { label: 'Pipeline', to: '/pipeline', icon: BriefcaseBusiness as ProductNavItem['icon'], sectionBreakBefore: true },
  { label: 'Commercial', to: '/commercial', icon: Handshake as ProductNavItem['icon'] },
  { label: 'Support', to: '/support', icon: LifeBuoy as ProductNavItem['icon'] },
  { label: 'Operations', to: '/operations', icon: ClipboardCheck as ProductNavItem['icon'] },
  { label: 'Health', to: '/health', icon: HeartPulse as ProductNavItem['icon'] },
  { label: 'Imports & Merge', to: '/imports', icon: UploadCloud as ProductNavItem['icon'] },
  { label: 'Integrations', to: '/integrations', icon: PlugZap as ProductNavItem['icon'] },
  { label: 'Settings', to: '/settings', icon: Settings as ProductNavItem['icon'], sectionBreakBefore: true },
]

type CrmModuleConfig = {
  key: CustomArrCrmModuleRoute
  title: string
  description: string
  icon: ReactNode
}

const crmAreas: Record<string, { eyebrow: string; title: string; description: string; modules: CrmModuleConfig[] }> = {
  pipeline: {
    eyebrow: 'Pipeline',
    title: 'Lead and opportunity workspace',
    description: 'Track customer commercial intent from prospect intake through explicit downstream handoff readiness.',
    modules: [
      { key: 'leads', title: 'Leads', description: 'Prospect intake, source, fit, and next follow-up.', icon: <BriefcaseBusiness className="h-4 w-4 text-cyan-300" /> },
      { key: 'opportunities', title: 'Opportunities', description: 'Customer opportunities, stage, forecast, and won handoffs.', icon: <RouteIcon className="h-4 w-4 text-cyan-300" /> },
    ],
  },
  commercial: {
    eyebrow: 'Commercial',
    title: 'Proposals and agreements',
    description: 'Manage proposal snapshots, customer responses, and agreement metadata without becoming invoice or ledger truth.',
    modules: [
      { key: 'proposals', title: 'Proposals', description: 'Pricing and terms snapshots for customer response.', icon: <FileCheck2 className="h-4 w-4 text-cyan-300" /> },
      { key: 'agreements', title: 'Agreements', description: 'Agreement metadata and RecordArr contract references.', icon: <Handshake className="h-4 w-4 text-cyan-300" /> },
    ],
  },
  support: {
    eyebrow: 'Support',
    title: 'Cases, activities, and tasks',
    description: 'Keep relationship work visible with customer cases, timeline events, and assigned follow-up tasks.',
    modules: [
      { key: 'cases', title: 'Cases', description: 'Customer relationship support and escalation records.', icon: <LifeBuoy className="h-4 w-4 text-cyan-300" /> },
      { key: 'activities', title: 'Activities', description: 'Timeline events and cross-product customer activity.', icon: <Activity className="h-4 w-4 text-cyan-300" /> },
      { key: 'tasks', title: 'Tasks', description: 'Assigned customer follow-up and readiness tasks.', icon: <Clock3 className="h-4 w-4 text-cyan-300" /> },
    ],
  },
  operations: {
    eyebrow: 'Operations',
    title: 'Locations, access, requirements, and eligibility',
    description: 'Maintain customer service readiness facts before handoffs move to execution products.',
    modules: [
      { key: 'locations', title: 'Locations', description: 'Customer locations exposed as customer_location records.', icon: <MapPinned className="h-4 w-4 text-cyan-300" /> },
      { key: 'contacts', title: 'Contacts', description: 'Customer contacts, authorization, consent, and freshness.', icon: <Contact2 className="h-4 w-4 text-cyan-300" /> },
      { key: 'portal-access', title: 'Portal access', description: 'NexArr-linked access records and role/location scope.', icon: <PanelTopOpen className="h-4 w-4 text-cyan-300" /> },
      { key: 'eligibility', title: 'Eligibility', description: 'Customer eligibility checks recorded before handoff.', icon: <ShieldCheck className="h-4 w-4 text-cyan-300" /> },
      { key: 'onboarding', title: 'Onboarding', description: 'Customer onboarding status and blockers.', icon: <ClipboardCheck className="h-4 w-4 text-cyan-300" /> },
    ],
  },
  health: {
    eyebrow: 'Health',
    title: 'Customer success and relationship health',
    description: 'Monitor customer health snapshots, churn risk, review cadence, and relationship freshness.',
    modules: [
      { key: 'health', title: 'Health profiles', description: 'Customer success status, score, and next review.', icon: <HeartPulse className="h-4 w-4 text-cyan-300" /> },
    ],
  },
  imports: {
    eyebrow: 'Imports & Merge',
    title: 'Imports, duplicate review, and merges',
    description: 'Review import batches, duplicate candidates, and merge proposals while keeping CustomArr the customer source of truth.',
    modules: [
      { key: 'imports', title: 'Imports', description: 'Import batches and validation state.', icon: <UploadCloud className="h-4 w-4 text-cyan-300" /> },
      { key: 'merge-review', title: 'Merge review', description: 'Customer merge review and survivor decisions.', icon: <GitBranch className="h-4 w-4 text-cyan-300" /> },
    ],
  },
  integrations: {
    eyebrow: 'Integrations',
    title: 'Integration references',
    description: 'Track external mappings and cross-product references without duplicating customer truth downstream.',
    modules: [
      { key: 'integration-references', title: 'Integration references', description: 'External system references for customer-owned records.', icon: <PlugZap className="h-4 w-4 text-cyan-300" /> },
    ],
  },
}

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
    case 'lead':
      return 'Lead'
    case 'prospect':
      return 'Prospect'
    case 'qualified':
      return 'Qualified'
    case 'onboarding':
      return 'Onboarding'
    case 'active':
      return 'Active'
    case 'suspended':
      return 'Suspended'
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
    case 'lost':
      return 'Lost'
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
    case 'qualified':
    case 'watch':
    case 'on_hold':
      return 'border-amber-500/40 bg-amber-500/10 text-amber-100'
    case 'lead':
      return 'border-cyan-500/40 bg-cyan-500/10 text-cyan-100'
    case 'blocked':
    case 'suspended':
      return 'border-rose-500/40 bg-rose-500/10 text-rose-100'
    case 'inactive':
    case 'archived':
    case 'lost':
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

const staffOwnerOptions: Array<{ personId: string; displayName: string; role: string }> = []

const staffTeamOptions: Array<{ teamId: string; displayName: string }> = []

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
  status: 'lead',
  tier: 'standard',
  segment: '',
  ownerPersonId: '',
  assignedTeamId: '',
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
  workspaceSession: {
    userId?: string
    tenantId?: string
    userDisplayName: string
    tenantDisplayName: string
    tenantSlug: string
  } | null
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
      workspaceSubtitle="CRM source of truth for tenant customers and customer relationships"
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
}: {
  accessToken: string
}) {
  const dashboardQuery = useQuery({
    queryKey: ['customarr', 'dashboard'],
    queryFn: () => getDashboard(accessToken),
    enabled: Boolean(accessToken),
    staleTime: 20_000,
  })
  const crmOverviewQuery = useQuery({
    queryKey: ['customarr', 'crm-overview'],
    queryFn: () => getCrmOverview(accessToken),
    enabled: Boolean(accessToken),
    staleTime: 20_000,
  })

  if (dashboardQuery.isError || crmOverviewQuery.isError) {
    return (
      <div className="customarr-page">
        <PageHeader
          eyebrow="CustomArr"
          title="Customer CRM control center"
          description="Maintain customer relationships, pipeline, commercial snapshots, support work, eligibility, onboarding, health, imports, and integration references from one source of truth."
        />
        <ApiErrorCallout
          title="Unable to load dashboard"
          message={getErrorMessage(dashboardQuery.error ?? crmOverviewQuery.error, 'Failed to load CustomArr dashboard.')}
        />
      </div>
    )
  }

  if (dashboardQuery.isLoading || crmOverviewQuery.isLoading || !dashboardQuery.data || !crmOverviewQuery.data) {
    return (
      <div className="customarr-page">
        <PageHeader
          eyebrow="CustomArr"
          title="Customer CRM control center"
          description="Maintain customer relationships, pipeline, commercial snapshots, support work, eligibility, onboarding, health, imports, and integration references from one source of truth."
        />
        <EmptyState title="Loading live CustomArr dashboard data from the API." />
      </div>
    )
  }

  const dashboard = dashboardQuery.data
  const crmOverview = crmOverviewQuery.data

  return (
    <div className="customarr-page">
      <PageHeader
        eyebrow="CustomArr"
        title="Customer CRM control center"
        description="Maintain customer relationships, pipeline, commercial snapshots, support work, eligibility, onboarding, health, imports, and integration references from one source of truth."
        action={
          <span className="customarr-pill">
            <DatabaseZap className="h-4 w-4" />
            Updated {formatDate(dashboard.generatedAt)}
          </span>
        }
      />
      <div className="customarr-grid cols-3">
        <MetricCard title="Customers" value={dashboard.customerCount} hint={`${dashboard.activeCustomerCount} active, ${dashboard.onboardingCustomerCount} onboarding`} />
        <MetricCard title="Leads" value={crmOverview.leadCount} hint={`${crmOverview.opportunityCount} open opportunities`} />
        <MetricCard title="Proposals" value={crmOverview.proposalCount} hint={`${crmOverview.agreementCount} agreement records`} />
        <MetricCard title="Cases" value={crmOverview.openCaseCount} hint={`${crmOverview.openTaskCount} open tasks`} />
        <MetricCard title="Eligibility" value={crmOverview.blockedEligibilityCount} hint="Blocked customer handoff checks" />
        <MetricCard title="Sites" value={dashboard.siteCount} hint="Billing, shipping, and service locations" />
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
        eyebrow="Accounts"
        title="Customer account register"
        description="Search the customer relationship source of truth, inspect hierarchy ownership, and jump into a timeline-centered customer record."
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
  tenantSettings,
}: {
  accessToken: string
  customers: CustomArrCustomerDetail[]
  tenantSettings: CustomArrTenantSettingsResponse
}) {
  const queryClient = useQueryClient()
  const navigate = useNavigate()
  const [form, setForm] = useState<CustomerFormState>(initialCustomerForm)
  const metadataQuery = useQuery({
    queryKey: ['customarr', 'customer-create-metadata'],
    queryFn: () => getCustomerCreateMetadata(accessToken),
    enabled: Boolean(accessToken),
    staleTime: 30_000,
  })
  const createMetadata = useMemo(
    () => metadataQuery.data ?? buildCreateMetadataFromSettings(tenantSettings),
    [metadataQuery.data, tenantSettings],
  )
  const lifecycleOptions = useMemo(
    () => createMetadata.lifecycleStages.filter((stage) => !stage.isTerminal),
    [createMetadata.lifecycleStages],
  )
  const customerTypeOptions = useMemo(
    () => createMetadata.classificationCatalogs.filter((item) => item.catalogType === 'customer_type' && item.isActive),
    [createMetadata.classificationCatalogs],
  )
  const paymentTermsOptions = useMemo(
    () => createMetadata.classificationCatalogs.filter((item) => item.catalogType === 'payment_terms' && item.isActive),
    [createMetadata.classificationCatalogs],
  )

  useEffect(() => {
    const initialStage = createMetadata.initialLifecycleStageKey
    const defaultType = customerTypeOptions.find((item) => item.isDefault)?.key ?? customerTypeOptions[0]?.key
    const defaultTerms = paymentTermsOptions.find((item) => item.isDefault)?.key ?? paymentTermsOptions[0]?.key
    setForm((previous) => ({
      ...previous,
      status: lifecycleOptions.some((stage) => stage.key === previous.status)
        ? previous.status
        : (initialStage as CustomerFormState['status']),
      tier: defaultType && !customerTypeOptions.some((item) => item.key === previous.tier)
        ? (defaultType as CustomerFormState['tier'])
        : previous.tier,
      paymentTermsKey: defaultTerms && !paymentTermsOptions.some((item) => item.key === previous.paymentTermsKey)
        ? defaultTerms
        : previous.paymentTermsKey,
    }))
  }, [createMetadata.initialLifecycleStageKey, customerTypeOptions, lifecycleOptions, paymentTermsOptions])

  const createMutation = useMutation({
    mutationFn: async () => {
      const request = buildCustomerRequest(form)
      if (!accessToken) {
        throw new Error('Missing access token for customer creation')
      }
      return createCustomer(accessToken, request)
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
        description="Capture the customer account once, then let downstream workflows consume the same canonical relationship record."
        action={<span className="customarr-pill">Live create</span>}
      />
      <div className="customarr-card">
        <div className="customarr-card-inner space-y-4">
          <div className="grid gap-3 md:grid-cols-2">
            <Field label="Legal name"><input className="customarr-input" value={form.legalName} onChange={(event) => setForm({ ...form, legalName: event.target.value })} /></Field>
            <Field label="Display name"><input className="customarr-input" value={form.displayName} onChange={(event) => setForm({ ...form, displayName: event.target.value })} /></Field>
            <Field label="DBA name"><input className="customarr-input" value={form.dbaName} onChange={(event) => setForm({ ...form, dbaName: event.target.value })} /></Field>
            <Field label="Legacy trade name"><input className="customarr-input" value={form.tradeName} onChange={(event) => setForm({ ...form, tradeName: event.target.value })} /></Field>
            <Field label="Lifecycle stage">
              <select className="customarr-select" value={form.status} onChange={(event) => setForm({ ...form, status: event.target.value as CustomerFormState['status'] })}>
                {lifecycleOptions.map((stage) => (
                  <option key={stage.key} value={stage.key}>
                    {stage.label}
                  </option>
                ))}
              </select>
            </Field>
            <Field label="Customer type">
              <select className="customarr-select" value={form.tier} onChange={(event) => setForm({ ...form, tier: event.target.value as CustomerFormState['tier'] })}>
                {customerTypeOptions.map((type) => (
                  <option key={type.key} value={type.key}>
                    {type.label}
                  </option>
                ))}
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
                {paymentTermsOptions.map((term) => (
                  <option key={term.key} value={term.key}>
                    {term.label}
                  </option>
                ))}
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

function buildCreateMetadataFromSettings(settings: CustomArrTenantSettingsResponse): CustomArrCustomerCreateMetadataResponse {
  return {
    customerNumberPreview: settings.numbering.preview,
    initialLifecycleStageKey: settings.lifecycleStages.find((stage) => stage.isInitial)?.key ?? settings.lifecycleStages[0]?.key ?? 'prospect',
    lifecycleStages: settings.lifecycleStages,
    classificationCatalogs: settings.classificationCatalogs,
    contactRoles: settings.contactRoles,
    addressTypes: settings.addressTypes,
    requiredFieldRules: settings.requiredFieldRules,
    onboardingTemplates: settings.onboardingTemplates,
    documentRequirements: settings.documentRequirements,
    customFieldDefinitions: settings.customFieldDefinitions,
    ownerRules: settings.ownerRules,
  }
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

function CrmAreaPage({
  accessToken,
  areaKey,
}: {
  accessToken: string
  areaKey: keyof typeof crmAreas
}) {
  const area = crmAreas[areaKey]
  const results = useQueries({
    queries: area.modules.map((module) => ({
      queryKey: ['customarr', 'crm-module', module.key],
      queryFn: () => listCrmRecords(accessToken, module.key),
      enabled: Boolean(accessToken),
      staleTime: 20_000,
    })),
  })
  const moduleRecords = area.modules.map((module, index) => {
    const query = results[index]
    return {
      module,
      query,
      records: (query?.data ?? []) as CustomArrCrmRecord[],
    }
  })

  if (moduleRecords.some((entry) => entry.query?.isLoading && !entry.query.data)) {
    return (
      <div className="customarr-page">
        <PageHeader eyebrow={area.eyebrow} title={area.title} description={area.description} />
        <EmptyState title={`Loading live ${area.title.toLowerCase()} data from the API.`} />
      </div>
    )
  }

  return (
    <div className="customarr-page">
      <PageHeader eyebrow={area.eyebrow} title={area.title} description={area.description} />
      {moduleRecords.some((entry) => entry.query?.isError) ? (
        <ApiErrorCallout
          title="Unable to load one or more CRM modules"
          message="Live CRM data could not be loaded for every module, so some sections may be empty until the API responds."
        />
      ) : null}
      <div className="space-y-4">
        {moduleRecords.map(({ module, records }) => (
          <SectionCard
            key={module.key}
            title={module.title}
            icon={module.icon}
            action={<span className="customarr-pill">{records.length} records</span>}
          >
            <p className="mb-3 text-sm text-slate-300">{module.description}</p>
            <CrmRecordTable records={records} />
          </SectionCard>
        ))}
      </div>
    </div>
  )
}

function CrmRecordTable({ records }: { records: CustomArrCrmRecord[] }) {
  if (records.length === 0) {
    return <EmptyState title="No records are currently available for this module." />
  }

  return (
    <div className="overflow-x-auto">
      <table className="min-w-full border-separate border-spacing-y-2 text-left text-sm">
        <thead className="text-xs uppercase tracking-[0.18em] text-cyan-200">
          <tr>
            <th className="px-3 py-2">Record</th>
            <th className="px-3 py-2">Customer</th>
            <th className="px-3 py-2">Status</th>
            <th className="px-3 py-2">Owner</th>
            <th className="px-3 py-2">Value</th>
            <th className="px-3 py-2">Due</th>
            <th className="px-3 py-2">Freshness</th>
          </tr>
        </thead>
        <tbody>
          {records.map((record) => (
            <tr key={`${record.module}-${record.id}`} className="rounded-2xl bg-slate-900/70 text-slate-200">
              <td className="rounded-l-2xl border-y border-l border-slate-700/70 px-3 py-3">
                <div className="font-semibold text-slate-50">{record.title}</div>
                <div className="mt-1 text-xs text-slate-400">{record.number} · {humanizeKey(record.module)}</div>
                {record.summary ? <div className="mt-1 max-w-xl text-xs text-slate-400">{record.summary}</div> : null}
              </td>
              <td className="border-y border-slate-700/70 px-3 py-3">{record.customerName ?? record.customerId ?? 'n/a'}</td>
              <td className="border-y border-slate-700/70 px-3 py-3">
                <div className="flex flex-col gap-1">
                  {statusBadge(record.statusKey)}
                  {record.secondaryStatusKey ? <span className="text-xs text-slate-400">{humanizeKey(record.secondaryStatusKey)}</span> : null}
                </div>
              </td>
              <td className="border-y border-slate-700/70 px-3 py-3">{staffPersonLabel(record.ownerPersonId)}</td>
              <td className="border-y border-slate-700/70 px-3 py-3">{typeof record.value === 'number' ? record.value.toLocaleString() : 'n/a'}</td>
              <td className="border-y border-slate-700/70 px-3 py-3">{formatDate(record.dueAt)}</td>
              <td className="rounded-r-2xl border-y border-r border-slate-700/70 px-3 py-3">
                <div className="flex flex-col gap-1">
                  <span className="customarr-pill w-fit">{record.freshness}</span>
                  <span className="text-xs text-slate-400">{record.sourceProductKey}</span>
                </div>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}

type SettingsSectionKey =
  | 'customer-identity'
  | 'lifecycle'
  | 'classifications'
  | 'required-fields'
  | 'contact-roles'
  | 'address-types'
  | 'onboarding'
  | 'portal'
  | 'documents'
  | 'duplicates'
  | 'integrations'
  | 'notifications'
  | 'custom-fields'

const settingsSections: Array<{ key: SettingsSectionKey; title: string; description: string; icon: ReactNode }> = [
  { key: 'customer-identity', title: 'Customer Identity', description: 'Numbering, manual override policy, and customer number preview.', icon: <CreditCard className="h-4 w-4 text-cyan-300" /> },
  { key: 'lifecycle', title: 'Lifecycle', description: 'Stages, transition rules, activation gates, and portal/order blockers.', icon: <ShieldCheck className="h-4 w-4 text-cyan-300" /> },
  { key: 'classifications', title: 'Classifications', description: 'Customer types, tiers, industries, territories, terms, and statuses.', icon: <GitBranch className="h-4 w-4 text-cyan-300" /> },
  { key: 'required-fields', title: 'Required Fields', description: 'Field-level requirements by lifecycle stage and customer type.', icon: <ClipboardCheck className="h-4 w-4 text-cyan-300" /> },
  { key: 'contact-roles', title: 'Contact Roles', description: 'Customer contact roles, portal access, and notification eligibility.', icon: <Contact2 className="h-4 w-4 text-cyan-300" /> },
  { key: 'address-types', title: 'Address Types', description: 'Customer-side location types and their operational use.', icon: <MapPinned className="h-4 w-4 text-cyan-300" /> },
  { key: 'onboarding', title: 'Onboarding', description: 'Checklist templates that govern customer activation readiness.', icon: <ClipboardCheck className="h-4 w-4 text-cyan-300" /> },
  { key: 'portal', title: 'Portal', description: 'Customer portal behavior, invitations, approvals, and allowed actions.', icon: <PanelTopOpen className="h-4 w-4 text-cyan-300" /> },
  { key: 'documents', title: 'Documents', description: 'Customer document requirements backed by RecordArr document types.', icon: <FileCheck2 className="h-4 w-4 text-cyan-300" /> },
  { key: 'duplicates', title: 'Duplicates', description: 'Duplicate detection fields, scoring, review, and blocking thresholds.', icon: <Search className="h-4 w-4 text-cyan-300" /> },
  { key: 'integrations', title: 'Integrations', description: 'External IDs, sync behavior, and conflict handling.', icon: <PlugZap className="h-4 w-4 text-cyan-300" /> },
  { key: 'notifications', title: 'Notifications', description: 'Customer lifecycle, document, portal, and escalation notifications.', icon: <Activity className="h-4 w-4 text-cyan-300" /> },
  { key: 'custom-fields', title: 'Custom Fields', description: 'Limited typed customer fields with no raw JSON or free-form references.', icon: <FilePlus2 className="h-4 w-4 text-cyan-300" /> },
]

function SettingsPage({
  tenantSettings,
  isLoading,
  isError,
  onSave,
  isSaving,
}: {
  tenantSettings: CustomArrTenantSettingsResponse
  isLoading: boolean
  isError: boolean
  onSave: (settings: CustomArrTenantSettingsResponse) => Promise<CustomArrTenantSettingsResponse>
  isSaving: boolean
}) {
  const { sectionKey } = useParams()
  const [draft, setDraft] = useState(tenantSettings)
  const selectedSection = settingsSections.find((section) => section.key === sectionKey)
  const hasChanges = JSON.stringify(draft) !== JSON.stringify(tenantSettings)

  useEffect(() => {
    setDraft(tenantSettings)
  }, [tenantSettings])

  const save = async () => {
    const saved = await onSave(draft)
    setDraft(saved)
  }

  return (
    <div className="customarr-page">
      <PageHeader
        eyebrow="Settings"
        title={selectedSection?.title ?? 'CustomArr tenant settings'}
        description={selectedSection?.description ?? 'Configure how this tenant creates, validates, classifies, onboards, portals, and integrates customer records.'}
        action={
          selectedSection ? (
            <div className="flex flex-wrap gap-2">
              <button className="customarr-button secondary" type="button" onClick={() => setDraft(tenantSettings)} disabled={!hasChanges || isSaving}>Cancel</button>
              <button className="customarr-button" type="button" onClick={() => void save()} disabled={!hasChanges || isSaving}>{isSaving ? 'Saving' : 'Save'}</button>
            </div>
          ) : (
            <span className="customarr-pill">Version {tenantSettings.settingsVersion}</span>
          )
        }
      />
      {isError ? <ApiErrorCallout title="Unable to load tenant settings" message="Live tenant settings could not be loaded from the API." /> : null}
      {isLoading ? <EmptyState title="Loading tenant settings." /> : null}
      {selectedSection ? (
        <SettingsSectionEditor settings={draft} sectionKey={selectedSection.key} onChange={setDraft} />
      ) : (
        <SettingsLanding settings={draft} />
      )}
    </div>
  )
}

function SettingsLanding({ settings }: { settings: CustomArrTenantSettingsResponse }) {
  return (
    <div className="space-y-4">
      <div className="customarr-grid cols-3">
        {settingsSections.map((section) => (
          <Link key={section.key} to={`/settings/${section.key}`} className="customarr-card block transition hover:border-cyan-400/60">
            <div className="customarr-card-inner space-y-3">
              <div className="flex items-center justify-between gap-3">
                <div className="flex items-center gap-2">
                  {section.icon}
                  <h2 className="text-base font-semibold text-slate-50">{section.title}</h2>
                </div>
                <span className="customarr-pill">{settingsSectionCount(settings, section.key)}</span>
              </div>
              <p className="text-sm text-slate-300">{section.description}</p>
            </div>
          </Link>
        ))}
      </div>
      <SectionCard title="Change impact" icon={<ShieldCheck className="h-4 w-4 text-cyan-300" />}>
        <div className="space-y-2">
          {settings.warnings.map((warning) => (
            <p key={warning.key} className="rounded-xl border border-amber-500/30 bg-amber-500/10 p-3 text-sm text-amber-100">{warning.message}</p>
          ))}
          <p className="text-xs text-slate-400">Last updated {formatDate(settings.updatedAt)}. Scope: {humanizeKey(settings.scope)}.</p>
        </div>
      </SectionCard>
    </div>
  )
}

function SettingsSectionEditor({
  settings,
  sectionKey,
  onChange,
}: {
  settings: CustomArrTenantSettingsResponse
  sectionKey: SettingsSectionKey
  onChange: (settings: CustomArrTenantSettingsResponse) => void
}) {
  const update = (next: Partial<CustomArrTenantSettingsResponse>) => onChange({ ...settings, ...next })
  const updateNumbering = (next: Partial<typeof settings.numbering>) => update({ numbering: { ...settings.numbering, ...next } })
  const updatePortal = (next: Partial<typeof settings.portalSettings>) => update({ portalSettings: { ...settings.portalSettings, ...next } })
  const updateIntegration = (next: Partial<typeof settings.integrationSettings>) => update({ integrationSettings: { ...settings.integrationSettings, ...next } })

  switch (sectionKey) {
    case 'customer-identity':
      return (
        <SectionCard title="Customer numbering" icon={<CreditCard className="h-4 w-4 text-cyan-300" />}>
          <div className="grid gap-3 md:grid-cols-2">
            <SettingTextInput label="Prefix" value={settings.numbering.prefix} onChange={(value) => updateNumbering({ prefix: value.toUpperCase() })} />
            <SettingNumberInput label="Padding length" value={settings.numbering.paddingLength} onChange={(value) => updateNumbering({ paddingLength: value })} />
            <SettingNumberInput label="Next number" value={settings.numbering.nextNumber} onChange={(value) => updateNumbering({ nextNumber: value })} />
            <SettingTextInput label="Display format" value={settings.numbering.displayFormat} onChange={(value) => updateNumbering({ displayFormat: value })} />
            <SettingToggle label="Allow manual override" checked={settings.numbering.allowManualOverride} onChange={(value) => updateNumbering({ allowManualOverride: value })} />
            <SettingToggle label="Override requires permission" checked={settings.numbering.manualOverrideRequiresPermission} onChange={(value) => updateNumbering({ manualOverrideRequiresPermission: value })} />
          </div>
          <p className="mt-4 rounded-xl border border-cyan-500/30 bg-cyan-500/10 p-3 text-sm text-cyan-100">Preview: {numberPreview(settings.numbering)}</p>
        </SectionCard>
      )
    case 'lifecycle':
      return (
        <div className="space-y-4">
          <SettingsTable title="Lifecycle stages" columns={['Stage', 'Behavior', 'Next']}>
            {settings.lifecycleStages.map((stage, index) => (
              <tr key={stage.key} className="bg-slate-900/70">
                <td className="rounded-l-xl border-y border-l border-slate-700/70 p-3">
                  <SettingTextInput label="Label" value={stage.label} onChange={(value) => updateList(settings, 'lifecycleStages', index, { label: value }, onChange)} />
                  <p className="mt-2 text-xs text-slate-400">{stage.key}</p>
                </td>
                <td className="border-y border-slate-700/70 p-3">
                  <div className="grid gap-2">
                    <SettingToggle label="Initial" checked={stage.isInitial} onChange={(value) => updateList(settings, 'lifecycleStages', index, { isInitial: value }, onChange)} />
                    <SettingToggle label="Active customer stage" checked={stage.isActiveCustomerStage} onChange={(value) => updateList(settings, 'lifecycleStages', index, { isActiveCustomerStage: value }, onChange)} />
                    <SettingToggle label="Blocks orders" checked={stage.blocksOrders} onChange={(value) => updateList(settings, 'lifecycleStages', index, { blocksOrders: value }, onChange)} />
                    <SettingToggle label="Blocks portal" checked={stage.blocksPortalAccess} onChange={(value) => updateList(settings, 'lifecycleStages', index, { blocksPortalAccess: value }, onChange)} />
                  </div>
                </td>
                <td className="rounded-r-xl border-y border-r border-slate-700/70 p-3 text-sm text-slate-300">{stage.allowedNextStageKeys.map(humanizeKey).join(', ') || 'Terminal'}</td>
              </tr>
            ))}
          </SettingsTable>
          <SettingsTable title="Transition rules" columns={['Transition', 'Approval', 'Blocks']}>
            {settings.transitionRules.map((rule) => (
              <tr key={`${rule.fromStageKey}-${rule.toStageKey}`} className="bg-slate-900/70 text-sm text-slate-300">
                <td className="rounded-l-xl border-y border-l border-slate-700/70 p-3">{humanizeKey(rule.fromStageKey)} to {humanizeKey(rule.toStageKey)}</td>
                <td className="border-y border-slate-700/70 p-3">{rule.requiresApproval ? rule.requiredPermission ?? 'Approval required' : 'No approval'}</td>
                <td className="rounded-r-xl border-y border-r border-slate-700/70 p-3">{[rule.blockIfOpenIssues && 'open issues', rule.blockIfExpiredRequiredDocuments && 'expired documents', rule.blockIfMissingRequiredFields && 'missing fields'].filter(Boolean).join(', ') || 'No configured blockers'}</td>
              </tr>
            ))}
          </SettingsTable>
        </div>
      )
    case 'classifications':
      return <CatalogSettings settings={settings} onChange={onChange} />
    case 'required-fields':
      return <RequiredFieldSettings settings={settings} onChange={onChange} />
    case 'contact-roles':
      return <ContactRoleSettings settings={settings} onChange={onChange} />
    case 'address-types':
      return <AddressTypeSettings settings={settings} onChange={onChange} />
    case 'onboarding':
      return <OnboardingSettings settings={settings} onChange={onChange} />
    case 'portal':
      return (
        <SectionCard title="Customer portal behavior" icon={<PanelTopOpen className="h-4 w-4 text-cyan-300" />}>
          <div className="grid gap-3 md:grid-cols-2">
            <SettingToggle label="Portal enabled" checked={settings.portalSettings.portalEnabled} onChange={(value) => updatePortal({ portalEnabled: value })} />
            <SettingToggle label="Invite only" checked={settings.portalSettings.inviteOnly} onChange={(value) => updatePortal({ inviteOnly: value })} />
            <SettingToggle label="Self registration allowed" checked={settings.portalSettings.selfRegistrationAllowed} onChange={(value) => updatePortal({ selfRegistrationAllowed: value })} />
            <SettingToggle label="Internal approval for portal users" checked={settings.portalSettings.requireInternalApprovalForPortalUsers} onChange={(value) => updatePortal({ requireInternalApprovalForPortalUsers: value })} />
            <SettingTextInput label="Portal display name" value={settings.portalSettings.portalDisplayName} onChange={(value) => updatePortal({ portalDisplayName: value })} />
            <SettingTextInput label="Support email" value={settings.portalSettings.supportContactEmail} onChange={(value) => updatePortal({ supportContactEmail: value })} />
          </div>
          <div className="mt-4 grid gap-2 md:grid-cols-3">
            {Object.entries(settings.portalSettings.allowedActions).map(([key, value]) => (
              <SettingToggle
                key={key}
                label={humanizeKey(key)}
                checked={value}
                onChange={(checked) => updatePortal({ allowedActions: { ...settings.portalSettings.allowedActions, [key]: checked } })}
              />
            ))}
          </div>
        </SectionCard>
      )
    case 'documents':
      return <DocumentSettings settings={settings} onChange={onChange} />
    case 'duplicates':
      return <DuplicateSettings settings={settings} onChange={onChange} />
    case 'integrations':
      return (
        <div className="space-y-4">
          <SectionCard title="Sync behavior" icon={<PlugZap className="h-4 w-4 text-cyan-300" />}>
            <div className="grid gap-3 md:grid-cols-2">
              <Field label="ERP sync mode">
                <select className="customarr-select" value={settings.integrationSettings.erpSyncMode} onChange={(event) => updateIntegration({ erpSyncMode: event.target.value })}>
                  <option value="none">None</option>
                  <option value="import_only">Import only</option>
                  <option value="external_master_readonly">External master read-only</option>
                  <option value="review_queue">Review queue</option>
                  <option value="bidirectional_proposal">Bidirectional proposal</option>
                </select>
              </Field>
              <Field label="Conflict resolution">
                <select className="customarr-select" value={settings.integrationSettings.defaultConflictResolution} onChange={(event) => updateIntegration({ defaultConflictResolution: event.target.value })}>
                  <option value="customarr_wins">CustomArr wins</option>
                  <option value="external_wins">External wins</option>
                  <option value="manual_review">Manual review</option>
                </select>
              </Field>
              <SettingToggle label="Allow external create" checked={settings.integrationSettings.allowExternalCreate} onChange={(value) => updateIntegration({ allowExternalCreate: value })} />
              <SettingToggle label="Allow external update" checked={settings.integrationSettings.allowExternalUpdate} onChange={(value) => updateIntegration({ allowExternalUpdate: value })} />
              <SettingToggle label="Review external updates" checked={settings.integrationSettings.requireReviewForExternalUpdate} onChange={(value) => updateIntegration({ requireReviewForExternalUpdate: value })} />
              <SettingToggle label="Emit prospect events" checked={settings.integrationSettings.emitEventsForProspects} onChange={(value) => updateIntegration({ emitEventsForProspects: value })} />
            </div>
          </SectionCard>
          <SettingsTable title="External ID sources" columns={['Source', 'Behavior', 'UI']}>
            {settings.externalIdSources.map((source) => (
              <tr key={source.key} className="bg-slate-900/70 text-sm text-slate-300">
                <td className="rounded-l-xl border-y border-l border-slate-700/70 p-3">{source.label}<div className="text-xs text-slate-400">{humanizeKey(source.sourceType)}</div></td>
                <td className="border-y border-slate-700/70 p-3">{source.required ? 'Required' : 'Optional'} · {source.uniqueWithinTenant ? 'Unique within tenant' : 'Reusable'}</td>
                <td className="rounded-r-xl border-y border-r border-slate-700/70 p-3">{source.visibleInUi ? 'Visible' : 'Hidden'} · {source.editableInUi ? 'Editable' : 'Read-only'}</td>
              </tr>
            ))}
          </SettingsTable>
        </div>
      )
    case 'notifications':
      return <NotificationSettings settings={settings} onChange={onChange} />
    case 'custom-fields':
      return <CustomFieldSettings settings={settings} onChange={onChange} />
    default:
      return <EmptyState title="Settings section was not found." />
  }
}

type SettingsArrayKey =
  | 'lifecycleStages'
  | 'classificationCatalogs'
  | 'requiredFieldRules'
  | 'contactRoles'
  | 'addressTypes'
  | 'onboardingTemplates'
  | 'onboardingChecklistItems'
  | 'documentRequirements'
  | 'duplicateDetectionRules'
  | 'notificationRules'
  | 'customFieldDefinitions'

function updateList<T>(
  settings: CustomArrTenantSettingsResponse,
  key: SettingsArrayKey,
  index: number,
  patch: Partial<T>,
  onChange: (settings: CustomArrTenantSettingsResponse) => void,
) {
  const current = settings[key] as unknown as T[]
  onChange({
    ...settings,
    [key]: current.map((item, itemIndex) => (itemIndex === index ? { ...item, ...patch } : item)),
  } as CustomArrTenantSettingsResponse)
}

function settingsSectionCount(settings: CustomArrTenantSettingsResponse, key: SettingsSectionKey): string {
  switch (key) {
    case 'customer-identity':
      return settings.numbering.preview || numberPreview(settings.numbering)
    case 'lifecycle':
      return `${settings.lifecycleStages.length} stages`
    case 'classifications':
      return `${settings.classificationCatalogs.length} values`
    case 'required-fields':
      return `${settings.requiredFieldRules.length} rules`
    case 'contact-roles':
      return `${settings.contactRoles.length} roles`
    case 'address-types':
      return `${settings.addressTypes.length} types`
    case 'onboarding':
      return `${settings.onboardingTemplates.length} templates`
    case 'portal':
      return settings.portalSettings.portalEnabled ? 'Enabled' : 'Disabled'
    case 'documents':
      return `${settings.documentRequirements.length} docs`
    case 'duplicates':
      return `${settings.duplicateDetectionRules.length} rules`
    case 'integrations':
      return humanizeKey(settings.integrationSettings.erpSyncMode)
    case 'notifications':
      return `${settings.notificationRules.length} rules`
    case 'custom-fields':
      return `${settings.customFieldDefinitions.length} fields`
  }
}

function numberPreview(numbering: CustomArrTenantSettingsResponse['numbering']) {
  const padded = String(numbering.nextNumber).padStart(Math.max(1, numbering.paddingLength), '0')
  return numbering.displayFormat.replace('{prefix}', numbering.prefix).replace('{number}', padded)
}

function SettingsTable({
  title,
  columns,
  children,
}: {
  title: string
  columns: string[]
  children: ReactNode
}) {
  return (
    <SectionCard title={title} icon={<DatabaseZap className="h-4 w-4 text-cyan-300" />}>
      <div className="overflow-x-auto">
        <table className="min-w-full border-separate border-spacing-y-2 text-left">
          <thead className="text-xs uppercase tracking-[0.18em] text-cyan-200">
            <tr>
              {columns.map((column) => (
                <th key={column} className="px-3 py-2">{column}</th>
              ))}
            </tr>
          </thead>
          <tbody>{children}</tbody>
        </table>
      </div>
    </SectionCard>
  )
}

function SettingTextInput({
  label,
  value,
  onChange,
}: {
  label: string
  value: string
  onChange: (value: string) => void
}) {
  return (
    <Field label={label}>
      <input className="customarr-input" value={value} onChange={(event) => onChange(event.target.value)} />
    </Field>
  )
}

function SettingNumberInput({
  label,
  value,
  onChange,
}: {
  label: string
  value: number
  onChange: (value: number) => void
}) {
  return (
    <Field label={label}>
      <input className="customarr-input" type="number" value={value} onChange={(event) => onChange(Number(event.target.value))} />
    </Field>
  )
}

function SettingToggle({
  label,
  checked,
  onChange,
}: {
  label: string
  checked: boolean
  onChange: (value: boolean) => void
}) {
  return (
    <label className="flex items-center gap-3 rounded-xl border border-slate-700/70 bg-slate-900/70 px-4 py-3 text-sm text-slate-200">
      <input type="checkbox" checked={checked} onChange={(event) => onChange(event.target.checked)} />
      {label}
    </label>
  )
}

function CatalogSettings({
  settings,
  onChange,
}: {
  settings: CustomArrTenantSettingsResponse
  onChange: (settings: CustomArrTenantSettingsResponse) => void
}) {
  return (
    <SettingsTable title="Classification catalogs" columns={['Catalog', 'Label', 'State']}>
      {settings.classificationCatalogs.map((item, index) => (
        <tr key={`${item.catalogType}-${item.key}`} className="bg-slate-900/70">
          <td className="rounded-l-xl border-y border-l border-slate-700/70 p-3 text-sm text-slate-300">
            <strong className="text-slate-50">{humanizeKey(item.catalogType)}</strong>
            <div className="text-xs text-slate-400">{item.key}</div>
          </td>
          <td className="border-y border-slate-700/70 p-3">
            <input className="customarr-input" value={item.label} onChange={(event) => updateList<CustomerClassificationCatalogItem>(settings, 'classificationCatalogs', index, { label: event.target.value }, onChange)} />
          </td>
          <td className="rounded-r-xl border-y border-r border-slate-700/70 p-3">
            <div className="grid gap-2">
              <SettingToggle label="Active" checked={item.isActive} onChange={(value) => updateList<CustomerClassificationCatalogItem>(settings, 'classificationCatalogs', index, { isActive: value }, onChange)} />
              <SettingToggle label="Default" checked={item.isDefault} onChange={(value) => updateList<CustomerClassificationCatalogItem>(settings, 'classificationCatalogs', index, { isDefault: value }, onChange)} />
            </div>
          </td>
        </tr>
      ))}
    </SettingsTable>
  )
}

function RequiredFieldSettings({
  settings,
  onChange,
}: {
  settings: CustomArrTenantSettingsResponse
  onChange: (settings: CustomArrTenantSettingsResponse) => void
}) {
  return (
    <div className="space-y-4">
      <SettingsTable title="Required field rules" columns={['Field', 'Applies', 'Requirement']}>
        {settings.requiredFieldRules.map((rule, index) => (
          <tr key={`${rule.fieldKey}-${rule.lifecycleStageKey ?? 'any'}-${rule.customerTypeKey ?? 'any'}`} className="bg-slate-900/70">
            <td className="rounded-l-xl border-y border-l border-slate-700/70 p-3 text-sm text-slate-300">{humanizeKey(rule.fieldKey)}</td>
            <td className="border-y border-slate-700/70 p-3 text-sm text-slate-300">
              {humanizeKey(rule.lifecycleStageKey ?? 'all stages')} · {humanizeKey(rule.customerTypeKey ?? 'all types')}
              <div className="mt-1 text-xs text-slate-400">{rule.validationMessage}</div>
            </td>
            <td className="rounded-r-xl border-y border-r border-slate-700/70 p-3">
              <select className="customarr-select" value={rule.requirementLevel} onChange={(event) => updateList<CustomerRequiredFieldRuleItem>(settings, 'requiredFieldRules', index, { requirementLevel: event.target.value }, onChange)}>
                <option value="hidden">Hidden</option>
                <option value="optional">Optional</option>
                <option value="recommended">Recommended</option>
                <option value="required">Required</option>
              </select>
            </td>
          </tr>
        ))}
      </SettingsTable>
      <SettingsTable title="Ownership rules" columns={['Rule', 'Default owner', 'Approval']}>
        {settings.ownerRules.map((rule) => (
          <tr key={rule.ruleName} className="bg-slate-900/70 text-sm text-slate-300">
            <td className="rounded-l-xl border-y border-l border-slate-700/70 p-3">{rule.ruleName}</td>
            <td className="border-y border-slate-700/70 p-3">{humanizeKey(rule.defaultOwnerType)} · {rule.defaultOwnerNameSnapshot}</td>
            <td className="rounded-r-xl border-y border-r border-slate-700/70 p-3">{rule.requiresApprovalForReassignment ? rule.approvalPermission : 'No reassignment approval'}</td>
          </tr>
        ))}
      </SettingsTable>
    </div>
  )
}

function ContactRoleSettings({
  settings,
  onChange,
}: {
  settings: CustomArrTenantSettingsResponse
  onChange: (settings: CustomArrTenantSettingsResponse) => void
}) {
  return (
    <SettingsTable title="Contact roles" columns={['Role', 'Requirements', 'Notifications']}>
      {settings.contactRoles.map((role, index) => (
        <tr key={role.key} className="bg-slate-900/70">
          <td className="rounded-l-xl border-y border-l border-slate-700/70 p-3">
            <input className="customarr-input" value={role.label} onChange={(event) => updateList<CustomerContactRoleItem>(settings, 'contactRoles', index, { label: event.target.value }, onChange)} />
            <p className="mt-1 text-xs text-slate-400">{role.key}</p>
          </td>
          <td className="border-y border-slate-700/70 p-3">
            <div className="grid gap-2">
              <SettingToggle label="Required for active customer" checked={role.isRequiredForActiveCustomer} onChange={(value) => updateList<CustomerContactRoleItem>(settings, 'contactRoles', index, { isRequiredForActiveCustomer: value }, onChange)} />
              <SettingToggle label="Unique primary" checked={role.requiresUniquePrimary} onChange={(value) => updateList<CustomerContactRoleItem>(settings, 'contactRoles', index, { requiresUniquePrimary: value }, onChange)} />
              <SettingToggle label="Allows portal access" checked={role.allowsPortalAccess} onChange={(value) => updateList<CustomerContactRoleItem>(settings, 'contactRoles', index, { allowsPortalAccess: value }, onChange)} />
            </div>
          </td>
          <td className="rounded-r-xl border-y border-r border-slate-700/70 p-3 text-sm text-slate-300">
            {[role.canReceiveOrderNotifications && 'Orders', role.canReceiveBillingNotifications && 'Billing', role.canReceiveComplianceNotifications && 'Compliance'].filter(Boolean).join(', ') || 'No notifications'}
          </td>
        </tr>
      ))}
    </SettingsTable>
  )
}

function AddressTypeSettings({
  settings,
  onChange,
}: {
  settings: CustomArrTenantSettingsResponse
  onChange: (settings: CustomArrTenantSettingsResponse) => void
}) {
  return (
    <SettingsTable title="Address and location types" columns={['Type', 'Validation', 'Usable for']}>
      {settings.addressTypes.map((type, index) => (
        <tr key={type.key} className="bg-slate-900/70">
          <td className="rounded-l-xl border-y border-l border-slate-700/70 p-3">
            <input className="customarr-input" value={type.label} onChange={(event) => updateList<CustomerAddressTypeItem>(settings, 'addressTypes', index, { label: event.target.value }, onChange)} />
            <p className="mt-1 text-xs text-slate-400">{type.key}</p>
          </td>
          <td className="border-y border-slate-700/70 p-3">
            <div className="grid gap-2">
              <SettingToggle label="Required for active customer" checked={type.isRequiredForActiveCustomer} onChange={(value) => updateList<CustomerAddressTypeItem>(settings, 'addressTypes', index, { isRequiredForActiveCustomer: value }, onChange)} />
              <SettingToggle label="Requires validation" checked={type.requiresValidation} onChange={(value) => updateList<CustomerAddressTypeItem>(settings, 'addressTypes', index, { requiresValidation: value }, onChange)} />
              <SettingToggle label="Requires geocode" checked={type.requiresGeocode} onChange={(value) => updateList<CustomerAddressTypeItem>(settings, 'addressTypes', index, { requiresGeocode: value }, onChange)} />
            </div>
          </td>
          <td className="rounded-r-xl border-y border-r border-slate-700/70 p-3 text-sm text-slate-300">
            {[type.usableForBilling && 'Billing', type.usableForPickup && 'Pickup', type.usableForDelivery && 'Delivery', type.usableForService && 'Service'].filter(Boolean).join(', ')}
          </td>
        </tr>
      ))}
    </SettingsTable>
  )
}

function OnboardingSettings({
  settings,
  onChange,
}: {
  settings: CustomArrTenantSettingsResponse
  onChange: (settings: CustomArrTenantSettingsResponse) => void
}) {
  return (
    <div className="space-y-4">
      <SettingsTable title="Onboarding templates" columns={['Template', 'Applies', 'Activation']}>
        {settings.onboardingTemplates.map((template, index) => (
          <tr key={template.key} className="bg-slate-900/70">
            <td className="rounded-l-xl border-y border-l border-slate-700/70 p-3">
              <input className="customarr-input" value={template.label} onChange={(event) => updateList(settings, 'onboardingTemplates', index, { label: event.target.value }, onChange)} />
              <p className="mt-1 text-xs text-slate-400">{template.key}</p>
            </td>
            <td className="border-y border-slate-700/70 p-3 text-sm text-slate-300">{humanizeKey(template.customerTypeKey ?? 'all customer types')}</td>
            <td className="rounded-r-xl border-y border-r border-slate-700/70 p-3">
              <SettingToggle label="Blocks activation until complete" checked={template.blocksActivationUntilComplete} onChange={(value) => updateList(settings, 'onboardingTemplates', index, { blocksActivationUntilComplete: value }, onChange)} />
            </td>
          </tr>
        ))}
      </SettingsTable>
      <SettingsTable title="Checklist item templates" columns={['Item', 'Type', 'Blocks']}>
        {settings.onboardingChecklistItems.map((item) => (
          <tr key={`${item.templateKey}-${item.key}`} className="bg-slate-900/70 text-sm text-slate-300">
            <td className="rounded-l-xl border-y border-l border-slate-700/70 p-3">{item.label}<div className="text-xs text-slate-400">{item.templateKey}</div></td>
            <td className="border-y border-slate-700/70 p-3">{humanizeKey(item.itemType)} · {item.required ? 'Required' : 'Optional'}</td>
            <td className="rounded-r-xl border-y border-r border-slate-700/70 p-3">{[item.blocksActivation && 'Activation', item.blocksOrders && 'Orders', item.blocksPortalAccess && 'Portal'].filter(Boolean).join(', ') || 'No blockers'}</td>
          </tr>
        ))}
      </SettingsTable>
    </div>
  )
}

function DocumentSettings({
  settings,
  onChange,
}: {
  settings: CustomArrTenantSettingsResponse
  onChange: (settings: CustomArrTenantSettingsResponse) => void
}) {
  return (
    <SettingsTable title="Document requirements" columns={['Requirement', 'RecordArr type', 'Blocks']}>
      {settings.documentRequirements.map((requirement, index) => (
        <tr key={requirement.key} className="bg-slate-900/70">
          <td className="rounded-l-xl border-y border-l border-slate-700/70 p-3">
            <input className="customarr-input" value={requirement.label} onChange={(event) => updateList<CustomerDocumentRequirementItem>(settings, 'documentRequirements', index, { label: event.target.value }, onChange)} />
            <p className="mt-1 text-xs text-slate-400">{requirement.required ? 'Required' : 'Optional'} · {requirement.expires ? `${requirement.expirationWarningDays ?? 0} day warning` : 'No expiration'}</p>
          </td>
          <td className="border-y border-slate-700/70 p-3 text-sm text-slate-300">{requirement.recordArrDocumentTypeKey}</td>
          <td className="rounded-r-xl border-y border-r border-slate-700/70 p-3">
            <div className="grid gap-2">
              <SettingToggle label="Blocks activation" checked={requirement.blocksActivation} onChange={(value) => updateList<CustomerDocumentRequirementItem>(settings, 'documentRequirements', index, { blocksActivation: value }, onChange)} />
              <SettingToggle label="Blocks orders" checked={requirement.blocksOrders} onChange={(value) => updateList<CustomerDocumentRequirementItem>(settings, 'documentRequirements', index, { blocksOrders: value }, onChange)} />
              <SettingToggle label="Visible in portal" checked={requirement.visibleInPortal} onChange={(value) => updateList<CustomerDocumentRequirementItem>(settings, 'documentRequirements', index, { visibleInPortal: value }, onChange)} />
            </div>
          </td>
        </tr>
      ))}
    </SettingsTable>
  )
}

function DuplicateSettings({
  settings,
  onChange,
}: {
  settings: CustomArrTenantSettingsResponse
  onChange: (settings: CustomArrTenantSettingsResponse) => void
}) {
  return (
    <SettingsTable title="Duplicate detection rules" columns={['Rule', 'Scoring', 'Thresholds']}>
      {settings.duplicateDetectionRules.map((rule, index) => (
        <tr key={rule.key} className="bg-slate-900/70">
          <td className="rounded-l-xl border-y border-l border-slate-700/70 p-3">
            <input className="customarr-input" value={rule.label} onChange={(event) => updateList<CustomerDuplicateDetectionRuleItem>(settings, 'duplicateDetectionRules', index, { label: event.target.value }, onChange)} />
            <p className="mt-1 text-xs text-slate-400">{humanizeKey(rule.matchField)} · {humanizeKey(rule.matchType)}</p>
          </td>
          <td className="border-y border-slate-700/70 p-3"><SettingNumberInput label="Weight" value={rule.weight} onChange={(value) => updateList<CustomerDuplicateDetectionRuleItem>(settings, 'duplicateDetectionRules', index, { weight: value }, onChange)} /></td>
          <td className="rounded-r-xl border-y border-r border-slate-700/70 p-3 text-sm text-slate-300">Review {rule.reviewThreshold} · Block {rule.autoBlockThreshold}</td>
        </tr>
      ))}
    </SettingsTable>
  )
}

function NotificationSettings({
  settings,
  onChange,
}: {
  settings: CustomArrTenantSettingsResponse
  onChange: (settings: CustomArrTenantSettingsResponse) => void
}) {
  return (
    <SettingsTable title="Notification and escalation rules" columns={['Rule', 'Recipient', 'Timing']}>
      {settings.notificationRules.map((rule, index) => (
        <tr key={rule.key} className="bg-slate-900/70">
          <td className="rounded-l-xl border-y border-l border-slate-700/70 p-3">
            <input className="customarr-input" value={rule.label} onChange={(event) => updateList<CustomerNotificationRuleItem>(settings, 'notificationRules', index, { label: event.target.value }, onChange)} />
            <p className="mt-1 text-xs text-slate-400">{humanizeKey(rule.eventType)}</p>
          </td>
          <td className="border-y border-slate-700/70 p-3 text-sm text-slate-300">{humanizeKey(rule.recipientType)}{rule.recipientNameSnapshot ? ` · ${rule.recipientNameSnapshot}` : ''}</td>
          <td className="rounded-r-xl border-y border-r border-slate-700/70 p-3 text-sm text-slate-300">Delay {rule.delayMinutes} min · Escalate {rule.escalationAfterMinutes ? `${rule.escalationAfterMinutes} min` : 'never'}</td>
        </tr>
      ))}
    </SettingsTable>
  )
}

function CustomFieldSettings({
  settings,
  onChange,
}: {
  settings: CustomArrTenantSettingsResponse
  onChange: (settings: CustomArrTenantSettingsResponse) => void
}) {
  const addField = () => {
    const nextField: CustomerCustomFieldDefinitionItem = {
      key: `customer_field_${settings.customFieldDefinitions.length + 1}`,
      label: 'New Customer Field',
      description: 'Tenant-defined customer field.',
      fieldType: 'text',
      appliesToCustomerTypeKey: null,
      appliesToLifecycleStageKey: null,
      required: false,
      visibleInPortal: false,
      editableInPortal: false,
      internalOnly: true,
      sortOrder: settings.customFieldDefinitions.length + 1,
      isActive: true,
    }
    onChange({ ...settings, customFieldDefinitions: [...settings.customFieldDefinitions, nextField] })
  }

  return (
    <SectionCard title="Limited customer custom fields" icon={<FilePlus2 className="h-4 w-4 text-cyan-300" />} action={<button type="button" className="customarr-button secondary" onClick={addField}>Add field</button>}>
      <div className="space-y-3">
        {settings.customFieldDefinitions.map((field, index) => (
          <div key={field.key} className="grid gap-3 rounded-xl border border-slate-700/70 bg-slate-900/70 p-3 md:grid-cols-4">
            <SettingTextInput label="Label" value={field.label} onChange={(value) => updateList<CustomerCustomFieldDefinitionItem>(settings, 'customFieldDefinitions', index, { label: value }, onChange)} />
            <Field label="Type">
              <select className="customarr-select" value={field.fieldType} onChange={(event) => updateList<CustomerCustomFieldDefinitionItem>(settings, 'customFieldDefinitions', index, { fieldType: event.target.value }, onChange)}>
                <option value="text">Text</option>
                <option value="number">Number</option>
                <option value="date">Date</option>
                <option value="boolean">Boolean</option>
                <option value="enum">Enum</option>
                <option value="money">Money</option>
                <option value="email">Email</option>
                <option value="phone">Phone</option>
              </select>
            </Field>
            <SettingToggle label="Required" checked={field.required} onChange={(value) => updateList<CustomerCustomFieldDefinitionItem>(settings, 'customFieldDefinitions', index, { required: value }, onChange)} />
            <SettingToggle label="Visible in portal" checked={field.visibleInPortal} onChange={(value) => updateList<CustomerCustomFieldDefinitionItem>(settings, 'customFieldDefinitions', index, { visibleInPortal: value }, onChange)} />
          </div>
        ))}
        {settings.customFieldDefinitions.length === 0 ? <EmptyState title="No tenant-defined customer fields are configured." /> : null}
      </div>
    </SectionCard>
  )
}

export default function App() {
  const location = useLocation()
  const queryClient = useQueryClient()
  const session = loadSession()

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

  const tenantSettingsQuery = useQuery({
    queryKey: ['customarr', 'tenant-settings'],
    queryFn: () => getTenantSettings(session!.accessToken),
    enabled: Boolean(session?.accessToken),
    staleTime: 30_000,
  })

  const updateSettingsMutation = useMutation({
    mutationFn: async (next: CustomArrTenantSettingsResponse) => {
      if (!session?.accessToken) {
        throw new Error('Missing access token for tenant settings update')
      }
      return updateTenantSettings(session.accessToken, next)
    },
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['customarr', 'tenant-settings'] })
      await queryClient.invalidateQueries({ queryKey: ['customarr', 'customer-create-metadata'] })
    },
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

  const workspaceCustomers = customersQuery.data ?? []
  const tenantSettings = tenantSettingsQuery.data
  const requirementCatalog = requirementsQuery.data ?? []
  const workspaceSession =
    session && sessionQuery.data && !bootstrapError
      ? {
          userId: session.userId,
          tenantId: session.tenantId,
          userDisplayName: session.displayName,
          tenantDisplayName: session.tenantDisplayName,
          tenantSlug: session.tenantSlug,
        }
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
    if (path.startsWith('/accounts')) return 'Accounts'
    if (path.startsWith('/pipeline')) return 'Pipeline'
    if (path.startsWith('/commercial')) return 'Commercial'
    if (path.startsWith('/support')) return 'Support'
    if (path.startsWith('/operations')) return 'Operations'
    if (path.startsWith('/health')) return 'Health'
    if (path.startsWith('/imports')) return 'Imports & Merge'
    if (path.startsWith('/integrations')) return 'Integrations'
    if (path.startsWith('/customers')) return 'Customers'
    if (path.startsWith('/hierarchy')) return 'Hierarchy'
    if (path.startsWith('/requirements')) return 'Requirements'
    if (path.startsWith('/contacts')) return 'Contacts'
    if (path.startsWith('/settings')) return 'Settings'
    return 'Dashboard'
  })()

  if (location.pathname === '/handoff') {
    return <Navigate replace to={{ pathname: '/launch', search: location.search }} />
  }

  if (location.pathname === '/launch') {
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
        <Route path="/dashboard" element={<DashboardPage accessToken={session?.accessToken ?? ''} />} />
        <Route path="/accounts" element={<CustomersPage customers={workspaceCustomers} />} />
        <Route path="/customers" element={<CustomersPage customers={workspaceCustomers} />} />
        <Route
          path="/customers/create"
          element={tenantSettingsQuery.isError ? (
            <ApiErrorCallout
              title="Unable to load customer create settings"
              message={getErrorMessage(tenantSettingsQuery.error, 'Failed to load CustomArr tenant settings.')}
            />
          ) : tenantSettings ? (
            <CreateCustomerPage accessToken={session?.accessToken ?? ''} customers={workspaceCustomers} tenantSettings={tenantSettings} />
          ) : (
            <EmptyState title="Loading tenant settings from the API." />
          )}
        />
        <Route path="/customers/:customerId" element={<CustomerDetailPage accessToken={session?.accessToken ?? ''} customers={workspaceCustomers} />} />
        <Route path="/hierarchy" element={<HierarchyPage customers={workspaceCustomers} />} />
        <Route path="/requirements" element={<RequirementsPage requirements={requirementCatalog} customers={workspaceCustomers} />} />
        <Route path="/contacts" element={<ContactsPage customers={workspaceCustomers} />} />
        <Route path="/pipeline" element={<CrmAreaPage accessToken={session?.accessToken ?? ''} areaKey="pipeline" />} />
        <Route path="/commercial" element={<CrmAreaPage accessToken={session?.accessToken ?? ''} areaKey="commercial" />} />
        <Route path="/support" element={<CrmAreaPage accessToken={session?.accessToken ?? ''} areaKey="support" />} />
        <Route path="/operations" element={<CrmAreaPage accessToken={session?.accessToken ?? ''} areaKey="operations" />} />
        <Route path="/health" element={<CrmAreaPage accessToken={session?.accessToken ?? ''} areaKey="health" />} />
        <Route path="/imports" element={<CrmAreaPage accessToken={session?.accessToken ?? ''} areaKey="imports" />} />
        <Route path="/integrations" element={<CrmAreaPage accessToken={session?.accessToken ?? ''} areaKey="integrations" />} />
        <Route
          path="/settings"
          element={tenantSettingsQuery.isError ? (
            <ApiErrorCallout
              title="Unable to load tenant settings"
              message={getErrorMessage(tenantSettingsQuery.error, 'Failed to load CustomArr tenant settings.')}
            />
          ) : tenantSettings ? (
            <SettingsPage tenantSettings={tenantSettings} isLoading={tenantSettingsQuery.isLoading} isError={tenantSettingsQuery.isError} onSave={(settings) => updateSettingsMutation.mutateAsync(settings)} isSaving={updateSettingsMutation.isPending} />
          ) : (
            <EmptyState title="Loading tenant settings from the API." />
          )}
        />
        <Route
          path="/settings/:sectionKey"
          element={tenantSettingsQuery.isError ? (
            <ApiErrorCallout
              title="Unable to load tenant settings"
              message={getErrorMessage(tenantSettingsQuery.error, 'Failed to load CustomArr tenant settings.')}
            />
          ) : tenantSettings ? (
            <SettingsPage tenantSettings={tenantSettings} isLoading={tenantSettingsQuery.isLoading} isError={tenantSettingsQuery.isError} onSave={(settings) => updateSettingsMutation.mutateAsync(settings)} isSaving={updateSettingsMutation.isPending} />
          ) : (
            <EmptyState title="Loading tenant settings from the API." />
          )}
        />
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
