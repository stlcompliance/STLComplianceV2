# How to invite or create a user

## Audience
Platform administrators, tenant administrators, and appropriately permissioned StaffArr administrators

## Product
StaffArr with NexArr-backed account actions

## Purpose
Provision a platform login while keeping the internal person record and authority context in StaffArr and account/session truth in NexArr.

## Before you start
- Create or select the StaffArr person record for an internal person.
- You need permission to manage the person and delegated NexArr account action.

## Steps
1. Open the StaffArr person detail page.
2. Open **Account** or **Permissions**.
3. Choose **Provision account** or link an existing account.
4. Enter or confirm email, display name, tenant membership, and invitation method.
5. Set platform-admin status only through the separate, highly restricted workflow when genuinely required.
6. Submit the NexArr-backed action.
7. Assign product permissions and scope through StaffArr.
8. Verify invitation/account state without exposing temporary credentials.

## What happens next
NexArr owns login, tenant membership, sessions, security, and platform-admin status. StaffArr remains the person and authority-context owner. Every ordinary product is available after active membership, but actions remain permission-controlled.
