#!/usr/bin/env python3
"""Generate Compliance Core baseline rule-pack CSV bundles and docs."""

from __future__ import annotations

import argparse
import csv
import hashlib
import io
import json
import shutil
import sys
from dataclasses import dataclass, field
from pathlib import Path
from typing import Any


SOURCE_DATE = "2026-06-18"
SOURCE_NOTE = "Source date 2026-06-18; source URLs in coverage report."

CSV_HEADERS: dict[str, list[str]] = {
    "controlled_vocabulary.csv": ["term_key", "vocabulary_type_key", "label", "description", "active"],
    "vocabulary_aliases.csv": ["term_key", "alias_text", "active"],
    "compliance_keys.csv": ["key", "label", "category", "description", "active"],
    "material_keys.csv": ["key", "label", "category", "description", "active"],
    "rule_packs.csv": [
        "pack_key",
        "program_key",
        "version_number",
        "label",
        "description",
        "status",
        "active",
        "rule_content_json",
    ],
    "rule_requirements.csv": [
        "citation_key",
        "program_key",
        "pack_key",
        "pack_version",
        "label",
        "source_reference",
        "description",
        "active",
        "supersedes_citation_key",
    ],
    "rule_fact_requirements.csv": [
        "requirement_key",
        "fact_key",
        "pack_key",
        "pack_version",
        "citation_key",
        "citation_version",
        "applicability_key",
        "source_product",
        "source_entity",
        "source_field_or_record_type",
        "value_type",
        "operator",
        "expected_value",
        "evidence_kind",
        "required_document_type",
        "retention_period",
        "audit_question",
        "failure_severity",
        "automatic_failure_flag",
        "override_allowed",
        "override_permission",
        "remediation_required",
        "label",
        "description",
        "is_required",
        "active",
    ],
    "regulatory_mappings.csv": [
        "mapping_key",
        "target_kind",
        "program_key",
        "pack_key",
        "pack_version",
        "citation_key",
        "compliance_key",
        "material_key",
        "fact_key",
        "label",
        "description",
        "active",
    ],
    "sds_references.csv": ["sds_key", "material_key", "product_name", "manufacturer", "document_url", "revision_date", "active"],
    "exception_exemptions.csv": [
        "key",
        "label",
        "type",
        "governing_body",
        "program_key",
        "pack_key",
        "citation_key",
        "applicability_key",
        "applies_to_subject_kind",
        "applies_to_source_product",
        "applies_to_source_entity",
        "effect_type",
        "condition_logic_json",
        "required_evidence_option_group_key",
        "issuing_authority",
        "authorization_number",
        "effective_at",
        "expires_at",
        "active",
        "description",
    ],
    "evidence_references.csv": [
        "evidence_id",
        "fact_key",
        "source_product",
        "source_entity",
        "source_record_id",
        "source_field",
        "document_type",
        "document_url",
        "storage_key",
        "file_hash",
        "captured_at",
        "effective_at",
        "expires_at",
        "review_status",
        "notes",
    ],
}


@dataclass(frozen=True)
class CitationSpec:
    key: str
    label: str
    source_reference: str
    description: str
    url: str = ""


@dataclass(frozen=True)
class FactSpec:
    key: str
    label: str
    description: str
    products: list[str]
    entities: list[str]
    citation_key: str
    source_field_or_record_type: str
    evidence_kind: str = "product_record"
    required_document_type: str = ""
    retention_period: str = "per_citation_or_company_policy"
    audit_question: str = ""
    failure_severity: str = "major"
    automatic_failure_flag: bool = False
    override_allowed: bool = True
    override_permission: str = ""
    remediation_required: bool = True
    value_type: str = "boolean"
    operator: str = "equals"
    expected_value: str = "true"
    is_required: bool = True


@dataclass(frozen=True)
class ExceptionSpec:
    key: str
    label: str
    type: str
    effect_type: str
    citation_key: str
    subject_kind: str
    products: list[str]
    entities: list[str]
    condition_logic: dict[str, Any]
    description: str


@dataclass(frozen=True)
class PackSpec:
    key: str
    label: str
    description: str
    program_key: str
    compliance_key: str
    citations: list[CitationSpec]
    facts: list[FactSpec]
    aliases: list[str] = field(default_factory=list)
    exceptions: list[ExceptionSpec] = field(default_factory=list)
    reference_only: bool = False


PROGRAMS: dict[str, dict[str, str]] = {
    "osha_general_industry": {
        "body_key": "osha",
        "body_label": "Occupational Safety and Health Administration",
        "jurisdiction_key": "us_federal_osha",
        "label": "OSHA General Industry and Recordkeeping",
        "description": "Federal OSHA recordkeeping and general industry baseline requirements.",
    },
    "epa_environmental_spine": {
        "body_key": "epa",
        "body_label": "Environmental Protection Agency",
        "jurisdiction_key": "us_federal_epa",
        "label": "EPA Environmental Spine",
        "description": "Federal environmental waste, release, water, air, refrigerant, chemical, and tank baseline requirements.",
    },
    "dol_employment_labor": {
        "body_key": "dol",
        "body_label": "U.S. Department of Labor",
        "jurisdiction_key": "us_federal_dol",
        "label": "DOL Employment and Labor Baseline",
        "description": "Federal wage, leave, notice, and labor recordkeeping baseline requirements.",
    },
    "ftc_privacy_communications": {
        "body_key": "ftc",
        "body_label": "Federal Trade Commission",
        "jurisdiction_key": "us_federal_ftc",
        "label": "FTC Privacy, Safeguards, and Communications",
        "description": "Federal privacy, safeguards, telemarketing, and commercial email baseline requirements.",
    },
    "federal_electronic_records": {
        "body_key": "congress",
        "body_label": "United States Congress",
        "jurisdiction_key": "us_federal_congress",
        "label": "Electronic Records and Signatures",
        "description": "E-SIGN and state UETA electronic records and signatures baseline requirements.",
    },
    "state_business_authority": {
        "body_key": "state_authorities",
        "body_label": "State and Local Filing Authorities",
        "jurisdiction_key": "us_state_local_overlay",
        "label": "State and Local Business Authority",
        "description": "Jurisdiction-specific entity, licensing, permit, tax, UCC, consumer, and accessibility overlays.",
    },
    "doj_accessibility": {
        "body_key": "doj",
        "body_label": "U.S. Department of Justice",
        "jurisdiction_key": "us_federal_doj",
        "label": "ADA Public Accommodation Accessibility",
        "description": "ADA Title III public accommodation and effective communication baseline requirements.",
    },
    "trade_sanctions_supply_chain": {
        "body_key": "trade_authorities",
        "body_label": "U.S. Trade, Customs, and Sanctions Authorities",
        "jurisdiction_key": "us_federal_trade",
        "label": "Trade, Customs, Sanctions, and Supply Chain",
        "description": "Trade, customs, sanctions, forced-labor, and product compliance intake baseline requirements.",
    },
}


