
using Grpc.Core;


namespace GrpcConnection
{
    class GrpcConnection
    {

        private static SslCredentials buildSslCredentials(string host, int port, string rootCert, string keyFile, string certFile)
        {
            var keyCertPair = new KeyCertificatePair(certFile, keyFile);
            return new SslCredentials(rootCert, keyCertPair);
        }

        public static Channel ConnectWithPassKitServer()
        {
            var port = 443;
            var host = "grpc.pub1.passkit.io";

            try
            {
                var rootCert = File.ReadAllText("certs/ca-chain.pem");
                var keyFile = File.ReadAllText("certs/key.pem");
                var certFile = File.ReadAllText("certs/certificate.pem");
                SslCredentials credentials = buildSslCredentials(host, port, rootCert, keyFile, certFile);
                var channel = new Channel(host, port, credentials);
                return channel;
            }
            catch (RpcException e)
            {
                throw new RpcException(new Status(e.Status.StatusCode, e.Status.Detail));
            }
        }
    }
}