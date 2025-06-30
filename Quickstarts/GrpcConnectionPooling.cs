using System.Security.Cryptography.X509Certificates;
using Grpc.Net.Client;
using Quickstart.Common;

namespace GrpcConnectionPool
{
    class GrpcConnectionPool
    {
        private static readonly object _lock = new object();
        private static GrpcConnectionPool? _instance;
        private readonly List<GrpcChannel> _channelPool;
        private int _currentIndex;
        private readonly int _poolSize;

        private GrpcConnectionPool(int poolSize = 5)
        {
            _poolSize = poolSize;
            _channelPool = new List<GrpcChannel>();

            for (int i = 0; i < _poolSize; i++)
            {
                _channelPool.Add(CreateChannel());
            }
        }

        public static GrpcConnectionPool GetInstance(int poolSize = 5)
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new GrpcConnectionPool(poolSize);
                    }
                }
            }
            return _instance;
        }

        private static GrpcChannel CreateChannel()
        {
            var host = $"https://grpc.{Constants.Environment}.passkit.io";

            try
            {
                // Load client cert
                var clientCert = new X509Certificate2("certs/client.pfx", "", X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.Exportable);

                // Create handler for mTLS
                var handler = new HttpClientHandler();
                handler.ClientCertificates.Add(clientCert);
                handler.ServerCertificateCustomValidationCallback = (httpRequestMessage, cert, chain, errors) =>
                {
                    // Optional: validate server cert chain manually using `rootCA`
                    return true;
                };

                var httpClient = new HttpClient(handler);
                return GrpcChannel.ForAddress(host, new GrpcChannelOptions
                {
                    HttpClient = httpClient
                });
            }
            catch (Exception e)
            {
                throw new Exception($"Failed to configure mTLS channel: {e.Message}", e);
            }
        }

        public GrpcChannel GetChannel()
        {
            lock (_lock)
            {
                var channel = _channelPool[_currentIndex];
                _currentIndex = (_currentIndex + 1) % _poolSize;
                return channel;
            }
        }

        public void Shutdown()
        {
            lock (_lock)
            {
                foreach (var channel in _channelPool)
                {
                    channel.Dispose(); // Dispose is async-safe
                }
                _channelPool.Clear();
            }
        }
    }
}