namespace STLCompliance.Shared.Integration;

public static class StlSuiteEventCatalog
{
    public static class NexArr
    {
        public const string IdentitySignedIn = "nexarr.identity.signedIn";
        public const string IdentitySignedOut = "nexarr.identity.signedOut";
        public const string TenantSelected = "nexarr.tenant.selected";
        public const string EntitlementGranted = "nexarr.entitlement.granted";
        public const string EntitlementRevoked = "nexarr.entitlement.revoked";
        public const string ProductLaunched = "nexarr.product.launched";
        public const string HandoffCreated = "nexarr.handoff.created";
        public const string HandoffRedeemed = "nexarr.handoff.redeemed";
        public const string ServiceTokenIssued = "nexarr.serviceToken.issued";
        public const string ServiceTokenRevoked = "nexarr.serviceToken.revoked";
        public const string PrintPreviewed = "nexarr.print.previewed";
        public const string PrintBrowserPrintRequested = "nexarr.print.browser_print_requested";
        public const string PrintPdfGenerated = "nexarr.print.pdf_generated";
        public const string PrintDownloaded = "nexarr.print.downloaded";
        public const string PrintArchived = "nexarr.print.archived";
        public const string PrintReprinted = "nexarr.print.reprinted";
        public const string PrintFailed = "nexarr.print.failed";
        public const string PrintTemplateUpdated = "nexarr.print.template.updated";
    }

    public static class StaffArr
    {
        public const string PersonCreated = "staffarr.person.created";
        public const string PersonUpdated = "staffarr.person.updated";
        public const string PersonStatusChanged = "staffarr.person.statusChanged";
        public const string RoleCreated = "staffarr.role.created";
        public const string RoleUpdated = "staffarr.role.updated";
        public const string PermissionAssignmentChanged = "staffarr.permissionAssignment.changed";
        public const string OrgUnitCreated = "staffarr.orgUnit.created";
        public const string OrgUnitUpdated = "staffarr.orgUnit.updated";
        public const string LocationCreated = "staffarr.location.created";
        public const string LocationUpdated = "staffarr.location.updated";
        public const string LocationStatusChanged = "staffarr.location.statusChanged";
        public const string ShiftCreated = "staffarr.shift.created";
        public const string ShiftUpdated = "staffarr.shift.updated";
        public const string ShiftCancelled = "staffarr.shift.cancelled";
        public const string AvailabilityChanged = "staffarr.availability.changed";
        public const string TeamCreated = "staffarr.team.created";
        public const string TeamUpdated = "staffarr.team.updated";
        public const string IncidentReported = "staffarr.incident.reported";
        public const string IncidentRouted = "staffarr.incident.routed";
        public const string ResourceConflictDetected = "staffarr.resourceConflict.detected";
        public const string ResourceConflictResolved = "staffarr.resourceConflict.resolved";
    }

