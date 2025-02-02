namespace VivesBankApi.Rest.Product.CreditCard.Generators;

public interface IExpirationDateGenerator
{
    DateOnly GenerateRandomDate();
}