// Creates channel for connecting to PassKit server
//Single Connection
//var channel = GrpcConnection.GrpcConnection.ConnectWithPassKitServer();
//gRPC Connection Pooling
var loyaltyChannel = GrpcConnectionPool.GrpcConnectionPool.GetInstance().GetChannel(); // Get a connection from the pool
var couponChannel = GrpcConnectionPool.GrpcConnectionPool.GetInstance().GetChannel(); // Get a connection from the pool
var flightChannel = GrpcConnectionPool.GrpcConnectionPool.GetInstance().GetChannel(); // Get a connection from the pool
var ticketChannel = GrpcConnectionPool.GrpcConnectionPool.GetInstance().GetChannel(); // Get a connection from the pool


// Loyalty Quickstart
QuickstartLoyalty.Membership buildLoyalty = new QuickstartLoyalty.Membership();
buildLoyalty.Quickstart(loyaltyChannel);

// Coupons Quickstart
QuickstartCoupons.Coupons buildCoupons = new QuickstartCoupons.Coupons();
buildCoupons.Quickstart(couponChannel);

// Flight Quickstart
QuickstartFlightTickets.FlightTickets buildFlights = new QuickstartFlightTickets.FlightTickets();
buildFlights.QuickStart(flightChannel);

// Event Tickets Quickstart
QuickstartEventickets.EventTicket buildEventTickets = new QuickstartEventickets.EventTicket();
buildEventTickets.QuickStart(ticketChannel);
