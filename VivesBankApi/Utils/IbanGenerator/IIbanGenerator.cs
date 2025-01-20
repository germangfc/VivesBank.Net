namespace VivesBankApi.Utils.IbanGenerator;

public interface IIbanGenerator
{
    Task<string> GenerateUniqueIbanAsync();
}