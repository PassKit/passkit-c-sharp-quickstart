using PassKit.Grpc;
using PassKit.Grpc.EventTickets;
using Grpc.Core;
using Google.Protobuf.WellKnownTypes;

/* Quickstart Event Tickets runs through the high level steps required to create event tickets from scratch using the PassKit gRPC C Sharp SDK. 
 */
namespace QuickstartEventickets
{
    class EventTicket
    {

        /*
         * Stubs are used to access PassKit gRPC Functions. Blocking stubs can process
         * both unary and streaming server
         * responses, and therefore can be used with all current SDK methods. You are
         * free to modify this implementation
         * to add service, async or futures stubs for more efficient operations.
         */

        private static EventTickets.EventTicketsClient eventsStub;
        private static Templates.TemplatesClient templatesStub;

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

        // Public objects for testing purposes   public static Image.ImageIds flightImageIds;
        public static PassKit.Grpc.Id templateId;
        public static PassKit.Grpc.Id productionId;
        public static PassKit.Grpc.Id venueId;
        public static PassKit.Grpc.Id eventId;
        public static PassKit.Grpc.Id ticketTypeId;
        public static PassKit.Grpc.Id pass;

        public void QuickStart(Channel channel)
        {
            createStubs(channel);
            createTemplate();
            createVenue();
            createProduction();
            createEvent();
            createTicketType();
            issueEventTicket();
            validateTicket();
            redeemTicket();
            // always close the channel when there will be no further calls made.
            channel.ShutdownAsync().Wait();

        }

        private void createStubs(Channel channel)
        {
            templatesStub = new Templates.TemplatesClient(channel);
            eventsStub = new EventTickets.EventTicketsClient(channel);
        }

        private void createTemplate()
        {// Get default template
            Console.WriteLine("Creating template");
            DefaultTemplateRequest templateRequest = new DefaultTemplateRequest();
            templateRequest.Protocol = PassProtocol.EventTicketing;
            templateRequest.Revision = 1;
            PassTemplate defaultTemplate = templatesStub.getDefaultTemplate(templateRequest);

            // Modify the default template 
            defaultTemplate.Name = "Quickstart Event Tickets";
            defaultTemplate.Description = "Quickstart Event Ticket";
            defaultTemplate.Timezone = "Europe/London";

            templateId = templatesStub.createTemplate(defaultTemplate);
            Console.WriteLine("Created template, template id is " + templateId.Id_);
        }

        private void createVenue()
        {
            // Creates venue 
            Console.WriteLine("Creating venue");
            Venue venue = new Venue();
            venue.Name = "Quickstart Venue";
            venue.Address = "123 Abc Street";
            venue.Timezone = "Europe/London";

            venueId = eventsStub.createVenue(venue);
            Console.WriteLine("Venue created" + venue);
        }

        private void createProduction()
        {  // Creates production
            Console.WriteLine("Creating Production");
            Production production = new Production();
            production.Name = "Quickstart Production";
            production.FinePrint = "Quickstart Fine Print";
            production.AutoInvalidateTicketsUponEventEnd = Toggle.On;

            productionId = eventsStub.createProduction(production);
            Console.WriteLine("Production created" + production);
        }

        private void createEvent()
        {
            DateTime startDate = new DateTime(2023, 12, 12, 12, 0, 0, 0, DateTimeKind.Unspecified);
            startDate = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
            DateTime endDate = new DateTime(2023, 12, 13, 13, 0, 0, 0, DateTimeKind.Unspecified);
            endDate = DateTime.SpecifyKind(endDate, DateTimeKind.Utc);
            DateTime doorsOpen = new DateTime(2023, 12, 13, 12, 13, 0, 0, DateTimeKind.Unspecified);
            doorsOpen = DateTime.SpecifyKind(doorsOpen, DateTimeKind.Utc);

            // Creates event
            Console.WriteLine("Creating Production");
            Event newEvent = new Event();
            newEvent.Production = new Production();
            newEvent.Production.Id = productionId.Id_;
            newEvent.Venue = new Venue();
            newEvent.Venue.Id = venueId.Id_;
            newEvent.ScheduledStartDate = startDate.ToTimestamp();
            newEvent.DoorsOpen = doorsOpen.ToTimestamp();
            newEvent.EndDate = endDate.ToTimestamp();
            newEvent.RelevantDate = startDate.ToTimestamp();

            eventId = eventsStub.createEvent(newEvent);

            Console.WriteLine("Event created " + newEvent);

        }

        private void createTicketType()
        {// Create Ticket Type
            Console.WriteLine("Creating ticket type");

            TicketType newTicketType = new TicketType();
            newTicketType.Name = "Quickstart Ticket Type";
            newTicketType.ProductionId = productionId.Id_;
            newTicketType.BeforeRedeemPassTemplateId = templateId.Id_;
            newTicketType.Uid = "";

            ticketTypeId = eventsStub.createTicketType(newTicketType);
            Console.WriteLine("Created ticket type" + newTicketType);
        }

        private void issueEventTicket()
        {
            DateTime endDate = new DateTime(2023, 12, 13, 13, 0, 0, 0, DateTimeKind.Unspecified);
            endDate = DateTime.SpecifyKind(endDate, DateTimeKind.Utc);
            //Issue event ticket
            Console.WriteLine("Issuing event ticket");
            IssueTicketRequest ticket = new IssueTicketRequest();
            ticket.TicketTypeId = ticketTypeId.Id_;
            ticket.EventId = eventId.Id_;
            ticket.ExpiryDate = endDate.ToTimestamp();
            ticket.OrderNumber = "1";
            ticket.TicketNumber = "1";
            ticket.Person = new Person();
            ticket.Person.Surname = "Loyal";
            ticket.Person.Forename = "Larry";
            ticket.Person.DisplayName = "Larry";
            ticket.Person.EmailAddress = "";

            pass = eventsStub.issueTicket(ticket);
            Console.WriteLine("Event ticket created, event ticket url: https://pub1.pskt.io/" + pass.Id_);
        }

        private void validateTicket()
        {
            DateTime validateDate = DateTime.Now;
            validateDate = DateTime.SpecifyKind(validateDate, DateTimeKind.Utc);

            //Validate event ticket
            Console.WriteLine("Validating event ticket");
            ValidateTicketRequest ticketToValidate = new ValidateTicketRequest();
            ticketToValidate.MaxNumberOfValidations = 1;
            ticketToValidate.Ticket = new TicketId();
            ticketToValidate.Ticket.TicketId_ = pass.Id_;
            ticketToValidate.ValidateDetails = new ValidateDetails();
            ticketToValidate.ValidateDetails.ValidateDate = validateDate.ToTimestamp();


            eventsStub.validateTicket(ticketToValidate);
            Console.WriteLine("Event ticket has been validated");
        }

        private void redeemTicket()
        {
            DateTime redeemDate = DateTime.Now;
            redeemDate = DateTime.SpecifyKind(redeemDate, DateTimeKind.Utc);

            //Redeem event ticket
            Console.WriteLine("Redeeming event ticket");
            RedeemTicketRequest ticketToRedeem = new RedeemTicketRequest();
            ticketToRedeem.Ticket = new TicketId();
            ticketToRedeem.Ticket.TicketId_ = pass.Id_;
            ticketToRedeem.RedemptionDetails = new RedemptionDetails();
            ticketToRedeem.RedemptionDetails.RedemptionDate = redeemDate.ToTimestamp();


            eventsStub.redeemTicket(ticketToRedeem);
            Console.WriteLine("Event ticket has been redeemed");
        }

    }
}