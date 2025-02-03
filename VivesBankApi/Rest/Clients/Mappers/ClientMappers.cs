using VivesBankApi.Rest.Clients.Dto;
using VivesBankApi.Rest.Clients.Models;

namespace VivesBankApi.Rest.Clients.Mappers
{
    /// <summary>
    /// Clase de mapeo para convertir entre diferentes tipos de objetos relacionados con clientes.
    /// </summary>
    public static class ClientMappers
    {
        /// <summary>
        /// Convierte un objeto de tipo <see cref="Client"/> a <see cref="ClientResponse"/>.
        /// </summary>
        /// <param name="client">El cliente a convertir.</param>
        /// <returns>Un objeto <see cref="ClientResponse"/> con los datos del cliente.</returns>
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

        /// <summary>
        /// Convierte un objeto de tipo <see cref="ClientRequest"/> a un objeto <see cref="Client"/>.
        /// </summary>
        /// <param name="createRequest">El objeto <see cref="ClientRequest"/> con los datos del nuevo cliente.</param>
        /// <returns>Un objeto <see cref="Client"/> con los datos del cliente.</returns>
        public static Client FromDtoRequest(this ClientRequest createRequest)
        {
            return new Client
            {
                FullName = createRequest.FullName,
                Adress = createRequest.Address
            };
        }

        /// <summary>
        /// Convierte un objeto de tipo <see cref="ClientPatchRequest"/> a un objeto <see cref="Client"/>.
        /// </summary>
        /// <param name="updateRequest">El objeto <see cref="ClientPatchRequest"/> con los datos de la actualización.</param>
        /// <param name="clientToUpdate">El objeto <see cref="Client"/> que se va a actualizar.</param>
        /// <returns>El objeto <see cref="Client"/> actualizado.</returns>
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

        /// <summary>
        /// Convierte un objeto de tipo <see cref="ClientResponse"/> a un objeto <see cref="Client"/>.
        /// </summary>
        /// <param name="response">El objeto <see cref="ClientResponse"/> con los datos del cliente.</param>
        /// <returns>Un objeto <see cref="Client"/> con los datos del cliente.</returns>
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
}
