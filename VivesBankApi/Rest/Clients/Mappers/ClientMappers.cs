using VivesBankApi.Rest.Clients.Dto;
using VivesBankApi.Rest.Clients.Models;

namespace VivesBankApi.Rest.Clients.Mappers;

public static class ClientMappers
{
    public static ClientResponse toResponse(this Client client)
    {
        return new ClientResponse
        {
            Id = client.Id,
            Fullname = client.FullName,
            UserId = client.UserId,
            Address = client.Adress,
            Accounts = client.AccountsIds,
            CreatedAt = client.CreatedAt,
            UpdatedAt = client.UpdatedAt,
            IsDeleted = client.IsDeleted
           
        };
    }

    public static Client fromDtoRequest(this ClientRequest createRequest)
    {
        return new Client
        {
            FullName = createRequest.FullName,
            UserId = createRequest.UserId,
            Adress = createRequest.Address,
            IsDeleted = false
        };
    }

    public static Client fromDtoUpdateRequest(this ClientUpdateRequest updateRequest)
    {
        return new Client
        {
            FullName = updateRequest.FullName,
            Adress = updateRequest.Address,
        };
    }
}