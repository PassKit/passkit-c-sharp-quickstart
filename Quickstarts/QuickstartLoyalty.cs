using PassKit.Grpc;
using PassKit.Grpc.Members;
using Grpc.Core;



namespace QuickstartLoyalty
{

    class Membership
    {
        /*
         * Stubs are used to access PassKit gRPC Functions. Blocking stubs can process
         * both unary and streaming server
         * responses, and therefore can be used with all current SDK methods. You are
         * free to modify this implementation
         * to add service, async or futures stubs for more efficient operations.
         */
        private static Templates.TemplatesClient templatesStub;
        private static Members.MembersClient membersStub;
        // Public objects for testing purposes
        public static PassKit.Grpc.Id programId;
        public static PassKit.Grpc.Id baseTierId;
        public static PassKit.Grpc.Id vipTierId;
        public static PassKit.Grpc.Id baseMemberId;
        public static PassKit.Grpc.Id vipMemberId;
        public static PassKit.Grpc.Id baseTemplateId;
        public static PassKit.Grpc.Id vipTemplateId;
        public static String baseEmail = "loyal.larry@dummy.passkit.com"; // Change to your email to receive cards
        public static String vipEmail = "harry.highroller@dummy.passkit.com"; // Change to your email to receive cards

        /*
                * Quickstart will walk through the following steps:
                * - Create template assets
                *- Modify default template for base tier
                *- Modify default template for VIP tier
                *- Create base and VIP tiers
                *- Create a loyalty program with base and VIP tiers
                *- Enrol a member in each tier
                *- Check-in a member
                *- Check-out a member
                *- Add loyalty points to a member
                *- Delete all membership assets
                */
        public void Quickstart(Channel channel)
        {
            createStubs(channel);
            createTemplate();
            createProgram();
            createTier();
            enrolMember();
            checkInMember(); //optional
            checkOutMember();  //optional
            addPoints(); //optional
            burnPoints(); //optional
            deleteProgram(); //optional
            // always close the channel when there will be no further calls made.
            channel.ShutdownAsync().Wait();


        }
        private void createStubs(Channel channel)
        {
            templatesStub = new Templates.TemplatesClient(channel);
            membersStub = new Members.MembersClient(channel);
        }
        private void createTemplate()
        {   // Get default template
            Console.WriteLine("Creating base template");
            DefaultTemplateRequest templateRequest = new DefaultTemplateRequest();
            templateRequest.Protocol = PassProtocol.Membership;
            templateRequest.Revision = 1;
            PassTemplate defaultTemplate = templatesStub.getDefaultTemplate(templateRequest);

            // Modify the default template for the base tier
            defaultTemplate.Name = "Quickstart Base Loyalty";
            defaultTemplate.Description = "Quickstart Base Loyalty";
            defaultTemplate.Timezone = "Europe/London";

            baseTemplateId = templatesStub.createTemplate(defaultTemplate);
            Console.WriteLine("Created base template, base template id is " + baseTemplateId.Id_);

            Console.WriteLine("Creating vip template");
            // Modify the default template for the vip tier
            defaultTemplate.Name = "Quickstart VIP Loyalty ";
            defaultTemplate.Description = "Quickstart VIP Loyalty";
            defaultTemplate.Colors.BackgroundColor = "#000000";
            defaultTemplate.Colors.LabelColor = "#FFFFFF";
            defaultTemplate.Colors.TextColor = "#FFFFFF";

            vipTemplateId = templatesStub.createTemplate(defaultTemplate);
            Console.WriteLine("Created vip template, vip template id is " + vipTemplateId.Id_);


        }
        private void createProgram()
        {   // Create the program
            Console.WriteLine("Creating program");
            PassKit.Grpc.Members.Program program = new PassKit.Grpc.Members.Program();
            program.Name = "Members Program";
            program.Status.Add(ProjectStatus.ProjectDraft);
            program.Status.Add(ProjectStatus.ProjectActiveForObjectCreation);
            programId = membersStub.createProgram(program);
            Console.WriteLine("Created program, program id is" + programId.Id_);
        }

        private void createTier()
        { // Create the base tier
            Console.WriteLine("Creating base tier");
            Tier tier = new Tier();
            tier.ProgramId = programId.Id_;
            tier.PassTemplateId = baseTemplateId.Id_;
            tier.Id = "Base";
            tier.Name = "Base Tier";
            tier.TierIndex = 1;
            tier.Timezone = "Europe/London";

            baseTierId = membersStub.createTier(tier);
            Console.WriteLine("Created base tier, tier id is " + baseTierId.Id_);

            // Create the VIP tier
            Console.WriteLine("Creating vip tier");
            tier.Id = "VIP";
            tier.PassTemplateId = vipTemplateId.Id_;
            tier.Name = "VIP tier";
            tier.TierIndex = 10;

            vipTierId = membersStub.createTier(tier);
            Console.WriteLine("Created vip tier, tier id is " + vipTierId.Id_);

        }

