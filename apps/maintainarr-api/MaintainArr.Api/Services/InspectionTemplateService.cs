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

        InspectionTemplateStatuses.Inactive

    };



    private static readonly HashSet<string> AllowedItemTypes = new(StringComparer.OrdinalIgnoreCase)

    {

        InspectionChecklistItemTypes.PassFail,

        InspectionChecklistItemTypes.Numeric,

        InspectionChecklistItemTypes.Text

    };



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

                template.Version,

                template.Status,

                categoryCounts.GetValueOrDefault(template.Id),

                itemCounts.GetValueOrDefault(template.Id),

                linkCounts.GetValueOrDefault(template.Id),

                template.CreatedAt,

                template.UpdatedAt))

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



        var categories = await db.InspectionTemplateCategories

            .AsNoTracking()

            .Where(x => x.TenantId == tenantId && x.InspectionTemplateId == inspectionTemplateId)

            .OrderBy(x => x.SortOrder)

            .ThenBy(x => x.Name)

            .Select(x => new InspectionTemplateCategoryResponse(

                x.Id,

                x.CategoryKey,

                x.Name,

                x.SortOrder,

                x.CreatedAt,

                x.UpdatedAt))

            .ToListAsync(cancellationToken);



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

            template.Version,

            template.Status,

            categories,

            checklistItems,

            linkedAssetTypes,

            template.CreatedAt,

            template.UpdatedAt);

    }



    public async Task<InspectionTemplateDetailResponse> CreateAsync(

        Guid tenantId,

        Guid actorUserId,

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

            "inspection_template",

            entity.Id.ToString(),

            "Succeeded",

            cancellationToken: cancellationToken);



        return await GetAsync(tenantId, entity.Id, cancellationToken);

    }



    public async Task<InspectionTemplateDetailResponse> UpdateAsync(

        Guid tenantId,

        Guid actorUserId,

        Guid inspectionTemplateId,

        UpdateInspectionTemplateRequest request,

        CancellationToken cancellationToken = default)

    {

        var entity = await GetTemplateForWriteAsync(tenantId, inspectionTemplateId, cancellationToken);

        entity.Name = NormalizeName(request.Name);

        entity.Description = NormalizeDescription(request.Description);

        entity.UpdatedAt = DateTimeOffset.UtcNow;



        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(

            "inspection_template.update",

            tenantId,

            actorUserId,

            "inspection_template",

            entity.Id.ToString(),

            "Succeeded",

            cancellationToken: cancellationToken);



        return await GetAsync(tenantId, entity.Id, cancellationToken);

    }



    public async Task<InspectionTemplateDetailResponse> UpdateStatusAsync(

        Guid tenantId,

        Guid actorUserId,

        Guid inspectionTemplateId,

        UpdateInspectionTemplateStatusRequest request,

        CancellationToken cancellationToken = default)

    {

        var status = request.Status.Trim().ToLowerInvariant();

        if (!AllowedTemplateStatuses.Contains(status))

        {

            throw new StlApiException(

                "inspection_template.invalid_status",

                "Template status must be draft, active, or inactive.",

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

        entity.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(

            "inspection_template.status.update",

            tenantId,

            actorUserId,

            "inspection_template",

            entity.Id.ToString(),

            "Succeeded",

            cancellationToken: cancellationToken);



        return await GetAsync(tenantId, entity.Id, cancellationToken);

    }



    public async Task<InspectionTemplateCategoryResponse> CreateCategoryAsync(

        Guid tenantId,

        Guid actorUserId,

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

            "inspection_template_category",

            entity.Id.ToString(),

            "Succeeded",

            cancellationToken: cancellationToken);



        return MapCategory(entity);

    }



    public async Task<InspectionTemplateCategoryResponse> UpdateCategoryAsync(

        Guid tenantId,

        Guid actorUserId,

        Guid inspectionTemplateId,

        Guid categoryId,

        UpdateInspectionTemplateCategoryRequest request,

        CancellationToken cancellationToken = default)

    {

        var entity = await GetCategoryForWriteAsync(tenantId, inspectionTemplateId, categoryId, cancellationToken);

        entity.Name = NormalizeCategoryName(request.Name);

        entity.SortOrder = NormalizeSortOrder(request.SortOrder);

        entity.UpdatedAt = DateTimeOffset.UtcNow;



        await TouchTemplateAsync(tenantId, inspectionTemplateId, cancellationToken);

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(

            "inspection_template.category.update",

            tenantId,

            actorUserId,

            "inspection_template_category",

            entity.Id.ToString(),

            "Succeeded",

            cancellationToken: cancellationToken);



        return MapCategory(entity);

    }



    public async Task DeleteCategoryAsync(

        Guid tenantId,

        Guid actorUserId,

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

            "inspection_template_category",

            entity.Id.ToString(),

            "Succeeded",

            cancellationToken: cancellationToken);

    }



    public async Task<InspectionChecklistItemResponse> CreateChecklistItemAsync(

        Guid tenantId,

        Guid actorUserId,

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

            ItemType = NormalizeItemType(request.ItemType),

            IsRequired = request.IsRequired,

            SortOrder = NormalizeSortOrder(request.SortOrder),

            CreatedAt = now,

            UpdatedAt = now

        };



        db.InspectionChecklistItems.Add(entity);

        await TouchTemplateAsync(tenantId, inspectionTemplateId, cancellationToken);

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(

            "inspection_template.checklist_item.create",

            tenantId,

            actorUserId,

            "inspection_checklist_item",

            entity.Id.ToString(),

            "Succeeded",

            cancellationToken: cancellationToken);



        return MapChecklistItem(entity, category?.CategoryKey);

    }



    public async Task<InspectionChecklistItemResponse> UpdateChecklistItemAsync(

        Guid tenantId,

        Guid actorUserId,

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

        entity.ItemType = NormalizeItemType(request.ItemType);

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

            "inspection_checklist_item",

            entity.Id.ToString(),

            "Succeeded",

            cancellationToken: cancellationToken);



        return MapChecklistItem(entity, category?.CategoryKey);

    }



    public async Task DeleteChecklistItemAsync(

        Guid tenantId,

        Guid actorUserId,

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

            "inspection_checklist_item",

            entity.Id.ToString(),

            "Succeeded",

            cancellationToken: cancellationToken);

    }



    public async Task<InspectionTemplateDetailResponse> ReplaceAssetTypesAsync(

        Guid tenantId,

        Guid actorUserId,

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



    private static InspectionTemplateCategoryResponse MapCategory(InspectionTemplateCategory entity) =>

        new(

            entity.Id,

            entity.CategoryKey,

            entity.Name,

            entity.SortOrder,

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

            entity.ItemType,

            entity.IsRequired,

            entity.SortOrder,

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

                "Item type must be pass_fail, numeric, or text.",

                400);

        }



        return normalized;

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


