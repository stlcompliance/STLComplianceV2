/// <reference types="vite/client" />

interface ImportMetaEnv {
  readonly VITE_ASSURARR_API_BASE?: string
  readonly VITE_COMPLIANCECORE_API_BASE?: string
  readonly VITE_LOADARR_API_BASE?: string
  readonly VITE_ORDARR_API_BASE?: string
  readonly VITE_ROUTARR_API_BASE?: string
  readonly VITE_CUSTOMARR_API_BASE?: string
  readonly VITE_SUPPLYARR_API_BASE?: string
}

interface ImportMeta {
  readonly env: ImportMetaEnv
}
