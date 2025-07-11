// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Endpoints;
using Microsoft.Extensions.Logging;

namespace IdentityServer.UnitTests.Endpoints.EndSession;

public class EndSessionCallbackEndpointTests
{
    private const string Category = "End Session Callback Endpoint";

    private StubEndSessionRequestValidator _stubEndSessionRequestValidator = new();
    private EndSessionCallbackEndpoint _subject;

    public EndSessionCallbackEndpointTests()
    {
        _subject = new EndSessionCallbackEndpoint(
            _stubEndSessionRequestValidator,
            new LoggerFactory().CreateLogger<EndSessionCallbackEndpoint>());
    }
}