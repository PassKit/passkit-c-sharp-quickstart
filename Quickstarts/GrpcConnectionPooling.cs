using Grpc.Net.Client;
using Quickstart.Common;

namespace GrpcConnectionPool;

internal sealed class GrpcConnectionPool : IDisposable
{
    private readonly object sync = new();
    private readonly List<GrpcChannel> channels;
    private int currentIndex;

    public GrpcConnectionPool(int? poolSize = null)
    {
        var size = poolSize ?? Constants.PoolSize;
        channels = Enumerable.Range(0, size)
            .Select(_ => GrpcConnection.GrpcConnection.ConnectWithPassKitServer())
            .ToList();
    }

    public GrpcChannel GetChannel()
    {
        lock (sync)
        {
            ObjectDisposedException.ThrowIf(channels.Count == 0, this);
            var channel = channels[currentIndex];
            currentIndex = (currentIndex + 1) % channels.Count;
            return channel;
        }
    }

    public void Dispose()
    {
        lock (sync)
        {
            foreach (var channel in channels) channel.Dispose();
            channels.Clear();
        }
    }
}
