using VivesBankApi.Rest.Movimientos.Models;

namespace VivesBankApi.Rest.Movimientos.Storage;

public interface IMovimientoStoragePDF
{
    Task<FileStream> Export(List<Movimiento> data);
}