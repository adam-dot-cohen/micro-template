## Provisioning
This service is responsible for creating and configuring all resources necessary for a user to perform any tasks related to a product.

## Key Concepts
1. Target Users - The id is supplied by the Identity service.  Users have a type(role?) with a given organization(s?) and product(s)
1. Target Products - Product id is supplied by the (x) service.  Products have resource(s) associated with them based on user type(role).
1. Target Resources - Resource id is supplied by the (x) service.  Resources have provisioning tasks based on product and type(role)
1. Provisioning Tasks - A record (entity) of what Command(s) and Event(s) are required to complete an atomic provisioning step.  
1. Provisioning Plan - A collection of Commands and Events to provision a target user for 1 or more target products
1. Provisioning Command - A command that will provision a target user for a target resource
1. Provisioning Event - The Event record of a completed command for a target user and resource
