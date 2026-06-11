using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using NexArr.Api.Data;
using NexArr.Api.Entities;
using NexArr.Api.Options;

namespace NexArr.Api.Services;

public static class PlatformSeeder
{
    // Test/demo identifiers are kept for test fixtures and explicit admin input flows.
    public static readonly Guid DemoTenantId = Guid.Parse("11111111-1111-1111-1111-111111111101");
    public static readonly Guid DemoAdminUserId = Guid.Parse("22222222-2222-2222-2222-222222222201");

    public static readonly Guid DemoTenantAdminUserId = Guid.Parse("22222222-2222-2222-2222-222222222202");

    public const string DemoAdminEmail = "admin@demo.stl";
    public const string DemoTenantAdminEmail = "tenant-admin@demo.stl";
    public const string DemoAdminPassword = "ChangeMe!Demo2026";
    private const string FirstAdminEmailConfigKey = "Seed:FirstAdminEmail";
    private const string FirstAdminPasswordConfigKey = "Seed:FirstAdminPassword";
    private const string FirstAdminDisplayNameConfigKey = "Seed:FirstAdminDisplayName";
    private const string MasterCsvIntakeDatasetKey = "master-reference-intake";
    private const string MasterCsvSourceKey = "master-reference-csv";

    private static readonly ProductSeed[] Products =
    [
        new("shared-worker", "STL Shared Worker", 5, "platform", "Platform Engineering", "worker", "", "", "stl:shared-worker:worker"),
        new("nexarr-worker", "NexArr Worker", 8, "platform", "Platform Engineering", "worker", "", "", "stl:nexarr-worker:worker"),
        new("nexarr", "NexArr", 10, "platform", "Platform Engineering", "available", "http://localhost:5101", "http://localhost:5101/health/ready", "stl:nexarr:api"),
        new("staffarr", "StaffArr", 20, "workforce", "People Operations", "available", "http://localhost:5102", "http://localhost:5102/health/ready", "stl:staffarr:api"),
        new("trainarr", "TrainArr", 30, "training", "Learning and Qualification", "available", "http://localhost:5103", "http://localhost:5103/health/ready", "stl:trainarr:api"),
        new("maintainarr", "MaintainArr", 40, "maintenance", "Maintenance Operations", "available", "http://localhost:5104", "http://localhost:5104/health/ready", "stl:maintainarr:api"),
        new("routarr", "RoutArr", 50, "transportation", "Transportation Operations", "available", "http://localhost:5105", "http://localhost:5105/health/ready", "stl:routarr:api"),
        new("supplyarr", "SupplyArr", 60, "supply-chain", "Procurement Operations", "available", "http://localhost:5106", "http://localhost:5106/health/ready", "stl:supplyarr:api"),
        new("compliancecore", "Compliance Core", 70, "compliance", "Compliance Platform", "available", "http://localhost:5107", "http://localhost:5107/health/ready", "stl:compliancecore:api"),
        new("loadarr", "LoadArr", 75, "warehouse", "Warehouse Operations", "available", "http://localhost:5108", "http://localhost:5108/health/ready", "stl:loadarr:api"),
        new("assurarr", "AssurArr", 76, "assurance", "Quality and Assurance", "available", "http://localhost:5109", "http://localhost:5109/health/ready", "stl:assurarr:api"),
        new("reportarr", "ReportArr", 77, "analytics", "Analytics and Reporting", "available", "http://localhost:5111", "http://localhost:5111/health/ready", "stl:reportarr:api"),
        new("recordarr", "RecordArr", 78, "records", "Records and Evidence", "available", "http://localhost:5110", "http://localhost:5110/health/ready", "stl:recordarr:api"),
        new("fieldcompanion", "Field Companion", 80, "field-execution", "Platform Engineering", "available", "", "", "stl:fieldcompanion:frontend")
    ];

    private static readonly (string ProductKey, string BaseUrl, string LaunchPath)[] DefaultLaunchProfiles =
    [
        ("nexarr", "http://localhost:5101", "/"),
        ("staffarr", "http://localhost:5175", "/launch"),
        ("trainarr", "http://localhost:5176", "/launch"),
        ("maintainarr", "http://localhost:5178", "/launch"),
        ("routarr", "http://localhost:5180", "/launch"),
        ("supplyarr", "http://localhost:5179", "/launch"),
        ("compliancecore", "http://localhost:5177", "/launch"),
        ("loadarr", "http://localhost:5182", "/launch"),
        ("assurarr", "http://localhost:5183", "/launch"),
        ("reportarr", "http://localhost:5185", "/launch"),
        ("recordarr", "http://localhost:5184", "/launch"),
        ("fieldcompanion", "http://localhost:5181", "/launch")
    ];

    private static readonly string[] MaintainArrReferenceDatasetNames =
    [
        "Asset Class",
        "Asset Type",
        "Asset Subtype",
        "Asset Category",
        "Asset Status",
        "Lifecycle Status",
        "Criticality",
        "Maintenance Class",
        "Service Group",
        "Readiness Status",
        "Operational Status",
        "Availability Status",
        "Make",
        "Manufacturer",
        "Model",
        "Model Year",
        "Trim",
        "Series",
        "Body Style",
        "Body Type",
        "Configuration",
        "Cab Type",
        "Sleeper Cab",
        "Day Cab",
        "Drivetrain",
        "Axle Configuration",
        "Steer Axle Count",
        "Drive Axle Count",
        "Trailer Axle Count",
        "Single Drive Axle",
        "Tandem Drive Axle",
        "Lift Axle Equipped",
        "Tag Axle Equipped",
        "Pusher Axle Equipped",
        "Dual Rear Wheel",
        "Super Singles Equipped",
        "Duals Equipped",
        "Four Wheel Drive",
        "All Wheel Drive",
        "Two Wheel Drive",
        "Tire Configuration",
        "Fuel Type",
        "Secondary Fuel Type",
        "DEF Required",
        "Aftertreatment Type",
        "EV Battery Type",
        "Charging Connector Type",
        "Hybrid Type",
        "CNG Equipped",
        "LNG Equipped",
        "Propane Equipped",
        "Brake System Type",
        "Brake Type",
        "Air Brakes",
        "Hydraulic Brakes",
        "Electric Brakes",
        "Drum Brakes",
        "Disc Brakes",
        "ABS Required",
        "ABS Equipped",
        "Slack Adjuster Type",
        "Brake Chamber Type",
        "Parking Brake Type",
        "Trailer Type",
        "Trailer Body Type",
        "Trailer Door Type",
        "Roof Type",
        "Floor Type",
        "Landing Gear Type",
        "Kingpin Type",
        "Reefer Equipped",
        "Lift Gate Equipped",
        "Axle Spread Type",
        "Suspension Type",
        "Engine Make",
        "Engine Model",
        "Engine Family",
        "Emissions Level",
        "Transmission Make",
        "Transmission Model",
        "Transmission Type",
        "PTO Equipped",
        "PTO Type",
        "Drive Type",
        "Tire Size",
        "Wheel Size",
        "Wheel Material",
        "Tire Position Layout",
        "Steer Tire Size",
        "Drive Tire Size",
        "Trailer Tire Size",
        "Spare Tire Equipped",
        "Retread Allowed",
        "Tread Depth Minimum Rule",
        "Torque Spec",
        "Pressure Spec",
        "Primary Meter Type",
        "Secondary Meter Types",
        "Meter Type",
        "Meter Unit",
        "Usage Profile",
        "Meter Reading Source",
        "Meter Rollover Behavior",
        "PM Program",
        "PM Program Status",
        "PM Template",
        "PM Type",
        "PM Interval Type",
        "Inspection Template Category",
        "Inspection Template Owner Role",
        "Inspection Execution Mode",
        "Inspection Result Mode",
        "Inspection Readiness Impact",
        "Inspection Template",
        "Inspection Type",
        "Required Inspection Types",
        "Inspection Frequency Type",
        "Seasonal PM Group",
        "Regulatory Inspection Required",
        "Annual Inspection Required",
        "DVIR Required",
        "Pre Trip Required",
        "Post Trip Required",
        "Shop Inspection Required",
        "Defect Type",
        "Defect Category",
        "Defect System",
        "Defect Component",
        "Report Source",
        "Symptom",
        "Failure Mode",
        "Severity",
        "Priority",
        "Safety Critical",
        "Operating Restriction",
        "Operating Condition",
        "Side Position",
        "Deferral Code",
        "Repair Disposition",
        "Root Cause",
        "Corrective Action Type",
        "Work Order Type",
        "Work Order Priority",
        "Maintenance Type",
        "Repair Type",
        "Labor Category",
        "Downtime Reason",
        "Shop Status",
        "Bay",
        "Approval Status",
        "Completion Status",
        "Return To Service Status",
        "Document Type",
        "Document Status",
        "Verification Status",
        "Expiration Type",
        "Fluid Spec",
        "Oil Spec",
        "Coolant Spec",
        "DEF Spec",
        "Filter Spec",
        "Belt Spec",
        "Battery Spec",
        "Lamp Spec",
        "Brake Part Spec",
        "Tire Spec",
        "Wheel Spec",
        "Telematics Provider",
        "Diagnostic Protocol",
        "ELD Provider",
        "GPS Provider",
        "Fault Code Source",
        "Fault Code Standard",
        "Data Sync Status"
    ];

    private static readonly string[] StaffArrReferenceDatasetNames =
    [
        "Person Status",
        "Employment Status",
        "Work Relationship Type",
        "Org Unit Type",
        "Site Type",
        "Location Type",
        "Position Type",
        "Team Type",
        "Permission Template",
        "Role Template",
        "Restriction Type",
        "Delegation Type",
        "Incident Type",
        "Readiness Status"
    ];

