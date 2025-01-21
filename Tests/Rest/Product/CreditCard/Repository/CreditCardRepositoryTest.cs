using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework.Legacy;
using VivesBankApi.Database;

namespace Tests.Rest.Product.CreditCard.Repository;

[TestFixture]
public class CreditCardRepositoryTest
{
    private Mock<BancoDbContext> _mockDbContext;
    private Mock<DbSet<VivesBankApi.Rest.Product.CreditCard.Models.CreditCard>> _mockDbSet;
    private CreditCardRepository _creditCardRepository;

    [SetUp]
    public void SetUp()
    {
        // Mock para DbSet y DbContext
        _mockDbSet = new Mock<DbSet<VivesBankApi.Rest.Product.CreditCard.Models.CreditCard>>();
        _mockDbContext = new Mock<BancoDbContext>();
        _mockDbContext.Setup(c => c.Set<VivesBankApi.Rest.Product.CreditCard.Models.CreditCard>()).Returns(_mockDbSet.Object);

        // Instanciar el repositorio con el contexto simulado
        _creditCardRepository =
            new CreditCardRepository(_mockDbContext.Object, Mock.Of<ILogger<CreditCardRepository>>());
    }

    [Test]
    public async Task GetAllAsync_ShouldReturnAllCreditCards()
    {
        // Arrange
        var creditCards = new List<VivesBankApi.Rest.Product.CreditCard.Models.CreditCard>
        {
            new VivesBankApi.Rest.Product.CreditCard.Models.CreditCard { Id = "1", CardNumber = "1234" },
            new VivesBankApi.Rest.Product.CreditCard.Models.CreditCard { Id = "2", CardNumber = "5678" }
        }.AsQueryable();

        _mockDbSet.As<IQueryable<VivesBankApi.Rest.Product.CreditCard.Models.CreditCard>>().Setup(m => m.Provider).Returns(creditCards.Provider);
        _mockDbSet.As<IQueryable<VivesBankApi.Rest.Product.CreditCard.Models.CreditCard>>().Setup(m => m.Expression).Returns(creditCards.Expression);
        _mockDbSet.As<IQueryable<VivesBankApi.Rest.Product.CreditCard.Models.CreditCard>>().Setup(m => m.ElementType).Returns(creditCards.ElementType);
        _mockDbSet.As<IQueryable<VivesBankApi.Rest.Product.CreditCard.Models.CreditCard>>().Setup(m => m.GetEnumerator()).Returns(creditCards.GetEnumerator());

        // Act
        var result = await _creditCardRepository.GetAllAsync();

        // Assert
        ClassicAssert.IsNotNull(result);
        ClassicAssert.AreEqual(2, result.Count);
        ClassicAssert.AreEqual("1234", result[0].CardNumber);
    }
}