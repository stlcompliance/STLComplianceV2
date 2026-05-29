using System.Net.Http.Headers;
using System.Text;

namespace STLCompliance.Shared.Integration;

public sealed class StlNexArrLaunchClient(HttpClient httpClient)
{
    public async Task<(int StatusCode, string Body, string ContentType)> ForwardAsync(
        HttpMethod method,
        string relativePath,
        string? authorizationHeader,
        string? jsonBody,
        CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(method, relativePath);
        if (!string.IsNullOrWhiteSpace(authorizationHeader))
        {
            request.Headers.TryAddWithoutValidation("Authorization", authorizationHeader);
        }

        if (jsonBody is not null)
        {
            request.Content = new ByteArrayContent(Encoding.UTF8.GetBytes(jsonBody))
            {
                Headers = { ContentType = new MediaTypeHeaderValue("application/json") },
            };
        }

        using var response = await httpClient.SendAsync(request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        var contentType = response.Content.Headers.ContentType?.ToString() ?? "application/json";
        return ((int)response.StatusCode, body, contentType);
    }
}
