import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { buildSemanticKey, GeneratedKeyField } from '@stl/shared-ui'
import { useMemo, useState } from 'react'

import {
  approveComplianceWaiver,
  createComplianceWaiver,
  listComplianceWaivers,
  revokeComplianceWaiver,
} from '../api/client'
import type { RulePackResponse } from '../api/types'

interface ComplianceWaiversPanelProps {
  accessToken: string
  rulePacks: RulePackResponse[]
  canManage: boolean
}

export function ComplianceWaiversPanel({
  accessToken,
  rulePacks,
  canManage,
}: ComplianceWaiversPanelProps) {
  const queryClient = useQueryClient()
  const [rulePackId, setRulePackId] = useState('')
  const [scopeKey, setScopeKey] = useState('tenant')
  const [reasonCode, setReasonCode] = useState('temporary_ops_override')
  const [explanation, setExplanation] = useState('')
  const [showPolicyHint, setShowPolicyHint] = useState(false)

  const waiversQuery = useQuery({
    queryKey: ['compliancecore-waivers', accessToken],
    queryFn: () => listComplianceWaivers(accessToken, { limit: 50 }),
  })

  const createMutation = useMutation({
    mutationFn: () =>
      createComplianceWaiver(accessToken, {
        waiverKey: generatedWaiverKey,
        rulePackId,
        subjectScopeKey: scopeKey.trim() || 'tenant',
        reasonCode: reasonCode.trim(),
        explanation: explanation.trim(),
        effectiveAt: new Date().toISOString(),
      }),
    onSuccess: () => {
      setExplanation('')
      void queryClient.invalidateQueries({ queryKey: ['compliancecore-waivers', accessToken] })
    },
  })

  const approveMutation = useMutation({
    mutationFn: (waiverId: string) => approveComplianceWaiver(accessToken, waiverId),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ['compliancecore-waivers', accessToken] })
    },
  })

  const revokeMutation = useMutation({
    mutationFn: (waiverId: string) => revokeComplianceWaiver(accessToken, waiverId),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ['compliancecore-waivers', accessToken] })
    },
  })

  const publishedPacks = rulePacks.filter((pack) => pack.status === 'published')
  const selectedPack = publishedPacks.find((pack) => pack.rulePackId === rulePackId) ?? null
  const existingWaiverKeys = waiversQuery.data?.map((waiver) => waiver.waiverKey) ?? []
  const waiverKeySource = `${selectedPack?.packKey ?? ''} ${reasonCode} waiver`
  const generatedWaiverKey = buildSemanticKey({
    domain: 'rule',
    kind: 'waiver',
    title: waiverKeySource,
    existingKeys: existingWaiverKeys,
    maxLength: 128,
  })
  const scopeOptions = useMemo(() => {
    const known = new Set<string>(['tenant'])
    for (const waiver of waiversQuery.data ?? []) {
      if (waiver.subjectScopeKey?.trim()) {
        known.add(waiver.subjectScopeKey)
      }
    }
    return [...known].sort((a, b) => {
      if (a === 'tenant') return -1
      if (b === 'tenant') return 1
      return a.localeCompare(b)
    })
  }, [waiversQuery.data])
  const reasonCodeOptions = useMemo(() => {
    const known = new Set<string>([
      'temporary_ops_override',
      'incident_containment',
      'corrective_action_in_progress',
      'customer_exception',
      'legal_stay',
      'other',
    ])
    for (const waiver of waiversQuery.data ?? []) {
      if (waiver.reasonCode?.trim()) {
        known.add(waiver.reasonCode)
      }
    }
    return [...known].sort()
  }, [waiversQuery.data])

  return (
    <section
      data-testid="compliance-waivers-panel"
      className="space-y-4 rounded-xl border border-slate-700 bg-slate-900/80 p-5"
    >
      <header>
        <h2 className="text-lg font-semibold text-slate-50">Compliance waivers</h2>
        <p className="mt-1 text-sm text-slate-400">
          Time-bound compliance waivers against rule packs. Approved waivers surface as{' '}
          <span className="font-mono text-amber-300">waived</span> outcomes in workflow gate checks
          instead of block/warn when scope matches. Rules marked{' '}
          <span className="font-mono text-amber-300">nonWaivable</span> in rule pack content cannot be waived.
        </p>
      </header>

      {canManage && (
        <form
          className="grid gap-3 rounded-lg border border-slate-800 bg-slate-950/50 p-4 md:grid-cols-2"
          onSubmit={(event) => {
            event.preventDefault()
            createMutation.mutate()
          }}
        >
          <div className="space-y-1">
            <GeneratedKeyField
              sourceLabel={waiverKeySource}
              generatedKey={generatedWaiverKey}
              manualOverride=""
              onManualOverrideChange={() => {}}
              showAdvancedKey={showPolicyHint}
              disabled={createMutation.isPending}
              label="Waiver key"
            />
            {!showPolicyHint ? (
              <button
                type="button"
                className="text-xs text-slate-500 underline-offset-2 hover:text-slate-300 hover:underline"
                onClick={() => setShowPolicyHint(true)}
                disabled={createMutation.isPending}
              >
                Key policy
              </button>
            ) : null}
          </div>
          <label className="block text-sm text-slate-300">
            Rule pack
            <select
              value={rulePackId}
              onChange={(event) => setRulePackId(event.target.value)}
              className="mt-1 w-full rounded-md border border-slate-700 bg-slate-900 px-3 py-2 text-sm"
              required
            >
              <option value="">Select published pack…</option>
              {publishedPacks.map((pack) => (
                <option key={pack.rulePackId} value={pack.rulePackId}>
                  {pack.label} ({pack.packKey})
                </option>
              ))}
            </select>
          </label>
          <label className="block text-sm text-slate-300">
            Subject scope key
            <select
              value={scopeKey}
              onChange={(event) => setScopeKey(event.target.value)}
              className="mt-1 w-full rounded-md border border-slate-700 bg-slate-900 px-3 py-2 text-sm"
            >
              {scopeOptions.map((option) => (
                <option key={option} value={option}>
                  {option}
                </option>
              ))}
            </select>
          </label>
          <label className="block text-sm text-slate-300">
            Reason code
            <select
              value={reasonCode}
              onChange={(event) => setReasonCode(event.target.value)}
              className="mt-1 w-full rounded-md border border-slate-700 bg-slate-900 px-3 py-2 text-sm"
            >
              {reasonCodeOptions.map((option) => (
                <option key={option} value={option}>
                  {option}
                </option>
              ))}
            </select>
          </label>
          <label className="md:col-span-2 block text-sm text-slate-300">
            Explanation
            <textarea
              value={explanation}
              onChange={(event) => setExplanation(event.target.value)}
              className="mt-1 w-full rounded-md border border-slate-700 bg-slate-900 px-3 py-2 text-sm"
              rows={3}
              placeholder="Document why this rule requirement is waived and for how long."
              required
            />
          </label>
          <div className="md:col-span-2">
            <button
              type="submit"
              disabled={
                createMutation.isPending || !generatedWaiverKey || !rulePackId || !explanation.trim()
              }
              className="rounded-md bg-violet-600 px-3 py-2 text-sm font-medium text-white hover:bg-violet-500 disabled:opacity-50"
            >
              {createMutation.isPending ? 'Submitting…' : 'Request waiver'}
            </button>
          </div>
        </form>
      )}

      <ul className="space-y-2" data-testid="compliance-waiver-list">
        {(waiversQuery.data ?? []).length === 0 ? (
          <li className="text-sm text-slate-400">No compliance waivers recorded yet.</li>
        ) : (
          waiversQuery.data?.map((waiver) => (
            <li key={waiver.waiverId} className="rounded-lg border border-slate-800 bg-slate-950/50 p-3">
              <div className="flex flex-wrap items-start justify-between gap-2">
                <div>
                  <p className="font-medium text-slate-100">{waiver.waiverKey}</p>
                  <p className="font-mono text-xs text-violet-300">
                    {waiver.packKey} · {waiver.subjectScopeKey}
                  </p>
                  <p className="mt-1 text-sm text-slate-400">{waiver.explanation}</p>
                </div>
                <span className="rounded bg-slate-800 px-2 py-0.5 text-xs uppercase text-slate-300">
                  {waiver.status}
                </span>
              </div>
              {canManage && waiver.status === 'pending' && (
                <button
                  type="button"
                  onClick={() => approveMutation.mutate(waiver.waiverId)}
                  disabled={approveMutation.isPending}
                  className="mt-2 rounded-md bg-emerald-700 px-2 py-1 text-xs font-medium text-white hover:bg-emerald-600"
                >
                  Approve
                </button>
              )}
              {canManage && waiver.status === 'approved' && (
                <button
                  type="button"
                  onClick={() => revokeMutation.mutate(waiver.waiverId)}
                  disabled={revokeMutation.isPending}
                  className="mt-2 rounded-md bg-rose-800 px-2 py-1 text-xs font-medium text-white hover:bg-rose-700"
                >
                  Revoke
                </button>
              )}
            </li>
          ))
        )}
      </ul>
    </section>
  )
}
