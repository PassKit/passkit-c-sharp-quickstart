using Google.Protobuf.WellKnownTypes;
using Grpc.Net.Client;
using PassKit.Grpc.DotNet.SingleUseCoupons;
using PassKit.Grpc.DotNet;
using Quickstart.Common;


namespace QuickstartCoupons
{

    class Coupons
    {
        /*
         * Stubs are used to access PassKit gRPC Functions. Blocking stubs can process
         * both unary and streaming serverresponses, and therefore can be used with all 
         * current SDK methods. You are free to modify this implementation
         * to add service, async or futures stubs for more efficient operations.
         */
        private static SingleUseCoupons.SingleUseCouponsClient? couponsStub;
        private static Templates.TemplatesClient? templatesStub;
        // Public objects for testing purposes
        public static Id? campaignId;
        public static Id? baseOfferId;
        public static Id? vipOfferId;
        public static Id? baseCouponId;
        public static Id? vipCouponId;
        public static Id? baseTemplateId;
        public static Id? vipTemplateId;

        /*
         * Quickstart will walk through the following steps:
         * - Create a base and vip template 
         * - Create a campaign
         * - Modify a campaign
         * - Create a base and vip offer
         * - Enrol customer on offer
         * - Redeem a coupon
         * - Deletes campaign
         */
        public void Quickstart(GrpcChannel channel)
        {
            CreateStubs(channel);
            CreateTemplate();
            CreateCampaign();
            CreateOffer();
            CreateCoupon();
            GetSingleCoupon(); //optional
            Console.WriteLine("Pausing to examine pass output. Press ESC to redeem/void coupons, after which the passes will become unavailble.");
            do
            {
                while (!Console.KeyAvailable)
                {
                    // Do nothing
                }
            } while (Console.ReadKey(true).Key != ConsoleKey.Escape);
            RedeemCoupon(); //optional
            VoidCoupon(); //optional
            Console.WriteLine("Pausing to examine pass output. Press ESC to delete coupon campaign assets.");
            do
            {
                while (!Console.KeyAvailable)
                {
                    // Do nothing
                }
            } while (Console.ReadKey(true).Key != ConsoleKey.Escape);
            DeleteCampaign(); //optional
            // always close the channel when there will be no further calls made.
            channel.ShutdownAsync().Wait();
        }
        private static void CreateStubs(GrpcChannel channel)
        {
            templatesStub = new Templates.TemplatesClient(channel);
            couponsStub = new SingleUseCoupons.SingleUseCouponsClient(channel);
        }
        private static void CreateTemplate()
        {
            // Get default template
            Console.WriteLine("Creating base template");
            DefaultTemplateRequest templateRequest = new()
            {
                Protocol = PassProtocol.SingleUseCoupon,
                Revision = 1
            };
            PassTemplate defaultTemplate = templatesStub!.getDefaultTemplate(templateRequest);

            // Modify the default template for the base offer
            defaultTemplate.Name = "Quickstart Before Redeem Campaign";
            defaultTemplate.Description = "Quickstart Base Unredeemed Offer Pass";
            defaultTemplate.Timezone = "Europe/London";

            baseTemplateId = templatesStub.createTemplate(defaultTemplate);
            Console.WriteLine($"Created base template, base template id is {baseTemplateId.Id_}");

            Console.WriteLine("Creating vip template");
            // Modify the default template for the vip offer
            defaultTemplate.Name = "Quickstart VIP Before Redeem Campaign";
            defaultTemplate.Description = "Quickstart VIP Unredeemed Offer Pass";
            defaultTemplate.Colors.BackgroundColor = "#000000";
            defaultTemplate.Colors.LabelColor = "#FFFFFF";
            defaultTemplate.Colors.TextColor = "#FFFFFF";

            vipTemplateId = templatesStub!.createTemplate(defaultTemplate);
            Console.WriteLine($"Created vip template, vip template id is {vipTemplateId.Id_}");
        }

        private static void CreateCampaign()
        {
            // Create the campaign
            Console.WriteLine("Creating campaign");
            CouponCampaign campaign = new()
            {
                Name = "Coupon Campaign",
                Status =
                {
                    ProjectStatus.ProjectDraft,
                    ProjectStatus.ProjectActiveForObjectCreation
                }
            };

            campaignId = couponsStub?.createCouponCampaign(campaign);
            Console.WriteLine($"Created campaign, campaign id is {campaignId?.Id_}");
        }

