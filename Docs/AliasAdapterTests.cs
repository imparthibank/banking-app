using System;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Microsoft.Extensions.Logging;

namespace AliasService.Tests
{
    [TestFixture]
    public class AliasAdapterTests
    {
        private Mock<IAliasRepository> _aliasRepositoryMock;
        private Mock<ILogger<IAliasAdapterPort>> _loggerMock;
        private Mock<IAuthorizationService> _authorizationServiceMock;
        private Mock<IAliasValidationService> _validationServiceMock;
        private AliasAdapter _adapter;

        [SetUp]
        public void Setup()
        {
            _aliasRepositoryMock = new Mock<IAliasRepository>();
            _loggerMock = new Mock<ILogger<IAliasAdapterPort>>();
            _authorizationServiceMock = new Mock<IAuthorizationService>();
            _validationServiceMock = new Mock<IAliasValidationService>();

            _adapter = new AliasAdapter(
                _authorizationServiceMock.Object,
                _aliasRepositoryMock.Object,
                _loggerMock.Object,
                _validationServiceMock.Object);
        }

        [Test]
        public async Task CreateOrUpdateAliasAsync_UpdateAlias_Success()
        {
            // Arrange
            var technicalAlias = new TechnicalAlias { AliasId = 10 };

            _authorizationServiceMock.Setup(x => x.CanReadAllFxStream())
                .ReturnsAsync(Result.Success(true));

            _validationServiceMock.Setup(x => x.ValidateUpdateTechnicalAliasRequest(technicalAlias))
                .ReturnsAsync(Result.Success());

            _aliasRepositoryMock.Setup(x => x.CreateOrUpdateAliasAsync(technicalAlias))
                .ReturnsAsync(technicalAlias);

            // Act
            var result = await _adapter.CreateOrUpdateAliasAsync(technicalAlias);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(technicalAlias, result.Value);
        }

        [Test]
        public async Task CreateOrUpdateAliasAsync_CreateAlias_Success()
        {
            // Arrange
            var technicalAlias = new TechnicalAlias { AliasId = 0 };

            _authorizationServiceMock.Setup(x => x.CanReadAllFxStream())
                .ReturnsAsync(Result.Success(true));

            _validationServiceMock.Setup(x => x.ValidateCreateTechnicalAliasRequest(technicalAlias))
                .ReturnsAsync(Result.Success());

            _aliasRepositoryMock.Setup(x => x.CreateOrUpdateAliasAsync(technicalAlias))
                .ReturnsAsync(technicalAlias);

            // Act
            var result = await _adapter.CreateOrUpdateAliasAsync(technicalAlias);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(technicalAlias, result.Value);
        }

        [Test]
        public async Task CreateOrUpdateAliasAsync_FailsAuthorization_ReturnsFailure()
        {
            // Arrange
            var technicalAlias = new TechnicalAlias { AliasId = 1 };
            var authError = new Error("AuthError");

            _authorizationServiceMock.Setup(x => x.CanReadAllFxStream())
                .ReturnsAsync(Result.Failure<bool>(authError));

            // Act
            var result = await _adapter.CreateOrUpdateAliasAsync(technicalAlias);

            // Assert
            Assert.IsTrue(result.IsFailure);
            Assert.AreEqual(authError, result.Error);
        }

        [Test]
        public async Task CreateOrUpdateAliasAsync_UpdateValidationFails_ReturnsFailure()
        {
            // Arrange
            var technicalAlias = new TechnicalAlias { AliasId = 5 };
            var validationError = new Error("Validation failed");

            _authorizationServiceMock.Setup(x => x.CanReadAllFxStream())
                .ReturnsAsync(Result.Success(true));

            _validationServiceMock.Setup(x => x.ValidateUpdateTechnicalAliasRequest(technicalAlias))
                .ReturnsAsync(Result.Failure(validationError));

            // Act
            var result = await _adapter.CreateOrUpdateAliasAsync(technicalAlias);

            // Assert
            Assert.IsTrue(result.IsFailure);
            Assert.AreEqual(validationError, result.Error);
        }

        [Test]
        public async Task CreateOrUpdateAliasAsync_CreateValidationFails_ReturnsFailure()
        {
            // Arrange
            var technicalAlias = new TechnicalAlias { AliasId = 0 };
            var validationError = new Error("Validation failed");

            _authorizationServiceMock.Setup(x => x.CanReadAllFxStream())
                .ReturnsAsync(Result.Success(true));

            _validationServiceMock.Setup(x => x.ValidateCreateTechnicalAliasRequest(technicalAlias))
                .ReturnsAsync(Result.Failure(validationError));

            // Act
            var result = await _adapter.CreateOrUpdateAliasAsync(technicalAlias);

            // Assert
            Assert.IsTrue(result.IsFailure);
            Assert.AreEqual(validationError, result.Error);
        }

