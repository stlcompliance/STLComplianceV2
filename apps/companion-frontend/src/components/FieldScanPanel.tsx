import { BrowserMultiFormatReader } from '@zxing/browser'
import { ApiErrorCallout } from '@stl/shared-ui'
import { ScanLine } from 'lucide-react'
import { useCallback, useEffect, useRef, useState } from 'react'
import type { CompanionScanResolveResponse } from '../api/types'
import { resolveCompanionScan } from '../api/client'
import { resolveDeniedReason } from '../lib/companionDeniedReasonCatalog'
import { companionPlainReason } from '../lib/companionPlainReason'
import { productLabel } from '../lib/fieldInbox'

interface FieldScanPanelProps {
  accessToken: string
  onResolved: (result: CompanionScanResolveResponse) => void
}

export function FieldScanPanel({ accessToken, onResolved }: FieldScanPanelProps) {
  const videoRef = useRef<HTMLVideoElement | null>(null)
  const readerRef = useRef<BrowserMultiFormatReader | null>(null)
  const scanControlsRef = useRef<{ stop: () => void } | null>(null)
  const [cameraActive, setCameraActive] = useState(false)
  const [manualValue, setManualValue] = useState('')
  const [isResolving, setIsResolving] = useState(false)
  const [errorMessage, setErrorMessage] = useState<string | null>(null)
  const [lastDenied, setLastDenied] = useState<CompanionScanResolveResponse | null>(null)
  const [lastResolved, setLastResolved] = useState<CompanionScanResolveResponse | null>(null)

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
    async (scannedValue: string, symbology?: string) => {
      const trimmed = scannedValue.trim()
      if (!trimmed) {
        return
      }

      setIsResolving(true)
      setErrorMessage(null)
      setLastDenied(null)
      setLastResolved(null)

      try {
        const result = await resolveCompanionScan(accessToken, {
          scannedValue: trimmed,
          symbology: symbology ?? null,
        })

        if (result.outcome === 'resolved') {
          setLastResolved(result)
          onResolved(result)
          return
        }

        setLastDenied(result)
      } catch (error) {
        setErrorMessage(companionPlainReason(error, 'Scan resolve failed.'))
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
            void resolveScan(result.getText(), result.getBarcodeFormat().toString()).then(() => {
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
      data-testid="companion-field-scan-panel"
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

      <div className="mt-4 space-y-3">
        <video
          id="companion-scan-video"
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
              data-testid="companion-scan-stop-camera"
              onClick={stopCamera}
            >
              Stop camera
            </button>
          ) : (
            <button
              type="button"
              className="inline-flex min-h-11 items-center rounded-lg bg-teal-600 px-4 py-2 text-sm font-medium text-white hover:bg-teal-500"
              data-testid="companion-scan-start-camera"
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
            void resolveScan(manualValue, 'manual')
          }}
        >
          <label className="sr-only" htmlFor="companion-scan-manual-input">
            Manual scan code
          </label>
          <input
            id="companion-scan-manual-input"
            data-testid="companion-scan-manual-input"
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
            data-testid="companion-scan-submit"
          >
            {isResolving ? 'Resolving…' : 'Resolve code'}
          </button>
        </form>

        {errorMessage && (
          <ApiErrorCallout
            testId="companion-scan-error"
            title="Scan failed"
            message={errorMessage}
          />
        )}

        {lastDenied && (
          <p className="rounded-lg border border-amber-500/40 bg-amber-950/30 px-3 py-2 text-sm text-amber-100" data-testid="companion-scan-denied">
            {resolveDeniedReason(lastDenied, 'Scan was denied.')}
          </p>
        )}

        {lastResolved?.taskKey && (
          <div
            className="rounded-lg border border-teal-500/40 bg-teal-950/30 px-3 py-2 text-sm text-teal-50"
            data-testid="companion-scan-result"
          >
            <p className="font-medium">{lastResolved.title}</p>
            <p className="mt-1 text-xs text-teal-100/80">
              {productLabel(lastResolved.productKey ?? '')} · {lastResolved.taskKey}
            </p>
            {lastResolved.deepLinkUrl && (
              <a
                href={lastResolved.deepLinkUrl}
                className="mt-2 inline-flex min-h-10 items-center text-sm font-medium text-teal-200 underline"
                data-testid="companion-scan-open-deeplink"
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
