﻿namespace VivesBankApi.Rest.Product.CreditCard.Generators;

public class CvvGenerator
{
    public string Generate()
    {
        var random = new Random();
        int randomNumber = random.Next(0, 1000);
        return randomNumber.ToString("D3");
    }
}