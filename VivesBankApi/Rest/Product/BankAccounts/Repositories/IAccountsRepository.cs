using ApiFunkosCS.Utils.GenericRepository;
using VivesBankApi.Rest.Product.BankAccounts.Models;

namespace VivesBankApi.Rest.Product.BankAccounts.Repositories
{
    /// <summary>
    /// Interfaz para el repositorio de cuentas bancarias.
    /// Define los métodos necesarios para interactuar con la base de datos relacionados con las cuentas bancarias.
    /// Hereda de la interfaz genérica <see cref="IGenericRepository{TEntity}"/>.
    /// </summary>
    /// <remarks>
    /// Autor: Raúl Fernández, Javier Hernández, Samuel Cortés, Germán, Álvaro Herrero, Tomás
    /// Versión: 1.0
    /// </remarks>
    public interface IAccountsRepository : IGenericRepository<Account>
    {
        /// <summary>
        /// Obtiene una cuenta bancaria a partir de su IBAN.
        /// </summary>
        /// <param name="Iban">El IBAN de la cuenta que se desea obtener.</param>
        /// <returns>La cuenta bancaria asociada al IBAN, o null si no se encuentra.</returns>
        Task<Account?> getAccountByIbanAsync(string Iban);

        /// <summary>
        /// Obtiene todas las cuentas bancarias asociadas a un cliente a partir de su ID de cliente.
        /// </summary>
        /// <param name="UserId">El ID del cliente cuyas cuentas se desean obtener.</param>
        /// <returns>Una lista de cuentas bancarias asociadas al ID del cliente.</returns>
        Task<List<Account?>> getAccountByClientIdAsync(string UserId);
    }
}
