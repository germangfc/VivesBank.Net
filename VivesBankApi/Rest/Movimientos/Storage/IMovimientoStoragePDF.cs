using VivesBankApi.Rest.Movimientos.Models;

/// <summary>
/// Interfaz para la gestión de exportación de movimientos a formato PDF.
/// </summary>
/// <remarks>
/// Esta interfaz define un método para exportar una lista de movimientos a un archivo PDF.
/// </remarks>
/// <author>Raul Fernandez, Javier Hernandez, Samuel Cortes, German, Alvaro Herrero, Tomas</author>
/// <version>1.0.0</version>
public interface IMovimientoStoragePDF
{
    /// <summary>
    /// Exporta una lista de movimientos a un archivo PDF.
    /// </summary>
    /// <param name="data">Lista de movimientos a exportar</param>
    /// <returns>Un <see cref="FileStream"/> que representa el archivo PDF generado</returns>
    /// <exception cref="ArgumentNullException">Lanzado cuando la lista de movimientos proporcionada es null</exception>
    /// <exception cref="InvalidOperationException">Lanzado cuando ocurre un error durante la generación del archivo PDF</exception>
    /// <remarks>
    /// Este método toma una lista de objetos de tipo <see cref="Movimiento"/> y genera un archivo PDF.
    /// El archivo PDF generado se entrega como un flujo de archivo (FileStream).
    /// </remarks>
    Task<FileStream> Export(List<Movimiento> data);
}