PACKS: list[PackSpec] = [
    PackSpec(
        key="osha.recordkeeping_reporting",
        label="OSHA Injury and Illness Recordkeeping",
        description="Operational requirements for OSHA injury and illness logs, severe incident reporting, and annual summary posting.",
        program_key="osha_general_industry",
        compliance_key="ck_osha_recordkeeping_reporting",
        aliases=["OSHA 300", "OSHA 300A", "severe injury reporting"],
        citations=[
            CitationSpec("osha_part_1904", "Recording and Reporting Occupational Injuries and Illnesses", "29 CFR Part 1904", "Recordkeeping and reporting framework for work-related fatalities, injuries, and illnesses.", "https://www.ecfr.gov/current/title-29/subtitle-B/chapter-XVII/part-1904"),
            CitationSpec("osha_sec_1904_39", "Reporting fatalities, hospitalizations, amputations, and eye losses", "29 CFR 1904.39", "Severe injury and fatality reporting trigger.", "https://www.ecfr.gov/current/title-29/subtitle-B/chapter-XVII/part-1904/section-1904.39"),
            CitationSpec("osha_sec_1904_32", "Annual summary", "29 CFR 1904.32", "Annual summary review, certification, and posting expectation.", "https://www.ecfr.gov/current/title-29/subtitle-B/chapter-XVII/part-1904/section-1904.32"),
        ],
        facts=[
            FactSpec("osha_recordable_incident_log_current", "OSHA log current", "OSHA recordable injury and illness log is current for covered establishments.", ["StaffArr", "AssurArr", "RecordArr"], ["incident", "establishment"], "osha_part_1904", "osha_log", "document_record", "osha_300_log", "per_29_cfr_1904", "Is the OSHA recordable incident log current?", "critical", True, False),
            FactSpec("osha_severe_incident_reported_on_time", "Severe incident reported on time", "Fatality, hospitalization, amputation, or eye-loss events are escalated and reported within the required OSHA window.", ["StaffArr", "AssurArr", "ReportArr"], ["incident", "notification"], "osha_sec_1904_39", "incident_report", "system_fact", "", "per_29_cfr_1904", "Was the severe incident reported to OSHA on time?", "critical", True, False),
            FactSpec("osha_annual_summary_posted", "Annual summary posted", "Annual OSHA summary is reviewed, certified, and posted for the applicable posting window.", ["StaffArr", "RecordArr"], ["establishment", "posting"], "osha_sec_1904_32", "annual_summary", "document_record", "osha_300a_summary", "per_29_cfr_1904", "Is the annual OSHA summary posted and retained?", "major"),
        ],
    ),
    PackSpec(
        key="osha.hazard_communication",
        label="OSHA Hazard Communication",
        description="Operational requirements for hazardous chemical labels, SDS access, and hazard communication training.",
        program_key="osha_general_industry",
        compliance_key="ck_osha_hazard_communication",
        aliases=["HazCom", "SDS access", "hazard communication"],
        citations=[
            CitationSpec("osha_sec_1910_1200", "Hazard communication", "29 CFR 1910.1200", "Hazard communication requirements for hazardous chemicals in the workplace.", "https://www.ecfr.gov/current/title-29/subtitle-B/chapter-XVII/part-1910/section-1910.1200"),
        ],
        facts=[
            FactSpec("osha_hazcom_sds_accessible", "SDS accessible", "Safety data sheets are accessible to exposed workers for hazardous chemicals in the workplace.", ["SupplyArr", "LoadArr", "RecordArr", "FieldCompanion"], ["chemical", "sds", "workplace"], "osha_sec_1910_1200", "sds_access", "document_record", "safety_data_sheet", "while_chemical_present_plus_policy", "Are SDS records accessible for hazardous chemicals?", "critical", True, False),
            FactSpec("osha_hazcom_container_labels_current", "Container labels current", "Hazardous chemical containers have required identity and hazard communication labeling.", ["LoadArr", "MaintainArr", "FieldCompanion"], ["chemical", "container", "workplace"], "osha_sec_1910_1200", "container_label", "product_record", "", "while_chemical_present_plus_policy", "Are hazardous chemical container labels current?", "major"),
            FactSpec("osha_hazcom_training_complete", "HazCom training complete", "Workers with hazardous chemical exposure have completed required hazard communication training.", ["TrainArr", "StaffArr"], ["person", "training_assignment"], "osha_sec_1910_1200", "training_record", "product_record", "", "employment_or_exposure_period_plus_policy", "Have exposed workers completed HazCom training?", "critical", True, False),
        ],
    ),
    PackSpec(
        key="osha.ppe_general_industry",
        label="OSHA PPE General Industry",
        description="Operational requirements for PPE hazard assessments, provision, and training.",
        program_key="osha_general_industry",
        compliance_key="ck_osha_ppe_general_industry",
        aliases=["PPE", "hazard assessment", "personal protective equipment"],
        citations=[
            CitationSpec("osha_subpart_i_1910", "Personal Protective Equipment", "29 CFR Part 1910 Subpart I", "General industry PPE standards.", "https://www.ecfr.gov/current/title-29/subtitle-B/chapter-XVII/part-1910/subpart-I"),
            CitationSpec("osha_sec_1910_132", "General PPE requirements", "29 CFR 1910.132", "PPE assessment, selection, use, and training requirements.", "https://www.ecfr.gov/current/title-29/subtitle-B/chapter-XVII/part-1910/section-1910.132"),
        ],
        facts=[
            FactSpec("osha_ppe_hazard_assessment_current", "PPE hazard assessment current", "Workplace PPE hazard assessment is current for the task or location.", ["StaffArr", "MaintainArr", "LoadArr", "AssurArr"], ["workplace", "task", "hazard_assessment"], "osha_sec_1910_132", "hazard_assessment", "document_record", "ppe_hazard_assessment", "while_hazard_exists_plus_policy", "Is the PPE hazard assessment current?", "major"),
            FactSpec("osha_ppe_required_items_available", "Required PPE available", "Required PPE is available before covered work begins.", ["MaintainArr", "LoadArr", "FieldCompanion"], ["task", "person", "equipment"], "osha_subpart_i_1910", "ppe_issue", "product_record", "", "per_task_or_policy", "Is required PPE available for the work?", "critical", True, False),
            FactSpec("osha_ppe_training_complete", "PPE training complete", "Affected workers have completed PPE training for required equipment use.", ["TrainArr", "StaffArr"], ["person", "training_assignment"], "osha_sec_1910_132", "training_record", "product_record", "", "employment_or_exposure_period_plus_policy", "Has required PPE training been completed?", "major"),
        ],
    ),
    PackSpec(
        key="epa.hazardous_waste_generator",
        label="EPA Hazardous Waste Generator",
        description="Operational requirements for hazardous-waste determination, generator status, manifests, accumulation, and emergency preparedness.",
        program_key="epa_environmental_spine",
        compliance_key="ck_epa_hazardous_waste_generator",
        aliases=["RCRA generator", "hazardous waste determination", "waste manifest"],
        citations=[
            CitationSpec("epa_part_262", "Standards Applicable to Generators of Hazardous Waste", "40 CFR Part 262", "Hazardous-waste generator standards.", "https://www.ecfr.gov/current/title-40/chapter-I/subchapter-I/part-262"),
            CitationSpec("epa_sec_262_11", "Hazardous waste determination", "40 CFR 262.11", "Hazardous waste determination and supporting records.", "https://www.ecfr.gov/current/title-40/chapter-I/subchapter-I/part-262/section-262.11"),
            CitationSpec("epa_sec_262_40", "Recordkeeping", "40 CFR 262.40", "Manifest and hazardous-waste record retention.", "https://www.ecfr.gov/current/title-40/chapter-I/subchapter-I/part-262/section-262.40"),
        ],
        facts=[
            FactSpec("epa_waste_determination_documented", "Waste determination documented", "Hazardous-waste determinations are documented for generated waste streams.", ["MaintainArr", "LoadArr", "SupplyArr", "RecordArr"], ["waste_stream", "material"], "epa_sec_262_11", "waste_profile", "document_record", "waste_determination", "per_40_cfr_262", "Is the waste determination documented?", "critical", True, False),
            FactSpec("epa_generator_category_current", "Generator category current", "Generator status/category is current for the site's hazardous-waste generation quantities.", ["AssurArr", "MaintainArr", "ReportArr"], ["site", "waste_stream"], "epa_part_262", "generator_status", "system_fact", "", "per_reporting_period_or_policy", "Is generator status current for the site?", "critical", True, False),
            FactSpec("epa_hazardous_waste_manifests_retained", "Hazardous waste manifests retained", "Hazardous-waste manifests and related records are retained for the required period.", ["RecordArr", "LoadArr", "MaintainArr"], ["waste_shipment", "record"], "epa_sec_262_40", "manifest_record", "document_record", "hazardous_waste_manifest", "three_years_or_longer_if_required", "Are hazardous-waste manifests retained?", "major"),
        ],
    ),
    PackSpec(
        key="epa.spcc_oil_storage",
        label="EPA SPCC Oil Storage",
        description="Operational requirements for SPCC applicability, plans, inspections, and secondary containment.",
        program_key="epa_environmental_spine",
        compliance_key="ck_epa_spcc_oil_storage",
        aliases=["SPCC", "oil storage", "secondary containment"],
        citations=[
            CitationSpec("epa_part_112", "Oil Pollution Prevention", "40 CFR Part 112", "SPCC and oil pollution prevention requirements.", "https://www.ecfr.gov/current/title-40/chapter-I/subchapter-D/part-112"),
        ],
        facts=[
            FactSpec("epa_spcc_applicability_review_current", "SPCC applicability reviewed", "Facility oil storage facts are reviewed for SPCC applicability.", ["MaintainArr", "StaffArr", "AssurArr"], ["site", "tank", "oil_storage"], "epa_part_112", "oil_storage_profile", "system_fact", "", "per_site_change_or_policy", "Is SPCC applicability current?", "major"),
            FactSpec("epa_spcc_plan_current", "SPCC plan current", "Covered facility has a current SPCC plan and review evidence.", ["RecordArr", "MaintainArr", "AssurArr"], ["site", "plan"], "epa_part_112", "spcc_plan", "document_record", "spcc_plan", "per_40_cfr_112", "Is the SPCC plan current?", "critical", True, False),
            FactSpec("epa_spcc_inspections_current", "SPCC inspections current", "Required SPCC inspections or integrity checks are current for covered tanks/equipment.", ["MaintainArr", "FieldCompanion", "RecordArr"], ["tank", "inspection"], "epa_part_112", "inspection_record", "product_record", "", "per_plan_or_citation", "Are SPCC inspections current?", "critical", True, False),
        ],
    ),
    PackSpec(
        key="epa.epcra_cercla_release_reporting",
        label="EPCRA and CERCLA Release Reporting",
        description="Operational requirements for emergency planning, Tier II/TRI thresholds, and release notifications.",
        program_key="epa_environmental_spine",
        compliance_key="ck_epa_epcra_cercla_release_reporting",
        aliases=["EPCRA", "Tier II", "TRI", "CERCLA release"],
        citations=[
            CitationSpec("epa_part_370", "Hazardous Chemical Reporting", "40 CFR Part 370", "EPCRA hazardous chemical inventory reporting.", "https://www.ecfr.gov/current/title-40/chapter-I/subchapter-J/part-370"),
            CitationSpec("epa_part_372", "Toxic Chemical Release Reporting", "40 CFR Part 372", "TRI toxic chemical release reporting.", "https://www.ecfr.gov/current/title-40/chapter-I/subchapter-J/part-372"),
            CitationSpec("epa_cercla_release_reporting", "CERCLA release reporting", "40 CFR Part 302", "Reportable quantity release designation and notification baseline.", "https://www.ecfr.gov/current/title-40/chapter-I/subchapter-J/part-302"),
        ],
        facts=[
            FactSpec("epa_tier_ii_inventory_review_current", "Tier II inventory review current", "Facility hazardous chemical inventory is reviewed for EPCRA Tier II reporting thresholds.", ["SupplyArr", "LoadArr", "ReportArr"], ["site", "chemical_inventory"], "epa_part_370", "chemical_inventory", "system_fact", "", "annual_or_site_change", "Is Tier II inventory review current?", "major"),
            FactSpec("epa_tri_threshold_review_current", "TRI threshold review current", "Facility toxic chemical activity is reviewed for TRI threshold and reporting obligations.", ["SupplyArr", "LoadArr", "ReportArr"], ["site", "chemical_activity"], "epa_part_372", "chemical_activity", "system_fact", "", "annual_or_site_change", "Is TRI threshold review current?", "major"),
            FactSpec("epa_reportable_release_escalated", "Reportable release escalated", "Potential reportable releases are escalated for CERCLA/EPCRA notification review.", ["AssurArr", "MaintainArr", "ReportArr"], ["incident", "release"], "epa_cercla_release_reporting", "release_event", "product_record", "", "per_incident", "Was the reportable release escalated?", "critical", True, False),
        ],
    ),
    PackSpec(
        key="employment.flsa_recordkeeping_notice",
        label="FLSA Recordkeeping and Notice",
        description="Operational requirements for covered employee wage/hour records and required FLSA posting.",
        program_key="dol_employment_labor",
        compliance_key="ck_employment_flsa_recordkeeping_notice",
        aliases=["FLSA", "wage hour records", "minimum wage poster"],
        citations=[
            CitationSpec("dol_29_cfr_516", "Records To Be Kept By Employers", "29 CFR Part 516", "FLSA wage, hour, and payroll recordkeeping regulations.", "https://www.ecfr.gov/current/title-29/subtitle-B/chapter-V/subchapter-A/part-516"),
            CitationSpec("dol_flsa_recordkeeping_fact_sheet", "DOL FLSA recordkeeping guidance", "DOL Fact Sheet #21", "DOL recordkeeping guidance for FLSA-covered employers.", "https://www.dol.gov/agencies/whd/fact-sheets/21-flsa-recordkeeping"),
        ],
        facts=[
            FactSpec("flsa_time_pay_records_current", "FLSA time and pay records current", "Covered employee time and pay records are current and retained.", ["StaffArr", "LedgArr", "RecordArr"], ["person", "timesheet", "pay_record"], "dol_29_cfr_516", "time_pay_record", "product_record", "", "per_29_cfr_516", "Are FLSA time and pay records current?", "critical", True, False),
            FactSpec("flsa_required_poster_posted", "FLSA poster posted", "Required FLSA notice/poster is posted or made available for covered workers.", ["StaffArr", "RecordArr"], ["worksite", "notice"], "dol_flsa_recordkeeping_fact_sheet", "posting_record", "document_record", "flsa_poster", "while_applicable_plus_policy", "Is the required FLSA poster posted?", "minor"),
            FactSpec("flsa_worker_classification_reviewed", "Worker classification reviewed", "Worker exemption/classification facts are reviewed before pay-rule application.", ["StaffArr", "LedgArr"], ["person", "position"], "dol_29_cfr_516", "classification_record", "system_fact", "", "employment_period_plus_policy", "Has worker classification been reviewed?", "major"),
        ],
    ),
    PackSpec(
        key="employment.fmla_notice_leave",
        label="FMLA Notice and Leave Administration",
        description="Operational requirements for FMLA coverage, notice, eligibility, certification, and leave records.",
        program_key="dol_employment_labor",
        compliance_key="ck_employment_fmla_notice_leave",
        aliases=["FMLA", "leave eligibility", "FMLA poster"],
        citations=[
            CitationSpec("dol_29_cfr_825", "Family and Medical Leave Act regulations", "29 CFR Part 825", "FMLA employer notice, eligibility, certification, and recordkeeping regulations.", "https://www.ecfr.gov/current/title-29/subtitle-B/chapter-V/subchapter-C/part-825"),
            CitationSpec("dol_fmla_employment_guide", "DOL FMLA employment law guide", "DOL Employment Law Guide - FMLA", "DOL guide for FMLA notices and covered employer obligations.", "https://webapps.dol.gov/elaws/elg/fmla.htm"),
        ],
        facts=[
            FactSpec("fmla_coverage_review_current", "FMLA coverage review current", "Employer/worksite/headcount facts are current for FMLA coverage and eligibility decisions.", ["StaffArr"], ["tenant", "worksite", "person"], "dol_29_cfr_825", "coverage_profile", "system_fact", "", "per_headcount_period_or_policy", "Is FMLA coverage review current?", "major"),
            FactSpec("fmla_required_notice_posted", "FMLA notice posted", "Required FMLA notice is posted or distributed when applicable.", ["StaffArr", "RecordArr"], ["worksite", "notice"], "dol_fmla_employment_guide", "posting_record", "document_record", "fmla_notice", "while_applicable_plus_policy", "Is FMLA notice posted or distributed?", "minor"),
            FactSpec("fmla_leave_file_complete", "FMLA leave file complete", "FMLA leave eligibility, notices, certifications, and designation records are complete for the leave case.", ["StaffArr", "RecordArr"], ["person", "leave_case"], "dol_29_cfr_825", "leave_case", "product_record", "", "per_29_cfr_825", "Is the FMLA leave file complete?", "major"),
        ],
    ),
    PackSpec(
        key="privacy.ftc_glba_safeguards",
        label="FTC GLBA Privacy and Safeguards",
        description="Operational requirements for FTC jurisdiction financial-institution privacy notices and safeguards programs.",
        program_key="ftc_privacy_communications",
        compliance_key="ck_privacy_ftc_glba_safeguards",
        aliases=["GLBA Safeguards Rule", "FTC privacy rule", "customer information security"],
        citations=[
            CitationSpec("ftc_16_cfr_313", "Privacy of Consumer Financial Information", "16 CFR Part 313", "FTC GLBA privacy notice and nonpublic personal information rules.", "https://www.ecfr.gov/current/title-16/chapter-I/subchapter-C/part-313"),
            CitationSpec("ftc_16_cfr_314", "Standards for Safeguarding Customer Information", "16 CFR Part 314", "FTC Safeguards Rule information security program requirements.", "https://www.ecfr.gov/current/title-16/chapter-I/subchapter-C/part-314"),
        ],
        facts=[
            FactSpec("ftc_glba_financial_institution_review_current", "GLBA applicability reviewed", "Tenant activities are reviewed for FTC GLBA financial-institution applicability.", ["NexArr", "CustomArr", "ComplianceCore"], ["tenant", "data_processing_activity"], "ftc_16_cfr_314", "applicability_profile", "system_fact", "", "annual_or_business_change", "Is FTC GLBA applicability reviewed?", "major"),
            FactSpec("ftc_privacy_notice_current", "Privacy notice current", "Required privacy notices are current for covered customer information practices.", ["CustomArr", "STLComplianceSite", "RecordArr"], ["customer", "notice"], "ftc_16_cfr_313", "privacy_notice", "document_record", "privacy_notice", "while_applicable_plus_policy", "Is the privacy notice current?", "major"),
            FactSpec("ftc_safeguards_program_current", "Safeguards program current", "Covered safeguards program, risk assessment, and controls are current.", ["NexArr", "RecordArr", "AssurArr"], ["security_program", "risk_assessment"], "ftc_16_cfr_314", "security_program", "document_record", "information_security_program", "per_16_cfr_314", "Is the safeguards program current?", "critical", True, False),
        ],
    ),
    PackSpec(
        key="communications.tsr_can_spam",
        label="Telemarketing and Commercial Email",
        description="Operational requirements for TSR calling rules, Do-Not-Call suppression, telemarketing records, and CAN-SPAM commercial email controls.",
        program_key="ftc_privacy_communications",
        compliance_key="ck_communications_tsr_can_spam",
        aliases=["TSR", "Do Not Call", "CAN-SPAM", "marketing email"],
        citations=[
            CitationSpec("ftc_16_cfr_310", "Telemarketing Sales Rule", "16 CFR Part 310", "FTC Telemarketing Sales Rule.", "https://www.ecfr.gov/current/title-16/chapter-I/subchapter-C/part-310"),
            CitationSpec("ftc_sec_310_5", "TSR recordkeeping", "16 CFR 310.5", "TSR telemarketing recordkeeping requirements.", "https://www.ecfr.gov/current/title-16/chapter-I/subchapter-C/part-310/section-310.5"),
            CitationSpec("ftc_can_spam_guide", "CAN-SPAM compliance guide", "FTC CAN-SPAM Act compliance guide", "FTC business guidance for commercial email requirements.", "https://www.ftc.gov/business-guidance/resources/can-spam-act-compliance-guide-business"),
        ],
        facts=[
            FactSpec("tsr_dnc_suppression_current", "DNC suppression current", "Calling lists are screened against applicable Do-Not-Call and internal suppression lists before outbound telemarketing.", ["CustomArr", "OrdArr", "RecordArr"], ["campaign", "contact"], "ftc_16_cfr_310", "suppression_check", "system_fact", "", "per_campaign_or_call_window", "Is DNC suppression current?", "critical", True, False),
            FactSpec("tsr_records_retained", "TSR records retained", "Required telemarketing scripts, promotional materials, consent, and transaction records are retained.", ["CustomArr", "RecordArr"], ["campaign", "record"], "ftc_sec_310_5", "telemarketing_record", "document_record", "telemarketing_campaign_record", "five_years_or_longer_if_required", "Are TSR records retained?", "major"),
            FactSpec("can_spam_email_controls_current", "CAN-SPAM controls current", "Commercial email has accurate headers, non-deceptive subject, postal address, and opt-out handling.", ["CustomArr", "STLComplianceSite", "RecordArr"], ["campaign", "email"], "ftc_can_spam_guide", "email_campaign", "system_fact", "", "per_campaign_plus_policy", "Are CAN-SPAM controls current?", "major"),
        ],
    ),
    PackSpec(
        key="electronic_records.esign_ueta",
        label="E-SIGN and UETA Electronic Records",
        description="Operational requirements for electronic consent, attribution, integrity, reproducibility, delivery, and retention evidence.",
        program_key="federal_electronic_records",
        compliance_key="ck_electronic_records_esign_ueta",
        aliases=["E-SIGN", "UETA", "electronic signature", "electronic record"],
        citations=[
            CitationSpec("esign_15_usc_7001", "E-SIGN general validity rule", "15 U.S.C. 7001", "Electronic signatures and records general validity rule.", "https://www.govinfo.gov/link/uscode/15/7001"),
            CitationSpec("state_ueta_overlay", "State UETA enactments", "State UETA enactments", "Jurisdiction-specific state electronic transaction overlay.", "https://www.uniformlaws.org/committees/community-home?CommunityKey=2c04b76d-7897-4f69-b1fc-98e55e5c1d12"),
        ],
        facts=[
            FactSpec("esign_consumer_consent_captured", "Electronic consent captured", "Required consent to use electronic records/signatures is captured before electronic delivery or signature workflow.", ["NexArr", "CustomArr", "FieldCompanion", "RecordArr"], ["person", "signature_session"], "esign_15_usc_7001", "consent_record", "document_record", "esign_consent", "transaction_life_plus_policy", "Was electronic consent captured?", "critical", True, False),
            FactSpec("esign_record_integrity_preserved", "Electronic record integrity preserved", "Electronic records preserve integrity, attribution, reproducibility, and audit chain.", ["RecordArr", "NexArr", "FieldCompanion"], ["record", "signature_session"], "esign_15_usc_7001", "record_integrity", "system_fact", "", "record_retention_period", "Is electronic record integrity preserved?", "critical", True, False),
            FactSpec("ueta_state_overlay_reviewed", "State UETA overlay reviewed", "State enactment, exclusions, and amendments are reviewed for the transaction jurisdiction.", ["ComplianceCore", "RecordArr"], ["transaction", "jurisdiction"], "state_ueta_overlay", "jurisdiction_overlay", "derived_fact", "", "per_transaction_or_policy", "Was the state UETA overlay reviewed?", "major"),
        ],
    ),
    PackSpec(
        key="business.entity_authority_licensing",
        label="Business Authority and Licensing",
        description="Jurisdiction-specific requirements for entity status, registered agent, annual reports, assumed names, licenses, permits, and renewals.",
        program_key="state_business_authority",
        compliance_key="ck_business_entity_authority_licensing",
        aliases=["annual report", "registered agent", "business license", "DBA"],
        citations=[
            CitationSpec("state_entity_statutes_overlay", "State entity statutes", "State corporation, LLC, assumed-name, and registered-agent statutes", "Jurisdiction-specific entity and registration source overlay."),
            CitationSpec("state_local_license_overlay", "State and local licensing", "State, county, and municipal business license and permit requirements", "Jurisdiction-specific business license and permit source overlay."),
        ],
        facts=[
            FactSpec("entity_status_current", "Entity status current", "Legal entity standing/status is current in the formation and qualification jurisdictions.", ["LedgArr", "RecordArr", "ReportArr"], ["legal_entity", "jurisdiction"], "state_entity_statutes_overlay", "entity_status", "external_registry", "", "per_jurisdiction_or_policy", "Is entity status current?", "critical", True, False),
            FactSpec("registered_agent_current", "Registered agent current", "Registered agent and registered office information are current where required.", ["LedgArr", "RecordArr"], ["legal_entity", "jurisdiction"], "state_entity_statutes_overlay", "registered_agent_record", "external_registry", "", "per_jurisdiction_or_policy", "Is registered agent information current?", "major"),
            FactSpec("business_license_renewals_current", "Business licenses current", "Required business licenses, permits, assumed names, and renewals are current for active jurisdictions and activities.", ["LedgArr", "RecordArr", "ReportArr"], ["legal_entity", "license", "site"], "state_local_license_overlay", "license_register", "document_record", "business_license", "per_license_or_permit", "Are business licenses and permits current?", "critical", True, False),
        ],
    ),
    PackSpec(
        key="tax.statutory_financial_obligations",
        label="Tax and Statutory Financial Obligations",
        description="Jurisdiction-specific registration, filing-calendar, payment, and statutory financial evidence requirements.",
        program_key="state_business_authority",
        compliance_key="ck_tax_statutory_financial_obligations",
        aliases=["tax calendar", "payroll tax", "sales tax", "franchise tax"],
        citations=[
            CitationSpec("irs_employment_tax_program", "Federal employment tax program", "IRS employment tax and information return requirements", "Federal payroll, information return, and tax deposit source family.", "https://www.irs.gov/businesses/small-businesses-self-employed/employment-taxes"),
            CitationSpec("state_tax_overlay", "State and local tax overlay", "State and local tax registration, filing, and payment requirements", "Jurisdiction-specific tax source overlay."),
        ],
        facts=[
            FactSpec("tax_registration_matrix_current", "Tax registrations current", "Federal, state, and local tax registrations are current for active legal entities and activities.", ["LedgArr", "StaffArr", "ReportArr"], ["legal_entity", "tax_registration"], "state_tax_overlay", "tax_registration", "external_registry", "", "per_tax_type_or_policy", "Are tax registrations current?", "critical", True, False),
            FactSpec("tax_filing_calendar_current", "Tax filing calendar current", "Tax return, information return, payment, and deposit due dates are maintained for active obligations.", ["LedgArr", "ReportArr"], ["legal_entity", "filing_calendar"], "irs_employment_tax_program", "filing_calendar", "system_fact", "", "per_tax_period", "Is the tax filing calendar current?", "critical", True, False),
            FactSpec("statutory_tax_payment_evidence_retained", "Tax payment evidence retained", "Payment/deposit confirmations and filed return evidence are retained.", ["LedgArr", "RecordArr"], ["tax_return", "payment"], "irs_employment_tax_program", "payment_record", "document_record", "tax_filing_or_payment", "per_tax_type_or_policy", "Is statutory tax evidence retained?", "major"),
        ],
    ),
    PackSpec(
        key="commercial.ucc_orders_warranties",
        label="UCC Orders, Documents of Title, and Warranties",
        description="Jurisdiction-specific UCC sales, leases, documents of title, secured transactions, warranty, rejection, acceptance, and cure controls.",
        program_key="state_business_authority",
        compliance_key="ck_commercial_ucc_orders_warranties",
        aliases=["UCC Article 2", "warranty", "warehouse receipt", "bill of lading"],
        citations=[
            CitationSpec("ucc_article_2_overlay", "State UCC Article 2 enactments", "State UCC Article 2 enactments", "Jurisdiction-specific sales-of-goods overlay.", "https://www.uniformlaws.org/acts/ucc"),
            CitationSpec("ucc_article_7_overlay", "State UCC Article 7 enactments", "State UCC Article 7 enactments", "Jurisdiction-specific documents-of-title overlay.", "https://www.uniformlaws.org/acts/ucc"),
            CitationSpec("ucc_article_9_overlay", "State UCC Article 9 enactments", "State UCC Article 9 enactments", "Jurisdiction-specific secured-transactions overlay.", "https://www.uniformlaws.org/acts/ucc"),
        ],
        facts=[
            FactSpec("ucc_order_terms_snapshot_retained", "Order terms snapshot retained", "Order terms, acceptance, rejection, cure, warranty, and title/risk snapshots are retained for goods transactions.", ["OrdArr", "CustomArr", "RecordArr"], ["order", "transaction"], "ucc_article_2_overlay", "order_terms_snapshot", "product_record", "", "contract_life_plus_policy", "Is the order terms snapshot retained?", "major"),
            FactSpec("ucc_document_of_title_controls_current", "Documents of title controls current", "Warehouse receipt, bill of lading, and document-of-title controls are current where used.", ["LoadArr", "RoutArr", "RecordArr"], ["shipment", "warehouse_receipt", "bill_of_lading"], "ucc_article_7_overlay", "document_of_title", "document_record", "document_of_title", "transaction_life_plus_policy", "Are document-of-title controls current?", "major"),
            FactSpec("ucc_secured_transaction_review_complete", "Secured transaction review complete", "Article 9 security interest facts are reviewed before creating or relying on secured-transaction documents.", ["LedgArr", "OrdArr", "RecordArr"], ["transaction", "security_interest"], "ucc_article_9_overlay", "secured_transaction_review", "system_fact", "", "transaction_life_plus_policy", "Was secured-transaction review completed?", "major"),
        ],
    ),
    PackSpec(
        key="consumer.accessibility_disclosures",
        label="Consumer Accessibility and Disclosures",
        description="Operational requirements for public-accommodation accessibility, effective communication, consumer disclosures, refunds, warranties, and auto-renewal overlays.",
        program_key="doj_accessibility",
        compliance_key="ck_consumer_accessibility_disclosures",
        aliases=["ADA Title III", "effective communication", "consumer disclosures", "refund policy"],
        citations=[
            CitationSpec("ada_title_iii_regs", "ADA Title III regulations", "28 CFR Part 36", "ADA Title III public accommodation regulations.", "https://www.ada.gov/law-and-regs/regulations/title-iii-regulations/"),
            CitationSpec("ada_effective_communication", "ADA effective communication guidance", "ADA.gov effective communication guidance", "Effective communication resource for Title II and Title III covered entities.", "https://www.ada.gov/resources/effective-communication/"),
            CitationSpec("state_consumer_protection_overlay", "State consumer protection overlay", "State UDAP, auto-renewal, refund, warranty, and disclosure laws", "Jurisdiction-specific consumer protection overlay."),
        ],
        facts=[
            FactSpec("ada_public_accommodation_review_current", "ADA applicability review current", "Customer-facing sites, services, and public accommodation facts are reviewed for ADA Title III applicability.", ["CustomArr", "STLComplianceSite", "StaffArr"], ["site", "customer_channel"], "ada_title_iii_regs", "accessibility_profile", "system_fact", "", "annual_or_site_change", "Is ADA public-accommodation review current?", "major"),
            FactSpec("ada_effective_communication_process_ready", "Effective communication process ready", "Effective communication/accommodation process is available for customer interactions.", ["CustomArr", "FieldCompanion", "RecordArr"], ["customer", "communication"], "ada_effective_communication", "accommodation_process", "product_record", "", "while_applicable_plus_policy", "Is the effective communication process ready?", "major"),
            FactSpec("consumer_disclosure_review_current", "Consumer disclosures current", "Refund, warranty, shipping, auto-renewal, and required consumer disclosures are reviewed for enabled sales channels and jurisdictions.", ["OrdArr", "CustomArr", "STLComplianceSite", "RecordArr"], ["order", "sales_channel"], "state_consumer_protection_overlay", "consumer_disclosure", "document_record", "consumer_disclosure", "per_offer_or_policy", "Are consumer disclosures current?", "major"),
        ],
    ),
    PackSpec(
        key="supplychain.trade_sanctions_import_product",
        label="Trade, Sanctions, Import, and Product Compliance Intake",
        description="Operational intake requirements for sanctions screening, customs/import facts, forced-labor controls, and product-compliance routing.",
        program_key="trade_sanctions_supply_chain",
        compliance_key="ck_supplychain_trade_sanctions_import_product",
        aliases=["OFAC", "customs", "UFLPA", "HTS", "import controls"],
        citations=[
            CitationSpec("ofac_sanctions_programs", "OFAC sanctions programs", "OFAC sanctions programs and SDN controls", "Sanctions screening source family.", "https://ofac.treasury.gov/sanctions-programs-and-country-information"),
            CitationSpec("cbp_importer_recordkeeping", "CBP importer recordkeeping", "19 CFR Part 163", "Customs importer recordkeeping source family.", "https://www.ecfr.gov/current/title-19/chapter-I/part-163"),
            CitationSpec("product_compliance_overlay", "Product compliance overlays", "CPSC, FDA, USDA, EPA, FCC, NHTSA, and state product compliance source families", "Product-specific compliance source overlay."),
        ],
        facts=[
            FactSpec("sanctions_screening_current", "Sanctions screening current", "Customer, vendor, supplier, and transaction counterparties are screened against applicable sanctions/blocked-party lists.", ["SupplyArr", "CustomArr", "OrdArr", "RecordArr"], ["party", "transaction"], "ofac_sanctions_programs", "screening_result", "external_registry", "", "per_transaction_or_policy", "Is sanctions screening current?", "critical", True, False),
            FactSpec("import_recordkeeping_packet_complete", "Import packet complete", "Importer-of-record, entry, classification, valuation, origin, and required customs records are complete and retained.", ["SupplyArr", "LoadArr", "RecordArr", "ReportArr"], ["import_entry", "item"], "cbp_importer_recordkeeping", "import_packet", "document_record", "import_entry_packet", "per_19_cfr_163_or_longer_if_required", "Is the import recordkeeping packet complete?", "critical", True, False),
            FactSpec("product_compliance_routing_complete", "Product compliance routing complete", "Product category, agency jurisdiction, labeling, testing, certification, recall, or restricted-product routing has been completed before sale/use.", ["SupplyArr", "AssurArr", "OrdArr", "RecordArr"], ["item", "product", "order"], "product_compliance_overlay", "product_compliance_review", "system_fact", "", "per_product_life_or_policy", "Is product compliance routing complete?", "critical", True, False),
        ],
    ),
]


