const DEEP_LINK_PATTERNS: ReadonlyArray<{
  productKey: string
  resourceType: string
  pathPrefix: string
}> = [
  { productKey: 'trainarr', resourceType: 'assignment', pathPrefix: '/assignments/' },
  { productKey: 'maintainarr', resourceType: 'work-order', pathPrefix: '/work-orders/' },
  { productKey: 'maintainarr', resourceType: 'inspection', pathPrefix: '/inspections/' },
  { productKey: 'routarr', resourceType: 'trip', pathPrefix: '/trips/' },
  { productKey: 'staffarr', resourceType: 'incident', pathPrefix: '/incidents/' },
]

const TASK_KEY_PATTERN = /^[a-z][a-z0-9_-]*:[a-z][a-z0-9_-]*:[0-9a-f-]{36}$/i

export function normalizeScanPayload(scannedValue: string): string {
  const trimmed = scannedValue.trim()
  if (TASK_KEY_PATTERN.test(trimmed)) {
    return trimmed
  }

  if (trimmed.toLowerCase().startsWith('stl-field-task:')) {
    return trimmed.slice('stl-field-task:'.length).trim()
  }

  try {
    if (trimmed.startsWith('{')) {
      const parsed = JSON.parse(trimmed) as { taskKey?: string }
      if (parsed.taskKey?.trim()) {
        return parsed.taskKey.trim()
      }
    }
  } catch {
    // fall through
  }

  if (trimmed.startsWith('http://') || trimmed.startsWith('https://')) {
    const url = new URL(trimmed)
    const fromQuery = url.searchParams.get('taskKey')?.trim()
    if (fromQuery) {
      return fromQuery
    }
    return pathToTaskKey(url.pathname) ?? trimmed
  }

  return pathToTaskKey(trimmed) ?? trimmed
}

function pathToTaskKey(path: string): string | null {
  const queryIndex = path.indexOf('?')
  if (queryIndex >= 0) {
    const query = new URLSearchParams(path.slice(queryIndex + 1))
    const taskKey = query.get('taskKey')?.trim()
    if (taskKey && TASK_KEY_PATTERN.test(taskKey)) {
      return taskKey
    }
  }

  const normalized = path.split('?')[0] ?? path
  for (const pattern of DEEP_LINK_PATTERNS) {
    if (!normalized.toLowerCase().startsWith(pattern.pathPrefix)) {
      continue
    }

    const remainder = normalized.slice(pattern.pathPrefix.length).replace(/^\/+|\/+$/g, '')
    const idSegment = remainder.split('/')[0]
    if (!idSegment || !/^[0-9a-f-]{36}$/i.test(idSegment)) {
      continue
    }

    return `${pattern.productKey}:${pattern.resourceType}:${idSegment}`
  }

  return null
}
