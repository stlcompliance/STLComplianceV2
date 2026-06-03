import path from 'node:path'
import { fileURLToPath } from 'node:url'
import { defineConfig } from 'vitest/config'
import react from '@vitejs/plugin-react'
import tailwindcss from '@tailwindcss/vite'

const appRoot = path.dirname(fileURLToPath(import.meta.url))

export default defineConfig({
  base: './',
  plugins: [react(), tailwindcss()],
  resolve: {
    dedupe: ['react', 'react-dom', 'react-router-dom'],
    alias: {
      '@stl/shared-ui': path.resolve(appRoot, '../../packages/shared-ui/src'),
    },
  },
  server: {
    port: 5182,
    fs: {
      allow: [path.resolve(appRoot, '../..')],
    },
    proxy: {
      '/api': {
        target: 'http://localhost:5108',
        changeOrigin: true,
      },
    },
  },
  test: {
    environment: 'node',
    include: ['src/**/*.{test,spec}.{ts,tsx}'],
  },
})
