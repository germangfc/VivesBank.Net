using VivesBankApi.Rest.Movimientos.Models;

namespace VivesBankApi.Rest.Movimientos.Storage;

using iText.Kernel.Font;
using iText.IO.Font.Constants;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

public class MovimientoStoragePDF : IMovimientoStoragePDF
{
    private readonly ILogger<MovimientoStoragePDF> _logger;

    public MovimientoStoragePDF(ILogger<MovimientoStoragePDF> logger)
    {
        _logger = logger;
    }

    public async Task<FileStream> Export(List<Movimiento> entities)
    {
        _logger.LogInformation($"Exporting {typeof(Movimiento).Name} to a PDF file");
        
        var directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "uploads", "pdf");

        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }
        
        var fileName = $"MovimientoExport-" + DateTime.UtcNow.ToString("yyyyMMdd_HHmmss") + ".pdf";
        var filePath = Path.Combine(directoryPath, fileName);

        using (PdfWriter writer = new PdfWriter(filePath))
        using (PdfDocument pdf = new PdfDocument(writer))
        using (Document document = new Document(pdf))
        {
            document.Add(new Paragraph("Movimientos Report")
                .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD))
                .SetFontSize(16));
            
            Table table = new Table(UnitValue.CreatePercentArray(4)).UseAllAvailableWidth();
            table.AddHeaderCell("ID");
            table.AddHeaderCell("Cliente GUID");
            table.AddHeaderCell("Tipo");
            table.AddHeaderCell("Fecha");

            foreach (var movimiento in entities)
            {
                table.AddCell(movimiento.Id ?? "N/A");
                table.AddCell(movimiento.ClienteGuid);
                
                string tipo = movimiento.Domiciliacion != null ? "Domiciliación" :
                              movimiento.IngresoDeNomina != null ? "Ingreso Nómina" :
                              movimiento.PagoConTarjeta != null ? "Pago Tarjeta" :
                              movimiento.Transferencia != null ? "Transferencia" : "Desconocido";
                table.AddCell(tipo);

                table.AddCell(movimiento.CreatedAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? "N/A");
            }

            document.Add(table);
        }

        _logger.LogInformation($"File written to: {filePath}");
        
        return new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
    }
}
