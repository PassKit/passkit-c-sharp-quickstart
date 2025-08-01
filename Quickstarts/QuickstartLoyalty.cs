using Grpc.Net.Client;
using PassKit.Grpc.DotNet;
using PassKit.Grpc.DotNet.Members;
using PassKit.Grpc.DotNet.SingleUseCoupons;
using Quickstart.Common;
using System;
using System.Threading;


namespace QuickstartLoyalty
{

    class Membership
    {
        /*
         * Stubs are used to access PassKit gRPC Functions. Blocking stubs can process
         * both unary and streaming serverresponses, and therefore can be used with all 
         * current SDK methods. You are free to modify this implementation
         * to add service, async or futures stubs for more efficient operations.
         */
        private static Templates.TemplatesClient? templatesStub;
        private static Members.MembersClient? membersStub;
        // Public objects for testing purposes
        public static Id? programId;
        public static Id? baseTierId;
        public static Id? vipTierId;
        public static Id? baseMemberId;
        public static Id? vipMemberId;
        public static Id? baseTemplateId;
        public static Id? vipTemplateId;
        public static String externalId = "12345";

        /*
         * Quickstart will walk through the following steps:
         * - Create template assets
         * - Modify default template for base tier
         * - Modify default template for VIP tier
         * - Create base and VIP tiers
         * - Create a loyalty program with base and VIP tiers
         * - Enrol a member in each tier
         * - Check-in a member
         * - Check-out a member
         * - Add loyalty points to a member
         * - Delete all membership assets
         */
        public void Quickstart(GrpcChannel channel)
        {
            CreateStubs(channel);
            CreateTemplate();
            CreateProgram();
            CreateTier();
            EnrolMember();
            GetMemberByExternalId();
            CheckInMember(); //optional
            CheckOutMember();  //optional
            AddPoints(); //optional
            BurnPoints(); //optional
            Console.WriteLine("Waiting 60 seconds before deleting loyalty assets...");
            Thread.Sleep(TimeSpan.FromSeconds(60));
            DeleteProgram(); //optional
            // always close the channel when there will be no further calls made.
            channel.ShutdownAsync().Wait();


        }
        private static void CreateStubs(GrpcChannel channel)
        {
            templatesStub = new Templates.TemplatesClient(channel);
            membersStub = new Members.MembersClient(channel);
        }
        private static void CreateTemplate()
        {
            // Get default template
            Console.WriteLine("Creating base template");
            DefaultTemplateRequest templateRequest = new()
            {
                Protocol = PassProtocol.Membership,
                Revision = 1
            };
            PassTemplate defaultTemplate = templatesStub!.getDefaultTemplate(templateRequest);

            // Modify the default template for the base tier
            defaultTemplate.Name = "Quickstart Base Loyalty";
            defaultTemplate.Description = "Quickstart Base Loyalty";
            defaultTemplate.Timezone = "Europe/London";

            baseTemplateId = templatesStub.createTemplate(defaultTemplate);
            Console.WriteLine($"Created base template, base template id is {baseTemplateId.Id_}");

            Console.WriteLine("Creating vip template");
            // Modify the default template for the vip tier
            defaultTemplate.Name = "Quickstart VIP Loyalty ";
            defaultTemplate.Description = "Quickstart VIP Loyalty";
            defaultTemplate.Colors.BackgroundColor = "#000000";
            defaultTemplate.Colors.LabelColor = "#FFFFFF";
            defaultTemplate.Colors.TextColor = "#FFFFFF";

            vipTemplateId = templatesStub.createTemplate(defaultTemplate);
            Console.WriteLine($"Created vip template, vip template id is {vipTemplateId.Id_}");


        }
        private static void CreateProgram()
        {
            // Create the program
            Console.WriteLine("Creating program");
            PassKit.Grpc.DotNet.Members.Program program = new()
            {
                Name = "Members Program"
            };
            program.Status.Add(ProjectStatus.ProjectDraft);
            program.Status.Add(ProjectStatus.ProjectActiveForObjectCreation);
            programId = membersStub?.createProgram(program);
            Console.WriteLine($"Created program, program id is {programId?.Id_}");
        }

        private static void CreateTier()
        {
            // Create the base tier
            Console.WriteLine("Creating base tier");
            Tier tier = new()
            {
                ProgramId = programId?.Id_,
                PassTemplateId = baseTemplateId?.Id_,
                Id = "Base",
                Name = "Base Tier",
                TierIndex = 1,
                Timezone = "Europe/London"
            };

            baseTierId = membersStub?.createTier(tier);
            Console.WriteLine($"Created base tier, tier id is {baseTierId?.Id_}");

            // Create the VIP tier
            Console.WriteLine("Creating vip tier");
            tier.Id = "VIP";
            tier.PassTemplateId = vipTemplateId!.Id_;
            tier.Name = "VIP tier";
            tier.TierIndex = 10;

            vipTierId = membersStub?.createTier(tier);
            Console.WriteLine($"Created vip tier, tier id is {vipTierId?.Id_}");

        }

