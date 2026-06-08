using System.Text.Json;

using Microsoft.EntityFrameworkCore;

using MaintainArr.Api.Contracts;

using MaintainArr.Api.Data;

using MaintainArr.Api.Entities;

using STLCompliance.Shared.Contracts;



namespace MaintainArr.Api.Services;



public sealed class InspectionTemplateService(

    MaintainArrDbContext db,

    AssetTypeService assetTypeService,

    IMaintainArrAuditService audit)

{

    private static readonly HashSet<string> AllowedTemplateStatuses = new(StringComparer.OrdinalIgnoreCase)

    {

        InspectionTemplateStatuses.Draft,

        InspectionTemplateStatuses.Active,

        InspectionTemplateStatuses.Retired,

        InspectionTemplateStatuses.Archived,

        InspectionTemplateStatuses.Inactive

    };



    private static readonly HashSet<string> AllowedItemTypes = new(StringComparer.OrdinalIgnoreCase)

    {

        InspectionChecklistItemTypes.PassFail,

        InspectionChecklistItemTypes.PassFailNa,

        InspectionChecklistItemTypes.YesNo,

        InspectionChecklistItemTypes.YesNoNa,

        InspectionChecklistItemTypes.Numeric,

        InspectionChecklistItemTypes.Text,

        InspectionChecklistItemTypes.DateTime,

        InspectionChecklistItemTypes.Select,

        InspectionChecklistItemTypes.MultiSelect,

        InspectionChecklistItemTypes.Photo,

        InspectionChecklistItemTypes.Signature,

        InspectionChecklistItemTypes.MeterReading,

        InspectionChecklistItemTypes.OdometerMileage,

        InspectionChecklistItemTypes.EngineHours,

        InspectionChecklistItemTypes.ChecklistAcknowledgment,

        InspectionChecklistItemTypes.BarcodeQrScan,

        InspectionChecklistItemTypes.VinSerialVerification

    };

    private static readonly HashSet<string> AllowedInspectionTypes = new(InspectionTemplateInspectionTypes.All, StringComparer.OrdinalIgnoreCase);



    public async Task<IReadOnlyList<InspectionTemplateSummaryResponse>> ListAsync(

        Guid tenantId,

        CancellationToken cancellationToken = default)

    {

        var templates = await db.InspectionTemplates

            .AsNoTracking()

            .Where(x => x.TenantId == tenantId)

            .OrderBy(x => x.Name)

            .ThenBy(x => x.TemplateKey)

            .ToListAsync(cancellationToken);



        if (templates.Count == 0)

        {

            return [];

        }



        var templateIds = templates.Select(x => x.Id).ToList();

        var categoryCounts = await db.InspectionTemplateCategories

            .AsNoTracking()

            .Where(x => x.TenantId == tenantId && templateIds.Contains(x.InspectionTemplateId))

            .GroupBy(x => x.InspectionTemplateId)

            .Select(x => new { x.Key, Count = x.Count() })

            .ToDictionaryAsync(x => x.Key, x => x.Count, cancellationToken);



        var itemCounts = await db.InspectionChecklistItems

            .AsNoTracking()

            .Where(x => x.TenantId == tenantId && templateIds.Contains(x.InspectionTemplateId))

            .GroupBy(x => x.InspectionTemplateId)

            .Select(x => new { x.Key, Count = x.Count() })

            .ToDictionaryAsync(x => x.Key, x => x.Count, cancellationToken);



        var linkCounts = await db.InspectionTemplateAssetTypes

            .AsNoTracking()

            .Where(x => x.TenantId == tenantId && templateIds.Contains(x.InspectionTemplateId))

            .GroupBy(x => x.InspectionTemplateId)

            .Select(x => new { x.Key, Count = x.Count() })

            .ToDictionaryAsync(x => x.Key, x => x.Count, cancellationToken);



        return templates

            .Select(template => new InspectionTemplateSummaryResponse(

                template.Id,

                template.TemplateKey,

                template.Name,

                template.Description,

                template.TemplateCategoryKey ?? string.Empty,

                template.OwningSiteRef ?? string.Empty,

                template.OwningTeamRef ?? string.Empty,

                template.OwnerPersonId ?? string.Empty,

                template.InspectionType,

                template.Version,

                template.Status,

                categoryCounts.GetValueOrDefault(template.Id),

                itemCounts.GetValueOrDefault(template.Id),

                linkCounts.GetValueOrDefault(template.Id),

                template.CreatedAt,

                template.UpdatedAt,

                template.PublishedAt,

                template.RetiredAt))

            .ToList();

    }



    public async Task<InspectionTemplateDetailResponse> GetAsync(

        Guid tenantId,

        Guid inspectionTemplateId,

        CancellationToken cancellationToken = default)

    {

        var template = await db.InspectionTemplates

            .AsNoTracking()

            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == inspectionTemplateId, cancellationToken);

        if (template is null)

        {

            throw new StlApiException("inspection_template.not_found", "Inspection template was not found.", 404);

        }



        var categoryEntities = await db.InspectionTemplateCategories

            .AsNoTracking()

            .Where(x => x.TenantId == tenantId && x.InspectionTemplateId == inspectionTemplateId)

            .OrderBy(x => x.SortOrder)

            .ThenBy(x => x.Name)

            .ToListAsync(cancellationToken);

        var categories = categoryEntities
            .Select(MapCategory)
            .ToList();



        var categoryKeys = categories.ToDictionary(x => x.CategoryId, x => x.CategoryKey);



        var checklistItemEntities = await db.InspectionChecklistItems

            .AsNoTracking()

            .Where(x => x.TenantId == tenantId && x.InspectionTemplateId == inspectionTemplateId)

            .OrderBy(x => x.SortOrder)

            .ThenBy(x => x.ItemKey)

            .ToListAsync(cancellationToken);

        var checklistItems = checklistItemEntities

            .Select(x => MapChecklistItem(

                x,

                x.CategoryId.HasValue && categoryKeys.TryGetValue(x.CategoryId.Value, out var key) ? key : null))

            .ToList();



        var linkedAssetTypes = await db.InspectionTemplateAssetTypes

            .AsNoTracking()

            .Where(x => x.TenantId == tenantId && x.InspectionTemplateId == inspectionTemplateId)

            .Join(

                db.AssetTypes.AsNoTracking().Where(t => t.TenantId == tenantId),

                link => link.AssetTypeId,

                assetType => assetType.Id,

                (link, assetType) => new { link, assetType })

            .Join(

                db.AssetClasses.AsNoTracking().Where(c => c.TenantId == tenantId),

                row => row.assetType.AssetClassId,

                assetClass => assetClass.Id,

                (row, assetClass) => new InspectionTemplateAssetTypeLinkResponse(

                    row.assetType.Id,

                    row.assetType.TypeKey,

                    row.assetType.Name,

                    assetClass.ClassKey,

                    assetClass.Name))

            .OrderBy(x => x.ClassName)

            .ThenBy(x => x.TypeName)

            .ToListAsync(cancellationToken);



        return new InspectionTemplateDetailResponse(

            template.Id,

            template.TemplateKey,

            template.Name,

            template.Description,

            template.TemplateCategoryKey ?? string.Empty,

            template.OwningSiteRef,

            template.OwningTeamRef,

            template.OwnerPersonId,

            template.OwnerRoleKey,

            template.EstimatedDurationMinutes,

            DeserializeStringList(template.TagsJson),

            DeserializeObjectDict(template.SettingsJson),

            template.InspectionType,

            template.Version,

            template.Status,

            categories,

            checklistItems,

            linkedAssetTypes,

            template.CreatedAt,

            template.UpdatedAt,

            template.PublishedAt,

            template.RetiredAt,

            template.CreatedByPersonId,

            template.UpdatedByPersonId,

            template.PublishedByPersonId,

            template.RetiredByPersonId);

    }



    public async Task<InspectionTemplateDetailResponse> CreateAsync(

        Guid tenantId,

        Guid actorUserId,

        string actorPersonId,

        CreateInspectionTemplateRequest request,

        CancellationToken cancellationToken = default)

    {

        var templateKey = NormalizeTemplateKey(request.TemplateKey);

        var exists = await db.InspectionTemplates.AnyAsync(

            x => x.TenantId == tenantId && x.TemplateKey == templateKey,

            cancellationToken);

        if (exists)

        {

            throw new StlApiException(

                "inspection_template.duplicate_key",

                "An inspection template with this key already exists.",

                409);

        }



        var now = DateTimeOffset.UtcNow;

        var entity = new InspectionTemplate

        {

            Id = Guid.NewGuid(),

            TenantId = tenantId,

            TemplateKey = templateKey,

            Name = NormalizeName(request.Name),

            Description = NormalizeDescription(request.Description),

            TemplateCategoryKey = NormalizeOptionalKey(request.TemplateCategoryKey),

            OwningSiteRef = NormalizeOptionalReference(request.OwningSiteRef),

            OwningTeamRef = NormalizeOptionalReference(request.OwningTeamRef),

            OwnerPersonId = NormalizeOptionalReference(request.OwnerPersonId),

            OwnerRoleKey = NormalizeOptionalReference(request.OwnerRoleKey),

            EstimatedDurationMinutes = NormalizeOptionalDurationMinutes(request.EstimatedDurationMinutes),

            TagsJson = SerializeStringList(request.Tags),

            SettingsJson = SerializeSettings(request.Settings),

            CreatedByPersonId = actorPersonId,

            UpdatedByPersonId = actorPersonId,

            InspectionType = NormalizeInspectionType(request.InspectionType),

            Version = 1,

            Status = InspectionTemplateStatuses.Draft,

            CreatedAt = now,

            UpdatedAt = now

        };



        db.InspectionTemplates.Add(entity);

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(

            "inspection_template.create",

            tenantId,

            actorUserId,
            actorPersonId,

            "inspection_template",

            entity.Id.ToString(),

            "Succeeded",

            cancellationToken: cancellationToken);



        return await GetAsync(tenantId, entity.Id, cancellationToken);

    }



    public async Task<InspectionTemplateDetailResponse> UpdateAsync(

        Guid tenantId,

        Guid actorUserId,

        string actorPersonId,

        Guid inspectionTemplateId,

        UpdateInspectionTemplateRequest request,

        CancellationToken cancellationToken = default)

    {

        var entity = await GetTemplateForWriteAsync(tenantId, inspectionTemplateId, cancellationToken);

        entity.Name = NormalizeName(request.Name);

        entity.Description = NormalizeDescription(request.Description);

        if (request.TemplateCategoryKey is not null)
        {
            entity.TemplateCategoryKey = NormalizeOptionalKey(request.TemplateCategoryKey);
        }

        if (request.OwningSiteRef is not null)
        {
            entity.OwningSiteRef = NormalizeOptionalReference(request.OwningSiteRef);
        }

        if (request.OwningTeamRef is not null)
        {
            entity.OwningTeamRef = NormalizeOptionalReference(request.OwningTeamRef);
        }

        if (request.OwnerPersonId is not null)
        {
            entity.OwnerPersonId = NormalizeOptionalReference(request.OwnerPersonId);
        }

        if (request.OwnerRoleKey is not null)
        {
            entity.OwnerRoleKey = NormalizeOptionalReference(request.OwnerRoleKey);
        }

        if (request.EstimatedDurationMinutes.HasValue)
        {
            entity.EstimatedDurationMinutes = NormalizeOptionalDurationMinutes(request.EstimatedDurationMinutes);
        }

        if (!string.IsNullOrWhiteSpace(request.InspectionType))
        {
            entity.InspectionType = NormalizeInspectionType(request.InspectionType);
        }

        if (request.Tags is not null)
        {
            entity.TagsJson = SerializeStringList(request.Tags);
        }

        if (request.Settings is not null)
        {
            entity.SettingsJson = SerializeSettings(request.Settings);
        }

        entity.UpdatedAt = DateTimeOffset.UtcNow;
        entity.UpdatedByPersonId = actorPersonId;



        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(

            "inspection_template.update",

            tenantId,

            actorUserId,
            actorPersonId,

            "inspection_template",

            entity.Id.ToString(),

            "Succeeded",

            cancellationToken: cancellationToken);



        return await GetAsync(tenantId, entity.Id, cancellationToken);

    }



    public async Task<InspectionTemplateDetailResponse> UpdateStatusAsync(

        Guid tenantId,

        Guid actorUserId,

        string actorPersonId,

        Guid inspectionTemplateId,

        UpdateInspectionTemplateStatusRequest request,

        string? reasonCode = null,

        CancellationToken cancellationToken = default)

    {

        var status = NormalizeTemplateStatus(request.Status);

        if (!AllowedTemplateStatuses.Contains(status))

        {

            throw new StlApiException(

                "inspection_template.invalid_status",

                "Template status must be draft, active, retired, or archived.",

                400);

        }



        var entity = await GetTemplateForWriteAsync(tenantId, inspectionTemplateId, cancellationToken);

        if (status == InspectionTemplateStatuses.Active)

        {

            var hasItems = await db.InspectionChecklistItems.AnyAsync(

                x => x.TenantId == tenantId && x.InspectionTemplateId == inspectionTemplateId,

                cancellationToken);

            if (!hasItems)

            {

                throw new StlApiException(

                    "inspection_template.missing_checklist",

                    "Active templates require at least one checklist item.",

                    400);

            }

        }



        entity.Status = status;
        if (status == InspectionTemplateStatuses.Active)
        {
            entity.PublishedAt ??= DateTimeOffset.UtcNow;
            entity.PublishedByPersonId ??= actorPersonId;
            entity.RetiredAt = null;
            entity.RetiredByPersonId = null;
        }
        else if (status == InspectionTemplateStatuses.Retired || status == InspectionTemplateStatuses.Archived)
        {
            entity.RetiredAt ??= DateTimeOffset.UtcNow;
            entity.RetiredByPersonId ??= actorPersonId;
        }
        else
        {
            entity.PublishedAt = null;
            entity.PublishedByPersonId = null;
            entity.RetiredAt = null;
            entity.RetiredByPersonId = null;
        }

        entity.UpdatedAt = DateTimeOffset.UtcNow;
        entity.UpdatedByPersonId = actorPersonId;

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(

            "inspection_template.status.update",

            tenantId,

            actorUserId,

            actorPersonId,

            "inspection_template",

            entity.Id.ToString(),

            "Succeeded",

            reasonCode,

            cancellationToken: cancellationToken);



        return await GetAsync(tenantId, entity.Id, cancellationToken);

    }

    public async Task<InspectionTemplateValidationResponse> ValidateAsync(
        Guid tenantId,
        Guid inspectionTemplateId,
        CancellationToken cancellationToken = default)
    {
        var template = await GetAsync(tenantId, inspectionTemplateId, cancellationToken);
        var issues = await BuildValidationIssuesAsync(tenantId, template, cancellationToken);
        var preview = await BuildCompatibleAssetPreviewAsync(tenantId, template, cancellationToken);
        var blocking = issues.Any(x => x.IsBlocking);
        return new InspectionTemplateValidationResponse(
            !blocking,
            issues,
            template.Categories.Count,
            template.ChecklistItems.Count,
            preview.CompatibleCount);
    }

    public async Task<InspectionTemplatePreviewResponse> PreviewAsync(
        Guid tenantId,
        Guid inspectionTemplateId,
        CancellationToken cancellationToken = default)
    {
        var template = await GetAsync(tenantId, inspectionTemplateId, cancellationToken);
        var validation = await ValidateAsync(tenantId, inspectionTemplateId, cancellationToken);
        var assets = await BuildCompatibleAssetPreviewAsync(tenantId, template, cancellationToken);
        var summary = BuildPreviewSummary(template, validation, assets);
        return new InspectionTemplatePreviewResponse(template, validation, assets, summary);
    }

    public async Task<InspectionTemplateDetailResponse> PublishAsync(
        Guid tenantId,
        Guid actorUserId,
        string actorPersonId,
        Guid inspectionTemplateId,
        PublishInspectionTemplateRequest request,
        CancellationToken cancellationToken = default)
    {
        EnsurePublishConfirmations(request);
        return await UpdateStatusAsync(
            tenantId,
            actorUserId,
            actorPersonId,
            inspectionTemplateId,
            new UpdateInspectionTemplateStatusRequest(InspectionTemplateStatuses.Active),
            cancellationToken: cancellationToken);
    }

    public async Task<InspectionTemplateDetailResponse> RetireAsync(
        Guid tenantId,
        Guid actorUserId,
        string actorPersonId,
        Guid inspectionTemplateId,
        RetireInspectionTemplateRequest request,
        CancellationToken cancellationToken = default)
    {
        return await UpdateStatusAsync(
            tenantId,
            actorUserId,
            actorPersonId,
            inspectionTemplateId,
            new UpdateInspectionTemplateStatusRequest(InspectionTemplateStatuses.Retired),
            NormalizeReasonCode(request.Reason),
            cancellationToken);
    }

    public async Task<InspectionTemplateDetailResponse> CloneAsync(
        Guid tenantId,
        Guid actorUserId,
        string actorPersonId,
        Guid inspectionTemplateId,
        CancellationToken cancellationToken = default)
    {
        var source = await GetAsync(tenantId, inspectionTemplateId, cancellationToken);
        var now = DateTimeOffset.UtcNow;
        var cloneKey = await GenerateCloneTemplateKeyAsync(tenantId, source.TemplateKey, cancellationToken);

        var clone = new InspectionTemplate
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            TemplateKey = cloneKey,
            Name = $"{source.Name} Copy",
            Description = source.Description,
            TemplateCategoryKey = source.TemplateCategoryKey,
            OwningSiteRef = source.OwningSiteRef,
            OwningTeamRef = source.OwningTeamRef,
            OwnerPersonId = source.OwnerPersonId,
            OwnerRoleKey = source.OwnerRoleKey,
            EstimatedDurationMinutes = source.EstimatedDurationMinutes,
            TagsJson = SerializeStringList(source.Tags),
            SettingsJson = SerializeSettings(source.Settings),
            InspectionType = source.InspectionType,
            Version = 1,
            Status = InspectionTemplateStatuses.Draft,
            CreatedAt = now,
            UpdatedAt = now,
            CreatedByPersonId = actorPersonId,
            UpdatedByPersonId = actorPersonId
        };

        db.InspectionTemplates.Add(clone);
        await db.SaveChangesAsync(cancellationToken);

        var categoryMap = new Dictionary<Guid, Guid>();
        foreach (var category in source.Categories.OrderBy(x => x.SortOrder).ThenBy(x => x.Name, StringComparer.OrdinalIgnoreCase))
        {
            var clonedCategory = new InspectionTemplateCategory
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                InspectionTemplateId = clone.Id,
                CategoryKey = category.CategoryKey,
                Name = category.Name,
                Description = category.Description,
                IsRequired = category.IsRequired,
                CanBeSkipped = category.CanBeSkipped,
                SkipReasonRequired = category.SkipReasonRequired,
                TimingTracked = category.TimingTracked,
                SettingsJson = SerializeSettings(category.Settings),
                SortOrder = category.SortOrder,
                CreatedAt = now,
                UpdatedAt = now
            };
            categoryMap[category.CategoryId] = clonedCategory.Id;
            db.InspectionTemplateCategories.Add(clonedCategory);
        }

        foreach (var item in source.ChecklistItems.OrderBy(x => x.SortOrder).ThenBy(x => x.ItemKey, StringComparer.OrdinalIgnoreCase))
        {
            db.InspectionChecklistItems.Add(new InspectionChecklistItem
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                InspectionTemplateId = clone.Id,
                CategoryId = item.CategoryId.HasValue && categoryMap.TryGetValue(item.CategoryId.Value, out var mappedCategoryId)
                    ? mappedCategoryId
                    : null,
                ItemKey = item.ItemKey,
                Prompt = item.Prompt,
                HelpText = item.HelpText,
                ItemType = item.ItemType,
                SettingsJson = SerializeSettings(item.Settings),
                ControlledOptionsJson = SerializeStringList(item.ControlledOptions),
                AcceptableRangeMin = item.AcceptableRangeMin,
                AcceptableRangeMax = item.AcceptableRangeMax,
                UnitOfMeasure = item.UnitOfMeasure,
                IsRequired = item.IsRequired,
                SortOrder = item.SortOrder,
                CreatedAt = now,
                UpdatedAt = now
            });
        }

        foreach (var assetTypeLink in source.LinkedAssetTypes)
        {
            var assetTypeId = assetTypeLink.AssetTypeId;
            db.InspectionTemplateAssetTypes.Add(new InspectionTemplateAssetType
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                InspectionTemplateId = clone.Id,
                AssetTypeId = assetTypeId,
                CreatedAt = now
            });
        }

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "inspection_template.clone",
            tenantId,
            actorUserId,
            actorPersonId,
            "inspection_template",
            clone.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return await GetAsync(tenantId, clone.Id, cancellationToken);
    }



    public async Task<InspectionTemplateCategoryResponse> CreateCategoryAsync(

        Guid tenantId,

        Guid actorUserId,

        string actorPersonId,

        Guid inspectionTemplateId,

        CreateInspectionTemplateCategoryRequest request,

        CancellationToken cancellationToken = default)

    {

        _ = await GetTemplateForWriteAsync(tenantId, inspectionTemplateId, cancellationToken);

        var categoryKey = NormalizeCategoryKey(request.CategoryKey);

        var exists = await db.InspectionTemplateCategories.AnyAsync(

            x => x.TenantId == tenantId

                && x.InspectionTemplateId == inspectionTemplateId

                && x.CategoryKey == categoryKey,

            cancellationToken);

        if (exists)

        {

            throw new StlApiException(

                "inspection_template.category.duplicate_key",

                "A category with this key already exists on the template.",

                409);

        }



        var now = DateTimeOffset.UtcNow;

        var entity = new InspectionTemplateCategory

        {

            Id = Guid.NewGuid(),

            TenantId = tenantId,

            InspectionTemplateId = inspectionTemplateId,

            CategoryKey = categoryKey,

            Name = NormalizeCategoryName(request.Name),

            Description = NormalizeOptionalText(request.Description, 512),

            IsRequired = request.IsRequired,

            CanBeSkipped = request.CanBeSkipped,

            SkipReasonRequired = request.SkipReasonRequired,

            TimingTracked = request.TimingTracked,

            SettingsJson = SerializeSettings(request.Settings),

            SortOrder = NormalizeSortOrder(request.SortOrder),

            CreatedAt = now,

            UpdatedAt = now

        };



        db.InspectionTemplateCategories.Add(entity);

        await TouchTemplateAsync(tenantId, inspectionTemplateId, cancellationToken);

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(

            "inspection_template.category.create",

            tenantId,

            actorUserId,

            actorPersonId,

            "inspection_template_category",

            entity.Id.ToString(),

            "Succeeded",

            cancellationToken: cancellationToken);



        return MapCategory(entity);

    }



    public async Task<InspectionTemplateCategoryResponse> UpdateCategoryAsync(

        Guid tenantId,

        Guid actorUserId,

        string actorPersonId,

        Guid inspectionTemplateId,

        Guid categoryId,

        UpdateInspectionTemplateCategoryRequest request,

        CancellationToken cancellationToken = default)

    {

        var entity = await GetCategoryForWriteAsync(tenantId, inspectionTemplateId, categoryId, cancellationToken);

        entity.Name = NormalizeCategoryName(request.Name);

        entity.Description = NormalizeOptionalText(request.Description, 512);

        entity.IsRequired = request.IsRequired;

        entity.CanBeSkipped = request.CanBeSkipped;

        entity.SkipReasonRequired = request.SkipReasonRequired;

        entity.TimingTracked = request.TimingTracked;

        entity.SettingsJson = SerializeSettings(request.Settings);

        entity.SortOrder = NormalizeSortOrder(request.SortOrder);

        entity.UpdatedAt = DateTimeOffset.UtcNow;



        await TouchTemplateAsync(tenantId, inspectionTemplateId, cancellationToken);

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(

            "inspection_template.category.update",

            tenantId,

            actorUserId,

            actorPersonId,

            "inspection_template_category",

            entity.Id.ToString(),

            "Succeeded",

            cancellationToken: cancellationToken);



        return MapCategory(entity);

    }



    public async Task DeleteCategoryAsync(

        Guid tenantId,

        Guid actorUserId,

        string actorPersonId,

        Guid inspectionTemplateId,

        Guid categoryId,

        CancellationToken cancellationToken = default)

    {

        var entity = await GetCategoryForWriteAsync(tenantId, inspectionTemplateId, categoryId, cancellationToken);

        var linkedItems = await db.InspectionChecklistItems

            .Where(x => x.TenantId == tenantId && x.CategoryId == categoryId)

            .ToListAsync(cancellationToken);

        foreach (var item in linkedItems)

        {

            item.CategoryId = null;

            item.UpdatedAt = DateTimeOffset.UtcNow;

        }



        db.InspectionTemplateCategories.Remove(entity);

        await TouchTemplateAsync(tenantId, inspectionTemplateId, cancellationToken);

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(

            "inspection_template.category.delete",

            tenantId,

            actorUserId,

            actorPersonId,

            "inspection_template_category",

            entity.Id.ToString(),

            "Succeeded",

            cancellationToken: cancellationToken);

    }



    public async Task<InspectionChecklistItemResponse> CreateChecklistItemAsync(

        Guid tenantId,

        Guid actorUserId,

        string actorPersonId,

        Guid inspectionTemplateId,

        CreateInspectionChecklistItemRequest request,

        CancellationToken cancellationToken = default)

    {

        _ = await GetTemplateForWriteAsync(tenantId, inspectionTemplateId, cancellationToken);

        var itemKey = NormalizeItemKey(request.ItemKey);

        var exists = await db.InspectionChecklistItems.AnyAsync(

            x => x.TenantId == tenantId

                && x.InspectionTemplateId == inspectionTemplateId

                && x.ItemKey == itemKey,

            cancellationToken);

        if (exists)

        {

            throw new StlApiException(

                "inspection_template.item.duplicate_key",

                "A checklist item with this key already exists on the template.",

                409);

        }



        var category = await ResolveCategoryAsync(

            tenantId,

            inspectionTemplateId,

            request.CategoryId,

            cancellationToken);



        var now = DateTimeOffset.UtcNow;

        var entity = new InspectionChecklistItem

        {

            Id = Guid.NewGuid(),

            TenantId = tenantId,

            InspectionTemplateId = inspectionTemplateId,

            CategoryId = category?.Id,

            ItemKey = itemKey,

            Prompt = NormalizePrompt(request.Prompt),

            HelpText = NormalizeOptionalText(request.HelpText, 1024),

            ItemType = NormalizeItemType(request.ItemType),

            SettingsJson = SerializeSettings(request.Settings),

            ControlledOptionsJson = SerializeControlledOptions(NormalizeControlledOptions(request.ItemType, request.ControlledOptions)),

            AcceptableRangeMin = NormalizeAcceptableRangeMin(request.ItemType, request.AcceptableRangeMin),

            AcceptableRangeMax = NormalizeAcceptableRangeMax(request.ItemType, request.AcceptableRangeMax),

            UnitOfMeasure = NormalizeUnitOfMeasure(request.ItemType, request.UnitOfMeasure),

            IsRequired = request.IsRequired,

            SortOrder = NormalizeSortOrder(request.SortOrder),

            CreatedAt = now,

            UpdatedAt = now

        };

        ValidateAcceptableRange(entity.AcceptableRangeMin, entity.AcceptableRangeMax);



        db.InspectionChecklistItems.Add(entity);

        await TouchTemplateAsync(tenantId, inspectionTemplateId, cancellationToken);

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(

            "inspection_template.checklist_item.create",

            tenantId,

            actorUserId,

            actorPersonId,

            "inspection_checklist_item",

            entity.Id.ToString(),

            "Succeeded",

            cancellationToken: cancellationToken);



        return MapChecklistItem(entity, category?.CategoryKey);

    }



    public async Task<InspectionChecklistItemResponse> UpdateChecklistItemAsync(

        Guid tenantId,

        Guid actorUserId,

        string actorPersonId,

        Guid inspectionTemplateId,

        Guid checklistItemId,

        UpdateInspectionChecklistItemRequest request,

        CancellationToken cancellationToken = default)

    {

        var entity = await GetChecklistItemForWriteAsync(

            tenantId,

            inspectionTemplateId,

            checklistItemId,

            cancellationToken);

        var category = await ResolveCategoryAsync(

            tenantId,

            inspectionTemplateId,

            request.CategoryId,

            cancellationToken);



        entity.Prompt = NormalizePrompt(request.Prompt);

        entity.HelpText = NormalizeOptionalText(request.HelpText, 1024);

        entity.ItemType = NormalizeItemType(request.ItemType);

        entity.SettingsJson = SerializeSettings(request.Settings);

        entity.ControlledOptionsJson = SerializeControlledOptions(NormalizeControlledOptions(request.ItemType, request.ControlledOptions));

        entity.AcceptableRangeMin = NormalizeAcceptableRangeMin(request.ItemType, request.AcceptableRangeMin);

        entity.AcceptableRangeMax = NormalizeAcceptableRangeMax(request.ItemType, request.AcceptableRangeMax);

        entity.UnitOfMeasure = NormalizeUnitOfMeasure(request.ItemType, request.UnitOfMeasure);

        ValidateAcceptableRange(entity.AcceptableRangeMin, entity.AcceptableRangeMax);

        entity.IsRequired = request.IsRequired;

        entity.SortOrder = NormalizeSortOrder(request.SortOrder);

        entity.CategoryId = category?.Id;

        entity.UpdatedAt = DateTimeOffset.UtcNow;



        await TouchTemplateAsync(tenantId, inspectionTemplateId, cancellationToken);

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(

            "inspection_template.checklist_item.update",

            tenantId,

            actorUserId,

            actorPersonId,

            "inspection_checklist_item",

            entity.Id.ToString(),

            "Succeeded",

            cancellationToken: cancellationToken);



        return MapChecklistItem(entity, category?.CategoryKey);

    }



    public async Task DeleteChecklistItemAsync(

        Guid tenantId,

        Guid actorUserId,

        string actorPersonId,

        Guid inspectionTemplateId,

        Guid checklistItemId,

        CancellationToken cancellationToken = default)

    {

        var entity = await GetChecklistItemForWriteAsync(

            tenantId,

            inspectionTemplateId,

            checklistItemId,

            cancellationToken);



        db.InspectionChecklistItems.Remove(entity);

        await TouchTemplateAsync(tenantId, inspectionTemplateId, cancellationToken);

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(

            "inspection_template.checklist_item.delete",

            tenantId,

            actorUserId,

            actorPersonId,

            "inspection_checklist_item",

            entity.Id.ToString(),

            "Succeeded",

            cancellationToken: cancellationToken);

    }



    public async Task<InspectionTemplateDetailResponse> ReplaceAssetTypesAsync(

        Guid tenantId,

        Guid actorUserId,

        string actorPersonId,

        Guid inspectionTemplateId,

        ReplaceInspectionTemplateAssetTypesRequest request,

        CancellationToken cancellationToken = default)

    {

        _ = await GetTemplateForWriteAsync(tenantId, inspectionTemplateId, cancellationToken);

        var distinctIds = request.AssetTypeIds.Distinct().ToList();

        foreach (var assetTypeId in distinctIds)

        {

            _ = await assetTypeService.GetActiveTypeAsync(tenantId, assetTypeId, cancellationToken);

        }



        var existing = await db.InspectionTemplateAssetTypes

            .Where(x => x.TenantId == tenantId && x.InspectionTemplateId == inspectionTemplateId)

            .ToListAsync(cancellationToken);

        db.InspectionTemplateAssetTypes.RemoveRange(existing);



        var now = DateTimeOffset.UtcNow;

        foreach (var assetTypeId in distinctIds)

        {

            db.InspectionTemplateAssetTypes.Add(new InspectionTemplateAssetType

            {

                Id = Guid.NewGuid(),

                TenantId = tenantId,

                InspectionTemplateId = inspectionTemplateId,

                AssetTypeId = assetTypeId,

                CreatedAt = now

            });

        }



        await TouchTemplateAsync(tenantId, inspectionTemplateId, cancellationToken);

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(

            "inspection_template.asset_types.replace",

            tenantId,

            actorUserId,

            actorPersonId,

            "inspection_template",

            inspectionTemplateId.ToString(),

            "Succeeded",

            cancellationToken: cancellationToken);



        return await GetAsync(tenantId, inspectionTemplateId, cancellationToken);

    }



    private async Task<InspectionTemplate> GetTemplateForWriteAsync(

        Guid tenantId,

        Guid inspectionTemplateId,

        CancellationToken cancellationToken)

    {

        var entity = await db.InspectionTemplates.FirstOrDefaultAsync(

            x => x.TenantId == tenantId && x.Id == inspectionTemplateId,

            cancellationToken);

        if (entity is null)

        {

            throw new StlApiException("inspection_template.not_found", "Inspection template was not found.", 404);

        }



        return entity;

    }



    private async Task<InspectionTemplateCategory> GetCategoryForWriteAsync(

        Guid tenantId,

        Guid inspectionTemplateId,

        Guid categoryId,

        CancellationToken cancellationToken)

    {

        var entity = await db.InspectionTemplateCategories.FirstOrDefaultAsync(

            x => x.TenantId == tenantId

                && x.InspectionTemplateId == inspectionTemplateId

                && x.Id == categoryId,

            cancellationToken);

        if (entity is null)

        {

            throw new StlApiException("inspection_template.category.not_found", "Template category was not found.", 404);

        }



        return entity;

    }



    private async Task<InspectionChecklistItem> GetChecklistItemForWriteAsync(

        Guid tenantId,

        Guid inspectionTemplateId,

        Guid checklistItemId,

        CancellationToken cancellationToken)

    {

        var entity = await db.InspectionChecklistItems.FirstOrDefaultAsync(

            x => x.TenantId == tenantId

                && x.InspectionTemplateId == inspectionTemplateId

                && x.Id == checklistItemId,

            cancellationToken);

        if (entity is null)

        {

            throw new StlApiException("inspection_template.item.not_found", "Checklist item was not found.", 404);

        }



        return entity;

    }



    private async Task<InspectionTemplateCategory?> ResolveCategoryAsync(

        Guid tenantId,

        Guid inspectionTemplateId,

        Guid? categoryId,

        CancellationToken cancellationToken)

    {

        if (!categoryId.HasValue)

        {

            return null;

        }



        return await GetCategoryForWriteAsync(tenantId, inspectionTemplateId, categoryId.Value, cancellationToken);

    }



    private async Task TouchTemplateAsync(

        Guid tenantId,

        Guid inspectionTemplateId,

        CancellationToken cancellationToken)

    {

        var template = await GetTemplateForWriteAsync(tenantId, inspectionTemplateId, cancellationToken);

        template.Version += 1;

        template.UpdatedAt = DateTimeOffset.UtcNow;

    }

    private async Task<IReadOnlyList<InspectionTemplateValidationIssueResponse>> BuildValidationIssuesAsync(
        Guid tenantId,
        InspectionTemplateDetailResponse template,
        CancellationToken cancellationToken)
    {
        var issues = new List<InspectionTemplateValidationIssueResponse>();

        if (string.IsNullOrWhiteSpace(template.TemplateCategoryKey))
        {
            issues.Add(new InspectionTemplateValidationIssueResponse(
                "inspection_template.category_missing",
                "Assign a template category before publishing.",
                "basics",
                false));
        }

        if (string.IsNullOrWhiteSpace(template.OwningSiteRef))
        {
            issues.Add(new InspectionTemplateValidationIssueResponse(
                "inspection_template.site_missing",
                "Choose an owning site to make the template easier to route.",
                "ownership",
                false));
        }

        if (string.IsNullOrWhiteSpace(template.OwningTeamRef))
        {
            issues.Add(new InspectionTemplateValidationIssueResponse(
                "inspection_template.team_missing",
                "Choose an owning team to clarify responsibility.",
                "ownership",
                false));
        }

        if (string.IsNullOrWhiteSpace(template.OwnerPersonId))
        {
            issues.Add(new InspectionTemplateValidationIssueResponse(
                "inspection_template.owner_missing",
                "Choose an owner person for this template.",
                "ownership",
                false));
        }

        if (template.EstimatedDurationMinutes is null)
        {
            issues.Add(new InspectionTemplateValidationIssueResponse(
                "inspection_template.duration_missing",
                "Add an estimated duration to help planners and technicians.",
                "basics",
                false));
        }

        if (template.Categories.Count == 0)
        {
            issues.Add(new InspectionTemplateValidationIssueResponse(
                "inspection_template.categoryless_template",
                "Add at least one category to structure the checklist.",
                "categories",
                false));
        }

        if (template.ChecklistItems.Count == 0)
        {
            issues.Add(new InspectionTemplateValidationIssueResponse(
                "inspection_template.checklist_missing",
                "Add at least one checklist item before publishing.",
                "checklist",
                true));
        }

        if (template.LinkedAssetTypes.Count == 0)
        {
            issues.Add(new InspectionTemplateValidationIssueResponse(
                "inspection_template.asset_coverage_missing",
                "Link at least one asset type so the template has a target audience.",
                "coverage",
                false));
        }

        if (string.IsNullOrWhiteSpace(template.Settings.TryGetValue("executionMode", out var executionMode) ? executionMode?.ToString() : null))
        {
            issues.Add(new InspectionTemplateValidationIssueResponse(
                "inspection_template.execution_mode_missing",
                "Set an execution mode in the template settings.",
                "review",
                false));
        }

        var compatibleAssetCount = await CountCompatibleAssetsAsync(
            tenantId,
            template.LinkedAssetTypes.Select(x => x.AssetTypeId).ToArray(),
            cancellationToken);

        if (compatibleAssetCount == 0)
        {
            issues.Add(new InspectionTemplateValidationIssueResponse(
                "inspection_template.no_compatible_assets",
                "No compatible assets were found for the linked asset types.",
                "coverage",
                false));
        }

        return issues;
    }

    private async Task<CompatibleAssetPreviewResponse> BuildCompatibleAssetPreviewAsync(
        Guid tenantId,
        InspectionTemplateDetailResponse template,
        CancellationToken cancellationToken)
    {
        var linkedAssetTypeIds = template.LinkedAssetTypes.Select(x => x.AssetTypeId).Distinct().ToArray();
        var compatibleCount = await CountCompatibleAssetsAsync(tenantId, linkedAssetTypeIds, cancellationToken);
        var sampleAssets = await LoadAssetPreviewAssetsAsync(tenantId, linkedAssetTypeIds, true, 8, cancellationToken);
        var excludedAssets = await LoadAssetPreviewAssetsAsync(tenantId, linkedAssetTypeIds, false, 8, cancellationToken);

        return new CompatibleAssetPreviewResponse(
            compatibleCount,
            sampleAssets,
            excludedAssets);
    }

    private async Task<int> CountCompatibleAssetsAsync(
        Guid tenantId,
        IReadOnlyCollection<Guid> linkedAssetTypeIds,
        CancellationToken cancellationToken)
    {
        if (linkedAssetTypeIds.Count == 0)
        {
            return 0;
        }

        return await db.Assets.AsNoTracking()
            .Where(x => x.TenantId == tenantId && linkedAssetTypeIds.Contains(x.AssetTypeId))
            .CountAsync(cancellationToken);
    }

    private async Task<IReadOnlyList<AssetSearchResponse>> LoadAssetPreviewAssetsAsync(
        Guid tenantId,
        IReadOnlyCollection<Guid> linkedAssetTypeIds,
        bool compatible,
        int take,
        CancellationToken cancellationToken)
    {
        var query = db.Assets
            .AsNoTracking()
            .Include(x => x.AssetType)
            .ThenInclude(x => x.AssetClass)
            .Where(x => x.TenantId == tenantId);

        if (compatible)
        {
            query = query.Where(x => linkedAssetTypeIds.Contains(x.AssetTypeId));
        }
        else if (linkedAssetTypeIds.Count > 0)
        {
            query = query.Where(x => !linkedAssetTypeIds.Contains(x.AssetTypeId));
        }

        var assets = await query
            .OrderBy(x => x.AssetTag)
            .ThenBy(x => x.Name)
            .Take(Math.Clamp(take, 1, 50))
            .ToListAsync(cancellationToken);

        if (assets.Count == 0)
        {
            return [];
        }

        var assetIds = assets.Select(x => x.Id).ToArray();
        var openDefectCounts = await db.Defects
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId
                && assetIds.Contains(x.AssetId)
                && (x.Status == DefectStatuses.Open
                    || x.Status == DefectStatuses.Acknowledged
                    || x.Status == DefectStatuses.InRepair))
            .GroupBy(x => x.AssetId)
            .Select(x => new { AssetId = x.Key, Count = x.Count() })
            .ToDictionaryAsync(x => x.AssetId, x => x.Count, cancellationToken);

        var openWorkOrderCounts = await db.WorkOrders
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId
                && assetIds.Contains(x.AssetId)
                && WorkOrderStatuses.Active.Contains(x.Status))
            .GroupBy(x => x.AssetId)
            .Select(x => new { AssetId = x.Key, Count = x.Count() })
            .ToDictionaryAsync(x => x.AssetId, x => x.Count, cancellationToken);

        var readinessByAssetId = await db.AssetReadinessStates
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && assetIds.Contains(x.AssetId))
            .ToDictionaryAsync(x => x.AssetId, x => x.ReadinessStatusKey, cancellationToken);

        return assets
            .Select(asset => new AssetSearchResponse(
                asset.Id,
                asset.AssetTypeId,
                asset.AssetType.TypeKey,
                asset.AssetType.Name,
                asset.AssetType.AssetClass.ClassKey,
                asset.AssetType.AssetClass.Name,
                asset.AssetTag,
                asset.Name,
                asset.Description,
                asset.LifecycleStatus,
                asset.SiteRef,
                asset.StaffarrSiteOrgUnitId,
                asset.StaffarrSiteNameSnapshot,
                openDefectCounts.GetValueOrDefault(asset.Id, 0),
                openWorkOrderCounts.GetValueOrDefault(asset.Id, 0),
                readinessByAssetId.GetValueOrDefault(asset.Id, "unknown"),
                asset.CreatedAt,
                asset.UpdatedAt))
            .ToList();
    }

    private static void EnsurePublishConfirmations(PublishInspectionTemplateRequest request)
    {
        var missing = new List<string>();

        if (!request.ConfirmComplianceRelated)
        {
            missing.Add("confirmComplianceRelated");
        }

        if (!request.ConfirmReadinessImpact)
        {
            missing.Add("confirmReadinessImpact");
        }

        if (!request.ConfirmFailureAutomation)
        {
            missing.Add("confirmFailureAutomation");
        }

        if (!request.ConfirmSupervisorRelease)
        {
            missing.Add("confirmSupervisorRelease");
        }

        if (missing.Count > 0)
        {
            throw new StlApiException(
                "inspection_template.publish_confirmation_required",
                "Confirm the publish checklist before activating the template.",
                400,
                new Dictionary<string, object?>
                {
                    ["missingConfirmations"] = missing,
                });
        }
    }

    private static string BuildPreviewSummary(
        InspectionTemplateDetailResponse template,
        InspectionTemplateValidationResponse validation,
        CompatibleAssetPreviewResponse assets)
    {
        return $"{template.Name}: {validation.ChecklistItemCount} checklist items, {validation.SectionCount} sections, {assets.CompatibleCount} compatible assets";
    }

    private static string NormalizeInspectionType(string? inspectionType)
    {
        var normalized = string.IsNullOrWhiteSpace(inspectionType)
            ? InspectionTemplateInspectionTypes.Custom
            : inspectionType.Trim().ToLowerInvariant();

        if (!AllowedInspectionTypes.Contains(normalized))
        {
            throw new StlApiException(
                "inspection_template.invalid_inspection_type",
                "Inspection type must be one of the supported values.",
                400);
        }

        return normalized;
    }

    private static string NormalizeTemplateStatus(string status)
    {
        var normalized = status.Trim().ToLowerInvariant();

        if (!AllowedTemplateStatuses.Contains(normalized))
        {
            throw new StlApiException(
                "inspection_template.invalid_status",
                "Template status must be draft, active, retired, or archived.",
                400);
        }

        return string.Equals(normalized, InspectionTemplateStatuses.Inactive, StringComparison.OrdinalIgnoreCase)
            ? InspectionTemplateStatuses.Retired
            : normalized;
    }

    private static string? NormalizeOptionalKey(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim().ToLowerInvariant();
        if (normalized.Length is < 2 or > 128)
        {
            throw new StlApiException(
                "inspection_template.invalid_reference",
                "Reference keys must be between 2 and 128 characters.",
                400);
        }

        return normalized;
    }

    private static string? NormalizeOptionalReference(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim();
        if (trimmed.Length is < 1 or > 128)
        {
            throw new StlApiException(
                "inspection_template.invalid_reference",
                "Reference values must be between 1 and 128 characters.",
                400);
        }

        return trimmed;
    }

    private static int? NormalizeOptionalDurationMinutes(int? durationMinutes)
    {
        if (!durationMinutes.HasValue)
        {
            return null;
        }

        if (durationMinutes.Value is < 1 or > 24 * 60)
        {
            throw new StlApiException(
                "inspection_template.invalid_duration",
                "Estimated duration must be between 1 and 1,440 minutes.",
                400);
        }

        return durationMinutes.Value;
    }

    private static string NormalizeOptionalText(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var trimmed = value.Trim();
        if (trimmed.Length > maxLength)
        {
            throw new StlApiException(
                "inspection_template.invalid_text",
                $"Text must be {maxLength} characters or fewer.",
                400);
        }

        return trimmed;
    }

    private static string? NormalizeReasonCode(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim().ToLowerInvariant().Replace(' ', '_').Replace('-', '_');
        return normalized.Length <= 64 ? normalized : normalized[..64];
    }

    private async Task<string> GenerateCloneTemplateKeyAsync(
        Guid tenantId,
        string sourceTemplateKey,
        CancellationToken cancellationToken)
    {
        var normalizedSource = NormalizeTemplateKey(sourceTemplateKey);
        var candidate = BuildCloneKey(normalizedSource, "copy");
        var suffixIndex = 2;

        while (await db.InspectionTemplates.AnyAsync(
            x => x.TenantId == tenantId && x.TemplateKey == candidate,
            cancellationToken))
        {
            candidate = BuildCloneKey(normalizedSource, $"copy-{suffixIndex++}");
        }

        return candidate;
    }

    private static string BuildCloneKey(string normalizedSource, string suffix)
    {
        const int maxLength = 128;
        var normalized = normalizedSource.Trim().ToLowerInvariant();
        var normalizedSuffix = suffix.Trim().ToLowerInvariant();
        var separator = "-";
        var availableSourceLength = maxLength - separator.Length - normalizedSuffix.Length;

        if (availableSourceLength <= 0)
        {
            return normalizedSuffix.Length <= maxLength
                ? normalizedSuffix[..maxLength]
                : normalizedSuffix[..maxLength];
        }

        var trimmedSource = normalized.Length > availableSourceLength
            ? normalized[..availableSourceLength]
            : normalized;

        var candidate = $"{trimmedSource}{separator}{normalizedSuffix}";
        return candidate.Length <= maxLength ? candidate : candidate[..maxLength];
    }



    private static InspectionTemplateCategoryResponse MapCategory(InspectionTemplateCategory entity) =>

        new(

            entity.Id,

            entity.CategoryKey,

            entity.Name,

            entity.Description,

            entity.IsRequired,

            entity.CanBeSkipped,

            entity.SkipReasonRequired,

            entity.TimingTracked,

            entity.SortOrder,

            DeserializeObjectDict(entity.SettingsJson),

            entity.CreatedAt,

            entity.UpdatedAt);



    private static InspectionChecklistItemResponse MapChecklistItem(

        InspectionChecklistItem entity,

        string? categoryKey) =>

        new(

            entity.Id,

            entity.CategoryId,

            categoryKey,

            entity.ItemKey,

            entity.Prompt,

            entity.HelpText,

            entity.ItemType,

            DeserializeStringList(entity.ControlledOptionsJson),

            entity.AcceptableRangeMin,

            entity.AcceptableRangeMax,

            entity.UnitOfMeasure,

            entity.IsRequired,

            entity.SortOrder,

            DeserializeObjectDict(entity.SettingsJson),

            entity.CreatedAt,

            entity.UpdatedAt);



    private static string NormalizeTemplateKey(string templateKey)

    {

        var normalized = templateKey.Trim().ToLowerInvariant();

        if (normalized.Length is < 2 or > 128)

        {

            throw new StlApiException(

                "inspection_template.invalid_key",

                "Template key must be between 2 and 128 characters.",

                400);

        }



        return normalized;

    }



    private static string NormalizeName(string name)

    {

        var normalized = name.Trim();

        if (normalized.Length is < 2 or > 128)

        {

            throw new StlApiException(

                "inspection_template.invalid_name",

                "Template name must be between 2 and 128 characters.",

                400);

        }



        return normalized;

    }



    private static string NormalizeDescription(string description) =>

        description.Trim().Length <= 512 ? description.Trim() : description.Trim()[..512];

    private static string SerializeStringList(IReadOnlyList<string>? values) =>
        JsonSerializer.Serialize((values ?? [])
            .Select(value => value.Trim())
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList());

    private static string SerializeSettings(IReadOnlyDictionary<string, object?>? settings) =>
        JsonSerializer.Serialize(settings ?? new Dictionary<string, object?>());

    private static IReadOnlyDictionary<string, object?> DeserializeObjectDict(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return new Dictionary<string, object?>();
        }

        try
        {
            var data = JsonSerializer.Deserialize<Dictionary<string, object?>>(json);
            return data is null
                ? new Dictionary<string, object?>()
                : new Dictionary<string, object?>(data, StringComparer.OrdinalIgnoreCase);
        }
        catch (JsonException)
        {
            return new Dictionary<string, object?>();
        }
    }



    private static string NormalizeCategoryKey(string categoryKey)

    {

        var normalized = categoryKey.Trim().ToLowerInvariant();

        if (normalized.Length is < 2 or > 128)

        {

            throw new StlApiException(

                "inspection_template.category.invalid_key",

                "Category key must be between 2 and 128 characters.",

                400);

        }



        return normalized;

    }



    private static string NormalizeCategoryName(string name)

    {

        var normalized = name.Trim();

        if (normalized.Length is < 2 or > 128)

        {

            throw new StlApiException(

                "inspection_template.category.invalid_name",

                "Category name must be between 2 and 128 characters.",

                400);

        }



        return normalized;

    }



    private static string NormalizeItemKey(string itemKey)

    {

        var normalized = itemKey.Trim().ToLowerInvariant();

        if (normalized.Length is < 2 or > 128)

        {

            throw new StlApiException(

                "inspection_template.item.invalid_key",

                "Checklist item key must be between 2 and 128 characters.",

                400);

        }



        return normalized;

    }



    private static string NormalizePrompt(string prompt)

    {

        var normalized = prompt.Trim();

        if (normalized.Length is < 2 or > 512)

        {

            throw new StlApiException(

                "inspection_template.item.invalid_prompt",

                "Checklist prompt must be between 2 and 512 characters.",

                400);

        }



        return normalized;

    }



    private static string NormalizeItemType(string itemType)

    {

        var normalized = itemType.Trim().ToLowerInvariant();

        if (!AllowedItemTypes.Contains(normalized))

        {

            throw new StlApiException(

                "inspection_template.item.invalid_type",

                "Item type must be pass_fail, yes_no, numeric, text, select, multi_select, photo, signature, or meter_reading.",

                400);

        }



        return normalized;

    }

    private static IReadOnlyList<string> NormalizeControlledOptions(
        string itemType,
        IReadOnlyList<string>? controlledOptions)
    {
        var normalizedItemType = itemType.Trim().ToLowerInvariant();
        var isSelectable = string.Equals(normalizedItemType, InspectionChecklistItemTypes.Select, StringComparison.OrdinalIgnoreCase)
            || string.Equals(normalizedItemType, InspectionChecklistItemTypes.MultiSelect, StringComparison.OrdinalIgnoreCase);

        if (!isSelectable)
        {
            return [];
        }

        var normalized = (controlledOptions ?? [])
            .Select(option => option.Trim())
            .Where(option => !string.IsNullOrWhiteSpace(option))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (normalized.Count == 0)
        {
            throw new StlApiException(
                "inspection_template.item.controlled_options_required",
                "Selectable checklist items require at least one controlled option.",
                400);
        }

        if (normalized.Count > 50)
        {
            throw new StlApiException(
                "inspection_template.item.controlled_options_too_many",
                "Selectable checklist items can have at most 50 controlled options.",
                400);
        }

        return normalized;
    }

    private static decimal? NormalizeAcceptableRangeMin(string itemType, decimal? acceptableRangeMin)
    {
        if (!string.Equals(itemType.Trim(), InspectionChecklistItemTypes.MeterReading, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return acceptableRangeMin;
    }

    private static decimal? NormalizeAcceptableRangeMax(string itemType, decimal? acceptableRangeMax)
    {
        if (!string.Equals(itemType.Trim(), InspectionChecklistItemTypes.MeterReading, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return acceptableRangeMax;
    }

    private static string? NormalizeUnitOfMeasure(string itemType, string? unitOfMeasure)
    {
        if (!string.Equals(itemType.Trim(), InspectionChecklistItemTypes.MeterReading, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var normalized = unitOfMeasure?.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new StlApiException(
                "inspection_template.item.unit_required",
                "Meter reading checklist items require a unit of measure.",
                400);
        }

        if (normalized.Length is < 1 or > 32)
        {
            throw new StlApiException(
                "inspection_template.item.invalid_unit",
                "Unit of measure must be 32 characters or fewer.",
                400);
        }

        return normalized;
    }

    private static void ValidateAcceptableRange(decimal? acceptableRangeMin, decimal? acceptableRangeMax)
    {
        if (acceptableRangeMin.HasValue && acceptableRangeMax.HasValue && acceptableRangeMin.Value > acceptableRangeMax.Value)
        {
            throw new StlApiException(
                "inspection_template.item.invalid_range",
                "Acceptable range minimum cannot be greater than the maximum.",
                400);
        }
    }

    private static string SerializeControlledOptions(IReadOnlyList<string> controlledOptions) =>
        JsonSerializer.Serialize(controlledOptions);

    private static IReadOnlyList<string> DeserializeStringList(string json) =>
        string.IsNullOrWhiteSpace(json)
            ? []
            : DeserializeStringArray(json);

    private static IReadOnlyList<string> DeserializeStringArray(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<string[]>(json) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }



    private static int NormalizeSortOrder(int sortOrder)

    {

        if (sortOrder is < 0 or > 10000)

        {

            throw new StlApiException(

                "inspection_template.invalid_sort_order",

                "Sort order must be between 0 and 10000.",

                400);

        }



        return sortOrder;

    }

}


