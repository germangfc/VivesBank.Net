using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using VivesBankApi.utils.GuuidGenerator;

namespace VivesBankApi.Rest.Product.CreditCard.Models;

/// <summary>
/// Representa una tarjeta de crédito en el sistema. Esta clase mapea los datos a la tabla `CreditCards`
/// en la base de datos y contiene propiedades para almacenar los detalles de una tarjeta de crédito.
/// </summary>
/// <author>Raul Fernandez, Javier Hernandez, Samuel Cortes, German, Alvaro Herrero, Tomas</author>
[Table("CreditCards")]
public class CreditCard
{
    /// <summary>
    /// Obtiene o establece el identificador único de la tarjeta de crédito.
    /// Este identificador es generado automáticamente mediante un generador de hash (UUID).
    /// </summary>
    [Key] 
    public String Id { get; set; } = GuuidGenerator.GenerateHash();
    
    /// <summary>
    /// Obtiene o establece el identificador de la cuenta asociada con la tarjeta de crédito.
    /// Esta propiedad es obligatoria.
    /// </summary>
    [Required]
    public String AccountId { get; set; }
    
    /// <summary>
    /// Obtiene o establece el número de la tarjeta de crédito.
    /// Esta propiedad es obligatoria.
    /// </summary>
    [Required]
    public String CardNumber { get; set; }
    
    /// <summary>
    /// Obtiene o establece el PIN de la tarjeta de crédito.
    /// Esta propiedad es obligatoria.
    /// </summary>
    [Required]
    public String Pin { get; set; }
    
    /// <summary>
    /// Obtiene o establece el código de verificación de la tarjeta de crédito (CVC).
    /// Esta propiedad es obligatoria.
    /// </summary>
    [Required]
    public String Cvc { get; set; }
    
    /// <summary>
    /// Obtiene o establece la fecha de expiración de la tarjeta de crédito.
    /// Esta propiedad es obligatoria.
    /// </summary>
    [Required]
    public DateOnly ExpirationDate { get; set; }
    
    /// <summary>
    /// Obtiene o establece la fecha y hora de creación de la tarjeta de crédito.
    /// Esta propiedad tiene un valor predeterminado que corresponde a la fecha y hora actual (en UTC).
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Obtiene o establece la fecha y hora de la última actualización de la tarjeta de crédito.
    /// Esta propiedad tiene un valor predeterminado que corresponde a la fecha y hora actual (en UTC).
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Obtiene o establece un valor que indica si la tarjeta de crédito ha sido eliminada.
    /// El valor predeterminado es `false`.
    /// </summary>
    public bool IsDeleted { get; set; } = false;
}
