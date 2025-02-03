using NUnit.Framework.Legacy;
using VivesBankApi.Rest.Product.CreditCard.Generators;

namespace Tests.Rest.Product.CreditCard.Generator;

public class NumberGeneratorTests
{
    private NumberGenerator _numberGenerator;

    [SetUp]
    public void SetUp()
    {
        _numberGenerator = new NumberGenerator();
    }
    
    [Test]
    public void GenerateCreditCardNumber_ShouldReturnValidCardNumber()
    {
        string cardNumber = _numberGenerator.GenerateCreditCardNumber();
        
        ClassicAssert.AreEqual(16, cardNumber.Length);
        
        ClassicAssert.IsTrue(validNumber(cardNumber));
    }
    
    [Test]
    public void CalculateLuhnCheckDigit_ShouldReturnCorrectCheckDigit()
    {
        int[] cardNumberWithoutCheckDigit = new int[15]
        {
            4, 5, 3, 9, 0, 1, 7, 3, 8, 9, 2, 5, 6, 4, 2
        };
        
        int checkDigit = _numberGenerator.checkNumber(cardNumberWithoutCheckDigit);
        
        ClassicAssert.AreEqual(7, checkDigit);
    }


    private bool validNumber(string cardNumber)
    {
        int sum = 0;
        bool isSecond = false;
        
        for (int i = cardNumber.Length - 1; i >= 0; i--)
        {
            int digit = cardNumber[i] - '0';

            if (isSecond)
            {
                digit *= 2;
                if (digit > 9)
                {
                    digit -= 9;
                }
            }

            sum += digit;
            isSecond = !isSecond;
        }

        return sum % 10 == 0;
    }
}