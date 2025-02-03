using ApiFunkosCS.Utils.GenericRepository;
using Microsoft.EntityFrameworkCore;
using VivesBankApi.Database;
using VivesBankApi.Rest.Product.BankAccounts.Models;

namespace VivesBankApi.Rest.Product.BankAccounts.Repositories
{
    /// <summary>
    /// Repositorio para la gestión de las cuentas bancarias.
    /// Esta clase se encarga de interactuar con la base de datos para realizar operaciones relacionadas con las cuentas bancarias.
    /// Hereda de la clase genérica <see cref="GenericRepository{TContext, TEntity}"/>.
    /// </summary>
    /// <remarks>
    /// Autor: Raúl Fernández, Javier Hernández, Samuel Cortés, Germán, Álvaro Herrero, Tomás
    /// Versión: 1.0
    /// </remarks>
    public class AccountsRepository : GenericRepository<BancoDbContext, Account>, IAccountsRepository
    {
        /// <summary>
        /// Constructor de la clase <see cref="AccountsRepository"/>.
        /// Inicializa el repositorio con el contexto de la base de datos y el logger.
        /// </summary>
        /// <param name="context">El contexto de la base de datos.</param>
        /// <param name="logger">El logger utilizado para registrar los eventos.</param>
        public AccountsRepository(BancoDbContext context, ILogger<AccountsRepository> logger) : base(context, logger)
        {
        }

        /// <summary>
        /// Obtiene una cuenta bancaria a partir de su IBAN.
        /// </summary>
        /// <param name="Iban">El IBAN de la cuenta que se desea obtener.</param>
        /// <returns>La cuenta bancaria asociada al IBAN, o null si no se encuentra.</returns>
        /// <remarks>
        /// Registra una entrada en el log indicando el IBAN utilizado para la búsqueda.
        /// </remarks>
        public async Task<Account?> getAccountByIbanAsync(string Iban)
        {
            _logger.LogInformation($"Getting account with IBAN: {Iban}");
            return await _dbSet.FirstOrDefaultAsync(a => a.IBAN == Iban);
        }

        /// <summary>
        /// Obtiene las cuentas bancarias asociadas a un cliente a partir de su ID de cliente.
        /// </summary>
        /// <param name="client">El ID del cliente cuyas cuentas se desean obtener.</param>
        /// <returns>Una lista de cuentas bancarias asociadas al ID del cliente.</returns>
        /// <remarks>
        /// Registra una entrada en el log indicando el ID del cliente utilizado para la búsqueda.
        /// </remarks>
        public async Task<List<Account?>> getAccountByClientIdAsync(string client)
        {
            _logger.LogInformation($"Getting accounts with user ID: {client}");
            return await _dbSet
                .Where(a => a.ClientId == client)
                .Select(a => (Account?)a) 
                .ToListAsync();
        }
    }
}
