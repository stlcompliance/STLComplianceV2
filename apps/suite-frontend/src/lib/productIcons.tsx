import type { LucideIcon } from 'lucide-react'
import { getSuiteProductIcon } from '@stl/shared-ui'

export function getProductIcon(productKey: string): LucideIcon {
  return getSuiteProductIcon(productKey)
}
