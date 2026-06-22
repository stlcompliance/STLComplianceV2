import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { render, screen } from '@testing-library/react'
import type { ReactNode } from 'react'
import { MemoryRouter } from 'react-router-dom'
import { describe, expect, it, vi } from 'vitest'

vi.mock('@stl/shared-ui', () => {
  return {
    StaticSearchPicker: ({
      label,
      value,
      options,
      onChange,
      placeholder,
      testId,
    }: {
      label?: string
      value: string
      options: Array<{ value: string; label: string }>
      onChange: (value: string) => void
      placeholder?: string
      testId?: string
    }) => (
      <label>
        {label ? <span>{label}</span> : null}
        <select
          aria-label={label ?? placeholder ?? 'Static search picker'}
          data-testid={testId}
          value={value}
          onChange={(event) => onChange(event.target.value)}
        >
          <option value="">{placeholder ?? 'Select…'}</option>
          {options.map((option) => (
            <option key={option.value} value={option.value}>
              {option.label}
            </option>
          ))}
        </select>
      </label>
    ),
    ControlledSelect: ({
      label,
      value,
      options,
      onChange,
      emptyLabel,
      testId,
      className,
    }: {
      label?: string
      value: string
      options: Array<{ value: string; label: string }>
      onChange: (value: string) => void
      emptyLabel?: string
      testId?: string
      className?: string
    }) => (
      <label>
        {label ? <span>{label}</span> : null}
        <select
          aria-label={label ?? emptyLabel ?? 'Controlled select'}
          data-testid={testId}
          className={className}
          value={value}
          onChange={(event) => onChange(event.target.value)}
        >
          <option value="">{emptyLabel ?? 'Select…'}</option>
          {options.map((option) => (
            <option key={option.value} value={option.value}>
              {option.label}
            </option>
          ))}
        </select>
      </label>
    ),
    ReferenceProviderClient: class ReferenceProviderClient {
      constructor(_options: unknown) {}
    },
    ReferencePicker: ({
      value,
      onChange,
      placeholder,
      disabled,
    }: {
      value: { displayLabelSnapshot?: string } | null
      onChange: (value: { displayLabelSnapshot?: string } | null) => void
      placeholder?: string
      disabled?: boolean
    }) => (
      <input
        aria-label={placeholder ?? 'Reference picker'}
        disabled={disabled}
        value={value?.displayLabelSnapshot ?? ''}
        onChange={(event) => onChange(event.target.value ? { displayLabelSnapshot: event.target.value } : null)}
      />
    ),
    DetailBadge: ({ label }: { label: string }) => <span>{label}</span>,
    DetailEmptyState: ({ text }: { text: string }) => <p>{text}</p>,
    ProfileDetailsLayout: ({
      testId,
      title,
      subtitle,
      mainContent,
      railSections,
      decisionTitle,
      decisionSummary,
      decisionDetail,
    }: {
      testId?: string
      title: string
      subtitle?: ReactNode
      mainContent?: ReactNode
      railSections?: Array<{ title: string; content?: ReactNode }>
      decisionTitle?: string
      decisionSummary?: string
      decisionDetail?: string
    }) => (
      <div data-testid={testId}>
        <h1>{title}</h1>
        {subtitle}
        {decisionTitle ? <h2>{decisionTitle}</h2> : null}
        {decisionSummary ? <p>{decisionSummary}</p> : null}
        {decisionDetail ? <p>{decisionDetail}</p> : null}
        <div>{mainContent}</div>
        <div>
          {railSections?.map((section) => (
            <section key={section.title}>
              <h3>{section.title}</h3>
              <div>{section.content}</div>
            </section>
          ))}
        </div>
      </div>
    ),
  }
})

import { WorkOrderProfile } from './MaintenanceDetailProfiles'

