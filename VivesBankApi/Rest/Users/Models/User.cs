using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using VivesBankApi.utils.GuuidGenerator;

namespace VivesBankApi.Rest.Users.Models;

 /// <summary>
    /// Representa un usuario en el sistema.
    /// </summary>
    [Table("Users")]
    public class User
    {
        /// <summary>
        /// Identificador único del usuario, generado automáticamente.
        /// </summary>
        [Key]
        [JsonProperty("id")]
        public String Id { get; set; } = GuuidGenerator.GenerateHash();

        /// <summary>
        /// Número de Documento Nacional de Identidad (DNI) del usuario.
        /// Requiere un valor entre 5 y 50 caracteres.
        /// </summary>
        [Required]
        [MinLength(5)]
        [MaxLength(50)]
        [JsonProperty("Dni")]
        public String Dni { get; set; }

        /// <summary>
        /// Contraseña del usuario.
        /// </summary>
        [Required]
        [JsonProperty("password")]
        public String Password { get; set; }

        /// <summary>
        /// Rol del usuario dentro del sistema.
        /// Se asigna mediante un valor del enum <see cref="Role"/>.
        /// </summary>
        [Required]
        [JsonProperty("role")]
        [JsonConverter(typeof(StringEnumConverter))]
        public Role Role { get; set; }

        /// <summary>
        /// Fecha y hora de creación del usuario, asignada automáticamente en UTC.
        /// </summary>
        [JsonProperty("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Fecha y hora de la última actualización del usuario, asignada automáticamente en UTC.
        /// </summary>
        [JsonProperty("updatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Indica si el usuario ha sido marcado como eliminado.
        /// </summary>
        [JsonProperty("isDeleted")]
        public bool IsDeleted { get; set; } = false;
    }

    /// <summary>
    /// Enum que representa los roles posibles de un usuario dentro del sistema.
    /// </summary>
    public enum Role
    {
        /// <summary>
        /// Rol de usuario estándar.
        /// </summary>
        User,

        /// <summary>
        /// Rol de cliente.
        /// </summary>
        Client,

        /// <summary>
        /// Rol de administrador.
        /// </summary>
        Admin,

        /// <summary>
        /// Rol revocado, utilizado para usuarios cuya cuenta ha sido deshabilitada.
        /// </summary>
        Revoked
    } 