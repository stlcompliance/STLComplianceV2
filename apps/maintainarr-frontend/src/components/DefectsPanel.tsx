import type { AssetResponse, DefectEvidenceResponse, DefectSummaryResponse } from '../api/types'
import { DefectEvidencePanel } from './DefectEvidencePanel'

interface DefectsPanelProps {
  canCreate: boolean
  canCreateWorkOrder: boolean
  canManageStatus: boolean
  canUploadEvidence: boolean
  viewAllDefects: boolean
  assets: AssetResponse[]
  defects: DefectSummaryResponse[]
  selectedDefectId: string
  selectedDefect: DefectSummaryResponse | null
  defectEvidence: DefectEvidenceResponse[]
  selectedAssetId: string
  defectTitle: string
  defectDescription: string
  defectSeverity: string
  statusFilter: string
  evidenceTypeKey: string
  evidenceNotes: string
  selectedEvidenceFileName: string | null
  isLoading: boolean
  isEvidenceLoading: boolean
  isCreating: boolean
  isUpdatingStatus: boolean
  isUploadingEvidence: boolean
  onSelectedDefectIdChange: (value: string) => void
  onSelectedAssetIdChange: (value: string) => void
  onDefectTitleChange: (value: string) => void
  onDefectDescriptionChange: (value: string) => void
  onDefectSeverityChange: (value: string) => void
  onStatusFilterChange: (value: string) => void
  onEvidenceTypeKeyChange: (value: string) => void
  onEvidenceNotesChange: (value: string) => void
  onSelectEvidenceFile: (file: File | null) => void
  onUploadEvidence: () => void
  onCreateDefect: () => void
  onCreateWorkOrderFromDefect: (defectId: string) => void
  onUpdateStatus: (defectId: string, status: string) => void
  creatingWorkOrderDefectId?: string | null
}

function canOpenWorkOrderFromDefect(status: string): boolean {
  return status === 'open' || status === 'acknowledged' || status === 'in_repair'
}

function formatSource(source: string): string {
  if (source === 'inspection_auto') {
    return 'Inspection (auto)'
  }
  if (source === 'inspection_manual') {
    return 'Inspection (manual)'
  }
  return 'Manual'
}

