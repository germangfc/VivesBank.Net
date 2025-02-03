namespace VivesBankApi.Rest.Clients.Dto
{
    /// <summary>
    /// Representa una solicitud de actualización parcial (PATCH) de un cliente.
    /// </summary>
    /// <author>Raul Fernandez, Javier Hernandez, Samuel Cortes, Alvaro Herrero, German, Tomas</author>
    /// <version>1.0</version>
    public class ClientPatchRequest
    {
        /// <summary>
        /// Nombre completo del cliente.
        /// </summary>
        public String? FullName { get; set; }

        /// <summary>
        /// Dirección del cliente.
        /// </summary>
        public String? Address { get; set; }

        /// <summary>
        /// Foto de perfil del cliente.
        /// </summary>
        public String? Photo { get; set; }

        /// <summary>
        /// Foto del DNI del cliente.
        /// </summary>
        public String? PhotoDni { get; set; }
    }
}