#!/usr/bin/env python3
"""Generate Compliance Core Title 49 rule-pack CSV bundles and docs."""

from __future__ import annotations

import argparse
import csv
import hashlib
import io
import json
import os
import re
import shutil
import sys
import textwrap
import unicodedata
import urllib.request
from collections import Counter, defaultdict
from dataclasses import dataclass, field
from pathlib import Path
from typing import Any


SOURCE_DATE = "2026-05-28"
TITLE_URL = "https://www.ecfr.gov/current/title-49"
TITLES_URL = "https://www.ecfr.gov/api/versioner/v1/titles.json"
STRUCTURE_URL = f"https://www.ecfr.gov/api/versioner/v1/structure/{SOURCE_DATE}/title-49.json"
FULL_XML_URL = f"https://www.ecfr.gov/api/versioner/v1/full/{SOURCE_DATE}/title-49.xml"
GPO_XML_URL = "https://www.govinfo.gov/bulkdata/ECFR/title-49/ECFR-title49.xml"
FMCSA_URL = "https://www.fmcsa.dot.gov/regulations/49-cfr-parts-300-399"
PHMSA_HMR_URL = "https://www.phmsa.dot.gov/regulations/title49/part/172"
DOT_PART40_URL = "https://www.transportation.gov/odapc/part40"

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
}

PROGRAMS = {
    "dot_part40": "DOT Part 40 Drug and Alcohol Testing Procedures",
    "dot_title49_metadata": "DOT Title 49 Citation Metadata",
    "fmcsa_fmcsr": "FMCSA Federal Motor Carrier Safety Regulations",
    "fmcsa_passenger": "FMCSA Passenger Carrier Operations",
    "fmcsa_household_goods": "FMCSA Household Goods Consumer Protection",
    "fmcsa_intermodal": "FMCSA Intermodal Equipment Provider",
    "phmsa_hmr": "PHMSA Hazardous Materials Regulations",
    "phmsa_pipeline": "PHMSA Pipeline Safety",
    "fra_rail": "FRA Rail Safety",
    "fta_transit": "FTA Transit Safety",
    "nhtsa_vehicle": "NHTSA Vehicle Standards",
    "tsa_security": "TSA Transportation Security",
    "stb_surface": "Surface Transportation Board",
}

PRODUCTS = {"StaffArr", "TrainArr", "MaintainArr", "RoutArr", "SupplyArr", "ComplianceCore"}


@dataclass
class FactSpec:
    key: str
    label: str
    description: str
    products: list[str]
    entities: list[str]
    citations: list[str]
    non_waivable: bool = False
    source_field_or_record_type: str = ""
    value_type: str = "boolean"
    operator: str = "equals"
    expected_value: str = "true"
    evidence_kind: str = "product_record"
    required_document_type: str = ""
    retention_period: str = "per_citation_or_company_policy"
    audit_question: str = ""
    failure_severity: str = ""
    automatic_failure_flag: bool | None = None
    override_allowed: bool | None = None
    override_permission: str = ""
    remediation_required: bool = True
    derived: bool = False


@dataclass
class PackSpec:
    key: str
    label: str
    program_key: str
    level: str
    description: str
    products: list[str]
    entities: list[str]
    parts: set[str] = field(default_factory=set)
    part_ranges: list[tuple[int, int]] = field(default_factory=list)
    subparts: set[str] = field(default_factory=set)
    sections: set[str] = field(default_factory=set)
    section_prefixes: list[str] = field(default_factory=list)
    exclude_subparts: set[str] = field(default_factory=set)
    exclude_sections: set[str] = field(default_factory=set)
    facts: list[FactSpec] = field(default_factory=list)
    manual_review: list[str] = field(default_factory=list)


def ascii_text(value: Any) -> str:
    text = "" if value is None else str(value)
    replacements = {
        "\u2014": "-",
        "\u2013": "-",
        "\u2018": "'",
        "\u2019": "'",
        "\u201c": '"',
        "\u201d": '"',
        "\u00a0": " ",
        "\u00a7": "Sec.",
        "&amp;": "&",
    }
    for source, target in replacements.items():
        text = text.replace(source, target)
    text = unicodedata.normalize("NFKD", text)
    text = text.encode("ascii", "ignore").decode("ascii")
    return re.sub(r"\s+", " ", text).strip()


def trim(value: str, limit: int) -> str:
    value = ascii_text(value)
    return value if len(value) <= limit else value[: limit - 3].rstrip() + "..."


def slug(value: str) -> str:
    text = ascii_text(value).lower()
    text = re.sub(r"[^a-z0-9]+", "_", text).strip("_")
    return text or "x"


def short_key(prefix: str, value: str, limit: int = 64) -> str:
    candidate = f"{prefix}_{slug(value)}"
    if len(candidate) <= limit:
        return candidate
    digest = hashlib.sha1(candidate.encode("utf-8")).hexdigest()[:8]
    return f"{candidate[: limit - 9].rstrip('_')}_{digest}"


def fact(
    key: str,
    label: str,
    description: str,
    products: list[str],
    entities: list[str],
    citations: list[str],
    non_waivable: bool = False,
    *,
    source_field_or_record_type: str = "",
    value_type: str = "boolean",
    operator: str = "equals",
    expected_value: str = "true",
    evidence_kind: str = "product_record",
    required_document_type: str = "",
    retention_period: str = "per_citation_or_company_policy",
    audit_question: str = "",
    failure_severity: str = "",
    automatic_failure_flag: bool | None = None,
    override_allowed: bool | None = None,
    override_permission: str = "",
    remediation_required: bool = True,
    derived: bool = False,
) -> FactSpec:
    severity = failure_severity or ("critical" if non_waivable else "major")
    automatic_failure = non_waivable if automatic_failure_flag is None else automatic_failure_flag
    can_override = (not non_waivable) if override_allowed is None else override_allowed
    if not override_permission and can_override:
        override_permission = "compliance.override.title49"
    return FactSpec(
        key=key,
        label=label,
        description=description,
        products=products,
        entities=entities,
        citations=citations,
        non_waivable=non_waivable,
        source_field_or_record_type=source_field_or_record_type or (entities[-1] if entities else "compliance_fact"),
        value_type=value_type,
        operator=operator,
        expected_value=expected_value,
        evidence_kind=evidence_kind,
        required_document_type=required_document_type,
        retention_period=retention_period,
        audit_question=audit_question or f"Is {label.lower()}?",
        failure_severity=severity,
        automatic_failure_flag=automatic_failure,
        override_allowed=can_override,
        override_permission=override_permission,
        remediation_required=remediation_required,
        derived=derived,
    )


def pack(
    key: str,
    label: str,
    program_key: str,
    level: str,
    description: str,
    products: list[str],
    entities: list[str],
    **selectors: Any,
) -> PackSpec:
    return PackSpec(
        key=key,
        label=label,
        program_key=program_key,
        level=level,
        description=description,
        products=products,
        entities=entities,
        parts=set(selectors.get("parts", [])),
        part_ranges=selectors.get("part_ranges", []),
        subparts=set(selectors.get("subparts", [])),
        sections=set(selectors.get("sections", [])),
        section_prefixes=selectors.get("section_prefixes", []),
        exclude_subparts=set(selectors.get("exclude_subparts", [])),
        exclude_sections=set(selectors.get("exclude_sections", [])),
        manual_review=selectors.get("manual_review", []),
    )


