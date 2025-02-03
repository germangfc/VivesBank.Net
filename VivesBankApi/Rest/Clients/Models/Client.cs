using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using VivesBankApi.utils.GuuidGenerator;

namespace VivesBankApi.Rest.Clients.Models
{
    /// <summary>
    /// Representa la entidad Client en la base de datos.
    /// </summary>
    [Table("Clients")]
    public class Client
    {
        /// <summary>
        /// Obtiene o establece el identificador único del cliente.
        /// </summary>
        [Key]
        public string Id { get; set; } = GuuidGenerator.GenerateHash();

        /// <summary>
        /// Obtiene o establece el identificador de usuario del cliente.
        /// Este es un UUID único asociado al usuario.
        /// </summary>
        [Required]
        [JsonProperty("userId")]
        [Length(9, 9, ErrorMessage = "The userId must be a guuid")]
        public string UserId { get; set; }

        /// <summary>
        /// Obtiene o establece el nombre completo del cliente.
        /// El nombre debe tener entre 3 y 100 caracteres.
        /// </summary>
        [Required]
        [JsonProperty("fullName")]
        [MaxLength(100, ErrorMessage = "The name must be at most 100 characters long")]
        [MinLength(3, ErrorMessage = "The name must be at least 3 characters long")]
        public string FullName { get; set; }

        /// <summary>
        /// Obtiene o establece la dirección del cliente.
        /// La dirección debe tener entre 5 y 200 caracteres.
        /// </summary>
        [Required]
        [JsonProperty("address")]
        [MaxLength(200, ErrorMessage = "The address must be at most 200 characters long")]
        [MinLength(5, ErrorMessage = "The address must be at least 5 characters long")]
        public string Adress { get; set; }

        /// <summary>
        /// Obtiene o establece la foto de perfil del cliente.
        /// Si no se proporciona, se asigna el valor por defecto "defaultProfile.png".
        /// </summary>
        [Required] 
        [JsonProperty("photo")]
        public string Photo { get; set; } = "defaultProfile.png";

        /// <summary>
        /// Obtiene o establece la foto de DNI del cliente.
        /// Si no se proporciona, se asigna el valor por defecto "defaultDni.png".
        /// </summary>
        [Required]
        [JsonProperty("idPhoto")]
        public string PhotoDni { get; set; } = "defaultDni.png";

        /// <summary>
        /// Obtiene o establece las identificaciones de las cuentas asociadas al cliente.
        /// </summary>
        [Required]
        [JsonProperty("accounts")]
        public List<string> AccountsIds { get; set; } = new();

        /// <summary>
        /// Obtiene o establece la fecha de creación del cliente.
        /// Se establece por defecto a la fecha y hora actual en formato UTC.
        /// </summary>
        [JsonProperty("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Obtiene o establece la fecha de la última actualización del cliente.
        /// Se establece por defecto a la fecha y hora actual en formato UTC.
        /// </summary>
        [JsonProperty("updatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Obtiene o establece un valor que indica si el cliente está marcado como eliminado.
        /// </summary>
        [JsonProperty("isDeleted")]
        public bool IsDeleted { get; set; } = false;
    }
}
