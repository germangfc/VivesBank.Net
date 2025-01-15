using ApiFunkosCS.Utils.GenericRepository;
using Microsoft.EntityFrameworkCore;
using VivesBankApi.Database;
using VivesBankApi.Rest.Product.BankAccounts.Models;

namespace VivesBankApi.Rest.Product.BankAccounts.Repositories;

public class AccountsRepository : GenericRepository<BancoDbContext ,Account>, IAccountsRepository
{
    public AccountsRepository(BancoDbContext context, ILogger<AccountsRepository> logger) : base(context, logger)
    {
    }

    public async Task<Account?> getAccountByIbanAsync(string Iban)
    {
        _logger.LogInformation($"Getting account with IBAN: {Iban}");
        return await _dbSet.FirstOrDefaultAsync(a => a.IBAN == Iban);
    }

    public async Task<List<Account?>> getAccountByClientIdAsync(string client)
    {
        _logger.LogInformation($"Getting accounts with user ID: {client}");
        return await _dbSet
            .Where(a => a.ClientId == client)
            .Select(a => (Account?)a) 
            .ToListAsync();
    }

}