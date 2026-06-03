import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'

import { getVendorEmailInbox, ingestVendorEmailInbox } from '../api/client'
import type { IngestVendorEmailInboxResponse, VendorEmailInboxMessageResponse } from '../api/types'

interface VendorEmailInboxPanelProps {
  accessToken: string
  canManage: boolean
}

const messageKinds = [
  { value: 'quote_received', label: 'Quote received' },
  { value: 'order_confirmation_received', label: 'Order confirmation received' },
]

function formatDate(value: string) {
  return new Date(value).toLocaleString()
}

function statusTone(status: string) {
  return status === 'matched'
    ? 'border-emerald-500/30 bg-emerald-500/10 text-emerald-700 dark:text-emerald-300'
    : 'border-amber-500/30 bg-amber-500/10 text-amber-700 dark:text-amber-300'
}

function MessageCard({ message }: { message: VendorEmailInboxMessageResponse }) {
  return (
    <li className="rounded border border-border bg-muted/20 p-3 text-xs">
      <div className="flex flex-wrap items-start justify-between gap-2">
        <div>
          <div className="font-medium text-foreground">{message.subject}</div>
          <div className="text-muted-foreground">
            {message.senderName} &lt;{message.senderEmail}&gt; · {message.messageKind.replaceAll('_', ' ')}
          </div>
        </div>
        <span className={`rounded-full border px-2 py-0.5 text-[10px] font-semibold uppercase ${statusTone(message.matchStatus)}`}>
          {message.matchStatus}
        </span>
      </div>
      <div className="mt-2 flex flex-wrap gap-2 text-[11px] text-muted-foreground">
        <span>Received {formatDate(message.receivedAt)}</span>
        {message.vendorPartyKey ? <span>Vendor {message.vendorPartyKey}</span> : null}
        {message.linkedReferenceType && message.linkedReferenceKey ? (
          <span>
            Linked {message.linkedReferenceType.toUpperCase()} {message.linkedReferenceKey}
          </span>
        ) : null}
      </div>
      <p className="mt-2 whitespace-pre-wrap text-muted-foreground">{message.bodyPreview}</p>
      <p className="mt-2 text-[11px] text-muted-foreground">{message.matchReason}</p>
    </li>
  )
}

