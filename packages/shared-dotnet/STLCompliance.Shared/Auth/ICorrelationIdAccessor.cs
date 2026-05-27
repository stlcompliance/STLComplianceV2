namespace STLCompliance.Shared.Auth;

public interface ICorrelationIdAccessor
{
    Guid CorrelationId { get; }

    void Set(Guid correlationId);
}
