/// <reference types="vite/client" />

interface ImportMetaEnv {
  readonly VITE_SUPPLYARR_API_BASE?: string
  readonly VITE_ROUTARR_API_BASE?: string
}

interface ImportMeta {
  readonly env: ImportMetaEnv
}
