import { ControlledSelect } from '@stl/shared-ui'

import type {
  TechnicianRefResponse,
  WorkOrderDetailResponse,
  WorkOrderEvidenceResponse,
  WorkOrderLaborEntryResponse,
  WorkOrderTaskLineResponse,
} from '../api/types'
import { WORK_ORDER_EVIDENCE_TYPE_OPTIONS } from './formOptions'

interface WorkOrderLaborEvidencePanelProps {
  workOrder: WorkOrderDetailResponse | null
  tasks: WorkOrderTaskLineResponse[]
  labor: WorkOrderLaborEntryResponse[]
  evidence: WorkOrderEvidenceResponse[]
  canPerform: boolean
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
  isAddingTask,
  isLoggingLabor,
  isUploadingEvidence,
}: WorkOrderLaborEvidencePanelProps) {
  if (!workOrder) {
    return (
      <div className="mt-4 rounded-lg border border-dashed border-slate-700 p-4 text-sm text-slate-400">
        Select a work order to manage tasks, labor, and evidence.
      </div>
    )
  }

  const editable = workOrderEditable(workOrder.status)

  return (
    <div className="mt-4 space-y-6 border-t border-slate-800 pt-4">
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
          <div className="mt-3 flex flex-wrap gap-2">
            <input
              className="min-w-[12rem] flex-1 rounded border border-slate-600 bg-slate-950 px-2 py-1 text-sm text-white"
              placeholder="Task title"
              value={taskTitle}
              onChange={(event) => onTaskTitleChange(event.target.value)}
            />
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
                <span className="font-medium text-slate-100">{entry.hoursWorked}h</span>
                <span className="ml-2 text-xs text-slate-500">
                  {entry.laborTypeKey} · person {entry.personId}
                </span>
                <p className="mt-1 text-xs text-slate-400">{new Date(entry.loggedAt).toLocaleString()}</p>
                {entry.notes ? <p className="mt-1 text-xs text-slate-300">{entry.notes}</p> : null}
              </li>
            ))}
          </ul>
        )}
        {canPerform && editable ? (
          <div className="mt-3 grid gap-2 md:grid-cols-2">
            <select
              className="rounded border border-slate-600 bg-slate-950 px-2 py-1 text-sm text-white"
              value={laborPersonId}
              onChange={(event) => onLaborPersonIdChange(event.target.value)}
            >
              <option value="">Select technician…</option>
              <option value={sessionPersonId}>Me ({sessionPersonId})</option>
              {technicianRefs
                .filter((ref) => ref.personId !== sessionPersonId)
                .map((ref) => (
                  <option key={ref.personId} value={ref.personId}>
                    {ref.displayName}
                  </option>
                ))}
            </select>
            <input
              className="rounded border border-slate-600 bg-slate-950 px-2 py-1 text-sm text-white"
              placeholder="Hours"
              type="number"
              min="0.01"
              step="0.25"
              value={laborHours}
              onChange={(event) => onLaborHoursChange(event.target.value)}
            />
            <select
              className="rounded border border-slate-600 bg-slate-950 px-2 py-1 text-sm text-white"
              value={laborTypeKey}
              onChange={(event) => onLaborTypeKeyChange(event.target.value)}
            >
              <option value="regular">Regular</option>
              <option value="overtime">Overtime</option>
              <option value="travel">Travel</option>
            </select>
            <select
              className="rounded border border-slate-600 bg-slate-950 px-2 py-1 text-sm text-white"
              value={selectedTaskLineId}
              onChange={(event) => onSelectedTaskLineIdChange(event.target.value)}
            >
              <option value="">No task link</option>
              {tasks.map((task) => (
                <option key={task.taskLineId} value={task.taskLineId}>
                  {task.title}
                </option>
              ))}
            </select>
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
            <input
              type="file"
              className="block w-full text-sm text-slate-300"
              onChange={(event) => onSelectFile(event.target.files?.[0] ?? null)}
            />
            {selectedFileName ? <p className="text-xs text-slate-500">{selectedFileName}</p> : null}
            <input
              className="w-full rounded border border-slate-600 bg-slate-950 px-2 py-1 text-sm text-white"
              placeholder="Notes (optional)"
              value={evidenceNotes}
              onChange={(event) => onEvidenceNotesChange(event.target.value)}
            />
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
