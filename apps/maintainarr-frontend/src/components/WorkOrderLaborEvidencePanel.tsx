import { useMemo, useState } from 'react'
import { ControlledSelect, StaticSearchPicker, type PickerOption } from '@stl/shared-ui'

import type {
  TechnicianRefResponse,
  WorkOrderDetailResponse,
  WorkOrderEvidenceResponse,
  WorkOrderLaborEntryResponse,
  WorkOrderTaskLineResponse,
} from '../api/types'
import { WORK_ORDER_EVIDENCE_TYPE_OPTIONS, WORK_ORDER_LABOR_TYPE_OPTIONS } from './formOptions'

interface WorkOrderLaborEvidencePanelProps {
  workOrder: WorkOrderDetailResponse | null
  tasks: WorkOrderTaskLineResponse[]
  labor: WorkOrderLaborEntryResponse[]
  evidence: WorkOrderEvidenceResponse[]
  canPerform: boolean
  canApprove: boolean
  sessionPersonId: string
  technicianRefs: TechnicianRefResponse[]
  taskTitle: string
  laborHours: string
  laborTypeKey: string
  laborPersonId: string
  selectedTaskLineId: string
  evidenceTypeKey: string
  evidenceNotes: string
  selectedFileName: string | null
  onTaskTitleChange: (value: string) => void
  onLaborHoursChange: (value: string) => void
  onLaborTypeKeyChange: (value: string) => void
  onLaborPersonIdChange: (value: string) => void
  onSelectedTaskLineIdChange: (value: string) => void
  onEvidenceTypeKeyChange: (value: string) => void
  onEvidenceNotesChange: (value: string) => void
  onSelectFile: (file: File | null) => void
  onAddTask: () => void
  onLogLabor: () => void
  onUploadEvidence: () => void
  onApproveLabor?: (laborEntryId: string) => void
  onRejectLabor?: (laborEntryId: string, rejectionReason: string) => void
  isAddingTask: boolean
  isLoggingLabor: boolean
  isUploadingEvidence: boolean
}

function formatBytes(sizeBytes: number): string {
  if (sizeBytes < 1024) return `${sizeBytes} B`
  if (sizeBytes < 1024 * 1024) return `${(sizeBytes / 1024).toFixed(1)} KB`
  return `${(sizeBytes / (1024 * 1024)).toFixed(1)} MB`
}

function workOrderEditable(status: string): boolean {
  return status === 'open' || status === 'in_progress'
}

