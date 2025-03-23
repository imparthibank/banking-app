using AutoMapper;
using BankingApp.Application.DTOs;
using BankingApp.Application.Ports.Input;
using BankingApp.Application.Common;
using BankingApp.Application.Validation;
using BankingApp.Core.Entities;
using BankingApp.Core.Ports.Output;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using Microsoft.Extensions.Logging;

namespace BankingApp.Application.UseCases
{
    public class BankAccountService : IBankAccountService
    {
        private readonly IBankAccountRepository _repository;
        private readonly IBankAccountValidator _validator;
        private readonly IMapper _mapper;
        private readonly ILogger<BankAccountService> _logger;

        public BankAccountService(
            IBankAccountRepository repository,
            IBankAccountValidator validator,
            IMapper mapper,
            ILogger<BankAccountService> logger)
        {
            _repository = repository;
            _validator = validator;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result> CreateAsync(CreateBankAccountRequest request)
        {
            _logger.LogInformation("Creating bank account for: {Email}", request.Email);

            var validation = await _validator.ValidateAsync(request);
            if (!validation.IsSuccess)
            {
                _logger.LogWarning("Validation failed: {Error}", validation.Error);
                return validation;
            }

            var entity = _mapper.Map<BankAccount>(request);
            entity.Id = Guid.NewGuid();
            await _repository.AddAsync(entity);
            _logger.LogInformation("Bank account created with ID: {Id}", entity.Id);
            return Result.Success();
        }

        public async Task<Result<IEnumerable<CreateBankAccountRequest>>> GetAllAsync()
        {
            _logger.LogInformation("GetAllAsync bank accounts");

            var accounts = await _repository.GetAllAsync();
            var dtos = _mapper.Map<IEnumerable<CreateBankAccountRequest>>(accounts);

            _logger.LogInformation("GetAllAsync bank account with list of : {dtos}", dtos);
            return Result<IEnumerable<CreateBankAccountRequest>>.Success(dtos);
        }

        public async Task<Result<CreateBankAccountRequest>> GetByIdAsync(Guid id)
        {
            var account = await _repository.GetByIdAsync(id);
            if (account == null)
                return Result<CreateBankAccountRequest>.Fail("Account not found");

            return Result<CreateBankAccountRequest>.Success(_mapper.Map<CreateBankAccountRequest>(account));
        }

        public async Task<Result> UpdateAsync(Guid id, CreateBankAccountRequest request)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null)
                return Result.Fail("Account not found");

            _mapper.Map(request, existing);
            await _repository.UpdateAsync(existing);
            return Result.Success();
        }

        public async Task<Result> DeleteAsync(Guid id)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null)
                return Result.Fail("Account not found");

            await _repository.DeleteAsync(id);
            return Result.Success();
        }
    }
}
