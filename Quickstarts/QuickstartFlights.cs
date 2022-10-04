using PassKit.Grpc;
using PassKit.Grpc.Flights;
using Grpc.Core;

/* Quickstart Flight Tickets runs through the high level steps required to create flight tickets from scratch using the PassKit gRPC Java SDK. 
 */
namespace QuickstartFlightTickets
{
    class FlightTickets
    {

        /*
         * Stubs are used to access PassKit gRPC Functions. Blocking stubs can process
         * both unary and streaming server
         * responses, and therefore can be used with all current SDK methods. You are
         * free to modify this implementation
         * to add service, async or futures stubs for more efficient operations.
         */

        private static Flights.FlightsClient flightsStub;
        private static Templates.TemplatesClient templatesStub;
        private static String appleCertificate = "";

        /*
         * Quickstart will walk through the following steps:
         * - Modify default template for a regular flight ticket
         * - Modify default template for a vip flight ticket
         * - Create a flight
         * - Create an airport
         * - Create a basic ticket type
         * - Create a VIP ticket type
         * - Issue a basic ticket (auto create an event)
         * - Issue a VIP ticket
         * - Delete all flight assets
         * 
         */

        // Public objects for testing purposes   public static Image.ImageIds flightImageIds;
        public static PassKit.Grpc.Id templateId;
        public static BoardingPassesResponse pass;

        public void QuickStart(Channel channel)
        {
            createStubs(channel);
            createTemplates();
            createCarrier();
            createAirports();
            createFlight();
            createFlightDesignator();
            createBoardingPass();
            deleteFlightAssets();
            // always close the channel when there will be no further calls made.
            channel.ShutdownAsync().Wait();

        }

        private void createStubs(Channel channel)
        {
            templatesStub = new Templates.TemplatesClient(channel);
            flightsStub = new Flights.FlightsClient(channel);
        }

        private void createTemplates()
        {// Get default template
            Console.WriteLine("Creating base template");
            DefaultTemplateRequest templateRequest = new DefaultTemplateRequest();
            templateRequest.Protocol = PassProtocol.FlightProtocol;
            templateRequest.Revision = 1;
            PassTemplate defaultTemplate = templatesStub.getDefaultTemplate(templateRequest);

            // Modify the default template 
            defaultTemplate.Name = "Quickstart Boarding Pass";
            defaultTemplate.Description = "Quickstart Base Boarding Pass";
            defaultTemplate.Timezone = "Europe/London";

            templateId = templatesStub.createTemplate(defaultTemplate);
            Console.WriteLine("Created base template, base template id is " + templateId.Id_);
        }

        private void createAirports()
        {
            // Creates departure airport    
            Console.WriteLine("Creating departure airport");
            Port departureAirport = new Port();
            departureAirport.AirportName = "ABC Airport";
            departureAirport.CityName = "ABC";
            departureAirport.IataAirportCode = "YY4";
            departureAirport.IcaoAirportCode = "YYYY";
            departureAirport.CountryCode = "IE";
            departureAirport.Timezone = "Europe/London";

            flightsStub.createPort(departureAirport);
            Console.WriteLine("Departure airport created" + departureAirport);

            //Creates arrival airport
            Console.WriteLine("Creating arrival airport");
            Port arrivalAirport = new Port();
            arrivalAirport.AirportName = "DEF Airport";
            arrivalAirport.CityName = "DEF";
            arrivalAirport.IataAirportCode = "ADP";
            arrivalAirport.IcaoAirportCode = "ADPY";
            arrivalAirport.CountryCode = "HK";
            arrivalAirport.Timezone = "Asia/Hong_Kong";

            flightsStub.createPort(arrivalAirport);
            Console.WriteLine("Arrival airport created" + arrivalAirport);

        }

        private void createCarrier()
        {  // Creates carrier
            Console.WriteLine("Creating carrier");
            Carrier carrier = new Carrier();
            carrier.AirlineName = "ABC Airline";
            carrier.IataCarrierCode = "YY";
            carrier.PassTypeIdentifier = appleCertificate;

            flightsStub.createCarrier(carrier);
            Console.WriteLine("Carrier created" + carrier);
        }

