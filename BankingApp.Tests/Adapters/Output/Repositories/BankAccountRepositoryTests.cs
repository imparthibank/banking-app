using BankingApp.Core.Entities;
using BankingApp.Infrastructure.Adapters.Output.Repositories;
using BankingApp.Infrastructure.Config;
using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Npgsql;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace BankingApp.Tests.Adapters.Output.Repositories
{
    public class BankAccountRepositoryTests
    {
        private BankAccountRepository _repository;
        private Mock<ILogger<BankAccountRepository>> _loggerMock;
        private IDbConnection _connection;

        [SetUp]
        public void Setup()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new[]
                {
                    new KeyValuePair<string, string>("ConnectionStrings:Postgres", "Host=localhost;Port=5432;Database=BankingTestDb;Username=postgres;Password=Admin@123")
                })
                .Build();

            var factory = new ConnectionFactory(config);
            _loggerMock = new Mock<ILogger<BankAccountRepository>>();
            _repository = new BankAccountRepository(factory, _loggerMock.Object);
            _connection = factory.CreateConnection();

            // Clean up before test
            _connection.Execute("DELETE FROM bank_accounts");
        }

        [Test]
        public async Task AddAndGetById_ShouldWorkCorrectly()
        {
            var account = new BankAccount
            {
                Id = Guid.NewGuid(),
                Name = "Test User",
                AccountNumber = "ACCT1001",
                Email = "testuser@example.com",
                DateOfBirth = new DateTime(1990, 1, 1),
                Nominee = "Nominee Name",
                MobileNumber = "9998887776",
                PAN = "PAN1234567"
            };

            await _repository.AddAsync(account);

            var result = await _repository.GetByIdAsync(account.Id);

            Assert.NotNull(result);
            Assert.AreEqual(account.Name, result.Name);
            Assert.AreEqual(account.Email, result.Email);
        }

        [Test]
        public async Task Delete_ShouldRemoveAccount()
        {
            var account = new BankAccount
            {
                Id = Guid.NewGuid(),
                Name = "Delete Me",
                AccountNumber = "ACCT9999",
                Email = "deleteme@example.com",
                DateOfBirth = new DateTime(1985, 1, 1),
                MobileNumber = "1234567890",
                PAN = "DELPAN1234"
            };

            await _repository.AddAsync(account);
            await _repository.DeleteAsync(account.Id);

            var result = await _repository.GetByIdAsync(account.Id);
            Assert.IsNull(result);
        }
    }
}
