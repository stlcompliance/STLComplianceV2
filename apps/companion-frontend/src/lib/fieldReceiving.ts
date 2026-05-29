export function receivingEditable(status: string): boolean {
  return status === 'draft'
}

export function parseQuantityInput(value: string): number | null {
  const trimmed = value.trim()
  if (!trimmed) {
    return null
  }

  const parsed = Number(trimmed)
  if (!Number.isFinite(parsed) || parsed < 0) {
    return null
  }

  return parsed
}

export function receivingPostReady(lines: ReadonlyArray<{ quantityReceived: number }>): boolean {
  return lines.some((line) => line.quantityReceived > 0)
}

export function receivingPostActionLabel(status: string): string | null {
  if (status === 'draft') {
    return 'Post receiving'
  }

  return null
}
