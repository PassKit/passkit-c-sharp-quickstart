using PassKit.Grpc.DotNet;
using PassKit.Grpc.DotNet.Analytics;
using PassKit.Grpc.DotNet.EventTickets;
using PassKit.Grpc.DotNet.Flights;
using PassKit.Grpc.DotNet.Members;
using PassKit.Grpc.DotNet.Raw;
using PassKit.Grpc.DotNet.SingleUseCoupons;
using Xunit;

namespace Quickstart.Tests;

public class ApiParityTests
{
    public static IEnumerable<object[]> NodeFacadeMethods()
    {
        yield return Domain(typeof(Members.MembersClient),
            "batchUpdate", "bulkDeleteMembers", "burnPoints", "changeMemberTier", "checkInMember",
            "checkOutMember", "copyProgram", "countMemberEvents", "countMembers", "createProgram",
            "createTier", "deleteEventsForMember", "deleteMember", "deleteMemberEvent",
            "deleteMembersBySegment", "deleteProgram", "deleteTier", "earnPoints", "enrolMember",
            "getMemberEventMetaKeysForProgram", "getMemberRecordByExternalId", "getMemberRecordById",
            "getMessageHistoryForMember", "getMetaKeysForProgram", "getProgram", "getProgramEnrolment",
            "getTier", "listMemberEvents", "listMembers", "listPrograms", "listTiers", "patchPerson",
            "renewMembersExpiry", "setPoints", "updateMember", "updateMemberExpiry",
            "updateMembersBySegment", "updateProgram", "updateTier");
        yield return Domain(typeof(SingleUseCoupons.SingleUseCouponsClient),
            "bulkVoidCoupons", "copyCouponCampaign", "countCouponsByCouponCampaign", "createCoupon",
            "createCouponCampaign", "createCouponOffer", "deleteCouponCampaign", "deleteCouponOffer",
            "getAnalytics", "getCouponByExternalId", "getCouponById", "getCouponCampaign",
            "getCouponOffer", "getMetaKeysForCampaign", "listCouponCampaigns", "listCouponOffers",
            "listCouponsByCouponCampaign", "patchPerson", "redeemCoupon", "streamCouponRedemptions",
            "streamCouponUpdates", "updateCoupon", "updateCouponCampaign", "updateCouponExternalId",
            "updateCouponOffer", "voidCoupon");
        yield return Domain(typeof(EventTickets.EventTicketsClient),
            "bulkDeleteTickets", "copyProduction", "countTickets", "createEvent", "createProduction",
            "createTicketType", "createVenue", "deleteEvent", "deleteProduction", "deleteTicket",
            "deleteTicketsByOrderNumber", "deleteTicketType", "deleteVenue", "getAnalytics",
            "getEventById", "getEventByStartDateAndVenue", "getEventTicketPass", "getProduction",
            "getTicketById", "getTicketByTicketNumber", "getTicketsByOrderNumber", "getTicketTypeById",
            "getTicketTypeByUserDefinedId", "getVenueById", "issueTicket", "issueTicketById",
            "listEvents", "listProductions", "listTickets", "listTicketTypes", "listVenues", "patchEvent",
            "patchPerson", "patchProduction", "patchTicketType", "patchVenue", "redeemTicket",
            "redeemTicketsByOrderNumber", "updateEvent", "updateProduction", "updateTicket",
            "updateTicketType", "updateVenue", "validateTicket");
        yield return Domain(typeof(Flights.FlightsClient),
            "createBoardingPass", "createCarrier", "createFlight", "createFlightDesignator", "createPort",
            "deleteBoardingPass", "deleteCarrier", "deleteFlight", "deleteFlightDesignator", "deletePort",
            "getBoardingPass", "getBoardingPassRecord", "getCarrier", "getFlight", "getFlightDesignator",
            "getPort", "updateBoardingPass", "updateCarrier", "updateFlight", "updateFlightDesignator",
            "updatePort");
        yield return Domain(typeof(Templates.TemplatesClient),
            "copyBeacon", "copyLink", "copyLocation", "copyTemplate", "countBeacons", "countLinks",
            "countLocations", "countTemplates", "createBeacon", "createLink", "createLocation",
            "createTemplate", "deleteBeacon", "deleteLink", "deleteLocation", "deleteTemplate", "getBeacon",
            "getDefaultTemplate", "getLink", "getLocation", "getTemplate", "listBeacons", "listLinks",
            "listLocations", "listTemplates", "updateBeacon", "updateLink", "updateLocation", "updateTemplate");
        yield return Domain(typeof(Images.ImagesClient),
            "countImages", "createImages", "deleteImage", "deleteLocalizedImage", "getImageBundle",
            "getImageData", "getImageURL", "getLocalizedImageURL", "getProfileImage", "getProfileImageById",
            "getStampImageConfig", "getStampImagePreview", "getStampImageURL", "listImages", "setProfileImage",
            "updateImage", "updateStampImageConfig");
        yield return Domain(typeof(Analytics.AnalyticsClient), "getAnalytics");
        yield return Domain(typeof(Distribution.DistributionClient),
            "addMessage", "cancelMessage", "getDataCollectionPageFields", "getMessage", "getMessages",
            "getSmartPassLink", "sendWelcomeEmail", "updateMessage", "validateBarcode");
        yield return Domain(typeof(Messages.MessagesClient),
            "createMessage", "deleteMessage", "getMessage", "sendMessage", "updateMessage");
        yield return Domain(typeof(Integrations.IntegrationsClient),
            "createSinkSubscription", "deleteSinkSubscription", "getSampleSubscriptionEvent",
            "getSinkSubscription", "listSinkSubscriptions", "updateSinkSubscription");
        yield return Domain(typeof(Users.UsersClient),
            "createScannerConfig", "getScannerConfig", "updateScannerConfig");
        yield return Domain(typeof(Certificates.CertificatesClient),
            "countAppleCertificates", "getAppleCertificateData", "listAppleCertificates");
        yield return Domain(typeof(Raw.RawClient),
            "copyPassProject", "createPass", "createPassProject", "deletePass", "deletePassProject",
            "getPassByExternalId", "getPassById", "getPassProject", "listPassesByPassProject",
            "listPassesByPassTemplate", "streamPassUpdates", "updatePass", "updatePassProject");
    }

    [Theory]
    [MemberData(nameof(NodeFacadeMethods))]
    public void CSharpSdkContainsEveryNodeFacadeMethod(Type clientType, string[] methodNames)
    {
        var available = clientType.GetMethods().Select(method => method.Name).ToHashSet();
        var missing = methodNames.Where(name => !available.Contains(name)).ToArray();
        Assert.True(missing.Length == 0, $"{clientType.Name} is missing: {string.Join(", ", missing)}");
    }

    private static object[] Domain(Type clientType, params string[] methods) => [clientType, methods];

    [Fact]
    public void PassKitApiProvidesTypedHelpersForNewSdkMethods()
    {
        var methods = typeof(Quickstart.Common.PassKitApi)
            .GetMethods()
            .Select(method => method.Name)
            .ToHashSet();
        Assert.Contains("BatchUpdateMembersAsync", methods);
        Assert.Contains("AddMessageAsync", methods);
        Assert.Contains("GetMessagesAsync", methods);
        Assert.Contains("CancelMessageAsync", methods);
    }

}