export function DefectsPanel({
  canCreate,
  canCreateWorkOrder,
  canManageStatus,
  canUploadEvidence,
  viewAllDefects,
  assets,
  defects,
  selectedDefectId,
  selectedDefect,
  defectEvidence,
  selectedAssetId,
  defectTitle,
  defectDescription,
  defectSeverity,
  statusFilter,
  evidenceTypeKey,
  evidenceNotes,
  selectedEvidenceFileName,
  isLoading,
  isEvidenceLoading,
  isCreating,
  isUpdatingStatus,
  isUploadingEvidence,
  onSelectedDefectIdChange,
  onSelectedAssetIdChange,
  onDefectTitleChange,
  onDefectDescriptionChange,
  onDefectSeverityChange,
  onStatusFilterChange,
  onEvidenceTypeKeyChange,
  onEvidenceNotesChange,
  onSelectEvidenceFile,
  onUploadEvidence,
  onCreateDefect,
  onCreateWorkOrderFromDefect,
  onUpdateStatus,
  creatingWorkOrderDefectId = null,
}: DefectsPanelProps) {
  return (
    <section className="rounded-xl border border-slate-700 bg-slate-900/60 p-6">
      <header className="mb-4">
        <h2 className="text-lg font-semibold text-white">Defects</h2>
        <p className="mt-1 text-sm text-slate-400">
          Failed inspection items auto-create defects on run completion.
          {viewAllDefects ? ' Managers see all tenant defects.' : ' Technicians see defects they reported.'}
        </p>
      </header>

      <div className="mb-4 flex flex-wrap gap-3">
        <label className="block text-sm" htmlFor="defects-status-filter">
          <span className="text-slate-300">Status filter</span>
          <select
            id="defects-status-filter"
            className="mt-1 rounded-lg border border-slate-600 bg-slate-950 px-3 py-2 text-white"
            value={statusFilter}
            onChange={(event) => onStatusFilterChange(event.target.value)}
          >
            <option value="">All statuses</option>
            <option value="open">Open</option>
            <option value="acknowledged">Acknowledged</option>
            <option value="in_repair">In repair</option>
            <option value="resolved">Resolved</option>
            <option value="closed">Closed</option>
          </select>
        </label>
      </div>

      {canCreate ? (
        <div className="mb-6 grid gap-4 rounded-lg border border-slate-800 bg-slate-950/50 p-4 md:grid-cols-2">
          <label className="block text-sm md:col-span-2" htmlFor="defect-create-asset">
            <span className="text-slate-300">Asset for defect</span>
            <select
              id="defect-create-asset"
              className="mt-1 w-full rounded-lg border border-slate-600 bg-slate-950 px-3 py-2 text-white"
              value={selectedAssetId}
              onChange={(event) => onSelectedAssetIdChange(event.target.value)}
            >
              <option value="">Select asset…</option>
              {assets.map((asset) => (
                <option key={asset.assetId} value={asset.assetId}>
                  {asset.assetTag} — {asset.name}
                </option>
              ))}
            </select>
          </label>

          <label className="block text-sm md:col-span-2" htmlFor="defect-create-title">
            <span className="text-slate-300">Defect title</span>
            <input
              id="defect-create-title"
              className="mt-1 w-full rounded-lg border border-slate-600 bg-slate-950 px-3 py-2 text-white"
              value={defectTitle}
              onChange={(event) => onDefectTitleChange(event.target.value)}
            />
          </label>

          <label className="block text-sm md:col-span-2" htmlFor="defect-create-description">
            <span className="text-slate-300">Defect description</span>
            <textarea
              id="defect-create-description"
              className="mt-1 w-full rounded-lg border border-slate-600 bg-slate-950 px-3 py-2 text-white"
              rows={2}
              value={defectDescription}
              onChange={(event) => onDefectDescriptionChange(event.target.value)}
            />
          </label>

          <label className="block text-sm" htmlFor="defect-create-severity">
            <span className="text-slate-300">Defect severity</span>
            <select
              id="defect-create-severity"
              className="mt-1 w-full rounded-lg border border-slate-600 bg-slate-950 px-3 py-2 text-white"
              value={defectSeverity}
              onChange={(event) => onDefectSeverityChange(event.target.value)}
            >
              <option value="low">Low</option>
              <option value="medium">Medium</option>
              <option value="high">High</option>
              <option value="critical">Critical</option>
            </select>
          </label>

          <div className="flex items-end">
            <button
              type="button"
              className="rounded-lg bg-amber-700 px-4 py-2 text-sm font-medium text-white hover:bg-amber-600 disabled:opacity-50"
              disabled={!selectedAssetId || !defectTitle.trim() || isCreating}
              onClick={onCreateDefect}
            >
              {isCreating ? 'Creating…' : 'Report defect'}
            </button>
          </div>
        </div>
      ) : null}

      {isLoading ? (
        <p className="text-sm text-slate-400">Loading defects…</p>
      ) : defects.length === 0 ? (
        <p className="text-sm text-slate-400">No defects match the current filter.</p>
      ) : (
        <div className="overflow-x-auto" data-testid="defect-list">
          <table className="min-w-full text-left text-sm">
            <thead className="border-b border-slate-700 text-slate-400">
              <tr>
                <th className="px-3 py-2 font-medium">Asset</th>
                <th className="px-3 py-2 font-medium">Title</th>
                <th className="px-3 py-2 font-medium">Severity</th>
                <th className="px-3 py-2 font-medium">Status</th>
                <th className="px-3 py-2 font-medium">Evidence</th>
                <th className="px-3 py-2 font-medium">Source</th>
                <th className="px-3 py-2 font-medium">Created</th>
                {canCreateWorkOrder || canManageStatus ? (
                  <th className="px-3 py-2 font-medium">Actions</th>
                ) : null}
              </tr>
            </thead>
            <tbody>
              {defects.map((defect) => (
                <tr
                  key={defect.defectId}
                  className={`border-b border-slate-800 text-slate-200 ${
                    selectedDefectId === defect.defectId ? 'bg-slate-800/60' : ''
                  }`}
                >
                  <td className="px-3 py-2">
                    <div className="font-medium">{defect.assetTag}</div>
                    <div className="text-xs text-slate-400">{defect.assetName}</div>
                  </td>
                  <td className="px-3 py-2">
                    <button
                      type="button"
                      className="text-left font-medium text-sky-300 hover:text-sky-200"
                      onClick={() => onSelectedDefectIdChange(defect.defectId)}
                    >
                      {defect.title}
                    </button>
                    {defect.checklistItemKey ? (
                      <div className="text-xs text-slate-400">{defect.checklistItemKey}</div>
                    ) : null}
                  </td>
                  <td className="px-3 py-2">{defect.severity}</td>
                  <td className="px-3 py-2">{defect.status}</td>
                  <td className="px-3 py-2">{defect.evidenceCount}</td>
                  <td className="px-3 py-2">{formatSource(defect.source)}</td>
                  <td className="px-3 py-2 text-slate-300">{new Date(defect.createdAt).toLocaleString()}</td>
                  {canCreateWorkOrder || canManageStatus ? (
                    <td className="px-3 py-2">
                      <div className="flex flex-wrap items-center gap-2">
                        {canCreateWorkOrder && canOpenWorkOrderFromDefect(defect.status) ? (
                          <button
                            type="button"
                            className="rounded bg-sky-800 px-2 py-1 text-xs text-white hover:bg-sky-700 disabled:opacity-50"
                            disabled={creatingWorkOrderDefectId === defect.defectId}
                            onClick={() => onCreateWorkOrderFromDefect(defect.defectId)}
                          >
                            {creatingWorkOrderDefectId === defect.defectId ? 'Opening…' : 'Open work order'}
                          </button>
                        ) : null}
                        {canManageStatus ? (
                          <select
                            id={`defect-status-${defect.defectId}`}
                            className="rounded border border-slate-600 bg-slate-950 px-2 py-1 text-white"
                            value={defect.status}
                            disabled={isUpdatingStatus}
                            aria-label={`Defect status for ${defect.title}`}
                            onChange={(event) => onUpdateStatus(defect.defectId, event.target.value)}
                          >
                            <option value="open">Open</option>
                            <option value="acknowledged">Acknowledged</option>
                            <option value="in_repair">In repair</option>
                            <option value="resolved">Resolved</option>
                            <option value="closed">Closed</option>
                          </select>
                        ) : null}
                      </div>
                    </td>
                  ) : null}
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      <DefectEvidencePanel
        defectId={selectedDefectId || null}
        defectTitle={selectedDefect?.title ?? null}
        defectStatus={selectedDefect?.status ?? null}
        evidence={defectEvidence}
        canUpload={canUploadEvidence}
        evidenceTypeKey={evidenceTypeKey}
        evidenceNotes={evidenceNotes}
        selectedFileName={selectedEvidenceFileName}
        onEvidenceTypeKeyChange={onEvidenceTypeKeyChange}
        onEvidenceNotesChange={onEvidenceNotesChange}
        onSelectFile={onSelectEvidenceFile}
        onUploadEvidence={onUploadEvidence}
        isUploadingEvidence={isUploadingEvidence}
        isLoading={isEvidenceLoading}
      />
    </section>
  )
}
