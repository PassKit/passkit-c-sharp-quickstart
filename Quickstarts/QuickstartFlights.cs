using Grpc.Net.Client;
using PassKit.Grpc.DotNet;
using PassKit.Grpc.DotNet.Flights;
using Quickstart.Common;
using System;
using System.Threading;

/* Quickstart Flight Tickets runs through the high level steps required to create flight tickets from scratch using the PassKit gRPC Java SDK. 
 */
namespace QuickstartFlightTickets
{
    public class FlightTickets
    {

        /*
         * Stubs are used to access PassKit gRPC Functions. Blocking stubs can process
         * both unary and streaming serverresponses, and therefore can be used with all 
         * current SDK methods. You are free to modify this implementation
         * to add service, async or futures stubs for more efficient operations.
         */

        private static Flights.FlightsClient? flightsStub;
        private static Templates.TemplatesClient? templatesStub;
        private static readonly string carrierCode = "PP";
        private static readonly string departureAirportCode = "DUB";
        private static readonly string departureICAOAirportCode = "EIDW";
        private static readonly string arrivalAirportCode = "BKK";
        private static readonly string arrivalICAOAirportCode = "VTBS";
        private static readonly string flightNumber = "888";
        private static readonly int sequenceNumber = 88;

        /*
         * Quickstart will walk through the following steps:
         * - Modify default template for a regular flight ticket
         * - Modify default template for a vip flight ticket
         * - Create a flight
         * - Create an airport
         * - Create a flight designator
         * - Create a flight
         * - Issue a boarding pass
         * - Delete all flight assets
         */

        // Public objects for testing purposes 
        public static Id? templateId;
        public static BoardingPassesResponse? pass;


        public void QuickStart(GrpcChannel channel)
        {
            if (Constants.AppleCertificate == "")
            {
                Console.WriteLine("Flights tests require an Apple Certificate. Skipping...");
                return;
            }
            CreateStubs(channel);
            CreateTemplates();
            CreateCarrier();
            CreateAirports();
            CreateFlight();
            CreateFlightDesignator();
            CreateBoardingPass();
            Console.WriteLine("Waiting 60 seconds before deleting flights assets...");
            Thread.Sleep(TimeSpan.FromSeconds(60));
            // always close the channel when there will be no further calls made.
            channel.ShutdownAsync().Wait();
        }

        private static void CreateStubs(GrpcChannel channel)
        {
            templatesStub = new Templates.TemplatesClient(channel);
            flightsStub = new Flights.FlightsClient(channel);
        }

        private static void CreateTemplates()
        {   // Use PassKit default templateas a base
            Console.WriteLine("Creating pass template");
            DefaultTemplateRequest templateRequest = new()
            {
                Protocol = PassProtocol.FlightProtocol,
                Revision = 1
            };
            PassTemplate defaultTemplate = templatesStub!.getDefaultTemplate(templateRequest);

            // Modify the default template 
            defaultTemplate.Name = "Quickstart Boarding Pass";
            defaultTemplate.Description = "Quickstart Boarding Pass";
            defaultTemplate.Timezone = "Europe/Dublin";

            // Add Sequence Number to barcode text
            defaultTemplate.Barcode.AltText = "${flights.pnr} | SEQ ${flights.sequenceNumber}";

            // Enable the template for iOS 26 Semantic Boarding Passes
            defaultTemplate.AppleWalletSettings.SemanticBoardingPass = true;

            templateId = templatesStub?.createTemplate(defaultTemplate);
            Console.WriteLine($"Created base template, base template id is {templateId?.Id_}");
        }

        private static void CreateAirports()
        {
            // Creates departure airport    
            Console.WriteLine("Creating departure airport");
            var securityPrograms = new List<AirportSecurityPrograms>() {
                AirportSecurityPrograms.TsaPreCheck,
                AirportSecurityPrograms.Itd,
                AirportSecurityPrograms.Iti,
                AirportSecurityPrograms.GlobalEntry,
            };

            Port departureAirport = new()
            {
                AirportName = "Dublin",
                CityName = "Dublin",
                IataAirportCode = departureAirportCode,
                IcaoAirportCode = departureICAOAirportCode,
                CountryCode = "IE",
                Timezone = "Europe/Dublin",
                // Set iOS 26 text for documents verified badge
                DocumentsVerifiedText = "Cleared To Travel",
                // Display a lounge map in the service section
                LoungeId = "I64D552175CB4497F"
            };
            departureAirport.SecurityPrograms.Add(securityPrograms);

            flightsStub?.createPort(departureAirport);
            Console.WriteLine($"Departure airport created: {departureAirport.IataAirportCode}");

            //Creates arrival airport
            Console.WriteLine("Creating arrival airport");
            Port arrivalAirport = new()
            {
                AirportName = "Bangkok (Suvarnabhumi)",
                CityName = "Bangkok",
                IataAirportCode = arrivalAirportCode,
                IcaoAirportCode = arrivalICAOAirportCode,
                CountryCode = "TH",
                Timezone = "Asia/Bangkok"
            };
            arrivalAirport.SecurityPrograms.Add(securityPrograms);

            flightsStub?.createPort(arrivalAirport);
            Console.WriteLine($"Arrival airport created: {arrivalAirport.IataAirportCode}");

        }

