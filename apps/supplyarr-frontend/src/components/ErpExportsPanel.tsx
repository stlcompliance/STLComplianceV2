import { useMutation } from '@tanstack/react-query'
import { FileDown } from 'lucide-react'
import { ApiErrorCallout, getErrorMessage } from '@stl/shared-ui'

type ExportTarget = {
  key: string
  label: string
  description: string
  path: string
  filenamePrefix: string
}

const exportTargets: ExportTarget[] = [
  {
    key: 'purchase-orders',
    label: 'Purchase orders',
    description: 'Issued purchase order lines for ERP/AP reconciliation.',
    path: '/api/v1/exports/purchase-orders.csv',
    filenamePrefix: 'supplyarr-erp-purchase-orders',
  },
  {
    key: 'receipts',
    label: 'Receipts',
    description: 'Posted receiving lines for inventory and ERP posting.',
    path: '/api/v1/exports/receipts.csv',
    filenamePrefix: 'supplyarr-erp-receipts',
  },
  {
    key: 'invoice-support',
    label: 'Invoice support',
    description: 'Accounting support rows for invoice matching and reconciliation.',
    path: '/api/v1/exports/invoice-support.csv',
    filenamePrefix: 'supplyarr-erp-invoice-support',
  },
  {
    key: 'spend',
    label: 'Spend report',
    description: 'Spend export for finance and ERP import pipelines.',
    path: '/api/v1/exports/spend.csv',
    filenamePrefix: 'supplyarr-erp-spend',
  },
  {
    key: 'compliance-evidence',
    label: 'Compliance evidence packet',
    description: 'Procurement evidence pack for ERP-supported audit workflows.',
    path: '/api/v1/exports/compliance-evidence-packet.csv',
    filenamePrefix: 'supplyarr-erp-compliance-evidence',
  },
]

interface ErpExportsPanelProps {
  accessToken: string
  canExport: boolean
}

async function downloadCsv(accessToken: string, path: string, errorMessage: string): Promise<Blob> {
  const response = await fetch(`${import.meta.env.VITE_API_BASE_URL ?? ''}${path}`, {
    headers: {
      Authorization: `Bearer ${accessToken}`,
    },
  })

  if (!response.ok) {
    throw new Error(errorMessage)
  }

  return response.blob()
}

export function ErpExportsPanel({ accessToken, canExport }: ErpExportsPanelProps) {
  const exportMutation = useMutation({
    mutationFn: async (target: ExportTarget) =>
      downloadCsv(accessToken, target.path, `Unable to export ${target.label.toLowerCase()}.`),
    onSuccess: (blob, target) => {
      const url = URL.createObjectURL(blob)
      const anchor = document.createElement('a')
      anchor.href = url
      anchor.download = `${target.filenamePrefix}-${new Date().toISOString().slice(0, 10)}.csv`
      anchor.click()
      URL.revokeObjectURL(url)
    },
  })

  if (!canExport) {
    return null
  }

  return (
    <section
      className="rounded-xl border border-slate-700 bg-slate-900/80 p-5"
      data-testid="supplyarr-erp-exports-panel"
    >
      <div className="flex gap-3">
        <FileDown className="mt-0.5 h-5 w-5 text-sky-400" aria-hidden />
        <div>
          <h2 className="text-lg font-semibold text-slate-50">Accounting / ERP exports</h2>
          <p className="mt-1 text-sm text-slate-400">
            Download purchase-order, receipt, invoice-support, spend, and evidence files for ERP
            reconciliation and downstream accounting systems.
          </p>
        </div>
      </div>

      {exportMutation.isError && (
        <ApiErrorCallout
          className="mt-4"
          title="ERP export failed"
          message={getErrorMessage(exportMutation.error, 'Unable to export ERP accounting file.')}
        />
      )}

      <div className="mt-4 grid gap-3 md:grid-cols-2">
        {exportTargets.map((target) => (
          <div key={target.key} className="rounded-lg border border-slate-800 bg-slate-950/60 p-3">
            <div className="text-sm font-semibold text-slate-100">{target.label}</div>
            <p className="mt-1 text-xs text-slate-400">{target.description}</p>
            <button
              type="button"
              className="mt-3 rounded-md bg-sky-700 px-3 py-1.5 text-sm font-medium text-white hover:bg-sky-600 disabled:opacity-50"
              disabled={exportMutation.isPending}
              onClick={() => exportMutation.mutate(target)}
            >
              {exportMutation.isPending && exportMutation.variables?.key === target.key
                ? 'Exporting…'
                : `Export ${target.label} CSV`}
            </button>
          </div>
        ))}
      </div>
    </section>
  )
}
