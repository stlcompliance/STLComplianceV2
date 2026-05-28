using System.Net.Http.Headers;
using System.Net.Http.Json;
using StaffArr.Api.Contracts;

namespace STLCompliance.StaffArr.Auth.Tests;

internal static class TrainArrAcknowledgementTestHelper
{
    public static async Task AcknowledgePendingForAssignmentAsync(
        HttpClient staffarrClient,
        Guid personId,
        Guid trainarrAssignmentId,
        string memberStaffarrToken)
    {
        var listRequest = Authorized(
            HttpMethod.Get,
            $"/api/training-acknowledgements?personId={personId:D}",
            memberStaffarrToken);
        var listResponse = await staffarrClient.SendAsync(listRequest);
        listResponse.EnsureSuccessStatusCode();
        var acknowledgements = (await listResponse.Content.ReadFromJsonAsync<IReadOnlyList<TrainingAcknowledgementResponse>>())!;
        var pending = acknowledgements.Single(x => x.TrainarrAssignmentId == trainarrAssignmentId);

        var acknowledgeRequest = Authorized(
            HttpMethod.Post,
            $"/api/training-acknowledgements/{pending.AcknowledgementId}/acknowledge",
            memberStaffarrToken);
        var acknowledgeResponse = await staffarrClient.SendAsync(acknowledgeRequest);
        acknowledgeResponse.EnsureSuccessStatusCode();
    }

    private static HttpRequestMessage Authorized(HttpMethod method, string url, string accessToken)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return request;
    }
}
