# Frontend UI and Design System

## App Shape

- stlcompliancesite: public static marketing site.
- suite-frontend: authenticated suite shell with product surfaces for all entitled products.
- Each product frontend (`apps/*-frontend`): authenticated workspace with **sidebar workflow routes** — not a single scrollable page of every panel.

## Layout

Topbar:

- Brand/product context left
- Tenant/user context visible
- **ProductSwitcher dropdown** (entitled products only) on the right — never a product list in the sidebar
- Global actions right
- Subtle bottom border

Sidebar (AppNav):

- **Current product workflow routes only** (People, Work orders, Dispatch, and so on)
- Group links by operational workflow, not database table names
- Admin/settings as their own route when needed
- Lucide icons from central registry (product apps: co-locate in `src/navigation/productNav.ts`)

Content:

- Forms and details use readable max width
- Dashboards, dispatch boards, tables, and matrices can use full width
- Cards use consistent padding, radius, border, shadow, and spacing

## Visual Direction

- Dark enterprise SaaS foundation
- Deep navy/charcoal surfaces
- Muted blue accents
- Cyan/blue focus states
- White/slate text hierarchy
- Subtle borders
- Soft elevated cards
- Clear risk/readiness/status colors

## Icon Standard

- Use lucide-react for React UI icons.
- Use Lucide icons for sidebar, topbar, settings, status, actions, empty states, and product switcher.
- Keep icons in `/apps/suite-frontend/src/navigation/navIcons.ts`.
- Product logos may be custom assets.
- Do not mix Font Awesome, Heroicons, Material Icons, and Lucide.

## Recommended navIcons.ts

```ts
import {
  ShieldCheck, Users, GraduationCap, Wrench, Route, PackageSearch,
  ClipboardCheck, Settings, LayoutDashboard, Bell, Search, FileText,
  Building2, KeyRound, Truck, HardHat, Boxes, BookOpenCheck,
  Database, Activity, AlertTriangle, ClipboardList, CalendarClock,
  Factory, Warehouse, UserCog, LockKeyhole, ShieldAlert
} from "lucide-react";

export const navIcons = {
  dashboard: LayoutDashboard,
  nexarr: ShieldCheck,
  staffarr: Users,
  trainarr: GraduationCap,
  maintainarr: Wrench,
  routarr: Route,
  supplyarr: PackageSearch,
  complianceCore: ClipboardCheck,
  settings: Settings,
  notifications: Bell,
  search: Search,
  documents: FileText,
  sites: Building2,
  permissions: KeyRound,
  fleet: Truck,
  safety: HardHat,
  inventory: Boxes,
  training: BookOpenCheck,
  database: Database,
  activity: Activity,
  warning: AlertTriangle,
  inspections: ClipboardList,
  preventiveMaintenance: CalendarClock,
  facilities: Factory,
  warehouse: Warehouse,
  userAdmin: UserCog,
  auth: LockKeyhole,
  complianceAlert: ShieldAlert
};
```

## Accessibility

- Keyboard navigation works.
- Focus states are visible.
- Forms have labels and validation text.
- Status is not color-only.
- Dialogs trap focus.
- Icons are decorative unless status-bearing.

## Authority Rule

The frontend may guide, hide, disable, warn, and explain. It may not grant authority. Every mutation is validated by the owning product API.
