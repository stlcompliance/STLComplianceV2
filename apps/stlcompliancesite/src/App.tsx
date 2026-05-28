import { Route, Routes } from 'react-router-dom'
import { MarketingLayout } from './components/MarketingLayout'
import { DataOwnershipPage } from './pages/DataOwnershipPage'
import { DemoContactPage } from './pages/DemoContactPage'
import { HomePage } from './pages/HomePage'
import { NotFoundPage } from './pages/NotFoundPage'
import { PrivacyPage } from './pages/PrivacyPage'
import { ProductPage } from './pages/ProductPage'
import { ProductsHubPage } from './pages/ProductsHubPage'
import { SecurityPage } from './pages/SecurityPage'
import { TermsPage } from './pages/TermsPage'

export default function App() {
  return (
    <Routes>
      <Route element={<MarketingLayout />}>
        <Route index element={<HomePage />} />
        <Route path="products" element={<ProductsHubPage />} />
        <Route path="products/:productKey" element={<ProductPage />} />
        <Route path="security" element={<SecurityPage />} />
        <Route path="data-ownership" element={<DataOwnershipPage />} />
        <Route path="demo" element={<DemoContactPage />} />
        <Route path="privacy" element={<PrivacyPage />} />
        <Route path="terms" element={<TermsPage />} />
        <Route path="*" element={<NotFoundPage />} />
      </Route>
    </Routes>
  )
}
