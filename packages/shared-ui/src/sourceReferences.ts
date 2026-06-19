import { IMPLEMENTED_PRODUCT_OWNERSHIP } from './productOwnershipManifest'
import type { PickerOption } from './forms/pickerTypes'

export type SourceReferenceOption = PickerOption & {
  sourceProduct: string
  sourceObjectType: string
  sourceObjectId: string
  sourceObjectDisplayName: string
}

export const SUITE_SOURCE_PRODUCT_OPTIONS: PickerOption[] = IMPLEMENTED_PRODUCT_OWNERSHIP.map((product) => ({
  value: product.productKey,
  label: product.displayName,
}))

export const SUITE_SOURCE_REFERENCE_OPTIONS: SourceReferenceOption[] = []

export const FACT_SOURCE_REFERENCE_OPTIONS: SourceReferenceOption[] = []

export function listSourceReferenceOptions(_sourceProduct?: string | null): SourceReferenceOption[] {
  return []
}

export function listFactSourceReferenceOptions(_sourceProduct?: string | null): SourceReferenceOption[] {
  return []
}

export function getSourceReferenceOption(_value?: string | null): SourceReferenceOption | undefined {
  return undefined
}

export type ParsedSourceObjectRef = {
  sourceProduct: string
  sourceObjectType: string
  sourceObjectId: string
}

export function parseSourceObjectRef(value?: string | null): ParsedSourceObjectRef | null {
  if (!value) {
    return null
  }

  const [sourceProduct, sourceObjectType, sourceObjectId, ...extra] = value.split(':').map((part) => part.trim())
  if (!sourceProduct || !sourceObjectType || !sourceObjectId || extra.length > 0) {
    return null
  }

  return {
    sourceProduct,
    sourceObjectType,
    sourceObjectId,
  }
}

export function buildSourceObjectRef(
  sourceProduct?: string | null,
  sourceObjectType?: string | null,
  sourceObjectId?: string | null,
): string {
  if (!sourceProduct || !sourceObjectType || !sourceObjectId) {
    return ''
  }

  return `${sourceProduct.trim()}:${sourceObjectType.trim()}:${sourceObjectId.trim()}`
}
