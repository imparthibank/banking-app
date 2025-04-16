using NUnit.Framework;
using Moq;
using BankingApp.Application.DTOs;
using BankingApp.Application.Validation;
using BankingApp.Core.Ports.Output;
using BankingApp.Core.Entities;
using System;
using System.Threading.Tasks;

namespace BankingApp.Application.Tests.Validation
{
    public class BankAccountValidatorTests
    {
        private BankAccountValidator _validator;
        private Mock<IBankAccountRepository> _repoMock;

        [SetUp]
        public void Setup()
        {
            _repoMock = new Mock<IBankAccountRepository>();
            _validator = new BankAccountValidator(_repoMock.Object);
        }

        [Test]
        public async Task ValidateAsync_MissingName_ReturnsFail()
        {
            var request = new CreateBankAccountRequest { Name = "", AccountNumber = "123", Email = "test@test.com", MobileNumber = "111", PAN = "PAN1234", DateOfBirth = DateTime.Now };
            var result = await _validator.ValidateAsync(request);

            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual("Name is required.", result.Error);
        }

        [Test]
        public async Task ValidateAsync_DuplicateEmail_ReturnsFail()
        {
            var request = new CreateBankAccountRequest
            {
                Name = "Test",
                AccountNumber = "123",
                Email = "test@test.com",
                MobileNumber = "111",
                PAN = "PAN1234",
                DateOfBirth = DateTime.Today
            };

            _repoMock.Setup(r => r.GetByEmailAsync("test@test.com"))
                     .ReturnsAsync(new BankAccount());

            var result = await _validator.ValidateAsync(request);

            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual("Email must be unique.", result.Error);
        }
    }
}
