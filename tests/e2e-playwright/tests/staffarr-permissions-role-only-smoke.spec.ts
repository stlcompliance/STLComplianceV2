import { expect, test } from '@playwright/test'

import { launchProductHandoffFromSuite } from '../support/handoffJourney.js'
import {
  isHandoffFrontendReachable,
  isLiveModeEnabled,
  isLiveStackReachable,
  signInFromSuite,
} from '../support/liveProbe.js'

const staffarrApiBase = process.env.E2E_STAFFARR_API_URL ?? 'http://localhost:5102'

async function staffarrJson<T>(
  accessToken: string,
  path: string,
  init: Parameters<typeof fetch>[1],
): Promise<T> {
  const response = await fetch(`${staffarrApiBase}${path}`, {
    ...init,
    headers: {
      Authorization: `Bearer ${accessToken}`,
      'Content-Type': 'application/json',
      ...(init.headers ?? {}),
    },
  })

  if (!response.ok) {
    throw new Error(`HTTP ${response.status}: ${await response.text()}`)
  }

  return (await response.json()) as T
}

test.describe('StaffArr permissions role-only smoke @requires-live', () => {
  test.beforeEach(async ({}, testInfo) => {
    if (!isLiveModeEnabled()) {
      testInfo.skip(true, 'Set E2E_LIVE=1 to run browser E2E against docker-compose.')
    }
    if (!(await isLiveStackReachable())) {
      testInfo.skip(
        true,
        'Suite frontend (5174) and NexArr API (5101) must be running. Use scripts/ops/e2e-stack-up.ps1 and e2e-frontends-preview.ps1.',
      )
    }
    if (!(await isHandoffFrontendReachable('staffarr'))) {
      testInfo.skip(true, 'StaffArr frontend (5175) is unreachable.')
    }
  })

  test('permission checks report grants via role assignments only', async ({ page }) => {
    await signInFromSuite(page)
    await launchProductHandoffFromSuite(page, 'staffarr')

    const sessionJson = await page.evaluate(() => sessionStorage.getItem('stl.staffarr.session'))
    expect(sessionJson).not.toBeNull()
    const session = JSON.parse(sessionJson!)
    const accessToken = session.accessToken as string
    const personId = session.personId as string

    const uniqueSuffix = `${Date.now()}`
    const permissionKey = `staffarr.e2e.role.only.${uniqueSuffix}`
    const permissionName = `E2E Role-Only Permission ${uniqueSuffix}`
    const roleKey = `staffarr.e2e.role.only.access.${uniqueSuffix}`
    const roleName = `E2E Role Only Access ${uniqueSuffix}`

    const permissionTemplate = await staffarrJson<{
      permissionTemplateId: string
      permissionKey: string
      name: string
    }>(accessToken, '/api/permissions', {
      method: 'POST',
      body: JSON.stringify({
        permissionKey,
        name: permissionName,
        description: 'E2E role-only permission grant.',
      }),
    })

    const roleTemplate = await staffarrJson<{
      roleTemplateId: string
      roleKey: string
      name: string
    }>(accessToken, '/api/roles', {
      method: 'POST',
      body: JSON.stringify({
        roleKey,
        name: roleName,
        description: 'E2E role-only assignment.',
        permissions: [
          {
            permissionTemplateId: permissionTemplate.permissionTemplateId,
            scopeType: 'tenant',
            scopeValue: null,
          },
        ],
      }),
    })

    const assignment = await staffarrJson<{
      assignmentId: string
      personId: string
      roleTemplateId: string
      roleKey: string
      roleName: string
      status: string
    }>(accessToken, `/api/people/${personId}/role-assignments`, {
      method: 'POST',
      body: JSON.stringify({
        roleTemplateId: roleTemplate.roleTemplateId,
        scopeType: 'tenant',
        scopeValue: null,
        expiresAt: null,
      }),
    })

    expect(assignment.roleKey).toBe(roleKey)

    await page.goto(new URL(`/people/details?person=${encodeURIComponent(personId)}&tab=permissions`, page.url()).toString())

    const rolePanel = page.getByRole('heading', { name: 'Role templates and role-based permissions' })
    await expect(rolePanel).toBeVisible({ timeout: 15_000 })
    await expect(page.getByText(roleName)).toBeVisible({ timeout: 15_000 })

    const permissionCheckInput = page.getByLabel('Permission keys')
    await permissionCheckInput.fill(permissionKey)
    await page.getByRole('button', { name: 'Run permission check' }).click()

    const result = page.getByTestId('permission-check-result')
    await expect(result).toBeVisible({ timeout: 15_000 })
    await expect(result).toContainText(`via ${roleName}`)
    await expect(result).toContainText('Tenant-wide')
  })
})
