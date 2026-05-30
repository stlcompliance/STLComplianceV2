import { ControlledSelect } from '@stl/shared-ui'
import { useEffect, useMemo, useState } from 'react'

import type {
  CreatePartyContactRequest,
  ExternalPartyResponse,
  PartyRegistryRoute,
  UpdateExternalPartyRequest,
} from '../api/types'
import { GeneratedKeyFieldGroup } from '../forms/GeneratedKeyFieldGroup'

interface PartyRegistryPanelProps {
  title: string
  partyType: PartyRegistryRoute
  parties: ExternalPartyResponse[]
  canManage: boolean
  isLoading: boolean
  partyKey: string
  displayName: string
  legalName: string
  taxIdentifier: string
  notes: string
  onPartyKeyChange: (value: string) => void
  onDisplayNameChange: (value: string) => void
  onLegalNameChange: (value: string) => void
  onTaxIdentifierChange: (value: string) => void
  onNotesChange: (value: string) => void
  onCreate: () => void
  isCreating: boolean
  onUpdateParty: (partyId: string, request: UpdateExternalPartyRequest) => void
  onUpdateApprovalStatus: (partyId: string, approvalStatus: string) => void
  onUpdateStatus: (partyId: string, status: string) => void
  onAddContact: (partyId: string, request: CreatePartyContactRequest) => void
  isUpdating: boolean
  isUpdatingApproval: boolean
  isUpdatingStatus: boolean
  isAddingContact: boolean
}

const APPROVAL_OPTIONS = [
  { value: 'pending', label: 'Pending' },
  { value: 'approved', label: 'Approved' },
  { value: 'restricted', label: 'Restricted' },
  { value: 'inactive', label: 'Inactive (approval)' },
]

const STATUS_OPTIONS = [
  { value: 'active', label: 'Active' },
  { value: 'inactive', label: 'Inactive' },
]

function approvalBadgeClass(status: string): string {
  switch (status) {
    case 'approved':
      return 'bg-emerald-500/20 text-emerald-300 ring-emerald-500/40'
    case 'restricted':
      return 'bg-amber-500/20 text-amber-200 ring-amber-500/40'
    case 'inactive':
      return 'bg-slate-500/20 text-slate-300 ring-slate-500/40'
    default:
      return 'bg-sky-500/20 text-sky-200 ring-sky-500/40'
  }
}

function statusBadgeClass(status: string): string {
  return status === 'active'
    ? 'bg-emerald-500/20 text-emerald-300 ring-emerald-500/40'
    : 'bg-slate-500/20 text-slate-300 ring-slate-500/40'
}

function formatTimestamp(value: string | null | undefined): string | null {
  if (!value) return null
  const date = new Date(value)
  if (Number.isNaN(date.getTime())) return null
  return date.toLocaleString()
}

