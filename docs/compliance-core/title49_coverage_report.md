# Title 49 coverage report

Source date: 2026-05-28

Sources:
- eCFR current Title 49: https://www.ecfr.gov/current/title-49
- eCFR titles API: https://www.ecfr.gov/api/versioner/v1/titles.json
- eCFR structure API: https://www.ecfr.gov/api/versioner/v1/structure/2026-05-28/title-49.json
- eCFR full XML API: https://www.ecfr.gov/api/versioner/v1/full/2026-05-28/title-49.xml
- GPO bulk XML: https://www.govinfo.gov/bulkdata/ECFR/title-49/ECFR-title49.xml
- FMCSA regulations: https://www.fmcsa.dot.gov/regulations/49-cfr-parts-300-399
- PHMSA HMR reference: https://www.phmsa.dot.gov/regulations/title49/part/172
- DOT Part 40: https://www.transportation.gov/odapc/part40

## Counts

| Metric | Count |
| --- | ---: |
| Parts in current eCFR hierarchy | 419 |
| Reserved parts in current hierarchy | 44 |
| Removed parts in current hierarchy | 0 |
| Unmapped/non-operational parts retained as metadata | 82 |
| Rule packs | 44 |
| Operational packs | 32 |
| Reference packs | 11 |
| Citation metadata packs | 1 |
| Citations | 10786 |
| Facts | 85 |
| Rules | 84 |
| Conditions | 84 |
| Outcomes | 64 |
| Regulatory programs used | 13 |

## Hierarchy coverage

| Hierarchy node type | Count |
| --- | ---: |
| title | 1 |
| subtitle | 2 |
| chapter | 11 |
| subchapter | 16 |
| part | 419 |
| subpart | 935 |
| section | 9112 |
| appendix | 290 |

## Manual-review areas

- title49.hazmat.loading_unloading_segregation_reference: Rail, air, and vessel operational gates are reference only until RoutArr/SupplyArr mode workflows exist.
- title49.motorcarrier.safety_fitness_proceedings_reference: Parts 384-386 are reference mapped until product workflows own state CDL compliance, safety fitness scoring, and FMCSA proceeding operations.
- Numeric thresholds, exception applicability, route-specific approvals, and document-retention windows need legal/product review before enforcement beyond boolean gate checks.
- HMR table row enumeration is modeled as lookup verification against 49 CFR 172.101, not as a row-per-material material-key catalog.
- The 9-CSV bundle intentionally has no separate fact-definition CSV; Compliance Core imports fact definitions and audit contracts from rule_fact_requirements.csv.

## Metadata-retained parts

1, 3, 5, 6, 7, 8, 9, 10, 11, 15, 17, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 37, 38, 39, 41, 71, 79, 80, 89, 91, 92, 98, 99, 105, 106, 109, 110, 130, 303, 325, 350, 360, 369, 370, 371, 372, 373, 376, 377, 378, 379, 381, 389, 398, 399, 450, 451, 452, 453, 700, 701, 800, 801, 802, 803, 804, 806, 807, 821, 825, 826, 830, 831, 835, 837, 840, 845, 850
