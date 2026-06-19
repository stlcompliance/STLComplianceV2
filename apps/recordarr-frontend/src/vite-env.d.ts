/// <reference types="vite/client" />

interface ImportMetaEnv {
  readonly VITE_RECORDARR_API_BASE?: string
  readonly VITE_STAFFARR_API_BASE?: string
  readonly VITE_SUPPLYARR_API_BASE?: string
  readonly VITE_CUSTOMARR_API_BASE?: string
  readonly VITE_MAINTAINARR_API_BASE?: string
  readonly VITE_COMPLIANCECORE_API_BASE?: string
  readonly VITE_SUITE_URL?: string
}

interface ImportMeta {
  readonly env: ImportMetaEnv
}