    public static class CustomArr
    {
        public const string CustomerCreated = "customarr.customer.created";
        public const string CustomerUpdated = "customarr.customer.updated";
        public const string CustomerStatusChanged = "customarr.customer.statusChanged";
        public const string CustomerLocationCreated = "customarr.customer_location.created";
        public const string CustomerLocationUpdated = "customarr.customer_location.updated";
        public const string CustomerContactCreated = "customarr.customer_contact.created";
        public const string CustomerContactUpdated = "customarr.customer_contact.updated";
        public const string CustomerRequirementCreated = "customarr.customer_requirement.created";
        public const string CustomerRequirementUpdated = "customarr.customer_requirement.updated";
        public const string CustomerRequirementEvaluationPassed = "customarr.customer_requirement.evaluation_passed";
        public const string CustomerRequirementEvaluationWarned = "customarr.customer_requirement.evaluation_warned";
        public const string CustomerRequirementEvaluationFailed = "customarr.customer_requirement.evaluation_failed";
        public const string CustomerRequirementEvaluationBlocked = "customarr.customer_requirement.evaluation_blocked";
        public const string LeadCreated = "customarr.lead.created";
        public const string LeadConverted = "customarr.lead.converted";
        public const string OpportunityCreated = "customarr.opportunity.created";
        public const string OpportunityWon = "customarr.opportunity.won";
        public const string ProposalCreated = "customarr.proposal.created";
        public const string ProposalAccepted = "customarr.proposal.accepted";
        public const string AgreementCreated = "customarr.agreement.created";
        public const string AgreementUpdated = "customarr.agreement.updated";
        public const string CustomerCaseCreated = "customarr.customer_case.created";
        public const string CustomerCaseUpdated = "customarr.customer_case.updated";
        public const string CustomerActivityLogged = "customarr.customer_activity.logged";
        public const string CustomerTaskCreated = "customarr.customer_task.created";
        public const string CustomerTaskCompleted = "customarr.customer_task.completed";
        public const string CustomerPortalAccessCreated = "customarr.customer_portal_access.created";
        public const string CustomerPortalAccessUpdated = "customarr.customer_portal_access.updated";
        public const string CustomerEligibilityChecked = "customarr.customer_eligibility.checked";
        public const string CustomerOnboardingCreated = "customarr.customer_onboarding.created";
        public const string CustomerHealthUpdated = "customarr.customer_health.updated";
        public const string CustomerImportBatchCreated = "customarr.customer_import_batch.created";
        public const string CustomerDedupeCandidateCreated = "customarr.customer_dedupe_candidate.created";
        public const string CustomerMergeProposed = "customarr.customer_merge.proposed";
        public const string CustomerMergeCompleted = "customarr.customer_merge.completed";
        public const string CustomerExternalMappingCreated = "customarr.customer_external_mapping.created";
        public const string CustomerExternalMappingUpdated = "customarr.customer_external_mapping.updated";
        public const string PortalCreated = "customarr.portal.created";
        public const string PortalUpdated = "customarr.portal.updated";
        public const string PortalSubmissionCreated = "customarr.portalSubmission.created";
        public const string PortalSubmissionValidated = "customarr.portalSubmission.validated";
        public const string PortalSubmissionRejected = "customarr.portalSubmission.rejected";
        public const string PortalSubmissionRouted = "customarr.portalSubmission.routed";
        public const string PortalSubmissionCancelled = "customarr.portalSubmission.cancelled";
        public const string WorkflowRecordCreated = "customarr.workflowRecord.created";
        public const string WorkflowRecordSubmitted = "customarr.workflowRecord.submitted";
        public const string WorkflowRecordApproved = "customarr.workflowRecord.approved";
        public const string WorkflowRecordRejected = "customarr.workflowRecord.rejected";
        public const string WorkflowRecordCompleted = "customarr.workflowRecord.completed";
    }

    public static class OrdArr
    {
        public const string OrderRequested = "ordarr.order.requested";
        public const string OrderCreated = "ordarr.order.created";
        public const string OrderAccepted = "ordarr.order.accepted";
        public const string OrderRejected = "ordarr.order.rejected";
        public const string OrderHeld = "ordarr.order.held";
        public const string OrderReleased = "ordarr.order.released";
        public const string OrderChanged = "ordarr.order.changed";
        public const string OrderChangeRequested = "ordarr.order.changeRequested";
        public const string OrderChangeRejected = "ordarr.order.changeRejected";
        public const string OrderCancelRequested = "ordarr.order.cancelRequested";
        public const string OrderCancelled = "ordarr.order.cancelled";
        public const string OrderPromisedWindowSet = "ordarr.order.promisedWindowSet";
        public const string OrderFulfillmentRequested = "ordarr.order.fulfillmentRequested";
        public const string OrderFulfillmentBlocked = "ordarr.order.fulfillmentBlocked";
        public const string OrderFulfillmentReleased = "ordarr.order.fulfillmentReleased";
        public const string OrderClosed = "ordarr.order.closed";
        public const string OrderReopened = "ordarr.order.reopened";
        public const string OrderLineCreated = "ordarr.orderLine.created";
        public const string OrderLineChanged = "ordarr.orderLine.changed";
        public const string OrderLineCancelled = "ordarr.orderLine.cancelled";
    }

