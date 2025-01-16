using TestContainers.Container.Database.PostgreSql;
using VivesBankApi.Rest.Product.Base.Repository;

namespace Tests.Rest.Product.Repository;


[TestFixture]
[TestOf(typeof(ProductRepository))]
public class ProductRepositoryTest
{
    private PostgreSqlContainer _container;
    private ProductRepository _productRepository;

    [OneTimeSetUp]
    public async Task Setup()
    {
        
    }
}