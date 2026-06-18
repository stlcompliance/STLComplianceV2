import { ChevronDown, Settings2, UserRound, LogOut } from 'lucide-react'
import { useEffect, useId, useMemo, useRef, useState } from 'react'
import { Link } from 'react-router-dom'

export type AccountMenuPopoverProps = {
  displayName: string
  subtitle?: string
  preferencesHref: string
  onSignOut?: () => void
  className?: string
}

function getInitials(displayName: string): string {
  const parts = displayName
    .trim()
    .split(/\s+/)
    .filter(Boolean)
    .slice(0, 2)
  if (parts.length === 0) {
    return 'U'
  }
  return parts
    .map((part) => part.charAt(0).toUpperCase())
    .join('')
}

export function AccountMenuPopover({
  displayName,
  subtitle,
  preferencesHref,
  onSignOut,
  className = '',
}: AccountMenuPopoverProps) {
  const [open, setOpen] = useState(false)
  const containerRef = useRef<HTMLDivElement>(null)
  const menuId = useId()
  const initials = useMemo(() => getInitials(displayName), [displayName])

  useEffect(() => {
    if (!open) {
      return
    }

    function handlePointerDown(event: MouseEvent) {
      if (!containerRef.current?.contains(event.target as Node)) {
        setOpen(false)
      }
    }

    function handleEscape(event: KeyboardEvent) {
      if (event.key === 'Escape') {
        setOpen(false)
      }
    }

    document.addEventListener('mousedown', handlePointerDown)
    document.addEventListener('keydown', handleEscape)
    return () => {
      document.removeEventListener('mousedown', handlePointerDown)
      document.removeEventListener('keydown', handleEscape)
    }
  }, [open])

  return (
    <div ref={containerRef} className={['relative max-w-full', className].join(' ').trim()}>
      <button
        type="button"
        aria-haspopup="menu"
        aria-expanded={open}
        aria-controls={menuId}
        onClick={() => setOpen((value) => !value)}
        className="inline-flex max-w-[16rem] items-center gap-2 rounded-lg border border-[var(--color-border-strong)] bg-[var(--color-bg-control)] px-3 py-2 text-left text-sm text-[var(--color-text-primary)] transition hover:border-[var(--color-accent-border)] hover:bg-[var(--color-bg-control-hover)] focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-[var(--color-focus-ring)]"
      >
        <span className="inline-flex h-8 w-8 shrink-0 items-center justify-center rounded-full bg-[var(--color-accent-soft)] text-xs font-semibold text-[var(--color-accent)]">
          {initials}
        </span>
        <span className="min-w-0 text-left">
          <span className="block truncate font-medium">{displayName}</span>
          {subtitle ? (
            <span className="block truncate text-xs text-[var(--color-text-muted)]">{subtitle}</span>
          ) : null}
        </span>
        <ChevronDown
          className={[
            'h-4 w-4 shrink-0 text-[var(--color-text-muted)] transition-transform',
            open ? 'rotate-180' : '',
          ].join(' ')}
          aria-hidden
        />
      </button>

      {open ? (
        <div
          id={menuId}
          role="menu"
          aria-label="Account menu"
          className="absolute right-0 z-50 mt-2 w-64 max-w-[calc(100vw-2rem)] overflow-hidden rounded-lg border border-[var(--color-border-strong)] bg-[var(--color-bg-shell)] py-1 shadow-xl [box-shadow:var(--shadow-shell-menu)]"
        >
          <div className="border-b border-[var(--color-border-subtle)] px-3 py-2">
            <p className="flex items-center gap-2 text-sm font-medium text-[var(--color-text-primary)]">
              <UserRound className="h-4 w-4 shrink-0 text-[var(--color-accent)]" aria-hidden />
              Account
            </p>
            {subtitle ? (
              <p className="mt-1 truncate text-xs text-[var(--color-text-muted)]">{subtitle}</p>
            ) : null}
          </div>
          <Link
            role="menuitem"
            to={preferencesHref}
            onClick={() => setOpen(false)}
            className="flex items-center gap-2 px-3 py-2 text-sm text-[var(--color-text-secondary)] transition-colors hover:bg-[var(--color-bg-control-hover)] hover:text-[var(--color-text-primary)] focus-visible:bg-[var(--color-bg-control-hover)] focus-visible:text-[var(--color-text-primary)]"
          >
            <Settings2 className="h-4 w-4 shrink-0 text-[var(--color-accent)]" aria-hidden />
            Preferences
          </Link>
          {onSignOut ? (
            <button
              type="button"
              role="menuitem"
              onClick={() => {
                setOpen(false)
                onSignOut()
              }}
              className="flex w-full items-center gap-2 px-3 py-2 text-left text-sm text-[var(--color-text-secondary)] transition-colors hover:bg-[var(--color-bg-control-hover)] hover:text-[var(--color-text-primary)] focus-visible:bg-[var(--color-bg-control-hover)] focus-visible:text-[var(--color-text-primary)]"
            >
              <LogOut className="h-4 w-4 shrink-0 text-[var(--color-accent)]" aria-hidden />
              Sign out
            </button>
          ) : null}
        </div>
      ) : null}
    </div>
  )
}
