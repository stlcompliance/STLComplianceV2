const KEY_MIN_LENGTH = 2
const KEY_MAX_LENGTH = 64

export function slugifyKey(label: string): string {
  const normalized = label
    .trim()
    .toLowerCase()
    .replace(/[^a-z0-9]+/g, '-')
    .replace(/^-+|-+$/g, '')

  if (normalized.length < KEY_MIN_LENGTH) {
    return ''
  }

  return normalized.slice(0, KEY_MAX_LENGTH)
}

export function withKeySuffix(base: string, suffix: number): string {
  const suffixText = `-${suffix}`
  const maxBaseLength = KEY_MAX_LENGTH - suffixText.length
  const trimmedBase = base.slice(0, Math.max(KEY_MIN_LENGTH, maxBaseLength))
  return `${trimmedBase}${suffixText}`
}
