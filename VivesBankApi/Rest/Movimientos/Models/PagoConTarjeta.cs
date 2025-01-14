namespace VivesBankApi.Rest.Movimientos.Models;

public class PagoConTarjeta
{
    public string NumeroTarjeta { get; set; }
    
    public decimal Cantidad { get; set; }
    
    public string NombreEmpresa { get; set; }
}