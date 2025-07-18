﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentAssertions;
using IdentityServer.IntegrationTests.Common;
using IdentityServer4.Models;
using IdentityServer4.Test;
using Xunit;

namespace IdentityServer.IntegrationTests.Conformance.Basic;

public class RedirectUriTests
{
    private const string Category = "Conformance.Basic.RedirectUriTests";

    private IdentityServerPipeline _mockPipeline = new();

    public RedirectUriTests()
    {
        _mockPipeline.Initialize();

        _mockPipeline.Clients.Add(new Client
        {
            Enabled = true,
            ClientId = "code_client",
            ClientSecrets = new List<Secret>
            {
                new("secret".Sha512())
            },

            AllowedGrantTypes = GrantTypes.Code,
            AllowedScopes = { "openid" },

            RequireConsent = false,
            RequirePkce = false,
            RedirectUris = new List<string>
            {
                "https://code_client/callback",
                "https://code_client/callback?foo=bar&baz=quux"
            }
        });

        _mockPipeline.IdentityScopes.Add(new IdentityResources.OpenId());

        _mockPipeline.Users.Add(new TestUser
        {
            SubjectId = "bob",
            Username = "bob",
            Claims =
            [
                new Claim("name", "Bob Loblaw"),
                new Claim("email", "bob@loblaw.com"),
                new Claim("role", "Attorney")
            ]
        });
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task Reject_redirect_uri_not_matching_registered_redirect_uri()
    {
        await _mockPipeline.LoginAsync("bob");

        var nonce = Guid.NewGuid().ToString();
        var state = Guid.NewGuid().ToString();

        var url = IdentityServerPipeline.CreateAuthorizeUrl(
            "code_client",
            "code",
            "openid",
            "https://bad",
            state,
            nonce);
        var response = await _mockPipeline.BrowserClient.GetAsync(url, TestContext.Current.CancellationToken);

        _mockPipeline.ErrorWasCalled.Should().BeTrue();
        _mockPipeline.ErrorMessage.Error.Should().Be("invalid_request");
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task Reject_request_without_redirect_uri_when_multiple_registered()
    {
        await _mockPipeline.LoginAsync("bob");

        var nonce = Guid.NewGuid().ToString();
        var state = Guid.NewGuid().ToString();

        var url = IdentityServerPipeline.CreateAuthorizeUrl(
            "code_client",
            "code",
            "openid",
            // redirectUri deliberately absent 
            null,
            state,
            nonce);
        var response = await _mockPipeline.BrowserClient.GetAsync(url, TestContext.Current.CancellationToken);

        _mockPipeline.ErrorWasCalled.Should().BeTrue();
        _mockPipeline.ErrorMessage.Error.Should().Be("invalid_request");
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task Preserves_query_parameters_in_redirect_uri()
    {
        await _mockPipeline.LoginAsync("bob");

        var nonce = Guid.NewGuid().ToString();
        var state = Guid.NewGuid().ToString();

        _mockPipeline.BrowserClient.AllowAutoRedirect = false;
        var url = IdentityServerPipeline.CreateAuthorizeUrl(
            "code_client",
            "code",
            "openid",
            "https://code_client/callback?foo=bar&baz=quux",
            state,
            nonce);
        var response = await _mockPipeline.BrowserClient.GetAsync(url, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location.ToString().Should().StartWith("https://code_client/callback?");
        var authorization = IdentityServerPipeline.ParseAuthorizationResponseUrl(response.Headers.Location.ToString());
        authorization.Code.Should().NotBeNull();
        authorization.State.Should().Be(state);
        var query = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(response.Headers.Location.Query);
        query["foo"].ToString().Should().Be("bar");
        query["baz"].ToString().Should().Be("quux");
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task Rejects_redirect_uri_when_query_parameter_does_not_match()
    {
        await _mockPipeline.LoginAsync("bob");

        var nonce = Guid.NewGuid().ToString();
        var state = Guid.NewGuid().ToString();

        var url = IdentityServerPipeline.CreateAuthorizeUrl(
            "code_client",
            "code",
            "openid",
            "https://code_client/callback?baz=quux&foo=bar",
            state,
            nonce);
        var response = await _mockPipeline.BrowserClient.GetAsync(url, TestContext.Current.CancellationToken);

        _mockPipeline.ErrorWasCalled.Should().BeTrue();
        _mockPipeline.ErrorMessage.Error.Should().Be("invalid_request");
    }
}