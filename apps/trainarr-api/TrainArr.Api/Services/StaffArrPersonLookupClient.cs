using System.Net.Http.Headers;

using Microsoft.Extensions.Options;

using TrainArr.Api.Options;



namespace TrainArr.Api.Services;



public sealed class StaffArrPersonLookupClient(

    HttpClient httpClient,

    IOptions<StaffArrClientOptions> options)

{

    public async Task<bool> PersonExistsAsync(

        Guid tenantId,

        Guid personId,

        CancellationToken cancellationToken = default)

    {

        var serviceToken = options.Value.ServiceToken;

        if (string.IsNullOrWhiteSpace(serviceToken))

        {

            return false;

        }



        using var request = new HttpRequestMessage(

            HttpMethod.Get,

            $"api/integrations/person-lookup?tenantId={tenantId}&personId={personId}");

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", serviceToken);



        using var response = await httpClient.SendAsync(request, cancellationToken);

        return response.IsSuccessStatusCode;

    }

}