    private static readonly string[] TrainArrReferenceDatasetNames =
    [
        "Program Type",
        "Program Status",
        "Module Status",
        "Step Status",
        "Assignment Status",
        "Qualification Status",
        "Certificate Status",
        "Evaluation Type",
        "Signoff Role",
        "Remediation Type",
        "Expiration Reason",
        "Training Requirement Type",
        "Evidence Type"
    ];

    private static readonly string[] RoutArrReferenceDatasetNames =
    [
        "Dispatch Status",
        "Route Status",
        "Trip Status",
        "Stop Status",
        "Assignment Status",
        "Equipment Type",
        "Proof Type",
        "Exception Type",
        "ETA Status",
        "Delivery Status",
        "Dock Appointment Status",
        "Readiness Status"
    ];

    private static readonly string[] LoadArrReferenceDatasetNames =
    [
        "Location Behavior",
        "Receipt Status",
        "Putaway Status",
        "Reservation Status",
        "Pick Status",
        "Issue Status",
        "Return Status",
        "Transfer Status",
        "Count Status",
        "Adjustment Reason",
        "Discrepancy Type",
        "Inventory Status",
        "Hold Reason"
    ];

    private static readonly string[] AssurArrReferenceDatasetNames =
    [
        "Nonconformance Type",
        "Severity Level",
        "Hold Type",
        "Containment Type",
        "Disposition Type",
        "CAPA Type",
        "Audit Type",
        "Finding Type",
        "Release Status",
        "Complaint Type",
        "Supplier Quality Issue Type",
        "Verification Status"
    ];

    private static readonly string[] RecordArrReferenceDatasetNames =
    [
        "Document Type",
        "Record Status",
        "Retention Policy",
        "Classification",
        "Evidence Type",
        "Package Type",
        "Access Policy",
        "Legal Hold Reason",
        "OCR Template",
        "Approval Status",
        "Sharing Scope",
        "Scan Type"
    ];

    private static readonly string[] ReportArrReferenceDatasetNames =
    [
        "Dataset Type",
        "Dashboard Type",
        "Metric Type",
        "Schedule Frequency",
        "Export Format",
        "Freshness Status",
        "Alert Type",
        "Drilldown Type",
        "Access Policy",
        "Delivery Channel",
        "Widget Type",
        "Report Run Status"
    ];

    private static readonly string[] FieldCompanionReferenceDatasetNames =
    [
        "Task Type",
        "Capture Type",
        "Offline Action Type",
        "Sync Status",
        "Form Type",
        "Scan Mode",
        "Upload Mode"
    ];

    private static readonly string[] ComplianceCoreReferenceDatasetNames =
    [
        "Governing Bodies",
        "Jurisdictions",
        "Regulation Sources",
        "Citation Types",
        "Requirement Types",
        "Evidence Types",
        "Applicability Subject Types",
        "Asset Compliance Categories",
        "Training Compliance Categories",
        "Document Compliance Categories",
        "Incident Compliance Categories",
        "Maintenance Compliance Categories",
        "Transportation Compliance Categories",
        "Inventory Compliance Categories",
        "Supplier Compliance Categories",
        "Customer Requirement Categories",
        "Exception Types",
        "Exemption Types",
        "Severity Levels",
        "Confidence Levels",
        "Retention Triggers"
    ];

    private static readonly (string Key, string Name)[] SupplyArrReferenceDatasetNames =
    [
        ("party", "Party"),
        ("part", "Part"),
        ("purchase_request", "Purchase Request"),
        ("purchase_order", "Purchase Order"),
        ("receipt", "Receipt"),
        ("warranty_claim", "Warranty Claim")
    ];

    public static async Task SeedAsync(
        NexArrDbContext db,
        IPasswordHasher passwordHasher,
        StlLaunchOptions? launchOptions = null,
        PlatformProductUrlsOptions? platformProductUrls = null,
        CancellationToken cancellationToken = default)
    {
        await SeedInfrastructureAsync(db, launchOptions, platformProductUrls, cancellationToken);
        await SeedDemoBusinessDataAsync(db, passwordHasher, DemoAdminUserId, cancellationToken);
    }

    public static async Task<Guid?> SeedFirstAdminAsync(
        NexArrDbContext db,
        IPasswordHasher passwordHasher,
        IConfiguration configuration,
        Microsoft.AspNetCore.Hosting.IWebHostEnvironment environment,
        CancellationToken cancellationToken = default)
    {
        var resolvedProfile = ResolveFirstAdminProfile(configuration, environment);
        if (resolvedProfile is null)
        {
            return null;
        }

        var now = DateTimeOffset.UtcNow;
        var adminEmail = resolvedProfile.Email;
        var adminDisplayName = resolvedProfile.DisplayName;
        var adminPassword = resolvedProfile.Password;

        var admin = await db.Users
            .Include(x => x.Credential)
            .FirstOrDefaultAsync(x => x.Email == adminEmail, cancellationToken);

        if (admin is null)
        {
            admin = new PlatformUser
            {
                Id = resolvedProfile.UseDeterministicId ? DemoAdminUserId : Guid.NewGuid(),
                Email = adminEmail,
                DisplayName = adminDisplayName,
                IsActive = true,
                IsPlatformAdmin = true,
                CreatedAt = now,
                ModifiedAt = now,
                Credential = new UserCredential
                {
                    UserId = resolvedProfile.UseDeterministicId ? DemoAdminUserId : Guid.Empty,
                    PasswordHash = passwordHasher.Hash(adminPassword),
                    PasswordChangedAt = now
                }
            };
            if (!resolvedProfile.UseDeterministicId)
            {
                admin.Credential.UserId = admin.Id;
            }

            db.Users.Add(admin);
        }
        else
        {
            admin.IsActive = true;
            admin.DisplayName = string.IsNullOrWhiteSpace(admin.DisplayName)
                ? adminDisplayName
                : admin.DisplayName;
            admin.IsPlatformAdmin = true;
            admin.ModifiedAt = now;

            if (admin.Credential is null)
            {
                admin.Credential = new UserCredential
                {
                    UserId = admin.Id,
                    PasswordHash = passwordHasher.Hash(adminPassword),
                    PasswordChangedAt = now
                };
            }
        }

        if (!await db.PlatformRoleAssignments.AnyAsync(
                x => x.UserId == admin.Id
                     && x.TenantId == null
                     && x.RoleKey == "platform_owner",
                cancellationToken))
        {
            db.PlatformRoleAssignments.Add(new PlatformRoleAssignment
            {
                Id = Guid.NewGuid(),
                UserId = admin.Id,
                TenantId = null,
                RoleKey = "platform_owner",
                CreatedAt = now,
                CreatedByUserId = admin.Id,
            });
        }

        await db.SaveChangesAsync(cancellationToken);
        return admin.Id;
    }

    public static async Task SeedMasterReferenceDataAsync(
        NexArrDbContext db,
        string csvFilePath,
        Guid? firstAdminUserId = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(csvFilePath))
        {
            return;
        }

        if (!File.Exists(csvFilePath))
        {
            return;
        }

        var csvText = await File.ReadAllTextAsync(csvFilePath, cancellationToken);
        if (string.IsNullOrWhiteSpace(csvText))
        {
            return;
        }

        var normalizedRows = await ParseReferenceSeedRowsAsync(db, csvText, cancellationToken);
        var rows = normalizedRows.ToList();
        if (rows.Count == 0)
        {
            return;
        }

        var now = DateTimeOffset.UtcNow;
        var datasetIndex = await db.ReferenceDatasets
            .AsNoTracking()
            .ToDictionaryAsync(x => x.Key, x => x, StringComparer.OrdinalIgnoreCase, cancellationToken);
        var datasetIndexById = datasetIndex.Values.ToDictionary(x => x.Id, x => x);

        var stagedRows = rows.Select(row =>
            new StagingRecord
            {
                Id = Guid.NewGuid(),
                JobId = Guid.Empty,
                TargetDatasetId = row.TargetDatasetId,
                RowNumber = row.RowNumber,
                RawPayloadJson = row.RawPayloadJson,
                NormalizedPayloadJson = row.NormalizedPayloadJson,
                ProposedEntityType = row.EntityType,
                ProposedCanonicalKey = row.CanonicalKey,
                Confidence = row.Confidence,
                Status = ReferenceStagingStatuses.NeedsReview,
                ReviewReason = row.ReviewReason,
                CreatedAt = now,
                UpdatedAt = now,
            })
            .ToList();

        if (stagedRows.Count == 0)
        {
            return;
        }

        var source = await EnsureMasterCsvSeedSourceAsync(db, cancellationToken);
        var sourceFingerprint = ComputeSha256Hex(csvText);
        var jobFileKey = $"stl://reference-seed/{sourceFingerprint}";
        var fileName = Path.GetFileName(csvFilePath);

        var existingCompletedJob = await db.IngestionJobs.AnyAsync(
            x => x.DatasetId != Guid.Empty
                 && x.SourceId == source.Id
                 && x.FileName == fileName
                 && x.RawObjectKey == jobFileKey
                 && x.Status == ReferenceImportStatuses.Completed,
            cancellationToken);
        if (existingCompletedJob)
        {
            return;
        }

