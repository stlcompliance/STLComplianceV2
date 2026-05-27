import { defineConfig } from 'vitest/config'
import react from '@vitejs/plugin-react'
import tailwindcss from '@tailwindcss/vite'

const nexarrApiTarget = process.env.VITE_NEXARR_PROXY_TARGET ?? 'http://localhost:5101'

export default defineConfig({
  plugins: [react(), tailwindcss()],
  server: {
    port: 5181,
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
