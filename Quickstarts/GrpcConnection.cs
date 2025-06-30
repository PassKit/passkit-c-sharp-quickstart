using System.Security.Cryptography.X509Certificates;
using Grpc.Net.Client;
using Quickstart.Common;

namespace GrpcConnection
{
    class GrpcConnection
    {
        public static GrpcChannel ConnectWithPassKitServer()
        {
            var host = $"https://grpc.{Constants.Environment}.passkit.io"; // HTTPS scheme required

            try
            {
                // Load client certificate (requires .pfx file, see note below)
                var clientCert = new X509Certificate2("certs/client.pfx", "", X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.Exportable);

                // Create HTTP handler with client certificate
                var handler = new HttpClientHandler();
                handler.ClientCertificates.Add(clientCert);

                // Optional: Skip server cert validation for local/dev (not recommended in prod)
                handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

                var httpClient = new HttpClient(handler);

                var channel = GrpcChannel.ForAddress(host, new GrpcChannelOptions
                {
                    HttpClient = httpClient
                });

                return channel;
            }
            catch (Exception e)
            {
                throw new Exception($"Failed to create gRPC channel with mutual TLS: {e.Message}", e);
            }
        }
    }
}
