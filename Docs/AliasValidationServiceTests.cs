
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
            .ReturnsAsync(new AliasDimensionValidationResult { MediaExists = true, MediaTypeExists = true, RegionExists = true, TradingStyleExists = true });

        var result = await _aliasValidationService.ValidateCreateTechnicalAliasRequest(model);

        Assert.IsTrue(result.IsSuccess);
    }

    [Test]
    public async Task ValidateCreateTechnicalAliasRequest_ShouldFail_WhenAliasIsEmpty()
    {
        var model = new TechnicalAlias { Alias = "", Media = "Media1", MediaType = "M1", Region = "R1", TradingStyle = "T1" };

        var result = await _aliasValidationService.ValidateCreateTechnicalAliasRequest(model);

        Assert.IsTrue(result.IsFailure);
        Assert.AreEqual(ErrorHelper.AliasNameCannotBeNullOrEmpty, result.Error);
    }

    [Test]
    public async Task ValidateCreateTechnicalAliasRequest_ShouldFail_WhenMediaDimensionIsInvalid()
    {
        var model = new TechnicalAlias { Alias = "Alias", Media = "Invalid", MediaType = "Valid", Region = "Valid", TradingStyle = "Valid" };

        _aliasRepositoryMock.Setup(r => r.IsDuplicateAliasAndMediaAsync(model.Alias, model.Media)).ReturnsAsync(false);
        _aliasRepositoryMock.Setup(r => r.ValidateAliasDimensionsAsync(model.Media, model.MediaType, model.Region, model.TradingStyle))
            .ReturnsAsync(new AliasDimensionValidationResult { MediaExists = false, MediaTypeExists = true, RegionExists = true, TradingStyleExists = true });

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
            .ReturnsAsync(new AliasDimensionValidationResult { MediaExists = true, MediaTypeExists = true, RegionExists = true, TradingStyleExists = true });

        var result = await _aliasValidationService.ValidateUpdateTechnicalAliasRequest(model);

        Assert.IsTrue(result.IsSuccess);
    }

    [Test]
    public async Task ValidateUpdateTechnicalAliasRequest_ShouldFail_WhenAliasIdInvalid()
    {
        var model = new TechnicalAlias { AliasId = 99, Alias = "Alias", Media = "M1", MediaType = "MT", Region = "R1", TradingStyle = "TS" };

        _aliasRepositoryMock.Setup(r => r.AliasIdExistsAsync(model.AliasId)).ReturnsAsync(false);

        var result = await _aliasValidationService.ValidateUpdateTechnicalAliasRequest(model);

        Assert.IsTrue(result.IsFailure);
        Assert.AreEqual(ErrorHelper.AliasIdShouldBeValid, result.Error);
    }

    [Test]
    public async Task ValidateUpdateTechnicalAliasRequest_ShouldFail_WhenMediaTypeDimensionInvalid()
    {
        var model = new TechnicalAlias { AliasId = 1, Alias = "Alias", Media = "Media", MediaType = "Invalid", Region = "Valid", TradingStyle = "Valid" };

        _aliasRepositoryMock.Setup(r => r.AliasIdExistsAsync(model.AliasId)).ReturnsAsync(true);
        _aliasRepositoryMock.Setup(r => r.IsDuplicateAliasAndMediaAsync(model.Alias, model.Media, model.AliasId)).ReturnsAsync(false);
        _aliasRepositoryMock.Setup(r => r.ValidateAliasDimensionsAsync(model.Media, model.MediaType, model.Region, model.TradingStyle))
            .ReturnsAsync(new AliasDimensionValidationResult { MediaExists = true, MediaTypeExists = false, RegionExists = true, TradingStyleExists = true });

        var result = await _aliasValidationService.ValidateUpdateTechnicalAliasRequest(model);

        Assert.IsTrue(result.IsFailure);
        Assert.AreEqual(ErrorHelper.MediaTypeDimensionShouldBeValid, result.Error);
    }

    [Test]
    public async Task ValidateUpdateTechnicalAliasRequest_ShouldFail_WhenRegionDimensionInvalid()
    {
        var model = new TechnicalAlias { AliasId = 1, Alias = "Alias", Media = "Media", MediaType = "MT", Region = "Invalid", TradingStyle = "TS" };

        _aliasRepositoryMock.Setup(r => r.AliasIdExistsAsync(model.AliasId)).ReturnsAsync(true);
        _aliasRepositoryMock.Setup(r => r.IsDuplicateAliasAndMediaAsync(model.Alias, model.Media, model.AliasId)).ReturnsAsync(false);
        _aliasRepositoryMock.Setup(r => r.ValidateAliasDimensionsAsync(model.Media, model.MediaType, model.Region, model.TradingStyle))
            .ReturnsAsync(new AliasDimensionValidationResult { MediaExists = true, MediaTypeExists = true, RegionExists = false, TradingStyleExists = true });

        var result = await _aliasValidationService.ValidateUpdateTechnicalAliasRequest(model);

        Assert.IsTrue(result.IsFailure);
        Assert.AreEqual(ErrorHelper.RegionDimensionShouldBeValid, result.Error);
    }

    [Test]
    public async Task ValidateUpdateTechnicalAliasRequest_ShouldFail_WhenTradingStyleDimensionInvalid()
    {
        var model = new TechnicalAlias { AliasId = 1, Alias = "Alias", Media = "Media", MediaType = "MT", Region = "R", TradingStyle = "Invalid" };

        _aliasRepositoryMock.Setup(r => r.AliasIdExistsAsync(model.AliasId)).ReturnsAsync(true);
        _aliasRepositoryMock.Setup(r => r.IsDuplicateAliasAndMediaAsync(model.Alias, model.Media, model.AliasId)).ReturnsAsync(false);
        _aliasRepositoryMock.Setup(r => r.ValidateAliasDimensionsAsync(model.Media, model.MediaType, model.Region, model.TradingStyle))
            .ReturnsAsync(new AliasDimensionValidationResult { MediaExists = true, MediaTypeExists = true, RegionExists = true, TradingStyleExists = false });

        var result = await _aliasValidationService.ValidateUpdateTechnicalAliasRequest(model);

        Assert.IsTrue(result.IsFailure);
        Assert.AreEqual(ErrorHelper.TradingStyleDimensionShouldBeValid, result.Error);
    }
}
