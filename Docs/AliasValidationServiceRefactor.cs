
/// <summary>
/// Contract for alias input validations for create, update, unassign operations.
/// </summary>
public interface IAliasValidationService
{
    /// <summary>
    /// Validates input for updating an alias.
    /// </summary>
    Task<Result<bool, Error>> ValidateUpdateRequestAsync(TechnicalAlias alias);

    /// <summary>
    /// Validates input for creating a new alias.
    /// </summary>
    Task<Result<bool, Error>> ValidateCreateRequestAsync(TechnicalAlias alias);

    /// <summary>
    /// Validates the alias string.
    /// </summary>
    Task<Result<bool, Error>> ValidateAliasAsync(string alias);

    /// <summary>
    /// Validates the media string.
    /// </summary>
    Task<Result<bool, Error>> ValidateMediaAsync(string media);

    /// <summary>
    /// Validates the reason for change object.
    /// </summary>
    Task<Result<bool, Error>> ValidateReasonForChangeAsync(ReasonForChange reason);

    /// <summary>
    /// Validates if dictionary values contain whitespace/null.
    /// </summary>
    Result<bool, Error> ValidateNoWhitespace(Dictionary<string, string> inputs);

    /// <summary>
    /// Composite validator for unassigning alias mapping.
    /// </summary>
    Task<Result<bool, Error>> ValidateUnAssignAliasProfileMappingInputsAsync(
        ReasonForChange reason, string product, string media, string alias);
}

/// <summary>
/// Implementation of IAliasValidationService for alias operations validation.
/// </summary>
public class AliasValidationService : IAliasValidationService
{
    private readonly IReasonForChangeRepositoryPort _reasonForChangeRepo;

    public AliasValidationService(IReasonForChangeRepositoryPort reasonRepo)
    {
        _reasonForChangeRepo = reasonRepo;
    }

    public Task<Result<bool, Error>> ValidateUpdateRequestAsync(TechnicalAlias alias)
    {
        // TODO: Implement alias update validation logic
        throw new NotImplementedException();
    }

    public Task<Result<bool, Error>> ValidateCreateRequestAsync(TechnicalAlias alias)
    {
        // TODO: Implement alias creation validation logic
        throw new NotImplementedException();
    }

    public Task<Result<bool, Error>> ValidateAliasAsync(string alias)
    {
        // TODO: Implement alias validation logic
        throw new NotImplementedException();
    }

    public Task<Result<bool, Error>> ValidateMediaAsync(string media)
    {
        // TODO: Implement media validation logic
        throw new NotImplementedException();
    }

    public Task<Result<bool, Error>> ValidateReasonForChangeAsync(ReasonForChange reason)
    {
        if (reason == null)
            return Task.FromResult(Result.Failure<bool, Error>(ErrorHelper.ReasonForChangeCannotBeNull));

        if (reason.ReasonId == 0)
            return Task.FromResult(Result.Failure<bool, Error>(ErrorHelper.ReasonIdCannotBeNullOrZero));

        return _reasonForChangeRepo.ValidateFxProfileChangeReasonId(reason.ReasonId)
            .ContinueWith(task => task.Result
                ? Result.Success<bool, Error>(true)
                : Result.Failure<bool, Error>(ErrorHelper.ReasonIdNotValid));
    }

    public Result<bool, Error> ValidateNoWhitespace(Dictionary<string, string> inputs)
    {
        foreach (var entry in inputs)
        {
            if (string.IsNullOrWhiteSpace(entry.Value))
                return Result.Failure<bool, Error>(ErrorHelper.RequiredFieldCannotBeWhitespace(entry.Key));
        }
        return Result.Success<bool, Error>(true);
    }

    public async Task<Result<bool, Error>> ValidateUnAssignAliasProfileMappingInputsAsync(
        ReasonForChange reason, string product, string media, string alias)
    {
        var whitespaceResult = ValidateNoWhitespace(new Dictionary<string, string>
        {
            {"Alias", alias},
            {"Media", media},
            {"Product", product}
        });

        if (whitespaceResult.IsFailure)
            return whitespaceResult;

        var aliasResult = await ValidateAliasAsync(alias);
        if (aliasResult.IsFailure)
            return aliasResult;

        var mediaResult = await ValidateMediaAsync(media);
        if (mediaResult.IsFailure)
            return mediaResult;

        var reasonResult = await ValidateReasonForChangeAsync(reason);
        if (reasonResult.IsFailure)
            return reasonResult;

        return Result.Success<bool, Error>(true);
    }
}
