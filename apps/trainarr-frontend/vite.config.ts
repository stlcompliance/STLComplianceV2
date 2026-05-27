import { defineConfig } from 'vitest/config'
import react from '@vitejs/plugin-react'
import tailwindcss from '@tailwindcss/vite'

const trainarrApiTarget = process.env.VITE_TRAINARR_PROXY_TARGET ?? 'http://localhost:5103'

export default defineConfig({
  plugins: [react(), tailwindcss()],
  server: {
    port: 5176,
    proxy: {
      '/api': {
        target: trainarrApiTarget,
        changeOrigin: true,
      },
    },
  },
  preview: {
    port: 5176,
    host: true,
    proxy: {
      '/api': {
        target: trainarrApiTarget,
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
