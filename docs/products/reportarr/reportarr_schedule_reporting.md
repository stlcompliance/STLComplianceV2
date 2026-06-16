# ReportArr Schedule Reporting

## Purpose

This document defines the event and projection inputs ReportArr uses for schedule reporting without mutating product workflows.

## Ownership

ReportArr owns reporting definitions, analytics datasets, schedule adherence reporting, backlog reporting, utilization reporting, and report snapshots.

ReportArr does not own or correct source operational records. Corrections happen in the source product.

## Required Inputs

ReportArr schedule reporting may consume:

- created events
- requested or promised window events
- schedulable demand created events
- scheduled, rescheduled, unscheduled, cancelled, completed events
- validation warning and blocker summaries when published
- product-owned read models with source/freshness metadata

## Core Metrics

ReportArr schedule datasets should support:

- unscheduled backlog count
- scheduled count
- completed count
- created-to-scheduled latency
- scheduled-to-completed latency
- missed requested window count
- missed promised window count
- reschedule count and reasons
- cancellation count and reasons
- utilization by resource lane
- conflict rate by conflict type
- stale or partial source count

## Source Requirements

Every schedule metric must identify:

- metric owner
- source product
- source record type
- time basis
- calculation basis
- freshness
- drill-in path

ReportArr must distinguish unscheduled demand, scheduled execution, and completed work.
