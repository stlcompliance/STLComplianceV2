import { describe, expect, it } from 'vitest'

import { formatLoadArrDependencyFailure, formatLoadArrMutationFailure } from './mutationMessages'

describe('formatLoadArrMutationFailure', () => {
  it('describes the failed action without implying a local success', () => {
    expect(formatLoadArrMutationFailure('Receiving completion')).toBe(
      'Receiving completion failed. The API write was not confirmed, so no local record was created.',
    )
  })

  it('explains dependency failures without inventing fallback data', () => {
    expect(formatLoadArrDependencyFailure('LoadArr workspace', 403)).toBe(
      'LoadArr workspace is unavailable because this user does not have permission to read the current LoadArr data.',
    )
    expect(formatLoadArrDependencyFailure('LoadArr workspace')).toBe(
      'LoadArr workspace is unavailable right now. LoadArr did not return authoritative data, so this view stays hidden until the API responds.',
    )
  })
})
