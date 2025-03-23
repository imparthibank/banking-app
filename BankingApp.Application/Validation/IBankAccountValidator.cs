using BankingApp.Application.DTOs;
using BankingApp.Application.Common;
using System.Threading.Tasks;

namespace BankingApp.Application.Validation
{
    public interface IBankAccountValidator
    {
        Task<Result> ValidateAsync(CreateBankAccountRequest request);
    }
}
