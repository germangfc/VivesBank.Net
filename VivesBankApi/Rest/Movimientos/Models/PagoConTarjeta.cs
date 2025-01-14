using System.ComponentModel.DataAnnotations;

namespace VivesBankApi.Rest.Movimientos.Models;

public class PagoConTarjeta
{
    [Required]
    public string NumeroTarjeta { get; set; }
    
    [Range(1, 10000, ErrorMessage = "La cantidad debe estar entre 1 y 10000")] 
    public decimal Cantidad { get; set; }
    
    [Required]
    [MaxLength(100, ErrorMessage = "El nombre del comercio no puede tener más de 100 caracteres")]
    public string NombreComercio { get; set; }
}