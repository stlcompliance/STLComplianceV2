# StaffArr — Person Workspace and Delegated Account Management

## Purpose

Provide one coherent person workspace while preserving StaffArr person ownership and NexArr account/login ownership.

## Person pages

Every person has: Overview, Permissions, Certifications, Assignments, Training, Incidents, Documents, and History. The overview is read-first with status/readiness/manager/site/contact summaries and clear actions.

## Edit person

Permissioned users may edit StaffArr-owned identity/profile/employment/org/location fields through explicit edit mode. Cross-product fields are owner-backed and raw IDs are hidden.

## Login/account delegation

Appropriately permissioned StaffArr users may provision, enable, disable, recover, or edit NexArr-owned login/account information through NexArr-backed actions/APIs displayed in the person workspace. “Owned by NexArr” is a write/source boundary, not a ban on delegated StaffArr UI.

The page clearly separates Person details from Login & account, records both owner systems, and never directly writes NexArr tables.

## Quick create

Person references in other products may open a minimal StaffArr quick-create flow. A login is optional and provisioned separately or during onboarding when authorized.