def bool_str(value: bool) -> str:
    return "true" if value else "false"


def csv_escape_rows(rows: list[dict[str, str]], headers: list[str]) -> str:
    output = io.StringIO(newline="")
    writer = csv.DictWriter(output, fieldnames=headers, lineterminator="\n")
    writer.writeheader()
    for row in rows:
        writer.writerow({header: row.get(header, "") for header in headers})
    return output.getvalue()


def slug_hash(*parts: str) -> str:
    digest = hashlib.sha1(":".join(parts).encode("utf-8")).hexdigest()
    return digest[:8]


def compact_key(prefix: str, *parts: str, max_length: int = 64) -> str:
    key_part = "_".join(part.replace(".", "_") for part in parts if part)
    candidate = f"{prefix}_{key_part}" if key_part else prefix
    if len(candidate) <= max_length:
        return candidate

    digest = slug_hash(prefix, *parts)
    room = max_length - len(prefix) - len(digest) - 2
    if room <= 0:
        return f"{prefix[: max_length - len(digest) - 1]}_{digest}"

    trimmed = key_part[:room].rstrip("_")
    return f"{prefix}_{trimmed}_{digest}"


def rule_content(pack: PackSpec) -> str:
    if pack.reference_only:
        return ""

    rules = []
    conditions = []
    for fact in pack.facts:
        rules.append(
            {
                "ruleKey": f"r_{fact.key}",
                "factKey": fact.key,
                "label": fact.label,
                "type": "fact_boolean" if fact.value_type == "boolean" else "fact_value",
                "expectedValue": True if fact.expected_value.lower() == "true" else fact.expected_value,
                "nonWaivable": fact.automatic_failure_flag and not fact.override_allowed,
            }
        )
        conditions.append(
            {
                "conditionKey": f"c_{fact.key}",
                "ruleKey": f"r_{fact.key}",
                "factKey": fact.key,
                "operator": fact.operator,
                "expectedValue": True if fact.expected_value.lower() == "true" else fact.expected_value,
                "sourceProducts": fact.products,
                "entities": fact.entities,
            }
        )

    return json.dumps(
        {
            "schemaVersion": 1,
            "logic": "all",
            "rules": rules,
            "conditions": conditions,
            "outcomes": [
                {
                    "outcomeKey": f"outcome_allow_{pack.key.replace('.', '_')}",
                    "result": "allow",
                    "when": "all_conditions_pass",
                    "message": f"{pack.label} is satisfied.",
                },
                {
                    "outcomeKey": f"outcome_block_{pack.key.replace('.', '_')}",
                    "result": "block",
                    "when": "any_required_condition_fails",
                    "message": f"{pack.label} is not satisfied; product workflow must block, collect evidence, or require authorized review.",
                },
            ],
        },
        sort_keys=True,
        separators=(",", ":"),
    )


