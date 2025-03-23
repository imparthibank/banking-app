using BankingApp.Core.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BankingApp.Core.Ports.Output
{
    public interface IBankAccountRepository
    {
        Task<BankAccount> GetByAccountNumberAsync(string accountNumber);
        Task<BankAccount> GetByEmailAsync(string email);
        Task<BankAccount> GetByMobileAsync(string mobileNumber);
        Task<BankAccount> GetByPANAsync(string pan);
        Task<IEnumerable<BankAccount>> GetAllAsync();
        Task<BankAccount> GetByIdAsync(Guid id);
        Task AddAsync(BankAccount account);
        Task UpdateAsync(BankAccount account);
        Task DeleteAsync(Guid id);
    }
}
