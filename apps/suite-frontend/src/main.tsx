import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import './index.css'
import { initializeSuiteTheme } from '@stl/shared-ui'
import { loadAuthSession } from './auth/authStorage'
import App from './App.tsx'

const session = loadAuthSession()
initializeSuiteTheme({
  userId: session?.userId,
  tenantId: session?.tenantId,
})

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <App />
  </StrictMode>,
)
