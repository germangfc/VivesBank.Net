using ApiFunkosCS.Utils.GenericRepository;
using VivesBankApi.Rest.Product.BankAccounts.Models;

namespace VivesBankApi.Rest.Product.BankAccounts.Repositories;

public interface IAccountsRepository : IGenericRepository<Account>
{
    Task<Account?> getAccountByIbanAsync(String Iban);
    Task<List<Account?>> getAccountByClientIdAsync(String UserId);
}