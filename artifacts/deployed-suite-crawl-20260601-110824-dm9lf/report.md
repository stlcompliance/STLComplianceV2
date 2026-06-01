# STL Suite deployed crawl

Run: 20260601-110824-dm9lf
Base: https://suite-frontend.onrender.com
Generated: 2026-06-01T11:12:45.960Z

## Summary

- Routes/screenshots: 62
- Network/console errors: 0
- Created records: 5
- Data creation errors: 0

## Created Data

- tenant: {"type":"tenant","slug":"qa-route-1110824dm9lf","displayName":"QA Route Crawl 1110824dm9lf","tenantId":"fe0ceaa3-b36f-4a57-888e-ef777256010b"}
- product: {"type":"product","productKey":"product.catalog.qarouteproduct1110824dm9lf","displayName":"QA Route Product 1110824dm9lf","sortOrder":987}
- entitlement: {"type":"entitlement","tenantId":"fe0ceaa3-b36f-4a57-888e-ef777256010b","productKey":"product.catalog.qarouteproduct1110824dm9lf","entitlementId":"3153d89d-ffea-48cf-bd82-681dfe60015e","status":"Active"}
- data-plane-profile: {"type":"data-plane-profile","tenantId":"fe0ceaa3-b36f-4a57-888e-ef777256010b","productKey":"product.catalog.qarouteproduct1110824dm9lf","deploymentMode":"hosted","trustStatus":"trusted"}
- service-client: {"type":"service-client","serviceClientId":"a2ab8c18-9002-4310-893d-6237fa12cc3a","clientKey":"product.serviceclient.qarouteclient1110824dm9lfretry","displayName":"QA Route Client 1110824dm9lf retry","sourceProductKey":"product.catalog.qarouteproduct1110824dm9lf","allowedProductKeys":["product.catalog.qarouteproduct1110824dm9lf"]}

## Route Screenshots

