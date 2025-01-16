namespace VivesBankApi.Rest.Product.CreditCard.Generators;

public class ExpirationDateGenerator
{
    public DateTime GenerateRandomDate()
    {
        var random = new Random();
        DateTime today = DateTime.Today;
        DateTime futureDate = today.AddYears(5);

        int range = (futureDate - today).Days;
        return today.AddDays(random.Next(range));
    }
}