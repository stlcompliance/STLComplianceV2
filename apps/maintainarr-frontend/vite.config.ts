import { defineConfig } from 'vitest/config'
import react from '@vitejs/plugin-react'
import tailwindcss from '@tailwindcss/vite'

const maintainarrApiTarget = process.env.VITE_MAINTAINARR_PROXY_TARGET ?? 'http://localhost:5104'

export default defineConfig({
  plugins: [react(), tailwindcss()],
  server: {
    port: 5178,
    proxy: {
      '/api': {
        target: maintainarrApiTarget,
        changeOrigin: true,
      },
    },
  },
  preview: {
    port: 5178,
    host: true,
    proxy: {
      '/api': {
        target: maintainarrApiTarget,
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
