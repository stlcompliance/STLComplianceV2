import { Boxes, ClipboardList, MapPin, Truck, Warehouse } from 'lucide-react'

export function App() {
  return (
    <main className="shell">
      <section className="workspace" aria-label="LoadArr workspace">
        <header className="header">
          <div>
            <p className="eyebrow">LoadArr</p>
            <h1>Warehouse load desk</h1>
          </div>
          <div className="status-pill">
            <Warehouse aria-hidden="true" />
            <span>Ready</span>
          </div>
        </header>

        <section className="toolbar" aria-label="Load filters">
          <button type="button" className="toolbar-button active">
            <ClipboardList aria-hidden="true" />
            Queue
          </button>
          <button type="button" className="toolbar-button">
            <Truck aria-hidden="true" />
            Dispatch
          </button>
          <button type="button" className="toolbar-button">
            <MapPin aria-hidden="true" />
            Sites
          </button>
        </section>

        <div className="grid">
          {[
            ['StaffArr sites', 'Internal origin', 'staffarrSiteOrgUnitId'],
            ['SupplyArr locations', 'Pick source', 'inventoryLocationId'],
            ['Outbound loads', 'Shipment intent', 'shipVia'],
          ].map(([title, label, value]) => (
            <article className="panel" key={title}>
              <span>{label}</span>
              <h2>{title}</h2>
              <code>{value}</code>
            </article>
          ))}
        </div>

        <section className="loads" aria-label="Load queue">
          <div className="load-row">
            <Boxes aria-hidden="true" />
            <div>
              <strong>Awaiting WMS assignment</strong>
              <span>StaffArr site and SupplyArr location required</span>
            </div>
            <button type="button">Open</button>
          </div>
        </section>
      </section>
    </main>
  )
}
