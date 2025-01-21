namespace VivesBankApi.Rest.Product.CreditCard.Generators;

public class ExpirationDateGenerator
{
    public virtual DateOnly GenerateRandomDate()
    {
        var random = new Random();
        DateOnly today = DateOnly.FromDateTime(DateTime.Today);
        DateOnly futureDate = today.AddYears(5);

        int range = (futureDate.ToDateTime(TimeOnly.MinValue) - today.ToDateTime(TimeOnly.MinValue)).Days;
        return today.AddDays(random.Next(range));
    }
}