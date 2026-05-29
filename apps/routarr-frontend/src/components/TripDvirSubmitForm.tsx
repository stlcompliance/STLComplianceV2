import { useState } from 'react'

export type TripDvirSubmitPayload = {
  phase: 'pre_trip' | 'post_trip'
  result: string
  odometerReading?: number
  defectNotes?: string
  vehicleRefKey?: string
}

type Props = {
  phase: 'pre_trip' | 'post_trip'
  label: string
  vehicleRefKey: string | null
  disabled: boolean
  onSubmit: (payload: TripDvirSubmitPayload) => void
  pending: boolean
}

export function TripDvirSubmitForm({
  phase,
  label,
  vehicleRefKey,
  disabled,
  onSubmit,
  pending,
}: Props) {
  const [result, setResult] = useState<'pass' | 'fail' | 'conditional'>('pass')
  const [odometer, setOdometer] = useState('')
  const [defectNotes, setDefectNotes] = useState('')

  const resultId = `trip-dvir-result-${phase}`
  const odometerId = `trip-dvir-odometer-${phase}`
  const defectNotesId = `trip-dvir-defect-notes-${phase}`

  return (
    <div className="rounded border border-slate-700 bg-slate-950/40 p-2" data-testid={`dvir-form-${phase}`}>
      <p className="text-xs font-medium text-slate-300">{label}</p>
      <div className="mt-2 flex flex-wrap items-end gap-3">
        <label htmlFor={resultId} className="block text-xs text-slate-300">
          Trip DVIR result
          <select
            id={resultId}
            className="mt-1 block rounded border border-slate-600 bg-slate-950 px-2 py-1 text-xs text-slate-100"
            value={result}
            onChange={(e) => setResult(e.target.value as 'pass' | 'fail' | 'conditional')}
          >
            <option value="pass">Pass</option>
            <option value="conditional">Conditional</option>
            <option value="fail">Fail</option>
          </select>
        </label>
        <label htmlFor={odometerId} className="block text-xs text-slate-300">
          Odometer reading
          <input
            id={odometerId}
            type="number"
            className="mt-1 block w-28 rounded border border-slate-600 bg-slate-950 px-2 py-1 text-xs text-slate-100"
            value={odometer}
            onChange={(e) => setOdometer(e.target.value)}
          />
        </label>
      </div>
      {(result === 'fail' || result === 'conditional') && (
        <label className="mt-2 block text-xs text-slate-300" htmlFor={defectNotesId}>
          Defect notes (required)
          <textarea
            id={defectNotesId}
            className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-2 py-1 text-xs text-slate-100"
            rows={2}
            value={defectNotes}
            onChange={(e) => setDefectNotes(e.target.value)}
          />
        </label>
      )}
      <button
        type="button"
        className="mt-2 rounded border border-slate-600 px-2 py-1 text-xs text-slate-200 disabled:opacity-50"
        disabled={disabled || pending}
        onClick={() =>
          onSubmit({
            phase,
            result,
            odometerReading: odometer ? Number(odometer) : undefined,
            defectNotes: defectNotes || undefined,
            vehicleRefKey: vehicleRefKey ?? undefined,
          })
        }
      >
        Submit {phase === 'pre_trip' ? 'pre-trip' : 'post-trip'} DVIR
      </button>
    </div>
  )
}