export function WorkOrderLaborEvidencePanel({
  workOrder,
  tasks,
  labor,
  evidence,
  canPerform,
  canApprove,
  sessionPersonId,
  technicianRefs,
  taskTitle,
  laborHours,
  laborTypeKey,
  laborPersonId,
  selectedTaskLineId,
  evidenceTypeKey,
  evidenceNotes,
  selectedFileName,
  onTaskTitleChange,
  onLaborHoursChange,
  onLaborTypeKeyChange,
  onLaborPersonIdChange,
  onSelectedTaskLineIdChange,
  onEvidenceTypeKeyChange,
  onEvidenceNotesChange,
  onSelectFile,
  onAddTask,
  onLogLabor,
  onUploadEvidence,
  onApproveLabor,
  onRejectLabor,
  isAddingTask,
  isLoggingLabor,
  isUploadingEvidence,
}: WorkOrderLaborEvidencePanelProps) {
  const [rejectionReasons, setRejectionReasons] = useState<Record<string, string>>({})

  if (!workOrder) {
    return (
      <div className="mt-4 rounded-lg border border-dashed border-slate-700 p-4 text-sm text-slate-400">
        Select a work order to manage tasks, labor, and evidence.
      </div>
    )
  }

  const editable = workOrderEditable(workOrder.status)
  const technicianOptions = useMemo<PickerOption[]>(
    () => [
      { value: sessionPersonId, label: `Me (${sessionPersonId})` },
      ...technicianRefs
        .filter((ref) => ref.personId !== sessionPersonId)
        .map((ref) => ({
          value: ref.personId,
          label: ref.displayName,
        })),
    ],
    [sessionPersonId, technicianRefs],
  )
  const taskOptions = useMemo<PickerOption[]>(
    () =>
      tasks.map((task) => ({
        value: task.taskLineId,
        label: task.title,
      })),
    [tasks],
  )
  const selectedTechnicianOption = useMemo<PickerOption | undefined>(
    () =>
      technicianOptions.find((option) => option.value === laborPersonId) ??
      (laborPersonId ? { value: laborPersonId, label: laborPersonId } : undefined),
    [laborPersonId, technicianOptions],
  )
  const selectedTaskOption = useMemo<PickerOption | undefined>(
    () =>
      taskOptions.find((option) => option.value === selectedTaskLineId) ??
      (selectedTaskLineId ? { value: selectedTaskLineId, label: selectedTaskLineId } : undefined),
    [selectedTaskLineId, taskOptions],
  )
  const updateRejectionReason = (laborEntryId: string, value: string) => {
    setRejectionReasons((current) => ({ ...current, [laborEntryId]: value }))
  }

  return (
    <div
      className="mt-4 space-y-6 border-t border-slate-800 pt-4"
      data-testid="work-order-labor-evidence-panel"
    >
      <div>
        <h4 className="text-sm font-semibold text-white">Task lines</h4>
        {tasks.length === 0 ? (
          <p className="mt-2 text-sm text-slate-400">No tasks yet.</p>
        ) : (
          <ul className="mt-2 space-y-2">
            {tasks.map((task) => (
              <li key={task.taskLineId} className="rounded border border-slate-700 bg-slate-950/40 px-3 py-2 text-sm">
                <span className="font-medium text-slate-100">{task.title}</span>
                <span className="ml-2 text-xs text-slate-500">{task.status}</span>
                {task.description ? <p className="mt-1 text-xs text-slate-400">{task.description}</p> : null}
              </li>
            ))}
          </ul>
        )}
        {canPerform && editable ? (
          <div className="mt-3 flex flex-wrap items-end gap-2">
            <label className="min-w-[12rem] flex-1 text-sm text-slate-300" htmlFor="work-order-task-title">
              Task title
              <input
                id="work-order-task-title"
                className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-2 py-1 text-sm text-white"
                value={taskTitle}
                onChange={(event) => onTaskTitleChange(event.target.value)}
              />
            </label>
            <button
              type="button"
              className="rounded bg-slate-700 px-3 py-1 text-sm text-white hover:bg-slate-600 disabled:opacity-50"
              disabled={!taskTitle.trim() || isAddingTask}
              onClick={onAddTask}
            >
              {isAddingTask ? 'Adding…' : 'Add task'}
            </button>
          </div>
        ) : null}
      </div>

      <div>
        <h4 className="text-sm font-semibold text-white">Labor</h4>
        {labor.length === 0 ? (
          <p className="mt-2 text-sm text-slate-400">No labor logged.</p>
        ) : (
          <ul className="mt-2 space-y-2">
            {labor.map((entry) => (
              <li key={entry.laborEntryId} className="rounded border border-slate-700 bg-slate-950/40 px-3 py-2 text-sm">
                <div className="flex flex-wrap items-center gap-2">
                  <span className="font-medium text-slate-100">{entry.hoursWorked}h</span>
                  <span className="text-xs text-slate-500">
                    {entry.laborTypeKey} · person {entry.personId}
                  </span>
                  <span className="rounded-full border border-slate-700 bg-slate-900 px-2 py-0.5 text-[11px] uppercase tracking-wide text-slate-300">
                    {entry.status}
                  </span>
                </div>
                <p className="mt-1 text-xs text-slate-400">{new Date(entry.loggedAt).toLocaleString()}</p>
                {entry.notes ? <p className="mt-1 text-xs text-slate-300">{entry.notes}</p> : null}
                {entry.submittedAt ? <p className="mt-1 text-xs text-slate-500">Submitted {new Date(entry.submittedAt).toLocaleString()}</p> : null}
                {entry.status === 'approved' && entry.approvedByPersonId ? (
                  <p className="mt-1 text-xs text-emerald-300">
                    Approved by {entry.approvedByPersonId}
                    {entry.approvedAt ? ` on ${new Date(entry.approvedAt).toLocaleString()}` : ''}
                  </p>
                ) : null}
                {entry.status === 'rejected' && entry.rejectionReason ? (
                  <p className="mt-1 text-xs text-rose-300">Rejected: {entry.rejectionReason}</p>
                ) : null}
                {canApprove && onApproveLabor && onRejectLabor && entry.status !== 'approved' && entry.status !== 'rejected' ? (
                  <div className="mt-3 space-y-2 rounded border border-slate-800 bg-slate-900/70 p-3">
                    <label className="block text-xs text-slate-400" htmlFor={`labor-rejection-${entry.laborEntryId}`}>
                      Rejection reason
                      <input
                        id={`labor-rejection-${entry.laborEntryId}`}
                        className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-2 py-1 text-sm text-white"
                        value={rejectionReasons[entry.laborEntryId] ?? ''}
                        onChange={(event) => updateRejectionReason(entry.laborEntryId, event.target.value)}
                        placeholder="Explain why this entry was rejected"
                      />
                    </label>
                    <div className="flex flex-wrap gap-2">
                      <button
                        type="button"
                        className="rounded bg-emerald-800 px-3 py-1 text-xs font-medium text-white hover:bg-emerald-700 disabled:opacity-50"
                        onClick={() => onApproveLabor(entry.laborEntryId)}
                      >
                        Approve
                      </button>
                      <button
                        type="button"
                        className="rounded bg-rose-800 px-3 py-1 text-xs font-medium text-white hover:bg-rose-700 disabled:opacity-50"
                        disabled={!rejectionReasons[entry.laborEntryId]?.trim()}
                        onClick={() => onRejectLabor(entry.laborEntryId, rejectionReasons[entry.laborEntryId] ?? '')}
                      >
                        Reject
                      </button>
                    </div>
                  </div>
                ) : null}
              </li>
            ))}
          </ul>
        )}
        {canPerform && editable ? (
          <div className="mt-3 grid gap-2 md:grid-cols-2">
            <StaticSearchPicker
              id="work-order-labor-technician"
              label="Technician for labor"
              value={laborPersonId}
              onChange={onLaborPersonIdChange}
              options={technicianOptions}
              selectedOption={selectedTechnicianOption}
              placeholder="Search technicians…"
              testId="work-order-labor-technician"
            />
            <label className="block text-sm text-slate-300" htmlFor="work-order-labor-hours">
              Labor hours
              <input
                id="work-order-labor-hours"
                className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-2 py-1 text-sm text-white"
                type="number"
                min="0.01"
                step="0.25"
                value={laborHours}
                onChange={(event) => onLaborHoursChange(event.target.value)}
              />
            </label>
            <label className="block text-sm text-slate-300" htmlFor="work-order-labor-type">
              Labor type
              <select
                id="work-order-labor-type"
                className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-2 py-1 text-sm text-white"
                value={laborTypeKey}
                onChange={(event) => onLaborTypeKeyChange(event.target.value)}
              >
                {WORK_ORDER_LABOR_TYPE_OPTIONS.map((option) => (
                  <option key={option.value} value={option.value}>
                    {option.label}
                  </option>
                ))}
              </select>
            </label>
            <StaticSearchPicker
              id="work-order-labor-task-link"
              label="Linked task line"
              value={selectedTaskLineId}
              onChange={onSelectedTaskLineIdChange}
              options={taskOptions}
              selectedOption={selectedTaskOption}
              placeholder="Search task lines…"
              testId="work-order-labor-task-link"
            />
            <button
              type="button"
              className="rounded bg-sky-800 px-3 py-1 text-sm text-white hover:bg-sky-700 disabled:opacity-50 md:col-span-2"
              disabled={!laborPersonId.trim() || !laborHours || isLoggingLabor}
              onClick={onLogLabor}
            >
              {isLoggingLabor ? 'Logging…' : 'Log labor'}
            </button>
          </div>
        ) : null}
      </div>

      <div>
        <h4 className="text-sm font-semibold text-white">Evidence</h4>
        {evidence.length === 0 ? (
          <p className="mt-2 text-sm text-slate-400">No evidence uploaded.</p>
        ) : (
          <ul className="mt-2 space-y-2">
            {evidence.map((item) => (
              <li key={item.evidenceId} className="rounded border border-slate-700 bg-slate-950/40 px-3 py-2 text-sm">
                <span className="font-medium text-slate-100">{item.fileName}</span>
                <p className="mt-1 text-xs text-slate-400">
                  {item.evidenceTypeKey} · {formatBytes(item.sizeBytes)} ·{' '}
                  {new Date(item.createdAt).toLocaleString()}
                </p>
                {item.notes ? <p className="mt-1 text-xs text-slate-300">{item.notes}</p> : null}
              </li>
            ))}
          </ul>
        )}
        {canPerform && editable ? (
          <div className="mt-3 space-y-2">
            <ControlledSelect
              label="Evidence type"
              value={evidenceTypeKey}
              onChange={onEvidenceTypeKeyChange}
              options={WORK_ORDER_EVIDENCE_TYPE_OPTIONS}
              emptyLabel="Select evidence type…"
              testId="work-order-evidence-type"
              className="w-full rounded border border-slate-600 bg-slate-950 px-2 py-1 text-sm text-white"
            />
            <label className="block text-sm text-slate-300" htmlFor="work-order-evidence-file">
              Work order evidence file
              <input
                id="work-order-evidence-file"
                type="file"
                className="mt-1 block w-full text-sm text-slate-300"
                onChange={(event) => onSelectFile(event.target.files?.[0] ?? null)}
              />
            </label>
            {selectedFileName ? <p className="text-xs text-slate-500">{selectedFileName}</p> : null}
            <label className="block text-sm text-slate-300" htmlFor="work-order-evidence-notes">
              Evidence notes (optional)
              <input
                id="work-order-evidence-notes"
                className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-2 py-1 text-sm text-white"
                value={evidenceNotes}
                onChange={(event) => onEvidenceNotesChange(event.target.value)}
              />
            </label>
            <button
              type="button"
              className="rounded bg-violet-800 px-3 py-1 text-sm text-white hover:bg-violet-700 disabled:opacity-50"
              disabled={!selectedFileName || !evidenceTypeKey.trim() || isUploadingEvidence}
              onClick={onUploadEvidence}
            >
              {isUploadingEvidence ? 'Uploading…' : 'Upload evidence'}
            </button>
          </div>
        ) : (
          !editable && (
            <p className="mt-2 text-xs text-amber-300">
              Evidence and labor capture are closed for completed or cancelled work orders.
            </p>
          )
        )}
      </div>
    </div>
  )
}
