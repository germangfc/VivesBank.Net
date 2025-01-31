using VivesBankApi.Rest.Clients.Dto;
using VivesBankApi.Rest.Clients.Models;
using VivesBankApi.Utils.GenericStorage.JSON;

namespace VivesBankApi.Rest.Clients.storage.JSON;

public interface IClientStorageJson : IGenericStorageJson<Client>
{
    Task<FileStream> ExportOnlyMeData(Client user);
}