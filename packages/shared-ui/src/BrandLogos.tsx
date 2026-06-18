import type { StlThemeMode } from './theme'

type LogoTone = 'light' | 'dark'

type LogoSource = {
  light: string
  dark: string
}

type LogoProps = {
  className?: string
  theme?: StlThemeMode
}

type ProductBrandLogoProps = LogoProps & {
  productName: string
  productKey: string
}

const brandLogoSources: Record<string, LogoSource> = {
  stl: {
    light: new URL('./assets/brand/stl-light.png', import.meta.url).href,
    dark: new URL('./assets/brand/stl-dark.png', import.meta.url).href,
  },
  assurarr: {
    light: new URL('./assets/brand/assurarr-light.png', import.meta.url).href,
    dark: new URL('./assets/brand/assurarr-dark.png', import.meta.url).href,
  },
  compliancecore: {
    light: new URL('./assets/brand/compliancecore-light.png', import.meta.url).href,
    dark: new URL('./assets/brand/compliancecore-dark.png', import.meta.url).href,
  },
  customarr: {
    light: new URL('./assets/brand/customarr-light.png', import.meta.url).href,
    dark: new URL('./assets/brand/customarr-dark.png', import.meta.url).href,
  },
  loadarr: {
    light: new URL('./assets/brand/loadarr-light.png', import.meta.url).href,
    dark: new URL('./assets/brand/loadarr-dark.png', import.meta.url).href,
  },
  maintainarr: {
    light: new URL('./assets/brand/maintainarr-light.png', import.meta.url).href,
    dark: new URL('./assets/brand/maintainarr-dark.png', import.meta.url).href,
  },
  ordarr: {
    light: new URL('./assets/brand/ordarr-light.png', import.meta.url).href,
    dark: new URL('./assets/brand/ordarr-dark.png', import.meta.url).href,
  },
  recordarr: {
    light: new URL('./assets/brand/recordarr-light.png', import.meta.url).href,
    dark: new URL('./assets/brand/recordarr-dark.png', import.meta.url).href,
  },
  reportarr: {
    light: new URL('./assets/brand/reportarr-light.png', import.meta.url).href,
    dark: new URL('./assets/brand/reportarr-dark.png', import.meta.url).href,
  },
  routarr: {
    light: new URL('./assets/brand/routarr-light.png', import.meta.url).href,
    dark: new URL('./assets/brand/routarr-dark.png', import.meta.url).href,
  },
  staffarr: {
    light: new URL('./assets/brand/staffarr-light.png', import.meta.url).href,
    dark: new URL('./assets/brand/staffarr-dark.png', import.meta.url).href,
  },
  supplyarr: {
    light: new URL('./assets/brand/supplyarr-light.png', import.meta.url).href,
    dark: new URL('./assets/brand/supplyarr-dark.png', import.meta.url).href,
  },
  trainarr: {
    light: new URL('./assets/brand/trainarr-light.png', import.meta.url).href,
    dark: new URL('./assets/brand/trainarr-dark.png', import.meta.url).href,
  },
}

function normalizeBrandKey(productKey: string): string {
  return productKey.trim().toLowerCase().replace(/[^a-z0-9]/g, '')
}

function toneForTheme(theme: StlThemeMode | undefined): LogoTone {
  return theme === 'light' ? 'dark' : 'light'
}

function resolveLogoSource(productKey: string): LogoSource {
  return brandLogoSources[normalizeBrandKey(productKey)] ?? brandLogoSources.stl
}

export function StlComplianceLogo({ className, theme }: LogoProps) {
  const tone = toneForTheme(theme)

  return (
    <img
      src={brandLogoSources.stl[tone]}
      alt="STL Compliance logo"
      className={className}
      draggable={false}
    />
  )
}

export function ProductBrandLogo({
  productName,
  productKey,
  className,
  theme,
}: ProductBrandLogoProps) {
  const source = resolveLogoSource(productKey)
  const tone = toneForTheme(theme)

  return (
    <img
      src={source[tone]}
      alt={`${productName} logo`}
      className={className}
      draggable={false}
    />
  )
}
