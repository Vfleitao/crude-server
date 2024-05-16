[![Codacy Badge](https://app.codacy.com/project/badge/Grade/2f935788913c45cc8264706ee210c1b3)](https://app.codacy.com/gh/Vfleitao/crude-server/dashboard?utm_source=gh&utm_medium=referral&utm_content=&utm_campaign=Badge_grade)


![CrudeServer](/Content/logo_small.png)
# Crude Server - A rough and simple webserver in .NET
Crude Server, is a simple server written in .NET and using a HttpListener to answer requests in an async fashion.

It uses something similar to a command pattern, where each request is its own class, which makes it quite easy to unit test and all functionality and find out what happens.

THIS PROJECT IS STILL IN A VERY ALPHA PHASE SO USE AT YOUR OWN RISK

## Why is this necessary
It's not. I built this just because I could and was curious if it would work, and while doing it, I actually enjoyed workin on it.

It is not meant to replace the default .NET Web Apps, just give an alternative for fun and a break from the usual day to day operations.

## What can I do with this server
Currently you can build any crude application you want with it, since it supports almost anything you need.
Unlike the standard .NET Web Apps, you still need to wire things up manually, such as most of your Database Dependencies, and etc.

However, since it uses the standard IServiceProvider Interface, it should not bring up any major issues.

It uses a simple middleware mechanism to process, execute and handle all responses.

To see examples, look into the Integration Tests or the project website [Crude Server](https://crudeserver.devtestplayground.com/)

## TO DO's
- [x] Build Nuget Packages
- [x] Antiforgery Tokens
- [x] Request Size Limiting
- [x] Common UI Helpers for HandleBards
- [ ] Commands as functions
- [ ] Metrics Dashboard
- [ ] Websocket support
- [x] Non embedded file support
- [x] Non view file support
- [x] Crude Server Documentation and Demo app
- [x] Add cookie based auth
- [x] Allow Replacing default status responses (ie: 401, 404, etc)
- [x] Allow support for sections (using inline partials)
- [x] Add Automated scans for code
- [ ] OpenAPI Support
