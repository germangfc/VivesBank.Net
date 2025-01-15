using VivesBankApi.Rest.Product.BankAccounts.Dto;

namespace VivesBankApi.Rest.Product.BankAccounts.Services;

public interface IAccountsService
{
    Task<List<AccountResponse>> GetAccountsAsync();
    Task<AccountResponse> GetAccountByIdAsync(string id);
    Task<List<AccountResponse>> GetAccountByClientIdAsync(string clientId);
    Task<AccountResponse> GetAccountByIbanAsync(string iban);
    Task<AccountResponse> CreateAccountAsync(CreateAccountRequest request);
    Task DeleteAccountAsync(string id);
}