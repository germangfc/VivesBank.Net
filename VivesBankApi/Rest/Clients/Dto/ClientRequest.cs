using System.ComponentModel.DataAnnotations;

namespace VivesBankApi.Rest.Clients.Dto
{
    /// <summary>
    /// Representa una solicitud para crear o actualizar un cliente.
    /// </summary>
    /// <author>Raul Fernandez, Javier Hernandez, Samuel Cortes, Alvaro Herrero, German, Tomas</author>
    /// <version>1.0</version>
    public class ClientRequest
    {
        /// <summary>
        /// Nombre completo del cliente.
        /// </summary>
        [Required]
        [MaxLength(50, ErrorMessage = "The name must be at most 50 characters")]
        [MinLength(5, ErrorMessage = "The name must be at least 5 characters")]
        public String FullName { get; set; }

        /// <summary>
        /// Dirección del cliente.
        /// </summary>
        [Required]
        [MaxLength(200, ErrorMessage = "The address must be at most 100 characters")]
        [MinLength(10, ErrorMessage = "The address must be at least 10 characters")]
        public String Address { get; set; }
    }
}
