using AutoMapper;
using BankingApp.Application.DTOs;
using BankingApp.Application.UseCases;
using BankingApp.Application.Validation;
using BankingApp.Core.Entities;
using BankingApp.Core.Ports.Output;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace BankingApp.Tests.UseCases
{
    public class BankAccountServiceTests
    {
        private BankAccountService _service;
        private Mock<IBankAccountRepository> _repoMock;
        private Mock<IBankAccountValidator> _validatorMock;
        private IMapper _mapper;
        private Mock<ILogger<BankAccountService>> _loggerMock;

        [SetUp]
        public void Setup()
        {
            _repoMock = new Mock<IBankAccountRepository>();
            _validatorMock = new Mock<IBankAccountValidator>();
            _loggerMock = new Mock<ILogger<BankAccountService>>();

            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<CreateBankAccountRequest, BankAccount>().ReverseMap();
            });

            _mapper = config.CreateMapper();

            _service = new BankAccountService(_repoMock.Object, _validatorMock.Object, _mapper, _loggerMock.Object);
        }

        [Test]
        public async Task CreateAsync_ValidRequest_ReturnsSuccess()
        {
            var request = new CreateBankAccountRequest
            {
                Name = "John",
                AccountNumber = "ACC123",
                Email = "john@example.com",
                DateOfBirth = new DateTime(1990, 1, 1),
                MobileNumber = "1234567890",
                PAN = "ABCDE1234F"
            };

            _validatorMock.Setup(v => v.ValidateAsync(request))
                .ReturnsAsync(Application.Common.Result.Success());

            var result = await _service.CreateAsync(request);

            Assert.IsTrue(result.IsSuccess);
            _repoMock.Verify(r => r.AddAsync(It.IsAny<BankAccount>()), Times.Once);
        }
    }
}
