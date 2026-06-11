const KEY_MIN_LENGTH = 2
const KEY_MAX_LENGTH = 64

function isKeyCharacter(character: string): boolean {
  const code = character.charCodeAt(0)
  return (code >= 48 && code <= 57) || (code >= 97 && code <= 122)
}

export function slugifyKey(label: string): string {
  const normalizedLabel = label.trim().toLowerCase()
  let normalized = ''
  let pendingSeparator = false

  for (const character of normalizedLabel) {
    if (isKeyCharacter(character)) {
      if (pendingSeparator && normalized.length > 0 && normalized.length < KEY_MAX_LENGTH) {
        normalized += '-'
      }

      pendingSeparator = false

      if (normalized.length < KEY_MAX_LENGTH) {
        normalized += character
      }
    } else if (normalized.length > 0) {
      pendingSeparator = true
    }

    if (normalized.length >= KEY_MAX_LENGTH) {
      break
    }
  }

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
