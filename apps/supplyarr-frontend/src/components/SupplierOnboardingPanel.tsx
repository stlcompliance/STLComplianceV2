import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useMemo, useState } from 'react'

import {
  approvePartyComplianceDocument,
  approveSupplierOnboarding,
  getSupplierOnboardingByParty,
  getSupplierOnboardingDocumentRequirements,
  listPendingSupplierOnboarding,
  registerPartyComplianceDocument,
  rejectSupplierOnboarding,
  startSupplierOnboarding,
  submitSupplierOnboarding,
} from '../api/client'
import type { ExternalPartyResponse } from '../api/types'
import { GeneratedKeyFieldGroup } from '../forms/GeneratedKeyFieldGroup'

interface SupplierOnboardingPanelProps {
  accessToken: string
  canManage: boolean
  canReview: boolean
  onboardableParties: ExternalPartyResponse[]
}

function statusBadgeClass(status: string): string {
  switch (status) {
    case 'approved':
      return 'bg-emerald-500/20 text-emerald-300 ring-emerald-500/40'
    case 'pending_review':
      return 'bg-amber-500/20 text-amber-200 ring-amber-500/40'
    case 'rejected':
      return 'bg-rose-500/20 text-rose-200 ring-rose-500/40'
    case 'suspended':
      return 'bg-slate-500/20 text-slate-300 ring-slate-500/40'
    default:
      return 'bg-sky-500/20 text-sky-200 ring-sky-500/40'
  }
}

