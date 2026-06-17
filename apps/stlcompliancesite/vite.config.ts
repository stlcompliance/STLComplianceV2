import path from 'node:path'
import { fileURLToPath } from 'node:url'

import { writeFileSync } from 'node:fs'

import { defineConfig, type Plugin } from 'vitest/config'

import react from '@vitejs/plugin-react'

import tailwindcss from '@tailwindcss/vite'
import {
  getProductRouteSlug,
  IMPLEMENTED_PRODUCT_KEYS,
} from '../../packages/shared-ui/src/productOwnershipManifest'

const appRoot = path.dirname(fileURLToPath(import.meta.url))
const nonMarketingProductKeys = new Set(['nexarr'])
const marketingProductKeys = IMPLEMENTED_PRODUCT_KEYS.filter(
  (productKey) => !nonMarketingProductKeys.has(productKey),
)

function buildStaticPublicPaths(): string[] {
  return [
    '/',
    '/platform-overview',
    '/products',
    '/industries',
    '/use-cases',
    '/compliance',
    '/why-stl-compliance',
    '/about-founder',
    '/pricing',
    '/request-access',
    '/contact',
    '/faq',
    '/resources',
    '/compare',
    '/security',
    '/data-ownership',
    '/demo',
    '/privacy',
    '/terms',
    ...marketingProductKeys.map((productKey) => `/products/${getProductRouteSlug(productKey)}`),
  ]
}


function marketingSiteArtifactsPlugin(): Plugin {

  return {

    name: 'marketing-site-artifacts',

    closeBundle() {

      const outDir = path.resolve(__dirname, 'dist')

      const base =

        process.env.VITE_SITE_BASE_URL?.replace(/\/+$/, '') ??

        'https://stlcompliance.com'

      const paths = buildStaticPublicPaths()

      const lastmod = new Date().toISOString().slice(0, 10)



      const urlEntries = paths

        .map((route) => {

          const loc = route === '/' ? base : `${base}${route}`

          return `  <url>\n    <loc>${loc}</loc>\n    <lastmod>${lastmod}</lastmod>\n    <changefreq>weekly</changefreq>\n    <priority>${route === '/' ? '1.0' : route.startsWith('/products/') ? '0.8' : '0.7'}</priority>\n  </url>`

        })

        .join('\n')



      writeFileSync(

        path.join(outDir, 'sitemap.xml'),

        `<?xml version="1.0" encoding="UTF-8"?>\n<urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9">\n${urlEntries}\n</urlset>\n`,

        'utf8',

      )



      writeFileSync(

        path.join(outDir, 'robots.txt'),

        `User-agent: *\nAllow: /\n\nSitemap: ${base}/sitemap.xml\n`,

        'utf8',

      )

    },

  }

}



export default defineConfig({

  plugins: [react(), tailwindcss(), marketingSiteArtifactsPlugin()],
  resolve: {
    dedupe: ['react', 'react-dom', 'react-router-dom'],
    alias: {
      '@stl/shared-ui': path.resolve(appRoot, '../../packages/shared-ui/src'),
    },
  },

  server: {

    port: 5173,
    fs: {
      allow: [path.resolve(appRoot, '../..')],
    },

  },

  preview: {

    port: 5173,

    host: true,

  },

  test: {

    environment: 'jsdom',

    setupFiles: ['./src/test/setup.ts'],

    include: ['src/**/*.{test,spec}.{ts,tsx}'],

  },

})


