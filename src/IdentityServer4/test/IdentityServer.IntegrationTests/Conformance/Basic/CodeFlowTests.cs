// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentAssertions;
using IdentityModel.Client;
using IdentityServer.IntegrationTests.Common;
using IdentityServer4.Configuration;
using IdentityServer4.Models;
using IdentityServer4.Test;
using Xunit;

namespace IdentityServer.IntegrationTests.Conformance.Basic;

public class CodeFlowTests 
{
    private const string Category = "Conformance.Basic.CodeFlowTests";

    private IdentityServerPipeline _pipeline = new();

    public CodeFlowTests()
    {
        _pipeline.IdentityScopes.Add(new IdentityResources.OpenId());
        _pipeline.Clients.Add(new Client
        {
            Enabled = true,
            ClientId = "code_pipeline.Client",
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
                "https://code_pipeline.Client/callback",
                "https://code_pipeline.Client/callback?foo=bar&baz=quux"
            }
        });

        _pipeline.Users.Add(new TestUser
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

        _pipeline.Initialize();
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task No_state_should_not_result_in_shash()
    {
        await _pipeline.LoginAsync("bob");

        var nonce = Guid.NewGuid().ToString();

        _pipeline.BrowserClient.AllowAutoRedirect = false;
        var url = IdentityServerPipeline.CreateAuthorizeUrl(
            "code_pipeline.Client",
            "code",
            "openid",
            "https://code_pipeline.Client/callback?foo=bar&baz=quux",
            nonce: nonce);
        var response = await _pipeline.BrowserClient.GetAsync(url, TestContext.Current.CancellationToken);

        var authorization = IdentityServerPipeline.ParseAuthorizationResponseUrl(response.Headers.Location.ToString());
        authorization.Code.Should().NotBeNull();

        var code = authorization.Code;

        // backchannel client
        var wrapper = new MessageHandlerWrapper(_pipeline.Handler);
        var tokenClient = new HttpClient(wrapper);
        var tokenResult = await tokenClient.RequestAuthorizationCodeTokenAsync(new AuthorizationCodeTokenRequest
        {
            Address = IdentityServerPipeline.TokenEndpoint,
            ClientId = "code_pipeline.Client",
            ClientSecret = "secret",

            Code = code,
            RedirectUri = "https://code_pipeline.Client/callback?foo=bar&baz=quux"
        }, TestContext.Current.CancellationToken);

        tokenResult.IsError.Should().BeFalse();
        tokenResult.HttpErrorReason.Should().Be("OK");
        tokenResult.TokenType.Should().Be("Bearer");
        tokenResult.AccessToken.Should().NotBeNull();
        tokenResult.ExpiresIn.Should().BeGreaterThan(0);
        tokenResult.IdentityToken.Should().NotBeNull();

        var token = new JwtSecurityToken(tokenResult.IdentityToken);
            
        var s_hash = token.Claims.FirstOrDefault(c => c.Type == "s_hash");
        s_hash.Should().BeNull();
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task State_should_result_in_shash()
    {
        await _pipeline.LoginAsync("bob");

        var nonce = Guid.NewGuid().ToString();

        _pipeline.BrowserClient.AllowAutoRedirect = false;
        var url = IdentityServerPipeline.CreateAuthorizeUrl(
            "code_pipeline.Client",
            "code",
            "openid",
            "https://code_pipeline.Client/callback?foo=bar&baz=quux",
            "state",
            nonce);
        var response = await _pipeline.BrowserClient.GetAsync(url, TestContext.Current.CancellationToken);

        var authorization = IdentityServerPipeline.ParseAuthorizationResponseUrl(response.Headers.Location.ToString());
        authorization.Code.Should().NotBeNull();

        var code = authorization.Code;

        // backchannel client
        var wrapper = new MessageHandlerWrapper(_pipeline.Handler);
        var tokenClient = new HttpClient(wrapper);
        var tokenResult = await tokenClient.RequestAuthorizationCodeTokenAsync(new AuthorizationCodeTokenRequest
        {
            Address = IdentityServerPipeline.TokenEndpoint,
            ClientId = "code_pipeline.Client",
            ClientSecret = "secret",

            Code = code,
            RedirectUri = "https://code_pipeline.Client/callback?foo=bar&baz=quux"
        }, TestContext.Current.CancellationToken);

        tokenResult.IsError.Should().BeFalse();
        tokenResult.HttpErrorReason.Should().Be("OK");
        tokenResult.TokenType.Should().Be("Bearer");
        tokenResult.AccessToken.Should().NotBeNull();
        tokenResult.ExpiresIn.Should().BeGreaterThan(0);
        tokenResult.IdentityToken.Should().NotBeNull();

        var token = new JwtSecurityToken(tokenResult.IdentityToken);
            
        var s_hash = token.Claims.FirstOrDefault(c => c.Type == "s_hash");
        s_hash.Should().NotBeNull();
        s_hash.Value.Should().Be(CryptoHelper.CreateHashClaimValue("state", "RS256"));
    }
}