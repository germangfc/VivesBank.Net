using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using VivesBankApi.Rest.Movimientos.Models;
using VivesBankApi.Rest.Movimientos.Resolver;
using VivesBankApi.Rest.Movimientos.Services.Movimientos;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using VivesBankApi.Rest.Movimientos.Services.Domiciliaciones;
using VivesBankApi.Rest.Users.Models;

namespace Tests.Rest.Movimientos.Resolver;

[TestFixture]
[TestOf(typeof(MovimientosQuery))]
public class MovimientosQueryTest
{
    private Mock<IMovimientoService> _movimientoServiceMock;
    private Mock<IMovimientoMeQueriesService> _movimientoMeQueriesServiceMock;
    private Mock<IDomiciliacionService> _domiciliacionServiceMock;
    private Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private TestServer _testServer;
    private HttpClient _client;

    [SetUp]
    public void SetUp()
    {
        // Crear el mock del servicio
        _movimientoServiceMock = new Mock<IMovimientoService>();
        _movimientoMeQueriesServiceMock = new Mock<IMovimientoMeQueriesService>();
        _domiciliacionServiceMock = new Mock<IDomiciliacionService>();
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        
        // Configurar IHttpContextAccessor para simular un usuario autenticado
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "test-user-id"),
            new Claim(ClaimTypes.Role, "User")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);
        
        var httpContext = new DefaultHttpContext { User = claimsPrincipal };
        _httpContextAccessorMock.Setup(a => a.HttpContext).Returns(httpContext);
    
        // Configurar el TestServer con el servicio mockeado
        _testServer = new TestServer(new WebHostBuilder()
            .ConfigureServices(services =>
            {
                // Agregar el servicio de enrutamiento
                services.AddRouting();
    
                // Agregar el servicio mockeado
                services.AddSingleton(_movimientoServiceMock.Object);
                services.AddSingleton(_movimientoMeQueriesServiceMock.Object);
                services.AddSingleton(_domiciliacionServiceMock.Object);
                services.AddSingleton(_httpContextAccessorMock.Object);
                
                services.AddAuthorization(options =>
                {
                    options.AddPolicy("AdminPolicy", policy => policy.RequireRole("Admin"));
                });
        
                services.AddAuthorization(options =>
                {
                    options.AddPolicy("UserPolicy", policy => policy.RequireRole("User"));
                });
        
                services.AddAuthorization(options =>
                {
                    options.AddPolicy("ClientPolicy", policy => policy.RequireRole("Client"));
                });
                
                services.AddGraphQLServer()
                    .AddQueryType<MovimientosQuery>()
                    .AddFiltering()
                    .AddSorting()
                    .AddAuthorization();
            })
            .Configure(app =>
            {
                app.UseRouting();
                app.UseAuthorization();
                
                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapGraphQL(); // Configura el endpoint de GraphQL
                });
            }));
    
        // Crear un cliente HTTP para enviar consultas GraphQL
        _client = _testServer.CreateClient();
    }
    
    [TearDown]
    public void TearDown()
    {
        // Liberar los recursos
        _client.Dispose();
        _testServer.Dispose();
    }

    [Test]
    public async Task TestGetAllMovimientos_Mock()
    {
        // Arrange
        var movimientos = new List<Movimiento>
        {
            new Movimiento { Id = "1", Guid = "guid1", ClienteGuid = "cliente1" },
            new Movimiento { Id = "2", Guid = "guid2", ClienteGuid = "cliente2" }
        };

        // Configurar el mock para que devuelva los movimientos
        _movimientoServiceMock.Setup(service => service.FindAllMovimientosAsync())
            .ReturnsAsync(movimientos);

        // Definir la consulta GraphQL
        var query = @"
            query {
                movimientos {
                    id
                    guid
                    clienteGuid
                }
            }";

        // Act: Ejecutar la consulta usando el cliente HTTP
        var response = await _client.PostAsJsonAsync("/graphql", new { query });

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        Assert.That(content, Contains.Substring("\"id\":null"));
        Assert.That(content, Contains.Substring("\"guid\":\"guid1\""));
        Assert.That(content, Contains.Substring("\"guid\":\"guid2\""));
    }

    [Test]
    public async Task TestGetMovimientoById_Mock()
    {
        // Arrange
        var movimiento = new Movimiento { Id = "1", Guid = "guid1", ClienteGuid = "cliente1" };

        // Configurar el mock para que devuelva el movimiento
        _movimientoServiceMock.Setup(service => service.FindMovimientoByIdAsync("1"))
            .ReturnsAsync(movimiento);

        // Definir la consulta GraphQL
        var query = @"
            query{
                movimientoById(id: ""1"") {
                    id
                    guid
                   }
                   }";
        
        // Act: Ejecutar la consulta usando el cliente HTTP
        var response = await _client.PostAsJsonAsync("/graphql", new { query });

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        Assert.That(content, Contains.Substring("\"id\":null"));
        Assert.That(content, Contains.Substring("\"guid\":\"guid1\""));
    }

    [Test]
    public async Task TestGetMovimientoById_NotFound()
    {
        // Arrange
        _movimientoServiceMock.Setup(service => service.FindMovimientoByIdAsync("1"))
           .ReturnsAsync((Movimiento?)null);

        // Definir la consulta GraphQL
        var query = @"
            query{
                movimientoById(id: ""1"") {
                    id
                    guid
                }
            }";

        // Act: Ejecutar la consulta usando el cliente HTTP
        var response = await _client.PostAsJsonAsync("/graphql", new { query });
        // Assert
        var content = await response.Content.ReadAsStringAsync();
        Assert.That(content, Contains.Substring("No se encontro el movimiento con el ID/Guid 1"));
    }

    [Test]
    public async Task TestGetMovimientosByClienteGuid_Mock()
    {
        // Arrange
        var movimientos = new List<Movimiento>
        {
            new Movimiento { Id = "1", Guid = "guid1", ClienteGuid = "cliente1" },
            new Movimiento { Id = "2", Guid = "guid2", ClienteGuid = "cliente1" }
        };

        // Configurar el mock para que devuelva los movimientos
        _movimientoServiceMock.Setup(service => service.FindAllMovimientosByClientAsync("cliente1"))
            .ReturnsAsync(movimientos);

        // Definir la consulta GraphQL
        var query = @"
            query{
                movimientosByCliente(clienteGuid: ""cliente1"") {
                    id
                    guid
                }
            }";

        // Act: Ejecutar la consulta usando el cliente HTTP
        var response = await _client.PostAsJsonAsync("/graphql", new { query });

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        Assert.That(content, Contains.Substring("\"id\":null"));
        Assert.That(content, Contains.Substring("\"guid\":\"guid1\""));
        Assert.That(content, Contains.Substring("\"guid\":\"guid2\""));
    }

    [Test]
    public async Task TestGetMovimientoByGuid_Mock()
    {
        // Arrange
        var movimiento = new Movimiento { Id = "1", Guid = "guid1", ClienteGuid = "cliente1" };

        // Configurar el mock para que devuelva el movimiento
        _movimientoServiceMock.Setup(service => service.FindMovimientoByGuidAsync("guid1"))
            .ReturnsAsync(movimiento);

        // Definir la consulta GraphQL
        var query = @"
            query{
                movimientoByGuid(guid: ""guid1"") {
                    id
                    guid
                    clienteGuid
                }
            }";

        // Act: Ejecutar la consulta usando el cliente HTTP
        var response = await _client.PostAsJsonAsync("/graphql", new { query });

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        Assert.That(content, Contains.Substring("\"id\":null"));
        Assert.That(content, Contains.Substring("\"clienteGuid\":\"cliente1\""));
    }

    [Test]
    public async Task TestGetMovimientosByGuid_NotFound_Mock()
    {
       // Arrange
       var user = new User { Id = "userGuid", Role = Role.Admin};

       // // Generar el token JWT
       // var token = GenerateJwtToken(user.Id, user.Role.ToString());
       //
       // // Agregar el token a la cabecera de autorización
       // _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

       
        // Configurar el mock para que devuelva el movimiento
        _movimientoServiceMock.Setup(service => service.FindMovimientoByGuidAsync("guid1"))
            .ReturnsAsync((Movimiento?)null);
        
        List<Claim> claims = new List<Claim>();

        if (Enum.TryParse(typeof(Role), user.Role.ToString(), out var roleEnum) &&
            Enum.IsDefined(typeof(Role), roleEnum))
        {
            var roleName = Enum.GetName(typeof(Role), roleEnum) ?? string.Empty;
             claims = new List<Claim>
            {
                new("UserId", user.Id),
                new(ClaimTypes.Role, roleName),
                new(ClaimTypes.NameIdentifier, user.Id),
            };
        }

       
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);
        _httpContextAccessorMock.Setup(a => a.HttpContext).Returns(new DefaultHttpContext { User = claimsPrincipal });

        // Definir la consulta GraphQL
        var query = @"
            query{
                movimientoByGuid(guid: ""guid1"") {
                    id
                    guid
                    clienteGuid
                }
            }";

        // Act: Ejecutar la consulta usando el cliente HTTP
        var response = await _client.PostAsJsonAsync("/graphql", new { query });
        // Assert
        var content = await response.Content.ReadAsStringAsync();
        Assert.That(content, Contains.Substring("No se encontro el movimiento con el ID/Guid guid1"));
    }
    
    
}