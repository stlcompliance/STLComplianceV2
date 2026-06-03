import { ApiErrorCallout } from '@stl/shared-ui'
import { type FormEvent } from 'react'
import type { PermissionCheckResponse } from '../api/types'

interface PermissionCheckPanelProps {
  personId: string
  personDisplayName: string
  permissionCheckInput: string
  checkResult: PermissionCheckResponse | null
  isChecking: boolean
  errorMessage: string | null
  onPermissionCheckInputChange: (value: string) => void
  onCheckPermissions: () => Promise<void>
}

function parseKeys(value: string): string[] {
  return value
    .split(/[\n,]/)
    .map((entry) => entry.trim())
    .filter(Boolean)
}

function formatScope(scopeType: string, scopeValue: string | null): string {
  if (scopeType === 'tenant') {
    return 'Tenant-wide'
  }
  return scopeValue ?? 'Unscoped'
}

export function PermissionCheckPanel({
  personId,
  personDisplayName,
  permissionCheckInput,
  checkResult,
  isChecking,
  errorMessage,
  onPermissionCheckInputChange,
  onCheckPermissions,
}: PermissionCheckPanelProps) {
  const handleSubmit = async (event: FormEvent) => {
    event.preventDefault()
    await onCheckPermissions()
  }

  return (
    <section className="mt-6 rounded-xl border border-slate-700 bg-slate-900/60 p-6">
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div>
          <h2 className="text-sm font-medium text-slate-300">Permission check</h2>
          <p className="mt-1 text-xs text-slate-500">
            Validate effective permissions for {personDisplayName} against the StaffArr projection service.
          </p>
        </div>
        <span className="rounded-full bg-slate-800 px-3 py-1 text-xs font-mono uppercase tracking-wide text-slate-300">
          {personId.slice(0, 8)}…
        </span>
      </div>

      <form className="mt-4 space-y-3" onSubmit={(event) => void handleSubmit(event)}>
        <label htmlFor="permission-check-input" className="block text-sm text-slate-300">
          Permission keys
          <textarea
            id="permission-check-input"
            value={permissionCheckInput}
            onChange={(event) => onPermissionCheckInputChange(event.target.value)}
            rows={3}
            placeholder="staffarr.people.read, maintainarr.work_orders.close"
            className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 font-mono text-sm text-slate-100 placeholder:text-slate-500"
          />
        </label>
        <div className="flex flex-wrap gap-3">
          <button
            type="submit"
            disabled={isChecking || parseKeys(permissionCheckInput).length === 0}
            className="rounded-md bg-sky-600 px-4 py-2 text-sm font-medium text-white hover:bg-sky-500 disabled:opacity-50"
          >
            {isChecking ? 'Checking…' : 'Run permission check'}
          </button>
          <button
            type="button"
            onClick={() => onPermissionCheckInputChange('')}
            disabled={isChecking && parseKeys(permissionCheckInput).length === 0}
            className="rounded-md border border-slate-600 px-4 py-2 text-sm text-slate-200 hover:bg-slate-800 disabled:opacity-50"
          >
            Clear
          </button>
        </div>
      </form>

      {errorMessage ? (
        <div className="mt-4">
          <ApiErrorCallout title="Permission check failed" message={errorMessage} />
        </div>
      ) : null}

      {checkResult ? (
        <div className="mt-5 space-y-4" data-testid="permission-check-result">
          <div className="flex flex-wrap gap-2 text-xs uppercase tracking-wide">
            <span className={`rounded-full px-2 py-1 ${checkResult.isPersonActive ? 'bg-emerald-500/20 text-emerald-200' : 'bg-rose-500/20 text-rose-200'}`}>
              {checkResult.isPersonActive ? 'Active person' : 'Inactive person'}
            </span>
            <span className={`rounded-full px-2 py-1 ${checkResult.isAuthorizedAll ? 'bg-emerald-500/20 text-emerald-200' : 'bg-amber-500/20 text-amber-100'}`}>
              {checkResult.isAuthorizedAll ? 'All granted' : 'Not all granted'}
            </span>
            <span className={`rounded-full px-2 py-1 ${checkResult.isAuthorizedAny ? 'bg-sky-500/20 text-sky-100' : 'bg-slate-700 text-slate-200'}`}>
              {checkResult.isAuthorizedAny ? 'Some granted' : 'None granted'}
            </span>
          </div>
          <p className="text-xs text-slate-500">Computed {new Date(checkResult.computedAt).toLocaleString()}</p>
          <ul className="divide-y divide-slate-700 rounded-lg border border-slate-800">
            {checkResult.checks.map((check) => (
              <li key={check.permissionKey} className="p-4 text-sm">
                <div className="flex flex-wrap items-start justify-between gap-3">
                  <div>
                    <p className="font-mono text-slate-100">{check.permissionKey}</p>
                    <p className="mt-1 text-xs text-slate-500">{check.granted ? 'Granted' : 'Denied'}</p>
                  </div>
                  <span className={`rounded-full px-2 py-1 text-xs uppercase tracking-wide ${check.granted ? 'bg-emerald-500/20 text-emerald-200' : 'bg-rose-500/20 text-rose-200'}`}>
                    {check.granted ? 'Allowed' : 'Blocked'}
                  </span>
                </div>
                {check.grants.length > 0 ? (
                  <ul className="mt-3 space-y-2 text-xs text-slate-400">
                    {check.grants.map((grant) => (
                      <li key={`${grant.permissionKey}-${grant.roleKey}-${grant.scopeType}-${grant.scopeValue ?? ''}`}>
                        {grant.permissionName} via {grant.roleName} · {formatScope(grant.scopeType, grant.scopeValue)}
                      </li>
                    ))}
                  </ul>
                ) : (
                  <p className="mt-3 text-xs text-slate-500">No matching grants were found.</p>
                )}
              </li>
            ))}
          </ul>
        </div>
      ) : null}
    </section>
  )
}
