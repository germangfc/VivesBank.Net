using VivesBankApi.Rest.Clients.Dto;
using VivesBankApi.Rest.Clients.Models;

namespace VivesBankApi.Rest.Clients.Mappers;

public static class ClientMappers
{
    public static ClientResponse ToResponse(this Client client)
    {
        return new ClientResponse
        {
            Id = client.Id,
            Fullname = client.FullName,
            UserId = client.UserId,
            Address = client.Adress,
            Photo = client.Photo,
            DniPhoto = client.PhotoDni,
            Accounts = client.AccountsIds,
            CreatedAt = client.CreatedAt,
            UpdatedAt = client.UpdatedAt,
            IsDeleted = client.IsDeleted
        };
    }

    public static Client FromDtoRequest(this ClientRequest createRequest)
    {
        return new Client
        {
            FullName = createRequest.FullName,
            Adress = createRequest.Address
        };
    }

    public static Client FromDtoUpdateRequest(this ClientPatchRequest updateRequest, Client clientToUpdate)
    {
        var updatedClient = clientToUpdate;
        if (updateRequest.Address != null)
        {
            updatedClient.Adress = updateRequest.Address;
        }
        if (updateRequest.FullName != null)
        {
            updatedClient.FullName = updateRequest.FullName;
        }
        if (updateRequest.Photo != null)
        {
            updatedClient.Photo = updateRequest.Photo;
        }
        if (updateRequest.PhotoDni != null)
        {
            updatedClient.PhotoDni = updateRequest.PhotoDni;
        }
        return updatedClient;
    }

    public static Client FromDtoResponse(this ClientResponse response)
    {
        return new Client
        {
            Id = response.Id,
            FullName = response.Fullname,
            UserId = response.UserId,
            Adress = response.Address,
            Photo = response.Photo,
            PhotoDni = response.DniPhoto,
            AccountsIds = response.Accounts,
            CreatedAt = response.CreatedAt,
            UpdatedAt = response.UpdatedAt,
            IsDeleted = response.IsDeleted
        };
    }
}