
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

public class AliasManager : IAliasPort
{
    private readonly IAliasRepositoryPort _aliasRepository;
    private readonly ILogger<AliasManager> _logger;
    private readonly IAuthorizationService _authorizationService;
    private readonly IReasonForChangeRepositoryPort _reasonForChangeRepositoryPort;
    private readonly IAliasValidationService _validationService;

    public AliasManager(
        IAliasRepositoryPort aliasRepository,
        ILogger<AliasManager> logger,
        IAuthorizationService authorizationService,
        IReasonForChangeRepositoryPort reasonForChangeRepositoryPort,
        IAliasValidationService validationService)
    {
        _aliasRepository = aliasRepository ?? throw new ArgumentNullException(nameof(aliasRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
        _reasonForChangeRepositoryPort = reasonForChangeRepositoryPort ?? throw new ArgumentNullException(nameof(reasonForChangeRepositoryPort));
        _validationService = validationService ?? throw new ArgumentNullException(nameof(validationService));
    }

    /// <summary>
    /// Creates or updates a technical alias based on the presence of an AliasId.
    /// Performs input validation and authorization before processing.
    /// </summary>
    /// <param name="technicalAlias">The technical alias object to create or update.</param>
    /// <returns>A success result containing the alias or a failure with error details.</returns>
    public async Task<Result<TechnicalAlias, Error>> CreateOrUpdateAliasAsync(TechnicalAlias technicalAlias)
    {
        _logger.LogInformation($"{nameof(CreateOrUpdateAliasAsync)} started");

        // Permission check
        var permissionCheck = await _authorizationService.CanEditAliasFXStream();
        if (!permissionCheck.IsFailure) return Result.Failure<TechnicalAlias, Error>(permissionCheck.Error);

        // Delegated validation logic to validation service
        var validationResult = technicalAlias.AliasId > 0 ?
            await _validationService.ValidateUpdateRequestAsync(technicalAlias) :
            await _validationService.ValidateCreateRequestAsync(technicalAlias);

        if (validationResult.IsFailure)
            return Result.Failure<TechnicalAlias, Error>(validationResult.Error);

        var result = await _aliasRepository.CreateOrUpdateAliasAsync(technicalAlias);
        _logger.LogInformation($"{nameof(CreateOrUpdateAliasAsync)} ended");
        return Result.Success(result);
    }

    /// <summary>
    /// Fetches alias information grouped by profile ID.
    /// </summary>
    /// <param name="filter">The filter object for alias retrieval.</param>
    /// <returns>Grouped dictionary of alias values by profile ID.</returns>
    public async Task<Result<Dictionary<int, List<string>>, Error>> GetAliasInfoAsync(TechnicalAliasFilter filter)
    {
        _logger.LogInformation($"{nameof(GetAliasInfoAsync)} started");

        // Authorization validation
        var permissionCheck = await _authorizationService.CanReadAliasFXStream();
        if (permissionCheck.IsFailure)
            return Result.Failure<Dictionary<int, List<string>>, Error>(permissionCheck.Error);

        var values = await _aliasRepository.GetAliasInfoAsync(filter);
        var result = values.GroupBy(x => x.ProfileId)
                           .ToDictionary(g => g.Key, g => g.Select(v => v.Value).ToList());

        _logger.LogInformation($"{nameof(GetAliasInfoAsync)} ended");
        return Result.Success(result);
    }

    /// <summary>
    /// Gets alias dimension values grouped by dimension key.
    /// </summary>
    /// <returns>Dictionary of dimensions and corresponding values.</returns>
    public async Task<Result<Dictionary<string, List<string>>, Error>> GetAliasDimensionsAsync()
    {
        _logger.LogInformation($"{nameof(GetAliasDimensionsAsync)} started");

        // Authorization validation
        var permissionCheck = await _authorizationService.CanReadAliasFXStream();
        if (permissionCheck.IsFailure)
            return Result.Failure<Dictionary<string, List<string>>, Error>(permissionCheck.Error);

        var values = await _aliasRepository.GetAliasDimensionsAsync();
        var result = values.GroupBy(x => x.Key)
                           .ToDictionary(g => g.Key, g => g.Select(v => v.Value).ToList());

        _logger.LogInformation($"{nameof(GetAliasDimensionsAsync)} ended");
        return Result.Success(result);
    }

    /// <summary>
    /// Validates and unassigns an alias profile mapping.
    /// Validation logic is fully delegated to the validation service.
    /// </summary>
    /// <param name="reasonForChange">Reason for unassignment.</param>
    /// <param name="product">The product name.</param>
    /// <param name="media">The media type.</param>
    /// <param name="alias">The alias to unassign.</param>
    /// <param name="gridDataType">Grid data type for unassignment context.</param>
    /// <returns>True if successful, false otherwise with error message.</returns>
    public async Task<Result<bool, Error>> UnAssignAliasProfileMappingAsync(
        ReasonForChange reasonForChange, string product, string media, string alias, string gridDataType)
    {
        _logger.LogInformation($"{nameof(UnAssignAliasProfileMappingAsync)} started");

        // Authorization validation
        var permissionCheck = await _authorizationService.CanEditAliasFXStream();
        if (permissionCheck.IsFailure)
            return Result.Failure<bool, Error>(permissionCheck.Error);

        // Validation handled by validation service
        var validationResult = await _validationService.ValidateUnAssignAliasProfileMappingInputsAsync(reasonForChange, product, media, alias);
        if (validationResult.IsFailure)
            return Result.Failure<bool, Error>(validationResult.Error);

        await _aliasRepository.UnAssignAliasProfileMappingAsync(reasonForChange, product, media, alias, gridDataType);

        _logger.LogInformation($"{nameof(UnAssignAliasProfileMappingAsync)} ended");
        return Result.Success<bool, Error>(true);
    }
}
