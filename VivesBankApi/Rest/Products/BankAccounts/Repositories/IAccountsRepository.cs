using ApiFunkosCS.Utils.GenericRepository;
using VivesBankApi.Products.BankAccounts.Models;
using VivesBankApi.Rest.Products.BankAccounts.Dto;

namespace VivesBankApi.Products.BankAccounts.Repositories;

public interface IAccountsRepository : IGenericRepository<Account>
{
    Task<Account?> getAccountByIbanAsync(String Iban);
    Task<List<Account?>> getAccountByClientIdAsync(String UserId);
}