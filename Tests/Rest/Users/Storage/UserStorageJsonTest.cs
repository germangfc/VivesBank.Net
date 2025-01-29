using System.Reactive.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework.Legacy;
using VivesBankApi.Rest.Users.Models;
using VivesBankApi.Rest.Users.Storage;
using Path = System.IO.Path;

namespace Tests.Rest.Users.Storage;

public class UserStorageJsonTest
{
    private UserStorageJson storage;

    [SetUp]
    public void Setup()
    {
        storage = new UserStorageJson(
            NullLogger<UserStorageJson>.Instance);
    }

    [Test]
    public async Task Import()
    {
        //Arrange
        var content = "[ {\n    \"id\": \"abcd1234-efgh5678-ijkl9012-mnop3456\",\n    \"dni\": \"1234567890\",\n    \"password\": \"securePassword123!\",\n    \"role\": \"User\",\n    \"createdAt\": \"2025-01-27T10:00:00Z\",\n    \"updatedAt\": \"2025-01-27T10:00:00Z\",\n    \"isDeleted\": false\n  },]";
        var fileName = "test.json";
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream);
        writer.Write(content);
        writer.Flush();
        stream.Position = 0;
        IFormFile file = new FormFile(stream, 0, stream.Length, "id_from_form", fileName);
        
        //Act
        var result = await storage.Import(file).ToList();

        //Assert
        ClassicAssert.IsInstanceOf<List<User>>(result);
        ClassicAssert.AreEqual(1, result.Count); 
        ClassicAssert.AreEqual("abcd1234-efgh5678-ijkl9012-mnop3456", result[0].Id);
        ClassicAssert.AreEqual("1234567890", result[0].Dni);
        ClassicAssert.AreEqual("securePassword123!", result[0].Password);
        ClassicAssert.AreEqual(Role.User, result[0].Role);
    }

    [Test]
    public async Task Import_ReturnsError_WhenDataIsInvalid()
    {
        //Arrange
        var content = "[ {\n    \"id\": \"abcd1234-efgh5678-ijkl9012-mnop3456\",\n    \"dni\": \"1234567890\",\n    \"notThePasswordField\": \"securePassword123!\",\n    \"role\": \"User\",\n    \"createdAt\": \"2025-01-27T10:00:00Z\",\n    \"updatedAt\": \"2025-01-27T10:00:00Z\",\n    \"isDeleted\": false\n  }, {\"invalid\": \"data\" }]";
        var fileName = "test.json";
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream);
        writer.Write(content);
        writer.Flush();
        stream.Position = 0;
        IFormFile file = new FormFile(stream, 0, stream.Length, "id_from_form", fileName);
        Exception caughtException = null;
        
        //Act
        storage.Import(file)
            .Subscribe(
                _ => { },
                ex => caughtException = ex,
                () => { }
            );
        
        //Assert
        ClassicAssert.NotNull(caughtException);
        ClassicAssert.IsInstanceOf<Exception>(caughtException);
    }

    [Test]
    public async Task Export()
    {
        //Arrange
        var users = new List<User>
        {
            new User
            {
                Id = "abcd1234-efgh5678-ijkl9012-mnop3456",
                Dni = "1234567890",
                Password = "securePassword123!",
                Role = Role.User,
                CreatedAt = new DateTime(2025, 01, 27, 10, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2025, 01, 27, 10, 0, 0, DateTimeKind.Utc),
                IsDeleted = false
            }
        };

        //Act
        var result = await storage.Export(users);

        //Assert
        var formFile = new FormFile(result, 0, result.Length, "something", "test.json"  );
        var secondResult = await storage.Import(formFile).ToList();
        ClassicAssert.IsInstanceOf<FileStream>(result);
        ClassicAssert.AreEqual(users[0].Id, secondResult[0].Id);
        ClassicAssert.AreEqual(users[0].Dni, secondResult[0].Dni);
        ClassicAssert.AreEqual(users[0].Password, secondResult[0].Password);
        ClassicAssert.AreEqual(users[0].Role, secondResult[0].Role);
    }

}