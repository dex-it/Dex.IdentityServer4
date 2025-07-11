using IdentityServer4.Models;
using IdentityServer4.Stores;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Xunit;
using FluentAssertions;

namespace IdentityServer.UnitTests.Stores;

[SuppressMessage("ReSharper", "ObjectCreationAsStatement")]
public class InMemoryResourcesStoreTests
{
    [Fact]
    public void InMemoryResourcesStore_should_throw_if_contains_duplicate_names()
    {
        List<IdentityResource> identityResources =
        [
            new() { Name = "A" },
            new() { Name = "A" },
            new() { Name = "C" }
        ];

        List<ApiResource> apiResources =
        [
            new() { Name = "B" },
            new() { Name = "B" },
            new() { Name = "C" }
        ];

        List<ApiScope> scopes =
        [
            new() { Name = "B" },
            new() { Name = "C" },
            new() { Name = "C" }
        ];

        Action act = () => new InMemoryResourcesStore(identityResources);
        act.Should().Throw<ArgumentException>();

        act = () => new InMemoryResourcesStore(null, apiResources);
        act.Should().Throw<ArgumentException>();

        act = () => new InMemoryResourcesStore(null, null, scopes);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void InMemoryResourcesStore_should_not_throw_if_does_not_contains_duplicate_names()
    {
        List<IdentityResource> identityResources =
        [
            new() { Name = "A" },
            new() { Name = "B" },
            new() { Name = "C" }
        ];

        List<ApiResource> apiResources =
        [
            new() { Name = "A" },
            new() { Name = "B" },
            new() { Name = "C" }
        ];

        List<ApiScope> apiScopes =
        [
            new() { Name = "A" },
            new() { Name = "B" },
            new() { Name = "C" }
        ];

        new InMemoryResourcesStore(identityResources);
        new InMemoryResourcesStore(null, apiResources);
        new InMemoryResourcesStore(null, null, apiScopes);
    }
}