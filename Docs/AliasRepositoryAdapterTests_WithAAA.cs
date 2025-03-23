
using System;
using System.Data;
using System.Threading.Tasks;
using AutoMapper;
using Dapper;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

[TestFixture]
public class AliasRepositoryAdapterTests
{
    private Mock<IDbConnection> _dbConnectionMock;
    private Mock<ILogger<BaseRepository>> _loggerMock;
    private Mock<IMapper> _mapperMock;
    private AliasRepositoryAdapter _repository;

    [SetUp]
    public void Setup()
    {
        _dbConnectionMock = new Mock<IDbConnection>();
        _loggerMock = new Mock<ILogger<BaseRepository>>();
        _mapperMock = new Mock<IMapper>();
        _repository = new AliasRepositoryAdapter(() => _dbConnectionMock.Object, _loggerMock.Object, _mapperMock.Object);
    }

    

    [Test]
    // Arrange
    public async Task CreateOrUpdateAliasAsync_ValidAlias_ReturnsUpdatedAlias()
    {
        var alias = new TechnicalAlias();
        var dbMock = new Mock<IDbTransaction>();
        _dbConnectionMock.Setup(c => c.BeginTransaction(IsolationLevel.Serializable)).Returns(dbMock.Object);
        _dbConnectionMock.Setup(c => c.Open());

        int expectedAliasId = 100;

        _repository.QuerySingleAsync<int>(It.IsAny<string>(), It.IsAny<object>())
                   .ReturnsAsync(expectedAliasId);

        // Act
        var result = await _repository.CreateOrUpdateAliasAsync(alias);

        // Assert
        Assert.AreEqual(expectedAliasId, result.AliasId);
    }

    

    [Test]
    // Arrange
    public async Task CreateOrUpdateAliasAsync_InsertFails_LogsErrorAndRollsBack()
    {
        var alias = new TechnicalAlias();
        var dbMock = new Mock<IDbTransaction>();
        _dbConnectionMock.Setup(c => c.BeginTransaction(IsolationLevel.Serializable)).Returns(dbMock.Object);
        _dbConnectionMock.Setup(c => c.Open());

        _repository.QuerySingleAsync<int>(It.IsAny<string>(), It.IsAny<object>())
                   .ReturnsAsync(0);

        // Act
        var result = await _repository.CreateOrUpdateAliasAsync(alias);

        _loggerMock.Verify(l => l.LogError(It.IsAny<string>()), Times.Once);
        // Assert
        Assert.AreEqual(0, result.AliasId);
    }

    

    [Test]
    // Arrange
    public async Task CreateOrUpdateAliasAsync_ExceptionThrown_LogsErrorAndRollsBack()
    {
        var alias = new TechnicalAlias();
        var dbMock = new Mock<IDbTransaction>();
        _dbConnectionMock.Setup(c => c.BeginTransaction(IsolationLevel.Serializable)).Returns(dbMock.Object);
        _dbConnectionMock.Setup(c => c.Open());

        _repository.QuerySingleAsync<int>(It.IsAny<string>(), It.IsAny<object>())
                   .ThrowsAsync(new Exception("DB Error"));

        // Act
        var result = await _repository.CreateOrUpdateAliasAsync(alias);

        _loggerMock.Verify(l => l.LogError(It.IsAny<string>()), Times.Once);
    }

    

    [Test]
    // Arrange
    public async Task IsDuplicateAliasAndMediaAsync_ReturnsTrue_IfDuplicateExists()
    {
        _repository.QuerySingleAsync<int>(It.IsAny<string>(), It.IsAny<object>())
                   .ReturnsAsync(1);

        // Act
        var result = await _repository.IsDuplicateAliasAndMediaAsync("alias", "media");

        // Assert
        Assert.IsTrue(result);
    }

    

    [Test]
    // Arrange
    public async Task IsDuplicateAliasAndMediaAsync_ReturnsFalse_IfNoDuplicate()
    {
        _repository.QuerySingleAsync<int>(It.IsAny<string>(), It.IsAny<object>())
                   .ReturnsAsync(0);

        // Act
        var result = await _repository.IsDuplicateAliasAndMediaAsync("alias", "media");

        // Assert
        Assert.IsFalse(result);
    }

    

    [Test]
    // Arrange
    public async Task IsDuplicateAliasAndMediaAsync_WithAliasId_ReturnsTrue_IfExists()
    {
        _repository.QuerySingleAsync<int>(It.IsAny<string>(), It.IsAny<object>())
                   .ReturnsAsync(1);

        // Act
        var result = await _repository.IsDuplicateAliasAndMediaAsync("alias", "media", 10);

        // Assert
        Assert.IsTrue(result);
    }

    

