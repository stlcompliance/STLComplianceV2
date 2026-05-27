import type { LucideIcon } from 'lucide-react'
import { getProductNavIcon } from '../navigation/navIcons'

export function getProductIcon(productKey: string): LucideIcon {
  return getProductNavIcon(productKey)
}
