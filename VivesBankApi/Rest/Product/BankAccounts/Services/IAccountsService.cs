using VivesBankApi.Rest.Product.BankAccounts.Dto;
using VivesBankApi.Rest.Product.BankAccounts.Models;
using VivesBankApi.Utils.GenericStorage.JSON;

namespace VivesBankApi.Rest.Product.BankAccounts.Services
{
    /// <summary>
    /// Interfaz para el servicio de cuentas bancarias.
    /// Define los métodos necesarios para interactuar con las cuentas bancarias, incluyendo operaciones de creación, actualización, eliminación y consulta.
    /// Hereda de la interfaz genérica <see cref="IGenericStorageJson{TEntity}"/>.
    /// </summary>
    /// <remarks>
    /// Autor: Raúl Fernández, Javier Hernández, Samuel Cortés, Germán, Álvaro Herrero, Tomás
    /// Versión: 1.0
    /// </remarks>
    public interface IAccountsService : IGenericStorageJson<Account>
    {
        /// <summary>
        /// Obtiene todas las cuentas bancarias.
        /// </summary>
        /// <returns>Una lista de todas las cuentas bancarias.</returns>
        Task<List<Account>> GetAll();

        /// <summary>
        /// Obtiene una lista de cuentas bancarias con paginación y ordenación.
        /// </summary>
        /// <param name="pageNumber">El número de página para la paginación (por defecto es 0).</param>
        /// <param name="pageSize">El tamaño de la página para la paginación (por defecto es 10).</param>
        /// <param name="sortBy">El campo por el cual se debe ordenar (por defecto es "id").</param>
        /// <param name="direction">La dirección de ordenación, ya sea ascendente o descendente (por defecto es "asc").</param>
        /// <returns>Un objeto <see cref="PageResponse{T}"/> con las cuentas paginadas y ordenadas.</returns>
        Task<PageResponse<AccountResponse>> GetAccountsAsync(int pageNumber = 0, int pageSize = 10, string sortBy = "id", string direction = "asc");

        /// <summary>
        /// Obtiene una cuenta bancaria a partir de su ID.
        /// </summary>
        /// <param name="id">El ID de la cuenta bancaria que se desea obtener.</param>
        /// <returns>Una respuesta con los detalles de la cuenta, o null si no se encuentra.</returns>
        Task<AccountResponse> GetAccountByIdAsync(string id);

        /// <summary>
        /// Obtiene todas las cuentas bancarias asociadas a un cliente por su ID de cliente.
        /// </summary>
        /// <param name="clientId">El ID del cliente cuyas cuentas se desean obtener.</param>
        /// <returns>Una lista de respuestas con las cuentas asociadas al cliente.</returns>
        Task<List<AccountResponse>> GetAccountByClientIdAsync(string clientId);

        /// <summary>
        /// Obtiene todas las cuentas completas asociadas a un cliente por su ID de cliente.
        /// </summary>
        /// <param name="clientId">El ID del cliente cuyas cuentas completas se desean obtener.</param>
        /// <returns>Una lista de respuestas completas con los detalles de las cuentas asociadas al cliente.</returns>
        Task<List<AccountCompleteResponse>> GetCompleteAccountByClientIdAsync(string clientId);

        /// <summary>
        /// Obtiene una cuenta bancaria a partir de su IBAN.
        /// </summary>
        /// <param name="iban">El IBAN de la cuenta que se desea obtener.</param>
        /// <returns>Una respuesta con los detalles de la cuenta, o null si no se encuentra.</returns>
        Task<AccountResponse> GetAccountByIbanAsync(string iban);

        /// <summary>
        /// Obtiene todas las cuentas bancarias asociadas al cliente autenticado.
        /// </summary>
        /// <returns>Una lista de respuestas con las cuentas del cliente autenticado.</returns>
        Task<List<AccountResponse>> GetMyAccountsAsClientAsync();

        /// <summary>
        /// Obtiene una cuenta completa a partir de su IBAN.
        /// </summary>
        /// <param name="iban">El IBAN de la cuenta que se desea obtener.</param>
        /// <returns>Una respuesta completa con los detalles de la cuenta asociada al IBAN.</returns>
        Task<AccountCompleteResponse> GetCompleteAccountByIbanAsync(string iban);

        /// <summary>
        /// Crea una nueva cuenta bancaria.
        /// </summary>
        /// <param name="request">Los datos necesarios para crear la nueva cuenta.</param>
        /// <returns>Una respuesta con los detalles de la cuenta recién creada.</returns>
        Task<AccountResponse> CreateAccountAsync(CreateAccountRequest request);

        /// <summary>
        /// Actualiza una cuenta bancaria existente.
        /// </summary>
        /// <param name="id">El ID de la cuenta que se desea actualizar.</param>
        /// <param name="request">Los nuevos datos para actualizar la cuenta.</param>
        /// <returns>Una respuesta completa con los detalles de la cuenta actualizada.</returns>
        Task<AccountCompleteResponse> UpdateAccountAsync(string id, UpdateAccountRequest request);

        /// <summary>
        /// Elimina una cuenta bancaria a partir de su ID.
        /// </summary>
        /// <param name="id">El ID de la cuenta que se desea eliminar.</param>
        /// <returns>Una tarea que representa la operación de eliminación.</returns>
        Task DeleteAccountAsync(string id);

        /// <summary>
        /// Elimina la cuenta bancaria asociada al IBAN proporcionado.
        /// </summary>
        /// <param name="iban">El IBAN de la cuenta que se desea eliminar.</param>
        /// <returns>Una tarea que representa la operación de eliminación.</returns>
        Task DeleteMyAccountAsync(string iban);
    }
}
