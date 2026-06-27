# Roadmapped Rollout Goal Prompt

Use this as a compact implementation goal when asking an agent to align code to the reconfigured roadmap.

## Prompt

Implement against the STL Compliance roadmapped rollout docs, not as an unordered feature dump. Treat `docs/roadmap/README.md`, `docs/roadmap/rollout-stages.md`, `docs/roadmap/release-gates-and-acceptance.md`, and `docs/roadmap/vertical-slice-backlog.md` as the sequencing layer. Treat all constitutions as binding and non-negotiable. Treat product `FEATURESET.md` and `WORKFLOWS.md` files as the complete retained product universe, not a demand to build every feature immediately.

Start with R0 trust gates and the current release train only. Do not remove features, workflows, page discipline, ownership boundaries, tenant scope, authorization, durable persistence, event/evidence/reportability requirements, quick create, print/export expectations, light/dark readability, or user-truthful error states. If a later feature is needed for the current vertical slice, pull it forward explicitly and preserve its source owner. Do not create shadow source-of-truth tables, cross-DB joins, fake local success paths, fixture-backed production writes, global in-memory production state, unnecessary internal IDs, dev labels, or freetext cross-product references.

Ship vertical proof before horizontal breadth. A release is complete only when the relevant workflow can execute, block, recover, persist, authorize, preserve evidence, show clear UI states, print/export where relevant, and report with source drillback.
