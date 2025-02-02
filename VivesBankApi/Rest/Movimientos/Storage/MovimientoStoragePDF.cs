using iText.Kernel.Colors;
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
        _logger.LogInformation("Exporting Movimientos to PDF");

        string directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "uploads", "pdf");
        Directory.CreateDirectory(directoryPath);

        string fileName = $"MovimientoExport-{DateTime.UtcNow:yyyyMMdd_HHmmss}.pdf";
        string filePath = Path.Combine(directoryPath, fileName);

        using (var writer = new PdfWriter(filePath))
        using (var pdf = new PdfDocument(writer))
        using (var document = new Document(pdf))
        {
            PdfFont boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
            
            document.Add(new Paragraph("Movimientos Report")
                .SetFont(boldFont)
                .SetFontSize(16)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(20));
            
            Table table = new Table(UnitValue.CreatePercentArray(4)).UseAllAvailableWidth();
            table.SetMarginBottom(30);
            
            string[] headers = { "ID", "Tipo", "Cantidad (€)", "Fecha" };
            foreach (var header in headers)
            {
                table.AddHeaderCell(new Cell()
                    .Add(new Paragraph(header).SetFont(boldFont))
                    .SetBackgroundColor(ColorConstants.LIGHT_GRAY)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetPadding(5));
            }
            
            foreach (var movimiento in entities)
            {
                string tipo = "Desconocido";
                decimal? cantidad = null;

                if (movimiento.Domiciliacion != null) { tipo = "Domiciliación"; cantidad = movimiento.Domiciliacion.Cantidad; }
                else if (movimiento.IngresoDeNomina != null) { tipo = "Ingreso Nómina"; cantidad = movimiento.IngresoDeNomina.Cantidad; }
                else if (movimiento.PagoConTarjeta != null) { tipo = "Pago Tarjeta"; cantidad = movimiento.PagoConTarjeta.Cantidad; }
                else if (movimiento.Transferencia != null) { tipo = "Transferencia"; cantidad = movimiento.Transferencia.Cantidad; }

                table.AddCell(new Cell().Add(new Paragraph(movimiento.Id ?? "N/A")).SetTextAlignment(TextAlignment.CENTER));
                table.AddCell(new Cell().Add(new Paragraph(tipo)).SetTextAlignment(TextAlignment.CENTER));
                table.AddCell(new Cell().Add(new Paragraph(cantidad.HasValue ? $"{cantidad}€" : "N/A")).SetTextAlignment(TextAlignment.CENTER));
                table.AddCell(new Cell().Add(new Paragraph(movimiento.CreatedAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? "N/A")).SetTextAlignment(TextAlignment.CENTER));
            }

            document.Add(table);
            
            document.Add(new Paragraph("Gracias por su confianza en nuestro banco.")
                .SetFont(boldFont)
                .SetFontSize(12)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginTop(20));
        }

        _logger.LogInformation($"File written to: {filePath}");
        return new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
    }
}
