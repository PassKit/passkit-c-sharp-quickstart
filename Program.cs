// Creates channel for connecting to PassKit server
var channel = GrpcConnection.GrpcConnection.ConnectWithPassKitServer();

// Loyalty Quickstart
QuickstartLoyalty.Membership buildLoyalty = new QuickstartLoyalty.Membership();
buildLoyalty.Quickstart(channel);

// Coupons Quickstart
QuickstartCoupons.Coupons buildCoupons = new QuickstartCoupons.Coupons();
buildCoupons.Quickstart(channel);

// Flight Quickstart
QuickstartFlightTickets.FlightTickets buildFlights = new QuickstartFlightTickets.FlightTickets();
buildFlights.QuickStart(channel);

// Event Tickets Quickstart
QuickstartEventickets.EventTicket buildEventTickets = new QuickstartEventickets.EventTicket();
buildEventTickets.QuickStart(channel);
