namespace VivesBankApi.Rest.Movimientos.Models;

/// <summary>
/// Representa la periodicidad de una domiciliación o movimiento programado.
/// Se utiliza para definir la frecuencia con la que se realiza una acción o pago.
/// </summary>
/// <remarks>
/// Esta enumeración es utilizada para establecer la periodicidad de los movimientos financieros, como domiciliaciones, pagos programados, etc.
/// </remarks>
/// <author>VivesBank Team</author>
public enum Periodicidad
{
    /// <summary>
    /// La acción o movimiento se realiza de forma diaria.
    /// </summary>
    DIARIA, 
        
    /// <summary>
    /// La acción o movimiento se realiza de forma mensual.
    /// </summary>
    MENSUAL, 
        
    /// <summary>
    /// La acción o movimiento se realiza de forma semanal.
    /// </summary>
    SEMANAL, 
        
    /// <summary>
    /// La acción o movimiento se realiza de forma anual.
    /// </summary>
    ANUAL
}