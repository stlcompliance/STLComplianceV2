/// <reference types="vite/client" />

interface ImportMetaEnv {
  readonly VITE_STAFFARR_API_BASE?: string
}

interface ImportMeta {
  readonly env: ImportMetaEnv
}
