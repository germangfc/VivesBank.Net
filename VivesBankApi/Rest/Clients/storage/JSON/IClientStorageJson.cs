using VivesBankApi.Rest.Clients.Dto;
using VivesBankApi.Utils.GenericStorage.JSON;

namespace VivesBankApi.Rest.Clients.storage.JSON;

public interface IClientStorageJson : IGenericStorageJson<ClientResponse>
{
    Task<FileStream> ExportOnlyMeData(ClientResponse user);
}