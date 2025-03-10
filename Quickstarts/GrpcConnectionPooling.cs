using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Grpc.Core;

namespace GrpcConnectionPool
{
    class GrpcConnectionPool
    {
        private static readonly object _lock = new object();
        private static GrpcConnectionPool _instance;
        private readonly List<Channel> _channelPool;
        private int _currentIndex;
        private readonly int _poolSize;

        private GrpcConnectionPool(int poolSize = 5)
        {
            _poolSize = poolSize;
            _channelPool = new List<Channel>();

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

        private Channel CreateChannel()
        {
            var host = "grpc.pub1.passkit.io";
            var port = 443;

            try
            {
                var rootCert = File.ReadAllText("certs/ca-chain.pem");
                var keyFile = File.ReadAllText("certs/key.pem");
                var certFile = File.ReadAllText("certs/certificate.pem");

                var keyCertPair = new KeyCertificatePair(certFile, keyFile);
                var credentials = new SslCredentials(rootCert, keyCertPair);

                return new Channel(host, port, credentials);
            }
            catch (IOException e)
            {
                throw new Exception($"Failed to read certificate files: {e.Message}", e);
            }
            catch (RpcException e)
            {
                throw new RpcException(new Status(e.Status.StatusCode, e.Status.Detail));
            }
        }

        public Channel GetChannel()
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
                    channel.ShutdownAsync().Wait();
                }
                _channelPool.Clear();
            }
        }
    }
}
