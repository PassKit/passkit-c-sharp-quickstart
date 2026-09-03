using Quickstart.Common;

EnvFile.Load();

var examples = new Dictionary<string, Action<Grpc.Net.Client.GrpcChannel>>(StringComparer.OrdinalIgnoreCase)
{
    ["loyalty"] = channel => new QuickstartLoyalty.Membership().Quickstart(channel),
    ["coupons"] = channel => new QuickstartCoupons.Coupons().Quickstart(channel),
    ["tickets"] = channel => new QuickstartEventickets.EventTicket().QuickStart(channel),
    ["flights"] = channel => new QuickstartFlightTickets.FlightTickets().QuickStart(channel)
};

if (args.FirstOrDefault() is "--help" or "-h")
{
    Console.WriteLine("Usage: dotnet run -- <loyalty|coupons|tickets|flights>");
    return 0;
}

var exampleName = args.FirstOrDefault();
if (exampleName is null && !Console.IsInputRedirected)
{
    Console.WriteLine("Choose an example:");
    var names = examples.Keys.ToArray();
    for (var index = 0; index < names.Length; index++)
        Console.WriteLine($"  {index + 1}. {names[index]}");
    Console.Write("Selection [1]: ");
    var selection = Console.ReadLine();
    exampleName = int.TryParse(selection, out var number) && number >= 1 && number <= names.Length
        ? names[number - 1]
        : names[0];
}
exampleName ??= "loyalty";
if (!examples.TryGetValue(exampleName, out var run))
{
    Console.Error.WriteLine($"Unknown example '{exampleName}'. Choose: {string.Join(", ", examples.Keys)}");
    return 2;
}

try
{
    Constants.Validate();
    using var pool = new GrpcConnectionPool.GrpcConnectionPool();
    Console.WriteLine($"Running the {exampleName} quickstart against {Constants.Environment}...");
    run(pool.GetChannel());
    return 0;
}
catch (Exception exception)
{
    Console.Error.WriteLine($"Quickstart failed: {exception.Message}");
    return 1;
}
