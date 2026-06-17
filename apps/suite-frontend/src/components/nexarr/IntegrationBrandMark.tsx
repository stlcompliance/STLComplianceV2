import type { TenantIntegrationBrandResponse } from '../../api/types'

type IntegrationBrandMarkProps = {
  brand: TenantIntegrationBrandResponse
  label: string
  size?: 'sm' | 'md' | 'lg'
}

const sizeClasses = {
  sm: 'h-8 w-8 text-[10px]',
  md: 'h-10 w-10 text-xs',
  lg: 'h-12 w-12 text-sm',
}

export function IntegrationBrandMark({
  brand,
  label,
  size = 'md',
}: IntegrationBrandMarkProps) {
  return (
    <span
      aria-label={`${label} brand mark`}
      className={`${sizeClasses[size]} inline-flex shrink-0 items-center justify-center rounded-md border font-semibold uppercase tracking-normal shadow-sm`}
      style={{
        backgroundColor: brand.backgroundColor,
        borderColor: `${brand.accentColor}99`,
        boxShadow: `inset 0 0 0 1px ${brand.accentColor}33`,
        color: brand.accentColor,
      }}
      title={brand.assetSourceLabel}
    >
      <span className="max-w-full truncate px-1">{brand.mark}</span>
    </span>
  )
}
