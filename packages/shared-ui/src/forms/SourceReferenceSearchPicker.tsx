import { useQuery } from '@tanstack/react-query'

import { AsyncSearchPicker } from './AsyncSearchPicker'
import { buildSourceObjectRef } from '../sourceReferences'
import type { ReferenceProviderClient } from './ReferenceProviderClient'
import type { ReferenceSummaryResponse, ReferenceTypeDescriptor } from './referenceTypes'
import type { SourceReferenceOption } from '../sourceReferences'

type ReferenceClient = Pick<
  ReferenceProviderClient,
  'getSummary' | 'listReferenceTypes' | 'searchReferences'
>

export type SourceReferenceClientMap = Record<string, ReferenceClient | undefined>

export type SourceReferenceSearchPickerProps = {
  clientsByProduct: SourceReferenceClientMap
  sourceProduct?: string | null
  value: string
  onChange: (value: string, selected?: SourceReferenceOption) => void
  label?: string
  id?: string
  placeholder?: string
  minQueryLength?: number
  debounceMs?: number
  limit?: number
  disabled?: boolean
  testId?: string
}

function formatSecondaryLabel(summary: ReferenceSummaryResponse): string {
  return [summary.secondaryLabel, summary.status].filter(Boolean).join(' / ')
}

function toPickerOption(
  sourceProduct: string,
  descriptor: ReferenceTypeDescriptor,
  summary: ReferenceSummaryResponse,
): SourceReferenceOption {
  const secondary = formatSecondaryLabel(summary)
  return {
    value: buildSourceObjectRef(sourceProduct, descriptor.referenceType, summary.referenceId),
    label: secondary ? `${descriptor.label}: ${summary.displayLabel} · ${secondary}` : `${descriptor.label}: ${summary.displayLabel}`,
    sourceProduct,
    sourceObjectType: descriptor.referenceType,
    sourceObjectId: summary.referenceId,
    sourceObjectDisplayName: summary.displayLabel,
    inactive: ['archived', 'deleted', 'merged', 'superseded', 'inactive', 'retired'].includes(
      summary.status?.trim().toLowerCase() ?? '',
    ),
  }
}

function parseSourceObjectRef(value: string): { sourceProduct: string; sourceObjectType: string; sourceObjectId: string } | null {
  const parts = value.split(':')
  if (parts.length < 3) {
    return null
  }

  const [sourceProduct, sourceObjectType, ...rest] = parts
  const sourceObjectId = rest.join(':')
  if (!sourceProduct || !sourceObjectType || !sourceObjectId) {
    return null
  }

  return {
    sourceProduct,
    sourceObjectType,
    sourceObjectId,
  }
}

export function SourceReferenceSearchPicker({
  clientsByProduct,
  sourceProduct,
  value,
  onChange,
  label,
  id,
  placeholder = 'Search source records…',
  minQueryLength = 2,
  debounceMs = 300,
  limit = 25,
  disabled = false,
  testId,
}: SourceReferenceSearchPickerProps) {
  const client = sourceProduct ? clientsByProduct[sourceProduct] : undefined
  const parsedValue = value ? parseSourceObjectRef(value) : null

  const selectedQuery = useQuery({
    queryKey: ['source-reference-search-picker-selected', sourceProduct, value],
    queryFn: async () => {
      if (!client) {
        return null
      }

      const parsed = parseSourceObjectRef(value)
      if (!parsed || parsed.sourceProduct !== sourceProduct) {
        return null
      }

      return client.getSummary(parsed.sourceObjectType, parsed.sourceObjectId)
    },
    enabled: Boolean(client && sourceProduct && value && !disabled),
    staleTime: 60_000,
  })

  const referenceTypesQuery = useQuery({
    queryKey: ['source-reference-search-picker-types', sourceProduct],
    queryFn: async () => {
      if (!client) {
        return [] as ReferenceTypeDescriptor[]
      }

      const types = await client.listReferenceTypes()
      return types.filter((type) => type.canSearch)
    },
    enabled: Boolean(client && sourceProduct && !disabled),
    staleTime: 60_000,
  })

  const selectedOption = selectedQuery.data && sourceProduct && parsedValue
    ? toPickerOption(
        sourceProduct,
        {
          ownerProductKey: sourceProduct,
          referenceType: parsedValue.sourceObjectType,
          label: parsedValue.sourceObjectType,
          canSearch: true,
          canQuickCreate: false,
        },
        selectedQuery.data,
      )
    : undefined

  return (
    <AsyncSearchPicker
      value={value}
      onChange={(nextValue) => {
        const parsed = parseSourceObjectRef(nextValue)
        onChange(
          nextValue,
          parsed
            ? {
                value: nextValue,
                label: nextValue,
                sourceProduct: parsed.sourceProduct,
                sourceObjectType: parsed.sourceObjectType,
                sourceObjectId: parsed.sourceObjectId,
                sourceObjectDisplayName: parsed.sourceObjectId,
              }
            : undefined,
        )
      }}
      queryKey={['source-reference-search-picker', sourceProduct, limit]}
      queryFn={async (query) => {
        if (!client || !sourceProduct) {
          return []
        }

        const types = referenceTypesQuery.data ?? []
        const results = await Promise.all(
          types.map(async (descriptor) => {
            const response = await client.searchReferences({
              referenceType: descriptor.referenceType,
              query,
              limit: Math.max(1, Math.ceil(limit / Math.max(types.length, 1))),
            })
            return response.results.map((summary) => toPickerOption(sourceProduct, descriptor, summary))
          }),
        )

        return results.flat().slice(0, limit)
      }}
      selectedOption={selectedOption}
      label={label}
      id={id}
      placeholder={client ? placeholder : 'Source product has no reference API'}
      minQueryLength={minQueryLength}
      debounceMs={debounceMs}
      enabled={Boolean(client && sourceProduct)}
      disabled={disabled || !client}
      testId={testId}
    />
  )
}
