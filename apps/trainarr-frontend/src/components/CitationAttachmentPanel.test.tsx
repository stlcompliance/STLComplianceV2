import { cleanup, fireEvent, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { CitationAttachmentPanel } from './CitationAttachmentPanel'

describe('CitationAttachmentPanel', () => {
  const citationOptions = [
    {
      citationId: 'c1',
      citationKey: 'cfr_391_11',
      label: '391.11 General qualifications (cfr_391_11)',
    },
  ]

  afterEach(() => {
    cleanup()
  })

  it('renders attach form for managers', () => {
    render(
        <CitationAttachmentPanel
          title="Definition citations"
          citations={[]}
          citationOptions={citationOptions}
          citationIdInput=""
          citationKeyInput=""
          onCitationSelectionChange={vi.fn()}
          onAttach={vi.fn()}
          onRemove={vi.fn()}
          isAttaching={false}
        isRemovingId={null}
        canManage
        validateWithComplianceCore
        onValidateWithComplianceCoreChange={vi.fn()}
      />,
    )

    expect(screen.getByRole('button', { name: /attach citation/i })).toBeDisabled()
    expect(screen.getByTestId('citation-attachment-citation-id')).toBeInTheDocument()
  })

  it('lists attached citations with metadata', () => {
    render(
        <CitationAttachmentPanel
          title="Program citations"
          citations={[
          {
            attachmentId: 'a1',
            entityType: 'training_program',
            entityId: 'p1',
            complianceCoreCitationId: 'c1',
            citationKey: 'cfr_391_11',
            citationVersion: 1,
            createdAt: '2026-05-27T00:00:00Z',
            metadata: {
              label: '391.11 General qualifications',
              sourceReference: '49 CFR 391.11',
              description: 'General qualifications of drivers.',
              regulatoryProgramKey: 'driver_compliance',
              rulePackKey: null,
              isActive: true,
            },
          },
          ]}
          citationOptions={citationOptions}
          citationIdInput=""
          citationKeyInput=""
          onCitationSelectionChange={vi.fn()}
          onAttach={vi.fn()}
          onRemove={vi.fn()}
          isAttaching={false}
        isRemovingId={null}
        canManage
        validateWithComplianceCore={false}
        onValidateWithComplianceCoreChange={vi.fn()}
      />,
    )

    expect(screen.getByText('cfr_391_11')).toBeInTheDocument()
    expect(screen.getByText('391.11 General qualifications')).toBeInTheDocument()
    fireEvent.click(screen.getByRole('button', { name: /remove/i }))
  })
})
