import { Bot, Send, X } from 'lucide-react'
import { useEffect, useMemo, useRef, useState } from 'react'
import type { FormEvent, KeyboardEvent } from 'react'
import { useHintsPreference } from './HintsPreferenceContext'

export type AiHelpMessage = {
  id: string
  role: 'user' | 'assistant'
  text: string
  outcome?: string
}

export type AiHelpDrawerProps = {
  open: boolean
  title?: string
  productKey: string
  route: string
  messages: AiHelpMessage[]
  isSending?: boolean
  errorMessage?: string | null
  onClose: () => void
  onSend: (message: string) => Promise<void> | void
}

const httpUrlPattern = /https?:\/\/[^\s<]+[^\s<.,;:!?)\]}]/gi

function MessageText({ text }: { text: string }) {
  const parts: Array<string | { href: string; text: string }> = []
  let lastIndex = 0

  for (const match of text.matchAll(httpUrlPattern)) {
    const href = match[0]
    const index = match.index ?? 0
    if (index > lastIndex) {
      parts.push(text.slice(lastIndex, index))
    }
    parts.push({ href, text: href })
    lastIndex = index + href.length
  }

  if (lastIndex < text.length) {
    parts.push(text.slice(lastIndex))
  }

  return (
    <p className="whitespace-pre-wrap">
      {parts.map((part, index) =>
        typeof part === 'string' ? (
          part
        ) : (
          <a
            key={`${part.href}-${index}`}
            href={part.href}
            target="_blank"
            rel="noreferrer"
            className="break-words text-[var(--color-accent)] underline decoration-[var(--color-accent-border)] underline-offset-2 hover:text-[var(--color-accent-hover)]"
          >
            {part.text}
          </a>
        ),
      )}
    </p>
  )
}

export function AiHelpButton({
  onClick,
  label = 'Show hints',
}: {
  onClick: () => void
  label?: string
}) {
  return (
    <button
      type="button"
      title={label}
      aria-label={label}
      onClick={onClick}
      className="inline-flex h-9 w-9 items-center justify-center rounded-md border border-[var(--color-border-strong)] bg-[var(--color-bg-control)] text-[var(--color-text-primary)] hover:border-[var(--color-accent-border)] hover:bg-[var(--color-bg-control-hover)]"
    >
      <Bot className="h-4 w-4" aria-hidden />
    </button>
  )
}

export function AiHelpDrawer({
  open,
  title = 'AI assistance',
  productKey,
  route,
  messages,
  isSending = false,
  errorMessage = null,
  onClose,
  onSend,
}: AiHelpDrawerProps) {
  const [draft, setDraft] = useState('')
  const scrollAnchorRef = useRef<HTMLDivElement>(null)
  const canSend = draft.trim().length > 0 && !isSending
  const status = useMemo(() => `${productKey} · ${route}`, [productKey, route])
  const { showHints } = useHintsPreference()

  useEffect(() => {
    if (!open) return

    scrollAnchorRef.current?.scrollIntoView?.({ block: 'end' })
  }, [messages, open, isSending])

  const submitDraft = async () => {
    if (!canSend) return

    const next = draft.trim()
    setDraft('')
    await onSend(next)
  }

  const handleSubmit = async (event: FormEvent) => {
    event.preventDefault()
    await submitDraft()
  }

  const handleDraftKeyDown = (event: KeyboardEvent<HTMLTextAreaElement>) => {
    if (
      event.key !== 'Enter' ||
      event.shiftKey ||
      event.altKey ||
      event.ctrlKey ||
      event.metaKey ||
      event.nativeEvent.isComposing
    ) {
      return
    }

    event.preventDefault()
    void submitDraft()
  }

  if (!open) return null

  return (
    <div className="fixed inset-0 z-50 flex justify-end bg-[var(--color-overlay-scrim)]">
      <aside className="flex h-full w-full max-w-xl flex-col border-l border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] text-[var(--color-text-primary)] shadow-2xl">
        <header className="flex items-center justify-between border-b border-[var(--color-border-subtle)] px-4 py-3">
          <div className="min-w-0">
            <h2 className="truncate text-sm font-semibold text-[var(--color-text-primary)]">{title}</h2>
            <p className="truncate text-xs text-[var(--color-text-muted)]">{status}</p>
          </div>
          <button
            type="button"
            title="Close"
            aria-label="Close"
            onClick={onClose}
            className="inline-flex h-8 w-8 items-center justify-center rounded-md text-[var(--color-text-secondary)] hover:bg-[var(--color-bg-control-hover)] hover:text-[var(--color-text-primary)]"
          >
            <X className="h-4 w-4" aria-hidden />
          </button>
        </header>

        <div className="min-h-0 flex-1 space-y-3 overflow-auto p-4">
          {messages.length === 0 ? (
            <div className="rounded-md border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-elevated)] p-4 text-sm text-[var(--color-text-secondary)]">
              {showHints
                ? 'Ask about the current page, validation errors, workflow next steps, or import review.'
                : 'Hints are hidden. Use the topbar toggle to show optional guidance.'}
            </div>
          ) : (
            messages.map((message) => (
              <div
                key={message.id}
                className={[
                  'rounded-md border p-3 text-sm leading-6',
                  message.role === 'user'
                    ? 'ml-10 border-[var(--color-accent-border)] bg-[var(--color-accent-soft)] text-[var(--color-text-primary)]'
                    : 'mr-10 border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-elevated)] text-[var(--color-text-primary)]',
                ].join(' ')}
              >
                <MessageText text={message.text} />
                {message.outcome && message.outcome !== 'success' ? (
                  <p className="mt-2 text-xs text-[var(--color-warning-text)]">{message.outcome}</p>
                ) : null}
              </div>
            ))
          )}
          <div ref={scrollAnchorRef} aria-hidden />
        </div>

        <form onSubmit={handleSubmit} className="border-t border-[var(--color-border-subtle)] p-4">
          {errorMessage ? (
            <p className="mb-2 rounded-md border border-[var(--color-destructive-border)] bg-[var(--color-destructive-bg)] px-3 py-2 text-sm text-[var(--color-destructive-text)]">
              {errorMessage}
            </p>
          ) : null}
          <div className="flex items-end gap-2">
            <label className="sr-only" htmlFor="ai-help-message">
              Message
            </label>
            <textarea
              id="ai-help-message"
              value={draft}
              onChange={(event) => setDraft(event.target.value)}
              onKeyDown={handleDraftKeyDown}
              rows={3}
              className="min-h-[88px] flex-1 resize-none rounded-md border border-[var(--color-border-subtle)] bg-[var(--color-bg-control)] px-3 py-2 text-sm text-[var(--color-text-primary)] outline-none focus:border-[var(--color-accent-border)]"
            />
            <button
              type="submit"
              title="Send"
              aria-label="Send"
              disabled={!canSend}
              className="inline-flex h-10 w-10 items-center justify-center rounded-md bg-[var(--color-accent)] text-[var(--color-on-accent)] hover:bg-[var(--color-accent-hover)] disabled:cursor-not-allowed disabled:bg-[var(--color-bg-surface-elevated)] disabled:text-[var(--color-text-disabled)]"
            >
              <Send className="h-4 w-4" aria-hidden />
            </button>
          </div>
        </form>
      </aside>
    </div>
  )
}