export function PartyRegistryPanel({
  title,
  partyType,
  parties,
  canManage,
  isLoading,
  partyKey,
  displayName,
  legalName,
  taxIdentifier,
  notes,
  onPartyKeyChange,
  onDisplayNameChange,
  onLegalNameChange,
  onTaxIdentifierChange,
  onNotesChange,
  onCreate,
  isCreating,
  onUpdateParty,
  onUpdateApprovalStatus,
  onUpdateStatus,
  onAddContact,
  isUpdating,
  isUpdatingApproval,
  isUpdatingStatus,
  isAddingContact,
}: PartyRegistryPanelProps) {
  const existingPartyKeys = useMemo(() => parties.map((party) => party.partyKey), [parties])
  const [selectedPartyId, setSelectedPartyId] = useState('')
  const [editDisplayName, setEditDisplayName] = useState('')
  const [editLegalName, setEditLegalName] = useState('')
  const [editTaxIdentifier, setEditTaxIdentifier] = useState('')
  const [editNotes, setEditNotes] = useState('')
  const [editApprovalStatus, setEditApprovalStatus] = useState('pending')
  const [editStatus, setEditStatus] = useState('active')
  const [contactName, setContactName] = useState('')
  const [contactEmail, setContactEmail] = useState('')
  const [contactPhone, setContactPhone] = useState('')
  const [contactRoleLabel, setContactRoleLabel] = useState('')
  const [contactIsPrimary, setContactIsPrimary] = useState(true)

  const selected = parties.find((party) => party.partyId === selectedPartyId) ?? null

  useEffect(() => {
    if (!selected) {
      setEditDisplayName('')
      setEditLegalName('')
      setEditTaxIdentifier('')
      setEditNotes('')
      setEditApprovalStatus('pending')
      setEditStatus('active')
      return
    }

    setEditDisplayName(selected.displayName)
    setEditLegalName(selected.legalName)
    setEditTaxIdentifier(selected.taxIdentifier ?? '')
    setEditNotes(selected.notes)
    setEditApprovalStatus(selected.approvalStatus)
    setEditStatus(selected.status)
  }, [selected])

  useEffect(() => {
    if (selectedPartyId && !parties.some((party) => party.partyId === selectedPartyId)) {
      setSelectedPartyId('')
    }
  }, [parties, selectedPartyId])

  const panelTestId = `party-registry-panel-${partyType}`
  const partyKeyKind = partyType.endsWith('s') ? partyType.slice(0, -1) : partyType

  if (isLoading) {
    return (
      <section
        className="rounded-xl border border-slate-700 bg-slate-900/60 p-5"
        data-testid={panelTestId}
      >
        <p className="text-sm text-slate-400" data-testid="party-registry-loading">
          Loading {title.toLowerCase()}…
        </p>
      </section>
    )
  }

  const createdAtLabel = formatTimestamp(selected?.createdAt)
  const updatedAtLabel = formatTimestamp(selected?.updatedAt)

  return (
    <section
      className="rounded-xl border border-slate-700 bg-slate-900/60 p-5"
      data-testid={panelTestId}
    >
      <h2 className="text-lg font-medium text-white">{title}</h2>
      <ul className="mt-4 space-y-2 text-sm" data-testid="party-registry-list">
        {parties.length === 0 ? (
          <li className="text-slate-400">No records yet.</li>
        ) : (
          parties.map((party) => (
            <li key={party.partyId}>
              <button
                type="button"
                className={`w-full rounded-lg border p-3 text-left transition ${
                  selectedPartyId === party.partyId
                    ? 'border-sky-500 bg-sky-950/30'
                    : 'border-slate-800 hover:border-slate-600'
                }`}
                data-testid={`party-registry-row-${party.partyId}`}
                onClick={() => setSelectedPartyId(party.partyId)}
              >
                <div className="flex items-start justify-between gap-3">
                  <div>
                    <div className="font-medium">{party.displayName}</div>
                    <div className="text-slate-400">{party.partyKey}</div>
                    {party.legalName ? (
                      <div className="mt-1 text-slate-500">{party.legalName}</div>
                    ) : null}
                  </div>
                  <div className="flex flex-col items-end gap-1">
                    <span
                      className={`rounded-full px-2 py-0.5 text-xs ring-1 ${approvalBadgeClass(party.approvalStatus)}`}
                    >
                      {party.approvalStatus}
                    </span>
                    <span
                      className={`rounded-full px-2 py-0.5 text-xs ring-1 ${statusBadgeClass(party.status)}`}
                    >
                      {party.status}
                    </span>
                  </div>
                </div>
                {party.contacts.length > 0 ? (
                  <p className="mt-2 text-slate-400">
                    Primary contact:{' '}
                    {party.contacts.find((c) => c.isPrimary)?.contactName ?? party.contacts[0].contactName}
                  </p>
                ) : null}
              </button>
            </li>
          ))
        )}
      </ul>

      {selected ? (
        <div className="mt-4 rounded-lg border border-slate-800 p-4" data-testid="party-registry-detail">
          <h3 className="text-sm font-medium text-slate-200">{selected.displayName}</h3>
          <p className="mt-1 text-xs text-slate-500">{selected.partyKey}</p>

          <div
            className="mt-3 space-y-1 text-xs text-slate-400"
            data-testid="party-registry-lifecycle-timeline"
          >
            {createdAtLabel ? <p>Registered: {createdAtLabel}</p> : null}
            {updatedAtLabel ? <p>Last updated: {updatedAtLabel}</p> : null}
          </div>

          {selected.contacts.length > 0 ? (
            <ul className="mt-3 space-y-1 text-xs text-slate-400">
              {selected.contacts.map((contact) => (
                <li key={contact.contactId}>
                  {contact.contactName}
                  {contact.roleLabel ? ` · ${contact.roleLabel}` : ''}
                  {contact.isPrimary ? ' · primary' : ''}
                </li>
              ))}
            </ul>
          ) : null}

          {canManage ? (
            <>
              <div className="mt-4 space-y-2 border-t border-slate-800 pt-4" data-testid="party-registry-edit-form">
                <h4 className="text-xs font-medium uppercase tracking-wide text-slate-500">Edit profile</h4>
                <label htmlFor="party-registry-edit-display-name" className="block text-sm text-slate-400">
                  Display name
                  <input
                    id="party-registry-edit-display-name"
                    className="mt-1 w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
                    value={editDisplayName}
                    onChange={(e) => setEditDisplayName(e.target.value)}
                    data-testid="party-registry-edit-display-name"
                  />
                </label>
                <label htmlFor="party-registry-edit-legal-name" className="block text-sm text-slate-400">
                  Legal name
                  <input
                    id="party-registry-edit-legal-name"
                    className="mt-1 w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
                    value={editLegalName}
                    onChange={(e) => setEditLegalName(e.target.value)}
                  />
                </label>
                <label htmlFor="party-registry-edit-tax-id" className="block text-sm text-slate-400">
                  Tax identifier (optional)
                  <input
                    id="party-registry-edit-tax-id"
                    className="mt-1 w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
                    value={editTaxIdentifier}
                    onChange={(e) => setEditTaxIdentifier(e.target.value)}
                  />
                </label>
                <label htmlFor="party-registry-edit-notes" className="block text-sm text-slate-400">
                  Notes
                  <textarea
                    id="party-registry-edit-notes"
                    className="mt-1 w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
                    rows={2}
                    value={editNotes}
                    onChange={(e) => setEditNotes(e.target.value)}
                  />
                </label>
                <button
                  type="button"
                  className="rounded-lg bg-sky-600 px-4 py-2 text-sm font-medium text-white hover:bg-sky-500 disabled:opacity-50"
                  disabled={isUpdating || !editDisplayName.trim()}
                  onClick={() =>
                    onUpdateParty(selected.partyId, {
                      displayName: editDisplayName.trim(),
                      legalName: editLegalName.trim(),
                      taxIdentifier: editTaxIdentifier.trim() || null,
                      notes: editNotes.trim(),
                    })
                  }
                  data-testid="party-registry-save-button"
                >
                  {isUpdating ? 'Saving…' : 'Save profile'}
                </button>
              </div>

              <div className="mt-4 grid gap-3 sm:grid-cols-2">
                <ControlledSelect
                  label="Approval status"
                  id="party-registry-approval-select"
                  value={editApprovalStatus}
                  onChange={setEditApprovalStatus}
                  options={APPROVAL_OPTIONS}
                  testId="party-registry-approval-select"
                />
                <button
                  type="button"
                  className="self-end rounded-lg bg-amber-700 px-4 py-2 text-sm font-medium text-white hover:bg-amber-600 disabled:opacity-50"
                  disabled={
                    isUpdatingApproval || editApprovalStatus === selected.approvalStatus
                  }
                  onClick={() => onUpdateApprovalStatus(selected.partyId, editApprovalStatus)}
                  data-testid="party-registry-approval-save-button"
                >
                  {isUpdatingApproval ? 'Updating…' : 'Update approval'}
                </button>
                <ControlledSelect
                  label="Lifecycle status"
                  id="party-registry-status-select"
                  value={editStatus}
                  onChange={setEditStatus}
                  options={STATUS_OPTIONS}
                  testId="party-registry-status-select"
                />
                <button
                  type="button"
                  className="self-end rounded-lg bg-slate-700 px-4 py-2 text-sm font-medium text-white hover:bg-slate-600 disabled:opacity-50"
                  disabled={isUpdatingStatus || editStatus === selected.status}
                  onClick={() => onUpdateStatus(selected.partyId, editStatus)}
                  data-testid="party-registry-status-save-button"
                >
                  {isUpdatingStatus ? 'Updating…' : 'Update status'}
                </button>
              </div>

              <div
                className="mt-4 space-y-2 border-t border-slate-800 pt-4"
                data-testid="party-registry-contact-form"
              >
                <h4 className="text-xs font-medium uppercase tracking-wide text-slate-500">Add contact</h4>
                <label htmlFor="party-registry-contact-name-input" className="block text-sm text-slate-400">
                  Contact name
                  <input
                    id="party-registry-contact-name-input"
                    className="mt-1 w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
                    value={contactName}
                    onChange={(e) => setContactName(e.target.value)}
                    data-testid="party-registry-contact-name-input"
                  />
                </label>
                <label htmlFor="party-registry-contact-email" className="block text-sm text-slate-400">
                  Email
                  <input
                    id="party-registry-contact-email"
                    className="mt-1 w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
                    value={contactEmail}
                    onChange={(e) => setContactEmail(e.target.value)}
                  />
                </label>
                <label htmlFor="party-registry-contact-phone" className="block text-sm text-slate-400">
                  Phone
                  <input
                    id="party-registry-contact-phone"
                    className="mt-1 w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
                    value={contactPhone}
                    onChange={(e) => setContactPhone(e.target.value)}
                  />
                </label>
                <label htmlFor="party-registry-contact-role" className="block text-sm text-slate-400">
                  Role label
                  <input
                    id="party-registry-contact-role"
                    className="mt-1 w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
                    value={contactRoleLabel}
                    onChange={(e) => setContactRoleLabel(e.target.value)}
                  />
                </label>
                <label htmlFor="party-registry-contact-primary" className="flex items-center gap-2 text-sm text-slate-400">
                  <input
                    id="party-registry-contact-primary"
                    type="checkbox"
                    checked={contactIsPrimary}
                    onChange={(e) => setContactIsPrimary(e.target.checked)}
                  />
                  Primary contact
                </label>
                <button
                  type="button"
                  className="rounded-lg bg-emerald-700 px-4 py-2 text-sm font-medium text-white hover:bg-emerald-600 disabled:opacity-50"
                  disabled={isAddingContact || !contactName.trim()}
                  onClick={() => {
                    onAddContact(selected.partyId, {
                      contactName: contactName.trim(),
                      email: contactEmail.trim(),
                      phone: contactPhone.trim(),
                      roleLabel: contactRoleLabel.trim(),
                      isPrimary: contactIsPrimary,
                    })
                    setContactName('')
                    setContactEmail('')
                    setContactPhone('')
                    setContactRoleLabel('')
                    setContactIsPrimary(true)
                  }}
                  data-testid="party-registry-contact-add-button"
                >
                  {isAddingContact ? 'Adding…' : 'Add contact'}
                </button>
              </div>
            </>
          ) : null}
        </div>
      ) : null}

      {canManage ? (
        <div className="mt-4 space-y-2 border-t border-slate-800 pt-4" data-testid="party-registry-create-form">
          <h3 className="text-sm font-medium text-slate-200">New {title.slice(0, -1).toLowerCase()}</h3>
          <label htmlFor="party-registry-create-display-name" className="block text-sm text-slate-400">
            Display name
            <input
              id="party-registry-create-display-name"
              className="mt-1 w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
              value={displayName}
              onChange={(e) => onDisplayNameChange(e.target.value)}
            />
          </label>
          <GeneratedKeyFieldGroup
            sourceLabel={displayName}
            existingKeys={existingPartyKeys}
            onKeyChange={onPartyKeyChange}
            domain="vendor"
            kind={partyKeyKind}
            label="Party key"
          />
          <label htmlFor="party-registry-create-legal-name" className="block text-sm text-slate-400">
            Legal name
            <input
              id="party-registry-create-legal-name"
              className="mt-1 w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
              value={legalName}
              onChange={(e) => onLegalNameChange(e.target.value)}
            />
          </label>
          <label htmlFor="party-registry-create-tax-id" className="block text-sm text-slate-400">
            Tax identifier (optional)
            <input
              id="party-registry-create-tax-id"
              className="mt-1 w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
              value={taxIdentifier}
              onChange={(e) => onTaxIdentifierChange(e.target.value)}
            />
          </label>
          <label htmlFor="party-registry-create-notes" className="block text-sm text-slate-400">
            Notes
            <textarea
              id="party-registry-create-notes"
              className="mt-1 w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
              rows={2}
              value={notes}
              onChange={(e) => onNotesChange(e.target.value)}
            />
          </label>
          <button
            type="button"
            className="rounded-lg bg-sky-600 px-4 py-2 text-sm font-medium text-white hover:bg-sky-500 disabled:opacity-50"
            disabled={isCreating || !partyKey.trim() || !displayName.trim()}
            onClick={onCreate}
            data-testid="party-registry-create-button"
          >
            {isCreating ? 'Creating…' : `Add ${title.slice(0, -1).toLowerCase()}`}
          </button>
        </div>
      ) : null}
    </section>
  )
}
