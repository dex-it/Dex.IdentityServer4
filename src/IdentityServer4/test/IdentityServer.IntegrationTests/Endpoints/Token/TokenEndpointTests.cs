﻿using FluentAssertions;
using IdentityServer4.Models;
using IdentityServer4.Test;

using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityServer.IntegrationTests.Common;
using Newtonsoft.Json.Linq;
using Xunit;

namespace IdentityServer.IntegrationTests.Endpoints.Token;

public class TokenEndpointTests
{
    private const string Category = "Token endpoint";

    private string client_id = "client";
    private string client_secret = "secret";

    private string scope_name = "api";
    private string scope_secret = "api_secret";

    private IdentityServerPipeline _mockPipeline = new();

    public TokenEndpointTests()
    {
        _mockPipeline.Clients.Add(new Client
        {
            ClientId = client_id,
            ClientSecrets = new List<Secret> { new(client_secret.Sha256()) },
            AllowedGrantTypes = { GrantType.ClientCredentials, GrantType.ResourceOwnerPassword },
            AllowedScopes = new List<string> { "api" },
        });


        _mockPipeline.Users.Add(new TestUser
        {
            SubjectId = "bob",
            Username = "bob",
            Password = "password",
            Claims =
            [
                new Claim("name", "Bob Loblaw"),
                new Claim("email", "bob@loblaw.com"),
                new Claim("role", "Attorney")
            ]
        });

        _mockPipeline.IdentityScopes.AddRange([
            new IdentityResources.OpenId()
        ]);

        _mockPipeline.ApiResources.AddRange([
            new ApiResource
            {
                Name = "api",
                ApiSecrets = new List<Secret> { new(scope_secret.Sha256()) },
                Scopes = {scope_name}
            }
        ]);

        _mockPipeline.ApiScopes.AddRange([
            new ApiScope
            {
                Name = scope_name
            }
        ]);

        _mockPipeline.Initialize();
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task client_credentials_request_with_funny_headers_should_not_hang()
    {
        var data = new Dictionary<string, string>
        {
            { "grant_type", "client_credentials" },
            { "client_id", client_id },
            { "client_secret", client_secret },
            { "scope", scope_name },
        };
        var form = new FormUrlEncodedContent(data);
        _mockPipeline.BackChannelClient.DefaultRequestHeaders.Add("Referer", "http://127.0.0.1:33086/appservice/appservice?t=1564165664142?load");
        var response = await _mockPipeline.BackChannelClient.PostAsync(IdentityServerPipeline.TokenEndpoint, form, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var result = JObject.Parse(json);
        result.ContainsKey("error").Should().BeFalse();
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task resource_owner_request_with_funny_headers_should_not_hang()
    {
        var data = new Dictionary<string, string>
        {
            { "grant_type", "password" },
            { "username", "bob" },
            { "password", "password" },
            { "client_id", client_id },
            { "client_secret", client_secret },
            { "scope", scope_name },
        };
        var form = new FormUrlEncodedContent(data);
        _mockPipeline.BackChannelClient.DefaultRequestHeaders.Add("Referer", "http://127.0.0.1:33086/appservice/appservice?t=1564165664142?load");
        var response = await _mockPipeline.BackChannelClient.PostAsync(IdentityServerPipeline.TokenEndpoint, form, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var result = JObject.Parse(json);
        result.ContainsKey("error").Should().BeFalse();
    }
}