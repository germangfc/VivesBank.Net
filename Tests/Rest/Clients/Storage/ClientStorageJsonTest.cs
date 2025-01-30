using System.Reactive.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework.Legacy;
using VivesBankApi.Rest.Clients.Models;
using VivesBankApi.Rest.Clients.storage.JSON;
using Path = System.IO.Path;

namespace Tests.Rest.Clients.Storage;

public class ClientStorageJsonTest
{
    private IClientStorageJson _storage;

    [SetUp]
    public void Setup()
    {
        _storage = new ClientStorageJson(
            NullLogger<ClientStorageJson>.Instance
        );
    }

    [Test]
    public async Task Export()
    {
        //Arrange
        var client = new Client { Id = "1", FullName = "John Doe", Adress = "Address 1", IsDeleted = false };

        //Act
        var result = await _storage.ExportOnlyMeData(client);

        //Assert
        var formFile = new FormFile(result, 0, result.Length, "something", "test.json"  );
        var secondResult = await _storage.Import(formFile).ToList();
        ClassicAssert.IsInstanceOf<FileStream>(result);
        ClassicAssert.AreEqual(client.Id, secondResult[0].Id);
        ClassicAssert.AreEqual(client.FullName, secondResult[0].FullName);
        ClassicAssert.AreEqual(client.UserId, secondResult[0].UserId);
        ClassicAssert.AreEqual(client.Adress, secondResult[0].Adress);
        
        //Clean up
        await result.DisposeAsync();
    }
}