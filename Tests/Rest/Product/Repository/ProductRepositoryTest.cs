using TestContainers.Container.Database.PostgreSql;
using VivesBankApi.Database;
using VivesBankApi.Rest.Product.Base.Repository;

namespace Tests.Rest.Product.Repository;


[TestFixture]
[TestOf(typeof(ProductRepository))]
public class ProductRepositoryTest
{
    private readonly PostgreSqlContainer _postgreSqlContainer;
    private BancoDbContext _dbContext;
    private ProductRepository _repository;

    [OneTimeSetUp]
    public async Task Setup()
    {
        
    }
}