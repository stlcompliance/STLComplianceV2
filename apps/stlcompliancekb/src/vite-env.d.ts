/// <reference types="vite/client" />

declare module 'virtual:kb-docs' {
  export const rawKbArticles: Array<{
    relativePath: string
    markdown: string
  }>
}
