namespace STLCompliance.Shared.Hosting;

public sealed record ProductDescriptor(
    string ProductKey,
    string DisplayName,
    int LocalDevPort);