    public static class SupplyArr
    {
        public const string VendorCreated = "supplyarr.vendor.created";
        public const string VendorUpdated = "supplyarr.vendor.updated";
        public const string VendorStatusChanged = "supplyarr.vendor.statusChanged";
        public const string VendorConfirmationRequested = "supplyarr.vendorConfirmation.requested";
        public const string VendorConfirmationReceived = "supplyarr.vendorConfirmation.received";
        public const string VendorConfirmationRejected = "supplyarr.vendorConfirmation.rejected";
        public const string ProcurementNeedCreated = "supplyarr.procurementNeed.created";
        public const string PurchaseOrderCreated = "supplyarr.purchaseOrder.created";
        public const string PurchaseOrderIssued = "supplyarr.purchaseOrder.issued";
        public const string PurchaseOrderAcknowledged = "supplyarr.purchaseOrder.acknowledged";
        public const string PurchaseOrderChanged = "supplyarr.purchaseOrder.changed";
        public const string PurchaseOrderPartiallyReceived = "supplyarr.purchaseOrder.partiallyReceived";
        public const string PurchaseOrderReceived = "supplyarr.purchaseOrder.received";
        public const string PurchaseOrderCancelled = "supplyarr.purchaseOrder.cancelled";
        public const string PurchaseOrderClosed = "supplyarr.purchaseOrder.closed";
        public const string MaterialShortageDetected = "supplyarr.material.shortageDetected";
        public const string MaterialSubstitutionRequested = "supplyarr.material.substitutionRequested";
        public const string MaterialSubstitutionApproved = "supplyarr.material.substitutionApproved";
        public const string MaterialSubstitutionRejected = "supplyarr.material.substitutionRejected";
    }

    public static class LoadArr
    {
        public const string InboundReceiptExpected = "loadarr.inboundReceipt.expected";
        public const string DockAppointmentRequested = "loadarr.dockAppointment.requested";
        public const string DockAppointmentScheduled = "loadarr.dockAppointment.scheduled";
        public const string DockAppointmentRescheduled = "loadarr.dockAppointment.rescheduled";
        public const string DockAppointmentUnscheduled = "loadarr.dockAppointment.unscheduled";
        public const string DockAppointmentCancelled = "loadarr.dockAppointment.cancelled";
        public const string DockAppointmentCheckedIn = "loadarr.dockAppointment.checkedIn";
        public const string DockAppointmentArrived = "loadarr.dockAppointment.arrived";
        public const string DockAppointmentCompleted = "loadarr.dockAppointment.completed";
        public const string ReceivingStarted = "loadarr.receiving.started";
        public const string ReceivingConfirmed = "loadarr.receiving.confirmed";
        public const string ReceivingExceptionRaised = "loadarr.receiving.exceptionRaised";
        public const string PutawayTaskCreated = "loadarr.putawayTask.created";
        public const string PutawayTaskScheduled = "loadarr.putawayTask.scheduled";
        public const string PutawayTaskRescheduled = "loadarr.putawayTask.rescheduled";
        public const string PutawayTaskCompleted = "loadarr.putawayTask.completed";
        public const string InventoryMoved = "loadarr.inventory.moved";
        public const string InventoryAdjusted = "loadarr.inventory.adjusted";
        public const string InventoryHeld = "loadarr.inventory.held";
        public const string InventoryReleased = "loadarr.inventory.released";
        public const string StagingTaskCreated = "loadarr.stagingTask.created";
        public const string StagingTaskScheduled = "loadarr.stagingTask.scheduled";
        public const string StagingTaskCompleted = "loadarr.stagingTask.completed";
    }

