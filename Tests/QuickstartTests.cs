using Grpc.Net.Client;
using Quickstart.Common;
using Xunit;

namespace Quickstart.Tests;

public sealed class QuickstartTests : IDisposable
{
    private readonly string[] variableNames =
    [
        "PASSKIT_ENVIRONMENT",
        "PASSKIT_POOL_SIZE",
        "PASSKIT_KEEP_ASSETS",
        "QUICKSTART_TEST_VALUE"
    ];

    [Fact]
    public void EnvFileLoadsValuesAndPreservesExplicitEnvironmentVariables()
    {
        var path = Path.GetTempFileName();
        try
        {
            File.WriteAllText(path, "# comment\nQUICKSTART_TEST_VALUE='from file'\nPASSKIT_ENVIRONMENT=pub2\n");
            Environment.SetEnvironmentVariable("PASSKIT_ENVIRONMENT", "pub1");

            EnvFile.Load(path);

            Assert.Equal("from file", Environment.GetEnvironmentVariable("QUICKSTART_TEST_VALUE"));
            Assert.Equal("pub1", Environment.GetEnvironmentVariable("PASSKIT_ENVIRONMENT"));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void ConstantsRejectInvalidPoolSizes()
    {
        Environment.SetEnvironmentVariable("PASSKIT_POOL_SIZE", "zero");
        Assert.Throws<InvalidOperationException>(() => Constants.PoolSize);
    }

    [Fact]
    public void SharedApiConstructsEveryClientWithoutConnecting()
    {
        using var channel = GrpcChannel.ForAddress("https://localhost");
        var api = new PassKitApi(channel);

        Assert.NotNull(api.Loyalty);
        Assert.NotNull(api.Coupons);
        Assert.NotNull(api.EventTickets);
        Assert.NotNull(api.Flights);
        Assert.NotNull(api.Templates);
        Assert.NotNull(api.Images);
        Assert.NotNull(api.Analytics);
        Assert.NotNull(api.Distribution);
        Assert.NotNull(api.Integrations);
        Assert.NotNull(api.Users);
        Assert.NotNull(api.Certificates);
        Assert.NotNull(api.RawPasses);
        Assert.NotNull(api.Messages);
    }

    public void Dispose()
    {
        foreach (var name in variableNames) Environment.SetEnvironmentVariable(name, null);
    }
}
