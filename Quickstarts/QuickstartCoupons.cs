using PassKit.Grpc.SingleUseCoupons;
using PassKit.Grpc;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;



namespace QuickstartCoupons
{

    class Coupons
    {
        /*
         * Stubs are used to access PassKit gRPC Functions. Blocking stubs can process
         * both unary and streaming server
         * responses, and therefore can be used with all current SDK methods. You are
         * free to modify this implementation
         * to add service, async or futures stubs for more efficient operations.
         */
        private static SingleUseCoupons.SingleUseCouponsClient couponsStub;
        private static Templates.TemplatesClient templatesStub;
        // Public objects for testing purposes
        public static PassKit.Grpc.Id campaignId;
        public static PassKit.Grpc.Id baseOfferId;
        public static PassKit.Grpc.Id vipOfferId;
        public static PassKit.Grpc.Id baseCouponId;
        public static PassKit.Grpc.Id vipCouponId;
        public static PassKit.Grpc.Id baseTemplateId;
        public PassKit.Grpc.Id vipTemplateId;
        public static String baseEmail = "loyal.larry@dummy.passkit.com"; // Change to your email to receive cards
        public static String vipEmail = "harry.highroller@dummy.passkit.com"; // Change to your email to receive cards

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
        public void Quickstart(Channel channel)
        {
            createStubs(channel);
            createTemplate();
            createCampaign();
            createOffer();
            createCoupon();
            getSingleCoupon(); //optional
            redeemCoupon(); //optional
            voidCoupon(); //optional
            deleteCampaign(); //optional
            // always close the channel when there will be no further calls made.
            channel.ShutdownAsync().Wait();
        }
        private void createStubs(Channel channel)
        {
            templatesStub = new Templates.TemplatesClient(channel);
            couponsStub = new SingleUseCoupons.SingleUseCouponsClient(channel);
        }
        private void createTemplate()
        {   // Get default template
            Console.WriteLine("Creating base template");
            DefaultTemplateRequest templateRequest = new DefaultTemplateRequest();
            templateRequest.Protocol = PassProtocol.SingleUseCoupon;
            templateRequest.Revision = 1;
            PassTemplate defaultTemplate = templatesStub.getDefaultTemplate(templateRequest);

            // Modify the default template for the base offer
            defaultTemplate.Name = "Quickstart Before Redeem Campaign";
            defaultTemplate.Description = "Quickstart Base Unredeemed Offer Pass";
            defaultTemplate.Timezone = "Europe/London";

            baseTemplateId = templatesStub.createTemplate(defaultTemplate);
            Console.WriteLine("Created base template, base template id is " + baseTemplateId.Id_);

            Console.WriteLine("Creating vip template");
            // Modify the default template for the vip offer
            defaultTemplate.Name = "Quickstart VIP Before Redeem Campaign";
            defaultTemplate.Description = "Quickstart VIP Unredeemed Offer Pass";
            defaultTemplate.Colors.BackgroundColor = "#000000";
            defaultTemplate.Colors.LabelColor = "#FFFFFF";
            defaultTemplate.Colors.TextColor = "#FFFFFF";

            vipTemplateId = templatesStub.createTemplate(defaultTemplate);
            Console.WriteLine("Created vip template, vip template id is " + vipTemplateId.Id_);


        }
        private void createCampaign()
        {   // Create the campaign
            Console.WriteLine("Creating campaign");
            CouponCampaign campaign = new CouponCampaign();
            campaign.Name = "Coupon Campaign";
            campaign.Status.Add(ProjectStatus.ProjectDraft);
            campaign.Status.Add(ProjectStatus.ProjectActiveForObjectCreation);

            campaignId = couponsStub.createCouponCampaign(campaign);
            Console.WriteLine("Created campaign, campaign id is " + campaignId.Id_);
        }

