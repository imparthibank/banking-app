using BankingApp.Application.DTOs;
using BankingApp.Application.Common;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace BankingApp.Application.Ports.Input
{
    public interface IBankAccountService
    {
        Task<Result> CreateAsync(CreateBankAccountRequest request);
        Task<Result<IEnumerable<CreateBankAccountRequest>>> GetAllAsync();
        Task<Result<CreateBankAccountRequest>> GetByIdAsync(Guid id);
        Task<Result> UpdateAsync(Guid id, CreateBankAccountRequest request);
        Task<Result> DeleteAsync(Guid id);
    }
}
