export type LoadArrItemReference = {
  supplyarrItemId: string
  itemNumberSnapshot: string
  itemNameSnapshot: string
}

export function resolveLoadArrItemLabel(supplyarrItemId: string, references: readonly LoadArrItemReference[]) {
  const reference = references.find((item) => item.supplyarrItemId === supplyarrItemId)
  if (!reference) {
    return 'Unknown item'
  }

  return [reference.itemNumberSnapshot, reference.itemNameSnapshot].filter(Boolean).join(' · ') || 'Unknown item'
}
