import path from 'node:path'
import { fileURLToPath } from 'node:url'
import { defineConfig } from 'vitest/config'
import react from '@vitejs/plugin-react'
import tailwindcss from '@tailwindcss/vite'

const appRoot = path.dirname(fileURLToPath(import.meta.url))
const ordarrApiTarget = process.env.VITE_ORDARR_PROXY_TARGET ?? 'http://localhost:5112'

export default defineConfig({
  base: './',
  plugins: [react(), tailwindcss()],
  resolve: {
    dedupe: ['react', 'react-dom', 'react-router-dom', '@tanstack/react-query'],
    alias: {
      '@stl/shared-ui': path.resolve(appRoot, '../../packages/shared-ui/src'),
    },
  },
  server: {
    port: 5187,
    fs: {
      allow: [path.resolve(appRoot, '../..')],
    },
    proxy: {
      '/api': {
        target: ordarrApiTarget,
        changeOrigin: true,
      },
    },
  },
  preview: {
    port: 5187,
    host: true,
    proxy: {
      '/api': {
        target: ordarrApiTarget,
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