PACKS: list[PackSpec] = [
    pack(
        "title49.motorcarrier.registration_authority",
        "Title 49 Motor Carrier Registration and Authority",
        "fmcsa_fmcsr",
        "operational",
        "Operating authority, process agent, state registration, and registration update controls.",
        ["StaffArr", "RoutArr", "ComplianceCore"],
        ["carrier", "authority_record", "registration"],
        parts=["365", "366", "367", "368"],
        subparts={"390:E-suspended"},
    ),
    pack(
        "title49.motorcarrier.insurance_financial_responsibility",
        "Title 49 Motor Carrier Financial Responsibility",
        "fmcsa_fmcsr",
        "operational",
        "Minimum financial responsibility and proof-of-insurance controls for regulated motor carriers.",
        ["StaffArr", "RoutArr", "SupplyArr", "ComplianceCore"],
        ["carrier", "insurance_policy", "shipment"],
        parts=["387"],
    ),
    pack(
        "title49.driver.drug_alcohol_program",
        "Title 49 Driver Drug and Alcohol Program",
        "dot_part40",
        "operational",
        "DOT Part 40 and FMCSA Part 382 drug and alcohol testing program controls.",
        ["StaffArr", "TrainArr", "RoutArr", "ComplianceCore"],
        ["driver", "test_result", "clearinghouse_query", "program"],
        parts=["40", "382"],
    ),
    pack(
        "title49.driver.cdl_clp_endorsements",
        "Title 49 CDL, CLP, and Endorsements",
        "fmcsa_fmcsr",
        "operational",
        "Commercial driver license, permit, disqualification, and endorsement readiness controls.",
        ["StaffArr", "TrainArr", "RoutArr", "ComplianceCore"],
        ["driver", "license", "endorsement"],
        parts=["383"],
    ),
    pack(
        "title49.driver.entry_level_driver_training",
        "Title 49 Entry-Level Driver Training",
        "fmcsa_fmcsr",
        "operational",
        "Entry-level driver training, provider registry, and curriculum completion controls.",
        ["TrainArr", "StaffArr", "RoutArr", "ComplianceCore"],
        ["driver", "training_assignment", "training_provider"],
        parts=["380"],
    ),
    pack(
        "title49.driver.medical_qualification",
        "Title 49 Driver Medical Qualification",
        "fmcsa_fmcsr",
        "operational",
        "Driver physical qualification, medical certificate, and examiner registry controls.",
        ["StaffArr", "TrainArr", "RoutArr", "ComplianceCore"],
        ["driver", "medical_certificate", "examiner"],
        subparts={"391:E", "390:D"},
        sections={"391.41", "391.43", "391.45", "391.46", "391.47", "391.49", "390.101"},
    ),
    pack(
        "title49.driver.qualification_file",
        "Title 49 Driver Qualification File",
        "fmcsa_fmcsr",
        "operational",
        "Driver qualification, application, investigation, review, and file retention controls.",
        ["StaffArr", "TrainArr", "RoutArr", "ComplianceCore"],
        ["driver", "qualification_file", "mvr"],
        subparts={"391:A", "391:B", "391:C", "391:D", "391:F", "391:G"},
        parts=["391"],
    ),
    pack(
        "title49.driver.hours_of_service",
        "Title 49 Hours of Service",
        "fmcsa_fmcsr",
        "operational",
        "Hours-of-service limit, log, supporting-document, and out-of-service controls.",
        ["RoutArr", "StaffArr", "ComplianceCore"],
        ["driver", "trip", "duty_status", "hos_log"],
        subparts={"395:A"},
    ),
    pack(
        "title49.driver.eld_records",
        "Title 49 ELD Records",
        "fmcsa_fmcsr",
        "operational",
        "Electronic logging device registration, transfer, malfunction, and record availability controls.",
        ["RoutArr", "StaffArr", "ComplianceCore"],
        ["driver", "eld_device", "eld_record"],
        subparts={"395:B"},
    ),
    pack(
        "title49.driver.accident_post_accident_actions",
        "Title 49 Accident and Post-Accident Actions",
        "fmcsa_fmcsr",
        "operational",
        "Accident register, post-accident testing, and corrective-action evidence controls.",
        ["StaffArr", "RoutArr", "ComplianceCore"],
        ["incident", "driver", "accident_register", "test_decision"],
        sections={"390.15", "382.303", "40.191", "177.854"},
    ),
    pack(
        "title49.motorcarrier.applicability",
        "Title 49 Motor Carrier Applicability",
        "fmcsa_fmcsr",
        "operational",
        "FMCSR applicability, definitions, marking, records, and operating-readiness controls.",
        ["StaffArr", "RoutArr", "MaintainArr", "ComplianceCore"],
        ["carrier", "driver", "vehicle", "dispatch"],
        parts=["390", "392"],
        exclude_subparts={"390:C", "390:D", "390:E-suspended", "390:G"},
    ),
    pack(
        "title49.vehicle.cargo_securement",
        "Title 49 Cargo Securement",
        "fmcsa_fmcsr",
        "operational",
        "Cargo securement controls under Part 393 Subpart I.",
        ["MaintainArr", "RoutArr", "SupplyArr", "ComplianceCore"],
        ["vehicle", "load", "securement_device"],
        subparts={"393:I"},
    ),
    pack(
        "title49.vehicle.parts_accessories_condition",
        "Title 49 Vehicle Parts and Accessories Condition",
        "fmcsa_fmcsr",
        "operational",
        "CMV parts and accessories readiness controls excluding cargo securement.",
        ["MaintainArr", "RoutArr", "ComplianceCore"],
        ["vehicle", "asset", "defect"],
        parts=["393"],
    ),
    pack(
        "title49.vehicle.dvir",
        "Title 49 Driver Vehicle Inspection Reports",
        "fmcsa_fmcsr",
        "operational",
        "DVIR submission, defect certification, and intermodal report acceptance controls.",
        ["MaintainArr", "RoutArr", "StaffArr", "ComplianceCore"],
        ["vehicle", "dvir", "defect", "repair"],
        sections={"396.11", "396.12"},
    ),
    pack(
        "title49.vehicle.annual_inspection",
        "Title 49 Annual Inspection",
        "fmcsa_fmcsr",
        "operational",
        "Periodic inspection, inspector qualification, brake inspector, and recordkeeping controls.",
        ["MaintainArr", "RoutArr", "ComplianceCore"],
        ["vehicle", "inspection", "inspector"],
        sections={"396.17", "396.19", "396.21", "396.23", "396.25"},
    ),
    pack(
        "title49.vehicle.roadside_inspection_correction",
        "Title 49 Roadside Inspection Correction",
        "fmcsa_fmcsr",
        "operational",
        "Roadside inspection defect correction, return-to-service, and report closeout controls.",
        ["MaintainArr", "RoutArr", "ComplianceCore"],
        ["vehicle", "roadside_inspection", "defect"],
        sections={"396.9"},
    ),
    pack(
        "title49.vehicle.out_of_service_readiness",
        "Title 49 Out-of-Service Readiness",
        "fmcsa_fmcsr",
        "operational",
        "Controls that prevent dispatch while vehicle, driver, or carrier out-of-service orders are active.",
        ["MaintainArr", "RoutArr", "StaffArr", "ComplianceCore"],
        ["vehicle", "driver", "dispatch", "out_of_service_order"],
        sections={"390.13", "392.7", "392.9a", "395.13", "396.7"},
    ),
    pack(
        "title49.vehicle.inspection_repair_maintenance",
        "Title 49 Inspection, Repair, and Maintenance",
        "fmcsa_fmcsr",
        "operational",
        "Inspection, repair, maintenance, lubrication, pretrip, and driveaway-towaway controls.",
        ["MaintainArr", "RoutArr", "ComplianceCore"],
        ["vehicle", "asset", "work_order", "inspection"],
        parts=["396"],
    ),
    pack(
        "title49.hazmat.applicability",
        "Title 49 Hazmat Applicability",
        "phmsa_hmr",
        "operational",
        "HMR applicability, definitions, forbidden shipments, and international standards controls.",
        ["SupplyArr", "RoutArr", "TrainArr", "ComplianceCore"],
        ["material", "shipment", "hazmat_function"],
        subparts={"171:A", "171:C"},
    ),
    pack(
        "title49.hazmat.incident_reporting",
        "Title 49 Hazmat Incident Reporting",
        "phmsa_hmr",
        "operational",
        "Immediate notice, written incident report, and BOE approval controls.",
        ["StaffArr", "SupplyArr", "RoutArr", "ComplianceCore"],
        ["incident", "shipment", "hazmat_release"],
        subparts={"171:B"},
    ),
    pack(
        "title49.hazmat.hazardous_materials_table",
        "Title 49 Hazardous Materials Table",
        "phmsa_hmr",
        "operational",
        "Hazardous Materials Table lookup, proper shipping name, and special provision controls.",
        ["SupplyArr", "RoutArr", "ComplianceCore"],
        ["material", "shipment", "shipping_description"],
        subparts={"172:B"},
    ),
    pack(
        "title49.hazmat.shipping_papers",
        "Title 49 Hazmat Shipping Papers",
        "phmsa_hmr",
        "operational",
        "Shipping paper description, certification, emergency response, and retention controls.",
        ["SupplyArr", "RoutArr", "ComplianceCore"],
        ["shipment", "shipping_paper", "emergency_response_info"],
        subparts={"172:C", "172:G"},
    ),
    pack(
        "title49.hazmat.marking",
        "Title 49 Hazmat Marking",
        "phmsa_hmr",
        "operational",
        "Package, bulk packaging, marine pollutant, lithium battery, and limited quantity marking controls.",
        ["SupplyArr", "RoutArr", "ComplianceCore"],
        ["package", "shipment", "marking"],
        subparts={"172:D"},
    ),
    pack(
        "title49.hazmat.labeling",
        "Title 49 Hazmat Labeling",
        "phmsa_hmr",
        "operational",
        "Hazmat label application, specifications, placement, and hazard communication controls.",
        ["SupplyArr", "RoutArr", "ComplianceCore"],
        ["package", "shipment", "label"],
        subparts={"172:E"},
    ),
    pack(
        "title49.hazmat.placarding",
        "Title 49 Hazmat Placarding",
        "phmsa_hmr",
        "operational",
        "Placard applicability, tables, display, visibility, and specification controls.",
        ["SupplyArr", "RoutArr", "ComplianceCore"],
        ["vehicle", "shipment", "placard"],
        subparts={"172:F"},
    ),
    pack(
        "title49.hazmat.training",
        "Title 49 Hazmat Training",
        "phmsa_hmr",
        "operational",
        "Hazmat employee training, recurrent training, and training record controls.",
        ["TrainArr", "StaffArr", "SupplyArr", "RoutArr", "ComplianceCore"],
        ["worker", "training_record", "hazmat_employee"],
        subparts={"172:H"},
        sections={"380.Appendix E"},
    ),
    pack(
        "title49.hazmat.security_plan",
        "Title 49 Hazmat Security Plan",
        "phmsa_hmr",
        "operational",
        "Hazmat security plan, route analysis, rail routing, and security-sensitive shipment controls.",
        ["SupplyArr", "RoutArr", "StaffArr", "ComplianceCore"],
        ["shipment", "route", "security_plan"],
        subparts={"172:I"},
    ),
    pack(
        "title49.hazmat.classification",
        "Title 49 Hazmat Classification",
        "phmsa_hmr",
        "operational",
        "Hazard class, packing group, exception, and shipper classification controls.",
        ["SupplyArr", "TrainArr", "RoutArr", "ComplianceCore"],
        ["material", "sds", "classification"],
        subparts={"173:A", "173:C", "173:D", "173:I"},
    ),
    pack(
        "title49.hazmat.packaging",
        "Title 49 Hazmat Packaging",
        "phmsa_hmr",
        "operational",
        "Authorized packaging, specification packaging, retesting, and requalification controls.",
        ["SupplyArr", "MaintainArr", "RoutArr", "ComplianceCore"],
        ["material", "package", "container", "asset"],
        parts=["178", "180"],
        subparts={"173:B", "173:E", "173:F", "173:G"},
    ),
    pack(
        "title49.hazmat.loading_unloading_segregation",
        "Title 49 Hazmat Loading, Unloading, and Segregation",
        "phmsa_hmr",
        "operational",
        "Highway carriage, loading, unloading, attendance, segregation, routing, and parking controls.",
        ["SupplyArr", "RoutArr", "MaintainArr", "ComplianceCore"],
        ["shipment", "vehicle", "load", "route"],
        parts=["177", "397"],
    ),
    pack(
        "title49.hazmat.registration",
        "Title 49 Hazmat Registration",
        "phmsa_hmr",
        "operational",
        "PHMSA hazmat registration for offerors and transporters requiring registration.",
        ["SupplyArr", "RoutArr", "StaffArr", "ComplianceCore"],
        ["carrier", "shipper", "registration"],
        subparts={"107:G"},
    ),
    pack(
        "title49.hazmat.special_permits_exceptions",
        "Title 49 Hazmat Special Permits and Exceptions",
        "phmsa_hmr",
        "operational",
        "Special permits, approvals, registrations, and exception-condition controls.",
        ["SupplyArr", "RoutArr", "ComplianceCore"],
        ["special_permit", "approval", "shipment"],
        subparts={"107:B", "107:H"},
        section_prefixes=["173.4", "173.5", "173.6"],
    ),
    pack(
        "title49.hazmat.loading_unloading_segregation_reference",
        "Title 49 Hazmat Rail Air Vessel Reference",
        "phmsa_hmr",
        "reference",
        "Reference coverage for HMR carriage by rail, aircraft, and vessel, plus tank car specifications.",
        ["SupplyArr", "RoutArr", "ComplianceCore"],
        ["shipment", "mode", "package"],
        parts=["174", "175", "176", "179"],
        manual_review=["Rail, air, and vessel operational gates are reference only until RoutArr/SupplyArr mode workflows exist."],
    ),
    pack(
        "title49.pipeline.safety_reference",
        "Title 49 Pipeline Safety Reference",
        "phmsa_pipeline",
        "reference",
        "Pipeline safety, damage prevention, and pipeline drug/alcohol program reference coverage.",
        ["ComplianceCore"],
        ["pipeline_operator", "pipeline_program"],
        part_ranges=[(190, 199)],
    ),
    pack(
        "title49.rail.safety_reference",
        "Title 49 Rail Safety Reference",
        "fra_rail",
        "reference",
        "Federal Railroad Administration rail safety reference coverage.",
        ["ComplianceCore"],
        ["railroad", "rail_equipment", "rail_operation"],
        part_ranges=[(200, 299)],
    ),
    pack(
        "title49.transit.safety_reference",
        "Title 49 Transit Safety Reference",
        "fta_transit",
        "reference",
        "Federal Transit Administration safety and program reference coverage.",
        ["ComplianceCore"],
        ["transit_agency", "transit_program"],
        part_ranges=[(600, 699)],
    ),
    pack(
        "title49.nhtsa.vehicle_standards_reference",
        "Title 49 NHTSA Vehicle Standards Reference",
        "nhtsa_vehicle",
        "reference",
        "NHTSA vehicle standards, recalls, theft prevention, and consumer information reference coverage.",
        ["MaintainArr", "ComplianceCore"],
        ["vehicle", "vehicle_standard", "manufacturer"],
        part_ranges=[(500, 599)],
    ),
    pack(
        "title49.tsa.transportation_security_reference",
        "Title 49 TSA Transportation Security Reference",
        "tsa_security",
        "reference",
        "Transportation Security Administration security rules reference coverage.",
        ["StaffArr", "TrainArr", "RoutArr", "ComplianceCore"],
        ["worker", "credential", "transportation_security"],
        part_ranges=[(1500, 1699)],
    ),
    pack(
        "title49.stb.surface_transportation_reference",
        "Title 49 STB Surface Transportation Reference",
        "stb_surface",
        "reference",
        "Surface Transportation Board procedural and economic regulation reference coverage.",
        ["ComplianceCore"],
        ["carrier", "rate_case", "surface_transportation"],
        part_ranges=[(1000, 1399)],
    ),
    pack(
        "title49.passenger_carrier.operations",
        "Title 49 Passenger Carrier Operations",
        "fmcsa_passenger",
        "reference",
        "Passenger carrier operations and consumer protection reference coverage.",
        ["RoutArr", "StaffArr", "ComplianceCore"],
        ["passenger_trip", "carrier", "lease"],
        parts=["374"],
        subparts={"390:G"},
    ),
    pack(
        "title49.household_goods.consumer_protection",
        "Title 49 Household Goods Consumer Protection",
        "fmcsa_household_goods",
        "reference",
        "Household goods transportation consumer protection reference coverage.",
        ["SupplyArr", "RoutArr", "ComplianceCore"],
        ["household_goods_shipment", "carrier", "consumer_disclosure"],
        parts=["375"],
    ),
    pack(
        "title49.motorcarrier.safety_fitness_proceedings_reference",
        "Title 49 FMCSA State Compliance and Proceedings Reference",
        "fmcsa_fmcsr",
        "reference",
        "Reference coverage for CDL state compliance, safety fitness procedures, and FMCSA rules of practice.",
        ["StaffArr", "RoutArr", "ComplianceCore"],
        ["state_compliance", "safety_fitness", "enforcement_proceeding"],
        parts=["384", "385", "386"],
        exclude_subparts={"385:F"},
        manual_review=["Parts 384-386 are reference mapped until product workflows own state CDL compliance, safety fitness scoring, and FMCSA proceeding operations."],
    ),
    pack(
        "title49.intermodal.equipment_provider",
        "Title 49 Intermodal Equipment Provider",
        "fmcsa_intermodal",
        "reference",
        "Intermodal equipment provider safety and inspection reference coverage.",
        ["MaintainArr", "RoutArr", "ComplianceCore"],
        ["intermodal_equipment", "provider", "inspection"],
        subparts={"390:C", "385:F"},
        sections={"396.12"},
    ),
    pack(
        "title49.transportation.citation_metadata",
        "Title 49 Citation Metadata",
        "dot_title49_metadata",
        "metadata",
        "Citation metadata for current Title 49 hierarchy not otherwise assigned to operational or reference rule packs.",
        ["ComplianceCore"],
        ["citation", "regulatory_hierarchy"],
    ),
]


