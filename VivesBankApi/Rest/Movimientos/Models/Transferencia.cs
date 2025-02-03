using System.ComponentModel.DataAnnotations;

namespace VivesBankApi.Rest.Movimientos.Models;

public class Transferencia
{
    [Required]
    public string IbanOrigen { get; set; }
    
    [Required]
    public string IbanDestino { get; set; }
    
    [Required]
    [Range(1, 10000, ErrorMessage = "La cantidad debe estar entre 1 y 10000")] 
    public decimal Cantidad { get; set; }
    
    [MaxLength(100, ErrorMessage = "El beneficiario no puede tener más de 100 caracteres")]
    public string NombreBeneficiario { get; set; }
    
    public string? MovimientoDestino { get; set; }

}