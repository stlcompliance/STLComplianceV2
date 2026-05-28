import { handoffProductFrontends } from './productFrontends.js'

const productApis: Record<string, string> = {
  nexarr: process.env.E2E_NEXARR_URL ?? 'http://localhost:5101',
  staffarr: process.env.E2E_STAFFARR_URL ?? 'http://localhost:5102',
  trainarr: process.env.E2E_TRAINARR_URL ?? 'http://localhost:5103',
  maintainarr: process.env.E2E_MAINTAINARR_URL ?? 'http://localhost:5104',
  routarr: process.env.E2E_ROUTARR_URL ?? 'http://localhost:5105',
  supplyarr: process.env.E2E_SUPPLYARR_URL ?? 'http://localhost:5106',
  compliancecore: process.env.E2E_COMPLIANCECORE_URL ?? 'http://localhost:5107',
}

export function isLiveModeEnabled(): boolean {
  return (
    process.env.E2E_LIVE === '1' ||
    process.env.E2E_LIVE?.toLowerCase() === 'true'
  )
}

export function suiteBaseUrl(): string {
  return process.env.E2E_SUITE_URL ?? 'http://localhost:5174'
}

export async function isHttpOk(url: string, path = '/health'): Promise<boolean> {
  try {
    const response = await fetch(new URL(path, url), {
      signal: AbortSignal.timeout(4000),
    })
    return response.ok
  } catch {
    return false
  }
}

export async function isSuiteFrontendReachable(): Promise<boolean> {
  return isHttpOk(suiteBaseUrl(), '/')
}

export async function isLiveStackReachable(): Promise<boolean> {
  const suiteOk = await isSuiteFrontendReachable()
  const nexarrOk = await isHttpOk(productApis.nexarr, '/health')
  return suiteOk && nexarrOk
}

export async function isHandoffFrontendReachable(productKey: string): Promise<boolean> {
  const frontend = handoffProductFrontends.find((p) => p.productKey === productKey)
  if (!frontend) {
    return false
  }

  return isHttpOk(frontend.baseUrl, '/')
}

export async function areAllHandoffFrontendsReachable(): Promise<boolean> {
  const checks = await Promise.all(
    handoffProductFrontends.map((p) => isHandoffFrontendReachable(p.productKey)),
  )
  return checks.every(Boolean)
}

export const demoCredentials = {
  email: process.env.E2E_DEMO_EMAIL ?? 'admin@demo.stl',
  password: process.env.E2E_DEMO_PASSWORD ?? 'ChangeMe!Demo2026',
  tenantId:
    process.env.E2E_DEMO_TENANT_ID ?? '11111111-1111-1111-1111-111111111101',
}

/** Seeded demo tenant values used by Playwright tenant-chrome assertions. */
export const demoTenant = {
  displayName: process.env.E2E_DEMO_TENANT_DISPLAY_NAME ?? 'STL Demo Tenant',
  slug: process.env.E2E_DEMO_TENANT_SLUG ?? 'demo-stl',
  userDisplayName: process.env.E2E_DEMO_USER_DISPLAY_NAME ?? 'Demo Platform Admin',
}

export async function signInFromSuite(page: import('@playwright/test').Page): Promise<void> {
  await page.goto('/login')
  await page.getByLabel('Email').fill(demoCredentials.email)
  await page.getByLabel('Password').fill(demoCredentials.password)
  await page.getByRole('button', { name: 'Sign in' }).click()
  await page.getByRole('heading', { name: /Welcome,/ }).waitFor({ timeout: 15_000 })
}
