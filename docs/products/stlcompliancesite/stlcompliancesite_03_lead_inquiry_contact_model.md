# STLComplianceSite — Lead Inquiry, Demo Request, and Contact Model

## Lead inquiry

A LeadInquiry is a public form submission from a prospect, customer, partner, or interested party.

```text
LeadInquiry
- leadInquiryId
- inquiryNumber
- submittedAt
- name
- company
- email
- phone
- jobTitle
- inquiryType
  - demo
  - pricing
  - general
  - support
  - partnership
  - press
  - investor
  - vendor
  - other
- interestedProducts
- interestedUseCases
- industry
- companySize
- fleetSize
- warehouseCount
- employeeCount
- message
- sourcePage
- sourceCampaign
- consentFlags
- spamScore
- status
  - new
  - spam
  - needs_review
  - reviewed
  - routed
  - contacted
  - qualified
  - disqualified
  - closed
- routedTo
  - external_crm
  - nexarr_tenant_prospect
  - future_platform_crm
  - email
  - support
  - manual_review
- nexarrTenantProspectRef
- externalCrmRef
- futurePlatformCrmRef
- assignedPersonId
- notes
- ipAddress
- userAgent
```

## Demo request

```text
DemoRequest
- demoRequestId
- demoNumber
- leadInquiryRef
- requestedAt
- requestedProducts
- requestedUseCases
- preferredContactMethod
  - email
  - phone
  - video_call
  - no_preference
- preferredTimeWindow
- timezone
- urgency
  - low
  - normal
  - high
- currentTools
- painPoints
- status
  - requested
  - reviewed
  - scheduled
  - completed
  - no_show
  - canceled
  - closed
- scheduledAt
- assignedPersonId
- meetingLink
- followUpNotes
```

## Contact form submission

```text
ContactSubmission
- contactSubmissionId
- submissionNumber
- submittedAt
- name
- email
- phone
- company
- subject
- message
- contactReason
  - sales
  - support
  - billing_reference
  - security
  - privacy
  - partnership
  - general
- status
  - new
  - spam
  - reviewed
  - routed
  - responded
  - closed
- assignedPersonId
- responseSummary
```

## Newsletter/update signup

```text
UpdateSignup
- updateSignupId
- email
- name
- company
- interests
  - product_updates
  - compliance_updates
  - founder_updates
  - launch_updates
  - general
- status
  - subscribed
  - unsubscribed
  - bounced
  - complained
- consentAt
- sourcePage
```

## Form definition

```text
PublicFormDefinition
- formDefinitionId
- formKey
- title
- description
- formType
  - lead
  - demo
  - contact
  - newsletter
  - support
  - privacy_request
- status
  - draft
  - active
  - inactive
  - archived
- fields
- validationRules
- spamProtection
- consentRequirements
- routingRules
- successMessage
- failureMessage
```

## Form field

```text
PublicFormField
- fieldId
- formDefinitionId
- fieldKey
- label
- helpText
- fieldType
  - text
  - email
  - phone
  - textarea
  - select
  - multi_select
  - checkbox
  - hidden
- required
- options
- validationRules
- pii
- sortOrder
```

## Consent flag

```text
ConsentFlag
- consentFlagId
- submissionRef
- consentType
  - contact_me
  - marketing_emails
  - privacy_policy
  - terms
  - cookies
- accepted
- acceptedAt
- consentTextVersion
```

## Spam/risk assessment

```text
SpamAssessment
- spamAssessmentId
- submissionRef
- score
- result
  - pass
  - challenge
  - spam
  - manual_review
- signals
- provider
- assessedAt
```

## Routing rule

```text
InquiryRoutingRule
- routingRuleId
- formType
- condition
- routeTo
  - external_crm
  - nexarr_tenant_prospect
  - future_platform_crm
  - email
  - support
  - manual_review
- assignedPersonId
- externalTarget
- status
```

## Lead handoff

```text
LeadHandoff
- leadHandoffId
- leadInquiryId
- destination
  - external_crm
  - nexarr_tenant_prospect
  - future_platform_crm
  - email
  - manual
- status
  - pending
  - sent
  - accepted
  - failed
  - canceled
- destinationRef
- sentAt
- acceptedAt
- failureReason
```

## Lead inquiry workflow

```text
1. Visitor submits form.
2. Site validates required fields.
3. Spam/risk assessment runs.
4. Consent flags are recorded.
5. LeadInquiry is created.
6. Routing rule runs.
7. Lead is sent to external CRM, NexArr tenant prospect/onboarding intake, future platform CRM, email, or manual review.
8. Internal notification is sent.
9. Public confirmation is shown.
10. Long-term relationship tracking happens outside the public site.
```

## Demo request workflow

```text
1. Visitor submits demo request.
2. DemoRequest and LeadInquiry are created.
3. Request is reviewed.
4. Demo is scheduled manually or via integration.
5. Follow-up notes are tracked in external CRM, NexArr tenant prospect/onboarding intake, future platform CRM, or manual review workflow.
6. Site retains submission/audit metadata.
```

## Privacy/contact request workflow

```text
1. Visitor submits privacy/security/contact request.
2. Submission is classified.
3. Sensitive/legal/privacy requests are routed to responsible reviewer.
4. Response is handled outside public site or through configured workflow.
5. Submission status is closed.
```

## Events

```text
site.lead_inquiry.created
site.lead_inquiry.spam_detected
site.lead_inquiry.reviewed
site.lead_inquiry.routed
site.lead_inquiry.contacted
site.lead_inquiry.closed

site.demo_request.created
site.demo_request.scheduled
site.demo_request.completed
site.demo_request.canceled

site.contact_submission.created
site.contact_submission.routed
site.contact_submission.responded
site.contact_submission.closed

site.update_signup.created
site.update_signup.unsubscribed

site.form.submitted
site.form.spam_assessed
site.lead_handoff.sent
site.lead_handoff.failed
```
