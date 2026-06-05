import { describe, expect, it } from 'vitest'
import {
  buildInspectionAnswerInputs,
  draftsFromInspectionAnswers,
  requiredInspectionItemsAnswered,
} from './fieldInspection'

describe('fieldInspection helpers', () => {
  it('builds answer inputs from drafts', () => {
    const inputs = buildInspectionAnswerInputs(
      {
        '11111111-1111-1111-1111-111111111111': {
          passFailValue: 'pass',
          numericValue: '',
          textValue: '',
        },
      },
      ['11111111-1111-1111-1111-111111111111'],
    )

    expect(inputs).toEqual([
      {
        checklistItemId: '11111111-1111-1111-1111-111111111111',
        passFailValue: 'pass',
        numericValue: null,
        textValue: null,
      },
    ])
  })

  it('requires all required checklist items before completion', () => {
    const items = [
      {
        checklistItemId: '11111111-1111-1111-1111-111111111111',
        itemType: 'pass_fail',
        isRequired: true,
      },
      {
        checklistItemId: '22222222-2222-2222-2222-222222222222',
        itemType: 'numeric',
        isRequired: false,
      },
    ]

    expect(
      requiredInspectionItemsAnswered(items, {
        '11111111-1111-1111-1111-111111111111': {
          passFailValue: '',
          numericValue: '',
          textValue: '',
        },
      }),
    ).toBe(false)

    expect(
      requiredInspectionItemsAnswered(items, {
        '11111111-1111-1111-1111-111111111111': {
          passFailValue: 'pass',
          numericValue: '',
          textValue: '',
        },
      }),
    ).toBe(true)
  })

  it('hydrates drafts from saved answers', () => {
    expect(
      draftsFromInspectionAnswers([
        {
          checklistItemId: '11111111-1111-1111-1111-111111111111',
          passFailValue: 'fail',
          numericValue: 12.5,
          textValue: 'observed wear',
        },
      ]),
    ).toEqual({
      '11111111-1111-1111-1111-111111111111': {
        passFailValue: 'fail',
        numericValue: '12.5',
        textValue: 'observed wear',
      },
    })
  })
})