FACTS: dict[str, list[FactSpec]] = {
    "title49.motorcarrier.registration_authority": [
        fact("t49_mc_authority_active", "Operating authority active", "Carrier authority is active for the operation.", ["StaffArr", "RoutArr"], ["carrier", "authority_record"], ["t49_part_365"], True),
        fact("t49_mc_process_agent_designated", "Process agent designated", "BOC-3/process-agent designation is recorded.", ["StaffArr"], ["carrier", "registration"], ["t49_part_366"], True),
        fact("t49_mc_registration_current", "Registration current", "State or unified registration evidence is current.", ["StaffArr", "RoutArr"], ["carrier", "registration"], ["t49_part_367", "t49_part_368"], False),
    ],
    "title49.motorcarrier.insurance_financial_responsibility": [
        fact("t49_mc_financial_minimum_met", "Financial minimum met", "Insurance or surety evidence meets the minimum level required for the operation.", ["StaffArr", "RoutArr", "SupplyArr"], ["carrier", "insurance_policy"], ["t49_part_387"], True),
        fact("t49_mc_mcs90_or_equivalent_on_file", "MCS-90 equivalent on file", "Required endorsement or equivalent financial responsibility evidence is on file.", ["StaffArr"], ["carrier", "insurance_policy"], ["t49_part_387"], True),
    ],
    "title49.motorcarrier.applicability": [
        fact("t49_mc_applicability_determined", "FMCSR applicability determined", "Operation, carrier, driver, and vehicle applicability are classified before evaluation.", ["RoutArr", "StaffArr", "MaintainArr"], ["carrier", "driver", "vehicle"], ["t49_part_390"], True),
        fact("t49_mc_usdot_marking_current", "USDOT marking current", "Required identification and marking evidence is current.", ["MaintainArr", "RoutArr"], ["vehicle", "carrier"], ["t49_sec_390_21"], False),
        fact("t49_driver_safe_operation_clear", "Safe operation checks clear", "Dispatch facts show no known safe-operation prohibition for the driver or CMV.", ["RoutArr", "StaffArr"], ["driver", "dispatch"], ["t49_part_392"], True),
    ],
    "title49.driver.qualification_file": [
        fact(
            "t49_dq_application_present",
            "DQ application present",
            "Driver qualification application is present and linked to the driver file.",
            ["StaffArr"],
            ["driver", "qualification_file", "application"],
            ["t49_sec_391_21", "t49_sec_391_51"],
            True,
            source_field_or_record_type="driver_qualification_application",
            evidence_kind="document_record",
            required_document_type="driver_qualification_application",
            retention_period="life_of_employment_plus_3_years",
            audit_question="Is the driver qualification application present in the DQ file?",
        ),
        fact(
            "t49_dq_mvr_initial_present",
            "Initial MVR present",
            "Initial motor vehicle record or equivalent inquiry evidence is present.",
            ["StaffArr"],
            ["driver", "qualification_file", "mvr"],
            ["t49_sec_391_23", "t49_sec_391_51"],
            True,
            source_field_or_record_type="initial_motor_vehicle_record",
            evidence_kind="document_record",
            required_document_type="initial_mvr",
            retention_period="life_of_employment_plus_3_years",
            audit_question="Is the initial MVR or equivalent inquiry evidence present?",
        ),
        fact(
            "t49_dq_mvr_annual_current",
            "Annual MVR current",
            "Annual MVR review evidence is current.",
            ["StaffArr"],
            ["driver", "qualification_file", "mvr"],
            ["t49_sec_391_25", "t49_sec_391_51"],
            True,
            source_field_or_record_type="annual_mvr_review",
            evidence_kind="document_record",
            required_document_type="annual_mvr_review",
            retention_period="3_years_from_execution",
            audit_question="Is the annual MVR review current for the driver?",
        ),
        fact(
            "t49_dq_medical_certificate_current",
            "Medical certificate current",
            "Medical examiner certificate or qualifying medical status is current and linked to the DQ file.",
            ["StaffArr", "RoutArr"],
            ["driver", "qualification_file", "medical_certificate"],
            ["t49_sec_391_41", "t49_sec_391_45", "t49_sec_391_51"],
            True,
            source_field_or_record_type="medical_certificate",
            evidence_kind="document_record",
            required_document_type="medical_examiner_certificate",
            retention_period="until_replaced_or_expired_plus_3_years",
            audit_question="Is the driver's medical certificate current and in the qualification file?",
        ),
        fact(
            "t49_dq_road_test_or_equivalent_present",
            "Road test or equivalent present",
            "Road test certificate or accepted CDL equivalent evidence is present.",
            ["StaffArr", "TrainArr"],
            ["driver", "qualification_file", "road_test"],
            ["t49_sec_391_31", "t49_sec_391_33", "t49_sec_391_51"],
            False,
            source_field_or_record_type="road_test_certificate_or_cdl_equivalent",
            evidence_kind="document_record",
            required_document_type="road_test_certificate_or_equivalent",
            retention_period="life_of_employment_plus_3_years",
            audit_question="Is a road test certificate or acceptable equivalent present?",
        ),
        fact(
            "t49_dq_prior_employer_inquiry_complete",
            "Prior employer inquiry complete",
            "Prior employer safety performance investigation or equivalent inquiry is complete.",
            ["StaffArr"],
            ["driver", "qualification_file", "prior_employer_inquiry"],
            ["t49_sec_391_23", "t49_sec_391_51"],
            True,
            source_field_or_record_type="prior_employer_safety_performance_inquiry",
            evidence_kind="document_record",
            required_document_type="prior_employer_inquiry",
            retention_period="life_of_employment_plus_3_years",
            audit_question="Is the prior employer safety performance inquiry complete?",
        ),
        fact(
            "t49_dq_annual_violation_review_complete",
            "Annual violation review complete",
            "Annual driver violation list/certification and review evidence is complete.",
            ["StaffArr"],
            ["driver", "qualification_file", "annual_violation_review"],
            ["t49_sec_391_25", "t49_sec_391_27", "t49_sec_391_51"],
            True,
            source_field_or_record_type="annual_violation_review",
            evidence_kind="document_record",
            required_document_type="annual_violation_review",
            retention_period="3_years_from_execution",
            audit_question="Is the annual violation review complete for the driver?",
        ),
        fact(
            "t49_driver_dq_file_complete",
            "DQ file complete rollup",
            "Derived rollup across the atomic driver qualification file audit facts.",
            ["ComplianceCore"],
            ["driver", "qualification_file", "derived_rollup"],
            ["t49_sec_391_51"],
            True,
            source_field_or_record_type="derived_rollup:title49.driver.qualification_file",
            value_type="boolean",
            operator="all_true",
            expected_value="t49_dq_application_present,t49_dq_mvr_initial_present,t49_dq_mvr_annual_current,t49_dq_medical_certificate_current,t49_dq_road_test_or_equivalent_present,t49_dq_prior_employer_inquiry_complete,t49_dq_annual_violation_review_complete",
            evidence_kind="derived_fact",
            required_document_type="none",
            retention_period="derived_from_component_facts",
            audit_question="Have all atomic driver qualification file audit facts passed?",
            automatic_failure_flag=False,
            override_allowed=False,
            override_permission="",
            remediation_required=False,
            derived=True,
        ),
    ],
    "title49.driver.cdl_clp_endorsements": [
        fact("t49_driver_cdl_valid_for_vehicle", "CDL valid for vehicle", "Driver CDL/CLP class is valid for assigned CMV group.", ["StaffArr", "RoutArr"], ["driver", "license"], ["t49_sec_383_23", "t49_sec_383_91"], True),
        fact("t49_driver_endorsements_match_load", "Endorsements match load", "Passenger, school bus, tank, double/triple, or hazmat endorsements match the assignment.", ["StaffArr", "RoutArr"], ["driver", "endorsement", "shipment"], ["t49_sec_383_93"], True),
        fact("t49_driver_no_cdl_disqualification", "No CDL disqualification", "No active disqualification or CDL penalty blocks dispatch.", ["StaffArr", "RoutArr"], ["driver", "license"], ["t49_sec_383_51"], True),
    ],
    "title49.driver.entry_level_driver_training": [
        fact("t49_driver_eldt_completed", "ELDT completed", "Required entry-level driver training is completed before CDL skills or endorsement use.", ["TrainArr", "StaffArr"], ["driver", "training_record"], ["t49_sec_380_603"], True),
        fact("t49_driver_training_provider_verified", "Training provider verified", "Training provider and curriculum evidence are linked to a registered provider.", ["TrainArr"], ["training_provider", "training_record"], ["t49_sec_380_703"], False),
        fact("t49_driver_hazmat_endorsement_training_current", "Hazmat endorsement training current", "Hazmat endorsement curriculum completion evidence is current.", ["TrainArr", "StaffArr"], ["driver", "training_record"], ["t49_app_part_380_e"], True),
    ],
    "title49.driver.medical_qualification": [
        fact("t49_driver_med_cert_current", "Medical certificate current", "Driver has a current medical examiner certificate or qualifying medical status.", ["StaffArr", "RoutArr"], ["driver", "medical_certificate"], ["t49_sec_391_41", "t49_sec_391_45"], True),
        fact("t49_driver_med_examiner_verified", "Medical examiner verified", "Medical examiner registry evidence is verified when required.", ["StaffArr"], ["driver", "examiner"], ["t49_sec_390_101", "t49_sec_391_43"], False),
        fact("t49_driver_med_variance_on_file", "Medical variance on file", "Required Skill Performance Evaluation or medical variance evidence is on file.", ["StaffArr"], ["driver", "medical_variance"], ["t49_sec_391_49"], True),
    ],
    "title49.driver.drug_alcohol_program": [
        fact("t49_da_policy_active", "Drug and alcohol policy active", "DOT/FMCSA drug and alcohol program policy and notices are active.", ["StaffArr", "TrainArr"], ["program", "driver"], ["t49_part_40", "t49_part_382"], True),
        fact("t49_da_preemployment_clear", "Pre-employment testing clear", "Pre-employment testing or qualifying exception is documented before safety-sensitive work.", ["StaffArr", "RoutArr"], ["driver", "test_result"], ["t49_sec_382_301"], True),
        fact("t49_da_clearinghouse_query_current", "Clearinghouse query current", "Required Clearinghouse query is complete and clear.", ["StaffArr", "RoutArr"], ["driver", "clearinghouse_query"], ["t49_sec_382_701"], True),
        fact("t49_da_post_accident_done_if_required", "Post-accident test done if required", "Post-accident testing decision and completion evidence are captured when required.", ["StaffArr", "RoutArr"], ["incident", "test_result"], ["t49_sec_382_303"], True),
        fact("t49_da_sap_return_to_duty_clear", "SAP return-to-duty clear", "SAP, return-to-duty, and follow-up requirements are complete after a violation.", ["StaffArr", "TrainArr"], ["driver", "sap_case"], ["t49_sec_40_285", "t49_sec_40_307"], True),
    ],
    "title49.driver.hours_of_service": [
        fact("t49_hos_limits_clear", "HOS limits clear", "Available duty status facts show the driver is within applicable hours-of-service limits.", ["RoutArr"], ["driver", "hos_log", "trip"], ["t49_sec_395_3", "t49_sec_395_5"], True),
        fact("t49_hos_log_complete", "HOS log complete", "Record of duty status is complete for the dispatch/evaluation period.", ["RoutArr"], ["driver", "hos_log"], ["t49_sec_395_8"], True),
        fact("t49_hos_support_docs_retained", "Supporting docs retained", "Supporting documents are available for the required retention window.", ["RoutArr"], ["driver", "supporting_document"], ["t49_sec_395_11"], False),
    ],
    "title49.driver.eld_records": [
        fact("t49_eld_device_registered", "ELD registered", "Assigned ELD is registered and not known to be revoked for the operation.", ["RoutArr", "MaintainArr"], ["eld_device", "vehicle"], ["t49_sec_395_22"], True),
        fact("t49_eld_records_transferable", "ELD records transferable", "ELD records can be displayed or transferred for inspection.", ["RoutArr"], ["eld_record", "driver"], ["t49_sec_395_24"], True),
        fact("t49_eld_malfunction_resolved", "ELD malfunction resolved", "Open ELD malfunctions are documented and resolved or operating under allowed process.", ["RoutArr", "MaintainArr"], ["eld_device", "defect"], ["t49_sec_395_34"], False),
    ],
    "title49.driver.accident_post_accident_actions": [
        fact("t49_accident_register_current", "Accident register current", "Recordable accidents are entered in the accident register with required details.", ["StaffArr", "RoutArr"], ["incident", "accident_register"], ["t49_sec_390_15"], True),
        fact("t49_accident_test_decision_recorded", "Post-accident test decision recorded", "Post-accident drug/alcohol testing decision and timing evidence are recorded.", ["StaffArr"], ["incident", "test_decision"], ["t49_sec_382_303"], True),
        fact("t49_accident_corrective_action_closed", "Corrective action closed", "Post-accident corrective actions and evidence are closed or escalated.", ["StaffArr", "MaintainArr"], ["incident", "corrective_action"], ["t49_sec_390_15"], False),
    ],
    "title49.vehicle.parts_accessories_condition": [
        fact("t49_vehicle_required_equipment_ready", "Required equipment ready", "Lamps, brakes, tires, coupling, emergency equipment, and other required parts are ready.", ["MaintainArr", "RoutArr"], ["vehicle", "asset"], ["t49_part_393"], True),
        fact("t49_vehicle_no_critical_defects", "No critical defects", "No unresolved critical defect blocks safe operation.", ["MaintainArr", "RoutArr"], ["vehicle", "defect"], ["t49_part_393"], True),
    ],
    "title49.vehicle.inspection_repair_maintenance": [
        fact("t49_vehicle_maintenance_program_current", "Maintenance program current", "Inspection, repair, and maintenance program evidence is current.", ["MaintainArr"], ["vehicle", "maintenance_program"], ["t49_sec_396_3"], True),
        fact("t49_vehicle_pretrip_completed", "Pretrip completed", "Driver pretrip/driver inspection evidence is complete before dispatch.", ["MaintainArr", "RoutArr"], ["vehicle", "inspection"], ["t49_sec_396_13"], True),
        fact("t49_vehicle_repairs_closed", "Required repairs closed", "Known safety defects are repaired before the vehicle is dispatched.", ["MaintainArr"], ["vehicle", "work_order"], ["t49_sec_396_7"], True),
    ],
    "title49.vehicle.dvir": [
        fact("t49_dvir_submitted", "DVIR submitted", "Required driver vehicle inspection report is submitted.", ["MaintainArr", "RoutArr"], ["vehicle", "dvir"], ["t49_sec_396_11"], True),
        fact("t49_dvir_defect_certified", "DVIR defect certified", "Listed DVIR defects are certified repaired or repair-not-necessary.", ["MaintainArr"], ["vehicle", "defect", "repair"], ["t49_sec_396_11"], True),
    ],
    "title49.vehicle.annual_inspection": [
        fact("t49_annual_inspection_current", "Annual inspection current", "Periodic inspection is current for the assigned vehicle.", ["MaintainArr", "RoutArr"], ["vehicle", "inspection"], ["t49_sec_396_17"], True),
        fact("t49_annual_inspector_qualified", "Inspector qualified", "Periodic/brake inspector qualification evidence is available.", ["MaintainArr", "StaffArr"], ["inspector", "qualification"], ["t49_sec_396_19", "t49_sec_396_25"], True),
        fact("t49_annual_record_retained", "Annual record retained", "Periodic inspection records are retained and available.", ["MaintainArr"], ["vehicle", "inspection_record"], ["t49_sec_396_21"], False),
    ],
    "title49.vehicle.roadside_inspection_correction": [
        fact("t49_roadside_correction_complete", "Roadside correction complete", "Roadside inspection violations are corrected before return to service when required.", ["MaintainArr", "RoutArr"], ["vehicle", "roadside_inspection"], ["t49_sec_396_9"], True),
        fact("t49_roadside_report_returned", "Roadside report returned", "Roadside inspection report return/closeout evidence is recorded.", ["MaintainArr"], ["roadside_inspection"], ["t49_sec_396_9"], False),
    ],
    "title49.vehicle.out_of_service_readiness": [
        fact("t49_oos_no_active_order", "No active out-of-service order", "No active driver, vehicle, or carrier out-of-service order blocks the assignment.", ["RoutArr", "MaintainArr", "StaffArr"], ["driver", "vehicle", "carrier"], ["t49_sec_390_13", "t49_sec_395_13"], True),
        fact("t49_oos_vehicle_ready_for_dispatch", "Vehicle ready for dispatch", "Driver confirms vehicle readiness and no unsafe-operation condition exists.", ["RoutArr", "MaintainArr"], ["vehicle", "dispatch"], ["t49_sec_392_7", "t49_sec_396_7"], True),
    ],
    "title49.vehicle.cargo_securement": [
        fact("t49_cargo_securement_verified", "Cargo securement verified", "Cargo securement method is verified before dispatch.", ["RoutArr", "SupplyArr"], ["load", "securement"], ["t49_sec_393_100", "t49_sec_393_106"], True),
        fact("t49_cargo_devices_rated", "Securement devices rated", "Securement devices have sufficient working load limit for the articles carried.", ["RoutArr", "SupplyArr"], ["load", "securement_device"], ["t49_sec_393_102"], True),
    ],
    "title49.hazmat.applicability": [
        fact("t49_hazmat_applicability_done", "Hazmat applicability done", "Shipment has been classified for HMR applicability and hazmat function involvement.", ["SupplyArr", "RoutArr"], ["shipment", "material"], ["t49_sec_171_1"], True),
        fact("t49_hazmat_forbidden_checked", "Forbidden shipment checked", "Forbidden materials and forbidden offering conditions are checked.", ["SupplyArr"], ["shipment", "material"], ["t49_sec_171_2"], True),
    ],
    "title49.hazmat.classification": [
        fact("t49_hazmat_classification_complete", "Hazmat classification complete", "Hazard class, division, packing group, and exceptions are determined.", ["SupplyArr"], ["material", "classification"], ["t49_part_173"], True),
        fact("t49_hazmat_sds_classification_linked", "SDS classification linked", "SDS/material facts are linked to the classification decision.", ["SupplyArr"], ["material", "sds"], ["t49_sec_173_22"], False),
    ],
    "title49.hazmat.hazardous_materials_table": [
        fact("t49_hmt_entry_verified", "HMT entry verified", "UN/NA number, proper shipping name, class, packing group, labels, and provisions are verified against the HMT.", ["SupplyArr"], ["material", "shipment"], ["t49_sec_172_101"], True),
        fact("t49_hmt_special_provisions_applied", "HMT special provisions applied", "Applicable special provisions are captured and applied.", ["SupplyArr", "RoutArr"], ["shipment", "special_provision"], ["t49_sec_172_102"], True),
    ],
    "title49.hazmat.shipping_papers": [
        fact("t49_hazmat_shipping_paper_complete", "Shipping paper complete", "Shipping paper contains required basic description, sequence, certification, and emergency response information.", ["SupplyArr", "RoutArr"], ["shipment", "shipping_paper"], ["t49_sec_172_202", "t49_sec_172_204"], True),
        fact("t49_hazmat_er_info_available", "Emergency response information available", "Emergency response information is immediately available with the shipment.", ["SupplyArr", "RoutArr"], ["shipment", "emergency_response_info"], ["t49_sec_172_602"], True),
    ],
    "title49.hazmat.marking": [
        fact("t49_hazmat_marking_applied", "Hazmat marking applied", "Required marks are applied to packages or bulk packagings.", ["SupplyArr"], ["package", "marking"], ["t49_sec_172_301", "t49_sec_172_302"], True),
        fact("t49_hazmat_marking_durable", "Hazmat marking durable", "Markings are durable, visible, and meet specification placement requirements.", ["SupplyArr", "RoutArr"], ["package", "marking"], ["t49_sec_172_304"], True),
    ],
    "title49.hazmat.labeling": [
        fact("t49_hazmat_labels_applied", "Hazmat labels applied", "Hazard labels are applied where required.", ["SupplyArr"], ["package", "label"], ["t49_sec_172_400"], True),
        fact("t49_hazmat_label_specs_met", "Hazmat label specs met", "Label design, durability, and placement specifications are met.", ["SupplyArr"], ["package", "label"], ["t49_sec_172_406", "t49_sec_172_407"], True),
    ],
    "title49.hazmat.placarding": [
        fact("t49_hazmat_placards_required_checked", "Placard requirement checked", "Placard tables and exceptions are evaluated for the load.", ["SupplyArr", "RoutArr"], ["shipment", "vehicle"], ["t49_sec_172_504"], True),
        fact("t49_hazmat_placards_displayed", "Placards displayed", "Required placards are displayed, visible, and meet specifications.", ["RoutArr"], ["vehicle", "placard"], ["t49_sec_172_516", "t49_sec_172_519"], True),
    ],
    "title49.hazmat.packaging": [
        fact("t49_hazmat_packaging_authorized", "Packaging authorized", "Packaging selected is authorized for the material and quantity.", ["SupplyArr"], ["material", "package"], ["t49_sec_173_24"], True),
        fact("t49_hazmat_package_spec_valid", "Package specification valid", "Specification packaging, testing, and markings are valid.", ["SupplyArr", "MaintainArr"], ["package", "container"], ["t49_part_178"], True),
        fact("t49_hazmat_requalification_current", "Packaging requalification current", "Cylinder, cargo tank, IBC, tank car, or portable tank requalification is current where required.", ["MaintainArr", "SupplyArr"], ["container", "asset"], ["t49_part_180"], True),
    ],
    "title49.hazmat.loading_unloading_segregation": [
        fact("t49_hazmat_loading_rules_met", "Hazmat loading rules met", "Highway loading, unloading, attendance, and handling rules are met.", ["SupplyArr", "RoutArr"], ["shipment", "load"], ["t49_sec_177_834"], True),
        fact("t49_hazmat_segregation_checked", "Hazmat segregation checked", "Segregation and separation chart requirements are checked.", ["SupplyArr", "RoutArr"], ["shipment", "load"], ["t49_sec_177_848"], True),
        fact("t49_hazmat_parking_routing_clear", "Hazmat parking/routing clear", "Driving, parking, attendance, and routing restrictions are clear.", ["RoutArr"], ["route", "vehicle"], ["t49_part_397"], True),
    ],
    "title49.hazmat.training": [
        fact("t49_hazmat_employee_training_current", "Hazmat employee training current", "General awareness, function-specific, safety, security awareness, and in-depth training are current.", ["TrainArr", "StaffArr"], ["worker", "training_record"], ["t49_sec_172_704"], True),
        fact("t49_hazmat_training_records_retained", "Hazmat training records retained", "Training record evidence is retained and available.", ["TrainArr"], ["training_record"], ["t49_sec_172_704"], False),
    ],
    "title49.hazmat.security_plan": [
        fact("t49_hazmat_security_plan_current", "Security plan current", "Security plan is required where applicable and current.", ["SupplyArr", "RoutArr", "StaffArr"], ["shipment", "security_plan"], ["t49_sec_172_800", "t49_sec_172_802"], True),
        fact("t49_hazmat_route_security_review", "Route security review complete", "Required route analysis/security routing controls are complete.", ["RoutArr"], ["route", "security_plan"], ["t49_sec_172_820"], True),
    ],
    "title49.hazmat.incident_reporting": [
        fact("t49_hazmat_immediate_notice_done", "Immediate notice done", "Immediate notification is completed when incident criteria are met.", ["StaffArr", "SupplyArr", "RoutArr"], ["incident", "notification"], ["t49_sec_171_15"], True),
        fact("t49_hazmat_written_report_filed", "Written incident report filed", "Written incident report is filed when required.", ["StaffArr", "SupplyArr"], ["incident", "report"], ["t49_sec_171_16"], True),
    ],
    "title49.hazmat.registration": [
        fact("t49_hazmat_registration_current", "Hazmat registration current", "PHMSA hazmat registration is current for the offeror/transporter.", ["SupplyArr", "RoutArr", "StaffArr"], ["carrier", "shipper", "registration"], ["t49_sec_107_601"], True),
        fact("t49_hazmat_registration_available", "Registration proof available", "Registration number/certificate evidence is available.", ["SupplyArr", "RoutArr"], ["carrier", "registration"], ["t49_sec_107_620"], False),
    ],
    "title49.hazmat.special_permits_exceptions": [
        fact("t49_hazmat_special_permit_valid", "Special permit valid", "Special permit or approval is active and applicable to the shipment.", ["SupplyArr", "RoutArr"], ["special_permit", "shipment"], ["t49_sec_107_105"], True),
        fact("t49_hazmat_exception_conditions_met", "Exception conditions met", "Small quantity, material of trade, or other exception conditions are documented.", ["SupplyArr", "RoutArr"], ["shipment", "exception"], ["t49_sec_173_4", "t49_sec_173_6"], True),
    ],
}

