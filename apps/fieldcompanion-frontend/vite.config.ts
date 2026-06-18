import path from 'node:path'
import { fileURLToPath } from 'node:url'
import { defineConfig } from 'vitest/config'
import react from '@vitejs/plugin-react'
import tailwindcss from '@tailwindcss/vite'

const appRoot = path.dirname(fileURLToPath(import.meta.url))
const nexarrApiTarget = process.env.VITE_NEXARR_PROXY_TARGET ?? 'http://localhost:5101'

export default defineConfig({
  plugins: [react(), tailwindcss()],
  resolve: {
    dedupe: ['react', 'react-dom', 'react-router-dom', '@tanstack/react-query', 'lucide-react'],
    alias: {
      '@stl/shared-ui': path.resolve(appRoot, '../../packages/shared-ui/src'),
    },
  },
  server: {
    port: 5181,
    fs: {
      allow: [path.resolve(appRoot, '../..')],
    },
    proxy: {
      '/api': {
        target: nexarrApiTarget,
        changeOrigin: true,
      },
    },
  },
  test: {
    environment: 'jsdom',
    setupFiles: ['./src/test/setup.ts'],
    include: ['src/**/*.{test,spec}.{ts,tsx}'],
  },
})
