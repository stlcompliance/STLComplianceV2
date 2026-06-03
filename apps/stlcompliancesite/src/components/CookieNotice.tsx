import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'

const COOKIE_NOTICE_STORAGE_KEY = 'stl-cookie-notice-dismissed'

export function CookieNotice() {
  const [isVisible, setIsVisible] = useState(false)

  useEffect(() => {
    setIsVisible(window.localStorage.getItem(COOKIE_NOTICE_STORAGE_KEY) !== 'true')
  }, [])

  function dismissNotice() {
    window.localStorage.setItem(COOKIE_NOTICE_STORAGE_KEY, 'true')
    setIsVisible(false)
  }

  if (!isVisible) {
    return null
  }

  return (
    <section
      aria-label="Cookie notice"
      className="fixed inset-x-0 bottom-0 z-50 border-t border-slate-700 bg-slate-950/95 px-4 py-4 shadow-2xl shadow-slate-950/40 backdrop-blur sm:px-6"
      data-testid="cookie-notice"
    >
      <div className="mx-auto flex max-w-6xl flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
        <p className="max-w-4xl text-sm leading-6 text-slate-200">
          STL Compliance uses necessary cookies for login, security, and session management. We may
          also use limited usage data to fix bugs, improve features, monitor reliability, and
          conduct internal audits. See our{' '}
          <Link className="font-semibold text-teal-300 hover:text-teal-200" to="/privacy">
            Privacy Policy
          </Link>{' '}
          for details.
        </p>
        <button
          type="button"
          className="inline-flex min-h-11 items-center justify-center rounded-lg bg-teal-600 px-5 py-2 text-sm font-semibold text-white hover:bg-teal-500"
          onClick={dismissNotice}
        >
          Got it
        </button>
      </div>
    </section>
  )
}
