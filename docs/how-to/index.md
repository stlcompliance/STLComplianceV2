# STL Compliance How-To Library

These guides are task-based operator and admin documentation for STL Compliance. They follow the repository constitutions: NexArr owns platform access, StaffArr owns people and internal places, each product owns execution in its lane, Compliance Core owns rule meaning, RecordArr owns stored files, and ReportArr reports without correcting source data.

## Support Status Legend
- Supported by current UI/API: the current routes, pages, product docs, and API surface support the workflow well enough for user documentation.
- Supported by product contract/docs: the governing product docs define the workflow and ownership, but final visible UI labels may still vary by deployment.
- Intended workflow partially supported by current routes/docs: the product docs or route structure describe the workflow, but some labels, screens, or end-to-end controls still need confirmation.
- Boundary guidance: the page exists to prevent cross-product ownership drift and points users to the owning product.
- Placeholder: the workflow is not defined well enough or conflicts with the ownership constitution, so this page records current state, expected direction, and open questions.

## By Product
### Platform Access
- [How to sign in to STL Compliance](platform/sign-in-to-stl-compliance.md) - All users - Supported by current UI/API
- [How to switch products](platform/switch-products.md) - All users - Supported by current UI/API
- [How to invite or create a user](platform/invite-or-create-a-user.md) - Platform admins and tenant admins - Supported by current UI/API
- [How to give a user product access](platform/give-a-user-product-access.md) - Platform admins, tenant admins, and StaffArr permission admins - Supported by current UI/API
- [How to remove or deactivate access](platform/remove-or-deactivate-access.md) - Platform admins, tenant admins, and StaffArr permission admins - Supported by current UI/API
- [How to understand platform admin versus product permissions](platform/understand-platform-admin-versus-product-permissions.md) - Admins and product owners - Supported by current UI/API
- [How to troubleshoot login or entitlement problems](platform/troubleshoot-login-or-entitlement-problems.md) - Platform admins and support users - Supported by current UI/API

### StaffArr
- [How to create a person](staffarr/create-a-person.md) - HR, operations managers, and StaffArr admins - Supported by current UI/API
- [How to create an organization unit](staffarr/create-an-organization-unit.md) - StaffArr admins and operations leaders - Supported by current UI/API
- [How to create a site](staffarr/create-a-site.md) - StaffArr admins and facilities or operations managers - Supported by current UI/API
- [How to create a location](staffarr/create-a-location.md) - StaffArr admins and operations managers - Supported by current UI/API
- [How to create departments, positions, and teams](staffarr/create-departments-positions-and-teams.md) - StaffArr admins and department leaders - Supported by current UI/API
- [How to assign a role](staffarr/assign-a-role.md) - StaffArr permission admins and tenant admins - Supported by current UI/API
- [How to edit role permissions](staffarr/edit-role-permissions.md) - StaffArr permission admins and product owners - Supported by current UI/API
- [How to deactivate or offboard a person](staffarr/deactivate-or-offboard-a-person.md) - HR, operations managers, and StaffArr admins - Supported by current UI/API
- [How to view a reporting hierarchy](staffarr/view-a-reporting-hierarchy.md) - Managers, HR, and operations leaders - Supported by current UI/API
- [How to report an incident](staffarr/report-an-incident.md) - Managers, safety users, HR, and authorized staff - Supported by current UI/API
- [How to review incidents tied to a person](staffarr/review-incidents-tied-to-a-person.md) - Managers, HR, safety users, and StaffArr admins - Supported by current UI/API
- [How to handle a training-related incident](staffarr/handle-a-training-related-incident.md) - Managers, safety users, HR, and training coordinators - Supported by current UI/API with intended cross-product follow-up

