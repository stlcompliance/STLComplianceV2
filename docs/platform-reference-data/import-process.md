# Platform Reference Data Import Process

## Goals

The import process should turn raw external data into reviewable reference candidates, not into silent canonical truth.

## Supported processors

Initial processors:

1. CSV generic import
2. Vehicle taxonomy CSV import
3. UPC/GTIN product CSV import
4. SDS metadata CSV import
5. SDS document metadata intake
6. Manual single-record create/edit

Optional scaffolds:

- NHTSA/vPIC VIN decode connector
- GS1/GTIN lookup connector placeholder
- SDS parser placeholder
- heavy equipment taxonomy source placeholder
- vendor catalog source placeholder

## Import flow

1. Choose dataset.
2. Choose source.
3. Upload file or select connector.
4. Create ingestion job.
5. Save raw intake.
6. Parse and normalize rows.
7. Create staging records.
8. Detect duplicates and conflicts.
9. Assign confidence.
10. Send low-confidence or conflicting rows to review.
11. Approve, merge, reject, or escalate.
12. Publish dataset version when ready.

## Raw intake

Raw intake should be preserved for traceability.

Raw input may include:

- original file name
- object key or upload reference
- connector response payload
- row number
- parsed raw payload

Raw intake is audit support, not the primary user interface.

## Staging records

Staging records are the reviewer-facing artifact.

Each staging record should show:

- normalized summary
- proposed entity type
- proposed canonical key
- confidence
- reason for review
- source evidence summary

The main review UI should avoid raw JSON unless the user opens an explicit technical panel.

## Review decisions

- Approve: create a canonical entity or new version.
- Merge: link to an existing canonical entity and update crosswalks.
- Reject: keep the record in history but do not publish it.
- Escalate: route to a human owner or higher-trust review queue.

## SDS handling

SDS metadata imports should:

- normalize manufacturer name
- normalize product name
- capture revision date
- preserve document linkage to RecordArr
- route regulatory interpretation candidates to Compliance Core

The document file itself stays in RecordArr.

## Vehicle handling

Vehicle imports should:

- uppercase VIN
- normalize year as integer
- standardize make/model casing
- retain source decode fields as evidence

## GTIN handling

GTIN/UPC imports should:

- strip whitespace
- validate numeric format
- preserve the original scanned code
- infer packaging/UOM only when confidence is high
- avoid creating a SupplyArr SKU automatically

## Manual entry

Manual entry should behave like a controlled import:

- create a job
- create a staging record
- require review reason when appropriate
- keep the audit trail

## Publishing

Publishing should only happen after review criteria are satisfied.

Publish should:

- create a dataset version
- update the current published version pointer
- preserve prior versions
- emit publish and entity events

## Reprocessing

If a source changes:

- create a new job
- compare against prior published entities
- avoid silent overwrite
- keep superseded versions visible
