namespace STLCompliance.Shared.Auth;

public sealed class CorrelationIdAccessor : ICorrelationIdAccessor
{
    private readonly AsyncLocal<Guid?> _correlationId = new();

    public Guid CorrelationId => _correlationId.Value ?? Guid.NewGuid();

    public void Set(Guid correlationId) => _correlationId.Value = correlationId;
}
