import { useQuery, useQueryClient } from '@tanstack/react-query'
import { useCallback, useEffect, useMemo, useState } from 'react'

import { getFieldTaskSubmissionStatus } from '../api/client'
import type { FieldTaskSubmissionStatusItem } from '../api/types'
import {
  dismissSubmissionToast,
  getLocalSubmission,
  getSubmissionToasts,
  mergeSubmissionChips,
  subscribeSubmissionState,
  type MergedSubmissionChip,
  type SubmissionToast,
} from '../lib/submissionState'

export function useFieldTaskSubmissionState(accessToken: string, taskKeys: string[]) {
  const queryClient = useQueryClient()
  const [localTick, setLocalTick] = useState(0)

  useEffect(() => {
    return subscribeSubmissionState(() => setLocalTick((value) => value + 1))
  }, [])

  const sortedKeys = useMemo(
    () => [...new Set(taskKeys)].sort().join(','),
    [taskKeys],
  )

  const serverQuery = useQuery({
    queryKey: ['fieldcompanion-submission-status', accessToken, sortedKeys],
    queryFn: () => getFieldTaskSubmissionStatus(accessToken, taskKeys),
    enabled: Boolean(accessToken) && taskKeys.length > 0,
    staleTime: 15_000,
  })

  const serverByTask = useMemo(() => {
    const map = new Map<string, FieldTaskSubmissionStatusItem[]>()
    for (const item of serverQuery.data?.items ?? []) {
      const existing = map.get(item.taskKey) ?? []
      existing.push(item)
      map.set(item.taskKey, existing)
    }
    return map
  }, [serverQuery.data?.items])

  const getChips = useCallback(
    (taskKey: string): MergedSubmissionChip[] => {
      void localTick
      return mergeSubmissionChips({
        taskKey,
        acknowledgeLocal: getLocalSubmission(taskKey, 'acknowledge'),
        evidenceLocal: getLocalSubmission(taskKey, 'evidence'),
        dvirLocal: getLocalSubmission(taskKey, 'dvir'),
        inspectionLocal: getLocalSubmission(taskKey, 'inspection'),
        workOrderLocal: getLocalSubmission(taskKey, 'work-order'),
        receivingLocal: getLocalSubmission(taskKey, 'receiving'),
        serverItems: serverByTask.get(taskKey) ?? [],
      })
    },
    [localTick, serverByTask],
  )

  const toasts = useMemo((): SubmissionToast[] => {
    void localTick
    return getSubmissionToasts()
  }, [localTick])

  const refreshServerStatus = useCallback(() => {
    void queryClient.invalidateQueries({ queryKey: ['fieldcompanion-submission-status', accessToken] })
  }, [accessToken, queryClient])

  const dismissToast = useCallback((id: string) => {
    dismissSubmissionToast(id)
    setLocalTick((value) => value + 1)
  }, [])

  return {
    getChips,
    toasts,
    dismissToast,
    refreshServerStatus,
    isLoadingServer: serverQuery.isLoading,
  }
}