    public static class RoutArr
    {
        public const string TransportDemandCreated = "routarr.transportDemand.created";
        public const string TripCreated = "routarr.trip.created";
        public const string TripScheduled = "routarr.trip.scheduled";
        public const string TripRescheduled = "routarr.trip.rescheduled";
        public const string TripUnscheduled = "routarr.trip.unscheduled";
        public const string TripCancelled = "routarr.trip.cancelled";
        public const string TripDispatched = "routarr.trip.dispatched";
        public const string TripStarted = "routarr.trip.started";
        public const string TripCompleted = "routarr.trip.completed";
        public const string AssignmentChanged = "routarr.assignment.changed";
        public const string RouteCreated = "routarr.route.created";
        public const string RouteOptimized = "routarr.route.optimized";
        public const string RouteBlocked = "routarr.route.blocked";
        public const string StopArrived = "routarr.stop.arrived";
        public const string StopCompleted = "routarr.stop.completed";
        public const string StopExceptionRaised = "routarr.stop.exceptionRaised";
        public const string EtaUpdated = "routarr.eta.updated";
        public const string TransportExceptionRaised = "routarr.transportException.raised";
        public const string TransportExceptionResolved = "routarr.transportException.resolved";
    }

    public static class MaintainArr
    {
        public const string AssetCreated = "maintainarr.asset.created";
        public const string AssetUpdated = "maintainarr.asset.updated";
        public const string AssetStatusChanged = "maintainarr.asset.statusChanged";
        public const string AssetReadinessChanged = "maintainarr.asset.readinessChanged";
        public const string DefectReported = "maintainarr.defect.reported";
        public const string DefectAccepted = "maintainarr.defect.accepted";
        public const string DefectRejected = "maintainarr.defect.rejected";
        public const string PmDueGenerated = "maintainarr.pm.dueGenerated";
        public const string InspectionCreated = "maintainarr.inspection.created";
        public const string InspectionScheduled = "maintainarr.inspection.scheduled";
        public const string InspectionRescheduled = "maintainarr.inspection.rescheduled";
        public const string InspectionCompleted = "maintainarr.inspection.completed";
        public const string InspectionFailed = "maintainarr.inspection.failed";
        public const string WorkOrderCreated = "maintainarr.workOrder.created";
        public const string WorkOrderScheduled = "maintainarr.workOrder.scheduled";
        public const string WorkOrderRescheduled = "maintainarr.workOrder.rescheduled";
        public const string WorkOrderUnscheduled = "maintainarr.workOrder.unscheduled";
        public const string WorkOrderStarted = "maintainarr.workOrder.started";
        public const string WorkOrderCompleted = "maintainarr.workOrder.completed";
        public const string WorkOrderDeferred = "maintainarr.workOrder.deferred";
        public const string WorkOrderCancelled = "maintainarr.workOrder.cancelled";
        public const string PartNeedCreated = "maintainarr.part.needCreated";
        public const string PartReserved = "maintainarr.part.reserved";
        public const string PartShortageDetected = "maintainarr.part.shortageDetected";
        public const string DowntimeStarted = "maintainarr.downtime.started";
        public const string DowntimeEnded = "maintainarr.downtime.ended";
    }

    public static class TrainArr
    {
        public const string ProgramCreated = "trainarr.program.created";
        public const string ProgramUpdated = "trainarr.program.updated";
        public const string TrainingRequirementCreated = "trainarr.trainingRequirement.created";
        public const string TrainingRequirementUpdated = "trainarr.trainingRequirement.updated";
        public const string AssignmentCreated = "trainarr.assignment.created";
        public const string AssignmentScheduled = "trainarr.assignment.scheduled";
        public const string AssignmentRescheduled = "trainarr.assignment.rescheduled";
        public const string AssignmentUnscheduled = "trainarr.assignment.unscheduled";
        public const string AssignmentCompleted = "trainarr.assignment.completed";
        public const string AssignmentFailed = "trainarr.assignment.failed";
        public const string EvaluationCreated = "trainarr.evaluation.created";
        public const string EvaluationScheduled = "trainarr.evaluation.scheduled";
        public const string EvaluationCompleted = "trainarr.evaluation.completed";
        public const string CertificateIssued = "trainarr.certificate.issued";
        public const string CertificateSuspended = "trainarr.certificate.suspended";
        public const string CertificateExpired = "trainarr.certificate.expired";
        public const string QualificationGranted = "trainarr.qualification.granted";
        public const string QualificationRevoked = "trainarr.qualification.revoked";
        public const string RetrainingRequired = "trainarr.retraining.required";
    }

