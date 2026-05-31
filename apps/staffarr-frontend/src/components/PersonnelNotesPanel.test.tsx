import { cleanup, fireEvent, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { PersonnelNotesPanel } from './PersonnelNotesPanel'
import type { PersonnelNoteSummaryResponse } from '../api/types'

const sampleNotes: PersonnelNoteSummaryResponse[] = [
  {
    noteId: 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa',
    personId: 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb',
    categoryKey: 'coaching',
    visibilityKey: 'management',
    subject: 'Quarterly coaching follow-up',
    status: 'active',
    createdByUserId: 'cccccccc-cccc-cccc-cccc-cccccccccccc',
    createdAt: '2026-05-26T15:00:00.000Z',
    updatedAt: '2026-05-26T15:00:00.000Z',
  },
]

describe('PersonnelNotesPanel', () => {
  afterEach(() => {
    cleanup()
  })

  it('renders note list and intake form for authorized users', () => {
    render(
      <PersonnelNotesPanel
        personId={sampleNotes[0].personId}
        personDisplayName="Alex Worker"
        notes={sampleNotes}
        selectedNote={null}
        isLoading={false}
        isError={false}
        readErrorMessage={null}
        onRetryRead={vi.fn()}
        isLoadingDetail={false}
        isDetailError={false}
        detailErrorMessage={null}
        onRetryDetail={vi.fn()}
        canManage
        isSubmitting={false}
        actionErrorMessage={null}
        onSelectNote={vi.fn()}
        onCreateNote={vi.fn().mockResolvedValue(undefined)}
      />,
    )

    expect(screen.getByRole('heading', { name: /Personnel notes/i })).toBeTruthy()
    expect(screen.getByText(/Quarterly coaching follow-up/i)).toBeTruthy()
    expect(screen.getByRole('button', { name: /Save note/i })).toBeTruthy()
  })

  it('submits note intake with category and visibility', async () => {
    const onCreateNote = vi.fn().mockResolvedValue(undefined)

    render(
      <PersonnelNotesPanel
        personId={sampleNotes[0].personId}
        personDisplayName="Alex Worker"
        notes={[]}
        selectedNote={null}
        isLoading={false}
        isError={false}
        readErrorMessage={null}
        onRetryRead={vi.fn()}
        isLoadingDetail={false}
        isDetailError={false}
        detailErrorMessage={null}
        onRetryDetail={vi.fn()}
        canManage
        isSubmitting={false}
        actionErrorMessage={null}
        onSelectNote={vi.fn()}
        onCreateNote={onCreateNote}
      />,
    )

    fireEvent.change(screen.getByLabelText(/Subject/i), {
      target: { value: 'Performance check-in summary' },
    })
    fireEvent.change(screen.getByLabelText(/Body/i), {
      target: {
        value: 'Documented coaching conversation and agreed follow-up actions for next review cycle.',
      },
    })
    fireEvent.click(screen.getByRole('button', { name: /Save note/i }))

    expect(onCreateNote).toHaveBeenCalled()
    const payload = onCreateNote.mock.calls[0][0]
    expect(payload.subject).toBe('Performance check-in summary')
    expect(payload.categoryKey).toBe('general')
    expect(payload.visibilityKey).toBe('hr_only')
  })

  it('renders notes action errors in shared callout', () => {
    render(
      <PersonnelNotesPanel
        personId={sampleNotes[0].personId}
        personDisplayName="Alex Worker"
        notes={sampleNotes}
        selectedNote={null}
        isLoading={false}
        isError={false}
        readErrorMessage={null}
        onRetryRead={vi.fn()}
        isLoadingDetail={false}
        isDetailError={false}
        detailErrorMessage={null}
        onRetryDetail={vi.fn()}
        canManage
        isSubmitting={false}
        actionErrorMessage="Could not save note"
        onSelectNote={vi.fn()}
        onCreateNote={vi.fn().mockResolvedValue(undefined)}
      />,
    )

    expect(screen.getByRole('alert')).toBeTruthy()
    expect(screen.getByText('Personnel notes action failed')).toBeTruthy()
    expect(screen.getByText('Could not save note')).toBeTruthy()
  })

  it('renders retryable read error callout when notes query fails', () => {
    const onRetry = vi.fn()
    render(
      <PersonnelNotesPanel
        personId={sampleNotes[0].personId}
        personDisplayName="Alex Worker"
        notes={[]}
        selectedNote={null}
        isLoading={false}
        isError
        readErrorMessage="notes read failed"
        onRetryRead={onRetry}
        isLoadingDetail={false}
        isDetailError={false}
        detailErrorMessage={null}
        onRetryDetail={vi.fn()}
        canManage
        isSubmitting={false}
        actionErrorMessage={null}
        onSelectNote={vi.fn()}
        onCreateNote={vi.fn().mockResolvedValue(undefined)}
      />,
    )

    expect(screen.getByText('Personnel notes unavailable')).toBeTruthy()
    expect(screen.getByText('notes read failed')).toBeTruthy()
    fireEvent.click(screen.getByRole('button', { name: 'Retry notes' }))
    expect(onRetry).toHaveBeenCalledTimes(1)
  })

  it('renders retryable detail error callout when note detail query fails', () => {
    const onRetryDetail = vi.fn()
    render(
      <PersonnelNotesPanel
        personId={sampleNotes[0].personId}
        personDisplayName="Alex Worker"
        notes={sampleNotes}
        selectedNoteId={sampleNotes[0].noteId}
        selectedNote={{
          noteId: sampleNotes[0].noteId,
          personId: sampleNotes[0].personId,
          categoryKey: sampleNotes[0].categoryKey,
          visibilityKey: sampleNotes[0].visibilityKey,
          subject: sampleNotes[0].subject,
          body: 'Details',
          status: 'active',
          createdByUserId: sampleNotes[0].createdByUserId,
          createdAt: sampleNotes[0].createdAt,
          updatedAt: sampleNotes[0].updatedAt,
        }}
        isLoading={false}
        isError={false}
        readErrorMessage={null}
        onRetryRead={vi.fn()}
        isLoadingDetail={false}
        isDetailError
        detailErrorMessage="note detail read failed"
        onRetryDetail={onRetryDetail}
        canManage
        isSubmitting={false}
        actionErrorMessage={null}
        onSelectNote={vi.fn()}
        onCreateNote={vi.fn().mockResolvedValue(undefined)}
      />,
    )

    expect(screen.getByText('Note detail unavailable')).toBeTruthy()
    expect(screen.getByText('note detail read failed')).toBeTruthy()
    fireEvent.click(screen.getByRole('button', { name: 'Retry note detail' }))
    expect(onRetryDetail).toHaveBeenCalledTimes(1)
  })

  it('shows detail error callout when a note is selected but detail payload is null', () => {
    render(
      <PersonnelNotesPanel
        personId={sampleNotes[0].personId}
        personDisplayName="Alex Worker"
        notes={sampleNotes}
        selectedNoteId={sampleNotes[0].noteId}
        selectedNote={null}
        isLoading={false}
        isError={false}
        readErrorMessage={null}
        onRetryRead={vi.fn()}
        isLoadingDetail={false}
        isDetailError
        detailErrorMessage="note detail missing after read failure"
        onRetryDetail={vi.fn()}
        canManage
        isSubmitting={false}
        actionErrorMessage={null}
        onSelectNote={vi.fn()}
        onCreateNote={vi.fn().mockResolvedValue(undefined)}
      />,
    )

    expect(screen.getByText('Note detail unavailable')).toBeTruthy()
    expect(screen.getByText('note detail missing after read failure')).toBeTruthy()
  })
})
