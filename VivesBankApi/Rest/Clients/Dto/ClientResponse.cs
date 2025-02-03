
using Newtonsoft.Json;

namespace VivesBankApi.Rest.Clients.Dto
{
    /// <summary>
    /// Representa la respuesta de un cliente, con sus datos completos.
    /// </summary>
    /// <author>Raul Fernandez, Javier Hernandez, Samuel Cortes, Alvaro Herrero, German, Tomas</author>
    /// <version>1.0</version>
    public class ClientResponse
    {
        /// <summary>
        /// ID del cliente.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }
        
        /// <summary>
        /// ID de usuario asociado al cliente.
        /// </summary>
        [JsonProperty("userId")]
        public string UserId { get; set; }
        
        /// <summary>
        /// Dirección del cliente.
        /// </summary>
        [JsonProperty("address")]
        public string Address { get; set; } 
        
        /// <summary>
        /// Nombre completo del cliente.
        /// </summary>
        [JsonProperty("fullname")]
        public string Fullname { get; set; }
        
        /// <summary>
        /// Foto del DNI del cliente.
        /// </summary>
        [JsonProperty("dniPhoto")]
        public string DniPhoto { get; set; }
        
        /// <summary>
        /// Foto del cliente.
        /// </summary>
        [JsonProperty("photo")]
        public string Photo { get; set; }
        
        /// <summary>
        /// Cuentas asociadas al cliente.
        /// </summary>
        [JsonProperty("accounts")]
        public List<String> Accounts { get; set; }
        
        /// <summary>
        /// Fecha de creación del cliente.
        /// </summary>
        [JsonProperty("createdAt")]
        public DateTime CreatedAt { get; set; }
        
        /// <summary>
        /// Fecha de la última actualización del cliente.
        /// </summary>
        [JsonProperty("updatedAt")]
        public DateTime UpdatedAt { get; set; }
        
        /// <summary>
        /// Indica si el cliente está eliminado.
        /// </summary>
        [JsonProperty("isDeleted")]
        public bool IsDeleted { get; set; }
    }
}