        private void createFlightDesignator()
        {   // Creates flight deesignator
            Time departureTime = new Time(); departureTime.Hour = 13; departureTime.Minute = 00; departureTime.Second = 00;
            Time arrivalTime = new Time(); arrivalTime.Hour = 14; arrivalTime.Minute = 00; arrivalTime.Second = 00;
            Time gateTime = new Time(); arrivalTime.Hour = 12; arrivalTime.Minute = 30; arrivalTime.Second = 00;
            Time boardingTime = new Time(); boardingTime.Hour = 12; boardingTime.Minute = 15; boardingTime.Second = 00;

            Console.WriteLine("Creating flight designator");
            FlightDesignator flightDesignator = new FlightDesignator();
            flightDesignator.CarrierCode = "YY";
            flightDesignator.FlightNumber = "YY123";
            flightDesignator.Revision = 2;
            flightDesignator.Schedule.Monday.ScheduledDepartureTime = departureTime;
            flightDesignator.Schedule.Monday.ScheduledArrivalTime = arrivalTime;
            flightDesignator.Schedule.Monday.GateClosingTime = gateTime;
            flightDesignator.Schedule.Monday.BoardingTime = boardingTime;
            flightDesignator.Origin = "YY4";
            flightDesignator.Destination = "ADP";
            flightDesignator.PassTemplateId = templateId.Id_;
            flightsStub.createFlightDesignator(flightDesignator);
            Console.WriteLine("Flight designator created " + flightDesignator);
        }

        private void createFlight()
        {// Create flight
            Console.WriteLine("Creating flight");

            LocalDateTime flightDateTime = new LocalDateTime();
            flightDateTime.DateTime = "2022-04-28T18:00:00";
            Flight flight = new Flight();
            flight.CarrierCode = "YY";
            flight.FlightNumber = "YY123";
            flight.BoardingPoint = "YY4";
            flight.DeplaningPoint = "ADP";
            flight.DepartureDate.Day = 28;
            flight.DepartureDate.Month = 4;
            flight.DepartureDate.Year = 2022;
            flight.ScheduledDepartureTime = flightDateTime;
            flight.PassTemplateId = templateId.Id_;

            flightsStub.createFlight(flight);
            Console.WriteLine("Created flight" + flight);
        }

        private void createBoardingPass()
        {//Create boarding pass
            Console.WriteLine("Creating boarding pass");
            BoardingPassRecord boardingPassRecord = new BoardingPassRecord();
            boardingPassRecord.OperatingCarrierPNR = "P8F8R8";
            boardingPassRecord.BoardingPoint = "YY4";
            boardingPassRecord.DeplaningPoint = "ADP";
            boardingPassRecord.CarrierCode = "YY";
            boardingPassRecord.FlightNumber = "YY123";
            boardingPassRecord.DepartureDate.Day = 28;
            boardingPassRecord.DepartureDate.Month = 4;
            boardingPassRecord.DepartureDate.Year = 2022;
            boardingPassRecord.Passenger.PassengerDetails.Forename = "John";
            boardingPassRecord.Passenger.PassengerDetails.Surname = "Smith";
            boardingPassRecord.SequenceNumber = 123;

            pass = flightsStub.createBoardingPass(boardingPassRecord);
            Console.WriteLine("Boarding pass created, boarding pass url: https://pub1.pskt.io/" + pass.BoardingPasses);
        }
        private void deleteFlightAssets()
        { // Deletes all flight objects created
            Console.WriteLine("Deleting flight");
            FlightRequest flightRequest = new FlightRequest();
            flightRequest.CarrierCode = "YY";
            flightRequest.BoardingPoint = "YY4";
            flightRequest.FlightNumber = "YY123";
            flightRequest.DeplaningPoint = "ADP";
            flightsStub.deleteFlight(flightRequest);
            Console.WriteLine("Flight deleted");

            Console.WriteLine("Deleting flight designator");
            FlightDesignatorRequest flightDesignator = new FlightDesignatorRequest();
            flightDesignator.CarrierCode = "YY";
            flightDesignator.FlightNumber = "YY123";
            flightsStub.deleteFlightDesignator(flightDesignator);
            Console.WriteLine("Deleted flight designator");

            Console.WriteLine("Deleting airports");
            AirportCode departureAirport = new AirportCode();
            departureAirport.AirportCode_ = "YY4";
            flightsStub.deletePort(departureAirport);
            AirportCode arrivalAirport = new AirportCode();
            arrivalAirport.AirportCode_ = "ADP";
            flightsStub.deletePort(arrivalAirport);
            Console.WriteLine("Deleted airports");

            Console.WriteLine("Deleting carrier");
            CarrierCode carrier = new CarrierCode();
            carrier.CarrierCode_ = "YY";
            flightsStub.deleteCarrier(carrier);
            Console.WriteLine("Deleted Carrier");

            Console.WriteLine("Deleting template");
            templatesStub.deleteTemplate(templateId);
            Console.WriteLine("Deleted template");
        }
    }
}