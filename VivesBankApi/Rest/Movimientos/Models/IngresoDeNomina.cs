using System.ComponentModel.DataAnnotations;

namespace VivesBankApi.Rest.Movimientos.Models;

public class IngresoDeNomina
{
    [Required]
    public string IbanDestino { get; set; }
    
    [Required]
    public string IbanOrigen { get; set; }
    
    [Range(1, 10000, ErrorMessage = "La cantidad debe estar entre 1 y 10000")] 
    public decimal Cantidad { get; set; }
    
    [MaxLength(100, ErrorMessage = "El nombre de la empresa no puede tener más de 100 caracteres")]
    public string NombreEmpresa { get; set; }
    
    //[RegularExpression("^[A-Z0-9]{9}$", ErrorMessage = "El CIF debe tener 9 caracteres alfanuméricos")]
    public string CifEmpresa { get; set; }
}