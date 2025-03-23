
using Moq;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

[TestFixture]
public class AliasValidationServiceTests
{
    private Mock<IAliasRepository> _aliasRepositoryMock;
    private AliasValidationService _aliasValidationService;

    [SetUp]
    public void SetUp()
    {
        _aliasRepositoryMock = new Mock<IAliasRepository>();
        _aliasValidationService = new AliasValidationService(_aliasRepositoryMock.Object);
    }

    [Test]
    public async Task ValidateCreateTechnicalAliasRequest_ShouldReturnSuccess_WhenAllValidationsPass()
    {
        var model = new TechnicalAlias
        {
            Alias = "TestAlias",
            Media = "Media1",
            MediaType = "MediaType1",
            Region = "Region1",
            TradingStyle = "Style1"
        };

        _aliasRepositoryMock.Setup(r => r.IsDuplicateAliasAndMediaAsync(model.Alias, model.Media)).ReturnsAsync(false);
        _aliasRepositoryMock.Setup(r => r.ValidateAliasDimensionsAsync(model.Media, model.MediaType, model.Region, model.TradingStyle))
            .ReturnsAsync((true, true, true, true));

        var result = await _aliasValidationService.ValidateCreateTechnicalAliasRequest(model);

        Assert.IsTrue(result.IsSuccess);
    }

    [Test]
    public async Task ValidateCreateTechnicalAliasRequest_ShouldFail_WhenMediaDimensionIsInvalid()
    {
        var model = new TechnicalAlias { Alias = "Alias", Media = "Invalid", MediaType = "Valid", Region = "Valid", TradingStyle = "Valid" };

        _aliasRepositoryMock.Setup(r => r.IsDuplicateAliasAndMediaAsync(model.Alias, model.Media)).ReturnsAsync(false);
        _aliasRepositoryMock.Setup(r => r.ValidateAliasDimensionsAsync(model.Media, model.MediaType, model.Region, model.TradingStyle))
            .ReturnsAsync((false, true, true, true));

        var result = await _aliasValidationService.ValidateCreateTechnicalAliasRequest(model);

        Assert.IsTrue(result.IsFailure);
        Assert.AreEqual(ErrorHelper.MediaDimensionShouldBeValid, result.Error);
    }

    [Test]
    public async Task ValidateUpdateTechnicalAliasRequest_ShouldReturnSuccess_WhenAllValidationsPass()
    {
        var model = new TechnicalAlias
        {
            AliasId = 1,
            Alias = "Alias",
            Media = "Media",
            MediaType = "MediaType",
            Region = "Region",
            TradingStyle = "Style"
        };

        _aliasRepositoryMock.Setup(r => r.AliasIdExistsAsync(model.AliasId)).ReturnsAsync(true);
        _aliasRepositoryMock.Setup(r => r.IsDuplicateAliasAndMediaAsync(model.Alias, model.Media, model.AliasId)).ReturnsAsync(false);
        _aliasRepositoryMock.Setup(r => r.ValidateAliasDimensionsAsync(model.Media, model.MediaType, model.Region, model.TradingStyle))
            .ReturnsAsync((true, true, true, true));

        var result = await _aliasValidationService.ValidateUpdateTechnicalAliasRequest(model);

        Assert.IsTrue(result.IsSuccess);
    }

