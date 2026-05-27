using STLCompliance.Shared.Data;



namespace ComplianceCore.Api.Entities;



public sealed class RulePack : IHasTenant

{

    public Guid Id { get; set; }



    public Guid TenantId { get; set; }



    public Guid RegulatoryProgramId { get; set; }



    public string PackKey { get; set; } = string.Empty;



    public string Label { get; set; } = string.Empty;



    public string Description { get; set; } = string.Empty;



    public int VersionNumber { get; set; } = 1;



    public string Status { get; set; } = RulePackStatuses.Draft;



    public bool IsActive { get; set; } = true;



    public DateTimeOffset CreatedAt { get; set; }



    public DateTimeOffset UpdatedAt { get; set; }

    public string? RuleContentJson { get; set; }

    public DateTimeOffset? LastScheduledEvaluationAt { get; set; }

    public RegulatoryProgram? RegulatoryProgram { get; set; }

}



public static class RulePackStatuses

{

    public const string Draft = "draft";



    public const string Review = "review";



    public const string Published = "published";



    public const string Archived = "archived";



    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)

    {

        Draft,

        Review,

        Published,

        Archived

    };

}