        [Test]
        public void CreateOrUpdateAliasAsync_RepositoryThrowsException_ShouldThrow()
        {
            // Arrange
            var technicalAlias = new TechnicalAlias { AliasId = 0 };

            _authorizationServiceMock.Setup(x => x.CanReadAllFxStream())
                .ReturnsAsync(Result.Success(true));

            _validationServiceMock.Setup(x => x.ValidateCreateTechnicalAliasRequest(technicalAlias))
                .ReturnsAsync(Result.Success());

            _aliasRepositoryMock.Setup(x => x.CreateOrUpdateAliasAsync(technicalAlias))
                .ThrowsAsync(new Exception("DB Failure"));

            // Act & Assert
            Assert.ThrowsAsync<Exception>(async () => await _adapter.CreateOrUpdateAliasAsync(technicalAlias));
        }
    }

    // Placeholder classes to make the tests compile.
    public class TechnicalAlias { public int AliasId { get; set; } }
    public class Error
    {
        public string Message { get; }
        public Error(string message) => Message = message;
        public override bool Equals(object obj) => obj is Error other && other.Message == Message;
        public override int GetHashCode() => Message.GetHashCode();
    }
    public class Result<T, E>
    {
        public T Value { get; }
        public E Error { get; }
        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;

        private Result(T value, E error, bool isSuccess)
        {
            Value = value;
            Error = error;
            IsSuccess = isSuccess;
        }

        public static Result<T, E> Success(T value) => new Result<T, E>(value, default, true);
        public static Result<T, E> Failure(E error) => new Result<T, E>(default, error, false);
    }
    public static class Result
    {
        public static Result<T, Error> Success<T>(T value) => Result<T, Error>.Success(value);
        public static Result<T, Error> Failure<T>(Error error) => Result<T, Error>.Failure(error);
    }

    // Interfaces for mocking
    public interface IAliasRepository
    {
        Task<TechnicalAlias> CreateOrUpdateAliasAsync(TechnicalAlias alias);
    }
    public interface IAuthorizationService
    {
        Task<Result<bool, Error>> CanReadAllFxStream();
    }
    public interface IAliasValidationService
    {
        Task<Result<object, Error>> ValidateUpdateTechnicalAliasRequest(TechnicalAlias alias);
        Task<Result<object, Error>> ValidateCreateTechnicalAliasRequest(TechnicalAlias alias);
    }
    public interface IAliasAdapterPort { }

    // The class under test
    public class AliasAdapter : IAliasAdapterPort
    {
        private readonly IAliasRepository _aliasRepository;
        private readonly ILogger<IAliasAdapterPort> _logger;
        private readonly IAuthorizationService _authorizationService;
        private readonly IAliasValidationService _validationService;

        public AliasAdapter(
            IAuthorizationService authorizationService,
            IAliasRepository aliasRepository,
            ILogger<IAliasAdapterPort> logger,
            IAliasValidationService validationService)
        {
            _validationService = validationService;
            _authorizationService = authorizationService;
            _aliasRepository = aliasRepository;
            _logger = logger;
        }

        public async Task<Result<TechnicalAlias, Error>> CreateOrUpdateAliasAsync(TechnicalAlias technicalAlias)
        {
            _logger.LogInformation($"{nameof(AliasAdapter)} - {nameof(CreateOrUpdateAliasAsync)} started");

            var resultCanReadAllFxStream = await _authorizationService.CanReadAllFxStream();
            if (resultCanReadAllFxStream.IsFailure)
            {
                return Result.Failure<TechnicalAlias, Error>(resultCanReadAllFxStream.Error);
            }

            if (technicalAlias.AliasId > 0)
            {
                var validationResult = await _validationService.ValidateUpdateTechnicalAliasRequest(technicalAlias);
                if (validationResult.IsFailure)
                {
                    return Result.Failure<TechnicalAlias, Error>(validationResult.Error);
                }
            }
            else
            {
                var validationResult = await _validationService.ValidateCreateTechnicalAliasRequest(technicalAlias);
                if (validationResult.IsFailure)
                {
                    return Result.Failure<TechnicalAlias, Error>(validationResult.Error);
                }
            }

            var aliasResult = await _aliasRepository.CreateOrUpdateAliasAsync(technicalAlias);
            _logger.LogInformation($"{nameof(AliasAdapter)} - {nameof(CreateOrUpdateAliasAsync)} ended");
            return Result.Success<TechnicalAlias, Error>(aliasResult);
        }
    }
}
