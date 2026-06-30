import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { ApiErrorCallout, StaticSearchPicker, getErrorMessage, type PickerOption } from '@stl/shared-ui'
import { useMemo, useState } from 'react'

import {
  approveSupplierComplianceDocument,
  approveSupplierOnboarding,
  getSupplierOnboarding,
  getSupplierOnboardingDocumentRequirements,
  listPendingSupplierOnboarding,
  listSupplierComplianceDocuments,
  registerSupplierComplianceDocument,
  rejectSupplierOnboarding,
  startSupplierOnboarding,
  submitSupplierOnboarding,
} from '../api/client'
import type { SupplierResponse, SupplierComplianceDocumentResponse } from '../api/types'
import { GeneratedKeyFieldGroup } from '../forms/GeneratedKeyFieldGroup'
import {
  formatSupplierIdentitySummary,
  formatSupplierOperationalContext,
  humanizeSupplierUnitKind,
} from '../utils/supplierPresentation'

interface SupplierOnboardingPanelProps {
  accessToken: string
  canManage: boolean
  canReview: boolean
  onboardableSuppliers: SupplierResponse[]
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

function isMissingOnboardingRecordError(error: unknown): boolean {
  return (
    typeof error === 'object'
    && error !== null
    && 'status' in error
    && (error as { status?: number }).status === 404
  )
}

async function fileToBase64(file: File): Promise<string> {
  const buffer = await file.arrayBuffer()
  let binary = ''
  const bytes = new Uint8Array(buffer)
  for (const byte of bytes) {
    binary += String.fromCharCode(byte)
  }
  return window.btoa(binary)
}

export function SupplierOnboardingPanel({
  accessToken,
  canManage,
  canReview,
  onboardableSuppliers,
}: SupplierOnboardingPanelProps) {
  if (!canManage && !canReview) {
    return null
  }

  const queryClient = useQueryClient()
  const [selectedSupplierId, setSelectedSupplierId] = useState('')
  const [onboardingNotes, setOnboardingNotes] = useState('')
  const [rejectReason, setRejectReason] = useState('')
  const [docTypeKey, setDocTypeKey] = useState('w9')
  const [docKey, setDocKey] = useState('')
  const [docTitle, setDocTitle] = useState('')
  const [docFile, setDocFile] = useState<File | null>(null)

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
    queryKey: ['supplyarr-supplier-onboarding', accessToken, selectedSupplierId],
    queryFn: () => getSupplierOnboarding(accessToken, selectedSupplierId),
    enabled: Boolean(selectedSupplierId),
    retry: false,
  })

  const documentsQuery = useQuery({
    queryKey: ['supplyarr-supplier-compliance-documents', accessToken, selectedSupplierId],
    queryFn: () => listSupplierComplianceDocuments(accessToken, selectedSupplierId),
    enabled: Boolean(selectedSupplierId),
  })

  const selectedSupplier = useMemo(
    () => onboardableSuppliers.find((supplier) => supplier.supplierId === selectedSupplierId),
    [onboardableSuppliers, selectedSupplierId],
  )
  const supplierOptions = useMemo<PickerOption[]>(
    () =>
      onboardableSuppliers.map((supplier) => ({
        value: supplier.supplierId,
        label: `${formatSupplierIdentitySummary({
          supplierDisplayName: supplier.displayName,
          supplierKey: supplier.supplierKey,
          parentSupplierDisplayName: supplier.parentSupplierDisplayName,
          supplierUnitKind: supplier.unitKind,
        })} · ${humanizeSupplierUnitKind(supplier.unitKind)}`,
      })),
    [onboardableSuppliers],
  )
  const selectedSupplierOption = useMemo<PickerOption | undefined>(
    () => supplierOptions.find((option) => option.value === selectedSupplierId),
    [supplierOptions, selectedSupplierId],
  )

  const invalidate = () => {
    void queryClient.invalidateQueries({ queryKey: ['supplyarr-onboarding-pending', accessToken] })
    if (selectedSupplierId) {
      void queryClient.invalidateQueries({
        queryKey: ['supplyarr-supplier-onboarding', accessToken, selectedSupplierId],
      })
      void queryClient.invalidateQueries({
        queryKey: ['supplyarr-supplier-compliance-documents', accessToken, selectedSupplierId],
      })
    }
  }

  const startMutation = useMutation({
    mutationFn: () => startSupplierOnboarding(accessToken, selectedSupplierId, onboardingNotes),
    onSuccess: invalidate,
  })

  const submitMutation = useMutation({
    mutationFn: () => submitSupplierOnboarding(accessToken, selectedSupplierId, onboardingNotes),
    onSuccess: invalidate,
  })

  const approveMutation = useMutation({
    mutationFn: () => approveSupplierOnboarding(accessToken, selectedSupplierId),
    onSuccess: invalidate,
  })

  const rejectMutation = useMutation({
    mutationFn: () => rejectSupplierOnboarding(accessToken, selectedSupplierId, rejectReason),
    onSuccess: invalidate,
  })

  const registerDocMutation = useMutation({
    mutationFn: async () => {
      const generatedDocumentKey = docKey.trim()
      if (!generatedDocumentKey) {
        throw new Error('Generated document key is required.')
      }
      if (!docFile) {
        throw new Error('Select a document file.')
      }
      const contentBase64 = await fileToBase64(docFile)
      return registerSupplierComplianceDocument(accessToken, selectedSupplierId, {
        documentKey: generatedDocumentKey,
        documentTypeKey: docTypeKey,
        title: docTitle || docTypeKey,
        fileName: docFile.name,
        contentType: docFile.type || 'application/octet-stream',
        sizeBytes: docFile.size,
        notes: '',
        contentBase64,
      })
    },
    onSuccess: async (created) => {
      if (canReview) {
        await approveSupplierComplianceDocument(accessToken, selectedSupplierId, created.documentId)
      }
      invalidate()
    },
  })

  const onboarding = onboardingQuery.data
  const supplierDocuments = documentsQuery.data ?? []
  const actionError =
    (startMutation.isError && startMutation.error)
    || (submitMutation.isError && submitMutation.error)
    || (approveMutation.isError && approveMutation.error)
    || (rejectMutation.isError && rejectMutation.error)
    || (registerDocMutation.isError && registerDocMutation.error)
    || null
  const canSubmit =
    onboarding &&
    (onboarding.onboardingStatus === 'draft' || onboarding.onboardingStatus === 'rejected')
  const canApproveReview = onboarding?.onboardingStatus === 'pending_review' && canReview
  const documentPosture =
    onboarding?.documentRequirements.some((doc) => !doc.isSatisfied)
      ? 'missing required documents'
      : supplierDocuments.some((doc) => doc.reviewStatus === 'rejected')
        ? 'rejected document'
        : supplierDocuments.some((doc) => isDocumentExpiringSoon(doc))
          ? 'expiring soon'
          : supplierDocuments.length > 0
            ? 'approved'
            : 'no documents'

  return (
    <section
      data-testid="supplier-onboarding-panel"
      className="rounded-xl border border-slate-700 bg-slate-900/60 p-5 lg:col-span-2"
    >
      <h2 className="text-lg font-medium text-white">Supplier onboarding</h2>
      <p className="mt-1 text-sm text-slate-400">
        Register compliance documents, submit supplier identities or sub-units for review, and approve them for sourcing.
      </p>
      {requirementsQuery.isError ? (
        <div className="mt-3">
          <ApiErrorCallout
            title="Unable to load onboarding requirements"
            message={getErrorMessage(requirementsQuery.error, 'Failed to load required documents.')}
            onRetry={() => void requirementsQuery.refetch()}
            retryLabel="Retry requirements"
          />
        </div>
      ) : null}
      {pendingQuery.isError ? (
        <div className="mt-3">
          <ApiErrorCallout
            title="Unable to load pending reviews"
            message={getErrorMessage(pendingQuery.error, 'Failed to load pending supplier onboarding reviews.')}
            onRetry={() => void pendingQuery.refetch()}
            retryLabel="Retry pending reviews"
          />
        </div>
      ) : null}
      {actionError ? (
        <div className="mt-3" data-testid="supplier-onboarding-action-error">
          <ApiErrorCallout
            title="Onboarding action failed"
            message={getErrorMessage(actionError, 'Unable to complete onboarding action.')}
          />
        </div>
      ) : null}

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
                  onClick={() => {
                    if (item.supplierId) {
                  setSelectedSupplierId(item.supplierId)
                    }
                  }}
                >
                  {item.displayName} ({item.supplierKey})
                </button>
              </li>
            ))}
          </ul>
        </div>
      ) : null}

      <div className="mt-4 grid gap-3 sm:grid-cols-2">
        <StaticSearchPicker
          id="supplier-onboarding-supplier"
          label="Supplier identity or sub-unit"
          value={selectedSupplierId}
          onChange={setSelectedSupplierId}
          options={supplierOptions}
          selectedOption={selectedSupplierOption}
          placeholder="Search supplier identities or sub-units…"
          testId="supplier-onboarding-supplier-picker"
        />
        {selectedSupplier ? (
          <p className="sm:col-span-2 text-xs text-[var(--color-text-muted)]">
            {formatSupplierIdentitySummary({
              supplierDisplayName: selectedSupplier.displayName,
              supplierKey: selectedSupplier.supplierKey,
              parentSupplierDisplayName: selectedSupplier.parentSupplierDisplayName,
              supplierUnitKind: selectedSupplier.unitKind,
            })}{' '}
            · {humanizeSupplierUnitKind(selectedSupplier.unitKind)} ·{' '}
            {formatSupplierOperationalContext({
              supplierServiceTypes: selectedSupplier.serviceTypes,
              addressLine1: selectedSupplier.addressLine1,
              locality: selectedSupplier.locality,
              regionCode: selectedSupplier.regionCode,
              postalCode: selectedSupplier.postalCode,
            })}
          </p>
        ) : null}
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

      {canManage && selectedSupplierId ? (
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
          <div className="flex flex-wrap items-center justify-between gap-2">
            <h3 className="text-sm font-medium text-slate-300">Required documents</h3>
            {selectedSupplierId ? (
              <span className="rounded-full bg-slate-800 px-2 py-1 text-[11px] uppercase tracking-wide text-slate-300">
                {documentPosture}
              </span>
            ) : null}
          </div>
          <ul className="mt-2 space-y-1 text-sm">
            {requirementsQuery.data.requirements.map((req) => {
              const status =
                onboarding?.documentRequirements.find((d) => d.documentTypeKey === req.documentTypeKey)
                ?? deriveRequirementStatus(req.documentTypeKey, supplierDocuments)
              return (
                <li key={req.documentTypeKey} className="flex justify-between gap-2">
                  <span>{req.label}</span>
                  <span className={status?.isSatisfied ? 'text-emerald-400' : 'text-[var(--color-text-muted)]'}>
                    {status?.isSatisfied ? 'approved' : 'missing'}
                  </span>
                </li>
              )
            })}
          </ul>
        </div>
      ) : null}

      {selectedSupplierId && (
        <div className="mt-4 rounded-lg border border-slate-800 bg-slate-950/50 p-3">
          <div className="flex flex-wrap items-start justify-between gap-2">
            <div>
              <h3 className="text-sm font-medium text-slate-300">Compliance documents</h3>
              <p className="mt-1 text-xs text-[var(--color-text-muted)]">
                {supplierDocuments.length} document(s) · {countDocuments(supplierDocuments, 'approved')} approved ·{' '}
                {countDocuments(supplierDocuments, 'expiring')} expiring soon · {countDocuments(supplierDocuments, 'rejected')} rejected
              </p>
            </div>
            {documentsQuery.isLoading ? (
              <span className="text-xs text-[var(--color-text-muted)]">Loading documents…</span>
            ) : null}
          </div>

          {documentsQuery.isError ? (
            <div className="mt-3">
              <ApiErrorCallout
                title="Unable to load compliance documents"
                message={getErrorMessage(documentsQuery.error, 'Failed to load supplier compliance documents.')}
                onRetry={() => void documentsQuery.refetch()}
                retryLabel="Retry documents"
              />
            </div>
          ) : null}

          {!documentsQuery.isLoading && !documentsQuery.isError ? (
            supplierDocuments.length > 0 ? (
              <ul className="mt-3 space-y-2 text-sm">
                {supplierDocuments.map((doc) => (
                  <li key={doc.documentId} className="rounded-md border border-slate-800 bg-slate-900/60 px-3 py-2">
                    <div className="flex flex-wrap items-start justify-between gap-2">
                      <div>
                        <div className="font-medium text-slate-100">
                          {doc.documentKey} · {doc.title}
                        </div>
                        <div className="mt-1 text-xs text-[var(--color-text-muted)]">
                          {doc.documentTypeKey} · v{doc.version} · {doc.reviewStatus}
                        </div>
                      </div>
                      <span className={`text-xs ${documentStatusClass(doc)}`}>{documentStatusLabel(doc)}</span>
                    </div>
                    <p className="mt-2 text-xs text-[var(--color-text-muted)]">
                      {doc.expiresAt ? `Expires ${new Date(doc.expiresAt).toLocaleDateString()}` : 'No expiration date'} ·{' '}
                      {prettyBytes(doc.sizeBytes)}
                    </p>
                  </li>
                ))}
              </ul>
            ) : (
              <p className="mt-3 text-sm text-[var(--color-text-muted)]">No compliance documents uploaded yet.</p>
            )
          ) : null}
        </div>
      )}

      {canManage && selectedSupplierId && onboarding ? (
        <div className="mt-4 rounded-lg border border-slate-800 p-3">
          <h3 className="text-sm font-medium text-slate-300">Upload document</h3>
          <div className="mt-2 grid gap-2 sm:grid-cols-4">
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
              domain="supplier"
              kind="document"
              label="Document key"
            />
            <label htmlFor="supplier-onboarding-doc-file" className="block text-sm text-slate-400">
              File
              <input
                id="supplier-onboarding-doc-file"
                className="mt-1 w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
                type="file"
                onChange={(e) => setDocFile(e.currentTarget.files?.[0] ?? null)}
              />
            </label>
          </div>
          <button
            type="button"
            className="mt-2 rounded-lg border border-slate-600 px-3 py-1.5 text-sm hover:bg-slate-800 disabled:opacity-50"
            disabled={registerDocMutation.isPending || !docKey.trim() || !docFile}
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
              <div className="font-medium">
                {formatSupplierIdentitySummary({
                  supplierDisplayName: selectedSupplier?.displayName ?? onboarding.displayName,
                  supplierKey: onboarding.supplierKey,
                  parentSupplierDisplayName: selectedSupplier?.parentSupplierDisplayName ?? null,
                  supplierUnitKind: selectedSupplier?.unitKind ?? null,
                })}
              </div>
              <div className="text-sm text-slate-400">
                {humanizeSupplierUnitKind(selectedSupplier?.unitKind ?? null)}
              </div>
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
      ) : selectedSupplierId && onboardingQuery.isError && isMissingOnboardingRecordError(onboardingQuery.error) ? (
        <p className="mt-3 text-sm text-slate-400">No onboarding record yet — start onboarding above.</p>
      ) : null}
      {selectedSupplierId
      && onboardingQuery.isError
      && !isMissingOnboardingRecordError(onboardingQuery.error) ? (
        <div className="mt-3">
          <ApiErrorCallout
            title="Unable to load supplier onboarding"
            message={getErrorMessage(onboardingQuery.error, 'Failed to load onboarding details.')}
            onRetry={() => void onboardingQuery.refetch()}
            retryLabel="Retry onboarding"
          />
        </div>
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

function isDocumentExpiringSoon(document: SupplierComplianceDocumentResponse): boolean {
  if (!document.expiresAt) {
    return false
  }
  const expiresAt = new Date(document.expiresAt).getTime()
  const now = Date.now()
  const diffDays = (expiresAt - now) / (1000 * 60 * 60 * 24)
  return diffDays >= 0 && diffDays <= 30
}

function deriveRequirementStatus(
  documentTypeKey: string,
  documents: SupplierComplianceDocumentResponse[],
): { isSatisfied: boolean } | undefined {
  const matched = documents.find((doc) => doc.documentTypeKey === documentTypeKey)
  return matched ? { isSatisfied: matched.reviewStatus === 'approved' } : undefined
}

function countDocuments(
  documents: SupplierComplianceDocumentResponse[],
  kind: 'approved' | 'expiring' | 'rejected',
): number {
  switch (kind) {
    case 'approved':
      return documents.filter((doc) => doc.reviewStatus === 'approved').length
    case 'expiring':
      return documents.filter((doc) => isDocumentExpiringSoon(doc)).length
    case 'rejected':
      return documents.filter((doc) => doc.reviewStatus === 'rejected').length
  }
}

function documentStatusLabel(document: SupplierComplianceDocumentResponse): string {
  if (document.reviewStatus === 'rejected') return 'rejected'
  if (isDocumentExpiringSoon(document)) return 'expiring soon'
  if (document.reviewStatus === 'approved') return 'approved'
  return document.reviewStatus
}

function documentStatusClass(document: SupplierComplianceDocumentResponse): string {
  if (document.reviewStatus === 'rejected') return 'text-rose-300'
  if (isDocumentExpiringSoon(document)) return 'text-amber-300'
  if (document.reviewStatus === 'approved') return 'text-emerald-300'
  return 'text-slate-400'
}

function prettyBytes(bytes: number): string {
  if (bytes < 1024) return `${bytes} B`
  if (bytes < 1024 * 1024) return `${Math.round(bytes / 1024)} KB`
  return `${Math.round(bytes / (1024 * 1024))} MB`
}
