import { useEffect } from 'react'

type SiteSeoProps = {
  title: string
  description: string
}

export function SiteSeo({ title, description }: SiteSeoProps) {
  useEffect(() => {
    document.title = title
    const meta = document.querySelector('meta[name="description"]')
    if (meta) {
      meta.setAttribute('content', description)
    }
  }, [title, description])

  return null
}
