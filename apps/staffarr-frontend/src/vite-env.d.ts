/// <reference types="vite/client" />

interface ImportMetaEnv {
  readonly VITE_STAFFARR_API_BASE?: string
  readonly VITE_MAINTAINARR_FRONTEND_BASE?: string
  readonly VITE_ROUTARR_FRONTEND_BASE?: string
  readonly VITE_SUPPLYARR_FRONTEND_BASE?: string
  readonly VITE_RECORDARR_FRONTEND_BASE?: string
}

interface ImportMeta {
  readonly env: ImportMetaEnv
}