### TrainArr
- [How to create a training program](trainarr/create-a-training-program.md) - Training admins and program owners - Intended workflow partially supported by current routes/docs
- [How to create a certification requirement](trainarr/create-a-certification-requirement.md) - Training admins and compliance managers - Intended workflow partially supported by current routes/docs
- [How to assign training to a person](trainarr/assign-training-to-a-person.md) - Training admins, managers, and supervisors - Intended workflow partially supported by current routes/docs
- [How to complete a training step](trainarr/complete-a-training-step.md) - Trainees, trainers, and training admins - Supported by current UI/API
- [How to sign off trainee completion](trainarr/sign-off-trainee-completion.md) - Trainees and authorized training admins - Supported by current UI/API
- [How to sign off trainer completion](trainarr/sign-off-trainer-completion.md) - Trainers and training admins - Supported by current UI/API
- [How to issue a certificate](trainarr/issue-a-certificate.md) - Training admins and qualification managers - Intended workflow partially supported by current routes/docs
- [How to review expiring certifications](trainarr/review-expiring-certifications.md) - Training admins, compliance managers, and supervisors - Supported by current UI/API with intended renewal workflow
- [How to assign retraining after an incident](trainarr/assign-retraining-after-an-incident.md) - Training admins, safety users, and managers - Intended workflow partially supported by current routes/docs
- [How to handle failed or incomplete training](trainarr/handle-failed-or-incomplete-training.md) - Training admins, trainers, and managers - Supported by current UI/API with intended remediation follow-up

### MaintainArr
- [How to create an asset](maintainarr/create-an-asset.md) - Maintenance admins and asset managers - Supported by current UI/API
- [How to edit asset details](maintainarr/edit-asset-details.md) - Maintenance admins and asset managers - Supported by current UI/API
- [How to create a work order](maintainarr/create-a-work-order.md) - Maintenance planners, technicians, and supervisors - Supported by current UI/API
- [How to create a defect report](maintainarr/create-a-defect-report.md) - Technicians, operators, supervisors, and maintenance admins - Supported by current UI/API
- [How to complete an inspection](maintainarr/complete-an-inspection.md) - Technicians, inspectors, and maintenance supervisors - Intended workflow partially supported by current routes/docs
- [How to create an inspection template](maintainarr/create-an-inspection-template.md) - Maintenance admins and compliance managers - Supported by current UI/API
- [How to create a preventive maintenance program](maintainarr/create-a-preventive-maintenance-program.md) - Maintenance planners and maintenance admins - Supported by current UI/API
- [How to generate work from a preventive maintenance program](maintainarr/generate-work-from-a-preventive-maintenance-program.md) - Maintenance planners and supervisors - Intended workflow partially supported by current routes/docs
- [How to request parts for a work order](maintainarr/request-parts-for-a-work-order.md) - Maintenance planners, technicians, and parts coordinators - Supported by current UI/API
- [How to attach documents or photos to an asset](maintainarr/attach-documents-or-photos-to-an-asset.md) - Maintenance users and records coordinators - Intended workflow partially supported by current routes/docs
- [How to update asset readiness](maintainarr/update-asset-readiness.md) - Maintenance supervisors and asset managers - Supported by current UI/API with product-owned validation
- [How to close a work order](maintainarr/close-a-work-order.md) - Technicians, maintenance supervisors, and maintenance admins - Supported by current UI/API

### CustomArr
- [How to create a customer](customarr/create-a-customer.md) - Customer operations users, account managers, and onboarding reviewers - Supported by product contract/docs
- [How to add a customer contact](customarr/add-a-customer-contact.md) - Customer operations users and account managers - Supported by product contract/docs
- [How to add a customer location](customarr/add-a-customer-location.md) - Customer operations users, account managers, dispatch coordinators, and compliance users - Supported by product contract/docs
- [How to review customer onboarding](customarr/review-customer-onboarding.md) - Onboarding reviewers, account managers, and compliance users - Supported by product contract/docs
- [How to check customer eligibility](customarr/check-customer-eligibility.md) - Customer service users, order coordinators, dispatch coordinators, and managers - Supported by product contract/docs

### OrdArr
- [How to create an order request](ordarr/create-an-order-request.md) - Customer service users, order coordinators, and operations coordinators - Supported by product contract/docs
- [How to triage an order request](ordarr/triage-an-order-request.md) - Order coordinators, customer service users, and operations managers - Supported by product contract/docs
- [How to track order handoffs](ordarr/track-order-handoffs.md) - Order coordinators, dispatch coordinators, customer service users, and managers - Supported by product contract/docs
- [How to close an order request](ordarr/close-an-order-request.md) - Order coordinators, operations managers, and customer service users - Supported by product contract/docs
- [How to prepare financial handoff packets](ordarr/prepare-financial-handoff-packets.md) - Order coordinators, billing preparation users, and managers - Supported by product contract/docs

