export function formatRoleLabel(roleKey: string) {
  const trimmed = roleKey.trim()
  if (!trimmed) {
    return 'Member'
  }

  return trimmed
    .split(/[_\s-]+/)
    .filter(Boolean)
    .map((part) => part.charAt(0).toUpperCase() + part.slice(1).toLowerCase())
    .join(' ')
}

