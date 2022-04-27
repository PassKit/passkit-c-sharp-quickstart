# passkit-c-sharp-grpc-quickstart

The PassKit C# SDK makes it quick and easy to create and install your branded membership passes for Apple Wallet and Google Pay.

This repository has following structure with each purpose.
- `certs` folder is a place to store your credential files.
- `Quickstarts` folder contains SDK methods you can use to create membership cards, coupons and boarding passes. 

## Table of Content
* [Prerequisites](#prerequisites)
* [Quickstart](#quickstart)
* [Examples](#examples)
* [Documentation](#documentation)
* [Getting Help](#getting-help)

## Prerequisites
1. Create a PassKit account. Sign up for free [HERE](https://app.passkit.com/).

2. Generate & Download your SDK credentials by clicking the 'GENERATE NEW SDK CREDENTIALS' button from the Developer Tools page in the [portal](https://app.passkit.com/app/account/developer-tools).

3. Your Apple Wallet certificate id (for boarding passes only)
   
## Quickstart
By completing this Quickstart, you will be able to up and running with the PassKit SDK as quickly as possible.

1. Ensure your followed the steps in [prerequisites](#prerequisites).

2. Place your SDK credential files (`certificate.pem`, `key.pem` and `ca-chain.pem`) in the certs folder in this repoo. The SDK uses these .pem files to authenticate against the PassKit server.

3. Now we need to decrypt your `key.pem`. At your project root directory, run `cd ./certs`  `openssl ec -in key.pem -out key.pem`. If you do not see `Proc-Type: 4,ENCEYPTED` on line 2, you have successfully decrypted `key.pem`. 

4. Go back to root directory with `cd ../..`. Then run `dotnet run`, to create a sample membership card, coupon card and boarding pass (with default templates & tiers/offers) and issue them.

## Examples
###  Membership Cards
QuickstartLoyalty will create a membership program with 2 tiers, base and VIP.  It will enrol two members, one in each tier.

###  Coupons
QuickstartCoupons will create a campaign with 2 offers, base and VIP. It will create two coupons, one in each offer. It will then redeem one of the coupons.

### Boarding Passes
QuickstartFlights will create a carrier, flight, an arrival airport, a departure airport, flight designator and boarding pass for one person. 

## Documentation
* [PassKit Membership Official Documentation](https://docs.passkit.io/protocols/member)
* [PassKit Coupons Official Documentation](https://docs.passkit.io/protocols/coupon)
* [PassKit Boarding Passes Official Documentation](https://docs.passkit.io/protocols/boarding)


## Getting Help
* Email [support@passkit.com](email:support@passkit.com)
* [Online chat support](https://passkit.com/)

