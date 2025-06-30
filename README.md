PassKit C# Quickstart
=======================

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Nuget](https://img.shields.io/nuget/v/PassKit.Grpc.DotNet)](https://www.nuget.org/packages/PassKit.Grpc.Net)

### Overview

This quickstart aims to help get C# developers up and running with the PassKit SDK as quickly as possible.

### Prerequisites

You will need the following:

- A PassKit account (signup for free at [PassKit](https://app.passkit.com))
- Your PassKit SDK Credentials (available from the [Developer Tools Page](https://app.passkit.com/app/account/developer-tools))
- Apple wallet certificate id (for flights only, available from the [certificate page](https://app.passkit.com/app/account/certificates))
 ![ScreenShot](images/certificate.png)
- Recommended Code Editor Visual Studio Code [Guide to Installation](https://code.visualstudio.com/docs/setup/setup-overview)
- If you use Visual Studio Code use this [guide](https://code.visualstudio.com/docs/languages/csharp) to install the necessary extensions
- .NET v6.0 or later [Guide to Installation](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)

 ### Configuration

1. You should have received the following 3 files when you requested your gRPC credentials:
    - certificate.pem
    - ca-chain.pem
    - key.pem
    
    You can disregard the key-java.pem credentials file as it is not required for C#.

2.  The `PassKit.Grpc.DotNet` SDK uses the `Grpc.Net.Client` library which requires the credentials in PKCS12 format.  To convert your files using openssl, use the following command. You will need to enter the password that you used when requesting your credentials to decrypt the private key.

    ```
    openssl pkcs12 -export \
    -in certificate.pem \
    -inkey key.pem \
    -certfile ca-chain.pem \
    -out client.pfx \
    -passout pass:
    ```

    You should now have a client.pfx file.  Create a `certs` folder in the root of the project and copy this file there e.g. `certs/client.pfx`

3. Edit the `Quickstarts/Constants.cs` file to set your region (For Europe, use pub1 and for US, use pub2).  If you wish to use the Flights quickstart, you will also need to have uploaded an Apple Pass Certificate to your account and supply the PassTypeId (e.g. `pass.com.passkit.quickstart`).  If you want to receive welcome emails for coupons, loyalty, and event tickets, provide a valid email address.

4. In the root poriject directory run `dotnet restore` to download any missing dependencies, followed by `dotnet build` to compile the project.

5. Finally `dotnet run`, to create a sample membership card, coupon card, event ticket and boarding pass (with default templates & tiers/offers) and issue them.

> [!NOTE]
> You will receive some compiler warnings about unreachable code. This happens because of conditional checks on whether constants for the Apple Certificate or Email Address have been provided.  They can be safely ignored.

> [!NOTE]
> The out-of-the-box configuration will pause after each set of tests for you to examine the output.  Pressing Escape at each stage will delete the assets that have been created.  We recommend doing this, particularly for boarding passes where there are restrictions on multiple Carrier, Airport, Flight Designator, and Flight records.

## Examples
All quickstarts are found in the Quickstarts folder.
### GRPC Connection
For implementing in your own projects, use the `GrpcConnectionPool` or `GrpcConnection` class to manage connections to the PassKit gRPC endpoints. By default, this quickstart uses gRPC connection pooling to optimize performance and manage multiple requests efficiently. However, if you prefer to use the single-connection method, comment out the pooling line and use the regular connection method line in `Program.cs`

When to Use Each Mehod:
- gRPC Connection Pooling (Default) - Recommended for production environments with high concurrency and multiple requests. Improves performance by reusing connections.
- Single Connection - Useful for simple applications, debugging, or when only a few API calls are needed.

###  Membership Cards
QuickstartLoyalty will create a membership program with 2 tiers, base and VIP.  It will enrol two members, one in each tier.
It contains the methods:
- CreateProgram() - takes a new program name and creates a new program
- CreateTier() - takes the programId of the program just created in the above program, creates a new template (based of default template), creates a tier, and links this tier to the program
- EnrolMember() - takes programId and tierId created by the above methods, and memberDetails, creates a new member record, and sends a welcome email to deliver membership card url 
- CheckInMember() - an optional method that checks in a member at an event
- CheckOutMember() - an optional method that checks out a member at an event
- AddPoints() - an optional method that takes the memberId of existing member to add points to chosen member
- BurnPoints() - an optional method that burns the selected number of points of a chosen member
- DeleteProgram() -  an optional method that deletes a program and any tiers or members associated with it.

###  Coupons
QuickstartCoupons will create a campaign with 2 offers, base and VIP. It will create two coupons, one in each offer. It will then redeem one of the coupons.
It contains the methods:
- CreateCampaign() - takes a new campaign name and creates a new campaign
- CreateOffer() - takes a campaignId of the campaign you just created and creates a new template (based of default template), creates an offer, and links this offer to the campaign
- CreateCoupon() - takes campaignId and offerId created by the above methods, and couponDetails, creates a new coupon record, and sends a welcome email to deliver coupon card url
- GetSingleCoupon() - an optional method that takes couponId and returns the record of that coupon
- RedeemCoupon() - an optional method that redeems a coupon that has been made
- VoidCoupon() - an optional method that voids a coupon that has been made
- DeleteCampaign() - an optional method that deletes a campaign and any associated offers or coupons

### Boarding Passes
QuickstartFlights will create a carrier, flight, an arrival airport, a departure airport, flight designator and boarding pass for one person. 
It contains the methods:
- CreateTemplate() - creates the pass template for flights and boarding passes
- CreateCarrier() - takes a new carrier code and creates a new carrier
- CreateAirports() - takes a new airport code and creates a new airport
- CreateFlight() - takes templateId , from previous method, to use as base template and uses a carrier code, created from previous method, and creates a new flight
- CreateFlightDesignator() - creates flight designator using flight code
- CreateBoardingPass() - takes templateId, from previous method, and customer details creates a new boarding pass, and sends a welcome email to deliver boarding pass url
- DeleteFlightAssets() - an optional method that deletes the flight objects recently created

### Event Tickets
QuickstartEventTickets will create a venue, production, an event, a ticket type, issue a ticket, validate a ticket and redeem a ticket. 
- CreateTemplate() - creates the pass template for event tickets
- CreateVenue() - creates a venue for the event 
- CreateProduction() - takes a new production name and creates a new production
- CreateTicketType() - takes templateId , from previous method, to use as base template and the productionId, created from previous method, and creates a new ticketType 
- CreateEvent() - takes productionId and venueId ,from previous method, and creates a new Event
- IssueEventTicket() - takes ticketTypeId and  eventId, from previous method, and customer details creates a event ticket, and sends a welcome email to deliver event ticket url
- ValidateTicket() - takes an existing ticket number as well as other details and validates it
- RedeemTicket() - takes an existing ticket number and redeems the event ticket associate with it
- DeleteEventAssets() - an optional method that deletes the tickerts and event objects recently created

## Documentation
* [PassKit Membership Official Documentation](https://docs.passkit.io/protocols/member)
* [PassKit Coupons Official Documentation](https://docs.passkit.io/protocols/coupon)
* [PassKit Boarding Passes Official Documentation](https://docs.passkit.io/protocols/boarding)
* [PassKit Event Tickets Official Documentation](https://docs.passkit.io/protocols/event-tickets/)


## Getting Help
* Email [support@passkit.com](email:support@passkit.com)
* [Online chat support](https://passkit.com/)

