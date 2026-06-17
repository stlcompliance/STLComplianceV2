import { SmartImportReviewWorkspace } from '@stl/shared-ui'
import { useEffect, useState } from 'react'
import { useSearchParams } from 'react-router-dom'
import {
  applySmartImportMappingOverride,
  approveSmartImportCommitPlan,
  bulkReviewSmartImportRecords,
  commitSmartImportCommitPlan,
  createSmartImportBatch,
  createSmartImportCommitPlan,
  getSmartImportBatch,
  listSmartImportBatches,
  reviewSmartImportRecord,
  type SmartImportBatchDetail,
  type SmartImportBatchRow,
  type SmartImportManualFieldMapping,
} from '../api/nexarrClient'

export function SmartImportPage() {
  const [searchParams] = useSearchParams()
  const [batches, setBatches] = useState<SmartImportBatchRow[]>([])
  const [selectedBatch, setSelectedBatch] = useState<SmartImportBatchDetail | null>(null)
  const [isLoading, setIsLoading] = useState(false)
  const [errorMessage, setErrorMessage] = useState<string | null>(null)

  const refresh = async () => {
    setErrorMessage(null)
    setIsLoading(true)
    try {
      const next = await listSmartImportBatches()
      setBatches(next)
      if (selectedBatch) {
        setSelectedBatch(await getSmartImportBatch(selectedBatch.batch.batchId))
      }
    } catch (error) {
      setErrorMessage(error instanceof Error ? error.message : 'Smart Import failed to load.')
    } finally {
      setIsLoading(false)
    }
  }

  const selectBatch = async (batchId: string) => {
    setErrorMessage(null)
    setIsLoading(true)
    try {
      setSelectedBatch(await getSmartImportBatch(batchId))
    } catch (error) {
      setErrorMessage(error instanceof Error ? error.message : 'Smart Import batch failed to load.')
    } finally {
      setIsLoading(false)
    }
  }

  const upload = async (file: File, destinationProduct: string) => {
    setErrorMessage(null)
    setIsLoading(true)
    try {
      const created = await createSmartImportBatch(file, destinationProduct)
      const next = await listSmartImportBatches()
      setBatches(next)
      setSelectedBatch(await getSmartImportBatch(created.batchId))
    } catch (error) {
      setErrorMessage(error instanceof Error ? error.message : 'Smart Import upload failed.')
    } finally {
      setIsLoading(false)
    }
  }

  const review = async (
    proposedRecordId: string,
    decision: 'approved' | 'rejected' | 'needs_changes',
  ) => {
    if (!selectedBatch) return
    setErrorMessage(null)
    setIsLoading(true)
    try {
      await reviewSmartImportRecord(selectedBatch.batch.batchId, proposedRecordId, decision)
      setSelectedBatch(await getSmartImportBatch(selectedBatch.batch.batchId))
      setBatches(await listSmartImportBatches())
    } catch (error) {
      setErrorMessage(error instanceof Error ? error.message : 'Smart Import review failed.')
    } finally {
      setIsLoading(false)
    }
  }

  const approveAll = async (proposedRecordIds: string[]) => {
    if (!selectedBatch || proposedRecordIds.length === 0) return
    setErrorMessage(null)
    setIsLoading(true)
    try {
      await bulkReviewSmartImportRecords(selectedBatch.batch.batchId, proposedRecordIds, 'approved')
      setSelectedBatch(await getSmartImportBatch(selectedBatch.batch.batchId))
      setBatches(await listSmartImportBatches())
    } catch (error) {
      setErrorMessage(error instanceof Error ? error.message : 'Smart Import bulk approval failed.')
    } finally {
      setIsLoading(false)
    }
  }

  const applyMappingOverride = async (fieldMappings: SmartImportManualFieldMapping[]) => {
    if (!selectedBatch || fieldMappings.length === 0) return
    setErrorMessage(null)
    setIsLoading(true)
    try {
      await applySmartImportMappingOverride(selectedBatch.batch.batchId, fieldMappings)
      setSelectedBatch(await getSmartImportBatch(selectedBatch.batch.batchId))
      setBatches(await listSmartImportBatches())
    } catch (error) {
      setErrorMessage(error instanceof Error ? error.message : 'Smart Import mapping override failed.')
    } finally {
      setIsLoading(false)
    }
  }

  const createPlan = async (batchId: string) => {
    setErrorMessage(null)
    setIsLoading(true)
    try {
      await createSmartImportCommitPlan(batchId)
      setSelectedBatch(await getSmartImportBatch(batchId))
      setBatches(await listSmartImportBatches())
    } catch (error) {
      setErrorMessage(error instanceof Error ? error.message : 'Smart Import commit plan failed.')
    } finally {
      setIsLoading(false)
    }
  }

  const approvePlan = async (commitPlanId: string) => {
    if (!selectedBatch) return
    setErrorMessage(null)
    setIsLoading(true)
    try {
      await approveSmartImportCommitPlan(commitPlanId)
      setSelectedBatch(await getSmartImportBatch(selectedBatch.batch.batchId))
      setBatches(await listSmartImportBatches())
    } catch (error) {
      setErrorMessage(error instanceof Error ? error.message : 'Smart Import commit plan approval failed.')
    } finally {
      setIsLoading(false)
    }
  }

  const commitPlan = async (commitPlanId: string) => {
    if (!selectedBatch) return
    setErrorMessage(null)
    setIsLoading(true)
    try {
      await commitSmartImportCommitPlan(commitPlanId)
      setSelectedBatch(await getSmartImportBatch(selectedBatch.batch.batchId))
      setBatches(await listSmartImportBatches())
    } catch (error) {
      setErrorMessage(error instanceof Error ? error.message : 'Smart Import commit failed.')
    } finally {
      setIsLoading(false)
    }
  }

  useEffect(() => {
    void refresh()
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [])

  return (
    <div className="space-y-4">
      <div>
        <h1 className="text-2xl font-semibold text-white">Smart Import</h1>
        <p className="mt-1 max-w-3xl text-sm text-slate-300">
          Review retained source files, AI-assisted classifications, proposed records, and commit plans.
        </p>
      </div>

      {errorMessage ? (
        <div className="rounded-md border border-red-500/30 bg-red-500/10 px-4 py-3 text-sm text-red-100">
          {errorMessage}
        </div>
      ) : null}

      <SmartImportReviewWorkspace
        batches={batches}
        selectedBatch={selectedBatch}
        isLoading={isLoading}
        onRefresh={refresh}
        onSelectBatch={selectBatch}
        onUpload={upload}
        onReview={review}
        onApproveAll={approveAll}
        onApplyMappingOverride={applyMappingOverride}
        onCreateCommitPlan={createPlan}
        onApproveCommitPlan={approvePlan}
        onCommitPlan={commitPlan}
        initialDestinationProduct={searchParams.get('destinationProduct') ?? undefined}
      />
    </div>
  )
}
