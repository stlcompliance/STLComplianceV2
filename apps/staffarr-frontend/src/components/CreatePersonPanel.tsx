import { type FormEvent, useState } from 'react'
import { ControlledSelect } from '@stl/shared-ui'
import type { OrgUnitResponse } from '../api/types'

const EMPLOYMENT_STATUS_OPTIONS = [
  { value: 'active', label: 'Active' },
  { value: 'inactive', label: 'Inactive' },
  { value: 'terminated', label: 'Terminated' },
]

interface CreatePersonPanelProps {
  orgUnits: OrgUnitResponse[]
  peopleOptions: Array<{ personId: string; displayName: string }>
  canManage: boolean
  isSubmitting: boolean
  errorMessage: string | null
  onCreate: (request: {
    givenName: string
    familyName: string
    primaryEmail: string
    employmentStatus: string
    primaryOrgUnitId: string | null
    managerPersonId: string | null
    jobTitle: string | null
  }) => Promise<void>
}

export function CreatePersonPanel({
  orgUnits,
  peopleOptions,
  canManage,
  isSubmitting,
  errorMessage,
  onCreate,
}: CreatePersonPanelProps) {
  const [givenName, setGivenName] = useState('')
  const [familyName, setFamilyName] = useState('')
  const [primaryEmail, setPrimaryEmail] = useState('')
  const [employmentStatus, setEmploymentStatus] = useState('active')
  const [primaryOrgUnitId, setPrimaryOrgUnitId] = useState('')
  const [managerPersonId, setManagerPersonId] = useState('')
  const [jobTitle, setJobTitle] = useState('')

  const handleSubmit = async (event: FormEvent) => {
    event.preventDefault()
    await onCreate({
      givenName: givenName.trim(),
      familyName: familyName.trim(),
      primaryEmail: primaryEmail.trim(),
      employmentStatus,
      primaryOrgUnitId: primaryOrgUnitId || null,
      managerPersonId: managerPersonId || null,
      jobTitle: jobTitle.trim() || null,
    })
    setGivenName('')
    setFamilyName('')
    setPrimaryEmail('')
    setEmploymentStatus('active')
    setPrimaryOrgUnitId('')
    setManagerPersonId('')
    setJobTitle('')
  }

  if (!canManage) {
    return null
  }

  return (
    <section className="mt-6 rounded-xl border border-slate-700 bg-slate-900/60 p-6" data-testid="create-person-panel">
      <header>
        <h2 className="text-sm font-medium text-slate-300">Create person</h2>
        <p className="mt-1 text-xs text-slate-500">
          Add a workforce record to the people directory. docs/13 people directory.
        </p>
      </header>

      <form className="mt-4 grid gap-4 md:grid-cols-2" onSubmit={handleSubmit}>
        <label className="block text-sm text-slate-300">
          Given name
          <input
            value={givenName}
            onChange={(event) => setGivenName(event.target.value)}
            className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
            required
            minLength={1}
          />
        </label>
        <label className="block text-sm text-slate-300">
          Family name
          <input
            value={familyName}
            onChange={(event) => setFamilyName(event.target.value)}
            className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
            required
            minLength={1}
          />
        </label>
        <label className="block text-sm text-slate-300 md:col-span-2">
          Primary email
          <input
            type="email"
            value={primaryEmail}
            onChange={(event) => setPrimaryEmail(event.target.value)}
            className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
            required
          />
        </label>
        <ControlledSelect
          label="Employment status"
          value={employmentStatus}
          onChange={setEmploymentStatus}
          options={EMPLOYMENT_STATUS_OPTIONS}
          emptyLabel="Select status…"
          testId="create-person-employment-status"
        />
        <label className="block text-sm text-slate-300">
          Job title
          <input
            value={jobTitle}
            onChange={(event) => setJobTitle(event.target.value)}
            className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
          />
        </label>
        <label className="block text-sm text-slate-300">
          Primary org unit
          <select
            value={primaryOrgUnitId}
            onChange={(event) => setPrimaryOrgUnitId(event.target.value)}
            className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
          >
            <option value="">Unassigned</option>
            {orgUnits.map((orgUnit) => (
              <option key={orgUnit.orgUnitId} value={orgUnit.orgUnitId}>
                {orgUnit.name} ({orgUnit.unitType})
              </option>
            ))}
          </select>
        </label>
        <label className="block text-sm text-slate-300">
          Manager
          <select
            value={managerPersonId}
            onChange={(event) => setManagerPersonId(event.target.value)}
            className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
          >
            <option value="">None</option>
            {peopleOptions.map((person) => (
              <option key={person.personId} value={person.personId}>
                {person.displayName}
              </option>
            ))}
          </select>
        </label>
        <div className="md:col-span-2">
          {errorMessage ? <p className="mb-3 text-sm text-rose-300">{errorMessage}</p> : null}
          <button
            type="submit"
            disabled={isSubmitting}
            className="rounded-md bg-sky-600 px-4 py-2 text-sm font-medium text-white hover:bg-sky-500 disabled:opacity-50"
          >
            {isSubmitting ? 'Creating…' : 'Create person'}
          </button>
        </div>
      </form>
    </section>
  )
}
