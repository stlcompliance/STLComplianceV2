# CustomArr User Guide

## What This Product Is For
CustomArr is for customer accounts, customer onboarding, customer contacts, customer locations, customer service profiles, access requirements, customer requirements, communication preferences, portal access records, and customer risk or review context.

CustomArr owns customer truth. It does not own public-site marketing intake, order orchestration, dispatch execution, retained files, platform identity, or accounting.

## Who Uses It
- customer operations users
- account managers
- onboarding reviewers
- customer service users
- compliance users reviewing customer-specific requirements

## Main Pages
- Customers
- Prospects
- Onboarding
- Contacts
- Locations
- Service profiles
- Requirements
- Communication preferences
- Portal access
- Reviews
- Settings

## Main Records
- customer account
- customer onboarding
- customer contact
- customer contact authorization
- customer location
- customer service profile
- customer access requirement
- customer requirement
- customer communication preference
- customer portal access record

## Common Workflows
- create a customer prospect or onboarding record
- approve onboarding and activate a customer account
- add customer contacts and authorization records
- add customer locations and access instructions
- manage service eligibility separately from account lifecycle
- grant, suspend, or revoke customer portal access

## Permissions Usually Needed
- customarr.customers.read
- customarr.customers.manage
- customarr.contacts.manage
- customarr.locations.manage
- customarr.onboarding.review
- customarr.requirements.manage
- customarr.portalAccess.manage

## Related Products
- OrdArr owns order and request orchestration for customers.
- RoutArr, LoadArr, MaintainArr, and other execution products own execution truth.
- RecordArr stores retained files and persistent evidence links.
- NexArr owns platform identity, product launch, entitlement, and portal trust.
- STL Compliance Site routes public lead intake outside CustomArr unless a future platform CRM or approved onboarding intake is used.

## Common Troubleshooting
- [Product not visible](../troubleshooting/product-not-visible.md)
- [Missing permission](../troubleshooting/missing-permission.md)
- If an order, trip, work order, or receiving task is missing, check the owning execution product or OrdArr rather than editing the customer account.
- Remember: account lifecycle, onboarding status, and service eligibility are separate signals.