        var masterDataset = await EnsureMasterCsvIntakeDatasetAsync(db, cancellationToken);
        var importJob = new IngestionJob
        {
            Id = Guid.NewGuid(),
            DatasetId = masterDataset.Id,
            SourceId = source.Id,
            TenantId = null,
            RequestedByPersonId = firstAdminUserId,
            Status = ReferenceImportStatuses.ReviewRequired,
            RawObjectKey = jobFileKey,
            FileName = fileName,
            StartedAt = now,
            CreatedAt = now,
            UpdatedAt = now
        };

        db.IngestionJobs.Add(importJob);
        await db.SaveChangesAsync(cancellationToken);

        foreach (var staged in stagedRows)
        {
            staged.JobId = importJob.Id;
            db.StagingRecords.Add(staged);
        }

        await db.SaveChangesAsync(cancellationToken);

        var rowsToApprove = await db.StagingRecords
            .Where(x => x.JobId == importJob.Id)
            .ToListAsync(cancellationToken);
        foreach (var staged in rowsToApprove)
        {
            if (staged.TargetDatasetId is null)
            {
                continue;
            }

            if (staged.TargetDatasetId is null || !datasetIndexById.TryGetValue(staged.TargetDatasetId.Value, out var targetDataset))
            {
                continue;
            }

            var entity = await UpsertReferenceEntityFromSeedAsync(
                db,
                staged,
                targetDataset,
                source.Key,
                source.Id,
                now,
                cancellationToken);
            staged.ReferenceEntityId = entity.Id;
            staged.Status = ReferenceStagingStatuses.Approved;
            staged.ReviewerPersonId = firstAdminUserId;
            staged.ReviewedAt = now;
            staged.UpdatedAt = now;
        }

