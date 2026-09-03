using Grpc.Core;

namespace Quickstart.Common;

public static class GrpcStreamExtensions
{
    public static async Task<List<T>> ToListAsync<T>(
        this AsyncServerStreamingCall<T> call,
        CancellationToken cancellationToken = default
    )
    {
        using (call)
        {
            var values = new List<T>();
            while (await call.ResponseStream.MoveNext(cancellationToken))
                values.Add(call.ResponseStream.Current);
            return values;
        }
    }

    public static async IAsyncEnumerable<T> AsAsyncEnumerable<T>(
        this AsyncServerStreamingCall<T> call,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        using (call)
        {
            while (await call.ResponseStream.MoveNext(cancellationToken))
                yield return call.ResponseStream.Current;
        }
    }
}
