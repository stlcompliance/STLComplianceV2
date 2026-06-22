import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'

import { HintsPreferenceProvider, PageHeader } from './index'

describe('PageHeader', () => {
  it('hides optional subtitle text when hints are disabled', () => {
    render(
      <HintsPreferenceProvider showHints={false} setShowHints={() => undefined}>
        <PageHeader title="Preferences" subtitle="Optional helper guidance" />
      </HintsPreferenceProvider>,
    )

    expect(screen.getByRole('heading', { name: 'Preferences' })).toBeInTheDocument()
    expect(screen.queryByText('Optional helper guidance')).toBeNull()
  })
})
