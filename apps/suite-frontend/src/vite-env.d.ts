/// <reference types="vite/client" />

interface ImportMetaEnv {
  readonly VITE_NEXARR_API_URL: string
  readonly VITE_DEMO_TENANT_ID: string
}

interface ImportMeta {
  readonly env: ImportMetaEnv
}
