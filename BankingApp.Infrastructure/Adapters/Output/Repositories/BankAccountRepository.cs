using Dapper;
using BankingApp.Core.Entities;
using BankingApp.Core.Ports.Output;
using BankingApp.Infrastructure.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;

namespace BankingApp.Infrastructure.Adapters.Output.Repositories
{
    public class BankAccountRepository : IBankAccountRepository
    {
        private readonly ConnectionFactory _connectionFactory;
        private readonly ILogger<BankAccountRepository> _logger;

        public BankAccountRepository(ConnectionFactory connectionFactory, ILogger<BankAccountRepository> logger)
        {
            _connectionFactory = connectionFactory;
            _logger = logger;
        }

        public async Task AddAsync(BankAccount account)
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = @"INSERT INTO bank_accounts (id, name, account_number, email, date_of_birth, nominee, mobile_number, pan)
                        VALUES (@Id, @Name, @AccountNumber, @Email, @DateOfBirth, @Nominee, @MobileNumber, @PAN)";
            await connection.ExecuteAsync(sql, account);
        }

        public async Task DeleteAsync(Guid id)
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = "DELETE FROM bank_accounts WHERE id = @Id";
            await connection.ExecuteAsync(sql, new { Id = id });
        }

        public async Task<IEnumerable<BankAccount>> GetAllAsync()
        {
            using var connection = _connectionFactory.CreateConnection();
            //var sql = "SELECT * FROM bank_accounts";
            var sql = @"SELECT 
                id AS Id,
                name AS Name,
                account_number AS AccountNumber,
                email AS Email,
                date_of_birth AS DateOfBirth,
                nominee AS Nominee,
                mobile_number AS MobileNumber,
                pan AS PAN
            FROM bank_accounts";
            var result = await connection.QueryAsync<BankAccount>(sql);
            return result;
        }

        public async Task<BankAccount> GetByIdAsync(Guid id)
        {
            _logger.LogInformation("Start >> Fetching BankAccount with ID: {Id}", id);

            using var connection = _connectionFactory.CreateConnection();
            var sql = "SELECT * FROM bank_accounts WHERE id = @Id";
            var result = await connection.QueryFirstOrDefaultAsync<BankAccount>(sql, new { Id = id });

            _logger.LogInformation("Query result: {@Result}", result);

            return result;
        }

        public async Task<BankAccount> GetByAccountNumberAsync(string accountNumber)
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = "SELECT * FROM bank_accounts WHERE account_number = @AccountNumber";
            return await connection.QueryFirstOrDefaultAsync<BankAccount>(sql, new { AccountNumber = accountNumber });
        }

        public async Task<BankAccount> GetByEmailAsync(string email)
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = "SELECT * FROM bank_accounts WHERE email = @Email";
            return await connection.QueryFirstOrDefaultAsync<BankAccount>(sql, new { Email = email });
        }

        public async Task<BankAccount> GetByMobileAsync(string mobileNumber)
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = "SELECT * FROM bank_accounts WHERE mobile_number = @MobileNumber";
            return await connection.QueryFirstOrDefaultAsync<BankAccount>(sql, new { MobileNumber = mobileNumber });
        }

        public async Task<BankAccount> GetByPANAsync(string pan)
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = "SELECT * FROM bank_accounts WHERE pan = @PAN";
            return await connection.QueryFirstOrDefaultAsync<BankAccount>(sql, new { PAN = pan });
        }

        public async Task UpdateAsync(BankAccount account)
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = @"UPDATE bank_accounts
                        SET name = @Name, email = @Email, date_of_birth = @DateOfBirth, 
                            nominee = @Nominee, mobile_number = @MobileNumber, pan = @PAN
                        WHERE id = @Id";
            await connection.ExecuteAsync(sql, account);
        }
    }
}
