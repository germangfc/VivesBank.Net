﻿using VivesBankApi.Rest.Product.CreditCard.Dto;

namespace VivesBankApi.Rest.Product.CreditCard.Service;

public interface ICreditCardService
{
    Task<List<CreditCardAdminResponse>> GetAllCreditCardAdminAsync(int pageNumber,
        int pageSize,
        string fullName,
        bool? isDeleted,
        string direction);
    //Task<List<CreditCardClientResponse>> GetAllCreditCardClientAsync();
    Task<CreditCardAdminResponse?> GetCreditCardByIdAdminAsync(string id);
    Task<CreditCardAdminResponse?> GetCreditCardByCardNumber(string cardNumber);
    Task<CreditCardClientResponse> CreateCreditCardAsync(CreditCardRequest createRequest);
    Task<CreditCardClientResponse> UpdateCreditCardAsync(String cardId, CreditCardUpdateRequest updateRequest);
    Task DeleteCreditCardAsync(String cardId);
}