using VivesBankApi.Rest.Product.BankAccounts.Dto;

namespace VivesBankApi.Rest.Product.BankAccounts.Services;

public interface IAccountsService
{
    Task<PageResponse<AccountResponse>> GetAccountsAsync(int pageNumber = 0, int pageSize = 10, string sortBy = "id", string direction = "asc");
    Task<AccountResponse> GetAccountByIdAsync(string id);
    Task<List<AccountResponse>> GetAccountByClientIdAsync(string clientId);
    Task<List<AccountCompleteResponse>> GetCompleteAccountByClientIdAsync(string clientId);
    Task<AccountResponse> GetAccountByIbanAsync(string iban);
    Task<List<AccountResponse>> GetMyAccountsAsClientAsync();
    Task<AccountCompleteResponse> GetCompleteAccountByIbanAsync(string iban);
    Task<AccountResponse> CreateAccountAsync(CreateAccountRequest request);
    Task<AccountCompleteResponse> UpdateAccountAsync(string id, UpdateAccountRequest request);
    Task DeleteAccountAsync(string id);
    Task DeleteMyAccountAsync(String iban);
}