using Moq;
using NUnit.Framework.Legacy;
using VivesBankApi.Rest.Product.BankAccounts.Models;
using VivesBankApi.Rest.Product.BankAccounts.Repositories;
using VivesBankApi.Rest.Products.BankAccounts.Exceptions;

namespace Tests.Utils.IbanGenerator;

public class IbanGeneratorTests
{
    private readonly Mock<IAccountsRepository> _mockRepository;
        private readonly VivesBankApi.Utils.IbanGenerator.IbanGenerator _ibanGenerator;

        public IbanGeneratorTests()
        {
            _mockRepository = new Mock<IAccountsRepository>();
            _ibanGenerator = new VivesBankApi.Utils.IbanGenerator.IbanGenerator(_mockRepository.Object);
        }

        [Test]
        public async Task GenerateUniqueIbanAsync_ShouldGenerateUniqueIban()
        {
            string ibanToCheck = "ES9712800001000000000000";
            _mockRepository.Setup(repo => repo.getAccountByIbanAsync(ibanToCheck))
                           .ReturnsAsync((Account)null); 
            
            string generatedIban = await _ibanGenerator.GenerateUniqueIbanAsync();
            
            ClassicAssert.NotNull(generatedIban);
            ClassicAssert.AreEqual(24, generatedIban.Length);
        }

        [Test]
        public async Task GenerateUniqueIbanAsync_ShouldThrowExceptionWhenIbanExists()
        {
            _mockRepository.Setup(repo => repo.getAccountByIbanAsync(It.IsAny<string>()))
                .ReturnsAsync(new Account());
            
            var exception =  Assert.ThrowsAsync<AccountsExceptions.AccountIbanNotGeneratedException>(
                () => _ibanGenerator.GenerateUniqueIbanAsync());
            
            ClassicAssert.AreEqual("Iban Couldnt be created after 1000 tries", exception.Message);
        }

        [Test]
        public void CalculateControlDigits_ShouldCalculateCorrectly()
        {
            string ibanBase = "01280001000000000000"; 
            
            int controlDigits = _ibanGenerator.CalculateControlDigits(ibanBase);
            
            ClassicAssert.AreEqual(66, controlDigits); 
        }

        [Test]
        public void GenerateRandomDigits_ShouldGenerateCorrectLength()
        {
            int length = 10;
            
            string randomDigits = _ibanGenerator.GenerateRandomDigits(length);
            
            ClassicAssert.AreEqual(length, randomDigits.Length);
        } 
}
