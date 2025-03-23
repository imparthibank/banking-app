using BankingApp.Core.Entities;
using System;
using System.Collections.Generic;

namespace BankingApp.Application.DTOs
{
    public class BankAccountReponse
    {
        public List<BankAccount> BankAccounts { get; set; }
    }
}
