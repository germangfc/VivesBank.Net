using ApiFunkosCS.Utils.GenericRepository;
using VivesBankApi.Rest.Product.CreditCard.Dto;
using VivesBankApi.Rest.Product.CreditCard.Models;


public interface ICreditCardRepository : IGenericRepository<CreditCard>
{
    Task<CreditCard?> GetByCardNumber(string cardNumber);
}