export function VendorEmailInboxPanel({ accessToken, canManage }: VendorEmailInboxPanelProps) {
  const queryClient = useQueryClient()
  const [messageKey, setMessageKey] = useState('')
  const [messageKind, setMessageKind] = useState('quote_received')
  const [senderEmail, setSenderEmail] = useState('')
  const [senderName, setSenderName] = useState('')
  const [subject, setSubject] = useState('')
  const [referenceKey, setReferenceKey] = useState('')
  const [body, setBody] = useState('')
  const [lastResult, setLastResult] = useState<IngestVendorEmailInboxResponse | null>(null)

  const inboxQuery = useQuery({
    queryKey: ['supplyarr-vendor-email-inbox', accessToken],
    queryFn: () => getVendorEmailInbox(accessToken, 25),
    enabled: canManage,
  })

  const ingestMutation = useMutation({
    mutationFn: () =>
      ingestVendorEmailInbox(accessToken, {
        messageKey,
        messageKind,
        senderEmail,
        senderName,
        subject,
        body,
        referenceKey: referenceKey.trim() || null,
      }),
    onSuccess: async (result) => {
      setLastResult(result)
      setMessageKey('')
      setMessageKind('quote_received')
      setSenderEmail('')
      setSenderName('')
      setSubject('')
      setReferenceKey('')
      setBody('')
      await queryClient.invalidateQueries({ queryKey: ['supplyarr-vendor-email-inbox', accessToken] })
    },
  })

  if (!canManage) {
    return null
  }

  const messages = inboxQuery.data?.items ?? []
  const matchedCount = messages.filter((message) => message.matchStatus === 'matched').length
  const unmatchedCount = messages.filter((message) => message.matchStatus !== 'matched').length

  return (
    <section className="rounded-lg border border-border bg-card p-4 shadow-sm" data-testid="vendor-email-inbox-panel">
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div>
          <h3 className="text-sm font-semibold text-foreground">Vendor email inbox</h3>
          <p className="mt-1 text-xs text-muted-foreground">
            Capture quote and order-confirmation emails, then auto-link them to RFQs or purchase orders.
          </p>
        </div>
        <div className="flex gap-2 text-[11px] text-muted-foreground">
          <span className="rounded-full border border-border px-2 py-1">Matched {matchedCount}</span>
          <span className="rounded-full border border-border px-2 py-1">Needs review {unmatchedCount}</span>
        </div>
      </div>

      <div className="mt-4 grid gap-3 md:grid-cols-2">
        <label className="text-sm">
          Message key
          <input
            className="mt-1 w-full rounded border border-input bg-background px-2 py-1"
            value={messageKey}
            onChange={(event) => setMessageKey(event.target.value)}
            placeholder="vendor-mail-001"
          />
        </label>
        <label className="text-sm">
          Message kind
          <select
            className="mt-1 w-full rounded border border-input bg-background px-2 py-1"
            value={messageKind}
            onChange={(event) => setMessageKind(event.target.value)}
          >
            {messageKinds.map((option) => (
              <option key={option.value} value={option.value}>
                {option.label}
              </option>
            ))}
          </select>
        </label>
        <label className="text-sm">
          Sender email
          <input
            className="mt-1 w-full rounded border border-input bg-background px-2 py-1"
            value={senderEmail}
            onChange={(event) => setSenderEmail(event.target.value)}
            placeholder="vendor@example.com"
          />
        </label>
        <label className="text-sm">
          Sender name
          <input
            className="mt-1 w-full rounded border border-input bg-background px-2 py-1"
            value={senderName}
            onChange={(event) => setSenderName(event.target.value)}
            placeholder="Vendor Logistics"
          />
        </label>
        <label className="text-sm md:col-span-2">
          Subject
          <input
            className="mt-1 w-full rounded border border-input bg-background px-2 py-1"
            value={subject}
            onChange={(event) => setSubject(event.target.value)}
            placeholder="RFQ-2026-001 quote confirmation"
          />
        </label>
        <label className="text-sm md:col-span-2">
          Reference key
          <input
            className="mt-1 w-full rounded border border-input bg-background px-2 py-1"
            value={referenceKey}
            onChange={(event) => setReferenceKey(event.target.value)}
            placeholder="RFQ-2026-001 or PO-2026-004"
          />
        </label>
        <label className="text-sm md:col-span-2">
          Email body
          <textarea
            className="mt-1 min-h-28 w-full rounded border border-input bg-background px-2 py-1"
            value={body}
            onChange={(event) => setBody(event.target.value)}
            placeholder="Paste the email body here"
          />
        </label>
      </div>

      <button
        type="button"
        className="mt-4 rounded bg-primary px-3 py-1.5 text-sm text-primary-foreground disabled:opacity-50"
        disabled={ingestMutation.isPending}
        onClick={() => ingestMutation.mutate()}
      >
        {ingestMutation.isPending ? 'Ingesting…' : 'Ingest email'}
      </button>

      {lastResult ? (
        <p className="mt-2 text-xs text-muted-foreground">
          Last ingest {lastResult.wasDuplicate ? 'matched an existing message' : 'created a new inbox record'} ·{' '}
          {lastResult.message.matchStatus}
        </p>
      ) : null}

      <div className="mt-6">
        <h4 className="text-xs font-medium uppercase text-muted-foreground">Recent messages</h4>
        <ul className="mt-2 space-y-2">
          {messages.map((message) => (
            <MessageCard key={message.messageId} message={message} />
          ))}
          {messages.length === 0 ? <li className="text-sm text-muted-foreground">No vendor email messages yet.</li> : null}
        </ul>
      </div>
    </section>
  )
}