        private static void CreateOffer()
        {
            // Create the base offer
            Console.WriteLine("Creating base offer");
            CouponOffer offer = new()
            {
                CampaignId = campaignId?.Id_,
                BeforeRedeemPassTemplateId = baseTemplateId?.Id_,
                Id = "Base",
                OfferTitle = "Base Offer",
                OfferShortTitle = "Base Offer",
                OfferDetails = "Base Offer",
                IssueStartDate = DateTime.UtcNow.ToTimestamp(),
                IssueEndDate = new DateTime(2028, 06, 30).ToUniversalTime().ToTimestamp()
            };

            baseOfferId = couponsStub?.createCouponOffer(offer);
            Console.WriteLine($"Created base offer, base offer id is {baseOfferId?.Id_}");

            // Create the VIP offer
            Console.WriteLine("Creating vip offer");
            offer.Id = "VIP offer";
            offer.OfferTitle = "VIP offer title";
            offer.OfferShortTitle = "VIP offer";
            offer.OfferDetails = "VIP Offer details";
            offer.OfferFinePrint = "VIP Offer fine print";
            offer.BeforeRedeemPassTemplateId = vipTemplateId?.Id_;

            vipOfferId = couponsStub?.createCouponOffer(offer);
            Console.WriteLine($"Created vip offer, vip offer id is {vipOfferId?.Id_}");
        }

        private static void CreateCoupon()
        {
            // Create base coupon
            Console.WriteLine("Creating base coupon");
            Coupon coupon = new()
            {
                OfferId = baseOfferId?.Id_,
                CampaignId = campaignId?.Id_,
                Person = new()
                {
                    Surname = "Loyal",
                    Forename = "Larry",
                    DisplayName = "Larry",                                       
                },
                Status = CouponStatus.Unredeemed
            };

            if (Constants.EmailAddress != "")
            {
                coupon.Person.EmailAddress = Constants.EmailAddress;

            }

            baseCouponId = couponsStub?.createCoupon(coupon);
            Console.WriteLine("Created base coupon, base coupon id is " + baseCouponId?.Id_);

            Console.WriteLine("Creating vip coupon");
            coupon.OfferId = vipOfferId?.Id_;
            coupon.Person.Surname = "Highroller";
            coupon.Person.Forename = "Harry";
            coupon.Person.DisplayName = "Harry";
            
            vipCouponId = couponsStub?.createCoupon(coupon);
            Console.WriteLine("Created vip coupon, vip coupon id is " + vipCouponId?.Id_);

            Console.WriteLine("Coupon urls:");
            Console.WriteLine($"Base coupon URL: https://{Constants.Environment}.pskt.io/{baseCouponId?.Id_}");
            Console.WriteLine($"Vip coupon URL: https://{Constants.Environment}.pskt.io/{vipCouponId?.Id_}");
        }

        private static void GetSingleCoupon()
        {
            // Takes a coupon id and returns that coupon
            Console.WriteLine("Getting coupon");
            couponsStub?.getCouponById(baseCouponId);
            Console.WriteLine("Coupon retrieved " + couponsStub?.getCouponById(baseCouponId));
        }

        private static void RedeemCoupon()
        {
            // Redeems base coupon, if redeemed pass url will no longer be valid
            Console.WriteLine("Redeeming coupon");
            Coupon redeemRequest = new()
            {
                Id = baseCouponId?.Id_,
                CampaignId = campaignId?.Id_
            };

            couponsStub?.redeemCoupon(redeemRequest);
            Console.WriteLine($"Redeemed coupon {redeemRequest.Id}");
        }

        private static void VoidCoupon()
        {
            // Voids vip coupon if voided pass url will no longer be valid
            Console.WriteLine("Voiding coupon");
            Coupon voidRequest = new()
            {
                Id = vipCouponId?.Id_,
                CampaignId = campaignId?.Id_
            };

            couponsStub?.voidCoupon(voidRequest);
            Console.WriteLine($"Voided coupon {voidRequest.Id}");
        }

        private static void DeleteCampaign()
        {
            // Deletes campaign, offers, and associated passes 
            Console.WriteLine("Deleting campaign");
            couponsStub?.deleteCouponCampaign(campaignId);
            Console.WriteLine("Deleted campaign");

            // Delete templates
            Console.WriteLine("Deleting templates");
            templatesStub!.deleteTemplate(baseTemplateId);
            templatesStub!.deleteTemplate(vipTemplateId) ;
            Console.WriteLine("Deleted templates");
        }
    }
}