import { type FormEvent, useEffect, useState } from 'react'
import { ApiErrorCallout, StaticSearchPicker, type PickerOption } from '@stl/shared-ui'
import type { OrgUnitResponse, StaffPersonDetailResponse } from '../api/types'

interface PersonProfileEditorPanelProps {
  profile: StaffPersonDetailResponse
  orgUnits: OrgUnitResponse[]
  peopleOptions: Array<{ personId: string; displayName: string }>
  canManage: boolean
  isSubmitting: boolean
  errorMessage: string | null
  onUpdate: (request: {
    givenName: string
    familyName: string
    primaryEmail: string
    primaryOrgUnitId: string | null
    managerPersonId: string | null
    jobTitle: string | null
  }) => Promise<void>
  onEmploymentStatusChange: (request: { employmentStatus: string; reason: string | null }) => Promise<void>
}

const WRITER_ROLES = new Set(['tenant_admin', 'staffarr_admin', 'hr_admin'])

export function canManagePeople(roleKey: string, isPlatformAdmin: boolean): boolean {
  return isPlatformAdmin || WRITER_ROLES.has(roleKey)
}

export function PersonProfileEditorPanel({
  profile,
  orgUnits,
  peopleOptions,
  canManage,
  isSubmitting,
  errorMessage,
  onUpdate,
  onEmploymentStatusChange,
}: PersonProfileEditorPanelProps) {
  const [givenName, setGivenName] = useState(profile.givenName)
  const [familyName, setFamilyName] = useState(profile.familyName)
  const [primaryEmail, setPrimaryEmail] = useState(profile.primaryEmail)
  const [primaryOrgUnitId, setPrimaryOrgUnitId] = useState(profile.primaryOrgUnitId ?? '')
  const [managerPersonId, setManagerPersonId] = useState(profile.managerPersonId ?? '')
  const [jobTitle, setJobTitle] = useState(profile.jobTitle ?? '')
  const [statusReason, setStatusReason] = useState('')

  useEffect(() => {
    setGivenName(profile.givenName)
    setFamilyName(profile.familyName)
    setPrimaryEmail(profile.primaryEmail)
    setPrimaryOrgUnitId(profile.primaryOrgUnitId ?? '')
    setManagerPersonId(profile.managerPersonId ?? '')
    setJobTitle(profile.jobTitle ?? '')
    setStatusReason('')
  }, [profile])

  const handleSubmit = async (event: FormEvent) => {
    event.preventDefault()
    await onUpdate({
      givenName,
      familyName,
      primaryEmail,
      primaryOrgUnitId: primaryOrgUnitId || null,
      managerPersonId: managerPersonId || null,
      jobTitle: jobTitle.trim() || null,
    })
  }

  const managerChoices = peopleOptions.filter((person) => person.personId !== profile.personId)
  const orgUnitOptions = orgUnits.map((unit) => ({
    value: unit.orgUnitId,
    label: `${unit.unitType} · ${unit.name}`,
  }))
  const selectedOrgUnitOption = orgUnitOptions.find((option) => option.value === primaryOrgUnitId)
  const managerOptions: PickerOption[] = managerChoices.map((person) => ({
    value: person.personId,
    label: person.displayName,
  }))
  const selectedManagerOption = managerOptions.find((option) => option.value === managerPersonId)

  return (
    <section className="mt-6 space-y-4 rounded-xl border border-slate-700 bg-slate-900/60 p-6">
      <header className="flex flex-wrap items-center justify-between gap-3">
        <div>
          <h2 className="text-sm font-medium text-slate-300">Profile management</h2>
          <p className="mt-1 text-xs text-slate-500">
            Employment status:{' '}
            <span className="uppercase tracking-wide text-slate-300">{profile.employmentStatus}</span>
          </p>
        </div>
        <span className={`text-xs ${canManage ? 'text-emerald-300' : 'text-slate-500'}`}>
          {canManage ? 'Write enabled' : 'Read only'}
        </span>
      </header>

      {canManage ? (
        <form className="grid gap-4 md:grid-cols-2" onSubmit={handleSubmit}>
          <label htmlFor="edit-person-given-name" className="block text-sm text-slate-300">
            Given name
            <input
              id="edit-person-given-name"
              value={givenName}
              onChange={(event) => setGivenName(event.target.value)}
              className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
              required
            />
          </label>
          <label htmlFor="edit-person-family-name" className="block text-sm text-slate-300">
            Family name
            <input
              id="edit-person-family-name"
              value={familyName}
              onChange={(event) => setFamilyName(event.target.value)}
              className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
              required
            />
          </label>
          <label htmlFor="edit-person-primary-email" className="block text-sm text-slate-300 md:col-span-2">
            Primary email
            <input
              id="edit-person-primary-email"
              type="email"
              value={primaryEmail}
              onChange={(event) => setPrimaryEmail(event.target.value)}
              className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
              required
            />
          </label>
          <label htmlFor="edit-person-primary-org-unit" className="block text-sm text-slate-300">
            Primary org unit
            <StaticSearchPicker
              id="edit-person-primary-org-unit"
              label="Primary org unit"
              value={primaryOrgUnitId}
              onChange={setPrimaryOrgUnitId}
              options={orgUnitOptions}
              placeholder="Search org units…"
              testId="edit-person-primary-org-unit-picker"
              selectedOption={selectedOrgUnitOption}
            />
          </label>
          <label htmlFor="edit-person-manager" className="block text-sm text-slate-300">
            Manager
            <StaticSearchPicker
              id="edit-person-manager"
              label="Manager"
              value={managerPersonId}
              onChange={setManagerPersonId}
              options={managerOptions}
              placeholder="Search managers…"
              testId="edit-person-manager-picker"
              selectedOption={selectedManagerOption}
            />
          </label>
          <label htmlFor="edit-person-job-title" className="block text-sm text-slate-300 md:col-span-2">
            Job title
            <input
              id="edit-person-job-title"
              value={jobTitle}
              onChange={(event) => setJobTitle(event.target.value)}
              className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
            />
          </label>
          <div className="md:col-span-2">
            <button
              type="submit"
              disabled={isSubmitting}
              className="rounded-md bg-sky-600 px-4 py-2 text-sm font-medium text-white hover:bg-sky-500 disabled:opacity-50"
            >
              {isSubmitting ? 'Saving…' : 'Save profile changes'}
            </button>
          </div>
        </form>
      ) : (
        <p className="text-sm text-slate-500">
          Profile edits require tenant admin, StaffArr admin, or HR admin role.
        </p>
      )}

      {canManage ? (
        <div className="rounded-lg border border-slate-800 bg-slate-950/50 p-4">
          <h3 className="text-sm font-medium text-slate-200">Employment status</h3>
          <label htmlFor="edit-person-status-reason" className="mt-3 block text-sm text-slate-300">
            Reason (optional)
            <input
              id="edit-person-status-reason"
              value={statusReason}
              onChange={(event) => setStatusReason(event.target.value)}
              className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
            />
          </label>
          <div className="mt-3 flex flex-wrap gap-2">
            {profile.employmentStatus !== 'active' ? (
              <button
                type="button"
                disabled={isSubmitting}
                className="rounded-md border border-emerald-700 px-3 py-2 text-xs text-emerald-200 hover:bg-emerald-950/40 disabled:opacity-50"
                onClick={() =>
                  onEmploymentStatusChange({ employmentStatus: 'active', reason: statusReason || null })
                }
              >
                Reactivate
              </button>
            ) : null}
            {profile.employmentStatus !== 'inactive' ? (
              <button
                type="button"
                disabled={isSubmitting}
                className="rounded-md border border-amber-700 px-3 py-2 text-xs text-amber-200 hover:bg-amber-950/40 disabled:opacity-50"
                onClick={() =>
                  onEmploymentStatusChange({ employmentStatus: 'inactive', reason: statusReason || null })
                }
              >
                Mark inactive
              </button>
            ) : null}
            {profile.employmentStatus !== 'terminated' ? (
              <button
                type="button"
                disabled={isSubmitting}
                className="rounded-md border border-red-700 px-3 py-2 text-xs text-red-200 hover:bg-red-950/40 disabled:opacity-50"
                onClick={() =>
                  onEmploymentStatusChange({ employmentStatus: 'terminated', reason: statusReason || null })
                }
              >
                Terminate
              </button>
            ) : null}
          </div>
        </div>
      ) : null}

      {errorMessage ? <ApiErrorCallout title="Profile update failed" message={errorMessage} /> : null}
    </section>
  )
}