def build_rows(pack: PackSpec) -> dict[str, list[dict[str, str]]]:
    rows = {name: [] for name in CSV_HEADERS}

    rows["controlled_vocabulary.csv"].append(
        {
            "term_key": pack.compliance_key,
            "vocabulary_type_key": "compliance_domain",
            "label": pack.label,
            "description": f"Baseline compliance domain for {', '.join(sorted({product for fact in pack.facts for product in fact.products}))}.",
            "active": "true",
        }
    )
    for alias in pack.aliases:
        rows["vocabulary_aliases.csv"].append({"term_key": pack.compliance_key, "alias_text": alias, "active": "true"})

    rows["compliance_keys.csv"].append(
        {
            "key": pack.compliance_key,
            "label": pack.label,
            "category": "compliance_domain",
            "description": f"{pack.description} Mapping level: operational. Products: {', '.join(sorted({product for fact in pack.facts for product in fact.products}))}.",
            "active": "true",
        }
    )

    rows["rule_packs.csv"].append(
        {
            "pack_key": pack.key,
            "program_key": pack.program_key,
            "version_number": "1",
            "label": pack.label,
            "description": f"{pack.description} {SOURCE_NOTE}",
            "status": "published",
            "active": "true",
            "rule_content_json": rule_content(pack),
        }
    )

    for citation in pack.citations:
        source = citation.source_reference
        if citation.url:
            source = f"{source}; Source: {citation.url}"
        rows["rule_requirements.csv"].append(
            {
                "citation_key": citation.key,
                "program_key": pack.program_key,
                "pack_key": pack.key,
                "pack_version": "1",
                "label": citation.label,
                "source_reference": citation.source_reference,
                "description": f"{citation.description} {SOURCE_NOTE}{' Source: ' + citation.url if citation.url else ''}",
                "active": "true",
                "supersedes_citation_key": "",
            }
        )
        rows["regulatory_mappings.csv"].append(
            {
                "mapping_key": compact_key("map", pack.key, citation.key),
                "target_kind": "compliance_key",
                "program_key": pack.program_key,
                "pack_key": pack.key,
                "pack_version": "1",
                "citation_key": citation.key,
                "compliance_key": pack.compliance_key,
                "material_key": "",
                "fact_key": "",
                "label": f"{pack.label} to {citation.source_reference}",
                "description": f"Maps {citation.source_reference} to {pack.key}.",
                "active": "true",
            }
        )

    for fact in pack.facts:
        requirement_key = f"fr_{pack.key.replace('.', '_')}_{slug_hash(pack.key, fact.key)}"
        description = (
            f"{fact.description} products={','.join(fact.products)}; entities={','.join(fact.entities)}; "
            f"value_type={fact.value_type}; operator={fact.operator}; expected_value={fact.expected_value}; "
            f"evidence_kind={fact.evidence_kind}; required_document_type={fact.required_document_type or 'none'}; "
            f"retention_period={fact.retention_period}; failure_severity={fact.failure_severity}; "
            f"automatic_failure={bool_str(fact.automatic_failure_flag)}; override_allowed={bool_str(fact.override_allowed)}."
        )
        rows["rule_fact_requirements.csv"].append(
            {
                "requirement_key": requirement_key,
                "fact_key": fact.key,
                "pack_key": pack.key,
                "pack_version": "1",
                "citation_key": fact.citation_key,
                "citation_version": "1",
                "applicability_key": pack.compliance_key,
                "source_product": ",".join(fact.products),
                "source_entity": ",".join(fact.entities),
                "source_field_or_record_type": fact.source_field_or_record_type,
                "value_type": fact.value_type,
                "operator": fact.operator,
                "expected_value": fact.expected_value,
                "evidence_kind": fact.evidence_kind,
                "required_document_type": fact.required_document_type,
                "retention_period": fact.retention_period,
                "audit_question": fact.audit_question or f"Is {fact.label.lower()} satisfied?",
                "failure_severity": fact.failure_severity,
                "automatic_failure_flag": bool_str(fact.automatic_failure_flag),
                "override_allowed": bool_str(fact.override_allowed),
                "override_permission": fact.override_permission,
                "remediation_required": bool_str(fact.remediation_required),
                "label": fact.label,
                "description": description,
                "is_required": bool_str(fact.is_required),
                "active": "true",
            }
        )
        rows["regulatory_mappings.csv"].append(
            {
                "mapping_key": compact_key("map", pack.key, fact.key),
                "target_kind": "fact_key",
                "program_key": pack.program_key,
                "pack_key": pack.key,
                "pack_version": "1",
                "citation_key": fact.citation_key,
                "compliance_key": pack.compliance_key,
                "material_key": "",
                "fact_key": fact.key,
                "label": f"{pack.label} to {fact.label}",
                "description": f"Maps {fact.label} fact to {pack.key}.",
                "active": "true",
            }
        )

    for exception in pack.exceptions:
        rows["exception_exemptions.csv"].append(
            {
                "key": exception.key,
                "label": exception.label,
                "type": exception.type,
                "governing_body": PROGRAMS[pack.program_key]["body_key"],
                "program_key": pack.program_key,
                "pack_key": pack.key,
                "citation_key": exception.citation_key,
                "applicability_key": pack.compliance_key,
                "applies_to_subject_kind": exception.subject_kind,
                "applies_to_source_product": ",".join(exception.products),
                "applies_to_source_entity": ",".join(exception.entities),
                "effect_type": exception.effect_type,
                "condition_logic_json": json.dumps(exception.condition_logic, sort_keys=True, separators=(",", ":")),
                "required_evidence_option_group_key": "",
                "issuing_authority": PROGRAMS[pack.program_key]["body_label"],
                "authorization_number": "",
                "effective_at": "",
                "expires_at": "",
                "active": "true",
                "description": exception.description,
            }
        )

    return rows


