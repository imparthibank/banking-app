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
        public async Task CanReadAllFxStream_ShouldReturnSuccess_WhenAuthorizationPasses()
        {
            _authorizationManagerMock.Setup(x => x.CanReadAllFxStream())
                .ReturnsAsync(Result.Success(true));

            var result = await _authorizationService.CanReadAllFxStream();

            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(result.Value);
        }

        [Test]
        public async Task CanReadAllFxStream_ShouldReturnFailure_WhenAuthorizationFails()
        {
            _authorizationManagerMock.Setup(x => x.CanReadAllFxStream())
                .ReturnsAsync(Result.Failure<bool>("Some error"));

            var result = await _authorizationService.CanReadAllFxStream();

            Assert.IsTrue(result.IsFailure);
        }

        [Test]
        public async Task CanWriteFxStreamProfile_ShouldReturnSuccess_WhenAuthorizedAndProfileIsStandard()
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
                .ReturnsAsync(Result.Failure<bool>("Not authorized"));

            var result = await _authorizationService.CanWriteFxStreamProfile(Constants.StandardProfileType);

            Assert.IsTrue(result.IsFailure);
        }

        [Test]
        public async Task CanWriteFxStreamProfile_ShouldReturnFailure_WhenStandardProfileWriteFails()
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