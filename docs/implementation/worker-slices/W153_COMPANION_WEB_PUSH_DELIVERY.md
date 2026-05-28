# W153 — Companion Web Push subscription and delivery (M11)

**Milestone:** M11 (Companion)  
**Products:** NexArr, companion-frontend  
**Status:** Complete

## Summary

Delivers browser Web Push for Companion operational notifications. NexArr stores per-user push subscriptions, exposes VAPID public key to the companion app, and sends push payloads during notification dispatch (alongside optional webhooks from W131).

## Backend (NexArr)

- `nexarr_companion_push_subscriptions` — tenant-scoped endpoint + p256dh/auth keys per user
- `GET /api/companion/push/vapid-public-key` — public VAPID key when configured
- `POST /api/companion/push/subscribe` — upsert subscription (companion JWT + entitlement)
- `DELETE /api/companion/push/subscribe` — remove subscription by endpoint
- `CompanionWebPushSender` — WebPush library + VAPID signing
- `CompanionNotificationDispatchService` — push delivery on dispatch batch; stale 404/410 subscriptions pruned
- `CompanionNotificationEnqueueService` / `CompanionNotificationRules` — enqueue when webhook **or** push subscription exists
- Migration: `20260528080528_NexArrCompanionPushSubscriptions`

## Frontend (companion)

- `public/sw.js` — push event handler, notification click focus
- `pushNotifications.ts` — service worker registration, VAPID subscribe, NexArr sync
- `useCompanionWebPush` — auto-sync on session bootstrap
- `NotificationSettingsPanel` — push permission + subscription status UI

## Configuration

| Variable | Service | Notes |
|----------|---------|-------|
| `CompanionWebPush__Subject` | `nexarr-api` | VAPID subject (`mailto:…`) |
| `CompanionWebPush__PublicKey` | `nexarr-api` | Dashboard secret (`sync: false`) |
| `CompanionWebPush__PrivateKey` | `nexarr-api` | Dashboard secret (`sync: false`) |

See `docs/deployment/ENV_VARS_V1.md`.

## Tests

- `NexArrCompanionWebPushTests` — subscribe/unsubscribe persistence, field-inbox dispatch → push send
- `pushNotifications.test.ts` — readiness labels and support detection
- `NotificationSettingsPanel.test.tsx` — push readiness surface for admins

## Permissions

- Companion JWT + entitlement (`RequireCompanionAccess`) on push subscribe/unsubscribe
- VAPID public key requires authenticated companion session

## Next gaps

- Native mobile push (FCM/APNs) — browser Web Push only
- Playwright live push delivery (requires browser push permission + VAPID in E2E env)
- Additional offline action kinds queued for sync (evidence)
