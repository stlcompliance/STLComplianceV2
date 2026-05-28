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
        isLoadingDetail={false}
        canManage
        isSubmitting={false}
        errorMessage={null}
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
        isLoadingDetail={false}
        canManage
        isSubmitting={false}
        errorMessage={null}
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
})
