using NUnit.Framework;
using Moq;
using BankingApp.Application.DTOs;
using BankingApp.Application.Ports.Input;
using BankingApp.WebApi.Adapters.Input.Controllers;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System;

namespace BankingApp.Tests.Adapters.Input.Controllers
{
    public class BankAccountControllerTests
    {
        private BankAccountsController _controller;
        private Mock<IBankAccountService> _serviceMock;

        [SetUp]
        public void Setup()
        {
            _serviceMock = new Mock<IBankAccountService>();
            _controller = new BankAccountsController(_serviceMock.Object);
        }

        [Test]
        public async Task Create_ValidRequest_ReturnsOk()
        {
            var request = new CreateBankAccountRequest { Name = "Test", AccountNumber = "123", Email = "test@test.com", MobileNumber = "9999999999", PAN = "PAN9999", DateOfBirth = DateTime.Today };

            _serviceMock.Setup(s => s.CreateAsync(request))
                        .ReturnsAsync(Application.Common.Result.Success());

            var result = await _controller.Create(request);

            Assert.IsInstanceOf<OkResult>(result);
        }

        [Test]
        public async Task GetById_NotFound_ReturnsNotFound()
        {
            _serviceMock.Setup(s => s.GetByIdAsync(It.IsAny<Guid>()))
                        .ReturnsAsync(Application.Common.Result<Application.DTOs.CreateBankAccountRequest>.Fail("Not found"));

            var result = await _controller.Get(Guid.NewGuid());

            Assert.IsInstanceOf<NotFoundObjectResult>(result);
        }
    }
}