        private void enrolMember()
        { // Enrolls member on base tier
            Console.WriteLine("Enrolling member on base tier");
            Member member = new Member();
            member.TierId = baseTierId.Id_;
            member.ProgramId = programId.Id_;
            member.Person = new Person();
            member.Person.Surname = "Loyal";
            member.Person.Forename = "Larry";
            member.Person.DisplayName = "Larry";
            member.Person.EmailAddress = baseEmail;
            member.Points = 0;

            baseMemberId = membersStub.enrolMember(member);
            Console.WriteLine("Enrolled member on base tier, member id is " + baseMemberId.Id_);

            // Enrolls member on vip tier
            Console.WriteLine("Enrolling member on vip tier");
            member.TierId = vipTierId.Id_;
            member.Points = 9999;
            member.Person.Surname = "Highroller";
            member.Person.Forename = "Harry";
            member.Person.DisplayName = "Harry";
            member.Person.EmailAddress = vipEmail;  // set to an email address that can receive mail to receive an enrolment email.

            vipMemberId = membersStub.enrolMember(member);
            Console.WriteLine("Enrolled member on vip tier, member id is " + vipMemberId.Id_);

            Console.WriteLine("Membership urls:");
            Console.WriteLine("Base member URL: " + "https://pub1.pskt.io/" + baseMemberId.Id_.ToString());
            Console.WriteLine("Vip member URL:" + "https://pub1.pskt.io/" + vipMemberId.Id_.ToString());
        }

        private void checkInMember()
        {   //Checks in base member
            Console.WriteLine("Checking in base member");
            MemberCheckInOutRequest request = new MemberCheckInOutRequest();
            request.MemberId = baseMemberId.Id_;
            request.Lat = 51.5014;
            request.Lon = 0.1419;
            request.Address = "Buckingham Palace, Westminster, London SW1A 1AA";
            request.MetaData.Add("ticketType", "royalDayOut");
            request.MetaData.Add("bookingReference", "4929910033527");
            request.ExternalEventId = "7253300199294";

            var checkInEvent = membersStub.checkInMember(request);
            Console.WriteLine("Checked in member, with member id " + baseMemberId.Id_ + " at event " + checkInEvent);
        }

        private void checkOutMember()
        {
            // Checks out base member 
            Console.WriteLine("Checking out base member");
            MemberCheckInOutRequest request = new MemberCheckInOutRequest();
            request.MemberId = baseMemberId.Id_;
            request.Lat = 51.5014;
            request.Lon = 0.1419;
            request.Address = "Buckingham Palace, Westminster, London SW1A 1AA";
            request.MetaData.Add("ticketType", "royalDayOut");
            request.MetaData.Add("bookingReference", "4929910033527");
            request.MetaData.Add("corgisSeen", "6");
            request.MetaData.Add("visitorSatisfactionRating", "9");
            request.ExternalEventId = "7253300199294";

            var checkOutEvent = membersStub.checkOutMember(request);
            Console.WriteLine("Checked out member, with member id " + baseMemberId.Id_ + " at event " + checkOutEvent);
        }

        private void addPoints()
        {
            //Adds points to base member
            Console.WriteLine("Add points to member ");
            EarnBurnPointsRequest request = new EarnBurnPointsRequest();
            request.Id = baseMemberId.Id_;
            request.Points = 10;

            var memberPoints = membersStub.earnPoints(request);
            Console.WriteLine("Added " + memberPoints.Points.ToString() + " points to member");

        }

        private void burnPoints()
        {
            // Burns points of a base member
            Console.WriteLine("Burning points of a member ");
            EarnBurnPointsRequest request = new EarnBurnPointsRequest();
            request.Id = baseMemberId.Id_;
            request.Points = 10;

            var memberPoints = membersStub.burnPoints(request);
            Console.WriteLine("Burned " + memberPoints.Points.ToString() + " points of a member");
        }

        private void deleteProgram()
        {
            // Deletes program
            Console.WriteLine("Deleting program");
            Id deleteProgramId = new Id();
            deleteProgramId.Id_ = programId.Id_;

            membersStub.deleteProgram(deleteProgramId);
            Console.WriteLine("Deleted program ");
        }
    }
}