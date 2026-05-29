using System.Net.Http.Json;
using TrainArr.Api.Contracts;

namespace STLCompliance.E2E.Support;

internal static class TrainArrQualificationCheckHelper
{
    public static async Task<QualificationCheckResponse> RunQualificationCheckAsync(
        HttpClient trainarrClient,
        string accessToken,
        Guid staffarrPersonId,
        string qualificationKey,
        Guid? trainingDefinitionId = null)
    {
        var request = HttpTestClient.Authorized(HttpMethod.Post, "/api/qualification-checks", accessToken);
        request.Content = JsonContent.Create(new CreateQualificationCheckRequest(
            staffarrPersonId,
            qualificationKey,
            null,
            null,
            trainingDefinitionId,
            null));
        var response = await trainarrClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<QualificationCheckResponse>())!;
    }

    public static async Task<TrainingAssignmentDetailResponse> CreateManualAssignmentAsync(
        HttpClient trainarrClient,
        string accessToken,
        Guid staffarrPersonId,
        Guid trainingDefinitionId,
        string qualificationKey,
        DateTimeOffset? dueAt = null)
    {
        var check = await RunQualificationCheckAsync(
            trainarrClient,
            accessToken,
            staffarrPersonId,
            qualificationKey,
            trainingDefinitionId);

        var createRequest = HttpTestClient.Authorized(HttpMethod.Post, "/api/training-assignments", accessToken);
        createRequest.Content = JsonContent.Create(new CreateTrainingAssignmentRequest(
            staffarrPersonId,
            trainingDefinitionId,
            null,
            "manual",
            dueAt,
            check.CheckId));
        var createResponse = await trainarrClient.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();
        return (await createResponse.Content.ReadFromJsonAsync<TrainingAssignmentDetailResponse>())!;
    }

    public static async Task<TrainingAssignmentDetailResponse> CreateRemediationAssignmentAsync(
        HttpClient trainarrClient,
        string accessToken,
        Guid staffarrPersonId,
        Guid trainingDefinitionId,
        string qualificationKey,
        Guid remediationId,
        DateTimeOffset? dueAt = null)
    {
        var check = await RunQualificationCheckAsync(
            trainarrClient,
            accessToken,
            staffarrPersonId,
            qualificationKey,
            trainingDefinitionId);

        var createRequest = HttpTestClient.Authorized(HttpMethod.Post, "/api/training-assignments", accessToken);
        createRequest.Content = JsonContent.Create(new CreateTrainingAssignmentRequest(
            staffarrPersonId,
            trainingDefinitionId,
            remediationId,
            "incident_remediation",
            dueAt,
            check.CheckId));
        var createResponse = await trainarrClient.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();
        return (await createResponse.Content.ReadFromJsonAsync<TrainingAssignmentDetailResponse>())!;
    }
}
