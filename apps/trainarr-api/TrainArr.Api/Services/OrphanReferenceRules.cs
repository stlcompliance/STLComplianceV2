namespace TrainArr.Api.Services;



public static class OrphanReferenceRules

{

    public const int DefaultStalenessHours = 24;



    public const string ReferenceKindStaffarrPerson = "staffarr_person";



    public const string ReferenceKindComplianceCoreCitation = "compliancecore_citation";



    public const string ReferenceKindComplianceCoreRulePack = "compliancecore_rule_pack";



    public static int NormalizeBatchSize(int? batchSize) =>

        Math.Clamp(batchSize ?? 10, 1, 50);



    public static int NormalizeStalenessHours(int? stalenessHours) =>

        Math.Clamp(stalenessHours ?? DefaultStalenessHours, 1, 168);



    public static int NormalizeRunListLimit(int? limit) =>

        Math.Clamp(limit ?? 20, 1, 100);



    public static int NormalizeFindingListLimit(int? limit) =>

        Math.Clamp(limit ?? 20, 1, 100);



    public static bool IsStale(DateTimeOffset? scannedAt, DateTimeOffset asOfUtc, int stalenessHours)

    {

        if (scannedAt is null)

        {

            return true;

        }



        var threshold = asOfUtc.AddHours(-stalenessHours);

        return scannedAt < threshold;

    }



    public static string BuildStaffarrPersonReferenceKey(Guid personId) =>

        personId.ToString("D");



    public static string BuildComplianceCoreCitationReferenceKey(Guid citationId) =>

        citationId.ToString("D");



    public static string BuildComplianceCoreRulePackReferenceKey(string rulePackKey) =>

        rulePackKey.Trim();

}


