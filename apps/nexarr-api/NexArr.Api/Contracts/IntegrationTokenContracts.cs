namespace NexArr.Api.Contracts;

public sealed record IntegrationTokenProvisionResponse(IReadOnlyDictionary<string, string> Tokens);
