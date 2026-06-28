import { BrowserMultiFormatReader } from '@zxing/browser'
import { ApiErrorCallout } from '@stl/shared-ui'
import { ScanLine } from 'lucide-react'
import { useCallback, useEffect, useRef, useState } from 'react'

import { resolveFieldCompanionScan } from '../api/client'
import type { FieldCompanionScanResolveResponse } from '../api/types'
import { resolveDeniedReason } from '../lib/FieldCompanionDeniedReasonCatalog'
import { FieldCompanionPlainReason } from '../lib/FieldCompanionPlainReason'
import { productLabel } from '../lib/fieldInbox'
import { normalizeScanPayload } from '../lib/scanPayload'

interface FieldScanPanelProps {
  accessToken: string
  onResolved: (result: FieldCompanionScanResolveResponse) => void
}

type ScanSource = 'camera' | 'manual'

interface ScanContext {
  normalizedValue: string
  source: ScanSource
  symbology: string | null
}

const DUPLICATE_SCAN_WINDOW_MS = 2500

export function FieldScanPanel({ accessToken, onResolved }: FieldScanPanelProps) {
  const videoRef = useRef<HTMLVideoElement | null>(null)
  const readerRef = useRef<BrowserMultiFormatReader | null>(null)
  const scanControlsRef = useRef<{ stop: () => void } | null>(null)
  const lastScanRef = useRef<{ normalizedValue: string; at: number } | null>(null)
  const [cameraActive, setCameraActive] = useState(false)
  const [manualValue, setManualValue] = useState('')
  const [isResolving, setIsResolving] = useState(false)
  const [errorMessage, setErrorMessage] = useState<string | null>(null)
  const [duplicateMessage, setDuplicateMessage] = useState<string | null>(null)
  const [lastDenied, setLastDenied] = useState<FieldCompanionScanResolveResponse | null>(null)
  const [lastResolved, setLastResolved] = useState<FieldCompanionScanResolveResponse | null>(null)
  const [lastScanContext, setLastScanContext] = useState<ScanContext | null>(null)
  const lastScanSourceLabel = lastScanContext ? formatScanSource(lastScanContext.source) : 'manual entry'
  const lastScanSymbology = lastScanContext?.symbology ?? null

  const stopCamera = useCallback(() => {
    scanControlsRef.current?.stop()
    scanControlsRef.current = null
    readerRef.current = null
    const stream = videoRef.current?.srcObject
    if (typeof MediaStream !== 'undefined' && stream instanceof MediaStream) {
      for (const track of stream.getTracks()) {
        track.stop()
      }
    }
    if (videoRef.current) {
      videoRef.current.srcObject = null
    }
    setCameraActive(false)
  }, [])

  useEffect(() => () => stopCamera(), [stopCamera])

  const resolveScan = useCallback(
    async (scannedValue: string, source: ScanSource, symbology?: string) => {
      const normalizedValue = normalizeScanPayload(scannedValue).trim()
      if (!normalizedValue) {
        return
      }

      const now = Date.now()
      const lastScan = lastScanRef.current
      if (lastScan && lastScan.normalizedValue === normalizedValue && now - lastScan.at < DUPLICATE_SCAN_WINDOW_MS) {
        setDuplicateMessage('Duplicate scan ignored. You already scanned that code just now.')
        return
      }

      lastScanRef.current = { normalizedValue, at: now }
      setIsResolving(true)
      setErrorMessage(null)
      setDuplicateMessage(null)
      setLastDenied(null)
      setLastResolved(null)
      setLastScanContext({
        normalizedValue,
        source,
        symbology: symbology?.trim() ? symbology.trim() : null,
      })

      try {
        const result = await resolveFieldCompanionScan(accessToken, {
          scannedValue: normalizedValue,
          symbology: symbology ?? null,
        })

        if (result.outcome === 'resolved') {
          setLastResolved(result)
          onResolved(result)
          return
        }

        setLastDenied(result)
      } catch (error) {
        setErrorMessage(FieldCompanionPlainReason(error, 'Scan resolve failed.'))
      } finally {
        setIsResolving(false)
      }
    },
    [accessToken, onResolved],
  )

  const startCamera = useCallback(async () => {
    setErrorMessage(null)
    stopCamera()

    if (!navigator.mediaDevices?.getUserMedia) {
      setErrorMessage('Camera access is not available in this browser.')
      return
    }

    try {
      const reader = new BrowserMultiFormatReader()
      readerRef.current = reader
      if (!videoRef.current) {
        throw new Error('Camera preview is not ready.')
      }

      scanControlsRef.current = await reader.decodeFromVideoDevice(
        undefined,
        videoRef.current,
        (result, error) => {
          if (result) {
            void resolveScan(
              result.getText(),
              'camera',
              result.getBarcodeFormat().toString(),
            ).then(() => {
              stopCamera()
            })
            return
          }

          if (error && !String(error.name).includes('NotFound')) {
            setErrorMessage(error.message)
          }
        },
      )
      setCameraActive(true)
    } catch (error) {
      setErrorMessage(error instanceof Error ? error.message : 'Unable to start camera scanner.')
      stopCamera()
    }
  }, [resolveScan, stopCamera])

  return (
    <section
      className="rounded-xl border border-slate-700 bg-slate-900/80 p-4"
      data-testid="fieldcompanion-field-scan-panel"
    >
      <div className="flex items-center gap-2">
        <ScanLine className="h-5 w-5 text-teal-300" aria-hidden />
        <h2 className="text-sm font-semibold uppercase tracking-wide text-slate-200">
          Scan field task
        </h2>
      </div>
      <p className="mt-2 text-sm text-slate-400">
        Scan a QR code or barcode on a field label, or enter the code manually.
      </p>

      {lastScanContext && (
        <div
          className="mt-3 rounded-lg border border-slate-700 bg-slate-950/50 px-3 py-2 text-xs text-slate-300"
          data-testid="fieldcompanion-scan-context"
        >
          <p className="font-medium text-slate-100">
            Last scan via {lastScanSourceLabel}
            {shouldShowSymbology(lastScanSymbology)
              ? ` · ${formatScanSymbology(lastScanSymbology)}`
              : ''}
          </p>
          <p className="mt-1 text-slate-400">
            Code normalized and sent to the permitted product resolver.
          </p>
        </div>
      )}

      <div className="mt-4 space-y-3">
        <video
          id="fieldcompanion-scan-video"
          ref={videoRef}
          className={`w-full rounded-lg border border-slate-700 bg-black ${cameraActive ? 'block' : 'hidden'}`}
          muted
          playsInline
        />

        <div className="flex flex-wrap gap-2">
          {cameraActive ? (
            <button
              type="button"
              className="inline-flex min-h-11 items-center rounded-lg border border-slate-600 px-4 py-2 text-sm font-medium text-slate-100"
              data-testid="fieldcompanion-scan-stop-camera"
              onClick={stopCamera}
            >
              Stop camera
            </button>
          ) : (
            <button
              type="button"
              className="inline-flex min-h-11 items-center rounded-lg bg-teal-600 px-4 py-2 text-sm font-medium text-white hover:bg-teal-500"
              data-testid="fieldcompanion-scan-start-camera"
              onClick={() => {
                void startCamera()
              }}
            >
              Start camera scan
            </button>
          )}
        </div>

        <form
          className="flex flex-col gap-2 sm:flex-row"
          onSubmit={(event) => {
            event.preventDefault()
            void resolveScan(manualValue, 'manual', 'manual')
          }}
        >
          <label className="sr-only" htmlFor="fieldcompanion-scan-manual-input">
            Manual scan code
          </label>
          <input
            id="fieldcompanion-scan-manual-input"
            data-testid="fieldcompanion-scan-manual-input"
            className="min-h-11 flex-1 rounded-lg border border-slate-600 bg-slate-950 px-3 text-sm text-white"
            placeholder="trainarr:assignment:… or /assignments/…"
            value={manualValue}
            onChange={(event) => setManualValue(event.target.value)}
            autoComplete="off"
          />
          <button
            type="submit"
            className="inline-flex min-h-11 items-center justify-center rounded-lg border border-teal-500 px-4 py-2 text-sm font-medium text-teal-100 disabled:opacity-50"
            disabled={isResolving || !manualValue.trim()}
            data-testid="fieldcompanion-scan-submit"
          >
            {isResolving ? 'Resolving…' : 'Resolve code'}
          </button>
        </form>

        {errorMessage && (
          <ApiErrorCallout
            testId="fieldcompanion-scan-error"
            title="Scan failed"
            message={errorMessage}
          />
        )}

        {duplicateMessage && (
          <p
            className="rounded-lg border border-slate-600 bg-slate-950/40 px-3 py-2 text-sm text-slate-200"
            data-testid="fieldcompanion-scan-duplicate"
          >
            {duplicateMessage}
          </p>
        )}

        {lastDenied && (
          <div
            className="rounded-lg border border-amber-500/40 bg-amber-950/30 px-3 py-2 text-sm text-amber-100"
            data-testid="fieldcompanion-scan-denied"
          >
            <p className="font-medium">{resolveDeniedReason(lastDenied, 'Scan was denied.')}</p>
            <p className="mt-1 text-xs text-amber-100/80">
              {lastDenied.taskKey
                ? `The ${productLabel(lastDenied.productKey ?? '')} task is not currently available to this session.`
                : 'The scan did not resolve to a permitted task.'}
            </p>
          </div>
        )}

        {lastResolved?.taskKey && (
          <div
            className="rounded-lg border border-teal-500/40 bg-teal-950/30 px-3 py-2 text-sm text-teal-50"
            data-testid="fieldcompanion-scan-result"
          >
            <p className="font-medium">{lastResolved.title}</p>
            <p className="mt-1 text-xs text-teal-100/80">
              {productLabel(lastResolved.productKey ?? '')}
            </p>
            <p className="mt-1 text-xs text-teal-100/80">
              {lastResolved.taskType ? `${lastResolved.taskType.replaceAll('_', ' ')} · ` : ''}
              {lastResolved.status ? `${lastResolved.status.replaceAll('_', ' ')} · ` : ''}
              {lastScanSourceLabel}
              {shouldShowSymbology(lastScanSymbology)
                ? ` · ${formatScanSymbology(lastScanSymbology)}`
                : ''}
            </p>
            {lastResolved.deepLinkUrl && (
              <a
                href={lastResolved.deepLinkUrl}
                className="mt-2 inline-flex min-h-10 items-center text-sm font-medium text-teal-200 underline"
                data-testid="fieldcompanion-scan-open-deeplink"
              >
                Open in product
              </a>
            )}
          </div>
        )}
      </div>
    </section>
  )
}

function formatScanSource(source: ScanSource): string {
  return source === 'camera' ? 'camera scan' : 'manual entry'
}

function formatScanSymbology(value: string | null | undefined): string {
  const normalized = value?.trim().replace(/[_-]+/g, ' ') ?? ''
  if (!normalized) {
    return 'Unknown symbology'
  }

  if (normalized.toUpperCase() === 'QR CODE' || normalized.toUpperCase() === 'QR') {
    return 'QR code'
  }

  return normalized
    .split(' ')
    .filter(Boolean)
    .map((part) => {
      if (part.toUpperCase() === 'QR') {
        return 'QR'
      }

      return part.length <= 3 ? part.toUpperCase() : part[0].toUpperCase() + part.slice(1).toLowerCase()
    })
    .join(' ')
}

function shouldShowSymbology(symbology: string | null | undefined): boolean {
  return Boolean(symbology && symbology.trim() && symbology.trim().toLowerCase() !== 'manual')
}
