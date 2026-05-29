export type InspectionAnswerDraft = {
  passFailValue: string
  numericValue: string
  textValue: string
}

export function buildInspectionAnswerInputs(
  drafts: Record<string, InspectionAnswerDraft>,
  checklistItemIds: string[],
): Array<{
  checklistItemId: string
  passFailValue: string | null
  numericValue: number | null
  textValue: string | null
}> {
  return checklistItemIds.map((checklistItemId) => {
    const draft = drafts[checklistItemId] ?? {
      passFailValue: '',
      numericValue: '',
      textValue: '',
    }

    return {
      checklistItemId,
      passFailValue: draft.passFailValue.trim() || null,
      numericValue: draft.numericValue.trim() ? Number(draft.numericValue) : null,
      textValue: draft.textValue.trim() || null,
    }
  })
}

export function requiredInspectionItemsAnswered(
  items: ReadonlyArray<{ checklistItemId: string; itemType: string; isRequired: boolean }>,
  drafts: Record<string, InspectionAnswerDraft>,
): boolean {
  return items
    .filter((item) => item.isRequired)
    .every((item) => {
      const draft = drafts[item.checklistItemId]
      if (!draft) {
        return false
      }

      if (item.itemType === 'pass_fail') {
        return Boolean(draft.passFailValue.trim())
      }

      if (item.itemType === 'numeric') {
        return Boolean(draft.numericValue.trim())
      }

      return Boolean(draft.textValue.trim())
    })
}

export function draftsFromInspectionAnswers(
  answers: ReadonlyArray<{
    checklistItemId: string
    passFailValue: string | null
    numericValue: number | null
    textValue: string | null
  }>,
): Record<string, InspectionAnswerDraft> {
  const drafts: Record<string, InspectionAnswerDraft> = {}
  for (const answer of answers) {
    drafts[answer.checklistItemId] = {
      passFailValue: answer.passFailValue ?? '',
      numericValue: answer.numericValue?.toString() ?? '',
      textValue: answer.textValue ?? '',
    }
  }
  return drafts
}