describe('WorkOrderProfile', () => {
  it('renders supply readiness inside the work order detail profile', () => {
    const state = {
      selectedWorkOrderId: 'wo-1',
      workOrdersQuery: {
        data: [
          {
            workOrderId: 'wo-1',
            workOrderNumber: 'WO-100',
            assetTag: 'FLT-01',
            assetName: 'Forklift 01',
            title: 'Replace brake pads',
            priority: 'high',
            status: 'open',
            source: 'defect',
            assignedTechnicianPersonId: 'person-1',
            defectId: 'def-1',
            pmScheduleId: null,
            createdAt: '2026-05-29T00:00:00Z',
          },
        ],
      },
      workOrderDetailQuery: {
        data: {
          workOrderId: 'wo-1',
          workOrderNumber: 'WO-100',
          assetId: 'asset-1',
          assetTag: 'FLT-01',
          assetName: 'Forklift 01',
          defectId: 'def-1',
          defectTitle: 'Brake issue',
          pmScheduleId: null,
          pmScheduleName: null,
          title: 'Replace brake pads',
          description: 'Replace pads and inspect calipers',
          priority: 'high',
          status: 'open',
          source: 'defect',
          assignedTechnicianPersonId: 'person-1',
          createdByUserId: 'user-1',
          createdAt: '2026-05-29T00:00:00Z',
          updatedAt: '2026-05-29T00:00:00Z',
          startedAt: null,
          completedAt: null,
          cancelledAt: null,
          closeout: {
            closeoutId: 'closeout-1',
            workOrderId: 'wo-1',
            completionSummary: 'Completed repair and returned to service.',
            rootCause: 'wear',
            correctiveAction: 'Replaced brake pads',
            preventiveActionRecommendation: 'Inspect in next PM',
            assetReturnedToService: true,
            returnToServiceAt: '2026-05-29T18:00:00Z',
            returnToServiceByPersonId: 'person-1',
            postRepairInspectionRequired: true,
            postRepairInspectionRef: 'inspection-1',
            supervisorReviewRequired: true,
            supervisorReviewedByPersonId: 'person-supervisor-1',
            supervisorReviewedAt: '2026-05-29T18:15:00Z',
            complianceReviewRequired: false,
            complianceReviewedByPersonId: null,
            complianceReviewedAt: null,
            qualityReviewRequired: false,
            qualityReviewedByPersonId: null,
            qualityReviewedAt: null,
            evidenceAccepted: true,
            unresolvedDefectRefs: 'defect-a; defect-b',
            followUpWorkOrderRefs: 'wo-200, wo-201',
            customerImpactSummary: 'Minor production slowdown',
            downtimeSummary: '2.5 hours downtime',
            finalAssetReadinessStatus: 'ready',
            finalStatus: 'closed',
            evidenceRecordRefs: ['evidence-1'],
            createdAt: '2026-05-29T18:20:00Z',
            createdByPersonId: 'person-1',
          },
        },
      },
      workOrderTasksQuery: {
        data: [
          {
            taskLineId: 'task-1',
            title: 'Inspect pads',
            status: 'completed',
          },
        ],
      },
      workOrderLaborQuery: {
        data: [],
      },
      workOrderCommentsQuery: {
        data: [],
      },
      workOrderTimelineQuery: {
        data: [],
      },
      workOrderEvidenceQuery: {
        data: [
          {
            evidenceId: 'evidence-1',
            workOrderId: 'wo-1',
            evidenceTypeKey: 'after_photo',
            fileName: 'after.jpg',
            contentType: 'image/jpeg',
            sizeBytes: 1024,
            notes: null,
            uploadedByUserId: 'user-1',
            createdAt: '2026-05-29T17:45:00Z',
          },
        ],
      },
      canExecuteInspections: true,
      canApproveLabor: true,
      session: {
        personId: 'person-1',
      },
      technicianRefs: [
        {
          personId: 'person-1',
          displayName: 'Alex Tech',
          activeStatus: 'active',
          primarySite: 'Central Maintenance',
        },
      ],
      woCommentBody: '',
      woCommentVisibility: 'internal',
      woCommentPinned: false,
      setWoCommentBody: () => {},
      setWoCommentVisibility: () => {},
      setWoCommentPinned: () => {},
      addWorkOrderCommentMutation: {
        isPending: false,
        mutate: () => {},
      },
      woTaskTitle: '',
      setWoTaskTitle: () => {},
      woLaborHours: '1',
      setWoLaborHours: () => {},
      woLaborTypeKey: 'regular',
      setWoLaborTypeKey: () => {},
      woLaborPersonId: 'person-1',
      setWoLaborPersonId: () => {},
      woSelectedTaskLineId: '',
      setWoSelectedTaskLineId: () => {},
      woEvidenceTypeKey: 'completion_photo',
      setWoEvidenceTypeKey: () => {},
      woEvidenceNotes: '',
      setWoEvidenceNotes: () => {},
      woEvidenceFile: null,
      setWoEvidenceFile: () => {},
      addWorkOrderTaskMutation: {
        isPending: false,
        mutate: () => {},
      },
      logWorkOrderLaborMutation: {
        isPending: false,
        mutate: () => {},
      },
      approveWorkOrderLaborMutation: {
        isPending: false,
        mutate: () => {},
      },
      rejectWorkOrderLaborMutation: {
        isPending: false,
        mutate: () => {},
      },
      uploadWorkOrderEvidenceMutation: {
        isPending: false,
        mutate: () => {},
      },
      workOrderPartsDemandQuery: {
        data: [
          {
            demandLineId: 'demand-1',
          },
        ],
      },
      workOrderSupplyReadinessQuery: {
        isLoading: false,
        data: {
          workOrderId: 'wo-1',
          workOrderNumber: 'WO-100',
          generatedAt: '2026-05-29T00:00:00Z',
          overallReadinessStatus: 'not_ready',
          totalDemandLines: 1,
          linesChecked: 1,
          linesReady: 0,
          linesBlocked: 1,
          linesSkipped: 0,
          lines: [
            {
              demandLineId: 'demand-1',
              lineNumber: 1,
              supplyarrPartId: 'part-1',
              partNumber: 'BRK-001',
              quantityRequested: 2,
              lineStatus: 'pending',
              readinessStatus: 'not_ready',
              readinessBasis: 'availability',
              skipReason: null,
              quantityAvailable: 0,
              calculatedAt: '2026-05-29T00:00:00Z',
              blockers: [
                {
                  reasonCode: 'part_stockout',
                  message: 'Insufficient available quantity.',
                  sourceEntityType: 'part_stock',
                  sourceEntityId: 'part-1',
                  relatedEntityId: null,
                },
              ],
            },
          ],
        },
      },
    } as any

    const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })

    render(
      <QueryClientProvider client={queryClient}>
        <MemoryRouter>
          <WorkOrderProfile state={state} />
        </MemoryRouter>
      </QueryClientProvider>,
    )

    expect(screen.getByText('Supply readiness')).toBeInTheDocument()
    expect(screen.getByText('Vendor coordination')).toBeInTheDocument()
    expect(screen.getByText('Labor and evidence')).toBeInTheDocument()
    expect(screen.getByText('Closeout')).toBeInTheDocument()
    expect(screen.getByTestId('work-order-supply-readiness-panel')).toBeInTheDocument()
    expect(screen.getByTestId('work-order-supply-readiness-overall')).toHaveTextContent('Supply blocked')
    expect(screen.getAllByText('after.jpg')).toHaveLength(2)
    expect(screen.getByText('Completed repair and returned to service.')).toBeInTheDocument()
  })
})