    public static class AssurArr
    {
        public const string QualityCheckCreated = "assurarr.qualityCheck.created";
        public const string QualityCheckScheduled = "assurarr.qualityCheck.scheduled";
        public const string QualityCheckRescheduled = "assurarr.qualityCheck.rescheduled";
        public const string QualityCheckCompleted = "assurarr.qualityCheck.completed";
        public const string QualityCheckFailed = "assurarr.qualityCheck.failed";
        public const string NonconformanceCreated = "assurarr.nonconformance.created";
        public const string NonconformanceDispositioned = "assurarr.nonconformance.dispositioned";
        public const string CorrectiveActionCreated = "assurarr.correctiveAction.created";
        public const string CorrectiveActionScheduled = "assurarr.correctiveAction.scheduled";
        public const string CorrectiveActionCompleted = "assurarr.correctiveAction.completed";
        public const string ReceivingExceptionReviewRequested = "assurarr.receivingException.reviewRequested";
        public const string ReceivingExceptionAccepted = "assurarr.receivingException.accepted";
        public const string ReceivingExceptionRejected = "assurarr.receivingException.rejected";
        public const string AuditCreated = "assurarr.audit.created";
        public const string AuditScheduled = "assurarr.audit.scheduled";
        public const string AuditCompleted = "assurarr.audit.completed";
    }

    public static class RecordArr
    {
        public const string DocumentUploaded = "recordarr.document.uploaded";
        public const string DocumentClassified = "recordarr.document.classified";
        public const string DocumentLinked = "recordarr.document.linked";
        public const string DocumentUnlinked = "recordarr.document.unlinked";
        public const string EvidenceAccepted = "recordarr.evidence.accepted";
        public const string EvidenceRejected = "recordarr.evidence.rejected";
        public const string RecordPackageCreated = "recordarr.recordPackage.created";
        public const string RecordPackageCompleted = "recordarr.recordPackage.completed";
        public const string RetentionHoldPlaced = "recordarr.retentionHold.placed";
        public const string RetentionHoldReleased = "recordarr.retentionHold.released";
    }

    public static class ComplianceCore
    {
        public const string QuestionnaireCreated = "compliancecore.questionnaire.created";
        public const string QuestionnaireAnswered = "compliancecore.questionnaire.answered";
        public const string ComplianceFactRecorded = "compliancecore.complianceFact.recorded";
        public const string ComplianceFactSuperseded = "compliancecore.complianceFact.superseded";
        public const string EvaluationRequested = "compliancecore.evaluation.requested";
        public const string EvaluationCompleted = "compliancecore.evaluation.completed";
        public const string EvaluationBlocked = "compliancecore.evaluation.blocked";
        public const string FollowUpRequired = "compliancecore.followUp.required";
        public const string RulepackCreated = "compliancecore.rulepack.created";
        public const string RulepackUpdated = "compliancecore.rulepack.updated";
        public const string RulepackPublished = "compliancecore.rulepack.published";
        public const string VocabularyUpdated = "compliancecore.vocabulary.updated";
    }

    public static class FieldCompanion
    {
        public const string TaskViewed = "fieldcompanion.task.viewed";
        public const string TaskAcknowledged = "fieldcompanion.task.acknowledged";
        public const string TaskProgressUpdated = "fieldcompanion.task.progressUpdated";
        public const string TaskPhotoCaptured = "fieldcompanion.task.photoCaptured";
        public const string TaskNoteCaptured = "fieldcompanion.task.noteCaptured";
        public const string TaskSignatureCaptured = "fieldcompanion.task.signatureCaptured";
        public const string OfflineChangeQueued = "fieldcompanion.offlineChange.queued";
        public const string OfflineChangeSynced = "fieldcompanion.offlineChange.synced";
        public const string OfflineChangeRejected = "fieldcompanion.offlineChange.rejected";
    }

