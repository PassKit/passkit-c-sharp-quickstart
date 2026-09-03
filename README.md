# PassKit C# Quickstart

[![CI](https://github.com/PassKit/passkit-c-sharp-quickstart/actions/workflows/ci.yml/badge.svg)](https://github.com/PassKit/passkit-c-sharp-quickstart/actions/workflows/ci.yml)
[![NuGet](https://img.shields.io/nuget/v/PassKit.Grpc.DotNet)](https://www.nuget.org/packages/PassKit.Grpc.DotNet)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

Create a working PassKit membership card, coupon, event ticket, or flight boarding pass with the official .NET SDK. You can choose an example from a simple menu—no source-code changes are required.

The examples print a wallet pass URL and automatically remove the test resources they create.

## Quick start

You need:

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or a newer supported .NET SDK/runtime
- a free [PassKit account](https://app.passkit.com/signup)
- PassKit SDK credentials

### 1. Download this project

```sh
git clone https://github.com/PassKit/passkit-c-sharp-quickstart.git
cd passkit-c-sharp-quickstart
dotnet restore c-sharp-quickstart.csproj
```

You can also download the repository as a ZIP from GitHub and open a terminal in the extracted folder.

### 2. Get your PassKit credentials

1. Sign in to [PassKit](https://app.passkit.com).
2. Open **Developer Tools** from the account menu.
3. Under **Account Credentials**, select **SDK Credentials**.
4. Choose a strong password when prompted. This encrypts your private key; it is not your PassKit account password.
5. Download the three files sent to your registered email address:

   - `certificate.pem`
   - `key.pem`
   - `ca-chain.pem`

Keep the password safe. PassKit does not store your private key or certificate. Generating another credential set invalidates the existing credentials.

### 3. Add the credential files

Create a `certs` folder and copy all three files into it.

macOS or Linux:

```sh
mkdir -p certs
cp /path/to/downloads/certificate.pem certs/
cp /path/to/downloads/key.pem certs/
cp /path/to/downloads/ca-chain.pem certs/
cp .env.example .env
```

Windows PowerShell:

```powershell
New-Item -ItemType Directory -Force certs
Copy-Item "$HOME\Downloads\certificate.pem" certs\
Copy-Item "$HOME\Downloads\key.pem" certs\
Copy-Item "$HOME\Downloads\ca-chain.pem" certs\
Copy-Item .env.example .env
```

Your folder should look like this:

```text
passkit-c-sharp-quickstart/
├── .env
└── certs/
    ├── ca-chain.pem
    ├── certificate.pem
    └── key.pem
```

These files and `.env` are ignored by Git. Never commit or share them.

### 4. Complete `.env`

Open `.env` in a text editor. Add the password you chose when generating the SDK credentials:

```dotenv
PASSKIT_ENVIRONMENT=pub1
PASSKIT_CERTIFICATE=certs/certificate.pem
PASSKIT_PRIVATE_KEY=certs/key.pem
PASSKIT_ROOT_CERT=certs/ca-chain.pem
PASSKIT_PASSPHRASE=replace-with-your-sdk-credential-password
PASSKIT_POOL_SIZE=5
PASSKIT_RECIPIENT_EMAIL=
PASSKIT_APPLE_CERTIFICATE=
PASSKIT_KEEP_ASSETS=false
```

Check **Developer Tools → API Region** in PassKit:

- use `pub1` for the European server
- use `pub2` for the US server

Your PassKit account and its data belong to one region, so this value must match your account. The quickstart loads `.env` automatically.

### 5. Run the quickstart

```sh
dotnet run --project c-sharp-quickstart.csproj
```

Choose an example from the menu:

```text
Choose an example:
  1. loyalty
  2. coupons
  3. tickets
  4. flights
Selection [1]:
```

You can also run an example directly:

```sh
dotnet run --project c-sharp-quickstart.csproj -- loyalty
dotnet run --project c-sharp-quickstart.csproj -- coupons
dotnet run --project c-sharp-quickstart.csproj -- tickets
dotnet run --project c-sharp-quickstart.csproj -- flights
```

When successful, the quickstart prints a URL similar to:

```text
https://pub1.pskt.io/4MEIqDFudziP4ZFKx5osw3
```

Open the URL on a phone to add the pass to Apple Wallet or Google Wallet. On a desktop, the PassKit page displays a QR code you can scan with a phone.

## Keeping the generated resources

By default, cleanup runs even when an example fails partway through. To inspect the generated records in your PassKit account, change this setting in `.env`:

```dotenv
PASSKIT_KEEP_ASSETS=true
```

You will then need to delete those resources manually. Change it back to `false` for normal quickstart use.

## Running the flights example

Flights require an Apple pass certificate uploaded to PassKit. Add its pass type identifier to `.env`:

```dotenv
PASSKIT_APPLE_CERTIFICATE=pass.com.example.airline
```

If this value is empty, the flights example explains the requirement and exits without creating resources.

## What the examples demonstrate

- `loyalty`: templates, a program, tiers, enrolment, external-ID lookup, check-in/out, and earning and burning points
- `coupons`: templates, a campaign, offers, coupons, lookup, redemption, and voiding
- `tickets`: a template, production, venue, event, ticket type, issuance, validation, and redemption
- `flights`: a template, carrier, airports, flight, scheduled designator, semantic boarding-pass fields, and passenger and infant passes

## Configuration reference

Most users only need to set the region and passphrase.

| Variable | Default | Purpose |
| --- | --- | --- |
| `PASSKIT_ENVIRONMENT` | `pub1` | `pub1` for Europe or `pub2` for USA |
| `PASSKIT_ADDRESS` | Derived from the region | Optional full PassKit gRPC HTTPS address |
| `PASSKIT_CERTIFICATE` | `certs/certificate.pem` | PEM certificate path; `.pfx` and `.p12` also work |
| `PASSKIT_PRIVATE_KEY` | `certs/key.pem` | PEM private-key path |
| `PASSKIT_ROOT_CERT` | `certs/ca-chain.pem` | PassKit CA chain used for TLS validation |
| `PASSKIT_PASSPHRASE` | Empty | SDK private-key passphrase, or PKCS #12 password |
| `PASSKIT_POOL_SIZE` | `5` | Number of reusable gRPC channels |
| `PASSKIT_RECIPIENT_EMAIL` | Empty | Optional welcome-email recipient |
| `PASSKIT_APPLE_CERTIFICATE` | Empty | Apple pass type identifier for flights |
| `PASSKIT_KEEP_ASSETS` | `false` | Preserve generated PassKit resources |

Values already set in your terminal take precedence over `.env`.

## Troubleshooting

### A credential file cannot be found

Confirm that all three `.pem` files are inside `certs/`, that their names have not changed, and that you are running the command from the repository folder.

### The private key or certificate cannot be loaded

Check `PASSKIT_PASSPHRASE`. It must be the password chosen when generating **SDK Credentials**, not your PassKit login password. Generate a new credential set if the password or files have been lost, then replace all three local files together.

### Authentication or connection fails

Check your API region under **Developer Tools → API Region**. Use `pub1` for Europe or `pub2` for the US. Also check whether a corporate firewall or VPN blocks outbound HTTPS/gRPC traffic.

### The pass URL uses the wrong region

The pass URL begins with `https://pub1.pskt.io/` for Europe or `https://pub2.pskt.io/` for the US. Correct `PASSKIT_ENVIRONMENT` in `.env` and rerun the example.

### No credentials email arrived

Check spam and quarantine folders. Corporate mail systems may block certificate attachments. Follow the guidance in the PassKit help centre or contact PassKit support if the email is still missing.

### Flights are skipped

Upload an Apple pass certificate to PassKit and set `PASSKIT_APPLE_CERTIFICATE` to its pass type identifier.

### Resources remain after a failed run

The quickstart attempts each cleanup operation independently and reports anything it cannot delete. Remove reported resources in the PassKit portal, then verify that `PASSKIT_KEEP_ASSETS=false`.

## Using the wider API

[`Quickstarts/PassKitApi.cs`](Quickstarts/PassKitApi.cs) groups the strongly typed SDK clients in one discoverable place. It covers loyalty, coupons, event tickets, flights, templates, images, analytics, SmartPass distribution and messaging, integrations/webhooks, scanner configuration, certificates, and raw passes.

```csharp
using Quickstart.Common;

using var pool = new GrpcConnectionPool.GrpcConnectionPool();
var api = new PassKitApi(pool.GetChannel());

var program = await api.Loyalty.getProgramAsync(programId);
var template = await api.Templates.getTemplateAsync(templateId);
```

The SDK provides strongly typed protobuf request and response objects and synchronous and asynchronous methods. Keep account deletion, password/API-secret management, team administration, certificate mutation, NFC credentials, and broad bulk operations in dedicated, access-controlled administrative tools.

### Node API parity

The test suite checks the complete Node quickstart facade method list against SDK 1.1.162. [`Tests/ApiParityTests.cs`](Tests/ApiParityTests.cs) fails if a corresponding C# RPC is missing, including membership batch updates and distribution messaging methods.

The methods introduced in SDK 1.1.162 also have typed convenience helpers:

```csharp
await api.AddMessageAsync(message);
var messages = await api.GetMessagesAsync();
await api.CancelMessageAsync(messageId);

// Bulk changes require an explicit safety opt-in.
await api.BatchUpdateMembersAsync(batchRequest, allowBulkOperation: true);
```

## Tests and project checks

These checks do not connect to PassKit or require credentials:

```sh
dotnet build c-sharp-quickstart.csproj --configuration Release
dotnet test Tests/c-sharp-quickstart.Tests.csproj --configuration Release
dotnet format c-sharp-quickstart.csproj --verify-no-changes
dotnet list c-sharp-quickstart.csproj package --vulnerable --include-transitive
```

CI runs the same offline checks without receiving PassKit credentials.

## Documentation and support

- [Getting started as a developer](https://help.passkit.com/en/articles/5360063-how-to-get-started-as-a-developer)
- [Working with the pass URL](https://help.passkit.com/en/articles/11891934-working-with-the-pass-url)
- [Membership API](https://docs.passkit.io/protocols/member)
- [Coupons API](https://docs.passkit.io/protocols/coupon)
- [Event Tickets API](https://docs.passkit.io/protocols/event-tickets)
- [Boarding Pass API](https://docs.passkit.io/protocols/boarding)
- [PassKit support](mailto:support@passkit.com)
