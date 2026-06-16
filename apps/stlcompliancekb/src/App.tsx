import { Route, Routes } from 'react-router-dom'
import { KbLayout } from './components/KbLayout'
import { ArticlePage } from './pages/ArticlePage'
import { HomePage } from './pages/HomePage'
import { NotFoundPage } from './pages/NotFoundPage'
import { SectionPage } from './pages/SectionPage'

export default function App() {
  return (
    <Routes>
      <Route element={<KbLayout />}>
        <Route index element={<HomePage />} />
        <Route path="sections/:sectionId" element={<SectionPage />} />
        <Route path="articles/:slug" element={<ArticlePage />} />
        <Route path="*" element={<NotFoundPage />} />
      </Route>
    </Routes>
  )
}
