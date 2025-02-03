using Microsoft.Extensions.FileProviders;

namespace VivesBankApi.Rest.Product.Base.Storage;

/// <summary>
/// Define la funcionalidad para cargar datos de productos desde un archivo CSV.
/// </summary>
public interface IStorageCsv
{
    /// <summary>
    /// Carga una lista de productos desde un flujo de datos CSV.
    /// </summary>
    /// <param name="stream">El flujo de datos que contiene la información del archivo CSV.</param>
    /// <returns>Una lista de objetos <see cref="Base.Models.Product"/> representando los productos cargados.</returns>
    List<Base.Models.Product> LoadCsv(Stream stream);
}