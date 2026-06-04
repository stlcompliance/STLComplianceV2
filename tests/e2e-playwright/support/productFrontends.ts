export type HandoffProductFrontend = {
  productKey: string
  port: number
  baseUrl: string
}

export const companionFrontend: HandoffProductFrontend = {
  productKey: 'fieldcompanion',
  port: 5181,
  baseUrl: process.env.E2E_COMPANION_URL ?? 'http://localhost:5181',
}

export const handoffProductFrontends: readonly HandoffProductFrontend[] = [
  { productKey: 'staffarr', port: 5175, baseUrl: process.env.E2E_STAFFARR_URL ?? 'http://localhost:5175' },
  { productKey: 'trainarr', port: 5176, baseUrl: process.env.E2E_TRAINARR_URL ?? 'http://localhost:5176' },
  {
    productKey: 'compliancecore',
    port: 5177,
    baseUrl: process.env.E2E_COMPLIANCECORE_URL ?? 'http://localhost:5177',
  },
  {
    productKey: 'maintainarr',
    port: 5178,
    baseUrl: process.env.E2E_MAINTAINARR_URL ?? 'http://localhost:5178',
  },
  { productKey: 'supplyarr', port: 5179, baseUrl: process.env.E2E_SUPPLYARR_URL ?? 'http://localhost:5179' },
  { productKey: 'routarr', port: 5180, baseUrl: process.env.E2E_ROUTARR_URL ?? 'http://localhost:5180' },
  { productKey: 'loadarr', port: 5182, baseUrl: process.env.E2E_LOADARR_URL ?? 'http://localhost:5182' },
  { productKey: 'recordarr', port: 5184, baseUrl: process.env.E2E_RECORDARR_URL ?? 'http://localhost:5184' },
]

export function handoffUrlPattern(frontend: HandoffProductFrontend): RegExp {
  const hostPattern = frontend.baseUrl.replace(/[.*+?^${}()|[\]\\]/g, '\\$&')
  return new RegExp(`${hostPattern}|localhost:${frontend.port}`, 'i')
}
