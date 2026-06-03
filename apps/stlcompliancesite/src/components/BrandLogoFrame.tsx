type BrandLogoFrameProps = {
  src: string
  alt?: string
  size?: 'sm' | 'md' | 'lg'
  className?: string
}

const sizeClasses = {
  sm: {
    frame: 'h-8 w-8 rounded-md p-1',
    image: 'max-h-6 max-w-6',
  },
  md: {
    frame: 'h-12 w-12 rounded-xl p-2',
    image: 'max-h-8 max-w-8',
  },
  lg: {
    frame: 'min-h-24 w-fit max-w-sm rounded-xl px-5 py-4',
    image: 'max-h-16 max-w-[17rem]',
  },
} as const

export function BrandLogoFrame({ src, alt = '', size = 'md', className = '' }: BrandLogoFrameProps) {
  const classes = sizeClasses[size]

  return (
    <span
      className={`inline-flex shrink-0 items-center justify-center border border-slate-300 bg-white shadow-sm ring-1 ring-white/50 ${classes.frame} ${className}`}
    >
      <img src={src} alt={alt} className={`${classes.image} object-contain`} />
    </span>
  )
}
