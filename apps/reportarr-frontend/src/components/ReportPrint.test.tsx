import { renderToString } from 'react-dom/server'
import type { ReactElement } from 'react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import {
  AuditPackagePrintPreview,
  DashboardPrintPreview,
  ReportRunPrintPreview,
  ReportSchedulePrintPreview,
} from './ReportPrint'

vi.mock('@stl/shared-ui', () => ({
  PacketPreview: ({ title, sections }: { title: string; sections: Array<{ title: string; content: unknown }> }) => (
    <section data-testid="packet-preview">
      <h2>{title}</h2>
      {sections.map((section) => (
        <article key={section.title}>
          <h3>{section.title}</h3>
          <div>{section.content as any}</div>
        </article>
      ))}
    </section>
  ),
  PrintableDocumentHeader: ({ title, metadata }: { title: string; metadata: unknown }) => (
    <header data-testid="print-header">
      <h1>{title}</h1>
      <div>{metadata as any}</div>
    </header>
  ),
  PrintableDocumentShell: ({
    title,
    subtitle,
    productLabel,
    tenantLabel,
    sourceDisplayRef,
    documentStatus,
    generatedBy,
    watermarkLabel,
    footer,
    children,
  }: {
    title: string
    subtitle: string
    productLabel?: string
    tenantLabel?: string
    sourceDisplayRef?: string
    documentStatus?: string
    generatedBy?: string
    watermarkLabel?: string
    footer: unknown
    children: unknown
  }) => (
    <section
      data-testid="print-shell"
      data-title={title}
      data-subtitle={subtitle}
      data-product={productLabel ?? ''}
      data-tenant={tenantLabel ?? ''}
      data-source={sourceDisplayRef ?? ''}
      data-status={documentStatus ?? ''}
      data-generated-by={generatedBy ?? ''}
      data-watermark={watermarkLabel ?? ''}
    >
      <div>{children as any}</div>
      <footer>{footer as any}</footer>
    </section>
  ),
  downloadPrintPdf: vi.fn(),
}))

function renderMarkup(node: ReactElement) {
  return renderToString(node)
}

