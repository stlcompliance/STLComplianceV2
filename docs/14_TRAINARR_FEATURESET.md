# TrainArr Feature Set

## Product Definition

TrainArr is the training proof and qualification workflow system.

## Owns

- training definitions
- programs
- program versions
- requirements
- assignments
- steps
- evidence
- tests
- evaluations
- signoffs
- completions
- recertification
- retraining
- qualifications
- qualification publication events

## Does Not Own

- platform login
- person source of truth
- org source of truth
- maintenance execution
- dispatch execution
- procurement
- rule packs

## Core Features

- program builder
- program versioning
- requirement mapping
- assignment engine
- trainee completion flow
- evidence upload
- trainer/evaluator signoff
- quiz/test/practical steps
- qualification issue/suspend/revoke/expire
- publication to StaffArr
- authorization check API
- training matrix
- audit package

## Required API Surfaces

- `/api/training-definitions`
- `/api/training-programs`
- `/api/program-versions`
- `/api/training-requirements`
- `/api/training-assignments`
- `/api/training-evidence`
- `/api/signoffs`
- `/api/evaluations`
- `/api/completions`
- `/api/qualifications`
- `/api/qualification-checks`
- `/api/certification-publications`
- `/health`

## Completion Definition

A tenant can define training, assign it, capture evidence, sign off results, issue qualifications, publish them to StaffArr, and support authorization checks.
