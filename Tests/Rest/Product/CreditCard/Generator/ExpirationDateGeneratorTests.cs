using Moq;
using NUnit.Framework.Legacy;
using VivesBankApi.Rest.Product.CreditCard.Generators;

namespace Tests.Rest.Product.CreditCard.Generator;

public class ExpirationDateGeneratorTests
{
    
    [Test]
    public void GenerateRandomDate_ShouldReturnDateInTheFuture()
    {
        var generator = new ExpirationDateGenerator();
        
        var result = generator.GenerateRandomDate();
        var today = DateOnly.FromDateTime(DateTime.Today);
        var futureDate = today.AddYears(5);
        
        ClassicAssert.True(result.Year >= today.Year && result.Year <= futureDate.Year, 
            $"Expected year between {today.Year} and {futureDate.Year}, but got {result.Year}");
        
        ClassicAssert.True(result.Month >= 1 && result.Month <= 12, 
            $"Expected month between 1 and 12, but got {result.Month}");
        
        ClassicAssert.True(result.Day >= 1 && result.Day <= DateTime.DaysInMonth(result.Year, result.Month), 
            $"Expected day to be between 1 and {DateTime.DaysInMonth(result.Year, result.Month)}, but got {result.Day}");
    }
}