### SupplyArr
- [How to create a vendor](supplyarr/create-a-vendor.md) - Supply chain users and SupplyArr admins - Supported by current UI/API
- [How to handle customer context in SupplyArr](supplyarr/create-a-customer.md) - Sales, operations, and supply chain users - Boundary guidance
- [How to create a part](supplyarr/create-a-part.md) - Supply chain users, maintenance planners, and inventory coordinators - Supported by current UI/API
- [How to create a purchase order](supplyarr/create-a-purchase-order.md) - Supply chain users and purchasing approvers - Supported by current UI/API with vendor-order surface
- [How to send or track a purchase order](supplyarr/send-or-track-a-purchase-order.md) - Supply chain users and purchasing coordinators - Supported by current UI/API with intended external handoff
- [How to update vendor order status](supplyarr/update-vendor-order-status.md) - Supply chain users and vendor portal users - Supported by current UI/API
- [How to review pricing and lead-time snapshots](supplyarr/review-pricing-and-lead-time-snapshots.md) - Supply chain users, planners, and purchasing managers - Supported by current UI/API
- [How to handle a backordered part](supplyarr/handle-a-backordered-part.md) - Supply chain users, maintenance planners, and inventory coordinators - Supported by current UI/API with cross-product follow-up
- [How to connect procurement expectations to receiving](supplyarr/connect-procurement-expectations-to-receiving.md) - Supply chain users and warehouse coordinators - Supported by current UI/API with intended handoff flow

### LoadArr
- [How to receive inbound goods](loadarr/receive-inbound-goods.md) - Warehouse receivers and inventory coordinators - Supported by current UI/API
- [How to check in a dock appointment](loadarr/check-in-a-dock-appointment.md) - Warehouse receivers and dock coordinators - Intended workflow partially supported by current routes/docs
- [How to stage received items](loadarr/stage-received-items.md) - Warehouse receivers and inventory coordinators - Supported by current UI/API
- [How to handle receiving exceptions](loadarr/handle-receiving-exceptions.md) - Warehouse receivers, inventory supervisors, and supply chain users - Supported by current UI/API
- [How to move inventory to the parts room](loadarr/move-inventory-to-the-parts-room.md) - Warehouse users and maintenance parts coordinators - Supported by current UI/API
- [How to put away inventory](loadarr/put-away-inventory.md) - Warehouse receivers and inventory coordinators - Supported by current UI/API
- [How to transfer inventory between locations](loadarr/transfer-inventory-between-locations.md) - Warehouse users and inventory coordinators - Supported by current UI/API
- [How to quarantine received items](loadarr/quarantine-received-items.md) - Warehouse receivers, quality users, and inventory supervisors - Supported by current UI/API
- [How to release items from quarantine](loadarr/release-items-from-quarantine.md) - Warehouse supervisors, quality users, and inventory admins - Supported by current UI/API
- [How to confirm parts available for a work order](loadarr/confirm-parts-available-for-a-work-order.md) - Maintenance parts coordinators and warehouse users - Supported by current UI/API with cross-product demand

### RoutArr
- [How to create a dispatch](routarr/create-a-dispatch.md) - Dispatchers and transportation supervisors - Supported by current UI/API with intended validation gates
- [How to assign a driver](routarr/assign-a-driver.md) - Dispatchers and transportation supervisors - Supported by current UI/API with readiness checks
- [How to assign equipment](routarr/assign-equipment.md) - Dispatchers and transportation supervisors - Supported by current UI/API with readiness checks
- [How to create a route](routarr/create-a-route.md) - Dispatchers and route planners - Supported by current UI/API with route planner surface
- [How to add stops to a route](routarr/add-stops-to-a-route.md) - Dispatchers and route planners - Supported by current UI/API with route planner surface
- [How to update trip status](routarr/update-trip-status.md) - Drivers, dispatchers, and transportation supervisors - Supported by current UI/API with driver portal surface
- [How to notify LoadArr about an inbound delivery](routarr/notify-loadarr-about-an-inbound-delivery.md) - Dispatchers, dock coordinators, and warehouse supervisors - Supported by current UI/API with intended dock handoff
- [How to handle delays, missed stops, or exceptions](routarr/handle-delays-missed-stops-or-exceptions.md) - Drivers, dispatchers, and transportation supervisors - Supported by current UI/API
- [How to complete a trip](routarr/complete-a-trip.md) - Drivers, dispatchers, and transportation supervisors - Supported by current UI/API with proof-review surface

