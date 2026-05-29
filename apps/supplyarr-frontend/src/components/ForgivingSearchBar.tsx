import { useMemo } from 'react'
import { useNavigate } from 'react-router-dom'

import { AsyncSearchPicker } from '@stl/shared-ui'

import { forgivingSearch } from '../api/client'

interface ForgivingSearchBarProps {
  accessToken: string
  canSearch: boolean
}

export function ForgivingSearchBar({ accessToken, canSearch }: ForgivingSearchBarProps) {
  const navigate = useNavigate()

  const queryKey = useMemo(() => ['supplyarr-forgiving-search', accessToken] as const, [accessToken])

  if (!canSearch) {
    return null
  }

  return (
    <div data-testid="forgiving-search-bar">
      <AsyncSearchPicker
        value=""
        onChange={(deepLinkPath) => {
          navigate(deepLinkPath)
        }}
        queryKey={queryKey}
        queryFn={async (query) => {
          const response = await forgivingSearch(accessToken, { q: query, limit: 20 })
          return response.results.map((item) => ({
            value: item.deepLinkPath,
            label: `${item.primaryKey} · ${item.title}`,
          }))
        }}
        placeholder="Search vendors, parts, SKUs, PR/PO…"
        testId="forgiving-search-picker"
      />
    </div>
  )
}