    [Test]
    public async Task ValidateUpdateTechnicalAliasRequest_ShouldFail_WhenTradingStyleDimensionInvalid()
    {
        var model = new TechnicalAlias { AliasId = 1, Alias = "Alias", Media = "Media", MediaType = "MT", Region = "R", TradingStyle = "Invalid" };

        _aliasRepositoryMock.Setup(r => r.AliasIdExistsAsync(model.AliasId)).ReturnsAsync(true);
        _aliasRepositoryMock.Setup(r => r.IsDuplicateAliasAndMediaAsync(model.Alias, model.Media, model.AliasId)).ReturnsAsync(false);
        _aliasRepositoryMock.Setup(r => r.ValidateAliasDimensionsAsync(model.Media, model.MediaType, model.Region, model.TradingStyle))
            .ReturnsAsync((true, true, true, false));

        var result = await _aliasValidationService.ValidateUpdateTechnicalAliasRequest(model);

        Assert.IsTrue(result.IsFailure);
        Assert.AreEqual(ErrorHelper.TradingStyleDimensionShouldBeValid, result.Error);
    }
}

    [Test]
    public async Task ValidateCreateTechnicalAliasRequest_ShouldFail_WhenMediaTypeIsEmpty()
    {
        var model = new TechnicalAlias
        {
            Alias = "Alias1",
            Media = "Media1",
            MediaType = "",
            Region = "Region1",
            TradingStyle = "Style1"
        };

        var result = await _aliasValidationService.ValidateCreateTechnicalAliasRequest(model);

        Assert.IsTrue(result.IsFailure);
        Assert.AreEqual(ErrorHelper.MediaTypeCannotBeNullOrEmpty, result.Error);
    }

    [Test]
    public async Task ValidateCreateTechnicalAliasRequest_ShouldFail_WhenRegionIsEmpty()
    {
        var model = new TechnicalAlias
        {
            Alias = "Alias1",
            Media = "Media1",
            MediaType = "MediaType1",
            Region = "",
            TradingStyle = "Style1"
        };

        var result = await _aliasValidationService.ValidateCreateTechnicalAliasRequest(model);

        Assert.IsTrue(result.IsFailure);
        Assert.AreEqual(ErrorHelper.RegionCannotBeNullOrEmpty, result.Error);
    }

    [Test]
    public async Task ValidateCreateTechnicalAliasRequest_ShouldFail_WhenTradingStyleIsEmpty()
    {
        var model = new TechnicalAlias
        {
            Alias = "Alias1",
            Media = "Media1",
            MediaType = "MediaType1",
            Region = "Region1",
            TradingStyle = ""
        };

        var result = await _aliasValidationService.ValidateCreateTechnicalAliasRequest(model);

        Assert.IsTrue(result.IsFailure);
        Assert.AreEqual(ErrorHelper.TradingStyleCannotBeNullOrEmpty, result.Error);
    }

    [Test]
    public void ValidateCreateTechnicalAliasRequest_ShouldThrowException_WhenRepositoryThrows()
    {
        var model = new TechnicalAlias
        {
            Alias = "Alias1",
            Media = "Media1",
            MediaType = "MediaType1",
            Region = "Region1",
            TradingStyle = "Style1"
        };

        _aliasRepositoryMock.Setup(r => r.IsDuplicateAliasAndMediaAsync(model.Alias, model.Media))
            .ThrowsAsync(new Exception("Simulated DB failure"));

        Assert.ThrowsAsync<Exception>(async () =>
        {
            await _aliasValidationService.ValidateCreateTechnicalAliasRequest(model);
        });
    }

    [Test]
    public void ValidateUpdateTechnicalAliasRequest_ShouldThrowException_WhenDimensionCheckThrows()
    {
        var model = new TechnicalAlias
        {
            AliasId = 1,
            Alias = "Alias1",
            Media = "Media1",
            MediaType = "MediaType1",
            Region = "Region1",
            TradingStyle = "Style1"
        };

        _aliasRepositoryMock.Setup(r => r.AliasIdExistsAsync(model.AliasId)).ReturnsAsync(true);
        _aliasRepositoryMock.Setup(r => r.IsDuplicateAliasAndMediaAsync(model.Alias, model.Media, model.AliasId)).ReturnsAsync(false);
        _aliasRepositoryMock.Setup(r => r.ValidateAliasDimensionsAsync(model.Media, model.MediaType, model.Region, model.TradingStyle))
            .ThrowsAsync(new Exception("Simulated dimension error"));

        Assert.ThrowsAsync<Exception>(async () =>
        {
            await _aliasValidationService.ValidateUpdateTechnicalAliasRequest(model);
        });
    }
}
