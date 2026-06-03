import path from 'node:path'
import { fileURLToPath } from 'node:url'
import { defineConfig } from 'vitest/config'
import react from '@vitejs/plugin-react'
import tailwindcss from '@tailwindcss/vite'

const appRoot = path.dirname(fileURLToPath(import.meta.url))
const staffarrApiTarget = process.env.VITE_STAFFARR_PROXY_TARGET ?? 'http://localhost:5102'

export default defineConfig({
  base: './',
  plugins: [react(), tailwindcss()],
  resolve: {
    // shared-ui is aliased to source; dedupe prevents duplicate React / TanStack Query (blank UI / hook context crash).
    dedupe: ['react', 'react-dom', 'react-router-dom', '@tanstack/react-query'],
    alias: {
      '@stl/shared-ui': path.resolve(appRoot, '../../packages/shared-ui/src'),
    },
  },
  server: {
    port: 5175,
    fs: {
      allow: [path.resolve(appRoot, '../..')],
    },
    proxy: {
      '/api': {
        target: staffarrApiTarget,
        changeOrigin: true,
      },
    },
  },
  preview: {
    port: 5175,
    host: true,
    proxy: {
      '/api': {
        target: staffarrApiTarget,
        changeOrigin: true,
      },
    },
  },
  test: {
    environment: 'jsdom',
    include: ['src/**/*.{test,spec}.{ts,tsx}'],
  },
})
