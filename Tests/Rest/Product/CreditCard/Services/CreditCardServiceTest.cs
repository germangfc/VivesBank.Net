using Moq;
using Microsoft.Extensions.Logging;
using NUnit.Framework.Legacy;
using VivesBankApi.Rest.Product.BankAccounts.Repositories;
using VivesBankApi.Rest.Product.CreditCard.Exceptions;
using VivesBankApi.Rest.Product.CreditCard.Generators;
using VivesBankApi.Rest.Product.CreditCard.Service;

namespace VivesBankApi.Tests.CreditCard
{
    [TestFixture]
    public class CreditCardServiceTests
    {
        private Mock<ICreditCardRepository> _creditCardRepositoryMock;
        private Mock<ILogger<CreditCardService>> _loggerMock;
        private Mock<CvcGenerator> _cvcGeneratorMock;
        private Mock<ExpirationDateGenerator> _expirationDateGeneratorMock;
        private Mock<NumberGenerator> _numberGeneratorMock;
        private Mock<IAccountsRepository> _accountsRepositoryMock;

        private CreditCardService _service;

        [SetUp]
        public void SetUp()
        {
            _creditCardRepositoryMock = new Mock<ICreditCardRepository>();
            _loggerMock = new Mock<ILogger<CreditCardService>>();
            _cvcGeneratorMock = new Mock<CvcGenerator>();
            _expirationDateGeneratorMock = new Mock<ExpirationDateGenerator>();
            _numberGeneratorMock = new Mock<NumberGenerator>();
            _accountsRepositoryMock = new Mock<IAccountsRepository>();

            _service = new CreditCardService(
                _creditCardRepositoryMock.Object,
                _loggerMock.Object,
                _cvcGeneratorMock.Object,
                _expirationDateGeneratorMock.Object,
                _numberGeneratorMock.Object,
                _accountsRepositoryMock.Object
            );
        }

        [Test]
        public async Task GetAllCreditCardAsyncReturnsOk()
        {
            var cardList = new List<Rest.Product.CreditCard.Models.CreditCard>
            {
                new() { Id = "1", AccountId = "Acc1", CardNumber = "1234567890123456", Pin = "1111", Cvc = "123" },
                new() { Id = "2", AccountId = "Acc2", CardNumber = "6543210987654321", Pin = "2222", Cvc = "321" }
            };

            _creditCardRepositoryMock.Setup(repo => repo.GetAllAsync()).ReturnsAsync(cardList);

            var result = await _service.GetAllCreditCardAdminAsync();

            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result[0].Id, Is.EqualTo("1"));
            Assert.That(result[0].AccountId, Is.EqualTo(null));
            Assert.That(result[0].CardNumber, Is.EqualTo("1234567890123456"));
            _creditCardRepositoryMock.Verify(repo => repo.GetAllAsync(), Times.Once);
        }
        
        [Test]
        public async Task GetCreditCardByIdAdminAsync()
        {
            var creditCardId = "1";
            var creditCard = new Rest.Product.CreditCard.Models.CreditCard
            {
                Id = creditCardId,
                AccountId = null,
                CardNumber = "1234567890123456",
                ExpirationDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1)),
                CreatedAt = DateTime.UtcNow.AddDays(-30),
                UpdatedAt = DateTime.UtcNow.AddDays(-1)
            };

            _creditCardRepositoryMock
                .Setup(repo => repo.GetByIdAsync(creditCardId))
                .ReturnsAsync(creditCard);

            var result = await _service.GetCreditCardByIdAdminAsync(creditCardId);

            ClassicAssert.IsNotNull(result);
            Assert.That(result.Id, Is.EqualTo(creditCard.Id));
            Assert.That(result.AccountId, Is.EqualTo(creditCard.AccountId));
            Assert.That(result.CardNumber, Is.EqualTo(creditCard.CardNumber));
            _creditCardRepositoryMock.Verify(repo => repo.GetByIdAsync(creditCardId), Times.Once);
        }
        
        [Test]
        public void GetCreditCardByIdAdminAsyncNotFound()
        {
            var creditCardId = "1";

            _creditCardRepositoryMock
                .Setup(repo => repo.GetByIdAsync(creditCardId))
                .ReturnsAsync((Rest.Product.CreditCard.Models.CreditCard)null);

            var exception = Assert.ThrowsAsync<CreditCardException.CreditCardNotFoundException>(
                async () => await _service.GetCreditCardByIdAdminAsync(creditCardId));

            Assert.That(exception.Message, Is.EqualTo($"The credit card with the ID {creditCardId} was not found"));
            _creditCardRepositoryMock.Verify(repo => repo.GetByIdAsync(creditCardId), Times.Once);
        }
    }
}