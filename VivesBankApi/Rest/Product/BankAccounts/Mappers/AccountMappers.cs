using VivesBankApi.Rest.Product.BankAccounts.Dto;
using VivesBankApi.Rest.Product.BankAccounts.Models;

namespace VivesBankApi.Rest.Product.BankAccounts.Mappers;

/// <summary>
/// Proporciona funciones de mapeo para transformar entre los modelos de Cuenta y sus respectivos DTOs.
/// Estas funciones permiten la conversión entre objetos del dominio (modelos) y los objetos que se utilizan en las respuestas o solicitudes API.
/// </summary>
/// <remarks>
/// Autor: Raúl Fernández, Javier Hernández, Samuel Cortés, Germán, Álvaro Herrero, Tomás
/// Versión: 1.0
/// </remarks>
public static class AccountMappers
{
    
    /// <summary>
    /// Mapea el modelo de Cuenta a un DTO de AccountResponse para las respuestas de la API.
    /// </summary>
    /// <param name="account">El modelo de Cuenta a ser mapeado.</param>
    /// <returns>Un DTO AccountResponse poblado con los datos de la cuenta.</returns>
    /// <remarks>
    /// Este método transforma un objeto de tipo Account en un DTO que será utilizado para responder a las peticiones de la API.
    /// </remarks>
    /// <example>
    /// Account account = new Account();
    /// AccountResponse response = account.ToResponse();
    /// </example>
    public static AccountResponse toResponse(this Account account)
    {
        return new AccountResponse
        {
            Id = account.Id,
            IBAN = account.IBAN,
            clientID = account.ClientId,
            productID = account.ProductId,
            AccountType = account.AccountType,
            InterestRate = account.InterestRate
        };
    }
    
    /// <summary>
    /// Mapea un DTO de AccountCompleteResponse a un DTO de UpdateAccountRequest para actualizar una cuenta.
    /// </summary>
    /// <param name="account">El DTO AccountCompleteResponse a ser mapeado.</param>
    /// <returns>Un DTO UpdateAccountRequest con los datos necesarios para actualizar una cuenta.</returns>
    /// <remarks>
    /// Este método transforma un DTO de AccountCompleteResponse en un DTO de UpdateAccountRequest
    /// para ser utilizado en el proceso de actualización de cuentas.
    /// </remarks>
    /// <example>
    /// AccountCompleteResponse account = new AccountCompleteResponse();
    /// UpdateAccountRequest updateRequest = account.ToUpdateAccountRequest();
    /// </example>
    public static UpdateAccountRequest toUpdateAccountRequest(this AccountCompleteResponse account)
    {
        return new UpdateAccountRequest
        {
            ProductID = account.productID,
            ClientID = account.ClientID,
            TarjetaId = account.TarjetaId,
            IBAN = account.IBAN,
            Balance = account.Balance,
            AccountType = account.AccountType
        };
    }
    /// <summary>
    /// Mapea un DTO de CreateAccountRequest a un modelo de Account para crear una nueva cuenta.
    /// </summary>
    /// <param name="createRequest">El DTO CreateAccountRequest a ser mapeado.</param>
    /// <returns>Un modelo Account poblado con los datos necesarios para crear una cuenta.</returns>
    /// <remarks>
    /// Este método transforma un DTO de CreateAccountRequest en un modelo de dominio Account
    /// para ser utilizado en el proceso de creación de cuentas.
    /// </remarks>
    /// <example>
    /// CreateAccountRequest createRequest = new CreateAccountRequest();
    /// Account newAccount = createRequest.FromDtoRequest();
    /// </example>
    public static Account fromDtoRequest(this CreateAccountRequest createRequest)
    {
        return new Account
        {
            ProductId = createRequest.ProductName, 
            AccountType = createRequest.AccountType,
        };
    }
    
    /// <summary>
    /// Mapea un DTO de UpdateAccountRequest a un modelo de Account para actualizar una cuenta existente.
    /// </summary>
    /// <param name="updateRequest">El DTO UpdateAccountRequest a ser mapeado.</param>
    /// <returns>Un modelo Account poblado con los datos necesarios para actualizar una cuenta.</returns>
    /// <remarks>
    /// Este método transforma un DTO de UpdateAccountRequest en un modelo de dominio Account
    /// para ser utilizado en el proceso de actualización de cuentas.
    /// </remarks>
    /// <example>
    /// UpdateAccountRequest updateRequest = new UpdateAccountRequest();
    /// Account account = updateRequest.FromDtoRequest();
    /// </example>
    public static Account fromDtoRequest(this UpdateAccountRequest updateRequest)
    {
        return new Account
        {
            ProductId = updateRequest.ProductID, // Luego en el servicio le cambio el nombre por el id del producto
            ClientId = updateRequest.ClientID,
            TarjetaId = updateRequest.TarjetaId,
            IBAN = updateRequest.IBAN,
            Balance = updateRequest.Balance,
            AccountType = updateRequest.AccountType
        };
    }
    
    /// <summary>
    /// Mapea un modelo de Account a un DTO de AccountCompleteResponse con toda la información detallada de la cuenta.
    /// </summary>
    /// <param name="account">El modelo de Cuenta a ser mapeado.</param>
    /// <returns>Un DTO AccountCompleteResponse con toda la información detallada de la cuenta.</returns>
    /// <remarks>
    /// Este método transforma un objeto de tipo Account en un DTO de AccountCompleteResponse con todos los campos de la cuenta.
    /// </remarks>
    /// <example>
    /// Account account = new Account();
    /// AccountCompleteResponse completeResponse = account.ToCompleteResponse();
    /// </example>
    public static AccountCompleteResponse toCompleteResponse(this Account account)
    {
        return new AccountCompleteResponse
        {
            Id = account.Id,
            IBAN = account.IBAN,
            ClientID = account.ClientId,
            productID = account.ProductId,
            AccountType = account.AccountType,
            InterestRate = account.InterestRate,
            Balance = account.Balance,
            TarjetaId = account.TarjetaId,
            CreatedAt = account.CreatedAt,
            UpdatedAt = account.UpdatedAt,
            IsDeleted = account.IsDeleted
        };
    }
}