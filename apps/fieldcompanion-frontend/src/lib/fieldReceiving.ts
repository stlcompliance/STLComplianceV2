export function receivingPostReady(lines: ReadonlyArray<{ quantityReceived: number }>): boolean {
  return lines.some((line) => line.quantityReceived > 0)
}

export function receivingPostActionLabel(
  status: string,
  productKey: string,
  blockedReason?: string | null,
): string | null {
  if (blockedReason) {
    return null
  }

  if (productKey === 'loadarr' && status === 'open') {
    return 'Complete receiving'
  }

  return null
}
