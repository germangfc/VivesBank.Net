using VivesBankApi.Rest.Clients.Dto;
using VivesBankApi.Rest.Clients.Models;
using VivesBankApi.Rest.Clients.Mappers;
using NUnit.Framework.Legacy;

[TestFixture]
public class ClientMappersTests
{
    private Client _client;
    private ClientRequest _clientRequest;
    private ClientPatchRequest _clientUpdateRequest;

    [SetUp]
    public void SetUp()
    {
        _client = new Client
        {
            Id = "1",
            FullName = "Manuel García ",
            UserId = "53692294J",
            Adress = "C. de Fuencarral, 144",
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
            IsDeleted = false
        };

        _clientRequest = new ClientRequest
        {
            FullName = "Manuel García ",
            Address = "C. de Fuencarral, 144"
        };

        _clientUpdateRequest = new ClientPatchRequest
        {
            FullName = "Gonzalo Fernández",
            Address = "C. de Orense, 9-5"
        };
    }

    [Test]
    public void ToResponse()
    {
        // Act
        var response = _client.ToResponse();

        // Assert
        Assert.Multiple(() =>
        {
            ClassicAssert.AreEqual(_client.Id, response.Id);
            ClassicAssert.AreEqual(_client.FullName, response.Fullname);
            ClassicAssert.AreEqual(_client.UserId, response.UserId);
            ClassicAssert.AreEqual(_client.Adress, response.Address);
            ClassicAssert.AreEqual(_client.CreatedAt, response.CreatedAt);
            ClassicAssert.AreEqual(_client.UpdatedAt, response.UpdatedAt);
            ClassicAssert.AreEqual(_client.IsDeleted, response.IsDeleted);
        });
    }

    [Test]
    public void FromDtoRequest()
    {
        // Act
        var client = _clientRequest.FromDtoRequest();

        // Assert
        Assert.Multiple(() =>
        {
            ClassicAssert.AreEqual(_clientRequest.FullName, client.FullName);
            ClassicAssert.AreEqual(_clientRequest.Address, client.Adress);
            ClassicAssert.IsFalse(client.IsDeleted);
        });
    }

    [Test]
    public void FromDtoUpdateRequest()
    {
        // Act
        var client = _clientUpdateRequest.FromDtoUpdateRequest(_client);

        // Assert
        Assert.Multiple(() =>
        {
            ClassicAssert.AreEqual(_clientUpdateRequest.FullName, client.FullName);
            ClassicAssert.AreEqual(_clientUpdateRequest.Address, client.Adress);
        });
    }
    
    
    [Test]
    public void FromDtoResponse_ConvertsClientResponseToClient_Correctly()
    {
        // Arrange
        var response = new ClientResponse
        {
            Id = "123",
            Fullname = "John Doe",
            UserId = "User-456",
            Address = "123 Main St",
            Photo = "photo.jpg",
            DniPhoto = "dni.jpg",
            Accounts = new List<string> { "Account1", "Account2" },
            CreatedAt = new DateTime(2024, 1, 1, 12, 0, 0),
            UpdatedAt = new DateTime(2024, 1, 2, 14, 30, 0),
            IsDeleted = false
        };

        // Act
        var result = response.FromDtoResponse();

        // Assert
        Assert.Multiple(() =>
        {
            ClassicAssert.NotNull(result);
            ClassicAssert.AreEqual(response.Id, result.Id);
            ClassicAssert.AreEqual(response.Fullname, result.FullName);
            ClassicAssert.AreEqual(response.UserId, result.UserId);
            ClassicAssert.AreEqual(response.Address, result.Adress);
            ClassicAssert.AreEqual(response.Photo, result.Photo);
            ClassicAssert.AreEqual(response.DniPhoto, result.PhotoDni);
            ClassicAssert.AreEqual(response.Accounts, result.AccountsIds);
            ClassicAssert.AreEqual(response.CreatedAt, result.CreatedAt);
            ClassicAssert.AreEqual(response.UpdatedAt, result.UpdatedAt);
            ClassicAssert.AreEqual(response.IsDeleted, result.IsDeleted);
        }
            );
    }
}