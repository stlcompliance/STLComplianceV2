# How to create a vendor

## Audience
SupplyArr admins, managers, or procurement users with party management access.

## Purpose
Create a vendor or supplier party in SupplyArr.

## Before You Start
- SupplyArr access.
- Party management access.
- Vendor display name, party key, contact details, and required documents if applicable.

## Steps
1. Open SupplyArr.
2. Open **Parties** and select **Create**.
3. Enter the vendor or supplier details shown by the form.
4. Add contact or document information where the page supports it.
5. Save the party.
6. Review the party in **Parties** > **Details**.
7. Use onboarding or supplier review actions where available.

## What Happens Next
SupplyArr owns the vendor or supplier record. LoadArr and MaintainArr may reference the vendor context but do not own it.

## Troubleshooting
- If **Create** is missing, check supplyarr.parties.manage access.
- If vendor documents are required, attach them through the available document controls.
- If payment details are needed, remember financial execution is outside STL Compliance.
- If the party is a customer, create or update it in CustomArr and use SupplyArr only for supplier/vendor records or labeled customer references.

## Related Docs
- [SupplyArr guide](../../products/supplyarr-user-guide.md)
- [CustomArr guide](../../products/customarr-user-guide.md)
- [Vendor user guide](../../roles/vendor-user-guide.md)