        private static void CreateCarrier()
        {  // Creates carrier
            Console.WriteLine("Creating carrier");
            Carrier carrier = new()
            {
                AirlineName = "PK Airways",
                IataCarrierCode = carrierCode,
                IcaoCarrierCode = carrierCode + "Y",
                PassTypeIdentifier = Constants.AppleCertificate
            };

            flightsStub?.createCarrier(carrier);
            Console.WriteLine($"Carrier created: {carrier.IataCarrierCode}");
        }

        private static void CreateFlightDesignator()
        {   // Creates flight deesignator
            Time departureTime = new()
            {
                Hour = 13,
                Minute = 00,
                Second = 00
            };
            Time arrivalTime = new()
            {
                Hour = 14,
                Minute = 00,
                Second = 00
            };
            Time gateTime = new()
            {
                Hour = 12,
                Minute = 30,
                Second = 00
            };
            Time boardingTime = new()
            {
                Hour = 12,
                Minute = 15,
                Second = 00
            };
            Console.WriteLine("Creating flight designator");
            FlightDesignator flightDesignator = new()
            {
                CarrierCode = carrierCode,
                FlightNumber = flightNumber,
                Revision = 1,
                Origin = departureAirportCode,
                Destination = arrivalAirportCode,
                PassTemplateId = templateId?.Id_,
                Schedule = new()
                {
                    Monday = new()
                    {
                        ScheduledDepartureTime = departureTime,
                        ScheduledArrivalTime = arrivalTime,
                        BoardingTime = boardingTime,
                        GateClosingTime = gateTime
                    }
                }
            };

            flightsStub?.createFlightDesignator(flightDesignator);
            Console.WriteLine("Flight designator created: " +
                $"{flightDesignator.CarrierCode}{flightDesignator.FlightNumber}");

        }

        private static void CreateFlight()
        {// Create flight
            Console.WriteLine("Creating flight");

            LocalDateTime flightDateTime = new()
            {
                DateTime = "2026-04-28T18:00:00"
            };
            LocalDateTime arrivalDateTime = new()
            {
                DateTime = "2026-04-29T14:20:00"
            };
            LocalDateTime boardingDateTime = new()
            {
                DateTime = "2026-04-28T17:20:00"
            };
            LocalDateTime gateCloseDateTime = new()
            {
                DateTime = "2026-04-28T17:50:00"
            };

            Flight flight = new()
            {
                CarrierCode = carrierCode,
                FlightNumber = flightNumber,
                BoardingPoint = departureAirportCode,
                DeplaningPoint = arrivalAirportCode,
                PassTemplateId = templateId?.Id_ ?? "",
                ScheduledDepartureTime = flightDateTime,
                Urls = new FlightURLs
                {
                    ChangeSeatURL = "https://carrier.com/seat",
                    EntertainmentURL = "https://carrier.com/movies",
                    OrderFoodURL = "https://carrier.com/food",
                    PurchaseAdditionalBaggageURL = "https://carrier.com/buyBaggage",
                    PurchaseLoungeAccessURL = "https://carrier.com/relax",
                    PurchaseWifiURL = "https://carrier.com/connect",
                    ReportLostBagURL = "https://carrier.com/lostBag",
                    UpgradeURL = "https://carrier.com/upgrade",
                    TransitProviderEmail = "fly@carrier.com",
                    TransitProviderPhoneNumber = "+1-234-567-8900",
                    TransitProviderWebsiteURL = "https://carrier.com",
                    BagPolicyURL = "https://carrier.com/baggagePolicy",
                    RegisterServiceAnimalURL = "https://carrier.com/serviceAnimal",
                    RequestWheelchairURL = "https://carrier.com/wheelchair",
                    ManagementURL = "https://carrier.com/manageBooking"
                },
                DepartureDate = new Date
                {
                    Day = 28,
                    Month = 4,
                    Year = 2026,
                },

                ScheduledArrivalTime = arrivalDateTime,
                BoardingTime = boardingDateTime,
                GateClosingTime = gateCloseDateTime,
                DepartureTerminal = "2",
                DepartureGate = "22",
                // Add 2 locations for the departure airport and arrival ariport
                // These are used by Apple to provide direcional assistance with semantic baording passes
                LocationMessages =
                {
                    new GPSLocation
                    {
                        Lat = 53.4256,
                        Lon = -6.2574,
                        LockScreenMessage = "Have a safe trip!"
                    },
                     new GPSLocation
                    {
                        Lat = 13.6819,
                        Lon = 100.7469,
                        LockScreenMessage = "Thank you for flying with us!"
                    }
                }
            };

            flightsStub?.createFlight(flight);
            Console.WriteLine($"Created flight: {flight.CarrierCode}{flight.FlightNumber} " +
                $"{flight.DepartureDate.Day}/{flight.DepartureDate.Month}/{flight.DepartureDate.Year}");
        }

