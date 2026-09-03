using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using Grpc.Net.Client;
using Quickstart.Common;

namespace GrpcConnection;

internal static class GrpcConnection
{
    public static GrpcChannel ConnectWithPassKitServer()
    {
        Constants.Validate();
        var clientCertificate = LoadClientCertificate();
        var handler = new HttpClientHandler();
        handler.ClientCertificates.Add(clientCertificate);
        handler.ServerCertificateCustomValidationCallback = ValidateServerCertificate;

        return GrpcChannel.ForAddress(Constants.Address, new GrpcChannelOptions { HttpHandler = handler });
    }

    private static bool ValidateServerCertificate(
        HttpRequestMessage _,
        X509Certificate2? certificate,
        X509Chain? __,
        SslPolicyErrors errors
    )
    {
        if (certificate is null) return false;
        if (errors == SslPolicyErrors.None) return true;
        if ((errors & ~SslPolicyErrors.RemoteCertificateChainErrors) != 0) return false;

        var trustedCertificates = new X509Certificate2Collection();
        trustedCertificates.ImportFromPemFile(Constants.RootCertificatePath);
        using var customChain = new X509Chain();
        customChain.ChainPolicy.TrustMode = X509ChainTrustMode.CustomRootTrust;
        customChain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
        customChain.ChainPolicy.CustomTrustStore.AddRange(trustedCertificates);
        return customChain.Build(certificate);
    }

    private static X509Certificate2 LoadClientCertificate()
    {
        if (Constants.UsesPkcs12)
            return new X509Certificate2(
                Constants.CertificatePath,
                Constants.Passphrase,
                X509KeyStorageFlags.Exportable
            );

        return string.IsNullOrEmpty(Constants.Passphrase)
            ? X509Certificate2.CreateFromPemFile(Constants.CertificatePath, Constants.PrivateKeyPath)
            : X509Certificate2.CreateFromEncryptedPemFile(
                Constants.CertificatePath,
                Constants.Passphrase,
                Constants.PrivateKeyPath
            );
    }
}
