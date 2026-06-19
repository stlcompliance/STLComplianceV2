import { type ChangeEvent, type FormEvent, useState } from 'react'
import { ApiErrorCallout } from '@stl/shared-ui'
import type {
  CreatePersonnelDocumentRequest,
  PersonnelDocumentDetailResponse,
  PersonnelDocumentSummaryResponse,
  PersonnelDocumentTypeKey,
} from '../api/types'

interface PersonnelDocumentsPanelProps {
  personId: string
  personDisplayName: string
  accessToken: string
  documents: PersonnelDocumentSummaryResponse[]
  selectedDocumentId?: string | null
  selectedDocument: PersonnelDocumentDetailResponse | null
  isLoading: boolean
  isError?: boolean
  readErrorMessage?: string | null
  onRetryRead?: () => void
  isLoadingDetail: boolean
  isDetailError?: boolean
  detailErrorMessage?: string | null
  onRetryDetail?: () => void
  canManage: boolean
  isSubmitting: boolean
  actionErrorMessage: string | null
  onSelectDocument: (documentId: string) => void
  onUploadDocument: (request: CreatePersonnelDocumentRequest) => Promise<void>
  contentUrlFor: (documentId: string) => string
}

const documentTypeOptions: { value: PersonnelDocumentTypeKey; label: string }[] = [
  { value: 'id_verification', label: 'ID verification' },
  { value: 'employment_contract', label: 'Employment contract' },
  { value: 'certification_copy', label: 'Certification copy' },
  { value: 'medical_form', label: 'Medical form' },
  { value: 'policy_acknowledgment', label: 'Policy acknowledgment' },
  { value: 'offer_letter', label: 'Offer letter' },
  { value: 'employment_agreement', label: 'Employment agreement' },
  { value: 'handbook_acknowledgment', label: 'Handbook acknowledgment' },
  { value: 'emergency_contact', label: 'Emergency contact' },
  { value: 'job_description_acknowledgment', label: 'Job description acknowledgment' },
  { value: 'corrective_action', label: 'Corrective action' },
  { value: 'performance_review', label: 'Performance review' },
  { value: 'leave_paperwork', label: 'Leave paperwork' },
  { value: 'termination_paperwork', label: 'Termination paperwork' },
  { value: 'work_authorization', label: 'Work authorization' },
  { value: 'medical_accommodation', label: 'Medical accommodation' },
  { value: 'eeo_self_id', label: 'EEO / self-ID' },
  { value: 'other', label: 'Other' },
]

const accessLevelOptions = [
  { value: 'employee', label: 'Employee' },
  { value: 'manager', label: 'Manager' },
  { value: 'hr', label: 'HR' },
  { value: 'restricted', label: 'Restricted' },
] as const

const retentionCategoryOptions = [
  { value: 'personnel_file', label: 'Personnel file' },
  { value: 'employment_eligibility', label: 'Employment eligibility' },
  { value: 'discipline', label: 'Discipline' },
  { value: 'performance', label: 'Performance' },
  { value: 'leave', label: 'Leave' },
  { value: 'termination', label: 'Termination' },
  { value: 'medical', label: 'Medical' },
  { value: 'eeo', label: 'EEO' },
  { value: 'other', label: 'Other' },
] as const

export function canManagePersonnelDocuments(tenantRoleKey: string, isPlatformAdmin: boolean): boolean {
  if (isPlatformAdmin) {
    return true
  }

  return ['tenant_admin', 'staffarr_admin', 'hr_admin'].includes(tenantRoleKey)
}

function formatDocumentTypeLabel(key: string): string {
  const match = documentTypeOptions.find((option) => option.value === key)
  return match?.label ?? key
}

function formatFileSize(bytes: number): string {
  if (bytes < 1024) {
    return `${bytes} B`
  }

  if (bytes < 1024 * 1024) {
    return `${(bytes / 1024).toFixed(1)} KB`
  }

  return `${(bytes / (1024 * 1024)).toFixed(1)} MB`
}

