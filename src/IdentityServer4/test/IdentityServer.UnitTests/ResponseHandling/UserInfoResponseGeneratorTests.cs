﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using IdentityServer.UnitTests.Common;
using IdentityServer4;
using IdentityServer4.Models;
using IdentityServer4.ResponseHandling;
using IdentityServer4.Stores;
using IdentityServer4.Validation;
using Xunit;

namespace IdentityServer.UnitTests.ResponseHandling;

public class UserInfoResponseGeneratorTests
{
    private UserInfoResponseGenerator _subject;
    private MockProfileService _mockProfileService = new();
    private ClaimsPrincipal _user;
    private Client _client;

    private InMemoryResourcesStore _resourceStore;
    private List<IdentityResource> _identityResources = [];
    private List<ApiResource> _apiResources = [];
    private List<ApiScope> _apiScopes = [];

    public UserInfoResponseGeneratorTests()
    {
        _client = new Client
        {
            ClientId = "client"
        };

        _user = new IdentityServerUser("bob")
        {
            AdditionalClaims =
            {
                new Claim("foo", "foo1"),
                new Claim("foo", "foo2"),
                new Claim("bar", "bar1"),
                new Claim("bar", "bar2")
            }
        }.CreatePrincipal();

        _resourceStore = new InMemoryResourcesStore(_identityResources, _apiResources, _apiScopes);
        _subject = new UserInfoResponseGenerator(_mockProfileService, _resourceStore, TestLogger.Create<UserInfoResponseGenerator>());
    }

    [Fact]
    public async Task GetRequestedClaimTypesAsync_when_no_scopes_requested_should_return_empty_claim_types()
    {
        var resources = await _subject.GetRequestedResourcesAsync(null);
        var claims = await _subject.GetRequestedClaimTypesAsync(resources);
        claims.Should().BeEquivalentTo();
    }

    [Fact]
    public async Task GetRequestedClaimTypesAsync_should_return_correct_identity_claims()
    {
        _identityResources.Add(new IdentityResource("id1", ["c1", "c2"]));
        _identityResources.Add(new IdentityResource("id2", ["c2", "c3"]));

        var resources = await _subject.GetRequestedResourcesAsync(["id1", "id2", "id3"]);
        var claims = await _subject.GetRequestedClaimTypesAsync(resources);
        claims.Should().BeEquivalentTo("c1", "c2", "c3");
    }

    [Fact]
    public async Task GetRequestedClaimTypesAsync_should_only_return_enabled_identity_claims()
    {
        _identityResources.Add(new IdentityResource("id1", ["c1", "c2"]) { Enabled = false });
        _identityResources.Add(new IdentityResource("id2", ["c2", "c3"]));

        var resources = await _subject.GetRequestedResourcesAsync(["id1", "id2", "id3"]);
        var claims = await _subject.GetRequestedClaimTypesAsync(resources);
        claims.Should().BeEquivalentTo("c2", "c3");
    }

    [Fact]
    public async Task ProcessAsync_should_call_profile_service_with_requested_claim_types()
    {
        _identityResources.Add(new IdentityResource("id1", ["foo"]));
        _identityResources.Add(new IdentityResource("id2", ["bar"]));

        var result = new UserInfoRequestValidationResult
        {
            Subject = _user,
            TokenValidationResult = new TokenValidationResult
            {
                Claims = new List<Claim>
                {
                    new("scope", "id1"),
                    new("scope", "id2"),
                    new("scope", "id3")
                },
                Client = _client
            }
        };

        var claims = await _subject.ProcessAsync(result);

        _mockProfileService.GetProfileWasCalled.Should().BeTrue();
        _mockProfileService.ProfileContext.RequestedClaimTypes.Should().BeEquivalentTo("foo", "bar");
    }

    [Fact]
    public async Task ProcessAsync_should_return_claims_issued_by_profile_service()
    {
        _identityResources.Add(new IdentityResource("id1", ["foo"]));
        _identityResources.Add(new IdentityResource("id2", ["bar"]));
            
        var address = new
        {
            street_address = "One Hacker Way",
            locality = "Heidelberg",
            postal_code = 69118,
            country = "Germany"
        };
            
        _mockProfileService.ProfileClaims =
        [
            new Claim("email", "fred@gmail.com"),
            new Claim("name", "fred jones"),
            new Claim("address", "{ 'street_address': 'One Hacker Way', 'locality': 'Heidelberg', 'postal_code': 69118, 'country': 'Germany' }", IdentityServerConstants.ClaimValueTypes.Json),
            new Claim("address2", JsonSerializer.Serialize(address), IdentityServerConstants.ClaimValueTypes.Json)
        ];

        var result = new UserInfoRequestValidationResult
        {
            Subject = _user,
            TokenValidationResult = new TokenValidationResult
            {
                Claims = new List<Claim>
                {
                    new("scope", "id1"),
                    new("scope", "id2"),
                    new("scope", "id3")
                },
                Client = _client
            }
        };

        var claims = await _subject.ProcessAsync(result);

        claims.Should().ContainKey("email");
        claims["email"].Should().Be("fred@gmail.com");
        claims.Should().ContainKey("name");
        claims["name"].Should().Be("fred jones");
            
        // this will be treated as a string because this is not valid JSON from the System.Text library point of view
        claims.Should().ContainKey("address");
        claims["address"].Should().Be("{ 'street_address': 'One Hacker Way', 'locality': 'Heidelberg', 'postal_code': 69118, 'country': 'Germany' }");
            
        // this is a JsonElement
        claims.Should().ContainKey("address2");
        claims["address2"].ToString().Should().Be("{\"street_address\":\"One Hacker Way\",\"locality\":\"Heidelberg\",\"postal_code\":69118,\"country\":\"Germany\"}");
    }

    [Fact]
    public async Task ProcessAsync_should_return_sub_from_user()
    {
        _identityResources.Add(new IdentityResource("id1", ["foo"]));
        _identityResources.Add(new IdentityResource("id2", ["bar"]));

        var result = new UserInfoRequestValidationResult
        {
            Subject = _user,
            TokenValidationResult = new TokenValidationResult
            {
                Claims = new List<Claim>
                {
                    new("scope", "id1"),
                    new("scope", "id2"),
                    new("scope", "id3")
                },
                Client = _client
            }
        };

        var claims = await _subject.ProcessAsync(result);

        claims.Should().ContainKey("sub");
        claims["sub"].Should().Be("bob");
    }

    [Fact]
    public async Task ProcessAsync_should_throw_if_incorrect_sub_issued_by_profile_service()
    {
        _identityResources.Add(new IdentityResource("id1", ["foo"]));
        _identityResources.Add(new IdentityResource("id2", ["bar"]));
        _mockProfileService.ProfileClaims =
        [
            new Claim("sub", "fred")
        ];

        var result = new UserInfoRequestValidationResult
        {
            Subject = _user,
            TokenValidationResult = new TokenValidationResult
            {
                Claims = new List<Claim>
                {
                    new("scope", "id1"),
                    new("scope", "id2"),
                    new("scope", "id3")
                },
                Client = _client
            }
        };

        Func<Task> act = () => _subject.ProcessAsync(result);

       (await act.Should().ThrowAsync<InvalidOperationException>())
            .And.Message.Should().Contain("subject");
    }

}