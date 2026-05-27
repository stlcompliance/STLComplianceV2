# STL Compliance / Arr Suite Masterplan

## Vision

STL Compliance is a multi-product operational compliance suite for fleets, shops, transportation teams, workforce readiness, training, supply operations, and regulated workplace processes.

ARR means **Adaptive Risk Reduction**: the suite reduces operational risk by connecting people, assets, training, supply, routing, and compliance authority before problems turn into downtime, unsafe work, violations, failed audits, or scattered paperwork.

## Product Network

| Product | Role |
|---|---|
| NexArr | Platform control plane: login, tenants, entitlements, licensing, service clients, service tokens, suite launch. |
| StaffArr | Workforce backbone: people, org structure, permissions, certifications, readiness, incidents, personnel history. |
| TrainArr | Training proof: programs, requirements, evidence, evaluations, signoffs, qualification issue and publication. |
| MaintainArr | Maintenance execution: assets, inspections, defects, work orders, preventive maintenance, asset readiness. |
| RoutArr | Dispatch execution: routes, trips, driver assignment, DVIR, proof, exceptions, transportation audit trail. |
| SupplyArr | Supply execution: vendors, parts, inventory, purchase requests, purchase orders, receiving, pricing, lead times. |
| Compliance Core | Authority layer: vocabulary, keys, material keys, rule packs, mappings, SDS/HazCom, evaluation patterns. |
| STLComplianceSite | Public site: marketing, trust messaging, product education, demo/contact entry. |

## Platform Shape

The suite is unified through NexArr and the Suite Frontend, not through one shared database. Each product has its own API, worker, and PostgreSQL database. Products cooperate through APIs, events, service tokens, and local references.

## V1 Posture

V1 is the full suite shape. Every product ships as a real deployable service with real boundaries. Advanced workflows may start thin, but no product is merely a fake frontend shell, and no product is folded into another product for convenience.

## Enterprise Principles

1. Ownership first: every record has one authoritative product.
2. Server authority always wins over frontend hints.
3. Audit evidence is designed into every high-risk workflow.
4. StaffArr is the workforce truth.
5. NexArr is the platform control plane.
6. Compliance Core is the authority and vocabulary layer.
7. Operational users stay in flow through owner-controlled surfaces and APIs.
8. No cross-product database foreign keys.
9. Product-specific databases and workers exist from day one.
10. Render deployment mirrors the long-term service shape.

## Completion Definition

The suite is complete when it can prove who performed work, whether they were authorized, what evidence was captured, which product owned the source record, which rule or readiness result applied, what changed over time, and why a workflow was allowed, warned, blocked, or escalated.

## Core Rules

{hard_rules}