def write_manifest(out_root: Path) -> None:
    manifest = {
        "schemaVersion": 1,
        "source": "Compliance Core import schemas",
        "generatedFor": "baseline",
        "sourceDate": SOURCE_DATE,
        "programs": PROGRAMS,
        "directBundleFiles": [
            {"fileName": name, "headers": headers}
            for name, headers in CSV_HEADERS.items()
            if name != "evidence_references.csv"
        ],
        "stagedOnlyFiles": [{"fileName": "evidence_references.csv", "headers": CSV_HEADERS["evidence_references.csv"]}],
        "packs": [{"packKey": pack.key, "programKey": pack.program_key, "label": pack.label} for pack in PACKS],
    }
    (out_root / "manifest.json").write_text(json.dumps(manifest, indent=2) + "\n", encoding="utf-8")


def write_docs(repo_root: Path) -> None:
    docs_dir = repo_root / "docs" / "compliance-core"
    docs_dir.mkdir(parents=True, exist_ok=True)
    lines = [
        "# Baseline rulepack index",
        "",
        f"Generated from `tools/compliancecore/generate_baseline_rulepacks.py` on source date {SOURCE_DATE}.",
        "",
        "These packs cover the non-Title-49 operational baseline. Title 49/FMCSA/PHMSA packs remain in `root/rulepack/title49`.",
        "",
        "| Rulepack | Program | Primary products | Requirements |",
        "|---|---|---|---|",
    ]
    for pack in PACKS:
        products = ", ".join(sorted({product for fact in pack.facts for product in fact.products}))
        lines.append(f"| `{pack.key}` | `{pack.program_key}` | {products} | {len(pack.facts)} |")
    lines.append("")
    lines.append("State and local overlays intentionally carry jurisdiction-specific source references. They must be refined into state/local versions before tenant-specific legal conclusions are treated as final.")
    (docs_dir / "baseline_rulepack_index.md").write_text("\n".join(lines) + "\n", encoding="utf-8")


