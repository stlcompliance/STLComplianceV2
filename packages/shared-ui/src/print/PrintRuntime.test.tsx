import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react'
import { useState } from 'react'
import { afterEach, describe, expect, it } from 'vitest'

import { PrintRuntimeProvider, usePrintRuntime, useRegisterPrintableSurface } from './PrintRuntime'
import type { PrintableSurfaceRegistration } from './types'

const parentSurface: PrintableSurfaceRegistration = {
  title: 'Parent surface',
}

const childSurface: PrintableSurfaceRegistration = {
  title: 'Child surface',
}

function SurfaceRegistrar({ surface }: { surface: PrintableSurfaceRegistration | false | null }) {
  useRegisterPrintableSurface(surface)
  return null
}

function CurrentSurfaceTitle() {
  const { surface } = usePrintRuntime()
  return <div data-testid="current-surface">{surface?.title ?? 'none'}</div>
}

function NestedSurfaceHarness() {
  const [showChild, setShowChild] = useState(true)

  return (
    <>
      <SurfaceRegistrar surface={parentSurface} />
      {showChild ? <SurfaceRegistrar surface={childSurface} /> : null}
      <button type="button" onClick={() => setShowChild(false)}>
        Remove child
      </button>
      <CurrentSurfaceTitle />
    </>
  )
}

describe('PrintRuntimeProvider', () => {
  afterEach(() => {
    cleanup()
  })

  it('keeps the newest registered surface active without rerender thrash', async () => {
    render(
      <PrintRuntimeProvider>
        <NestedSurfaceHarness />
      </PrintRuntimeProvider>,
    )

    await waitFor(() => expect(screen.getByTestId('current-surface')).toHaveTextContent('Child surface'))

    await screen.findByRole('button', { name: 'Remove child' })
  })

  it('falls back to the previous surface when the active one unmounts', async () => {
    render(
      <PrintRuntimeProvider>
        <NestedSurfaceHarness />
      </PrintRuntimeProvider>,
    )

    await waitFor(() => expect(screen.getByTestId('current-surface')).toHaveTextContent('Child surface'))

    fireEvent.click(screen.getByRole('button', { name: 'Remove child' }))

    await waitFor(() => expect(screen.getByTestId('current-surface')).toHaveTextContent('Parent surface'))
  })
})
