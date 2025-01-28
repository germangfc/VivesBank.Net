db.createUser({
    user: 'user',
    pwd: 'pass',
    roles: [
        {
            role: 'readWrite',
            db: 'banco',
        },
    ],
});
db = db.getSiblingDB("banco");

db.movimientos.insertMany([
    {
        ClienteGuid: "123e4567-e89b-12d3-a456-426614174000",
        Domiciliacion: {
            ClienteGuid: "123e4567-e89b-12d3-a456-426614174000",
            IbanOrigen: "ES9121000418450200051332",
            IbanDestino: "ES9121000418450200051333",
            Cantidad: 500.00,
            NombreAcreedor: "Proveedor XYZ",
            FechaInicio: new Date("2024-01-01T00:00:00Z"),
            Periodicidad: "MENSUAL",
            Activa: true,
            UltimaEjecucion: new Date("2024-01-01T00:00:00Z")
        },
        IngresoDeNomina: null,
        PagoConTarjeta: null,
        Transferencia: null,
        CreatedAt: new Date("2024-01-01T10:00:00Z"),
        UpdatedAt: new Date("2024-01-01T10:00:00Z"),
        IsDeleted: false
    },
    {
        ClienteGuid: "789e4567-e89b-12d3-a456-426614174111",
        Domiciliacion: null,
        IngresoDeNomina: {
            IbanDestino: "ES9820385778983000760236",
            IbanOrigen: "ES9820385778983000760237",
            Cantidad: 2000.00,
            NombreEmpresa: "Empresa ABC",
            CifEmpresa: "B12345678"
        },
        PagoConTarjeta: null,
        Transferencia: null,
        CreatedAt: new Date("2024-01-02T12:30:00Z"),
        UpdatedAt: new Date("2024-01-02T12:30:00Z"),
        IsDeleted: false
    },
    {
        ClienteGuid: "456e7890-e89b-12d3-a456-426614174222",
        Domiciliacion: null,
        IngresoDeNomina: null,
        PagoConTarjeta: {
            NumeroTarjeta: "4111111111111111",
            Cantidad: 150.75,
            NombreComercio: "Supermercado 24h"
        },
        Transferencia: null,
        CreatedAt: new Date("2024-01-03T08:15:00Z"),
        UpdatedAt: new Date("2024-01-03T08:15:00Z"),
        IsDeleted: false
    },
    {
        ClienteGuid: "234e5678-e89b-12d3-a456-426614174333",
        Domiciliacion: null,
        IngresoDeNomina: null,
        PagoConTarjeta: null,
        Transferencia: {
            IbanOrigen: "ES1121000418450200051334",
            IbanDestino: "ES1121000418450200051335",
            Cantidad: 300.00,
            NombreBeneficiario: "Juan Pérez",
            MovimientoDestino: "Compra electrónica"
        },
        CreatedAt: new Date("2024-01-04T14:20:00Z"),
        UpdatedAt: new Date("2024-01-04T14:20:00Z"),
        IsDeleted: false
    }
]);