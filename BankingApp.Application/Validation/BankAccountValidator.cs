using BankingApp.Application.DTOs;
using BankingApp.Application.Common;
using BankingApp.Core.Ports.Output;
using System;
using System.Threading.Tasks;

namespace BankingApp.Application.Validation
{
    public class BankAccountValidator : IBankAccountValidator
    {
        private readonly IBankAccountRepository _repository;

        public BankAccountValidator(IBankAccountRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result> ValidateAsync(CreateBankAccountRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return Result.Fail("Name is required.");

            if (string.IsNullOrWhiteSpace(request.AccountNumber))
                return Result.Fail("Account Number is required.");

            if (string.IsNullOrWhiteSpace(request.Email))
                return Result.Fail("Email is required.");

            if (string.IsNullOrWhiteSpace(request.MobileNumber))
                return Result.Fail("Mobile Number is required.");

            if (string.IsNullOrWhiteSpace(request.PAN))
                return Result.Fail("PAN is required.");

            if (request.DateOfBirth == default)
                return Result.Fail("Date of Birth is required.");

            if (await _repository.GetByAccountNumberAsync(request.AccountNumber) is not null)
                return Result.Fail("Account Number must be unique.");

            if (await _repository.GetByEmailAsync(request.Email) is not null)
                return Result.Fail("Email must be unique.");

            if (await _repository.GetByMobileAsync(request.MobileNumber) is not null)
                return Result.Fail("Mobile Number must be unique.");

            if (await _repository.GetByPANAsync(request.PAN) is not null)
                return Result.Fail("PAN must be unique.");

            return Result.Success();
        }
    }
}