    [Test]
    // Arrange
    public async Task AliasIdExistsAsync_ReturnsTrue_IfExists()
    {
        _repository.QuerySingleAsync<int>(It.IsAny<string>(), It.IsAny<object>())
                   .ReturnsAsync(1);

        // Act
        var result = await _repository.AliasIdExistsAsync(10);

        // Assert
        Assert.IsTrue(result);
    }

    

    [Test]
    // Arrange
    public async Task AliasIdExistsAsync_ReturnsFalse_IfNotExists()
    {
        _repository.QuerySingleAsync<int>(It.IsAny<string>(), It.IsAny<object>())
                   .ReturnsAsync(0);

        // Act
        var result = await _repository.AliasIdExistsAsync(10);

        // Assert
        Assert.IsFalse(result);
    }

    

    [Test]
    // Arrange
    public async Task ValidateAliasDimensionsAsync_MediaExists_ReturnsTrue()
    {
        var resultDto = new AliasDimensionValidationResultDto
        {
            MediaExists = true,
            MediaTypeExists = false,
            RegionExists = false,
            TradingStyleExists = false
        };

        _repository.QuerySingleAsync<AliasDimensionValidationResultDto>(It.IsAny<string>(), It.IsAny<object>())
                   .ReturnsAsync(resultDto);

        var (media, mediaType, region, tradingStyle) =
            await _repository.ValidateAliasDimensionsAsync("media", "mediaType", "region", "tradingStyle");

        // Assert
        Assert.IsTrue(media);
        // Assert
        Assert.IsFalse(mediaType);
        // Assert
        Assert.IsFalse(region);
        // Assert
        Assert.IsFalse(tradingStyle);
    }

    

    [Test]
    // Arrange
    public async Task ValidateAliasDimensionsAsync_MediaTypeExists_ReturnsTrue()
    {
        var resultDto = new AliasDimensionValidationResultDto
        {
            MediaExists = false,
            MediaTypeExists = true,
            RegionExists = false,
            TradingStyleExists = false
        };

        _repository.QuerySingleAsync<AliasDimensionValidationResultDto>(It.IsAny<string>(), It.IsAny<object>())
                   .ReturnsAsync(resultDto);

        var (media, mediaType, region, tradingStyle) =
            await _repository.ValidateAliasDimensionsAsync("media", "mediaType", "region", "tradingStyle");

        // Assert
        Assert.IsFalse(media);
        // Assert
        Assert.IsTrue(mediaType);
        // Assert
        Assert.IsFalse(region);
        // Assert
        Assert.IsFalse(tradingStyle);
    }

    

    [Test]
    // Arrange
    public async Task ValidateAliasDimensionsAsync_RegionExists_ReturnsTrue()
    {
        var resultDto = new AliasDimensionValidationResultDto
        {
            MediaExists = false,
            MediaTypeExists = false,
            RegionExists = true,
            TradingStyleExists = false
        };

        _repository.QuerySingleAsync<AliasDimensionValidationResultDto>(It.IsAny<string>(), It.IsAny<object>())
                   .ReturnsAsync(resultDto);

        var (media, mediaType, region, tradingStyle) =
            await _repository.ValidateAliasDimensionsAsync("media", "mediaType", "region", "tradingStyle");

        // Assert
        Assert.IsFalse(media);
        // Assert
        Assert.IsFalse(mediaType);
        // Assert
        Assert.IsTrue(region);
        // Assert
        Assert.IsFalse(tradingStyle);
    }

    

    [Test]
    // Arrange
    public async Task ValidateAliasDimensionsAsync_TradingStyleExists_ReturnsTrue()
    {
        var resultDto = new AliasDimensionValidationResultDto
        {
            MediaExists = false,
            MediaTypeExists = false,
            RegionExists = false,
            TradingStyleExists = true
        };

        _repository.QuerySingleAsync<AliasDimensionValidationResultDto>(It.IsAny<string>(), It.IsAny<object>())
                   .ReturnsAsync(resultDto);

        var (media, mediaType, region, tradingStyle) =
            await _repository.ValidateAliasDimensionsAsync("media", "mediaType", "region", "tradingStyle");

        // Assert
        Assert.IsFalse(media);
        // Assert
        Assert.IsFalse(mediaType);
        // Assert
        Assert.IsFalse(region);
        // Assert
        Assert.IsTrue(tradingStyle);
    }
}
