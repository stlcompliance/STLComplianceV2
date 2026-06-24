import { describe, expect, it } from 'vitest'

import { formatLoadArrMutationFailure } from './mutationMessages'

describe('formatLoadArrMutationFailure', () => {
  it('describes the failed action without implying a local success', () => {
    expect(formatLoadArrMutationFailure('Receiving completion')).toBe(
      'Receiving completion failed. The API write was not confirmed, so no local record was created.',
    )
  })
})
