import { useQuery } from '@tanstack/react-query'
import { Search } from 'lucide-react'
import { useEffect, useMemo, useState } from 'react'
import { useNavigate } from 'react-router-dom'

import { forgivingSearch } from '../api/client'
import type { ForgivingSearchResultItem } from '../api/types'

interface ForgivingSearchBarProps {
  accessToken: string
  canSearch: boolean
}

function formatEntityType(entityType: string): string {
  return entityType.replace(/_/g, ' ')
}

export function ForgivingSearchBar({ accessToken, canSearch }: ForgivingSearchBarProps) {
  const navigate = useNavigate()
  const [query, setQuery] = useState('')
  const [debouncedQuery, setDebouncedQuery] = useState('')
  const [isOpen, setIsOpen] = useState(false)

  useEffect(() => {
    const timer = window.setTimeout(() => setDebouncedQuery(query.trim()), 300)
    return () => window.clearTimeout(timer)
  }, [query])

  const enabled = canSearch && debouncedQuery.length >= 2

  const searchQuery = useQuery({
    queryKey: ['supplyarr-forgiving-search', accessToken, debouncedQuery],
    queryFn: () => forgivingSearch(accessToken, { q: debouncedQuery, limit: 20 }),
    enabled,
  })

  const results = useMemo(() => searchQuery.data?.results ?? [], [searchQuery.data?.results])

  if (!canSearch) {
    return null
  }

  return (
    <div className="relative" data-testid="forgiving-search-bar">
      <div className="flex items-center gap-2 rounded-lg border border-slate-700 bg-slate-900/80 px-3 py-2">
        <Search className="h-4 w-4 shrink-0 text-slate-400" aria-hidden />
        <input
          type="search"
          value={query}
          onChange={(event) => {
            setQuery(event.target.value)
            setIsOpen(true)
          }}
          onFocus={() => setIsOpen(true)}
          onBlur={() => {
            window.setTimeout(() => setIsOpen(false), 150)
          }}
          placeholder="Search vendors, parts, SKUs, PR/PO…"
          className="w-full min-w-[16rem] bg-transparent text-sm text-slate-100 placeholder:text-slate-500 focus:outline-none"
          aria-label="Forgiving search"
        />
      </div>

      {isOpen && debouncedQuery.length >= 2 && (
        <div className="absolute left-0 right-0 z-20 mt-2 max-h-80 overflow-y-auto rounded-lg border border-slate-700 bg-slate-950 shadow-xl">
          {searchQuery.isLoading && (
            <p className="px-3 py-2 text-sm text-slate-500">Searching…</p>
          )}
          {searchQuery.isError && (
            <p className="px-3 py-2 text-sm text-rose-400">Search failed. Try again.</p>
          )}
          {searchQuery.isSuccess && results.length === 0 && (
            <p className="px-3 py-2 text-sm text-slate-500">No matches for “{debouncedQuery}”.</p>
          )}
          {results.length > 0 && (
            <ul className="divide-y divide-slate-800 text-sm">
              {results.map((item: ForgivingSearchResultItem) => (
                <li key={`${item.entityType}-${item.entityId}`}>
                  <button
                    type="button"
                    className="w-full px-3 py-2 text-left hover:bg-slate-900"
                    onMouseDown={(event) => event.preventDefault()}
                    onClick={() => {
                      navigate(item.deepLinkPath)
                      setIsOpen(false)
                      setQuery('')
                    }}
                  >
                    <div className="font-medium text-slate-100">
                      {item.primaryKey} · {item.title}
                    </div>
                    <div className="text-xs capitalize text-slate-500">
                      {formatEntityType(item.entityType)} · {item.subtitle}
                    </div>
                  </button>
                </li>
              ))}
            </ul>
          )}
        </div>
      )}
    </div>
  )
}
