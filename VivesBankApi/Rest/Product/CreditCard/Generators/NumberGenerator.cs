namespace VivesBankApi.Rest.Product.CreditCard.Generators;

public class NumberGenerator : INumberGenerator
{
    public virtual string GenerateCreditCardNumber()
    {
        var random = new Random();
        int[] cardNumber = new int[16];

        for (int i = 0; i < 15; i++)
        {
            cardNumber[i] = random.Next(0, 10);
        }

        cardNumber[15] = checkNumber(cardNumber);

        return string.Join(string.Empty, cardNumber);
    }

    public int checkNumber(int[] cardNumber)
    {
        int sum = 0;
        for (int i = 0; i < 15; i++)
        {
            int digit = cardNumber[i];

            if (i % 2 == 0)
            {
                digit *= 2;
                if (digit > 9)
                {
                    digit -= 9;
                }
            }

            sum += digit;
        }

        return (10 - (sum % 10)) % 10;
    }

}