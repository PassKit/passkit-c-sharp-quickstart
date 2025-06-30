// Creates channel for connecting to PassKit server
// Single Connection (for testing only)
// var channel = GrpcConnection.GrpcConnection.ConnectWithPassKitServer();



// gRPC Connection Pooling
var loyaltyChannel = GrpcConnectionPool.GrpcConnectionPool.GetInstance().GetChannel(); // Get a connection from the pool
var couponChannel = GrpcConnectionPool.GrpcConnectionPool.GetInstance().GetChannel(); // Get a connection from the pool
var flightChannel = GrpcConnectionPool.GrpcConnectionPool.GetInstance().GetChannel(); // Get a connection from the pool
var ticketChannel = GrpcConnectionPool.GrpcConnectionPool.GetInstance().GetChannel(); // Get a connection from the pool

// Flight Quickstart
QuickstartFlightTickets.FlightTickets buildFlights = new();
buildFlights.QuickStart(flightChannel);

// Loyalty Quickstart
QuickstartLoyalty.Membership buildLoyalty = new();
buildLoyalty.Quickstart(loyaltyChannel);

// Coupons Quickstart
QuickstartCoupons.Coupons buildCoupons = new();
buildCoupons.Quickstart(couponChannel);

// Event Tickets Quickstart
QuickstartEventickets.EventTicket buildEventTickets = new();
buildEventTickets.QuickStart(ticketChannel);
