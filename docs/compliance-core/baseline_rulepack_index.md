# Baseline rulepack index

Generated from `tools/compliancecore/generate_baseline_rulepacks.py` on source date 2026-06-18.

These packs cover the non-Title-49 operational baseline. Title 49/FMCSA/PHMSA packs remain in `root/rulepack/title49`.

| Rulepack | Program | Primary products | Requirements |
|---|---|---|---|
| `osha.recordkeeping_reporting` | `osha_general_industry` | AssurArr, RecordArr, ReportArr, StaffArr | 3 |
| `osha.hazard_communication` | `osha_general_industry` | FieldCompanion, LoadArr, MaintainArr, RecordArr, StaffArr, SupplyArr, TrainArr | 3 |
| `osha.ppe_general_industry` | `osha_general_industry` | AssurArr, FieldCompanion, LoadArr, MaintainArr, StaffArr, TrainArr | 3 |
| `epa.hazardous_waste_generator` | `epa_environmental_spine` | AssurArr, LoadArr, MaintainArr, RecordArr, ReportArr, SupplyArr | 3 |
| `epa.spcc_oil_storage` | `epa_environmental_spine` | AssurArr, FieldCompanion, MaintainArr, RecordArr, StaffArr | 3 |
| `epa.epcra_cercla_release_reporting` | `epa_environmental_spine` | AssurArr, LoadArr, MaintainArr, ReportArr, SupplyArr | 3 |
| `employment.flsa_recordkeeping_notice` | `dol_employment_labor` | LedgArr, RecordArr, StaffArr | 3 |
| `employment.fmla_notice_leave` | `dol_employment_labor` | RecordArr, StaffArr | 3 |
| `privacy.ftc_glba_safeguards` | `ftc_privacy_communications` | AssurArr, ComplianceCore, CustomArr, NexArr, RecordArr, STLComplianceSite | 3 |
| `communications.tsr_can_spam` | `ftc_privacy_communications` | CustomArr, OrdArr, RecordArr, STLComplianceSite | 3 |
| `electronic_records.esign_ueta` | `federal_electronic_records` | ComplianceCore, CustomArr, FieldCompanion, NexArr, RecordArr | 3 |
| `business.entity_authority_licensing` | `state_business_authority` | LedgArr, RecordArr, ReportArr | 3 |
| `tax.statutory_financial_obligations` | `state_business_authority` | LedgArr, RecordArr, ReportArr, StaffArr | 3 |
| `commercial.ucc_orders_warranties` | `state_business_authority` | CustomArr, LedgArr, LoadArr, OrdArr, RecordArr, RoutArr | 3 |
| `consumer.accessibility_disclosures` | `doj_accessibility` | CustomArr, FieldCompanion, OrdArr, RecordArr, STLComplianceSite, StaffArr | 3 |
| `supplychain.trade_sanctions_import_product` | `trade_sanctions_supply_chain` | AssurArr, CustomArr, LoadArr, OrdArr, RecordArr, ReportArr, SupplyArr | 3 |

State and local overlays intentionally carry jurisdiction-specific source references. They must be refined into state/local versions before tenant-specific legal conclusions are treated as final.
