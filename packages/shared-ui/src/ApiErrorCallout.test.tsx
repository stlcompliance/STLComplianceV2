import { fireEvent, render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'

import { ApiErrorCallout, getErrorMessage } from './ApiErrorCallout'

describe('ApiErrorCallout', () => {
  it('renders title and message', () => {
    render(<ApiErrorCallout title="Request failed" message="The service is unavailable." />)
    expect(screen.getByText('Request failed')).toBeTruthy()
    expect(screen.getByText('The service is unavailable.')).toBeTruthy()
  })

  it('calls retry handler when retry button is clicked', () => {
    const onRetry = vi.fn()
    render(<ApiErrorCallout message="Retry me" onRetry={onRetry} retryLabel="Try again" />)
    fireEvent.click(screen.getByText('Try again'))
    expect(onRetry).toHaveBeenCalledTimes(1)
  })
})

describe('getErrorMessage', () => {
  it('returns Error.message for Error objects', () => {
    expect(getErrorMessage(new Error('Boom'))).toBe('Boom')
  })

  it('returns fallback when message cannot be resolved', () => {
    expect(getErrorMessage({})).toBe('Something went wrong.')
  })
})