def write_import_script(repo_root: Path) -> None:
    path = repo_root / "tools" / "compliancecore" / "import-baseline-rulepacks.ps1"
    program_rows = []
    for key, value in PROGRAMS.items():
        program_rows.append(
            f'    @{{ Key = "{key}"; BodyKey = "{value["body_key"]}"; BodyLabel = "{value["body_label"]}"; JurisdictionKey = "{value["jurisdiction_key"]}"; Label = "{value["label"]}"; Description = "{value["description"]}" }}'
        )
    script = f'''param(
    [Parameter(Mandatory = $true)]
    [string]$ComplianceCoreBaseUrl,

    [Parameter(Mandatory = $true)]
    [string]$AccessToken,

    [string]$RulePackRoot = "",

    [switch]$DryRun
)

$ErrorActionPreference = "Stop"

if ([string]::IsNullOrWhiteSpace($RulePackRoot)) {{
    $ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
    $RepoRoot = Resolve-Path (Join-Path $ScriptDir "..\\..")
    $RulePackRoot = Join-Path $RepoRoot "root\\rulepack\\baseline"
}}

$RulePackRoot = (Resolve-Path $RulePackRoot).Path
$BaseUrl = $ComplianceCoreBaseUrl.TrimEnd("/")
$Headers = @{{ Authorization = "Bearer $AccessToken" }}

$CsvNames = @(
    "controlled_vocabulary.csv",
    "vocabulary_aliases.csv",
    "compliance_keys.csv",
    "material_keys.csv",
    "rule_packs.csv",
    "rule_requirements.csv",
    "rule_fact_requirements.csv",
    "regulatory_mappings.csv",
    "sds_references.csv",
    "exception_exemptions.csv",
    "evidence_references.csv"
)

$Programs = @(
{chr(10).join(program_rows)}
)

function Invoke-ComplianceJson {{
    param(
        [Parameter(Mandatory = $true)]
        [string]$Method,

        [Parameter(Mandatory = $true)]
        [string]$Path,

        [object]$Body = $null
    )

    $Uri = "$BaseUrl$Path"
    if ($null -eq $Body) {{
        return Invoke-RestMethod -Method $Method -Uri $Uri -Headers $Headers
    }}

    return Invoke-RestMethod -Method $Method -Uri $Uri -Headers $Headers -ContentType "application/json" -Body ($Body | ConvertTo-Json -Depth 20)
}}

function Ensure-RegulatorySpine {{
    $Bodies = Invoke-ComplianceJson -Method Get -Path "/api/governing-bodies"
    $Jurisdictions = Invoke-ComplianceJson -Method Get -Path "/api/jurisdictions"
    $ExistingPrograms = Invoke-ComplianceJson -Method Get -Path "/api/regulatory-programs"

    foreach ($Program in $Programs) {{
        $Body = $Bodies | Where-Object {{ $_.bodyKey -eq $Program.BodyKey }} | Select-Object -First 1
        if ($null -eq $Body) {{
            if ($DryRun) {{
                Write-Host "DRY RUN: would create governing body $($Program.BodyKey)"
                continue
            }}
            $Body = Invoke-ComplianceJson -Method Post -Path "/api/governing-bodies" -Body @{{
                bodyKey = $Program.BodyKey
                label = $Program.BodyLabel
                description = "Baseline rulepack governing body."
            }}
            $Bodies += $Body
        }}

        $Jurisdiction = $Jurisdictions | Where-Object {{ $_.jurisdictionKey -eq $Program.JurisdictionKey }} | Select-Object -First 1
        if ($null -eq $Jurisdiction) {{
            if ($DryRun) {{
                Write-Host "DRY RUN: would create jurisdiction $($Program.JurisdictionKey)"
                continue
            }}
            $Jurisdiction = Invoke-ComplianceJson -Method Post -Path "/api/jurisdictions" -Body @{{
                governingBodyId = $Body.governingBodyId
                jurisdictionKey = $Program.JurisdictionKey
                label = $Program.Label
                description = $Program.Description
            }}
            $Jurisdictions += $Jurisdiction
        }}

        $Existing = $ExistingPrograms | Where-Object {{ $_.programKey -eq $Program.Key }} | Select-Object -First 1
        if ($null -ne $Existing) {{
            continue
        }}

        if ($DryRun) {{
            Write-Host "DRY RUN: would create regulatory program $($Program.Key)"
        }} else {{
            $Created = Invoke-ComplianceJson -Method Post -Path "/api/regulatory-programs" -Body @{{
                jurisdictionId = $Jurisdiction.jurisdictionId
                programKey = $Program.Key
                label = $Program.Label
                description = $Program.Description
            }}
            $ExistingPrograms += $Created
        }}
    }}
}}

function Import-RulePackBundles {{
    $Endpoint = if ($DryRun) {{ "/api/v1/rule-pack-imports/validate" }} else {{ "/api/v1/rule-pack-imports/publish-draft" }}
    foreach ($PackDir in Get-ChildItem -Path $RulePackRoot -Directory | Sort-Object Name) {{
        $Form = @{{}}
        $Index = 0
        foreach ($CsvName in $CsvNames) {{
            $Path = Join-Path $PackDir.FullName $CsvName
            if (-not (Test-Path $Path)) {{
                throw "Missing $CsvName in $($PackDir.FullName)"
            }}
            $Form["file$Index"] = Get-Item $Path
            $Index++
        }}

        Write-Host "Importing $($PackDir.Name) via $Endpoint"
        $Result = Invoke-RestMethod -Method Post -Uri "$BaseUrl$Endpoint" -Headers $Headers -Form $Form
        if ($Result.result.issues.Count -gt 0) {{
            $Result.result.issues | ConvertTo-Json -Depth 10
            throw "Import validation failed for $($PackDir.Name)"
        }}
    }}
}}

Ensure-RegulatorySpine
Import-RulePackBundles

if ($DryRun) {{
    Write-Host "Baseline rule-pack dry-run validation completed."
}} else {{
    Write-Host "Baseline rule-pack import completed."
}}
'''
    path.write_text(script, encoding="utf-8")


