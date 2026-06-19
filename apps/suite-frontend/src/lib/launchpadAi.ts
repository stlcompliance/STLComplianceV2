import type { AiNavigationLink } from '@stl/shared-ui/aiNavigationLinks'

const INTERNAL_LINK_PATTERN = /\/app\/[^\s<)]+/gi
const ABSOLUTE_LINK_PATTERN = /https?:\/\/[^\s<)]+/gi

function normalizeLinkText(value: string): string {
  return value.trim().replace(/[.,;:!?]+$/g, '')
}

function normalizeHref(value: string): string {
  return normalizeLinkText(value).replace(/\/$/, '')
}

function scoreLink(answer: string, link: AiNavigationLink): number {
  const lowerAnswer = answer.toLowerCase()
  let score = 0

  const href = link.href.toLowerCase()
  if (lowerAnswer.includes(href)) {
    score = Math.max(score, 200)
  }

  const route = link.route.toLowerCase()
  if (lowerAnswer.includes(route)) {
    score = Math.max(score, 180)
  }

  const label = link.label.toLowerCase()
  if (lowerAnswer.includes(label)) {
    score = Math.max(score, 160 + label.length)
  }

  for (const alias of link.aliases ?? []) {
    const aliasMatch = alias.toLowerCase()
    if (aliasMatch && lowerAnswer.includes(aliasMatch)) {
      score = Math.max(score, 80 + aliasMatch.length)
    }
  }

  return score
}

function findExplicitHref(answer: string): string | null {
  const matches = [
    ...answer.matchAll(ABSOLUTE_LINK_PATTERN),
    ...answer.matchAll(INTERNAL_LINK_PATTERN),
  ]

  if (matches.length === 0) {
    return null
  }

  return normalizeLinkText(matches[0][0])
}

export function resolveLaunchpadDeepLink(
  answer: string,
  links: readonly AiNavigationLink[],
): AiNavigationLink | null {
  const explicitHref = findExplicitHref(answer)
  if (explicitHref) {
    const normalizedHref = normalizeHref(explicitHref)
    const explicitMatch = links.find((link) => normalizeHref(link.href) === normalizedHref)
    if (explicitMatch) {
      return explicitMatch
    }

    if (normalizedHref.startsWith('/app/')) {
      return {
        label: 'Suggested page',
        productKey: 'nexarr',
        route: normalizedHref,
        href: normalizedHref,
      }
    }
  }

  let bestLink: AiNavigationLink | null = null
  let bestScore = 0

  for (const link of links) {
    const score = scoreLink(answer, link)
    if (score > bestScore) {
      bestScore = score
      bestLink = link
    }
  }

  return bestScore > 0 ? bestLink : null
}
