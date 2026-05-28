import { type ChangeEvent, type FormEvent, useState } from 'react'
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
  selectedDocument: PersonnelDocumentDetailResponse | null
  isLoading: boolean
  isLoadingDetail: boolean
  canManage: boolean
  isSubmitting: boolean
  errorMessage: string | null
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
  { value: 'other', label: 'Other' },
]

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
  selectedDocument,
  isLoading,
  isLoadingDetail,
  canManage,
  isSubmitting,
  errorMessage,
  onSelectDocument,
  onUploadDocument,
  contentUrlFor,
}: PersonnelDocumentsPanelProps) {
  const [documentTypeKey, setDocumentTypeKey] = useState<PersonnelDocumentTypeKey>('other')
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
    setSelectedFile(null)
  }

  return (
    <section className="mt-6 rounded-xl border border-slate-700 bg-slate-900/60 p-6">
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div>
          <h2 className="text-sm font-medium text-slate-300">Personnel documents</h2>
          <p className="mt-1 text-sm text-slate-400">
            Document registry for {personDisplayName}. StaffArr owns personnel document metadata and file storage.
          </p>
        </div>
        {canManage ? (
          <span className="rounded-full bg-slate-800 px-3 py-1 text-xs text-slate-300 ring-1 ring-slate-600">
            staffarr.documents.manage
          </span>
        ) : null}
      </div>

      {errorMessage ? (
        <p className="mt-4 rounded-lg border border-rose-500/40 bg-rose-950/40 px-3 py-2 text-sm text-rose-200">
          {errorMessage}
        </p>
      ) : null}

      {isLoading ? (
        <p className="mt-4 text-sm text-slate-400">Loading documents…</p>
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
                  {formatDocumentTypeLabel(document.documentTypeKey)} · {document.fileName} ·{' '}
                  {new Date(document.createdAt).toLocaleString()}
                </p>
              </button>
            </li>
          ))}
        </ul>
      )}

      {selectedDocument ? (
        <div className="mt-4 rounded-lg border border-slate-700 bg-slate-950/50 p-4">
          <h3 className="text-sm font-medium text-slate-200">Document detail</h3>
          {isLoadingDetail ? (
            <p className="mt-2 text-sm text-slate-400">Loading detail…</p>
          ) : (
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
          )}
        </div>
      ) : null}

      {canManage ? (
        <form onSubmit={handleSubmit} className="mt-6 space-y-3 border-t border-slate-700 pt-4">
          <h3 className="text-sm font-medium text-slate-200">Upload personnel document</h3>
          <label className="block text-xs text-slate-400">
            Document type
            <select
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
          <label className="block text-xs text-slate-400">
            Title
            <input
              value={title}
              onChange={(event) => setTitle(event.target.value)}
              required
              minLength={4}
              className="mt-1 w-full rounded-lg border border-slate-600 bg-slate-950 px-3 py-2 text-sm text-slate-100"
            />
          </label>
          <label className="block text-xs text-slate-400">
            Description
            <textarea
              value={description}
              onChange={(event) => setDescription(event.target.value)}
              rows={2}
              className="mt-1 w-full rounded-lg border border-slate-600 bg-slate-950 px-3 py-2 text-sm text-slate-100"
            />
          </label>
          <label className="block text-xs text-slate-400">
            Expires at (optional)
            <input
              type="datetime-local"
              value={expiresAt}
              onChange={(event) => setExpiresAt(event.target.value)}
              className="mt-1 w-full rounded-lg border border-slate-600 bg-slate-950 px-3 py-2 text-sm text-slate-100"
            />
          </label>
          <label className="block text-xs text-slate-400">
            File
            <input
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
