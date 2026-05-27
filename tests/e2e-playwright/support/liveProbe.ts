const productApis: Record<string, string> = {
  nexarr: process.env.E2E_NEXARR_URL ?? 'http://localhost:5101',
  staffarr: process.env.E2E_STAFFARR_URL ?? 'http://localhost:5102',
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

export async function isLiveStackReachable(): Promise<boolean> {
  const suiteOk = await isHttpOk(suiteBaseUrl(), '/')
  const nexarrOk = await isHttpOk(productApis.nexarr, '/health')
  return suiteOk && nexarrOk
}

export const demoCredentials = {
  email: process.env.E2E_DEMO_EMAIL ?? 'admin@demo.stl',
  password: process.env.E2E_DEMO_PASSWORD ?? 'ChangeMe!Demo2026',
  tenantId:
    process.env.E2E_DEMO_TENANT_ID ?? '11111111-1111-1111-1111-111111111101',
}
