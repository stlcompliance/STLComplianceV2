import path from 'node:path'
import { fileURLToPath } from 'node:url'
import { defineConfig } from 'vitest/config'
import react from '@vitejs/plugin-react'
import tailwindcss from '@tailwindcss/vite'

const appRoot = path.dirname(fileURLToPath(import.meta.url))
const apiTarget = process.env.VITE_ASSURARR_PROXY_TARGET ?? 'http://localhost:5109'
const routerBase = process.env.VITE_ROUTER_BASENAME?.trim().replace(/\/+$/, '')
const assetBase = routerBase ? `${routerBase}/` : '/'

export default defineConfig({
  base: assetBase,
  plugins: [react(), tailwindcss()],
  resolve: {
    dedupe: ['react', 'react-dom', 'react-router-dom', '@tanstack/react-query', 'lucide-react'],
    alias: {
      '@stl/shared-ui': path.resolve(appRoot, '../../packages/shared-ui/src'),
    },
  },
  server: {
    port: 5183,
    fs: {
      allow: [path.resolve(appRoot, '../..')],
    },
    proxy: {
      '/api': {
        target: 'http://localhost:5109',
        changeOrigin: true,
      },
    },
  },
  preview: {
    port: 5183,
    host: true,
    proxy: {
      '/api': {
        target: apiTarget,
        changeOrigin: true,
      },
    },
  },
  test: {
    environment: 'node',
    include: ['src/**/*.{test,spec}.{ts,tsx}'],
  },
})
