using Moq;
using NUnit.Framework;
using System.Threading.Tasks;

namespace Tests
{
    public class AuthorizationServiceTests
    {
        private Mock<IAuthorizationManager> _authorizationManagerMock;
        private AuthorizationService _authorizationService;

        [SetUp]
        public void Setup()
        {
            _authorizationManagerMock = new Mock<IAuthorizationManager>();
            _authorizationService = new AuthorizationService(_authorizationManagerMock.Object);
        }

        [Test]
        public async Task CanReadAllFxStream_ShouldReturnSuccess_WhenAuthorized()
        {
            _authorizationManagerMock.Setup(x => x.CanReadAllFxStream())
                .ReturnsAsync(Result.Success(true));

            var result = await _authorizationService.CanReadAllFxStream();

            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(result.Value);
        }

        [Test]
        public async Task CanReadAllFxStream_ShouldReturnFailure_WhenUnauthorized()
        {
            _authorizationManagerMock.Setup(x => x.CanReadAllFxStream())
                .ReturnsAsync(Result.Failure<bool>("Error"));

            var result = await _authorizationService.CanReadAllFxStream();

            Assert.IsTrue(result.IsFailure);
        }

        [Test]
        public async Task CanWriteAllFxStream_ShouldReturnSuccess_WhenAuthorized()
        {
            _authorizationManagerMock.Setup(x => x.CanWriteAllFxStream())
                .ReturnsAsync(Result.Success(true));

            var result = await _authorizationService.CanWriteAllFxStream();

            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(result.Value);
        }

        [Test]
        public async Task CanWriteAllFxStream_ShouldReturnFailure_WhenUnauthorized()
        {
            _authorizationManagerMock.Setup(x => x.CanWriteAllFxStream())
                .ReturnsAsync(Result.Failure<bool>("Error"));

            var result = await _authorizationService.CanWriteAllFxStream();

            Assert.IsTrue(result.IsFailure);
        }

        [Test]
        public async Task CanWriteFxStreamProfile_ShouldReturnSuccess_WhenAllPermissionsExist()
        {
            _authorizationManagerMock.Setup(x => x.CanWriteAllFxStream())
                .ReturnsAsync(Result.Success(true));
            _authorizationManagerMock.Setup(x => x.CanWriteFxStreamStandardProfile())
                .ReturnsAsync(Result.Success(true));

            var result = await _authorizationService.CanWriteFxStreamProfile(Constants.StandardProfileType);

            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(result.Value);
        }

        [Test]
        public async Task CanWriteFxStreamProfile_ShouldReturnFailure_WhenWriteAllFails()
        {
            _authorizationManagerMock.Setup(x => x.CanWriteAllFxStream())
                .ReturnsAsync(Result.Failure<bool>("Fail"));

            var result = await _authorizationService.CanWriteFxStreamProfile(Constants.StandardProfileType);

            Assert.IsTrue(result.IsFailure);
        }

        [Test]
        public async Task CanWriteFxStreamProfile_ShouldReturnFailure_WhenStandardProfileFails()
        {
            _authorizationManagerMock.Setup(x => x.CanWriteAllFxStream())
                .ReturnsAsync(Result.Success(true));
            _authorizationManagerMock.Setup(x => x.CanWriteFxStreamStandardProfile())
                .ReturnsAsync(Result.Success(false));

            var result = await _authorizationService.CanWriteFxStreamProfile(Constants.StandardProfileType);

            Assert.IsTrue(result.IsFailure);
        }
    }
}