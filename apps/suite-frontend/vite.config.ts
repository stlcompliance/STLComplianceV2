import { defineConfig } from 'vitest/config'
import react from '@vitejs/plugin-react'
import tailwindcss from '@tailwindcss/vite'

const nexarrTarget = process.env.VITE_NEXARR_PROXY_TARGET ?? 'http://localhost:5101'

export default defineConfig({
  plugins: [react(), tailwindcss()],
  server: {
    port: 5174,
    proxy: {
      '/api': {
        target: nexarrTarget,
        changeOrigin: true,
      },
    },
  },
  preview: {
    port: 5174,
    host: true,
    proxy: {
      '/api': {
        target: nexarrTarget,
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