- 01. Root redirect unauthenticated (/) -> screenshots/01-public-root.png
- 02. Login (/login) -> screenshots/02-public-login.png
- 03. Forgot password (/forgot-password) -> screenshots/03-public-forgot-password.png
- 04. Reset password (/reset-password) -> screenshots/04-public-reset-password.png
- 05. App redirect unauthenticated (/app) -> screenshots/05-public-app.png
- 06. Login before sign-in (/login) -> screenshots/06-auth-login.png
- 07. Suite dashboard after sign-in (/app) -> screenshots/07-auth-app.png
- 08. Platform admin dashboard (/app/platform-admin) -> screenshots/08-auth-app-platform-admin.png
- 09. Launch diagnostics (/app/platform-admin/launch) -> screenshots/09-auth-app-platform-admin-launch.png
- 10. Tenants (/app/platform-admin/tenants) -> screenshots/10-auth-app-platform-admin-tenants.png
- 11. Products (/app/platform-admin/products) -> screenshots/11-auth-app-platform-admin-products.png
- 12. Data plane (/app/platform-admin/data-plane) -> screenshots/12-auth-app-platform-admin-data-plane.png
- 13. Audit search and export (/app/platform-admin/audit-export) -> screenshots/13-auth-app-platform-admin-audit-export.png
- 14. Lifecycle workers (/app/platform-admin/lifecycle) -> screenshots/14-auth-app-platform-admin-lifecycle.png
- 15. Event outbox (/app/platform-admin/platform-outbox) -> screenshots/15-auth-app-platform-admin-platform-outbox.png
- 16. Worker health (/app/platform-admin/orchestration) -> screenshots/16-auth-app-platform-admin-orchestration.png
- 17. Service tokens (/app/platform-admin/service-tokens) -> screenshots/17-auth-app-platform-admin-service-tokens.png
- 18. Entitlements (/app/platform-admin/entitlements) -> screenshots/18-auth-app-platform-admin-entitlements.png
- 19. Tenant lifecycle (/app/platform-admin/tenant-lifecycle) -> screenshots/19-auth-app-platform-admin-tenant-lifecycle.png
- 20. STL Shared Worker / Overview (/app/shared-worker) -> screenshots/20-auth-app-shared-worker.png
- 21. STL Shared Worker / Open product app (/app/shared-worker/launch) -> screenshots/21-auth-app-shared-worker-launch.png [visible error: Launch profile missingCannot launch STL Shared Worker. NexArr does not have an active launch profile for this product.Platform administrators can configure the product launch URL and callback allowlist.Reason code: profile_missingOpen launch diagnosticsReview launch surface]
- 22. NexArr Worker / Overview (/app/nexarr-worker) -> screenshots/22-auth-app-nexarr-worker.png
- 23. NexArr Worker / Open product app (/app/nexarr-worker/launch) -> screenshots/23-auth-app-nexarr-worker-launch.png [visible error: Launch profile missingCannot launch NexArr Worker. NexArr does not have an active launch profile for this product.Platform administrators can configure the product launch URL and callback allowlist.Reason code: profile_missingOpen launch diagnosticsReview launch surface]
- 24. NexArr / Overview (/app/nexarr) -> screenshots/24-auth-app-nexarr.png
- 25. NexArr / Identity & access (/app/nexarr/identity) -> screenshots/25-auth-app-nexarr-identity.png
- 26. NexArr / Tenants (/app/nexarr/tenants) -> screenshots/26-auth-app-nexarr-tenants.png
- 27. StaffArr / Overview (/app/staffarr) -> screenshots/27-auth-app-staffarr.png
- 28. StaffArr / People directory (/app/staffarr/people) -> screenshots/28-auth-app-staffarr-people.png
- 29. StaffArr / Readiness (/app/staffarr/readiness) -> screenshots/29-auth-app-staffarr-readiness.png
- 30. StaffArr / Open StaffArr app (/app/staffarr/launch) -> screenshots/30-auth-app-staffarr-launch.png
- 31. TrainArr / Overview (/app/trainarr) -> screenshots/31-auth-app-trainarr.png
- 32. TrainArr / Assignments (/app/trainarr/assignments) -> screenshots/32-auth-app-trainarr-assignments.png
- 33. TrainArr / Qualifications (/app/trainarr/qualifications) -> screenshots/33-auth-app-trainarr-qualifications.png
- 34. TrainArr / Open TrainArr app (/app/trainarr/launch) -> screenshots/34-auth-app-trainarr-launch.png
- 35. MaintainArr / Overview (/app/maintainarr) -> screenshots/35-auth-app-maintainarr.png
- 36. MaintainArr / Assets (/app/maintainarr/assets) -> screenshots/36-auth-app-maintainarr-assets.png
- 37. MaintainArr / Work orders (/app/maintainarr/work-orders) -> screenshots/37-auth-app-maintainarr-work-orders.png
- 38. MaintainArr / Open MaintainArr app (/app/maintainarr/launch) -> screenshots/38-auth-app-maintainarr-launch.png
- 39. RoutArr / Overview (/app/routarr) -> screenshots/39-auth-app-routarr.png
- 40. RoutArr / Dispatch (/app/routarr/dispatch) -> screenshots/40-auth-app-routarr-dispatch.png
- 41. RoutArr / Routes (/app/routarr/routes) -> screenshots/41-auth-app-routarr-routes.png
- 42. RoutArr / Open RoutArr app (/app/routarr/launch) -> screenshots/42-auth-app-routarr-launch.png
- 43. SupplyArr / Overview (/app/supplyarr) -> screenshots/43-auth-app-supplyarr.png
- 44. SupplyArr / Inventory (/app/supplyarr/inventory) -> screenshots/44-auth-app-supplyarr-inventory.png
- 45. SupplyArr / Procurement (/app/supplyarr/procurement) -> screenshots/45-auth-app-supplyarr-procurement.png
- 46. SupplyArr / Open SupplyArr app (/app/supplyarr/launch) -> screenshots/46-auth-app-supplyarr-launch.png
- 47. Compliance Core / Overview (/app/compliancecore) -> screenshots/47-auth-app-compliancecore.png
- 48. Compliance Core / Vocabulary (/app/compliancecore/vocabulary) -> screenshots/48-auth-app-compliancecore-vocabulary.png
- 49. Compliance Core / Rule packs (/app/compliancecore/rules) -> screenshots/49-auth-app-compliancecore-rules.png
- 50. Compliance Core / Open Compliance Core app (/app/compliancecore/launch) -> screenshots/50-auth-app-compliancecore-launch.png
- 51. Companion App / Overview (/app/companion) -> screenshots/51-auth-app-companion.png
- 52. Companion App / Open product app (/app/companion/launch) -> screenshots/52-auth-app-companion-launch.png
- 53. Tenants before create (/app/platform-admin/tenants) -> screenshots/53-data-app-platform-admin-tenants.png
- 54. Tenants after create (/app/platform-admin/tenants) -> screenshots/54-data-app-platform-admin-tenants.png
- 55. Products before create (/app/platform-admin/products) -> screenshots/55-data-app-platform-admin-products.png
- 56. Products after create (/app/platform-admin/products) -> screenshots/56-data-app-platform-admin-products.png
- 57. Entitlements before grant (/app/platform-admin/entitlements) -> screenshots/57-data-app-platform-admin-entitlements.png
- 58. Entitlements after grant (/app/platform-admin/entitlements) -> screenshots/58-data-app-platform-admin-entitlements.png
- 59. Data plane before upsert (/app/platform-admin/data-plane) -> screenshots/59-data-app-platform-admin-data-plane.png
- 60. Data plane after upsert (/app/platform-admin/data-plane) -> screenshots/60-data-app-platform-admin-data-plane.png
- 61. Service tokens before client create (/app/platform-admin/service-tokens) -> screenshots/61-data-app-platform-admin-service-tokens.png
- 62. Service tokens after client create (/app/platform-admin/service-tokens) -> screenshots/62-data-app-platform-admin-service-tokens-after-client-create.png

## Error Log

- No errors captured