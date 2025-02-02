using VivesBankApi.Rest.Product.CreditCard.Dto;
using VivesBankApi.Utils.GenericStorage.JSON;

namespace VivesBankApi.Rest.Product.CreditCard.Service;

public interface ICreditCardService : IGenericStorageJson<Models.CreditCard>
{
    Task<List<CreditCardAdminResponse>> GetAllCreditCardAdminAsync(int pageNumber,
        int pageSize,
        string fullName,
        bool? isDeleted,
        string direction);
    Task<List<Models.CreditCard>> GetAll();

    //Task<List<CreditCardClientResponse>> GetAllCreditCardClientAsync();
    Task<List<CreditCardClientResponse?>> GetMyCreditCardsAsync();
    Task<CreditCardAdminResponse?> GetCreditCardByIdAdminAsync(string id);
    Task<CreditCardAdminResponse?> GetCreditCardByCardNumber(string cardNumber);
    Task<CreditCardClientResponse> CreateCreditCardAsync(CreditCardRequest createRequest);
    Task<CreditCardClientResponse> UpdateCreditCardAsync(String cardNumber, CreditCardUpdateRequest updateRequest);
    Task DeleteCreditCardAsync(String cardId);
}