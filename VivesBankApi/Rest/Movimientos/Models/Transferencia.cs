namespace VivesBankApi.Rest.Movimientos.Models;

public class Transferencia
{
    public string IbanOrigen { get; set; }
    
    public string IbanDestino { get; set; }
    
    public decimal Cantidad { get; set; }
    
    public string NombreBeneficiario { get; set; }
    
    public string MovimientoDestino { get; set; }

}