        private static void EnrolMember()
        {
            // Enrolls member on base tier
            Console.WriteLine("Enrolling member on base tier");
            Member member = new()
            {
                TierId = baseTierId!.Id_,
                ProgramId = programId!.Id_,
                Person = new()
                {
                    Surname = "Loyal",
                    Forename = "Larry",
                    DisplayName = "Larry",
                },
                Points = 0
            };
            if (Constants.EmailAddress != "")
            {
                member.Person.EmailAddress = Constants.EmailAddress;

            }

            baseMemberId = membersStub?.enrolMember(member);
            Console.WriteLine($"Enrolled member on base tier, member id is {baseMemberId?.Id_}");

            // Enrolls member on vip tier
            Console.WriteLine("Enrolling member on vip tier");
            member.TierId = vipTierId!.Id_;
            member.ExternalId = externalId;
            member.Points = 9999;
            member.Person.Surname = "Highroller";
            member.Person.Forename = "Harry";
            member.Person.DisplayName = "Harry";

            vipMemberId = membersStub?.enrolMember(member);
            Console.WriteLine($"Enrolled member on vip tier, member id is {vipMemberId?.Id_}");

            Console.WriteLine("Membership urls:");
            Console.WriteLine($"Base member URL: https://{Constants.Environment}.pskt.io/{baseMemberId?.Id_}"); ;
            Console.WriteLine($"Vip member URL: https://{Constants.Environment}.pskt.io/{vipMemberId?.Id_}");
        }

        private static void CheckInMember()
        {
            //Checks in base member
            Console.WriteLine("Checking in base member");
            MemberCheckInOutRequest request = new()
            {
                MemberId = baseMemberId!.Id_,
                Lat = 51.5014,
                Lon = 0.1419,
                Address = "Buckingham Palace, Westminster, London SW1A 1AA",
                ExternalEventId = "7253300199294"
            };
            request.MetaData.Add("ticketType", "royalDayOut");
            request.MetaData.Add("bookingReference", "4929910033527");

            var checkInEvent = membersStub?.checkInMember(request);
            Console.WriteLine($"Checked in member, with member id {baseMemberId?.Id_} at event " + checkInEvent);
        }

        private static void CheckOutMember()
        {
            // Checks out base member 
            Console.WriteLine("Checking out base member");
            MemberCheckInOutRequest request = new()
            {
                MemberId = baseMemberId!.Id_,
                Lat = 51.5014,
                Lon = 0.1419,
                Address = "Buckingham Palace, Westminster, London SW1A 1AA",
                ExternalEventId = "7253300199294",
            };
            request.MetaData.Add("ticketType", "royalDayOut");
            request.MetaData.Add("bookingReference", "4929910033527");
            request.MetaData.Add("corgisSeen", "6");
            request.MetaData.Add("visitorSatisfactionRating", "9");

            var checkOutEvent = membersStub?.checkOutMember(request);
            Console.WriteLine($"Checked out member, with member id {baseMemberId?.Id_} at event {checkOutEvent}");
        }

        private static void AddPoints()
        {
            //Adds points to base member
            Console.WriteLine("Add points to member ");
            EarnBurnPointsRequest request = new()
            {
                Id = baseMemberId!.Id_,
                Points = 10
            };

            var memberPoints = membersStub?.earnPoints(request);
            Console.WriteLine($"Added {memberPoints?.Points} points to member");

        }

        private static void BurnPoints()
        {
            // Burns points of a base member
            Console.WriteLine("Burning points of a member ");
            EarnBurnPointsRequest request = new()
            {
                Id = baseMemberId?.Id_,
                Points = 10
            };

            var memberPoints = membersStub?.burnPoints(request);
            Console.WriteLine($"Burned {memberPoints?.Points} points of a member");
        }

        private static void GetMemberByExternalId()
        {
            Console.WriteLine("Getting member by external Id");
            try
            {
                MemberRecordByExternalIdRequest memberRequest = new()
                {
                    ProgramId = programId?.Id_,
                    ExternalId = externalId
                };

                var member = membersStub?.getMemberRecordByExternalId(memberRequest);
                if (member != null)
                {
                    Console.WriteLine($"Found Member: {member.Id}");
                }
                else
                {
                    Console.WriteLine("Member Not Found");
                }
            }
            catch (Exception ex)
            { Console.WriteLine(ex.ToString()); }

        }

        private static void DeleteProgram()
        {
            // Deletes program
            Console.WriteLine("Deleting program");
            membersStub?.deleteProgram(programId);
            Console.WriteLine("Deleted program");

            // Delete templates
            Console.WriteLine("Deleting templates");
            templatesStub!.deleteTemplate(baseTemplateId);
            templatesStub!.deleteTemplate(vipTemplateId);
            Console.WriteLine("Deleted templates");
        }
    }
}