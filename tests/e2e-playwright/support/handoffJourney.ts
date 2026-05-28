import type { Page } from '@playwright/test'

import { suiteBaseUrl } from './liveProbe.js'
import {
  handoffProductFrontends,
  handoffUrlPattern,
  type HandoffProductFrontend,
} from './productFrontends.js'

export function handoffProduct(productKey: string): HandoffProductFrontend {
  const product = handoffProductFrontends.find((entry) => entry.productKey === productKey)
  if (!product) {
    throw new Error(`Unknown handoff product key: ${productKey}`)
  }
  return product
}

/** Suite launch surface → NexArr handoff → product frontend (same session). */
export async function launchProductHandoffFromSuite(
  page: Page,
  productKey: string,
): Promise<void> {
  const product = handoffProduct(productKey)
  await page.goto(`${suiteBaseUrl()}/app/${product.productKey}/launch`)
  const launchButton = page.getByRole('button', { name: 'Launch product (handoff)' })
  await launchButton.waitFor({ state: 'visible', timeout: 15_000 })
  await Promise.all([
    page.waitForURL(handoffUrlPattern(product), { timeout: 30_000 }),
    launchButton.click(),
  ])
  await page.waitForFunction(() => !window.location.href.includes('handoff='), undefined, {
    timeout: 25_000,
  })
}

export async function returnToSuiteApp(page: Page): Promise<void> {
  await page.goto(`${suiteBaseUrl()}/app`)
  await page.getByRole('heading', { name: /Welcome,/ }).waitFor({ timeout: 15_000 })
}