for spec in PACKS:
    spec.facts = FACTS.get(spec.key, [])


def load_structure(path_arg: str | None = None) -> dict[str, Any]:
    candidates: list[Path] = []
    if path_arg:
        candidates.append(Path(path_arg))
    temp = os.environ.get("TEMP") or os.environ.get("TMP")
    if temp:
        candidates.append(Path(temp) / "ecfr_title49_structure.json")
    for candidate in candidates:
        if candidate.exists():
            with candidate.open("r", encoding="utf-8") as handle:
                return json.load(handle)
    with urllib.request.urlopen(STRUCTURE_URL, timeout=120) as response:
        return json.loads(response.read().decode("utf-8"))


def part_int(part: str | None) -> int | None:
    if not part:
        return None
    match = re.match(r"^(\d+)", part)
    return int(match.group(1)) if match else None


def citation_key(node_type: str, identifier: str, part: str | None, chapter: str | None, subpart: str | None) -> str:
    ident = ascii_text(identifier)
    if node_type == "title":
        return "t49_title"
    if node_type == "subtitle":
        return short_key("t49_subtitle", ident)
    if node_type == "chapter":
        chapter_id = "xi" if "chapter xi" in ident.lower() or ident == "0" else ident
        return short_key("t49_ch", chapter_id)
    if node_type == "subchapter":
        return short_key("t49_subch", f"{chapter or 'x'}_{ident}")
    if node_type == "part":
        return short_key("t49_part", ident)
    if node_type == "subpart":
        return short_key("t49_subpart", f"{part}_{ident}")
    if node_type == "appendix":
        subpart_app = re.search(
            r"appendix(?:es)?\s+([a-z0-9-]+)?\s*to\s+subpart\s+([a-z0-9-]+)\s+of\s+part\s+(\d+)",
            ident,
            re.IGNORECASE,
        )
        if subpart_app:
            appendix_id = subpart_app.group(1) or "appendix"
            return short_key("t49_app_part", f"{part}_subpart_{subpart_app.group(2)}_{appendix_id}")
        part_app = re.search(r"appendix(?:es)?\s+([a-z0-9-]+)\s+to\s+part\s+(\d+)", ident, re.IGNORECASE)
        appendix_id = part_app.group(1) if part_app else ident
        return short_key("t49_app_part", f"{part}_{appendix_id}")
    if node_type == "section":
        return short_key("t49_sec", ident)
    return short_key("t49_node", ident)


