
using Moq;
using NUnit.Framework;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Dapper;
using System.Collections.Generic;

[TestFixture]
public class AliasRepositoryAdapterCopyTests
{
    private Mock<Func<IDbConnection>> _mockDbConnectionFactory;
    private Mock<IDbConnection> _mockDbConnection;
    private Mock<IDbTransaction> _mockTransaction;
    private Mock<ILogger<BaseRepository>> _mockLogger;
    private Mock<IMapper> _mockMapper;
    private AliasRepositoryAdapterCopy _repository;

    [SetUp]
    public void Setup()
    {
        _mockDbConnectionFactory = new Mock<Func<IDbConnection>>();
        _mockDbConnection = new Mock<IDbConnection>();
        _mockTransaction = new Mock<IDbTransaction>();
        _mockLogger = new Mock<ILogger<BaseRepository>>();
        _mockMapper = new Mock<IMapper>();

        _mockDbConnection.Setup(c => c.BeginTransaction(It.IsAny<IsolationLevel>())).Returns(_mockTransaction.Object);
        _mockDbConnection.Setup(c => c.Open());

        _mockDbConnectionFactory.Setup(f => f()).Returns(_mockDbConnection.Object);

        _repository = new AliasRepositoryAdapterCopy(
            _mockDbConnectionFactory.Object,
            _mockLogger.Object,
            _mockMapper.Object
        );
    }

    [Test]
    public async Task CreateOrUpdateAliasAsync_ShouldInsert_WhenAliasIsValid()
    {
        // Arrange
        var alias = new TechnicalAlias();
        var expectedId = 1;

        _mockDbConnection.Setup(d => d.QuerySingleAsync<int>(
            It.IsAny<string>(),
            It.IsAny<object>(),
            null, null, null
        )).ReturnsAsync(expectedId);

        // Act
        var result = await _repository.CreateOrUpdateAliasAsync(alias);

        // Assert
        Assert.AreEqual(expectedId, result.AliasId);
        _mockTransaction.Verify(t => t.Commit(), Times.Once);
    }

    [Test]
    public async Task IsDuplicateAliasAndMediaAsync_ShouldReturnTrue_WhenDuplicateExists()
    {
        // Arrange
        _mockDbConnection.Setup(d => d.QuerySingleAsync<int>(
            It.IsAny<string>(),
            It.IsAny<object>(),
            null, null, null
        )).ReturnsAsync(1);

        // Act
        var result = await _repository.IsDuplicateAliasAndMediaAsync("alias", "media");

        // Assert
        Assert.IsTrue(result);
    }

    [Test]
    public async Task IsDuplicateAliasAndMediaAsync_ShouldReturnFalse_WhenNoDuplicateExists()
    {
        _mockDbConnection.Setup(d => d.QuerySingleAsync<int>(
            It.IsAny<string>(),
            It.IsAny<object>(),
            null, null, null
        )).ReturnsAsync(0);

        var result = await _repository.IsDuplicateAliasAndMediaAsync("alias", "media");

        Assert.IsFalse(result);
    }

    [Test]
    public async Task AliasIdExistsAsync_ShouldReturnTrue_WhenAliasIdExists()
    {
        _mockDbConnection.Setup(d => d.QuerySingleAsync<int>(
            It.IsAny<string>(),
            It.IsAny<object>(),
            null, null, null
        )).ReturnsAsync(1);

        var result = await _repository.AliasIdExistsAsync(123);

        Assert.IsTrue(result);
    }

    [Test]
    public async Task AliasIdExistsAsync_ShouldReturnFalse_WhenAliasIdDoesNotExist()
    {
        _mockDbConnection.Setup(d => d.QuerySingleAsync<int>(
            It.IsAny<string>(),
            It.IsAny<object>(),
            null, null, null
        )).ReturnsAsync(0);

        var result = await _repository.AliasIdExistsAsync(123);

        Assert.IsFalse(result);
    }

    [Test]
    public async Task ValidateAliasDimensionsAsync_ShouldReturnExpectedValidationResult()
    {
        var expected = new
        {
            MediaExists = true,
            MediaTypeExists = false,
            RegionExists = true,
            TradingStyleExists = false
        };

        _mockDbConnection.Setup(d => d.QuerySingleAsync<dynamic>(
            It.IsAny<string>(),
            It.IsAny<object>(),
            null, null, null
        )).ReturnsAsync(expected);

        var result = await _repository.ValidateAliasDimensionsAsync("media", "mediaType", "region", "tradingStyle");

        Assert.AreEqual(expected.MediaExists, result.MediaExists);
        Assert.AreEqual(expected.MediaTypeExists, result.MediaTypeExists);
        Assert.AreEqual(expected.RegionExists, result.RegionExists);
        Assert.AreEqual(expected.TradingStyleExists, result.TradingStyleExists);
    }
}
