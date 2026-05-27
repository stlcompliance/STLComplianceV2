using System.Net.Http.Headers;
using System.Net.Http.Json;
using TrainArr.Api.Contracts;

namespace STLCompliance.StaffArr.Auth.Tests;

internal static class TrainArrCompletionTestHelper
{
    public static async Task SatisfyCompletionRequirementsAsync(
        HttpClient trainarrClient,
        Guid assignmentId,
        string trainerToken,
        string traineeToken,
        string? evaluationNotes = null)
    {
        var evaluationRequest = Authorized(
            HttpMethod.Post,
            $"/api/training-assignments/{assignmentId}/evaluations",
            trainerToken);
        evaluationRequest.Content = JsonContent.Create(new SubmitTrainingEvaluationRequest(
            assignmentId,
            "pass",
            100m,
            evaluationNotes ?? "Meets qualification standard."));
        (await trainarrClient.SendAsync(evaluationRequest)).EnsureSuccessStatusCode();

        var traineeSignoffRequest = Authorized(
            HttpMethod.Post,
            $"/api/training-assignments/{assignmentId}/signoffs",
            traineeToken);
        traineeSignoffRequest.Content = JsonContent.Create(new SubmitTrainingSignoffRequest(
            assignmentId,
            "trainee",
            "Trainee acknowledges completion."));
        (await trainarrClient.SendAsync(traineeSignoffRequest)).EnsureSuccessStatusCode();

        var trainerSignoffRequest = Authorized(
            HttpMethod.Post,
            $"/api/training-assignments/{assignmentId}/signoffs",
            trainerToken);
        trainerSignoffRequest.Content = JsonContent.Create(new SubmitTrainingSignoffRequest(
            assignmentId,
            "trainer",
            "Trainer confirms practical competency."));
        (await trainarrClient.SendAsync(trainerSignoffRequest)).EnsureSuccessStatusCode();
    }

    private static HttpRequestMessage Authorized(HttpMethod method, string url, string accessToken)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return request;
    }
}
