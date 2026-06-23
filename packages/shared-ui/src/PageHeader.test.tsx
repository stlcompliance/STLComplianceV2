import { cleanup, render, screen, waitFor } from '@testing-library/react'
import { afterEach, describe, expect, it } from 'vitest'

import { HintsPreferenceProvider, PageHeader, PrintRuntimeProvider, usePrintRuntime } from './index'

function CurrentSurfaceTitle() {
  const { surface } = usePrintRuntime()
  return <div data-testid="current-surface">{surface?.title ?? 'none'}</div>
}

describe('PageHeader', () => {
  afterEach(() => {
    cleanup()
  })

  it('hides optional subtitle text when hints are disabled', () => {
    render(
      <HintsPreferenceProvider showHints={false} setShowHints={() => undefined}>
        <PageHeader title="Preferences" subtitle="Optional helper guidance" />
      </HintsPreferenceProvider>,
    )

    expect(screen.getByRole('heading', { name: 'Preferences' })).toBeInTheDocument()
    expect(screen.queryByText('Optional helper guidance')).toBeNull()
  })

  it('does not register a printable surface unless print registration is explicit', async () => {
    render(
      <PrintRuntimeProvider>
        <HintsPreferenceProvider showHints={true} setShowHints={() => undefined}>
          <PageHeader title="Preferences" subtitle="Optional helper guidance" />
          <CurrentSurfaceTitle />
        </HintsPreferenceProvider>
      </PrintRuntimeProvider>,
    )

    await waitFor(() => expect(screen.getByTestId('current-surface')).toHaveTextContent('none'))
  })

  it('registers a printable surface when print registration is provided', async () => {
    render(
      <PrintRuntimeProvider>
        <HintsPreferenceProvider showHints={true} setShowHints={() => undefined}>
          <PageHeader
            title="Person profile"
            subtitle="Optional helper guidance"
            printRegistration={{ sourceEntityType: 'person', sourceEntityId: 'person-1' }}
          />
          <CurrentSurfaceTitle />
        </HintsPreferenceProvider>
      </PrintRuntimeProvider>,
    )

    await waitFor(() => expect(screen.getByTestId('current-surface')).toHaveTextContent('Person profile'))
  })
})
