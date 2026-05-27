using System.Net.Http.Headers;

namespace STLCompliance.E2E.Support;

internal static class HttpTestClient
{
    public static HttpRequestMessage Authorized(HttpMethod method, string url, string accessToken)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return request;
    }
}
