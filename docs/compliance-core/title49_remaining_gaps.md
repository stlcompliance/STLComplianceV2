# Title 49 remaining gaps

- The current Compliance Core direct bundle has no separate fact-definition CSV; Compliance Core derives canonical fact definitions from `rule_fact_requirements.csv` during import.
- `evidence_references.csv` is generated as an empty staged-import schema file; tenant evidence references must arrive through product/RecordArr-backed workflows.
- Numeric thresholds, route approvals, hazmat quantity tables, insurance amount tables, and retention durations are represented as audit fact requirements with source/evidence/retention metadata; product-specific calculators should publish those facts deterministically.
- 49 CFR 172.101 Hazardous Materials Table is mapped as citation and lookup-verification control, not material-key enumeration.
- FMCSA Parts 384-386 and HMR Parts 174-176/179 are reference mapped unless a product workflow currently owns direct operational facts.
- Aviation, maritime, NTSB, Amtrak, TSA, STB, transit, NHTSA, and pipeline areas outside STL's motor-carrier/hazmat workflows are reference or metadata until product support exists.
- Historical removed-part mapping is not generated from current eCFR structure; current reserved parts are retained as inactive citation rows.
- Legal review is required before marking these preproduction packs as customer-enforceable policy.
