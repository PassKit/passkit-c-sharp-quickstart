namespace Quickstart.Common;

public static class Constants
{
    public static string Environment => Get("PASSKIT_ENVIRONMENT", "pub1");
    public static string Address => Get("PASSKIT_ADDRESS", $"https://grpc.{Environment}.passkit.io");
    public static string CertificatePath => Get("PASSKIT_CERTIFICATE", "certs/certificate.pem");
    public static string PrivateKeyPath => Get("PASSKIT_PRIVATE_KEY", "certs/key.pem");
    public static string RootCertificatePath => Get("PASSKIT_ROOT_CERT", "certs/ca-chain.pem");
    public static string Passphrase => Get("PASSKIT_PASSPHRASE", "");
    public static string AppleCertificate => Get("PASSKIT_APPLE_CERTIFICATE", "");
    public static string EmailAddress => Get("PASSKIT_RECIPIENT_EMAIL", "");
    public static int PoolSize => int.TryParse(Get("PASSKIT_POOL_SIZE", "5"), out var value) && value > 0
        ? value
        : throw new InvalidOperationException("PASSKIT_POOL_SIZE must be a positive integer.");
    public static bool KeepAssets => bool.TryParse(Get("PASSKIT_KEEP_ASSETS", "false"), out var value) && value;

    public static void Validate()
    {
        if (Environment is not ("pub1" or "pub2"))
            throw new InvalidOperationException("PASSKIT_ENVIRONMENT must be pub1 or pub2.");
        if (!File.Exists(CertificatePath))
            throw new FileNotFoundException(
                $"PassKit client certificate not found at '{CertificatePath}'. See .env.example and README.md."
            );
        if (!UsesPkcs12 && !File.Exists(PrivateKeyPath))
            throw new FileNotFoundException(
                $"PassKit private key not found at '{PrivateKeyPath}'. See .env.example and README.md."
            );
        if (!File.Exists(RootCertificatePath))
            throw new FileNotFoundException(
                $"PassKit CA chain not found at '{RootCertificatePath}'. See .env.example and README.md."
            );
    }

    public static bool UsesPkcs12 => Path.GetExtension(CertificatePath).ToLowerInvariant() is ".pfx" or ".p12";

    private static string Get(string name, string fallback) =>
        System.Environment.GetEnvironmentVariable(name)?.Trim() is { Length: > 0 } value ? value : fallback;
}