def source_reference(node_type: str, identifier: str, part: str | None) -> str:
    if node_type == "title":
        return "49 CFR Title 49"
    if node_type == "subtitle":
        return f"49 CFR Subtitle {identifier}"
    if node_type == "chapter":
        return f"49 CFR Chapter {identifier}"
    if node_type == "subchapter":
        return f"49 CFR Subchapter {identifier}"
    if node_type == "part":
        return f"49 CFR Part {identifier}"
    if node_type == "subpart":
        return f"49 CFR Part {part} Subpart {identifier}"
    if node_type == "appendix":
        return f"49 CFR {identifier}"
    if node_type == "section":
        return f"49 CFR {identifier}"
    return f"49 CFR {identifier}"


def ecfr_url(node_type: str, identifier: str, part: str | None) -> str:
    if node_type == "section":
        return f"https://www.ecfr.gov/current/title-49/section-{identifier}"
    if node_type == "part":
        return f"https://www.ecfr.gov/current/title-49/part-{identifier}"
    if node_type == "subpart" and part:
        return f"https://www.ecfr.gov/current/title-49/part-{part}/subpart-{identifier}"
    if node_type == "appendix" and part:
        return f"https://www.ecfr.gov/current/title-49/part-{part}"
    return TITLE_URL


def flatten_structure(root: dict[str, Any]) -> list[dict[str, Any]]:
    nodes: list[dict[str, Any]] = []

    def walk(node: dict[str, Any], context: dict[str, str | None], path: list[str]) -> None:
        node_type = node.get("type", "")
        identifier = ascii_text(node.get("identifier") or node.get("label_level") or node.get("label") or "")
        label_description = ascii_text(node.get("label_description") or node.get("label") or identifier)
        next_context = dict(context)
        if node_type == "chapter":
            next_context["chapter"] = "XI" if node.get("identifier") == "0" else identifier
        if node_type == "part":
            next_context["part"] = identifier
            next_context["subpart"] = None
        if node_type == "subpart":
            next_context["subpart"] = identifier
        if node_type in {"title", "subtitle", "chapter", "subchapter", "part", "subpart", "section", "appendix"}:
            key = citation_key(node_type, identifier, next_context.get("part"), next_context.get("chapter"), next_context.get("subpart"))
            source = source_reference(node_type, identifier, next_context.get("part"))
            hierarchy = " > ".join([p for p in path + [source] if p])
            nodes.append(
                {
                    "key": key,
                    "type": node_type,
                    "identifier": identifier,
                    "part": next_context.get("part"),
                    "part_int": part_int(next_context.get("part")),
                    "subpart": next_context.get("subpart"),
                    "chapter": next_context.get("chapter"),
                    "label": trim(label_description, 128),
                    "source_reference": trim(source, 128),
                    "description": trim(
                        f"Mapping level candidate for {source}. Hierarchy: {hierarchy}. Source: {ecfr_url(node_type, identifier, next_context.get('part'))}",
                        1024,
                    ),
                    "reserved": bool(node.get("reserved")),
                }
            )
        next_path = path + ([source_reference(node_type, identifier, next_context.get("part"))] if node_type in {"subtitle", "chapter", "subchapter", "part", "subpart"} else [])
        for child in node.get("children", []) or []:
            walk(child, next_context, next_path)

    walk(root, {"part": None, "subpart": None, "chapter": None}, [])
    return nodes


