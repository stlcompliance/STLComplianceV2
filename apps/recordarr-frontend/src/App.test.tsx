import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, fireEvent, render, screen, waitFor, within } from '@testing-library/react'
import type { ReactNode } from 'react'
import { MemoryRouter } from 'react-router-dom'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { saveSession } from './auth/sessionStorage'
import * as client from './api/client'

vi.mock('@stl/shared-ui', async () => {
  return {
    ApiErrorCallout: ({ title, message }: { title: string; message: string }) => (
      <div>
        <strong>{title}</strong>
        <p>{message}</p>
      </div>
    ),
    AsyncSearchPicker: ({ id, value, onChange, placeholder, disabled }: any) => (
      <input
        id={id}
        value={value}
        disabled={disabled}
        onChange={(event) => onChange(event.currentTarget.value)}
        placeholder={placeholder}
      />
    ),
    ControlledSelect: ({ id, value, onChange, options, emptyLabel, className, disabled }: any) => (
      <select
        id={id}
        className={className}
        value={value}
        disabled={disabled}
        onChange={(event) => onChange(event.currentTarget.value)}
      >
        <option value="">{emptyLabel}</option>
        {(options ?? []).map((option: { value: string; label: string; inactive?: boolean }) => (
          <option key={option.value} value={option.value} disabled={option.inactive}>
            {option.label}
          </option>
        ))}
      </select>
    ),
    StaticSearchPicker: ({ id, value, onChange, options, placeholder, disabled }: any) => (
      <select
        id={id}
        value={value}
        disabled={disabled}
        onChange={(event) => onChange(event.currentTarget.value)}
      >
        <option value="">{placeholder}</option>
        {(options ?? []).map((option: { value: string; label: string; inactive?: boolean }) => (
          <option key={option.value} value={option.value} disabled={option.inactive}>
            {option.label}
          </option>
        ))}
      </select>
    ),
    ProductWorkspaceFrame: ({ children, productName }: { children: ReactNode; productName: string }) => (
      <div data-testid="workspace-frame">
        <h2>{productName}</h2>
        {children}
      </div>
    ),
    ReferenceProviderClient: class ReferenceProviderClient {
      constructor(_options: unknown) {}
    },
    ReferenceSearchPicker: ({ id, value, onChange, placeholder, disabled }: any) => (
      <input
        id={id}
        value={value}
        disabled={disabled}
        onChange={(event) => onChange(event.currentTarget.value)}
        placeholder={placeholder}
      />
    ),
    SourceReferenceSearchPicker: ({ id, value, onChange, placeholder, disabled }: any) => (
      <input
        id={id}
        value={value}
        disabled={disabled}
        onChange={(event) => {
          const rawValue = event.currentTarget.value
          const [sourceProduct, sourceObjectType, ...rest] = rawValue.split(':')
          const selected =
            rawValue.includes(':') && sourceProduct && sourceObjectType && rest.length > 0
              ? {
                  sourceProduct,
                  sourceObjectType,
                  sourceObjectId: rest.join(':'),
                }
              : null
          onChange(rawValue, selected)
        }}
        placeholder={placeholder}
      />
    ),
    SUITE_SOURCE_PRODUCT_OPTIONS: [
      { value: 'recordarr', label: 'RecordArr' },
      { value: 'staffarr', label: 'StaffArr' },
      { value: 'supplyarr', label: 'SupplyArr' },
      { value: 'customarr', label: 'CustomArr' },
      { value: 'maintainarr', label: 'MaintainArr' },
    ],
    buildProductLaunchUrlMap: () => ({}),
    buildSourceObjectRef: (product: string, objectType: string, objectId: string) => `${product}:${objectType}:${objectId}`,
    formatDisplayLabel: (value: string) => value.replaceAll('_', ' ').replace(/^[a-z]/, (char) => char.toUpperCase()),
    formatProductLaunchError: (error: unknown) => String(error),
    getErrorMessage: (error: unknown, fallback: string) => (error instanceof Error ? error.message : fallback),
    getLaunchCatalog: vi.fn().mockResolvedValue({ products: [{ productKey: 'recordarr' }] }),
    resolveSuiteHomeUrl: () => '/',
    resolveProductWorkspaceBootstrapError: () => null,
    useProductWorkspaceLaunch: () => ({
      mutate: vi.fn(),
      isPending: false,
      isError: false,
      error: null,
    }),
    useRegisterPrintableSurface: () => undefined,
  }
})

vi.mock('./api/client', async () => {
  const actual = await vi.importActual<typeof client>('./api/client')

  return {
    ...actual,
    getSessionBootstrap: vi.fn(),
    getRecord: vi.fn(),
    listControlledDocuments: vi.fn(),
    createControlledDocument: vi.fn(),
    listDocumentVersions: vi.fn(),
    createDocumentVersion: vi.fn(),
    listDocumentReviews: vi.fn(),
    createDocumentReview: vi.fn(),
    listDocumentDistributions: vi.fn(),
    createDocumentDistribution: vi.fn(),
    listDocumentAcknowledgements: vi.fn(),
    createDocumentAcknowledgement: vi.fn(),
    listEvidenceCoverage: vi.fn(),
    listEvidenceMappings: vi.fn(),
    createEvidenceMapping: vi.fn(),
    createPhotoEvidence: vi.fn(),
    listFiles: vi.fn(),
    listRecordMetadata: vi.fn(),
    listRecordLinks: vi.fn(),
    listRecordComments: vi.fn(),
    getRetentionStatus: vi.fn(),
    listAccessLogs: vi.fn(),
    listUploadSessions: vi.fn(),
    listCaptureRequests: vi.fn(),
    listScans: vi.fn(),
    getDashboard: vi.fn(),
    listReminders: vi.fn(),
    listRecords: vi.fn(),
    listPackages: vi.fn(),
    listAccessPolicies: vi.fn(),
    listAccessGrants: vi.fn(),
    listExternalShares: vi.fn(),
    listDisposalReviews: vi.fn(),
    createLegalHold: vi.fn(),
    activateLegalHold: vi.fn(),
    releaseLegalHold: vi.fn(),
    createPackage: vi.fn(),
    getPackageManifest: vi.fn(),
    lockPackage: vi.fn(),
    archivePackage: vi.fn(),
    downloadPackage: vi.fn(),
    listLegalHolds: vi.fn(),
    listRedactions: vi.fn(),
    createRedaction: vi.fn(),
    createAccessPolicy: vi.fn(),
    updateAccessPolicy: vi.fn(),
    createAccessGrant: vi.fn(),
    revokeAccessGrant: vi.fn(),
    refreshAccessGrants: vi.fn(),
    createExternalShare: vi.fn(),
    revokeExternalShare: vi.fn(),
    recordExternalShareAccess: vi.fn(),
    expireExternalShare: vi.fn(),
    refreshExternalShares: vi.fn(),
    listVocabularyTerms: vi.fn(),
    listRetentionPolicies: vi.fn(),
    recalculateRetentionStatuses: vi.fn(),
    createDisposalReview: vi.fn(),
    completeDisposalReview: vi.fn(),
    createRecord: vi.fn(),
    createScan: vi.fn(),
    createSignatureRecord: vi.fn(),
    getOcrResult: vi.fn(),
    getExtractionResult: vi.fn(),
  }
})

vi.mock('./LaunchPage', () => ({
  LaunchPage: () => <div data-testid="launch-page" />,
}))

vi.mock('./components/RecordPrint', () => ({
  RecordPrintPreview: () => null,
  RecordPrintToolbarActions: () => null,
}))

function renderApp(route = '/capture') {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: {
        retry: false,
      },
      mutations: {
        retry: false,
      },
    },
  })

  return render(
    <QueryClientProvider client={queryClient}>
      <MemoryRouter initialEntries={[route]}>
        <AppComponent />
      </MemoryRouter>
    </QueryClientProvider>,
  )
}

let AppComponent: typeof import('./App').App

function mockVocabularyTerms() {
  vi.mocked(client.listVocabularyTerms).mockImplementation(async (_accessToken, vocabularyTypeKey) => {
    const createdAt = new Date().toISOString()
    switch (vocabularyTypeKey) {
      case 'document_class':
        return [
          {
            termId: 'term-1',
            termKey: 'operations',
            label: 'Operations',
            vocabularyTypeKey: 'document_class',
            description: 'Operations documents',
            isActive: true,
            aliases: [],
            createdAt,
          },
        ]
      case 'document_type':
        return [
          {
            termId: 'term-2',
            termKey: 'manifest',
            label: 'Manifest',
            vocabularyTypeKey: 'document_type',
            description: 'Manifest records',
            isActive: true,
            aliases: [],
            createdAt,
          },
        ]
      case 'document_subtype':
        return [
          {
            termId: 'term-3',
            termKey: 'outbound',
            label: 'Outbound',
            vocabularyTypeKey: 'document_subtype',
            description: 'Outbound manifests',
            isActive: true,
            aliases: [],
            createdAt,
          },
        ]
      default:
        return []
    }
  })
}

