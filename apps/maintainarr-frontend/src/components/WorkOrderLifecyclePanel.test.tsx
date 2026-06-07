import { cleanup, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it } from 'vitest'
import { WorkOrderLifecyclePanel } from './WorkOrderLifecyclePanel'

const baseWorkOrder = {
  workOrderId: '33333333-3333-3333-3333-333333333333',
  workOrderNumber: 'WO-20260527-AB12CD34',
  assetId: '11111111-1111-1111-1111-111111111111',
  assetTag: 'FL-100',
  assetName: 'Forklift 100',
  defectId: null,
  defectTitle: null,
  pmScheduleId: null,
  pmScheduleName: null,
  title: 'Hydraulic repair',
  description: 'Leak at cylinder',
  priority: 'high',
  status: 'in_progress',
  source: 'manual',
  assignedTechnicianPersonId: 'person-tech-001',
  createdByUserId: 'user-001',
  createdAt: '2026-05-27T10:00:00Z',
  updatedAt: '2026-05-27T12:00:00Z',
  startedAt: '2026-05-27T11:00:00Z',
  completedAt: null,
  cancelledAt: null,
}

describe('WorkOrderLifecyclePanel', () => {
  afterEach(() => {
    cleanup()
  })

  it('shows empty state when no work order selected', () => {
    render(
      <WorkOrderLifecyclePanel
        workOrder={null}
        tasks={[]}
        labor={[]}
        evidence={[]}
        isDetailLoading={false}
      />,
    )
    expect(screen.getByTestId('work-order-lifecycle-empty')).toBeInTheDocument()
  })

  it('renders stepper, timestamps, and completion signals', () => {
    render(
      <WorkOrderLifecyclePanel
        workOrder={baseWorkOrder}
        tasks={[
          {
            taskLineId: 'task-1',
            workOrderId: baseWorkOrder.workOrderId,
            title: 'Inspect cylinder',
            description: '',
            sortOrder: 0,
            status: 'open',
            createdByUserId: 'user-001',
            createdAt: '2026-05-27T11:00:00Z',
            completedAt: null,
          },
        ]}
        labor={[
          {
            laborEntryId: 'labor-1',
            workOrderId: baseWorkOrder.workOrderId,
            workOrderTaskLineId: null,
            personId: 'person-tech-001',
            hoursWorked: 2,
            laborTypeKey: 'regular',
            status: 'submitted',
            notes: null,
            submittedAt: '2026-05-27T12:00:00Z',
            approvedByPersonId: null,
            approvedAt: null,
            rejectionReason: null,
            loggedByUserId: 'user-001',
            loggedAt: '2026-05-27T12:00:00Z',
          },
        ]}
        evidence={[
          {
            evidenceId: 'ev-1',
            workOrderId: baseWorkOrder.workOrderId,
            evidenceTypeKey: 'before_photo',
            fileName: 'before.jpg',
            contentType: 'image/jpeg',
            sizeBytes: 1024,
            notes: null,
            uploadedByUserId: 'user-001',
            createdAt: '2026-05-27T12:30:00Z',
          },
        ]}
        isDetailLoading={false}
      />,
    )

    expect(screen.getByTestId('work-order-lifecycle-stepper')).toBeInTheDocument()
    expect(screen.getByTestId('work-order-lifecycle-step-in_progress')).toBeInTheDocument()
    expect(screen.getByTestId('work-order-completion-signals')).toBeInTheDocument()
    expect(screen.getByTestId('work-order-signal-tasks')).toHaveTextContent('1')
    expect(screen.getByTestId('work-order-signal-labor')).toHaveTextContent('2.00')
    expect(screen.getByTestId('work-order-signal-evidence')).toHaveTextContent('1')
    expect(screen.getByTestId('work-order-lifecycle-completion-hint')).toHaveTextContent(
      /ready to mark completed/i,
    )
  })

  it('renders closeout summary with accepted evidence labels', () => {
    render(
      <WorkOrderLifecyclePanel
        workOrder={{
          ...baseWorkOrder,
          status: 'completed',
          closeout: {
            closeoutId: 'closeout-1',
            workOrderId: baseWorkOrder.workOrderId,
            completionSummary: 'Completed',
            rootCause: 'wear',
            correctiveAction: 'Replaced seal',
            preventiveActionRecommendation: 'Inspect next PM',
            assetReturnedToService: true,
            returnToServiceAt: '2026-05-27T14:00:00Z',
            returnToServiceByPersonId: 'person-tech-001',
            postRepairInspectionRequired: true,
            postRepairInspectionRef: 'inspect-123',
            supervisorReviewRequired: true,
            supervisorReviewedByPersonId: 'person-supervisor-001',
            supervisorReviewedAt: '2026-05-27T14:10:00Z',
            complianceReviewRequired: true,
            complianceReviewedByPersonId: 'person-compliance-001',
            complianceReviewedAt: '2026-05-27T14:15:00Z',
            qualityReviewRequired: true,
            qualityReviewedByPersonId: 'person-quality-001',
            qualityReviewedAt: '2026-05-27T14:20:00Z',
            evidenceAccepted: true,
            unresolvedDefectRefs: 'defect-1, defect-2',
            followUpWorkOrderRefs: 'wo-200, wo-201',
            customerImpactSummary: 'Limited production impact',
            downtimeSummary: '2.5 hours downtime',
            finalAssetReadinessStatus: 'ready',
            finalStatus: 'closed',
            evidenceRecordRefs: ['ev-1'],
            createdAt: '2026-05-27T14:05:00Z',
            createdByPersonId: 'person-tech-001',
          },
        }}
        tasks={[]}
        labor={[]}
        evidence={[
          {
            evidenceId: 'ev-1',
            workOrderId: baseWorkOrder.workOrderId,
            evidenceTypeKey: 'after_photo',
            fileName: 'after.jpg',
            contentType: 'image/jpeg',
            sizeBytes: 2048,
            notes: 'Post-repair evidence',
            uploadedByUserId: 'user-001',
            createdAt: '2026-05-27T13:45:00Z',
          },
        ]}
        isDetailLoading={false}
      />,
    )

    expect(screen.getByTestId('work-order-closeout-summary')).toBeInTheDocument()
    expect(screen.getByTestId('work-order-closeout-summary')).toHaveTextContent('Completed')
    expect(screen.getByTestId('work-order-closeout-summary')).toHaveTextContent('closed')
    expect(screen.getByTestId('work-order-closeout-summary')).toHaveTextContent('wear')
    expect(screen.getByTestId('work-order-closeout-summary')).toHaveTextContent('Replaced seal')
    expect(screen.getByTestId('work-order-closeout-summary')).toHaveTextContent('inspect-123')
    expect(screen.getByTestId('work-order-closeout-summary')).toHaveTextContent('person-tech-001')
    expect(screen.getByTestId('work-order-closeout-summary')).toHaveTextContent('person-supervisor-001')
    expect(screen.getByTestId('work-order-closeout-summary')).toHaveTextContent('person-compliance-001')
    expect(screen.getByTestId('work-order-closeout-summary')).toHaveTextContent('person-quality-001')
    expect(screen.getByTestId('work-order-closeout-summary')).toHaveTextContent('Limited production impact')
    expect(screen.getByTestId('work-order-closeout-summary')).toHaveTextContent('2.5 hours downtime')
    expect(screen.getByTestId('work-order-closeout-summary')).toHaveTextContent('Yes')
    expect(screen.getByTestId('work-order-closeout-unresolved-defects')).toHaveTextContent('defect-1')
    expect(screen.getByTestId('work-order-closeout-unresolved-defects')).toHaveTextContent('defect-2')
    expect(screen.getByTestId('work-order-closeout-followups')).toHaveTextContent('wo-200')
    expect(screen.getByTestId('work-order-closeout-followups')).toHaveTextContent('wo-201')
    expect(screen.getByTestId('work-order-closeout-evidence')).toHaveTextContent('after.jpg')
    expect(screen.getByTestId('work-order-closeout-evidence')).not.toHaveTextContent('ev-1')
  })

  it('shows capture hint when in progress without full signals', () => {
    render(
      <WorkOrderLifecyclePanel
        workOrder={baseWorkOrder}
        tasks={[]}
        labor={[]}
        evidence={[]}
        isDetailLoading={false}
      />,
    )
    expect(screen.getByTestId('work-order-lifecycle-completion-hint')).toHaveTextContent(
      /Add at least one task/i,
    )
  })
})
