import { Route, Routes } from 'react-router-dom'
import { MarketingLayout } from './components/MarketingLayout'
import { DataOwnershipPage } from './pages/DataOwnershipPage'
import { DemoContactPage } from './pages/DemoContactPage'
import { HomePage } from './pages/HomePage'
import { NotFoundPage } from './pages/NotFoundPage'
import { ComparePage } from './pages/ComparePage'
import { PricingPage } from './pages/PricingPage'
import { PlatformOverviewPage } from './pages/PlatformOverviewPage'
import { IndustriesPage } from './pages/IndustriesPage'
import { UseCasesPage } from './pages/UseCasesPage'
import { CompliancePage } from './pages/CompliancePage'
import { WhyStlCompliancePage } from './pages/WhyStlCompliancePage'
import { AboutFounderPage } from './pages/AboutFounderPage'
import { FaqPage } from './pages/FaqPage'
import { PrivacyPage } from './pages/PrivacyPage'
import { ProductPage } from './pages/ProductPage'
import { ProductsHubPage } from './pages/ProductsHubPage'
import { ResourcesPage } from './pages/ResourcesPage'
import { SecurityPage } from './pages/SecurityPage'
import { TermsPage } from './pages/TermsPage'

export default function App() {
  return (
      <Routes>
        <Route element={<MarketingLayout />}>
          <Route index element={<HomePage />} />
          <Route path="platform-overview" element={<PlatformOverviewPage />} />
          <Route path="products" element={<ProductsHubPage />} />
          <Route path="products/:productKey" element={<ProductPage />} />
          <Route path="industries" element={<IndustriesPage />} />
          <Route path="use-cases" element={<UseCasesPage />} />
          <Route path="compliance" element={<CompliancePage />} />
          <Route path="why-stl-compliance" element={<WhyStlCompliancePage />} />
          <Route path="about-founder" element={<AboutFounderPage />} />
          <Route path="resources" element={<ResourcesPage />} />
          <Route path="compare" element={<ComparePage />} />
          <Route path="pricing" element={<PricingPage />} />
          <Route path="request-access" element={<PricingPage />} />
          <Route path="contact" element={<DemoContactPage />} />
          <Route path="security" element={<SecurityPage />} />
          <Route path="data-ownership" element={<DataOwnershipPage />} />
          <Route path="demo" element={<DemoContactPage />} />
          <Route path="faq" element={<FaqPage />} />
          <Route path="privacy" element={<PrivacyPage />} />
          <Route path="terms" element={<TermsPage />} />
          <Route path="*" element={<NotFoundPage />} />
        </Route>
    </Routes>
  )
}
