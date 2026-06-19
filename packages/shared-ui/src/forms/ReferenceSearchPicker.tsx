import { useQuery } from '@tanstack/react-query'
import { useState } from 'react'

import { AsyncSearchPicker } from './AsyncSearchPicker'
import { QuickCreateDrawer } from './QuickCreateDrawer'
import type { ReferenceProviderClient } from './ReferenceProviderClient'
import type { PickerOption } from './pickerTypes'

type ReferenceClient = Pick<
  ReferenceProviderClient,
  'getSummary' | 'searchReferences' | 'getQuickCreateSchema' | 'quickCreate'
>

export type ReferenceSearchPickerProps = {
  client: ReferenceClient
  referenceType: string
  value: string
  onChange: (value: string) => void
  label?: string
  id?: string
  placeholder?: string
  minQueryLength?: number
  debounceMs?: number
  limit?: number
  disabled?: boolean
  allowQuickCreate?: boolean
  testId?: string
}

function toPickerOption(summary: Awaited<ReturnType<ReferenceClient['getSummary']>>): PickerOption {
  const secondaryBits = [summary.secondaryLabel, summary.status].filter(Boolean)
  const label = secondaryBits.length > 0 ? `${summary.displayLabel} · ${secondaryBits.join(' · ')}` : summary.displayLabel
  const normalizedStatus = summary.status?.trim().toLowerCase() ?? ''
  const inactiveStatuses = new Set(['archived', 'deleted', 'merged', 'superseded', 'inactive', 'retired'])

  return {
    value: summary.referenceId,
    label,
    inactive: inactiveStatuses.has(normalizedStatus),
  }
}

export function ReferenceSearchPicker({
  client,
  referenceType,
  value,
  onChange,
  label,
  id,
  placeholder = 'Search…',
  minQueryLength = 2,
  debounceMs = 300,
  limit = 25,
  disabled = false,
  allowQuickCreate = true,
  testId,
}: ReferenceSearchPickerProps) {
  const [quickCreateOpen, setQuickCreateOpen] = useState(false)
  const [quickCreateSeed, setQuickCreateSeed] = useState('')

  const selectedQuery = useQuery({
    queryKey: ['reference-search-picker-selected', referenceType, value],
    queryFn: () => client.getSummary(referenceType, value),
    enabled: Boolean(referenceType && value && !disabled),
    staleTime: 60_000,
  })

  const quickCreateSchemaQuery = useQuery({
    queryKey: ['reference-search-picker-quick-create-schema', referenceType],
    queryFn: () => client.getQuickCreateSchema(referenceType),
    enabled: allowQuickCreate && !disabled && Boolean(referenceType),
    staleTime: 60_000,
  })

  const quickCreateSchema = quickCreateSchemaQuery.data
  const showQuickCreate =
    allowQuickCreate &&
    quickCreateSchema &&
    (quickCreateSchema.allowed || quickCreateSchema.disabledReason || quickCreateSchema.permissionKey)

  return (
    <>
      <AsyncSearchPicker
        value={value}
        onChange={onChange}
        queryKey={['reference-search-picker', referenceType, limit]}
        queryFn={async (query) => {
          const response = await client.searchReferences({
            referenceType,
            query,
            limit,
          })
          return response.results.map(toPickerOption)
        }}
        selectedOption={selectedQuery.data ? toPickerOption(selectedQuery.data) : undefined}
        label={label}
        id={id}
        placeholder={placeholder}
        minQueryLength={minQueryLength}
        debounceMs={debounceMs}
        enabled={Boolean(referenceType)}
        disabled={disabled}
        quickCreateOption={
          showQuickCreate
            ? {
                label: 'Quick create',
                description: quickCreateSchema?.disabledReason
                  ? quickCreateSchema.disabledReason
                  : `Create a new ${quickCreateSchema?.referenceType ?? referenceType} in ${quickCreateSchema?.managedByLabel ?? 'the owning product'}.`,
                onSelect: (query) => {
                  setQuickCreateSeed(query)
                  setQuickCreateOpen(true)
                },
              }
            : null
        }
        testId={testId}
      />

      <QuickCreateDrawer
        open={quickCreateOpen}
        schema={quickCreateSchema}
        initialValues={{ name: quickCreateSeed, displayName: quickCreateSeed, legalName: quickCreateSeed }}
        onClose={() => {
          setQuickCreateOpen(false)
          setQuickCreateSeed('')
        }}
        onCreate={(values) =>
          client.quickCreate(referenceType, {
            referenceType,
            values,
          })
        }
        onCreated={(response) => {
          if (response.reference) {
            onChange(response.reference.referenceId)
          }
        }}
        testId={testId ? `${testId}-quick-create` : undefined}
      />
    </>
  )
}
