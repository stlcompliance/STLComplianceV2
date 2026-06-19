import { useQuery } from '@tanstack/react-query'
import { ApiErrorCallout, getErrorMessage } from '@stl/shared-ui'
import { Building2, KeyRound, ShieldCheck, User } from 'lucide-react'

import * as nexarr from '../../api/nexarrClient'

import type { TenantSummary } from '../../api/types'

import { useAuth } from '../../auth/AuthProvider'

import { entitlementStatusClass } from '../../lib/entitlements'

import { isPlatformAdmin } from '../../lib/permissions'



function tenantStatusClass(status: string): string {

  const normalized = status.trim().toLowerCase()

  if (normalized === 'active') {

    return 'text-emerald-300'

  }

  if (normalized === 'suspended') {

    return 'text-amber-300'

  }

  return 'text-slate-400'

}

function TenantMembershipRow({

  tenant,

  isActive,

}: {

  tenant: TenantSummary

  isActive: boolean

}) {

  return (

    <li

      className={[

        'flex flex-col gap-1 px-4 py-3 sm:flex-row sm:items-center sm:justify-between',

        isActive ? 'bg-teal-950/20' : '',

      ].join(' ')}

    >

      <div className="min-w-0 space-y-0.5">

        <p className="text-sm font-medium text-white">

          {tenant.displayName}

          {isActive ? (

            <span className="ml-2 text-xs font-normal text-teal-400">Current workspace</span>

          ) : null}

        </p>

        <p className="font-mono text-xs text-[var(--color-text-muted)]">{tenant.slug}</p>

      </div>

      <div className="flex shrink-0 flex-wrap items-center gap-2 text-xs">

        <span className={`font-medium capitalize ${tenantStatusClass(tenant.status)}`}>

          {tenant.status}

        </span>

        <span className="text-[var(--color-text-muted)]">·</span>

        <span className="text-slate-400">{tenant.roleKey}</span>

      </div>

    </li>

  )

}



export function IdentityProfilePanel() {

  const { me } = useAuth()



  const tenantsQuery = useQuery({

    queryKey: ['my-tenants', me?.userId],

    queryFn: () => nexarr.getMyTenants(),

    enabled: me !== undefined,

  })



  const entitlementsQuery = useQuery({

    queryKey: ['me-entitlements', me?.tenantId],

    queryFn: () => nexarr.getMyEntitlements(),

    enabled: me !== undefined,

  })



  if (!me) {

    return <p className="text-sm text-slate-400">Loading profile…</p>

  }



  const tenants = tenantsQuery.data ?? []

  const entitlements = entitlementsQuery.data ?? []



  return (

    <section aria-labelledby="identity-profile-heading" className="space-y-4">

      <div>

        <h3 id="identity-profile-heading" className="text-xl font-semibold text-white">

          Profile

        </h3>

        <p className="mt-1 text-sm text-slate-400">

          Your NexArr account and tenant memberships for this platform identity.

        </p>

      </div>



      <div className="rounded-lg border border-slate-700 bg-slate-900/60 p-4">

        <div className="flex gap-3">

          <div className="flex h-10 w-10 shrink-0 items-center justify-center rounded-full bg-slate-800">

            <User className="h-5 w-5 text-teal-400" aria-hidden />

          </div>

          <dl className="min-w-0 flex-1 space-y-3 text-sm">

            <div>

              <dt className="text-xs font-medium uppercase tracking-wide text-[var(--color-text-muted)]">

                Display name

              </dt>

              <dd className="mt-0.5 font-medium text-white">{me.displayName}</dd>

            </div>

            <div>

              <dt className="text-xs font-medium uppercase tracking-wide text-[var(--color-text-muted)]">Email</dt>

              <dd className="mt-0.5 text-slate-300">{me.email}</dd>

            </div>

            <div>

              <dt className="text-xs font-medium uppercase tracking-wide text-[var(--color-text-muted)]">

                Active workspace

              </dt>

              <dd className="mt-0.5 text-slate-300">

                {me.tenantDisplayName}{' '}

                <span className="font-mono text-xs text-[var(--color-text-muted)]">({me.tenantSlug})</span>

              </dd>

            </div>

            {isPlatformAdmin(me) ? (

              <div className="flex items-center gap-2 text-xs font-medium text-teal-400">

                <ShieldCheck className="h-4 w-4 shrink-0" aria-hidden />

                Platform administrator

              </div>

            ) : null}

          </dl>

        </div>

      </div>



      <div className="space-y-2" data-testid="profile-entitlements-section">

        <div className="flex items-center gap-2">

          <KeyRound className="h-4 w-4 text-slate-400" aria-hidden />

          <h4 className="text-sm font-semibold text-white">

            Product entitlements

            <span className="ml-2 font-normal text-[var(--color-text-muted)]">({me.tenantSlug})</span>

          </h4>

        </div>

        <p className="text-xs text-[var(--color-text-muted)]">

          Products licensed for your current workspace tenant.

        </p>



        {entitlementsQuery.isLoading ? (

          <p className="text-sm text-slate-400">Loading entitlements…</p>

        ) : null}



        {entitlementsQuery.isError ? (
          <ApiErrorCallout
            message={getErrorMessage(entitlementsQuery.error, 'Failed to load entitlements.')}
            onRetry={() => void entitlementsQuery.refetch()}
            retryLabel="Retry entitlements"
          />
        ) : null}


        {entitlementsQuery.isSuccess && entitlements.length === 0 ? (

          <p className="text-sm text-slate-400">No product entitlements for this workspace.</p>

        ) : null}



        {entitlements.length > 0 ? (

          <ul className="divide-y divide-slate-700 rounded-lg border border-slate-700 bg-slate-900/60">

            {entitlements.map((entitlement) => (

              <li

                key={entitlement.productKey}

                className="flex items-center justify-between gap-2 px-4 py-3 text-sm"

              >

                <span className="font-medium text-white">{entitlement.displayName}</span>

                <span

                  className={`text-xs font-medium capitalize ${entitlementStatusClass(entitlement.status)}`}

                >

                  {entitlement.status}

                </span>

              </li>

            ))}

          </ul>

        ) : null}

      </div>



      <div className="space-y-2">

        <div className="flex items-center gap-2">

          <Building2 className="h-4 w-4 text-slate-400" aria-hidden />

          <h4 className="text-sm font-semibold text-white">Tenant memberships</h4>

        </div>



        {tenantsQuery.isLoading ? (

          <p className="text-sm text-slate-400">Loading tenant memberships…</p>

        ) : null}



        {tenantsQuery.isError ? (
          <ApiErrorCallout
            message={getErrorMessage(tenantsQuery.error, 'Failed to load tenant memberships.')}
            onRetry={() => void tenantsQuery.refetch()}
            retryLabel="Retry tenant memberships"
          />
        ) : null}


        {tenantsQuery.isSuccess && tenants.length === 0 ? (

          <p className="text-sm text-slate-400">No tenant memberships found.</p>

        ) : null}



        {tenants.length > 0 ? (

          <ul className="divide-y divide-slate-700 rounded-lg border border-slate-700 bg-slate-900/60">

            {tenants.map((tenant) => (

              <TenantMembershipRow

                key={tenant.tenantId}

                tenant={tenant}

                isActive={tenant.tenantId === me.tenantId}

              />

            ))}

          </ul>

        ) : null}



        {tenants.length > 1 ? (

          <p className="text-xs text-[var(--color-text-muted)]">

            To switch workspaces, sign out and sign in with the desired tenant.

          </p>

        ) : null}

      </div>

    </section>

  )

}

