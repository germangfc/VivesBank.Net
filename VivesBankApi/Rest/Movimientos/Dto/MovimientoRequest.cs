using VivesBankApi.Rest.Movimientos.Models;
using VivesBankApi.utils.GuuidGenerator;

namespace VivesBankApi.Rest.Movimientos.Dto;

public class MovimientoRequest
{
    public string guid { get; set; } = GuuidGenerator.GenerateHash();
    public string clienteGuid { get; set; }
    public Domiciliacion domiciliacion { get; set; }
    public IngresoDeNomina ingresoDeNomina { get; set; }
    public PagoConTarjeta pagoConTarjeta { get; set; }
}