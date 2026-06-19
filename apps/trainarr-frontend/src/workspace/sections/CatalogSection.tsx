import { BookOpen, CheckCircle2, Clock3, Layers, Search } from 'lucide-react'
import { Link } from 'react-router-dom'
import { useMemo, useState } from 'react'
import type { TrainArrWorkspaceState } from '../useTrainArrWorkspaceState'

type Props = { state: TrainArrWorkspaceState }

function formatDate(value: string | null | undefined): string {
  if (!value) return 'Not recorded'
  const date = new Date(value)
  return Number.isNaN(date.getTime()) ? 'Not recorded' : date.toLocaleDateString()
}

function StatCard({ label, value }: { label: string; value: string }) {
  return (
    <div className="rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-elevated)] p-4">
      <p className="text-xs uppercase tracking-wide text-[var(--color-text-muted)]">{label}</p>
      <p className="mt-1 text-2xl font-semibold text-[var(--color-text-primary)]">{value}</p>
    </div>
  )
}

export function CatalogSection({ state }: Props) {
  const [search, setSearch] = useState('')
  const [statusFilter, setStatusFilter] = useState('all')

  const programs = state.programsQuery.data ?? []
  const definitions = state.definitionsQuery.data ?? []
  const selectedProgram = state.programDetailQuery.data ?? null

  const filteredPrograms = useMemo(() => {
    const term = search.trim().toLowerCase()
    return programs.filter((program) => {
      const matchesStatus = statusFilter === 'all' || program.status === statusFilter
      const haystack = [program.name, program.programKey, program.status].join(' ').toLowerCase()
      return matchesStatus && (!term || haystack.includes(term))
    })
  }, [programs, search, statusFilter])

  const filteredDefinitions = useMemo(() => {
    const term = search.trim().toLowerCase()
    return definitions.filter((definition) => {
      const haystack = [
        definition.name,
        definition.definitionKey,
        definition.qualificationName,
        definition.qualificationKey,
        definition.status,
      ]
        .join(' ')
        .toLowerCase()
      return !term || haystack.includes(term)
    })
  }, [definitions, search])

  const selectedProgramId = state.selectedProgramId ?? filteredPrograms[0]?.programId ?? null
  const selectedSummary = filteredPrograms.find((program) => program.programId === selectedProgramId) ?? filteredPrograms[0] ?? null
  const selectedDefinitionCount = selectedProgram?.definitions.length ?? selectedSummary?.definitionCount ?? 0
  const activePrograms = programs.filter((program) => program.status === 'active').length
  const publishedVersions = programs.reduce((count, program) => count + program.publishedVersionCount, 0)

  return (
    <div className="space-y-6">
      <section className="rounded-2xl border border-[var(--color-border-subtle)] bg-gradient-to-br from-[var(--color-bg-surface)] to-[var(--color-bg-surface-elevated)] p-5">
        <div className="flex flex-wrap items-start justify-between gap-4">
          <div className="max-w-3xl">
            <p className="text-xs uppercase tracking-[0.25em] text-[var(--color-text-muted)]">TrainArr catalog</p>
            <h1 className="mt-2 text-2xl font-semibold text-[var(--color-text-primary)]">Training Catalog</h1>
            <p className="mt-2 text-sm text-[var(--color-text-secondary)]">
              Browse learning paths, course definitions, qualification ties, and version status. Open any course to
              inspect the structure or jump into the builder.
            </p>
          </div>
          <div className="flex flex-wrap gap-2">
            <Link className="rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-control)] px-3 py-2 text-sm text-[var(--color-text-primary)] hover:bg-[var(--color-bg-control-hover)]" to="/programs/drawer">
              Open builder
            </Link>
            <Link className="rounded-lg bg-[var(--color-accent)] px-3 py-2 text-sm font-semibold text-white hover:bg-[var(--color-accent-hover)]" to="/assignments/queue">
              Open course player
            </Link>
          </div>
        </div>

        <div className="mt-5 grid gap-3 md:grid-cols-3">
          <StatCard label="Active courses" value={String(activePrograms)} />
          <StatCard label="Published versions" value={String(publishedVersions)} />
          <StatCard label="Definitions" value={String(definitions.length)} />
        </div>
      </section>

      <section className="rounded-2xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-4">
        <div className="grid gap-3 md:grid-cols-[1fr_220px]">
          <label htmlFor="trainarr-catalog-search" className="flex items-center gap-2 rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-elevated)] px-3 py-2 text-sm text-[var(--color-text-secondary)]">
            <Search className="h-4 w-4" />
            <input
              id="trainarr-catalog-search"
              value={search}
              onChange={(event) => setSearch(event.target.value)}
              className="w-full bg-transparent outline-none placeholder:text-[var(--color-text-muted)]"
              placeholder="Search courses, keys, or qualifications"
            />
          </label>
          <label htmlFor="trainarr-catalog-status" className="block">
            <span className="sr-only">Status filter</span>
            <select
              id="trainarr-catalog-status"
              value={statusFilter}
              onChange={(event) => setStatusFilter(event.target.value)}
              className="w-full rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-elevated)] px-3 py-2 text-sm text-[var(--color-text-primary)]"
            >
              <option value="all">All statuses</option>
              <option value="active">Active</option>
              <option value="draft">Draft</option>
              <option value="paused">Paused</option>
              <option value="retired">Retired</option>
            </select>
          </label>
        </div>
      </section>

      <div className="grid gap-6 xl:grid-cols-[minmax(0,1.1fr)_minmax(0,0.9fr)]">
        <section className="space-y-4">
          <div className="flex items-center gap-2">
            <BookOpen className="h-5 w-5 text-[var(--color-link-text)]" />
            <h2 className="text-lg font-semibold text-[var(--color-text-primary)]">Courses</h2>
          </div>
          {filteredPrograms.length === 0 ? (
            <p className="text-sm text-[var(--color-text-muted)]">No courses match this filter.</p>
          ) : (
            <div className="grid gap-3">
              {filteredPrograms.map((program) => (
                <button
                  key={program.programId}
                  type="button"
                  onClick={() => state.setSelectedProgramId(program.programId)}
                  className={`rounded-2xl border p-4 text-left transition ${
                    program.programId === selectedProgramId
                      ? 'border-[var(--color-accent-border)] bg-[var(--color-accent-soft)]'
                      : 'border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] hover:border-[var(--color-accent-border)]'
                  }`}
                >
                  <div className="flex flex-wrap items-start justify-between gap-3">
                    <div>
                      <p className="text-xs uppercase tracking-wide text-[var(--color-text-muted)]">{program.programKey}</p>
                      <h3 className="mt-1 text-base font-semibold text-[var(--color-text-primary)]">{program.name}</h3>
                      <p className="mt-1 text-sm text-[var(--color-text-secondary)]">
                        {program.definitionCount} definitions · {program.publishedVersionCount} published versions
                      </p>
                    </div>
                    <span className="rounded-full border border-[var(--color-border-subtle)] bg-[var(--color-bg-control)] px-3 py-1 text-xs text-[var(--color-text-secondary)]">
                      {program.status}
                    </span>
                  </div>
                  <div className="mt-3 flex flex-wrap items-center gap-3 text-xs text-[var(--color-text-muted)]">
                    <span className="inline-flex items-center gap-1">
                      <CheckCircle2 className="h-3.5 w-3.5" />
                      {program.status === 'active' ? 'Assignable' : 'Review needed'}
                    </span>
                    <span className="inline-flex items-center gap-1">
                      <Clock3 className="h-3.5 w-3.5" />
                      Updated {formatDate(program.updatedAt)}
                    </span>
                  </div>
                </button>
              ))}
            </div>
          )}
        </section>

        <section className="space-y-4">
          <div className="flex items-center gap-2">
            <Layers className="h-5 w-5 text-[var(--color-link-text)]" />
            <h2 className="text-lg font-semibold text-[var(--color-text-primary)]">Course detail</h2>
          </div>
          {selectedProgramId ? (
            <div className="rounded-2xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-5">
              <div className="flex flex-wrap items-start justify-between gap-3">
                <div>
                  <p className="text-xs uppercase tracking-wide text-[var(--color-text-muted)]">
                    {selectedSummary?.programKey ?? selectedProgram?.programKey}
                  </p>
                  <h3 className="mt-1 text-xl font-semibold text-[var(--color-text-primary)]">
                    {selectedSummary?.name ?? selectedProgram?.name}
                  </h3>
                  <p className="mt-2 text-sm text-[var(--color-text-secondary)]">
                    {selectedProgram?.description ?? 'Selected course from the catalog.'}
                  </p>
                </div>
                <span className="rounded-full border border-[var(--color-border-subtle)] bg-[var(--color-bg-control)] px-3 py-1 text-xs text-[var(--color-text-secondary)]">
                  {selectedSummary?.status ?? selectedProgram?.status}
                </span>
              </div>

              <div className="mt-4 grid gap-3 md:grid-cols-3">
                <StatCard label="Definitions" value={String(selectedDefinitionCount)} />
                <StatCard label="Published versions" value={String(selectedSummary?.publishedVersionCount ?? 0)} />
                <StatCard label="Updated" value={formatDate(selectedSummary?.updatedAt ?? selectedProgram?.updatedAt)} />
              </div>

              <div className="mt-4 flex flex-wrap gap-2">
                <Link
                  className="rounded-lg bg-[var(--color-accent)] px-3 py-2 text-sm font-semibold text-white hover:bg-[var(--color-accent-hover)]"
                  to="/programs/details"
                >
                  Open full detail
                </Link>
                <Link
                  className="rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-control)] px-3 py-2 text-sm text-[var(--color-text-primary)] hover:bg-[var(--color-bg-control-hover)]"
                  to="/programs/drawer"
                >
                  Edit course
                </Link>
              </div>
            </div>
          ) : (
            <div className="rounded-2xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-5">
              <p className="text-sm text-[var(--color-text-muted)]">Select a course to inspect its structure and requirements.</p>
            </div>
          )}

          <div className="rounded-2xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-5">
            <div className="flex items-center gap-2">
              <CheckCircle2 className="h-5 w-5 text-[var(--color-success)]" />
              <h2 className="text-lg font-semibold text-[var(--color-text-primary)]">Definitions</h2>
            </div>
            <p className="mt-1 text-sm text-[var(--color-text-secondary)]">
              Reusable course units that can be assigned directly or linked into programs.
            </p>
            <div className="mt-4 space-y-2">
              {filteredDefinitions.slice(0, 6).map((definition) => (
                <div key={definition.trainingDefinitionId} className="rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-elevated)] p-3">
                  <div className="flex flex-wrap items-start justify-between gap-3">
                    <div>
                      <p className="font-medium text-[var(--color-text-primary)]">{definition.name}</p>
                      <p className="mt-1 text-xs text-[var(--color-text-muted)]">
                        {definition.definitionKey} · {definition.qualificationName}
                      </p>
                    </div>
                    <span className="rounded-full border border-[var(--color-border-subtle)] bg-[var(--color-bg-control)] px-2 py-1 text-xs text-[var(--color-text-secondary)]">
                      {definition.status}
                    </span>
                  </div>
                </div>
              ))}
            </div>
          </div>
        </section>
      </div>
    </div>
  )
}
