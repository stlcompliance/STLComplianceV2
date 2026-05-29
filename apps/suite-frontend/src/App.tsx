import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { AppRoutes } from './app/routes'
import { AuthProvider } from './auth/AuthProvider'
import { ToastProvider } from './feedback'

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 30_000,
      refetchOnWindowFocus: false,
    },
  },
})

export default function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <AuthProvider>
        <ToastProvider>
          <AppRoutes />
        </ToastProvider>
      </AuthProvider>
    </QueryClientProvider>
  )
}
