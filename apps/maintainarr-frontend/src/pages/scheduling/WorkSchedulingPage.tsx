import { useMemo, useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { ApiErrorCallout, SchedulingBoard, getErrorMessage } from '@stl/shared-ui'
import type {
  SchedulingConflict,
  SchedulingDisplayItem,
  SchedulingResourceAssignment,
  SchedulingResourceLane,
  SchedulingSourceReference,
  SchedulingWindow,
} from '@stl/shared-ui'
import { useNavigate } from 'react-router-dom'

import {
  cancelScheduledWork,
  completeScheduledWork,
  getSchedulingResources,
  getSchedulingScheduled,
  getSchedulingUnscheduled,
  rescheduleWork,
  scheduleWork,
  unscheduleWork,
} from '../../api/client'
import type {
  SchedulingConflictResponse,
  SchedulingDisplayItemResponse,
  SchedulingRequest,
  SchedulingResourceAssignmentResponse,
  SchedulingResourceLaneResponse,
  SchedulingSourceReferenceResponse,
} from '../../api/types'
import { loadSession } from '../../auth/sessionStorage'

const emptyTenantId = '00000000-0000-0000-0000-000000000000'

function toIso(value: string | null | undefined): string | null {
  if (!value) return null
  const date = new Date(value)
  return Number.isNaN(date.getTime()) ? value : date.toISOString()
}

function plusHours(value: Date, hours: number): string {
  const copy = new Date(value)
  copy.setHours(copy.getHours() + hours)
  return copy.toISOString()
}

function newId(): string {
  return globalThis.crypto?.randomUUID?.() ?? `id-${Date.now()}-${Math.random().toString(16).slice(2)}`
}

function mapWindow(window: SchedulingDisplayItemResponse['requestedWindow']): SchedulingWindow | null {
  if (!window) return null
  return {
    startAt: window.start,
    endAt: window.end,
    timeZone: window.timezone,
  }
}

function mapResource(assignment: SchedulingResourceAssignmentResponse): SchedulingResourceAssignment {
  return {
    resourceId: assignment.resourceId,
    label: assignment.displayName ?? assignment.resourceId,
    productKey: assignment.sourceProductKey,
    role: assignment.role,
  }
}

function mapConflict(conflict: SchedulingConflictResponse): SchedulingConflict {
  return {
    conflictType: conflict.code || conflict.conflictType,
    severity: conflict.severity,
    message: conflict.message,
    sourceReference: conflict.sourceProductKey && conflict.sourceObjectType && conflict.sourceObjectId
      ? {
          productKey: conflict.sourceProductKey,
          resourceType: conflict.sourceObjectType,
          resourceId: conflict.sourceObjectId,
        }
      : null,
  }
}

function mapSourceRef(
  ref: SchedulingSourceReferenceResponse,
  item: SchedulingDisplayItemResponse,
): SchedulingSourceReference {
  return {
    productKey: ref.productKey,
    resourceType: ref.objectType,
    resourceId: ref.objectId,
    label: ref.objectNumber ?? `${ref.objectType} ${ref.objectId}`,
    href: item.owningProductUrl,
  }
}

function mapItem(item: SchedulingDisplayItemResponse): SchedulingDisplayItem {
  return {
    itemId: item.itemId,
    productKey: item.productKey,
    itemType: item.itemType,
    title: item.subtitle ? `${item.title} - ${item.subtitle}` : item.title,
    status: item.currentStatus,
    priority: item.priority,
    requestedWindow: mapWindow(item.requestedWindow),
    promisedWindow: mapWindow(item.promisedWindow),
    scheduledWindow: mapWindow(item.scheduledWindow),
    resourceAssignments: item.assignedResources.map(mapResource),
    resourceNeeds: item.resourceNeeds.map(mapResource),
    sourceReferences: item.sourceRefs.map((sourceRef) => mapSourceRef(sourceRef, item)),
    blockers: [...item.blockers, ...item.warnings].map(mapConflict),
    allowedActions: item.allowedActions as SchedulingDisplayItem['allowedActions'],
    permissions: item.permissionFlags as SchedulingDisplayItem['permissions'],
  }
}

function mapLane(resource: SchedulingResourceLaneResponse): SchedulingResourceLane {
  return {
    resourceId: resource.resourceId,
    label: resource.displayName,
    productKey: resource.productKey,
    status: resource.status,
  }
}

function firstWindowValue(item: SchedulingDisplayItem, field: 'startAt' | 'endAt'): string | null {
  return (
    toIso(item.scheduledWindow?.[field]) ??
    toIso(item.promisedWindow?.[field]) ??
    toIso(item.requestedWindow?.[field])
  )
}

function buildRequest(item: SchedulingDisplayItem, action: string): SchedulingRequest {
  const start = firstWindowValue(item, 'startAt') ?? plusHours(new Date(), 1)
  const end = firstWindowValue(item, 'endAt') ?? plusHours(new Date(start), 2)
  const resourceAssignments = (item.resourceAssignments ?? []).map((resource) => ({
    resourceType: resource.role ?? 'technician',
    resourceId: resource.resourceId,
    sourceProductKey: resource.productKey ?? 'staffarr',
    displayName: resource.label,
    role: resource.role ?? 'primary_technician',
  }))

  return {
    tenantId: emptyTenantId,
    productKey: item.productKey,
    itemType: item.itemType,
    itemId: item.itemId,
    requestedStart: action === 'unschedule' || action === 'cancel' ? null : start,
    requestedEnd: action === 'unschedule' || action === 'cancel' ? null : end,
    timezone: 'UTC',
    resourceAssignments,
    locationAssignments: [],
    assetAssignments: [],
    reason: action,
    correlationId: newId(),
    idempotencyKey: `${action}-${item.itemId}-${newId()}`,
    sourceContext: (item.sourceReferences ?? []).map((source) => ({
      productKey: source.productKey,
      objectType: source.resourceType,
      objectId: source.resourceId,
      objectNumber: source.label ?? null,
    })),
    override: null,
    validationOnly: false,
  }
}

export function WorkSchedulingPage() {
  const session = loadSession()
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const [mutationConflicts, setMutationConflicts] = useState<SchedulingConflict[]>([])

  const unscheduledQuery = useQuery({
    queryKey: ['maintainarr-scheduling-unscheduled', session?.accessToken],
    queryFn: () => getSchedulingUnscheduled(session!.accessToken),
    enabled: Boolean(session?.accessToken),
  })
  const scheduledQuery = useQuery({
    queryKey: ['maintainarr-scheduling-scheduled', session?.accessToken],
    queryFn: () => getSchedulingScheduled(session!.accessToken),
    enabled: Boolean(session?.accessToken),
  })
  const resourcesQuery = useQuery({
    queryKey: ['maintainarr-scheduling-resources', session?.accessToken],
    queryFn: () => getSchedulingResources(session!.accessToken),
    enabled: Boolean(session?.accessToken),
  })

  const refresh = async () => {
    await Promise.all([
      queryClient.invalidateQueries({ queryKey: ['maintainarr-scheduling-unscheduled', session?.accessToken] }),
      queryClient.invalidateQueries({ queryKey: ['maintainarr-scheduling-scheduled', session?.accessToken] }),
      queryClient.invalidateQueries({ queryKey: ['maintainarr-scheduling-resources', session?.accessToken] }),
    ])
  }

  const mutation = useMutation({
    mutationFn: async ({ item, action }: { item: SchedulingDisplayItem; action: 'schedule' | 'reschedule' | 'unschedule' | 'cancel' | 'complete' }) => {
      const request = buildRequest(item, action)
      if (action === 'schedule') return scheduleWork(session!.accessToken, request)
      if (action === 'reschedule') return rescheduleWork(session!.accessToken, request)
      if (action === 'unschedule') return unscheduleWork(session!.accessToken, request)
      if (action === 'cancel') return cancelScheduledWork(session!.accessToken, request)
      return completeScheduledWork(session!.accessToken, request)
    },
    onSuccess: async (response) => {
      setMutationConflicts([...response.validation.blockers, ...response.validation.warnings].map(mapConflict))
      await refresh()
    },
  })

  const unscheduledItems = useMemo(
    () => (unscheduledQuery.data?.items ?? []).map(mapItem),
    [unscheduledQuery.data?.items],
  )
  const scheduledItems = useMemo(
    () => (scheduledQuery.data?.items ?? []).map(mapItem),
    [scheduledQuery.data?.items],
  )
  const resources = useMemo(
    () => (resourcesQuery.data ?? scheduledQuery.data?.resources ?? []).map(mapLane),
    [resourcesQuery.data, scheduledQuery.data?.resources],
  )
  const loadError = unscheduledQuery.error ?? scheduledQuery.error ?? resourcesQuery.error
  const isLoading = unscheduledQuery.isLoading || scheduledQuery.isLoading || resourcesQuery.isLoading

  if (!session?.accessToken) {
    return (
      <ApiErrorCallout
        title="Workspace session required"
        message="Launch MaintainArr from the suite to open scheduling."
      />
    )
  }

  return (
    <div className="space-y-4">
      {loadError ? (
        <ApiErrorCallout
          title="Unable to load scheduling"
          message={getErrorMessage(loadError)}
          onRetry={() => void refresh()}
        />
      ) : null}
      {mutation.isError ? (
        <ApiErrorCallout
          title="Unable to update schedule"
          message={getErrorMessage(mutation.error)}
          onRetry={() => mutation.reset()}
        />
      ) : null}
      <SchedulingBoard
        title="Work scheduling"
        unscheduledItems={unscheduledItems}
        scheduledItems={scheduledItems}
        resources={resources}
        isLoading={isLoading || mutation.isPending}
        conflicts={mutationConflicts}
        onRefresh={refresh}
        onSchedule={(item) => mutation.mutate({ item, action: 'schedule' })}
        onReschedule={(item) => mutation.mutate({ item, action: 'reschedule' })}
        onUnschedule={(item) => mutation.mutate({ item, action: 'unschedule' })}
        onCancel={(item) => mutation.mutate({ item, action: 'cancel' })}
        onComplete={(item) => mutation.mutate({ item, action: 'complete' })}
        onOpenProductRecord={(reference) => {
          if (reference.href) {
            navigate(reference.href)
          }
        }}
      />
    </div>
  )
}