describe('RecordArr app', () => {
  beforeEach(async () => {
    cleanup()
    sessionStorage.clear()
    vi.stubEnv('VITE_COMPLIANCECORE_API_BASE', 'http://compliance.test')
    saveSession({
      accessToken: 'token-1',
      accessTokenExpiresAt: new Date(Date.now() + 60_000).toISOString(),
      userId: 'user-1',
      personId: 'person-1',
      tenantId: 'tenant-1',
      tenantSlug: 'tenant-one',
      tenantDisplayName: 'Tenant One',
      displayName: 'Records User',
      email: 'records@example.com',
      isPlatformAdmin: true,
    })
    AppComponent = (await import('./App')).App

    Object.defineProperty(URL, 'createObjectURL', {
      value: vi.fn(() => 'blob:preview'),
      writable: true,
    })
    Object.defineProperty(URL, 'revokeObjectURL', {
      value: vi.fn(),
      writable: true,
    })
    mockVocabularyTerms()
  })

  afterEach(() => {
    cleanup()
    vi.clearAllMocks()
    vi.unstubAllEnvs()
  })

  it('files a captured record and queues OCR from the triage workflow', async () => {
    vi.mocked(client.getSessionBootstrap).mockResolvedValue({
      userId: 'user-1',
      personId: 'person-1',
      tenantId: 'tenant-1',
      sessionId: 'session-1',
      tenantRoleKey: 'recordarr-ops',
      isPlatformAdmin: true,
      productKey: 'recordarr',
      hasRecordArrAccess: true,
      launchableProductKeys: ['recordarr'],
    })
    vi.mocked(client.listUploadSessions).mockResolvedValue([])
    vi.mocked(client.listCaptureRequests).mockResolvedValue([])
    vi.mocked(client.listScans).mockResolvedValue([])
    vi.mocked(client.createRecord).mockResolvedValue({
      recordId: 'record-1',
      recordNumber: 'REC-2026-1001',
      title: 'June inbound manifest',
      description: 'Inbound shipment manifest for filing',
      recordType: 'document',
      documentClass: 'operations',
      documentType: 'manifest',
      documentSubtype: 'outbound',
      status: 'active',
      classification: 'confidential',
      sourceProduct: 'recordarr',
      sourceObjectType: 'capture',
      sourceObjectId: 'capture-1',
      sourceObjectDisplayName: 'shipping manifest',
      ownerPersonId: 'person-1',
      uploadedByPersonId: 'person-1',
      uploadedAt: new Date().toISOString(),
      effectiveAt: null,
      expiresAt: null,
      archivedAt: null,
      purgedAt: null,
      currentFileName: 'shipping-manifest.pdf',
      currentMimeType: 'application/pdf',
      versionNumber: 1,
      tags: [],
      currentFileRef: 'file-1',
      fileRefs: ['file-1'],
      currentVersionRef: 'version-1',
      sourceObjectRefs: [],
      metadataRefs: [],
      versionRefs: [],
      ocrResultRefs: [],
      extractionResultRefs: [],
      evidenceMappingRefs: [],
      packageRefs: [],
      retentionPolicyRef: null,
      retentionStatusRef: null,
      legalHoldRefs: [],
      accessPolicyRef: null,
      complianceRefs: [],
      auditTrail: [],
      recordRef: null,
    } as any)
    vi.mocked(client.createScan).mockResolvedValue({
      scanProcessingId: 'scan-1',
      recordId: 'record-1',
      originalFileName: 'shipping-manifest.pdf',
      status: 'queued',
      scanPurpose: 'June inbound manifest',
      edgeCoordinates: null,
      manualEdgeCoordinates: null,
      correctedByPersonId: null,
      correctedAt: null,
      originalFileRef: null,
      generatedPdfFileRef: null,
      generatedPdfRecordRef: null,
      ocrResultId: 'ocr-1',
      extractionResultId: 'extraction-1',
      edgeDetectionResult: null,
      enhancementSettings: null,
      confidenceScore: 0.92,
      processedAt: null,
      failureReason: null,
    } as any)
    vi.mocked(client.getOcrResult).mockResolvedValue({
      ocrResultId: 'ocr-1',
      recordId: 'record-1',
      fileId: 'file-1',
      engine: 'Mock OCR',
      status: 'complete',
      language: 'en',
      confidenceScore: 0.97,
      fullText: 'Shipment manifest text',
      pageResults: [],
      blockResults: [],
      extractedAt: new Date().toISOString(),
      failureReason: null,
    } as any)
    vi.mocked(client.getExtractionResult).mockResolvedValue({
      extractionResultId: 'extraction-1',
      recordId: 'record-1',
      extractionType: 'document',
      status: 'ready',
      extractedFields: [
        {
          extractedFieldId: 'field-1',
          extractionResultId: 'extraction-1',
          fieldKey: 'documentNumber',
          label: 'Document number',
          value: 'INV-1001',
          valueType: 'string',
          confidenceScore: 0.94,
          pageNumber: 1,
          boundingBox: null,
          reviewStatus: 'unreviewed',
          correctedValue: null,
          correctedByPersonId: null,
          correctedAt: null,
        },
        {
          extractedFieldId: 'field-2',
          extractionResultId: 'extraction-1',
          fieldKey: 'sourceParty',
          label: 'Source party',
          value: 'Northwind Logistics',
          valueType: 'string',
          confidenceScore: 0.9,
          pageNumber: 1,
          boundingBox: null,
          reviewStatus: 'unreviewed',
          correctedValue: null,
          correctedByPersonId: null,
          correctedAt: null,
        },
      ],
      confidenceScore: 0.95,
      extractedAt: new Date().toISOString(),
      reviewedByPersonId: null,
      reviewedAt: null,
      failureReason: null,
    } as any)

    const { container } = renderApp('/capture')

    expect(await screen.findByText('Scan Capture')).toBeTruthy()
    expect(screen.getByText('Upload first, triage second, submit last.')).toBeTruthy()

    const uploadInput = container.querySelectorAll('input[type="file"]')[1] as HTMLInputElement
    const capturedFile = new File(['pdf'], 'shipping-manifest.pdf', { type: 'application/pdf' })
    fireEvent.change(uploadInput, { target: { files: [capturedFile] } })

    await waitFor(() => {
      expect(screen.getByLabelText('Document class')).toHaveValue('operations')
      expect(screen.getByLabelText('Document type')).toHaveValue('manifest')
      expect(screen.getByLabelText('Document subtype')).toHaveValue('outbound')
    })

    fireEvent.change(screen.getByLabelText('Title'), { target: { value: 'June inbound manifest' } })
    fireEvent.change(screen.getByLabelText('Classification'), { target: { value: 'confidential' } })
    fireEvent.change(screen.getByLabelText('Description'), {
      target: { value: 'Inbound shipment manifest for filing' },
    })
    await waitFor(() => {
      expect((screen.getByLabelText('Title') as HTMLInputElement).value).toBe('June inbound manifest')
      expect((screen.getByLabelText('Classification') as HTMLSelectElement).value).toBe('confidential')
      expect((screen.getByLabelText('Document class') as HTMLSelectElement).value).toBe('operations')
      expect((screen.getByLabelText('Document type') as HTMLSelectElement).value).toBe('manifest')
      expect((screen.getByLabelText('Document subtype') as HTMLSelectElement).value).toBe('outbound')
      expect((screen.getByLabelText('Description') as HTMLTextAreaElement).value).toBe('Inbound shipment manifest for filing')
      expect(screen.getByRole('button', { name: 'Submit document' })).not.toBeDisabled()
    })
    fireEvent.click(screen.getByRole('button', { name: 'Submit document' }))

    await waitFor(() => {
      expect(vi.mocked(client.createRecord)).toHaveBeenCalledTimes(1)
    })
    expect(vi.mocked(client.createRecord)).toHaveBeenCalledWith(
      'token-1',
      expect.objectContaining({
        title: 'June inbound manifest',
        description: 'Inbound shipment manifest for filing',
        recordType: 'document',
        documentClass: 'operations',
        documentType: 'manifest',
        documentSubtype: 'outbound',
        classification: 'confidential',
        sourceProduct: 'recordarr',
        sourceObjectType: 'capture',
        sourceObjectId: expect.stringMatching(/^capture-/),
        sourceObjectDisplayName: 'shipping manifest',
        ownerPersonId: 'person-1',
        uploadedByPersonId: 'person-1',
        currentFileName: 'shipping-manifest.pdf',
        currentMimeType: 'application/pdf',
      }),
    )
    expect(vi.mocked(client.createScan)).toHaveBeenCalledWith(
      'token-1',
      expect.objectContaining({
        recordId: 'record-1',
        originalFileName: 'shipping-manifest.pdf',
        scanPurpose: 'June inbound manifest',
      }),
    )

    expect(await screen.findByText('REC-2026-1001')).toBeTruthy()
    expect((await screen.findAllByText('Mock OCR · 97%')).length).toBeGreaterThan(0)
    expect((await screen.findAllByText('Ready · 2')).length).toBeGreaterThan(0)
  })

  it('surfaces due controlled document reviews on the dashboard', async () => {
    vi.mocked(client.getSessionBootstrap).mockResolvedValue({
      userId: 'user-1',
      personId: 'person-1',
      tenantId: 'tenant-1',
      sessionId: 'session-1',
      tenantRoleKey: 'recordarr-ops',
      isPlatformAdmin: true,
      productKey: 'recordarr',
      hasRecordArrAccess: true,
      launchableProductKeys: ['recordarr'],
    })
    vi.mocked(client.getDashboard).mockResolvedValue({
      generatedAt: new Date().toISOString(),
      recordCount: 1,
      activeCount: 0,
      reviewCount: 1,
      uploadSessionCount: 0,
      packageCount: 0,
      controlledDocumentCount: 1,
      legalHoldCount: 0,
      recentRecords: [],
      openPackages: [],
      controlledDocuments: [
        {
          controlledDocumentId: 'doc-1',
          documentNumber: 'DOC-2026-1001',
          recordId: 'record-1',
          title: 'Periodic review procedure',
          description: 'Verifies the periodic review refresh workflow.',
          documentClass: 'procedure',
          documentType: 'operations',
          documentSubtype: 'review_cycle',
          controlledDocumentType: 'procedure',
          status: 'review',
          ownerPersonId: 'person-1',
          departmentOrgUnitId: 'org-receiving',
          staffarrSiteId: 'site-north-yard',
          currentVersionId: 'ver-1',
          reviewIntervalDays: 180,
          nextReviewAt: new Date(Date.now() - 86_400_000).toISOString(),
          effectiveAt: new Date(Date.now() - 181 * 86_400_000).toISOString(),
          expiresAt: null,
          supersedesDocumentRef: null,
          supersededByDocumentRef: null,
          acknowledgementRequired: true,
          relatedRecordRefs: [],
          auditTrail: [],
        },
      ] as any,
      legalHolds: [],
    } as any)
    vi.mocked(client.listReminders).mockResolvedValue([
      {
        reminderId: 'rem-1',
        reminderType: 'controlled_document_review',
        status: 'due_for_review',
        title: 'DOC-2026-1001 review due',
        description: 'Periodic review procedure is scheduled for periodic review.',
        recordId: 'record-1',
        controlledDocumentId: 'doc-1',
        versionId: 'ver-1',
        personId: 'person-1',
        dueAt: new Date(Date.now() - 86_400_000).toISOString(),
        createdAt: new Date().toISOString(),
        sourceRef: 'controlled-document:doc-1',
      } as any,
    ])
    vi.mocked(client.listUploadSessions).mockResolvedValue([])
    vi.mocked(client.listCaptureRequests).mockResolvedValue([])
    vi.mocked(client.listScans).mockResolvedValue([])

    renderApp('/')

    expect(await screen.findByText('Records and evidence control center')).toBeTruthy()
    expect(await screen.findByText('DOC-2026-1001 review due')).toBeTruthy()
    expect(screen.getByText(/controlled document review/)).toBeTruthy()
    expect(screen.getByText('Periodic review procedure is scheduled for periodic review.')).toBeTruthy()
  })

  it('creates a controlled document and requests review from the document workspace', async () => {
    const controlledDocuments: any[] = []
    const versionsByDocumentId = new Map<string, any[]>()
    const reviewsByDocumentId = new Map<string, any[]>()
    const distributionsByDocumentId = new Map<string, any[]>()
    const acknowledgementsByDocumentId = new Map<string, any[]>()

    vi.mocked(client.getSessionBootstrap).mockResolvedValue({
      userId: 'user-1',
      personId: 'person-1',
      tenantId: 'tenant-1',
      sessionId: 'session-1',
      tenantRoleKey: 'recordarr-ops',
      isPlatformAdmin: true,
      productKey: 'recordarr',
      hasRecordArrAccess: true,
      launchableProductKeys: ['recordarr'],
    })
    vi.mocked(client.listControlledDocuments).mockImplementation(async () => [...controlledDocuments])
    vi.mocked(client.listDocumentVersions).mockImplementation(async (_accessToken, documentId) => [
      ...(versionsByDocumentId.get(documentId) ?? []),
    ])
    vi.mocked(client.listDocumentReviews).mockImplementation(async (_accessToken, documentId) => [
      ...(reviewsByDocumentId.get(documentId) ?? []),
    ])
    vi.mocked(client.listDocumentDistributions).mockImplementation(async (_accessToken, documentId) => [
      ...(distributionsByDocumentId.get(documentId) ?? []),
    ])
    vi.mocked(client.listDocumentAcknowledgements).mockImplementation(async (_accessToken, documentId) => [
      ...(acknowledgementsByDocumentId.get(documentId) ?? []),
    ])
    vi.mocked(client.listUploadSessions).mockResolvedValue([])
    vi.mocked(client.listCaptureRequests).mockResolvedValue([])
    vi.mocked(client.listScans).mockResolvedValue([])

    vi.mocked(client.createControlledDocument).mockImplementation(async (_accessToken, request) => {
      const documentId = 'doc-1'
      const versionId = 'version-1'
      const document = {
        controlledDocumentId: documentId,
        documentNumber: 'DOC-2026-0001',
        recordId: 'record-1',
        title: request.title,
        description: request.description,
        documentClass: request.documentClass,
        documentType: request.documentType,
        documentSubtype: request.documentSubtype,
        controlledDocumentType: 'procedure',
        status: 'draft',
        ownerPersonId: request.ownerPersonId,
        departmentOrgUnitId: request.departmentOrgUnitId,
        staffarrSiteId: request.staffarrSiteId,
        currentVersionId: versionId,
        reviewIntervalDays: 90,
        nextReviewAt: null,
        effectiveAt: null,
        expiresAt: null,
        supersedesDocumentRef: null,
        supersededByDocumentRef: null,
        acknowledgementRequired: request.acknowledgementRequired,
        relatedRecordRefs: [],
        auditTrail: [],
      } as any
      controlledDocuments.push(document)
      versionsByDocumentId.set(documentId, [
        {
          versionId,
          controlledDocumentId: documentId,
          versionNumber: 1,
          versionLabel: 'v1',
          status: 'draft',
          fileName: 'procedure-v1.pdf',
          createdAt: new Date().toISOString(),
          createdByPersonId: 'person-1',
          submittedForReviewAt: null,
          approvedAt: null,
          approvedByPersonId: null,
          effectiveAt: null,
          supersededAt: null,
          changeSummary: 'Initial controlled document draft',
          previousVersionRef: null,
          nextVersionRef: null,
          fileRef: null,
        },
      ])
      reviewsByDocumentId.set(documentId, [])
      distributionsByDocumentId.set(documentId, [])
      acknowledgementsByDocumentId.set(documentId, [])
      return document
    })
    vi.mocked(client.createDocumentVersion).mockImplementation(async (_accessToken, documentId, request) => {
      const current = versionsByDocumentId.get(documentId) ?? []
      const nextVersion = {
        versionId: `version-${current.length + 1}`,
        controlledDocumentId: documentId,
        versionNumber: current.length + 1,
        versionLabel: `v${current.length + 1}`,
        status: 'draft',
        fileName: request.fileName,
        createdAt: new Date().toISOString(),
        createdByPersonId: request.createdByPersonId,
        submittedForReviewAt: null,
        approvedAt: null,
        approvedByPersonId: null,
        effectiveAt: null,
        supersededAt: null,
        changeSummary: request.changeSummary,
        previousVersionRef: current[current.length - 1]?.versionId ?? null,
        nextVersionRef: null,
        fileRef: null,
      } as any
      versionsByDocumentId.set(documentId, [...current, nextVersion])
      controlledDocuments[0].currentVersionId = nextVersion.versionId
      return nextVersion
    })
    vi.mocked(client.createDocumentReview).mockImplementation(async (_accessToken, documentId, request) => {
      const review = {
        documentReviewId: 'review-1',
        controlledDocumentId: documentId,
        versionId: request.versionId,
        reviewType: request.reviewType,
        status: 'requested',
        requestedByPersonId: request.requestedByPersonId,
        reviewerPersonId: request.reviewerPersonId,
        requestedAt: new Date().toISOString(),
        dueAt: request.dueAt,
        reviewedAt: null,
        decisionReason: null,
        comments: null,
      } as any
      reviewsByDocumentId.set(documentId, [review])
      return review
    })
    vi.mocked(client.createDocumentDistribution).mockImplementation(async (_accessToken, documentId, request) => {
      const distribution = {
        distributionId: 'distribution-1',
        controlledDocumentId: documentId,
        versionId: request.versionId,
        distributionType: request.distributionType,
        targetRef: request.targetRef,
        status: 'active',
        distributedAt: new Date().toISOString(),
        revokedAt: null,
        expireAt: null,
      } as any
      distributionsByDocumentId.set(documentId, [distribution])
      return distribution
    })
    vi.mocked(client.createDocumentAcknowledgement).mockImplementation(async (_accessToken, documentId, request) => {
      const acknowledgement = {
        acknowledgementId: 'ack-1',
        controlledDocumentId: documentId,
        versionId: request.versionId,
        personId: request.personId,
        status: 'pending',
        attestationText: request.attestationText,
        dueAt: request.dueAt,
        completedAt: null,
        signatureRecordRef: null,
      } as any
      acknowledgementsByDocumentId.set(documentId, [acknowledgement])
      return acknowledgement
    })

    const { container } = renderApp('/controlled-documents')

    expect(await screen.findByText('Controlled document management')).toBeTruthy()
    const createCard = screen.getByRole('heading', { name: 'Create controlled document' }).closest('.recordarr-card') as HTMLElement
    const createSelects = createCard.querySelectorAll('select') as NodeListOf<HTMLSelectElement>
    const createTextInputs = createCard.querySelectorAll('input.recordarr-input') as NodeListOf<HTMLInputElement>
    const createTextAreas = createCard.querySelectorAll('textarea.recordarr-textarea') as NodeListOf<HTMLTextAreaElement>

    await waitFor(() => {
      expect(createSelects[0]).not.toBeDisabled()
      expect(createSelects[1]).not.toBeDisabled()
      expect(createSelects[2]).not.toBeDisabled()
    })

    fireEvent.change(createTextInputs[0], { target: { value: 'Emergency procedure' } })
    fireEvent.change(createSelects[0], { target: { value: 'operations' } })
    fireEvent.change(createSelects[1], { target: { value: 'manifest' } })
    fireEvent.change(createSelects[2], { target: { value: 'outbound' } })
    fireEvent.change(createCard.querySelector('input[placeholder="Search StaffArr people"]') as HTMLInputElement, {
      target: { value: 'person-1' },
    })
    fireEvent.change(createCard.querySelector('input[placeholder="Search StaffArr org units"]') as HTMLInputElement, {
      target: { value: 'org-unit-1' },
    })
    fireEvent.change(createSelects[3], { target: { value: 'staffarr-site-main' } })
    fireEvent.change(createSelects[4], { target: { value: 'true' } })
    fireEvent.change(createTextAreas[0], { target: { value: 'Emergency controlled procedure' } })

    await waitFor(() => {
      expect(screen.getByRole('button', { name: 'Create document' })).not.toBeDisabled()
    })
    fireEvent.click(screen.getByRole('button', { name: 'Create document' }))

    await waitFor(() => {
      expect(vi.mocked(client.createControlledDocument)).toHaveBeenCalledTimes(1)
    })
    expect(await screen.findByText('DOC-2026-0001')).toBeTruthy()
    expect(await screen.findByText('Emergency procedure')).toBeTruthy()

    const versionCard = screen.getByRole('heading', { name: 'Version and review' }).closest('.recordarr-card') as HTMLElement
    const versionInputs = versionCard.querySelectorAll('input.recordarr-input') as NodeListOf<HTMLInputElement>
    const versionPeople = versionCard.querySelectorAll('input[placeholder="Search StaffArr people"]') as NodeListOf<HTMLInputElement>
    fireEvent.change(versionInputs[0], { target: { value: 'emergency-procedure-v2.pdf' } })
    fireEvent.change(versionInputs[1], { target: { value: 'Clarify emergency escalation steps' } })
    fireEvent.click(screen.getByRole('button', { name: 'Create version' }))

    await waitFor(() => {
      expect(vi.mocked(client.createDocumentVersion)).toHaveBeenCalledTimes(1)
    })
    expect(await screen.findByText('Emergency procedure')).toBeTruthy()

    fireEvent.change(versionInputs[2], { target: { value: 'periodic_review' } })
    fireEvent.change(versionPeople[1], { target: { value: 'person-1' } })
    fireEvent.click(screen.getByRole('button', { name: 'Request review' }))

    await waitFor(() => {
      expect(vi.mocked(client.createDocumentReview)).toHaveBeenCalledTimes(1)
    })
    expect((await screen.findAllByText('requested')).length).toBeGreaterThan(0)
    const distributionSelects = versionCard.querySelectorAll('select') as NodeListOf<HTMLSelectElement>
    fireEvent.change(distributionSelects[0], { target: { value: 'product' } })
    await waitFor(() => {
      expect((versionCard.querySelectorAll('select') as NodeListOf<HTMLSelectElement>).length).toBeGreaterThan(1)
    })
    fireEvent.change((versionCard.querySelectorAll('select') as NodeListOf<HTMLSelectElement>)[1], { target: { value: 'recordarr' } })
    const acknowledgementPeople = versionCard.querySelectorAll('input[placeholder="Search StaffArr people"]') as NodeListOf<HTMLInputElement>
    fireEvent.change(acknowledgementPeople[acknowledgementPeople.length - 1], { target: { value: 'person-1' } })
    const acknowledgementInputs = versionCard.querySelectorAll('input.recordarr-input') as NodeListOf<HTMLInputElement>
    fireEvent.change(acknowledgementInputs[acknowledgementInputs.length - 1], { target: { value: '2026-06-30T12:00:00.000Z' } })
    fireEvent.change(versionCard.querySelector('textarea.recordarr-textarea') as HTMLTextAreaElement, {
      target: { value: 'I acknowledge receipt and review of the controlled procedure.' },
    })
    fireEvent.click(screen.getByRole('button', { name: 'Create distribution' }))
    fireEvent.click(screen.getByRole('button', { name: 'Create acknowledgement' }))

    await waitFor(() => {
      expect(vi.mocked(client.createDocumentDistribution)).toHaveBeenCalledTimes(1)
      expect(vi.mocked(client.createDocumentAcknowledgement)).toHaveBeenCalledTimes(1)
    })
    expect(await screen.findByText('product')).toBeTruthy()
    expect(await screen.findByText('recordarr')).toBeTruthy()
    expect(await screen.findByText('person-1')).toBeTruthy()
    expect((await screen.findAllByText('I acknowledge receipt and review of the controlled procedure.')).length).toBeGreaterThan(0)
    expect(container).toBeTruthy()
  })

  it('creates an evidence mapping and shows coverage on a record detail page', async () => {
    const record = {
      recordId: 'record-1',
      recordNumber: 'REC-2026-2001',
      title: 'Operational evidence record',
      description: 'Record used to map evidence to a compliance requirement',
      recordType: 'document',
      documentClass: 'operations',
      documentType: 'manifest',
      documentSubtype: 'outbound',
      status: 'active',
      classification: 'internal',
      sourceProduct: 'recordarr',
      sourceObjectType: 'capture',
      sourceObjectId: 'capture-1',
      sourceObjectDisplayName: 'shipping manifest',
      ownerPersonId: 'person-1',
      uploadedByPersonId: 'person-1',
      uploadedAt: new Date().toISOString(),
      effectiveAt: null,
      expiresAt: null,
      archivedAt: null,
      purgedAt: null,
      currentFileName: 'shipping-manifest.pdf',
      currentMimeType: 'application/pdf',
      versionNumber: 1,
      tags: [],
      currentFileRef: 'file-1',
      fileRefs: ['file-1'],
      currentVersionRef: 'version-1',
      sourceObjectRefs: [],
      metadataRefs: [],
      versionRefs: [],
      ocrResultRefs: [],
      extractionResultRefs: [],
      evidenceMappingRefs: [],
      packageRefs: [],
      retentionPolicyRef: null,
      retentionStatusRef: null,
      legalHoldRefs: [],
      accessPolicyRef: null,
      complianceRefs: [],
      auditTrail: [],
      recordRef: null,
    } as any
    const evidenceMappings: any[] = []
    const evidenceCoverage: any[] = []

    vi.mocked(client.getSessionBootstrap).mockResolvedValue({
      userId: 'user-1',
      personId: 'person-1',
      tenantId: 'tenant-1',
      sessionId: 'session-1',
      tenantRoleKey: 'recordarr-ops',
      isPlatformAdmin: true,
      productKey: 'recordarr',
      hasRecordArrAccess: true,
      launchableProductKeys: ['recordarr'],
    })
    vi.mocked(client.listRecords).mockResolvedValue([record])
    vi.mocked(client.getRecord).mockResolvedValue(record)
    vi.mocked(client.listRecordMetadata).mockResolvedValue([])
    vi.mocked(client.listRecordLinks).mockResolvedValue([])
    vi.mocked(client.listRecordComments).mockResolvedValue([])
    vi.mocked(client.listFiles).mockResolvedValue([])
    vi.mocked(client.getRetentionStatus).mockResolvedValue(null as any)
    vi.mocked(client.listAccessLogs).mockResolvedValue([])
    vi.mocked(client.listScans).mockResolvedValue([])
    vi.mocked(client.listEvidenceMappings).mockImplementation(async () => [...evidenceMappings])
    vi.mocked(client.listEvidenceCoverage).mockImplementation(async () => [...evidenceCoverage])
    vi.mocked(client.listPackages).mockResolvedValue([])
    vi.mocked(client.listUploadSessions).mockResolvedValue([])
    vi.mocked(client.listLegalHolds).mockResolvedValue([])
    vi.mocked(client.listRedactions).mockResolvedValue([])
    vi.mocked(client.listControlledDocuments).mockResolvedValue([])
    vi.mocked(client.createEvidenceMapping).mockImplementation(async (_accessToken, body) => {
      const mapping = {
        evidenceMappingId: 'map-1',
        recordId: body.recordId,
        sourceProduct: body.sourceProduct,
        sourceObjectType: body.sourceObjectType,
        sourceObjectId: body.sourceObjectId,
        complianceRequirementRef: body.complianceRequirementRef,
        evidenceTypeKey: body.evidenceTypeKey,
        status: 'suggested',
        mappingSource: body.mappingSource,
        confidenceScore: body.confidenceScore,
        confirmedByPersonId: null,
        confirmedAt: null,
        rejectedByPersonId: null,
        rejectedAt: null,
        rejectionReason: null,
        notes: null,
      } as any
      evidenceMappings.push(mapping)
      evidenceCoverage.length = 0
      evidenceCoverage.push({
        evidenceCoverageId: 'cov-1',
        tenantId: 'tenant-1',
        sourceProduct: body.sourceProduct,
        sourceObjectRef: `${body.sourceProduct}:${body.sourceObjectType}:${body.sourceObjectId}`,
        complianceCoreRequirementRef: body.complianceRequirementRef,
        status: 'warning',
        recordRefs: [body.recordId],
        missingEvidenceTypes: [body.evidenceTypeKey],
        invalidRecordRefs: [],
        evaluatedAt: new Date().toISOString(),
        evaluationRef: 'eval-1',
      } as any)
      return mapping
    })

    const { container } = renderApp('/records/record-1')

    expect(await screen.findByText('Operational evidence record')).toBeTruthy()
    expect(await screen.findByText('Coverage for Operational evidence record')).toBeTruthy()
    expect(screen.getByText('Create an evidence mapping to a compliance requirement, then run or review a coverage evaluation.')).toBeTruthy()
    const evidenceCard = screen.getByRole('heading', { name: 'Create evidence mapping' }).closest('.recordarr-card') as HTMLElement
    fireEvent.change(screen.getByLabelText('Compliance requirement ref'), { target: { value: 'compliancecore:req-42' } })
    fireEvent.change(screen.getByLabelText('Evidence type'), { target: { value: 'site_visit_photo' } })
    fireEvent.change(screen.getByLabelText('Mapping source'), { target: { value: 'product_asserted' } })
    fireEvent.change(screen.getByLabelText('Confidence'), { target: { value: '0.85' } })
    fireEvent.click(screen.getByRole('button', { name: 'Create evidence mapping' }))

    await waitFor(() => {
      expect(vi.mocked(client.createEvidenceMapping)).toHaveBeenCalledTimes(1)
    })
    expect(vi.mocked(client.createEvidenceMapping)).toHaveBeenCalledWith(
      'token-1',
      expect.objectContaining({
        recordId: 'record-1',
        sourceProduct: 'recordarr',
        sourceObjectType: 'capture',
        sourceObjectId: 'capture-1',
        complianceRequirementRef: 'compliancecore:req-42',
        evidenceTypeKey: 'site_visit_photo',
        mappingSource: 'product_asserted',
        confidenceScore: 0.85,
      }),
    )
    expect((await screen.findAllByText('compliancecore:req-42')).length).toBeGreaterThan(1)
    expect(await screen.findByText('Coverage review')).toBeTruthy()
    expect(await screen.findByText('Coverage is partially satisfied.')).toBeTruthy()
    expect(screen.getByText('Partial coverage')).toBeTruthy()
    expect(screen.getByText('Mappings 1')).toBeTruthy()
    expect(await screen.findByText('suggested')).toBeTruthy()
    expect((await screen.findAllByText('warning')).length).toBeGreaterThan(0)
    expect((await screen.findAllByText(/site_visit_photo/)).length).toBeGreaterThan(1)
    expect(evidenceCard).toBeTruthy()
    expect(container).toBeTruthy()
  })

  it('captures photo evidence from the record detail workspace', async () => {
    const record = {
      recordId: 'record-1',
      recordNumber: 'REC-2026-2001',
      title: 'Operational evidence record',
      description: 'Record used to capture photo evidence',
      recordType: 'document',
      documentClass: 'operations',
      documentType: 'manifest',
      documentSubtype: 'outbound',
      status: 'active',
      classification: 'internal',
      sourceProduct: 'recordarr',
      sourceObjectType: 'capture',
      sourceObjectId: 'capture-1',
      sourceObjectDisplayName: 'shipping manifest',
      ownerPersonId: 'person-1',
      uploadedByPersonId: 'person-1',
      uploadedAt: new Date().toISOString(),
      effectiveAt: null,
      expiresAt: null,
      archivedAt: null,
      purgedAt: null,
      currentFileName: 'shipping-manifest.pdf',
      currentMimeType: 'application/pdf',
      versionNumber: 1,
      tags: [],
      currentFileRef: 'file-1',
      fileRefs: ['file-1'],
      currentVersionRef: 'version-1',
      sourceObjectRefs: [],
      metadataRefs: [],
      versionRefs: [],
      ocrResultRefs: [],
      extractionResultRefs: [],
      evidenceMappingRefs: [],
      packageRefs: [],
      retentionPolicyRef: null,
      retentionStatusRef: null,
      legalHoldRefs: [],
      accessPolicyRef: null,
      complianceRefs: [],
      auditTrail: [],
      recordRef: null,
    } as any

    vi.mocked(client.getSessionBootstrap).mockResolvedValue({
      userId: 'user-1',
      personId: 'person-1',
      tenantId: 'tenant-1',
      sessionId: 'session-1',
      tenantRoleKey: 'recordarr-ops',
      isPlatformAdmin: true,
      productKey: 'recordarr',
      hasRecordArrAccess: true,
      launchableProductKeys: ['recordarr'],
    })
    vi.mocked(client.listRecords).mockResolvedValue([record])
    vi.mocked(client.getRecord).mockResolvedValue(record)
    vi.mocked(client.listRecordMetadata).mockResolvedValue([])
    vi.mocked(client.listRecordLinks).mockResolvedValue([])
    vi.mocked(client.listRecordComments).mockResolvedValue([])
    vi.mocked(client.listFiles).mockResolvedValue([])
    vi.mocked(client.getRetentionStatus).mockResolvedValue(null as any)
    vi.mocked(client.listAccessLogs).mockResolvedValue([])
    vi.mocked(client.listScans).mockResolvedValue([])
    vi.mocked(client.listEvidenceMappings).mockResolvedValue([])
    vi.mocked(client.listEvidenceCoverage).mockResolvedValue([])
    vi.mocked(client.listPackages).mockResolvedValue([])
    vi.mocked(client.listUploadSessions).mockResolvedValue([])
    vi.mocked(client.listLegalHolds).mockResolvedValue([])
    vi.mocked(client.listRedactions).mockResolvedValue([])
    vi.mocked(client.listControlledDocuments).mockResolvedValue([])
    vi.mocked(client.createPhotoEvidence).mockImplementation(async (_accessToken, request) => ({
      photoEvidenceId: 'photo-1',
      recordId: request.recordId,
      photoPurpose: request.photoPurpose,
      capturedByPersonId: request.capturedByPersonId,
      sourceProduct: request.sourceProduct,
      sourceObjectRef: request.sourceObjectRef,
      geoCoordinates: request.geoCoordinates,
      deviceSnapshot: request.deviceSnapshot,
      notes: request.notes,
      createdAt: new Date().toISOString(),
    } as any))

    renderApp('/records/record-1')

    expect(await screen.findByText('Operational evidence record')).toBeTruthy()
    const photoSection = screen.getByRole('heading', { name: 'Photo evidence' }).parentElement as HTMLElement
    const photoSelect = photoSection.querySelector('select.recordarr-select') as HTMLSelectElement
    const photoNotes = photoSection.querySelector('textarea.recordarr-textarea') as HTMLTextAreaElement

    await waitFor(() => {
      expect(photoSelect).not.toBeDisabled()
    })

    fireEvent.change(photoSelect, { target: { value: 'completion' } })
    fireEvent.change(photoNotes, { target: { value: 'Photo shows the completed shipment' } })
    fireEvent.click(within(photoSection).getByRole('button', { name: 'Capture photo evidence' }))

    await waitFor(() => {
      expect(vi.mocked(client.createPhotoEvidence)).toHaveBeenCalledTimes(1)
    })
    expect(vi.mocked(client.createPhotoEvidence)).toHaveBeenCalledWith(
      'token-1',
      expect.objectContaining({
        recordId: 'record-1',
        photoPurpose: 'completion',
        capturedByPersonId: 'person-1',
        sourceProduct: '',
        sourceObjectRef: '',
        geoCoordinates: null,
        deviceSnapshot: null,
        notes: 'Photo shows the completed shipment',
      }),
    )
    expect(photoSection).toBeTruthy()
  })

  it('captures a signature from the record detail workspace', async () => {
    const record = {
      recordId: 'record-1',
      recordNumber: 'REC-2026-2001',
      title: 'Operational evidence record',
      description: 'Record used to capture signatures',
      recordType: 'document',
      documentClass: 'operations',
      documentType: 'manifest',
      documentSubtype: 'outbound',
      status: 'active',
      classification: 'internal',
      sourceProduct: 'recordarr',
      sourceObjectType: 'capture',
      sourceObjectId: 'capture-1',
      sourceObjectDisplayName: 'shipping manifest',
      ownerPersonId: 'person-1',
      uploadedByPersonId: 'person-1',
      uploadedAt: new Date().toISOString(),
      effectiveAt: null,
      expiresAt: null,
      archivedAt: null,
      purgedAt: null,
      currentFileName: 'shipping-manifest.pdf',
      currentMimeType: 'application/pdf',
      versionNumber: 1,
      tags: [],
      currentFileRef: 'file-1',
      fileRefs: ['file-1'],
      currentVersionRef: 'version-1',
      sourceObjectRefs: [],
      metadataRefs: [],
      versionRefs: [],
      ocrResultRefs: [],
      extractionResultRefs: [],
      evidenceMappingRefs: [],
      packageRefs: [],
      retentionPolicyRef: null,
      retentionStatusRef: null,
      legalHoldRefs: [],
      accessPolicyRef: null,
      complianceRefs: [],
      auditTrail: [],
      recordRef: null,
    } as any

    vi.mocked(client.getSessionBootstrap).mockResolvedValue({
      userId: 'user-1',
      personId: 'person-1',
      tenantId: 'tenant-1',
      sessionId: 'session-1',
      tenantRoleKey: 'recordarr-ops',
      isPlatformAdmin: true,
      productKey: 'recordarr',
      hasRecordArrAccess: true,
      launchableProductKeys: ['recordarr'],
    })
    vi.mocked(client.listRecords).mockResolvedValue([record])
    vi.mocked(client.getRecord).mockResolvedValue(record)
    vi.mocked(client.listRecordMetadata).mockResolvedValue([])
    vi.mocked(client.listRecordLinks).mockResolvedValue([])
    vi.mocked(client.listRecordComments).mockResolvedValue([])
    vi.mocked(client.listFiles).mockResolvedValue([])
    vi.mocked(client.getRetentionStatus).mockResolvedValue(null as any)
    vi.mocked(client.listAccessLogs).mockResolvedValue([])
    vi.mocked(client.listScans).mockResolvedValue([])
    vi.mocked(client.listEvidenceMappings).mockResolvedValue([])
    vi.mocked(client.listEvidenceCoverage).mockResolvedValue([])
    vi.mocked(client.listPackages).mockResolvedValue([])
    vi.mocked(client.listUploadSessions).mockResolvedValue([])
    vi.mocked(client.listLegalHolds).mockResolvedValue([])
    vi.mocked(client.listRedactions).mockResolvedValue([])
    vi.mocked(client.listControlledDocuments).mockResolvedValue([])
    vi.mocked(client.createSignatureRecord).mockImplementation(async (_accessToken, request) => ({
      signatureRecordId: 'signature-1',
      recordId: request.recordId,
      signaturePurpose: request.signaturePurpose,
      signerPersonId: request.signerPersonId,
      signerExternalName: request.signerExternalName,
      signerTitle: request.signerTitle,
      attestationText: request.attestationText,
      capturedByPersonId: request.capturedByPersonId,
      sourceProduct: request.sourceProduct,
      sourceObjectRef: request.sourceObjectRef,
      geoCoordinates: request.geoCoordinates,
      deviceSnapshot: request.deviceSnapshot,
      createdAt: new Date().toISOString(),
    } as any))

    renderApp('/records/record-1')

    expect(await screen.findByText('Operational evidence record')).toBeTruthy()
    const signatureSection = screen.getByRole('heading', { name: 'Signature' }).parentElement as HTMLElement
    const signatureSelect = signatureSection.querySelector('select.recordarr-select') as HTMLSelectElement
    const signatureNotes = signatureSection.querySelector('textarea.recordarr-textarea') as HTMLTextAreaElement

    await waitFor(() => {
      expect(signatureSelect).not.toBeDisabled()
    })

    fireEvent.change(signatureSelect, { target: { value: 'customer_acceptance' } })
    fireEvent.change(signatureNotes, { target: { value: 'I approve the record contents.' } })
    fireEvent.click(within(signatureSection).getByRole('button', { name: 'Capture signature' }))

    await waitFor(() => {
      expect(vi.mocked(client.createSignatureRecord)).toHaveBeenCalledTimes(1)
    })
    expect(vi.mocked(client.createSignatureRecord)).toHaveBeenCalledWith(
      'token-1',
      expect.objectContaining({
        recordId: 'record-1',
        signaturePurpose: 'customer_acceptance',
        signerPersonId: 'person-1',
        signerExternalName: null,
        signerTitle: null,
        attestationText: 'I approve the record contents.',
        capturedByPersonId: 'person-1',
        sourceProduct: '',
        sourceObjectRef: '',
        geoCoordinates: null,
        deviceSnapshot: null,
      }),
    )
    expect(signatureSection).toBeTruthy()
  })

  it('creates a package, inspects its manifest, and exports the package packet', async () => {
    const record = {
      recordId: 'record-1',
      recordNumber: 'REC-2026-3001',
      title: 'Package source record',
      status: 'active',
      purgedAt: null,
    } as any
    const packages: any[] = []
    const manifestsByPackageId = new Map<string, any>()

    vi.mocked(client.getSessionBootstrap).mockResolvedValue({
      userId: 'user-1',
      personId: 'person-1',
      tenantId: 'tenant-1',
      sessionId: 'session-1',
      tenantRoleKey: 'recordarr-ops',
      isPlatformAdmin: true,
      productKey: 'recordarr',
      hasRecordArrAccess: true,
      launchableProductKeys: ['recordarr'],
    })
    vi.mocked(client.listRecords).mockResolvedValue([record])
    vi.mocked(client.listPackages).mockImplementation(async () => [...packages])
    vi.mocked(client.getPackageManifest).mockImplementation(async (_accessToken, packageId) => {
      const manifest = manifestsByPackageId.get(packageId)
      if (!manifest) {
        throw new Error(`Manifest ${packageId} not found.`)
      }
      return manifest
    })
    vi.mocked(client.createPackage).mockImplementation(async (_accessToken, request) => {
      const now = new Date().toISOString()
      const packageId = 'pkg-1'
      const packageEntry = {
        packageId,
        packageNumber: 'PKG-2026-0001',
        title: request.title,
        packageType: request.packageType,
        status: 'complete',
        sourceProduct: request.sourceProduct,
        sourceObjectRefs: [request.sourceObjectRef],
        recordRefs: [request.recordRef],
        manifestChecksum: 'checksum-1',
        generatedPdfRecordRef: 'record-pdf-1',
        generatedZipFileRef: 'file-zip-1',
        createdAt: now,
        completedAt: now,
        lockedAt: null,
        archivedAt: null,
        expiresAt: null,
      } as any
      packages.push(packageEntry)
      manifestsByPackageId.set(packageId, {
        manifestId: 'manifest-1',
        packageId,
        manifestVersion: 1,
        generatedAt: now,
        recordEntries: [
          {
            entryId: 'record-entry-1',
            entryType: 'record',
            displayName: 'Package source record',
            sourceProduct: 'recordarr',
            sourceObjectRef: 'recordarr:capture:capture-1',
            recordRef: 'record-1',
            complianceRequirementRef: null,
            statusSnapshot: 'active',
            checksum: 'record-checksum-1',
          },
        ],
        sourceObjectEntries: [
          {
            entryId: 'source-entry-1',
            entryType: 'source_object',
            displayName: 'StaffArr source snapshot',
            sourceProduct: 'staffarr',
            sourceObjectRef: request.sourceObjectRef,
            recordRef: null,
            complianceRequirementRef: null,
            statusSnapshot: 'linked',
            checksum: 'source-checksum-1',
          },
        ],
        requirementEntries: [
          {
            entryId: 'requirement-entry-1',
            entryType: 'requirement',
            displayName: 'Evidence requirement',
            sourceProduct: 'staffarr',
            sourceObjectRef: request.sourceObjectRef,
            recordRef: request.recordRef,
            complianceRequirementRef: 'compliancecore:req-7',
            statusSnapshot: 'linked',
            checksum: 'requirement-checksum-1',
          },
        ],
        checksum: 'manifest-checksum-1',
        generatedByPersonId: 'person-1',
      } as any)
      return packageEntry
    })
    vi.mocked(client.lockPackage).mockImplementation(async (_accessToken, packageId) => {
      const current = packages.find((pkg) => pkg.packageId === packageId)
      if (!current) {
        throw new Error(`Package ${packageId} not found.`)
      }
      current.status = 'locked'
      current.lockedAt = new Date().toISOString()
      return current
    })
    vi.mocked(client.archivePackage).mockImplementation(async (_accessToken, packageId) => {
      const current = packages.find((pkg) => pkg.packageId === packageId)
      if (!current) {
        throw new Error(`Package ${packageId} not found.`)
      }
      current.status = 'archived'
      current.archivedAt = new Date().toISOString()
      return current
    })
    vi.mocked(client.downloadPackage).mockResolvedValue('package export packet')

    renderApp('/packages')

    expect(await screen.findByText('Evidence packet builder')).toBeTruthy()
    fireEvent.change(screen.getByLabelText('Title'), { target: { value: 'Audit evidence packet' } })
    fireEvent.change(screen.getByLabelText('Package type'), { target: { value: 'audit' } })
    fireEvent.change(screen.getByLabelText('Source product'), { target: { value: 'staffarr' } })
    fireEvent.change(screen.getByLabelText('Source object ref'), { target: { value: 'staffarr:person:person-1' } })
    fireEvent.change(screen.getByLabelText('Record'), { target: { value: 'record-1' } })
    fireEvent.click(screen.getByRole('button', { name: 'Create package' }))

    await waitFor(() => {
      expect(vi.mocked(client.createPackage)).toHaveBeenCalledTimes(1)
    })
    expect(vi.mocked(client.createPackage)).toHaveBeenCalledWith(
      'token-1',
      expect.objectContaining({
        title: 'Audit evidence packet',
        packageType: 'audit',
        sourceProduct: 'staffarr',
        sourceObjectRef: 'staffarr:person:person-1',
        recordRef: 'record-1',
      }),
    )
    expect((await screen.findAllByText('PKG-2026-0001')).length).toBeGreaterThan(1)
    expect(await screen.findByText('checksum-1')).toBeTruthy()
    expect(await screen.findByText('Package source record')).toBeTruthy()
    expect(await screen.findByText('StaffArr source snapshot')).toBeTruthy()
    expect(await screen.findByText('compliancecore:req-7')).toBeTruthy()

    fireEvent.click(screen.getByRole('button', { name: 'Lock selected package' }))
    await waitFor(() => {
      expect(vi.mocked(client.lockPackage)).toHaveBeenCalledTimes(1)
    })
    expect((await screen.findAllByText('locked')).length).toBeGreaterThan(0)

    fireEvent.click(screen.getByRole('button', { name: 'Archive selected package' }))
    await waitFor(() => {
      expect(vi.mocked(client.archivePackage)).toHaveBeenCalledTimes(1)
    })
    expect((await screen.findAllByText('archived')).length).toBeGreaterThan(0)

    const anchorClickSpy = vi.spyOn(HTMLAnchorElement.prototype, 'click').mockImplementation(() => undefined)
    fireEvent.click(screen.getByRole('button', { name: 'Download package' }))
    await waitFor(() => {
      expect(vi.mocked(client.downloadPackage)).toHaveBeenCalledTimes(1)
    })
    expect(URL.createObjectURL).toHaveBeenCalled()
    expect(packages[0].status).toBe('archived')
    anchorClickSpy.mockRestore()
  })

  it('creates, activates, and releases a legal hold from the hold workspace', async () => {
    const holds: any[] = []

    vi.mocked(client.listLegalHolds).mockImplementation(async () => [...holds])
    vi.mocked(client.createLegalHold).mockImplementation(async (_accessToken, request) => {
      const hold = {
        legalHoldId: 'hold-1',
        holdNumber: 'HOLD-2026-0001',
        title: request.title,
        description: request.description,
        status: 'draft',
        holdType: request.holdType,
        scopeRules: request.scopeRules,
        recordRefs: request.recordRefs,
        sourceProduct: request.sourceProduct,
        sourceObjectType: request.sourceObjectType,
        sourceObjectId: request.sourceObjectId,
        createdAt: new Date().toISOString(),
        createdByPersonId: request.createdByPersonId,
        activatedAt: null,
        releasedAt: null,
        releasedByPersonId: null,
        releaseReason: null,
      } as any
      holds.push(hold)
      return hold
    })
    vi.mocked(client.activateLegalHold).mockImplementation(async (_accessToken, holdId) => {
      const hold = holds.find((item) => item.legalHoldId === holdId)
      if (!hold) {
        throw new Error(`Hold ${holdId} not found.`)
      }
      hold.status = 'active'
      hold.activatedAt = new Date().toISOString()
      return hold
    })
    vi.mocked(client.releaseLegalHold).mockImplementation(async (_accessToken, holdId, request) => {
      const hold = holds.find((item) => item.legalHoldId === holdId)
      if (!hold) {
        throw new Error(`Hold ${holdId} not found.`)
      }
      hold.status = 'released'
      hold.releasedAt = new Date().toISOString()
      hold.releasedByPersonId = request.releasedByPersonId
      hold.releaseReason = request.releaseReason
      return hold
    })

    renderApp('/holds')

    expect(await screen.findByText('Legal hold management')).toBeTruthy()
    fireEvent.change(screen.getByLabelText('Title'), { target: { value: 'Audit preservation hold' } })
    fireEvent.change(screen.getByLabelText('Hold type'), { target: { value: 'audit' } })
    fireEvent.change(screen.getByLabelText('Source product'), { target: { value: 'staffarr' } })
    fireEvent.change(screen.getByLabelText('Source reference'), { target: { value: 'staffarr:case:hold-1' } })
    fireEvent.change(screen.getByLabelText('Scope rules'), {
      target: { value: 'recordarr.records.read\nrecordarr.files.download' },
    })
    fireEvent.change(screen.getByLabelText('Record refs'), { target: { value: 'record-1' } })
    fireEvent.change(screen.getByLabelText('Description'), {
      target: { value: 'Preserve evidence during internal review' },
    })
    fireEvent.click(screen.getByRole('button', { name: 'Create hold' }))

    await waitFor(() => {
      expect(vi.mocked(client.createLegalHold)).toHaveBeenCalledTimes(1)
    })
    expect(vi.mocked(client.createLegalHold)).toHaveBeenCalledWith(
      'token-1',
      expect.objectContaining({
        title: 'Audit preservation hold',
        holdType: 'audit',
        sourceProduct: 'staffarr',
        sourceObjectType: 'case',
        sourceObjectId: 'hold-1',
        recordRefs: ['record-1'],
        scopeRules: ['recordarr.records.read', 'recordarr.files.download'],
      }),
    )
    expect((await screen.findAllByText('HOLD-2026-0001')).length).toBeGreaterThan(0)

    fireEvent.click(screen.getByRole('button', { name: 'Activate selected' }))
    await waitFor(() => {
      expect(vi.mocked(client.activateLegalHold)).toHaveBeenCalledTimes(1)
    })
    expect((await screen.findAllByText('active')).length).toBeGreaterThan(0)

    fireEvent.click(screen.getByRole('button', { name: 'Release selected' }))
    await waitFor(() => {
      expect(vi.mocked(client.releaseLegalHold)).toHaveBeenCalledTimes(1)
    })
    expect((await screen.findAllByText('released')).length).toBeGreaterThan(0)
    expect(holds[0].status).toBe('released')
  })

  it('creates and manages access controls, external shares, and redactions from the access workspace', async () => {
    const records = [
      {
        recordId: 'record-1',
        recordNumber: 'REC-2026-4001',
        title: 'Confidential operating record',
        status: 'active',
        purgedAt: null,
      },
      {
        recordId: 'record-2',
        recordNumber: 'REC-2026-4002',
        title: 'Redacted operating record',
        status: 'active',
        purgedAt: null,
      },
    ] as any[]
    const policies: any[] = []
    const grants: any[] = []
    const shares: any[] = []
    const redactions: any[] = []
    const logs: any[] = []

    vi.mocked(client.getSessionBootstrap).mockResolvedValue({
      userId: 'user-1',
      personId: 'person-1',
      tenantId: 'tenant-1',
      sessionId: 'session-1',
      tenantRoleKey: 'recordarr-ops',
      isPlatformAdmin: true,
      productKey: 'recordarr',
      hasRecordArrAccess: true,
      launchableProductKeys: ['recordarr'],
    })
    vi.mocked(client.listRecords).mockResolvedValue(records)
    vi.mocked(client.listAccessPolicies).mockImplementation(async () => [...policies])
    vi.mocked(client.listAccessGrants).mockImplementation(async () => [...grants])
    vi.mocked(client.listExternalShares).mockImplementation(async () => [...shares])
    vi.mocked(client.listRedactions).mockImplementation(async () => [...redactions])
    vi.mocked(client.listDisposalReviews).mockResolvedValue([])
    vi.mocked(client.listAccessLogs).mockImplementation(async () => [...logs])

    vi.mocked(client.createExternalShare).mockImplementation(async (_accessToken, request) => {
      const now = new Date().toISOString()
      const share = {
        externalShareId: 'share-1',
        shareNumber: 'SHR-2026-0001',
        recordId: request.recordId,
        recipientName: request.recipientName,
        recipientEmail: request.recipientEmail,
        sharePurpose: request.sharePurpose,
        allowedActions: [...request.allowedActions],
        status: 'created',
        createdByPersonId: request.createdByPersonId,
        createdAt: now,
        expiresAt: new Date(Date.now() + 7 * 24 * 60 * 60 * 1000).toISOString(),
        revokedAt: null,
        revokedByPersonId: null,
        expiredAt: null,
        accessCount: 0,
      } as any
      shares.push(share)
      return share
    })
    vi.mocked(client.recordExternalShareAccess).mockImplementation(async (_accessToken, externalShareId, request) => {
      const share = shares.find((item) => item.externalShareId === externalShareId)
      if (!share) {
        throw new Error(`Share ${externalShareId} not found.`)
      }
      share.status = 'active'
      share.accessCount = (share.accessCount ?? 0) + 1
      const log = {
        accessLogId: `log-${logs.length + 1}`,
        recordId: share.recordId,
        externalShareId: share.externalShareId,
        action: request.accessAction,
        result: 'recorded',
        occurredAt: new Date().toISOString(),
        actorPersonId: request.accessedByPersonId,
        sourceIp: request.sourceIp,
        userAgent: request.userAgent,
      } as any
      logs.push(log)
      return share
    })
    vi.mocked(client.expireExternalShare).mockImplementation(async (_accessToken, externalShareId, request) => {
      const share = shares.find((item) => item.externalShareId === externalShareId)
      if (!share) {
        throw new Error(`Share ${externalShareId} not found.`)
      }
      share.status = 'expired'
      share.expiredAt = new Date().toISOString()
      share.expiredByPersonId = request.expiredByPersonId
      return share
    })
    vi.mocked(client.revokeExternalShare).mockImplementation(async (_accessToken, externalShareId, request) => {
      const share = shares.find((item) => item.externalShareId === externalShareId)
      if (!share) {
        throw new Error(`Share ${externalShareId} not found.`)
      }
      share.status = 'revoked'
      share.revokedAt = new Date().toISOString()
      share.revokedByPersonId = request.revokedByPersonId
      return share
    })
    vi.mocked(client.createAccessGrant).mockImplementation(async (_accessToken, request) => {
      const grant = {
        accessGrantId: 'grant-1',
        recordId: request.recordId,
        granteeType: request.granteeType,
        granteeRef: request.granteeRef,
        permission: request.permission,
        status: 'active',
        grantedByPersonId: request.grantedByPersonId,
        createdAt: new Date().toISOString(),
        expiresAt: request.expiresAt,
        revokedAt: null,
        revokedByPersonId: null,
      } as any
      grants.push(grant)
      return grant
    })
    vi.mocked(client.revokeAccessGrant).mockImplementation(async (_accessToken, accessGrantId, request) => {
      const grant = grants.find((item) => item.accessGrantId === accessGrantId)
      if (!grant) {
        throw new Error(`Grant ${accessGrantId} not found.`)
      }
      grant.status = 'revoked'
      grant.revokedAt = new Date().toISOString()
      grant.revokedByPersonId = request.revokedByPersonId
      return grant
    })
    vi.mocked(client.createAccessPolicy).mockImplementation(async (_accessToken, request) => {
      const policy = {
        accessPolicyId: 'policy-1',
        recordId: request.recordId,
        policyType: request.policyType,
        status: request.status,
        readRules: [...request.readRules],
        writeRules: [...request.writeRules],
        downloadRules: [...request.downloadRules],
        shareRules: [...request.shareRules],
        exportRules: [...request.exportRules],
        purgeRules: [...request.purgeRules],
        createdByPersonId: request.createdByPersonId,
        updatedByPersonId: null,
        createdAt: new Date().toISOString(),
      } as any
      policies.push(policy)
      return policy
    })
    vi.mocked(client.updateAccessPolicy).mockImplementation(async (_accessToken, accessPolicyId, request) => {
      const policy = policies.find((item) => item.accessPolicyId === accessPolicyId)
      if (!policy) {
        throw new Error(`Policy ${accessPolicyId} not found.`)
      }
      policy.recordId = request.recordId
      policy.policyType = request.policyType
      policy.status = request.status
      policy.readRules = [...request.readRules]
      policy.writeRules = [...request.writeRules]
      policy.downloadRules = [...request.downloadRules]
      policy.shareRules = [...request.shareRules]
      policy.exportRules = [...request.exportRules]
      policy.purgeRules = [...request.purgeRules]
      policy.updatedByPersonId = request.updatedByPersonId
      policy.updatedAt = new Date().toISOString()
      return policy
    })
    vi.mocked(client.createRedaction).mockImplementation(async (_accessToken, request) => {
      const redaction = {
        redactionId: 'redaction-1',
        sourceRecordId: request.sourceRecordId,
        redactedRecordId: request.redactedRecordId,
        redactionReason: request.redactionReason,
        redactedByPersonId: request.redactedByPersonId,
        redactionRules: [...request.redactionRules],
        status: 'draft',
        createdAt: new Date().toISOString(),
      } as any
      redactions.push(redaction)
      return redaction
    })

    renderApp('/access')

    expect(await screen.findByText('Access controls and trail')).toBeTruthy()
    await waitFor(() => {
      expect(screen.getByLabelText('Share record')).not.toBeDisabled()
    })

    fireEvent.change(screen.getByLabelText('Share record'), { target: { value: 'record-1' } })
    fireEvent.change(screen.getByLabelText('Recipient name'), { target: { value: 'Northwind Legal' } })
    fireEvent.change(screen.getByLabelText('Recipient email'), { target: { value: 'legal@northwind.test' } })
    fireEvent.change(screen.getByLabelText('Share purpose'), { target: { value: 'external counsel review' } })
    fireEvent.change(screen.getByLabelText('Allowed actions'), {
      target: { value: 'view\ndownload' },
    })
    fireEvent.click(screen.getByRole('button', { name: 'Create external share' }))

    await waitFor(() => {
      expect(vi.mocked(client.createExternalShare)).toHaveBeenCalledTimes(1)
    })
    expect(vi.mocked(client.createExternalShare)).toHaveBeenCalledWith(
      'token-1',
      expect.objectContaining({
        recordId: 'record-1',
        recipientName: 'Northwind Legal',
        recipientEmail: 'legal@northwind.test',
        sharePurpose: 'external counsel review',
        allowedActions: ['view', 'download'],
      }),
    )
    expect(await screen.findByText('SHR-2026-0001')).toBeTruthy()
    expect(await screen.findByText('Northwind Legal · legal@northwind.test')).toBeTruthy()

    fireEvent.click(screen.getByRole('button', { name: 'Activate / log access' }))
    await waitFor(() => {
      expect(vi.mocked(client.recordExternalShareAccess)).toHaveBeenCalledTimes(1)
    })
    expect(await screen.findByText('view')).toBeTruthy()
    expect(await screen.findByText('recorded')).toBeTruthy()
    await waitFor(() => {
      expect(screen.getByRole('button', { name: 'Log access' })).not.toBeDisabled()
    })

    fireEvent.click(screen.getByRole('button', { name: 'Expire share' }))
    await waitFor(() => {
      expect(vi.mocked(client.expireExternalShare)).toHaveBeenCalledTimes(1)
    })
    await waitFor(() => {
      expect(screen.getByRole('button', { name: 'Log access' })).toBeDisabled()
    })

    fireEvent.change(screen.getByLabelText('Grant record'), { target: { value: 'record-1' } })
    fireEvent.change(screen.getByLabelText('Grantee type'), { target: { value: 'person' } })
    fireEvent.change(screen.getByLabelText('Grantee reference'), { target: { value: 'person-2' } })
    fireEvent.change(screen.getByLabelText('Permission'), { target: { value: 'read' } })
    fireEvent.click(screen.getByRole('button', { name: 'Create access grant' }))

    await waitFor(() => {
      expect(vi.mocked(client.createAccessGrant)).toHaveBeenCalledTimes(1)
    })
    expect(vi.mocked(client.createAccessGrant)).toHaveBeenCalledWith(
      'token-1',
      expect.objectContaining({
        recordId: 'record-1',
        granteeType: 'person',
        granteeRef: 'person-2',
        permission: 'read',
      }),
    )
    expect(await screen.findByText('person-2')).toBeTruthy()
    expect(await screen.findByText('read')).toBeTruthy()
    expect(grants[0].status).toBe('active')

    fireEvent.change(screen.getByLabelText('Policy record'), { target: { value: 'record-1' } })
    fireEvent.change(screen.getByLabelText('Policy type'), { target: { value: 'restricted' } })
    fireEvent.change(screen.getByLabelText('Read rules'), { target: { value: 'recordarr.records.read' } })
    fireEvent.change(screen.getByLabelText('Write rules'), { target: { value: 'recordarr.records.write' } })
    fireEvent.change(screen.getByLabelText('Download rules'), { target: { value: 'recordarr.files.download' } })
    fireEvent.change(screen.getByLabelText('Share rules'), { target: { value: 'recordarr.records.share' } })
    fireEvent.change(screen.getByLabelText('Export rules'), { target: { value: 'recordarr.records.export' } })
    fireEvent.change(screen.getByLabelText('Purge rules'), { target: { value: 'recordarr.records.purge' } })
    fireEvent.click(screen.getByRole('button', { name: 'Create access policy' }))

    await waitFor(() => {
      expect(vi.mocked(client.createAccessPolicy)).toHaveBeenCalledTimes(1)
    })
    expect(await screen.findByText('restricted')).toBeTruthy()
    expect(await screen.findByText('recordarr.records.read')).toBeTruthy()
    fireEvent.click(screen.getByRole('button', { name: 'Deactivate policy' }))

    await waitFor(() => {
      expect(vi.mocked(client.updateAccessPolicy)).toHaveBeenCalledTimes(1)
    })
    expect(await screen.findByText('inactive')).toBeTruthy()

    fireEvent.change(screen.getByLabelText('Redaction source record'), { target: { value: 'record-1' } })
    fireEvent.change(screen.getByLabelText('Redacted record'), { target: { value: 'record-2' } })
    fireEvent.change(screen.getByLabelText('Redaction reason'), { target: { value: 'privacy review' } })
    fireEvent.change(screen.getByLabelText('Redaction rules'), { target: { value: 'mask:ssn' } })
    fireEvent.click(screen.getByRole('button', { name: 'Create redacted copy' }))

    await waitFor(() => {
      expect(vi.mocked(client.createRedaction)).toHaveBeenCalledTimes(1)
    })
    expect(await screen.findByText('record-2')).toBeTruthy()
    expect(await screen.findByText('privacy review')).toBeTruthy()
    expect(redactions[0].status).toBe('draft')
    expect(shares[0].status).toBe('expired')
    expect(grants[0].status).toBe('active')
    expect(policies[0].status).toBe('inactive')
    expect(logs[0].action).toBe('view')
  })

  it('recalculates retention and completes a disposal review from the retention workspace', async () => {
    const records = [
      {
        recordId: 'record-1',
        recordNumber: 'REC-2026-5001',
        title: 'Retention candidate record',
        status: 'active',
        purgedAt: null,
      },
    ] as any[]
    const retentionPolicies = [
      {
        retentionPolicyId: 'policy-1',
        policyKey: 'ops-retain-7y',
        title: 'Operations seven-year retention',
        description: 'Keep operational evidence for seven years.',
        recordTypeApplicability: 'document',
        documentTypeApplicability: 'manifest',
        sourceProductApplicability: 'recordarr',
        retainFor: 7,
        retentionUnit: 'years',
        retentionStartTrigger: 'record.created',
        disposalAction: 'archive',
        legalHoldOverrides: true,
        status: 'active',
        createdAt: new Date().toISOString(),
      },
    ] as any[]
    const retentionStatuses = [
      {
        retentionStatusId: 'status-1',
        recordId: 'record-1',
        retentionPolicyRef: 'policy-1',
        status: 'blocked_by_legal_hold',
        retentionStartAt: new Date().toISOString(),
        retentionExpiresAt: new Date(Date.now() + 30 * 24 * 60 * 60 * 1000).toISOString(),
        nextReviewAt: new Date(Date.now() + 14 * 24 * 60 * 60 * 1000).toISOString(),
        lastReviewedAt: null,
        reviewedByPersonId: null,
        disposalReviewRef: null,
      },
    ] as any[]
    const holds = [
      {
        legalHoldId: 'hold-1',
        holdNumber: 'HOLD-2026-0001',
        title: 'Retention review hold',
        description: 'Preserve candidate record during legal review',
        status: 'active',
        holdType: 'audit',
        scopeRules: ['recordarr.records.read'],
        recordRefs: ['record-1'],
        sourceProduct: 'staffarr',
        sourceObjectType: 'case',
        sourceObjectId: 'hold-1',
        createdAt: new Date().toISOString(),
        createdByPersonId: 'person-1',
        activatedAt: new Date().toISOString(),
        releasedAt: null,
        releasedByPersonId: null,
        releaseReason: null,
      },
    ] as any[]
    const disposalReviews: any[] = []

    vi.mocked(client.getSessionBootstrap).mockResolvedValue({
      userId: 'user-1',
      personId: 'person-1',
      tenantId: 'tenant-1',
      sessionId: 'session-1',
      tenantRoleKey: 'recordarr-ops',
      isPlatformAdmin: true,
      productKey: 'recordarr',
      hasRecordArrAccess: true,
      launchableProductKeys: ['recordarr'],
    })
    vi.mocked(client.listRecords).mockResolvedValue(records)
    vi.mocked(client.listRetentionPolicies).mockResolvedValue(retentionPolicies)
    vi.mocked(client.getRetentionStatus).mockImplementation(async (_accessToken, recordId) => {
      return retentionStatuses.find((status) => status.recordId === recordId) ?? null
    })
    vi.mocked(client.listLegalHolds).mockImplementation(async () => [...holds])
    vi.mocked(client.listDisposalReviews).mockImplementation(async () => [...disposalReviews])
    vi.mocked(client.recalculateRetentionStatuses).mockImplementation(async () => [...retentionStatuses])
    vi.mocked(client.createDisposalReview).mockImplementation(async (_accessToken, request) => {
      const review = {
        disposalReviewId: 'review-1',
        recordId: request.recordId,
        retentionStatusRef: request.retentionStatusRef,
        proposedAction: request.proposedAction,
        status: 'requested',
        requestedAt: new Date().toISOString(),
        requestedByPersonId: request.requestedByPersonId,
        reviewedByPersonId: null,
        reviewedAt: null,
        decisionReason: null,
        completedAt: null,
      } as any
      disposalReviews.push(review)
      return review
    })
    vi.mocked(client.completeDisposalReview).mockImplementation(async (_accessToken, reviewId, request) => {
      const review = disposalReviews.find((item) => item.disposalReviewId === reviewId)
      if (!review) {
        throw new Error(`Review ${reviewId} not found.`)
      }
      review.status = request.status
      review.reviewedByPersonId = request.reviewedByPersonId ?? null
      review.reviewedAt = new Date().toISOString()
      review.decisionReason = request.decisionReason ?? null
      review.completedAt = new Date().toISOString()
      return review
    })

    renderApp('/retention')

    expect(await screen.findByText('Retention rules and expiration')).toBeTruthy()
    const retentionCard = screen.getByRole('heading', { name: 'Record retention status' }).closest('.recordarr-card') as HTMLElement
    const retentionSelect = retentionCard.querySelector('select') as HTMLSelectElement
    await waitFor(() => {
      expect(retentionSelect.querySelector('option[value="record-1"]')).not.toBeNull()
    })
    fireEvent.change(retentionSelect, { target: { value: 'record-1' } })

    await waitFor(() => {
      expect(screen.getByText('blocked_by_legal_hold')).toBeTruthy()
      expect(screen.getByText('HOLD-2026-0001')).toBeTruthy()
    })

    fireEvent.click(within(retentionCard).getByRole('button', { name: 'Refresh retention scheduler' }))
    await waitFor(() => {
      expect(vi.mocked(client.recalculateRetentionStatuses)).toHaveBeenCalledTimes(1)
    })

    const disposalCard = screen.getByRole('heading', { name: 'Disposal review' }).closest('.recordarr-card') as HTMLElement
    const disposalSelect = disposalCard.querySelector('select') as HTMLSelectElement
    const disposalAction = disposalCard.querySelector('input.recordarr-input') as HTMLInputElement
    await waitFor(() => {
      expect(disposalSelect.querySelector('option[value="record-1"]')).not.toBeNull()
    })
    fireEvent.change(disposalSelect, { target: { value: 'record-1' } })
    fireEvent.change(disposalAction, { target: { value: 'destroy' } })

    fireEvent.click(within(disposalCard).getByRole('button', { name: 'Create disposal review' }))
    await waitFor(() => {
      expect(vi.mocked(client.createDisposalReview)).toHaveBeenCalledTimes(1)
    })
    expect(await screen.findByText('requested')).toBeTruthy()

    const completeCard = screen.getByRole('heading', { name: 'Complete review' }).closest('.recordarr-card') as HTMLElement
    const completeSelect = completeCard.querySelector('select.recordarr-select') as HTMLSelectElement
    const completeTextarea = completeCard.querySelector('textarea.recordarr-textarea') as HTMLTextAreaElement
    fireEvent.change(completeSelect, { target: { value: 'completed' } })
    fireEvent.change(completeTextarea, { target: { value: 'Retention review completed after legal review' } })
    fireEvent.click(within(completeCard).getByRole('button', { name: 'Apply completion' }))

    await waitFor(() => {
      expect(vi.mocked(client.completeDisposalReview)).toHaveBeenCalledTimes(1)
    })
    expect(await screen.findByText('completed')).toBeTruthy()
    expect(disposalReviews[0].status).toBe('completed')
  })
})
