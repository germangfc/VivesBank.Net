using Moq;
using Microsoft.Extensions.Logging;
using NUnit.Framework.Legacy;
using StackExchange.Redis;
using VivesBankApi.Rest.Product.BankAccounts.Models;
using VivesBankApi.Rest.Product.BankAccounts.Repositories;
using VivesBankApi.Rest.Product.CreditCard.Dto;
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
        private Mock<IConnectionMultiplexer> _cacheMock;
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
            _cacheMock = new Mock<IConnectionMultiplexer>();

            var dbMock = new Mock<IDatabase>();
            dbMock.Setup(db => db.KeyDeleteAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
                .ReturnsAsync(true); 
            dbMock.Setup(db => db.StringSetAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<TimeSpan?>(), It.IsAny<When>(), It.IsAny<CommandFlags>()))
                .ReturnsAsync(true);

            _cacheMock.Setup(cache => cache.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
                .Returns(dbMock.Object);

            _service = new CreditCardService(
                _creditCardRepositoryMock.Object,
                _loggerMock.Object,
                _cvcGeneratorMock.Object,
                _expirationDateGeneratorMock.Object,
                _numberGeneratorMock.Object,
                _accountsRepositoryMock.Object,
                _cacheMock.Object
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
            Assert.That(result[1].AccountId, Is.EqualTo(null));
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

            Assert.That(exception.Message,
                Is.EqualTo("The credit card with the ID 1 was not found"));
            _creditCardRepositoryMock.Verify(repo => repo.GetByIdAsync(creditCardId), Times.Once);
        }
        
        [Test]
        public async Task CreateCreditCardAsyncCreditCard()
        {
            var createRequest = new CreditCardRequest
            {
                AccountIban = "IBAN123456",
            };

            var account = new Account
            {
                Id = "acc123",
                IBAN = "IBAN123456"
            };

            var generatedCardNumber = "1234567812345678";
            var generatedExpirationDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(3));
            var generatedCvc = "123";

            _accountsRepositoryMock
                .Setup(repo => repo.getAccountByIbanAsync(createRequest.AccountIban))
                .ReturnsAsync(account);

            _numberGeneratorMock
                .Setup(generator => generator.GenerateCreditCardNumber())
                .Returns(generatedCardNumber);

            _expirationDateGeneratorMock
                .Setup(generator => generator.GenerateRandomDate())
                .Returns(generatedExpirationDate);

            _cvcGeneratorMock
                .Setup(generator => generator.Generate())
                .Returns(generatedCvc);

            var response = await _service.CreateCreditCardAsync(createRequest);
            
            ClassicAssert.NotNull(response);
            ClassicAssert.AreEqual(generatedCardNumber, response.CardNumber);
            ClassicAssert.AreEqual(generatedExpirationDate.ToString("dd/MM/yyyy"), response.ExpirationDate);
            ClassicAssert.AreEqual(generatedCvc, response.Cvc);

            _accountsRepositoryMock.Verify(repo => repo.getAccountByIbanAsync(createRequest.AccountIban), Times.Once);
            _creditCardRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Rest.Product.CreditCard.Models.CreditCard>()), Times.Once);
        }

        [Test]
        public async Task UpdateCreditCardAsyncReturnsOk()
        {
            var cardId = "Card123";
            var updateRequest = new CreditCardUpdateRequest
            {
                Pin = "5678"
            };

            var existingCreditCard = new Rest.Product.CreditCard.Models.CreditCard
            {
                Id = cardId,
                AccountId = "Account0001",
                Pin = "1234",
                UpdatedAt = DateTime.UtcNow.AddDays(-1)
            };

            _creditCardRepositoryMock
                .Setup(repo => repo.GetByIdAsync(cardId))
                .ReturnsAsync(existingCreditCard);

            _creditCardRepositoryMock
                .Setup(repo => repo.UpdateAsync(It.IsAny<Rest.Product.CreditCard.Models.CreditCard>()))
                .Returns(Task.CompletedTask);

            var dbMock = new Mock<IDatabase>();
            dbMock.Setup(db => db.KeyDeleteAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
                .ReturnsAsync(true); 

            dbMock.Setup(db => db.StringSetAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<TimeSpan?>(), It.IsAny<When>(), It.IsAny<CommandFlags>()))
                .ReturnsAsync(true);

            _cacheMock.Setup(cache => cache.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
                .Returns(dbMock.Object);

            var result = await _service.UpdateCreditCardAsync(cardId, updateRequest);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Pin, Is.EqualTo(updateRequest.Pin));

            dbMock.Verify(db => db.KeyDeleteAsync(It.Is<RedisKey>(key => key == cardId), It.IsAny<CommandFlags>()), Times.Once);
            dbMock.Verify(db => db.StringSetAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<TimeSpan?>(), It.IsAny<When>(), It.IsAny<CommandFlags>()), Times.Once);
        }
        
        
        [Test]
        public void UpdateCreditCardAsyncNotFound()
        {
            var cardId = "InvalidCard123";
            var updateRequest = new CreditCardUpdateRequest
            {
                Pin = "5678"
            };

            _creditCardRepositoryMock
                .Setup(repo => repo.GetByIdAsync(cardId))
                .ReturnsAsync((Rest.Product.CreditCard.Models.CreditCard)null);

            Assert.ThrowsAsync<CreditCardException.CreditCardNotFoundException>(async () =>
            {
                await _service.UpdateCreditCardAsync(cardId, updateRequest);
            });

            _creditCardRepositoryMock.Verify(repo => repo.GetByIdAsync(cardId), Times.Once);
            _creditCardRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<Rest.Product.CreditCard.Models.CreditCard>()), Times.Never);
        }
        
        [Test]
        public async Task DeleteCreditCardAsyncReturnsOk()
        {
            var cardId = "ValidCard123";

            _creditCardRepositoryMock
                .Setup(repo => repo.DeleteAsync(cardId))
                .Returns(Task.CompletedTask);

            await _service.DeleteCreditCardAsync(cardId);

            _creditCardRepositoryMock.Verify(repo => repo.DeleteAsync(cardId), Times.Once);
        }
        
        [Test]
        public void DeleteCreditCardAsyncNotFound()
        {
            var cardId = "InvalidCard123";

            _creditCardRepositoryMock
                .Setup(repo => repo.DeleteAsync(cardId))
                .Throws(new CreditCardException.CreditCardNotFoundException(cardId));

            Assert.ThrowsAsync<CreditCardException.CreditCardNotFoundException>(async () =>
            {
                await _service.DeleteCreditCardAsync(cardId);
            });

            _creditCardRepositoryMock.Verify(repo => repo.DeleteAsync(cardId), Times.Once);
        }
        
        
    }
}