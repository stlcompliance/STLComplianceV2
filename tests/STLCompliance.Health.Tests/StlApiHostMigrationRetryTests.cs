using System.Net.Sockets;
using STLCompliance.Shared.Hosting;

namespace STLCompliance.Health.Tests;

public class StlApiHostMigrationRetryTests
{
    [Fact]
    public void Migration_startup_retry_treats_timeouts_as_transient()
    {
        Assert.True(StlApiHost.IsTransientMigrationStartupException(new TimeoutException("startup database timeout")));
    }

    [Fact]
    public void Migration_startup_retry_treats_name_resolution_socket_errors_as_transient()
    {
        Assert.True(StlApiHost.IsTransientMigrationStartupException(new SocketException((int)SocketError.HostNotFound)));
    }

    [Fact]
    public void Migration_startup_retry_does_not_treat_application_errors_as_transient()
    {
        Assert.False(StlApiHost.IsTransientMigrationStartupException(new InvalidOperationException("bad migration")));
    }

    [Theory]
    [InlineData(1, 2)]
    [InlineData(2, 4)]
    [InlineData(3, 8)]
    [InlineData(5, 30)]
    [InlineData(8, 30)]
    public void Migration_startup_retry_delay_uses_bounded_exponential_backoff(int failedAttempt, int expectedSeconds)
    {
        Assert.Equal(TimeSpan.FromSeconds(expectedSeconds), StlApiHost.ComputeMigrationStartupRetryDelay(failedAttempt));
    }
}