async function readFileAsBase64(file: File): Promise<string> {
  const buffer = await file.arrayBuffer()
  const bytes = new Uint8Array(buffer)
  let binary = ''
  for (const byte of bytes) {
    binary += String.fromCharCode(byte)
  }

  return btoa(binary)
}

export function PersonnelDocumentsPanel({
  personId: _personId,
  personDisplayName,
  accessToken,
  documents,
  selectedDocumentId = null,
  selectedDocument,
  isLoading,
  isError = false,
  readErrorMessage = null,
  onRetryRead,
  isLoadingDetail,
  isDetailError = false,
  detailErrorMessage = null,
  onRetryDetail,
  canManage,
  isSubmitting,
  actionErrorMessage,
  onSelectDocument,
  onUploadDocument,
  contentUrlFor,
}: PersonnelDocumentsPanelProps) {
  const [documentTypeKey, setDocumentTypeKey] = useState<PersonnelDocumentTypeKey>('other')
  const [accessLevel, setAccessLevel] = useState<(typeof accessLevelOptions)[number]['value']>('manager')
  const [retentionCategory, setRetentionCategory] =
    useState<(typeof retentionCategoryOptions)[number]['value']>('personnel_file')
  const [restrictedData, setRestrictedData] = useState(false)
  const [title, setTitle] = useState('')
  const [description, setDescription] = useState('')
  const [expiresAt, setExpiresAt] = useState('')
  const [selectedFile, setSelectedFile] = useState<File | null>(null)

  const handleFileChange = (event: ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0] ?? null
    setSelectedFile(file)
    if (file && !title.trim()) {
      setTitle(file.name.replace(/\.[^.]+$/, ''))
    }
  }

  const handleSubmit = async (event: FormEvent) => {
    event.preventDefault()
    if (!selectedFile) {
      return
    }

    const contentBase64 = await readFileAsBase64(selectedFile)
    await onUploadDocument({
      documentTypeKey,
      accessLevel,
      retentionCategory,
      restrictedData,
      title,
      fileName: selectedFile.name,
      contentType: selectedFile.type || 'application/octet-stream',
      contentBase64,
      description: description.trim() ? description.trim() : null,
      expiresAt: expiresAt ? new Date(expiresAt).toISOString() : null,
    })
    setTitle('')
    setDescription('')
    setExpiresAt('')
    setAccessLevel('manager')
    setRetentionCategory('personnel_file')
    setRestrictedData(false)
    setSelectedFile(null)
  }

  return (
    <section className="mt-6 rounded-xl border border-slate-700 bg-slate-900/60 p-6">
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div>
          <h2 className="text-sm font-medium text-slate-300">Personnel documents</h2>
          <p className="mt-1 text-sm text-slate-400">
            Document registry for {personDisplayName}. StaffArr owns personnel document metadata, retention, and file storage.
          </p>
        </div>
        {canManage ? (
          <span className="rounded-full bg-slate-800 px-3 py-1 text-xs text-slate-300 ring-1 ring-slate-600">
            staffarr.documents.manage
          </span>
        ) : null}
      </div>

      {actionErrorMessage ? (
        <div className="mt-4">
          <ApiErrorCallout title="Personnel document action failed" message={actionErrorMessage} />
        </div>
      ) : null}

      {isLoading ? (
        <p className="mt-4 text-sm text-slate-400">Loading documents…</p>
      ) : isError ? (
        <div className="mt-4">
          <ApiErrorCallout
            title="Personnel documents unavailable"
            message={readErrorMessage ?? 'Failed to load personnel documents.'}
            onRetry={onRetryRead}
            retryLabel="Retry documents"
          />
        </div>
      ) : documents.length === 0 ? (
        <p className="mt-4 text-sm text-slate-400">No personnel documents uploaded for this person yet.</p>
      ) : (
        <ul className="mt-4 space-y-2">
          {documents.map((document) => (
            <li key={document.documentId}>
              <button
                type="button"
                onClick={() => onSelectDocument(document.documentId)}
                className={`w-full rounded-lg border px-3 py-2 text-left transition ${
                  selectedDocument?.documentId === document.documentId
                    ? 'border-sky-500/60 bg-sky-950/30'
                    : 'border-slate-700 bg-slate-950/40 hover:border-slate-500'
                }`}
              >
                <div className="flex flex-wrap items-center justify-between gap-2">
                  <span className="font-medium text-slate-100">{document.title}</span>
                  <span className="text-xs text-slate-400">{formatFileSize(document.sizeBytes)}</span>
                </div>
                <p className="mt-1 text-xs text-slate-400">
                  {formatDocumentTypeLabel(document.documentTypeKey)} · {document.accessLevel} · {document.retentionCategory} · {document.fileName} ·{' '}
                  {new Date(document.createdAt).toLocaleString()}
                </p>
                {document.restrictedData ? (
                  <p className="mt-1 text-xs font-medium text-amber-300">Restricted data</p>
                ) : null}
              </button>
            </li>
          ))}
        </ul>
      )}

      {selectedDocumentId ? (
        <div className="mt-4 rounded-lg border border-slate-700 bg-slate-950/50 p-4">
          <h3 className="text-sm font-medium text-slate-200">Document detail</h3>
          {isLoadingDetail ? (
            <p className="mt-2 text-sm text-slate-400">Loading detail…</p>
          ) : isDetailError ? (
            <div className="mt-2">
              <ApiErrorCallout
                title="Document detail unavailable"
                message={detailErrorMessage ?? 'Failed to load document detail.'}
                onRetry={onRetryDetail}
                retryLabel="Retry document detail"
              />
            </div>
          ) : (
            <>
              {selectedDocument ? (
                <>
                  {selectedDocument.description ? (
                    <p className="mt-2 text-sm text-slate-300">{selectedDocument.description}</p>
                  ) : null}
                  <dl className="mt-3 grid gap-2 text-xs text-slate-400 sm:grid-cols-2">
                    <div>
                      <dt className="uppercase tracking-wide">Type</dt>
                      <dd className="text-slate-200">{formatDocumentTypeLabel(selectedDocument.documentTypeKey)}</dd>
                    </div>
                    <div>
                      <dt className="uppercase tracking-wide">Access</dt>
                      <dd className="text-slate-200">{selectedDocument.accessLevel}</dd>
                    </div>
                    <div>
                      <dt className="uppercase tracking-wide">Retention</dt>
                      <dd className="text-slate-200">{selectedDocument.retentionCategory}</dd>
                    </div>
                    <div>
                      <dt className="uppercase tracking-wide">Restricted</dt>
                      <dd className="text-slate-200">{selectedDocument.restrictedData ? 'Yes' : 'No'}</dd>
                    </div>
                    <div>
                      <dt className="uppercase tracking-wide">File</dt>
                      <dd className="text-slate-200">{selectedDocument.fileName}</dd>
                    </div>
                    {selectedDocument.expiresAt ? (
                      <div>
                        <dt className="uppercase tracking-wide">Expires</dt>
                        <dd className="text-slate-200">{new Date(selectedDocument.expiresAt).toLocaleString()}</dd>
                      </div>
                    ) : null}
                  </dl>
                  <a
                    href={contentUrlFor(selectedDocument.documentId)}
                    download={selectedDocument.fileName}
                    className="mt-4 inline-flex rounded-lg bg-slate-700 px-4 py-2 text-sm font-medium text-white hover:bg-slate-600"
                    onClick={(event) => {
                      event.preventDefault()
                      void fetch(contentUrlFor(selectedDocument.documentId), {
                        headers: { Authorization: `Bearer ${accessToken}` },
                      })
                        .then(async (response) => {
                          if (!response.ok) {
                            throw new Error('Download failed')
                          }

                          const blob = await response.blob()
                          const url = URL.createObjectURL(blob)
                          const anchor = window.document.createElement('a')
                          anchor.href = url
                          anchor.download = selectedDocument.fileName
                          anchor.click()
                          URL.revokeObjectURL(url)
                        })
                        .catch(() => {
                          window.alert('Could not download document.')
                        })
                    }}
                  >
                    Download file
                  </a>
                </>
              ) : (
                <p className="mt-2 text-sm text-slate-400">Document detail is unavailable.</p>
              )}
            </>
          )}
        </div>
      ) : null}

      {canManage ? (
        <form onSubmit={handleSubmit} className="mt-6 space-y-3 border-t border-slate-700 pt-4">
          <h3 className="text-sm font-medium text-slate-200">Upload personnel document</h3>
          <label htmlFor="personnel-document-type" className="block text-xs text-slate-400">
            Document type
            <select
              id="personnel-document-type"
              value={documentTypeKey}
              onChange={(event) => setDocumentTypeKey(event.target.value as PersonnelDocumentTypeKey)}
              className="mt-1 w-full rounded-lg border border-slate-600 bg-slate-950 px-3 py-2 text-sm text-slate-100"
            >
              {documentTypeOptions.map((option) => (
                <option key={option.value} value={option.value}>
                  {option.label}
                </option>
              ))}
            </select>
          </label>
          <label htmlFor="personnel-document-access" className="block text-xs text-slate-400">
            Access level
            <select
              id="personnel-document-access"
              value={accessLevel}
              onChange={(event) => setAccessLevel(event.target.value as typeof accessLevel)}
              className="mt-1 w-full rounded-lg border border-slate-600 bg-slate-950 px-3 py-2 text-sm text-slate-100"
            >
              {accessLevelOptions.map((option) => (
                <option key={option.value} value={option.value}>
                  {option.label}
                </option>
              ))}
            </select>
          </label>
          <label htmlFor="personnel-document-retention" className="block text-xs text-slate-400">
            Retention category
            <select
              id="personnel-document-retention"
              value={retentionCategory}
              onChange={(event) => setRetentionCategory(event.target.value as typeof retentionCategory)}
              className="mt-1 w-full rounded-lg border border-slate-600 bg-slate-950 px-3 py-2 text-sm text-slate-100"
            >
              {retentionCategoryOptions.map((option) => (
                <option key={option.value} value={option.value}>
                  {option.label}
                </option>
              ))}
            </select>
          </label>
          <label htmlFor="personnel-document-restricted" className="flex items-center gap-2 text-xs text-slate-400">
            <input
              id="personnel-document-restricted"
              type="checkbox"
              checked={restrictedData}
              onChange={(event) => setRestrictedData(event.target.checked)}
              className="h-4 w-4 rounded border-slate-700 bg-slate-950 text-sky-500"
            />
            Restricted data
          </label>
          <label htmlFor="personnel-document-title" className="block text-xs text-slate-400">
            Title
            <input
              id="personnel-document-title"
              value={title}
              onChange={(event) => setTitle(event.target.value)}
              required
              minLength={4}
              className="mt-1 w-full rounded-lg border border-slate-600 bg-slate-950 px-3 py-2 text-sm text-slate-100"
            />
          </label>
          <label htmlFor="personnel-document-description" className="block text-xs text-slate-400">
            Description
            <textarea
              id="personnel-document-description"
              value={description}
              onChange={(event) => setDescription(event.target.value)}
              rows={2}
              className="mt-1 w-full rounded-lg border border-slate-600 bg-slate-950 px-3 py-2 text-sm text-slate-100"
            />
          </label>
          <label htmlFor="personnel-document-expires-at" className="block text-xs text-slate-400">
            Expires at (optional)
            <input
              id="personnel-document-expires-at"
              type="datetime-local"
              value={expiresAt}
              onChange={(event) => setExpiresAt(event.target.value)}
              className="mt-1 w-full rounded-lg border border-slate-600 bg-slate-950 px-3 py-2 text-sm text-slate-100"
            />
          </label>
          <label htmlFor="personnel-document-file" className="block text-xs text-slate-400">
            File
            <input
              id="personnel-document-file"
              type="file"
              required
              onChange={handleFileChange}
              className="mt-1 block w-full text-sm text-slate-300"
            />
          </label>
          <button
            type="submit"
            disabled={isSubmitting || !selectedFile}
            className="rounded-lg bg-sky-600 px-4 py-2 text-sm font-medium text-white hover:bg-sky-500 disabled:opacity-50"
          >
            {isSubmitting ? 'Uploading…' : 'Upload document'}
          </button>
        </form>
      ) : null}
    </section>
  )
}
