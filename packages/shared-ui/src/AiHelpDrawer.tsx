import { Bot, Send, X } from 'lucide-react'
import { useMemo, useState } from 'react'
import type { FormEvent } from 'react'

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

export function AiHelpButton({
  onClick,
  label = 'AI help',
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
      className="inline-flex h-9 w-9 items-center justify-center rounded-md border border-slate-600 bg-slate-900/70 text-slate-100 hover:border-teal-400/60 hover:bg-slate-800"
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
  const canSend = draft.trim().length > 0 && !isSending
  const status = useMemo(() => `${productKey} · ${route}`, [productKey, route])

  const handleSubmit = async (event: FormEvent) => {
    event.preventDefault()
    if (!canSend) return
    const next = draft.trim()
    setDraft('')
    await onSend(next)
  }

  if (!open) return null

  return (
    <div className="fixed inset-0 z-50 flex justify-end bg-black/40">
      <aside className="flex h-full w-full max-w-xl flex-col border-l border-slate-700 bg-[#0b1120] text-slate-100 shadow-2xl">
        <header className="flex items-center justify-between border-b border-slate-700 px-4 py-3">
          <div className="min-w-0">
            <h2 className="truncate text-sm font-semibold text-white">{title}</h2>
            <p className="truncate text-xs text-slate-400">{status}</p>
          </div>
          <button
            type="button"
            title="Close"
            aria-label="Close"
            onClick={onClose}
            className="inline-flex h-8 w-8 items-center justify-center rounded-md text-slate-300 hover:bg-slate-800 hover:text-white"
          >
            <X className="h-4 w-4" aria-hidden />
          </button>
        </header>

        <div className="min-h-0 flex-1 space-y-3 overflow-auto p-4">
          {messages.length === 0 ? (
            <div className="rounded-md border border-slate-700 bg-slate-900/70 p-4 text-sm text-slate-300">
              Ask about the current page, validation errors, workflow next steps, or import review.
            </div>
          ) : (
            messages.map((message) => (
              <div
                key={message.id}
                className={[
                  'rounded-md border p-3 text-sm leading-6',
                  message.role === 'user'
                    ? 'ml-10 border-teal-500/30 bg-teal-500/10 text-teal-50'
                    : 'mr-10 border-slate-700 bg-slate-900/80 text-slate-100',
                ].join(' ')}
              >
                <p className="whitespace-pre-wrap">{message.text}</p>
                {message.outcome && message.outcome !== 'success' ? (
                  <p className="mt-2 text-xs text-amber-300">{message.outcome}</p>
                ) : null}
              </div>
            ))
          )}
        </div>

        <form onSubmit={handleSubmit} className="border-t border-slate-700 p-4">
          {errorMessage ? (
            <p className="mb-2 rounded-md border border-red-500/30 bg-red-500/10 px-3 py-2 text-sm text-red-100">
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
              rows={3}
              className="min-h-[88px] flex-1 resize-none rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-white outline-none focus:border-teal-400"
            />
            <button
              type="submit"
              title="Send"
              aria-label="Send"
              disabled={!canSend}
              className="inline-flex h-10 w-10 items-center justify-center rounded-md bg-teal-500 text-slate-950 hover:bg-teal-400 disabled:cursor-not-allowed disabled:bg-slate-700 disabled:text-slate-400"
            >
              <Send className="h-4 w-4" aria-hidden />
            </button>
          </div>
        </form>
      </aside>
    </div>
  )
}
