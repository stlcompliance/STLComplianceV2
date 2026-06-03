import { cleanup, fireEvent, render, screen } from '@testing-library/react'
import { useState } from 'react'
import { afterEach, describe, expect, it, vi } from 'vitest'

vi.mock('@stl/shared-ui', async (importOriginal) => {
  const actual = await importOriginal<typeof import('@stl/shared-ui')>()
  return {
    ...actual,
    StaticSearchPicker: ({
      id,
      label,
      value,
      onChange,
      options,
      placeholder,
      testId,
    }: {
      id: string
      label: string
      value: string
      onChange: (value: string) => void
      options: Array<{ value: string; label: string }>
      placeholder?: string
      testId?: string
    }) => (
      <label htmlFor={id}>
        <span>{label}</span>
        <select
          id={id}
          aria-label={label}
          data-testid={testId}
          value={value}
          onChange={(event) => onChange(event.target.value)}
        >
          <option value="">{placeholder ?? 'Select…'}</option>
          {options.map((option) => (
            <option key={option.value} value={option.value}>
              {option.label}
            </option>
          ))}
        </select>
      </label>
    ),
  }
})

import { ProgramContentReferencePanel } from './ProgramContentReferencePanel'

function PanelHarness({
  onAttach,
  onRemove,
  onContentTypeKeyChange,
  onContentTitleChange,
  onContentReferenceValueChange,
  onContentNotesChange,
  onContentLocaleTagChange,
  contentReferences = [],
}: {
  onAttach: () => void
  onRemove: (contentReferenceId: string) => Promise<void>
  onContentTypeKeyChange?: (value: string) => void
  onContentTitleChange?: (value: string) => void
  onContentReferenceValueChange?: (value: string) => void
  onContentNotesChange?: (value: string) => void
  onContentLocaleTagChange?: (value: string) => void
  contentReferences?: Array<{
    contentReferenceId: string
    trainingProgramId: string
    contentType: string
    title: string
    referenceValue: string
    notes: string | null
    localeTag: string | null
    createdByUserId: string | null
    createdAt: string
  }>
}) {
  const [contentTypeKey, setContentTypeKey] = useState('')
  const [contentTitle, setContentTitle] = useState('')
  const [contentReferenceValue, setContentReferenceValue] = useState('')
  const [contentNotes, setContentNotes] = useState('')

  return (
    <ProgramContentReferencePanel
      title="Program content references"
      contentReferences={contentReferences}
      contentTypeKey={contentTypeKey}
      contentTitle={contentTitle}
      contentReferenceValue={contentReferenceValue}
      contentNotes={contentNotes}
      contentLocaleTag=""
      onContentTypeKeyChange={(value) => {
        onContentTypeKeyChange?.(value)
        setContentTypeKey(value)
      }}
      onContentTitleChange={(value) => {
        onContentTitleChange?.(value)
        setContentTitle(value)
      }}
      onContentReferenceValueChange={(value) => {
        onContentReferenceValueChange?.(value)
        setContentReferenceValue(value)
      }}
      onContentNotesChange={(value) => {
        onContentNotesChange?.(value)
        setContentNotes(value)
      }}
      onContentLocaleTagChange={(value) => {
        onContentLocaleTagChange?.(value)
      }}
      onAttach={onAttach}
      onRemove={onRemove}
      isAttaching={false}
      isRemovingId={null}
      canManage
    />
  )
}

describe('ProgramContentReferencePanel', () => {
  afterEach(() => {
    cleanup()
    vi.clearAllMocks()
  })

  it('adds a content reference through the searchable content-type picker', () => {
    const onContentTypeKeyChange = vi.fn()
    const onContentTitleChange = vi.fn()
    const onContentReferenceValueChange = vi.fn()
    const onContentNotesChange = vi.fn()
    const onContentLocaleTagChange = vi.fn()
    const onAttach = vi.fn()

    render(
      <PanelHarness
        onAttach={onAttach}
        onRemove={vi.fn()}
        onContentTypeKeyChange={onContentTypeKeyChange}
        onContentTitleChange={onContentTitleChange}
        onContentReferenceValueChange={onContentReferenceValueChange}
        onContentNotesChange={onContentNotesChange}
        onContentLocaleTagChange={onContentLocaleTagChange}
      />,
    )

    fireEvent.change(screen.getByLabelText(/Content type/i), { target: { value: 'external_url' } })
    fireEvent.change(screen.getByLabelText(/^Title/i), { target: { value: 'Reference title' } })
    fireEvent.change(screen.getByLabelText(/Reference value/i), { target: { value: 'https://example.com' } })
    fireEvent.change(screen.getByLabelText(/Locale tag/i), { target: { value: 'en-us' } })
    fireEvent.change(screen.getByLabelText(/^Notes/i), { target: { value: 'Helpful notes' } })
    fireEvent.click(screen.getByRole('button', { name: /add content reference/i }))

    expect(onContentTypeKeyChange).toHaveBeenCalledWith('external_url')
    expect(onContentTitleChange).toHaveBeenCalledWith('Reference title')
    expect(onContentReferenceValueChange).toHaveBeenCalledWith('https://example.com')
    expect(onContentLocaleTagChange).toHaveBeenCalledWith('en-us')
    expect(onContentNotesChange).toHaveBeenCalledWith('Helpful notes')
    expect(onAttach).toHaveBeenCalledOnce()
  })

  it('renders attached references and remove action', () => {
    const onRemove = vi.fn().mockResolvedValue(undefined)

    render(
      <PanelHarness
        onAttach={vi.fn()}
        onRemove={onRemove}
        contentReferences={[
          {
            contentReferenceId: 'ref-1',
            trainingProgramId: 'program-1',
            contentType: 'policy_document',
            title: 'Policy attachment',
            referenceValue: 'POL-001',
            notes: 'Internal policy reference',
            localeTag: 'en-us',
            createdByUserId: 'user-1',
            createdAt: '2026-05-28T12:00:00Z',
          },
        ]}
      />,
    )

    expect(screen.getByText('Policy attachment')).toBeInTheDocument()
    expect(screen.getByText('Policy document', { selector: 'p' })).toBeInTheDocument()
    expect(screen.getByText(/Locale en-us/i)).toBeInTheDocument()
    fireEvent.click(screen.getByRole('button', { name: /remove/i }))
    expect(onRemove).toHaveBeenCalledWith('ref-1')
  })
})
