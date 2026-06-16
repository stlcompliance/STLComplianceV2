# How to add a customer contact

## Audience
Customer operations users and account managers

## Product
CustomArr

## Support Status
Supported by product contract/docs

## Purpose
Add an external customer contact and record the contact authorization that controls what the contact can do.

## Before You Start
- Customer contacts belong to CustomArr.
- Supplier contacts belong to SupplyArr.
- StaffArr people are tenant-managed workers or login-authority identities, not general customer contacts.

## Steps
1. Open CustomArr.
2. Open the customer account.
3. Open Contacts.
4. Add contact details.
5. Add authorization records for permitted actions such as requesting service, approving waivers, receiving notices, or signing proof of delivery.
6. Save the contact.
7. Review the authorization summary.

## What Happens Next
CustomArr owns the contact and authorization record. If the contact needs portal access, use the portal access workflow so NexArr remains the login and trust authority.

## Troubleshooting
- If authorization cannot be changed, check `customarr.contacts.manage`.
- If the contact represents a supplier, create or update the contact in SupplyArr instead.

## Related How-To Documents
- [How to check customer eligibility](check-customer-eligibility.md)