    public static class LedgArr
    {
        public const string FinancialPacketReceived = "ledgarr.financial_packet.received";
        public const string FinancialPacketValidationFailed = "ledgarr.financial_packet.validation_failed";
        public const string FinancialPacketNeedsMapping = "ledgarr.financial_packet.needs_mapping";
        public const string FinancialPacketMapped = "ledgarr.financial_packet.mapped";
        public const string FinancialPacketPreviewReady = "ledgarr.financial_packet.preview_ready";
        public const string FinancialPacketApproved = "ledgarr.financial_packet.approved";
        public const string FinancialPacketPosted = "ledgarr.financial_packet.posted";
        public const string FinancialPacketRejected = "ledgarr.financial_packet.rejected";
        public const string PostingPreviewCreated = "ledgarr.posting_preview.created";
        public const string JournalSubmitted = "ledgarr.journal.submitted";
        public const string JournalApproved = "ledgarr.journal.approved";
        public const string JournalPosted = "ledgarr.journal.posted";
        public const string JournalReversed = "ledgarr.journal.reversed";
        public const string PeriodClosed = "ledgarr.period.closed";
        public const string PeriodReopened = "ledgarr.period.reopened";
        public const string PeriodLocked = "ledgarr.period.locked";
        public const string VendorBillCreated = "ledgarr.vendor_bill.created";
        public const string VendorBillMatched = "ledgarr.vendor_bill.matched";
        public const string VendorBillApproved = "ledgarr.vendor_bill.approved";
        public const string VendorBillPosted = "ledgarr.vendor_bill.posted";
        public const string PaymentRunCreated = "ledgarr.payment_run.created";
        public const string PaymentRunExported = "ledgarr.payment_run.exported";
        public const string CustomerInvoiceCreated = "ledgarr.customer_invoice.created";
        public const string CustomerInvoiceIssued = "ledgarr.customer_invoice.issued";
        public const string CustomerInvoicePosted = "ledgarr.customer_invoice.posted";
        public const string CustomerPaymentRecorded = "ledgarr.customer_payment.recorded";
        public const string InventoryValuationUpdated = "ledgarr.inventory_valuation.updated";
        public const string InventoryReconciliationIssueDetected = "ledgarr.inventory_reconciliation.issue_detected";
        public const string FixedAssetCapitalized = "ledgarr.fixed_asset.capitalized";
        public const string FixedAssetDepreciationPosted = "ledgarr.fixed_asset.depreciation_posted";
        public const string BudgetApproved = "ledgarr.budget.approved";
        public const string BudgetThresholdExceeded = "ledgarr.budget.threshold_exceeded";
        public const string ExternalExportCreated = "ledgarr.external_export.created";
        public const string ExternalExportSent = "ledgarr.external_export.sent";
        public const string ExternalExportFailed = "ledgarr.external_export.failed";
        public const string FinancialLegalEntityCreated = "ledgarr.financial_legal_entity.created";
        public const string FinancialLegalEntityUpdated = "ledgarr.financial_legal_entity.updated";
        public const string FinancialLegalEntityDeactivated = "ledgarr.financial_legal_entity.deactivated";
    }

    public static class ReportArr
    {
        public const string ProjectionUpdated = "reportarr.projection.updated";
        public const string DatasetRefreshed = "reportarr.dataset.refreshed";
        public const string ReportCreated = "reportarr.report.created";
        public const string ReportGenerated = "reportarr.report.generated";
        public const string ReportScheduled = "reportarr.report.scheduled";
        public const string ReportDelivered = "reportarr.report.delivered";
        public const string SnapshotCreated = "reportarr.snapshot.created";
    }

    public static class ReferenceDataCore
    {
        public const string CatalogCreated = "referencedatacore.catalog.created";
        public const string CatalogUpdated = "referencedatacore.catalog.updated";
        public const string ItemCreated = "referencedatacore.item.created";
        public const string ItemUpdated = "referencedatacore.item.updated";
        public const string ItemDeprecated = "referencedatacore.item.deprecated";
    }

    public static class StlComplianceSite
    {
        public const string LeadCreated = "stlcompliancesite.lead.created";
        public const string ContactRequestCreated = "stlcompliancesite.contactRequest.created";
        public const string DemoRequestCreated = "stlcompliancesite.demoRequest.created";
    }
}
