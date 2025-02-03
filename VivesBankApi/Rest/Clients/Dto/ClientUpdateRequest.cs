using System.ComponentModel.DataAnnotations;
namespace VivesBankApi.Rest.Clients.Dto
{
    /// <summary>
    /// Representa la solicitud de actualización de un cliente, con los datos que pueden ser modificados.
    /// </summary>
    /// <author>Raul Fernandez, Javier Hernandez, Samuel Cortes, Alvaro Herrero, German, Tomas</author>
    /// <version>1.0</version>
    public class ClientUpdateRequest
    {
        /// <summary>
        /// Nombre completo del cliente, con una longitud mínima de 3 caracteres y máxima de 80.
        /// </summary>
        [StringLength(80, MinimumLength = 3, ErrorMessage = "FullName must be between 3 and 80 characters.")]
        public string FullName { get; set; }

        /// <summary>
        /// Dirección del cliente, con una longitud mínima de 3 caracteres y máxima de 80.
        /// </summary>
        [StringLength(80, MinimumLength = 3, ErrorMessage = "Address must be between 3 and 80 characters.")]
        public string Address { get; set; }
    }
}
