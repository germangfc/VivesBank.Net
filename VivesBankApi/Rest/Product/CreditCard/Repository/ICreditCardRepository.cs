using ApiFunkosCS.Utils.GenericRepository;
using VivesBankApi.Rest.Product.CreditCard.Dto;
using VivesBankApi.Rest.Product.CreditCard.Models;


public interface ICreditCardRepository : IGenericRepository<CreditCard>
{
    Task<CreditCard?> GetByCardNumber(string cardNumber);
    public Task<PagedList<CreditCard>> GetAllClientsPagedAsync(
        int pageNumber,
        int pageSize,
        string name,
        bool? isDeleted,
        string direction);
}