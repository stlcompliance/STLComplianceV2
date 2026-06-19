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
  const [userIdFilter, setUserIdFilter] = useState('')
  const [correlationIdFilter, setCorrelationIdFilter] = useState('')
  const [fromUtcFilter, setFromUtcFilter] = useState('')
  const [toUtcFilter, setToUtcFilter] = useState('')

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
    queryKey: [
      'platform-admin-launch-attempts',
      tenantIdFilter,
      productKeyFilter,
      resultFilter,
      userIdFilter,
      correlationIdFilter,
      fromUtcFilter,
      toUtcFilter,
    ],
    queryFn: () =>
      nexarr.getPlatformAdminLaunchAttempts({
        tenantId: tenantIdFilter || undefined,
        productKey: productKeyFilter || undefined,
        result: resultFilter || undefined,
        userId: userIdFilter || undefined,
        correlationId: correlationIdFilter || undefined,
        fromUtc: fromUtcFilter ? new Date(fromUtcFilter).toISOString() : undefined,
        toUtc: toUtcFilter ? new Date(toUtcFilter).toISOString() : undefined,
        page: 1,
        pageSize: 25,
      }),
  })

  if (diagnosticsQuery.isLoading || (!diagnosticsQuery.data && attemptsQuery.isLoading)) {
    return <p className="text-sm text-[var(--color-text-muted)]">Loading launch diagnostics…</p>
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
        userId={userIdFilter}
        correlationId={correlationIdFilter}
        fromUtc={fromUtcFilter}
        toUtc={toUtcFilter}
        onTenantIdChange={setTenantIdFilter}
        onProductKeyChange={setProductKeyFilter}
        onResultChange={setResultFilter}
        onUserIdChange={setUserIdFilter}
        onCorrelationIdChange={setCorrelationIdFilter}
        onFromUtcChange={setFromUtcFilter}
        onToUtcChange={setToUtcFilter}
        onReset={() => {
          setTenantIdFilter('')
          setProductKeyFilter('')
          setResultFilter('')
          setUserIdFilter('')
          setCorrelationIdFilter('')
          setFromUtcFilter('')
          setToUtcFilter('')
        }}
      />
      <LaunchIssuesAndReadiness diagnostics={diagnostics} />
      <LaunchAttemptsTable
        isLoading={attemptsQuery.isLoading && !attemptsQuery.data}
        attemptsResult={attemptsQuery.data}
        isError={attemptsQuery.isError}
        error={attemptsQuery.error as Error | null}
        onRetry={() => void attemptsQuery.refetch()}
        generatedAt={diagnostics.generatedAt}
      />
    </div>
  )
}
