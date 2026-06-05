/** Canonical product scope copy for public product pages. */
import { IMPLEMENTED_PRODUCT_OWNERSHIP } from '@stl/shared-ui'

export type ProductOwnershipCopy = {
  owns: string
  doesNotOwn: string
}

export const PRODUCT_OWNERSHIP: Record<string, ProductOwnershipCopy> = Object.fromEntries(
  IMPLEMENTED_PRODUCT_OWNERSHIP.map((entry) => [
    entry.productKey,
    {
      owns: entry.owns,
      doesNotOwn: entry.doesNotOwn,
    },
  ]),
)

export const COMPLIANCE_CORE_EDUCATION = {
  headline: 'Rules and proof, connected to real work',
  lead:
    'Compliance Core helps the suite understand which rules apply, what proof matters, and where evidence should come from. The work still happens in the product built for that job.',
  bullets: [
    'A trainer, mechanic, dispatcher, or buyer keeps working in the tool made for their workflow.',
    'Compliance Core helps connect that work to rules, citations, evidence expectations, and audit questions.',
    'This keeps compliance proof tied to what actually happened instead of buried in side documents.',
  ],
} as const
