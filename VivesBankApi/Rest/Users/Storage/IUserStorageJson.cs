using VivesBankApi.Rest.Users.Models;

namespace VivesBankApi.Rest.Users.Storage;

public interface IUserStorageJson
{
   IObservable<User> Import(IFormFile file);
   Task<FileStream> Export(string filename, IObservable<User> users);
}