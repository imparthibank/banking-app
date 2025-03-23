using System;

namespace BankingApp.Core.Entities
{
    public class BankAccount
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string AccountNumber { get; set; }
        public string Email { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string? Nominee { get; set; }
        public string MobileNumber { get; set; }
        public string PAN { get; set; }
    }
}
