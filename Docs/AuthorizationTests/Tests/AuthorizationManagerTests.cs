using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tests
{
    public class AuthorizationManagerTests
    {
        private Mock<ILoggedInUserPort> _loggedInUserPortMock;
        private AuthorizationManager _authorizationManager;

        [SetUp]
        public void Setup()
        {
            _loggedInUserPortMock = new Mock<ILoggedInUserPort>();
        }

        [Test]
        public async Task CanReadAllFxStream_ShouldReturnSuccess_WhenPermissionExists()
        {
            _loggedInUserPortMock.Setup(x => x.Permissions)
                .Returns(new List<Permission> { new Permission { Name = FxStreamingPermissionConstants.ReadAllStream } });

            _authorizationManager = new AuthorizationManager(_loggedInUserPortMock.Object);

            var result = await _authorizationManager.CanReadAllFxStream();

            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(result.Value);
        }

        [Test]
        public async Task CanReadAllFxStream_ShouldReturnFailure_WhenPermissionDoesNotExist()
        {
            _loggedInUserPortMock.Setup(x => x.Permissions)
                .Returns(new List<Permission>());

            _authorizationManager = new AuthorizationManager(_loggedInUserPortMock.Object);

            var result = await _authorizationManager.CanReadAllFxStream();

            Assert.IsTrue(result.IsSuccess);
            Assert.IsFalse(result.Value);
        }

        [Test]
        public async Task CanWriteFxStreamStandardProfile_ShouldReturnSuccess_WhenPermissionExists()
        {
            _loggedInUserPortMock.Setup(x => x.Permissions)
                .Returns(new List<Permission> { new Permission { Name = ClientPermissionConstants.WriteFxStandardPresets } });

            _authorizationManager = new AuthorizationManager(_loggedInUserPortMock.Object);

            var result = await _authorizationManager.CanWriteFxStreamStandardProfile();

            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(result.Value);
        }
    }
}