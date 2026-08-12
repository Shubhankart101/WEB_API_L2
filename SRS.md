# The Auction House

|Title|Description|
|-|-|
| **Author**|L. B. Rakesh Kumar|
| **EmpID** | 20004122 |
| **Document Template** | Systems Requirement Specification|
| **Version** | 1.0 |
| **Status** | Draft |

### Revision History
|Date|Version|Description|Author|
|-|-|-|-|
|15/05/2025|1.0|Initial Draft| Rakesh|

## Table of contents
1. [Introduction](#1-introduction)
2. [Overall Description](#2-overall-description)
3. [Business Rules](#3-business-rules)
4. [Functional Requirements](#4-funcational-requirements)


## 1. Introduction
### 1.1 Purpose
This document is a Systems Requirement Specification document which outlines the functional and non-functional requirements of `The Auction House` Project for Release 1.0. This document must be used by the development team to verify the correct functionality of the software.

### 1.2 Project Scope
The scope of the project is to build an online Auction Platform for an imaginary business. The platform should allow users to register and create their profile, add their used/pre-owned assets, place some of their assets on sale with minimum auction price and allow other users to place a bid. The auction entry will be expired after a certain time and the highest bidder will get the ownership of the assets and the platform will facilitate the closure of the transaction. 

## 2. Overall Description
### 2.1 Description (Product Perspective)
The Auction House application is an online platform which allows users to browse various products/assets that other users are willing to sell by the process of Auction. 

### 2.2 User Class
1. A **Seller** Lists an asset he owns at a **Reserve Price (Minimum Listing Price)** defining a **Expiry Date and Time** until which the auction is open for bidding. He would also define a **Incremental Value** which will define the value of the bid for the next bidder. ex. If the current highest bid is $100, the next bidder will have to agree to a pay a price of $110. ($10 incremental)
2. A **User** is able to view a catalog of all open auctions, details of the asset being auctioned and the current highest bid price. User will have a option to place the bid if he is interested in participating in the Auction.
3. A **Bidder** places a bid agreeing to pay the current price + Incremental value if he would endup being the highest bidder.
4. A **Buyer** is a person who ended up being the highest bidder at the end of the Auction process.
5. The **System** takes care of Auction Expiry (Expiry date getting lapsed) and settling the transaction between the Buyer/Seller.
6. An **Administrator** can manage. He regenerate a password of an existing user, verify user activity audit log, block or allow specific users` access or delete a user from the system. 

### 2.3 Operating Environment
* The Website must on all major browsers like IE, Safari, Chrome and Mozilla.
* The Website must be accessible for browsers on the phone confined to the dimensions of the phone (Responsive UI).

### 2.4 Design and Implementation Constraints
* For Release version 1.0, we can ignore the image upload.
* The color theme for the application should be `white`, `gray` and `black`. No other colors should be used.

### 2.5 Assumptions
* The Website will be running 24/7.
* It will use a signal database hosted along side the backend application (same tier).
* The system will not be receiving more that 30 Requests per minute so a single instance of the website is good enough. 

### 2.6 Dependencies
* None

## 3. Business Rules
### 3.1 Rules List
|Rule ID|Description|
|1.|**System**  should ensure the **Bidder** has money in the wallet to be able to afford to place the bid. After placing the bid successfully, the value of the bid amount should be blocked as long as the **Bidder** maintains the highest bidder status. The movement there is another successful bid by another user, the blocked amount should be unblocked and made available for future bidding.|
|2.|As part of the Auction closure process, the assets have to be transfered to the new owner`s assets list. The new owner should be able to auction the asset back into the market by post a new auction. |

## 4. Funcational Requirements
### 4.1 Assets
#### 4.1.1 Create Assets
The system should allow users to create new Assets in the system. The status of the assets during the creation process will be draft. The minimum requirements for creating the assets is `Title`, `Description` and `Retail Value`.
##### 4.1.1.1 Restrictions
* Title should be a minimum of 10 to a maximum of 150 characters long. It should not allow any special characters. Multiple consecutive spaces should be replaced with a single space with edges trimmed. 
* Description should be a minimum of 10 to a maximum of 1000 characters long. It can allow special characters.
* Retail Value should be an positive integer value.

#### 4.1.2 Update Assets
The system should allow users to update the asset `Title`,`Description` and `Retail Value` of their existing assets. 
##### 4.1.2.1 Restrictions
* Only the assets in `Draft` status can be updated.
* Title should be a minimum of 10 to a maximum of 150 characters long. It should not allow any special characters. Multiple consecutive spaces should be replaced with a single space with edges trimmed. 
* Description should be a minimum of 10 to a maximum of 1000 characters long. It can allow special characters.
* Retail Value should be an positive integer value.

#### 4.1.3 Open to Auction
Users can change an Assets status from `Draft` to `Open`. The assets should be in `Open` State to be able to post an Auction.

#### 4.1.4 Active Auction
The System should change the status of the asset to `Active` if the asset is posted in an Auction which is active.

#### 4.1.5 Open to Auction (No Bids)
If the auction expired with no bids, the System should change the status of the asset from `Active` to `Open`.

#### 4.1.6 Change Ownership
If the auction expired with a successful auction closure, the System should change the status of the asset from `Active` to `Open` along with the change in the asset ownership against the buyer.

#### 4.1.7 Delete
The user should be able to delete the asset from his assets list. 
##### 4.1.7.1 Restrictions
* The asset's status should either be in `Open` or `Draft` for this action.

#### 4.1.8 Assets List
The system should allow all users to view the assets they own. It should display `Status`, `Title`,`Description` and `Retail Value` 

### 4.2 Wallet
The system should allow users to deposit or withdraw amount into wallet balence. The wallet balence should also allow for provision to block an available balence against an open highest bid.

#### 4.2.1 Deposit
The system should allow users to add money to the wallet balence. For Release 1, we dont have to work on integration with Payment gateways.
##### 4.2.1.1 Restrictions
* `Amount` has to be a positive integer values. Maximum amount that should be allowed is $999,999. 

#### 4.2.2 Withdrawal
The system should allow users to withdraw money from the wallet. For Release 1, we dont have to work on integration with Payment gateways. 
##### 4.2.2.1 Restrictions
* `Amount` has to be a positive integer values. Maximum amount that should be allowed is $999,999. 

#### 4.2.3 Wallet Dashboard
The system should allow users to display the wallet balence and blocked amount. It should also display any open auctions with the highest bids against which the amount is blocked. 

### 4.3 Dashboard
The main dashboard is like the home page of the application which should be visible for all the users when they first login. This should show all the active auctions in the system sorted by the nearest expiry date. It will also show any bids that the current user might have placed. It should also highlight the bids placed that are still highest bid.

#### 4.3.1 Auction Listing
The system should show all the open auctions hosted by other users sorted by the latest expiry date and active bid (highest bid by the user). The Auction with the highest bid against the user should be on the top, followed by other Auctions that are close to expiring in decending order. It should show `Auction Status`, `Title`, `Description`, `Retail Value`, `Current Highest bid`, `Highest Bidder` and `Next Call` (Next bid amount). There should also be a button to place the `Bid`. 

### 4.4 Auction
The system should allow users to post an Auction. 

#### 4.4.1 Post an Auction
The system should allow to create and post an Auction. `Asset` should be a dropdown which will show all the assets owned by the user that are in `Open` status. It should also capture `Reserve Price` minimum price from where the bid should start and `Incremental Value` which will be used to calculate the `Next Call` = (`Current Highest bid` + `Incremental Value`) and most importantly `Expiration Time (Mins)`. The Auction should also have the provision for `Highest Bidder` and `Current Highest bid`. 

##### 4.4.1.1 Restrictions
* `Asset` should only allow `Open` to auction assets
* `Reserve Price` should be a non-zero positive integer. Maximum value should be $9999. 
* `Incremental Value` should be a non-zero positive integer. Maximum value should be $999.
* `Expiration Time (min)` should be a non-zero positive integer which represents total minutes to expiry. Maximum should be 10080 minutes (7 days).


## 5. Non-Functional Requirements
### 5.1 Low-Traffic
Consideration to be in place to make sure the application architecture is not over designed. As the traffic expectations are less (30 requests per minute), we can host both the database and the application api on the same server. The application api should be REST and exchange data over http and json.