def selector_matches(spec: PackSpec, node: dict[str, Any]) -> bool:
    part = node.get("part")
    node_type = node.get("type")
    identifier = node.get("identifier", "")
    subpart_key = f"{part}:{node.get('subpart')}"
    if identifier in spec.exclude_sections or subpart_key in spec.exclude_subparts:
        return False
    if part in spec.parts:
        if spec.subparts:
            # Explicit subpart selectors should narrow broad part ownership.
            return node_type == "part" or subpart_key in spec.subparts or identifier in spec.sections
        return True
    p_int = node.get("part_int")
    if p_int is not None:
        for start, end in spec.part_ranges:
            if start <= p_int <= end:
                return True
    if subpart_key in spec.subparts:
        return True
    if identifier in spec.sections:
        return True
    return any(identifier.startswith(prefix) for prefix in spec.section_prefixes)


def assign_nodes(nodes: list[dict[str, Any]]) -> dict[str, list[dict[str, Any]]]:
    assigned: dict[str, list[dict[str, Any]]] = {spec.key: [] for spec in PACKS}
    claimed: set[str] = set()
    metadata_key = "title49.transportation.citation_metadata"
    for spec in PACKS:
        if spec.level == "metadata":
            continue
        for node in nodes:
            if node["key"] in claimed:
                continue
            if selector_matches(spec, node):
                assigned[spec.key].append(node)
                claimed.add(node["key"])
    for node in nodes:
        if node["key"] not in claimed:
            assigned[metadata_key].append(node)
    return assigned


def pack_compliance_key(pack_key: str) -> str:
    return short_key("ck", pack_key.replace("title49.", "t49_"))


def build_rule_content(spec: PackSpec) -> str:
    if not spec.facts:
        return ""
    rules = []
    conditions = []
    for index, item in enumerate((fact for fact in spec.facts if not fact.derived), start=1):
        suffix = slug(item.key.replace("t49_", ""))[:42]
        rule_key = short_key("r", suffix)
        condition_key = short_key("c", suffix)
        rules.append(
            {
                "ruleKey": rule_key,
                "label": trim(item.label, 128),
                "type": "fact_boolean",
                "factKey": item.key,
                "expectedValue": True,
                "nonWaivable": item.non_waivable,
            }
        )
        conditions.append(
            {
                "conditionKey": condition_key,
                "ruleKey": rule_key,
                "factKey": item.key,
                "operator": "equals",
                "expectedValue": True,
                "sourceProducts": item.products,
                "entities": item.entities,
            }
        )
    content = {
        "schemaVersion": 1,
        "logic": "all",
        "rules": rules,
        "conditions": conditions,
        "outcomes": [
            {
                "outcomeKey": short_key("outcome_allow", spec.key),
                "when": "all_conditions_pass",
                "result": "allow",
                "message": f"{spec.label} is satisfied.",
            },
            {
                "outcomeKey": short_key("outcome_block", spec.key),
                "when": "any_required_condition_fails",
                "result": "block",
                "message": f"{spec.label} is not satisfied; product workflow must block or require authorized override.",
            },
        ],
    }
    return json.dumps(content, separators=(",", ":"), sort_keys=True)


def rows_for_pack(spec: PackSpec, citations: list[dict[str, Any]]) -> dict[str, list[list[str]]]:
    compliance_key = pack_compliance_key(spec.key)
    rows: dict[str, list[list[str]]] = {name: [] for name in CSV_HEADERS}
    rows["controlled_vocabulary.csv"].append(
        [
            compliance_key,
            "compliance_domain",
            trim(spec.label, 128),
            trim(f"{spec.level} Title 49 domain for {', '.join(spec.entities)}.", 1024),
            "true",
        ]
    )
    rows["compliance_keys.csv"].append(
        [
            compliance_key,
            trim(spec.label, 128),
            "compliance_domain",
            trim(f"{spec.description} Mapping level: {spec.level}. Products: {', '.join(spec.products)}. Entities: {', '.join(spec.entities)}.", 1024),
            "true",
        ]
    )
    rows["rule_packs.csv"].append(
        [
            spec.key,
            spec.program_key,
            "1",
            trim(spec.label, 128),
            trim(f"{spec.description} Source date {SOURCE_DATE}; source URLs in coverage report.", 1024),
            "published" if spec.level == "operational" else "draft",
            "true",
            build_rule_content(spec),
        ]
    )
    for citation in citations:
        rows["rule_requirements.csv"].append(
            [
                citation["key"],
                spec.program_key,
                spec.key,
                "1",
                citation["label"],
                citation["source_reference"],
                citation["description"],
                "false" if citation["reserved"] else "true",
                "",
            ]
        )
        rows["regulatory_mappings.csv"].append(
            [
                short_key("map", f"{spec.key}_{citation['key']}"),
                "compliance_key",
                spec.program_key,
                spec.key,
                "1",
                citation["key"],
                compliance_key,
                "",
                "",
                trim(f"{spec.label} to {citation['source_reference']}", 128),
                trim(f"Maps {citation['source_reference']} to {spec.key} ({spec.level}).", 1024),
                "false" if citation["reserved"] else "true",
            ]
        )
    citation_keys = {citation["key"] for citation in citations}
    fallback_citation = citations[0]["key"] if citations else ""
    for item in spec.facts:
        citation_key = next((key for key in item.citations if key in citation_keys), fallback_citation)
        source_product = ",".join(item.products)
        source_entity = ",".join(item.entities)
        description = trim(
            (
                f"{item.description} products={source_product}; entities={source_entity}; "
                f"value_type={item.value_type}; operator={item.operator}; expected_value={item.expected_value}; "
                f"evidence_kind={item.evidence_kind}; required_document_type={item.required_document_type or 'none'}; "
                f"retention_period={item.retention_period}; failure_severity={item.failure_severity}; "
                f"automatic_failure={str(item.automatic_failure_flag).lower()}; override_allowed={str(item.override_allowed).lower()}."
            ),
            1024,
        )
        rows["rule_fact_requirements.csv"].append(
            [
                short_key("fr", f"{spec.key}_{item.key}"),
                item.key,
                spec.key,
                "1",
                citation_key,
                "1" if citation_key else "",
                compliance_key,
                source_product,
                source_entity,
                item.source_field_or_record_type,
                item.value_type,
                item.operator,
                item.expected_value,
                item.evidence_kind,
                item.required_document_type,
                item.retention_period,
                item.audit_question,
                item.failure_severity,
                str(item.automatic_failure_flag).lower(),
                str(item.override_allowed).lower(),
                item.override_permission,
                str(item.remediation_required).lower(),
                trim(item.label, 128),
                description,
                "true",
                "true",
            ]
        )
        rows["regulatory_mappings.csv"].append(
            [
                short_key("map_fact", f"{spec.key}_{item.key}"),
                "compliance_key",
                spec.program_key,
                spec.key,
                "1",
                citation_key,
                compliance_key,
                "",
                item.key,
                trim(f"{spec.label} fact {item.label}", 128),
                description,
                "true",
            ]
        )
    return rows


def write_csv(path: Path, headers: list[str], rows: list[list[str]]) -> None:
    with path.open("w", encoding="utf-8", newline="") as handle:
        writer = csv.writer(handle, lineterminator="\n")
        writer.writerow(headers)
        writer.writerows(rows)


def generate_files(repo_root: Path, structure_path: str | None = None) -> dict[str, Any]:
    root = load_structure(structure_path)
    nodes = flatten_structure(root)
    assigned = assign_nodes(nodes)
    out_root = repo_root / "root" / "rulepack" / "title49"
    resolved = out_root.resolve()
    expected = (repo_root / "root" / "rulepack").resolve()
    if expected not in resolved.parents and resolved != expected:
        raise RuntimeError(f"Refusing to write outside rulepack root: {resolved}")
    if out_root.exists():
        shutil.rmtree(out_root)
    out_root.mkdir(parents=True, exist_ok=True)
    all_rows_by_pack: dict[str, dict[str, list[list[str]]]] = {}
    for spec in PACKS:
        pack_dir = out_root / spec.key
        pack_dir.mkdir(parents=True, exist_ok=True)
        rows = rows_for_pack(spec, assigned[spec.key])
        all_rows_by_pack[spec.key] = rows
        for file_name, headers in CSV_HEADERS.items():
            write_csv(pack_dir / file_name, headers, rows[file_name])
    summary = build_summary(nodes, assigned, all_rows_by_pack)
    write_docs(repo_root, summary, assigned, all_rows_by_pack)
    return summary


