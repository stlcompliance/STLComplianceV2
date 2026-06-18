import { Moon, Sun } from 'lucide-react'
import type { StlThemeMode } from './theme'

export function ThemeToggleButton({
  theme,
  onToggle,
}: {
  theme: StlThemeMode
  onToggle: () => void
}) {
  const nextTheme = theme === 'dark' ? 'light' : 'dark'
  const Icon = theme === 'dark' ? Sun : Moon
  const label = `Switch to ${nextTheme} mode`

  return (
    <button
      type="button"
      onClick={onToggle}
      title={label}
      aria-label={label}
      className="inline-flex h-9 w-9 items-center justify-center rounded-lg border border-[var(--color-border-strong)] bg-[var(--color-bg-control)] text-[var(--color-text-primary)] transition hover:border-[var(--color-accent-border)] hover:bg-[var(--color-bg-control-hover)] focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-[var(--color-focus-ring)]"
    >
      <Icon className="h-4 w-4 shrink-0 text-[var(--color-accent)]" aria-hidden />
    </button>
  )
}
