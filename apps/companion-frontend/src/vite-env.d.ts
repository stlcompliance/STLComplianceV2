/// <reference types="vite/client" />

interface ImportMetaEnv {
  readonly VITE_NEXARR_API_BASE?: string
  readonly VITE_MAINTAINARR_FRONTEND_BASE?: string
  readonly VITE_ROUTARR_FRONTEND_BASE?: string
  readonly VITE_TRAINARR_FRONTEND_BASE?: string
  readonly VITE_STAFFARR_FRONTEND_BASE?: string
  readonly VITE_SUPPLYARR_FRONTEND_BASE?: string
  readonly VITE_LOADARR_FRONTEND_BASE?: string
}

interface ImportMeta {
  readonly env: ImportMetaEnv
}

declare module '*.css' {
  const content: string
  export default content
}
