export function classifyFieldCompanionBrowser(userAgent: string | null | undefined): string {
  const normalized = userAgent?.toLowerCase() ?? ''

  if (normalized.includes('edg/')) return 'Edge'
  if (normalized.includes('firefox/')) return 'Firefox'
  if (normalized.includes('samsungbrowser/')) return 'Samsung Internet'
  if (normalized.includes('chrome/') || normalized.includes('crios/')) return 'Chrome'
  if (normalized.includes('safari/')) return 'Safari'

  return 'Browser'
}

export function classifyFieldCompanionDeviceClass(
  platform: string | null | undefined,
  userAgent: string | null | undefined,
): string {
  const normalized = `${platform ?? ''} ${userAgent ?? ''}`.toLowerCase()

  if (normalized.includes('android')) return 'Android device'
  if (normalized.includes('iphone') || normalized.includes('ipad') || normalized.includes('ios')) return 'iOS device'
  if (normalized.includes('win')) return 'Windows device'
  if (normalized.includes('mac')) return 'macOS device'
  if (normalized.includes('linux')) return 'Linux device'

  return 'device'
}

export function formatFieldCompanionLanguageGroup(language: string | null | undefined): string {
  const primaryLanguage = language?.split('-')[0]?.trim().toLowerCase()
  return primaryLanguage || 'unknown'
}

export function formatCurrentFieldCompanionDeviceSourceLabel(): string {
  const userAgent = typeof navigator !== 'undefined' ? navigator.userAgent : ''
  const platform = typeof navigator !== 'undefined' ? navigator.platform : ''

  return `${classifyFieldCompanionBrowser(userAgent)} on ${classifyFieldCompanionDeviceClass(platform, userAgent)}`
}
