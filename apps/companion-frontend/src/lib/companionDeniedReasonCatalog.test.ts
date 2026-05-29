import { describe, expect, it } from 'vitest'

import {
  formatBlockedTaskReason,
  formatInboxSourceError,
  reasonCodeToPlainMessage,
  resolveDeniedReason,
} from './companionDeniedReasonCatalog'
import {
  CompanionFieldValidationReasonCodes,
  CompanionLaunchDenialCodes,
  CompanionScanReasonCodes,
} from './companionValidationReasonCodes'

describe('companionDeniedReasonCatalog', () => {
  it('maps field validation codes to plain language', () => {
    expect(reasonCodeToPlainMessage(CompanionFieldValidationReasonCodes.NotInInbox)).toContain(
      'field inbox',
    )
    expect(reasonCodeToPlainMessage(CompanionFieldValidationReasonCodes.DvirUnsupported)).toContain(
      'RoutArr',
    )
  })

  it('maps scan and launch denial codes', () => {
    expect(reasonCodeToPlainMessage(CompanionScanReasonCodes.InvalidPayload)).toContain('scan')
    expect(reasonCodeToPlainMessage(CompanionLaunchDenialCodes.NotEntitled)).toContain('entitled')
  })

  it('prefers reasonMessage over reasonCode', () => {
    expect(
      resolveDeniedReason(
        {
          reasonCode: CompanionFieldValidationReasonCodes.NotInInbox,
          reasonMessage: 'Custom supervisor message.',
        },
        'Fallback.',
      ),
    ).toBe('Custom supervisor message.')
  })

  it('falls back to catalog when only reasonCode is present', () => {
    expect(
      resolveDeniedReason(
        { reasonCode: CompanionFieldValidationReasonCodes.InboxUnavailable },
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
