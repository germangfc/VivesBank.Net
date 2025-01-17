using System.Numerics;
using System.Text;
using VivesBankApi.Rest.Product.BankAccounts.Repositories;
using VivesBankApi.Rest.Products.BankAccounts.Exceptions;

namespace VivesBankApi.Utils.IbanGenerator;

public class IbanGenerator : IIbanGenerator
{
    private readonly IAccountsRepository _bankAccountRepository;
    
    public IbanGenerator(IAccountsRepository bankAccountRepository)
    {
        _bankAccountRepository = bankAccountRepository;
    }
    
    public async Task<string> GenerateUniqueIbanAsync()
        {
            string iban;
            int attempts = 0;

            do
            {
                iban = GenerateIban();
                attempts++;

                if (attempts > 1000)
                {
                    throw new AccountsExceptions.AccountIbanNotGeneratedException();
                }
            } while (await IbanExistsAsync(iban));

            return iban;
        }

        /// <summary>
        /// Verifica si un IBAN ya existe en la base de datos.
        /// </summary>
        /// <param name="iban">El IBAN a verificar</param>
        /// <returns>True si el IBAN ya existe, false en caso contrario</returns>
        public async Task<bool> IbanExistsAsync(string iban)
        {
            return await _bankAccountRepository.getAccountByIbanAsync(iban) != null;
        }

        /// <summary>
        /// Genera un nuevo IBAN en formato estándar, con un código de país, entidad, sucursal y dígitos de control.
        /// </summary>
        /// <returns>El IBAN generado</returns>
        public string GenerateIban()
        {
            string countryCode = "ES";
            string entityCode = "0128";
            string branchCode = "0001";
            string accountControlDigits = "00";
            string accountNumber = GenerateRandomDigits(10);

            string ibanBase = $"{entityCode}{branchCode}{accountControlDigits}{accountNumber}142800";
            int checkDigits = CalculateControlDigits(ibanBase);

            return $"{countryCode}{checkDigits:D2}{entityCode}{branchCode}{accountControlDigits}{accountNumber}";
        }

        /// <summary>
        /// Calcula los dígitos de control de un IBAN usando el algoritmo estándar.
        /// </summary>
        /// <param name="ibanBase">La base del IBAN sin los dígitos de control.</param>
        /// <returns>Los dígitos de control calculados.</returns>
        public int CalculateControlDigits(string ibanBase)
        {
            StringBuilder numericIban = new StringBuilder();

            foreach (char ch in ibanBase)
            {
                if (char.IsDigit(ch))
                {
                    numericIban.Append(ch);
                }
                else
                {
                    numericIban.Append((int)ch - 'A' + 10);
                }
            }

            BigInteger numericIbanBigInt = BigInteger.Parse(numericIban.ToString());
            BigInteger remainder = numericIbanBigInt % 97;
            BigInteger checkDigits = 98 - remainder;

            return (int)checkDigits;
        }

        /// <summary>
        /// Genera una cadena de dígitos aleatorios de una longitud específica.
        /// </summary>
        /// <param name="length">Longitud de la cadena de dígitos.</param>
        /// <returns>La cadena de dígitos aleatorios.</returns>
        public string GenerateRandomDigits(int length)
        {
            Random random = new Random();
            StringBuilder digits = new StringBuilder();

            for (int i = 0; i < length; i++)
            {
                digits.Append(random.Next(0, 10));
            }

            return digits.ToString();
        } 
}
