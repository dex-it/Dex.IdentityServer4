using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Entities;
using IdentityServer4.EntityFramework.Interfaces;
using IdentityServer4.EntityFramework.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace IdentityServer4.EntityFramework.IntegrationTests.TokenCleanup
{
    public class TokenCleanupTests : IntegrationTest<TokenCleanupTests, PersistedGrantDbContext, OperationalStoreOptions>
    {
        public TokenCleanupTests(DatabaseProviderFixture<PersistedGrantDbContext> fixture) : base(fixture)
        {
            InitializeDatabases();
        }

        [Theory, MemberData(nameof(TestDatabaseProviders))]
        public async Task RemoveExpiredGrantsAsync_WhenExpiredGrantsExist_ExpectExpiredGrantsRemoved(DbContextOptions<PersistedGrantDbContext> options)
        {
            var expiredGrant = new PersistedGrant
            {
                Key = Guid.NewGuid().ToString(),
                ClientId = "app1",
                Type = "reference",
                SubjectId = "123",
                Expiration = DateTime.UtcNow.AddDays(-3),
                Data = "{!}"
            };

            await using (var context = new PersistedGrantDbContext(options, StoreOptions))
            {
                context.PersistedGrants.Add(expiredGrant);
                await context.SaveChangesAsync();
            }

            await CreateSut(options).RemoveExpiredGrantsAsync();

            await using (var context = new PersistedGrantDbContext(options, StoreOptions))
            {
                context.PersistedGrants.FirstOrDefault(x => x.Key == expiredGrant.Key).Should().BeNull();
            }
        }

        [Theory, MemberData(nameof(TestDatabaseProviders))]
        public async Task RemoveExpiredGrantsAsync_WhenValidGrantsExist_ExpectValidGrantsInDb(DbContextOptions<PersistedGrantDbContext> options)
        {
            var validGrant = new PersistedGrant
            {
                Key = Guid.NewGuid().ToString(),
                ClientId = "app1",
                Type = "reference",
                SubjectId = "123",
                Expiration = DateTime.UtcNow.AddDays(3),
                Data = "{!}"
            };

            await using (var context = new PersistedGrantDbContext(options, StoreOptions))
            {
                context.PersistedGrants.Add(validGrant);
                await context.SaveChangesAsync();
            }

            await CreateSut(options).RemoveExpiredGrantsAsync();

            await using (var context = new PersistedGrantDbContext(options, StoreOptions))
            {
                context.PersistedGrants.FirstOrDefault(x => x.Key == validGrant.Key).Should().NotBeNull();
            }
        }

        [Theory, MemberData(nameof(TestDatabaseProviders))]
        public async Task RemoveExpiredGrantsAsync_WhenExpiredDeviceGrantsExist_ExpectExpiredDeviceGrantsRemoved(DbContextOptions<PersistedGrantDbContext> options)
        {
            var expiredGrant = new DeviceFlowCodes
            {
                DeviceCode = Guid.NewGuid().ToString(),
                UserCode = Guid.NewGuid().ToString(),
                ClientId = "app1",
                SubjectId = "123",
                CreationTime = DateTime.UtcNow.AddDays(-4),
                Expiration = DateTime.UtcNow.AddDays(-3),
                Data = "{!}"
            };

            await using (var context = new PersistedGrantDbContext(options, StoreOptions))
            {
                context.DeviceFlowCodes.Add(expiredGrant);
                await context.SaveChangesAsync();
            }

            await CreateSut(options).RemoveExpiredGrantsAsync();

            await using (var context = new PersistedGrantDbContext(options, StoreOptions))
            {
                context.DeviceFlowCodes.FirstOrDefault(x => x.DeviceCode == expiredGrant.DeviceCode).Should().BeNull();
            }
        }

        [Theory, MemberData(nameof(TestDatabaseProviders))]
        public async Task RemoveExpiredGrantsAsync_WhenValidDeviceGrantsExist_ExpectValidDeviceGrantsInDb(DbContextOptions<PersistedGrantDbContext> options)
        {
            var validGrant = new DeviceFlowCodes
            {
                DeviceCode = Guid.NewGuid().ToString(),
                UserCode = "2468",
                ClientId = "app1",
                SubjectId = "123",
                CreationTime = DateTime.UtcNow.AddDays(-4),
                Expiration = DateTime.UtcNow.AddDays(3),
                Data = "{!}"
            };

            await using (var context = new PersistedGrantDbContext(options, StoreOptions))
            {
                context.DeviceFlowCodes.Add(validGrant);
                await context.SaveChangesAsync();
            }

            await CreateSut(options).RemoveExpiredGrantsAsync();

            await using (var context = new PersistedGrantDbContext(options, StoreOptions))
            {
                context.DeviceFlowCodes.FirstOrDefault(x => x.DeviceCode == validGrant.DeviceCode).Should().NotBeNull();
            }
        }

        private TokenCleanupService CreateSut(DbContextOptions<PersistedGrantDbContext> options)
        {
            IServiceCollection services = new ServiceCollection();
            services.AddIdentityServer();

            services.AddScoped<IPersistedGrantDbContext, PersistedGrantDbContext>(_ =>
                new PersistedGrantDbContext(options, StoreOptions));

            services.AddTransient<TokenCleanupService>();
            services.AddSingleton(StoreOptions);

            return services.BuildServiceProvider().GetRequiredService<TokenCleanupService>();
        }

        private void InitializeDatabases()
        {
            var dbProviders = TestDatabaseProviders
                .SelectMany(x => x.Select(y => (DbContextOptions<PersistedGrantDbContext>)y))
                .ToList();

            dbProviders.ForEach(EnsureDatabaseCreated);
        }

        private void EnsureDatabaseCreated(DbContextOptions<PersistedGrantDbContext> options)
        {
            using var context = new PersistedGrantDbContext(options, StoreOptions);
            context.Database.EnsureCreated();
        }
    }
}