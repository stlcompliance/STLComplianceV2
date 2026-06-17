import path from 'node:path'
import { fileURLToPath } from 'node:url'
import { defineConfig } from 'vitest/config'
import react from '@vitejs/plugin-react'
import tailwindcss from '@tailwindcss/vite'

const appRoot = path.dirname(fileURLToPath(import.meta.url))
const supplyarrApiTarget = process.env.VITE_SUPPLYARR_PROXY_TARGET ?? 'http://localhost:5106'
const routerBase = process.env.VITE_ROUTER_BASENAME?.trim().replace(/\/+$/, '')
const assetBase = routerBase ? `${routerBase}/` : '/'

export default defineConfig({
  base: assetBase,
  plugins: [react(), tailwindcss()],
  resolve: {
    dedupe: ['react', 'react-dom', 'react-router-dom', '@tanstack/react-query'],
    alias: {
      '@stl/shared-ui': path.resolve(appRoot, '../../packages/shared-ui/src'),
      react: path.resolve(appRoot, 'node_modules/react'),
      'react-dom': path.resolve(appRoot, 'node_modules/react-dom'),
      'lucide-react': path.resolve(appRoot, 'node_modules/lucide-react'),
    },
  },
  server: {
    port: 5179,
    fs: {
      allow: [path.resolve(appRoot, '../..')],
    },
    proxy: {
      '/api': {
        target: supplyarrApiTarget,
        changeOrigin: true,
      },
    },
  },
  preview: {
    port: 5179,
    host: true,
    proxy: {
      '/api': {
        target: supplyarrApiTarget,
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