        var pending = await db.StagingRecords.CountAsync(
            x => x.JobId == importJob.Id && x.Status != ReferenceStagingStatuses.Approved,
            cancellationToken);
        importJob.Status = pending == 0 ? ReferenceImportStatuses.Completed : ReferenceImportStatuses.ReviewRequired;
        importJob.CompletedAt = pending == 0 ? now : null;
        importJob.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);
    }

    private static FirstAdminSeedProfile? ResolveFirstAdminProfile(
        IConfiguration configuration,
        Microsoft.AspNetCore.Hosting.IWebHostEnvironment environment)
    {
        var configuredEmail = configuration[FirstAdminEmailConfigKey];
        var configuredPassword = configuration[FirstAdminPasswordConfigKey];
        if (!string.IsNullOrWhiteSpace(configuredEmail) && !string.IsNullOrWhiteSpace(configuredPassword))
        {
            return new FirstAdminSeedProfile(
                configuredEmail.Trim().ToLowerInvariant(),
                configuredPassword.Trim(),
                configuration[FirstAdminDisplayNameConfigKey]?.Trim() ?? "Platform Admin",
                false);
        }

        if (environment.IsDevelopment() || environment.EnvironmentName == "Testing")
        {
            return new FirstAdminSeedProfile(DemoAdminEmail, DemoAdminPassword, "Demo Platform Admin", true);
        }

        return null;
    }

    public static async Task SeedDemoBusinessDataAsync(
        NexArrDbContext db,
        IPasswordHasher passwordHasher,
        Guid? firstAdminUserId = null,
        CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var resolvedAdminUserId = firstAdminUserId ?? DemoAdminUserId;
        var isDemoAdminFromSeed = firstAdminUserId is null;

        if (!await db.Tenants.AnyAsync(t => t.Id == DemoTenantId, cancellationToken))
        {
            db.Tenants.Add(new Tenant
            {
                Id = DemoTenantId,
                Slug = "demo-stl",
                DisplayName = "STL Demo Tenant",
                Status = TenantStatuses.Active,
                CreatedAt = now,
                ModifiedAt = now
            });
        }

        if (!await db.Users.AnyAsync(u => u.Id == resolvedAdminUserId, cancellationToken))
        {
            db.Users.Add(new PlatformUser
            {
                Id = resolvedAdminUserId,
                Email = DemoAdminEmail,
                DisplayName = "Demo Platform Admin",
                IsActive = true,
                IsPlatformAdmin = true,
                CreatedAt = now,
                ModifiedAt = now,
                Credential = new UserCredential
                {
                    UserId = resolvedAdminUserId,
                    PasswordHash = passwordHasher.Hash(DemoAdminPassword),
                    PasswordChangedAt = now
                }
            });
            if (!isDemoAdminFromSeed)
            {
                await db.SaveChangesAsync(cancellationToken);
            }
        }

        if (!await db.TenantMemberships.AnyAsync(
                m => m.TenantId == DemoTenantId && m.UserId == resolvedAdminUserId,
                cancellationToken))
        {
            db.TenantMemberships.Add(new TenantMembership
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333301"),
                TenantId = DemoTenantId,
                UserId = resolvedAdminUserId,
                RoleKey = "platform_admin",
                IsActive = true,
                CreatedAt = now
            });
        }

        await EnsureDemoOwnerRoleAsync(db, resolvedAdminUserId, cancellationToken);

        if (!await db.Users.AnyAsync(u => u.Id == DemoTenantAdminUserId, cancellationToken))
        {
            db.Users.Add(new PlatformUser
            {
                Id = DemoTenantAdminUserId,
                Email = DemoTenantAdminEmail,
                DisplayName = "Demo Tenant Admin",
                IsActive = true,
                IsPlatformAdmin = false,
                CreatedAt = now,
                ModifiedAt = now,
                Credential = new UserCredential
                {
                    UserId = DemoTenantAdminUserId,
                    PasswordHash = passwordHasher.Hash(DemoAdminPassword),
                    PasswordChangedAt = now
                }
            });
        }

        if (!await db.TenantMemberships.AnyAsync(
                m => m.TenantId == DemoTenantId && m.UserId == DemoTenantAdminUserId,
                cancellationToken))
        {
            db.TenantMemberships.Add(new TenantMembership
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333302"),
                TenantId = DemoTenantId,
                UserId = DemoTenantAdminUserId,
                RoleKey = "tenant_admin",
                IsActive = true,
                CreatedAt = now
            });
        }

        foreach (var product in Products)
        {
            if (await db.Entitlements.AnyAsync(
                    e => e.TenantId == DemoTenantId && e.ProductKey == product.Key,
                    cancellationToken))
            {
                continue;
            }

            db.Entitlements.Add(new TenantProductEntitlement
            {
                Id = Guid.NewGuid(),
                TenantId = DemoTenantId,
                ProductKey = product.Key,
                Status = EntitlementStatuses.Active,
                GrantedAt = now
            });
        }

        foreach (var product in Products)
        {
            if (await db.TenantProductLicenses.AnyAsync(
                    l => l.TenantId == DemoTenantId && l.ProductKey == product.Key,
                    cancellationToken))
            {
                continue;
            }

            db.TenantProductLicenses.Add(new TenantProductLicense
            {
                Id = Guid.NewGuid(),
                TenantId = DemoTenantId,
                ProductKey = product.Key,
                Status = LicenseStatuses.Active,
                ValidFrom = now.AddYears(-1),
                ValidTo = now.AddYears(1),
                CreatedAt = now,
                ModifiedAt = now,
            });
        }

        SeedDemoTenantCallbackAllowlist(db, now);
        await db.SaveChangesAsync(cancellationToken);
    }

    private static async Task EnsureDemoOwnerRoleAsync(
        NexArrDbContext db,
        Guid userId,
        CancellationToken cancellationToken)
    {
        var hasOwner = await db.PlatformRoleAssignments.AnyAsync(
            x => x.UserId == userId
                 && x.TenantId == null
                 && x.RoleKey == "platform_owner",
            cancellationToken);
        if (hasOwner)
        {
            return;
        }

        db.PlatformRoleAssignments.Add(new PlatformRoleAssignment
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TenantId = null,
            RoleKey = "platform_owner",
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedByUserId = userId,
        });
    }

    public static async Task SeedInfrastructureAsync(
        NexArrDbContext db,
        StlLaunchOptions? launchOptions = null,
        PlatformProductUrlsOptions? platformProductUrls = null,
        CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        await EnsureProductCatalogManifestColumnsAsync(db, cancellationToken);
        await EnsurePlatformSessionSettingsColumnsAsync(db, cancellationToken);
        await EnsureProductCatalogAsync(db, platformProductUrls, cancellationToken);
        await SeedReferenceDatasetsAsync(db, now, cancellationToken);
        await SeedLaunchProfilesAsync(db, launchOptions, now, cancellationToken);
        SeedSuiteShellCallbackAllowlist(db, now);
        await db.SaveChangesAsync(cancellationToken);
    }

    private static async Task EnsureProductCatalogManifestColumnsAsync(
        NexArrDbContext db,
        CancellationToken cancellationToken)
    {
        if (!db.Database.IsRelational())
        {
            return;
        }

        // Keep startup resilient when a deployed DB has older `product_catalog` shape.
        await db.Database.ExecuteSqlRawAsync(
            """
            ALTER TABLE product_catalog
                ADD COLUMN IF NOT EXISTS "ProductCategory" character varying(64) NOT NULL DEFAULT 'operations',
                ADD COLUMN IF NOT EXISTS "ProductOwner" character varying(128) NOT NULL DEFAULT 'STL Compliance',
                ADD COLUMN IF NOT EXISTS "ProductStatus" character varying(32) NOT NULL DEFAULT 'available',
                ADD COLUMN IF NOT EXISTS "CanonicalCallbackPath" character varying(128) NOT NULL DEFAULT '/auth/nexarr/callback',
                ADD COLUMN IF NOT EXISTS "ApiBaseUrl" character varying(512) NOT NULL DEFAULT '',
                ADD COLUMN IF NOT EXISTS "HealthUrl" character varying(512) NOT NULL DEFAULT '',
                ADD COLUMN IF NOT EXISTS "ServiceAudience" character varying(128) NOT NULL DEFAULT '',
                ADD COLUMN IF NOT EXISTS "MarketingUrl" character varying(512) NOT NULL DEFAULT '',
                ADD COLUMN IF NOT EXISTS "DocumentationUrl" character varying(512) NOT NULL DEFAULT '',
                ADD COLUMN IF NOT EXISTS "SupportUrl" character varying(512) NOT NULL DEFAULT '',
                ADD COLUMN IF NOT EXISTS "EnvironmentKey" character varying(64) NOT NULL DEFAULT 'local',
                ADD COLUMN IF NOT EXISTS "EntitlementDependencyRules" character varying(2048) NOT NULL DEFAULT '',
                ADD COLUMN IF NOT EXISTS "ProductDependencyMetadata" character varying(2048) NOT NULL DEFAULT '';
            """,
            cancellationToken);
    }

    private static async Task EnsurePlatformSessionSettingsColumnsAsync(
        NexArrDbContext db,
        CancellationToken cancellationToken)
    {
        if (!db.Database.IsRelational())
        {
            return;
        }

        // Keep login and password-policy reads resilient when a deployed DB
        // is missing the newer session-settings columns.
        await db.Database.ExecuteSqlRawAsync(
            """
            ALTER TABLE nexarr_platform_session_settings
                ADD COLUMN IF NOT EXISTS "RequirePlatformAdminMfa" boolean NULL,
                ADD COLUMN IF NOT EXISTS "PasswordMinLength" integer NOT NULL DEFAULT 12,
                ADD COLUMN IF NOT EXISTS "RequirePasswordComplexity" boolean NOT NULL DEFAULT true;
            """,
            cancellationToken);
    }

    private static async Task EnsureProductCatalogAsync(
        NexArrDbContext db,
        PlatformProductUrlsOptions? platformProductUrls,
        CancellationToken cancellationToken)
    {
        foreach (var seed in Products)
        {
            var effectiveSeed = ApplyConfiguredProductUrl(seed, platformProductUrls);
            var product = await db.ProductCatalog
                .FirstOrDefaultAsync(p => p.ProductKey == effectiveSeed.Key, cancellationToken);
            if (product is null)
            {
                db.ProductCatalog.Add(CreateProduct(effectiveSeed));
                continue;
            }

            ApplyMissingManifestMetadata(product, effectiveSeed);
        }

        static ProductCatalogItem CreateProduct(ProductSeed seed) =>
            new()
            {
                ProductKey = seed.Key,
                DisplayName = seed.Name,
                ProductCategory = seed.Category,
                ProductOwner = seed.Owner,
                ProductStatus = seed.Status,
                SortOrder = seed.Order,
                IsActive = true,
                CanonicalCallbackPath = "/auth/nexarr/callback",
                ApiBaseUrl = seed.ApiBaseUrl,
                HealthUrl = seed.HealthUrl,
                ServiceAudience = seed.ServiceAudience,
                MarketingUrl = $"https://stlcompliance.com/products/{seed.Key}",
                DocumentationUrl = $"https://stlcompliance.com/docs/{seed.Key}",
                SupportUrl = "https://stlcompliance.com/support",
                EnvironmentKey = ResolveEnvironmentKey(seed.ApiBaseUrl),
                EntitlementDependencyRules = seed.Key is "shared-worker" or "nexarr-worker"
                    ? "internal-platform-worker"
                    : "tenant-product-entitlement-required",
                ProductDependencyMetadata = ResolveDependencyMetadata(seed.Key),
            };

        static void ApplyMissingManifestMetadata(ProductCatalogItem product, ProductSeed seed)
        {
            if (string.IsNullOrWhiteSpace(product.ProductCategory))
            {
                product.ProductCategory = seed.Category;
            }

            if (seed.Key is "fieldcompanion"
                && product.DisplayName.Equals("fieldcompanion App", StringComparison.OrdinalIgnoreCase))
            {
                product.DisplayName = seed.Name;
            }

            if (string.IsNullOrWhiteSpace(product.ProductOwner))
            {
                product.ProductOwner = seed.Owner;
            }

            if (string.IsNullOrWhiteSpace(product.ProductStatus))
            {
                product.ProductStatus = seed.Status;
            }

            if (string.IsNullOrWhiteSpace(product.CanonicalCallbackPath))
            {
                product.CanonicalCallbackPath = "/auth/nexarr/callback";
            }

            if (string.IsNullOrWhiteSpace(product.ApiBaseUrl))
            {
                product.ApiBaseUrl = seed.ApiBaseUrl;
            }
            else if (IsLocalUrl(product.ApiBaseUrl) && !IsLocalUrl(seed.ApiBaseUrl))
            {
                product.ApiBaseUrl = seed.ApiBaseUrl;
            }

            if (string.IsNullOrWhiteSpace(product.HealthUrl))
            {
                product.HealthUrl = seed.HealthUrl;
            }
            else if (IsLocalUrl(product.HealthUrl) && !IsLocalUrl(seed.HealthUrl))
            {
                product.HealthUrl = seed.HealthUrl;
            }

            if (string.IsNullOrWhiteSpace(product.ServiceAudience))
            {
                product.ServiceAudience = seed.ServiceAudience;
            }

            if (string.IsNullOrWhiteSpace(product.MarketingUrl))
            {
                product.MarketingUrl = $"https://stlcompliance.com/products/{seed.Key}";
            }

            if (string.IsNullOrWhiteSpace(product.DocumentationUrl))
            {
                product.DocumentationUrl = $"https://stlcompliance.com/docs/{seed.Key}";
            }

            if (string.IsNullOrWhiteSpace(product.SupportUrl))
            {
                product.SupportUrl = "https://stlcompliance.com/support";
            }

            if (string.IsNullOrWhiteSpace(product.EnvironmentKey))
            {
                product.EnvironmentKey = "local";
            }
            else if (product.EnvironmentKey.Equals("local", StringComparison.OrdinalIgnoreCase)
                && ShouldUseProductionEnvironmentKey(seed.ApiBaseUrl))
            {
                product.EnvironmentKey = "production";
            }

            if (string.IsNullOrWhiteSpace(product.EntitlementDependencyRules))
            {
                product.EntitlementDependencyRules = seed.Key is "shared-worker" or "nexarr-worker"
                    ? "internal-platform-worker"
                    : "tenant-product-entitlement-required";
            }

            if (string.IsNullOrWhiteSpace(product.ProductDependencyMetadata))
            {
                product.ProductDependencyMetadata = ResolveDependencyMetadata(seed.Key);
            }
        }
    }

    private static ProductSeed ApplyConfiguredProductUrl(
        ProductSeed seed,
        PlatformProductUrlsOptions? platformProductUrls)
    {
        var configuredBaseUrl = seed.Key switch
        {
            "nexarr" => platformProductUrls?.NexArrBaseUrl,
            "staffarr" => platformProductUrls?.StaffArrBaseUrl,
            "trainarr" => platformProductUrls?.TrainArrBaseUrl,
            "maintainarr" => platformProductUrls?.MaintainArrBaseUrl,
            "routarr" => platformProductUrls?.RoutArrBaseUrl,
            "supplyarr" => platformProductUrls?.SupplyArrBaseUrl,
            "compliancecore" => platformProductUrls?.ComplianceCoreBaseUrl,
            "loadarr" => platformProductUrls?.LoadArrBaseUrl,
            "assurarr" => platformProductUrls?.AssurArrBaseUrl,
            "reportarr" => platformProductUrls?.ReportArrBaseUrl,
            "recordarr" => platformProductUrls?.RecordArrBaseUrl,
            _ => null
        };

        if (string.IsNullOrWhiteSpace(configuredBaseUrl))
        {
            return seed;
        }

        var normalizedBaseUrl = configuredBaseUrl.Trim().TrimEnd('/');
        if (string.IsNullOrWhiteSpace(normalizedBaseUrl) || string.Equals(normalizedBaseUrl, seed.ApiBaseUrl, StringComparison.OrdinalIgnoreCase))
        {
            return seed;
        }

        return seed with
        {
            ApiBaseUrl = normalizedBaseUrl,
            HealthUrl = $"{normalizedBaseUrl}/health/ready"
        };
    }

    private static bool IsLocalUrl(string? url) =>
        !string.IsNullOrWhiteSpace(url)
        && (url.Contains("localhost", StringComparison.OrdinalIgnoreCase)
            || url.Contains("127.0.0.1", StringComparison.OrdinalIgnoreCase)
            || url.Contains("[::1]", StringComparison.OrdinalIgnoreCase));

    private static string ResolveEnvironmentKey(string? apiBaseUrl) =>
        ShouldUseProductionEnvironmentKey(apiBaseUrl) ? "production" : "local";

    private static bool ShouldUseProductionEnvironmentKey(string? apiBaseUrl) =>
        !string.IsNullOrWhiteSpace(apiBaseUrl) && !IsLocalUrl(apiBaseUrl);

    private static string ResolveDependencyMetadata(string productKey) =>
        productKey switch
        {
            "nexarr" => "identity,tenant,entitlement,launch,service-token",
            "staffarr" => "nexarr,trainarr,compliancecore",
            "trainarr" => "nexarr,staffarr,compliancecore",
            "maintainarr" => "nexarr,staffarr,trainarr,supplyarr,compliancecore",
            "routarr" => "nexarr,staffarr,trainarr,maintainarr,supplyarr,compliancecore",
            "supplyarr" => "nexarr,staffarr,maintainarr,routarr,trainarr,compliancecore",
            "compliancecore" => "nexarr,staffarr,trainarr,maintainarr,routarr,supplyarr",
            "loadarr" => "nexarr,staffarr,supplyarr,routarr",
            "assurarr" => "nexarr,staffarr,trainarr,maintainarr,routarr,supplyarr,compliancecore,loadarr,recordarr",
            "reportarr" => "nexarr,staffarr,trainarr,maintainarr,routarr,supplyarr,compliancecore,loadarr,recordarr,assurarr",
            "recordarr" => "nexarr,staffarr,trainarr,maintainarr,routarr,supplyarr,compliancecore,loadarr",
            "fieldcompanion" => "nexarr,staffarr,trainarr,maintainarr,routarr,supplyarr",
            _ => "nexarr"
        };

    private static async Task SeedLaunchProfilesAsync(
        NexArrDbContext db,
        StlLaunchOptions? launchOptions,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        foreach (var profile in DefaultLaunchProfiles)
        {
            var configured = launchOptions?.Products.GetValueOrDefault(profile.ProductKey);
            var baseUrl = configured?.BaseUrl ?? profile.BaseUrl;
            var launchPath = configured?.LaunchPath ?? profile.LaunchPath;
            var existing = await db.LaunchProfiles
                .FirstOrDefaultAsync(p => p.ProductKey == profile.ProductKey, cancellationToken);

            if (existing is not null)
            {
                if (configured is not null
                    && (!string.Equals(existing.BaseUrl, baseUrl, StringComparison.OrdinalIgnoreCase)
                        || !string.Equals(existing.LaunchPath, launchPath, StringComparison.OrdinalIgnoreCase)))
                {
                    existing.BaseUrl = baseUrl;
                    existing.LaunchPath = launchPath;
                    existing.IsActive = true;
                    existing.ModifiedAt = now;
                }

                continue;
            }

            db.LaunchProfiles.Add(new ProductLaunchProfile
            {
                ProductKey = profile.ProductKey,
                BaseUrl = baseUrl,
                LaunchPath = launchPath,
                IsActive = true,
                ModifiedAt = now
            });
        }
    }

    private static readonly string[] SuiteShellOrigins =
    [
        "http://localhost:5173",
        "http://localhost:5174",
        "http://localhost:5175"
    ];

    public static Task EnsureDevSuiteShellOriginsAsync(
        NexArrDbContext db,
        CancellationToken cancellationToken = default) =>
        EnsureSuiteShellOriginsAsync(db, SuiteShellOrigins, cancellationToken);

    public static async Task EnsureSuiteShellOriginsAsync(
        NexArrDbContext db,
        IEnumerable<string> origins,
        CancellationToken cancellationToken = default)
    {
        foreach (var product in Products)
        {
            foreach (var origin in origins
                         .Select(NormalizeCallbackOrigin)
                         .Where(static value => !string.IsNullOrWhiteSpace(value))
                         .Select(static value => value!))
            {
                var exists = await db.CallbackAllowlist.AnyAsync(
                    e => e.ProductKey == product.Key
                        && e.TenantId == null
                        && e.UrlPattern == origin
                        && e.IsActive,
                    cancellationToken);
                if (exists)
                {
                    continue;
                }

                var now = DateTimeOffset.UtcNow;
                db.CallbackAllowlist.Add(new ProductCallbackAllowlistEntry
                {
                    Id = Guid.NewGuid(),
                    ProductKey = product.Key,
                    TenantId = null,
                    UrlPattern = origin,
                    PatternType = CallbackPatternTypes.Origin,
                    IsActive = true,
                    CreatedAt = now,
                    ModifiedAt = now
                });
            }
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private static string? NormalizeCallbackOrigin(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim().TrimEnd('/');
        if (Uri.TryCreate(trimmed, UriKind.Absolute, out var uri))
        {
            return $"{uri.Scheme}://{uri.Authority}";
        }

        return trimmed;
    }

    private static void SeedSuiteShellCallbackAllowlist(NexArrDbContext db, DateTimeOffset now)
    {
        foreach (var product in Products)
        {
            foreach (var suiteShellOrigin in SuiteShellOrigins
                         .Select(NormalizeCallbackOrigin)
                         .Where(static value => !string.IsNullOrWhiteSpace(value))
                         .Select(static value => value!))
            {
                db.CallbackAllowlist.Add(new ProductCallbackAllowlistEntry
                {
                    Id = Guid.NewGuid(),
                    ProductKey = product.Key,
                    TenantId = null,
                    UrlPattern = suiteShellOrigin,
                    PatternType = CallbackPatternTypes.Origin,
                    IsActive = true,
                    CreatedAt = now,
                    ModifiedAt = now
                });
            }

            var launchProfileIndex = Array.FindIndex(
                DefaultLaunchProfiles,
                p => string.Equals(p.ProductKey, product.Key, StringComparison.Ordinal));
            if (launchProfileIndex < 0)
            {
                continue;
            }

        }
    }

    private static async Task SeedReferenceDatasetsAsync(
        NexArrDbContext db,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var existingDatasets = await db.ReferenceDatasets.ToListAsync(cancellationToken);
        var existingByKey = existingDatasets.ToDictionary(x => x.Key, StringComparer.OrdinalIgnoreCase);

        foreach (var seed in GetReferenceDatasetSeeds())
        {
            var key = seed.Key;
            var category = seed.Category;

            if (existingByKey.TryGetValue(key, out var existing))
            {
                existing.Name = seed.Name;
                existing.Category = category;
                existing.OwnerService = seed.OwnerService;
                existing.Status = ReferenceDatasetStatuses.Ready;
                existing.UpdatedAt = now;
                continue;
            }

            db.ReferenceDatasets.Add(new ReferenceDataset
            {
                Id = Guid.NewGuid(),
                Key = key,
                Name = seed.Name,
                Category = category,
                OwnerService = seed.OwnerService,
                Status = ReferenceDatasetStatuses.Ready,
                CreatedAt = now,
                UpdatedAt = now
            });
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private static void SeedDemoTenantCallbackAllowlist(NexArrDbContext db, DateTimeOffset now)
    {
        foreach (var product in Products)
        {
            var launchProfileIndex = Array.FindIndex(
                DefaultLaunchProfiles,
                p => string.Equals(p.ProductKey, product.Key, StringComparison.Ordinal));
            if (launchProfileIndex < 0)
            {
                continue;
            }

            var launchProfile = DefaultLaunchProfiles[launchProfileIndex];
            var launchProfileOrigin = NormalizeCallbackOrigin(launchProfile.BaseUrl);
            if (string.IsNullOrWhiteSpace(launchProfileOrigin))
            {
                continue;
            }

            db.CallbackAllowlist.Add(new ProductCallbackAllowlistEntry
            {
                Id = Guid.NewGuid(),
                ProductKey = product.Key,
                TenantId = DemoTenantId,
                UrlPattern = launchProfileOrigin,
                PatternType = CallbackPatternTypes.Origin,
                IsActive = true,
                CreatedAt = now,
                ModifiedAt = now
            });
        }
    }

    private static IEnumerable<ReferenceDatasetSeed> GetReferenceDatasetSeeds()
    {
        foreach (var datasetName in MaintainArrReferenceDatasetNames)
        {
            yield return new ReferenceDatasetSeed(
                NormalizeKey($"MaintainArr {datasetName}"),
                datasetName,
                ResolveMaintainArrReferenceDatasetCategory(datasetName),
                "MaintainArr");
        }

        foreach (var datasetName in ComplianceCoreReferenceDatasetNames)
        {
            yield return new ReferenceDatasetSeed(
                NormalizeKey($"ComplianceCore {datasetName}"),
                datasetName,
                "compliance",
                "Compliance Core");
        }

        foreach (var datasetName in StaffArrReferenceDatasetNames)
        {
            yield return new ReferenceDatasetSeed(
                NormalizeKey($"StaffArr {datasetName}"),
                datasetName,
                ResolveStaffArrReferenceDatasetCategory(datasetName),
                "StaffArr");
        }

        foreach (var datasetName in TrainArrReferenceDatasetNames)
        {
            yield return new ReferenceDatasetSeed(
                NormalizeKey($"TrainArr {datasetName}"),
                datasetName,
                ResolveTrainArrReferenceDatasetCategory(datasetName),
                "TrainArr");
        }

        foreach (var datasetName in RoutArrReferenceDatasetNames)
        {
            yield return new ReferenceDatasetSeed(
                NormalizeKey($"RoutArr {datasetName}"),
                datasetName,
                ResolveRoutArrReferenceDatasetCategory(datasetName),
                "RoutArr");
        }

        foreach (var datasetName in LoadArrReferenceDatasetNames)
        {
            yield return new ReferenceDatasetSeed(
                NormalizeKey($"LoadArr {datasetName}"),
                datasetName,
                ResolveLoadArrReferenceDatasetCategory(datasetName),
                "LoadArr");
        }

        foreach (var datasetName in AssurArrReferenceDatasetNames)
        {
            yield return new ReferenceDatasetSeed(
                NormalizeKey($"AssurArr {datasetName}"),
                datasetName,
                ResolveAssurArrReferenceDatasetCategory(datasetName),
                "AssurArr");
        }

        foreach (var datasetName in RecordArrReferenceDatasetNames)
        {
            yield return new ReferenceDatasetSeed(
                NormalizeKey($"RecordArr {datasetName}"),
                datasetName,
                ResolveRecordArrReferenceDatasetCategory(datasetName),
                "RecordArr");
        }

        foreach (var datasetName in ReportArrReferenceDatasetNames)
        {
            yield return new ReferenceDatasetSeed(
                NormalizeKey($"ReportArr {datasetName}"),
                datasetName,
                ResolveReportArrReferenceDatasetCategory(datasetName),
                "ReportArr");
        }

        foreach (var datasetName in FieldCompanionReferenceDatasetNames)
        {
            yield return new ReferenceDatasetSeed(
                NormalizeKey($"Field Companion {datasetName}"),
                datasetName,
                ResolveFieldCompanionReferenceDatasetCategory(datasetName),
                "Field Companion");
        }

        foreach (var dataset in SupplyArrReferenceDatasetNames)
        {
            yield return new ReferenceDatasetSeed(
                NormalizeKey($"SupplyArr {dataset.Name}"),
                dataset.Name,
                "supply",
                "SupplyArr");
        }
    }

    private static string ResolveStaffArrReferenceDatasetCategory(string datasetName) =>
        datasetName switch
        {
            "Person Status" or "Employment Status" or "Readiness Status" => "people",
            "Work Relationship Type" or "Org Unit Type" or "Position Type" or "Team Type" => "organization",
            "Site Type" or "Location Type" => "location",
            "Permission Template" or "Role Template" or "Restriction Type" or "Delegation Type" => "permission",
            "Incident Type" => "incident",
            _ => "reference"
        };

    private static string ResolveTrainArrReferenceDatasetCategory(string datasetName) =>
        datasetName switch
        {
            "Program Type" or "Program Status" or "Module Status" or "Step Status" => "training",
            "Assignment Status" => "assignment",
            "Qualification Status" or "Certificate Status" => "qualification",
            "Evaluation Type" or "Signoff Role" => "evaluation",
            "Remediation Type" or "Expiration Reason" => "remediation",
            "Training Requirement Type" => "requirement",
            "Evidence Type" => "evidence",
            _ => "reference"
        };

    private static string ResolveRoutArrReferenceDatasetCategory(string datasetName) =>
        datasetName switch
        {
            "Dispatch Status" or "Route Status" or "Trip Status" or "Stop Status" or "Delivery Status" => "transportation",
            "Assignment Status" => "assignment",
            "Equipment Type" or "Readiness Status" => "equipment",
            "Proof Type" => "proof",
            "Exception Type" => "exception",
            "ETA Status" => "eta",
            "Dock Appointment Status" => "dock",
            _ => "reference"
        };

    private static string ResolveLoadArrReferenceDatasetCategory(string datasetName) =>
        datasetName switch
        {
            "Location Behavior" => "warehouse",
            "Receipt Status" => "receiving",
            "Putaway Status" => "putaway",
            "Reservation Status" or "Pick Status" or "Issue Status" or "Transfer Status" => "movement",
            "Return Status" => "returns",
            "Count Status" => "count",
            "Adjustment Reason" or "Discrepancy Type" => "adjustment",
            "Inventory Status" => "inventory",
            "Hold Reason" => "hold",
            _ => "reference"
        };

    private static string ResolveAssurArrReferenceDatasetCategory(string datasetName) =>
        datasetName switch
        {
            "Nonconformance Type" or "Complaint Type" or "Supplier Quality Issue Type" => "quality",
            "Severity Level" => "severity",
            "Hold Type" => "hold",
            "Containment Type" => "containment",
            "Disposition Type" => "disposition",
            "CAPA Type" => "capa",
            "Audit Type" or "Finding Type" => "audit",
            "Release Status" => "release",
            "Verification Status" => "verification",
            _ => "reference"
        };

    private static string ResolveRecordArrReferenceDatasetCategory(string datasetName) =>
        datasetName switch
        {
            "Document Type" or "Classification" or "OCR Template" => "document",
            "Record Status" or "Approval Status" => "record",
            "Retention Policy" or "Legal Hold Reason" => "retention",
            "Evidence Type" or "Package Type" => "evidence",
            "Access Policy" or "Sharing Scope" => "access",
            "Scan Type" => "scan",
            _ => "reference"
        };

    private static string ResolveReportArrReferenceDatasetCategory(string datasetName) =>
        datasetName switch
        {
            "Dataset Type" or "Dashboard Type" or "Widget Type" => "dashboard",
            "Metric Type" => "metric",
            "Schedule Frequency" => "schedule",
            "Export Format" => "export",
            "Freshness Status" => "freshness",
            "Alert Type" => "alert",
            "Drilldown Type" => "drilldown",
            "Access Policy" => "access",
            "Delivery Channel" => "delivery",
            "Report Run Status" => "run",
            _ => "reference"
        };

    private static string ResolveFieldCompanionReferenceDatasetCategory(string datasetName) =>
        datasetName switch
        {
            "Task Type" => "task",
            "Capture Type" or "Scan Mode" => "capture",
            "Offline Action Type" => "offline",
            "Sync Status" => "sync",
            "Form Type" => "form",
            "Upload Mode" => "upload",
            _ => "reference"
        };

    private static string ResolveMaintainArrReferenceDatasetCategory(string datasetName) =>
        datasetName switch
        {
            "PM Program" or "PM Program Status" or "PM Template" or "PM Type" or "PM Interval Type" => "maintenance",
            "Work Order Type" or "Work Order Priority" or "Maintenance Type" or "Repair Type" or "Labor Category" or "Downtime Reason" or "Shop Status" or "Bay" or "Approval Status" or "Completion Status" or "Return To Service Status" => "maintenance",
            "Inspection Template Category" or "Inspection Template Owner Role" or "Inspection Execution Mode" or "Inspection Result Mode" or "Inspection Readiness Impact" or "Inspection Template" or "Inspection Type" or "Required Inspection Types" or "Inspection Frequency Type" or "Seasonal PM Group" or "Regulatory Inspection Required" or "Annual Inspection Required" or "DVIR Required" or "Pre Trip Required" or "Post Trip Required" or "Shop Inspection Required" => "inspection",
            "Defect Type" or "Defect Category" or "Defect System" or "Defect Component" or "Report Source" or "Symptom" or "Failure Mode" or "Severity" or "Priority" or "Safety Critical" or "Operating Restriction" or "Operating Condition" or "Side Position" or "Deferral Code" or "Repair Disposition" or "Root Cause" or "Corrective Action Type" => "defect",
            "Document Type" or "Document Status" or "Verification Status" or "Expiration Type" => "document",
            "Fluid Spec" or "Oil Spec" or "Coolant Spec" or "DEF Spec" or "Filter Spec" or "Belt Spec" or "Battery Spec" or "Lamp Spec" or "Brake Part Spec" or "Tire Spec" or "Wheel Spec" => "spec",
            "Telematics Provider" or "Diagnostic Protocol" or "ELD Provider" or "GPS Provider" or "Fault Code Source" or "Fault Code Standard" or "Data Sync Status" => "telemetry",
            _ => "asset"
        };

    private static async Task<ReferenceDataset> EnsureMasterCsvIntakeDatasetAsync(
        NexArrDbContext db,
        CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var dataset = await db.ReferenceDatasets
            .FirstOrDefaultAsync(x => x.Key == MasterCsvIntakeDatasetKey, cancellationToken);
        if (dataset is not null)
        {
            return dataset;
        }

        dataset = new ReferenceDataset
        {
            Id = Guid.NewGuid(),
            Key = MasterCsvIntakeDatasetKey,
            Name = "Master Reference Intake",
            Category = "platform",
            OwnerService = "NexArr",
            Status = ReferenceDatasetStatuses.Ready,
            CreatedAt = now,
            UpdatedAt = now,
        };

        db.ReferenceDatasets.Add(dataset);
        await db.SaveChangesAsync(cancellationToken);
        return dataset;
    }

    private static async Task<ReferenceSource> EnsureMasterCsvSeedSourceAsync(
        NexArrDbContext db,
        CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var source = await db.ReferenceSources
            .FirstOrDefaultAsync(x => x.Key == MasterCsvSourceKey, cancellationToken);
        if (source is not null)
        {
            return source;
        }

        source = new ReferenceSource
        {
            Id = Guid.NewGuid(),
            Key = MasterCsvSourceKey,
            Name = "Master CSV upload",
            SourceType = "manual",
            ConnectorType = "csv_upload",
            AuthorityRank = 1000,
            RefreshCadence = "on_demand",
            TermsNotes = "Platform admin master CSV intake source.",
            Enabled = true,
            CreatedAt = now,
            UpdatedAt = now,
        };

        db.ReferenceSources.Add(source);
        await db.SaveChangesAsync(cancellationToken);
        return source;
    }

    private static async Task<IReadOnlyList<ParsedReferenceSeedRow>> ParseReferenceSeedRowsAsync(
        NexArrDbContext db,
        string csvText,
        CancellationToken cancellationToken)
    {
        var datasetIndex = (await db.ReferenceDatasets
            .AsNoTracking()
            .ToListAsync(cancellationToken))
            .ToDictionary(x => x.Key, StringComparer.OrdinalIgnoreCase);

        var rows = ParseCsvRows(csvText);
        if (rows.Count == 0)
        {
            return [];
        }

        var headers = rows[0].Select(header => header.Trim()).ToList();
        if (headers.Count == 0)
        {
            return [];
        }

        var parsedRows = new List<ParsedReferenceSeedRow>(rows.Count - 1);
        for (var index = 1; index < rows.Count; index++)
        {
            var row = rows[index];
            var rowNumber = index + 1;
            var columnMap = BuildColumnMap(headers, row);
            if (columnMap.Count == 0)
            {
                continue;
            }

            var targetDataset = ResolveTargetDatasetFromColumns(columnMap, datasetIndex);
            var product = ReadColumn(columnMap, "product", "product_key", "owner_service", "ownerService", "productCode");
            var datasetName = ReadColumn(columnMap, "dataset", "dataset_name", "datasetName", "dataset_label");
            var entityType = ReadColumn(columnMap, "entity_type", "entityType", "record_type", "type") ?? targetDataset?.Category ?? "reference";
            var canonicalKey = ReadColumn(columnMap, "canonical_key", "canonicalKey", "key");
            var displayName = ReadColumn(columnMap, "display_name", "displayName", "name");
            var sourceSystem = ReadColumn(columnMap, "source_system", "sourceSystem", "source");
            var sourceKey = ReadColumn(columnMap, "source_key", "sourceKey");
            var confidence = ReadConfidence(ReadColumn(columnMap, "confidence", "score"), targetDataset is not null);

            var normalizedCanonicalKey =
                !string.IsNullOrWhiteSpace(canonicalKey) ? NormalizeKey(canonicalKey) :
                !string.IsNullOrWhiteSpace(displayName) ? NormalizeKey(displayName) :
                !string.IsNullOrWhiteSpace(datasetName) ? NormalizeKey(datasetName) :
                null;

            var rawPayloadJson = Serialize(new
            {
                rowNumber,
                columns = columnMap,
            });

            var normalizedPayloadJson = Serialize(new
            {
                rowNumber,
                product,
                dataset = datasetName,
                datasetKey = targetDataset?.Key,
                targetDatasetId = targetDataset?.Id,
                targetDatasetName = targetDataset?.Name,
                targetOwnerService = targetDataset?.OwnerService,
                entityType = NormalizeKey(entityType),
                canonicalKey = normalizedCanonicalKey,
                displayName,
                sourceSystem,
                sourceKey,
                confidence,
                data = BuildDataPayload(columnMap),
            });

            parsedRows.Add(new ParsedReferenceSeedRow(
                rowNumber,
                targetDataset?.Id,
                rawPayloadJson,
                normalizedPayloadJson,
                NormalizeKey(entityType),
                normalizedCanonicalKey,
                confidence,
                targetDataset is null ? "Assign a target dataset before approving this row." : "Review and approve before upsert."));
        }

        return parsedRows;
    }

    private static ReferenceDataset? ResolveTargetDatasetFromColumns(
        IReadOnlyDictionary<string, string> columns,
        IReadOnlyDictionary<string, ReferenceDataset> datasets)
    {
        var datasetKey = ReadColumn(columns, "dataset_key", "datasetKey", "target_dataset_key", "targetDatasetKey");
        if (!string.IsNullOrWhiteSpace(datasetKey) && datasets.TryGetValue(NormalizeKey(datasetKey), out var directMatch))
        {
            return directMatch;
        }

        var product = ReadColumn(columns, "product", "product_key", "owner_service", "ownerService", "productCode");
        var datasetName = ReadColumn(columns, "dataset", "dataset_name", "datasetName", "dataset_label");
        if (!string.IsNullOrWhiteSpace(product) && !string.IsNullOrWhiteSpace(datasetName))
        {
            var combinedKey = $"{NormalizeKey(product)}-{NormalizeKey(datasetName)}";
            if (datasets.TryGetValue(combinedKey, out var combinedMatch))
            {
                return combinedMatch;
            }
        }

        if (!string.IsNullOrWhiteSpace(datasetName))
        {
            var normalizedName = NormalizeKey(datasetName);
            var matches = datasets.Values
                .Where(x => string.Equals(NormalizeKey(x.Name), normalizedName, StringComparison.OrdinalIgnoreCase))
                .ToList();
            if (matches.Count == 1)
            {
                return matches[0];
            }
        }

        return null;
    }

    private static ReferenceEntity CreateOrUpdateEntityFromSeed(
        ReferenceEntity? existing,
        Guid targetDatasetId,
        string entityType,
        string canonicalKey,
        string displayName,
        string normalizedFieldsJson,
        DateTimeOffset now)
    {
        if (existing is null)
        {
            return new ReferenceEntity
            {
                Id = Guid.NewGuid(),
                DatasetId = targetDatasetId,
                EntityType = entityType,
                CanonicalKey = canonicalKey,
                DisplayName = displayName,
                Status = ReferenceEntityStatuses.Active,
                NormalizedFieldsJson = normalizedFieldsJson,
                FirstSeenSourceId = null,
                CreatedAt = now,
                UpdatedAt = now,
            };
        }

        existing.EntityType = entityType;
        existing.DisplayName = displayName;
        existing.NormalizedFieldsJson = normalizedFieldsJson;
        existing.Status = ReferenceEntityStatuses.Active;
        existing.UpdatedAt = now;
        return existing;
    }

    private static async Task<ReferenceEntity> UpsertReferenceEntityFromSeedAsync(
        NexArrDbContext db,
        StagingRecord staged,
        ReferenceDataset targetDataset,
        string sourceKey,
        Guid sourceId,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var normalized = DeserializeObject(staged.NormalizedPayloadJson);
        var canonicalKey = NormalizeKey(staged.ProposedCanonicalKey ?? DetermineFallbackCanonicalKey(normalized, targetDataset));
        var displayName = DetermineDisplayName(normalized, canonicalKey);
        var normalizedFieldsJson = NormalizeJson(staged.NormalizedPayloadJson);

        ReferenceEntity entity;
        var existingEntity = await db.ReferenceEntities
            .FirstOrDefaultAsync(
                x => x.DatasetId == targetDataset.Id && x.CanonicalKey == canonicalKey,
                cancellationToken);
        if (existingEntity is not null)
        {
            entity = CreateOrUpdateEntityFromSeed(
                existingEntity,
                targetDataset.Id,
                NormalizeKey(staged.ProposedEntityType),
                canonicalKey,
                displayName,
                normalizedFieldsJson,
                now);
        }
        else
        {
            entity = CreateOrUpdateEntityFromSeed(
                null,
                targetDataset.Id,
                NormalizeKey(staged.ProposedEntityType),
                canonicalKey,
                displayName,
                normalizedFieldsJson,
                now);
            entity.FirstSeenSourceId = sourceId;
            db.ReferenceEntities.Add(entity);
        }

        await db.SaveChangesAsync(cancellationToken);
        await UpsertReferenceEntityVersionFromSeedAsync(
            db,
            entity,
            normalizedFieldsJson,
            staged.RawPayloadJson,
            now,
            cancellationToken);

        if (staged.Confidence >= 0.75m)
        {
            var externalKey = ResolveCrosswalkExternalKey(normalized, canonicalKey);
            await UpsertReferenceCrosswalkAsync(
                db,
                entity.Id,
                sourceKey,
                externalKey,
                sourceId,
                staged.Confidence,
                now,
                cancellationToken);
        }

        return entity;
    }

    private static async Task UpsertReferenceEntityVersionFromSeedAsync(
        NexArrDbContext db,
        ReferenceEntity entity,
        string normalizedFieldsJson,
        string sourceEvidenceJson,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        // Keep seed-import behavior consistent with approval flow.
        var currentVersion = await db
            .ReferenceEntityVersions
            .Where(x => x.ReferenceEntityId == entity.Id)
            .OrderByDescending(x => x.Version)
            .FirstOrDefaultAsync(cancellationToken);
        var versionNumber = (currentVersion?.Version ?? 0) + 1;

        var version = new ReferenceEntityVersion
        {
            Id = Guid.NewGuid(),
            ReferenceEntityId = entity.Id,
            Version = versionNumber,
            FieldsJson = normalizedFieldsJson,
            SourceEvidenceJson = NormalizeJson(sourceEvidenceJson),
            PublishedAt = now,
            CreatedAt = now,
            UpdatedAt = now,
        };
        db.ReferenceEntityVersions.Add(version);
        entity.CurrentVersionId = version.Id;
        await db.SaveChangesAsync(cancellationToken);
    }

    private static string NormalizeJson(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return "{}";
        }

        try
        {
            using var document = JsonDocument.Parse(json);
            return JsonSerializer.Serialize(document.RootElement);
        }
        catch
        {
            return JsonSerializer.Serialize(new { value = json.Trim() });
        }
    }

    private static JsonElement DeserializeObject(string? json)
    {
        using var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(json) ? "{}" : json);
        return document.RootElement.Clone();
    }

    private static string DetermineFallbackCanonicalKey(JsonElement normalized, ReferenceDataset targetDataset)
    {
        if (normalized.ValueKind == JsonValueKind.Object)
        {
            foreach (var key in new[] { "canonicalKey", "displayName", "name", "productName", "entityType" })
            {
                if (normalized.TryGetProperty(key, out var value) && value.ValueKind == JsonValueKind.String)
                {
                    var text = value.GetString();
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        return NormalizeKey(text);
                    }
                }
            }
        }

        return NormalizeKey(targetDataset.Name);
    }

    private static string DetermineDisplayName(JsonElement normalized, string fallback)
    {
        foreach (var key in new[] { "displayName", "name", "productName", "manufacturer", "make", "model" })
        {
            if (normalized.ValueKind == JsonValueKind.Object
                && normalized.TryGetProperty(key, out var value)
                && value.ValueKind == JsonValueKind.String)
            {
                var text = value.GetString();
                if (!string.IsNullOrWhiteSpace(text))
                {
                    return text.Trim();
                }
            }
        }

        return fallback;
    }

    private static string ResolveCrosswalkExternalKey(JsonElement normalized, string fallbackCanonicalKey)
    {
        if (normalized.ValueKind == JsonValueKind.Object
            && normalized.TryGetProperty("sourceKey", out var sourceKeyValue)
            && sourceKeyValue.ValueKind == JsonValueKind.String)
        {
            var sourceKey = sourceKeyValue.GetString();
            if (!string.IsNullOrWhiteSpace(sourceKey))
            {
                return NormalizeKey(sourceKey);
            }
        }

        return fallbackCanonicalKey;
    }

    private static async Task UpsertReferenceCrosswalkAsync(
        NexArrDbContext db,
        Guid referenceEntityId,
        string externalSystem,
        string externalKey,
        Guid sourceId,
        decimal confidence,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        if (string.Equals(db.Database.ProviderName, "Npgsql.EntityFrameworkCore.PostgreSQL", StringComparison.Ordinal))
        {
            await db.Database.ExecuteSqlInterpolatedAsync($"""
                INSERT INTO reference_crosswalks ("Id", "Confidence", "CreatedAt", "ExternalKey", "ExternalSystem", "ReferenceEntityId", "SourceId", "Status", "UpdatedAt")
                VALUES ({Guid.NewGuid()}, {confidence}, {now}, {externalKey}, {externalSystem}, {referenceEntityId}, {sourceId}, {ReferenceCrosswalkStatuses.Active}, {now})
                ON CONFLICT ("ExternalSystem", "ExternalKey")
                DO UPDATE SET
                    "ReferenceEntityId" = EXCLUDED."ReferenceEntityId",
                    "SourceId" = EXCLUDED."SourceId",
                    "Confidence" = EXCLUDED."Confidence",
                    "Status" = EXCLUDED."Status",
                    "UpdatedAt" = EXCLUDED."UpdatedAt";
                """, cancellationToken);
            return;
        }

        var crosswalk = await db.ReferenceCrosswalks.FirstOrDefaultAsync(
            x => x.ExternalSystem == externalSystem && x.ExternalKey == externalKey,
            cancellationToken)
            ?? await db.ReferenceCrosswalks.FirstOrDefaultAsync(
                x => x.ReferenceEntityId == referenceEntityId && x.ExternalSystem == externalSystem,
                cancellationToken);

        if (crosswalk is null)
        {
            crosswalk = new ReferenceCrosswalk
            {
                Id = Guid.NewGuid(),
                CreatedAt = now,
            };
            db.ReferenceCrosswalks.Add(crosswalk);
        }

        crosswalk.ReferenceEntityId = referenceEntityId;
        crosswalk.ExternalSystem = externalSystem;
        crosswalk.ExternalKey = externalKey;
        crosswalk.SourceId = sourceId;
        crosswalk.Confidence = confidence;
        crosswalk.Status = ReferenceCrosswalkStatuses.Active;
        crosswalk.UpdatedAt = now;
        await db.SaveChangesAsync(cancellationToken);
    }

    private static decimal ReadConfidence(string? confidence, bool targetAssigned)
    {
        if (decimal.TryParse(confidence, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsed))
        {
            return ClampConfidence(parsed);
        }

        return targetAssigned ? 0.9m : 0.5m;
    }

    private static IReadOnlyDictionary<string, string> BuildColumnMap(IReadOnlyList<string> headers, IReadOnlyList<string> row)
    {
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        for (var index = 0; index < headers.Count; index++)
        {
            var header = NormalizeColumnKey(headers[index]);
            if (string.IsNullOrWhiteSpace(header))
            {
                continue;
            }

            var value = index < row.Count ? row[index].Trim() : string.Empty;
            if (string.IsNullOrWhiteSpace(value))
            {
                continue;
            }

            map[header] = value;
        }

        return map;
    }

    private static IReadOnlyDictionary<string, object?> BuildDataPayload(IReadOnlyDictionary<string, string> columns)
    {
        var payload = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        foreach (var (key, value) in columns)
        {
            if (IsRoutingColumn(key))
            {
                continue;
            }

            payload[key] = value;
        }

        return payload;
    }

    private static bool IsRoutingColumn(string columnName) =>
        string.Equals(columnName, NormalizeColumnKey("product"), StringComparison.OrdinalIgnoreCase)
        || string.Equals(columnName, NormalizeColumnKey("product_key"), StringComparison.OrdinalIgnoreCase)
        || string.Equals(columnName, NormalizeColumnKey("owner_service"), StringComparison.OrdinalIgnoreCase)
        || string.Equals(columnName, NormalizeColumnKey("product_code"), StringComparison.OrdinalIgnoreCase)
        || string.Equals(columnName, NormalizeColumnKey("dataset"), StringComparison.OrdinalIgnoreCase)
        || string.Equals(columnName, NormalizeColumnKey("dataset_name"), StringComparison.OrdinalIgnoreCase)
        || string.Equals(columnName, NormalizeColumnKey("dataset_label"), StringComparison.OrdinalIgnoreCase)
        || string.Equals(columnName, NormalizeColumnKey("dataset_key"), StringComparison.OrdinalIgnoreCase)
        || string.Equals(columnName, NormalizeColumnKey("target_dataset_key"), StringComparison.OrdinalIgnoreCase)
        || string.Equals(columnName, NormalizeColumnKey("entity_type"), StringComparison.OrdinalIgnoreCase)
        || string.Equals(columnName, NormalizeColumnKey("record_type"), StringComparison.OrdinalIgnoreCase)
        || string.Equals(columnName, NormalizeColumnKey("type"), StringComparison.OrdinalIgnoreCase)
        || string.Equals(columnName, NormalizeColumnKey("canonical_key"), StringComparison.OrdinalIgnoreCase)
        || string.Equals(columnName, NormalizeColumnKey("display_name"), StringComparison.OrdinalIgnoreCase)
        || string.Equals(columnName, NormalizeColumnKey("name"), StringComparison.OrdinalIgnoreCase)
        || string.Equals(columnName, NormalizeColumnKey("source_system"), StringComparison.OrdinalIgnoreCase)
        || string.Equals(columnName, NormalizeColumnKey("source"), StringComparison.OrdinalIgnoreCase)
        || string.Equals(columnName, NormalizeColumnKey("source_key"), StringComparison.OrdinalIgnoreCase)
        || string.Equals(columnName, NormalizeColumnKey("confidence"), StringComparison.OrdinalIgnoreCase)
        || string.Equals(columnName, NormalizeColumnKey("score"), StringComparison.OrdinalIgnoreCase)
        || string.Equals(columnName, NormalizeColumnKey("fields_json"), StringComparison.OrdinalIgnoreCase)
        || string.Equals(columnName, NormalizeColumnKey("normalized_fields_json"), StringComparison.OrdinalIgnoreCase)
        || string.Equals(columnName, NormalizeColumnKey("source_evidence_json"), StringComparison.OrdinalIgnoreCase);

    private static string? ReadColumn(IReadOnlyDictionary<string, string> columns, params string[] aliases)
    {
        foreach (var alias in aliases)
        {
            var normalizedAlias = NormalizeColumnKey(alias);
            if (columns.TryGetValue(normalizedAlias, out var value) && !string.IsNullOrWhiteSpace(value))
            {
                return value.Trim();
            }
        }

        return null;
    }

    private static IReadOnlyList<IReadOnlyList<string>> ParseCsvRows(string csv)
    {
        if (string.IsNullOrWhiteSpace(csv))
        {
            return [];
        }

        return csv
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace('\r', '\n')
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(ParseCsvRow)
            .ToList();
    }

    private static IReadOnlyList<string> ParseCsvRow(string line)
    {
        var fields = new List<string>();
        var current = new StringBuilder();
        var inQuotes = false;

        for (var index = 0; index < line.Length; index++)
        {
            var character = line[index];
            if (inQuotes)
            {
                if (character == '"')
                {
                    if (index + 1 < line.Length && line[index + 1] == '"')
                    {
                        current.Append('"');
                        index++;
                    }
                    else
                    {
                        inQuotes = false;
                    }
                }
                else
                {
                    current.Append(character);
                }

                continue;
            }

            if (character == '"')
            {
                inQuotes = true;
                continue;
            }

            if (character == ',')
            {
                fields.Add(current.ToString().Trim());
                current.Clear();
                continue;
            }

            current.Append(character);
        }

        fields.Add(current.ToString().Trim());
        return fields;
    }

    private static string NormalizeColumnKey(string value) =>
        new string(value.Where(char.IsLetterOrDigit).ToArray()).ToLowerInvariant();

    private static decimal ClampConfidence(decimal confidence) =>
        confidence < 0 ? 0 : confidence > 1 ? 1 : confidence;

    private static string ComputeSha256Hex(string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static string Serialize(object value) =>
        JsonSerializer.Serialize(value, new JsonSerializerOptions(JsonSerializerDefaults.Web));

    private static string NormalizeKey(string value) =>
        value.Trim().ToLowerInvariant().Replace(' ', '-');

    private sealed record ProductSeed(
        string Key,
        string Name,
        int Order,
        string Category,
        string Owner,
        string Status,
        string ApiBaseUrl,
        string HealthUrl,
        string ServiceAudience);

    private sealed record ReferenceDatasetSeed(
        string Key,
        string Name,
        string Category,
        string OwnerService);

    private sealed record ParsedReferenceSeedRow(
        int RowNumber,
        Guid? TargetDatasetId,
        string RawPayloadJson,
        string NormalizedPayloadJson,
        string EntityType,
        string? CanonicalKey,
        decimal Confidence,
        string ReviewReason);

    private sealed record FirstAdminSeedProfile(
        string Email,
        string Password,
        string DisplayName,
        bool UseDeterministicId);
}
