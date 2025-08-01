using Grpc.Net.Client;
using PassKit.Grpc.DotNet;
using PassKit.Grpc.DotNet.EventTickets;
using Google.Protobuf.WellKnownTypes;
using Quickstart.Common;
using PassKit.Grpc.DotNet.Members;
using System;
using System.Threading;

/* Quickstart Event Tickets runs through the high level steps required to create event tickets from scratch using the PassKit gRPC C Sharp SDK. 
 */
namespace QuickstartEventickets
{
    class EventTicket
    {
        /*
         * Stubs are used to access PassKit gRPC Functions. Blocking stubs can process
         * both unary and streaming serverresponses, and therefore can be used with all 
         * current SDK methods. You are free to modify this implementation
         * to add service, async or futures stubs for more efficient operations.
         */

        private static EventTickets.EventTicketsClient? eventsStub;
        private static Templates.TemplatesClient? templatesStub;

        /*
         * Quickstart will walk through the following steps:
         * - Modify default template for a regular flight ticket
         * - Modify default template for a vip flight ticket
         * - Create a venue
         * - Create a Production
         * - Create an event
         * - Create a ticket type
         * - Issue an event ticket
         * - Validate an event ticket
         * - Redeem an event ticket
         * 
         */

        // Public objects for testing purposes
        public static Id? templateId;
        public static Id? productionId;
        public static Id? venueId;
        public static Id? eventId;
        public static Id? ticketTypeId;
        public static Id? pass;

        public void QuickStart(GrpcChannel channel)
        {
            CreateStubs(channel);
            CreateTemplate();
            CreateVenue();
            CreateProduction();
            CreateEvent();
            CreateTicketType();
            IssueEventTicket();
            ValidateTicket();
            RedeemTicket();
            Console.WriteLine("Waiting 60 seconds before deleting event ticket assets...");
            Thread.Sleep(TimeSpan.FromSeconds(60));
            DeleteEventAssets();
            // always close the channel when there will be no further calls made.
            channel.ShutdownAsync().Wait();

        }

        private static void CreateStubs(GrpcChannel channel)
        {
            templatesStub = new Templates.TemplatesClient(channel);
            eventsStub = new EventTickets.EventTicketsClient(channel);
        }

        private static void CreateTemplate()
        {
            // Get default template
            Console.WriteLine("Creating template");
            DefaultTemplateRequest templateRequest = new()
            {
                Protocol = PassProtocol.EventTicketing,
                Revision = 1
            };
            PassTemplate defaultTemplate = templatesStub!.getDefaultTemplate(templateRequest);

            // Modify the default template 
            defaultTemplate.Name = "Quickstart Event Tickets";
            defaultTemplate.Description = "Quickstart Event Ticket";
            defaultTemplate.Timezone = "Europe/London";

            templateId = templatesStub.createTemplate(defaultTemplate);
            Console.WriteLine($"Created template, template id is {templateId.Id_}");
        }

        private static void CreateVenue()
        {
            // Creates venue 
            Console.WriteLine("Creating venue");
            Venue venue = new()
            {
                Name = "Quickstart Venue",
                Address = "123 Abc Street",
                Timezone = "Europe/London"
            };

            venueId = eventsStub?.createVenue(venue);
            Console.WriteLine($"Venue created {venue.Id}");
        }

        private static void CreateProduction()
        {
            // Creates production
            Console.WriteLine("Creating Production");
            Production production = new()
            {
                Name = "Quickstart Production",
                FinePrint = "Quickstart Fine Print",
                AutoInvalidateTicketsUponEventEnd = Toggle.On
            };

            productionId = eventsStub?.createProduction(production);
            Console.WriteLine($"Production created {productionId?.Id_}");
        }

        private static void CreateEvent()
        {
            DateTime startDate = new(2028, 12, 12, 12, 0, 0, 0, DateTimeKind.Local);
            startDate = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
            DateTime endDate = new(2028, 12, 13, 13, 0, 0, 0, DateTimeKind.Local);
            endDate = DateTime.SpecifyKind(endDate, DateTimeKind.Utc);
            DateTime doorsOpen = new(2028, 12, 13, 12, 13, 0, 0, DateTimeKind.Local);
            doorsOpen = DateTime.SpecifyKind(doorsOpen, DateTimeKind.Utc);

            // Creates event
            Console.WriteLine("Creating Production");
            Event newEvent = new()
            {
                Production = new()
                {
                    Id = productionId?.Id_
                },
                Venue = new Venue
                {
                    Id = venueId?.Id_
                },
                ScheduledStartDate = startDate.ToTimestamp(),
                DoorsOpen = doorsOpen.ToTimestamp(),
                EndDate = endDate.ToTimestamp(),
                RelevantDate = startDate.ToTimestamp()
            };

            eventId = eventsStub?.createEvent(newEvent);
            Console.WriteLine($"Event created {eventId?.Id_}");

        }

