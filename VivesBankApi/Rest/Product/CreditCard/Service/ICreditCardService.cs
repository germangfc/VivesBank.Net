using VivesBankApi.Rest.Product.CreditCard.Dto;

namespace VivesBankApi.Rest.Product.CreditCard.Service;

public interface ICreditCardService
{
    Task<List<CreditCardAdminResponse>> GetAllCreditCardAdminAsync();
    //Task<List<CreditCardClientResponse>> GetAllCreditCardClientAsync();
    Task<CreditCardAdminResponse?> GetCreditCardByIdAdminAsync(string id);
    Task<CreditCardClientResponse> CreateProductAsync(CreditCardRequest createRequest);
    Task<CreditCardClientResponse> UpdateProductAsync(String cardId, CreditCardRequest updateRequest);
    Task DeleteProductAsync(String cardId);
}