export function SupplierOnboardingPanel({
  accessToken,
  canManage,
  canReview,
  onboardableParties,
}: SupplierOnboardingPanelProps) {
  if (!canManage && !canReview) {
    return null
  }

  const queryClient = useQueryClient()
  const [selectedPartyId, setSelectedPartyId] = useState('')
  const [onboardingNotes, setOnboardingNotes] = useState('')
  const [rejectReason, setRejectReason] = useState('')
  const [docTypeKey, setDocTypeKey] = useState('w9')
  const [docKey, setDocKey] = useState('')
  const [docTitle, setDocTitle] = useState('')

  const requirementsQuery = useQuery({
    queryKey: ['supplyarr-onboarding-requirements', accessToken],
    queryFn: () => getSupplierOnboardingDocumentRequirements(accessToken),
  })

  const pendingQuery = useQuery({
    queryKey: ['supplyarr-onboarding-pending', accessToken],
    queryFn: () => listPendingSupplierOnboarding(accessToken),
    enabled: canReview,
  })

  const onboardingQuery = useQuery({
    queryKey: ['supplyarr-onboarding', accessToken, selectedPartyId],
    queryFn: () => getSupplierOnboardingByParty(accessToken, selectedPartyId),
    enabled: Boolean(selectedPartyId),
    retry: false,
  })

  const selectedParty = useMemo(
    () => onboardableParties.find((p) => p.partyId === selectedPartyId),
    [onboardableParties, selectedPartyId],
  )

  const invalidate = () => {
    void queryClient.invalidateQueries({ queryKey: ['supplyarr-onboarding-pending', accessToken] })
    if (selectedPartyId) {
      void queryClient.invalidateQueries({
        queryKey: ['supplyarr-onboarding', accessToken, selectedPartyId],
      })
    }
  }

  const startMutation = useMutation({
    mutationFn: () => startSupplierOnboarding(accessToken, selectedPartyId, onboardingNotes),
    onSuccess: invalidate,
  })

  const submitMutation = useMutation({
    mutationFn: () => submitSupplierOnboarding(accessToken, selectedPartyId, onboardingNotes),
    onSuccess: invalidate,
  })

  const approveMutation = useMutation({
    mutationFn: () => approveSupplierOnboarding(accessToken, selectedPartyId),
    onSuccess: invalidate,
  })

  const rejectMutation = useMutation({
    mutationFn: () => rejectSupplierOnboarding(accessToken, selectedPartyId, rejectReason),
    onSuccess: invalidate,
  })

  const registerDocMutation = useMutation({
    mutationFn: () => {
      const generatedDocumentKey = docKey.trim()
      if (!generatedDocumentKey) {
        throw new Error('Generated document key is required.')
      }
      return registerPartyComplianceDocument(accessToken, selectedPartyId, {
        documentKey: generatedDocumentKey,
        documentTypeKey: docTypeKey,
        title: docTitle || docTypeKey,
        fileName: `${docTypeKey}.pdf`,
        contentType: 'application/pdf',
        sizeBytes: 1024,
        notes: '',
      })
    },
    onSuccess: async (created) => {
      if (canReview) {
        await approvePartyComplianceDocument(accessToken, selectedPartyId, created.documentId)
      }
      invalidate()
    },
  })

  const onboarding = onboardingQuery.data
  const canSubmit =
    onboarding &&
    (onboarding.onboardingStatus === 'draft' || onboarding.onboardingStatus === 'rejected')
  const canApproveReview = onboarding?.onboardingStatus === 'pending_review' && canReview

  return (
    <section
      data-testid="supplier-onboarding-panel"
      className="rounded-xl border border-slate-700 bg-slate-900/60 p-5 lg:col-span-2"
    >
      <h2 className="text-lg font-medium text-white">Supplier onboarding</h2>
      <p className="mt-1 text-sm text-slate-400">
        Register compliance documents, submit for review, and approve vendor or supplier parties.
      </p>

      {canReview && (pendingQuery.data?.length ?? 0) > 0 ? (
        <div className="mt-4 rounded-lg border border-amber-800/50 bg-amber-950/30 p-3">
          <p className="text-sm font-medium text-amber-200">
            {pendingQuery.data!.length} pending review
          </p>
          <ul className="mt-2 space-y-1 text-sm text-slate-300">
            {pendingQuery.data!.map((item) => (
              <li key={item.onboardingId}>
                <button
                  type="button"
                  className="text-left underline decoration-dotted hover:text-white"
                  onClick={() => setSelectedPartyId(item.externalPartyId)}
                >
                  {item.displayName} ({item.partyKey})
                </button>
              </li>
            ))}
          </ul>
        </div>
      ) : null}

      <div className="mt-4 grid gap-3 sm:grid-cols-2">
        <label htmlFor="supplier-onboarding-party" className="text-sm text-slate-400">
          Onboarding party
          <select
            id="supplier-onboarding-party"
            className="mt-1 w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-white"
            value={selectedPartyId}
            onChange={(e) => setSelectedPartyId(e.target.value)}
          >
            <option value="">Select vendor or supplier…</option>
            {onboardableParties.map((p) => (
              <option key={p.partyId} value={p.partyId}>
                {p.displayName} ({p.partyType})
              </option>
            ))}
          </select>
        </label>
        <label htmlFor="supplier-onboarding-notes" className="text-sm text-slate-400">
          Onboarding notes
          <input
            id="supplier-onboarding-notes"
            className="mt-1 w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-sm"
            value={onboardingNotes}
            onChange={(e) => setOnboardingNotes(e.target.value)}
          />
        </label>
      </div>

      {canManage && selectedPartyId ? (
        <div className="mt-3 flex flex-wrap gap-2">
          <button
            type="button"
            className="rounded-lg bg-sky-600 px-3 py-1.5 text-sm text-white disabled:opacity-50"
            disabled={startMutation.isPending}
            onClick={() => startMutation.mutate()}
          >
            {onboardingQuery.isError ? 'Start onboarding' : 'Restart / start draft'}
          </button>
          {canSubmit ? (
            <button
              type="button"
              className="rounded-lg bg-amber-600 px-3 py-1.5 text-sm text-white disabled:opacity-50"
              disabled={submitMutation.isPending}
              onClick={() => submitMutation.mutate()}
            >
              Submit for review
            </button>
          ) : null}
        </div>
      ) : null}

      {requirementsQuery.data ? (
        <div className="mt-4">
          <h3 className="text-sm font-medium text-slate-300">Required documents</h3>
          <ul className="mt-2 space-y-1 text-sm">
            {requirementsQuery.data.requirements.map((req) => {
              const status = onboarding?.documentRequirements.find(
                (d) => d.documentTypeKey === req.documentTypeKey,
              )
              return (
                <li key={req.documentTypeKey} className="flex justify-between gap-2">
                  <span>{req.label}</span>
                  <span className={status?.isSatisfied ? 'text-emerald-400' : 'text-slate-500'}>
                    {status?.isSatisfied ? 'approved' : 'missing'}
                  </span>
                </li>
              )
            })}
          </ul>
        </div>
      ) : null}

      {canManage && selectedPartyId && onboarding ? (
        <div className="mt-4 rounded-lg border border-slate-800 p-3">
          <h3 className="text-sm font-medium text-slate-300">Upload document (metadata)</h3>
          <div className="mt-2 grid gap-2 sm:grid-cols-3">
            <label htmlFor="supplier-onboarding-doc-type" className="block text-sm text-slate-400">
              Document type
              <select
                id="supplier-onboarding-doc-type"
                className="mt-1 w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
                value={docTypeKey}
                onChange={(e) => setDocTypeKey(e.target.value)}
              >
                {requirementsQuery.data?.requirements.map((r) => (
                  <option key={r.documentTypeKey} value={r.documentTypeKey}>
                    {r.label}
                  </option>
                ))}
              </select>
            </label>
            <label htmlFor="supplier-onboarding-doc-title" className="block text-sm text-slate-400">
              Document title
              <input
                id="supplier-onboarding-doc-title"
                className="mt-1 w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
                placeholder="Title"
                value={docTitle}
                onChange={(e) => setDocTitle(e.target.value)}
              />
            </label>
            <GeneratedKeyFieldGroup
              sourceLabel={docTitle || docTypeKey}
              existingKeys={[]}
              onKeyChange={setDocKey}
              domain="vendor"
              kind="document"
              label="Document key"
            />
          </div>
          <button
            type="button"
            className="mt-2 rounded-lg border border-slate-600 px-3 py-1.5 text-sm hover:bg-slate-800 disabled:opacity-50"
            disabled={registerDocMutation.isPending || !docKey.trim()}
            onClick={() => registerDocMutation.mutate()}
          >
            Register {canReview ? '& approve' : ''} document
          </button>
        </div>
      ) : null}

      {onboarding ? (
        <div className="mt-4 rounded-lg border border-slate-800 p-3">
          <div className="flex items-center justify-between gap-2">
            <div>
              <div className="font-medium">{selectedParty?.displayName ?? onboarding.displayName}</div>
              <div className="text-sm text-slate-400">{onboarding.partyKey}</div>
            </div>
            <span
              className={`rounded-full px-2 py-0.5 text-xs ring-1 ${statusBadgeClass(onboarding.onboardingStatus)}`}
            >
              {onboarding.onboardingStatus}
            </span>
          </div>
          {onboarding.rejectionReason ? (
            <p className="mt-2 text-sm text-rose-300">Rejected: {onboarding.rejectionReason}</p>
          ) : null}
        </div>
      ) : selectedPartyId && onboardingQuery.isError ? (
        <p className="mt-3 text-sm text-slate-400">No onboarding record yet — start onboarding above.</p>
      ) : null}

      {canApproveReview ? (
        <div className="mt-4 flex flex-wrap items-end gap-2">
          <button
            type="button"
            className="rounded-lg bg-emerald-600 px-3 py-1.5 text-sm text-white disabled:opacity-50"
            disabled={approveMutation.isPending}
            onClick={() => approveMutation.mutate()}
          >
            Approve onboarding
          </button>
          <label htmlFor="supplier-onboarding-reject-reason" className="block min-w-[12rem] flex-1 text-sm text-slate-400">
            Rejection reason
            <input
              id="supplier-onboarding-reject-reason"
              className="mt-1 w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-sm"
              placeholder="Rejection reason"
              value={rejectReason}
              onChange={(e) => setRejectReason(e.target.value)}
            />
          </label>
          <button
            type="button"
            className="rounded-lg bg-rose-700 px-3 py-1.5 text-sm text-white disabled:opacity-50"
            disabled={rejectMutation.isPending || !rejectReason.trim()}
            onClick={() => rejectMutation.mutate()}
          >
            Reject
          </button>
        </div>
      ) : null}
    </section>
  )
}
