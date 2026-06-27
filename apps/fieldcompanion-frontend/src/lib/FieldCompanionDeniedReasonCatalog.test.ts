import { describe, expect, it } from 'vitest'

import {
  formatBlockedTaskReason,
  formatInboxSourceError,
  reasonCodeToPlainMessage,
  resolveDeniedReason,
} from './FieldCompanionDeniedReasonCatalog'
import {
  FieldCompanionFieldValidationReasonCodes,
  FieldCompanionLaunchDenialCodes,
  FieldCompanionScanReasonCodes,
} from './FieldCompanionValidationReasonCodes'

describe('FieldCompanionDeniedReasonCatalog', () => {
  it('maps field validation codes to plain language', () => {
    expect(reasonCodeToPlainMessage(FieldCompanionFieldValidationReasonCodes.NotInInbox)).toContain(
      'field inbox',
    )
    expect(reasonCodeToPlainMessage(FieldCompanionFieldValidationReasonCodes.DvirUnsupported)).toContain(
      'RoutArr',
    )
  })

  it('maps scan and launch denial codes', () => {
    expect(reasonCodeToPlainMessage(FieldCompanionScanReasonCodes.InvalidPayload)).toContain('scan')
    expect(reasonCodeToPlainMessage(FieldCompanionLaunchDenialCodes.AccessUnavailable)).toContain(
      'temporarily unavailable',
    )
    expect(reasonCodeToPlainMessage('fieldcompanion.not_available')).toContain(
      'temporarily unavailable',
    )
    expect(reasonCodeToPlainMessage('product_not_available')).toContain('temporarily unavailable')
    expect(reasonCodeToPlainMessage('product_unavailable')).toContain('temporarily unavailable')
    expect(reasonCodeToPlainMessage('launch.product_unavailable')).toContain(
      'temporarily unavailable',
    )
    expect(reasonCodeToPlainMessage('handoff.not_available')).toContain('temporarily unavailable')
    expect(reasonCodeToPlainMessage('not_available')).toContain('temporarily unavailable')
    expect(reasonCodeToPlainMessage('availability_inactive')).toContain('temporarily unavailable')
    expect(reasonCodeToPlainMessage('availability_revoked')).toContain('temporarily unavailable')
  })

  it('maps offline conflict reason codes to review guidance', () => {
    expect(reasonCodeToPlainMessage('fieldcompanion.offline_actions.record_changed')).toContain(
      'Open the current task',
    )
    expect(reasonCodeToPlainMessage('fieldcompanion.offline_actions.idempotency_conflict')).toContain(
      'Discard the stale copy',
    )
    expect(reasonCodeToPlainMessage('fieldcompanion.offline_actions.payload_idempotency_mismatch')).toContain(
      'fresh clock event',
    )
  })

  it('prefers reasonMessage over reasonCode', () => {
    expect(
      resolveDeniedReason(
        {
          reasonCode: FieldCompanionFieldValidationReasonCodes.NotInInbox,
          reasonMessage: 'Custom supervisor message.',
        },
        'Fallback.',
      ),
    ).toBe('Custom supervisor message.')
  })

  it('falls back to catalog when only reasonCode is present', () => {
    expect(
      resolveDeniedReason(
        { reasonCode: FieldCompanionFieldValidationReasonCodes.InboxUnavailable },
        'Fallback.',
      ),
    ).toContain('connectivity')
  })

  it('adds next-step guidance for known blocked task reasons', () => {
    expect(formatBlockedTaskReason('Pre-trip DVIR required')).toContain('RoutArr')
    expect(formatBlockedTaskReason('Evidence required')).toContain('TrainArr')
  })

  it('formats inbox source errors from code or message', () => {
    expect(
      formatInboxSourceError('routarr', 'upstream_unreachable', null),
    ).toContain('connectivity')
    expect(formatInboxSourceError('trainarr', null, 'TrainArr timed out.')).toBe(
      'TrainArr timed out.',
    )
  })
})
