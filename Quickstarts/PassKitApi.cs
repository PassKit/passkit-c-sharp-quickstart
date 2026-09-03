using Google.Protobuf.WellKnownTypes;
using Grpc.Net.Client;
using PassKit.Grpc.DotNet;
using PassKit.Grpc.DotNet.Analytics;
using PassKit.Grpc.DotNet.EventTickets;
using PassKit.Grpc.DotNet.Flights;
using PassKit.Grpc.DotNet.Members;
using PassKit.Grpc.DotNet.Raw;
using PassKit.Grpc.DotNet.SingleUseCoupons;

namespace Quickstart.Common;

/// <summary>
/// A discoverable entry point for the PassKit service clients. Request and response
/// messages remain the strongly typed protobuf classes supplied by the official SDK.
/// </summary>
public sealed class PassKitApi
{
    public PassKitApi(GrpcChannel channel)
    {
        Loyalty = new Members.MembersClient(channel);
        Coupons = new SingleUseCoupons.SingleUseCouponsClient(channel);
        EventTickets = new EventTickets.EventTicketsClient(channel);
        Flights = new Flights.FlightsClient(channel);
        Templates = new Templates.TemplatesClient(channel);
        Images = new Images.ImagesClient(channel);
        Analytics = new Analytics.AnalyticsClient(channel);
        Distribution = new Distribution.DistributionClient(channel);
        Integrations = new Integrations.IntegrationsClient(channel);
        Users = new Users.UsersClient(channel);
        Certificates = new Certificates.CertificatesClient(channel);
        RawPasses = new Raw.RawClient(channel);
        Messages = new Messages.MessagesClient(channel);
    }

    public Members.MembersClient Loyalty { get; }
    public SingleUseCoupons.SingleUseCouponsClient Coupons { get; }
    public EventTickets.EventTicketsClient EventTickets { get; }
    public Flights.FlightsClient Flights { get; }
    public Templates.TemplatesClient Templates { get; }
    public Images.ImagesClient Images { get; }
    public Analytics.AnalyticsClient Analytics { get; }
    public Distribution.DistributionClient Distribution { get; }
    public Integrations.IntegrationsClient Integrations { get; }
    public Users.UsersClient Users { get; }
    public Certificates.CertificatesClient Certificates { get; }
    public Raw.RawClient RawPasses { get; }
    public Messages.MessagesClient Messages { get; }

    public Task<Empty> BatchUpdateMembersAsync(
        BatchUpdateRequest request,
        bool allowBulkOperation = false,
        CancellationToken cancellationToken = default
    )
    {
        if (!allowBulkOperation)
            throw new InvalidOperationException(
                "Batch member updates are disabled by default. Pass allowBulkOperation: true after reviewing the request."
            );

        return Loyalty.batchUpdateAsync(request, cancellationToken: cancellationToken).ResponseAsync;
    }

    public Task<Empty> AddMessageAsync(Message request, CancellationToken cancellationToken = default) =>
        Distribution.addMessageAsync(request, cancellationToken: cancellationToken).ResponseAsync;

    public Task<List<Message>> GetMessagesAsync(CancellationToken cancellationToken = default) =>
        Distribution.getMessages(new Empty(), cancellationToken: cancellationToken)
            .ToListAsync(cancellationToken);

    public Task<Empty> CancelMessageAsync(Id request, CancellationToken cancellationToken = default) =>
        Distribution.cancelMessageAsync(request, cancellationToken: cancellationToken).ResponseAsync;
}