def validate(out_root: Path) -> list[str]:
    issues: list[str] = []
    if not out_root.exists():
        return [f"Missing baseline rulepack root: {out_root}"]

    pack_keys = set()
    citation_keys = set()
    fact_keys = set()
    requirement_keys = set()
    mapping_keys = set()
    compliance_keys = set()
    for pack_dir in sorted(path for path in out_root.iterdir() if path.is_dir()):
        for file_name, headers in CSV_HEADERS.items():
            path = pack_dir / file_name
            if not path.exists():
                issues.append(f"{pack_dir.name}: missing {file_name}")
                continue
            with path.open(newline="", encoding="utf-8") as handle:
                reader = csv.DictReader(handle)
                if reader.fieldnames != headers:
                    issues.append(f"{pack_dir.name}/{file_name}: header mismatch")
                    continue
                rows = list(reader)
            if file_name == "rule_packs.csv":
                if len(rows) != 1:
                    issues.append(f"{pack_dir.name}: expected exactly one rule_packs.csv row")
                for row in rows:
                    pack_keys.add(row["pack_key"])
                    if row["pack_key"] != pack_dir.name:
                        issues.append(f"{pack_dir.name}: pack key does not match directory")
            elif file_name == "rule_requirements.csv":
                for row in rows:
                    citation_keys.add(row["citation_key"])
            elif file_name == "rule_fact_requirements.csv":
                for row in rows:
                    if row["requirement_key"] in requirement_keys:
                        issues.append(f"duplicate requirement key {row['requirement_key']}")
                    requirement_keys.add(row["requirement_key"])
                    fact_keys.add(row["fact_key"])
                    if row["citation_key"] and row["citation_key"] not in citation_keys:
                        issues.append(f"{pack_dir.name}: fact {row['fact_key']} references unknown citation {row['citation_key']}")
            elif file_name == "regulatory_mappings.csv":
                for row in rows:
                    if row["mapping_key"] in mapping_keys:
                        issues.append(f"duplicate mapping key {row['mapping_key']}")
                    mapping_keys.add(row["mapping_key"])
            elif file_name == "compliance_keys.csv":
                for row in rows:
                    compliance_keys.add(row["key"])

    expected = {pack.key for pack in PACKS}
    if pack_keys != expected:
        issues.append(f"pack key mismatch: expected {sorted(expected)}, found {sorted(pack_keys)}")
    return issues


