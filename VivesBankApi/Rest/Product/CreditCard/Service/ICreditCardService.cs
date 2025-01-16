using VivesBankApi.Rest.Product.CreditCard.Dto;

namespace VivesBankApi.Rest.Product.CreditCard.Service;

public interface ICreditCardService
{
    Task<List<CreditCardAdminResponse>> GetAllCreditCardAdminAsync();
    //Task<List<CreditCardClientResponse>> GetAllCreditCardClientAsync();
    Task<CreditCardAdminResponse?> GetCreditCardByIdAdminAsync(string id);
    Task<CreditCardClientResponse> CreateCreditCardAsync(CreditCardRequest createRequest);
    Task<CreditCardClientResponse> UpdateCreditCardAsync(String cardId, CreditCardRequest updateRequest);
    Task DeleteCreditCardAsync(String cardId);
}