/// <reference types="vite/client" />

interface ImportMetaEnv {
  readonly VITE_SUPPLYARR_API_BASE?: string
}

interface ImportMeta {
  readonly env: ImportMetaEnv
}