        private void createOffer()
        { // Create the base offer
            Console.WriteLine("Creating base offer");
            CouponOffer offer = new CouponOffer();
            offer.CampaignId = campaignId.Id_;
            offer.BeforeRedeemPassTemplateId = baseTemplateId.Id_;
            offer.Id = "Base";
            offer.OfferTitle = "Base Offer";
            offer.OfferShortTitle = "Base Offer";
            offer.OfferDetails = "Base Offer";
            offer.IssueStartDate = DateTime.UtcNow.ToTimestamp();
            offer.IssueEndDate = new DateTime(2022, 06, 30).ToUniversalTime().ToTimestamp();

            baseOfferId = couponsStub.createCouponOffer(offer);
            Console.WriteLine("Created base offer, base offer id is " + baseOfferId.Id_);

            // Create the VIP offer
            Console.WriteLine("Creating vip offer");
            offer.Id = "VIP offer";
            offer.OfferTitle = "VIP offer title";
            offer.OfferShortTitle = "VIP offer";
            offer.OfferDetails = "VIP Offer details";
            offer.OfferFinePrint = "VIP Offer fine print";
            offer.BeforeRedeemPassTemplateId = vipTemplateId.Id_;

            vipOfferId = couponsStub.createCouponOffer(offer);
            Console.WriteLine("Created vip offer, vip offer id is " + vipOfferId.Id_);
        }

        private void createCoupon()
        { // Create base coupon
            Console.WriteLine("Creating base coupon");
            Coupon coupon = new Coupon();
            coupon.OfferId = baseOfferId.Id_;
            coupon.CampaignId = campaignId.Id_;
            coupon.Person = new Person();
            coupon.Person.Surname = "Loyal";
            coupon.Person.Forename = "Larry";
            coupon.Person.DisplayName = "Larry";
            coupon.Person.EmailAddress = baseEmail; // set to an email address that can receive mail to receive an enrolment email                                           
            coupon.Status = CouponStatus.Unredeemed;

            baseCouponId = couponsStub.createCoupon(coupon);
            Console.WriteLine("Created base coupon, base coupon id is " + baseCouponId.Id_);

            Console.WriteLine("Creating vip coupon");
            coupon.OfferId = vipOfferId.Id_;
            coupon.Person.Surname = "Highroller";
            coupon.Person.Forename = "Harry";
            coupon.Person.DisplayName = "Harry";
            coupon.Person.EmailAddress = vipEmail; // set to an email address that can receive mail to receive an enrolment
                                                   // email.
            vipCouponId = couponsStub.createCoupon(coupon);
            Console.WriteLine("Created vip coupon, vip coupon id is " + vipCouponId.Id_);

            Console.WriteLine("Coupon urls:");
            Console.WriteLine("Base coupon URL: " + "https://pub1.pskt.io/" + baseCouponId.Id_.ToString());
            Console.WriteLine("Vip coupon URL:" + "https://pub1.pskt.io/" + vipCouponId.Id_.ToString());
        }

        private void getSingleCoupon()
        {
            // Takes a coupon id and returns that coupon
            Console.WriteLine("Getting coupon");
            couponsStub.getCouponById(baseCouponId);
            Console.WriteLine("Coupon retrieved " + couponsStub.getCouponById(baseCouponId));

        }

        private void redeemCoupon()
        {
            // Redeems base coupon, if redeemed pass url will no longer be valid
            Console.WriteLine("Redeeming coupon");
            Coupon redeemRequest = new Coupon();
            redeemRequest.Id = baseCouponId.Id_;
            redeemRequest.CampaignId = campaignId.Id_;

            couponsStub.redeemCoupon(redeemRequest);
            Console.WriteLine("Redeemed coupon " + redeemRequest.Id);
        }

        private void voidCoupon()
        {   // Voids vip coupon
            Console.WriteLine("Voiding coupon");
            Coupon voidRequest = new Coupon();
            voidRequest.Id = vipCouponId.Id_;
            voidRequest.CampaignId = campaignId.Id_;

            couponsStub.voidCoupon(voidRequest);
            Console.WriteLine("Voided coupon " + voidRequest.Id);
        }
        private void deleteCampaign()
        {
            // Deletes campaign 
            Console.WriteLine("Deleting campaign");
            Id deleteCampaignId = new Id();
            deleteCampaignId.Id_ = campaignId.Id_;

            couponsStub.deleteCouponCampaign(deleteCampaignId);
            Console.WriteLine("Deleted campaign ");
        }
    }
}