### AssurArr
- [How to create a nonconformance](assurarr/create-a-nonconformance.md) - Quality technicians, quality reviewers, managers, and authorized operations users - Supported by current UI/API
- [How to place or release a quality hold](assurarr/place-or-release-a-quality-hold.md) - Quality reviewers, quality managers, warehouse supervisors, maintenance supervisors, and release approvers - Supported by current UI/API
- [How to create and close a CAPA](assurarr/create-and-close-a-capa.md) - Quality managers, quality reviewers, CAPA owners, auditors, and process owners - Supported by current UI/API
- [How to review a supplier quality issue](assurarr/review-a-supplier-quality-issue.md) - Supplier quality managers, supply chain users, quality reviewers, and purchasing managers - Supported by current UI/API
- [How to handle a customer complaint quality case](assurarr/handle-a-customer-complaint-quality-case.md) - Customer quality managers, account owners, quality reviewers, and operations managers - Supported by current UI/API
- [How to run a quality audit and findings](assurarr/run-a-quality-audit-and-findings.md) - Quality auditors, quality managers, process owners, and finding reviewers - Supported by current UI/API

### Compliance Core
- [How to access Compliance Core as a platform admin](compliancecore/access-compliance-core-as-a-platform-admin.md) - Platform admins and compliance admins - Supported by current UI/API
- [How to import rule reference data](compliancecore/import-rule-reference-data.md) - Compliance admins and rulepack managers - Supported by current UI/API
- [How to review staged import rows](compliancecore/review-staged-import-rows.md) - Compliance admins and data stewards - Supported by current UI/API
- [How to map uploaded documents to requirements](compliancecore/map-uploaded-documents-to-requirements.md) - Compliance admins and records coordinators - Supported by current UI/API
- [How to evaluate a theoretical situation](compliancecore/evaluate-a-theoretical-situation.md) - Compliance admins, risk analysts, and product owners - Supported by current UI/API
- [How to review governing bodies](compliancecore/review-governing-bodies.md) - Compliance admins and legal/compliance reviewers - Supported by current UI/API
- [How to review citations](compliancecore/review-citations.md) - Compliance admins, auditors, and rule reviewers - Supported by current UI/API
- [How to update controlled vocabulary](compliancecore/update-controlled-vocabulary.md) - Compliance admins and data stewards - Supported by current API/docs with admin surface to confirm
- [How to troubleshoot missing rule matches](compliancecore/troubleshoot-missing-rule-matches.md) - Compliance admins and product owners - Supported by current UI/API

### Field Companion
- [How to view assigned work](fieldcompanion/view-assigned-work.md) - Field workers, drivers, technicians, and supervisors - Supported by current UI/API
- [How to complete a mobile inspection](fieldcompanion/complete-a-mobile-inspection.md) - Technicians, inspectors, and field workers - Supported by current UI/API
- [How to report a defect from the field](fieldcompanion/report-a-defect-from-the-field.md) - Field workers, technicians, and supervisors - Intended workflow partially supported by current routes/docs
- [How to upload a photo or field evidence](fieldcompanion/upload-a-photo-or-field-evidence.md) - Field workers, inspectors, drivers, and technicians - Supported by current UI/API with evidence workflow
- [How to acknowledge a task](fieldcompanion/acknowledge-a-task.md) - Field workers and assigned users - Supported by current UI/API
- [How to complete a training step from mobile](fieldcompanion/complete-a-training-step-from-mobile.md) - Trainees, trainers, and field supervisors - Intended workflow not fully exposed in current Field Companion UI
- [How to view asset or location context in the field](fieldcompanion/view-asset-or-location-context-in-the-field.md) - Field workers, inspectors, drivers, and technicians - Supported by current UI/API with task-specific context

