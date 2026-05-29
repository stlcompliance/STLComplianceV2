import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { type FormEvent, useState } from 'react'

import * as nexarr from '../../api/nexarrClient'

export function TenantCatalogAdminPanel() {
  const queryClient = useQueryClient()
  const [selectedTenantId, setSelectedTenantId] = useState('')
  const [slug, setSlug] = useState('')
  const [displayName, setDisplayName] = useState('')
  const [editDisplayName, setEditDisplayName] = useState('')
  const [status, setStatus] = useState('active')
  const [errorMessage, setErrorMessage] = useState<string | null>(null)

  const tenantsQuery = useQuery({
    queryKey: ['platform-tenants-admin'],
    queryFn: () => nexarr.listTenants(1, 100),
  })

  const tenants = tenantsQuery.data?.items ?? []
  const selectedTenant = tenants.find((tenant) => tenant.tenantId === selectedTenantId) ?? null

  const createMutation = useMutation({
    mutationFn: () => nexarr.createTenant({ slug: slug.trim(), displayName: displayName.trim() }),
    onSuccess: () => {
      setErrorMessage(null)
      setSlug('')
      setDisplayName('')
      void queryClient.invalidateQueries({ queryKey: ['platform-tenants-admin'] })
      void queryClient.invalidateQueries({ queryKey: ['platform-admin-tenant-overview'] })
    },
    onError: (error: Error) => setErrorMessage(error.message),
  })

  const updateMutation = useMutation({
    mutationFn: () => nexarr.updateTenant(selectedTenantId, { displayName: editDisplayName.trim() }),
    onSuccess: () => {
      setErrorMessage(null)
      void queryClient.invalidateQueries({ queryKey: ['platform-tenants-admin'] })
      void queryClient.invalidateQueries({ queryKey: ['platform-admin-tenant-overview'] })
    },
    onError: (error: Error) => setErrorMessage(error.message),
  })

  const statusMutation = useMutation({
    mutationFn: () => nexarr.updateTenantStatus(selectedTenantId, { status }),
    onSuccess: () => {
      setErrorMessage(null)
      void queryClient.invalidateQueries({ queryKey: ['platform-tenants-admin'] })
      void queryClient.invalidateQueries({ queryKey: ['platform-admin-tenant-overview'] })
    },
    onError: (error: Error) => setErrorMessage(error.message),
  })

  const handleCreate = (event: FormEvent) => {
    event.preventDefault()
    createMutation.mutate()
  }

  return (
    <section
      data-testid="tenant-catalog-admin-panel"
      className="mt-6 space-y-6 rounded-xl border border-slate-200 bg-white p-5"
    >
      <header>
        <h2 className="text-lg font-semibold text-stl-navy">Tenant administration</h2>
        <p className="mt-1 text-sm text-slate-600">
          Create and update tenants via NexArr <code className="text-xs">/api/tenants</code>.
        </p>
      </header>

      {errorMessage ? (
        <p className="text-sm text-red-700" role="alert">
          {errorMessage}
        </p>
      ) : null}

      <form className="grid gap-3 md:grid-cols-3" onSubmit={handleCreate}>
        <label htmlFor="tenant-catalog-create-slug" className="block text-sm text-slate-700">
          New tenant slug
          <input
            id="tenant-catalog-create-slug"
            value={slug}
            onChange={(event) => setSlug(event.target.value)}
            className="mt-1 w-full rounded-md border border-slate-300 px-3 py-2 text-sm"
            required
          />
        </label>
        <label htmlFor="tenant-catalog-create-display-name" className="block text-sm text-slate-700 md:col-span-2">
          New tenant display name
          <input
            id="tenant-catalog-create-display-name"
            value={displayName}
            onChange={(event) => setDisplayName(event.target.value)}
            className="mt-1 w-full rounded-md border border-slate-300 px-3 py-2 text-sm"
            required
          />
        </label>
        <div className="md:col-span-3">
          <button
            type="submit"
            disabled={createMutation.isPending}
            className="rounded-md bg-stl-navy px-4 py-2 text-sm font-medium text-white hover:bg-stl-navy/90 disabled:opacity-50"
          >
            {createMutation.isPending ? 'Creating…' : 'Create tenant'}
          </button>
        </div>
      </form>

      <div className="grid gap-3 md:grid-cols-2">
        <label htmlFor="tenant-catalog-selected-tenant" className="block text-sm text-slate-700">
          Tenant to edit
          <select
            id="tenant-catalog-selected-tenant"
            value={selectedTenantId}
            onChange={(event) => {
              const tenantId = event.target.value
              setSelectedTenantId(tenantId)
              const tenant = tenants.find((item) => item.tenantId === tenantId)
              setEditDisplayName(tenant?.displayName ?? '')
              setStatus(tenant?.status ?? 'active')
            }}
            className="mt-1 w-full rounded-md border border-slate-300 px-3 py-2 text-sm"
          >
            <option value="">Select tenant…</option>
            {tenants.map((tenant) => (
              <option key={tenant.tenantId} value={tenant.tenantId}>
                {tenant.displayName} ({tenant.slug})
              </option>
            ))}
          </select>
        </label>
        {selectedTenant ? (
          <>
            <label htmlFor="tenant-catalog-edit-display-name" className="block text-sm text-slate-700">
              Updated tenant display name
              <input
                id="tenant-catalog-edit-display-name"
                value={editDisplayName}
                onChange={(event) => setEditDisplayName(event.target.value)}
                className="mt-1 w-full rounded-md border border-slate-300 px-3 py-2 text-sm"
              />
            </label>
            <label htmlFor="tenant-catalog-edit-status" className="block text-sm text-slate-700">
              Tenant lifecycle status
              <select
                id="tenant-catalog-edit-status"
                value={status}
                onChange={(event) => setStatus(event.target.value)}
                className="mt-1 w-full rounded-md border border-slate-300 px-3 py-2 text-sm"
              >
                <option value="active">active</option>
                <option value="inactive">inactive</option>
                <option value="suspended">suspended</option>
              </select>
            </label>
            <div className="flex flex-wrap gap-2 md:col-span-2">
              <button
                type="button"
                disabled={updateMutation.isPending}
                onClick={() => updateMutation.mutate()}
                className="rounded-md border border-slate-300 px-4 py-2 text-sm hover:bg-slate-50 disabled:opacity-50"
              >
                Save display name
              </button>
              <button
                type="button"
                disabled={statusMutation.isPending}
                onClick={() => statusMutation.mutate()}
                className="rounded-md border border-slate-300 px-4 py-2 text-sm hover:bg-slate-50 disabled:opacity-50"
              >
                Update status
              </button>
            </div>
          </>
        ) : null}
      </div>
    </section>
  )
}
