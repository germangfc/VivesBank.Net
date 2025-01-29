using VivesBankApi.Rest.Users.Models;

namespace VivesBankApi.Rest.Users.Storage;

public interface IUserStorageJson
{
   IObservable<User> Import(IFormFile fileStream);
   Task<FileStream> Export(List<User> users);
}