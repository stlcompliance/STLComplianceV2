import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useRef,
  useState,
  type ReactNode,
} from 'react'
import { AlertCircle, CheckCircle2, Info, X } from 'lucide-react'

export type ToastVariant = 'success' | 'error' | 'info'

export type ToastInput = {
  message: string
  variant?: ToastVariant
  durationMs?: number
}

type ToastRecord = ToastInput & {
  id: string
}

type ToastContextValue = {
  pushToast: (toast: ToastInput) => void
}

const ToastContext = createContext<ToastContextValue | null>(null)

const DEFAULT_DURATION_MS = 5000

function toastVariantClass(variant: ToastVariant): string {
  if (variant === 'success') {
    return 'border-emerald-800/60 bg-emerald-950/90 text-emerald-100'
  }
  if (variant === 'error') {
    return 'border-red-800/60 bg-red-950/90 text-red-100'
  }
  return 'border-slate-700 bg-slate-900/95 text-slate-100'
}

function ToastIcon({ variant }: { variant: ToastVariant }) {
  if (variant === 'success') {
    return <CheckCircle2 className="h-4 w-4 shrink-0 text-emerald-400" aria-hidden />
  }
  if (variant === 'error') {
    return <AlertCircle className="h-4 w-4 shrink-0 text-red-400" aria-hidden />
  }
  return <Info className="h-4 w-4 shrink-0 text-slate-400" aria-hidden />
}

function ToastViewport({
  toasts,
  onDismiss,
}: {
  toasts: ToastRecord[]
  onDismiss: (id: string) => void
}) {
  if (toasts.length === 0) {
    return null
  }

  return (
    <div
      aria-live="polite"
      aria-relevant="additions"
      className="pointer-events-none fixed right-4 top-4 z-[100] flex w-full max-w-sm flex-col gap-2"
    >
      {toasts.map((toast) => (
        <div
          key={toast.id}
          role="status"
          className={[
            'pointer-events-auto flex items-start gap-2 rounded-lg border px-3 py-2 text-sm shadow-lg',
            toastVariantClass(toast.variant ?? 'info'),
          ].join(' ')}
        >
          <ToastIcon variant={toast.variant ?? 'info'} />
          <p className="min-w-0 flex-1">{toast.message}</p>
          <button
            type="button"
            aria-label="Dismiss notification"
            onClick={() => onDismiss(toast.id)}
            className="shrink-0 rounded p-0.5 opacity-70 hover:opacity-100"
          >
            <X className="h-4 w-4" aria-hidden />
          </button>
        </div>
      ))}
    </div>
  )
}

export function ToastProvider({ children }: { children: ReactNode }) {
  const [toasts, setToasts] = useState<ToastRecord[]>([])
  const timersRef = useRef<Map<string, ReturnType<typeof setTimeout>>>(new Map())

  const dismissToast = useCallback((id: string) => {
    const timer = timersRef.current.get(id)
    if (timer) {
      clearTimeout(timer)
      timersRef.current.delete(id)
    }
    setToasts((current) => current.filter((toast) => toast.id !== id))
  }, [])

  const pushToast = useCallback(
    ({ message, variant = 'info', durationMs = DEFAULT_DURATION_MS }: ToastInput) => {
      const id = crypto.randomUUID()
      setToasts((current) => [...current, { id, message, variant, durationMs }])

      const timer = setTimeout(() => dismissToast(id), durationMs)
      timersRef.current.set(id, timer)
    },
    [dismissToast],
  )

  useEffect(() => {
    const timers = timersRef.current
    return () => {
      for (const timer of timers.values()) {
        clearTimeout(timer)
      }
      timers.clear()
    }
  }, [])

  const value = useMemo(() => ({ pushToast }), [pushToast])

  return (
    <ToastContext.Provider value={value}>
      {children}
      <ToastViewport toasts={toasts} onDismiss={dismissToast} />
    </ToastContext.Provider>
  )
}

export function useToast(): ToastContextValue {
  const context = useContext(ToastContext)
  if (!context) {
    throw new Error('useToast must be used within ToastProvider')
  }
  return context
}
