namespace VivesBankApi.Rest.Movimientos.Models;

public class IngresoDeNomina
{
    public string IbanDestino { get; set; }
    
    public string IbanOrigen { get; set; }
    
    public decimal Cantidad { get; set; }
    
    public string NombreEmpresa { get; set; }
    
    public string CifEmpresa { get; set; }
}