        private static void CreateTicketType()
        {
            // Create Ticket Type
            Console.WriteLine("Creating ticket type");

            TicketType newTicketType = new()
            {
                Name = "Quickstart",
                ProductionId = productionId?.Id_,
                BeforeRedeemPassTemplateId = templateId?.Id_,
                Uid = ""
            };

            ticketTypeId = eventsStub?.createTicketType(newTicketType);
            Console.WriteLine($"Created ticket type {ticketTypeId?.Id_}");
        }

        private static void IssueEventTicket()
        {
            DateTime endDate = new(2028, 12, 13, 13, 0, 0, 0, DateTimeKind.Local);
            endDate = DateTime.SpecifyKind(endDate, DateTimeKind.Utc);
            //Issue event ticket
            Console.WriteLine("Issuing event ticket");
            IssueTicketRequest ticket = new()
            {
                TicketTypeId = ticketTypeId?.Id_,
                EventId = eventId?.Id_,
                ExpiryDate = endDate.ToTimestamp(),
                OrderNumber = "1",
                TicketNumber = "1",
                Person = new()
                {
                    Surname = "Loyal",
                    Forename = "Larry",
                    DisplayName = "Larry",
                }
            };
            if (Constants.EmailAddress != "")
            {
                ticket.Person.EmailAddress = Constants.EmailAddress;

            }

            pass = eventsStub?.issueTicket(ticket);
            Console.WriteLine($"Event ticket created, event ticket url: https://{Constants.Environment}.pskt.io/{pass?.Id_}");

        }



        private static void ValidateTicket()
        {
            DateTime validateDate = DateTime.Now;
            validateDate = DateTime.SpecifyKind(validateDate, DateTimeKind.Utc);

            //Validate event ticket
            Console.WriteLine("Validating event ticket");
            ValidateTicketRequest ticketToValidate = new()
            {
                MaxNumberOfValidations = 1,
                Ticket = new()
                {
                    TicketId_ = pass?.Id_,
                },
                ValidateDetails = new()
                {
                    ValidateDate = validateDate.ToTimestamp()
                }
            };
            ;


            eventsStub?.validateTicket(ticketToValidate);
            Console.WriteLine("Event ticket has been validated");
        }

        private static void RedeemTicket()
        {
            DateTime redeemDate = DateTime.Now;
            redeemDate = DateTime.SpecifyKind(redeemDate, DateTimeKind.Utc);

            //Redeem event ticket
            Console.WriteLine("Redeeming event ticket");
            RedeemTicketRequest ticketToRedeem = new()
            {
                Ticket = new()
                {
                    TicketId_ = pass?.Id_,

                },
                RedemptionDetails = new()
                {
                    RedemptionDate = redeemDate.ToTimestamp()
                }

            };

            eventsStub?.redeemTicket(ticketToRedeem);
            Console.WriteLine("Event ticket has been redeemed");
        }

        private static void DeleteEventAssets()
        {
            // Delete the event and all associated tickets
            Console.WriteLine("Deleting event");
            Event req = new()
            {
                Id = eventId?.Id_
            };
            eventsStub?.deleteEvent(req);
            Console.WriteLine("Deleted event");

            // Delete Ticket Type
            Console.WriteLine("Deleting ticket type");
            TicketType ticketType = new()
            {
                Id = ticketTypeId?.Id_
            };
            eventsStub?.deleteTicketType(ticketType);

            // Delete Production
            Console.WriteLine("Deleting production");
            Production production = new()
            {
                Id = productionId?.Id_
            };
            eventsStub?.deleteProduction(production);
            Console.WriteLine("Deleted production");

            // Delete Production
            Console.WriteLine("Deleting venue");
            Venue venue = new()
            {
                Id = venueId?.Id_
            };
            eventsStub?.deleteVenue(venue);
            Console.WriteLine("Deleted venue");

            // Delete template
            Console.WriteLine("Deleting templates");
            templatesStub!.deleteTemplate(templateId);
            Console.WriteLine("Deleted templates");
        }
    }
}