        private static void CreateBoardingPass()
        {
            //Create boarding pass
            Console.WriteLine("Creating boarding pass");
            BoardingPassRecord boardingPassRecord = new()
            {
                OperatingCarrierPNR = "P8F8R8",
                BoardingPoint = departureAirportCode,
                DeplaningPoint = arrivalAirportCode,
                CarrierCode = carrierCode,
                FlightNumber = flightNumber,
                SequenceNumber = sequenceNumber,
                // Set document verification to display iOS 26 badge
                ConditionalItems = new ConditionalItems()
                {
                    InternationalDocVerification = InternationalDocVerification.Completed
                },
                DepartureDate = new Date
                {
                    Day = 28,
                    Month = 4,
                    Year = 2026,
                },
                Class = "Economy",
                Passenger = new Passenger
                {
                    PassengerDetails = new Person
                    {
                        Forename = "John",
                        Surname = "Smith",
                    },
                    FrequentFlyerInfo = new()
                    {
                        Tier = "Platinum",
                        AirlineDesignator = "PK",
                        Number = "2468-13579"
                    },
                    WithInfant = true,
                    InfantDetails = new Infant
                    {
                        InfantDetails = new Person
                        {
                            Forename = "Jonny",
                            Surname = "Smith",
                            Suffix = "Jr."
                        },
                        ConditionalItems = new ConditionalItems()
                        {
                            InternationalDocVerification = InternationalDocVerification.Completed
                        },
                        SsrCodes = { "INFT" },
                        // Add iOS 26 Capabilities to the pass record
                        Capabilities = { PassengerCapabilities.PreBoarding },
                        // Optionally remove barcode from infant boarding pass
                        BarcodePayload = "nocode"
                    }
                },
                SeatNumber = "22A",
                BoardingGroup = "2",
                CarryOnAllowance = "7K",
                FreeBaggageAllowance = "20K",
                SsrCodes = { "WCOB", "CO14" },
                // Add iOS 26 Capabilities and Eligible Security Programs to the pass record
                Capabilities =
                {
                    PassengerCapabilities.CarryOn,
                    PassengerCapabilities.PersonalItem,
                    PassengerCapabilities.PreBoarding,
                    // PassengerCapabilities.PriorityBoarding,
                },
                SecurityPrograms =
                {
                    AirportSecurityPrograms.TsaPreCheck
                }
            };

            pass = flightsStub?.createBoardingPass(boardingPassRecord);
            Console.WriteLine($"Boarding pass created, boarding pass urls: {pass?.BoardingPasses}");
        }

        private static void DeleteFlightAssets()
        {
            // Deletes flight and all associated boarding passes created
            Console.WriteLine("Deleting flight");
            FlightRequest flightRequest = new()
            {
                CarrierCode = carrierCode,
                BoardingPoint = departureAirportCode,
                FlightNumber = flightNumber,
                DeplaningPoint = arrivalAirportCode,
                DepartureDate = new Date
                {
                    Day = 28,
                    Month = 4,
                    Year = 2026,
                }

            };
            flightsStub?.deleteFlight(flightRequest);
            Console.WriteLine("Flight deleted");

            Console.WriteLine("Deleting flight designator");
            FlightDesignatorRequest flightDesignator = new()
            {
                CarrierCode = carrierCode,
                FlightNumber = flightNumber,
                Revision = 1
            };
            flightsStub?.deleteFlightDesignator(flightDesignator);
            Console.WriteLine("Deleted flight designator");

            Console.WriteLine("Deleting airports");
            AirportCode departureAirport = new()
            {
                AirportCode_ = departureAirportCode
            };
            flightsStub?.deletePort(departureAirport);
            AirportCode arrivalAirport = new()
            {
                AirportCode_ = arrivalAirportCode
            };
            flightsStub?.deletePort(arrivalAirport);
            Console.WriteLine("Deleted airports");

            Console.WriteLine("Deleting carrier");
            CarrierCode carrier = new()
            {
                CarrierCode_ = carrierCode
            };
            flightsStub?.deleteCarrier(carrier);
            Console.WriteLine("Deleted Carrier");

            Console.WriteLine("Deleting template");
            templatesStub?.deleteTemplate(templateId);
            Console.WriteLine("Deleted template");
        }
    }
}