import { Link } from 'react-router-dom'

export function NotFoundPage() {
  return (
    <section className="content-band page-band">
      <div className="page-heading">
        <p className="eyebrow">Not found</p>
        <h1>That KB page is not available.</h1>
        <p>Try the knowledge base home page or browse by section.</p>
        <Link className="button-link" to="/">
          Back to KB home
        </Link>
      </div>
    </section>
  )
}
