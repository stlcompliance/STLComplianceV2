import { useQuery } from '@tanstack/react-query'
import { useState } from 'react'
import { ApiErrorCallout, getErrorMessage } from '@stl/shared-ui'
import * as nexarr from '../../api/nexarrClient'
import { LaunchAttemptsTable } from './launch-diagnostics/LaunchAttemptsTable'
import { LaunchFiltersBar } from './launch-diagnostics/LaunchFiltersBar'
import { LaunchIssuesAndReadiness } from './launch-diagnostics/LaunchIssuesAndReadiness'
import { LaunchValidationCard } from './launch-diagnostics/LaunchValidationCard'

export function LaunchDiagnosticsPage() {
  const [tenantIdFilter, setTenantIdFilter] = useState('')
  const [productKeyFilter, setProductKeyFilter] = useState('')
  const [resultFilter, setResultFilter] = useState('')

  const diagnosticsQuery = useQuery({
    queryKey: ['platform-admin-launch-diagnostics', tenantIdFilter, productKeyFilter],
    queryFn: () =>
      nexarr.getPlatformAdminLaunchDiagnostics({
        tenantId: tenantIdFilter || undefined,
        productKey: productKeyFilter || undefined,
        page: 1,
        pageSize: 100,
      }),
  })
  const attemptsQuery = useQuery({
    queryKey: ['platform-admin-launch-attempts', tenantIdFilter, productKeyFilter, resultFilter],
    queryFn: () =>
      nexarr.getPlatformAdminLaunchAttempts({
        tenantId: tenantIdFilter || undefined,
        productKey: productKeyFilter || undefined,
        result: resultFilter || undefined,
        page: 1,
        pageSize: 25,
      }),
  })

  if (diagnosticsQuery.isLoading || attemptsQuery.isLoading) {
    return <p className="text-sm text-slate-500">Loading launch diagnostics…</p>
  }

  if (diagnosticsQuery.isError) {
    return (
      <ApiErrorCallout
        message={getErrorMessage(diagnosticsQuery.error, 'Failed to load diagnostics.')}
        onRetry={() => void diagnosticsQuery.refetch()}
        retryLabel="Retry diagnostics"
      />
    )
  }

  const diagnostics = diagnosticsQuery.data!

  return (
    <div className="space-y-6">
      <LaunchValidationCard rows={diagnostics.rows} />
      <LaunchFiltersBar
        rows={diagnostics.rows}
        tenantId={tenantIdFilter}
        productKey={productKeyFilter}
        result={resultFilter}
        onTenantIdChange={setTenantIdFilter}
        onProductKeyChange={setProductKeyFilter}
        onResultChange={setResultFilter}
        onReset={() => {
          setTenantIdFilter('')
          setProductKeyFilter('')
          setResultFilter('')
        }}
      />
      <LaunchIssuesAndReadiness diagnostics={diagnostics} />
      <LaunchAttemptsTable
        attemptsResult={attemptsQuery.data}
        isError={attemptsQuery.isError}
        error={attemptsQuery.error as Error | null}
        onRetry={() => void attemptsQuery.refetch()}
        generatedAt={diagnostics.generatedAt}
      />
    </div>
  )
}
