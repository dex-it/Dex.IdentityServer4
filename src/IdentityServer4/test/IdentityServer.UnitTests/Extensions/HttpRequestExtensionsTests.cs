﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Threading.Tasks;
using FluentAssertions;
using IdentityServer4.Extensions;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace IdentityServer.UnitTests.Extensions;

public class HttpRequestExtensionsTests
{
    [Fact]
    public Task GetCorsOrigin_valid_cors_request_should_return_cors_origin()
    {
        var ctx = new DefaultHttpContext();
        ctx.Request.Scheme = "http";
        ctx.Request.Host = new HostString("foo");
        ctx.Request.Headers.Add("Origin", "http://bar");

        ctx.Request.GetCorsOrigin().Should().Be("http://bar");
        return Task.CompletedTask;
    }

    [Fact]
    public Task GetCorsOrigin_origin_from_same_host_should_not_return_cors_origin()
    {
        var ctx = new DefaultHttpContext();
        ctx.Request.Scheme = "http";
        ctx.Request.Host = new HostString("foo");
        ctx.Request.Headers.Add("Origin", "http://foo");

        ctx.Request.GetCorsOrigin().Should().BeNull();
        return Task.CompletedTask;
    }

    [Fact]
    public Task GetCorsOrigin_no_origin_should_not_return_cors_origin()
    {
        var ctx = new DefaultHttpContext();
        ctx.Request.Scheme = "http";
        ctx.Request.Host = new HostString("foo");

        ctx.Request.GetCorsOrigin().Should().BeNull();
        return Task.CompletedTask;
    }
}