﻿using VivesBankApi.Rest.Product.CreditCard.Dto;
using VivesBankApi.Utils.GenericStorage.JSON;

namespace VivesBankApi.Rest.Product.CreditCard.Service;

public interface ICreditCardService : IGenericStorageJson<Models.CreditCard>
{
    Task<List<CreditCardAdminResponse>> GetAllCreditCardAdminAsync();
    //Task<List<CreditCardClientResponse>> GetAllCreditCardClientAsync();
    Task<CreditCardAdminResponse?> GetCreditCardByIdAdminAsync(string id);
    Task<CreditCardAdminResponse?> GetCreditCardByCardNumber(string cardNumber);
    Task<CreditCardClientResponse> CreateCreditCardAsync(CreditCardRequest createRequest);
    Task<CreditCardClientResponse> UpdateCreditCardAsync(String cardId, CreditCardUpdateRequest updateRequest);
    Task DeleteCreditCardAsync(String cardId);
}