import { defineConfig } from 'vitest/config'
import react from '@vitejs/plugin-react'
import tailwindcss from '@tailwindcss/vite'

const routarrApiTarget = process.env.VITE_ROUTARR_PROXY_TARGET ?? 'http://localhost:5105'

export default defineConfig({
  plugins: [react(), tailwindcss()],
  server: {
    port: 5180,
    proxy: {
      '/api': {
        target: routarrApiTarget,
        changeOrigin: true,
      },
    },
  },
  preview: {
    port: 5180,
    host: true,
    proxy: {
      '/api': {
        target: routarrApiTarget,
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