def build_summary(
    nodes: list[dict[str, Any]],
    assigned: dict[str, list[dict[str, Any]]],
    rows_by_pack: dict[str, dict[str, list[list[str]]]],
) -> dict[str, Any]:
    type_counts = Counter(node["type"] for node in nodes)
    part_nodes = [node for node in nodes if node["type"] == "part"]
    reserved_parts = [node for node in part_nodes if node["reserved"]]
    level_counts = Counter(spec.level for spec in PACKS)
    citation_count = sum(len(citations) for citations in assigned.values())
    fact_count = sum(len(spec.facts) for spec in PACKS)
    rule_count = sum(1 for spec in PACKS for item in spec.facts if not item.derived)
    condition_count = rule_count
    outcome_count = sum(2 for spec in PACKS if any(not item.derived for item in spec.facts))
    metadata_key = "title49.transportation.citation_metadata"
    covered_parts = {
        node["part"]
        for pack_key, pack_nodes in assigned.items()
        if pack_key != metadata_key
        for node in pack_nodes
        if node.get("part")
    }
    unmapped_parts = []
    for node in part_nodes:
        part = node["part"]
        if node["reserved"] or part in covered_parts:
            continue
        unmapped_parts.append(part)
    unmapped_parts = sorted(set(unmapped_parts), key=lambda item: (part_int(item) or 99999, item or ""))
    manual_review = []
    for spec in PACKS:
        manual_review.extend([f"{spec.key}: {item}" for item in spec.manual_review])
    manual_review.extend(
        [
            "Numeric thresholds, exception applicability, route-specific approvals, and document-retention windows need legal/product review before enforcement beyond boolean gate checks.",
            "HMR table row enumeration is modeled as lookup verification against 49 CFR 172.101, not as a row-per-material material-key catalog.",
            "The 9-CSV bundle intentionally has no separate fact-definition CSV; Compliance Core imports fact definitions and audit contracts from rule_fact_requirements.csv.",
        ]
    )
    return {
        "source_date": SOURCE_DATE,
        "type_counts": dict(type_counts),
        "parts": len(part_nodes),
        "reserved_parts": len(reserved_parts),
        "removed_parts": 0,
        "unmapped_parts": len(unmapped_parts),
        "unmapped_part_list": unmapped_parts,
        "pack_count": len(PACKS),
        "level_counts": dict(level_counts),
        "citation_count": citation_count,
        "fact_count": fact_count,
        "rule_count": rule_count,
        "condition_count": condition_count,
        "outcome_count": outcome_count,
        "manual_review": manual_review,
        "program_count": len({spec.program_key for spec in PACKS}),
    }


def write_docs(
    repo_root: Path,
    summary: dict[str, Any],
    assigned: dict[str, list[dict[str, Any]]],
    rows_by_pack: dict[str, dict[str, list[list[str]]]],
) -> None:
    docs = repo_root / "docs" / "compliance-core"
    docs.mkdir(parents=True, exist_ok=True)
    sources = "\n".join(
        [
            f"- eCFR current Title 49: {TITLE_URL}",
            f"- eCFR titles API: {TITLES_URL}",
            f"- eCFR structure API: {STRUCTURE_URL}",
            f"- eCFR full XML API: {FULL_XML_URL}",
            f"- GPO bulk XML: {GPO_XML_URL}",
            f"- FMCSA regulations: {FMCSA_URL}",
            f"- PHMSA HMR reference: {PHMSA_HMR_URL}",
            f"- DOT Part 40: {DOT_PART40_URL}",
        ]
    )
    coverage = f"""# Title 49 coverage report

Source date: {summary['source_date']}

Sources:
{sources}

## Counts

| Metric | Count |
| --- | ---: |
| Parts in current eCFR hierarchy | {summary['parts']} |
| Reserved parts in current hierarchy | {summary['reserved_parts']} |
| Removed parts in current hierarchy | {summary['removed_parts']} |
| Unmapped/non-operational parts retained as metadata | {summary['unmapped_parts']} |
| Rule packs | {summary['pack_count']} |
| Operational packs | {summary['level_counts'].get('operational', 0)} |
| Reference packs | {summary['level_counts'].get('reference', 0)} |
| Citation metadata packs | {summary['level_counts'].get('metadata', 0)} |
| Citations | {summary['citation_count']} |
| Facts | {summary['fact_count']} |
| Rules | {summary['rule_count']} |
| Conditions | {summary['condition_count']} |
| Outcomes | {summary['outcome_count']} |
| Regulatory programs used | {summary['program_count']} |

## Hierarchy coverage

| Hierarchy node type | Count |
| --- | ---: |
"""
    for node_type in ["title", "subtitle", "chapter", "subchapter", "part", "subpart", "section", "appendix"]:
        coverage += f"| {node_type} | {summary['type_counts'].get(node_type, 0)} |\n"
    coverage += "\n## Manual-review areas\n\n"
    coverage += "\n".join(f"- {item}" for item in summary["manual_review"])
    coverage += "\n\n## Metadata-retained parts\n\n"
    coverage += ", ".join(summary["unmapped_part_list"][:200])
    if len(summary["unmapped_part_list"]) > 200:
        coverage += f"\n\n{len(summary['unmapped_part_list']) - 200} additional metadata-retained parts omitted from this display list; see title49.transportation.citation_metadata CSV bundle."
    coverage += "\n"
    (docs / "title49_coverage_report.md").write_text(coverage, encoding="utf-8")

    index = "# Title 49 rulepack index\n\n"
    index += "| Rule pack | Level | Program | Citations | Facts | Products | Entities |\n"
    index += "| --- | --- | --- | ---: | ---: | --- | --- |\n"
    for spec in PACKS:
        index += (
            f"| `{spec.key}` | {spec.level} | `{spec.program_key}` | {len(assigned[spec.key])} | "
            f"{len(spec.facts)} | {', '.join(spec.products)} | {', '.join(spec.entities)} |\n"
        )
    (docs / "title49_rulepack_index.md").write_text(index, encoding="utf-8")

    alignment = "# Title 49 9-CSV alignment\n\n"
    alignment += "The repo already defines the Compliance Core CSV bundle as these nine files. Title 49 uses that existing import shape instead of the fallback names.\n\n"
    alignment += "| CSV | Title 49 use |\n| --- | --- |\n"
    for file_name, headers in CSV_HEADERS.items():
        use = {
            "controlled_vocabulary.csv": "Adds a compliance_domain term for each pack.",
            "vocabulary_aliases.csv": "Reserved for later aliases; generated with headers only.",
            "compliance_keys.csv": "Creates one deterministic compliance key per rule pack.",
            "material_keys.csv": "Reserved; HMR material classification stays in SupplyArr material/SDS facts.",
            "rule_packs.csv": "Creates one pack row and embeds operational rule content JSON where applicable.",
            "rule_requirements.csv": "Creates citation rows for Title 49 hierarchy nodes.",
            "rule_fact_requirements.csv": "Defines the audit-fact contract for each pack/citation: fact key, applicability, source product/entity/record, value semantics, evidence kind, document type, retention, audit question, severity, override, and remediation metadata.",
            "regulatory_mappings.csv": "Maps packs, citations, compliance keys, and fact keys.",
            "sds_references.csv": "Reserved; products own SDS documents and publish facts.",
        }[file_name]
        alignment += f"| `{file_name}` | {use} Headers: `{','.join(headers)}` |\n"
    alignment += "\nFact definitions are not represented by a separate CSV. The Compliance Core importer upserts fact definitions directly from `rule_fact_requirements.csv`, including `value_type`, before it persists pack-specific fact requirement metadata. `tools/compliancecore/import-title49-rulepacks.ps1` still posts exactly the 9 CSV files per bundle.\n"
    alignment += "\nCompliance Core owns rule packs, citations, fact requirements, audit contracts, rule evaluation, evidence references, audit traces, and report surfaces. Product apps own operational records and publish facts and evidence references. The CSVs contain deterministic keys only; no cross-product database foreign keys are introduced.\n"
    (docs / "title49_9_csv_alignment.md").write_text(alignment, encoding="utf-8")

    workflows = "# Title 49 product workflow map\n\n"
    workflow_rows = [
        ("StaffArr", "people/incidents/history/overrides", "Driver qualification, medical status, drug/alcohol events, accident actions, hazmat worker records."),
        ("TrainArr", "ELDT, hazmat, recurrent, retraining, certs", "ELDT, hazmat employee training, recurrent training and retraining facts."),
        ("MaintainArr", "assets, DVIR, inspections, PM, defects, repairs, readiness", "Vehicle parts/accessories, DVIR, annual inspection, roadside correction, out-of-service readiness facts."),
        ("RoutArr", "assignments, dispatch, HOS/ELD, hazmat load gates", "Dispatch gates for driver eligibility, HOS/ELD, cargo securement, hazmat loading/placarding/routing facts."),
        ("SupplyArr", "materials, SDS, classification, packaging, shipment docs", "Hazmat applicability, classification, HMT lookup, packaging, shipping papers, markings, labels, placards facts."),
    ]
    workflows += "| Product | Owned records | Publishes facts for |\n| --- | --- | --- |\n"
    for row in workflow_rows:
        workflows += f"| {row[0]} | {row[1]} | {row[2]} |\n"
    workflows += "\nCompliance Core owns rule packs, citations, fact requirements, audit contracts, rule evaluation, evidence references, audit traces, and report surfaces. Product apps own operational records and publish facts and evidence references. No cross-product DB FKs are introduced.\n"
    workflows += "\nNexArr owns platform admin/auth/entitlement only. Compliance Core is not directly administered by tenant users outside entitled Compliance Core workflows.\n"
    (docs / "title49_product_workflow_map.md").write_text(workflows, encoding="utf-8")

    gaps = "# Title 49 remaining gaps\n\n"
    gaps += "- The current Compliance Core 9-CSV bundle has no separate fact-definition CSV; Compliance Core derives canonical fact definitions from `rule_fact_requirements.csv` during import.\n"
    gaps += "- Numeric thresholds, route approvals, hazmat quantity tables, insurance amount tables, and retention durations are represented as audit fact requirements with source/evidence/retention metadata; product-specific calculators should publish those facts deterministically.\n"
    gaps += "- 49 CFR 172.101 Hazardous Materials Table is mapped as citation and lookup-verification control, not material-key enumeration.\n"
    gaps += "- FMCSA Parts 384-386 and HMR Parts 174-176/179 are reference mapped unless a product workflow currently owns direct operational facts.\n"
    gaps += "- Aviation, maritime, NTSB, Amtrak, TSA, STB, transit, NHTSA, and pipeline areas outside STL's motor-carrier/hazmat workflows are reference or metadata until product support exists.\n"
    gaps += "- Historical removed-part mapping is not generated from current eCFR structure; current reserved parts are retained as inactive citation rows.\n"
    gaps += "- Legal review is required before marking these preproduction packs as customer-enforceable policy.\n"
    (docs / "title49_remaining_gaps.md").write_text(gaps, encoding="utf-8")


def read_csv(path: Path) -> list[dict[str, str]]:
    with path.open("r", encoding="utf-8", newline="") as handle:
        return list(csv.DictReader(handle))