### RecordArr
- [How to find a record](recordarr/find-a-record.md) - Records coordinators, auditors, managers, and product users - Supported by current UI/API
- [How to upload a document](recordarr/upload-a-document.md) - Records coordinators and authorized product users - Supported by current UI/API
- [How to attach a document to a source record](recordarr/attach-a-document-to-a-source-record.md) - Records coordinators and product users - Supported by current UI/API with source-product coordination
- [How to review audit-ready records](recordarr/review-audit-ready-records.md) - Records coordinators, compliance users, and auditors - Supported by current UI/API

### ReportArr
- [How to generate a report](reportarr/generate-a-report.md) - Report builders, managers, auditors, and admins - Supported by current UI/API
- [How to filter a report](reportarr/filter-a-report.md) - Report users, managers, and auditors - Supported by current UI/API
- [How to export a report](reportarr/export-a-report.md) - Report users, managers, auditors, and admins - Supported by current UI/API
- [How to review audit-ready records](reportarr/review-audit-ready-records.md) - Auditors, compliance managers, report users, and records coordinators - Supported by current UI/API

## Coverage Summary
- Fully supported or current UI/API-backed workflows: 79
- Intended, product-contract-backed, or partially implemented workflows: 30
- Placeholder or ownership-conflict workflows: 0

