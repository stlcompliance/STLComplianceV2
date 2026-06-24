# STL Compliance Navigation and Information Architecture Constitution

## 1. Audit drivers

NAV-001 through NAV-005 found flat oversized sidebars, overloaded groups, duplicate labels, and unmanaged route aliases.

## 2. Prime directive

Once a user learns one STL Compliance product, the remaining products must feel natural. Product vocabulary changes; orientation, hierarchy, route behavior, page patterns, and action placement do not.

## 3. Sidebar rule

Sidebars contain durable workflow destinations, not every table or action. Group by user work, keep high-frequency destinations visible, and move secondary/rare surfaces into tabs, page actions, drawers, or administration.

No group should become a wall of links. When a group exceeds practical scanability, split it by workflow phase such as Inbound/Inventory/Outbound/Exceptions or Finance Operations/Planning/Close/Administration.

## 4. Route rule

Every route has a canonical URL, owner, page archetype, permission metadata, breadcrumb, and navigation location. Legacy aliases live in a redirect registry with reason, introduced date, telemetry, owner, and removal condition.

## 5. Record navigation

Primary records use the same list → drawer/peek → detail → edit/lifecycle pattern. Related foreign records use typed links and owner-backed labels. Transitional handoff pages are removed unless they provide a decision, review, or recovery function.

## 6. Labels

Use professional human labels. Hide raw IDs, permission keys, database terms, internal enums, and developer hints from ordinary pages. Technical values may appear on explicit admin/permission/advanced diagnostic surfaces.

## 7. Product-specific audit corrections

- AssurArr groups Quality Work, Audits, Quality Controls, Records and Analysis, Administration.
- LoadArr groups Inbound, Inventory, Outbound, Exceptions, Administration.
- LedgArr groups Finance Operations, Inventory and Assets, Planning, Close and Reporting, Administration.
- RecordArr avoids one-child duplicate groups.

## 8. Tests

Route-map tests prove all canonical routes resolve, breadcrumbs match, aliases redirect, permission-limited pages explain state, and keyboard/mobile navigation remains usable.
