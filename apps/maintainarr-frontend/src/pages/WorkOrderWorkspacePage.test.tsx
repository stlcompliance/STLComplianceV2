import { render, screen } from '@testing-library/react'
import { MemoryRouter, Route, Routes, useLocation } from 'react-router-dom'
import { describe, expect, it } from 'vitest'

import { WorkOrderWorkspacePage } from './WorkOrderWorkspacePage'

function LocationProbe() {
  const location = useLocation()

  return (
    <div data-testid="location-probe">
      {location.pathname}
      {location.search}
    </div>
  )
}

describe('WorkOrderWorkspacePage', () => {
  it('redirects work order deep links into the detail workspace', () => {
    render(
      <MemoryRouter initialEntries={['/work-orders/wo-123']}>
        <Routes>
          <Route path="/work-orders/:workOrderId" element={<WorkOrderWorkspacePage />} />
          <Route path="/work-orders/details" element={<LocationProbe />} />
        </Routes>
      </MemoryRouter>,
    )

    expect(screen.getByTestId('location-probe')).toHaveTextContent(
      '/work-orders/details?workOrderId=wo-123',
    )
  })
})
