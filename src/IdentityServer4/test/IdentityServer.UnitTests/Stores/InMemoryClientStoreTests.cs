using IdentityServer4.Models;
using IdentityServer4.Stores;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Xunit;
using FluentAssertions;

namespace IdentityServer.UnitTests.Stores;

[SuppressMessage("ReSharper", "ObjectCreationAsStatement")]
public class InMemoryClientStoreTests
{
    [Fact]
    public void InMemoryClient_should_throw_if_contain_duplicate_client_ids()
    {
        List<Client> clients =
        [
            new() { ClientId = "1" },
            new() { ClientId = "1" },
            new() { ClientId = "3" }
        ];

        Action act = () => new InMemoryClientStore(clients);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void InMemoryClient_should_not_throw_if_does_not_contain_duplicate_client_ids()
    {
        List<Client> clients =
        [
            new() { ClientId = "1" },
            new() { ClientId = "2" },
            new() { ClientId = "3" }
        ];

        new InMemoryClientStore(clients);
    }
}