## Fully Supported Workflows
- Platform Access: [How to sign in to STL Compliance](platform/sign-in-to-stl-compliance.md)
- Platform Access: [How to switch products](platform/switch-products.md)
- Platform Access: [How to invite or create a user](platform/invite-or-create-a-user.md)
- Platform Access: [How to give a user product access](platform/give-a-user-product-access.md)
- Platform Access: [How to remove or deactivate access](platform/remove-or-deactivate-access.md)
- Platform Access: [How to understand platform admin versus product permissions](platform/understand-platform-admin-versus-product-permissions.md)
- Platform Access: [How to troubleshoot login or entitlement problems](platform/troubleshoot-login-or-entitlement-problems.md)
- StaffArr: [How to create a person](staffarr/create-a-person.md)
- StaffArr: [How to create an organization unit](staffarr/create-an-organization-unit.md)
- StaffArr: [How to create a site](staffarr/create-a-site.md)
- StaffArr: [How to create a location](staffarr/create-a-location.md)
- StaffArr: [How to create departments, positions, and teams](staffarr/create-departments-positions-and-teams.md)
- StaffArr: [How to assign a role](staffarr/assign-a-role.md)
- StaffArr: [How to edit role permissions](staffarr/edit-role-permissions.md)
- StaffArr: [How to deactivate or offboard a person](staffarr/deactivate-or-offboard-a-person.md)
- StaffArr: [How to view a reporting hierarchy](staffarr/view-a-reporting-hierarchy.md)
- StaffArr: [How to report an incident](staffarr/report-an-incident.md)
- StaffArr: [How to review incidents tied to a person](staffarr/review-incidents-tied-to-a-person.md)
- TrainArr: [How to complete a training step](trainarr/complete-a-training-step.md)
- TrainArr: [How to sign off trainee completion](trainarr/sign-off-trainee-completion.md)
- TrainArr: [How to sign off trainer completion](trainarr/sign-off-trainer-completion.md)
- MaintainArr: [How to create an asset](maintainarr/create-an-asset.md)
- MaintainArr: [How to edit asset details](maintainarr/edit-asset-details.md)
- MaintainArr: [How to create a work order](maintainarr/create-a-work-order.md)
- MaintainArr: [How to create a defect report](maintainarr/create-a-defect-report.md)
- MaintainArr: [How to create an inspection template](maintainarr/create-an-inspection-template.md)
- MaintainArr: [How to create a preventive maintenance program](maintainarr/create-a-preventive-maintenance-program.md)
- MaintainArr: [How to request parts for a work order](maintainarr/request-parts-for-a-work-order.md)
- MaintainArr: [How to update asset readiness](maintainarr/update-asset-readiness.md)
- MaintainArr: [How to close a work order](maintainarr/close-a-work-order.md)
- SupplyArr: [How to create a vendor](supplyarr/create-a-vendor.md)
- SupplyArr: [How to create a part](supplyarr/create-a-part.md)
- SupplyArr: [How to create a purchase order](supplyarr/create-a-purchase-order.md)
- SupplyArr: [How to update vendor order status](supplyarr/update-vendor-order-status.md)
- SupplyArr: [How to review pricing and lead-time snapshots](supplyarr/review-pricing-and-lead-time-snapshots.md)
- SupplyArr: [How to handle a backordered part](supplyarr/handle-a-backordered-part.md)
- LoadArr: [How to receive inbound goods](loadarr/receive-inbound-goods.md)
- LoadArr: [How to stage received items](loadarr/stage-received-items.md)
- LoadArr: [How to handle receiving exceptions](loadarr/handle-receiving-exceptions.md)
- LoadArr: [How to move inventory to the parts room](loadarr/move-inventory-to-the-parts-room.md)
- LoadArr: [How to put away inventory](loadarr/put-away-inventory.md)
- LoadArr: [How to transfer inventory between locations](loadarr/transfer-inventory-between-locations.md)
- LoadArr: [How to quarantine received items](loadarr/quarantine-received-items.md)
- LoadArr: [How to release items from quarantine](loadarr/release-items-from-quarantine.md)
- LoadArr: [How to confirm parts available for a work order](loadarr/confirm-parts-available-for-a-work-order.md)
- RoutArr: [How to assign a driver](routarr/assign-a-driver.md)
- RoutArr: [How to assign equipment](routarr/assign-equipment.md)
- RoutArr: [How to create a route](routarr/create-a-route.md)
- RoutArr: [How to add stops to a route](routarr/add-stops-to-a-route.md)
- RoutArr: [How to update trip status](routarr/update-trip-status.md)
- RoutArr: [How to handle delays, missed stops, or exceptions](routarr/handle-delays-missed-stops-or-exceptions.md)
- RoutArr: [How to complete a trip](routarr/complete-a-trip.md)
- AssurArr: [How to create a nonconformance](assurarr/create-a-nonconformance.md)
- AssurArr: [How to place or release a quality hold](assurarr/place-or-release-a-quality-hold.md)
- AssurArr: [How to create and close a CAPA](assurarr/create-and-close-a-capa.md)
- AssurArr: [How to review a supplier quality issue](assurarr/review-a-supplier-quality-issue.md)
- AssurArr: [How to handle a customer complaint quality case](assurarr/handle-a-customer-complaint-quality-case.md)
- AssurArr: [How to run a quality audit and findings](assurarr/run-a-quality-audit-and-findings.md)
- Compliance Core: [How to access Compliance Core as a platform admin](compliancecore/access-compliance-core-as-a-platform-admin.md)
- Compliance Core: [How to import rule reference data](compliancecore/import-rule-reference-data.md)
- Compliance Core: [How to review staged import rows](compliancecore/review-staged-import-rows.md)
- Compliance Core: [How to map uploaded documents to requirements](compliancecore/map-uploaded-documents-to-requirements.md)
- Compliance Core: [How to evaluate a theoretical situation](compliancecore/evaluate-a-theoretical-situation.md)
- Compliance Core: [How to review governing bodies](compliancecore/review-governing-bodies.md)
- Compliance Core: [How to review citations](compliancecore/review-citations.md)
- Compliance Core: [How to troubleshoot missing rule matches](compliancecore/troubleshoot-missing-rule-matches.md)
- Field Companion: [How to view assigned work](fieldcompanion/view-assigned-work.md)
- Field Companion: [How to complete a mobile inspection](fieldcompanion/complete-a-mobile-inspection.md)
- Field Companion: [How to upload a photo or field evidence](fieldcompanion/upload-a-photo-or-field-evidence.md)
- Field Companion: [How to acknowledge a task](fieldcompanion/acknowledge-a-task.md)
- Field Companion: [How to view asset or location context in the field](fieldcompanion/view-asset-or-location-context-in-the-field.md)
- RecordArr: [How to find a record](recordarr/find-a-record.md)
- RecordArr: [How to upload a document](recordarr/upload-a-document.md)
- RecordArr: [How to attach a document to a source record](recordarr/attach-a-document-to-a-source-record.md)
- RecordArr: [How to review audit-ready records](recordarr/review-audit-ready-records.md)
- ReportArr: [How to generate a report](reportarr/generate-a-report.md)
- ReportArr: [How to filter a report](reportarr/filter-a-report.md)
- ReportArr: [How to export a report](reportarr/export-a-report.md)
- ReportArr: [How to review audit-ready records](reportarr/review-audit-ready-records.md)

