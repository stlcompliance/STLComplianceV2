import { PlatformOutboxPublisherPanel } from '../../components/platform-admin/PlatformOutboxPublisherPanel'

export function PlatformOutboxPage() {
  return (
    <div className="space-y-6">
      <div>
        <h4 className="text-lg font-semibold text-[var(--color-text-primary)]">Platform event outbox</h4>
        <p className="mt-1 text-sm text-[var(--color-text-muted)]">
          Integration events for tenant and entitlement changes — published by{' '}
          <code className="text-xs">nexarr-worker</code> for downstream product mirrors.
        </p>
      </div>
      <PlatformOutboxPublisherPanel />
    </div>
  )
}