def validate_generated(repo_root: Path) -> list[str]:
    issues: list[str] = []
    out_root = repo_root / "root" / "rulepack" / "title49"
    if not out_root.exists():
        return [f"Missing rulepack root: {out_root}"]
    pack_dirs = [item for item in out_root.iterdir() if item.is_dir()]
    expected_files = set(CSV_HEADERS)
    allowed_operators = {"equals"}
    allowed_fact_operators = {"equals", "all_true", "exists", "not_empty", "current"}
    allowed_value_types = {"boolean", "date", "datetime", "string", "number", "integer", "enum"}
    allowed_evidence_kinds = {"product_record", "document_record", "system_fact", "derived_fact", "external_registry", "inspection_record"}
    allowed_failure_severities = {"info", "minor", "major", "critical", "automatic_failure"}
    seen: dict[str, set[str]] = defaultdict(set)
    all_pack_keys: set[str] = set()
    all_citation_keys: set[str] = set()
    all_compliance_keys: set[str] = set()
    all_material_keys: set[str] = set()
    all_fact_keys: set[str] = set()
    rules_by_pack: dict[str, int] = defaultdict(int)
    conditions_by_pack: dict[str, int] = defaultdict(int)
    outcomes_by_pack: dict[str, int] = defaultdict(int)
    citations_by_pack: dict[str, int] = defaultdict(int)
    rows_by_dir: dict[str, dict[str, list[dict[str, str]]]] = {}
    for pack_dir in pack_dirs:
        actual_files = {path.name for path in pack_dir.glob("*.csv")}
        if actual_files != expected_files:
            issues.append(f"{pack_dir.name}: expected 9 CSV files, found {sorted(actual_files)}")
        rows_by_dir[pack_dir.name] = {}
        for file_name, headers in CSV_HEADERS.items():
            path = pack_dir / file_name
            if not path.exists():
                continue
            with path.open("r", encoding="utf-8", newline="") as handle:
                reader = csv.reader(handle)
                actual = next(reader, [])
            if actual != headers:
                issues.append(f"{pack_dir.name}/{file_name}: header mismatch")
            rows_by_dir[pack_dir.name][file_name] = read_csv(path)
        for row in rows_by_dir[pack_dir.name].get("rule_packs.csv", []):
            all_pack_keys.add(row["pack_key"])
            if row["pack_key"] != pack_dir.name:
                issues.append(f"{pack_dir.name}: rule_packs.csv pack_key {row['pack_key']} does not match directory")
            if row["rule_content_json"]:
                content = json.loads(row["rule_content_json"])
                rules_by_pack[row["pack_key"]] = len(content.get("rules", []))
                conditions_by_pack[row["pack_key"]] = len(content.get("conditions", []))
                outcomes_by_pack[row["pack_key"]] = len(content.get("outcomes", []))
                for condition in content.get("conditions", []):
                    if condition.get("operator") not in allowed_operators:
                        issues.append(f"{pack_dir.name}: unsupported operator {condition.get('operator')}")
                    for product in condition.get("sourceProducts", []):
                        if product not in PRODUCTS:
                            issues.append(f"{pack_dir.name}: unknown product {product}")
            else:
                rules_by_pack[row["pack_key"]] = 0
        for row in rows_by_dir[pack_dir.name].get("rule_requirements.csv", []):
            all_citation_keys.add(row["citation_key"])
            citations_by_pack[row["pack_key"]] += 1
            if row["citation_key"] in seen["citation_key"]:
                issues.append(f"Duplicate citation_key {row['citation_key']}")
            seen["citation_key"].add(row["citation_key"])
        for row in rows_by_dir[pack_dir.name].get("compliance_keys.csv", []):
            all_compliance_keys.add(row["key"])
        for row in rows_by_dir[pack_dir.name].get("material_keys.csv", []):
            all_material_keys.add(row["key"])
        for row in rows_by_dir[pack_dir.name].get("rule_fact_requirements.csv", []):
            all_fact_keys.add(row["fact_key"])
            if row["requirement_key"] in seen["requirement_key"]:
                issues.append(f"Duplicate requirement_key {row['requirement_key']}")
            seen["requirement_key"].add(row["requirement_key"])
            if row["pack_key"] not in all_pack_keys and row["pack_key"] != pack_dir.name:
                issues.append(f"{pack_dir.name}: orphan fact pack {row['pack_key']}")
            if row["citation_key"] and row["citation_key"] not in all_citation_keys:
                issues.append(f"{pack_dir.name}: orphan fact citation {row['citation_key']}")
            products_match = re.search(r"products=([^;]+)", row["description"])
            entities_match = re.search(r"entities=([^;]+)", row["description"])
            if not products_match or not entities_match:
                issues.append(f"{pack_dir.name}: fact {row['fact_key']} missing products/entities metadata")
            elif any(product not in PRODUCTS for product in products_match.group(1).split(",")):
                issues.append(f"{pack_dir.name}: fact {row['fact_key']} has unknown product metadata")
            required_metadata = [
                "applicability_key",
                "source_product",
                "source_entity",
                "source_field_or_record_type",
                "value_type",
                "operator",
                "expected_value",
                "evidence_kind",
                "retention_period",
                "audit_question",
                "failure_severity",
                "automatic_failure_flag",
                "override_allowed",
                "remediation_required",
            ]
            for column in required_metadata:
                if not row.get(column):
                    issues.append(f"{pack_dir.name}: fact {row['fact_key']} missing {column}")
            if row.get("operator") and row["operator"] not in allowed_fact_operators:
                issues.append(f"{pack_dir.name}: fact {row['fact_key']} has unsupported fact operator {row['operator']}")
            if row.get("value_type") and row["value_type"] not in allowed_value_types:
                issues.append(f"{pack_dir.name}: fact {row['fact_key']} has unsupported value_type {row['value_type']}")
            if row.get("evidence_kind") and row["evidence_kind"] not in allowed_evidence_kinds:
                issues.append(f"{pack_dir.name}: fact {row['fact_key']} has unsupported evidence_kind {row['evidence_kind']}")
            if row.get("failure_severity") and row["failure_severity"] not in allowed_failure_severities:
                issues.append(f"{pack_dir.name}: fact {row['fact_key']} has unsupported failure_severity {row['failure_severity']}")
            for product in (row.get("source_product") or "").split(","):
                if product and product not in PRODUCTS:
                    issues.append(f"{pack_dir.name}: fact {row['fact_key']} has unknown source_product {product}")
            if row.get("automatic_failure_flag") not in {"true", "false"}:
                issues.append(f"{pack_dir.name}: fact {row['fact_key']} has invalid automatic_failure_flag")
            if row.get("override_allowed") not in {"true", "false"}:
                issues.append(f"{pack_dir.name}: fact {row['fact_key']} has invalid override_allowed")
            if row.get("remediation_required") not in {"true", "false"}:
                issues.append(f"{pack_dir.name}: fact {row['fact_key']} has invalid remediation_required")
            is_derived = row.get("evidence_kind") == "derived_fact"
            if is_derived:
                if row.get("operator") != "all_true":
                    issues.append(f"{pack_dir.name}: derived fact {row['fact_key']} must use all_true")
                components = [item.strip() for item in row.get("expected_value", "").split(",") if item.strip()]
                if not components:
                    issues.append(f"{pack_dir.name}: derived fact {row['fact_key']} missing component fact keys")
                for component in components:
                    if component not in all_fact_keys:
                        issues.append(f"{pack_dir.name}: derived fact {row['fact_key']} references unknown component {component}")
            else:
                if row.get("operator") == "all_true":
                    issues.append(f"{pack_dir.name}: non-derived fact {row['fact_key']} cannot use all_true")
                for column in ["source_product", "source_entity", "source_field_or_record_type", "audit_question", "failure_severity", "remediation_required"]:
                    if not row.get(column):
                        issues.append(f"{pack_dir.name}: non-derived fact {row['fact_key']} missing audit metadata {column}")
                if row["fact_key"].endswith("_file_complete") or row["fact_key"].endswith("_program_complete"):
                    issues.append(f"{pack_dir.name}: broad rollup fact {row['fact_key']} must be emitted as derived_fact")
        for row in rows_by_dir[pack_dir.name].get("regulatory_mappings.csv", []):
            if row["mapping_key"] in seen["mapping_key"]:
                issues.append(f"Duplicate mapping_key {row['mapping_key']}")
            seen["mapping_key"].add(row["mapping_key"])
            if row["pack_key"] and row["pack_key"] not in all_pack_keys and row["pack_key"] != pack_dir.name:
                issues.append(f"{pack_dir.name}: orphan mapping pack {row['pack_key']}")
            if row["citation_key"] and row["citation_key"] not in all_citation_keys:
                issues.append(f"{pack_dir.name}: orphan mapping citation {row['citation_key']}")
            if row["compliance_key"] and row["compliance_key"] not in all_compliance_keys:
                issues.append(f"{pack_dir.name}: orphan compliance key {row['compliance_key']}")
            if row["material_key"] and row["material_key"] not in all_material_keys:
                issues.append(f"{pack_dir.name}: orphan material key {row['material_key']}")
    operational = {spec.key for spec in PACKS if spec.level == "operational"}
    reference = {spec.key for spec in PACKS if spec.level == "reference"}
    for pack_key in operational:
        if rules_by_pack.get(pack_key, 0) == 0:
            issues.append(f"{pack_key}: operational pack has no rules")
        if conditions_by_pack.get(pack_key, 0) == 0:
            issues.append(f"{pack_key}: operational pack has no conditions")
        if outcomes_by_pack.get(pack_key, 0) == 0:
            issues.append(f"{pack_key}: operational pack has no outcomes")
    for pack_key in reference:
        if citations_by_pack.get(pack_key, 0) == 0:
            issues.append(f"{pack_key}: reference pack has no citation coverage")
    for doc in [
        "title49_coverage_report.md",
        "title49_rulepack_index.md",
        "title49_9_csv_alignment.md",
        "title49_product_workflow_map.md",
        "title49_remaining_gaps.md",
    ]:
        if not (repo_root / "docs" / "compliance-core" / doc).exists():
            issues.append(f"Missing doc {doc}")
    return issues


def main(argv: list[str] | None = None) -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--structure-json", help="Optional eCFR Title 49 structure JSON path")
    parser.add_argument("--validate-only", action="store_true")
    args = parser.parse_args(argv)
    repo_root = Path(__file__).resolve().parents[2]
    if not args.validate_only:
        summary = generate_files(repo_root, args.structure_json)
        print(
            textwrap.dedent(
                f"""
                Generated Title 49 rule packs:
                  packs: {summary['pack_count']}
                  citations: {summary['citation_count']}
                  facts/rules/conditions: {summary['fact_count']}/{summary['rule_count']}/{summary['condition_count']}
                  outcomes: {summary['outcome_count']}
                  source date: {summary['source_date']}
                """
            ).strip()
        )
    issues = validate_generated(repo_root)
    if issues:
        print("Title 49 rulepack validation failed:", file=sys.stderr)
        for issue in issues:
            print(f"- {issue}", file=sys.stderr)
        return 1
    print("Title 49 rulepack validation passed.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