## Intended, Product-Contract-Backed, or Partially Implemented Workflows
- StaffArr: [How to handle a training-related incident](staffarr/handle-a-training-related-incident.md)
- TrainArr: [How to create a training program](trainarr/create-a-training-program.md)
- TrainArr: [How to create a certification requirement](trainarr/create-a-certification-requirement.md)
- TrainArr: [How to assign training to a person](trainarr/assign-training-to-a-person.md)
- TrainArr: [How to issue a certificate](trainarr/issue-a-certificate.md)
- TrainArr: [How to review expiring certifications](trainarr/review-expiring-certifications.md)
- TrainArr: [How to assign retraining after an incident](trainarr/assign-retraining-after-an-incident.md)
- TrainArr: [How to handle failed or incomplete training](trainarr/handle-failed-or-incomplete-training.md)
- MaintainArr: [How to complete an inspection](maintainarr/complete-an-inspection.md)
- MaintainArr: [How to generate work from a preventive maintenance program](maintainarr/generate-work-from-a-preventive-maintenance-program.md)
- MaintainArr: [How to attach documents or photos to an asset](maintainarr/attach-documents-or-photos-to-an-asset.md)
- CustomArr: [How to create a customer](customarr/create-a-customer.md)
- CustomArr: [How to add a customer contact](customarr/add-a-customer-contact.md)
- CustomArr: [How to add a customer location](customarr/add-a-customer-location.md)
- CustomArr: [How to review customer onboarding](customarr/review-customer-onboarding.md)
- CustomArr: [How to check customer eligibility](customarr/check-customer-eligibility.md)
- OrdArr: [How to create an order request](ordarr/create-an-order-request.md)
- OrdArr: [How to triage an order request](ordarr/triage-an-order-request.md)
- OrdArr: [How to track order handoffs](ordarr/track-order-handoffs.md)
- OrdArr: [How to close an order request](ordarr/close-an-order-request.md)
- OrdArr: [How to prepare financial handoff packets](ordarr/prepare-financial-handoff-packets.md)
- SupplyArr: [How to handle customer context in SupplyArr](supplyarr/create-a-customer.md)
- SupplyArr: [How to send or track a purchase order](supplyarr/send-or-track-a-purchase-order.md)
- SupplyArr: [How to connect procurement expectations to receiving](supplyarr/connect-procurement-expectations-to-receiving.md)
- LoadArr: [How to check in a dock appointment](loadarr/check-in-a-dock-appointment.md)
- RoutArr: [How to create a dispatch](routarr/create-a-dispatch.md)
- RoutArr: [How to notify LoadArr about an inbound delivery](routarr/notify-loadarr-about-an-inbound-delivery.md)
- Compliance Core: [How to update controlled vocabulary](compliancecore/update-controlled-vocabulary.md)
- Field Companion: [How to report a defect from the field](fieldcompanion/report-a-defect-from-the-field.md)
- Field Companion: [How to complete a training step from mobile](fieldcompanion/complete-a-training-step-from-mobile.md)

## Placeholder or Ownership-Conflict Workflows
- None after the CustomArr, OrdArr, and SupplyArr boundary reconciliation.

## Known Documentation Gaps
- Some product create/edit routes exist but the final button labels and form fields need UI-level confirmation before the docs can name them precisely.
- Several cross-product workflows rely on events, read models, or handoffs documented in product constitutions but not always exposed as a single visible user action yet.
- SupplyArr customer creation is documented as boundary guidance because customer master records belong to CustomArr.
- Field Companion mobile training completion needs a confirmed TrainArr task panel before it can be documented as fully supported.
- MaintainArr asset document/photo attachment should be verified in the asset detail UI; RecordArr Capture is documented as the fallback.