def generate(repo_root: Path) -> None:
    out_root = repo_root / "root" / "rulepack" / "baseline"
    resolved = out_root.resolve()
    expected = (repo_root / "root" / "rulepack").resolve()
    if expected not in resolved.parents:
        raise RuntimeError(f"Refusing to write outside rulepack root: {resolved}")
    if out_root.exists():
        shutil.rmtree(out_root)
    out_root.mkdir(parents=True)

    for pack in PACKS:
        pack_dir = out_root / pack.key
        pack_dir.mkdir()
        rows = build_rows(pack)
        for file_name, headers in CSV_HEADERS.items():
            (pack_dir / file_name).write_text(csv_escape_rows(rows[file_name], headers), encoding="utf-8")

    write_manifest(out_root)
    write_docs(repo_root)
    write_import_script(repo_root)


def main(argv: list[str] | None = None) -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--validate-only", action="store_true")
    args = parser.parse_args(argv)

    repo_root = Path(__file__).resolve().parents[2]
    out_root = repo_root / "root" / "rulepack" / "baseline"
    if not args.validate_only:
        generate(repo_root)

    issues = validate(out_root)
    if issues:
        print("Baseline rulepack validation failed:", file=sys.stderr)
        for issue in issues:
            print(f"- {issue}", file=sys.stderr)
        return 1
    print("Baseline rulepack validation passed.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
