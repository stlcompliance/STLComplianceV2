using System.Net.Http.Json;
using TrainArr.Api.Contracts;

namespace STLCompliance.E2E.Support;

internal static class TrainArrCompletionHelper
{
    public static async Task SatisfyCompletionRequirementsAsync(
        HttpClient trainarrClient,
        Guid assignmentId,
        string trainerToken,
        string traineeToken,
        string? evaluationNotes = null)
    {
        var evaluationRequest = HttpTestClient.Authorized(
            HttpMethod.Post,
            $"/api/training-assignments/{assignmentId}/evaluations",
            trainerToken);
        evaluationRequest.Content = JsonContent.Create(new SubmitTrainingEvaluationRequest(
            assignmentId,
            "pass",
            100m,
            evaluationNotes ?? "Meets qualification standard."));
        (await trainarrClient.SendAsync(evaluationRequest)).EnsureSuccessStatusCode();

        var traineeSignoffRequest = HttpTestClient.Authorized(
            HttpMethod.Post,
            $"/api/training-assignments/{assignmentId}/signoffs",
            traineeToken);
        traineeSignoffRequest.Content = JsonContent.Create(new SubmitTrainingSignoffRequest(
            assignmentId,
            "trainee",
            "Trainee acknowledges completion."));
        (await trainarrClient.SendAsync(traineeSignoffRequest)).EnsureSuccessStatusCode();

        var trainerSignoffRequest = HttpTestClient.Authorized(
            HttpMethod.Post,
            $"/api/training-assignments/{assignmentId}/signoffs",
            trainerToken);
        trainerSignoffRequest.Content = JsonContent.Create(new SubmitTrainingSignoffRequest(
            assignmentId,
            "trainer",
            "Trainer confirms practical competency."));
        (await trainarrClient.SendAsync(trainerSignoffRequest)).EnsureSuccessStatusCode();
    }
}