describe('ReportPrint', () => {
  afterEach(() => {
    vi.clearAllMocks()
  })

  it('renders dashboard snapshots as purpose-built print documents', () => {
    const html = renderMarkup(
      <DashboardPrintPreview
        dashboard={
          {
            dashboardNumber: 'dash-001',
            dashboardKey: 'quality-overview',
            title: 'Quality overview',
            dashboardType: 'management',
            status: 'published',
            freshnessStatus: 'fresh',
            defaultDateRange: 'Last 30 days',
            lastViewedAt: '2026-06-25T12:00:00Z',
            description: 'Dashboard summary for review.',
          } as any
        }
        policy={
          {
            exportAllowed: true,
            visibility: 'tenant',
          } as any
        }
        filters={[
          {
            label: 'Site',
            filterType: 'reference',
            required: true,
            defaultValue: 'South Depot',
          },
        ] as any}
        drilldowns={[
          {
            title: 'Open by site',
            targetType: 'dashboard',
            status: 'active',
          },
        ] as any}
        widgets={[
          {
            title: 'Quality trend',
            widgetType: 'chart',
            status: 'active',
            freshnessStatus: 'fresh',
            datasetRef: 'dataset-quality',
            readModelRef: 'read-model-quality',
          },
        ] as any}
        actorDisplayName="Demo Admin"
        tenantDisplayName="Demo Tenant"
      />,
    )

    expect(html).toContain('data-testid="print-shell"')
    expect(html).toContain('data-product="ReportArr"')
    expect(html).toContain('data-tenant="Demo Tenant"')
    expect(html).toContain('data-source="dash-001"')
    expect(html).toContain('data-status="working_copy"')
    expect(html).toContain('data-watermark="Working copy"')
    expect(html).toContain('Quality overview')
    expect(html).toContain('Snapshot summary')
    expect(html).toContain('Source boundaries')
    expect(html).toContain('Widget summary')
    expect(html).toContain('Filters and drilldowns')
    expect(html).toContain('This preview hides workspace chrome and labels the output as a working copy.')
  })

  it('renders report runs with the expected report and export context', () => {
    const html = renderMarkup(
      <ReportRunPrintPreview
        reportRun={
          {
            reportRunNumber: 'run-014',
            title: 'Weekly executive report',
            status: 'completed',
            outputFormat: 'pdf',
            requestedAt: '2026-06-25T08:00:00Z',
            completedAt: '2026-06-25T08:05:00Z',
            rowCount: 42,
            freshnessStatus: 'fresh',
            warningCount: 1,
            errorCount: 0,
            parametersUsed: ['site=all'],
            filtersUsed: ['freshness=fresh'],
            freshnessSummary: 'Current as of the morning refresh.',
            errorMessage: '',
          } as any
        }
        definition={
          {
            reportNumber: 'report-008',
            reportType: 'executive',
            description: 'Executive summary for leadership.',
            datasetRefs: ['dataset-exec'],
            readModelRefs: ['read-model-exec'],
          } as any
        }
        reportParameters={[
          {
            reportParameterId: 'param-1',
          },
        ] as any}
        reportSections={[
          {
            sequence: 1,
            title: 'Summary',
            sectionType: 'table',
          },
          {
            sequence: 2,
            title: 'Trends',
            sectionType: 'chart',
          },
        ] as any}
        exportJobs={[
          {
            exportType: 'report',
            exportFormat: 'pdf',
            status: 'completed',
            generatedAt: '2026-06-25T08:06:00Z',
          },
        ] as any}
        actorDisplayName="Demo Admin"
        tenantDisplayName="Demo Tenant"
      />,
    )

    expect(html).toContain('Weekly executive report')
    expect(html).toContain('report preview')
    expect(html).toContain('Run summary')
    expect(html).toContain('Execution health')
    expect(html).toContain('Approved run notes')
    expect(html).toContain('Parameters and filters')
    expect(html).toContain('Sections and export history')
    expect(html).toContain('report-008')
    expect(html).toContain('Summary')
    expect(html).toContain('Trends')
    expect(html.toLowerCase()).toContain('pdf')
  })

  it('renders audit packets with the expected packet and scope context', () => {
    const html = renderMarkup(
      <AuditPackagePrintPreview
        auditPackage={
          {
            auditReportPackageId: 'packet-007',
            packageNumber: 'PKT-007',
            title: 'Quarterly compliance packet',
            status: 'ready',
            readinessScore: 92,
            generatedAt: '2026-06-25T09:00:00Z',
            lockedAt: '2026-06-25T09:30:00Z',
            sourceProductRefs: ['reportarr', 'recordarr'],
            complianceEvaluationRefs: ['eval-1', 'eval-2'],
            missingEvidenceSummary: 'None',
            invalidEvidenceSummary: 'None',
            description: 'Evidence packet for the quarter.',
            auditScope: {
              scopeType: 'tenant',
              dateRangeStart: '2026-04-01T00:00:00Z',
              dateRangeEnd: '2026-06-30T23:59:59Z',
              includeEvidence: true,
              includeSourceTrace: true,
              productFilters: ['reportarr'],
              rulepackRefs: ['rules-1'],
              siteRefs: ['site-1'],
              departmentRefs: ['dept-1'],
              objectRefs: ['obj-1'],
            },
          } as any
        }
        linkedRuns={[
          {
            reportRunNumber: 'run-014',
            title: 'Weekly executive report',
            status: 'completed',
            rowCount: 42,
          },
        ] as any}
        actorDisplayName="Demo Admin"
        tenantDisplayName="Demo Tenant"
      />,
    )
    const normalized = html.toLowerCase()

    expect(html).toContain('Quarterly compliance packet')
    expect(html).toContain('audit packet preview')
    expect(html).toContain('Packet summary')
    expect(html).toContain('Readiness summary')
    expect(html).toContain('Audit packet contents')
    expect(html).toContain('Related products')
    expect(html).toContain('Included report runs')
    expect(html).toContain('Ownership note')
    expect(html).toContain('PKT-007')
    expect(html).toMatch(/92.*% ready/i)
    expect(normalized).toContain('reportarr')
    expect(html).toContain('Weekly executive report')
  })

  it('renders scheduled reports with recipient coverage and cadence metadata', () => {
    const html = renderMarkup(
      <ReportSchedulePrintPreview
        schedule={
          {
            scheduleNumber: 'sch-010',
            title: 'Weekly executive report',
            status: 'enabled',
            deliveryMethod: 'email',
            cadence: 'weekly',
            timezone: 'America/Chicago',
            nextRunAt: '2026-06-26T08:00:00Z',
            lastRunAt: '2026-06-19T08:00:00Z',
            parameters: ['site=all'],
            startsAt: '2026-01-01T00:00:00Z',
            endsAt: '2026-12-31T23:59:59Z',
            cronExpression: '0 8 * * 5',
          } as any
        }
        definition={
          {
            reportNumber: 'report-008',
          } as any
        }
        recipients={[
          {
            recipientType: 'person',
            deliveryFormat: 'pdf',
            status: 'active',
          },
          {
            recipientType: 'group',
            deliveryFormat: 'email',
            status: 'active',
          },
        ] as any}
        actorDisplayName="Demo Admin"
        tenantDisplayName="Demo Tenant"
      />,
    )
    const normalized = html.toLowerCase()

    expect(html).toContain('scheduled output')
    expect(html).toContain('Schedule summary')
    expect(html).toContain('Recipient handling')
    expect(html).toContain('Approved schedule notes')
    expect(html).toContain('Recipient summary')
    expect(normalized).toContain('weekly')
    expect(html).toContain('America/Chicago')
    expect(html).toContain('person')
    expect(html).toContain('group')
    expect(normalized).toContain('email')
  })
})
